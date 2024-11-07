using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreviewMode : MonoBehaviour
{
    public static PreviewMode instance;
    public List<GameObject> gameObjects;
    public bool Enabled = true;
    public RawImage img;
    public List<Texture2D> textures;
    public PlayerFly fly;
    public Vector3 staticCamPos;
    private Vector3 cachePos;
    private float cacheXRot;
    private float cacheYRot;

    public void TogglePreview()
    {
        Toggle();
    }

    // Update is called once per frame
    void Update()
    {
        instance = this;
        if (KeybindManager.instance.AreAllKeysPressed(Settings.instance.config.keybinds.previewMap)) {
            Toggle();
        }

    }

    private void Toggle()
    {
        int a = -100000;
        if (Enabled) a = 100000;
        foreach (var item in gameObjects)
        {
            item.transform.localPosition = new Vector3(item.transform.localPosition.x, item.transform.localPosition.y + a, item.transform.localPosition.z);
        }

        if (Enabled)
        {
            cachePos = fly.gameObject.transform.parent.position;
            cacheXRot = fly.transform.eulerAngles.x;
            cacheYRot = fly.transform.parent.eulerAngles.x;
            fly.transform.parent.position = staticCamPos;
            fly.transform.parent.eulerAngles = new Vector3(0, 0, 0);
            fly.transform.eulerAngles = new Vector3(0, 0, 0);
            img.texture = textures[0];
            fly.enabled = false;
        }
        else
        {
            fly.transform.parent.position = cachePos;
            fly.transform.parent.eulerAngles = new Vector3(0, cacheYRot, 0);
            fly.transform.eulerAngles = new Vector3(cacheXRot,0,0);
            img.texture = textures[1];
            fly.enabled = true;
        }

        Enabled = !!!Enabled; // They used to be friends, until they were....
    }
}
