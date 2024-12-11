using System;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class FolderPath : MonoBehaviour
{
    public string path;
    public static FolderPath instance;

    // Static flag to ensure arguments are processed only once
    private static bool argumentsProcessed = false;

    [Header("Map Settings")]
    public LoadMap._difficulty diff;
    public LoadMap._beatmapCharacteristicName beatchar;

    private void Start()
    {
        instance = this;
        
        if (argumentsProcessed)
            return;

        string[] arguments = Environment.GetCommandLineArgs();
        if (arguments.Length > 1 && !string.IsNullOrWhiteSpace(arguments[1]))
        {
            string argument = arguments[1];
            if (argument.Contains("Info.dat", StringComparison.OrdinalIgnoreCase))
            {
                path = Path.GetDirectoryName(argument);
                argumentsProcessed = true; // Set the flag to true
                SceneManager.LoadScene("EditInfo");
            }
            else if (argument.Contains(".dat", StringComparison.OrdinalIgnoreCase))
            {
                path = Path.GetDirectoryName(argument);
                argumentsProcessed = true; // Set the flag to true
                SceneManager.LoadScene("Editor");
            }
        }
    }
}