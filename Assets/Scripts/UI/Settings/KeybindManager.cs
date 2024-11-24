using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeybindManager : MonoBehaviour
{
    public static KeybindManager instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    public bool AreAllKeysPressed(List<KeyCode> keybinds)
    {
        // Check if the list is not empty
        if (keybinds.Count == 0) return false;

        // Check all keys except the last one
        for (int i = 0; i < keybinds.Count - 1; i++)
        {
            if (!Input.GetKey(keybinds[i]))
            {
                return false;
            }
        }

        // Check the last key for a single press
        if (!Input.GetKeyDown(keybinds[keybinds.Count - 1]))
        {
            return false;
        }

        // If all keys are pressed, return true
        return true;
    }

    public bool AreAllKeysHeld(List<KeyCode> keybinds)
    {
        // Check all keys except the last one
        for (int i = 0; i < keybinds.Count; i++)
        {
            if (!Input.GetKey(keybinds[i]))
            {
                return false;
            }
        }

        // If all keys are pressed, return true
        return true;
    }
}
