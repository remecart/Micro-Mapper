using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEngine;

public class DrawLines : MonoBehaviour
{
    public static DrawLines instance;
    private float fbeat;
    private float editorScale;
    public int distance;
    public BeatNumbers bn;
    public List<BeatLines> lines;

    private bool audioCheckOnStart;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    void Update()
    {
        fbeat = SpawnObjects.instance.currentBeat;
        editorScale = SpawnObjects.instance.editorScale;

        if (LoadSong.instance.audioSource.clip != null && !audioCheckOnStart)
        {
            audioCheckOnStart = true;
            DrawLines.instance.DrawLinesWhenRequired();
        }

        if (Mathf.FloorToInt(SpawnObjects.instance.currentBeat) != currentBeatCache)
        {
            DrawLinesWhenRequired();
            currentBeatCache = Mathf.FloorToInt(SpawnObjects.instance.currentBeat);
        }
    }

    private int currentBeatCache = 1;

    public void DrawLinesWhenRequired()
    {
        for (int i = 0; i < lines.Count; i++)
        {
            DrawBeatsFromScratch(SpawnObjects.instance.currentBeat, SpawnObjects.instance.precision, lines[i].beat, lines[i].precision, lines[i].width / 2);
        }

        FixPositions();
    }

    public void DrawXue(float fbeat, float precision, LineRenderer lineRenderer, LineRenderer lineRendererPrecision,
        float laneIndex)
    {
        float editorScale = SpawnObjects.instance.editorScale;
        lineRenderer.positionCount = 0;

        float start = SpawnObjects.instance.BeatFromPosition(SpawnObjects.instance.PositionFromBeat(fbeat) - distance) -
                      fbeat;
        float end = SpawnObjects.instance.BeatFromPosition(SpawnObjects.instance.PositionFromBeat(fbeat) + distance) -
                    fbeat;
        int repeat = Mathf.CeilToInt(end - start);

        int max = Mathf.CeilToInt(SpawnObjects.instance.BeatFromRealTime(LoadSong.instance.audioSource.clip.length));
        int beat = Mathf.CeilToInt(fbeat);

        foreach (Transform child in BeatNumbers.instance.parent.transform)
        {
            Destroy(child.gameObject);
        }

        int b = 0;

        for (int i = 0; i < repeat + 1; i++)
        {
            b = i * 4;

            // Removed outline creation
            lineRenderer.positionCount = lineRenderer.positionCount + 4;
            if (i + beat - repeat / 2 >= 0)
            {
                // Only keeping necessary positions
                lineRenderer.SetPosition(b,
                    new Vector3(0, 0, SpawnObjects.instance.PositionFromBeat(i + beat - repeat / 2) * editorScale));
                lineRenderer.SetPosition(b + 1,
                    new Vector3(0, 0, SpawnObjects.instance.PositionFromBeat(i + beat + 1 - repeat / 2) * editorScale));

                BeatNumbers.instance.SpawnNumber(i + beat - repeat / 2);
            }
            else if (i + beat - repeat / 2 < 0)
            {
                lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
            }
        }

        // BeatNumbers.instance.SpawnNumber(beat - repeat / 2 + 1);

        lineRenderer.positionCount = lineRenderer.positionCount + 1;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, new Vector3(0, 0, lineRenderer.GetPosition(0).z));
    }

    public void DrawBeatsFromScratch(float fbeat, float precision, LineRenderer lineRenderer,
        LineRenderer lineRendererPrecision, float laneIndex)
    {
        if (lineRenderer.transform.parent.gameObject.name.ToLower() == "xue")
        {
            //DrawXue(fbeat, precision, lineRenderer, lineRendererPrecision, laneIndex);
            return;
        }

        DrawLinesWithPrecision(fbeat, precision, lineRendererPrecision, laneIndex);
        float editorScale = SpawnObjects.instance.editorScale;
        lineRenderer.positionCount = 0;

        float start = SpawnObjects.instance.BeatFromPosition(SpawnObjects.instance.PositionFromBeat(fbeat) - distance) -
                      fbeat;
        float end = SpawnObjects.instance.BeatFromPosition(SpawnObjects.instance.PositionFromBeat(fbeat) + distance) -
                    fbeat;
        int repeat = Mathf.CeilToInt(end - start);

        int max = Int32.MaxValue;
        if (LoadSong.instance.audioSource.clip) max = Mathf.CeilToInt(SpawnObjects.instance.BeatFromRealTime(LoadSong.instance.audioSource.clip.length));
        int beat = Mathf.CeilToInt(fbeat);

        foreach (Transform child in BeatNumbers.instance.parent.transform)
        {
            Destroy(child.gameObject);
        }

        int b = 0;

        for (int i = 0; i < repeat + 1; i++)
        {
            b = i * 4;
            lineRenderer.positionCount = lineRenderer.positionCount + 4;
            if (i + beat - repeat / 2 >= 0) // && i + beat + repeat / 2 < max + repeat + 3
            {
                lineRenderer.SetPosition(b,
                    new Vector3(laneIndex, 0,
                        SpawnObjects.instance.PositionFromBeat(i + beat - repeat / 2) * editorScale));
                lineRenderer.SetPosition(b + 1,
                    new Vector3(-laneIndex, 0,
                        SpawnObjects.instance.PositionFromBeat(i + beat - repeat / 2) * editorScale));
                lineRenderer.SetPosition(b + 2,
                    new Vector3(-laneIndex, 0,
                        SpawnObjects.instance.PositionFromBeat(i + beat + 1 - repeat / 2) * editorScale));
                lineRenderer.SetPosition(b + 3,
                    new Vector3(laneIndex, 0,
                        SpawnObjects.instance.PositionFromBeat(i + beat + 1 - repeat / 2) * editorScale));

                BeatNumbers.instance.SpawnNumber(i + beat - repeat / 2);
            }
            else if (i + beat - repeat / 2 < 0)
            {
                lineRenderer.SetPosition(0, new Vector3(-laneIndex, 0, 0));
            }
            //else if (i + beat + repeat / 2 > max + repeat)
            //{
            //    lineRenderer.SetPosition(0, new Vector3(-laneIndex, 0, SpawnObjects.instance.PositionFromBeat(max) * editorScale));
            //}
        }

        BeatNumbers.instance.SpawnNumber(beat - repeat / 2 + 1);

        lineRenderer.positionCount = lineRenderer.positionCount + 1;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1,
            new Vector3(laneIndex, 0, lineRenderer.GetPosition(0).z));

        //FixPositions();
    }

    public void DrawLinesWithPrecision(float fbeat, float prec, LineRenderer lineRendererPrecision, float laneIndex)
    {
        float precision = 1 / prec;
        if (precision < 4 && precision != 3) precision = 4; // && precision != 3
        float editorScale = SpawnObjects.instance.editorScale;
        lineRendererPrecision.positionCount = 0;

        float start = SpawnObjects.instance.BeatFromPosition(SpawnObjects.instance.PositionFromBeat(fbeat) - distance) -
                      fbeat;
        float end = SpawnObjects.instance.BeatFromPosition(SpawnObjects.instance.PositionFromBeat(fbeat) + distance) -
                    fbeat;
        int repeat = Mathf.CeilToInt(end - start);
        int beat = Mathf.CeilToInt(fbeat);

        int b = 0;
        for (int i = 0; i < repeat * precision + precision; i++)
        {
            float currentBeat = i / precision + beat - repeat / 2;
            if (currentBeat < 0) continue; // Skip negative positions

            // Set the four corner positions for each line segment
            b = lineRendererPrecision.positionCount;
            lineRendererPrecision.positionCount += 4;

            float beatPosition = SpawnObjects.instance.PositionFromBeat(currentBeat) * editorScale;
            float nextBeatPosition = SpawnObjects.instance.PositionFromBeat(currentBeat + 1 / precision) * editorScale;

            // Assign positions for each segment precisely
            lineRendererPrecision.SetPosition(b, new Vector3(laneIndex, 0, beatPosition));
            lineRendererPrecision.SetPosition(b + 1, new Vector3(-laneIndex, 0, beatPosition));
            lineRendererPrecision.SetPosition(b + 2, new Vector3(-laneIndex, 0, nextBeatPosition));
            lineRendererPrecision.SetPosition(b + 3, new Vector3(laneIndex, 0, nextBeatPosition));
        }

        int edited = 0;

        lineRendererPrecision.positionCount = lineRendererPrecision.positionCount + 2;

        lineRendererPrecision.SetPosition(lineRendererPrecision.positionCount - 2,
            new Vector3(0, 0, lineRendererPrecision.GetPosition(b + 3).z));
        lineRendererPrecision.SetPosition(lineRendererPrecision.positionCount - 1,
            new Vector3(laneIndex - edited, 0, lineRendererPrecision.GetPosition(b + 3).z));

        for (int i = 0; i < laneIndex * 2; i++)
        {
            lineRendererPrecision.positionCount = lineRendererPrecision.positionCount + 3;

            lineRendererPrecision.SetPosition(lineRendererPrecision.positionCount - 3,
                new Vector3(laneIndex - edited, 0, lineRendererPrecision.GetPosition(0).z));
            lineRendererPrecision.SetPosition(lineRendererPrecision.positionCount - 2,
                new Vector3(laneIndex - edited, 0, lineRendererPrecision.GetPosition(b + 3).z));
            edited++;
            lineRendererPrecision.SetPosition(lineRendererPrecision.positionCount - 1,
                new Vector3(laneIndex - edited, 0, lineRendererPrecision.GetPosition(b + 3).z));
        }
    }

    public void FixPositions()
    {
        float lowerBound = 0;
        float upperBound = int.MinValue;
        if (LoadSong.instance.audioSource.clip) upperBound =
            SpawnObjects.instance.PositionFromBeat(
                SpawnObjects.instance.BeatFromRealTime(LoadSong.instance.audioSource.clip.length)) *
            SpawnObjects.instance.editorScale;
        

        foreach (var item in lines)
        {
            for (int i = 0; i < item.beat.positionCount; i++)
            {
                float clampedPos = Mathf.Clamp(item.beat.GetPosition(i).z, lowerBound, upperBound);
                item.beat.SetPosition(i, new Vector3(item.beat.GetPosition(i).x, 0.05f, clampedPos));
            }
        }

        foreach (var item in lines)
        {
            for (int i = 0; i < item.precision.positionCount; i++)
            {
                float clampedPos = Mathf.Clamp(item.precision.GetPosition(i).z, lowerBound, upperBound);
                item.precision.SetPosition(i, new Vector3(item.precision.GetPosition(i).x, -0.05f, clampedPos));
            }
        }
    }
}

[System.Serializable]
public class BeatLines
{
    public LineRenderer beat;
    public LineRenderer precision;
    public float width;
}