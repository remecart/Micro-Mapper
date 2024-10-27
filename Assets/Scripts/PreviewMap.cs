using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewMap : MonoBehaviour
{
    public static PreviewMap instance;
    public bool preview;
    public Transform content;
    public Transform grid;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.previewMap))
        {
            preview = !preview;
            if (preview == false)
            {
                SpawnObjects.instance.editorScale = Settings.instance.config.mapping.editorScale;
            }
            SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, false);
        }

        if (preview)
        {
            SpawnObjects.instance.editorScale = ReadMapInfo.instance.difficulty._noteJumpMovementSpeed;
        }

        foreach (Transform child in content)
        {
            if (child.CompareTag("NoteGravity"))
            {
                // Get the NoteData component once
                NoteData noteData = child.GetComponent<NoteData>();

                // Calculate start and end positions based on the note's timing
                float startPos = SpawnObjects.instance.PositionFromBeat(noteData.note.d + ReadMapInfo.instance.jumpDuration) * ReadMapInfo.instance.difficulty._noteJumpMovementSpeed;
                float endPos = SpawnObjects.instance.PositionFromBeat(noteData.note.d) * ReadMapInfo.instance.difficulty._noteJumpMovementSpeed;

                // Check if the child's current position is between the start and end positions
                if (grid.position.z < startPos && grid.position.z > endPos)
                {
                    // Calculate the duration and the current position relative to the end position
                    float duration = startPos - endPos;
                    float currentPos = grid.position.z - endPos;

                    // Update the child's position to stretch the jump
                    if (duration != 0)
                    {
                        child.position = new Vector3(child.position.x, (currentPos / duration) * noteData.note.y, child.position.z);
                    }
                }
            }
        }

    }
}
