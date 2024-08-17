using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class LoadMap : MonoBehaviour
{
    public static LoadMap instance;
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

    void Start()
    {
        instance = this;
    }

    public void IncreaseBeats(AudioClip clip)
    {
        int capacity = Mathf.CeilToInt(clip.length * ReadMapInfo.instance.info._beatsPerMinute / 60);

        for (int i = 0; i < capacity; i++)
        {
            beats.Add(new beatsV3
            {
                colorNotes = new List<colorNotes>(),
                bombNotes = new List<bombNotes>(),
                obstacles = new List<obstacles>(),
                bpmEvents = new List<bpmEvents>()
            });
        }
    }

    // Update is called once per frame
    public void Load(AudioSource audio)
    {
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

        if (AudioClipTimingGenerator.instance.generateMap != true)
        {
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
        }

        // Place bpm change at the start for sync
        bpmEvents bpm = new bpmEvents();
        bpm.b = 0;
        bpm.m = ReadMapInfo.instance.info._beatsPerMinute;
        beats[0].bpmEvents.Add(bpm);
        bpmEvents.Add(bpm);

        for (int i = 0; i < beatV3.bpmEvents.Count; i++)
        {
            int b = Mathf.FloorToInt(beatV3.bpmEvents[i].b);
            beats[b].bpmEvents.Add(beatV3.bpmEvents[i]);
            bpmEvents.Add(beatV3.bpmEvents[i]);
        }

        // READ V2 DATA
        beatsV2 beatv2 = JsonUtility.FromJson<beatsV2>(File.ReadAllText(Path.Combine(ReadMapInfo.instance.folderPath, name)));

        if (AudioClipTimingGenerator.instance.generateMap != true)
        {
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
    public customData customData;
}

[System.Serializable]
public class beatsV3
{
    public List<bpmEvents> bpmEvents;
    public List<rotationEvents> rotationEvents;
    public List<colorNotes> colorNotes;
    public List<bombNotes> bombNotes;
    public List<obstacles> obstacles;
    public List<sliders> sliders;
    public List<burstSliders> burstSliders;
    public List<basicBeatmapEvents> basicBeatmapEvents;

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

public class customData
{
    public float time;
}

/// V2 Structure
///

[System.Serializable]
public class beatsV2
{
    public List<_notes> _notes;
    public List<_obstacles> _obstacles;
    public List<_events> _events;
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


