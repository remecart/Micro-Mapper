using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using static LoadMap;

public class ReadMapInfo : MonoBehaviour
{
    public static ReadMapInfo instance;

    [Header("Map Info")]
    public Info info;

    [Header("Difficulty Info")]
    public _difficultyBeatmaps difficulty;

    [Header("Settings")]
    public string folderPath;

    public LoadMap loadmap;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        string infoPath = "info.dat";

        if (!folderPath.Contains("info.dat"))
        {
            infoPath = "Info.dat";
        }

        if (!Application.isEditor)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            if (arguments.Length > 1 && arguments[1].Trim() != "")
            {
                folderPath = Path.GetDirectoryName(arguments[1]) + "\\";
            }

            string rawData = File.ReadAllText(Path.Combine(folderPath, infoPath));
            info = JsonUtility.FromJson<Info>(rawData);

            if (arguments[1] != null)
            {
                for (int i = 0; i < info._difficultyBeatmapSets.Count; i++)
                {
                    for (int j = 0; j < info._difficultyBeatmapSets[i]._difficultyBeatmaps.Count; j++)
                    {
                        var name = Path.GetFileName(arguments[1]);
                        if (info._difficultyBeatmapSets[i]._difficultyBeatmaps[j]._beatmapFilename == name)
                        {
                            string beatchar = info._difficultyBeatmapSets[i]._beatmapCharacteristicName.ToString();

                            // i tried for like an hour to parse enum but it didnt work so i give up 
                            if (beatchar == "Standard") loadmap.beatchar = _beatmapCharacteristicName.Standard;
                            else if (beatchar == "Lawless") loadmap.beatchar = _beatmapCharacteristicName.Lawless;

                            string diff = info._difficultyBeatmapSets[i]._difficultyBeatmaps[j]._difficulty.ToString();
                            if (diff == "ExpertPlus") loadmap.diff = _difficulty.ExpertPlus;
                            else if (diff == "Expert") loadmap.diff = _difficulty.Expert;
                            else if (diff == "Hard") loadmap.diff = _difficulty.Hard;
                            else if (diff == "Normal") loadmap.diff = _difficulty.Normal;
                            else if (diff == "Easy") loadmap.diff = _difficulty.Easy;

                        }
                    }
                }
            }
        }
        else
        {
            string rawData = File.ReadAllText(Path.Combine(folderPath, infoPath));
            info = JsonUtility.FromJson<Info>(rawData);
        }
        // put calculate thing here
    }

    private void OnDestroy()
    {
        if (File.Exists(Path.Combine(folderPath, "imgui.ini"))) File.Delete(Path.Combine(folderPath, "imgui.ini"));
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