using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using ImGuiNET;
using System.Xml.Schema;
using static Spectrogram;
using static Waveform;
using static Visuals;
using System;
using static Audio;
using UnityEditor;
using Microsoft.Win32;
using System.Diagnostics;
using Skybox = Visuals.Skybox;

public class Settings : MonoBehaviour
{
    public static Settings instance;

    [Header("Load Settings")]
    public Config config;
    private string file;
    public bool isHovering;

    public Keybinds Keybinds;

    //private void CreateExtend()
    //{
    //    string appName = "MyUnityEditor";
    //    string exePath = Process.GetCurrentProcess().MainModule.FileName;
    //    string exeName = AppDomain.CurrentDomain.FriendlyName ?? "UnityEditor";

    //    using (var txtKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\.txt"))
    //    {
    //        txtKey?.SetValue("", appName);
    //    }

    //    using (var appKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{appName}\DefaultIcon"))
    //    {
    //        appKey?.SetValue("", exePath);
    //    }

    //    using (var openCommand = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{appName}\shell\open\command"))
    //    {
    //        openCommand?.SetValue("", exePath + " \"%1\"");
    //    }

    //    using (var editCommand = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{appName}\shell\edit\command"))
    //    {
    //        editCommand?.SetValue("", exePath + " \"%1\"");
    //    }
    //}

    public void ToggleSettings()
    {
        isShowing = !isShowing;
    }

    void Start()
    {
        //CreateExtend();

        instance = this;

        string documents = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        file = Path.Combine(documents, "Micro Mapper", "settings.json");

        if (!File.Exists(file) && Directory.Exists(Path.Combine(documents, "Micro Mapper")))
        {
            string raw = JsonUtility.ToJson(config, true);
            File.WriteAllText(file, raw);
        }
        else if (!Directory.Exists(Path.Combine(documents, "Micro Mapper")))
        {
            Directory.CreateDirectory(Path.Combine(documents, "Micro Mapper"));
            string raw = JsonUtility.ToJson(config, true);
            File.WriteAllText(file, raw);
        }
        else
        {
            config = JsonUtility.FromJson<Config>(File.ReadAllText(file));
        }

        config.keybinds = Keybinds;
        
        QualitySettings.vSyncCount = config.visuals.vsync ? 1 : 0;

    }

    void OnDestroy()
    {
        string raw = JsonUtility.ToJson(config, true);
        File.WriteAllText(file, raw);
    }

    void OnEnable()
    {
        ImGuiUn.Layout += OnLayout;
    }

    void OnDisable()
    {
        ImGuiUn.Layout -= OnLayout;
    }

    private bool isShowing = false;

