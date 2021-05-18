using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

static class Define
{
    public const int recvBufferSize = 8192;
    public const int portNum = 4211;
}

public class TCP_Client
{
    public Socket mainSock = null;
    public Socket mySocket;
    public List<BKSClient> myClientsList = null;

    public static void Main(string[] arg)
    {
        // 소켓 생성
        Socket mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

        // 클라이언트 리스트 초기화
        List<BKSClient> myClientsList = new List<BKSClient>();

        // 소켓 오픈
        IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, Define.portNum);

        // 소켓 바인드
        mainSock.Bind(serverEP);

        // 소켓 리슨
        mainSock.Listen(10); // Why 10 ???

        while (true)
        {
            try
            {
                // Accept 시작
                mainSock.BeginAccept(this.ClinetAccept, null);
            }
            catch (Exception ex)
            {
                // Debug.Log("FirstBeginAccept() " + ex.ToString());
            }
        }
    }

    public static void ClinetAccept(IAsyncResult ar)
    {
        Socket acceptedSocket = null;
        BKSClient newClient = null;

        try
        {
            // 클라이언트의 연결 요청 수락
            acceptedSocket = mainSock.EndAccept(ar);
        }
        catch (Exception ex)
        {
            // Debug.Log("EndAccept() " + ex.ToString());
        }

        try
        {
            // 또 다른 클라이언트의 연결 대기
            mainSock.BeginAccept(ClinetAccept, null);
        }
        catch (Exception ex)
        {
            // Debug.Log("BeginAccept() " + ex.ToString());
        }

        try
        {
            // 새로운 클라이언트 객체 생성
            newClient = new BKSClient(this, acceptedSocket); ;

            // 새로운 클라이언트 객체 배열에 추가
            myClientsList.Add(newClient);
        }
        catch (Exception ex)
        {
            // Debug.Log("new MyClient() " + ex.ToString());
        }

        if (newClient != null)
        {
            try
            {
                // 비동기적 Recv 시작
                acceptedSocket.BeginReceive(newClient.recvBuffer, 0, Define.recvBufferSize, 0, newClient.PacketReceived, newClient);
            }
            catch (Exception ex)
            {
                // Debug.Log("BeginReceive() " + ex.ToString());
            }
        }
    }

    public void DisconnectClient(BKSClient _localBKSClient)
    {
        // 클라이언트 리스트에서 연결이 끊어진 클라이언트 제거
        myClientsList.Remove(_localBKSClient);
    }
}
