using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class DrawLines : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public LineRenderer lineRendererPrecision;
    public static DrawLines instance;
    private float fbeat;
    private float editorScale;
    public int distance;
    public BeatNumbers bn;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    void Update()
    {
        fbeat = SpawnObjects.instance.currentBeat;
        editorScale = SpawnObjects.instance.editorScale;
    }

    public void DrawLinesFromScratch(float fbeat, float precision)
    {
        DrawLinesWithPrecision(fbeat, precision);
        float editorScale = SpawnObjects.instance.editorScale;
        lineRenderer.positionCount = 0;

        float start = SpawnObjects.instance.BeatFromPosition(SpawnObjects.instance.PositionFromBeat(fbeat) - distance) - fbeat;
        float end = SpawnObjects.instance.BeatFromPosition(SpawnObjects.instance.PositionFromBeat(fbeat) + distance) - fbeat;
        int repeat = Mathf.CeilToInt(end - start);

        float max = LoadSong.instance.audioSource.clip.length * ReadMapInfo.instance.info._beatsPerMinute / 60;
        int beat = Mathf.CeilToInt(fbeat);

        foreach (Transform child in BeatNumbers.instance.parent.transform)
        {
            Destroy(child.gameObject);
        }

        int b = 0;

        for (int i = 0; i < repeat; i++)
        {
            b = i * 4;
            lineRenderer.positionCount = lineRenderer.positionCount + 4;
            if (i + beat - repeat / 2 >= 0 && i + beat + repeat / 2 < max + repeat + 1)
            {
                lineRenderer.SetPosition(b, new Vector3(2, 0, SpawnObjects.instance.PositionFromBeat(i + beat - repeat / 2) * editorScale));
                lineRenderer.SetPosition(b + 1, new Vector3(-2, 0, SpawnObjects.instance.PositionFromBeat(i + beat - repeat / 2) * editorScale));
                lineRenderer.SetPosition(b + 2, new Vector3(-2, 0, SpawnObjects.instance.PositionFromBeat(i + beat + 1 - repeat / 2) * editorScale));
                lineRenderer.SetPosition(b + 3, new Vector3(2, 0, SpawnObjects.instance.PositionFromBeat(i + beat + 1 - repeat / 2) * editorScale));

                BeatNumbers.instance.SpawnNumber(i + beat - repeat / 2);
            }
            else if (i + beat - repeat / 2 < 0)
            {
                lineRenderer.SetPosition(0, new Vector3(-2, 0, 0));
            }
            else if (i + beat + repeat / 2 > max + repeat)
            {
                lineRenderer.SetPosition(0, new Vector3(-2, 0, SpawnObjects.instance.PositionFromBeat(max) * editorScale));
            }
        }

        lineRenderer.positionCount = lineRenderer.positionCount + 1;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, new Vector3(2, 0, lineRenderer.GetPosition(0).z));
    }

    public void DrawLinesWithPrecision(float fbeat, float prec)
    {
        float precision = 1 / prec;
        if (precision < 4 && precision != 3) precision = 4; // && precision != 3
        float editorScale = SpawnObjects.instance.editorScale;
        lineRendererPrecision.positionCount = 0;

        float start = SpawnObjects.instance.BeatFromPosition(SpawnObjects.instance.PositionFromBeat(fbeat) - distance) - fbeat;
        float end = SpawnObjects.instance.BeatFromPosition(SpawnObjects.instance.PositionFromBeat(fbeat) + distance) - fbeat;
        int repeat = Mathf.CeilToInt(end - start);

        float max = LoadSong.instance.audioSource.clip.length * ReadMapInfo.instance.info._beatsPerMinute / 60;
        int beat = Mathf.CeilToInt(fbeat);

        int b = 0;

        for (int i = 0; i < repeat * precision + 1; i++)
        {
            float a = i / precision + beat + repeat / 2;

            b = i * 4;
            lineRendererPrecision.positionCount = lineRendererPrecision.positionCount + 4;
            if (i / precision + beat - repeat / 2 >= 0 && i / precision + beat + repeat / 2 < max + repeat + precision)
            {
                lineRendererPrecision.SetPosition(b, new Vector3(2, 0, SpawnObjects.instance.PositionFromBeat(i / precision + beat - repeat / 2) * editorScale));
                lineRendererPrecision.SetPosition(b + 1, new Vector3(-2, 0, SpawnObjects.instance.PositionFromBeat(i / precision + beat - repeat / 2) * editorScale));
                lineRendererPrecision.SetPosition(b + 2, new Vector3(-2, 0, SpawnObjects.instance.PositionFromBeat(i / precision + beat + precision - repeat / 2) * editorScale));
                lineRendererPrecision.SetPosition(b + 3, new Vector3(2, 0, SpawnObjects.instance.PositionFromBeat(i / precision + beat + precision - repeat / 2) * editorScale));
            }
            else if (i / precision + beat - repeat / 2 < 0)
            {
                lineRenderer.SetPosition(0, new Vector3(-2, 0, 0));
            }
            else if (i / precision + beat + repeat / 2 > max + repeat)
            {
                lineRenderer.SetPosition(0, new Vector3(-2, 0, SpawnObjects.instance.PositionFromBeat(max) * editorScale));
            }
        }

        lineRendererPrecision.positionCount = lineRendererPrecision.positionCount + 7;
        lineRendererPrecision.SetPosition(lineRendererPrecision.positionCount - 7, new Vector3(2, 0, lineRendererPrecision.GetPosition(0).z));
        lineRendererPrecision.SetPosition(lineRendererPrecision.positionCount - 6, new Vector3(1, 0, lineRendererPrecision.GetPosition(0).z));
        lineRendererPrecision.SetPosition(lineRendererPrecision.positionCount - 5, new Vector3(1, 0, lineRendererPrecision.GetPosition(b + 3).z));
        lineRendererPrecision.SetPosition(lineRendererPrecision.positionCount - 4, new Vector3(0, 0, lineRendererPrecision.GetPosition(b + 3).z));
        lineRendererPrecision.SetPosition(lineRendererPrecision.positionCount - 3, new Vector3(0, 0, lineRendererPrecision.GetPosition(0).z));
        lineRendererPrecision.SetPosition(lineRendererPrecision.positionCount - 2, new Vector3(-1, 0, lineRendererPrecision.GetPosition(0).z));
        lineRendererPrecision.SetPosition(lineRendererPrecision.positionCount - 1, new Vector3(-1, 0, lineRendererPrecision.GetPosition(b + 3).z));

    }

    // void FixPositions() {
    //     float lowerBound = SpawnObjects.instance.BeatFromPosition(SpawnObjects.instance.PositionFromBeat(fbeat) - distance) * editorScale;
    //     float upperBound = SpawnObjects.instance.BeatFromPosition(SpawnObjects.instance.PositionFromBeat(fbeat) + distance) * editorScale;

    //     for (int i = 0; i < lineRenderer.positionCount; i++)
    //     {
    //         float clampedPos = Mathf.Clamp(lineRenderer.GetPosition(i).z, lowerBound, upperBound);
    //         lineRenderer.SetPosition(i, new Vector3(lineRenderer.GetPosition(i).x, 0, clampedPos));
    //     }

    //     lineRenderer.SetPosition(lineRenderer.positionCount - 1, new Vector3(lineRenderer.GetPosition(lineRenderer.positionCount - 1).x, 0, upperBound));
    //     lineRenderer.SetPosition(lineRenderer.positionCount - 2, new Vector3(lineRenderer.GetPosition(lineRenderer.positionCount - 2).x, 0, upperBound));


    //     for (int i = 0; i < lineRendererPrecision.positionCount; i++)
    //     {
    //         float clampedPos = Mathf.Clamp(lineRendererPrecision.GetPosition(i).z, lowerBound, upperBound);
    //         lineRendererPrecision.SetPosition(i, new Vector3(lineRendererPrecision.GetPosition(i).x, -0.05f, clampedPos));
    //     }
    // }
}
