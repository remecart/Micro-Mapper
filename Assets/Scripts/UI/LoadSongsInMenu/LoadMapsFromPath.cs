
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

    public void SearchForMapByName()
    {
        string search = input.text;
        int count = 0;

        foreach (Transform child in scroll.transform)
        {
            if (child.GetChild(1).GetComponent<TextMeshProUGUI>().text.ToLower().Contains(search.ToLower()) || search == string.Empty)
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

        string[] directories = Directory.GetDirectories(paths[loadIndex]);

        for (int i = 0; i < directories.Length; i++)
        {
            var map = Instantiate(mapPrefab, scroll);
            map.transform.parent = scroll;
            map.name = directories[i].Replace(paths[loadIndex], "");
            map.GetComponent<LoadMapSelectionPreview>().folderPath = directories[i];
            map.GetComponent<LoadMapSelectionPreview>().time = i / 25f;
        }

        SearchForMapByName();
        scroll.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(950, Mathf.Clamp(directories.Length * 140, 700, float.MaxValue));
    }
}
