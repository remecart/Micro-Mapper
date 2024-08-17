using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraOptions : MonoBehaviour
{
    public Camera cam;
    public TextMeshProUGUI fov;
    public TextMeshProUGUI intensity;
    public bool bloom;
    public BloomEffect bloomEffect;

    public void Enable(TextMeshProUGUI text)
    {
        if (text.text != "x")
        {
            bloom = true;
            text.text = "x";
        }
        else
        {
            bloom = false;
            text.text = "";
        }

        bloomEffect.enabled = bloom;
        Settings.instance.config.visuals.cameraSettings.bloom = bloom;
    }

    public void UpdateFOV(Slider slider)
    {
        cam.fieldOfView = slider.value;
        fov.text = (Mathf.RoundToInt(slider.value * 100f) / 100f).ToString();
        Settings.instance.config.visuals.cameraSettings.fov = Mathf.RoundToInt(slider.value);
    }

    public void UpdateIntensity(Slider slider)
    {
        bloomEffect.intensity = slider.value;
        intensity.text = (Mathf.RoundToInt(slider.value * 100f) / 100f).ToString();
        Settings.instance.config.visuals.cameraSettings.bloomIntensity = slider.value;
    }
}
