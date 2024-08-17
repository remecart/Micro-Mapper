using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ReadMapInfo : MonoBehaviour
{
    public static ReadMapInfo instance;

    [Header("Map Info")]
    public Info info;

    [Header("Difficulty Info")]
    public _difficultyBeatmaps difficulty;

    [Header("Settings")]
    public string folderPath;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        string infoPath = "info.dat";

        if (!folderPath.Contains("info.dat"))
        {
            infoPath = "Info.dat";
        }

        string rawData = File.ReadAllText(Path.Combine(folderPath, infoPath));
        info = JsonUtility.FromJson<Info>(rawData);

        // put calculate thing here
    }

    [Header("Offsets")]
    public float jumpDistance;
    public float jumpDuration;
    public float reactionTime;

    public void JumpDuration(float noteJumpSpeed, float startBeatOffset, float bpm)
    {
        float jDuration = 4f;
        float secondsPerBeat = 60 / bpm;

        while (noteJumpSpeed * secondsPerBeat * jDuration > 17.999f)
            jDuration /= 2;

        jDuration += startBeatOffset;
        Mathf.Clamp(jDuration, 0.25f, 9999f); // most swag use
        jumpDuration = jDuration;
        reactionTime = SpawnObjects.instance.GetRealTimeFromBeat(jumpDuration);
    }

    // public static float JumpDistance(float noteJumpSpeed, float startBeatOffset, float bpm)
    // {
    //     var secondsPerBeat = 60 / bpm;
    //     return jumpDuration * secondsPerBeat * noteJumpSpeed * 2;
    // }
}

[System.Serializable]
public class Info
{
    public string _version;
    public string _songName;
    public string _songSubName;
    public string _songAuthorName;
    public string _levelAuthorName;
    public float _beatsPerMinute;
    public float _shuffle;
    public float _shufflePeriod;
    public float _previewStartTime;
    public float _previewDuration;
    public string _songFilename;
    public string _coverImageFilename;
    public string _environmentName;
    public float _songTimeOffset;
    public _customData _customData;
    public List<_difficultyBeatmapSets> _difficultyBeatmapSets;
}

[System.Serializable]
public class _customData
{
    public _editors _editors;
}

[System.Serializable]
public class _editors
{
    public string _lastEditedBy;
}

[System.Serializable]
public class _difficultyBeatmapSets
{
    public string _beatmapCharacteristicName;
    public List<_difficultyBeatmaps> _difficultyBeatmaps;
}

[System.Serializable]
public class _difficultyBeatmaps
{
    public string _difficulty;
    public string _difficultyRank;
    public string _beatmapFilename;
    public float _noteJumpMovementSpeed;
    public float _noteJumpStartBeatOffset;
    public int _beatmapColorSchemeIdx;
    public int _environmentNameIdx;
}