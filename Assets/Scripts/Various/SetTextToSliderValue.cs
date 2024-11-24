using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SetTextToSliderValue : MonoBehaviour
{
    // This method will be called when the slider value changes
    public void ChangeText(Slider slider)
    {
        // Update the text with the slider's value
        this.GetComponent<TextMeshProUGUI>().text = slider.value.ToString("0.00");
    }
}
