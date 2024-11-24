using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transparent : MonoBehaviour
{
    public int type;
    private Transform grid;
    public List<Material> material;
    public bool updateColor;
    private bool getAllMaterials;
    public Texture2D tex;
    public Texture2D tex2;

    void Start()
    {
        grid = GameObject.FindWithTag("Map").transform.GetChild(1);

        material[0] = GetComponent<MeshRenderer>().material;
        if (updateColor)
        {
            material[1] = transform.parent.GetChild(0).GetComponent<MeshRenderer>().material;
            material[2] = transform.parent.GetChild(1).GetComponent<MeshRenderer>().material;
            material[3] = transform.parent.GetChild(3).GetComponent<MeshRenderer>().material;
        }

        SetTransparent();
    }

    // Update is called once per frame
    void Update()
    {
        if (updateColor && material[0] && material[1] && material[2]) getAllMaterials = true;
        if (updateColor) GetColor();
        SetTransparent();
    }

    void SetTransparent()
    {
        if (transform.position.z < grid.position.z)
        {
            foreach (var item in material)
            {
                item.SetFloat("_Transparent", 1f);
            }
        }
        else
        {
            foreach (var item in material)
            {
                if (PreviewMode.instance != null && !PreviewMode.instance.Enabled) item.SetTexture("_ScreenspaceTexture", tex);
                else item.SetTexture("_ScreenspaceTexture", tex2);
                item.SetFloat("_Transparent", 0f);
            }
        }
    }

    public void GetColor()
    {
        if (type == 0)
        {
            material[0].SetColor("_NoteColor", Settings.instance.config.mapping.colorSettings.leftNote);
            material[1].SetColor("_NoteColor", Settings.instance.config.mapping.colorSettings.leftNoteArrow);
            material[2].SetColor("_NoteColor", Settings.instance.config.mapping.colorSettings.leftNoteArrow);
        }
        else
        {
            material[0].SetColor("_NoteColor", Settings.instance.config.mapping.colorSettings.rightNote);
            material[1].SetColor("_NoteColor", Settings.instance.config.mapping.colorSettings.rightNoteArrow);
            material[2].SetColor("_NoteColor", Settings.instance.config.mapping.colorSettings.rightNoteArrow);
        }
    }
}
