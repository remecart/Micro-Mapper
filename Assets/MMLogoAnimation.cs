using UnityEngine;
using UnityEngine.UI;

public class HueAnimator : MonoBehaviour
{
    public Material hueShiftMaterial;
    [Range(-1, 1)] public float hueShiftValue = 0f;

    void Update()
    {
        if (hueShiftMaterial != null)
        {
            hueShiftMaterial.SetFloat("_HueShift", hueShiftValue);
        }
    }
}
