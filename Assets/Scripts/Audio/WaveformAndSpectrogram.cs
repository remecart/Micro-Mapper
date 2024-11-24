using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using static Spectrogram;
using static Waveform;

public class WaveformAndSpectrogram : MonoBehaviour
{
    [Header("Settings")]
    public AudioSource audioSource;
    public MeshRenderer render;
    private bool applied;
    private bool generatedShells;
    public int startOffset;
    public int duration;
    public bool generateWaveform;

    [Header("Spectrogram")]
    public int spectro_width;
    public int spectro_height;
    public float intensity;
    public Gradient gradient;

    [Header("Waveform")]
    public int wave_width;
    public int wave_height;

    public Color waveformColor;
    public Color waveformOuterColor;
    public Color waveformBackgroundColor;
    public float length;

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
        audioSource = LoadSong.instance.audioSource;
        generateWaveform = InstantiateSpectrogram.instance.waveform;

        // if (!applied && startOffset >= 0 && length > startOffset)
        // {
        //     if (SpectrogramTextures.instance.spectrogram[startOffset] != null)
        //     {
        //         render.material.mainTexture = SpectrogramTextures.instance.spectrogram[startOffset];
        //         applied = true;
        //     }
        // }

        if (audioSource.clip == null)
        {
            audioSource.clip = LoadSong.instance.audioSource.clip;
        }
        if (length == 0)
        {
            length = LoadSong.instance.audioSource.clip.length;
        }

        if (!applied && startOffset >= 0 && length > startOffset)
        {
            if (Settings.instance.config.visuals.audioVisualizer == Visuals.AudioVisualizer.Waveform)
            {

                if (SpectrogramTextures.instance.waveform[startOffset] == null)
                {
                    Texture2D blackTexture = new Texture2D(1, 1);
                    blackTexture.SetPixel(0, 0, Color.black);
                    blackTexture.Apply();
                    render.material.mainTexture = blackTexture;
                    StartCoroutine(GenerateWaveform());
                }
                else
                {
                    render.material.mainTexture = SpectrogramTextures.instance.waveform[startOffset];
                }
            }
            else
            {
                if (SpectrogramTextures.instance.spectrogram[startOffset] == null)
                {
                    Texture2D blackTexture = new Texture2D(1, 1);
                    blackTexture.SetPixel(0, 0, Color.black);
                    blackTexture.Apply();
                    render.material.mainTexture = blackTexture;
                    StartCoroutine(GenerateSpectrogram());
                }
                else
                {
                    render.material.mainTexture = SpectrogramTextures.instance.spectrogram[startOffset];
                }
            }
            applied = true;
        }

