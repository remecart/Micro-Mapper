using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using static LoadMap;
using UnityEngine.Rendering;

public class GetMapInfoUI : MonoBehaviour
{
    private static readonly int SpriteTexture = Shader.PropertyToID("_MainTex");
    
    bool applied;

    public Material cover;
    public TextMeshProUGUI songName;
    public TextMeshProUGUI songArtist;
    public TextMeshProUGUI difficulty;

    // Update is called once per frame
    void Update()
    {
        if (ReadMapInfo.instance.info != new Info() && !applied)
        {
            ApplyCover();
            songName.text = ReadMapInfo.instance.info._songName;
            songArtist.text = ReadMapInfo.instance.info._songAuthorName;

            string beatchar = LoadMap.instance.beatchar.ToString();
            string diff = LoadMap.instance.diff.ToString();
            if (diff == "ExpertPlus") diff = "Expert+";

            difficulty.text = diff + "\n" + beatchar;
        }
    }


    // ReSharper disable Unity.PerformanceAnalysis
    void ApplyCover()
    {
        byte[] imageData = File.ReadAllBytes(ReadMapInfo.instance.folderPath + "\\" + ReadMapInfo.instance.info._coverImageFilename);

        // Create a new Texture2D and load the image data
        Texture2D texture = new Texture2D(2, 2); // Adjust the size as needed
        texture.LoadImage(imageData);

        cover.SetTexture(SpriteTexture, texture);
        applied = true;
    }
}
