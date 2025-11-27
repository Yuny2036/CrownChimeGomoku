using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeechPortrait : MonoBehaviour
{
    string characterName;
    GomokuMain.Stone color;

    SpeechBubble speechBubble;
    [SerializeField] GameObject winmark;

    List<Texture> textures;
    Texture textureBlackWin;
    Texture textureWhiteWin;
    Texture textureBlackLose;
    Texture textureWhiteLose;
    RawImage rawImage;

    void Start()
    {
        textures = new List<Texture>();

        speechBubble = GetComponentInChildren<SpeechBubble>();
        rawImage = GetComponent<RawImage>();

        if (speechBubble == null)
        {
            Debug.LogWarning("SpeechBubble couldn't found.");
            return;
        }
        
        // Lazy.
        {
            textureBlackWin = Resources.Load<Texture>("Expression_StoneBlack/ChimeWin");
            textureBlackLose = Resources.Load<Texture>("Expression_StoneBlack/ChimeLose");
            textureWhiteWin = Resources.Load<Texture>("Expression_StoneWhite/CrownWin");
            textureWhiteLose = Resources.Load<Texture>("Expression_StoneWhite/CrownLose");
        }
        
        if (speechBubble.color == GomokuMain.Stone.Black) {
            characterName = "Black/Chime";
            color = GomokuMain.Stone.Black;
        } else if (speechBubble.color == GomokuMain.Stone.White)
        {
            characterName = "White/Crown";
            color = GomokuMain.Stone.White;
        }

        if (speechBubble.Speeches == null)
        {
            Debug.LogWarning("List<Speech> Speeches list is null!");
            return;
        }

        foreach (var speech in speechBubble.Speeches)
        {
            string path = "Expression_Stone" + characterName + speech.expression;
            Texture loadedTexture = Resources.Load<Texture>(path);

            if (loadedTexture == null)
            {
                Debug.LogWarning($"Failed to load texture : {path}");
            }

            textures.Add(loadedTexture);
        }

        if (speechBubble != null) speechBubble.TurnChanged += HandleTurnChange;
    }

    void Update()
    {
        // Lazy.
        if (GomokuMain.Instance.stoneWinner != GomokuMain.Stone.None)
        {
            switch (color)
            {
                case GomokuMain.Stone.Black:
                    if (GomokuMain.Instance.stoneWinner == color)
                    {
                        rawImage.texture = textureBlackWin;
                        winmark.SetActive(true);
                    } else
                    {
                        rawImage.texture = textureBlackLose;
                    }
                    break;
                
                case GomokuMain.Stone.White:
                    if (GomokuMain.Instance.stoneWinner == color)
                    {
                        rawImage.texture = textureWhiteWin;
                        winmark.SetActive(true);
                    } else
                    {
                        rawImage.texture = textureWhiteLose;
                    }
                    break;
            }
        }
        
    }

    public void HandleTurnChange(object sender, TurnChangedEventArgs e)
    {
        if (textures != null && e.Index >= 0 && e.Index < textures.Count)
        {
            if (textures[e.Index] != null)
            {        
                rawImage.texture = textures[e.Index];
            }
            else
            {
                Debug.LogWarning($"Texture at index {e.Index} is null.");
            }
        }
    }

    void OnDestroy()
    {
        if (speechBubble != null) speechBubble.TurnChanged -= HandleTurnChange;
    }
}
