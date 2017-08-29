using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Guis;

namespace Zeltex.AnimationUtilities
{
    /// <summary>
    /// Utility class for changing all gui colours
    /// </summary>
    [ExecuteInEditMode]
    public class GuiColorChanger : MonoBehaviour
    {
        #region Variables
        [Header("Options")]
        public Material MyMaterial; // material to give
        public ColorBlock MyColorBlock;
        public Color32 NewColor;
        public float NewOutlineSize = 1f;
        public Material MyCaretMaterial;
        [Header("TagMaterials")]
        public Material ButtonMaterial;
        public Material ButtonTextMaterial;
        public Material HeaderMaterial;
        public Material HeaderTextMaterial;
        [Header("Update")]
        public bool UpdateBackgroundColor;
        public bool UpdateBackgroundColorOutline;
        public bool UpdateLabelBackgroundColor;
        public bool UpdateLabelBackgroundColorOutline;
        public bool UpdateLabelTextColor;
        public bool UpdateLabelTextOutlineColor;
        [Header("Gather")]
        public bool Gather;
        public bool UpdateGathered;
        public bool GatherAll;
        public bool GatherBackgrounds;
        public bool GatherLabelBackgrounds;
        public string MyTag;
        [Header("Gathered")]
        public List<RawImage> Buttons;
        public List<Text> ButtonTexts;
        public List<RawImage> Backgrounds;
        public List<RawImage> LabelBackgrounds;
        public List<InputField> Inputs;
        [Header("Data")]
        public List<string> MyTags = new List<string>();
        #endregion

        #region Mono
        void Start()
        {
            MyTags.Clear();
            MyTags.Add("Button");
            MyTags.Add("ButtonText");
            MyTags.Add("Header");
            MyTags.Add("HeaderText");
            MyTags.Add("CloseButton");
            MyTags.Add("Background");
        }
        // Update is called once per frame
        void Update()
        {
            GatherData();
            UpdateData();
        }
        #endregion

        /*public List<Transform> GetChildrenOfName(string MyName)
        {
            List<Transform> MyTransforms = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform Child = transform.GetChild(i);
                for (int j = 0; j < Child.childCount; j++)
                {
                    Transform GrandChild = Child.GetChild(j);
                    if (!IsGatherFromChildrensChildren)
                    {
                        if (GrandChild.name == MyName)
                        {
                            MyTransforms.Add(GrandChild);
                        }
                    }
                    else
                    {
                        for (int k = 0; k < GrandChild.childCount; k++)
                        {
                            Transform GreatGrandChild = GrandChild.GetChild(k);
                            if (GreatGrandChild.name == MyName)
                            {
                                MyTransforms.Add(GreatGrandChild);
                            }
                        }
                    }
                }
            }
            return MyTransforms;
        }*/

