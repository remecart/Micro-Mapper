using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using Unity.VisualScripting;
using System;

public class SpawnObjects : MonoBehaviour
{
    private (int,int)[] precisionArray = new []
    {
        (1, 1),
        (1, 2)
    };

    private int currentIndex = 0;
    
    
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
    public Slider timeSlider;
    public TMP_InputField numberator;
    public TMP_InputField denominator;
    public Transform BpmChangeSpawm;
    public Transform TimingSpawn;
    public bool invertPrecisionScroll;
    public bool invertTimelineScroll;

    public void ChangeTime(Slider slider)
    {
            LoadWallsBackwards();
            currentBeat = Mathf.RoundToInt(slider.value *
                                           Mathf.RoundToInt(
                                               BeatFromRealTime(LoadSong.instance.audioSource.clip.length)));
            LoadObjectsFromScratch(currentBeat, true, true);
            DrawLines.instance.DrawLinesWhenRequired();

        if (playing)
        {
            LoadSong.instance.StopSong();
            HitSoundManager.instance.cache = false;
            LoadSong.instance.Offset(SpawnObjects.instance.GetRealTimeFromBeat(currentBeat));
        }
    }

    public void ChangePrecision()
    {
        float a = int.Parse(numberator.text);
        float b = int.Parse(denominator.text);

        precision = a / b;

        LoadObjectsFromScratch(currentBeat, true, true);
        DrawLines.instance.DrawLinesWhenRequired();
    }

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        //ChangePrecision();
        spawnInMs = Settings.instance.config.mapping.noteDistance * 100;
        invertPrecisionScroll = Settings.instance.config.controls.invertPrecisionScroll;
        invertTimelineScroll = Settings.instance.config.controls.invertTimelineScroll;

