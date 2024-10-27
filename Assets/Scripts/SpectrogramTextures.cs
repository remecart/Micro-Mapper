using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectrogramTextures : MonoBehaviour
{
    public static SpectrogramTextures instance;

    public List<Texture> spectrogram;
    public List<spectrogramTextures> spectrogram3d;


    public List<Texture> waveform;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }
}


[System.Serializable]
public class spectrogramTextures
{
    public List<Texture> spectrogram3d;
}