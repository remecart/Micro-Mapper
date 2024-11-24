using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Skybox = Visuals.Skybox;

public class SkyboxManager : MonoBehaviour
{
    public static SkyboxManager instance;
    public List<Cubemap> cubemaps;
    public Material material;
    public Camera cam;
    public Color color;
    void Start()
    {
        instance = this;
    }


    private bool initialized;

    public void Update()
    {
        if (!initialized)
        {
            string[] _resNames = new string[]
{
            "Sky1",
            "Sky2",
            "Sky3",
            "Default"
};

            int index = Settings.instance.config.visuals.skybox switch
            {
                Skybox.Sky1 => 0,
                Skybox.Sky2 => 1,
                Skybox.Sky3 => 2,
                _ => 3
            };

            ReloadSkybox(index);

            initialized = true;
        }
    }

    public void ReloadSkybox(int a)
    {
        if (a == 3)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = color; // Set your desired solid color here
            cam.cullingMask = LayerMask.GetMask("Default"); // Adjust layers as needed
            RenderSettings.skybox = null; // Clear the skybox
        }
        else
        {
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.cullingMask = -1; // Default culling mask
            material.SetTexture("_Tex", cubemaps[a]);
            RenderSettings.skybox = material; // Apply the material to the skybox
        }
        Debug.Log("Skybox index: " + a);
    }
}
