using System.Data.Common;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkMaster : MonoBehaviour
{
    public static NetworkMaster Instance;
    public Tcp tcp {get; private set;}
    public TMP_InputField inputFieldIP;

    bool isPlayingGame = false;
    bool isWaitingForConnection = false;

    private bool hasSentReadySignal = false;
    private bool hasReceivedReadySignal = false;
    private float handshakeTimeout = 10f;
    private float connectionStartTime = 0f;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this; 
        DontDestroyOnLoad(gameObject);
        tcp = GetComponent<Tcp>();

    }

    void Update()
    {
        // if (tcp.IsConnect() && !isPlayingGame)
        // {
        //     isPlayingGame = true;
        //     SceneManager.LoadScene(1);
        // }

        // if (!tcp.IsConnect() && isPlayingGame)
        // {
        //     isPlayingGame = false;
        //     SceneManager.LoadScene(0);
        // }

        if (isWaitingForConnection && !isPlayingGame)
        {
            ProcessHandshake();
        }

        if (!tcp.IsReadyForCommunication() && isPlayingGame)
        {
            isPlayingGame = false;
            ResetConnectionState();
            SceneManager.LoadScene(0);
            Debug.Log("Connection Lost. Returning to lobby..");
        }
    }

    void OnDestroy()
    {
        if (tcp != null)
        {
            if (tcp.IsConnect())
            {
                tcp.StopServer();
            }
            else if (tcp.IsConnect())
            {
                tcp.Disconnect();
            }
        }
    }

    void ProcessHandshake()
    {
        if (Time.time - connectionStartTime > handshakeTimeout)
        {
            Debug.LogError("Connection timeout!");
            ResetConnectionState();
            return;
        }

        if (!tcp.IsReadyForCommunication()) return;

        if (!hasSentReadySignal)
        {
            SendReadySignal();
            hasSentReadySignal = true;
        }

        CheckForReadySignal();

        if (hasSentReadySignal && hasReceivedReadySignal)
        {
            Debug.Log("Both sides ready! Loading game scene..");

            ClearReceiveQueue();

            isPlayingGame = true;
            isWaitingForConnection = false;
            SceneManager.LoadScene(1);
        }
    }

    void SendReadySignal()
    {
        string message = "READY";
        byte[] data = Encoding.UTF8.GetBytes(message);

        tcp.Send(data, data.Length);

        Debug.Log("Sent READY signal.");
    }

    void CheckForReadySignal()
    {
        byte[] buffer = new byte[1024];
        int receivedSize = tcp.Receive(ref buffer, buffer.Length);

        if (receivedSize > 0)
        {
            string message = Encoding.UTF8.GetString(buffer, 0, receivedSize);
            if (message.Contains("READY"))
            {
                hasReceivedReadySignal = true;
                Debug.Log("Received READY signal from peer.");
            }
        }
    }

    void ClearReceiveQueue()
    {
        byte[] dummy = new byte[1024];
        while (tcp.Receive(ref dummy, dummy.Length) > 0)
        {
            
        }
        Debug.Log("Receive queue cleared.");
    }

    void ResetConnectionState()
    {
        isWaitingForConnection = false;
        hasSentReadySignal = false;
        hasReceivedReadySignal = false;
        connectionStartTime = 0f;
    }

    public void SetPlayingState(bool state)
    {
        isPlayingGame = state;
    }
    
    public Tcp GetTCP()
    {
        return tcp;
    }

    public void ServerStart()
    {
        if (tcp.IsServer())
        {
            Debug.LogWarning("Server is already running!");
            return;
        }

        bool success = tcp.StartServer(10000, 10);
        if (success)
        {
            ResetConnectionState();
            isWaitingForConnection = true;
            connectionStartTime = Time.time;
            Debug.Log("Server started. Waiting for client..");
        }
    }

    public void ClientStart()
    {
        // if (inputFieldIP != null) tcp.Connect(inputFieldIP.text, 10000);
        if (inputFieldIP != null)
        {
            bool success = tcp.Connect(inputFieldIP.text, 10000);
            if (success)
            {
                ResetConnectionState();
                isWaitingForConnection = true;
                connectionStartTime = Time.time;
                Debug.Log("Connected to server. starting handshake..");
            }
        }
    }

    public void ServerEnd()
    {
        ResetConnectionState();
        tcp.StopServer();
    }

    public void ClientEnd()
    {
        ResetConnectionState();
        tcp.Disconnect();
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}
