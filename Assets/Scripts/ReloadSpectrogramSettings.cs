using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReloadSpectrogramSettings : MonoBehaviour
{
    public static ReloadSpectrogramSettings instance;
    public float contrast;
    public TextMeshProUGUI contrastText;
    public TextMeshProUGUI heightText;
    public Slider height;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    public void ChangeContrast(Slider slider)
    {
        contrast = slider.value;
        Settings.instance.config.visuals.spectrogram.contrast = contrast;

        for (int i = 0; i < SpectrogramTextures.instance.spectrogram.Count; i++)
        {
            SpectrogramTextures.instance.spectrogram[i] = null;
        }
        contrastText.text = (Mathf.RoundToInt(contrast * 100f) / 100f).ToString();
        InstantiateSpectrogram.instance.Reload();

        UpdateHeight(height);
    }

    // Update is called once per frame
    public void UpdateHeight(Slider slider)
    {
        Transform parent = InstantiateSpectrogram.instance.spectroChunkParent;
        int layers = InstantiateSpectrogram.instance.layers;
        float height = slider.value;
        InstantiateSpectrogram.instance.height = height;

        for (int i = 0; i < parent.childCount; i++)
        {
            for (int j = 0; j < parent.GetChild(i).childCount; j++)
            {
                parent.GetChild(i).GetChild(j).localPosition = new Vector3(0, 0, -((j + 1) * height / layers));
            }
        }

        heightText.text = (Mathf.RoundToInt(height * 100f) / 100f).ToString();
    }
}
