using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Colors : MonoBehaviour
{
    public static Colors instance;
    public List<RawImage> rawImage;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    public void ChangeColor(TMP_InputField hexRaw)
    {
        string hex = hexRaw.text;
        int index = int.Parse(hexRaw.gameObject.name);

        // Remove the hash at the start if it's there
        hex = hex.Replace("#", "");

        // Ensure the string is a valid length (either 6 or 8 characters)
        if (hex.Length != 6)
        {
            Debug.LogError("Invalid hex length. Must be 6 characters.");
            rawImage[index - 1].color = new Color32(1, 1, 1, 1);
            return;
        }
        byte a = 255; // default alpha value
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);


        if (index == 1)
        {
            Settings.instance.config.mapping.colorSettings.leftNote = new Color32(r, g, b, a);
            rawImage[4].color = new Color32(r, g, b, a);
        }
        if (index == 2) Settings.instance.config.mapping.colorSettings.leftNoteArrow = new Color32(r, g, b, a);
        if (index == 3)
        {
            Settings.instance.config.mapping.colorSettings.rightNote = new Color32(r, g, b, a);
            rawImage[5].color = new Color32(r, g, b, a);
        }
        if (index == 4) Settings.instance.config.mapping.colorSettings.rightNoteArrow = new Color32(r, g, b, a);

        rawImage[index - 1].color = new Color32(r, g, b, a);
    }

    public string UnityColorToHex(Color color)
    {
        Color32 color32 = color;
        return $"#{color32.r:X2}{color32.g:X2}{color32.b:X2}";
    }
}
