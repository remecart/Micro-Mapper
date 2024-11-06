using System.Collections;
using System.Collections.Generic;
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
        HighlightSelectedObject();

        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetMouseButtonDown(0)) FindSelection();
            if (Input.GetKeyDown(KeyCode.UpArrow)) OffsetBeat(SpawnObjects.instance.precision);
            if (Input.GetKeyDown(KeyCode.DownArrow)) OffsetBeat(-SpawnObjects.instance.precision);
        }
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.A))
        {
            ClearSelection();
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

    public void ClearSelection()
    {
        selectedColorNotes = new List<colorNotes>();
        selectedBombNotes = new List<bombNotes>();
        selectedBpmEvents = new List<bpmEvents>();
        selectedObstacles = new List<obstacles>();

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
                                if (selection)
                                {
                                    if (PointInsideOfSelection(new Vector2(SpawnObjects.instance.ConvertMEPos(item.x), SpawnObjects.instance.ConvertMEPos(item.y)), startPos, endPos)) selectedSliders.Add(item);
                                }
                                else selectedSliders.Add(item);
                            }
                        }

                        List<burstSliders> burstSliders = LoadMap.instance.beats[currentBeatIndex - 1].burstSliders;
                        foreach (var item in burstSliders)
                        {
                            if (item.b >= startCurrentBeat && item.b <= SpawnObjects.instance.currentBeat)
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
        }
    }

    void DeleteObjects()
    {
        foreach (var item in selectedColorNotes)
        {
            bool check = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].colorNotes.Find(n => n == item) != null;
            if (check) LoadMap.instance.beats[Mathf.FloorToInt(item.b)].colorNotes.Remove(item);
        }

        foreach (var item in selectedBombNotes)
        {
            bool check = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bombNotes.Find(n => n == item) != null;
            if (check) LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bombNotes.Remove(item);
        }

        foreach (var item in selectedObstacles)
        {
            bool check = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].obstacles.Find(n => n == item) != null;
            if (check) LoadMap.instance.beats[Mathf.FloorToInt(item.b)].obstacles.Remove(item);
        }

        foreach (var item in selectedBpmEvents)
        {
            bool check = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bpmEvents.Find(n => n == item) != null;
            if (check) LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bpmEvents.Remove(item);
        }

        ClearSelection();
        SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
    }

    void OffsetBeat(float precision)
    {
        foreach (var item in selectedColorNotes)
        {
            colorNotes note = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].colorNotes.Find(n => n.Equals(item));

            if (note != null)
            {
                LoadMap.instance.beats[Mathf.FloorToInt(item.b)].colorNotes.Remove(item);
                item.b += precision;
                LoadMap.instance.beats[Mathf.FloorToInt(item.b)].colorNotes.Add(item);
            }
        }

        foreach (var item in selectedBombNotes)
        {
            bombNotes bomb = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bombNotes.Find(n => n.Equals(item));

            if (bomb != null)
            {
                LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bombNotes.Remove(item);
                item.b += precision;
                LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bombNotes.Add(item);
            }
        }

        foreach (var item in selectedObstacles)
        {
            obstacles obstacle = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].obstacles.Find(n => n.Equals(item));

            if (obstacle != null)
            {
                LoadMap.instance.beats[Mathf.FloorToInt(item.b)].obstacles.Remove(item);
                item.b += precision;
                LoadMap.instance.beats[Mathf.FloorToInt(item.b)].obstacles.Add(item);
            }
        }

        foreach (var item in selectedBpmEvents)
        {
            bpmEvents bpm = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bpmEvents.Find(n => n.Equals(item));

            if (bpm != null)
            {
                LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bpmEvents.Remove(item);
                item.b += precision;
                LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bpmEvents.Add(item);
            }
        }

        SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
    }

    void MirrorSelection()
    {
        bool mirrorOnlyCheck = Input.GetKey(KeyCode.LeftShift);

        foreach (var item in selectedColorNotes)
        {
            colorNotes note = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].colorNotes.Find(n => n.Equals(item));

            if (note != null)
            {
                LoadMap.instance.beats[Mathf.FloorToInt(item.b)].colorNotes.Remove(item);
                if (!mirrorOnlyCheck)
                {
                    item.d = Invert(item.d);
                    item.x = InvertXAxis(item.x);
                    item.a = item.a + 180;
                }
                if (item.c == 0) item.c = 1; else item.c = 0;
                LoadMap.instance.beats[Mathf.FloorToInt(item.b)].colorNotes.Add(item);
            }
        }

        if (!mirrorOnlyCheck)
        {
            foreach (var item in selectedBombNotes)
            {
                bombNotes bomb = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bombNotes.Find(n => n.Equals(item));

                if (bomb != null)
                {
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bombNotes.Remove(item);
                    item.x = InvertXAxis(item.x);
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].bombNotes.Add(item);
                }
            }

            foreach (var item in selectedObstacles)
            {
                obstacles obstacle = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].obstacles.Find(n => n.Equals(item));

                if (obstacle != null)
                {
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].obstacles.Remove(item);
                    item.x = InvertXAxis(item.x);
                    LoadMap.instance.beats[Mathf.FloorToInt(item.b)].obstacles.Add(item);
                }
            }
        }
        SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
        SpawnObjects.instance.LoadWallsBackwards();
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
        List<float> closest = new List<float> { Mathf.Infinity, Mathf.Infinity, Mathf.Infinity, Mathf.Infinity };
        List<GameObject> closestObj = new List<GameObject> { null, null, null, null };

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
            // Reload objects after deletion
            SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
        }

        HighlightSelectedObject();
    }
}
