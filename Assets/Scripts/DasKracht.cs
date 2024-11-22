using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management

public class DasKracht : MonoBehaviour
{
    public static DasKracht instance;
    public GameObject obj;
    public string sceneName = "Krach"; // The name of the scene to switch to

    // The function to be executed
    public void TargetFunction()
    {
        // Switch to another scene after the check
        SceneManager.LoadScene(sceneName);
    }

    // Start is called before the first frame update
    public void Start()
    {
        instance = this;
    }
}
