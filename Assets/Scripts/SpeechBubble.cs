using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public delegate void TurnChangedEventHandler(object sender, TurnChangedEventArgs e);

public class TurnChangedEventArgs : EventArgs
{
    public int Index {get;}

    public TurnChangedEventArgs(int index)
    {
        Index = index;
    }
}

public class SpeechBubble : MonoBehaviour
{
    public event EventHandler<TurnChangedEventArgs> TurnChanged;

    WaitForSecondsRealtime waitForSecondsRealtime;
    System.Random rand;

    TextMeshProUGUI bubble;
    GomokuMain.Stone lastTurn;
    [SerializeField] private GomokuMain.Stone _color; // already set in the inspector.
    public GomokuMain.Stone color
    {
        get => _color;
        private set
        {
            _color = value;
        }
    }
    bool isSpeecherTurn;

    public List<Speech> Speeches {get; private set;}
    public Speech speech {get; private set;}
    private int _SpeechIndex;
    public int SpeechIndex
    {
        get
        {
            return _SpeechIndex;
        }
        private set
        {
            // if (_SpeechIndex != value)
            // {
                _SpeechIndex = value;
            // }
        }
    }

    void Start()
    {
        waitForSecondsRealtime = new WaitForSecondsRealtime(0.11f);
        // Fixed RNG; USE WITH CAUTION
        rand = new System.Random(12344);
        // rand = new System.Random();

        bubble = GetComponentInChildren<TextMeshProUGUI>();
        if (bubble == null)
        {
            Debug.LogWarning($"TextMeshProUGUI component not found.");
            return;
        }

        if (GomokuMain.Instance == null)
        {
            Debug.LogWarning($"GomokuMain.Instance is null; Need to modify loadtime.");
            return;
        }

        if (SpeechContainers.Instance == null)
        {
            Debug.LogWarning($"SpeechContainers.Instance is null; Need to modify loadtime.");
            return;
        }

        if (color == GomokuMain.Stone.White)
        {
            Speeches = SpeechContainers.Instance.SpeechesWhite;
            Debug.Log($"{Speeches.Count} Speeches for white stone set.");
        }
        else if (color == GomokuMain.Stone.Black)
        {
            Speeches = SpeechContainers.Instance.SpeechesBlack;
            Debug.Log($"{Speeches.Count} Speeches for black stone set.");
        }

        if (Speeches == null || Speeches.Count == 0)
        {
            Debug.LogWarning($"Speeches for {color} is null or Speeches.Count for {color} is 0 (Empty array).");
        }

        StartCoroutine("Speak");
    }

    void Update()
    {
        if (GomokuMain.Instance == null) return;

        if (GomokuMain.Instance.stoneTurn != GomokuMain.Stone.None
            && GomokuMain.Instance.stoneTurn == color)
        {
            isSpeecherTurn = true;
        } else
        {
            isSpeecherTurn = false;
        }
    }

    void SetNextSpeech(int previousSpeechIndex = -1)
    {
        do
        {
            SpeechIndex = rand.Next(Speeches.Count);
            Debug.Log($"RNG generated :: {SpeechIndex}, previousSpeechIndex is {previousSpeechIndex}");
        } while (SpeechIndex == previousSpeechIndex);

        Debug.Log($"{color} picked the Index :: {SpeechIndex}");
        speech = Speeches[SpeechIndex];

        TurnChanged?.Invoke(this, new TurnChangedEventArgs(SpeechIndex));
    }
    
    IEnumerator Speak()
    {
        bubble.text = "";
        int i = 0;
        int safetyCounter = 0;
        const int MAX_WAIT_FRAMES = 1000;
        Debug.Log("Speak initiated.");

        while (!isSpeecherTurn && safetyCounter < MAX_WAIT_FRAMES)
        {
            safetyCounter++;
            yield return null;
        }
        Debug.Log("safetyCounter stopped.");

        if (safetyCounter >= MAX_WAIT_FRAMES)
        {
            Debug.LogWarning($"Timeout waiting for turn : {color}");
            yield break;
        }

        Debug.Log($"Speak() logic started.");
        while (true)
        {
            if (GomokuMain.Instance == null
            || GomokuMain.Instance.stoneWinner != GomokuMain.Stone.None)
            {
                Debug.Log($"Speak() terminated");
                yield break;
            }

            if (lastTurn != GomokuMain.Instance.stoneTurn)
            {
                SetNextSpeech(SpeechIndex);
                lastTurn = GomokuMain.Instance.stoneTurn;
                i = 0;

                yield return null;
                continue;
            }
            // Debug.LogWarning($"Speak() :: Index changed.");

            if (isSpeecherTurn) 
            {
                bubble.text = "...";

                yield return null;
                continue;
            }
            // Debug.LogWarning($"Speak() :: Turn checked.");

            if (speech != null 
            && speech.speech != null 
            && i < speech.speech.Length)
            {
                bubble.text += speech.speech[i];
                i++;

                yield return null;
            } else
            {
                yield return null;
            }
            // Debug.LogWarning($"Speak() :: writing finished.");

        }
        Debug.Log($"Speak() logic ended; Unintentional behaviour happened.");

        yield return null;
    }
}
