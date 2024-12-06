using System;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class FolderPath : MonoBehaviour
{
    public string path;

    // Static flag to ensure arguments are processed only once
    private static bool argumentsProcessed = false;

    private void Start()
    {
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