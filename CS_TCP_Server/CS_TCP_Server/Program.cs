using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

public class TCP_Server
{
    public static void Main(string[] args)
    {
        TcpListener tcp_Listener = new TcpListener(IPAddress.Any, 4211);
        tcp_Listener.Start();

        Console.WriteLine("[Server] Start");

        TcpClient client = tcp_Listener.AcceptTcpClient(); // 클라이언트와 접속

        Console.WriteLine("[Server] 클라이언트가 연결되었습니다.");

        NetworkStream ns = client.GetStream();
        StreamReader reader = new StreamReader(ns);

        string msg = "";

        while (true)
        {
            try
            {
                msg = reader.ReadLine();

                Console.WriteLine("[Client 메시지] : " + msg);
            }
            catch
            {
                Console.WriteLine("[Client] Disconnected");
                break;
            }
        }

        reader.Close();
        client.Close();
        tcp_Listener.Stop();
    }
}
