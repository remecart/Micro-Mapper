using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class Tooltip : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
{
    public string text;
    public GameObject tooltipPrefab;
    private GameObject tooltip;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip == null)
        {
            tooltip = Instantiate(tooltipPrefab, new Vector3(transform.position.x, transform.position.y - 40, transform.position.z), Quaternion.identity);
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
