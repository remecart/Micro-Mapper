
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class LoadMapsFromPath : MonoBehaviour
{
    public static LoadMapsFromPath instance;
    public List<string> paths;
    public int index;
    public GameObject mapPrefab;
    public Transform scroll;
    public TMP_InputField input;
    public GameObject addPath;
    public GameObject newMap;
    public void Start()
    {
        instance = this;
        paths = Settings.instance.config.general.folderPaths;
        
        ReloadTabs.instance.Tabs();
        
        foreach (var item in paths)
        {
            if (Directory.Exists(item))
            {
                LoadMapFolders(index);
                return;
            }
        }

        OpenPathMenu();
    }

    public void OpenPathMenu(TextMeshProUGUI tmp = null)
    {
        addPath.transform.GetChild(1).GetChild(2).gameObject.SetActive(false);
        addPath.SetActive(true);

        if (tmp != null) tmp.text = tmp.text.Replace("WIP", "another");
    }
    
    public void OpenNewMapMenu()
    {
        newMap.SetActive(true);
    }

    public void SearchForMapByName()
    {
        string search = input.text;
        int count = 0;

        foreach (Transform child in scroll.transform)
        {
            if (child.name.ToLower().Contains(search.ToLower()) || search == string.Empty)
            {
                child.gameObject.SetActive(true);
                count += 1;
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }

        scroll.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(950, Mathf.Clamp(count * 140, 700, float.MaxValue));
    }

    public void LoadMapFolders(int loadIndex)
    {
        paths = Settings.instance.config.general.folderPaths;
        foreach (Transform child in scroll.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        var directories = Directory.GetDirectories(paths[loadIndex]);
        var count = 0;

        for (var i = 0; i < directories.Length; i++)
        {
            var info = LoadInfo(directories[i]);
            if (info != null)
            {
                var map = Instantiate(mapPrefab, scroll);
                map.transform.SetParent(scroll);
                map.name = info._songName;
                var loadMap = map.GetComponent<LoadMapSelectionPreview>();
                if (loadMap != null)
                {
                    loadMap.folderPath = directories[i];
                    loadMap.time = i / 25f;
                    loadMap.info = info;   
                }

                count++;
            }
        }
        SearchForMapByName();
        scroll.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(950, Mathf.Clamp(count * 140, 700, float.MaxValue));
    }

    private Info LoadInfo(string path)
    {
        string infoPath = path.Contains("info.dat") ? "info.dat" : "Info.dat";
        if (File.Exists(Path.Combine(path, infoPath)))
        {
            string rawData = File.ReadAllText(Path.Combine(path, infoPath));
            return JsonUtility.FromJson<Info>(rawData);
        }
        return null;
    }
}
