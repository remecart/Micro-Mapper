using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpectrogramUI : MonoBehaviour
{
    public AudioSource audioSource;
    public RawImage render;
    private bool applied;

    public int wave_width = 1024;
    public int wave_height = 256;
    public Color waveformColor = Color.white;
    public Color waveformBackgroundColor = Color.black;
    public Color waveformOuterColor = Color.red;

    void Start()
    {
        // Placeholder for potential initialization
    }

    void Update()
    {
        if (!applied && audioSource.clip != null)
        {
            StartCoroutine(GenerateWaveform());
            applied = true;
        }
    }

    IEnumerator GenerateWaveform()
    {
        Texture2D waveformTexture = new Texture2D(wave_width, wave_height);
        Color[] waveformColors = new Color[wave_width * wave_height];

        int stepSize = audioSource.clip.samples / wave_width;
        float[] samples = new float[stepSize];

        for (int x = 0; x < wave_width; x++)
        {
            audioSource.clip.GetData(samples, x * stepSize);

            float minSample = float.MaxValue;
            float maxSample = float.MinValue;

            for (int i = 0; i < stepSize; i++)
            {
                float sample = samples[i];
                if (sample < minSample) minSample = sample;
                if (sample > maxSample) maxSample = sample;
            }

            int minY = Mathf.FloorToInt((minSample + 1) * 0.5f * wave_height);
            int maxY = Mathf.FloorToInt((maxSample + 1) * 0.5f * wave_height);

            for (int y = 0; y < wave_height; y++)
            {
                if (y >= minY && y <= maxY)
                {
                    if (y > wave_height / 2 && y >= (maxY - wave_height / 8f))
                    {
                        waveformColors[x + y * wave_width] = waveformOuterColor;
                    }
                    else if (y < wave_height / 2 && y <= (minY + wave_height / 8f))
                    {
                        waveformColors[x + y * wave_width] = waveformOuterColor;
                    }
                    else waveformColors[x + y * wave_width] = waveformColor;
                }
                else
                {
                    waveformColors[x + y * wave_width] = waveformBackgroundColor;
                }
            }

            if (x % 5 == 0)
            {
                yield return null;
            }
        }

        waveformTexture.SetPixels(waveformColors);
        waveformTexture.wrapMode = TextureWrapMode.Clamp;
        waveformTexture.Apply();

        render.texture = waveformTexture;
    }
}
