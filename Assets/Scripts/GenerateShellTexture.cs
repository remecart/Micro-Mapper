using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateShellTexture : MonoBehaviour
{
    public Material shellMaterial;  // Assign the material using the ShellShader

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public void GenerateTexture(int i)
    {
        StartCoroutine(GenerateTextureCoroutine(i));
    }

    private IEnumerator GenerateTextureCoroutine(int i)
    {
        int layers = InstantiateSpectrogram.instance.layers;
        float height = InstantiateSpectrogram.instance.height;
        float distance = height / layers;

        if (i >= layers)
        {
            gameObject.SetActive(false);
        }
        else
        {
            transform.localPosition = new Vector3(0, 0, -((i + 1) * height / layers));
            float threshold = 1.0f / layers * i;

            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            Material material = meshRenderer.material;
            material.SetFloat("_Threshold", threshold);
            material.mainTexture = transform.parent.GetComponent<MeshRenderer>().material.mainTexture;

            yield return null;
        }
    }
}
