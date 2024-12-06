using UnityEngine;
using UnityEngine.EventSystems;

public class LoadTab : MonoBehaviour, IPointerClickHandler
{
    public int index;

    private void LoadSongList()
    {
        LoadMapsFromPath.instance.LoadMapFolders(index);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            LoadSongList();
        }
        else if (eventData.button == PointerEventData.InputButton.Middle)
        {
            Settings.instance.config.general.folderPaths.Remove(LoadMapsFromPath.instance.paths[index]);
            LoadMapsFromPath.instance.paths = Settings.instance.config.general.folderPaths;
            LoadMapsFromPath.instance.LoadMapFolders(0);
        }
    }
}
