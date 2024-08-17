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

    void Start()
    {
        grid = GameObject.FindWithTag("Map").transform.GetChild(1);

        material[0] = GetComponent<MeshRenderer>().material;
        material[1] = transform.parent.GetChild(0).GetComponent<MeshRenderer>().material;
        material[2] = transform.parent.GetChild(1).GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (material[0] && material[1] && material[2]) getAllMaterials = true;

        if (updateColor && getAllMaterials) GetColor();

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
