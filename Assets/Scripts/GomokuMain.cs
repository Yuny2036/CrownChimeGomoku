using System;
using TMPro;
using UnityEngine;

public class GomokuMain : MonoBehaviour
{
    public static GomokuMain Instance;
    public GameObject gameOverPopup;

    public Texture textureWhite;
    public Texture textureBlack;
    public Texture textureBoard;
    public int offsetHorizontal;
    public int offsetVertical;
    public float sizeBoard;
    public float sizeStone;

    Tcp tcp;
    public TMP_InputField inputFieldIP;

    int[,] boardCoordinate = new int[19, 19];
    GameState gameState;
    public Stone stoneTurn {get; private set;}
    Stone stoneMe;
    Stone stoneOpponent;
    public Stone stoneWinner {get; private set;} = Stone.None;

    void Start()
    {
        Instance = this;

        if (NetworkMaster.Instance == null)
        {
            Debug.LogError("NetworkMaster's Instance is null!");
            return;
        }

        tcp = NetworkMaster.Instance.tcp;

        if (tcp == null)
        {
            Debug.LogError("TCP Component is null!");
            return;
        }

        gameState = GameState.Start;

        // Size check
        {
            sizeBoard = (Screen.height <= Screen.width)
                ? (Screen.height / 2) - (Screen.height / 10)
                : (Screen.width / 2) - (Screen.width / 10);
            sizeStone = sizeBoard / 18; 

            offsetVertical = (Screen.height / 2) - 512 ;    
            offsetHorizontal = (Screen.width / 2) - 512;    
        }

        for (int row = 0; row < boardCoordinate.GetLength(0); row++)
        {
            for (int column = 0; column < boardCoordinate.GetLength(1); column++)
            {
                boardCoordinate[row, column] = (int)Stone.None;
            }
        }
    }

    void Update()
    {
        if (!tcp.IsReadyForCommunication())
        {
            Debug.LogWarning("TCP is not ready for communication!");
            return;
        }

        if (gameState == GameState.Title)
        {
            UpdateTitle();
        }

        if (gameState == GameState.Start)
        {
            UpdateStart();
        }

        if (gameState == GameState.PlayingGame)
        {
            UpdatePlayingGame();
        }

        if (gameState == GameState.End)
        {
            UpdateEnd();
        }
    }

    void OnGUI()
    {
        // sizeBoard = (Screen.height <= Screen.width) ? (Screen.height / 2) - (Screen.height / 10) : (Screen.width / 2) - (Screen.width / 10);
        // sizeStone = sizeBoard / 18;

        // offsetVertical = (Screen.height / 2) - 512 ;
        // offsetHorizontal = (Screen.width / 2) - 512;
        
        if (!Event.current.type.Equals(EventType.Repaint)) return;

        Graphics.DrawTexture(new Rect(offsetHorizontal, offsetVertical, 1024, 1024), textureBoard);

        if (gameState == GameState.PlayingGame)
        {
            if (stoneTurn == Stone.Black)
            {
                Graphics.DrawTexture(new Rect(64, 64, 128, 128), textureBlack);
            } else
            {
                Graphics.DrawTexture(new Rect(Screen.width - 192, 64, 128, 128), textureWhite);
            }
        }

        for (int i = 0; i < boardCoordinate.GetLength(0); i++)
        {
            for (int j = 0; j < boardCoordinate.GetLength(1); j++)
            {
                if (boardCoordinate[i, j] != (int)Stone.None)
                {
                    float x = offsetHorizontal - (56.89f / 2) + (i * 56.89f);
                    float y = offsetVertical - (56.89f / 2) + (j * 56.89f);

                    Texture texture = boardCoordinate[i, j] == (int)Stone.White ? textureWhite : textureBlack;
                    Graphics.DrawTexture(new Rect(x, y, 56.89f, 56.89f), texture);
                }
            }
        }


    }

    void UpdateStart()
    {
        // Debug.Log("Current GameState: Start");

        gameState = GameState.PlayingGame;
        stoneTurn = Stone.Black;

        if (tcp.IsServer())
        {
            stoneMe = Stone.Black;
            stoneOpponent = Stone.White;
        }
        else
        {
            stoneMe = Stone.White;
            stoneOpponent = Stone.Black;
        }
    }

    void UpdatePlayingGame()
    {
        // Debug.Log("Current GameState: PlayingGame");

        bool bSet = false;

        if (stoneTurn == stoneMe)
        {
            bSet = MyTurn();
        }
        else
        {
            bSet = YourTurn();
        }

        if (bSet == false) return;

        // Pseudo-Code::
        // if gameState is GameState.Calculating
        // Do CheckBoard once and change GameState to GameState.PlayingGame
        
        // if (gameState == GameState.Calculating)
        // {
        //     stoneWinner = CheckBoard();
        //     gameState = GameState.PlayingGame;
        // }

        stoneWinner = CheckBoard();
        
        if (stoneWinner != Stone.None)
        {
            gameState = GameState.End;
            Debug.Log($"Winner: {(int)stoneWinner}");
        }

        stoneTurn = (stoneTurn == Stone.Black) ? Stone.White : Stone.Black;
    }
    void UpdateEnd()
    {
        // Debug.Log("Current GameState: End");
        gameOverPopup.SetActive(true);
    }

    void UpdateTitle()
    {
        
    }

    // public void ServerStart()
    // {
    //     tcp.StartServer(10000, 10);
    // }

    // public void ClientStart()
    // {
    //     tcp.Connect(inputFieldIP.text, 10000);
    // }

    public void SessionEnd()
    {
        if (tcp.IsServer())
        {
            ServerEnd();
        } else {
            ClientEnd();
        }
    }

