using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
public class Settings : MonoBehaviour
{
    public static Settings instance;

    [Header("Load Settings")]
    public Config config;

    private Config temp;

    [Header("UI Settings")]
    public List<TextMeshProUGUI> text;

    public List<TMP_InputField> textInputs;
    public List<Slider> slider;
    public List<Button> buttons;

    [Header("Other Settings")]
    public string file;
    private bool cache;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        config = JsonUtility.FromJson<Config>(File.ReadAllText(file));
        temp = JsonUtility.FromJson<Config>(File.ReadAllText(file));

        LoadValues();
    }

    // Update is called once per frame
    void Update()
    {
        if (!cache && config != new Config())
        {
            LoadValues();
            cache = true;
            Debug.Log("nnnn");
        }
    }

    public void OnDestroy()
    {
        string raw = JsonUtility.ToJson(config, config.general.saveFormattedJson);
        File.WriteAllText(file, raw);
    }

    void LoadValues()
    {
        // idk why unity has the most skill issue slider bug ever but idk hardcoded fix 
        // float editorScale = temp.mapping.editorScale;
        // SpawnObjects.instance.editorScale = editorScale;
        // text[9].text = Mathf.RoundToInt(editorScale).ToString();
        // slider[7].value = editorScale;

        // idk why unity has the most skill issue slider bug ever but idk hardcoded fix 
        float height = temp.visuals.spectrogram.height;
        InstantiateSpectrogram.instance.height = height;
        text[2].text = (Mathf.RoundToInt(height * 100f) / 100f).ToString();
        slider[1].value = height;

        // idk why unity has the most skill issue slider bug ever but idk hardcoded fix 
        float contrast = temp.visuals.spectrogram.contrast;
        ReloadSpectrogramSettings.instance.contrast = contrast;
        text[0].text = (Mathf.RoundToInt(contrast * 100f) / 100f).ToString();
        slider[0].value = contrast;

        bool enable3d = temp.visuals.spectrogram.enable3d;
        if (enable3d) text[1].text = "x";
        else text[1].text = "";

        float fov = temp.visuals.cameraSettings.fov;
        text[5].text = (Mathf.RoundToInt(fov * 100f) / 100f).ToString();
        slider[3].value = fov;

        bool bloom = temp.visuals.cameraSettings.bloom;
        if (bloom)
        {
            text[3].text = "x";
            GameObject.FindWithTag("Player").transform.GetChild(0).GetComponent<BloomEffect>().enabled = true;
        }
        else
        {
            text[3].text = "";
            GameObject.FindWithTag("Player").transform.GetChild(0).GetComponent<BloomEffect>().enabled = false;
        }

        float intensity = temp.visuals.cameraSettings.bloomIntensity;
        text[4].text = (Mathf.RoundToInt(intensity * 100f) / 100f).ToString();
        slider[2].value = intensity;


        // Audio
        float volume = temp.audio.music;
        GameObject.FindWithTag("Map").GetComponent<AudioSource>().volume = volume;
        text[6].text = (Mathf.RoundToInt(volume * 100f) / 100f).ToString();
        slider[4].value = volume;

        float hitsound = temp.audio.hitsound;
        // HitSoundManager.instance. = hitsound;
        text[7].text = (Mathf.RoundToInt(hitsound * 100f) / 100f).ToString();
        slider[5].value = hitsound;

        float metronome = temp.audio.metronome;
        text[8].text = (Mathf.RoundToInt(metronome * 100f) / 100f).ToString();
        slider[6].value = metronome;

        textInputs[0].text = Colors.instance.UnityColorToHex(temp.mapping.colorSettings.leftNote);
        Colors.instance.rawImage[0].color = temp.mapping.colorSettings.leftNote;
        textInputs[1].text = Colors.instance.UnityColorToHex(temp.mapping.colorSettings.leftNoteArrow);
        Colors.instance.rawImage[1].color = temp.mapping.colorSettings.leftNoteArrow;
        textInputs[2].text = Colors.instance.UnityColorToHex(temp.mapping.colorSettings.rightNote);
        Colors.instance.rawImage[2].color = temp.mapping.colorSettings.rightNote;
        textInputs[3].text = Colors.instance.UnityColorToHex(temp.mapping.colorSettings.rightNoteArrow);
        Colors.instance.rawImage[3].color = temp.mapping.colorSettings.rightNoteArrow;

        Colors.instance.rawImage[4].color = temp.mapping.colorSettings.leftNote;
        Colors.instance.rawImage[5].color = temp.mapping.colorSettings.rightNote;

        config = temp;
    }
}


[System.Serializable]
public class Config
{
    public General general;
    public Mapping mapping;
    public Visuals visuals;
    public Audio audio;
    public Keybinds keybinds;
}

[System.Serializable]
public class General
{
    public string beatSaberPath;
    public bool autoSave;
    public bool saveFormattedJson;

}

[System.Serializable]
public class Mapping
{
    public float editorScale;
    public float songSpeed;
    public ColorSettings colorSettings;
}

[System.Serializable]
public class Visuals
{
    public Spectrogram spectrogram;
    public CameraSettings cameraSettings;
}

[System.Serializable]
public class Audio
{
    public float music;
    public float hitsound;
    public float metronome;
    // public Hitsound hitsounds;

    public enum Hitsound
    {
        Rabbit = 1,
        Osu = 2,
        Minecraft = 3,
        Remec = 4
    }
}

[System.Serializable]
public class Keybinds
{
    // Access the keys: KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.-)
    public List<KeyCode> previewMap;
    public List<KeyCode> playMap;
    public List<KeyCode> stepForward;
    public List<KeyCode> stepBackward;
    public List<KeyCode> quickSelectRedNote;
    public List<KeyCode> quickSelectBlueNote;
    public List<KeyCode> quickSelectBomb;
    public List<KeyCode> quickSelectWall;
    public List<KeyCode> quickSelectDelete;
    public List<KeyCode> changeNoteType;
    public List<KeyCode> changePrecisionMode;
}

////////// 
/// 

[System.Serializable]
public class ColorSettings
{
    public Color leftNote;
    public Color leftNoteArrow;
    public Color rightNote;
    public Color rightNoteArrow;
}


[System.Serializable]
public class Spectrogram
{
    // public Res resolutioWidth;
    public float contrast;
    public bool enable3d;
    public float height;

    // public enum Res
    // {
    //     low = 64,
    //     medium = 96,
    //     high = 128,
    //     ultra = 256
    // }
}

[System.Serializable]
public class CameraSettings
{
    public int fov;
    public bool bloom;
    public float bloomIntensity;
}

