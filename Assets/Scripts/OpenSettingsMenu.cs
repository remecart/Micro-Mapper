using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenSettingsMenu : MonoBehaviour
{
    public GameObject menu;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menu.SetActive(!menu.activeSelf);
        }
    }
}
