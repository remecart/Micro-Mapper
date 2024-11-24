using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchBetweenSettingPages : MonoBehaviour
{
    public List<GameObject> pages;

    public void SwitchPage(int index)
    {
        foreach (GameObject page in pages)
        {
            page.SetActive(false);
        }
        pages[index].SetActive(true);
    }
}
