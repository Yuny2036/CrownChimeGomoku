using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine; // Tcp 클래스가 Unity 의존성을 가지므로 모의 환경 필요

// 단일 클라이언트 연결 및 데이터 송수신 테스트
public class TcpBasicCommunicationTest
{
    private const int PORT = 10001;
    private const string IP = "127.0.0.1";
    private Tcp serverTcp = new Tcp(); // 실제 테스트에서는 Mock 또는 Adapter 사용 고려
    private Tcp clientTcp = new Tcp();

    // Mock Unity Environment
    // 실제 Unity 환경이 아니므로 Debug.Log 대신 Console.WriteLine 사용
    private void DebugLog(string message) => Console.WriteLine(message);

    public void Run()
    {
        Console.WriteLine("--- 2. TCP Basic Communication Test Start ---");
        
        // 1. 서버 시작
        bool serverStarted = serverTcp.StartServer(PORT, 1);
        DebugLog($"Server Start Status: {serverStarted}");

        if (!serverStarted)
        {
            Console.WriteLine("TCP Basic Communication Test: FAIL (Server failed to start)");
            return;
        }

        // 2. 클라이언트 연결 시도
        // 클라이언트 연결 전에 서버가 WaitClient()를 실행할 시간을 줌
        Thread.Sleep(100); 
        bool clientConnected = clientTcp.Connect(IP, PORT);
        DebugLog($"Client Connect Status: {clientConnected}");

        // 연결 스레드가 동작할 시간을 줌
        Thread.Sleep(500);

        if (!serverTcp.IsConnect() || !clientTcp.IsConnect())
        {
            Console.WriteLine("TCP Basic Communication Test: FAIL (Connection failed)");
            serverTcp.StopServer();
            return;
        }

        // 3. 데이터 송신 (클라이언트 -> 서버)
        string testMessage = "Hello from Client!";
        byte[] sendData = Encoding.UTF8.GetBytes(testMessage);
        int sentSize = clientTcp.Send(sendData, sendData.Length);
        DebugLog($"Client sent {sentSize} bytes.");
        
        // 데이터 수신 및 처리 시간을 줌 (NetworkUpdate 스레드)
        Thread.Sleep(100); 

        // 4. 데이터 수신 (서버)
        byte[] receiveBuffer = new byte[1024];
        int receivedSize = serverTcp.Receive(ref receiveBuffer, receiveBuffer.Length);

        string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, receivedSize);
        DebugLog($"Server received: {receivedMessage} ({receivedSize} bytes)");

        // 5. 결과 검증 및 정리
        if (receivedSize > 0 && receivedMessage == testMessage)
        {
            Console.WriteLine("TCP Basic Communication Test: PASS (Data received matches sent data)");
        }
        else
        {
            Console.WriteLine($"TCP Basic Communication Test: FAIL (Received data mismatch: Expected '{testMessage}', Got '{receivedMessage}')");
        }

        clientTcp.Disconnect();
        serverTcp.StopServer();
        Console.WriteLine("--- 2. TCP Basic Communication Test End ---");
    }
}