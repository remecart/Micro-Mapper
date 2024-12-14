using UnityEngine;
using TMPro;

public class PopUpText : MonoBehaviour
{
    public static PopUpText instance;
    public GameObject popUpText;

    void Start()
    {
        instance = this;
    }
    
    // Update is called once per frame
    public void Generate(string text)
    {
        GameObject popUp = Instantiate(popUpText, this.transform);
        popUp.transform.SetParent(this.transform);
        popUp.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -400);
        popUp.GetComponent<TextMeshProUGUI>().text = text;
    }
}
