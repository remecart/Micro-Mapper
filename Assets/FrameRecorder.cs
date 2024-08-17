
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class FrameRecorder : MonoBehaviour
{
    private const int FrameRate = 60;
    private int currentFrame;
    public string savePath;
    private bool isRecording;
    float seconds;
    // public List<TextMeshProUGUI> text;

    private void Start()
    {
        Directory.CreateDirectory(savePath);
        Time.captureFramerate = FrameRate;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            isRecording = true;
            StartCoroutine(CurrentTime());
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            isRecording = false;
            currentFrame = 0;
        }

        if (isRecording)
        {
            StartCoroutine(CaptureFrame());
        }
    }

    private IEnumerator CaptureFrame()
    {
        currentFrame++;
        // text[0].text = "frame" + currentFrame + ".jpg";
        yield return new WaitForEndOfFrame();
        byte[] bytes = ScreenCapture.CaptureScreenshotAsTexture().EncodeToJPG();
        string fileName = string.Format("frame{0:D5}.jpg", currentFrame);
        File.WriteAllBytes(savePath + "/" + fileName, bytes);
    }

    private IEnumerator CurrentTime()
    {
        seconds = seconds + 1;
        // text[1].text = "realtime: " + seconds + " seconds";
        // text[2].text = "speed(tfr/cf): " + (currentFrame / (FrameRate seconds) * 100) + "%";
        yield return new WaitForSecondsRealtime(1);
        StartCoroutine(CurrentTime());
    }
}