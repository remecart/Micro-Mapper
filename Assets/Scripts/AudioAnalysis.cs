using UnityEngine;

public class AudioAnalysis : MonoBehaviour
{
    // Function to get the volume of an AudioClip at a specific time in seconds
    public float GetVolumeAtTime(AudioClip audioClip, float timeInSeconds)
    {
        if (audioClip == null)
        {
            Debug.LogError("AudioClip is null.");
            return 0f;
        }

        if (timeInSeconds < 0f || timeInSeconds > audioClip.length)
        {
            return 0f;
        }

        // Calculate the sample position at the given time
        int samplePosition = Mathf.FloorToInt(timeInSeconds * audioClip.frequency);

        // Ensure the sample position is within bounds
        if (samplePosition < 0 || samplePosition >= audioClip.samples)
        {
            Debug.LogError("Sample position is out of range.");
            return 0f;
        }

        // Create an array to hold the audio data
        float[] samples = new float[audioClip.channels];

        // Get the sample data at the calculated position
        audioClip.GetData(samples, samplePosition);

        // Calculate the RMS (Root Mean Square) value of the audio samples
        float rmsValue = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            rmsValue += samples[i] * samples[i];
        }
        rmsValue = Mathf.Sqrt(rmsValue / samples.Length);

        return rmsValue;
    }

    // Function to get the highest value in the frequency spectrum of an AudioClip at a specific time in seconds
    public float GetHighestFrequencyValueAtTime(AudioClip audioClip, float timeInSeconds, int sampleSize)
    {
        if (audioClip == null)
        {
            Debug.LogError("AudioClip is null.");
            return 0f;
        }

        if (timeInSeconds < 0f || timeInSeconds > audioClip.length)
        {
            Debug.LogError("Time is out of range.");
            return 0f;
        }

        // Calculate the sample position at the given time
        int samplePosition = Mathf.FloorToInt(timeInSeconds * audioClip.frequency);

        // Ensure the sample position is within bounds
        if (samplePosition < 0 || samplePosition + sampleSize >= audioClip.samples)
        {
            Debug.LogError("Sample position is out of range for the specified sample size.");
            return 0f;
        }

        // Create an array to hold the audio data
        float[] samples = new float[sampleSize * audioClip.channels];

        // Get the sample data starting at the calculated position
        audioClip.GetData(samples, samplePosition);

        // Perform FFT
        float[] spectrum = new float[sampleSize];
        FFT(samples, spectrum, sampleSize);

        // Find the highest value in the spectrum
        float highestValue = 0f;
        for (int i = 0; i < sampleSize; i++)
        {
            if (spectrum[i] > highestValue)
            {
                highestValue = spectrum[i];
            }
        }

        return highestValue;
    }

    // FFT implementation (Cooley-Tukey radix-2 decimation-in-time)
    private void FFT(float[] input, float[] output, int n)
    {
        // Check if n is a power of 2
        if ((n & (n - 1)) != 0)
        {
            Debug.LogError("FFT size must be a power of 2.");
            return;
        }

        // Initialize complex number arrays
        Complex[] data = new Complex[n];
        Complex[] temp = new Complex[n];
        for (int i = 0; i < n; i++)
        {
            data[i] = new Complex(input[i], 0);
            temp[i] = new Complex();
        }

        // Perform FFT
        FFTRecursive(data, temp, 0, 1, n);

        // Compute magnitude spectrum
        for (int i = 0; i < n / 2; i++)
        {
            output[i] = Mathf.Sqrt(data[i].Real * data[i].Real + data[i].Imaginary * data[i].Imaginary);
        }
    }

    // Recursive FFT function
    private void FFTRecursive(Complex[] data, Complex[] temp, int start, int stride, int n)
    {
        if (n == 1)
        {
            temp[0] = data[start];
            return;
        }

        // Perform FFT recursively on even and odd halves
        FFTRecursive(data, temp, start, stride * 2, n / 2);
        FFTRecursive(data, temp, start + stride, stride * 2, n / 2);

        // Combine results
        for (int i = 0; i < n / 2; i++)
        {
            Complex even = temp[i];
            Complex odd = temp[i + n / 2];
            Complex twiddle = Complex.FromPolarCoordinates(1f, -2 * Mathf.PI * i / n);

            data[start + i] = even + twiddle * odd;
            data[start + i + n / 2] = even - twiddle * odd;
        }
    }

    // Helper class for complex number operations
    private struct Complex
    {
        public float Real;
        public float Imaginary;

        public Complex(float real, float imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }

        public static Complex FromPolarCoordinates(float magnitude, float phase)
        {
            return new Complex(magnitude * Mathf.Cos(phase), magnitude * Mathf.Sin(phase));
        }

        public static Complex operator +(Complex a, Complex b)
        {
            return new Complex(a.Real + b.Real, a.Imaginary + b.Imaginary);
        }

        public static Complex operator -(Complex a, Complex b)
        {
            return new Complex(a.Real - b.Real, a.Imaginary - b.Imaginary);
        }

        public static Complex operator *(Complex a, Complex b)
        {
            return new Complex(a.Real * b.Real - a.Imaginary * b.Imaginary,
                               a.Real * b.Imaginary + a.Imaginary * b.Real);
        }
    }
}
