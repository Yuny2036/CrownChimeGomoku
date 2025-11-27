using System;
using System.Threading;

// 연결 해제 및 재연결 안정성 테스트
public class TcpReconnectTest
{
    private const int PORT = 10002;
    private const string IP = "127.0.0.1";
    private Tcp serverTcp = new Tcp();
    private Tcp clientTcp = new Tcp();

    private void DebugLog(string message) => Console.WriteLine(message);

    public void Run()
    {
        Console.WriteLine("--- 3. TCP Reconnection Stability Test Start ---");

        // 1차 연결 및 해제
        if (!PerformConnectionAndDisconnect(serverTcp, clientTcp, PORT))
        {
            Console.WriteLine("TCP Reconnection Test: FAIL (Initial connection/disconnection failed)");
            return;
        }

        // 서버 재시작 및 2차 연결
        Console.WriteLine("\nAttempting Server Restart and Reconnect...");
        if (!PerformConnectionAndDisconnect(serverTcp, clientTcp, PORT))
        {
            Console.WriteLine("TCP Reconnection Test: FAIL (Second connection/disconnection failed)");
            return;
        }

        Console.WriteLine("TCP Reconnection Stability Test: PASS (Successfully connected and disconnected twice)");
        Console.WriteLine("--- 3. TCP Reconnection Stability Test End ---");
    }

    private bool PerformConnectionAndDisconnect(Tcp server, Tcp client, int port)
    {
        // Start Server
        bool serverStarted = server.StartServer(port, 1);
        if (!serverStarted) return false;
        Thread.Sleep(100);

        // Connect Client
        bool clientConnected = client.Connect(IP, port);
        Thread.Sleep(500); 

        // Check Connection
        if (!server.IsConnect() || !client.IsConnect())
        {
            server.StopServer();
            return false;
        }
        DebugLog("Connection 1 established.");

        // Disconnect Client and Stop Server
        client.Disconnect();
        server.StopServer();
        Thread.Sleep(500); 
        
        // Final State Check
        if (server.IsConnect() || client.IsConnect())
        {
            return false; // 연결이 남아있으면 실패
        }
        DebugLog("Disconnected successfully.");
        return true;
    }
}