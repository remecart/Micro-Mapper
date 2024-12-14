using System.Linq;
using TMPro;
using UnityEngine;

public class BpmMenu : MonoBehaviour
{
    public static BpmMenu instance;
    public GameObject bpmMenu;

    public bool menuOpen;

    public bool resetBeat;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Abort();
    }

    public void OpenMenu()
    {
        bpmMenu.SetActive(true);
        menuOpen = true;
    }

    public void Abort()
    {
        bpmMenu.SetActive(false);
        menuOpen = false;
    }

    public void Toggle()
    {
        resetBeat = !resetBeat;
    }

    public void CreateBpmChange()
    {
        var m = float.Parse(bpmMenu.transform.GetChild(2).GetComponent<TMP_InputField>().text);
        var b = SpawnObjects.instance.currentBeat;

        if (!resetBeat)
        {
            bool stackedCheck = false;
            bpmEvents bpmEvent = new bpmEvents();

            bpmEvent.b = b;
            bpmEvent.m = m;

            foreach (var item in LoadMap.instance.beats[Mathf.FloorToInt(b)].bpmEvents)
            {
                if (Mathf.Approximately(item.b, bpmEvent.b))
                {
                    stackedCheck = true;
                    break;
                }
            }
            
            if (!stackedCheck)
            {
                UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(bpmEvent.b)],
                    Mathf.FloorToInt(bpmEvent.b), false);
                LoadMap.instance.beats[Mathf.FloorToInt(b)].bpmEvents.Add(bpmEvent);
                LoadMap.instance.bpmEvents.Add(bpmEvent);
                LoadMap.instance.bpmEvents = LoadMap.instance.bpmEvents.OrderBy(x => x.b).ToList();
            }
        }
        else
        {
            bpmEvents resetEvent = new bpmEvents();
            resetEvent.b = b;
            resetEvent.m = 9999;
            
            UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(resetEvent.b)],
                Mathf.FloorToInt(resetEvent.b), false);
            LoadMap.instance.beats[Mathf.FloorToInt(b)].bpmEvents.Add(resetEvent);
            LoadMap.instance.bpmEvents.Add(resetEvent);
            LoadMap.instance.bpmEvents = LoadMap.instance.bpmEvents.OrderBy(x => x.b).ToList();
            
            bpmEvents bpmEvent = new bpmEvents();
            bpmEvent.b = Mathf.CeilToInt(b);
            bpmEvent.m = m;
            
            UndoRedoManager.instance.SaveState(LoadMap.instance.beats[Mathf.FloorToInt(bpmEvent.b)],
                Mathf.FloorToInt(bpmEvent.b), false);
            LoadMap.instance.beats[Mathf.FloorToInt(b)].bpmEvents.Add(bpmEvent);
            LoadMap.instance.bpmEvents.Add(bpmEvent);
            LoadMap.instance.bpmEvents = LoadMap.instance.bpmEvents.OrderBy(x => x.b).ToList();
        }
        
        DrawLines.instance.DrawLinesWhenRequired();
        SpawnObjects.instance.LoadObjectsFromScratch(b + 4, false, true);
        SpawnObjects.instance.LoadObjectsFromScratch(b, true, true);
        
        Abort();
    }
}