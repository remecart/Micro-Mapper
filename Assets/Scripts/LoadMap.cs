using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Runtime.Serialization.Formatters.Binary;

public class LoadMap : MonoBehaviour
{
    public static LoadMap instance;
    public GameObject text;
    public float mappingTime;
    public enum _difficulty
    {
        Easy,
        Normal,
        Hard,
        Expert,
        ExpertPlus
    }

    public enum _beatmapCharacteristicName
    {
        Standard,
        Lawless
    }

    [Header("Map Settings")]
    public _difficulty diff;
    public _beatmapCharacteristicName beatchar;


    [Header("Loaded Map")]
    public List<beatsV3> beats;
    public List<bpmEvents> bpmEvents;
    public List<obstacles> obstacles;

    public SpawnObjects spawnObjects;

    void Start()
    {
        instance = this;
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
        {
            Save();
        }

        mappingTime += Time.unscaledDeltaTime / 60f;
    }

    public void Save(bool closeEditor = false)
    {
        if (ReadMapInfo.instance == null || ReadMapInfo.instance.info == null ||
            ReadMapInfo.instance.info._difficultyBeatmapSets == null ||
            ReadMapInfo.instance.folderPath == null)
        {
            Debug.LogError("One or more required objects are null.");
            return;
        }

        if (beats == null || beats.Count == 0)
        {
            Debug.LogError("Beats list is null or empty.");
            return;
        }

        beatsV3 beat = new beatsV3
        {
            colorNotes = new List<colorNotes>(),
            bombNotes = new List<bombNotes>(),
            obstacles = new List<obstacles>(),
            bpmEvents = new List<bpmEvents>(),
            sliders = new List<sliders>(),
            burstSliders = new List<burstSliders>(),
            basicBeatmapEvents = new List<basicBeatmapEvents>(),
            timings = new List<timings>(),
            customData = new customData()
        };

        beat.version = "3.3.0";

        for (int i = 0; i < beats.Count - 1; i++)
        {
            if (beats[i] == null)
            {
                Debug.LogError($"beats[{i}] is null.");
                continue;
            }

            if (beats[i].colorNotes != null)
            {
                foreach (var item in beats[i].colorNotes)
                {
                    if (item != null)
                    {
                        beat.colorNotes.Add(item);
                    }
                    else
                    {
                        Debug.LogWarning($"Null colorNote at index {i}.");
                    }
                }
            }

            if (beats[i].bombNotes != null)
            {
                foreach (var item in beats[i].bombNotes)
                {
                    if (item != null)
                    {
                        beat.bombNotes.Add(item);
                    }
                    else
                    {
                        Debug.LogWarning($"Null bombNote at index {i}.");
                    }
                }
            }

            if (beats[i].obstacles != null)
            {
                foreach (var item in beats[i].obstacles)
                {
                    if (item != null)
                    {
                        beat.obstacles.Add(item);
                    }
                    else
                    {
                        Debug.LogWarning($"Null obstacle at index {i}.");
                    }
                }
            }
            if (beats[i].bpmEvents != null)
            {
                foreach (var item in beats[i].bpmEvents)
                {
                    if (item != null)
                    {
                        beat.bpmEvents.Add(item);
                    }
                    else
                    {
                        Debug.LogWarning($"Null bpmEvent at index {i}.");
                    }
                }
            }
            if (beats[i].sliders != null)
            {
                foreach (var item in beats[i].sliders)
                {
                    if (item != null)
                    {
                        beat.sliders.Add(item);
                    }
                    else
                    {
                        Debug.LogWarning($"Null sliders at index {i}.");
                    }
                }
            }
            if (beats[i].burstSliders != null)
            {
                foreach (var item in beats[i].burstSliders)
                {
                    if (item != null)
                    {
                        beat.burstSliders.Add(item);
                    }
                    else
                    {
                        Debug.LogWarning($"Null burstSliders at index {i}.");
                    }
                }
            }
            if (beats[i].basicBeatmapEvents != null)
            {
                foreach (var item in beats[i].basicBeatmapEvents)
                {
                    if (item != null)
                    {
                        beat.basicBeatmapEvents.Add(item);
                    }
                    else
                    {
                        Debug.LogWarning($"Null basicBeatmapEvents at index {i}.");
                    }
                }
            }
            if (beats[i].timings != null)
            {
                foreach (var item in beats[i].timings)
                {
                    if (item != null)
                    {
                        beat.timings.Add(item);
                    }
                    else
                    {
                        Debug.LogWarning($"Null basicBeatmapEvents at index {i}.");
                    }
                }
            }
        }

        int beatmapSet = 0;
        for (int i = 0; i < ReadMapInfo.instance.info._difficultyBeatmapSets.Count; i++)
        {
            if (ReadMapInfo.instance.info._difficultyBeatmapSets[i]._beatmapCharacteristicName == beatchar.ToString())
            {
                beatmapSet = i;
                break;
            }
        }

        int beatmapDiff = 0;
        for (int i = 0; i < ReadMapInfo.instance.info._difficultyBeatmapSets[beatmapSet]._difficultyBeatmaps.Count; i++)
        {
            if (ReadMapInfo.instance.info._difficultyBeatmapSets[beatmapSet]._difficultyBeatmaps[i]._difficulty == diff.ToString())
            {
                beatmapDiff = i;
                break;
            }
        }

        _difficultyBeatmaps difficulty = ReadMapInfo.instance.info._difficultyBeatmapSets[beatmapSet]._difficultyBeatmaps[beatmapDiff];
        string name = difficulty._beatmapFilename.ToString();
        ReadMapInfo.instance.difficulty = difficulty;
        beat.customData.time = mappingTime;
        string rawJson = JsonUtility.ToJson(beat, true);
        File.WriteAllText(Path.Combine(ReadMapInfo.instance.folderPath, name), rawJson);

        text.GetComponent<TextMeshProUGUI>().text = "Map saved successfully!";
        text.gameObject.GetComponent<Animator>().SetTrigger("TriggerPopUp");

        if (closeEditor) Application.Quit();
    }

