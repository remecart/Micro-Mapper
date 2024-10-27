using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cybernoodle_v2 : MonoBehaviour
{
    public float currentBeat;
    public float length;
    public List<colorNotes> nextLeftNotes;
    public List<colorNotes> nextRightNotes;
    public List<colorNotes> lastLeftNotes;
    public List<colorNotes> lastRightNotes;
    public GameObject leftSaber;
    public GameObject rightSaber;
    public int rotationSpeed;

    void Start()
    {

    }

    void Update()
    {
        currentBeat = SpawnObjects.instance.currentBeat;

        if (length == 0)
        {
            if (LoadSong.instance.audioSource.clip) length = LoadSong.instance.audioSource.clip.length;
        }
        else if (Input.GetKey(KeyCode.I) || Input.GetKeyDown(KeyCode.O))
        {
            GetNextNote();
            GetLastNote();

            UpdateSaberPositionAndRotation(leftSaber, lastLeftNotes[0], nextLeftNotes[0]);
            UpdateSaberPositionAndRotation(rightSaber, lastRightNotes[0], nextRightNotes[0]);
        }
    }

    void UpdateSaberPositionAndRotation(GameObject saber, colorNotes lastNote, colorNotes nextNote)
    {
        if (nextNote == null) return;
        if (lastNote == null)
        {
            lastNote = new colorNotes();
        }

        float durationLeft = nextNote.b - lastNote.b;
        float point = (currentBeat - lastNote.b) / durationLeft;

        int cutDirection = nextNote.d;
        float radian = (SpawnObjects.instance.Rotation(cutDirection) - 90) * Mathf.Deg2Rad;
        Vector2 cutVector = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));

        Vector3 lastPos = new Vector3(lastNote.x + 0.5f, lastNote.y + 0.5f, SpawnObjects.instance.PositionFromBeat(lastNote.b) * SpawnObjects.instance.editorScale - 0.9f);
        Vector3 nextPos = new Vector3(nextNote.x + 0.5f + cutVector.x * 0.5f, nextNote.y + 0.5f + cutVector.y * 0.5f, SpawnObjects.instance.PositionFromBeat(nextNote.b) * SpawnObjects.instance.editorScale - 0.9f);

        // Calculate a smooth step factor based on the point
        float smoothStepFactor = Mathf.SmoothStep(0, 1, point);
        Vector3 direction = Vector3.Lerp(lastPos, nextPos, Time.deltaTime * rotationSpeed * smoothStepFactor);

        // // Update target rotation to smoothly transition to the next note's direction
        // if (direction != Vector3.zero)
        // {
        //     targetRotation = Quaternion.LookRotation(direction);
        // }

        // Slerp to the target rotation using smooth step factor
        saber.transform.rotation = Quaternion.LookRotation(direction);

        float temp = 1.25f;
        if (saber.name.Contains("Left")) temp = -1.25f;

        // Calculate the saber position using smooth interpolation
        saber.transform.localPosition = new Vector3(
            Mathf.Lerp(lastNote.x, nextNote.x, smoothStepFactor) * 0.15f + temp,
            Mathf.Lerp(lastNote.y, nextNote.y, smoothStepFactor) * 0.15f + 0.85f,
            -2
        );
    }

    void GetNextNote()
    {
        nextLeftNotes = new List<colorNotes>();
        nextRightNotes = new List<colorNotes>();
        for (int i = 0; i < LoadMap.instance.beats.Count - Mathf.FloorToInt(currentBeat); i++)
        {
            if (currentBeat + i <= SpawnObjects.instance.BeatFromRealTime(length) && currentBeat + i >= 0)
            {
                List<colorNotes> notes = LoadMap.instance.beats[Mathf.FloorToInt(currentBeat + i)].colorNotes;
                foreach (var noteData in notes)
                {
                    if (noteData.b > currentBeat)
                    {
                        if (noteData.c == 0 && nextLeftNotes.Count == 0) GetNotesOnSameBeat(noteData.b, 0, lastLeftNotes, true);
                        else if (noteData.c == 1 && nextRightNotes.Count == 0) GetNotesOnSameBeat(noteData.b, 1, lastRightNotes, true);
                    }
                }
            }
        }
    }

    void GetLastNote()
    {
        lastLeftNotes = new List<colorNotes>();
        lastRightNotes = new List<colorNotes>();

        for (int i = 0; i < Mathf.FloorToInt(currentBeat); i++)
        {
            int beat = Mathf.FloorToInt(currentBeat) - i;
            if (beat <= SpawnObjects.instance.BeatFromRealTime(length) && beat >= 0)
            {
                List<colorNotes> notes = LoadMap.instance.beats[Mathf.FloorToInt(currentBeat - i + 1)].colorNotes;
                foreach (var noteData in notes)
                {
                    if (noteData.b < currentBeat)
                    {
                        if (noteData.c == 0 && lastLeftNotes.Count == 0) GetNotesOnSameBeat(noteData.b, 0, lastLeftNotes, false);
                        else if (noteData.c == 1 && lastRightNotes.Count == 0) GetNotesOnSameBeat(noteData.b, 1, lastRightNotes, false);
                    }
                }
            }
        }
    }

    void GetNotesOnSameBeat(float beat, int type, List<colorNotes> list, bool next)
    {
        List<colorNotes> notes = LoadMap.instance.beats[Mathf.FloorToInt(beat)].colorNotes;

        float averageRotation = 0;
        int foundNotesCount = 0;

        foreach (var noteData in notes)
        {
            if (noteData.b == beat && noteData.c == type)
            {
                list.Add(noteData);
                foundNotesCount++;
                averageRotation += SpawnObjects.instance.Rotation(noteData.d);
            }
        }

        if (foundNotesCount == 0)
            return;

        averageRotation /= foundNotesCount;

        float radian = averageRotation * Mathf.Deg2Rad;
        Vector2 direction = (new Vector2(Mathf.Cos(radian), Mathf.Sin(radian))) * -1;

        List<colorNotes> sortedNotes = new List<colorNotes>(notes);

        sortedNotes.Sort((note1, note2) =>
        {
            Vector2 note1Position = new Vector2(note1.x, note1.y);
            Vector2 note2Position = new Vector2(note2.x, note2.y);

            float dotProduct1 = Vector2.Dot(note1Position, direction);
            float dotProduct2 = Vector2.Dot(note2Position, direction);

            return dotProduct2.CompareTo(dotProduct1);
        });

        if (next && type == 0) nextLeftNotes = sortedNotes;
        else if (next && type == 1) nextRightNotes = sortedNotes;
        else if (!next && type == 0) lastLeftNotes = sortedNotes;
        else if (!next && type == 1) lastRightNotes = sortedNotes;
    }
}