    public void ServerEnd()
    {
        tcp.StopServer();
    }

    public void ClientEnd()
    {
        tcp.Disconnect();
    }

    bool SetStone(int row, int column, Stone stone)
    {
        if (boardCoordinate[row, column] == (int)Stone.None)
        {
            boardCoordinate[row, column] = (int)stone;
            Debug.Log("Stone Set. Color : " + stone.ToString());
            return true;
        }

        // Pseudo-Code
        // Needs to set gameState to GameState.Calculating

        return false;
    }

    (int row, int column) PositionToCoordinates(Vector3 pos)
    {
        float x = pos.x;
        float y = Screen.height - pos.y;

        if (x < 0f + offsetHorizontal || x >= 1024f + offsetHorizontal)
        {
            return (-1, -1);
        }

        if (y < 0f + offsetVertical || y >= 1024f + offsetVertical)
        {
            return (-1, -1);
        }

        int h = (int)((x - offsetHorizontal) / 56.89f);
        int v = (int)((y - offsetVertical) / 56.89f);

        if (h < 0 || h >= 19 || v < 0 || v >= 19)
        {
            return (-1, -1);
        }

        return (h, v);
    }

    bool MyTurn()
    {
        bool bClick = Input.GetMouseButtonDown(0);
        if (!bClick) return false;

        Vector3 pos = Input.mousePosition;
        Debug.Log($"Mouse X: {pos.x}, Mouse Y: {pos.y}");

        (int row, int column) = PositionToCoordinates(pos);
        if (row == -1 || column == -1) return false;

        bool bSet = SetStone(row, column, stoneMe);
        if (bSet == false) return false;

        // Character talking logic here.
        // ---


        byte[] data = new byte[2];
        data[0] = (byte)row;
        data[1] = (byte)column;
        tcp.Send(data, data.Length);

        Debug.Log($"Data sent, row: {row} || column: {column}");
        return true;
    }

    bool YourTurn()
    {
        byte[] data = new byte[2];
        int iSize = tcp.Receive(ref data, data.Length);

        if (iSize <= 0) return false;

        int row = (int)data[0];
        int column = (int)data[1];
        Debug.Log($"Coordinates received: {row}, {column}");

        bool result = SetStone(row, column, stoneOpponent);
        if (result == false) return false;
        return true;
    }

    Stone CheckBoard()
    {

        // Pseudo-Code
        // Check ONLY one color AT A TIME.
        
        // 0 = Blackstone
        // 1 = Whitestone
        for (int n = 0; n < 2; n++)
        {
            int color = (n == 0) ? (int)Stone.Black : (int)Stone.White;

            // Check rows
            // 19 Rows
            for (int i = 0; i < 19; i++)
            {
                // 15 Columns; No need to see the last 4s.
                for (int j = 0; j < 19 - 4; j++)
                {
                    // Only if same colored one set.
                    if (color == boardCoordinate[i, j])
                    {
                        int listup = 0;

                        for (int k = 0; k < 5; k++)
                        {
                            if (boardCoordinate[i, j + k] != color) break;
                            listup++;
                        }

                        if (listup == 5) return (Stone)color;
                    }
                }
            }

            // Check columns
            // 15 Rows; No need to see the last 4s.
            for (int i = 0; i < 19 - 4; i++)
            {
                // 19 Columns
                for (int j = 0; j < 19; j++)
                {
                    // Only if same colored one set.
                    if (color == boardCoordinate[i, j])
                    {
                        int listup = 0;

                        for (int k = 0; k < 5; k++)
                        {
                            if (boardCoordinate[i + k, j] != color) break;
                            listup++;
                        }

                        if (listup == 5) return (Stone)color;
                    }
                }
            }

            // Check left-tilts.
            // 15 Rows; No need to see the last 4s.
            for (int i = 0; i < 19 - 4; i++)
            {
                // 15 Columns; No need to see the last 4s.
                for (int j = 0; j < 19 - 4; j++)
                {
                    // Only if same colored one set.
                    if (color == boardCoordinate[i, j])
                    {
                        int listup = 0;

                        for (int k = 0; k < 5; k++)
                        {
                            if (boardCoordinate[i + k, j + k] != color) break;
                            listup++;
                        }

                        if (listup == 5) return (Stone)color;
                    }
                }
            }


            // Check right-tilts.
            // 15 Rows; No need to see the first 4s.
            for (int i = 4; i < 19; i++)
            {
                // 15 Columns; No need to see the last 4s.
                for (int j = 0; j < 19 - 4; j++)
                {
                    // Only if same colored one set.
                    if (color == boardCoordinate[i, j])
                    {
                        int listup = 0;

                        for (int k = 0; k < 5; k++)
                        {
                            if (boardCoordinate[i - k, j + k] != color) break;
                            listup++;
                        }

                        if (listup == 5) return (Stone)color;
                    }
                }
            }
            
        }

        return Stone.None;
    }

    int GetCountConsecutive(
        int[][] board,
        int row,
        int column,
        Stone color,
        int directionRow,
        int directionColumn)
    {
        int count = 0;
        int i = row + directionRow;
        int j = column + directionColumn;

        while ((i >= 0 && i < board.GetLength(0)) && (j >= 0 && j < board.GetLength(1)) && board[row][column] == (int)color)
        {
            count++;
            i = row + directionRow;
            j = column + directionColumn;
        }

        return count;
    }

    enum GameState
    {
        Start = 0,
        PlayingGame,
        Calculating,
        End,
        Title,
    }

    enum Turn
    {
        Me = 0,
        Opponent,
    }

    public enum Stone
    {
        None = 0,
        White,
        Black,
    }


}
