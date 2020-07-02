using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

public class TCP_Client
{
    public static void Main(string[] arg)
    {
        string args0 = "127.0.0.1"; // ip
        string args1 = "4211"; // 포트 번호
        string args2 = "Hello"; // 메시지

        TcpClient client = new TcpClient(); // (ip주소 , 포트 번호)

        while(true)
        {
            try
            {
                client.Connect(args0, Int32.Parse(args1));
                Console.WriteLine("서버에 연결되었습니다.");
                break;
            }
            catch
            {
                Console.WriteLine("서버에 연결이 되지 않습니다. 재시도 중...");
            }
        }
            
        NetworkStream ns = client.GetStream();
        StreamWriter writer = new StreamWriter(ns);

        while(true)
        {
            Console.Write("보낼 문자열 입력 : ");
            args2 = Console.ReadLine();

            try
            {
                writer.WriteLine(args2);
                writer.Flush();
            }
            catch
            {
                Console.WriteLine("서버와 연결이 끊어졌습니다.");
                break;
            }
        }
    }
}
