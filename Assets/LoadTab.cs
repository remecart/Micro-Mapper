using UnityEngine;

public class LoadTab : MonoBehaviour
{
    public int index;

    public void LoadSong()
    {
        LoadMapsFromPath.instance.LoadMapFolders(index);
    }
}
