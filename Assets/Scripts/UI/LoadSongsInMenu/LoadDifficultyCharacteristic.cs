using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoadDifficultyCharacteristic : MonoBehaviour, IPointerClickHandler
{
    public EditMetaData editMetaData;
    public List<GameObject> difficulties;
    public List<string> difficultyNames;
    
    public List<Texture2D> addDeleteTextures;
    public List<TMP_InputField> input;
    
    public string cacheCharacteristicName = "Standard";
    public string cacheDifficultyName;

    public List<GameObject> requirementButtons;

    void Start()
    {
        if (editMetaData.metaData._difficultyBeatmapSets.Count == 0)
        {
            var difficultyBeatmapSets = new _difficultyBeatmapSets();
            editMetaData.metaData._difficultyBeatmapSets.Add(difficultyBeatmapSets);
        }
        ChangeCharacteristics("Standard", true);
    }

    public void OnPointerClick(PointerEventData eventData)  {
        
    }
    
    public void SaveDifficultyData()
    {
        foreach (var t in editMetaData.metaData._difficultyBeatmapSets)
        {
            if (t._beatmapCharacteristicName == cacheCharacteristicName)
            {
                foreach (var d in t._difficultyBeatmaps)
                {
                    if (d._difficulty == cacheDifficultyName)
                    {
                        d._customData._difficultyLabel = input[0].text;
                        d._noteJumpMovementSpeed = float.Parse(input[1].text);
                        d._noteJumpStartBeatOffset = float.Parse(input[2].text);
                    }
                }
            }
        }
    }

    public void LoadDifficultyData()
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
                        input[1].text = d._noteJumpMovementSpeed.ToString(CultureInfo.InvariantCulture);
                        input[2].text = d._noteJumpStartBeatOffset.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }
        }
    }

    public void DeleteOrAddDifficulty(string diffName)
    {
        foreach (var diff in difficulties)
        {
            diff.transform.GetChild(0).gameObject.SetActive(false);
        }
        foreach (var t in editMetaData.metaData._difficultyBeatmapSets)
        {
            if (t._beatmapCharacteristicName == cacheCharacteristicName)
            {
                if (t._difficultyBeatmaps == null)
                {
                    t._difficultyBeatmaps = new List<_difficultyBeatmaps>();
                }

                // Check if the difficulty exists and remove it
                foreach (var d in t._difficultyBeatmaps.ToList())
                {
                    if (d._difficulty == diffName)
                    {
                        t._difficultyBeatmaps.Remove(d);
                        cacheDifficultyName = d._difficulty;
                        ChangeCharacteristics(cacheCharacteristicName); 
                        return;
                    }
                }

                // If it doesn't exist, add the difficulty
                t._difficultyBeatmaps.Add(new _difficultyBeatmaps
                {
                    _difficulty = diffName,
                    _difficultyRank = 7,
                    _beatmapFilename = diffName + cacheCharacteristicName + ".dat",
                    _noteJumpStartBeatOffset = 0,
                    _noteJumpMovementSpeed = 20,
                    _customData = new _difficultyBeatmapsCustomData()
                });
            
                cacheDifficultyName = diffName;
                ChangeCharacteristics(cacheCharacteristicName);
            }
        }
    }


    public void ChangeDifficulties(string diffName)
    {
        foreach (var t in editMetaData.metaData._difficultyBeatmapSets)
        {
            if (t._beatmapCharacteristicName == cacheCharacteristicName)
            {
                foreach (var d in t._difficultyBeatmaps)
                {
                    if (d._difficulty == diffName)
                    {
                        cacheDifficultyName = diffName;
                        LoadDifficultyData();
                        LoadBeatmapCharacteristic(t);
                    }
                }
            }
        }
    }

    public void ChangeCharacter(string charName)
    {
        ChangeCharacteristics(charName, true);
    }
    
    void ChangeCharacteristics(string charName, bool forceFirstDifficultyAsCache = false)
    {
        // Ensure difficulties list is valid
        if (difficulties == null || difficulties.Count == 0)
        {
            Debug.LogError("Difficulties list is not initialized or empty.");
            return;
        }

        // Hide all difficulties initially
        foreach (var diff in difficulties)
        {
            if (diff != null && diff.transform.childCount > 0)
            {
                diff.transform.GetChild(0).gameObject.SetActive(false);
            }
        }

        foreach (var t in editMetaData.metaData._difficultyBeatmapSets)
        {
            if (t != null && t._beatmapCharacteristicName == charName)
            {
                cacheCharacteristicName = t._beatmapCharacteristicName;

                // Automatically set the first difficulty if available
                if (t._difficultyBeatmaps != null && t._difficultyBeatmaps.Count > 0 && forceFirstDifficultyAsCache)
                {
                    foreach (var difficultyName in difficultyNames)
                    {
                        var matchingDifficulty = t._difficultyBeatmaps.FirstOrDefault(d => d._difficulty == difficultyName);
                        if (matchingDifficulty != null)
                        {
                            cacheDifficultyName = matchingDifficulty._difficulty;
                            LoadDifficultyData();
                            LoadBeatmapCharacteristic(t);
                            return;
                        }
                    }
                }

                LoadBeatmapCharacteristic(t);
                return;
            }
        }

        // If no match found, disable all difficulties
        foreach (var diff in difficulties)
        {
            EnableDisableDifficulty(diff, 0);
        }
    }

    public void LoadBeatmapCharacteristic(_difficultyBeatmapSets characteristic)
    {
        foreach (var diff in difficulties)
        {
            EnableDisableDifficulty(diff, 0);
        }
        
        if (characteristic != null)
        {
            if (characteristic?._difficultyBeatmaps != null)
            {
                foreach (var index in characteristic._difficultyBeatmaps
                             .Where(diff => diff != null)
                             .Select(diff => difficultyNames.IndexOf(diff._difficulty)))
                {
                    if (index >= 0) // Ensure the index exists in the list
                    {
                        EnableDisableDifficulty(difficulties[index], 1);
                    }
                }
            }
        }
    }

    private void EnableDisableDifficulty(GameObject diff, int index)
    {
        diff.GetComponent<Button>().interactable = index != 0;
        diff.transform.GetChild(2).GetComponent<RawImage>().texture = addDeleteTextures[index];
        diff.transform.GetChild(0).gameObject.SetActive(diff.transform.GetChild(1).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text == "cacheDifficultyName.Replace(\"ExpertPlus\", \"Expert+\")");
    }
}
