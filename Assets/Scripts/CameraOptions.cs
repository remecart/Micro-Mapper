using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CameraOptions : MonoBehaviour
{
    public static CameraOptions instance;
    public Camera cam;
    public int fov;

    public void Start()
    {
        instance = this;
        fov = Settings.instance.config.visuals.cameraSettings.fov;
        cam.fieldOfView = fov;
    }

    public void Update()
    {
        if (fov != cam.fieldOfView)
        {
            fov = Settings.instance.config.visuals.cameraSettings.fov;
            cam.fieldOfView = Settings.instance.config.visuals.cameraSettings.fov;
        }
    }
}
