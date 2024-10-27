using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewMode : MonoBehaviour
{
    public List<GameObject> gameObjects;
    public bool Enabled = true;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J)) {
            int a = -100000;
            if (Enabled) a = 100000;
            foreach (var item in gameObjects)
            {
                item.transform.localPosition = new Vector3(item.transform.localPosition.x, item.transform.localPosition.y + a, item.transform.localPosition.z);
            }
            Enabled = !Enabled; // They used to be friends, until they were....
        }
    }
}
