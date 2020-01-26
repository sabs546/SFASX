using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureScroll : MonoBehaviour
{
    public float scrollX;
    public float scrollY;
    Material[] mat;
    Material mat1;
    // Start is called before the first frame update
    void Start()
    {
        if (GetComponent<Renderer>() != null)
            mat = GetComponent<Renderer>().materials;
        else
            mat1 = GetComponent<Image>().material;
    }

    // Update is called once per frame
    void Update()
    {
        float offsetX = Time.time * scrollX;
        float offsetY = Time.time * scrollY;
        if (mat1 == null)
            mat[mat.Length - 1].SetTextureOffset(1, new Vector2(offsetX, offsetY));
        else
            mat1.SetTextureOffset(1, new Vector2(offsetX, offsetY));
    }
}
