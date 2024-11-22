using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class UndoRedoManager : MonoBehaviour
{
    public static UndoRedoManager instance;
    private readonly Stack<beatsV3> undoStack = new Stack<beatsV3>();
    private readonly Stack<int> undoIndexStack = new Stack<int>();
    private readonly Stack<bool> undoBatch = new Stack<bool>();

    private readonly Stack<beatsV3> redoStack = new Stack<beatsV3>();
    private readonly Stack<int> redoIndexStack = new Stack<int>();
    private readonly Stack<bool> redoBatch = new Stack<bool>();

    private void Start()
    {
        instance = this;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z)) Undo();
        //if (Input.GetKeyDown(KeyCode.Z)) Redo();
    }

    public void SaveState(beatsV3 item, int index, bool batch)
    {
        undoStack.Push(CloneItem(item));
        undoIndexStack.Push(index);
        undoBatch.Push(batch);

        redoStack.Clear();
        redoIndexStack.Clear();
        redoBatch.Clear();
    }

    public void Undo()
    {
        bool batchCheck = true;

        while (batchCheck == true)
        {
            var (previousState, index, batch) = UndoList(LoadMap.instance.beats.Count > 0 ? LoadMap.instance.beats[0] : null);
            batchCheck = batch;
            if (index != -1)
            {
                LoadMap.instance.beats[index] = previousState;
            }
        }

        SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
    }

    public void Redo()
    {
        bool batchCheck = true;

        while (batchCheck == true)
        {
            var (nextState, index, batch) = RedoList(LoadMap.instance.beats.Count > 0 ? LoadMap.instance.beats[0] : null);
            batchCheck = batch;
            if (index != -1)
            {
                LoadMap.instance.beats[index] = nextState;
            }
        }

        SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
    }

    public (beatsV3, int, bool) UndoList(beatsV3 currentItem)
    {
        if (undoStack.Count == 0) return (currentItem, -1, false);

        redoStack.Push(CloneItem(currentItem));
        redoIndexStack.Push(undoIndexStack.Peek());
        redoBatch.Push(undoBatch.Peek());

        int lastIndex = undoIndexStack.Pop();
        bool batch = undoBatch.Pop();

        return (undoStack.Pop(), lastIndex, batch);
    }

    public (beatsV3, int, bool) RedoList(beatsV3 currentItem)
    {
        if (redoStack.Count == 0) return (currentItem, -1, false);

        undoStack.Push(CloneItem(currentItem));
        undoIndexStack.Push(redoIndexStack.Peek());
        undoBatch.Push(redoBatch.Peek());

        int lastIndex = redoIndexStack.Pop();
        bool batch = redoBatch.Pop();

        return (redoStack.Pop(), lastIndex, batch);
    }

    private beatsV3 CloneItem(beatsV3 item)
    {
        return JsonUtility.FromJson<beatsV3>(JsonUtility.ToJson(item));
    }
}
