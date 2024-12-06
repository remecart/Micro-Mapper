using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class LoadCover : MonoBehaviour
{
    public RawImage rawImage;
    public EditMetaData metaData;
    public bool applied;

    // Update is called once per frame
    void Update()
    {
        if (metaData.metaData != null && !applied)
        {
            Cover();
        }
    }

    public void Cover()
    {
        if (File.Exists(metaData.folderPath + "\\" + metaData.metaData._coverImageFilename))
        {
            byte[] imageData = File.ReadAllBytes(metaData.folderPath + "\\" + metaData.metaData._coverImageFilename);

            // Create a new Texture2D and load the image data
            Texture2D texture = new Texture2D(2, 2); // Adjust the size as needed
            texture.LoadImage(imageData);

            // Set the loaded texture to the RawImage component
            rawImage.texture = texture;
        }
        applied = true;
    }
}