    public void Load(AudioSource audio)
    {
        int capacity = 40484; // only the real ones will understand

        for (int i = 0; i < capacity; i++)
        {
            beats.Add(new beatsV3
            {
                colorNotes = new List<colorNotes>(),
                bombNotes = new List<bombNotes>(),
                obstacles = new List<obstacles>(),
                bpmEvents = new List<bpmEvents>()
            });
        };

        int beatmapSet = 0;
        for (int i = 0; i < ReadMapInfo.instance.info._difficultyBeatmapSets.Count; i++)
        {
            if (ReadMapInfo.instance.info._difficultyBeatmapSets[i]._beatmapCharacteristicName == beatchar.ToString())
            {
                beatmapSet = i;
                break;
            }
        }

        int beatmapDiff = 0;
        for (int i = 0; i < ReadMapInfo.instance.info._difficultyBeatmapSets[beatmapSet]._difficultyBeatmaps.Count; i++)
        {
            if (ReadMapInfo.instance.info._difficultyBeatmapSets[beatmapSet]._difficultyBeatmaps[i]._difficulty == diff.ToString())
            {
                beatmapDiff = i;
                break;
            }
        }

        _difficultyBeatmaps difficulty = ReadMapInfo.instance.info._difficultyBeatmapSets[beatmapSet]._difficultyBeatmaps[beatmapDiff];
        string name = difficulty._beatmapFilename.ToString();
        ReadMapInfo.instance.difficulty = difficulty;

        ReadMapInfo.instance.JumpDuration(difficulty._noteJumpMovementSpeed, difficulty._noteJumpStartBeatOffset, ReadMapInfo.instance.info._beatsPerMinute);

        // READ V3 DATA
        beatsV3 beatV3 = JsonUtility.FromJson<beatsV3>(File.ReadAllText(Path.Combine(ReadMapInfo.instance.folderPath, name)));

        //string rawJson = JsonUtility.ToJson(beatV3, true);
        //File.WriteAllText(Path.Combine(ReadMapInfo.instance.folderPath, name), rawJson);

        if (beatV3 != null)
        {
            mappingTime = beatV3.customData.time;

            for (int i = 0; i < beatV3.colorNotes.Count; i++)
            {
                int b = Mathf.FloorToInt(beatV3.colorNotes[i].b);
                beats[b].colorNotes.Add(beatV3.colorNotes[i]);
            }

            for (int i = 0; i < beatV3.bombNotes.Count; i++)
            {
                int b = Mathf.FloorToInt(beatV3.bombNotes[i].b);
                beats[b].bombNotes.Add(beatV3.bombNotes[i]);
            }

            for (int i = 0; i < beatV3.obstacles.Count; i++)
            {
                int b = Mathf.FloorToInt(beatV3.obstacles[i].b);
                beats[b].obstacles.Add(beatV3.obstacles[i]);
            }

            if (beatV3.timings != null)
            {
                for (int i = 0; i < beatV3.timings.Count; i++)
                {
                    int b = Mathf.FloorToInt(beatV3.timings[i].b);
                    beats[b].timings.Add(beatV3.timings[i]);
                }
            }

            for (int i = 0; i < beatV3.bpmEvents.Count; i++)
            {
                int b = Mathf.FloorToInt(beatV3.bpmEvents[i].b);
                beats[b].bpmEvents.Add(beatV3.bpmEvents[i]);
                bpmEvents.Add(beatV3.bpmEvents[i]);
            }

            // Place bpm change at the start for sync
            bpmEvents bpm = new bpmEvents
            {
                b = 0,
                m = ReadMapInfo.instance.info._beatsPerMinute
            };

            // Check for an existing item with the same properties
            bool exists = bpmEvents.Any(e => e.b == bpm.b && e.m == bpm.m);

            if (!exists)
            {
                beats[0].bpmEvents.Insert(0, bpm);
                bpmEvents.Insert(0, bpm);
            }

        }

        // READ V2 DATA
        beatsV2 beatv2 = JsonUtility.FromJson<beatsV2>(File.ReadAllText(Path.Combine(ReadMapInfo.instance.folderPath, name)));

        if (beatv2 != null)
        {
            mappingTime += beatv2._customData._time;

            for (int i = 0; i < beatv2._notes.Count; i++)
            {
                if (beatv2._notes[i]._type != 3)
                {
                    colorNotes note = new colorNotes();
                    note.b = beatv2._notes[i]._time;
                    note.x = beatv2._notes[i]._lineIndex;
                    note.y = beatv2._notes[i]._lineLayer;
                    note.c = beatv2._notes[i]._type;
                    note.d = beatv2._notes[i]._cutDirection;
                    note.a = 0;
                    beats[Mathf.FloorToInt(note.b)].colorNotes.Add(note);
                }
                else
                {
                    bombNotes bomb = new bombNotes();
                    bomb.b = beatv2._notes[i]._time;
                    bomb.x = beatv2._notes[i]._lineIndex;
                    bomb.y = beatv2._notes[i]._lineLayer;
                    beats[Mathf.FloorToInt(bomb.b)].bombNotes.Add(bomb);
                }
            }

            for (int i = 0; i < beatv2._obstacles.Count; i++)
            {
                obstacles obstacle = new obstacles();
                if (beatv2._obstacles[i]._type == 1)
                {

                    obstacle.b = beatv2._obstacles[i]._time;
                    obstacle.x = beatv2._obstacles[i]._lineIndex;
                    obstacle.y = 2;
                    obstacle.d = beatv2._obstacles[i]._duration;
                    obstacle.w = beatv2._obstacles[i]._width;
                    obstacle.h = 3;
                }
                else if (beatv2._obstacles[i]._type == 0)
                {
                    obstacle.b = beatv2._obstacles[i]._time;
                    obstacle.x = beatv2._obstacles[i]._lineIndex;
                    obstacle.y = 0;
                    obstacle.d = beatv2._obstacles[i]._duration;
                    obstacle.w = beatv2._obstacles[i]._width;
                    obstacle.h = 5;
                }
                obstacles.Add(obstacle);
                beats[Mathf.FloorToInt(obstacle.b)].obstacles.Add(obstacle);
            }

            for (int i = 0; i < beatv2._events.Count; i++)
            {
                if (beatv2._events[i]._type == 100)
                {
                    bpmEvents bpmEvent = new bpmEvents();
                    bpmEvent.b = beatv2._events[i]._time;
                    // bpmEvent.x = beatv2._events[i]._type;        These don't exist in v3
                    // bpmEvent.y = beatv2._events[i]._value;       These don't exist in v3
                    bpmEvent.m = beatv2._events[i]._floatValue;

                    beats[Mathf.FloorToInt(bpmEvent.b)].bpmEvents.Add(bpmEvent);
                    bpmEvents.Add(bpmEvent);
                }
            }
        }
    }

