using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Zeltex.Editor
{
    /// <summary>
    /// A helper material shader setter script
    /// </summary>
    [ExecuteInEditMode]
    public class MaterialEditor : MonoBehaviour
    {
        public bool IsSetShaders;
        public bool IsGatherMaterials;
        public Transform MyTransform;
        public Shader MyShader;
        public Shader ReplaceWithShader;
        [Header("Data")]
        public List<Material> MyMaterials;
        public List<Shader> MyShaders;

        // Update is called once per frame
        void Update()
        {
            if (IsSetShaders)
            {
                IsSetShaders = false;
                for (int i = 0; i < MyMaterials.Count; i++)
                {
                    MyMaterials[i].shader = MyShader;
                }
            }

            if (IsGatherMaterials)
            {
                IsGatherMaterials = false;
                MyMaterials.Clear();
                MyShaders.Clear();
                MyMaterials = GatherMaterials(MyTransform, MyMaterials);
                for (int i = 0; i < MyMaterials.Count; i++)
                {
                    if (!MyShaders.Contains(MyMaterials[i].shader))
                    {
                        MyShaders.Add(MyMaterials[i].shader);
                    }
                }
            }
        }

        private List<Material> GatherMaterials(Transform MyTransform, List<Material> MyMaterials)
        {
            for (int i = 0; i < MyTransform.childCount; i++)
            {
                Transform Child = MyTransform.GetChild(i);
                RawImage MyRawImage = Child.gameObject.GetComponent<RawImage>();
                if (MyRawImage)
                {
                    if (!MyMaterials.Contains(MyRawImage.material))
                    {
                        MyMaterials.Add(MyRawImage.material);
                    }
                }
                Text MyText = Child.gameObject.GetComponent<Text>();
                if (MyText)
                {
                    if (!MyMaterials.Contains(MyText.material))
                    {
                        MyMaterials.Add(MyText.material);
                    }
                }
                Image MyImage = Child.gameObject.GetComponent<Image>();
                if (MyImage)
                {
                    if (!MyMaterials.Contains(MyImage.material))
                    {
                        MyMaterials.Add(MyImage.material);
                    }
                }
                GatherMaterials(Child, MyMaterials);
            }
            return MyMaterials;
        }
    }

}

