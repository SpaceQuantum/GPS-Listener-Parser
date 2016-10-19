using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GPSParser.DBLogic;

namespace GPSParser.Teltonika
{
    public class FMXXXX_Parser : ParserBase
    {
        public FMXXXX_Parser(bool showDiagnosticInfo)
        {
            _showDiagnosticInfo = showDiagnosticInfo;
        }
        public override int DecodeAVL(List<byte> receiveBytes, string IMEI)
        {           
            string hexDataLength = string.Empty;
            receiveBytes.Skip(4).Take(4).ToList().ForEach(delegate(byte b) { hexDataLength += String.Format("{0:X2}", b); });            
            int dataLength = Convert.ToInt32(hexDataLength, 16);    
            // byte[] test = receiveBytes.Skip(4).Take(4).ToArray();
           // Array.Reverse(test);
           // int dataLength = BitConverter.ToInt32(test, 0);

            ShowDiagnosticInfo("Data Length: -----".PadRight(40, '-') + " "  + dataLength);
            int codecId = Convert.ToInt32(receiveBytes.Skip(8).Take(1).ToList()[0]);
            ShowDiagnosticInfo("Codec ID: -----".PadRight(40, '-') + " "  + codecId);
            int numberOfData = Convert.ToInt32(receiveBytes.Skip(9).Take(1).ToList()[0]); ;
            ShowDiagnosticInfo("Number of data: ----".PadRight(40, '-') + " "  + numberOfData);

            int tokenAddress = 10;
            for (int n = 0; n < numberOfData; n++)
            {
                GPSdata gpsData = new GPSdata();
                string hexTimeStamp = string.Empty;
                receiveBytes.Skip(tokenAddress).Take(8).ToList().ForEach(delegate(byte b) { hexTimeStamp += String.Format("{0:X2}", b); });  
                long timeSt = Convert.ToInt64(hexTimeStamp, 16);

                DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                DateTime timestamp = origin.AddMilliseconds(Convert.ToDouble(timeSt));

                ShowDiagnosticInfo("Timestamp: -----".PadRight(40, '-') + " "  + timestamp.ToLongDateString() + " " + timestamp.ToLongTimeString());

                int priority = Convert.ToInt32(receiveBytes.Skip(tokenAddress + 8).Take(1).ToList()[0]);
                ShowDiagnosticInfo("Priority: ------------".PadRight(40, '-') + " "  + priority);

                string longt = string.Empty;
                receiveBytes.Skip(tokenAddress + 9).Take(4).ToList().ForEach(delegate(byte b) { longt += String.Format("{0:X2}", b); });
                double longtitude = ((double)Convert.ToInt32(longt, 16)) / 10000000;
                ShowDiagnosticInfo("Longtitude: -----".PadRight(40, '-') + " "  + longtitude);

                string lat = string.Empty;
                receiveBytes.Skip(tokenAddress + 13).Take(4).ToList().ForEach(delegate(byte b) { lat += String.Format("{0:X2}", b); });
                double latitude = ((double)Convert.ToInt32(lat, 16)) / 10000000;
                ShowDiagnosticInfo("Latitude: -----".PadRight(40, '-') + " "  + latitude);

                string alt = string.Empty;
                receiveBytes.Skip(tokenAddress + 17).Take(2).ToList().ForEach(delegate(byte b) { alt += String.Format("{0:X2}", b); });
                int altitude = Convert.ToInt32(alt, 16);
                ShowDiagnosticInfo("Altitude: -----".PadRight(40, '-') + " "  + altitude);

                string ang = string.Empty;
                receiveBytes.Skip(tokenAddress + 19).Take(2).ToList().ForEach(delegate(byte b) { ang += String.Format("{0:X2}", b); });
                int angle = Convert.ToInt32(ang, 16);
                ShowDiagnosticInfo("Angle: -----".PadRight(40, '-') + " "  + angle);

                int satellites = Convert.ToInt32(receiveBytes.Skip(tokenAddress + 21).Take(1).ToList()[0]);
                ShowDiagnosticInfo("Satellites: -----".PadRight(40, '-') + " "  + satellites);

                string sp = string.Empty;
                receiveBytes.Skip(tokenAddress + 22).Take(2).ToList().ForEach(delegate(byte b) { sp += String.Format("{0:X2}", b); });
                int speed = Convert.ToInt32(sp, 16);
                ShowDiagnosticInfo("Speed: -----".PadRight(40, '-') + " "  + speed);

                byte event_IO_element_ID = (byte)Convert.ToInt32(receiveBytes.Skip(tokenAddress + 24).Take(1).ToList()[0]);
                gpsData.Event_IO_element_ID = event_IO_element_ID;
                ShowDiagnosticInfo("IO element ID of Event generated: ------".PadRight(40, '-') + " "  + event_IO_element_ID);

                int IO_element_in_record = Convert.ToInt32(receiveBytes.Skip(tokenAddress + 25).Take(1).ToList()[0]);
                ShowDiagnosticInfo("IO_element_in_record: --------".PadRight(40, '-') + " "  + IO_element_in_record);


                if (IO_element_in_record != 0)
                {
                    int currentCursor = tokenAddress + 26;

                    int IO_Elements_1B_Quantity = Convert.ToInt32(receiveBytes.Skip(currentCursor).Take(1).ToList()[0]);
                    ShowDiagnosticInfo("1 byte IO element in record: --------".PadRight(40, '-') + " "  + IO_Elements_1B_Quantity);


                    for (int IO_1 = 0; IO_1 < IO_Elements_1B_Quantity; IO_1++)
                    {
                        var parameterID =  (byte)Convert.ToInt32(receiveBytes.Skip(currentCursor + 1 + IO_1 * 2).Take(1).ToList()[0]);
                        var IO_Element_1B = (byte)Convert.ToInt32(receiveBytes.Skip(currentCursor + 2 + IO_1 * 2).Take(1).ToList()[0]);
                        gpsData.IO_Elements_1B.Add(parameterID, IO_Element_1B);
                        ShowDiagnosticInfo("IO element 1B ID: --------".PadRight(40, '-') + " "  + parameterID);
                        ShowDiagnosticInfo(IO_1 + "'st 1B IO element value: --------".PadRight(40 - IO_1.ToString().Length, '-') + " "  + IO_Element_1B);
                    }
                    currentCursor += IO_Elements_1B_Quantity * 2 + 1;

                    int IO_Elements_2B_Quantity = Convert.ToInt32(receiveBytes.Skip(currentCursor).Take(1).ToList()[0]);
                    ShowDiagnosticInfo("2 byte IO element in record: --------".PadRight(40, '-') + " " + IO_Elements_2B_Quantity);

                    for (int IO_2 = 0; IO_2 < IO_Elements_2B_Quantity; IO_2++)
                    {
                        var parameterID = (byte)Convert.ToInt32(receiveBytes.Skip(currentCursor + 1 + IO_2 * 3).Take(1).ToList()[0]);
                        string value = string.Empty;
                        receiveBytes.Skip(currentCursor + 2 + IO_2 * 3).Take(2).ToList().ForEach(delegate(byte b) { value += String.Format("{0:X2}", b); });
                        var IO_Element_2B = Convert.ToInt16(value, 16);
                        gpsData.IO_Elements_2B.Add(parameterID, IO_Element_2B);
                        ShowDiagnosticInfo("IO element 2B ID: --------".PadRight(40, '-') + " "  + parameterID);
                        ShowDiagnosticInfo(IO_2 + "'st 2B IO element value: --------".PadRight(40 - IO_2.ToString().Length, '-') + " "  + IO_Element_2B);
                    }
                    currentCursor += IO_Elements_2B_Quantity * 3 + 1;

                    int IO_Elements_4B_Quantity = Convert.ToInt32(receiveBytes.Skip(currentCursor).Take(1).ToList()[0]);
                    ShowDiagnosticInfo("4 byte IO element in record: --------".PadRight(40, '-') + " " + IO_Elements_4B_Quantity);

                    for (int IO_4 = 0; IO_4 < IO_Elements_4B_Quantity; IO_4++)
                    {
                        var parameterID = (byte)Convert.ToInt32(receiveBytes.Skip(currentCursor + 1 + IO_4 * 5).Take(1).ToList()[0]);
                        string value = string.Empty;
                        receiveBytes.Skip(currentCursor + 2 + IO_4 * 5).Take(4).ToList().ForEach(delegate(byte b) { value += String.Format("{0:X2}", b); });
                        var IO_Element_4B = Convert.ToInt32(value, 16);
                        gpsData.IO_Elements_4B.Add(parameterID, IO_Element_4B);
                        ShowDiagnosticInfo("IO element 4B ID: --------".PadRight(40, '-') + " "  + parameterID);
                        ShowDiagnosticInfo(IO_4 + "'st 4B IO element value: --------".PadRight(40 - IO_4.ToString().Length, '-') + " "  + IO_Element_4B);
                    }
                    currentCursor += IO_Elements_4B_Quantity * 5 + 1;

                    int IO_Elements_8B_Quantity = Convert.ToInt32(receiveBytes.Skip(currentCursor).Take(1).ToList()[0]);
                    ShowDiagnosticInfo("8 byte IO element in record: --------".PadRight(40, '-') + " " + IO_Elements_8B_Quantity);

                    for (int IO_8 = 0; IO_8 < IO_Elements_8B_Quantity; IO_8++)
                    {
                        var parameterID = (byte)Convert.ToInt32(receiveBytes.Skip(currentCursor + 1 + IO_8 * 9).Take(1).ToList()[0]);
                        string value = string.Empty;
                        receiveBytes.Skip(currentCursor + 2 + IO_8 * 9).Take(8).ToList().ForEach(delegate(byte b) { value += String.Format("{0:X2}", b); });
                        var IO_Element_8B = Convert.ToInt64(value, 16);
                        gpsData.IO_Elements_8B.Add(parameterID, IO_Element_8B);
                        ShowDiagnosticInfo("IO element 8B ID: --------".PadRight(40, '-') + " "  + parameterID);
                        ShowDiagnosticInfo(IO_8 + "'st 8B IO element value: --------".PadRight(40 - IO_8.ToString().Length, '-') + " "  + IO_Element_8B);
                    }

                    tokenAddress += 30 + IO_Elements_1B_Quantity * 2 +
                        IO_Elements_2B_Quantity * 3 + IO_Elements_4B_Quantity * 5
                        + IO_Elements_8B_Quantity * 9;
                }
                else
                {                   
                    tokenAddress += 30;
                }

                Data dt = new Data();
               
                gpsData.Altitude = (short)altitude;
                gpsData.Direction = (short)angle;
                gpsData.Lat = latitude;
                gpsData.Long = longtitude;
                gpsData.Priority = (byte)priority;
                gpsData.Satellite = (byte)satellites;
                gpsData.Speed = (short)speed;
                gpsData.Timestamp = timestamp;
                gpsData.IMEI = IMEI.Substring(0, 15);
                dt.SaveGPSPositionFMXXXX(gpsData);

            }
            //CRC for check of data correction and request again data from device if it not correct
            string crcString = string.Empty;
            receiveBytes.Skip(dataLength + 8).Take(4).ToList().ForEach(delegate(byte b) { crcString += String.Format("{0:X2}", b); });
            int CRC = Convert.ToInt32(crcString, 16);
            ShowDiagnosticInfo("CRC: -----".PadRight(40, '-') + " "  + CRC);
            //We must skeep first 8 bytes and last 4 bytes with CRC value.
            int calculatedCRC = GetCRC16(receiveBytes.Skip(8).Take(receiveBytes.Count - 12).ToArray());
            ShowDiagnosticInfo("Calculated CRC: -------".PadRight(40, '-') + " "  + calculatedCRC);
            ShowDiagnosticInfo("||||||||||||||||||||||||||||||||||||||||||||||||");
            if (calculatedCRC == CRC)
                return numberOfData;
            else
            {
                ShowDiagnosticInfo("Incorect CRC ");
                return 0;
            }
        }
        private int GetCRC16(byte[] buffer)
        {
            return GetCRC16(buffer, buffer.Length, 0xA001);
        }
        private int GetCRC16(byte[] buffer, int bufLen, int polynom)
        {            
            polynom &= 0xFFFF;
            int crc = 0;
            for (int i = 0; i < bufLen; i++)
            {
                int data = buffer[i] & 0xFF;
                crc ^= data;
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc = (crc >> 1) ^ polynom;
                    }
                    else
                    {
                        crc = crc >> 1;
                    }
                }
            }
            return crc & 0xFFFF;
        }
    }  
}
