using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class WIPFolderMenu : MonoBehaviour
{
    public LoadMapsFromPath loadMapsFromPath;
    public void Abort()
    {
        this.gameObject.SetActive(false);
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
