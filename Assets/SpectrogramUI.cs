using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SpectrogramUI : MonoBehaviour
{
    public AudioSource audioSource;
    public int startOffset;
    public float duration;
    public int width;
    public int height;
    public RawImage render;
    private float contrast;
    public Gradient colorGradient;
    private bool applied;
    public float length;
    public bool generateWaveform = true; // Flag to decide if waveform should be generated


    void FFT(float[] x, float[] y, bool invert)
    {
        int n = x.Length;
        int m = (int)Mathf.Log(n, 2); // Calculate number of stages

        // Bit-reversal permutation
        for (int i = 0; i < n; i++)
        {
            int j = ReverseBits(i, m);
            if (j > i)
            {
                // Swap x[i] and x[j]
                float tempX = x[i];
                x[i] = x[j];
                x[j] = tempX;
                // Swap y[i] and y[j]
                float tempY = y[i];
                y[i] = y[j];
                y[j] = tempY;
            }
        }

        // Cooley-Tukey decimation-in-time radix-2 FFT
        for (int stage = 1; stage <= m; stage++)
        {
            int n2 = 1 << stage; // 2^stage
            int n1 = n2 >> 1; // n1 = n2 / 2

            float theta = (float)(Mathf.PI * 2 / n2) * (invert ? 1 : -1);
            float wReal = Mathf.Cos(theta);
            float wImaginary = Mathf.Sin(theta);

            for (int j = 0; j < n1; j++)
            {
                float wr = Mathf.Cos(j * theta);
                float wi = Mathf.Sin(j * theta);

                for (int i = j; i < n; i += n2)
                {
                    int i1 = i + n1;
                    float tempX = wr * x[i1] - wi * y[i1];
                    float tempY = wr * y[i1] + wi * x[i1];
                    x[i1] = x[i] - tempX;
                    y[i1] = y[i] - tempY;
                    x[i] += tempX;
                    y[i] += tempY;
                }
            }
        }

        if (invert)
        {
            // Scaling for inverse FFT
            for (int i = 0; i < n; i++)
            {
                x[i] /= n;
                y[i] /= n;
            }
        }
    }

    // Helper function to reverse bits in an integer
    int ReverseBits(int val, int bits)
    {
        int result = 0;
        for (int i = 0; i < bits; i++)
        {
            result = (result << 1) | (val & 1);
            val >>= 1;
        }
        return result;
    }

    void Start()
    {
        // if (!applied && startOffset >= 0 && length > startOffset)
        // {
        //     if (SpectrogramTextures.instance.spectrogram[startOffset] != null)
        //     {
        //         render.material.mainTexture = SpectrogramTextures.instance.spectrogram[startOffset];
        //         applied = true;
        //     }
        // }
    }

    void Update()
    {
        if (!applied && audioSource.clip != null)
        {
            Texture2D blackTexture = new Texture2D(1, 1);
            blackTexture.SetPixel(0, 0, Color.black);
            blackTexture.Apply();
            render.texture = blackTexture;
            StartCoroutine(GenerateSpectrogramCoroutine());
            applied = true;
        }
    }

    void GenerateWaveform()
    {
        Texture2D waveformTexture = new Texture2D(width, height);

        int startSample = (int)(startOffset * audioSource.clip.frequency);
        int endSample = Mathf.Min(startSample + (int)(duration * audioSource.clip.frequency), audioSource.clip.samples);
        int stepSize = (endSample - startSample) / width;

        Color[] waveformColors = new Color[width * height];

        for (int i = 0; i < width; i++)
        {
            float[] samples = new float[stepSize];
            audioSource.clip.GetData(samples, startSample + i * stepSize);

            float averageAmplitude = 0f;
            for (int j = 0; j < stepSize; j++)
            {
                averageAmplitude += Mathf.Abs(samples[j]);
            }
            averageAmplitude /= stepSize;

            int scaledAmplitude = (int)(averageAmplitude * height);

            for (int y = 0; y < height; y++)
            {
                Color color = y < scaledAmplitude ? Color.white : Color.black;
                waveformColors[i * height + y] = color;
            }
        }

        waveformTexture.SetPixels(waveformColors);
        waveformTexture.Apply();

        render.material.mainTexture = waveformTexture;
    }

    IEnumerator GenerateSpectrogramCoroutine()
    {
        Debug.Log("aaaaaaaaa");

        duration = LoadSong.instance.timeInSeconds;
        contrast = 0.25f;
        Texture2D spectrogramTexture = new Texture2D(width, height);

        int startSample = (int)(startOffset * audioSource.clip.frequency);
        int endSample = Mathf.Min(startSample + (int)(duration * audioSource.clip.frequency), audioSource.clip.samples);
        int windowSize = Mathf.ClosestPowerOfTwo(width);
        int stepSize = (endSample - startSample) / width;

        float[] spectrum = new float[windowSize];
        float[] samples = new float[windowSize];

        for (int i = 0; i < width; i++)
        {
            audioSource.clip.GetData(samples, startSample + i * stepSize);
            FFT(samples, new float[windowSize], false);

            for (int j = 0; j < spectrum.Length / 2; j++)
            {
                spectrum[j] = Mathf.Sqrt(samples[j] * samples[j] + samples[j + windowSize / 2] * samples[j + windowSize / 2]);
            }

            for (int j = 0; j < spectrum.Length; j++)
            {
                spectrum[j] = Mathf.Log(spectrum[j] + 1) * contrast;
            }

            Color[] columnColors = new Color[height];
            for (int j = 0; j < height; j++)
            {
                float value = j < spectrum.Length ? spectrum[j] : 0f;
                // Choose color based on gradient
                Color color = colorGradient.Evaluate(value);
                columnColors[j] = color;
            }

            spectrogramTexture.SetPixels(i, 0, 1, height, columnColors);
            yield return null;
        }

        spectrogramTexture.wrapMode = TextureWrapMode.Clamp;
        spectrogramTexture.Apply();
        render.texture = spectrogramTexture;
    }
}