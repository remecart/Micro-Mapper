using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#region Type Definitions

using PatternNote = colorNotes;
using PatternBomb = bombNotes;
using PatternObstacle = obstacles;
using PatternSlider = sliders;
using PatternBurstSlider = burstSliders;

#endregion

public class Library : MonoBehaviour
{
    public static Library instance;

    [HideInInspector] public int selectedPatternIndex;
    [HideInInspector] public string patternName;

    private readonly string LIBRARY_DIRECTORY =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Micro Mapper", "Pattern Library");

    void Start()
    {
        instance = this;

        if (!Directory.Exists(LIBRARY_DIRECTORY))
        {
            Directory.CreateDirectory(LIBRARY_DIRECTORY);
        }
    }

    public string[] GetPatternNames()
    {
        string[] patternFiles = Directory.GetFiles(LIBRARY_DIRECTORY, "*.json");
        string[] patternNames = new string[patternFiles.Length];

        for (int i = 0; i < patternFiles.Length; i++)
        {
            patternNames[i] = Path.GetFileNameWithoutExtension(patternFiles[i]);
        }

        return patternNames;
    }

    public Pattern GetPattern(int index)
    {
        string[] patternFiles = Directory.GetFiles(LIBRARY_DIRECTORY, "*.json");
        if (index < 0 || index >= patternFiles.Length)
        {
            Debug.LogError("Pattern index out of range.");
            return null;
        }

        string patternPath = patternFiles[index];
        string jsonContent = File.ReadAllText(patternPath);
        return JsonUtility.FromJson<Pattern>(jsonContent);
    }

    public int GetPatternCount()
    {
        return Directory.GetFiles(LIBRARY_DIRECTORY, "*.json").Length;
    }

    public void AddToLibrary(Pattern pattern)
    {
        string patternPath = Path.Combine(LIBRARY_DIRECTORY, $"{pattern._name}.json");
        string jsonContent = JsonUtility.ToJson(pattern, true);
        File.WriteAllText(patternPath, jsonContent);
    }

    public void RemoveFromLibrary(int index)
    {
        string[] patternFiles = Directory.GetFiles(LIBRARY_DIRECTORY, "*.json");
        if (index < 0 || index >= patternFiles.Length)
        {
            Debug.LogError("Pattern index out of range.");
            return;
        }

        File.Delete(patternFiles[index]);
    }
}

[Serializable]
public class Pattern
{
    public Pattern(string name, List<PatternNote> patternNotes, List<PatternBomb> patternBombs,
        List<PatternObstacle> patternObstacles, List<PatternSlider> patternSliders,
        List<PatternBurstSlider> patternBurstSliders)
    {
        _name = name;
        _patternNotes = patternNotes;
        _patternBombs = patternBombs;
        _patternObstacles = patternObstacles;
        _patternSliders = patternSliders;
        _patternBurstSliders = patternBurstSliders;
    }

    public string _name;
    public List<PatternNote> _patternNotes;
    public List<PatternBomb> _patternBombs;
    public List<PatternObstacle> _patternObstacles;
    public List<PatternSlider> _patternSliders;
    public List<PatternBurstSlider> _patternBurstSliders;
}
