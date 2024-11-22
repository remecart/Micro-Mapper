using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using Unity.Loading;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

public class Placement : MonoBehaviour
{
    public static Placement instance;
    public Transform objectParent;
    public List<GameObject> objects;
    public int placementIndex;
    public bool invertControls;
    public bool invertWallScroll;
    public bool dot;
    public float bufferTime;
    public float bufferTimeRunning;
    public Transform content;
    public bool isPlacingWall;
    private bool left;
    public List<RawImage> rawImages;
    public List<Color> colors;

    public bool enableME;
    public bool allowOutsideOfGrid;
    public Vector2Int MEgridSize;
    public int MEprecision;
    public int gridYPos;
    public GameObject grid;
    public List<GameObject> Lines;
    public GameObject spectro;
    public GameObject spectroLine;

    void Start()
    {
        enableME = Settings.instance.config.mapping.mappingExtensions.enabled;
        allowOutsideOfGrid = Settings.instance.config.mapping.mappingExtensions.allowPlacementOutsideOfGrid;
        gridYPos = Settings.instance.config.mapping.mappingExtensions.gridYPos;
        MEprecision = Settings.instance.config.mapping.mappingExtensions.precision;
        MEgridSize = new Vector2Int(Settings.instance.config.mapping.mappingExtensions.gridWidth, Settings.instance.config.mapping.mappingExtensions.gridHeight);

        input.text = ReadMapInfo.instance.info._beatsPerMinute.ToString();
        invertControls = Settings.instance.config.controls.invertNoteAngle;
        invertWallScroll = Settings.instance.config.controls.invertWallScroll;

        instance = this;
        foreach (GameObject item in objects)
        {
            GameObject go = Instantiate(item);
            go.transform.SetParent(objectParent);
            go.gameObject.SetActive(false);
            go.gameObject.GetComponent<BoxCollider>().enabled = false;
        }

        ReloadGrid();
    }

    public void ReloadGrid()
    {
        if (enableME)
        {
            grid.transform.localScale = new Vector3((float)MEgridSize.x / 10f, 1, (float)MEgridSize.y / 10f);
            grid.transform.localPosition = new Vector3(0, MEgridSize.y / 2f - gridYPos, 0);
            Vector4 tiling = new Vector4(MEgridSize.x, MEgridSize.y, 0, 0);
            grid.GetComponent<MeshRenderer>().materials[0].SetVector("_Tiling", tiling);

            spectro.transform.localPosition = new Vector3(-2.5f - ((float)MEgridSize.x - 4f) / 2f, 0, 0);
            spectroLine.transform.localPosition = new Vector3(-5f - ((float)MEgridSize.x - 4f) / 2f, 0.01f, 0);

            Lines[1].transform.localPosition = new Vector3(6 + ((float)MEgridSize.x - 4f) / 2f, 0, 0);
            Lines[2].transform.localPosition = new Vector3(9 + ((float)MEgridSize.x - 4f) / 2f, 0, 0);
            Lines[3].transform.localPosition = new Vector3(6 + ((float)MEgridSize.x - 4f) / 2f, 0.5f, 0);
            Lines[4].transform.localPosition = new Vector3(9 + ((float)MEgridSize.x - 4f) / 2f, 0.5f, 0);
            DrawLines.instance.lines[0].width = MEgridSize.x;
        }
        else
        {
            grid.transform.localScale = new Vector3(0.4f, 1, 0.3f);
            grid.transform.localPosition = new Vector3(0, 1.5f, 0);
            Vector4 tiling = new Vector4(4, 3, 0, 0);
            grid.GetComponent<MeshRenderer>().materials[0].SetVector("_Tiling", tiling);

            spectro.transform.localPosition = new Vector3(-2.5f, 0, 0);
            spectroLine.transform.localPosition = new Vector3(-5f, 0.01f, 0);

            Lines[1].transform.localPosition = new Vector3(6, 0, 0);
            Lines[2].transform.localPosition = new Vector3(9, 0, 0);
            Lines[3].transform.localPosition = new Vector3(6, 0.5f, 0);
            Lines[4].transform.localPosition = new Vector3(9, 0.5f, 0);
            DrawLines.instance.lines[0].width = 4;
        }

        SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
        DrawLines.instance.DrawLinesFromScratch(SpawnObjects.instance.currentBeat, SpawnObjects.instance.precision);
    }