    void OnLayout()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isShowing = !isShowing;
        }

        if (isShowing && !Menu.instance.open && !Bookmarks.instance.openMenu)
        {
            //ImGui.ShowDemoWindow();
            //ImGui.SetNextWindowPos(new Vector2((Screen.width - 450) / 2, (Screen.height - 650) / 2));
            ImGui.Begin("Settings", ImGuiWindowFlags.NoCollapse);
            ImGui.SetWindowSize(new Vector2(450, 650));

            isHovering = ImGui.IsWindowHovered();

            if (ImGui.BeginTabBar("SettingsTabBar"))
            {
                // Settings here
                Mapping();
                Graphics();
                Audio();
                Controls();
                MBot();
            }

            ImGui.EndTabBar();
            ImGui.End();
        }
        else isHovering = false;
    }

    void Mapping()
    {
        if (ImGui.BeginTabItem("Mapping"))
        {
            if (ImGui.TreeNodeEx("Editor Options", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.SliderInt("Editor Scale", ref config.mapping.editorScale, 4, 40))
                {
                    SpawnObjects.instance.editorScale = config.mapping.editorScale;
                    SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
                    DrawLines.instance.DrawLinesFromScratch(SpawnObjects.instance.currentBeat, SpawnObjects.instance.precision);
                    InstantiateSpectrogram.instance.Reload();
                }
                if (ImGui.SliderInt("Song Speed", ref config.mapping.songSpeed, 1, 20))
                {
                    LoadSong.instance.songSpeed = (float)config.mapping.songSpeed / 10f;
                    LoadSong.instance.audioSource.Stop();
                    if (SpawnObjects.instance.playing) LoadSong.instance.Offset(SpawnObjects.instance.GetRealTimeFromBeat(SpawnObjects.instance.currentBeat));
                }
                if (ImGui.SliderFloat("Note Distance", ref config.mapping.noteDistance, 4, 12))
                {
                    SpawnObjects.instance.spawnInMs = config.mapping.noteDistance * 100;
                    SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
                }
                if (ImGui.Checkbox("Box Select", ref config.mapping.selection))
                {
                    SelectObjects.instance.selection = config.mapping.selection;
                    SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
                }
                
                if (ImGui.Checkbox("Double Tap To Place Notes", ref config.mapping.allowDoubleTapping))
                {
                    Placement.instance.allowDoubleTapping = config.mapping.allowDoubleTapping;
                }

                ImGui.TreePop();
            }
            if (ImGui.TreeNodeEx("Mapping Extensions", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.Checkbox("Enable ME", ref config.mapping.mappingExtensions.enabled))
                {
                    Placement.instance.enableME = config.mapping.mappingExtensions.enabled;
                    Placement.instance.ReloadGrid();
                    SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
                }
                if (ImGui.InputInt("Grid Y-Pos", ref config.mapping.mappingExtensions.gridYPos))
                {
                    config.mapping.mappingExtensions.gridYPos = Mathf.Clamp(config.mapping.mappingExtensions.gridYPos, 0, 10);
                    Placement.instance.gridYPos = config.mapping.mappingExtensions.gridYPos;
                    Placement.instance.ReloadGrid();
                    SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
                }
                if (ImGui.InputInt("Grid width", ref config.mapping.mappingExtensions.gridWidth))
                {
                    config.mapping.mappingExtensions.gridWidth = Mathf.Clamp(config.mapping.mappingExtensions.gridWidth, 1, 100);
                    Placement.instance.MEgridSize.x = config.mapping.mappingExtensions.gridWidth;
                    Placement.instance.ReloadGrid();
                    SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
                }
                if (ImGui.InputInt("Grid height", ref config.mapping.mappingExtensions.gridHeight))
                {
                    config.mapping.mappingExtensions.gridHeight = Mathf.Clamp(config.mapping.mappingExtensions.gridHeight, 1, 100);
                    Placement.instance.MEgridSize.y = config.mapping.mappingExtensions.gridHeight;
                    Placement.instance.ReloadGrid();
                    SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
                }

                if (ImGui.SliderInt("Precision", ref config.mapping.mappingExtensions.precision, 1, 16))
                {
                    Placement.instance.MEprecision = config.mapping.mappingExtensions.precision;
                    Placement.instance.ReloadGrid();
                    SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
                }

                ImGui.TreePop();
            }
            if (ImGui.TreeNodeEx("Colors", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Left Note", ref config.mapping.colorSettings.leftNote);
                ImGui.ColorEdit4("Left Arrow", ref config.mapping.colorSettings.leftNoteArrow);
                ImGui.ColorEdit4("Right Note", ref config.mapping.colorSettings.rightNote);
                ImGui.ColorEdit4("Right Arrow", ref config.mapping.colorSettings.rightNoteArrow);

                ImGui.TreePop();
            }
            ImGui.EndTabItem();
        }
    }

    void Graphics()
    {
        if (ImGui.BeginTabItem("Graphics"))
        {
            if (ImGui.TreeNodeEx("Camera", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.SliderInt("FOV", ref config.visuals.cameraSettings.fov, 65, 110))
                {
                    CameraOptions.instance.fov = config.visuals.cameraSettings.fov;
                }
                if (ImGui.SliderFloat("Bloom", ref config.visuals.cameraSettings.bloom, 0, 5))
                {
                    BloomEffect.instance.intensity = config.visuals.cameraSettings.bloom / 10f;
                }
                
                if(ImGui.Checkbox("VSync", ref config.visuals.vsync))
                {
                    QualitySettings.vSyncCount = config.visuals.vsync ? 1 : 0;
                }  

                string[] _resNames = new string[]
                {
                    "Sky1",
                    "Sky2",
                    "Sky3",
                    "Default"
                };

                int index = config.visuals.skybox switch
                {
                    Skybox.Sky1 => 0,
                    Skybox.Sky2 => 1,
                    Skybox.Sky3 => 2,
                    _ => 3
                };

                HitSoundManager.instance.hitsoundIndex = index;

                if (ImGui.Combo("Pick Skybox", ref index, _resNames, _resNames.Length))
                {
                    switch (_resNames[index])
                    {
                        case "Sky1":
                            config.visuals.skybox = Visuals.Skybox.Sky1;
                            break;
                        case "Sky2":
                            config.visuals.skybox = Visuals.Skybox.Sky2;
                            break;
                        case "Sky3":
                            config.visuals.skybox = Visuals.Skybox.Sky3;
                            break;
                        case "Default":
                            config.visuals.skybox = Visuals.Skybox.Default;
                            break;
                    }

                    SkyboxManager.instance.ReloadSkybox(index);
                }

                ImGui.TreePop();
            }

            string[] _names = new string[]
        {
                "Spectrogram",
                "Waveform"
        };

            int _index = config.visuals.audioVisualizer switch
            {
                AudioVisualizer.Spectrogram => 0,
                _ => 1 // Default to 0 (Low) if the resolution does not match
            };

            if (ImGui.Combo("Audio Visualizer", ref _index, _names, _names.Length))
            {
                switch (_names[_index])
                {
                    case "Spectrogram":
                        config.visuals.audioVisualizer = Visuals.AudioVisualizer.Spectrogram;
                        break;
                    case "Waveform":
                        config.visuals.audioVisualizer = Visuals.AudioVisualizer.Waveform;
                        break;
                }

                ReloadWaveform();
            }

            if (ImGui.TreeNodeEx("Spectrogram", ImGuiTreeNodeFlags.DefaultOpen))
            {

                if (ImGui.SliderFloat("Intensity", ref config.visuals.spectrogram.intensity, 0, 1))
                {
                    ReloadSpectrogram();
                }
                if (ImGui.Checkbox("Depth", ref config.visuals.spectrogram.depth))
                {
                    InstantiateSpectrogram.instance.Reload();
                }
                if (ImGui.SliderInt("Layers", ref config.visuals.spectrogram.layers, 1, 16))
                {
                    InstantiateSpectrogram.instance.Reload();
                }
                if (ImGui.SliderFloat("Height", ref config.visuals.spectrogram.height, 0, 0.125f))
                {
                    InstantiateSpectrogram.instance.Reload();
                }

                string[] _resNames = new string[]
                {
                    "Low",
                    "Medium",
                    "High"
                };

                int index = config.visuals.spectrogram.resolution switch
                {
                    Res.low => 0,
                    Res.medium => 1,
                    Res.high => 2,
                    _ => 0 // Default to 0 (Low) if the resolution does not match
                };

                if (ImGui.Combo("Resolution ", ref index, _resNames, _resNames.Length))
                {
                    switch (_resNames[index])
                    {
                        case "Low":
                            config.visuals.spectrogram.resolution = Spectrogram.Res.low;
                            break;
                        case "Medium":
                            config.visuals.spectrogram.resolution = Spectrogram.Res.medium;
                            break;
                        case "High":
                            config.visuals.spectrogram.resolution = Spectrogram.Res.high;
                            break;
                    }

                    ReloadSpectrogram();
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNodeEx("Waveform", ImGuiTreeNodeFlags.DefaultOpen))
            {
                string[] _resNames = new string[]
                {
                    "Low",
                    "Medium",
                    "High"
                };

                int index = config.visuals.waveform.resolution switch
                {
                    WaveRes.low => 0,
                    WaveRes.medium => 1,
                    WaveRes.high => 2,
                    _ => 0 // Default to 0 (Low) if the resolution does not match
                };

                if (ImGui.Combo("Resolution", ref index, _resNames, _resNames.Length))
                {
                    switch (_resNames[index])
                    {
                        case "Low":
                            config.visuals.waveform.resolution = Waveform.WaveRes.low;
                            break;
                        case "Medium":
                            config.visuals.waveform.resolution = Waveform.WaveRes.medium;
                            break;
                        case "High":
                            config.visuals.waveform.resolution = Waveform.WaveRes.high;
                            break;
                    }

                    ReloadWaveform();
                }
                if (ImGui.ColorEdit4("Main Color", ref config.visuals.waveform.mainColor))
                {
                    ReloadWaveform();
                }
                if (ImGui.ColorEdit4("Inner Color", ref config.visuals.waveform.innerColor))
                {
                    ReloadWaveform();
                }
                if (ImGui.ColorEdit4("Background Color", ref config.visuals.waveform.backgroundColor))
                {
                    ReloadWaveform();

                }

                ImGui.TreePop();
                //ImGuiNative.
            }

            ImGui.EndTabItem();
        }
    }

    void Controls()
    {
        if (ImGui.BeginTabItem("Controls"))
        {
            if (ImGui.TreeNodeEx("Camera Controls", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.SliderFloat("Movement Speed", ref config.controls.camSpeed, 10, 30))
                {
                    PlayerFly.instance.speed = config.controls.camSpeed;
                }
                if (ImGui.SliderFloat("Rotation Speed", ref config.controls.camRot, 0.33f, 3.5f))
                {
                    PlayerFly.instance.rotationSpeed = config.controls.camRot;
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNodeEx("Invert Controls", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.Checkbox("Invert Angle", ref config.controls.invertNoteAngle))
                {
                    Placement.instance.invertControls = config.controls.invertNoteAngle;
                }
                if (ImGui.Checkbox("Invert Precision Scroll", ref config.controls.invertPrecisionScroll))
                {
                    SpawnObjects.instance.invertPrecisionScroll = config.controls.invertPrecisionScroll;
                }
                if (ImGui.Checkbox("Invert Wall Scroll", ref config.controls.invertWallScroll))
                {
                    Placement.instance.invertWallScroll = config.controls.invertWallScroll;
                }
                if (ImGui.Checkbox("Invert Timeline Scroll", ref config.controls.invertTimelineScroll))
                {
                    SpawnObjects.instance.invertTimelineScroll = config.controls.invertTimelineScroll;
                }

                ImGui.TreePop();
            }
            ImGui.EndTabItem();
        }
    }
    void MBot()
    {
        if (ImGui.BeginTabItem("mBot"))
        {
            if (ImGui.TreeNodeEx("Swing Settings", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.SliderFloat("Intensity", ref config.mBot.mBotSettings.intensity, 0.25f, 3))
                {
                    mBot.instance.intensity = config.mBot.mBotSettings.intensity;
                }
                if (ImGui.SliderFloat("Plane Offset", ref config.mBot.mBotSettings.planeOffset, -1, 1))
                {
                    mBot.instance.planeOffset = config.mBot.mBotSettings.planeOffset;
                }
                if (ImGui.SliderFloat("Swing Overshoot", ref config.mBot.mBotSettings.overshoot, 0, 5))
                {
                    mBot.instance.overshoot = config.mBot.mBotSettings.overshoot;
                }
                if (ImGui.SliderFloat("Position Multiplier", ref config.mBot.mBotSettings.positionMultiplier, 1, 3))
                {
                    mBot.instance.positionMultiplier = config.mBot.mBotSettings.positionMultiplier;
                }
                if (ImGui.SliderFloat("Z Offset", ref config.mBot.mBotSettings.zOffset, -1, 1))
                {
                    mBot.instance.zOffset = config.mBot.mBotSettings.zOffset;
                }
                if (ImGui.Checkbox("Visualize Swings (Debug)", ref config.mBot.mBotSettings.visualizeSwings))
                {
                    mBot.instance.visualizeSwings = config.mBot.mBotSettings.visualizeSwings;
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNodeEx("Sabers", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.SliderFloat("Saber Width", ref config.mBot.mBotSaber.saberWidth, 1, 10))
                {
                    mBot.instance.saberWidth = config.mBot.mBotSaber.saberWidth;
                }
                if (ImGui.SliderFloat("Saber Length", ref config.mBot.mBotSaber.saberLength, 7, 13))
                {
                    mBot.instance.saberLength = config.mBot.mBotSaber.saberLength;
                }
                if (ImGui.SliderFloat("Trail Width", ref config.mBot.mBotSaber.trailWidth, 1, 10))
                {
                    mBot.instance.trailWidth = config.mBot.mBotSaber.trailWidth;
                }
                if (ImGui.SliderFloat("Trail Length", ref config.mBot.mBotSaber.trailLength, 1, 25))
                {
                    mBot.instance.trailLength = config.mBot.mBotSaber.trailLength;
                }

                ImGui.TreePop();
            }
            ImGui.EndTabItem();
        }
    }

    void Audio()
    {
        if (ImGui.BeginTabItem("Audio"))
        {
            if (ImGui.TreeNodeEx("Volume", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.SliderFloat("Music", ref config.audio.music, 0, 1))
                {
                    LoadSong.instance.audioSource.volume = config.audio.music;
                }
                if (ImGui.SliderFloat("Hitsound", ref config.audio.hitsound, 0, 1))
                {
                    HitSoundManager.instance.audioSource.volume = config.audio.hitsound;
                }
                if (ImGui.SliderFloat("Timing", ref config.audio.timing, 0, 1))
                {
                    TimingNoteManager.instance.audioSource.volume = config.audio.timing;
                }
                if (ImGui.SliderFloat("Metronome", ref config.audio.metronome, 0, 1))
                {
                    Metronome.instance.audioSource.volume = config.audio.metronome;
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNodeEx("Hitsounds", ImGuiTreeNodeFlags.DefaultOpen))
            {
                string[] _resNames = new string[]
                {
                    "MicroMapper",
                    "Rabbit",
                    "osu!",
                    "Custom"
                };

                int index = config.audio.hitsounds switch
                {
                    Hitsound.MicroMapper => 0,
                    Hitsound.Rabbit => 1,
                    Hitsound.osu => 2,
                    _ => 3
                };

                HitSoundManager.instance.hitsoundIndex = index;

                if (ImGui.Combo("Pick Hitsound", ref index, _resNames, _resNames.Length))
                {
                    switch (_resNames[index])
                    {
                        case "MicroMapper":
                            config.audio.hitsounds = Hitsound.MicroMapper;
                            break;
                        case "Rabbit":
                            config.audio.hitsounds = Hitsound.Rabbit;
                            break;
                        case "osu!":
                            config.audio.hitsounds = Hitsound.osu;
                            break;
                        case "Custom":
                            config.audio.hitsounds = Hitsound.Custom;
                            break;
                    }

                    ReloadWaveform();
                }

                if (ImGui.Button("Import custom hitsound"))
                {
                    HitSoundManager.instance.OpenFilePicker();
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNodeEx("Timings", ImGuiTreeNodeFlags.DefaultOpen))
            {
                string[] _resNames = new string[]
                {
                    "MicroMapper",
                    "Rabbit",
                    "osu!",
                    "Custom"
                };

                int index = config.audio.timings switch
                {
                    Hitsound.MicroMapper => 0,
                    Hitsound.Rabbit => 1,
                    Hitsound.osu => 2,
                    _ => 3
                };

                TimingNoteManager.instance.hitsoundIndex = index;

                if (ImGui.Combo("Pick Timing", ref index, _resNames, _resNames.Length))
                {
                    switch (_resNames[index]) // Corrected strings here
                    {
                        case "MicroMapper":
                            config.audio.timings = Hitsound.MicroMapper;
                            break;
                        case "Rabbit":
                            config.audio.timings = Hitsound.Rabbit;
                            break;
                        case "osu!":
                            config.audio.timings = Hitsound.osu;
                            break;
                        case "Custom":
                            config.audio.timings = Hitsound.Custom;
                            break;
                    }
                }

                if (ImGui.Button("Import custom timing"))
                {
                    TimingNoteManager.instance.OpenFilePicker();
                }

                ImGui.TreePop();
            }

            ImGui.EndTabItem();
        }
    }

    private void ReloadSpectrogram()
    {
        for (int i = 0; i < SpectrogramTextures.instance.spectrogram.Count; i++)
        {
            SpectrogramTextures.instance.spectrogram[i] = null;
        }
        InstantiateSpectrogram.instance.Reload();
    }

    private void ReloadWaveform()
    {
        for (int i = 0; i < SpectrogramTextures.instance.waveform.Count; i++)
        {
            SpectrogramTextures.instance.waveform[i] = null;
        }
        InstantiateSpectrogram.instance.Reload();
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
    public Controls controls;
    public MBot mBot;
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
    public int editorScale;
    public int songSpeed;
    public float noteDistance;
    public bool selection;
    public bool allowDoubleTapping;
    public ColorSettings colorSettings;
    public MappingExtensions mappingExtensions;
}

[System.Serializable]
public class MappingExtensions
{
    public bool enabled;
    public bool allowPlacementOutsideOfGrid;
    public int gridYPos;
    public int gridWidth;
    public int gridHeight;
    public int precision;
}

[System.Serializable]
public class Visuals
{
    public enum Skybox
    {
        Sky1,
        Sky2,
        Sky3,
        Default
    }

    public enum AudioVisualizer
    {
        Spectrogram,
        Waveform
    }

    public Skybox skybox;
    public AudioVisualizer audioVisualizer;
    public Spectrogram spectrogram;
    public Waveform waveform;
    public CameraSettings cameraSettings;
    public bool vsync;

}

[System.Serializable]
public class MBot
{
    public MBotSettings mBotSettings;
    public MBotSaber mBotSaber;
}

[System.Serializable]
public class MBotSettings
{
    public float intensity;
    public float planeOffset;
    public float overshoot;
    public float positionMultiplier;
    public float zOffset;
    public bool visualizeSwings;
}


[System.Serializable]
public class MBotSaber
{
    public float saberWidth;
    public float saberLength;
    public float trailWidth;
    public float trailLength;
}

[System.Serializable]
public class Audio
{
    public float music;
    public float hitsound;
    public float timing;
    public float metronome;
    public Hitsound hitsounds;
    public Hitsound timings;

    public enum Hitsound
    {
        MicroMapper = 1,
        Rabbit = 2,
        osu = 3,
        Custom = 4
    }

    public string customSoundPath;
    public string customTimingSoundPath;
}

[System.Serializable]
public class Controls
{
    public float camSpeed;
    public float camRot;
    public bool invertNoteAngle;
    public bool invertWallScroll;
    public bool invertTimelineScroll;
    public bool invertPrecisionScroll;
}

[System.Serializable]
public class Keybinds
{
    // Access the keys: KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.-)
    //                  KeybindManager.instance.AreAllKeysHeld(Settings.instance.config.keybinds.-)
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
    public List<KeyCode> allowFusedNotePlacement;
    public List<KeyCode> enableMBot;
}

[System.Serializable]
public class ColorSettings
{
    public Vector4 leftNote;
    public Vector4 leftNoteArrow;
    public Vector4 rightNote;
    public Vector4 rightNoteArrow;
}


[System.Serializable]
public class Spectrogram
{
    // public Res resolutioWidth;
    public float intensity;
    public bool depth;
    public float height;
    public int layers;
    public Res resolution;

    public enum Res
    {
        low = 64,
        medium = 128,
        high = 256
    }
}

[System.Serializable]
public class Waveform
{
    public WaveRes resolution;

    public enum WaveRes
    {
        low = 2048,
        medium = 4098,
        high = 8196
    }

    public Vector4 mainColor;
    public Vector4 innerColor;
    public Vector4 backgroundColor;
}

[System.Serializable]
public class CameraSettings
{
    public int fov;
    public float bloom;
}

