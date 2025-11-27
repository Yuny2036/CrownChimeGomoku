using System;
using System.Collections.Generic;
using UnityEngine;

// 이런 식으로 할 거 같으면
// 그냥 여기에서 양쪽 대화내용 다 들고 있고
// 각 턴마다 한 쪽에서 한번씩 표정파일+텍스트 가져가게 하는게 낫지 않을까?
// 굳이 여기저기로 옮겨가면서 복사하기엔 품이 너무 많이 드는데

public class SpeechContainers : MonoBehaviour
{
    public static SpeechContainers Instance;
    TextAsset loadedJson;

    public SpeechContainersWrapper speechContainersWrapper;
    public List<Speech> SpeechesBlack {get; private set;}
    public List<Speech> SpeechesWhite {get; private set;}

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        loadedJson = Resources.Load<TextAsset>("JSON/Speech");
        if (loadedJson == null)
        {
            Debug.LogWarning("Failed to load JSON/Speech from Resources.");
            return;
        }
        Debug.Log(loadedJson.text);

        try
        {
            speechContainersWrapper = JsonUtility.FromJson<SpeechContainersWrapper>(loadedJson.text);

            // parse successful?
            if (speechContainersWrapper == null)
            {
                Debug.LogWarning("Failed to parse JSON : wrapper is null");
                return;
            }

            if (speechContainersWrapper.speechContainers == null
            || speechContainersWrapper.speechContainers.Count < 2)
            {
                Debug.LogWarning($"Speech list for black or white is invaild; Cannot use.");
                return;
            }

            SpeechesBlack = speechContainersWrapper.speechContainers[0]?.speeches;
            SpeechesWhite = speechContainersWrapper.speechContainers[1]?.speeches;

            if (SpeechesBlack == null
            || SpeechesWhite == null)
            {
                Debug.LogWarning("Failed to load speeches; Check Black or White array");
                return;
            }

            Debug.Log($"Loaded {SpeechesBlack.Count} speeches for black, {SpeechesWhite.Count} spechess for white.");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error parsing JSON :: \n {e.Message}");
            Debug.LogWarning($"StackTrace :: \n {e.StackTrace}");
        }
    }
}

[System.Serializable]
public class Speech
{
    public string speech;
    public string expression;
    public int index;
}

[System.Serializable]
public class SpeechContainer
{
    public string characterName;
    public List<Speech> speeches;
}

[System.Serializable]
public class SpeechContainersWrapper
{
    public List<SpeechContainer> speechContainers;
}