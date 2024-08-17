using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using Unity.VisualScripting;

public class SpawnObjects : MonoBehaviour
{
    public static SpawnObjects instance;
    public float currentBeat;
    public float precision;
    public float spawnInMs;
    public float spawnOffset;
    public Transform content;
    public float editorScale;
    public List<GameObject> objects;
    public Transform Grid;
    public bool playing;
    private float lastBeat;
    private float length;
    private float beatCache;
    private int currentBeatPlay;
    private float offsetCache;
    public TextMeshPro currentBeatText;
    public TextMeshProUGUI editorScaleText;
    public Slider EditorScaleSlider;
    public Slider timeSlider;
    private bool precisionMode;

    public void editorScaleSlider(Slider slider)
    {
        editorScale = slider.value;
        LoadObjectsFromScratch(currentBeat, true, true);
        DrawLines.instance.DrawLinesFromScratch(currentBeat, precision);
        InstantiateSpectrogram.instance.Reload();
    }

    public void ChangeTime(Slider slider)
    {
        if (!playing)
        {
            currentBeat = Mathf.RoundToInt(slider.value * LoadMap.instance.beats.Count * precision) / precision;
            LoadObjectsFromScratch(currentBeat, true, true);
            DrawLines.instance.DrawLinesFromScratch(currentBeat, precision);
        }
    }

    void ChangePrecisionMode(bool mode)
    {
        if (mode == true)
        {
            precisionMode = true;
        }
        else
        {
            precisionMode = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        currentBeatText.text = (Mathf.FloorToInt(currentBeat / precision) * precision).ToString();

        Slider.SliderEvent sliderEvent = timeSlider.onValueChanged;
        timeSlider.onValueChanged = new Slider.SliderEvent();
        timeSlider.value = currentBeat / LoadMap.instance.beats.Count;
        timeSlider.onValueChanged = sliderEvent;


        if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.changePrecisionMode))
        {
            ChangePrecisionMode(!precisionMode);
            LoadObjectsFromScratch(currentBeat, true, true);
        }

        if (0 > currentBeat)
        {
            currentBeat = 0;
            ReloadMap.instance.ReloadEverything();
        }
        if (playing)
        {
            if (currentBeat > currentBeatPlay + 1)
            {
                DrawLines.instance.DrawLinesFromScratch(currentBeat, precision);
                currentBeatPlay = Mathf.FloorToInt(currentBeat);
                Metronome.instance.MetronomeSound();
            }
        }
        else currentBeatPlay = Mathf.FloorToInt(currentBeat);

        if (Input.mouseScrollDelta.y != 0 && !playing)
        {
            float scroll = Input.mouseScrollDelta.y;
            if (currentBeat + scroll * precision >= 0) currentBeat += scroll * precision;
            LoadObjectsWithinDuration(lastBeat, BeatFromRealTime(GetRealTimeFromBeat(currentBeat) + spawnInMs / 1000));
            Grid.localPosition = new Vector3(2, 0, PositionFromBeat(currentBeat) * editorScale);
            DrawLines.instance.DrawLinesFromScratch(currentBeat, precision);
        }

        if (length == 0 && LoadSong.instance.audioSource.clip != null)
        {
            length = LoadSong.instance.audioSource.clip.length;
            DrawLines.instance.DrawLinesFromScratch(currentBeat, precision);
        }

        spawnOffset = BeatFromRealTime(GetRealTimeFromBeat(currentBeat) + spawnInMs / 1000) - BeatFromRealTime(GetRealTimeFromBeat(currentBeat));