        if (!generatedShells && applied)
        {
            if (SpectrogramTextures.instance.spectrogram[startOffset] == render.material.mainTexture)
            {
                if (generateWaveform == false)
                {
                    for (int i = 0; i < this.transform.childCount; i++)
                    {
                        transform.GetChild(i).GetComponent<GenerateShellTexture>().GenerateTexture(i);
                    }
                    generatedShells = true;
                }
            }
        }
    }

    void Update()
    {
        if (audioSource.clip == null)
        {
            audioSource.clip = LoadSong.instance.audioSource.clip;
        }
        if (length == 0)
        {
            length = LoadSong.instance.audioSource.clip.length;
        }

        if (!applied && startOffset >= 0 && length > startOffset)
        {
            if (Settings.instance.config.visuals.audioVisualizer == Visuals.AudioVisualizer.Waveform)
            {

                if (SpectrogramTextures.instance.waveform[startOffset] == null)
                {
                    Texture2D blackTexture = new Texture2D(1, 1);
                    blackTexture.SetPixel(0, 0, Color.black);
                    blackTexture.Apply();
                    render.material.mainTexture = blackTexture;
                    StartCoroutine(GenerateWaveform());
                }
                else
                {
                    render.material.mainTexture = SpectrogramTextures.instance.waveform[startOffset];
                }
            }
            else
            {
                if (SpectrogramTextures.instance.spectrogram[startOffset] == null)
                {
                    Texture2D blackTexture = new Texture2D(1, 1);
                    blackTexture.SetPixel(0, 0, Color.black);
                    blackTexture.Apply();
                    render.material.mainTexture = blackTexture;
                    StartCoroutine(GenerateSpectrogram());
                }
                else
                {
                    render.material.mainTexture = SpectrogramTextures.instance.spectrogram[startOffset];
                }
            }
            applied = true;
        }

        if (!generatedShells && applied)
        {
            if (SpectrogramTextures.instance.spectrogram[startOffset] == render.material.mainTexture)
            {
                if (generateWaveform == false)
                {
                    for (int i = 0; i < this.transform.childCount; i++)
                    {
                        transform.GetChild(i).GetComponent<GenerateShellTexture>().GenerateTexture(i);
                    }
                    generatedShells = true;
                }
            }
        }
    }

    IEnumerator GenerateWaveform()
    {
        waveformOuterColor = ((Color)Settings.instance.config.visuals.waveform.mainColor);
        waveformColor = ((Color)Settings.instance.config.visuals.waveform.innerColor);
        waveformBackgroundColor = ((Color)Settings.instance.config.visuals.waveform.backgroundColor);

        wave_width = (int)Settings.instance.config.visuals.waveform.resolution;
        wave_height = wave_width / 16;

        Texture2D waveformTexture = new Texture2D(wave_width, wave_height);
        Color[] waveformColors = new Color[wave_width * wave_height];



        // Adjust the start sample calculation to align better with the timing
        float timeOffset = 0.08f; // Adjust this value as needed
        int startSample = (int)((startOffset + timeOffset) * audioSource.clip.frequency);
        int endSample = Mathf.Min(startSample + (int)(duration * audioSource.clip.frequency), audioSource.clip.samples);
        int stepSize = (endSample - startSample) / wave_width;

        float[] samples = new float[stepSize];
        for (int x = 0; x < wave_width; x++)
        {
            audioSource.clip.GetData(samples, startSample + x * stepSize);

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

            // Draw the waveform line as a vertical line between minY and maxY
            for (int y = 0; y < wave_height; y++)
            {
                if (y >= minY && y <= maxY)
                {
                    if (y >= minY + ((maxY - minY) / 3.25f) && y <= maxY - ((maxY - minY) / 3.25f))
                    {
                        if (y >= minY + ((maxY - minY) / 3.25f) + 12 && y <= maxY - ((maxY - minY) / 3.25f) + 12)
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

        SpectrogramTextures.instance.waveform[startOffset] = waveformTexture;
        render.material.mainTexture = waveformTexture;
    }



    IEnumerator GenerateSpectrogram()
    {
        intensity = Settings.instance.config.visuals.spectrogram.intensity / 2f;

        spectro_height = (int)Settings.instance.config.visuals.spectrogram.resolution;
        spectro_width = spectro_height * 4;

        Texture2D spectrogramTexture = new Texture2D(spectro_width, spectro_height);

        int startSample = (int)(startOffset * audioSource.clip.frequency);
        int endSample = Mathf.Min(startSample + (int)(duration * audioSource.clip.frequency), audioSource.clip.samples);
        int windowSize = Mathf.ClosestPowerOfTwo(spectro_width);
        int stepSize = (endSample - startSample) / spectro_width;

        float[] spectrum = new float[windowSize];
        float[] samples = new float[windowSize];

        for (int x = 0; x < spectro_width; x++)
        {
            audioSource.clip.GetData(samples, startSample + x * stepSize);
            FFT(samples, new float[windowSize], false);

            for (int j = 0; j < spectrum.Length / 2; j++)
            {
                spectrum[j] = Mathf.Sqrt(samples[j] * samples[j] + samples[j + windowSize / 2] * samples[j + windowSize / 2]);
            }

            for (int j = 0; j < spectrum.Length; j++)
            {
                spectrum[j] = Mathf.Log(spectrum[j] + 1) * intensity;
            }

            Color[] columnColors = new Color[spectro_height];
            for (int y = 0; y < spectro_height; y++)
            {
                float value = y < spectrum.Length ? spectrum[y] : 0f;
                // Choose color based on gradient
                Color color = gradient.Evaluate(value);
                columnColors[y] = color;
            }

            spectrogramTexture.SetPixels(x, 0, 1, spectro_height, columnColors);

            if (x % 5 == 0)
            {
                yield return null;
            }
        }

        spectrogramTexture.wrapMode = TextureWrapMode.Clamp;
        spectrogramTexture.Apply();
        SpectrogramTextures.instance.spectrogram[startOffset] = spectrogramTexture;
        render.material.mainTexture = spectrogramTexture;
    }
}