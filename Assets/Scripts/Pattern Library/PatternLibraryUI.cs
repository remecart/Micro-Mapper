using System;
using System.Linq;
using ImGuiNET;
using UnityEngine;

public class PatternLibraryUI : MonoBehaviour
{
    public bool showPatternLibrary = false;

    private void OnEnable()
    {
        ImGuiUn.Layout += CreatePatternLibraryUI;
    }

    private void OnDisable()
    {
        ImGuiUn.Layout -= CreatePatternLibraryUI;
    }

    private void CreatePatternLibraryUI()
    {
        if (!showPatternLibrary) return;


        ImGui.Begin("Pattern Library", ImGuiWindowFlags.NoCollapse);
        ImGui.SetWindowSize(new Vector2(450, 650));

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

        if (ImGui.Button("Remove Pattern"))
        {
            Library.instance.RemoveFromLibrary(Library.instance.selectedPatternIndex);
        }

        ImGui.End();
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

        var notes = pattern._patternNotes.Select(note =>
        {
            note.b += currentBeat;
            return note;
        });
        var bombs = pattern._patternBombs.Select(note =>
        {
            note.b += currentBeat;
            return note;
        });
        var obstacles = pattern._patternObstacles.Select(note =>
        {
            note.b += currentBeat;
            return note;
        });
        var sliders = pattern._patternSliders.Select(note =>
        {
            note.b += currentBeat;
            return note;
        });
        var burstSliders = pattern._patternBurstSliders.Select(note =>
        {
            note.b += currentBeat;
            return note;
        });

        foreach (var note in notes)
        {
            LoadMap.instance.beats[Mathf.FloorToInt(note.b)].colorNotes.Add(note);
        }

        foreach (var bomb in bombs)
        {
            LoadMap.instance.beats[Mathf.FloorToInt(bomb.b)].bombNotes.Add(bomb);
        }

        foreach (var obstacle in obstacles)
        {
            LoadMap.instance.beats[Mathf.FloorToInt(obstacle.b)].obstacles.Add(obstacle);
        }

        foreach (var slider in sliders)
        {
            LoadMap.instance.beats[Mathf.FloorToInt(slider.b)].sliders.Add(slider);
        }

        foreach (var burstSlider in burstSliders)
        {
            LoadMap.instance.beats[Mathf.FloorToInt(burstSlider.b)].burstSliders.Add(burstSlider);
        }

        SpawnObjects.instance.LoadObjectsFromScratch(currentBeat, true, true);
        DrawLines.instance.DrawLinesFromScratch(currentBeat, SpawnObjects.instance.precision);
    }
}