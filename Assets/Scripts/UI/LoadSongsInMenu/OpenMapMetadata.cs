using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenMapMetadata : MonoBehaviour
{
    public void OpenMap()
    {
        GameObject.FindWithTag("FolderPath").GetComponent<FolderPath>().path = this.gameObject.GetComponent<LoadMapSelectionPreview>().folderPath;
        SceneManager.LoadScene("EditInfo");
    }
}
