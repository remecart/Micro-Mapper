using UnityEngine;
using UnityEngine.UI;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public class SpectrogramGenerator : MonoBehaviour
{
    public AudioClip audioClip; // AudioClip to extract audio data from
    public RawImage rawImage; // RawImage to display the spectrogram texture
    public int textureWidth = 1024;
    public int textureHeight = 256;
    public int fftSize = 1024; // Size of FFT (controls spectrogram resolution)
    public float sampleRate = 44100f; // Audio sample rate (matches AudioSource)

    private Texture2D spectrogramTexture;

    void Start()
    {
        spectrogramTexture = new Texture2D(textureWidth, textureHeight);
        rawImage.texture = spectrogramTexture; // Assign texture to RawImage
        StartCoroutine(GenerateSpectrogram());
    }

    System.Collections.IEnumerator GenerateSpectrogram()
    {
        float[] samples = new float[audioClip.samples];
        audioClip.GetData(samples, 0); // Get all the audio data from the AudioClip

        int hopSize = fftSize / 2;
        int numFrames = Mathf.CeilToInt((float)samples.Length / hopSize);

        NativeArray<float> fftData = new NativeArray<float>(fftSize, Allocator.TempJob);
        NativeArray<float> magnitudeData = new NativeArray<float>(fftSize / 2, Allocator.TempJob);

        for (int i = 0; i < numFrames; i++)
        {
            // Get the slice of samples corresponding to the current frame
            int startIndex = i * hopSize;
            int endIndex = Mathf.Min(startIndex + fftSize, samples.Length);

            NativeArray<float> frameSamples = new NativeArray<float>(fftSize, Allocator.TempJob);
            for (int j = 0; j < fftSize; j++)
            {
                if (startIndex + j < samples.Length)
                    frameSamples[j] = samples[startIndex + j];
                else
                    frameSamples[j] = 0f;
            }

            // Create a job to process the FFT
            SpectrogramJob spectrogramJob = new SpectrogramJob
            {
                samples = frameSamples,
                fftData = fftData,
                magnitudeData = magnitudeData,
                fftSize = fftSize,
                sampleRate = sampleRate
            };

            JobHandle jobHandle = spectrogramJob.Schedule(1, 1);
            jobHandle.Complete();

            // Update the spectrogram texture
            for (int bin = 0; bin < magnitudeData.Length; bin++)
            {
                float magnitude = magnitudeData[bin];
                float colorValue = Mathf.Log10(magnitude + 1) * 0.1f; // Apply log scaling
                spectrogramTexture.SetPixel(i, bin, new Color(colorValue, colorValue, colorValue));
            }

            spectrogramTexture.Apply();
            frameSamples.Dispose();
        }

        yield return null; // Wait for next frame if needed
    }

    void OnGUI()
    {
        // The spectrogram is already applied to the RawImage, so no need to draw manually.
    }

    [BurstCompile]
    struct SpectrogramJob : IJobParallelFor
    {
        public NativeArray<float> samples;
        public NativeArray<float> fftData;
        public NativeArray<float> magnitudeData;
        public int fftSize;
        public float sampleRate;

        public void Execute(int index)
        {
            // Apply window function (Hamming window)
            for (int i = 0; i < fftSize; i++)
            {
                fftData[i] = samples[i] * (0.54f - 0.46f * math.cos(2f * math.PI * i / (fftSize - 1)));
            }

            // Perform FFT
            FFT(fftData);

            // Calculate magnitudes
            for (int i = 0; i < fftSize / 2; i++)
            {
                float real = fftData[i];
                float imaginary = fftData[fftSize - i - 1];
                magnitudeData[i] = math.sqrt(real * real + imaginary * imaginary);
            }
        }

        private void FFT(NativeArray<float> data)
        {
            int n = data.Length;
            int bitReversedIndex;

            // Bit-reversal reordering
            for (int i = 0; i < n; i++)
            {
                bitReversedIndex = ReverseBits(i, Mathf.FloorToInt(math.log2(n)));
                if (i < bitReversedIndex)
                {
                    float temp = data[i];
                    data[i] = data[bitReversedIndex];
                    data[bitReversedIndex] = temp;
                }
            }

            // Cooley-Tukey FFT
            for (int step = 2; step <= n; step *= 2)
            {
                float angle = 2f * math.PI / step;
                for (int i = 0; i < n; i += step)
                {
                    for (int j = 0; j < step / 2; j++)
                    {
                        float even = data[i + j];
                        float odd = data[i + j + step / 2];

                        float t = math.cos(j * angle) * odd - math.sin(j * angle) * odd;
                        data[i + j + step / 2] = even - t;
                        data[i + j] = even + t;
                    }
                }
            }
        }

        private int ReverseBits(int n, int numBits)
        {
            int reversed = 0;
            for (int i = 0; i < numBits; i++)
            {
                if ((n & (1 << i)) != 0)
                {
                    reversed |= (1 << (numBits - 1 - i));
                }
            }
            return reversed;
        }
    }
}
