using System;
using System.IO;
using TMPro;
using UnityEngine;
using SFB;

public class WIPFolderMenu : MonoBehaviour
{
    public LoadMapsFromPath loadMapsFromPath;
    public TMP_InputField input;

    public void Abort()
    {
        this.gameObject.SetActive(false);
    }

    public void PickDirectory()
    {
        // OpenFolderPanel allows the user to pick a directory
        string[] selectedFolders = StandaloneFileBrowser.OpenFolderPanel(
            "Select a Folder",
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            false
        );

        if (selectedFolders.Length > 0)
        {
            if (!string.IsNullOrEmpty(selectedFolders[0]))
            {
                input.text = selectedFolders[0];
            }
        }
    }


    public void Confirm()
    {
        string path = this.transform.GetChild(2).GetComponent<TMP_InputField>().text;

        if (Directory.Exists(path))
        {
            Settings.instance.config.general.folderPaths.Add(path);
            ReloadTabs.instance.Tabs();
            loadMapsFromPath.LoadMapFolders(loadMapsFromPath.paths.Count - 1);
            this.gameObject.SetActive(false);
        }
        else
        {
            this.transform.GetChild(1).GetChild(2).gameObject.SetActive(true);
        }
    }
}