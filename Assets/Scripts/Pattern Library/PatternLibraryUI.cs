using System;
using System.Linq;
using ImGuiNET;
using UnityEngine;

public class PatternLibraryUI : MonoBehaviour
{
    public static PatternLibraryUI instance;
    public bool showPatternLibrary = false;
    public bool addingPattern;

    private void OnEnable()
    {
        ImGuiUn.Layout += CreatePatternLibraryUI;
    }

    private void OnDisable()
    {
        ImGuiUn.Layout -= CreatePatternLibraryUI;
    }

    public void CreatePatternLibraryUI()
    {
        if (ImGui.BeginTabItem("Pattern Library"))
        {
            ImGui.ListBox("Patterns", ref Library.instance.selectedPatternIndex, Library.instance.GetPatternNames(),
                Library.instance.GetPatternCount(), 5);

            if (ImGui.Button("Place Pattern"))
            {
                PlacePattern();
            }

            if (ImGui.Button("Add Pattern"))
            {
                ImGui.OpenPopup("Pattern Name");
            }

            if (ImGui.BeginPopup("Pattern Name"))
            {
                addingPattern = true;
                
                ImGui.Text("Enter Pattern Name");
                ImGui.InputText("Pattern Save Name", ref Library.instance.patternName, 100);
                if (ImGui.Button("Add"))
                {
                    if (Library.instance.patternName == "")
                    {
                        Library.instance.patternName = "Unnamed Pattern";
                    }

                    AddPatternToList(Library.instance.patternName);
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
            else addingPattern = false;

            if (ImGui.Button("Remove Pattern"))
            {
                Library.instance.RemoveFromLibrary(Library.instance.selectedPatternIndex);
            }

            ImGui.EndTabItem();
        }
    }

    public void Start()
    {
        instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            showPatternLibrary = !showPatternLibrary;
        }
    }

    private void AddPatternToList(string patternName)
    {
        var lowestBeat = SelectObjects.instance.selectedColorNotes.Min(x => x.b);

        var notes = SelectObjects.instance.selectedColorNotes.Select(colorNotes => colorNotes.Copy())
            .Select(note =>
            {
                note.b -= lowestBeat;
                return note;
            });
        var bombs = SelectObjects.instance.selectedBombNotes.Select(colorNotes => colorNotes.Copy())
            .Select(note =>
            {
                note.b -= lowestBeat;
                return note;
            });
        var obstacles = SelectObjects.instance.selectedObstacles.Select(colorNotes => colorNotes.Copy())
            .Select(note =>
            {
                note.b -= lowestBeat;
                return note;
            });
        var sliders = SelectObjects.instance.selectedSliders.Select(colorNotes => colorNotes.Copy())
            .Select(note =>
            {
                note.b -= lowestBeat;
                return note;
            });
        var burstSliders = SelectObjects.instance.selectedBurstSliders.Select(colorNotes => colorNotes.Copy())
            .Select(
                note =>
                {
                    note.b -= lowestBeat;
                    return note;
                });

        Library.instance.AddToLibrary(new Pattern(patternName, notes.ToList(), bombs.ToList(), obstacles.ToList(),
            sliders.ToList(), burstSliders.ToList()));
    }

    private void PlacePattern()
    {
        var pattern = Library.instance.GetPattern(Library.instance.selectedPatternIndex);
        var currentBeat = SpawnObjects.instance.currentBeat;

        UndoRedoManager.instance.SaveState(LoadMap.instance.beats[40484], 40484, false);

        var notes = pattern._patternNotes.Select(note => note.Copy())
            .Select(note =>
            {
                note.b += currentBeat;
                return note;
            });

        var bombs = pattern._patternBombs.Select(bomb => bomb.Copy())
            .Select(bomb =>
            {
                bomb.b += currentBeat;
                return bomb;
            });

        var obstacles = pattern._patternObstacles.Select(obstacle => obstacle.Copy())
            .Select(obstacle =>
            {
                obstacle.b += currentBeat;
                return obstacle;
            });

        var sliders = pattern._patternSliders.Select(slider => slider.Copy())
            .Select(slider =>
            {
                slider.b += currentBeat;
                return slider;
            });

        var burstSliders = pattern._patternBurstSliders.Select(burstSlider => burstSlider.Copy())
            .Select(burstSlider =>
            {
                burstSlider.b += currentBeat;
                return burstSlider;
            });

        foreach (var note in notes)
        {
            UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(note.b)],
                Mathf.FloorToInt(note.b), true);
            LoadMap.instance.beats[Mathf.FloorToInt(note.b)].colorNotes.Add(note);
        }

        foreach (var bomb in bombs)
        {
            UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(bomb.b)],
                Mathf.FloorToInt(bomb.b), true);
            LoadMap.instance.beats[Mathf.FloorToInt(bomb.b)].bombNotes.Add(bomb);
        }

        foreach (var obstacle in obstacles)
        {
            UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(obstacle.b)],
                Mathf.FloorToInt(obstacle.b), true);
            LoadMap.instance.beats[Mathf.FloorToInt(obstacle.b)].obstacles.Add(obstacle);
        }

        foreach (var slider in sliders)
        {
            UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(slider.b)],
                Mathf.FloorToInt(slider.b), true);
            LoadMap.instance.beats[Mathf.FloorToInt(slider.b)].sliders.Add(slider);
        }

        foreach (var burstSlider in burstSliders)
        {
            UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(burstSlider.b)],
                Mathf.FloorToInt(burstSlider.b), true);
            LoadMap.instance.beats[Mathf.FloorToInt(burstSlider.b)].burstSliders.Add(burstSlider);
        }

        SpawnObjects.instance.LoadObjectsFromScratch(currentBeat, true, true);
        DrawLines.instance.DrawLinesWhenRequired();
    }
}