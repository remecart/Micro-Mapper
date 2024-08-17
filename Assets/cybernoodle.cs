using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cybernoodle : MonoBehaviour
{
    public float currentBeat;
    public float length;
    public colorNotes left;
    public colorNotes right;
    public colorNotes lastLeft;
    public colorNotes lastRight;
    public GameObject leftSaber;
    public GameObject rightSaber;
    public float swingSize;
    public float rotationSpeed;
    private colorNotes cacheNextleft;
    private colorNotes cacheNextRight;
    public Quaternion targetRotation;
    public LineRenderer line;
    public LineRenderer line2;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
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

            leftSaber.transform.GetChild(0).gameObject.SetActive(!false);
            rightSaber.transform.GetChild(0).gameObject.SetActive(!false);

            UpdateSaberPositionAndRotation(leftSaber, lastLeft, left);
            UpdateSaberPositionAndRotation(rightSaber, lastRight, right);
        }
        else
        {
            leftSaber.transform.GetChild(0).gameObject.SetActive(false);
            rightSaber.transform.GetChild(0).gameObject.SetActive(false);
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

        Vector3 direction = nextPos - saber.transform.position;

        // Calculate a smooth step factor based on the point
        float smoothStepFactor = Mathf.SmoothStep(0, 1, point);

        // Update target rotation to smoothly transition to the next note's direction
        if (direction != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(direction);
        }

        // Slerp to the target rotation using smooth step factor
        saber.transform.rotation = Quaternion.Slerp(saber.transform.rotation, targetRotation, smoothStepFactor * Time.deltaTime * rotationSpeed);

        float temp = 1.25f;
        if (saber.name.Contains("Left")) temp = -1.25f;

        // Calculate the saber position using smooth interpolation
        saber.transform.localPosition = new Vector3(
            Mathf.Lerp(lastNote.x, nextNote.x, point) * 0.15f + temp,
            Mathf.Lerp(lastNote.y, nextNote.y, point) * 0.15f + 0.85f,
            -2
        );


        // VisualizeSwingPos(saber, nextNote.c);
    }

    void VisualizeSwingPos(GameObject saber, int type)
    {
        Vector3 pos = saber.transform.GetChild(0).GetChild(3).transform.position;

        if (type == 0)
        {
            line.positionCount++;
            line.SetPosition(line.positionCount - 1, pos);
        }
        else if (type == 1)
        {
            line2.positionCount++;
            line2.SetPosition(line.positionCount - 1, pos);

        }
    }

    void GetNextNote()
    {
        left = null;
        right = null;
        for (int i = 0; i < LoadMap.instance.beats.Count - Mathf.FloorToInt(currentBeat); i++)
        {
            if (currentBeat + i <= SpawnObjects.instance.BeatFromRealTime(length) && currentBeat + i >= 0)
            {
                List<colorNotes> notes = LoadMap.instance.beats[Mathf.FloorToInt(currentBeat + i)].colorNotes;
                foreach (var noteData in notes)
                {
                    if (noteData.b > currentBeat)
                    {
                        if (noteData.c == 0 && left == null) left = noteData;
                        else if (noteData.c == 1 && right == null) right = noteData;

                        if (left != null && right != null) return;
                    }
                }
            }
        }
    }

    void GetLastNote()
    {
        lastLeft = null;
        lastRight = null;

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
                        if (noteData.c == 0 && lastLeft == null) lastLeft = noteData;
                        else if (noteData.c == 1 && lastRight == null) lastRight = noteData;

                        if (lastLeft != null && lastRight != null) return;
                    }
                }
            }
        }
    }
}
