using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FpsCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText;

    private int frames;
    private float timer;

    void Update()
    {
        frames++;
        timer += Time.deltaTime;

        if (timer >= 1.0f)
        {
            int fps = Mathf.RoundToInt(frames / timer);
            fpsText.text = " " + fps.ToString() + " fps";

            frames = 0;
            timer = 0;
        }
    }
}
