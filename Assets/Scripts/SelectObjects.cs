using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SelectObjects : MonoBehaviour
{
    public static SelectObjects instance;
    public bool selection;

    public List<colorNotes> selectedColorNotes = new List<colorNotes>();
    public List<bombNotes> selectedBombNotes = new List<bombNotes>();
    public List<bpmEvents> selectedBpmEvents = new List<bpmEvents>();
    public List<obstacles> selectedObstacles = new List<obstacles>();
    public List<sliders> selectedSliders = new List<sliders>();
    public List<burstSliders> selectedBurstSliders = new List<burstSliders>();
    public List<timings> selectedTimings = new List<timings>();

    public List<Material> wallMaterials;

    public GameObject content;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        selection = Settings.instance.config.mapping.selection;
    }


    bool clickCheck = false;
    bool click = false;
    Vector3 startPos;
    Vector3 endPos;

    // Update is called once per frame
    void Update()
    {
        GenerateText();
        HighlightSelectedObject();

        if (!Settings.instance.isHovering && !DrawInEditor.instance.drawing && !Menu.instance.open && !Bookmarks.instance.openMenu)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetMouseButtonDown(0)) FindSelection();
                if (Input.GetKeyDown(KeyCode.UpArrow)) OffsetBeat(SpawnObjects.instance.precision);
                if (Input.GetKeyDown(KeyCode.DownArrow)) OffsetBeat(-SpawnObjects.instance.precision);
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.A)) ClearSelection();
                if (Input.GetKeyDown(KeyCode.C)) CopySelection();
                if (Input.GetKeyDown(KeyCode.V)) PasteSelection();
                Vector2 vec = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                if (vec != Vector2.zero)
                {
                    if (ArrowVector() != new Vector2Int()) OffsetPosition(Placement.instance.MEprecision, ArrowVector());
                }
            }

            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace)) DeleteObjects();

            if (Input.GetKeyDown(KeyCode.M)) MirrorSelection();

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0) && !isSelecting) StartCoroutine(Select());

            if (!isSelecting || !hovering)
            {
                preview.transform.localScale = new Vector3(0, 0, 0);
                preview.transform.localPosition = new Vector3(0, 100000, 0);
            }
            else
            {
                if (Input.GetMouseButtonUp(0)) clickCheck = true;
                if (Input.GetMouseButtonDown(0) && clickCheck == true) click = true;

                if (selection)
                {
                    int positiveX = 1;
                    if (endPos.x - startPos.x < 0) positiveX = -1;

                    int positiveY = 1;
                    if (endPos.y - startPos.y < 0) positiveY = -1;

                    preview.transform.position = new Vector3(endPos.x - (endPos.x - startPos.x) / 2, startPos.y + (endPos.y - startPos.y) / 2, -(SpawnObjects.instance.PositionFromBeat(SpawnObjects.instance.currentBeat) - SpawnObjects.instance.PositionFromBeat(startCurrentBeat)) * SpawnObjects.instance.editorScale / 2);
                    preview.transform.localScale = new Vector3(endPos.x - startPos.x + positiveX, endPos.y - startPos.y + positiveY, (SpawnObjects.instance.PositionFromBeat(SpawnObjects.instance.currentBeat) - SpawnObjects.instance.PositionFromBeat(startCurrentBeat)) * SpawnObjects.instance.editorScale);
                }
                else
                {
                    preview.transform.position = new Vector3(2, 1.5f, -(SpawnObjects.instance.PositionFromBeat(SpawnObjects.instance.currentBeat) - SpawnObjects.instance.PositionFromBeat(startCurrentBeat)) * SpawnObjects.instance.editorScale / 2);
                    preview.transform.localScale = new Vector3(4, 3, (SpawnObjects.instance.PositionFromBeat(SpawnObjects.instance.currentBeat) - SpawnObjects.instance.PositionFromBeat(startCurrentBeat)) * SpawnObjects.instance.editorScale);
                }
            }
        }
    }

    public beatsV3 copiedObjects = new beatsV3();
    public List<TextMeshProUGUI> strings;
    public GameObject panel;

    void GenerateText()
    {
        if (selectedColorNotes.Count != 0 || selectedBombNotes.Count != 0 || selectedObstacles.Count != 0 || selectedSliders.Count != 0 || selectedBurstSliders.Count != 0 || selectedBpmEvents.Count != 0 || selectedTimings.Count != 0)
        {
            panel.SetActive(true);
            int total = selectedColorNotes.Count + selectedBombNotes.Count + selectedObstacles.Count + selectedSliders.Count + selectedBurstSliders.Count + selectedBpmEvents.Count + selectedTimings.Count;
            strings[0].text = "Selected : " + total;

            int red = 0;
            int blue = 0;

            foreach (var item in selectedColorNotes)
            {
                if (item.c == 0) red++;
                else if (item.c == 1) blue++;
            }

            // Dynamically create the string based on counts
            strings[1].text =
                (red > 0 || blue > 0 ? "Red / Blue : " + red + " / " + blue + "\n" : "") +
                (selectedBombNotes.Count > 0 ? "Bombs : " + selectedBombNotes.Count + "\n" : "") +
                (selectedObstacles.Count > 0 ? "Obstacles : " + selectedObstacles.Count + "\n" : "") +
                (selectedTimings.Count > 0 ? "Timings : " + selectedTimings.Count + "\n" : "") +
                (selectedBpmEvents.Count > 0 ? "BpmChanges : " + selectedBpmEvents.Count : "").TrimEnd();
        }
        else
        {
            panel.SetActive(false);
            strings[0].text = strings[1].text = string.Empty;
        }
    }

    public void CopySelection()
    {
        // Find the earliest beat
        float lowestBeat = Mathf.Infinity;

        foreach (var item in selectedColorNotes)
        {
            if (lowestBeat > item.b) lowestBeat = item.b;
        }
        foreach (var item in selectedBombNotes)
        {
            if (lowestBeat > item.b) lowestBeat = item.b;
        }
        foreach (var item in selectedObstacles)
        {
            if (lowestBeat > item.b) lowestBeat = item.b;
        }
        foreach (var item in selectedTimings)
        {
            if (lowestBeat > item.b) lowestBeat = item.b;
        }
        foreach (var item in selectedBpmEvents)
        {
            if (lowestBeat > item.b) lowestBeat = item.b;
        }

        // Initialize copiedObjects
        copiedObjects = new beatsV3();

        // Manually copy each item and adjust `b`
        foreach (var item in selectedColorNotes)
        {
            colorNotes adjustedItem = new colorNotes
            {
                b = item.b - lowestBeat,
                x = item.x,
                y = item.y,
                a = item.a,
                c = item.c,
                d = item.d
            };
            copiedObjects.colorNotes.Add(adjustedItem);
        }

        foreach (var item in selectedBombNotes)
        {
            bombNotes adjustedItem = new bombNotes
            {
                b = item.b - lowestBeat,
                x = item.x,
                y = item.y
            };
            copiedObjects.bombNotes.Add(adjustedItem);
        }

        foreach (var item in selectedTimings)
        {
            timings adjustedItem = new timings
            {
                b = item.b - lowestBeat,
                t = item.t
            };
            copiedObjects.timings.Add(adjustedItem);
        }

        foreach (var item in selectedBpmEvents)
        {
            bpmEvents adjustedItem = new bpmEvents
            {
                b = item.b - lowestBeat,
                m = item.m
            };
            copiedObjects.bpmEvents.Add(adjustedItem);
        }

        foreach (var item in selectedObstacles)
        {
            obstacles adjustedItem = new obstacles
            {
                b = item.b - lowestBeat,
                x = item.x,
                y = item.y,
                d = item.d,
                h = item.h,
                w = item.w
            };
            copiedObjects.obstacles.Add(adjustedItem);
        }

        SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
    }

    public void PasteSelection()
    {
        if (selectedColorNotes.Count != 0 || selectedBombNotes.Count != 0 || selectedObstacles.Count != 0 || selectedSliders.Count != 0 || selectedBurstSliders.Count != 0 || selectedBpmEvents.Count != 0 || selectedTimings.Count != 0)
        {
            UndoRedoManager.instance.SaveState(LoadMap.instance.beats[40484], 40484, false);
            ClearSelection();

            // Manually copy each item from copiedObjects, adjust `b`, and add to selection
            foreach (var item in copiedObjects.colorNotes)
            {
                colorNotes adjustedItem = new colorNotes
                {
                    b = item.b + SpawnObjects.instance.currentBeat,
                    x = item.x,
                    y = item.y,
                    a = item.a,
                    c = item.c,
                    d = item.d
                };
                UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(adjustedItem.b)], Mathf.FloorToInt(adjustedItem.b), true);
                LoadMap.instance.beats[Mathf.FloorToInt(adjustedItem.b)].colorNotes.Add(adjustedItem);
                selectedColorNotes.Add(adjustedItem);  // Automatically add to selection
            }

            foreach (var item in copiedObjects.bombNotes)
            {
                bombNotes adjustedItem = new bombNotes
                {
                    b = item.b + SpawnObjects.instance.currentBeat,
                    x = item.x,
                    y = item.y
                };
                UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(adjustedItem.b)], Mathf.FloorToInt(adjustedItem.b), true);
                LoadMap.instance.beats[Mathf.FloorToInt(adjustedItem.b)].bombNotes.Add(adjustedItem);
                selectedBombNotes.Add(adjustedItem);  // Automatically add to selection
            }

            foreach (var item in copiedObjects.timings)
            {
                timings adjustedItem = new timings
                {
                    b = item.b + SpawnObjects.instance.currentBeat,
                    t = item.t
                };
                UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(adjustedItem.b)], Mathf.FloorToInt(adjustedItem.b), true);
                LoadMap.instance.beats[Mathf.FloorToInt(adjustedItem.b)].timings.Add(adjustedItem);
                selectedTimings.Add(adjustedItem);  // Automatically add to selection
            }

            foreach (var item in copiedObjects.bpmEvents)
            {
                bpmEvents adjustedItem = new bpmEvents
                {
                    b = item.b + SpawnObjects.instance.currentBeat,
                    m = item.m
                };
                UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(adjustedItem.b)], Mathf.FloorToInt(adjustedItem.b), true);
                LoadMap.instance.beats[Mathf.FloorToInt(adjustedItem.b)].bpmEvents.Add(adjustedItem);
                LoadMap.instance.bpmEvents.Add(adjustedItem);
                LoadMap.instance.bpmEvents.Sort((a, b) => a.b.CompareTo(b.b));
                selectedBpmEvents.Add(adjustedItem);  // Automatically add to selection
            }

            foreach (var item in copiedObjects.obstacles)
            {
                obstacles adjustedItem = new obstacles
                {
                    b = item.b + SpawnObjects.instance.currentBeat,
                    x = item.x,
                    y = item.y,
                    d = item.d,
                    h = item.h,
                    w = item.w
                };
                UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(adjustedItem.b)], Mathf.FloorToInt(adjustedItem.b), true);
                LoadMap.instance.beats[Mathf.FloorToInt(adjustedItem.b)].obstacles.Add(adjustedItem);
                selectedObstacles.Add(adjustedItem);  // Automatically add to selection
            }

            // Refresh to ensure the pasted items are visible and selected
            SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
            DrawLines.instance.DrawLinesFromScratch(SpawnObjects.instance.currentBeat, SpawnObjects.instance.precision);
            HighlightSelectedObject();
        }
    }



    public void ClearSelection()
    {
        selectedColorNotes = new List<colorNotes>();
        selectedBombNotes = new List<bombNotes>();
        selectedBpmEvents = new List<bpmEvents>();
        selectedObstacles = new List<obstacles>();
        selectedTimings = new List<timings>();

        HighlightSelectedObject();
    }

    bool hovering = false;
    public bool isSelecting = false;
    public GameObject preview;
    float startCurrentBeat;

    List<colorNotes> selectableColorNotes = new List<colorNotes>();
    List<bombNotes> selectableBombNotes = new List<bombNotes>();
    List<bpmEvents> selectableBpmEvents = new List<bpmEvents>();
    List<obstacles> selectableObstacles = new List<obstacles>();
    List<sliders> selectableSliders = new List<sliders>();
    List<burstSliders> selectableBurstSliders = new List<burstSliders>();
    List<timings> selectableTimings = new List<timings>();

    IEnumerator Select()
    {
        isSelecting = true;
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        RaycastHit[] hits = Physics.RaycastAll(ray);
        RaycastHit hit = new RaycastHit();
        bool gridFound = false;

        foreach (var h in hits)
        {
            if (h.collider.CompareTag("Grid"))
            {
                hit = h;
                gridFound = true;
                break;
            }
        }

        float beat = SpawnObjects.instance.currentBeat;

        if (gridFound)
        {
            startPos = new Vector3(Mathf.RoundToInt(hit.point.x - 0.5f) + 0.5f, Mathf.RoundToInt(hit.point.y - 0.5f) + 0.5f, SpawnObjects.instance.PositionFromBeat(SpawnObjects.instance.currentBeat) * SpawnObjects.instance.editorScale);
            startCurrentBeat = SpawnObjects.instance.currentBeat;
            click = false;

            while (!Input.GetMouseButton(0))
            {
                yield return null;
            }

            while (!click)
            {
                selectableColorNotes = new List<colorNotes>();
                selectableBombNotes = new List<bombNotes>();
                selectableBpmEvents = new List<bpmEvents>();
                selectableObstacles = new List<obstacles>();
                selectableSliders = new List<sliders>();
                selectableBurstSliders = new List<burstSliders>();


                mousePosition = Input.mousePosition;
                ray = Camera.main.ScreenPointToRay(mousePosition);

                RaycastHit[] hits2 = Physics.RaycastAll(ray);
                RaycastHit hit2 = new RaycastHit();
                bool gridFound2 = false;

                foreach (var h in hits2)
                {
                    if (h.collider.CompareTag("Grid"))
                    {
                        hit2 = h;
                        gridFound2 = true;
                    }
                }

                hovering = gridFound2;

                if (gridFound2 == true)
                {
                    endPos = new Vector3(Mathf.RoundToInt(hit2.point.x - 0.5f) + 0.5f, Mathf.RoundToInt(hit2.point.y - 0.5f) + 0.5f, SpawnObjects.instance.PositionFromBeat(SpawnObjects.instance.currentBeat) * SpawnObjects.instance.editorScale);

                    int selectedBeats = Mathf.CeilToInt(SpawnObjects.instance.currentBeat) - Mathf.FloorToInt(startCurrentBeat) + 2;

                    for (int k = 0; k < selectedBeats; k++)
                    {
                        int currentBeatIndex = Mathf.FloorToInt(startCurrentBeat) + k;

                        if (currentBeatIndex < 1 || currentBeatIndex > LoadMap.instance.beats.Count)
                        {
                            continue;
                        }

                        List<colorNotes> colorNotes = LoadMap.instance.beats[currentBeatIndex - 1].colorNotes;
                        foreach (var item in colorNotes)
                        {
                            if (item.b >= startCurrentBeat && item.b <= SpawnObjects.instance.currentBeat)
                            {
                                if (!selectableColorNotes.Contains(item))
                                {
                                    if (selection)
                                    {
                                        if (PointInsideOfSelection(new Vector2(SpawnObjects.instance.ConvertMEPos(item.x), SpawnObjects.instance.ConvertMEPos(item.y)), startPos, endPos)) selectableColorNotes.Add(item);
                                    }
                                    else selectableColorNotes.Add(item);
                                }
                            }
                        }

                        List<bombNotes> bombNotes = LoadMap.instance.beats[currentBeatIndex - 1].bombNotes;
                        foreach (var item in bombNotes)
                        {
                            if (item.b >= startCurrentBeat && item.b <= SpawnObjects.instance.currentBeat)
                            {
                                if (!selectableBombNotes.Contains(item))
                                {
                                    if (selection)
                                    {
                                        if (PointInsideOfSelection(new Vector2(SpawnObjects.instance.ConvertMEPos(item.x), SpawnObjects.instance.ConvertMEPos(item.y)), startPos, endPos)) selectableBombNotes.Add(item);
                                    }
                                    else selectableBombNotes.Add(item);
                                }
                            }
                        }

                        List<obstacles> obstacles = LoadMap.instance.beats[currentBeatIndex - 1].obstacles;
                        foreach (var item in obstacles)
                        {
                            if (item.b >= startCurrentBeat && item.b <= SpawnObjects.instance.currentBeat)
                            {
                                if (!selectableObstacles.Contains(item))
                                {
                                    if (selection)
                                    {
                                        if (PointInsideOfSelection(new Vector2(SpawnObjects.instance.ConvertMEPos(item.x), SpawnObjects.instance.ConvertMEPos(item.y)), startPos, endPos)) selectableObstacles.Add(item);
                                    }
                                    else selectableObstacles.Add(item);
                                }
                            }
                        }

                        List<sliders> sliders = LoadMap.instance.beats[currentBeatIndex - 1].sliders;
                        foreach (var item in sliders)
                        {
                            if (item.b >= startCurrentBeat && item.b <= SpawnObjects.instance.currentBeat)
                            {
                                if (!selectedSliders.Contains(item))
                                {
                                    if (selection)
                                    {
                                        if (PointInsideOfSelection(new Vector2(SpawnObjects.instance.ConvertMEPos(item.x), SpawnObjects.instance.ConvertMEPos(item.y)), startPos, endPos)) selectedSliders.Add(item);
                                    }
                                    else selectedSliders.Add(item);
                                }
                            }
                        }

                        // Process burstSliders
                        List<burstSliders> burstSliders = LoadMap.instance.beats[currentBeatIndex - 1].burstSliders;
                        foreach (var item in burstSliders)
                        {
                            if (item.b >= startCurrentBeat && item.b <= SpawnObjects.instance.currentBeat)
                            {
                                if (!selectedBurstSliders.Contains(item))
                                {
                                    if (selection)
                                    {
                                        if (PointInsideOfSelection(new Vector2(SpawnObjects.instance.ConvertMEPos(item.x), SpawnObjects.instance.ConvertMEPos(item.y)), startPos, endPos)) selectedBurstSliders.Add(item);
                                    }
                                    else selectedBurstSliders.Add(item);
                                }
                            }
                        }
                    }
                }
                else
                {
                    ClearSelection();
                    preview.transform.localScale = new Vector3(0, 0, 0);
                    preview.transform.localPosition = new Vector3(0, 100000, 0);
                }
                yield return new WaitForSeconds(0.05f);
            }

            selectedColorNotes.AddRange(selectableColorNotes);
            selectedBombNotes.AddRange(selectableBombNotes);
            selectedObstacles.AddRange(selectableObstacles);
            selectedSliders.AddRange(selectedSliders);
            selectedBurstSliders.AddRange(selectedBurstSliders);

            selectableColorNotes = new List<colorNotes>();
            selectableBombNotes = new List<bombNotes>();
            selectableBpmEvents = new List<bpmEvents>();
            selectableObstacles = new List<obstacles>();
            selectableSliders = new List<sliders>();
            selectableBurstSliders = new List<burstSliders>();
            //selectedSlider.AddRange(selectableSliders);
            //selectedBurstSlider.AddRange(selectableBurstSliders);
        }

        isSelecting = false;
        click = false;
        clickCheck = false;
        //SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
        yield break;
    }

    public bool PointInsideOfSelection(Vector2 itemPos, Vector2 selectionStart, Vector2 selectionEnd)
    {
        // Calculate the min and max bounds based on start and end points
        float minX = Mathf.Min(selectionStart.x, selectionEnd.x);
        float maxX = Mathf.Max(selectionStart.x, selectionEnd.x);
        float minY = Mathf.Min(selectionStart.y, selectionEnd.y);
        float maxY = Mathf.Max(selectionStart.y, selectionEnd.y);

        // Check if itemPos is within the bounds
        return itemPos.x >= minX - 1 && itemPos.x <= maxX && itemPos.y >= minY - 1 && itemPos.y <= maxY;
    }


    public void HighlightSelectedObject()
    {
        foreach (Transform itemTransform in content.transform)
        {
            GameObject item = itemTransform.gameObject;

            // Cache components once to avoid repeated GetComponent calls
            NoteData noteData = item.GetComponent<NoteData>();
            BombData bombData = item.GetComponent<BombData>();
            ObstacleData obstacleData = item.GetComponent<ObstacleData>();
            BpmEventData bpmEventData = item.GetComponent<BpmEventData>();
            TimingData timingData = item.GetComponent<TimingData>();

            // Check for NoteData
            if (noteData != null)
            {
                bool check = selectedColorNotes.Contains(noteData.note);
                if (!check)
                {
                    check = selectableColorNotes.Contains(noteData.note);
                }
                item.transform.GetChild(3).gameObject.SetActive(check);
            }

            // Check for BombData
            if (bombData != null)
            {
                bool check = selectedBombNotes.Contains(bombData.bomb);
                if (!check)
                {
                    check = selectableBombNotes.Contains(bombData.bomb);
                }
                item.transform.GetChild(1).gameObject.SetActive(check);
            }

            // Check for ObstacleData
            if (obstacleData != null)
            {
                bool check = selectedObstacles.Contains(obstacleData.obstacle);
                if (!check)
                {
                    check = selectableObstacles.Contains(obstacleData.obstacle);
                }
                item.GetComponent<LineRenderer>().material = check ? wallMaterials[0] : wallMaterials[1];
            }

            // Check for BpmEventData
            if (bpmEventData != null)
            {
                bool check = selectedBpmEvents.Contains(bpmEventData.bpmEvent);
                if (!check)
                {
                    check = selectableBpmEvents.Contains(bpmEventData.bpmEvent);
                }
                item.transform.GetChild(2).gameObject.SetActive(check);
            }

            // Check for BpmEventData
            if (timingData != null)
            {
                bool check = selectedTimings.Contains(timingData.timings);
                if (!check)
                {
                    check = selectableTimings.Contains(timingData.timings);
                }
                item.transform.GetChild(2).gameObject.SetActive(check);
            }
        }
    }

    void DeleteObjects()
    {
        if (selectedColorNotes.Count != 0 || selectedBombNotes.Count != 0 || selectedObstacles.Count != 0 || selectedSliders.Count != 0 || selectedBurstSliders.Count != 0 || selectedBpmEvents.Count != 0 || selectedTimings.Count != 0)
        {
            UndoRedoManager.instance.SaveState(LoadMap.instance.beats[40484], 40484, false);

            foreach (var item in selectedColorNotes)
            {
                bool check = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].colorNotes.Find(n => n == item) != null;
                if (check)
                {
                    UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].colorNotes.Remove(item);
                }
            }

            foreach (var item in selectedBombNotes)
            {
                bool check = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bombNotes.Find(n => n == item) != null;
                if (check)
                {
                    UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bombNotes.Remove(item);
                }
            }

            foreach (var item in selectedObstacles)
            {
                bool check = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].obstacles.Find(n => n == item) != null;
                if (check)
                {
                    UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].obstacles.Remove(item);
                }
            }

            foreach (var item in selectedBpmEvents)
            {
                bool check = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bpmEvents.Find(n => n == item) != null;
                if (check)
                {
                    UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                    LoadMap.instance.bpmEvents.Remove(item);
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bpmEvents.Remove(item);
                }
            }

            foreach (var item in selectedTimings)
            {
                bool check = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].timings.Find(n => n == item) != null;
                if (check)
                {
                    UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].timings.Remove(item);
                }
            }

            ClearSelection();
            DrawLines.instance.DrawLinesFromScratch(SpawnObjects.instance.currentBeat, SpawnObjects.instance.precision);
            SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
        }
    }

    void OffsetBeat(float precision)
    {
        if (selectedColorNotes.Count != 0 || selectedBombNotes.Count != 0 || selectedObstacles.Count != 0 || selectedSliders.Count != 0 || selectedBurstSliders.Count != 0 || selectedBpmEvents.Count != 0 || selectedTimings.Count != 0)
        {
            UndoRedoManager.instance.SaveState(LoadMap.instance.beats[40484], 40484, false);

            foreach (var item in selectedColorNotes)
            {
                colorNotes note = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].colorNotes.Find(n => n.Equals(item));

                if (note != null)
                {
                    UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].colorNotes.Remove(item);
                    item.b += precision;
                    UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].colorNotes.Add(item);
                }
            }

            foreach (var item in selectedBombNotes)
            {
                bombNotes bomb = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bombNotes.Find(n => n.Equals(item));

                if (bomb != null)
                {
                    UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bombNotes.Remove(item);
                    item.b += precision;
                    UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bombNotes.Add(item);
                }
            }

            foreach (var item in selectedObstacles)
            {
                obstacles obstacle = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].obstacles.Find(n => n.Equals(item));

                if (obstacle != null)
                {
                    UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].obstacles.Remove(item);
                    item.b += precision;
                    UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].obstacles.Add(item);
                }
            }

            foreach (var item in selectedTimings)
            {
                timings timing = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].timings.Find(n => n.Equals(item));

                if (timing != null)
                {
                    UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].timings.Remove(item);
                    item.b += precision;
                    UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].timings.Add(item);
                }
            }

            foreach (var item in selectedBpmEvents)
            {
                bpmEvents bpm = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bpmEvents.Find(n => n.Equals(item));

                if (bpm != null)
                {
                    UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bpmEvents.Remove(item);
                    LoadMap.instance.bpmEvents.Remove(item);
                    item.b += precision;
                    UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                    LoadMap.instance.bpmEvents.Add(item);
                    LoadMap.instance.bpmEvents.Sort((a, b) => a.b.CompareTo(b.b));
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bpmEvents.Add(item);
                }
            }

            DrawLines.instance.DrawLinesFromScratch(SpawnObjects.instance.currentBeat, SpawnObjects.instance.precision);
            SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
        }
    }

    public Vector2Int ArrowVector()
    {
        Vector2Int vector = new Vector2Int();

        // Check arrow key inputs
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            vector.y += 1;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            vector.y -= 1;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            vector.x -= 1;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            vector.x += 1;
        }

        return vector;
    }

    void OffsetPosition(float MEPrecision, Vector2Int vec)
    {
        if (selectedColorNotes.Count != 0 || selectedBombNotes.Count != 0 || selectedObstacles.Count != 0 || selectedSliders.Count != 0 || selectedBurstSliders.Count != 0 || selectedBpmEvents.Count != 0 || selectedTimings.Count != 0)
        {
            UndoRedoManager.instance.SaveState(LoadMap.instance.beats[40484], 40484, false);

            foreach (var item in selectedColorNotes)
            {
                colorNotes note = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].colorNotes.Find(n => n.Equals(item));

                if (note != null)
                {
                    UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].colorNotes.Remove(item);
                    if (item.x < 999 && item.x > -999) item.x += vec.x;
                    else
                    {
                        if (item.x > 999 && item.x + Mathf.RoundToInt((vec.x * 1000) / MEPrecision) < 999) item.x += -2000;
                        else if (item.x < -999 && item.x + Mathf.RoundToInt((vec.x * 1000) / MEPrecision) > -999) item.x += +2000;
                        item.x += Mathf.RoundToInt((vec.x * 1000) / MEPrecision);
                    }

                    if (item.y < 999 && item.y > -999) item.y += vec.y;
                    else
                    {
                        if (item.y > 999 && item.y + Mathf.RoundToInt((vec.y * 1000) / MEPrecision) < 999) item.y += -2000;
                        else if (item.y < -999 && item.y + Mathf.RoundToInt((vec.y * 1000) / MEPrecision) > -999) item.y += +2000;
                        item.y += Mathf.RoundToInt((vec.y * 1000) / MEPrecision);
                    }
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].colorNotes.Add(item);
                }
            }

            foreach (var item in selectedBombNotes)
            {
                bombNotes bomb = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bombNotes.Find(n => n.Equals(item));

                if (bomb != null)
                {
                    UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bombNotes.Remove(item);

                    if (item.x < 999 && item.y > -999) item.x += vec.x;
                    else
                    {
                        item.x += Mathf.RoundToInt((vec.x * 1000) / MEPrecision);
                        if (item.x < 999 && item.x > -999) item.x -= 2000;
                    }
                    if (item.y < 999 && item.y > -999) item.y += vec.y;
                    else
                    {
                        item.y += Mathf.RoundToInt((vec.y * 1000) / MEPrecision);
                        if (item.y < 999 && item.y > -999) item.y -= 2000;
                    }


                    UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bombNotes.Add(item);
                }
            }

            DrawLines.instance.DrawLinesFromScratch(SpawnObjects.instance.currentBeat, SpawnObjects.instance.precision);
            SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
        }
    }

    void MirrorSelection()
    {
        if (selectedColorNotes.Count != 0 || selectedBombNotes.Count != 0 || selectedObstacles.Count != 0 || selectedSliders.Count != 0 || selectedBurstSliders.Count != 0)
        {
            UndoRedoManager.instance.SaveState(LoadMap.instance.beats[40484], 40484, false);

            bool mirrorOnlyCheck = Input.GetKey(KeyCode.LeftShift);

            // Mirror color notes
            foreach (var item in selectedColorNotes)
            {
                colorNotes note = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].colorNotes.Find(n => n.Equals(item));

                if (note != null)
                {
                    UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                    // Perform mirror operation
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].colorNotes.Remove(item);
                    if (!mirrorOnlyCheck)
                    {
                        item.d = Invert(item.d);
                        item.x = InvertXAxis(item.x);
                        //item.a = item.a + 180; // Those who know, know that 
                    }
                    if (item.c == 0) item.c = 1; else item.c = 0;
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].colorNotes.Add(item);
                }
            }

            // Mirror bomb notes (with batching)
            if (!mirrorOnlyCheck)
            {
                foreach (var item in selectedBombNotes)
                {
                    bombNotes bomb = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bombNotes.Find(n => n.Equals(item));
                    if (bomb != null)
                    {
                        UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                        LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bombNotes.Remove(item);
                        item.x = InvertXAxis(item.x);
                        LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bombNotes.Add(item);
                    }
                }

                // Mirror obstacles (with batching)
                foreach (var item in selectedObstacles)
                {
                    obstacles obstacle = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].obstacles.Find(n => n.Equals(item));

                    if (obstacle != null)
                    {
                        UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(item.b)], Mathf.FloorToInt(item.b), true);
                        LoadMap.instance.beats[Mathf.FloorToInt(item.b)].obstacles.Remove(item);
                        item.x = InvertXAxis(item.x);
                        LoadMap.instance.beats[Mathf.FloorToInt(item.b)].obstacles.Add(item);
                    }
                }
            }

            // Rebuild the map
            SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
            DrawLines.instance.DrawLinesFromScratch(SpawnObjects.instance.currentBeat, SpawnObjects.instance.precision);
            SpawnObjects.instance.LoadWallsBackwards();
        }
    }


    public int Invert(int level)
    {
        return level switch
        {
            0 => 0,
            1 => 1,
            2 => 3,
            3 => 2,
            4 => 5,
            5 => 4,
            6 => 7,
            7 => 6,
            8 => 8,
            _ => 0
        };
    }

    public int InvertXAxis(int level)
    {
        return level switch
        {
            0 => 3,
            1 => 2,
            2 => 1,
            _ => 0
        };
    }

    void FindSelection()
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        RaycastHit[] hits = Physics.RaycastAll(ray);

        // Initialize lists for storing closest distances and corresponding game objects
        List<float> closest = new List<float> { Mathf.Infinity, Mathf.Infinity, Mathf.Infinity, Mathf.Infinity, Mathf.Infinity };
        List<GameObject> closestObj = new List<GameObject> { null, null, null, null, null };

        // Find the closest object for each type
        foreach (var hit in hits)
        {
            float distance = hit.distance;
            GameObject hitObject = hit.collider.gameObject;

            if (hit.collider.CompareTag("Note") && distance < closest[0])
            {
                closest[0] = distance;
                closestObj[0] = hitObject;
            }
            else if (hit.collider.CompareTag("Bomb") && distance < closest[1])
            {
                closest[1] = distance;
                closestObj[1] = hitObject;
            }
            else if (hit.collider.CompareTag("Obstacle") && distance < closest[2])
            {
                closest[2] = distance;
                closestObj[2] = hitObject;
            }
            else if (hit.collider.CompareTag("BpmEvent") && distance < closest[3])
            {
                closest[3] = distance;
                closestObj[3] = hitObject;
            }
            else if (hit.collider.CompareTag("Timing") && distance < closest[4])
            {
                closest[4] = distance;
                closestObj[4] = hitObject;
            }
        }

        // Determine which of the closest objects is the overall closest
        GameObject closestType = null;
        float minDistance = Mathf.Infinity;

        for (int i = 0; i < closestObj.Count; i++)
        {
            if (closestObj[i] != null && closest[i] < minDistance)
            {
                minDistance = closest[i];
                closestType = closestObj[i];
            }
        }

        if (closestType != null)
        {
            if (closestType.CompareTag("Note"))
            {
                colorNotes note = closestType.GetComponent<NoteData>().note;

                if (selectedColorNotes.Find(n => n == note) == null)
                {
                    selectedColorNotes.Add(note);
                }
                else
                {
                    selectedColorNotes.Remove(note);
                }
            }
            else if (closestType.CompareTag("Bomb"))
            {
                bombNotes bomb = closestType.GetComponent<BombData>().bomb;

                if (selectedBombNotes.Find(n => n == bomb) == null)
                {
                    selectedBombNotes.Add(bomb);
                }
                else
                {
                    selectedBombNotes.Remove(bomb);
                }
            }
            else if (closestType.CompareTag("Obstacle"))
            {
                obstacles obstacle = closestType.GetComponent<ObstacleData>().obstacle;

                if (selectedObstacles.Find(n => n == obstacle) == null)
                {
                    selectedObstacles.Add(obstacle);
                }
                else
                {
                    selectedObstacles.Remove(obstacle);
                }
            }
            else if (closestType.CompareTag("BpmEvent"))
            {
                bpmEvents bpmEvents = closestType.GetComponent<BpmEventData>().bpmEvent;

                if (selectedBpmEvents.Find(n => n == bpmEvents) == null)
                {
                    selectedBpmEvents.Add(bpmEvents);
                }
                else
                {
                    selectedBpmEvents.Remove(bpmEvents);
                }
            }
            else if (closestType.CompareTag("Timing"))
            {
                timings timing = closestType.GetComponent<TimingData>().timings;

                if (selectedTimings.Find(n => n == timing) == null)
                {
                    selectedTimings.Add(timing);
                }
                else
                {
                    selectedTimings.Remove(timing);
                }
            }
            // Reload objects after deletion
            SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
        }

        HighlightSelectedObject();
    }
}
