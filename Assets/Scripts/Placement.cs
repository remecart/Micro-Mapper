using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Placement : MonoBehaviour
{
    public Transform objectParent;
    public int placementIndex;
    public float angle;
    public bool invertControls;
    public Vector2 cachedDirection;
    public Vector2 cachedDirection2;
    private bool dot;
    public List<GameObject> objects;
    private bool isTransitioning;
    public float transitionDelay = 0.125f; // Adjust the delay time as needed

    private Vector3 lastMousePosition;
    private Vector3 hitPos;
    private bool cacheNoteType;
    public bool diag;
    private bool isPlacingWall;

    public void changePlacementObject(int i)
    {
        if (i == 0)
        {
            if (cacheNoteType == true) placementIndex = 1;
            else placementIndex = 0;
        }
        else placementIndex = i;
    }

    void Start()
    {
        foreach (GameObject item in objects)
        {
            GameObject go = Instantiate(item);
            go.transform.SetParent(objectParent);
        }
    }

    public void Update()
    {
        StartCoroutine(HandleTransitionDelay());
    }

    IEnumerator HandleTransitionDelay()
    {
        Vector3 mousePosition = Input.mousePosition;

        lastMousePosition = mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.quickSelectRedNote)) placementIndex = 0;
        if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.quickSelectBlueNote)) placementIndex = 1;
        if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.quickSelectBomb)) placementIndex = 2;
        if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.quickSelectWall)) placementIndex = 3;

        RaycastHit[] hits = Physics.RaycastAll(ray);
        RaycastHit hit = new RaycastHit();
        bool gridFound = false;
        RaycastHit noteRayCast = new RaycastHit();

        foreach (var h in hits)
        {
            if (h.collider.CompareTag("Grid"))
            {
                hit = h;
                gridFound = true;
                break;
            }
            else if (h.collider.CompareTag("Note"))
            {
                hit = h;
                noteRayCast = h;
                break;
            }
        }

        if (gridFound)
        {
            foreach (Transform child in objectParent)
            {
                child.gameObject.SetActive(false);
            }

            if (!Input.GetMouseButton(1))
            {
                GameObject preview = objectParent.transform.GetChild(placementIndex).gameObject;
                preview.SetActive(true);

                if (placementIndex != 3 && placementIndex != 4)
                {
                    preview.transform.localPosition = new Vector3(Mathf.RoundToInt(hit.point.x - 1.5f) - 0.5f, Mathf.RoundToInt(hit.point.y - 0.5f) + 0.5f, 0);

                    if (placementIndex != 2)
                    {
                        if (placementIndex == 1) cacheNoteType = true;

                        Vector2 direction = new Vector2();

                        if (!invertControls) direction = new Vector2(-Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                        else direction = new Vector2(Input.GetAxisRaw("Horizontal"), -Input.GetAxisRaw("Vertical"));

                        if (Input.GetKeyDown(KeyCode.F))
                        {
                            dot = true;
                            cachedDirection = direction;
                        }

                        if (direction.x != 0 && direction.y != 0)
                        {
                            diag = true;
                            if (direction != Vector2.zero)
                            {
                                float anglepi = Mathf.Atan2(direction.x, direction.y);
                                angle = anglepi * Mathf.Rad2Deg;
                                objectParent.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, angle);
                                objectParent.GetChild(1).transform.rotation = Quaternion.Euler(0, 0, angle);
                            }
                        }

                        if (diag == true)
                        {
                            if (direction.x == 0 || direction.y == 0)
                            {
                                yield return new WaitForSeconds(transitionDelay);

                                if (!invertControls) direction = new Vector2(-Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                                else direction = new Vector2(Input.GetAxisRaw("Horizontal"), -Input.GetAxisRaw("Vertical"));

                                if (direction != Vector2.zero)
                                {
                                    float anglepi = Mathf.Atan2(direction.x, direction.y);
                                    angle = anglepi * Mathf.Rad2Deg;
                                    objectParent.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, angle);
                                    objectParent.GetChild(1).transform.rotation = Quaternion.Euler(0, 0, angle);
                                }
                                diag = false;
                            }
                        }
                        else
                        {
                            if (direction != Vector2.zero)
                            {
                                float anglepi = Mathf.Atan2(direction.x, direction.y);
                                angle = anglepi * Mathf.Rad2Deg;
                                objectParent.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, angle);
                                objectParent.GetChild(1).transform.rotation = Quaternion.Euler(0, 0, angle);
                            }
                        }

                        if (!dot)
                        {
                            objectParent.GetChild(0).transform.GetChild(0).gameObject.SetActive(true);
                            objectParent.GetChild(0).transform.GetChild(1).gameObject.SetActive(false);

                            objectParent.GetChild(1).transform.GetChild(0).gameObject.SetActive(true);
                            objectParent.GetChild(1).transform.GetChild(1).gameObject.SetActive(false);
                        }
                        else
                        {
                            objectParent.GetChild(0).transform.GetChild(0).gameObject.SetActive(false);
                            objectParent.GetChild(0).transform.GetChild(1).gameObject.SetActive(true);
                            objectParent.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, 0);

                            objectParent.GetChild(1).transform.GetChild(0).gameObject.SetActive(false);
                            objectParent.GetChild(1).transform.GetChild(1).gameObject.SetActive(true);
                            objectParent.GetChild(1).transform.rotation = Quaternion.Euler(0, 0, 0);
                        }

                        if (cachedDirection != direction) dot = false;

                        if (Input.GetMouseButtonDown(0)) PlaceNote();
                    }
                    else
                    {
                        if (Input.GetMouseButtonDown(0)) PlaceBomb();
                    }
                }
                else
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
                        Debug.Log("something should happen");
                    }
                }
            }
        }
        else
        {
            foreach (Transform child in objectParent)
            {
                child.gameObject.SetActive(false);
            }
        }

        if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.changeNoteType))
        {
            if (noteRayCast.collider != null)
            {
                colorNotes noteHit = noteRayCast.collider.gameObject.GetComponent<NoteData>().note;
                colorNotes oldNote = new colorNotes();

                for (int i = 0; i < LoadMap.instance.beats[Mathf.FloorToInt(noteHit.b)].colorNotes.Count; i++)
                {
                    if (LoadMap.instance.beats[Mathf.FloorToInt(noteHit.b)].colorNotes[i] == noteHit)
                    {
                        oldNote = LoadMap.instance.beats[Mathf.FloorToInt(noteHit.b)].colorNotes[i];
                        break;
                    }
                }

                if (oldNote.c == 0) oldNote.c = 1;
                else oldNote.c = 0;

                SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, false);
            }
        }
    }


    IEnumerator PlaceWall(bool crouch)
    {
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

        isPlacingWall = true;
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
                if (Physics.Raycast(ray, out RaycastHit hit2) && hit.collider.CompareTag("Grid"))
                {
                    endPos = new Vector3(Mathf.RoundToInt(hit2.point.x - 0.5f) + 0.5f, Mathf.RoundToInt(hit2.point.y - 0.5f) + 0.5f, SpawnObjects.instance.PositionFromBeat(SpawnObjects.instance.currentBeat) * SpawnObjects.instance.editorScale);

                    if (Input.GetKeyDown(KeyCode.I)) click = true;
                    Debug.Log(startPos + " - " + endPos);

                    int positive = 1;
                    if (endPos.x - startPos.x < 0) positive = -1;

                    if (hit2.point.y < 1.49f)
                    {
                        preview.transform.position = new Vector3(startPos.x + (endPos.x - startPos.x) / 2, 2.25f, startPos.z + (endPos.z - startPos.z) / 2);
                        preview.transform.localScale = new Vector3(endPos.x - startPos.x + positive, 4.5f, endPos.z - startPos.z);
                    }
                    else
                    {
                        preview.transform.position = new Vector3(startPos.x + (endPos.x - startPos.x) / 2, 3, startPos.z + (endPos.z - startPos.z) / 2);
                        preview.transform.localScale = new Vector3(endPos.x - startPos.x + positive, 3, endPos.z - startPos.z);
                    }
                }
                yield return null;
            }

            bool stackedCheck = false;
            obstacles wall = new obstacles();
            wall.b = beat;
            wall.x = Mathf.RoundToInt(preview.transform.position.x - preview.transform.localScale.x / 2);
            wall.y = Mathf.RoundToInt(preview.transform.position.y - preview.transform.localScale.y / 2);
            wall.d = SpawnObjects.instance.currentBeat - beat;
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
                LoadMap.instance.beats[Mathf.FloorToInt(beat)].obstacles.Add(wall);
                SpawnObjects.instance.LoadObjectsFromScratch(Mathf.FloorToInt(beat), false, false);
            }
        }

        isPlacingWall = false;
        yield break;
    }


    void PlaceNote()
    {
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

        if (gridFound)
        {
            Vector3 pos = new Vector3(Mathf.RoundToInt(hit.point.x - 0.5f) + 0.5f, Mathf.RoundToInt(hit.point.y - 0.5f) + 0.5f, 0);

            float beat = SpawnObjects.instance.currentBeat;
            bool stackedCheck = false;
            colorNotes note = new colorNotes();
            note.b = beat;
            note.x = Mathf.FloorToInt(pos.x);
            note.y = Mathf.FloorToInt(pos.y);
            note.c = placementIndex;
            note.d = dot ? 8 : Rotation((int)angle);

            foreach (var item in LoadMap.instance.beats[Mathf.FloorToInt(beat)].colorNotes)
            {
                if (item.x == note.x && item.y == note.y && item.b == note.b)
                {
                    stackedCheck = true;
                    break;
                }
            }

            if (!stackedCheck)
            {
                LoadMap.instance.beats[Mathf.FloorToInt(beat)].colorNotes.Add(note);
                SpawnObjects.instance.LoadObjectsFromScratch(Mathf.FloorToInt(beat), false, false);
            }
        }
    }

    void PlaceBomb()
    {
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

        if (gridFound)
        {
            Vector3 pos = new Vector3(Mathf.RoundToInt(hit.point.x - 0.5f) + 0.5f, Mathf.RoundToInt(hit.point.y - 0.5f) + 0.5f, 0);

            float beat = SpawnObjects.instance.currentBeat;
            bool stackedCheck = false;
            bombNotes bomb = new bombNotes();
            bomb.b = beat;
            bomb.x = Mathf.FloorToInt(pos.x);
            bomb.y = Mathf.FloorToInt(pos.y);

            foreach (var item in LoadMap.instance.beats[Mathf.FloorToInt(beat)].colorNotes)
            {
                if (item.x == bomb.x && item.y == bomb.y && item.b == bomb.b)
                {
                    stackedCheck = true;
                    break;
                }
            }

            if (!stackedCheck)
            {
                LoadMap.instance.beats[Mathf.FloorToInt(beat)].bombNotes.Add(bomb);
                SpawnObjects.instance.LoadObjectsFromScratch(Mathf.FloorToInt(beat), false, false);
            }
        }
    }

    int Rotation(int level)
    {
        return level switch
        {
            180 => 0,
            0 => 1,
            -90 => 2,
            90 => 3,
            -135 => 4,
            135 => 5,
            -45 => 6,
            45 => 7,
            _ => 0
        };
    }

    public void switchType(int index)
    {
        placementIndex = index;
    }
}