        if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.stepForward))
        {
            currentBeat += precision;
            LoadObjectsFromScratch(currentBeat, true, true);
            timeSlider.value = currentBeat / LoadMap.instance.beats.Count;
        }

        if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.stepBackward))
        {
            currentBeat -= precision;
            LoadObjectsFromScratch(currentBeat, true, true);
            timeSlider.value = currentBeat / LoadMap.instance.beats.Count;
        }

        if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.playMap))
        {
            if (!playing)
            {
                if (GetRealTimeFromBeat(currentBeat) <= length)
                {
                    LoadSong.instance.Offset(GetRealTimeFromBeat(currentBeat));
                    playing = true;
                }
            }
            else if (playing)
            {
                LoadSong.instance.StopSong();
                playing = false;
                currentBeat = Mathf.RoundToInt(currentBeat / precision) * precision;
                LoadObjectsFromScratch(currentBeat, true, true);
            }
        }

        if (playing)
        {
            float deltaTime = Time.unscaledDeltaTime;
            float nextRealTime = GetRealTimeFromBeat(currentBeat) + deltaTime;
            float nextBeat = BeatFromRealTime(nextRealTime);

            float offset = spawnOffset - offsetCache;
            offsetCache = spawnOffset;

            currentBeat = nextBeat;
            LoadObjectsWithinDuration(lastBeat, BeatFromRealTime(GetRealTimeFromBeat(currentBeat) + spawnInMs / 1000));
            lastBeat = BeatFromRealTime(GetRealTimeFromBeat(currentBeat) + spawnInMs / 1000);

            if (currentBeat > beatCache + precision)
            {
                beatCache = currentBeat;
            }
        }

        if (GetRealTimeFromBeat(currentBeat) >= length) playing = false;
    }


    public void LoadObjectsFromScratch(float fbeat, bool resetGridPos, bool fromScratch)
    {
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }

        if (resetGridPos) Grid.localPosition = new Vector3(2, 0, PositionFromBeat(fbeat) * editorScale);

        int beat = Mathf.FloorToInt(fbeat);

        for (int i = 0; i < Mathf.CeilToInt(spawnOffset) + spawnOffset + 1; i++)
        {
            if (beat - spawnOffset + i >= 0)
            {
                if (fromScratch) SpawnObjectsAtBeat(beat - Mathf.CeilToInt(spawnOffset) + i + 1, fbeat + spawnOffset, fbeat - spawnOffset, false);
                else SpawnObjectsAtBeat(beat - Mathf.CeilToInt(spawnOffset) + i + 1, fbeat + spawnOffset, fbeat - spawnOffset, true);
            }
        }
    }

    public void LoadObjectsWithinDuration(float fbeat, float cbeat)
    {
        foreach (Transform child in content.transform)
        {
            if (child.transform.position.z < PositionFromBeat(currentBeat - spawnOffset) * editorScale)
            {
                if (!child.GetComponent<WallData>()) Destroy(child.gameObject);
            }
        }

        Grid.localPosition = new Vector3(2, 0, PositionFromBeat(fbeat - spawnOffset) * editorScale);

        int beat = Mathf.FloorToInt(cbeat);

        SpawnObjectsAtBeat(beat, cbeat, fbeat, false);
    }

    void SpawnObjectsAtBeat(int beat, float latestBeat, float earliestBeat, bool loadNewOnly)
    {
        if (beat <= BeatFromRealTime(length) && beat >= 0)
        {
            List<colorNotes> notes = LoadMap.instance.beats[beat].colorNotes;
            foreach (var noteData in notes)
            {
                bool newObject = true;

                if (loadNewOnly)
                {
                    foreach (Transform child in content)
                    {
                        Vector3 cache = new Vector3(noteData.x, noteData.y, PositionFromBeat(noteData.b) * editorScale);
                        if (child.transform.localPosition == cache)
                            newObject = false;
                    }
                }

                if (noteData.b <= latestBeat && noteData.b >= earliestBeat && newObject)
                {
                    GameObject note = Instantiate(objects[noteData.c]);
                    note.name = "NOTE_" + noteData.c + ": b=" + noteData.b;
                    note.transform.SetParent(content);
                    note.transform.localPosition = new Vector3(noteData.x, noteData.y, PositionFromBeat(noteData.b) * editorScale);
                    note.GetComponent<NoteData>().note = noteData;
                    if (noteData.d != 8)
                    {
                        note.transform.rotation = Quaternion.Euler(0, 0, Rotation(noteData.d));
                        note.transform.GetChild(1).gameObject.SetActive(false);
                    }
                    else note.transform.GetChild(0).gameObject.SetActive(false);
                }

            }

            // Load Bomb Objects
            List<bombNotes> bombs = LoadMap.instance.beats[beat].bombNotes;
            foreach (var bombData in bombs)
            {
                bool newObject = true;

                if (loadNewOnly)
                {
                    foreach (Transform child in content)
                    {
                        Vector3 cache = new Vector3(bombData.x, bombData.y, PositionFromBeat(bombData.b) * editorScale);
                        if (child.transform.localPosition == cache)
                            newObject = false;
                    }
                }

                if (bombData.b <= latestBeat && bombData.b >= earliestBeat && newObject)
                {
                    GameObject bomb = Instantiate(objects[2]);
                    bomb.transform.SetParent(content);
                    bomb.transform.localPosition = new Vector3(bombData.x, bombData.y, PositionFromBeat(bombData.b) * editorScale);
                }
            }

            List<obstacles> obstacles = LoadMap.instance.beats[beat].obstacles;
            foreach (var obstacleData in obstacles)
            {
                bool newObject = true;

                if (loadNewOnly)
                {
                    foreach (Transform child in content)
                    {
                        var scaleZ = (PositionFromBeat(obstacleData.b + obstacleData.d) - PositionFromBeat(obstacleData.b)) * editorScale;
                        Vector3 cache = new Vector3(obstacleData.x + (float)obstacleData.w / 2 - 0.5f, obstacleData.y + obstacleData.h / 2 - 0.5f, PositionFromBeat(obstacleData.b) * editorScale + 0.5f * scaleZ);
                        if (child.transform.localPosition == cache)
                            newObject = false;
                    }
                }

                if (obstacleData.b <= latestBeat && obstacleData.b >= earliestBeat && newObject)
                {
                    var scaleZ = (PositionFromBeat(obstacleData.b + obstacleData.d) - PositionFromBeat(obstacleData.b)) * editorScale;
                    GameObject obstacle = Instantiate(objects[4]);
                    obstacle.transform.SetParent(content);
                    obstacle.transform.localPosition = new Vector3(obstacleData.x + (float)obstacleData.w / 2 - 0.5f, obstacleData.y + obstacleData.h / 2 - 0.5f, PositionFromBeat(obstacleData.b) * editorScale + 0.5f * scaleZ);
                    obstacle.transform.localScale = new Vector3(obstacleData.w, obstacleData.h, scaleZ);
                    obstacle.name = obstacle.name + beat;
                }
            }

            // Load Bpm Objects
            List<bpmEvents> bpmChanges = LoadMap.instance.beats[beat].bpmEvents;
            foreach (var bpmChangeData in bpmChanges)
            {
                if (bpmChangeData.b <= latestBeat && bpmChangeData.b >= earliestBeat)
                {
                    GameObject bpmChange = Instantiate(objects[3]);
                    bpmChange.transform.SetParent(content);
                    bpmChange.transform.localPosition = new Vector3(8, 0, PositionFromBeat(bpmChangeData.b) * editorScale);
                    bpmChange.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>().text = bpmChangeData.m.ToString();
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

    public float PositionFromBeat(float beat)
    {
        List<bpmEvents> bpms = LoadMap.instance.bpmEvents;
        float position = 0;
        float previousBpm = ReadMapInfo.instance.info._beatsPerMinute;
        float previousBeat = 0f;

        foreach (var bpmEvent in bpms)
        {
            if (beat <= bpmEvent.b)
            {
                break;
            }

            float duration = bpmEvent.b - previousBeat;
            position += duration * (60f / previousBpm);

            previousBpm = bpmEvent.m;
            previousBeat = bpmEvent.b;
        }

        float remainingDuration = beat - previousBeat;
        position += remainingDuration * (60f / previousBpm);

        return position;
    }

    public float BeatFromPosition(float position)
    {
        List<bpmEvents> bpms = LoadMap.instance.bpmEvents;
        float _beat = 0;
        float accumulatedPosition = 0;
        for (int i = 0; i < bpms.Count; i++)
        {
            if (i + 1 < bpms.Count)
            {
                float nextBeatTime = bpms[i + 1].b;
                float duration = nextBeatTime - bpms[i].b;
                float sectionPosition = duration * (60f / bpms[i].m);

                if (accumulatedPosition + sectionPosition >= position)
                {
                    float remainingPosition = position - accumulatedPosition;
                    _beat = bpms[i].b + (remainingPosition / (60f / bpms[i].m));
                    return _beat;
                }
                else
                {
                    accumulatedPosition += sectionPosition;
                }
            }
            else
            {
                float remainingPosition = position - accumulatedPosition;
                _beat = bpms[i].b + (remainingPosition / (60f / bpms[i].m));
                return _beat;
            }
        }
        return _beat;
    }

    public float GetBpmAtBeat(float beat)
    {
        List<bpmEvents> bpms = LoadMap.instance.bpmEvents;
        float value = ReadMapInfo.instance.info._beatsPerMinute; ;

        for (int i = 0; i < bpms.Count; i++)
        {
            if (bpms[i].b > beat)
            {
                break;
            }
            value = bpms[i].m;
        }
        return value;
    }

    public float GetRealTimeFromBeat(float beat)
    {
        List<bpmEvents> bpms = LoadMap.instance.bpmEvents;
        float realTime = 0;
        float previousBpm = ReadMapInfo.instance.info._beatsPerMinute; ;
        float previousBeat = 0;

        foreach (var bpmEvent in bpms)
        {
            if (beat <= bpmEvent.b)
            {
                realTime += (beat - previousBeat) * (60f / previousBpm);
                return realTime;
            }
            realTime += (bpmEvent.b - previousBeat) * (60f / previousBpm);
            previousBpm = bpmEvent.m;
            previousBeat = bpmEvent.b;
        }
        realTime += (beat - previousBeat) * (60f / previousBpm);
        return realTime;
    }


    public float BeatFromRealTime(float realTime)
    {
        List<bpmEvents> bpms = LoadMap.instance.bpmEvents;
        float beat = 0;
        float accumulatedTime = 0;

        for (int i = 0; i < bpms.Count; i++)
        {
            if (i + 1 < bpms.Count)
            {
                float nextBeatTime = bpms[i + 1].b;
                float bpmDuration = (nextBeatTime - bpms[i].b) * (60f / bpms[i].m);

                if (accumulatedTime + bpmDuration >= realTime)
                {
                    float remainingTime = realTime - accumulatedTime;
                    beat = bpms[i].b + (remainingTime / (60f / bpms[i].m));
                    return beat;
                }
                else
                {
                    accumulatedTime += bpmDuration;
                }
            }
            else
            {
                float remainingTime = realTime - accumulatedTime;
                beat = bpms[i].b + (remainingTime / (60f / bpms[i].m));
                return beat;
            }
        }

        return beat;
    }
}
