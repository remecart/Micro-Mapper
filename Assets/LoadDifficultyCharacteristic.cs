using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LoadDifficultyCharacteristic : MonoBehaviour
{
    public EditMetaData editMetaData;
    public List<GameObject> difficulties;
    public List<string> difficultyNames;
    
    public List<Texture2D> addDeleteTextures;
    public List<TMP_InputField> input;
    
    private string cacheCharacteristicName = "Standard";
    private string cacheDifficultyName;

    public List<GameObject> requirementButtons;

    void Start()
    {
        ChangeCharacteristics("Standard", true);
    }
    
    void LoadDifficultyData()
    {
        foreach (var t in editMetaData.metaData._difficultyBeatmapSets)
        {
            if (t._beatmapCharacteristicName == cacheCharacteristicName)
            {
                foreach (var d in t._difficultyBeatmaps)
                {
                    if (d._difficulty == cacheDifficultyName)
                    {
                        input[0].text = d._customData._difficultyLabel;
                        input[1].text = d._noteJumpMovementSpeed.ToString();
                        input[2].text = d._noteJumpStartBeatOffset.ToString();
                    }
                }
            }
        }
    }

    public void DeleteOrAddDifficulty(string name)
    {
        foreach (var t in editMetaData.metaData._difficultyBeatmapSets)
        {
            if (t._beatmapCharacteristicName == cacheCharacteristicName)
            {
                foreach (var d in t._difficultyBeatmaps.ToList())
                {
                    if (d._difficulty == name)
                    {
                        t._difficultyBeatmaps.Remove(d);
                        cacheDifficultyName = t._difficultyBeatmaps[0]._difficulty;
                        ChangeCharacteristics(cacheCharacteristicName);
                        return;
                    }
                }
                t._difficultyBeatmaps.Add(new _difficultyBeatmaps
                {
                    _difficulty = name,
                    _difficultyRank = 7,
                    _beatmapFilename = name + cacheCharacteristicName + ".dat",
                    _noteJumpStartBeatOffset = 0,
                    _noteJumpMovementSpeed = 20,
                    _customData = new _difficultyBeatmapsCustomData()
                });
                
                cacheDifficultyName = name;
                ChangeCharacteristics(cacheCharacteristicName);
            }
        }
    }

    public void ChangeDifficulties(string name)
    {
        foreach (var t in editMetaData.metaData._difficultyBeatmapSets)
        {
            if (t._beatmapCharacteristicName == cacheCharacteristicName)
            {
                foreach (var d in t._difficultyBeatmaps)
                {
                    if (d._difficulty == name)
                    {
                        cacheDifficultyName = name;
                        LoadDifficultyData();
                    }
                }
            }
        }
    }

    
    public void ChangeCharacteristics(string name, bool forceFirstDifficultyAsCache = false)
    {
        foreach (var t in editMetaData.metaData._difficultyBeatmapSets)
        {
            if (t._beatmapCharacteristicName == name)
            {
                cacheCharacteristicName = t._beatmapCharacteristicName;
                LoadBeatmapCharacteristic(t);

                // Automatically set the first difficulty if available
                if (t._difficultyBeatmaps.Count > 0 && forceFirstDifficultyAsCache)
                {
                    cacheDifficultyName = t._difficultyBeatmaps[0]._difficulty;
                    LoadDifficultyData();
                }

                return;
            }
        }

        // Disable all difficulties if no match found
        foreach (var diff in difficulties)
        {
            EnableDisableDifficulty(diff, 0);
        }
    }

    void LoadBeatmapCharacteristic(_difficultyBeatmapSets characteristic)
    {
        foreach (var diff in difficulties)
        {
            EnableDisableDifficulty(diff, 0);
        }

        var applied = false;
        foreach (var diff in characteristic._difficultyBeatmaps)
        {
            int index = difficultyNames.IndexOf(diff._difficulty);
            EnableDisableDifficulty(difficulties[index], 1);
        }
    }

    private void EnableDisableDifficulty(GameObject diff, int index)
    {
        diff.GetComponent<Button>().interactable = index != 0;
        diff.transform.GetChild(1).GetComponent<RawImage>().texture = addDeleteTextures[index];
    }
}
