using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSwapper : MonoBehaviour
{
    public Texture Image1;
    public Texture Image2;
    
    private RawImage currentImage;
    
    private void Start()
    {
        currentImage = this.GetComponent<RawImage>();
        this.GetComponent<Button>().onClick.AddListener(SwapImage);
    }

    private void SwapImage()
    {
        if (currentImage.texture == Image1)
        {
            currentImage.texture = Image2;
        }
        else
        {
            currentImage.texture = Image1;
        }
    }
    
}
