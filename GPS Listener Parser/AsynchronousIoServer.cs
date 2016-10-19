using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Collections.ObjectModel;
using GPSParser.Teltonika;

namespace GPSParser
{
    public class AsynchronousIoServer
    {
        private Socket _serverSocket;
        private int _port;
        private ObservableCollection<ConnectionInfo> _connections = new ObservableCollection<ConnectionInfo>();     

        public AsynchronousIoServer(int port)
        { 
            _port = port;
            _connections.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_connections_CollectionChanged);
        }

        void _connections_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Console.Title = _connections.Count + " devices connected";
        }

        private void SetupServerSocket()
        {
            //IPHostEntry localMachineInfo =
            //    Dns.GetHostEntry(Dns.GetHostName());
            //IPEndPoint myEndpoint = new IPEndPoint(
            //   localMachineInfo.AddressList[3], _port);        
            //_serverSocket = new Socket(myEndpoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //_serverSocket.Bind(myEndpoint);

            string IP = GPSParser.Properties.Settings.Default.IPaddress;
            IPEndPoint myEndpoint = new IPEndPoint(IPAddress.Parse(IP), _port);

            _serverSocket = new Socket(myEndpoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(myEndpoint);

            _serverSocket.Listen((int)SocketOptionName.MaxConnections);
        }
        private class ConnectionInfo
        {
            public Socket Socket;
            public byte[] Buffer;
            public bool isPartialLoaded;
            public List<byte> TotalBuffer;
            public string IMEI;
        }          
   
        public void Start()
        {
            SetupServerSocket();
            // number of simultaneous connections can be accepted
            for (int i = 0; i < 2000; i++)
                _serverSocket.BeginAccept(new
                    AsyncCallback(AcceptCallback), _serverSocket);
        }

        private void AcceptCallback(IAsyncResult result)
        {
            ConnectionInfo connection = new ConnectionInfo();
            try
            {
                // Finish Accept operation
                Socket s = (Socket)result.AsyncState;
                connection.Socket = s.EndAccept(result);
                connection.Buffer = new byte[1024];
                // add connection to connection list
                lock (_connections)
                {
                    _connections.Add(connection);                   
                }

                // Start BeginReceive operation on connected device and make new BeginAccept operation on socket
                // for accept new connectionrequests.
                connection.Socket.BeginReceive(connection.Buffer,
                    0, connection.Buffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveCallback), connection);
                _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), result.AsyncState);
            }
            catch (SocketException exc)
            {
                CloseConnection(connection);
                Console.WriteLine("Socket exception: " +
                    exc.SocketErrorCode);
            }
            catch (Exception exc)
            {
                CloseConnection(connection);
                Console.WriteLine("Exception: " + exc);
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            ConnectionInfo connection = (ConnectionInfo)result.AsyncState;
            try
            {
                //get a number of received bytes
                int bytesRead = connection.Socket.EndReceive(result);
                if (bytesRead > 0)
                {
                    //because device sends data with portions we need summary all portions to total buffer
                    if (connection.isPartialLoaded)
                    {
                        connection.TotalBuffer.AddRange(connection.Buffer.Take(bytesRead).ToList());
                    }
                    else
                    {
                        if (connection.TotalBuffer != null)
                            connection.TotalBuffer.Clear();
                        connection.TotalBuffer = connection.Buffer.Take(bytesRead).ToList();
                    }
                    //-------- Get Length of current received data ----------
                    string hexDataLength = string.Empty;
                  
                    //Skip four zero bytes an take next four bytes with value of AVL data array length
                    connection.TotalBuffer.Skip(4).Take(4).ToList().ForEach(delegate(byte b) { hexDataLength += String.Format("{0:X2}", b); });

                    int dataLength = Convert.ToInt32(hexDataLength, 16);
                    //
                    //bytesRead = 17 when parser receive IMEI  from device
                    //if datalength encoded in data > then total buffer then is a partial data a device will send next part
                    //we send confirmation and wait next portion of data
                    if (dataLength + 12 > connection.TotalBuffer.Count && bytesRead != 17)
                    {
                        connection.isPartialLoaded = true;
                        connection.Socket.Send(new byte[] { 0x01 });
                        connection.Socket.BeginReceive(connection.Buffer, 0, connection.Buffer.Length, SocketFlags.None,
                        new AsyncCallback(ReceiveCallback), connection);
                        return;
                    }

                    bool isDataPacket = true;

                    //when device send AVL data first 4 bytes is 0
                    string firstRourBytes = string.Empty;
                    connection.TotalBuffer.Take(4).ToList().ForEach(delegate(byte b) { firstRourBytes += String.Format("{0:X2}", b); });
                    if (Convert.ToInt32(firstRourBytes, 16) > 0)
                        isDataPacket = false;

                    // if is true then is AVL data packet
                    // else that a IMEI sended
                    if (isDataPacket)
                    {
                        if (GPSParser.Properties.Settings.Default.ShowDiagnosticMessages)
                        {
                            //all data we convert this to string in hex format only for diagnostic
                            StringBuilder data = new StringBuilder();
                            connection.TotalBuffer.ForEach(delegate(byte b) { data.AppendFormat("{0:X2}", b); });
                            Console.WriteLine("<" + data);
                        }
                        TeltonikaDevicesParser decAVL = new TeltonikaDevicesParser(GPSParser.Properties.Settings.Default.ShowDiagnosticMessages);
                        decAVL.OnDataReceive += new Action<string>(decAVL_OnDataReceive);
                        //if CRC not correct number of data returned by AVL parser = 0;
                        int numberOfData = decAVL.Decode(connection.TotalBuffer, connection.IMEI);
                        if (!connection.isPartialLoaded)
                        {
                            // send to device number of received data for confirmation.
                            if (numberOfData > 0)
                                connection.Socket.Send(new byte[] { 0x00, 0x00, 0x00, Convert.ToByte(numberOfData) });
                            else
                                //send 0 number of data if CRC not correct for resend data from device
                                connection.Socket.Send(new byte[] { 0x00, 0x00, 0x00, 0x00 });
                        }
                        decAVL.OnDataReceive -= new Action<string>(decAVL_OnDataReceive);
                        Console.WriteLine("Modem ID: " + connection.IMEI + " send data");
                    }
                    else
                    {
                        //if is not data packet then is it IMEI info send from device
                        connection.IMEI = Encoding.ASCII.GetString(connection.TotalBuffer.Skip(2).ToArray());
                        connection.Socket.Send(new byte[] { 0x01 });
                        Console.WriteLine("Modem ID: " + connection.IMEI + " connected");
                    }
                    // Get next data portion from device
                    connection.Socket.BeginReceive(connection.Buffer, 0, connection.Buffer.Length, SocketFlags.None,
                        new AsyncCallback(ReceiveCallback), connection);
                }//if all data received then close connection                    
                else CloseConnection(connection);
            }
            catch (SocketException exc)
            {
                CloseConnection(connection);
                Console.WriteLine("Socket exception: " + exc.SocketErrorCode);
            }
            catch (Exception exc)
            {
                CloseConnection(connection);
                Console.WriteLine("Exception: " + exc);
            }
        }

        void decAVL_OnDataReceive(string obj)
        {
            Console.WriteLine(obj);
        }

        private void CloseConnection(ConnectionInfo ci)
        {
            ci.Socket.Close();
            lock (_connections)
            {
                _connections.Remove(ci);
                Console.WriteLine("Modem ID: " + ci.IMEI + " disconnected");
            }
        }
    }
}
