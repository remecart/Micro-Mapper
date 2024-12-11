using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
    [System.Serializable]
    public class MenuItem
    {
        public string name;
        public Texture icon; // Change from Sprite to Texture
        public UnityEngine.Events.UnityEvent onClick;
    }

    public List<MenuItem> menuItems = new List<MenuItem>();
    public GameObject buttonPrefab;
    public float radius = 100f;

    void Start()
    {
        CreateMenu();
    }

    void CreateMenu()
    {
        if (buttonPrefab == null)
        {
            Debug.LogError("Button Prefab is not assigned!");
            return;
        }

        int itemCount = menuItems.Count;
        if (itemCount == 0) return;

        float angleStep = 360f / itemCount;
        float angle = 0f;

        for (int i = 0; i < itemCount; i++)
        {
            // Instantiate button
            GameObject button = Instantiate(buttonPrefab, transform);
            button.name = menuItems[i].name;

            // Set button position
            RectTransform buttonTransform = button.GetComponent<RectTransform>();
            Vector2 position = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius
            );
            buttonTransform.anchoredPosition = position;

            // Set button icon (using RawImage)
            RawImage buttonImage = button.GetComponent<RawImage>();
            if (buttonImage != null && menuItems[i].icon != null)
            {
                buttonImage.texture = menuItems[i].icon;
            }

            // Add button click event
            Button buttonComponent = button.GetComponent<Button>();
            if (buttonComponent != null && menuItems[i].onClick != null)
            {
                int index = i; // Prevent closure issue
                buttonComponent.onClick.AddListener(() => menuItems[index].onClick.Invoke());
            }

            // Increment angle
            angle += angleStep;
        }
    }
}
