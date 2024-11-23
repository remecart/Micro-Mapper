using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Menu : MonoBehaviour
{
    public static Menu instance;
    public bool open;
    private void Start()
    {
        instance = this;
    }

    private void Update()
    {
        open = this.gameObject.transform.GetChild(0).gameObject.activeSelf;
        if (Input.GetKeyDown(KeyCode.Escape) && !Bookmarks.instance.openMenu)
        {
            if (open) Continue();
            else OpenMenu();
        }
    }
    public void OpenMenu()
    {
        this.gameObject.transform.GetChild(0).gameObject.SetActive(true);
    }
    public void Continue()
    {
        this.gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }

    public void SaveExit()
    {
        LoadMap.instance.Save(true);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
