using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
public class MaterialUISpreader : MonoBehaviour
{
    public bool IsReplaceMaterial;
    public Material NewUIMaterial;

    void Update()
    {
        if (IsReplaceMaterial)
        {
            IsReplaceMaterial = false;
            FloodFillChildren(transform);
        }
    }

    void FloodFillChildren(Transform MyChild)
    {
        if (MyChild.GetComponent<RawImage>())
        {
            MyChild.GetComponent<RawImage>().material = NewUIMaterial;
        }
        if (MyChild.GetComponent<Text>())
        {
            MyChild.GetComponent<Text>().material = NewUIMaterial;
        }
        if (MyChild.GetComponent<Image>())
        {
            MyChild.GetComponent<Image>().material = NewUIMaterial;
        }
        for (int i = 0; i < MyChild.childCount; i++)
        {
            FloodFillChildren(MyChild.GetChild(i));
        }
    }
}
