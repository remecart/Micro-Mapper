using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

public class ExplorerManager : MonoBehaviour
{
    public void OpenExplorerAtPath()
    {
        var path = EditMetaData.instance.folderPath;
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer",
            Arguments = $"\"{path}\"",
            UseShellExecute = true
        });
    }
    
    public static void CreateZipInFolder()
    {
        var path = EditMetaData.instance.folderPath;
        
        // Ensure the directory exists
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Directory not found: {path}");
        }

        // Create a temporary zip file path
        string tempZipFilePath = Path.Combine(Path.GetTempPath(), EditMetaData.instance.metaData._songName, ".zip");

        // If the temp zip file already exists, delete it
        if (File.Exists(tempZipFilePath))
        {
            File.Delete(tempZipFilePath);
        }

        // Create the zip file in the temp location
        ZipFile.CreateFromDirectory(path, tempZipFilePath);

        // Move the zip file into the target directory
        string finalZipFilePath = Path.Combine(path, EditMetaData.instance.metaData._songName, ".zip");
        if (File.Exists(finalZipFilePath))
        {
            File.Delete(finalZipFilePath);
        }
        File.Move(tempZipFilePath, finalZipFilePath);
    }
}