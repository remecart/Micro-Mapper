using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

public class Placement : MonoBehaviour
{
    public Transform objectParent;
    public List<GameObject> objects;
    public int placementIndex;
    public bool invertControls;
    public bool dot;
    public float bufferTime;
    public float bufferTimeRunning;
    public Transform content;
    public bool isPlacingWall;
    private bool left;
    public List<RawImage> rawImages;
    public List<Color> colors;

    void Start()
    {
        foreach (GameObject item in objects)
        {
            GameObject go = Instantiate(item);
            go.transform.SetParent(objectParent);
            go.gameObject.SetActive(false);
            go.gameObject.GetComponent<BoxCollider>().enabled = false;
        }
    }

    void Update()
    {
        if (!Settings.instance.isHovering)
        {
            bufferTimeRunning -= Time.deltaTime;
            if (!Input.GetMouseButton(1) && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftControl)) Place();
            if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.changeNoteType)) SwitchNoteType();

            if (placementIndex == 0) left = true;
            else if (placementIndex == 1) left = false;

            foreach (RawImage img in rawImages)
            {
                img.color = Color.white;
            }

            colors[0] = Settings.instance.config.mapping.colorSettings.leftNote;
            colors[1] = Settings.instance.config.mapping.colorSettings.rightNote;

            colors[0] = new Color(colors[0].r, colors[0].g, colors[0].b, 1);
            colors[1] = new Color(colors[1].r, colors[1].g, colors[1].b, 1);

            rawImages[5].color = colors[0];
            rawImages[6].color = colors[1];

            rawImages[placementIndex].color = colors[placementIndex];

            if (Input.GetKey(KeyCode.LeftAlt)) AltControls();
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

        foreach (var h in hits)
        {
            if (h.collider.CompareTag("Grid"))
            {
                hit = h;
                gridFound = true;
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
                Vector3 pos = new Vector3(Mathf.RoundToInt(hit.point.x - 1.5f) - 0.5f, Mathf.RoundToInt(hit.point.y - 0.5f) + 0.5f, 0);
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
                Vector3 pos = new Vector3(Mathf.RoundToInt(hit.point.x - 1.5f) - 0.5f, Mathf.RoundToInt(hit.point.y - 0.5f) + 0.5f, 0);
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
    void DeleteObject()
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

        if (closestType != null && Input.GetMouseButtonDown(0))
        {
            if (closestType.CompareTag("Note"))
            {
                colorNotes note = closestType.GetComponent<NoteData>().note;
                List<colorNotes> itemsToRemove = new List<colorNotes>();

                foreach (var item in LoadMap.instance.beats[Mathf.FloorToInt(note.b)].colorNotes)
                {
                    if (item == note)
                    {
                        itemsToRemove.Add(item);
                    }
                }

                foreach (var item in itemsToRemove)
                {
                    LoadMap.instance.beats[Mathf.FloorToInt(note.b)].colorNotes.Remove(item);
                }
            }
            else if (closestType.CompareTag("Bomb"))
            {
                bombNotes bomb = closestType.GetComponent<BombData>().bomb;
                List<bombNotes> itemsToRemove = new List<bombNotes>();

                foreach (var item in LoadMap.instance.beats[Mathf.FloorToInt(bomb.b)].bombNotes)
                {
                    if (item == bomb)
                    {
                        itemsToRemove.Add(item);
                    }
                }

                foreach (var item in itemsToRemove)
                {
                    LoadMap.instance.beats[Mathf.FloorToInt(bomb.b)].bombNotes.Remove(item);
                }
            }
            else if (closestType.CompareTag("Obstacle"))
            {
                obstacles obstacle = closestType.GetComponent<ObstacleData>().obstacle;
                List<obstacles> itemsToRemove = new List<obstacles>();

                foreach (var item in LoadMap.instance.beats[Mathf.FloorToInt(obstacle.b)].obstacles)
                {
                    if (item == obstacle)
                    {
                        itemsToRemove.Add(item);
                    }
                }

                foreach (var item in itemsToRemove)
                {
                    LoadMap.instance.beats[Mathf.FloorToInt(obstacle.b)].obstacles.Remove(item);
                }
            }
            else if (closestType.CompareTag("BpmEvent"))
            {
                bpmEvents bpmEvents = closestType.GetComponent<BpmEventData>().bpmEvent;
                List<bpmEvents> itemsToRemove = new List<bpmEvents>();

                foreach (var item in LoadMap.instance.beats[Mathf.FloorToInt(bpmEvents.b)].bpmEvents)
                {
                    if (item == bpmEvents)
                    {
                        itemsToRemove.Add(item);
                    }
                }

                foreach (var item in itemsToRemove)
                {
                    LoadMap.instance.beats[Mathf.FloorToInt(bpmEvents.b)].bpmEvents.Remove(item);
                }

                foreach (var item in LoadMap.instance.bpmEvents)
                {
                    if (item == bpmEvents)
                    {
                        LoadMap.instance.bpmEvents.Remove(item);
                    }
                }
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
                    if (Input.mouseScrollDelta.y > 0) obstacle.d += SpawnObjects.instance.precision;
                    else obstacle.d -= SpawnObjects.instance.precision;
                }

                // Reload objects after deletion
                SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (hitObject.CompareTag("Note"))
                    altMoveCache = hitObject.GetComponent<NoteData>().note;
            }
            if (Input.GetMouseButtonUp(0)) altMoveCache = null;

            if (Input.GetMouseButton(0) && altMoveCache != null)
            {
                RaycastHit[] hits = Physics.RaycastAll(ray);

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

                            Debug.Log(angle);

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

        // lastMousePosition = mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (var h in hits)
        {
            if (h.collider.CompareTag("Note"))
            {
                colorNotes note = h.collider.gameObject.GetComponent<NoteData>().note;
                foreach (var item in LoadMap.instance.beats[Mathf.FloorToInt(note.b)].colorNotes)
                {
                    if (item == note)
                    {
                        if (item.c == 0) item.c = 1;
                        else item.c = 0;
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
        note.x = Mathf.FloorToInt(x + 2);
        note.y = Mathf.FloorToInt(y);
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
            LoadMap.instance.beats[Mathf.FloorToInt(b)].colorNotes.Add(note);
        }

        SpawnObjects.instance.LoadObjectsFromScratch(b, true, true);
    }

    void PlaceBomb(float b, float x, float y)
    {
        bool stackedCheck = false;
        bombNotes bomb = new bombNotes();

        bomb.b = b;
        bomb.x = Mathf.FloorToInt(x + 2);
        bomb.y = Mathf.FloorToInt(y);

        if (!KeybindManager.instance.AreAllKeysHeld(Settings.instance.config.keybinds.allowFusedNotePlacement))
        {
            foreach (var item in LoadMap.instance.beats[Mathf.FloorToInt(b)].colorNotes)
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
            LoadMap.instance.beats[Mathf.FloorToInt(b)].bombNotes.Add(bomb);
        }

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

    public void switchType(int index)
    {
        placementIndex = index;
    }
}


// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Placement : MonoBehaviour
// {
//     public Transform objectParent;
//     public int placementIndex;
//     public float angle;
//     public bool invertControls;
//     public Vector2 cachedDirection;
//     public Vector2 cachedDirection2;
//     private bool dot;
//     public List<GameObject> objects;
//     private bool isTransitioning;
//     public float transitionDelay = 0.125f; // Adjust the delay time as needed

//     private Vector3 lastMousePosition;
//     private Vector3 hitPos;
//     private bool cacheNoteType;
//     public bool diag;
//     private bool isPlacingWall;

//     public void changePlacementObject(int i)
//     {
//         if (i == 0)
//         {
//             if (cacheNoteType == true) placementIndex = 1;
//             else placementIndex = 0;
//         }
//         else placementIndex = i;
//     }

//     void Start()
//     {
//         foreach (GameObject item in objects)
//         {
//             GameObject go = Instantiate(item);
//             go.transform.SetParent(objectParent);
//         }
//     }

//     public void Update()
//     {
//         StartCoroutine(HandleTransitionDelay());
//     }

//     IEnumerator HandleTransitionDelay()
//     {
//         Vector3 mousePosition = Input.mousePosition;

//         lastMousePosition = mousePosition;
//         Ray ray = Camera.main.ScreenPointToRay(mousePosition);

//         if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.quickSelectRedNote)) placementIndex = 0;
//         if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.quickSelectBlueNote)) placementIndex = 1;
//         if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.quickSelectBomb)) placementIndex = 2;
//         if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.quickSelectWall)) placementIndex = 3;

//         RaycastHit[] hits = Physics.RaycastAll(ray);
//         RaycastHit hit = new RaycastHit();
//         bool gridFound = false;
//         RaycastHit noteRayCast = new RaycastHit();

//         foreach (var h in hits)
//         {
//             if (h.collider.CompareTag("Grid"))
//             {
//                 hit = h;
//                 gridFound = true;
//                 break;
//             }
//             else if (h.collider.CompareTag("Note"))
//             {
//                 hit = h;
//                 noteRayCast = h;
//                 break;
//             }
//         }

//         if (gridFound)
//         {
//             foreach (Transform child in objectParent)
//             {
//                 child.gameObject.SetActive(false);
//             }

//             if (!Input.GetMouseButton(1))
//             {
//                 GameObject preview = objectParent.transform.GetChild(placementIndex).gameObject;
//                 preview.SetActive(true);

//                 if (placementIndex != 3 && placementIndex != 4)
//                 {
//                     preview.transform.localPosition = new Vector3(Mathf.RoundToInt(hit.point.x - 1.5f) - 0.5f, Mathf.RoundToInt(hit.point.y - 0.5f) + 0.5f, 0);

//                     if (placementIndex != 2)
//                     {
//                         if (placementIndex == 1) cacheNoteType = true;

//                         Vector2 direction = new Vector2();

//                         if (!invertControls) direction = new Vector2(-Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
//                         else direction = new Vector2(Input.GetAxisRaw("Horizontal"), -Input.GetAxisRaw("Vertical"));

//                         if (Input.GetKeyDown(KeyCode.F))
//                         {
//                             dot = true;
//                             cachedDirection = direction;
//                         }

//                         if (direction.x != 0 && direction.y != 0)
//                         {
//                             diag = true;
//                             if (direction != Vector2.zero)
//                             {
//                                 float anglepi = Mathf.Atan2(direction.x, direction.y);
//                                 angle = anglepi * Mathf.Rad2Deg;
//                                 objectParent.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, angle);
//                                 objectParent.GetChild(1).transform.rotation = Quaternion.Euler(0, 0, angle);
//                             }
//                         }

//                         if (diag == true)
//                         {
//                             if (direction.x == 0 || direction.y == 0)
//                             {
//                                 yield return new WaitForSeconds(transitionDelay);

//                                 if (!invertControls) direction = new Vector2(-Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
//                                 else direction = new Vector2(Input.GetAxisRaw("Horizontal"), -Input.GetAxisRaw("Vertical"));

//                                 if (direction != Vector2.zero)
//                                 {
//                                     float anglepi = Mathf.Atan2(direction.x, direction.y);
//                                     angle = anglepi * Mathf.Rad2Deg;
//                                     objectParent.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, angle);
//                                     objectParent.GetChild(1).transform.rotation = Quaternion.Euler(0, 0, angle);
//                                 }
//                                 diag = false;
//                             }
//                         }
//                         else
//                         {
//                             if (direction != Vector2.zero)
//                             {
//                                 float anglepi = Mathf.Atan2(direction.x, direction.y);
//                                 angle = anglepi * Mathf.Rad2Deg;
//                                 objectParent.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, angle);
//                                 objectParent.GetChild(1).transform.rotation = Quaternion.Euler(0, 0, angle);
//                             }
//                         }

//                         if (!dot)
//                         {
//                             objectParent.GetChild(0).transform.GetChild(0).gameObject.SetActive(true);
//                             objectParent.GetChild(0).transform.GetChild(1).gameObject.SetActive(false);

//                             objectParent.GetChild(1).transform.GetChild(0).gameObject.SetActive(true);
//                             objectParent.GetChild(1).transform.GetChild(1).gameObject.SetActive(false);
//                         }
//                         else
//                         {
//                             objectParent.GetChild(0).transform.GetChild(0).gameObject.SetActive(false);
//                             objectParent.GetChild(0).transform.GetChild(1).gameObject.SetActive(true);
//                             objectParent.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, 0);

//                             objectParent.GetChild(1).transform.GetChild(0).gameObject.SetActive(false);
//                             objectParent.GetChild(1).transform.GetChild(1).gameObject.SetActive(true);
//                             objectParent.GetChild(1).transform.rotation = Quaternion.Euler(0, 0, 0);
//                         }

//                         if (cachedDirection != direction) dot = false;

//                         if (Input.GetMouseButtonDown(0)) PlaceNote();
//                     }
//                     else
//                     {
//                         if (Input.GetMouseButtonDown(0)) PlaceBomb();
//                     }
//                 }
//                 else
//                 {
//                     bool crouch = false;
//                     if (hit.point.y > 1.49f && !isPlacingWall)
//                     {
//                         preview.transform.localPosition = new Vector3(Mathf.RoundToInt(hit.point.x - 1.5f) - 0.5f, 3f, 0);
//                         preview.transform.localScale = new Vector3(1, 3, 0.125f);
//                         crouch = true;
//                     }
//                     else if (!isPlacingWall)
//                     {
//                         preview.transform.localPosition = new Vector3(Mathf.RoundToInt(hit.point.x - 1.5f) - 0.5f, 2.25f, 0);
//                         preview.transform.localScale = new Vector3(1, 4.5f, 0.125f);
//                     }

//                     if (Input.GetMouseButtonDown(0))
//                     {
//                         StartCoroutine(PlaceWall(crouch));
//                         Debug.Log("something should happen");
//                     }
//                 }
//             }
//         }
//         else
//         {
//             foreach (Transform child in objectParent)
//             {
//                 child.gameObject.SetActive(false);
//             }
//         }

//         if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.changeNoteType))
//         {
//             if (noteRayCast.collider != null)
//             {
//                 colorNotes noteHit = noteRayCast.collider.gameObject.GetComponent<NoteData>().note;
//                 colorNotes oldNote = new colorNotes();

//                 for (int i = 0; i < LoadMap.instance.beats[Mathf.FloorToInt(noteHit.b)].colorNotes.Count; i++)
//                 {
//                     if (LoadMap.instance.beats[Mathf.FloorToInt(noteHit.b)].colorNotes[i] == noteHit)
//                     {
//                         oldNote = LoadMap.instance.beats[Mathf.FloorToInt(noteHit.b)].colorNotes[i];
//                         break;
//                     }
//                 }

//                 if (oldNote.c == 0) oldNote.c = 1;
//                 else oldNote.c = 0;

//                 SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, false);
//             }
//         }
//     }


//     IEnumerator PlaceWall(bool crouch)
//     {
//         GameObject preview = objectParent.transform.GetChild(placementIndex).gameObject;
//         Vector3 mousePosition = Input.mousePosition;
//         Ray ray = Camera.main.ScreenPointToRay(mousePosition);

//         RaycastHit[] hits = Physics.RaycastAll(ray);
//         RaycastHit hit = new RaycastHit();
//         bool gridFound = false;

//         foreach (var h in hits)
//         {
//             if (h.collider.CompareTag("Grid"))
//             {
//                 hit = h;
//                 gridFound = true;
//                 break;
//             }
//         }

//         isPlacingWall = true;
//         float beat = SpawnObjects.instance.currentBeat;

//         if (gridFound)
//         {
//             Vector3 startPos = new Vector3(Mathf.RoundToInt(hit.point.x - 0.5f) + 0.5f, Mathf.RoundToInt(hit.point.y - 0.5f) + 0.5f, SpawnObjects.instance.PositionFromBeat(SpawnObjects.instance.currentBeat) * SpawnObjects.instance.editorScale);
//             Vector3 endPos = new Vector3();

//             bool click = false;

//             while (!Input.GetMouseButton(0))
//             {
//                 yield return null;
//             }

//             while (!click)
//             {
//                 mousePosition = Input.mousePosition;
//                 ray = Camera.main.ScreenPointToRay(mousePosition);
//                 if (Physics.Raycast(ray, out RaycastHit hit2) && hit.collider.CompareTag("Grid"))
//                 {
//                     endPos = new Vector3(Mathf.RoundToInt(hit2.point.x - 0.5f) + 0.5f, Mathf.RoundToInt(hit2.point.y - 0.5f) + 0.5f, SpawnObjects.instance.PositionFromBeat(SpawnObjects.instance.currentBeat) * SpawnObjects.instance.editorScale);

//                     if (Input.GetKeyDown(KeyCode.I)) click = true;
//                     Debug.Log(startPos + " - " + endPos);

//                     int positive = 1;
//                     if (endPos.x - startPos.x < 0) positive = -1;

//                     if (hit2.point.y < 1.49f)
//                     {
//                         preview.transform.position = new Vector3(startPos.x + (endPos.x - startPos.x) / 2, 2.25f, startPos.z + (endPos.z - startPos.z) / 2);
//                         preview.transform.localScale = new Vector3(endPos.x - startPos.x + positive, 4.5f, endPos.z - startPos.z);
//                     }
//                     else
//                     {
//                         preview.transform.position = new Vector3(startPos.x + (endPos.x - startPos.x) / 2, 3, startPos.z + (endPos.z - startPos.z) / 2);
//                         preview.transform.localScale = new Vector3(endPos.x - startPos.x + positive, 3, endPos.z - startPos.z);
//                     }
//                 }
//                 yield return null;
//             }

//             bool stackedCheck = false;
//             obstacles wall = new obstacles();
//             wall.b = beat;
//             wall.x = Mathf.RoundToInt(preview.transform.position.x - preview.transform.localScale.x / 2);
//             wall.y = Mathf.RoundToInt(preview.transform.position.y - preview.transform.localScale.y / 2);
//             wall.d = SpawnObjects.instance.currentBeat - beat;
//             wall.w = Mathf.RoundToInt(preview.transform.localScale.x);
//             wall.h = Mathf.RoundToInt(preview.transform.localScale.y);

//             foreach (var item in LoadMap.instance.beats[Mathf.FloorToInt(beat)].colorNotes)
//             {
//                 if (item.x == wall.x && item.y == wall.y && item.b == wall.b)
//                 {
//                     stackedCheck = true;
//                     break;
//                 }
//             }

//             if (!stackedCheck)
//             {
//                 LoadMap.instance.beats[Mathf.FloorToInt(beat)].obstacles.Add(wall);
//                 SpawnObjects.instance.LoadObjectsFromScratch(Mathf.FloorToInt(beat), false, false);
//             }
//         }

//         isPlacingWall = false;
//         yield break;
//     }


//     void PlaceNote()
//     {
//         Vector3 mousePosition = Input.mousePosition;
//         Ray ray = Camera.main.ScreenPointToRay(mousePosition);

//         RaycastHit[] hits = Physics.RaycastAll(ray);
//         RaycastHit hit = new RaycastHit();
//         bool gridFound = false;

//         foreach (var h in hits)
//         {
//             if (h.collider.CompareTag("Grid"))
//             {
//                 hit = h;
//                 gridFound = true;
//                 break;
//             }
//         }

//         if (gridFound)
//         {
//             Vector3 pos = new Vector3(Mathf.RoundToInt(hit.point.x - 0.5f) + 0.5f, Mathf.RoundToInt(hit.point.y - 0.5f) + 0.5f, 0);

//             float beat = SpawnObjects.instance.currentBeat;
//             bool stackedCheck = false;
//             colorNotes note = new colorNotes();
//             note.b = beat;
//             note.x = Mathf.FloorToInt(pos.x);
//             note.y = Mathf.FloorToInt(pos.y);
//             note.c = placementIndex;
//             note.d = dot ? 8 : Rotation((int)angle);

//             foreach (var item in LoadMap.instance.beats[Mathf.FloorToInt(beat)].colorNotes)
//             {
//                 if (item.x == note.x && item.y == note.y && item.b == note.b)
//                 {
//                     stackedCheck = true;
//                     break;
//                 }
//             }

//             if (!stackedCheck)
//             {
//                 LoadMap.instance.beats[Mathf.FloorToInt(beat)].colorNotes.Add(note);
//                 SpawnObjects.instance.LoadObjectsFromScratch(Mathf.FloorToInt(beat), true, true);
//             }
//         }
//     }

//     void PlaceBomb()
//     {
//         Vector3 mousePosition = Input.mousePosition;
//         Ray ray = Camera.main.ScreenPointToRay(mousePosition);

//         RaycastHit[] hits = Physics.RaycastAll(ray);
//         RaycastHit hit = new RaycastHit();
//         bool gridFound = false;

//         foreach (var h in hits)
//         {
//             if (h.collider.CompareTag("Grid"))
//             {
//                 hit = h;
//                 gridFound = true;
//                 break;
//             }
//         }

//         if (gridFound)
//         {
//             Vector3 pos = new Vector3(Mathf.RoundToInt(hit.point.x - 0.5f) + 0.5f, Mathf.RoundToInt(hit.point.y - 0.5f) + 0.5f, 0);

//             float beat = SpawnObjects.instance.currentBeat;
//             bool stackedCheck = false;
//             bombNotes bomb = new bombNotes();
//             bomb.b = beat;
//             bomb.x = Mathf.FloorToInt(pos.x);
//             bomb.y = Mathf.FloorToInt(pos.y);

//             foreach (var item in LoadMap.instance.beats[Mathf.FloorToInt(beat)].colorNotes)
//             {
//                 if (item.x == bomb.x && item.y == bomb.y && item.b == bomb.b)
//                 {
//                     stackedCheck = true;
//                     break;
//                 }
//             }

//             if (!stackedCheck)
//             {
//                 LoadMap.instance.beats[Mathf.FloorToInt(beat)].bombNotes.Add(bomb);
//                 SpawnObjects.instance.LoadObjectsFromScratch(Mathf.FloorToInt(beat), true, true);
//             }
//         }
//     }

//     int Rotation(int level)
//     {
//         return level switch
//         {
//             180 => 0,
//             0 => 1,
//             -90 => 2,
//             90 => 3,
//             -135 => 4,
//             135 => 5,
//             -45 => 6,
//             45 => 7,
//             _ => 0
//         };
//     }

//     public void switchType(int index)
//     {
//         placementIndex = index;
//     }
// }