using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
{
    public string text;
    public GameObject tooltipPrefab;
    private GameObject tooltip;
    public int offset = -40;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip == null)
        {
            tooltip = Instantiate(tooltipPrefab, new Vector3(transform.position.x, transform.position.y + offset, transform.position.z), Quaternion.identity);
            GameObject tooltipsParent = GameObject.FindWithTag("Tooltips");
            if (tooltipsParent != null)
            {
                tooltip.transform.SetParent(tooltipsParent.transform, true);
            }
            tooltip.GetComponentInChildren<TextMeshProUGUI>().text = text;
        }
        else
        {
            Destroy(tooltip);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            Destroy(tooltip);

        }
    }
    
}