        #region Gather
        private void GatherData()
        {
            if (GatherBackgrounds)
            {
                GatherBackgrounds = false;
                GatherRawImages(Backgrounds, "Background");
            }
            if (GatherLabelBackgrounds)
            {
                GatherLabelBackgrounds = false;
                GatherRawImages(LabelBackgrounds, "BackgroundLabel");
            }
            if (GatherAll)
            {
                GatherAll = false;
                GatherRawImages(Backgrounds, "Background");
                GatherRawImages(Buttons, "Button");
                GatherTexts(ButtonTexts, "ButtonText");
            }
            if (Gather)
            {
                Gather = false;
                if (MyTag == "ButtonText")
                {
                    GatherTexts(ButtonTexts, "ButtonText");
                }
                if (MyTag == "Button")
                {
                    GatherRawImages(Buttons, "Button");
                }
                if (MyTag == "Background")
                {
                    GatherRawImages(Buttons, "Background");
                }
                if (MyTag == "Input")
                {
                    Inputs.Clear();
                    List<GameObject> MyGameObjects = HardFindGameObjects("InputField");
                    Debug.LogError("Found: " + MyGameObjects.Count + " objects!");
                    for (int i = 0; i < MyGameObjects.Count; i++)
                    {
                        Inputs.Add(MyGameObjects[i].GetComponent<InputField>());
                    }
                    //Inputs.AddRange();//GameObject.FindObjectsOfType<InputField>());
                    for (int i = Inputs.Count - 1; i >= 0; i--)
                    {
                        InputFieldMaterialUpdater MyInputThing = Inputs[i].gameObject.GetComponent<InputFieldMaterialUpdater>();
                        if (MyInputThing  == null)
                        {
                            MyInputThing = Inputs[i].gameObject.AddComponent<InputFieldMaterialUpdater>();
                        }
                        MyInputThing.MyCaretMaterial = MyCaretMaterial;
                        //if (Inputs[i].gameObject.tag != "Input")
                        {
                            //Inputs.RemoveAt(i);
                        }
                    }
                }
            }
            if (UpdateGathered)
            {
                UpdateGathered = false;
                if (MyTag == "ButtonText")
                {
                    UpdateTexts(ButtonTexts);
                }
                if (MyTag == "Button")
                {
                    UpdateImages(Buttons);
                }
            }
        }
        private void UpdateImages(List<RawImage> MyList)
        {
            for (int i = 0; i < MyList.Count; i++)
            {
                MyList[i].material = MyMaterial;
                if (MyList[i].GetComponent<Button>())
                {
                    MyList[i].GetComponent<Button>().colors = MyColorBlock;
                }
            }
        }
        private void UpdateTexts(List<Text> MyList)
        {
            for (int i = 0; i < MyList.Count; i++)
            {
                MyList[i].material = MyMaterial;
            }
        }
        Material GatherRawImages(List<RawImage> MyImages, string TagName)
        {
            MyImages.Clear();
            MyImages.AddRange(GameObject.FindObjectsOfType<RawImage>());
            for (int i = MyImages.Count-1; i >= 0; i--)
            {
                if (MyImages[i].gameObject.tag != TagName)
                {
                    MyImages.RemoveAt(i);
                }
            }
            if (MyImages.Count != 0)
            {
               return MyImages[0].material;
            }
            else
            {
                return MyMaterial;
            }
        }
        private void GatherTexts(List<Text> MyTexts, string TagName)
        {
            MyTexts.Clear();
            MyTexts.AddRange(GameObject.FindObjectsOfType<Text>());
            for (int i = MyTexts.Count - 1; i >= 0; i--)
            {
                if (MyTexts[i].gameObject.tag != TagName)
                {
                    MyTexts.RemoveAt(i);
                }
            }
        }
        #endregion
        #region Updates
        private void UpdateData()
        {
            // backgrounds
            if (UpdateBackgroundColor)
            {
                UpdateBackgroundColor = false;
                UpdateListWithColor(Backgrounds, NewColor);
            }
            if (UpdateBackgroundColorOutline)
            {
                UpdateBackgroundColorOutline = false;
                UpdateListWithColorOutline(Backgrounds, NewColor);
            }
            // Heading
            if (UpdateLabelBackgroundColor)
            {
                UpdateLabelBackgroundColor = false;
                UpdateListWithColor(LabelBackgrounds, NewColor);
            }
            if (UpdateLabelBackgroundColorOutline)
            {
                UpdateLabelBackgroundColorOutline = false;
                UpdateListWithColorOutline(LabelBackgrounds, NewColor);
            }
            if (UpdateLabelTextColor)
            {
                UpdateLabelTextColor = false;
                UpdateListTextColor(LabelBackgrounds, NewColor);
            }
            if (UpdateLabelTextOutlineColor)
            {
                List<GameObject> MyLabelTexts = new List<GameObject>();
                for (int i = 0; i < LabelBackgrounds.Count; i++)
                {
                    MyLabelTexts.Add(LabelBackgrounds[i].transform.GetChild(0).gameObject);
                }
                UpdateLabelTextOutlineColor = false;
                UpdateListWithColorOutline2(MyLabelTexts, NewColor);
            }
        }
        void UpdateListWithColor(List<RawImage> MyList, Color32 MyColor)
        {
            for (int i = 0; i < MyList.Count; i++)
            {
                MyList[i].color = MyColor;
            }
        }
        void UpdateListWithColorOutline2(List<GameObject> MyList, Color32 MyColor)
        {
            for (int i = 0; i < MyList.Count; i++)
            {
                Outline MyOutline = MyList[i].GetComponent<Outline>();
                if (MyOutline)
                {
                    MyOutline.effectColor = MyColor;
                    MyOutline.effectDistance = new Vector2(NewOutlineSize, NewOutlineSize);
                }
            }
        }
        void UpdateListWithColorOutline(List<RawImage> MyList, Color32 MyColor)
        {
            for (int i = 0; i < MyList.Count; i++)
            {
                Outline MyOutline = MyList[i].gameObject.GetComponent<Outline>();
                if (MyOutline)
                {
                    MyOutline.effectColor = MyColor;
                    MyOutline.effectDistance = new Vector2(NewOutlineSize, NewOutlineSize);
                }
            }
        }
        void UpdateListTextColor(List<RawImage> MyList, Color32 MyColor)
        {
            for (int i = 0; i < MyList.Count; i++)
            {
                if (MyList[i].transform.childCount >= 1)
                {
                    Text MyText = MyList[i].transform.GetChild(0).GetComponent<Text>();
                    if (MyText)
                    {
                        MyText.color = MyColor;
                    }
                }
            }
        }
        #endregion

        #region FindInactives
        static private List<GameObject> HardFindGameObjects(string ComponentName)
        {
            List<GameObject> MyGameObjects = new List<GameObject>();
            Transform[] AllObjects = GameObject.FindObjectsOfType(typeof(Transform)) as Transform[];
            Debug.Log("Found: " + AllObjects.Length + " objects");
            for (int i = 0; i < AllObjects.Length; i++)
            {
                if (AllObjects[i].parent == null)
                { // means it's a root GO
                    Debug.Log("Found Root: " + AllObjects[i].name);
                    MyGameObjects = FindGameObjects(AllObjects[i].gameObject, MyGameObjects, ComponentName);
                }
            }
            return MyGameObjects;
        }
        static private List<GameObject> FindGameObjects(GameObject RootObject, List<GameObject> MyGameObjects, string ComponentName)
        {
            for (int i = 0; i < RootObject.transform.childCount; i++)
            {
                GameObject MyObject = RootObject.transform.GetChild(i).gameObject;
                if (MyObject.GetComponent(ComponentName) != null)
                {
                    MyGameObjects.Add(MyObject);   // add all the gameobjects of type
                }
                MyGameObjects = FindGameObjects(MyObject, MyGameObjects, ComponentName);
            }
            return MyGameObjects;
        }
        #endregion
    }
}