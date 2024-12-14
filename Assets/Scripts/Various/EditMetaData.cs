using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using System.IO;
using SFB;
using TMPro;

public class EditMetaData : MonoBehaviour
{
    public LoadCover loadCover;
    public Info metaData;
    public static EditMetaData instance;
    public string folderPath;
    public List<TMP_InputField> inputs;
    private string infoFolderPath;

    void Start()
    {
        if (GameObject.FindWithTag("FolderPath"))
            folderPath = GameObject.FindWithTag("FolderPath").GetComponent<FolderPath>().path;
        instance = this;
        string infoPath = "info.dat";

        if (!folderPath.Contains("info.dat"))
        {
            infoPath = "Info.dat";
        }

        string rawData = File.ReadAllText(Path.Combine(folderPath, infoPath));
        metaData = JsonUtility.FromJson<Info>(rawData);

        // File.WriteAllText(folderPath + "\\info_2.dat", JsonUtility.ToJson(metaData, true));

        infoFolderPath = Path.Combine(folderPath, infoPath);

        // put calculate thing here
        LoadValues();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S)) SaveInfo("Map saved successfully!");
    }

    void LoadValues()
    {
        inputs[0].text = metaData._songName;
        inputs[1].text = metaData._songSubName;
        inputs[2].text = metaData._songAuthorName;
        inputs[3].text = metaData._levelAuthorName;
        inputs[4].text = metaData._beatsPerMinute.ToString(CultureInfo.InvariantCulture);
        inputs[5].text = metaData._previewStartTime.ToString(CultureInfo.InvariantCulture);
        inputs[6].text = metaData._previewDuration.ToString(CultureInfo.InvariantCulture);
        inputs[7].text = metaData._songFilename;
        inputs[8].text = metaData._coverImageFilename;
    }

    public void SaveValues()
    {
        metaData._songName = inputs[0].text;
        metaData._songSubName = inputs[1].text;
        metaData._songAuthorName = inputs[2].text;
        metaData._levelAuthorName = inputs[3].text;
        metaData._beatsPerMinute = float.Parse(inputs[4].text);
        metaData._previewStartTime = float.Parse(inputs[5].text);
        metaData._previewDuration = float.Parse(inputs[6].text);
        metaData._songFilename = inputs[7].text;
        metaData._coverImageFilename = inputs[8].text;
    }

    public void SaveInfo(string text)
    {
        SaveValues();
        metaData._customData._editors._lastEditedBy = "Micro Mapper";
        File.WriteAllText(infoFolderPath, JsonUtility.ToJson(metaData, true));

        PopUpText.instance.Generate(text);

        if (metaData._difficultyBeatmapSets.Count > 0)
        {
            for (int x = 0; x < metaData._difficultyBeatmapSets.Count; x++)
            {
                if (metaData._difficultyBeatmapSets[x]._difficultyBeatmaps != null && metaData._difficultyBeatmapSets[x]._difficultyBeatmaps.Count > 0)
                {
                    for (int y = 0; y < metaData._difficultyBeatmapSets[x]._difficultyBeatmaps.Count; y++)
                    {
                        var path = Path.Combine(folderPath,
                            metaData._difficultyBeatmapSets[x]._difficultyBeatmaps[y]._beatmapFilename);
                        if (!File.Exists(path))
                        {
                            beatsV3 beats = new beatsV3
                            {
                                version = "3.3.0",
                                bpmEvents = new List<bpmEvents>(),
                                rotationEvents = new List<rotationEvents>(),
                                colorNotes = new List<colorNotes>(),
                                bombNotes = new List<bombNotes>(),
                                obstacles = new List<obstacles>(),
                                sliders = new List<sliders>(),
                                burstSliders = new List<burstSliders>(),
                                basicBeatmapEvents = new List<basicBeatmapEvents>(),
                                timings = new List<timings>(),
                                customData = new customData(),
                                waypoints = new List<timings>(),
                                colorBoostBeatmapEvents = new List<timings>(),
                                lightColorEventBoxGroups = new List<timings>(),
                                vfxEventBoxGroups = new List<timings>(),
                                _fxEventsCollection = new List<timings>(),
                                basicEventTypesWithKeywords = new List<timings>(),
                            };
                            File.WriteAllText(path, JsonUtility.ToJson(beats, true));
                        }
                    }
                }
            }
        }
    }

    public void PickSong()
    {
        // Define file filters for allowed extensions.
        var extensions = new[]
        {
            new ExtensionFilter("Audio Files", "ogg", "wav", "egg")
        };

        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select a File", folderPath, extensions, false);

        if (paths.Length > 0)
        {
            string path = paths[0];

            if (File.Exists(path))
            {
                var fileName = path.Replace(folderPath + "\\", "");
                metaData._songFilename = fileName;
                inputs[7].text = fileName;
            }
        }
    }

    public void PickCover()
    {
        // Define file filters for allowed extensions.
        var extensions = new[]
        {
            new ExtensionFilter("Image Files", "png", "jpg")
        };

        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select a File", folderPath, extensions, false);

        if (paths.Length > 0)
        {
            string path = paths[0];

            if (File.Exists(path))
            {
                var fileName = path.Replace(folderPath + "\\", "");
                metaData._coverImageFilename = fileName;
                inputs[8].text = fileName;
                loadCover.Cover();
            }
        }
    }
}