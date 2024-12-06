using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class LoadMapSelectionPreview : MonoBehaviour
{
    public Info info;
    public float time;
    public RawImage cover;
    public TextMeshProUGUI songName;
    public TextMeshProUGUI songArtist;
    public TextMeshProUGUI mapper;
    
    public GameObject difficultyPrefab;
    public Transform difficultyParent;
    public string folderPath;
    public int seconds;

    void Start()
    {
        LoadData();
    }

    private void LoadData()
    {
        info ??= LoadInfo(folderPath);

        if (info != null)
        {
            StartCoroutine(ApplyCoverCoroutine());
            songName.text = info._songName;
            songArtist.text = info._songAuthorName;
            mapper.text = info._levelAuthorName;
            InstantiateDifficulties();
        }
    }

    private Info LoadInfo(string path)
    {
        string infoPath = path.Contains("info.dat") ? "info.dat" : "Info.dat";
        if (File.Exists(Path.Combine(path, infoPath))) {
            string rawData = File.ReadAllText(Path.Combine(path, infoPath));
            return JsonUtility.FromJson<Info>(rawData);
        }
        return null;
    }

    void InstantiateDifficulties()
    {
        var prefabWidth = difficultyPrefab.GetComponent<RectTransform>().rect.width;
        var count = 0;

        foreach (var set in info._difficultyBeatmapSets)
        {
            foreach (var beatmap in set._difficultyBeatmaps)
            {
                var newDifficulty = Instantiate(difficultyPrefab, difficultyParent);

                var colorIndex = difficulies.IndexOf(beatmap._difficulty);
                newDifficulty.GetComponent<Image>().color = colors[colorIndex];

                var rectTransform = newDifficulty.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(count * -(prefabWidth + 12), rectTransform.anchoredPosition.y);

                var difficultyText = newDifficulty.GetComponentInChildren<TextMeshProUGUI>();
                difficultyText.text = difficulyNames[colorIndex];

                count++;
            }
        }
    }

    private IEnumerator ApplyCoverCoroutine()
    {
        yield return new WaitForSeconds(time);

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