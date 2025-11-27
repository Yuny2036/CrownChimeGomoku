using System;
using System.Threading;

// 고모쿠 데이터 송수신 테스트 (2바이트 좌표)
public class GomokuDataTransferTest
{
    private const int PORT = 10003;
    private const string IP = "127.0.0.1";
    private Tcp serverTcp = new Tcp();
    private Tcp clientTcp = new Tcp();

    private void DebugLog(string message) => Console.WriteLine(message);

    public void Run()
    {
        Console.WriteLine("--- 4. Gomoku Data (2-byte Coordinate) Transfer Test Start ---");

        // 1. 서버 시작 및 클라이언트 연결
        if (!StartAndConnect(serverTcp, clientTcp, PORT))
        {
            Console.WriteLine("Gomoku Data Transfer Test: FAIL (Connection failed)");
            return;
        }

        // 2. 클라이언트 -> 서버 데이터 송신 (좌표: 18, 5)
        int expectedRow = 18; // 0-18 범위 테스트
        int expectedCol = 5;
        byte[] sendData = new byte[2] { (byte)expectedRow, (byte)expectedCol };
        int sentSize = clientTcp.Send(sendData, sendData.Length);
        DebugLog($"Client sent {sentSize} bytes: Row={expectedRow}, Col={expectedCol}");
        
        Thread.Sleep(100); 

        // 3. 서버 수신
        byte[] receiveBuffer = new byte[1024];
        int receivedSize = serverTcp.Receive(ref receiveBuffer, receiveBuffer.Length);

        // 4. 데이터 검증
        if (receivedSize == 2)
        {
            int receivedRow = (int)receiveBuffer[0];
            int receivedCol = (int)receiveBuffer[1];
            
            if (receivedRow == expectedRow && receivedCol == expectedCol)
            {
                Console.WriteLine("Gomoku Data Transfer Test: PASS (Received coordinates match sent data)");
            }
            else
            {
                Console.WriteLine($"Gomoku Data Transfer Test: FAIL (Data mismatch: Expected R{expectedRow}C{expectedCol}, Got R{receivedRow}C{receivedCol})");
            }
        }
        else
        {
            Console.WriteLine($"Gomoku Data Transfer Test: FAIL (Expected 2 bytes, Got {receivedSize} bytes)");
        }

        // 5. 정리
        clientTcp.Disconnect();
        serverTcp.StopServer();
        Console.WriteLine("--- 4. Gomoku Data Transfer Test End ---");
    }
    
    private bool StartAndConnect(Tcp server, Tcp client, int port)
    {
        if (!server.StartServer(port, 1)) return false;
        Thread.Sleep(100);
        if (!client.Connect(IP, port)) return false;
        Thread.Sleep(500);
        return server.IsConnect() && client.IsConnect();
    }
}