        LoadObjectsFromScratch(1, true, true);
    }

    void UpdateTimeline()
    {
        Slider.SliderEvent sliderEvent = timeSlider.onValueChanged;
        timeSlider.onValueChanged = new Slider.SliderEvent();
        timeSlider.value = currentBeat / Mathf.CeilToInt(BeatFromRealTime(LoadSong.instance.audioSource.clip.length));
        timeSlider.onValueChanged = sliderEvent;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            currentIndex++;
            if (currentIndex >= precisionArray.Length)
            {
                currentIndex = 0;
            }
            numberator.text = precisionArray[currentIndex].Item1.ToString();
            denominator.text = precisionArray[currentIndex].Item2.ToString();
            
            ChangePrecision();
        }
        
        gameObject.transform.parent.parent.transform.position =
            new Vector3(0, 0, PositionFromBeat(currentBeat) * -editorScale);
        editorScale = Settings.instance.config.mapping.editorScale;
        currentBeatText.text = (Mathf.FloorToInt(currentBeat / precision) * precision).ToString();

        if (0 > currentBeat)
        {
            currentBeat = 0;
            ReloadMap.instance.ReloadEverything();
        }
        else if (currentBeat > BeatFromRealTime(length) && length != 0)
        {
            currentBeat = BeatFromRealTime(length);
            ReloadMap.instance.ReloadEverything();
        }

        if (playing)
        {
            UpdateTimeline();
        }
        
        if (currentBeatPlay < Mathf.FloorToInt(currentBeat)) Metronome.instance.MetronomeSound();
        currentBeatPlay = Mathf.FloorToInt(currentBeat);

        if (Input.GetKey(KeyCode.LeftControl))
        {
            int prec = int.Parse(denominator.text);
            int a = invertPrecisionScroll ? -1 : 1;

            if (Input.mouseScrollDelta.y * a > 0)
            {
                if (Mathf.RoundToInt(prec * 2) <= 99)
                {
                    prec = Mathf.RoundToInt(prec * 2);
                    denominator.text = prec.ToString();
                    precisionArray[currentIndex] = (int.Parse(numberator.text), prec);
                    ChangePrecision();
                }
            }

            if (Input.mouseScrollDelta.y * a < 0)
            {
                if (Mathf.RoundToInt(prec / 2) >= 1)
                {
                    prec = Mathf.RoundToInt(prec / 2);
                    denominator.text = prec.ToString();
                    precisionArray[currentIndex] = (int.Parse(numberator.text), prec);
                    ChangePrecision();
                }
            }
        }

        if (Input.mouseScrollDelta.y != 0 && !playing && !Input.GetKey(KeyCode.LeftShift) &&
            !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftControl))
        {
            float scroll = Input.mouseScrollDelta.y;
            int a = invertTimelineScroll ? -1 : 1;
            if (currentBeat + scroll * precision * a >= 0)
            {
                currentBeat += scroll * precision * a;
                LoadObjectsFromScratch(currentBeat, true, true);
                // Grid.localPosition = new Vector3(2, 0, PositionFromBeat(currentBeat) * editorScale);
                DrawLines.instance.DrawLinesWhenRequired();
                if (Input.mouseScrollDelta.y < 0) LoadWallsBackwards();
                UpdateTimeline();
                mBot.instance.UpdateSabers();
                
                // LoadWallsBackwards();
                // currentBeat = Mathf.RoundToInt(slider.value *
                //                                Mathf.RoundToInt(
                //                                    BeatFromRealTime(LoadSong.instance.audioSource.clip.length)));
                // LoadObjectsFromScratch(currentBeat, true, true);
                // DrawLines.instance.DrawLinesWhenRequired();
            }

            if (currentBeat > BeatFromRealTime(length) && length != 0)
            {
                currentBeat = BeatFromRealTime(length);
            }
        }

        if (length == 0 && LoadSong.instance.audioSource.clip != null)
        {
            length = LoadSong.instance.audioSource.clip.length;
            DrawLines.instance.DrawLinesWhenRequired();
        }

        spawnOffset = BeatFromRealTime(GetRealTimeFromBeat(currentBeat) + spawnInMs / 1000) -
                      BeatFromRealTime(GetRealTimeFromBeat(currentBeat));

        if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.stepForward))
        {
            currentBeat = currentBeat + precision;
            LoadObjectsFromScratch(currentBeat, true, true);
            DrawLines.instance.DrawLinesWhenRequired();
            Slider.SliderEvent sliderEvent = timeSlider.onValueChanged;
            timeSlider.onValueChanged = new Slider.SliderEvent();
            timeSlider.value =
                currentBeat / Mathf.CeilToInt(BeatFromRealTime(LoadSong.instance.audioSource.clip.length));
            timeSlider.onValueChanged = sliderEvent;
        }

        if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.stepBackward))
        {
            currentBeat = currentBeat - precision;
            LoadObjectsFromScratch(currentBeat, true, true);
            DrawLines.instance.DrawLinesWhenRequired();
            Slider.SliderEvent sliderEvent = timeSlider.onValueChanged;
            timeSlider.onValueChanged = new Slider.SliderEvent();
            timeSlider.value =
                currentBeat / Mathf.CeilToInt(BeatFromRealTime(LoadSong.instance.audioSource.clip.length));
            timeSlider.onValueChanged = sliderEvent;
            LoadWallsBackwards();
        }

        if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.playMap) && !Input.GetMouseButton((int)MouseButton.Right) && !Bookmarks.instance.openMenu)
        {
            if (!playing)
            {
                if (GetRealTimeFromBeat(currentBeat) <= LoadSong.instance.audioSource.clip.length)
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
            double deltaTime = Time.deltaTime;
            double remainingTime = deltaTime;
            double updatedBeat = (double)currentBeat;

            while (remainingTime > 0)
            {
                double currentBpm = GetBpmAtBeat((float)updatedBeat); // Get BPM at the current updatedBeat
                double nextBpmEventBeat = GetNextBpmEventBeat((float)updatedBeat); // Get the next BPM event beat

                // Calculate the time to the next BPM event
                double timeToNextBpmEvent = (nextBpmEventBeat - updatedBeat) * (60f / currentBpm);

                // Determine if we should update up to the next BPM event or consume all the remaining time
                double timeStep = Mathf.Min((float)remainingTime, (float)timeToNextBpmEvent);
                double beatIncrement = (timeStep / 60f) * currentBpm;

                // Update the beat and remaining time
                updatedBeat += beatIncrement * Settings.instance.config.mapping.songSpeed / 10;
                remainingTime -= timeStep;
            }

            // Update currentBeat after processing all time steps
            currentBeat = (float)updatedBeat;

            // Proceed with other logic
            float nextRealTime = GetRealTimeFromBeat(currentBeat);
            float nextBeat = BeatFromRealTime(nextRealTime);

            float offset = spawnOffset - offsetCache;
            offsetCache = spawnOffset;

            LoadObjectsWithinDuration(lastBeat, BeatFromRealTime(GetRealTimeFromBeat(currentBeat) + spawnInMs / 1000));
            lastBeat = BeatFromRealTime(GetRealTimeFromBeat(currentBeat) + spawnInMs / 1000);

            if (currentBeat > beatCache + precision)
            {
                beatCache = currentBeat;
            }
        }

        if (GetRealTimeFromBeat(currentBeat) >= length) playing = false;
    }
    public void LoadWallsBackwards()
    {
        float check = 256;

        foreach (Transform child in content.transform)
        {
            if (child.GetComponent<ObstacleData>())
                Destroy(child.gameObject);
        }

        for (int i = 0; i < check; i++)
        {
            int targetBeat = Mathf.FloorToInt(currentBeat - i + Mathf.CeilToInt(spawnOffset));

            // Ensure obstacles at beats 0-1 are handled
            if (targetBeat >= 0)
            {
                List<obstacles> obstacles = LoadMap.instance.beats[targetBeat].obstacles;

                foreach (var obstacleData in obstacles)
                {
                    float obstacleStart = obstacleData.b;
                    float obstacleEnd = obstacleData.b + obstacleData.d;

                    // Correct start and end if duration is negative
                    if (obstacleData.d < 0)
                    {
                        (obstacleStart, obstacleEnd) = (obstacleEnd, obstacleStart);
                    }

                    // Allow obstacles at beat 0-1 and ensure they spawn correctly even if they overlap with beat 0
                    if ((obstacleEnd >= 0 && obstacleStart <= currentBeat + 12) &&
                        (obstacleEnd > currentBeat - 12 || targetBeat <= 1))
                    {
                        var scaleZ = (PositionFromBeat(obstacleEnd) - PositionFromBeat(obstacleStart)) * editorScale;

                        GameObject obstacle = Instantiate(objects[4]);
                        obstacle.transform.SetParent(content);

                        obstacle.transform.localPosition = new Vector3(
                            obstacleData.x + (float)obstacleData.w / 2 - 0.5f,
                            obstacleData.y + obstacleData.h / 2 - 0.5f,
                            PositionFromBeat(obstacleStart) * editorScale + 0.5f * scaleZ
                        );

                        obstacle.transform.localScale = new Vector3(obstacleData.w, obstacleData.h, scaleZ);
                        obstacle.name = obstacle.name + obstacleStart;
                        obstacle.GetComponent<ObstacleData>().obstacle = obstacleData;
                    }
                }
            }
        }

        SelectObjects.instance.HighlightSelectedObject();
    }


    public void LoadObjectsFromScratch(float fbeat, bool resetGridPos, bool fromScratch)
    {
        foreach (Transform child in content.transform)
        {
            var obstacleData = child.GetComponent<ObstacleData>();

            if (!obstacleData)
            {
                Destroy(child.gameObject);
            }
            else
            {
                float obstacleStart = obstacleData.obstacle.b;
                float obstacleEnd = obstacleStart + obstacleData.obstacle.d;

                // Ensure proper ordering even if d is negative
                if (obstacleEnd < obstacleStart)
                {
                    float temp = obstacleStart;
                    obstacleStart = obstacleEnd;
                    obstacleEnd = temp;
                }

                float obstacleCenter = (obstacleStart + obstacleEnd) / 2f;
                float obstaclePositionZ =
                    PositionFromBeat(currentBeat - spawnOffset + 5 - obstacleCenter) * editorScale;

                if (child.transform.localPosition.z < obstaclePositionZ)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        if (resetGridPos) Grid.localPosition = new Vector3(2, 0, PositionFromBeat(fbeat) * editorScale);

        int beat = Mathf.FloorToInt(fbeat);

        for (int i = 0; i < Mathf.CeilToInt(spawnOffset) + spawnOffset + 1; i++)
        {
            int targetBeat = beat - Mathf.FloorToInt(spawnOffset) + i + 1;
            if (targetBeat >= 0) // Ensure we check for the first beat as well
            {
                if (fromScratch) SpawnObjectsAtBeat(targetBeat, fbeat + spawnOffset, fbeat - spawnOffset, false);
                else SpawnObjectsAtBeat(targetBeat, fbeat + spawnOffset, fbeat - spawnOffset, true);
            }
            else if (beat == 0) // Explicitly handle beat zero case
            {
                SpawnObjectsAtBeat(0, fbeat + spawnOffset, fbeat - spawnOffset, false);
            }
        }
        
        mBot.instance.UpdateSabers();

        LoadWallsBackwards();
    }


    public void LoadObjectsWithinDuration(float fbeat, float cbeat)
    {
        foreach (Transform child in content.transform)
        {
            var obstacleData = child.GetComponent<ObstacleData>();

            if (child.transform.localPosition.z < PositionFromBeat(currentBeat - spawnOffset) * editorScale)
            {
                if (!obstacleData)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    float obstacleStart = obstacleData.obstacle.b;
                    float obstacleEnd = obstacleStart + obstacleData.obstacle.d;

                    // Ensure proper ordering even if d is negative
                    if (obstacleEnd < obstacleStart)
                    {
                        float temp = obstacleStart;
                        obstacleStart = obstacleEnd;
                        obstacleEnd = temp;
                    }

                    if (child.transform.localPosition.z <
                        PositionFromBeat(currentBeat - 12 - obstacleEnd) * editorScale)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }


        Grid.localPosition = new Vector3(2, 0, PositionFromBeat(fbeat - spawnOffset) * editorScale);

        int beat = Mathf.FloorToInt(cbeat);

        SpawnObjectsAtBeat(beat, cbeat, fbeat, false);
    }

    public float ConvertMEPos(int index)
    {
        float x = (float)index;
        if (x > 999 || x < -999) x = x / 1000f - Mathf.Clamp(x, -1, 1);
        return x;
    }

    public int ConvertToMEPos(float x)
    {
        int y;
        if (x >= 0) y = 1;
        else y = -1;

        x = (x + y) * 1000f;
        return (int)x;
    }
    void SpawnObjectsAtBeat(int beat, float latestBeat, float earliestBeat, bool loadNewOnly)
    {
        if (LoadSong.instance.audioSource.clip)
        {
            if (beat <= BeatFromRealTime(LoadSong.instance.audioSource.clip.length) && beat >= 0)
            {
                // Process Notes
                List<colorNotes> notes = LoadMap.instance.beats[beat].colorNotes;
                foreach (var noteData in notes)
                {
                    bool newObject = true;

                    if (loadNewOnly)
                    {
                        foreach (Transform child in content)
                        {
                            Vector3 cache = new Vector3(ConvertMEPos(noteData.x), ConvertMEPos(noteData.y),
                                PositionFromBeat(noteData.b) * editorScale);
                            if (child.transform.localPosition == cache)
                            {
                                newObject = false;
                                break;
                            }
                        }
                    }

                    if (noteData.b <= latestBeat && noteData.b >= earliestBeat && newObject)
                    {
                        GameObject note = Instantiate(objects[noteData.c]);
                        note.name = "NOTE_" + noteData.c + ": b=" + noteData.b;
                        note.transform.SetParent(content);
                        note.transform.localPosition = new Vector3(ConvertMEPos(noteData.x), ConvertMEPos(noteData.y),
                            PositionFromBeat(noteData.b) * editorScale);
                        note.GetComponent<NoteData>().note = noteData;

                        if (noteData.d != 8)
                        {
                            note.transform.rotation = Quaternion.Euler(0, 0, Rotation(noteData.d));
                            note.transform.GetChild(1).gameObject.SetActive(false);
                        }
                        else
                        {
                            note.transform.GetChild(0).gameObject.SetActive(false);
                        }
                    }
                }

                // Process Bombs
                List<bombNotes> bombs = LoadMap.instance.beats[beat].bombNotes;
                foreach (var bombData in bombs)
                {
                    bool newObject = true;

                    if (loadNewOnly)
                    {
                        foreach (Transform child in content)
                        {
                            Vector3 cache = new Vector3(bombData.x, bombData.y,
                                PositionFromBeat(bombData.b) * editorScale);
                            if (child.transform.localPosition == cache)
                            {
                                newObject = false;
                                break;
                            }
                        }
                    }

                    if (bombData.b <= latestBeat && bombData.b >= earliestBeat && newObject)
                    {
                        GameObject bomb = Instantiate(objects[2]);
                        bomb.transform.SetParent(content);
                        bomb.transform.localPosition = new Vector3(bombData.x, bombData.y,
                            PositionFromBeat(bombData.b) * editorScale);
                        bomb.GetComponent<BombData>().bomb = bombData;
                    }
                }

                // Process Obstacles
                List<obstacles> obstacles = LoadMap.instance.beats[beat].obstacles;
                foreach (var obstacleData in obstacles)
                {
                    bool newObject = true;

                    if (loadNewOnly)
                    {
                        foreach (Transform child in content)
                        {
                            var scaleZ =
                                (PositionFromBeat(obstacleData.b + obstacleData.d) - PositionFromBeat(obstacleData.b)) *
                                editorScale;
                            Vector3 cache = new Vector3(obstacleData.x + (float)obstacleData.w / 2 - 0.5f,
                                obstacleData.y + obstacleData.h / 2 - 0.5f,
                                PositionFromBeat(obstacleData.b) * editorScale + 0.5f * scaleZ);
                            if (child.transform.localPosition == cache)
                            {
                                newObject = false;
                                break;
                            }
                        }
                    }

                    if (obstacleData.b <= latestBeat && obstacleData.b >= earliestBeat && newObject)
                    {
                        var scaleZ =
                            (PositionFromBeat(obstacleData.b + obstacleData.d) - PositionFromBeat(obstacleData.b)) *
                            editorScale;
                        GameObject obstacle = Instantiate(objects[4]);
                        obstacle.transform.SetParent(content);
                        obstacle.transform.localPosition = new Vector3(
                            obstacleData.x + (float)obstacleData.w / 2 - 0.5f,
                            obstacleData.y + obstacleData.h / 2 - 0.5f,
                            PositionFromBeat(obstacleData.b) * editorScale + 0.5f * scaleZ);
                        obstacle.transform.localScale = new Vector3(obstacleData.w, obstacleData.h, scaleZ);
                        obstacle.name = "Obstacle_" + beat;
                        obstacle.GetComponent<ObstacleData>().obstacle = obstacleData;
                    }
                }

                // Process BPM Events
                List<bpmEvents> bpmChanges = LoadMap.instance.beats[beat].bpmEvents;
                foreach (var bpmChangeData in bpmChanges)
                {
                    bool newObject = true;

                    
                    if (bpmChangeData.b <= latestBeat && bpmChangeData.b >= earliestBeat)
                    {
                        GameObject bpmChange = Instantiate(objects[3]);
                        bpmChange.transform.SetParent(content);
                        bpmChange.transform.localPosition = new Vector3(BpmChangeSpawm.position.x - 0.5f, 0,
                            PositionFromBeat(bpmChangeData.b) * editorScale);
                        bpmChange.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>().text =
                            bpmChangeData.m.ToString();
                        bpmChange.GetComponent<BpmEventData>().bpmEvent = bpmChangeData;
                    }
                }
                
                // Load Timing Objects
                List<timings> timings = LoadMap.instance.beats[beat].timings;
                foreach (var timingData in timings)
                {
                    bool newObject = true;
                    if (loadNewOnly)
                    {
                        foreach (Transform child in content)
                        {
                            Vector3 cache = new Vector3(TimingSpawn.localPosition.x + timingData.t + 1, 0,
                                PositionFromBeat(timingData.b) * editorScale);
                            if (child.transform.localPosition == cache)
                                newObject = false;
                        }
                    }
                    if (timingData.b <= latestBeat && timingData.b >= earliestBeat && newObject)
                    {
                        GameObject timing = Instantiate(objects[5]);
                        timing.transform.SetParent(content);
                        timing.transform.localPosition = new Vector3(TimingSpawn.localPosition.x + timingData.t + 1, 0,
                            PositionFromBeat(timingData.b) * editorScale);
                        timing.GetComponent<TimingData>().timings = timingData;
                    }
                } 
            }

            SelectObjects.instance.HighlightSelectedObject();
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
            8 => 69,
            _ => 0
        };
    }

    public float GetNextBpmEventBeat(float currentBeat)
    {
        List<bpmEvents> bpmEvents = LoadMap.instance.bpmEvents;

        // Iterate through the BPM events to find the next one after the current beat
        foreach (var bpmEvent in bpmEvents)
        {
            if (bpmEvent.b > currentBeat)
            {
                return bpmEvent.b;
            }
        }

        // If there are no future BPM events, return a large value to indicate "no more events"
        return float.MaxValue;
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
        float value = ReadMapInfo.instance.info._beatsPerMinute;
        ;

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
        float previousBpm = ReadMapInfo.instance.info._beatsPerMinute;
        float previousBeat = 0;

        foreach (var bpmEvent in bpms)
        {
            // If the beat is before this BPM event, calculate the time and break.
            if (beat <= bpmEvent.b)
            {
                realTime += (beat - previousBeat) * (60f / previousBpm);
                return realTime;
            }

            // If the beat is past this BPM event, accumulate time.
            realTime += (bpmEvent.b - previousBeat) * (60f / previousBpm);
            // Update previousBpm and previousBeat for the next iteration
            previousBpm = bpmEvent.m;
            previousBeat = bpmEvent.b;
        }

        // In case the beat is after the last BPM event
        realTime += (beat - previousBeat) * (60f / previousBpm);
        return realTime;
    }


    public float BeatFromRealTime(float realTime)
    {
        List<bpmEvents> bpms = LoadMap.instance.bpmEvents;
        double accumulatedTime = 0.0;

        for (int i = 0; i < bpms.Count; i++)
        {
            if (i + 1 < bpms.Count)
            {
                // Calculate the duration until the next BPM change (use double for accuracy)
                double nextBeatTime = bpms[i + 1].b;
                double bpmDuration = (nextBeatTime - bpms[i].b) * (60.0 / bpms[i].m);

                if (accumulatedTime + bpmDuration >= realTime)
                {
                    double remainingTime = realTime - accumulatedTime;
                    return (float)(bpms[i].b + (remainingTime / (60.0 / bpms[i].m)));
                }

                accumulatedTime += bpmDuration;
            }
            else
            {
                // Calculate for the last BPM event
                double remainingTime = realTime - accumulatedTime;
                return (float)(bpms[i].b + (remainingTime / (60.0 / bpms[i].m)));
            }
        }

        return bpms.Count > 0 ? (float)bpms[bpms.Count - 1].b : 0f;
    }
}

// UndoRedoManager class for managing a List<MyClass>