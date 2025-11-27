using System;
using System.Net.Sockets;
using System.Threading;

// IsSocketConnected 정확성 테스트 (연결 끊김 시점)
public class SocketConnectionStatusTest
{
    private const int PORT = 10004;
    private const string IP = "127.0.0.1";
    private Tcp serverTcp = new Tcp();
    private Socket rawClientSocket; // Poll의 동작을 테스트하기 위해 클라이언트 소켓에 직접 접근

    private void DebugLog(string message) => Console.WriteLine(message);

    public void Run()
    {
        Console.WriteLine("--- 5. IsSocketConnected Accuracy Test Start ---");
        
        // 1. 서버 시작
        if (!serverTcp.StartServer(PORT, 1))
        {
            Console.WriteLine("IsSocketConnected Test: FAIL (Server failed to start)");
            return;
        }
        Thread.Sleep(100);

        // 2. Raw Socket 클라이언트 연결 및 서버 소켓 획득
        if (!ConnectRawClient(PORT))
        {
            serverTcp.StopServer();
            Console.WriteLine("IsSocketConnected Test: FAIL (Raw client connection failed)");
            return;
        }
        Thread.Sleep(500); // 서버의 WaitClient()가 클라이언트 소켓을 설정할 시간

        if (serverTcp.IsSocketConnected())
        {
            DebugLog("Initial state: Server reports connected (PASS)");
        }
        else
        {
            DebugLog("Initial state: Server reports disconnected (FAIL)");
            serverTcp.StopServer();
            return;
        }

        // 3. 클라이언트 강제 연결 종료
        rawClientSocket.Shutdown(SocketShutdown.Both);
        rawClientSocket.Close();
        DebugLog("Raw client socket closed.");
        
        Thread.Sleep(50); // 상태 업데이트 시간

        // 4. 서버의 상태 확인 (Poll 테스트)
        // Poll(1, SelectMode.SelectRead) && Available == 0 인지 확인
        if (!serverTcp.IsSocketConnected())
        {
            Console.WriteLine("IsSocketConnected Test: PASS (Correctly detected disconnect)");
        }
        else
        {
            Console.WriteLine("IsSocketConnected Test: FAIL (Failed to detect disconnect)");
        }

        serverTcp.StopServer();
        Console.WriteLine("--- 5. IsSocketConnected Accuracy Test End ---");
    }

    private bool ConnectRawClient(int port)
    {
        try
        {
            rawClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            rawClientSocket.Connect(IP, port);
            return rawClientSocket.Connected;
        }
        catch (Exception e)
        {
            DebugLog($"Raw client connect error: {e.Message}");
            return false;
        }
    }
}