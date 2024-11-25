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

    [HideInInspector]
    public int selectedPatternIndex;
    
    [HideInInspector]
    public string patternName;


    private readonly string LIBRARY_PATH =
        Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "Micro Mapper",
            "PatternLibrary.json");

    private PatternLibrary _patternLibrary;

    void Start()
    {
        instance = this;
        _patternLibrary = new PatternLibrary(LIBRARY_PATH);
    }

    public string[] GetPatternNames()
    {
        string[] patternNames = new string[_patternLibrary._patterns.Count];
        for (int i = 0; i < _patternLibrary._patterns.Count; i++)
        {
            patternNames[i] = _patternLibrary._patterns[i]._name;
        }

        return patternNames;
    }

    public Pattern GetPattern(int index)
    {
        return _patternLibrary._patterns[index];
    }

    public int GetPatternCount()
    {
        return _patternLibrary._patterns.Count;
    }

    public void AddToLibrary(Pattern pattern)
    {
        _patternLibrary._patterns.Add(pattern);
        _patternLibrary.WritePatterns();
    }

    public void RemoveFromLibrary(int index)
    {
        _patternLibrary._patterns.RemoveAt(index);
        _patternLibrary.WritePatterns();
    }
}

public class PatternLibrary
{
    private readonly string _path;

    public PatternLibrary(string path)
    {
        _path = path;

        if (!File.Exists(path))
        {
            WritePatterns();
        }
        _patterns = ReadPatterns(path)._patterns;
        Debug.Log(_patterns.Count);
    }

    private PatternLibrary ReadPatterns(string path)
    {
        string jsonContent = ReadJsonFile(path);
        return ParseJson<PatternLibrary>(jsonContent);
    }

    private string ReadJsonFile(string path)
    {
        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }
        else
        {
            Debug.LogError("File not found: " + path);
            return string.Empty;
        }
    }

    private T ParseJson<T>(string jsonContent)
    {
        return JsonUtility.FromJson<T>(jsonContent);
    }

    public void WritePatterns()
    {
        string jsonContent = JsonUtility.ToJson(this);
        File.WriteAllText(_path, jsonContent);
    }

    public List<Pattern> _patterns;
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

