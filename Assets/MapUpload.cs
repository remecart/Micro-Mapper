using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.IO;

public class MapUpload : MonoBehaviour
{
    public ExplorerManager expo;
    public string uploadUrl = "https://beatsaver.com/upload"; // Replace with your upload URL
    public string apiKey = "your_api_key"; // Replace with your API key if required

    public void UploadZipFile()
    {
        var filePath = expo.ZipPath;
        if (!File.Exists(filePath))
        {
            Debug.LogError("File does not exist at path: " + filePath);
            return;
        }

        // StartCoroutine(UploadFileCoroutine(filePath));
    }

    private IEnumerator UploadFileCoroutine(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", fileData, Path.GetFileName(filePath), "application/zip");

        UnityWebRequest www = UnityWebRequest.Post(uploadUrl, form);

        // Set timeout
        www.timeout = 30;

        // Add headers (if required)
        www.SetRequestHeader("Authorization", "Bearer " + apiKey);
        www.SetRequestHeader("Content-Type", "multipart/form-data");

        // Start the upload
        yield return www.SendWebRequest();

        // Handle the response
        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("File uploaded successfully: " + www.downloadHandler.text);
        }
        else
        {
            Debug.LogError($"File upload failed. Error: {www.error}\nDetails: {www.downloadHandler.text}");
        }
    }
}