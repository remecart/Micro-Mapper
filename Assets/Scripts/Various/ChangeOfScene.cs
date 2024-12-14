
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class ChangeOfScene : MonoBehaviour
{
    public EditMetaData editMetaData;
    public LoadDifficultyCharacteristic loadDiffChar;
    
    public void ButtonPressed()
    {
        foreach (var t in editMetaData.metaData._difficultyBeatmapSets)
        {
            if (t._beatmapCharacteristicName == loadDiffChar.cacheCharacteristicName)
            {
                foreach (var d in t._difficultyBeatmaps.ToList())
                {
                    if (d._difficulty == loadDiffChar.cacheDifficultyName)
                    {
                        FolderPath.instance.path = editMetaData.folderPath;
                        
                        FolderPath.instance.diff = (LoadMap._difficulty)System.Enum.Parse(typeof(LoadMap._difficulty), d._difficulty);
                        FolderPath.instance.beatchar = (LoadMap._beatmapCharacteristicName)System.Enum.Parse(typeof(LoadMap._beatmapCharacteristicName), loadDiffChar.cacheCharacteristicName);
    
                        SceneManager.LoadScene("Editor");
                        return;
                    }
                }
            }
        }
        
        PopUpText.instance.Generate("Error: Cannot open map because no difficulty is selected!");
    }
}
