using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadMapsFromPath : MonoBehaviour
{
    public List<string> paths;
    public int index;
    public GameObject MapPrefab;

    public void Start()
    {
        LoadMapFolders(index);
    }

    void LoadMapFolders(int index)
    {
        string[] directories = Directory.GetDirectories(paths[index]);

        for (int i = 0; i < directories.Length; i++)
        {
            GameObject map = Instantiate(MapPrefab); 
            map.name = directories[i].Replace(paths[index], "");
            map.GetComponent<LoadMapSelectionPreview>().folderPath = directories[i];
        }
    }
}
