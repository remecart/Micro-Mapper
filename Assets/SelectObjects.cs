using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

//using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SelectObjects : MonoBehaviour
{
    public static SelectObjects instance;

    public List<colorNotes> selectedColorNotes = new List<colorNotes>();
    public List<bombNotes> selectedBombNotes = new List<bombNotes>();
    public List<bpmEvents> selectedBpmEvents = new List<bpmEvents>();
    public List<obstacles> selectedObstacles = new List<obstacles>();

    public List<Material> wallMaterials;

    public GameObject content;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {
            FindSelection();
        }
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.A))
        {
            ClearSelection();
        }

        if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace)) DeleteObjects();

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.UpArrow)) OffsetBeat(SpawnObjects.instance.precision);
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.DownArrow)) OffsetBeat(-SpawnObjects.instance.precision);
        if (Input.GetKeyDown(KeyCode.M)) MirrorSelection();
    }

    public void ClearSelection()
    {
        selectedColorNotes = new List<colorNotes>();
        selectedBombNotes = new List<bombNotes>();
        selectedBpmEvents = new List<bpmEvents>();
        selectedObstacles = new List<obstacles>();

        HighlightSelectedObject();
    }

    public void HighlightSelectedObject()
    {
        foreach (Transform itemTransform in content.transform)
        {
            GameObject item = itemTransform.gameObject;

            if (item.GetComponent<NoteData>())
            {
                bool check = selectedColorNotes.Find(n => n == item.GetComponent<NoteData>().note) != null;
                item.transform.GetChild(3).gameObject.SetActive(check);
            }
            if (item.GetComponent<BombData>())
            {
                bool check = selectedBombNotes.Find(n => n == item.GetComponent<BombData>().bomb) != null;
                item.transform.GetChild(1).gameObject.SetActive(check);
            }
            if (item.GetComponent<ObstacleData>())
            {
                bool check = selectedObstacles.Find(n => n == item.GetComponent<ObstacleData>().obstacle) != null;
                if (check)
                {
                    item.GetComponent<LineRenderer>().material = wallMaterials[0];
                }
                else
                {
                    item.GetComponent<LineRenderer>().material = wallMaterials[1];
                }
            }
            if (item.GetComponent<BpmEventData>())
            {
                bool check = selectedBpmEvents.Find(n => n == item.GetComponent<BpmEventData>().bpmEvent) != null;
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
