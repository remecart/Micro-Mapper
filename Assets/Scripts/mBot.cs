using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class mBot : MonoBehaviour
{
    public static mBot instance;
    public float currentBeat;
    public float length;
    public colorNotes left;
    public colorNotes right;
    public colorNotes lastLeft;
    public colorNotes lastRight;
    public GameObject leftSaber;
    public GameObject rightSaber;
    public LineRenderer line;
    public LineRenderer line2;

    public int rotationSpeed;
    public float intensity;
    public float planeOffset;
    public float overshoot;
    public float zOffset;
    public float positionMultiplier;
    public float saberWidth;
    public float saberLength;
    public float trailWidth;
    public float trailLength;

    public bool visualizeSwings;

    public bool playing;
    public LineRenderer planeLine;

    public List<LineRenderer> curveLines;

    public RawImage img;
    public List<Texture2D> textures;

    // Start is called before the first frame update

    public void ToggleMBot()
    {
        playing = !playing;

        int index = playing ? 1 : 0;

        img.texture = textures[index];

        var leftTrail = leftSaber.transform.GetChild(0).GetChild(3).GetComponent<TrailRenderer>();
        var rightTrail = rightSaber.transform.GetChild(0).GetChild(3).GetComponent<TrailRenderer>();

        leftTrail.Clear();
        rightTrail.Clear();

        leftSaber.transform.GetChild(0).gameObject.SetActive(playing);
        rightSaber.transform.GetChild(0).gameObject.SetActive(playing);
    }

    void Start()
    {
        // Initialization if needed
        instance = this;

        intensity = Settings.instance.config.mBot.mBotSettings.intensity;
        planeOffset = Settings.instance.config.mBot.mBotSettings.planeOffset;
        overshoot = Settings.instance.config.mBot.mBotSettings.overshoot;
        positionMultiplier = Settings.instance.config.mBot.mBotSettings.positionMultiplier;
        zOffset = Settings.instance.config.mBot.mBotSettings.zOffset;
        visualizeSwings = Settings.instance.config.mBot.mBotSettings.visualizeSwings;
        saberWidth = Settings.instance.config.mBot.mBotSaber.saberWidth;
        saberLength = Settings.instance.config.mBot.mBotSaber.saberLength;
        trailWidth = Settings.instance.config.mBot.mBotSaber.trailWidth;
        trailLength = Settings.instance.config.mBot.mBotSaber.trailLength;
    }

    private Color LeftSaberColor;
    private Color RightSaberColor;
    [SerializeField]
    private Material[] LeftSaberMaterials;
    [SerializeField]
    private Material[] RightSaberMaterials;

    private void SetSaberColor()
    {
        Color newLeftSaberColor = Settings.instance.config.mapping.colorSettings.leftNote.ToColor();
        Color newRightSaberColor = Settings.instance.config.mapping.colorSettings.rightNote.ToColor();

        // Ensure alpha is at max
        newLeftSaberColor.a = 1f;
        newRightSaberColor.a = 1f;

        // Convert to HSV and adjust saturation if needed
        Color.RGBToHSV(newLeftSaberColor, out var leftH, out var leftS, out var leftV);
        Color.RGBToHSV(newRightSaberColor, out var rightH, out var rightS, out var rightV);

        if (leftS > 0.4f) leftS -= 0.4f;
        if (rightS > 0.4f) rightS -= 0.4f;

        newLeftSaberColor = Color.HSVToRGB(leftH, leftS, leftV);
        newRightSaberColor = Color.HSVToRGB(rightH, rightS, rightV);

        // Ensure alpha is at max again after conversion
        newLeftSaberColor.a = 1f;
        newRightSaberColor.a = 1f;

        if (LeftSaberColor != newLeftSaberColor)
        {
            LeftSaberColor = newLeftSaberColor;
            Color.RGBToHSV(LeftSaberColor, out var H, out var S, out var V);
            var HandleColor = Color.HSVToRGB(H, S / 2f, 0.4f);
            HandleColor.a = 1f; // Ensure alpha is at max

            var BladeColor = Color.HSVToRGB(H, S / 2f, 0.95f);
            BladeColor.a = 1f; // Ensure alpha is at max

            LeftSaberMaterials[0].color = BladeColor;
            LeftSaberMaterials[1].color = HandleColor;
        }

        if (RightSaberColor != newRightSaberColor)
        {
            RightSaberColor = newRightSaberColor;
            Color.RGBToHSV(RightSaberColor, out var H, out var S, out var V);
            var HandleColor = Color.HSVToRGB(H, S / 2f, 0.4f);
            HandleColor.a = 1f; // Ensure alpha is at max

            var BladeColor = Color.HSVToRGB(H, S / 2f, 0.95f);
            BladeColor.a = 1f; // Ensure alpha is at max

            RightSaberMaterials[0].color = BladeColor;
            RightSaberMaterials[1].color = HandleColor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        SetSaberColor();
        planeLine.gameObject.SetActive(visualizeSwings);
        if (visualizeSwings)
        {
            planeLine.gameObject.transform.localPosition = new Vector3(0, Settings.instance.config.mBot.mBotSettings.planeOffset * -1f, 0);
            int verticies = 24;

            curveLines[0].positionCount = verticies;
            curveLines[1].positionCount = verticies;

            for (int i = 0; i < verticies; i++)
            {
                curveLines[0].SetPosition(i, GetPoint(leftSaber, lastLeft, left, i / (float)verticies));
                curveLines[1].SetPosition(i, GetPoint(rightSaber, lastRight, right, i / (float)verticies));

                //Vector2 direc = curveLines[0].GetPosition(verticies - 2) - curveLines[0].GetPosition(verticies - 1);
                //curveLines[0].positionCount += 3;
            }
        }

        currentBeat = SpawnObjects.instance.currentBeat;

        if (length == 0)
        {
            if (LoadSong.instance.audioSource.clip)
                length = LoadSong.instance.audioSource.clip.length;
        }
        
        if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.enableMBot))
        {
            ToggleMBot(); 
        }

        if (playing)
        {
            UpdateSaberPositionAndRotation(leftSaber, lastLeft, left);
            UpdateSaberPositionAndRotation(rightSaber, lastRight, right);
        }

        var leftTrail = leftSaber.transform.GetChild(0).GetChild(3).GetComponent<TrailRenderer>();
        var rightTrail = rightSaber.transform.GetChild(0).GetChild(3).GetComponent<TrailRenderer>();

        if (!SpawnObjects.instance.playing)
        {
            leftTrail.enabled = false;
            rightTrail.enabled = false;
            leftTrail.Clear();
            rightTrail.Clear();
        }
        else
        {
            leftTrail.enabled = true;
            rightTrail.enabled = true;
        }
    }

    public LineRenderer test;

    private Vector2 controlPointA;
    private Vector2 controlPointB;
    public Quaternion targetRotation;

    void UpdateSaberPositionAndRotation(GameObject saber, colorNotes lastNote, colorNotes nextNote)
    {
        GetNextNote();
        GetLastNote();

        if (nextNote == null) return;
        if (lastNote == null) lastNote = new colorNotes();

        float durationLeft = nextNote.b - lastNote.b;
        float point = (currentBeat - lastNote.b) / durationLeft;

        int cutDirection1 = lastNote.d;
        float angle1 = SpawnObjects.instance.Rotation(cutDirection1) - 90;

        int cutDirection2 = nextNote.d;
        float angle2;

        // Adjust the angles based on cutDirection
        if (nextNote.d == 8 && lastNote.d == 8)
        {
            // If both notes are dot notes, perform a simple linear interpolation
            angle2 = angle1; // Keep the same direction for smooth interpolation
        }
        else if (nextNote.d == 8)
        {
            // If the next note is a dot note, swing in the opposite direction from the previous note
            angle2 = SpawnObjects.instance.Rotation(cutDirection1) - 90;
        }
        else if (lastNote.d == 8)
        {
            // If the last note was a dot note, invert the angle for the next note
            angle2 = SpawnObjects.instance.Rotation(cutDirection2) + 90;
            angle1 = SpawnObjects.instance.Rotation(cutDirection2) + 90;
        }
        else
        {
            // Standard case when both notes have a directional cut
            angle2 = SpawnObjects.instance.Rotation(cutDirection2) + 90;
        }

        List<colorNotes> foundNotes = new List<colorNotes>();
        List<colorNotes> allNotesOnBeat = LoadMap.instance.beats[Mathf.FloorToInt(nextNote.b)].colorNotes;
        foreach (var noteData in allNotesOnBeat)
        {
            if (noteData.b == nextNote.b) foundNotes.Add(noteData);
        }

        Vector3 lastPos = new Vector3(SpawnObjects.instance.ConvertMEPos(lastNote.x), SpawnObjects.instance.ConvertMEPos(lastNote.y), SpawnObjects.instance.PositionFromBeat(lastNote.b) * SpawnObjects.instance.editorScale);
        Vector3 nextPos = new Vector3(SpawnObjects.instance.ConvertMEPos(nextNote.x), SpawnObjects.instance.ConvertMEPos(nextNote.y), SpawnObjects.instance.PositionFromBeat(nextNote.b) * SpawnObjects.instance.editorScale);


        Vector3 CalculatedPos;

        if (nextNote.d == 8 && lastNote.d == 8)
        {
            // When both current and next notes are dot notes, perform a simple linear interpolation
            Vector2 LerpPos = Vector2.Lerp(lastPos, nextPos, point);
            CalculatedPos = new Vector3(LerpPos.x + 0.5f, LerpPos.y + 0.5f, planeOffset);
        }
        else
        {
            // Calculate the position on the Bezier curve
            Vector2 bezierPos = GetPointOnBezierCurve(lastPos, nextPos, angle1, angle2, point);
            CalculatedPos = new Vector3(bezierPos.x + 0.5f, bezierPos.y + 0.5f, planeOffset);
        }

        // Calculate the direction to look at
        Vector3 direction = (CalculatedPos - saber.transform.position).normalized;

        // Update target rotation to smoothly transition to the next note's direction
        if (direction != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(direction);
        }

        // Slerp to the target rotation using smooth step factor
        saber.transform.rotation = Quaternion.Slerp(saber.transform.rotation, targetRotation, point);
        saber.transform.localScale = new Vector3(saberWidth / 10, saberWidth / 10, saberLength / 10);

        saber.transform.GetChild(0).transform.localPosition = new Vector3(0, 0, zOffset / 5f);

        saber.transform.GetChild(0).GetChild(saber.transform.GetChild(0).childCount - 1).GetComponent<TrailRenderer>().time = trailLength / 100f;
        saber.transform.GetChild(0).GetChild(saber.transform.GetChild(0).childCount - 1).GetComponent<TrailRenderer>().widthMultiplier = trailWidth / 100f +0.01f;

        //intensity* Time.deltaTime* rotationSpeed

        var xoffset = -1.1f;
        if (saber.name.Contains("Right"))
        {
            xoffset = 1.1f;
        }
        saber.transform.localPosition = new Vector3((CalculatedPos.x - 2) * positionMultiplier / 10 + xoffset, CalculatedPos.y * positionMultiplier / 10 + 1f, saber.transform.localPosition.z);
    }

    public Vector3 GetPoint(GameObject saber, colorNotes lastNote, colorNotes nextNote, float time)
    {
        if (nextNote == null) nextNote = new colorNotes();
        if (lastNote == null) lastNote = new colorNotes();

        float point = time;

        int cutDirection1 = lastNote.d;
        float angle1 = SpawnObjects.instance.Rotation(cutDirection1) - 90;

        int cutDirection2 = nextNote.d;
        float angle2;

        // Adjust the angles based on cutDirection
        if (nextNote.d == 8 && lastNote.d == 8)
        {
            // If both notes are dot notes, perform a simple linear interpolation
            angle2 = angle1; // Keep the same direction for smooth interpolation
        }
        else if (nextNote.d == 8)
        {
            // If the next note is a dot note, swing in the opposite direction from the previous note
            angle2 = SpawnObjects.instance.Rotation(cutDirection1) - 90;
        }
        else if (lastNote.d == 8)
        {
            // If the last note was a dot note, invert the angle for the next note
            angle2 = SpawnObjects.instance.Rotation(cutDirection2) + 90;
            angle1 = SpawnObjects.instance.Rotation(cutDirection2) + 90;
        }
        else
        {
            // Standard case when both notes have a directional cut
            angle2 = SpawnObjects.instance.Rotation(cutDirection2) + 90;
        }

        List<colorNotes> foundNotes = new List<colorNotes>();
        List<colorNotes> allNotesOnBeat = LoadMap.instance.beats[Mathf.FloorToInt(nextNote.b)].colorNotes;
        foreach (var noteData in allNotesOnBeat)
        {
            if (noteData.b == nextNote.b) foundNotes.Add(noteData);
        }

        Vector3 lastPos = new Vector3(SpawnObjects.instance.ConvertMEPos(lastNote.x), SpawnObjects.instance.ConvertMEPos(lastNote.y), SpawnObjects.instance.PositionFromBeat(lastNote.b) * SpawnObjects.instance.editorScale);
        Vector3 nextPos = new Vector3(SpawnObjects.instance.ConvertMEPos(nextNote.x), SpawnObjects.instance.ConvertMEPos(nextNote.y), SpawnObjects.instance.PositionFromBeat(nextNote.b) * SpawnObjects.instance.editorScale);

        Vector3 CalculatedPos;

        if (nextNote.d == 8 && lastNote.d == 8)
        {
            // When both current and next notes are dot notes, perform a simple linear interpolation
            Vector2 LerpPos = Vector2.Lerp(lastPos, nextPos, point);
            CalculatedPos = new Vector3(LerpPos.x + 0.5f, LerpPos.y + 0.5f, planeOffset);
        }
        else
        {
            // Calculate the position on the Bezier curve
            Vector2 bezierPos = GetPointOnBezierCurve(lastPos, nextPos, angle1, angle2, point);
            CalculatedPos = new Vector3(bezierPos.x + 0.5f, bezierPos.y + 0.5f, planeOffset);
        }

        return CalculatedPos;
    }





    public Vector2 GetPointOnBezierCurve(Vector2 pointA, Vector2 pointB, float angleA, float angleB, float time)
    {
        float distance = (pointB - pointA).magnitude;
        float handleLengthA = distance * 0.3f * intensity + overshoot; // Additive overshoot
        float handleLengthB = distance * 0.3f * intensity + overshoot; // Additive overshoot

        Vector2 controlPointA = pointA + new Vector2(Mathf.Cos(angleA * Mathf.Deg2Rad), Mathf.Sin(angleA * Mathf.Deg2Rad)) * handleLengthA;
        Vector2 controlPointB = pointB + new Vector2(Mathf.Cos(angleB * Mathf.Deg2Rad), Mathf.Sin(angleB * Mathf.Deg2Rad)) * handleLengthB;

        return Mathf.Pow(1 - time, 3) * pointA +
               3 * Mathf.Pow(1 - time, 2) * time * controlPointA +
               3 * (1 - time) * Mathf.Pow(time, 2) * controlPointB +
               Mathf.Pow(time, 3) * pointB;
    }




    void RemoveFirstPosition()
    {
        if (test.positionCount > 1)
        {
            for (int i = 0; i < test.positionCount - 1; i++)
            {
                test.SetPosition(i, test.GetPosition(i + 1));
            }
            test.positionCount--;
        }
        else if (test.positionCount == 1)
        {
            test.positionCount = 0;
        }
    }

    void GetNextNote()
    {
        left = null;
        right = null;

        int beats = Mathf.FloorToInt(SpawnObjects.instance.BeatFromRealTime(LoadSong.instance.audioSource.clip.length));

        for (int i = 0; i < beats - Mathf.FloorToInt(currentBeat); i++)
        {
            //if (currentBeat + i <= SpawnObjects.instance.BeatFromRealTime(length) && currentBeat + i >= 0)
            //{
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
            //}
        }
    }

    void GetLastNote()
    {
        lastLeft = null;
        lastRight = null;
        float closestLeftBeat = float.MinValue;
        float closestRightBeat = float.MinValue;

        for (int i = 0; i < Mathf.FloorToInt(currentBeat); i++)
        {
            //if (i < LoadMap.instance.beats.Count)
            //{
                List<colorNotes> notes = LoadMap.instance.beats[Mathf.FloorToInt(currentBeat) - i + 1].colorNotes;
                foreach (var noteData in notes)
                {
                    if (noteData.b <= currentBeat)
                    {
                        if (noteData.c == 0 && noteData.b > closestLeftBeat)
                        {
                            lastLeft = noteData;
                            closestLeftBeat = noteData.b;
                        }
                        else if (noteData.c == 1 && noteData.b > closestRightBeat)
                        {
                            lastRight = noteData;
                            closestRightBeat = noteData.b;
                        }
                    }
                }
            //}
        }
    }
}