    public int Rotation(int level)
    {
        return level switch
        {
            0 => 180,
            1 => 0,
            2 => 270,
            3 => 90,
            4 => 225,
            5 => 135,
            6 => 315,
            7 => 45,
            _ => 0
        };
    }
}

/// V3 Structure
///

[System.Serializable]
public class v3Info
{
    public string version;
    public List<bpmEvents> bpmEvents;
    public List<rotationEvents> rotationEvents;
    public List<colorNotes> colorNotes;
    public List<bombNotes> bombNotes;
    public List<obstacles> obstacles;
    public List<sliders> sliders;
    public List<burstSliders> burstSliders;
    public List<basicBeatmapEvents> basicBeatmapEvents;
    public List<timings> timings;
    public customData customData;
}

[System.Serializable]
public class beatsV3
{
    public string version;
    public List<bpmEvents> bpmEvents = new List<bpmEvents>();
    public List<rotationEvents> rotationEvents = new List<rotationEvents>();
    public List<colorNotes> colorNotes = new List<colorNotes>();
    public List<bombNotes> bombNotes = new List<bombNotes>();
    public List<obstacles> obstacles = new List<obstacles>();
    public List<sliders> sliders = new List<sliders>();
    public List<burstSliders> burstSliders = new List<burstSliders>();
    public List<basicBeatmapEvents> basicBeatmapEvents = new List<basicBeatmapEvents>();
    public List<timings> timings = new List<timings>();
    public customData customData;

