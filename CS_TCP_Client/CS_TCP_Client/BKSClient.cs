using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
public class BKSClient
{
    // ========== TCP Module ==========
    public TCP_Client myTCP_Module = null;

    // ========== Network ==========
    public byte[] recvBuffer = new byte[Define.recvBufferSize];
    public Socket mySocket;
    public BKSClient(TCP_Client _TCP_Client, Socket _MySocket)
    {
        myTCP_Module = _TCP_Client;
        mySocket = _MySocket;
    }

    ~BKSClient()
    {
        // Debug.Log("Delete() " + mySocket.RemoteEndPoint.ToString());
    }

    // 아무 패킷이나 받으면 호출 되는 콜벡 함수
    public void PacketReceived(IAsyncResult ar)
    {
        int received = 0;

        try
        {
            // 패킷 수신 함수 호출
            received = mySocket.EndReceive(ar);

            // Debug.Log("EndReceive() called");
        }
        catch (Exception ex)
        {
            // Debug.Log("EndReceive() " + ex.ToString());
        }

        // 연결 끊김
        if (received == 0)
        {
            try
            {
                mySocket.Close();

                myTCP_Module.DisconnectClient(this);
            }
            catch (Exception ex)
            {
                // Debug.Log("Close() " + ex.ToString());
            }
            return;
        }

        try
        {
            // 패킷 처리 함수 호출
            ProccessRecv();
        }
        catch (Exception ex)
        {
            // Debug.Log("ProccessRecv() " + ex.ToString());
        }

        try
        {
            // 다시 비동기 수신 대기
            mySocket.BeginReceive(recvBuffer, 0, Define.recvBufferSize, 0, PacketReceived, this);
        }
        catch (Exception ex)
        {
            // Debug.Log("BeginReceive() " + ex.ToString());
        }
    }

