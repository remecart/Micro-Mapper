using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.SceneManagement;
public class NewMapFolderMenu : MonoBehaviour
{
    public LoadMapsFromPath loadMapsFromPath;
    public FolderPath folderPath;
    
    public void Abort()
    {
        this.gameObject.SetActive(false);
    }

    public void Confirm()
    {
        string songName = this.transform.GetChild(2).GetComponent<TMP_InputField>().text;

        var info = new Info
        {
            _songName = songName
        };

        var newPath = Path.Combine(loadMapsFromPath.paths[loadMapsFromPath.index], songName);
        Debug.Log(newPath);
        if (!Directory.Exists(newPath))
        {
            Directory.CreateDirectory(newPath);
            File.WriteAllText(newPath + "\\Info.dat", JsonUtility.ToJson(info, true));
            folderPath.path = newPath;
            SceneManager.LoadScene("EditInfo");
        }
        else
        {
            this.transform.GetChild(1).GetChild(2).gameObject.SetActive(true);
        }
    }
}