    // Deep Clone Method
    public beatsV3 Clone()
    {
        // Clone the lists
        return new beatsV3
        {
            bpmEvents = this.bpmEvents,
            colorNotes = this.colorNotes,
            bombNotes = this.bombNotes,
            obstacles = this.obstacles,
            sliders = this.sliders,
            burstSliders = this.burstSliders,
            timings = this.timings
        };
    }
}

[System.Serializable]
public class rotationEvents
{
    public float b;
    public int e;
    public int r;
}

[System.Serializable]
public class sliders
{
    public float b;
    public int c;
    public int x;
    public int y;
    public int d;
    public float mu;
    public float tb;
    public int tx;
    public int ty;
    public int tc;
    public float tmu;
    public int m;
}


[System.Serializable]
public class burstSliders
{
    public float b;
    public int c;
    public int x;
    public int y;
    public int d;
    public float tb;
    public int tx;
    public int ty;
    public int sc;
    public int s;
}

[System.Serializable]
public class basicBeatmapEvents
{
    public float b;
    public int et;
    public int i;
    public float f;
}

[System.Serializable]
public class bpmEvents
{
    public float b;
    public float m;
}

[System.Serializable]
public class colorNotes
{
    public float b;
    public int x;
    public int y;
    public int a;
    public int c;
    public int d;
}

[System.Serializable]
public class timings
{
    public float b;
    public int t;
}


[System.Serializable]
public class bombNotes
{
    public float b;
    public int x;
    public int y;
}

[System.Serializable]
public class obstacles
{
    public float b;
    public int x;
    public int y;
    public float d;
    public int h;
    public int w;
}


[System.Serializable]
public class customData
{
    public float time;
}

[System.Serializable]
public class customEvents
{
    public float b;
    public string t;
    public customEventsData d;
}

[System.Serializable]
public class customEventsData
{
    // where all the noodle stuff would go, tracks, animations and stuff
}


/// V2 Structure
///

[System.Serializable]
public class beatsV2
{
    public List<_notes> _notes;
    public List<_obstacles> _obstacles;
    public List<_events> _events;
    public _customDatav2 _customData;
}

[System.Serializable]
public class _events
{
    public float _time;
    public float _type;
    public int _value;
    public float _floatValue;
}

[System.Serializable]
public class _notes
{
    public float _time;
    public int _lineIndex;
    public int _lineLayer;
    public int _type;
    public int _cutDirection;
}

[System.Serializable]
public class _obstacles
{
    public float _time;
    public int _lineIndex;
    public int _type;
    public float _duration;
    public int _width;
}

[System.Serializable]
public class _customDatav2
{
    public float _time;
}