    // 패킷 처리 함수
    public unsafe void ProccessRecv()
    {
        string debugLine = "[" + mySocket.RemoteEndPoint.ToString();
        string ipStr = "";
        string deviceIDStr = "";
        string timeStr = "";
        byte[] ipStrBits;
        byte[] timeStrBits;
        byte objNum;
        byte[] deviceIDStrBits;
        byte deviceStatusInformation;
        byte[] fourByteBits;

        // 패킷의 종류에 따라 분기
        switch ((int)recvBuffer[0])
        {
            case 1: // IF-SC100.header
                ipStrBits = new byte[32];
                Array.Copy(recvBuffer, 1, ipStrBits, 0, 32);
                ipStr = Encoding.Unicode.GetString(ipStrBits); // IF-SC100.IP
                ipStr = ipStr.Replace("\0", "");

                timeStrBits = new byte[54];
                Array.Copy(recvBuffer, 33, timeStrBits, 0, 54);
                timeStr = Encoding.Unicode.GetString(timeStrBits); // IF-SC100.time
                timeStr = timeStr.Replace("\0", "");

                objNum = recvBuffer[87]; // IF-SC100.objNum

                debugLine = debugLine + " IF-SC100] " + ipStr + " " + timeStr + " " + objNum + "\n";

                fourByteBits = new byte[4];

                for (int i = 0; i < objNum; ++i)
                {
                    debugLine = debugLine + i + " : " + recvBuffer[88 + i * 29] + " "; // IF-SC100.objType

                    Array.Copy(recvBuffer, 88 + i * 29 + 1, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToSingle(fourByteBits, 0) + " "; // IF-SC100.XAxis

                    Array.Copy(recvBuffer, 88 + i * 29 + 5, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToSingle(fourByteBits, 0) + " "; // IF-SC100.ZAxis

                    Array.Copy(recvBuffer, 88 + i * 29 + 9, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToInt32(fourByteBits, 0) + " "; // IF-SC100.objX1

                    Array.Copy(recvBuffer, 88 + i * 29 + 13, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToInt32(fourByteBits, 0) + " "; // IF-SC100.objY1

                    Array.Copy(recvBuffer, 88 + i * 29 + 17, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToInt32(fourByteBits, 0) + " "; // IF-SC100.objX2

                    Array.Copy(recvBuffer, 88 + i * 29 + 21, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToInt32(fourByteBits, 0) + " "; // IF-SC100.objY2

                    Array.Copy(recvBuffer, 88 + i * 29 + 25, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToSingle(fourByteBits, 0) + "\n"; // IF-SC100.percent
                }
                break;
            case 2: // IF-SC200.header
                deviceIDStrBits = new byte[16];
                Array.Copy(recvBuffer, 1, deviceIDStrBits, 0, 16);
                deviceIDStr = Encoding.Unicode.GetString(deviceIDStrBits); // IF-SC200.deviceID
                deviceIDStr = deviceIDStr.Replace("\0", "");

                timeStrBits = new byte[20];
                Array.Copy(recvBuffer, 17, timeStrBits, 0, 20);
                timeStr = Encoding.Unicode.GetString(timeStrBits); // IF-SC200.time
                timeStr = timeStr.Replace("\0", "");

                byte fireAndSmokeStatusInformation = recvBuffer[37]; // IF-SC200.fireAndSmokeStatusInformation

                deviceStatusInformation = recvBuffer[38]; // IF-SC200.deviceStatusInformation

                debugLine += " IF-SC200] " + ipStr + " " + timeStr + " " +
                    fireAndSmokeStatusInformation + " " + deviceStatusInformation + "\n";
                break;
            case 3: // IF-SC300.header
                deviceIDStrBits = new byte[16];
                Array.Copy(recvBuffer, 1, deviceIDStrBits, 0, 16);
                deviceIDStr = Encoding.Unicode.GetString(deviceIDStrBits); // IF-SC300.deviceID
                ipStr = ipStr.Replace("\0", "");

                timeStrBits = new byte[20];
                Array.Copy(recvBuffer, 17, timeStrBits, 0, 20);
                timeStr = Encoding.Unicode.GetString(timeStrBits); // IF-SC300.time
                timeStr = timeStr.Replace("\0", "");

                byte eventStatusInformation = recvBuffer[37]; // IF-SC300.eventStatusInformation

                deviceStatusInformation = recvBuffer[38]; // IF-SC300.deviceStatusInformation

                byte emergencySituationInformation = recvBuffer[39]; // IF-SC300.emergencySituationInformation

                debugLine += " IF-SC300] " + ipStr + " " + timeStr + " " +
                    eventStatusInformation + " " + deviceStatusInformation + " " + emergencySituationInformation + "\n";
                break;
            case 4: // IF-SC400.header
                ipStrBits = new byte[32];
                Array.Copy(recvBuffer, 1, ipStrBits, 0, 32);
                ipStr = Encoding.Unicode.GetString(ipStrBits); // IF-SC400.IP
                ipStr = ipStr.Replace("\0", "");

                timeStrBits = new byte[54];
                Array.Copy(recvBuffer, 33, timeStrBits, 0, 54);
                timeStr = Encoding.Unicode.GetString(timeStrBits); // IF-SC400.time
                timeStr = timeStr.Replace("\0", "");

                objNum = recvBuffer[87]; // IF-SC400.objNum

                debugLine = debugLine + " IF-SC400] " + ipStr + " " + timeStr + " " + objNum + "\n";

                fourByteBits = new byte[4];

                for (int i = 0; i < objNum; ++i)
                {
                    debugLine = debugLine + i + " : " + recvBuffer[88 + i * 34] + " "; // IF-SC400.objType

                    Array.Copy(recvBuffer, 88 + i * 34 + 1, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToSingle(fourByteBits, 0) + " "; // IF-SC400.XAxis

                    Array.Copy(recvBuffer, 88 + i * 34 + 5, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToSingle(fourByteBits, 0) + " "; // IF-SC400.ZAxis

                    Array.Copy(recvBuffer, 88 + i * 34 + 9, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToInt32(fourByteBits, 0) + " "; // IF-SC400.objX1

                    Array.Copy(recvBuffer, 88 + i * 34 + 13, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToInt32(fourByteBits, 0) + " "; // IF-SC400.objY1

                    Array.Copy(recvBuffer, 88 + i * 34 + 17, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToInt32(fourByteBits, 0) + " "; // IF-SC400.objX2

                    Array.Copy(recvBuffer, 88 + i * 34 + 21, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToInt32(fourByteBits, 0) + " "; // IF-SC400.objY2

                    Array.Copy(recvBuffer, 88 + i * 34 + 25, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToSingle(fourByteBits, 0) + " "; // IF-SC400.percent

                    debugLine = debugLine + recvBuffer[88 + i * 34 + 29] + " "; // IF-SC400.action

                    debugLine = debugLine + recvBuffer[88 + i * 34 + 30] + " "; // IF-SC400.color

                    debugLine = debugLine + recvBuffer[88 + i * 34 + 31] + " "; // IF-SC400.Red
                    debugLine = debugLine + recvBuffer[88 + i * 34 + 32] + " "; // IF-SC400.Green
                    debugLine = debugLine + recvBuffer[88 + i * 34 + 33] + "\n"; // IF-SC400.Blue
                }
                break;
            case 5: // IF-SC500.header
                ipStrBits = new byte[32];
                Array.Copy(recvBuffer, 1, ipStrBits, 0, 32);
                ipStr = Encoding.Unicode.GetString(ipStrBits); // IF-SC500.IP
                ipStr = ipStr.Replace("\0", "");

                timeStrBits = new byte[54];
                Array.Copy(recvBuffer, 33, timeStrBits, 0, 54);
                timeStr = Encoding.Unicode.GetString(timeStrBits); // IF-SC500.time
                timeStr = timeStr.Replace("\0", "");

                objNum = recvBuffer[87]; // IF-SC500.objNum

                debugLine = debugLine + " IF-SC500] " + ipStr + " " + timeStr + " " + objNum + "\n";

                fourByteBits = new byte[4];

                for (int i = 0; i < objNum; ++i)
                {
                    debugLine = debugLine + i + " : " + recvBuffer[88 + i * 29] + " "; // IF-SC500.objType

                    Array.Copy(recvBuffer, 88 + i * 29 + 1, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToSingle(fourByteBits, 0) + " "; // IF-SC500.XAxis

                    Array.Copy(recvBuffer, 88 + i * 29 + 5, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToSingle(fourByteBits, 0) + " "; // IF-SC500.ZAxis

                    Array.Copy(recvBuffer, 88 + i * 29 + 9, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToInt32(fourByteBits, 0) + " "; // IF-SC500.objX1

                    Array.Copy(recvBuffer, 88 + i * 29 + 13, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToInt32(fourByteBits, 0) + " "; // IF-SC500.objY1

                    Array.Copy(recvBuffer, 88 + i * 29 + 17, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToInt32(fourByteBits, 0) + " "; // IF-SC500.objX2

                    Array.Copy(recvBuffer, 88 + i * 29 + 21, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToInt32(fourByteBits, 0) + " "; // IF-SC500.objY2

                    Array.Copy(recvBuffer, 88 + i * 29 + 25, fourByteBits, 0, 4);
                    debugLine = debugLine + BitConverter.ToSingle(fourByteBits, 0) + "\n"; // IF-SC500.percent
                }
                break;
            default:
                // Debug.Log("recv wrong data " + recvBuffer[0]);
                break;
        }

        // 로그 출력 할 것이 있으면 큐에 넣음
        // scrollController.logQueue.Enqueue((myScroll, myTextParent, debugLine));
    }
}