    void Update()
    {
        if (!Settings.instance.isHovering && !SelectObjects.instance.isSelecting && !DrawInEditor.instance.drawing && !Menu.instance.open)
        {
            bufferTimeRunning -= Time.deltaTime;
            if (!Input.GetMouseButton(1) && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftControl)) Place();
            if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.changeNoteType)) SwitchNoteType();

            if (placementIndex == 0) left = true;
            else if (placementIndex == 1) left = false;

            foreach (RawImage img in rawImages)
            {
                img.color = colors[1];
            }

            colors[2] = Settings.instance.config.mapping.colorSettings.leftNote;
            colors[3] = Settings.instance.config.mapping.colorSettings.rightNote;

            colors[2] = new Color(colors[2].r, colors[2].g, colors[2].b, 1);
            colors[3] = new Color(colors[3].r, colors[3].g, colors[3].b, 1);

            rawImages[5].color = colors[2];
            rawImages[6].color = colors[3];

            rawImages[placementIndex].color = colors[0];

            if (Input.GetKey(KeyCode.LeftAlt)) AltControls();
        }
        else
        {
            foreach (Transform child in objectParent)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    public void ChangePlacementIndex(int index)
    {
        if (index == 0)
        {
            if (left) placementIndex = 0;
            else placementIndex = 1;
        }
        else
        {
            placementIndex = index;
        }
    }

    public void SetColor(int index)
    {
        placementIndex = index;
    }

    void Place()
    {
        Vector3 mousePosition = Input.mousePosition;

        // lastMousePosition = mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (Input.GetKeyDown(KeyCode.Alpha1)) placementIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) placementIndex = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) placementIndex = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) placementIndex = 3;
        if (Input.GetKeyDown(KeyCode.Alpha5)) placementIndex = 4;

        RaycastHit[] hits = Physics.RaycastAll(ray);
        RaycastHit hit = new RaycastHit();
        bool gridFound = false;
        bool timingGridFound = false;
        bool bpmGridFound = false;

        foreach (var h in hits)
        {
            if (h.collider.CompareTag("Grid"))
            {
                hit = h;
                gridFound = true;
            }
            else if (h.collider.CompareTag("TimingGrid"))
            {
                hit = h;
                timingGridFound = true;
            }
            else if (h.collider.CompareTag("BpmGrid"))
            {
                hit = h;
                bpmGridFound = true;
            }
        }

        if (gridFound)
        {
            foreach (Transform child in objectParent)
            {
                child.gameObject.SetActive(false);
            }

            GameObject preview = objectParent.transform.GetChild(placementIndex).gameObject;
            if (placementIndex != 4) preview.SetActive(true);

            // NOTES
            if (placementIndex == 0 || placementIndex == 1)
            {
                Vector3 pos = new Vector3();

                if (enableME && MEprecision != 1)
                    pos = new Vector3(
                        Mathf.Round((hit.point.x) * MEprecision) / MEprecision - 2f,
                        Mathf.Round((hit.point.y) * MEprecision) / MEprecision,
                        0
                    );
                else pos = new Vector3(Mathf.RoundToInt(hit.point.x - 1.5f) - 0.5f, Mathf.RoundToInt(hit.point.y - 0.5f) + 0.5f, 0);

                preview.transform.localPosition = pos;

                Vector2 direction = new Vector2();
                float angle = 0;

                if (!invertControls) direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                else direction = new Vector2(-Input.GetAxisRaw("Horizontal"), -Input.GetAxisRaw("Vertical"));

                if (direction != new Vector2() && bufferTimeRunning <= 0)
                {
                    dot = false;
                    angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
                    objectParent.transform.GetChild(0).transform.eulerAngles = new Vector3(preview.transform.eulerAngles.x, preview.transform.eulerAngles.y, angle);
                    objectParent.transform.GetChild(1).transform.eulerAngles = new Vector3(preview.transform.eulerAngles.x, preview.transform.eulerAngles.y, angle);
                }

                if (direction.x != 0 && direction.y != 0)
                {
                    bufferTimeRunning = bufferTime;
                }

                if (Input.GetKeyDown(KeyCode.F))
                {
                    dot = true;
                }

                if (!dot)
                {
                    preview.transform.transform.GetChild(0).gameObject.SetActive(true);
                    preview.transform.transform.GetChild(1).gameObject.SetActive(false);
                }
                else
                {
                    preview.transform.transform.GetChild(0).gameObject.SetActive(false);
                    preview.transform.transform.GetChild(1).gameObject.SetActive(true);
                    preview.transform.eulerAngles = new Vector3(0, 0, 0);
                }

                int cd = 8;
                if (!dot) cd = Rotation((int)preview.transform.eulerAngles.z);

                if (Input.GetMouseButtonDown(0)) PlaceNote(SpawnObjects.instance.currentBeat, pos.x, pos.y, placementIndex, cd);
            }
            else if (placementIndex == 2)
            {
                Vector3 pos = new Vector3();

                if (enableME && MEprecision != 1)
                    pos = new Vector3(
                        Mathf.Round((hit.point.x) * MEprecision) / MEprecision - 2f,
                        Mathf.Round((hit.point.y) * MEprecision) / MEprecision,
                        0
                    );
                else pos = new Vector3(Mathf.RoundToInt(hit.point.x - 1.5f) - 0.5f, Mathf.RoundToInt(hit.point.y - 0.5f) + 0.5f, 0);

                preview.transform.localPosition = pos;

                if (Input.GetMouseButtonDown(0)) PlaceBomb(SpawnObjects.instance.currentBeat, pos.x, pos.y);
            }
            else if (placementIndex == 3 && !isPlacingWall)
            {
                bool crouch = false;
                if (hit.point.y > 1.49f && !isPlacingWall)
                {
                    preview.transform.localPosition = new Vector3(Mathf.RoundToInt(hit.point.x - 1.5f) - 0.5f, 3f, 0);
                    preview.transform.localScale = new Vector3(1, 3, 0.125f);
                    crouch = true;
                }
                else if (!isPlacingWall)
                {
                    preview.transform.localPosition = new Vector3(Mathf.RoundToInt(hit.point.x - 1.5f) - 0.5f, 2.25f, 0);
                    preview.transform.localScale = new Vector3(1, 4.5f, 0.125f);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    StartCoroutine(PlaceWall(crouch));
                }
            }
        }
        else if (timingGridFound)
        {
            foreach (Transform child in objectParent)
            {
                child.gameObject.SetActive(false);
            }

            GameObject preview = objectParent.transform.GetChild(5).gameObject;
            preview.SetActive(true);

            Vector3 pos = new Vector3(Mathf.RoundToInt(hit.point.x - 1.5f) - 0.5f, Mathf.RoundToInt(hit.point.y - 0.5f) + 0.5f, 0);

            preview.transform.localPosition = pos;

            if (Input.GetMouseButtonDown(0)) PlaceTiming(SpawnObjects.instance.currentBeat, Mathf.FloorToInt(pos.x - Lines[1].transform.position.x + 3));
        }
        else if (bpmGridFound)
        {
            foreach (Transform child in objectParent)
            {
                child.gameObject.SetActive(false);
            }

            GameObject preview = objectParent.transform.GetChild(4).gameObject;
            preview.SetActive(true);
            preview.transform.GetChild(0).GetComponent<TextMeshPro>().text = input.text;
            preview.transform.localPosition = new Vector3(SpawnObjects.instance.BpmChangeSpawm.transform.position.x -2f, 0.5f, 0);

            if (Input.GetMouseButtonDown(0)) PlaceBpmChange(SpawnObjects.instance.currentBeat, float.Parse(input.text));
        }
        else
        {
            foreach (Transform child in objectParent)
            {
                child.gameObject.SetActive(false);
            }
        }

        if (placementIndex == 4)
        {
            DeleteObject();
        }
    }

    public TMP_InputField input;

    void DeleteObject()
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

        if (closestType != null && Input.GetMouseButtonDown(0))
        {
            if (closestType.CompareTag("Note"))
            {
                colorNotes note = closestType.GetComponent<NoteData>().note;
                List<colorNotes> itemsToRemove = new List<colorNotes>();

                UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(note.b)], Mathf.FloorToInt(note.b), false);
                LoadMap.instance.beats[Mathf.FloorToInt(note.b)].colorNotes.Remove(note);

            }
            else if (closestType.CompareTag("Bomb"))
            {
                bombNotes bomb = closestType.GetComponent<BombData>().bomb;
                List<bombNotes> itemsToRemove = new List<bombNotes>();

                UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(bomb.b)], Mathf.FloorToInt(bomb.b), false);
                LoadMap.instance.beats[Mathf.FloorToInt(bomb.b)].bombNotes.Remove(bomb);

            }
            else if (closestType.CompareTag("Timing"))
            {
                timings timing = closestType.GetComponent<TimingData>().timings;
                List<bombNotes> itemsToRemove = new List<bombNotes>();

                UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(timing.b)], Mathf.FloorToInt(timing.b), false);
                LoadMap.instance.beats[Mathf.FloorToInt(timing.b)].timings.Remove(timing);

            }
            else if (closestType.CompareTag("Obstacle"))
            {
                obstacles obstacle = closestType.GetComponent<ObstacleData>().obstacle;
                List<obstacles> itemsToRemove = new List<obstacles>();

                UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(obstacle.b)], Mathf.FloorToInt(obstacle.b), false);
                LoadMap.instance.beats[Mathf.FloorToInt(obstacle.b)].obstacles.Remove(obstacle);

            }
            else if (closestType.CompareTag("BpmEvent"))
            {
                bpmEvents bpmEvents = closestType.GetComponent<BpmEventData>().bpmEvent;
                List<bpmEvents> itemsToRemove = new List<bpmEvents>();

                UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(bpmEvents.b)], Mathf.FloorToInt(bpmEvents.b), false);
                LoadMap.instance.beats[Mathf.FloorToInt(bpmEvents.b)].bpmEvents.Remove(bpmEvents);
                LoadMap.instance.bpmEvents.Remove(bpmEvents);
            }
            // Reload objects after deletion
            SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
        }
    }

    private colorNotes altMoveCache;

    void AltControls()
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            // If the raycast hits something, access the hit information
            GameObject hitObject = hitInfo.collider.gameObject;

            if (Input.mouseScrollDelta.y != 0)
            {
                if (hitObject.CompareTag("Obstacle"))
                {
                    obstacles item = hitObject.GetComponent<ObstacleData>().obstacle;
                    obstacles obstacle = LoadMap.instance.beats[Mathf.FloorToInt(item.b)].obstacles.Find(n => n.Equals(item));
                    int a = invertWallScroll ? -1 : 1;

                    if (Input.mouseScrollDelta.y > 0) obstacle.d += SpawnObjects.instance.precision * a;
                    else obstacle.d -= SpawnObjects.instance.precision * a;
                }

                // Reload objects after deletion
                SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
            }

            if (Input.GetMouseButtonDown(0))
            {
                float closestDistance = float.MaxValue;
                RaycastHit closestHit = default;

                foreach (var hitNote in hits)
                {
                    if (hitNote.collider.CompareTag("Note"))
                    {
                        float distance = Vector3.Distance(ray.origin, hitNote.point);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestHit = hitNote;
                        }
                    }
                }

                // If a closest hit was found, cache the note
                if (closestDistance < float.MaxValue)
                {
                    altMoveCache = closestHit.collider.gameObject.GetComponent<NoteData>().note;
                }
            }

            if (Input.GetMouseButtonUp(0)) altMoveCache = null;

            if (Input.GetMouseButton(0) && altMoveCache != null)
            {
                // Find the closest object for each type
                foreach (var hit in hits)
                {
                    if (hit.collider.CompareTag("Grid"))
                    {
                        colorNotes note = altMoveCache;
                        LoadMap.instance.beats[Mathf.FloorToInt(note.b)].colorNotes.Remove(note);

                        Vector3 pos = new Vector3(Mathf.RoundToInt(hit.point.x - 1.5f) - 0.5f, Mathf.RoundToInt(hit.point.y - 0.5f) + 0.5f, 0);
                        note.b = SpawnObjects.instance.currentBeat;
                        note.x = Mathf.FloorToInt(pos.x + 2);
                        note.y = Mathf.FloorToInt(pos.y);

                        Vector2 direction = new Vector2();
                        float angle = 0;

                        if (!invertControls) direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                        else direction = new Vector2(-Input.GetAxisRaw("Horizontal"), -Input.GetAxisRaw("Vertical"));

                        if (direction != new Vector2() && bufferTimeRunning <= 0)
                        {
                            dot = false;
                            angle = ClampAngle(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90);

                            if (direction.x != 0 && direction.y != 0)
                            {
                                bufferTimeRunning = bufferTime;
                            }

                            int cd = Rotation(Mathf.RoundToInt(angle));
                            if (dot) cd = 8;

                            note.d = cd;
                        }

                        if (Input.GetKeyDown(KeyCode.F))
                        {
                            dot = true;
                        }

                        LoadMap.instance.beats[Mathf.FloorToInt(note.b)].colorNotes.Add(note);

                        SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
                        SpawnObjects.instance.LoadWallsBackwards();


                    }
                }
            }
        }
    }

    float ClampAngle(float angle)
    {
        angle = angle % 360;
        if (angle < 0) angle += 360;
        return angle;
    }



    void SwitchNoteType()
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (var h in hits)
        {
            if (h.collider.CompareTag("Note"))
            {
                colorNotes note = h.collider.gameObject.GetComponent<NoteData>().note;
                int beatIndex = Mathf.FloorToInt(note.b);

                if (beatIndex < 1 || beatIndex > LoadMap.instance.beats.Count)
                {
                    continue;
                }

                foreach (var item in LoadMap.instance.beats[beatIndex - 1].colorNotes)
                {
                    if (item == note)
                    {
                        UndoRedoManager.instance.SaveState(LoadMap.instance.beats[beatIndex - 1], beatIndex, false);
                        item.c = (item.c == 0) ? 1 : 0;
                        SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, false, true);
                    }
                }
            }
        }
    }


    void PlaceNote(float b, float x, float y, int c, int d)
    {
        bool stackedCheck = false;
        colorNotes note = new colorNotes();

        note.b = b;
        if (enableME)
        {
            note.x = SpawnObjects.instance.ConvertToMEPos(x + 1.5f);
            note.y = SpawnObjects.instance.ConvertToMEPos(y - 0.5f);
        }
        else
        {
            note.x = Mathf.FloorToInt(x + 2);
            note.y = Mathf.FloorToInt(y);
        }
        note.c = c;
        note.d = d;

        if (!KeybindManager.instance.AreAllKeysHeld(Settings.instance.config.keybinds.allowFusedNotePlacement))
        {
            foreach (var item in LoadMap.instance.beats[Mathf.FloorToInt(b)].colorNotes)
            {
                if (item.x == note.x && item.y == note.y && item.b == note.b)
                {
                    stackedCheck = true;
                    break;
                }
            }
        }

        if (!stackedCheck)
        {
            UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(b)], Mathf.FloorToInt(note.b), false);
            LoadMap.instance.beats[Mathf.FloorToInt(b)].colorNotes.Add(note);
        }

        SpawnObjects.instance.LoadObjectsFromScratch(b, true, true);
    }

    void PlaceBomb(float b, float x, float y)
    {
        bool stackedCheck = false;
        bombNotes bomb = new bombNotes();

        bomb.b = b;
        if (enableME)
        {
            bomb.x = SpawnObjects.instance.ConvertToMEPos(x + 1.5f);
            bomb.y = SpawnObjects.instance.ConvertToMEPos(y - 0.5f);
        }
        else
        {
            bomb.x = Mathf.FloorToInt(x + 2);
            bomb.y = Mathf.FloorToInt(y);
        }

        if (!KeybindManager.instance.AreAllKeysHeld(Settings.instance.config.keybinds.allowFusedNotePlacement))
        {
            foreach (var item in LoadMap.instance.beats[Mathf.FloorToInt(b)].bombNotes)
            {
                if (item.x == bomb.x && item.y == bomb.y && item.b == bomb.b)
                {
                    stackedCheck = true;
                    break;
                }
            }
        }

        if (!stackedCheck)
        {
            UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(bomb.b)], Mathf.FloorToInt(bomb.b), false);
            LoadMap.instance.beats[Mathf.FloorToInt(b)].bombNotes.Add(bomb);
        }

        SpawnObjects.instance.LoadObjectsFromScratch(b, true, true);
    }

    void PlaceTiming(float b, int t)
    {
        bool stackedCheck = false;
        timings timing = new timings();

        timing.b = b;
        timing.t = t;

        if (!KeybindManager.instance.AreAllKeysHeld(Settings.instance.config.keybinds.allowFusedNotePlacement))
        {
            foreach (var item in LoadMap.instance.beats[Mathf.FloorToInt(b)].timings)
            {
                if (item.t == timing.t && item.b == timing.b)
                {
                    stackedCheck = true;
                    break;
                }
            }
        }

        if (!stackedCheck)
        {
            UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(timing.b)], Mathf.FloorToInt(timing.b), false);
            LoadMap.instance.beats[Mathf.FloorToInt(b)].timings.Add(timing);
        }

        SpawnObjects.instance.LoadObjectsFromScratch(b, true, true);
    }

    void PlaceBpmChange(float b, float m)
    {
        bool stackedCheck = false;
        bpmEvents bpmEvent = new bpmEvents();

        bpmEvent.b = b;
        bpmEvent.m = m;

        if (!KeybindManager.instance.AreAllKeysHeld(Settings.instance.config.keybinds.allowFusedNotePlacement))
        {
            foreach (var item in LoadMap.instance.beats[Mathf.FloorToInt(b)].bpmEvents)
            {
                if (item.m == bpmEvent.m && item.b == bpmEvent.b)
                {
                    stackedCheck = true;
                    break;
                }
            }
        }

        if (!stackedCheck)
        {
            UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(bpmEvent.b)], Mathf.FloorToInt(bpmEvent.b), false);
            LoadMap.instance.beats[Mathf.FloorToInt(b)].bpmEvents.Add(bpmEvent);
            LoadMap.instance.bpmEvents.Add(bpmEvent);
            LoadMap.instance.bpmEvents = LoadMap.instance.bpmEvents.OrderBy(x => x.b).ToList();
        }

        SpawnObjects.instance.LoadObjectsFromScratch(b + 4, false, true);
        SpawnObjects.instance.LoadObjectsFromScratch(b, true, true);
    }

    IEnumerator PlaceWall(bool crouch)
    {
        isPlacingWall = true;
        bool clickCheck = false;
        GameObject preview = objectParent.transform.GetChild(placementIndex).gameObject;
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
            Vector3 startPos = new Vector3(Mathf.RoundToInt(hit.point.x - 0.5f) + 0.5f, Mathf.RoundToInt(hit.point.y - 0.5f) + 0.5f, SpawnObjects.instance.PositionFromBeat(SpawnObjects.instance.currentBeat) * SpawnObjects.instance.editorScale);
            Vector3 endPos = new Vector3();

            bool click = false;

            while (!Input.GetMouseButton(0))
            {
                yield return null;
            }

            while (!click)
            {
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

                if (gridFound2 == true)
                {
                    endPos = new Vector3(Mathf.RoundToInt(hit2.point.x - 0.5f) + 0.5f, Mathf.RoundToInt(hit2.point.y - 0.5f) + 0.5f, SpawnObjects.instance.PositionFromBeat(SpawnObjects.instance.currentBeat) * SpawnObjects.instance.editorScale);

                    if (Input.GetMouseButtonUp(0)) clickCheck = true;
                    if (Input.GetMouseButtonDown(0) && clickCheck == true) click = true;

                    int positive = 1;
                    if (endPos.x - startPos.x < 0) positive = -1;

                    if (hit2.point.y < 1.49f)
                    {
                        preview.transform.position = new Vector3(startPos.x + (endPos.x - startPos.x) / 2, 2f, startPos.z + (endPos.z - startPos.z) / 2 - SpawnObjects.instance.PositionFromBeat(SpawnObjects.instance.currentBeat) * SpawnObjects.instance.editorScale);
                        preview.transform.localScale = new Vector3(endPos.x - startPos.x + positive, 5f, endPos.z - startPos.z);
                    }
                    else
                    {
                        preview.transform.position = new Vector3(startPos.x + (endPos.x - startPos.x) / 2, 3, startPos.z + (endPos.z - startPos.z) / 2 - SpawnObjects.instance.PositionFromBeat(SpawnObjects.instance.currentBeat) * SpawnObjects.instance.editorScale);
                        preview.transform.localScale = new Vector3(endPos.x - startPos.x + positive, 3, endPos.z - startPos.z);
                    }
                }
                yield return null;
            }

            var minDuration = 0f;
            if (SpawnObjects.instance.currentBeat - beat == 0) minDuration = 0.0625f;

            bool stackedCheck = false;
            obstacles wall = new obstacles();
            wall.b = beat;
            wall.x = Mathf.RoundToInt(preview.transform.position.x - preview.transform.localScale.x / 2);
            wall.y = Mathf.RoundToInt(preview.transform.position.y - preview.transform.localScale.y / 2);
            wall.d = SpawnObjects.instance.currentBeat - beat + minDuration;
            wall.w = Mathf.RoundToInt(preview.transform.localScale.x);
            wall.h = Mathf.RoundToInt(preview.transform.localScale.y);

            foreach (var item in LoadMap.instance.beats[Mathf.FloorToInt(beat)].colorNotes)
            {
                if (item.x == wall.x && item.y == wall.y && item.b == wall.b)
                {
                    stackedCheck = true;
                    break;
                }
            }

            if (!stackedCheck)
            {
                UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(wall.b)], Mathf.FloorToInt(wall.b), false);
                LoadMap.instance.beats[Mathf.FloorToInt(beat)].obstacles.Add(wall);
            }
        }

        isPlacingWall = false;

        SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
        yield break;
    }


    int Rotation(int level)
    {
        return level switch
        {
            180 => 0,
            0 => 1,
            270 => 2,
            90 => 3,
            225 => 4,
            135 => 5,
            315 => 6,
            45 => 7,
            _ => 8
        };
    }
}
