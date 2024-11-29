using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.Networking;
using System;

public class LoadMapSelectionPreview : MonoBehaviour
{
    private static readonly int SpriteTexture = Shader.PropertyToID("_MainTex");

    bool applied;
    public Info info = null;

    public RawImage cover;
    public TextMeshProUGUI songName;
    public TextMeshProUGUI songArtist;
    public TextMeshProUGUI mapper;
    
    public GameObject difficultyPrefab;
    public Transform difficultyParent;
    public string folderPath;
    public int seconds = 0;

    void Update()
    {
        if (!applied)
        {
            LoadInfo();
            if (info != null)
            {
                ApplyCover();
            }
            songName.text = info._songName;
            songArtist.text = info._songAuthorName;
            mapper.text = info._levelAuthorName;
            InstantiateDifficulties();
            applied = true;
        }
    }

    void LoadInfo()
    {
        string infoPath = folderPath.Contains("info.dat") ? "info.dat" : "Info.dat";
        if (File.Exists(Path.Combine(folderPath, infoPath))) {
            string rawData = File.ReadAllText(Path.Combine(folderPath, infoPath));
            info = JsonUtility.FromJson<Info>(rawData);
        }
    }

    void InstantiateDifficulties()
    {
        float prefabWidth = difficultyPrefab.GetComponent<RectTransform>().rect.width;
        int count = 0;

        foreach (var set in info._difficultyBeatmapSets)
        {
            foreach (var beatmap in set._difficultyBeatmaps)
            {
                GameObject newDifficulty = Instantiate(difficultyPrefab, difficultyParent);

                int colorIndex = difficulies.IndexOf(beatmap._difficulty);
                newDifficulty.GetComponent<Image>().color = colors[colorIndex];

                RectTransform rectTransform = newDifficulty.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(count * -(prefabWidth + 12), rectTransform.anchoredPosition.y);

                TextMeshProUGUI difficultyText = newDifficulty.GetComponentInChildren<TextMeshProUGUI>();
                difficultyText.text = difficulyNames[colorIndex];

                count++;
            }
        }
    }

    void ApplyCover()
    {
        if (File.Exists(folderPath + "\\" + info._coverImageFilename))
        {
            byte[] imageData = File.ReadAllBytes(folderPath + "\\" + info._coverImageFilename);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);
            cover.texture = texture;
        }
    }

    public List<Color> colors;
    public List<string> difficulies;
    public List<string> difficulyNames;
}