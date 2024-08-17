using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator instance;
    public AudioSource audioSource;
    public int sampleSize = 1024;  // Size of the sample to analyze
    public float analysisTime = 5f;  // Time in seconds to start analysis
    public LineRenderer lineRenderer;
    public float zThreshold = 0.5f;  // Define your threshold for the z-axis here

    private List<float[]> previousSpectra = new List<float[]>();
    private int bufferSize = 10;  // Number of previous spectra to retain for peak detection
    private List<float> dataPeaks = new List<float>();

    void Start()
    {
        instance = this;
    }

    public void AnalyzeClip(float a)
    {
        analysisTime = a;
        int offset = (int)(analysisTime * audioSource.clip.frequency);
        float[] samples = new float[sampleSize];

        audioSource.clip.GetData(samples, offset);

        float volume = AnalyzeVolume(samples);
        float frequency = AnalyzeFrequency(samples);
        float zeroCrossingRate = AnalyzeZeroCrossingRate(samples);
        float spectralCentroid = AnalyzeSpectralCentroid(samples) / 2;  // Normalized for better combination

        float av = Mathf.Sqrt(Mathf.Sqrt(AnalyzeVolume(samples))) * 8;

        dataPeaks.Add(av);


        if (dataPeaks.Count > 16)
        {
            if (dataPeaks[dataPeaks.Count - 1] >= dataPeaks[dataPeaks.Count - 2] + dataPeaks[dataPeaks.Count - 3] / 3)
            {
                colorNotes note = new colorNotes();
                note.b = SpawnObjects.instance.BeatFromRealTime(a);
                note.x = 0;
                note.y = 0;
                note.c = 0;
                note.d = 8;
                note.a = 0;
                LoadMap.instance.beats[Mathf.FloorToInt(SpawnObjects.instance.BeatFromRealTime(a))].colorNotes.Add(note);
            }
        }

        Vector3 pos = new Vector3(av, 0, SpawnObjects.instance.PositionFromBeat(SpawnObjects.instance.currentBeat + 10) * SpawnObjects.instance.editorScale);

        // Update previous spectra buffer
        UpdatePreviousSpectra(samples);

        lineRenderer.positionCount++;
        if (lineRenderer.positionCount > 1)
        {
            // Check the z-axis of the oldest position
            if (lineRenderer.GetPosition(0).z < zThreshold)
            {
                // Remove the oldest position (index 0)
                for (int i = 0; i < lineRenderer.positionCount - 1; i++)
                {
                    lineRenderer.SetPosition(i, lineRenderer.GetPosition(i + 1));
                }
                lineRenderer.positionCount--; // Reduce the position count by 1
            }
        }
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, pos);
    }

    public float AnalyzeVolume(float[] samples)
    {
        float sum = 0f;

        for (int i = 0; i < samples.Length; i++)
        {
            sum += samples[i] * samples[i];
        }

        float rmsValue = Mathf.Sqrt(sum / samples.Length);
        float volume = rmsValue * 100;
        return volume;
    }

    public float AnalyzeFrequency(float[] samples)
    {
        float[] spectrum = new float[sampleSize];

        // Use Unity's GetSpectrumData for frequency analysis
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Triangle);

        // Find the peak frequency within the spectrum
        float maxAmplitude = 0f;
        int maxIndex = 0;

        for (int i = 0; i < spectrum.Length; i++)
        {
            if (spectrum[i] > maxAmplitude)
            {
                maxAmplitude = spectrum[i];
                maxIndex = i;
            }
        }

        // Calculate frequency in Hz
        float freq = maxIndex * AudioSettings.outputSampleRate / sampleSize;
        return freq;
    }

    public float AnalyzeZeroCrossingRate(float[] samples)
    {
        int zeroCrossings = 0;
        for (int i = 1; i < samples.Length; i++)
        {
            if ((samples[i - 1] > 0 && samples[i] <= 0) || (samples[i - 1] < 0 && samples[i] >= 0))
            {
                zeroCrossings++;
            }
        }

        float zcr = (float)zeroCrossings / samples.Length;
        return zcr;
    }

    public float AnalyzeSpectralCentroid(float[] samples)
    {
        float[] spectrum = new float[sampleSize];

        // Use Unity's GetSpectrumData for spectral centroid estimation
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

        float weightedSum = 0f;
        float totalSum = 0f;

        for (int i = 0; i < spectrum.Length; i++)
        {
            weightedSum += i * spectrum[i];
            totalSum += spectrum[i];
        }

        float spectralCentroid = weightedSum / totalSum;
        return spectralCentroid;
    }

    void FFT(float[] data, float[] spectrum)
    {
        // Placeholder FFT function (replace with actual implementation)
        for (int i = 0; i < data.Length; i++)
        {
            spectrum[i] = Mathf.Abs(data[i]);
        }
    }

    private void UpdatePreviousSpectra(float[] currentSpectrum)
    {
        previousSpectra.Add(currentSpectrum);

        // Maintain buffer size
        if (previousSpectra.Count > bufferSize)
        {
            previousSpectra.RemoveAt(0);
        }
    }
}
