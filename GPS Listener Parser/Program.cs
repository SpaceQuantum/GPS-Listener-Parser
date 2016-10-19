using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using GPSParser;
using System.Net.Sockets;
using GPSParser.Teltonika;

public class MultiRecv
{
    public static void Main()
    {       
        Console.Write("Port to listen: ");
        int port = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine("Creating server...");
   
        AsynchronousIoServer Serv = new AsynchronousIoServer(port);
        Serv.Start();       

        Console.ReadLine();               
    }   
}