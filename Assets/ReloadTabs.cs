using TMPro;
using UnityEngine;
using System.IO;

public class ReloadTabs : MonoBehaviour
{
    public static ReloadTabs instance;
    public GameObject tabPrefab;
    public Transform content;
    public LoadMapsFromPath loadMaps;

    public void Start()
    {
        instance = this;
    }
    
    public void Tabs()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        for (var i = 0; i < loadMaps.paths.Count; i++)
        {
            var tab = Instantiate(tabPrefab, content);
            var tabName = Path.GetFileName(loadMaps.paths[i].TrimEnd());
            tab.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = tabName;
            tab.name = tabName;
            tab.GetComponent<LoadTab>().index = i;
        }
    }
}
