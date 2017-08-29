using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Zeltex.AnimationUtilities
{

    [ExecuteInEditMode]
    public class GuiColourManager : MonoBehaviour
    {
        public string MyBackgroundTag = "GuiBackground";
        public bool IsChangeBackgroundColor;
        public Color32 NewBackgroundColor;
        public Color32 NewBackgroundOutlineColor;
        public int NewBackgroundOutlineSize = 10;

        public string ButtonNextName = "NextButton";
        public bool IsReplaceNextImage;
        public Texture MyNextButtonTexture;

        public bool IsReplaceFonts;
        public Font MyFontStyle;
        public Color32 MyFontColor;
        public Color32 MyFontOutlineColor;

        // Update is called once per frame
        void Update()
        {
            if (IsChangeBackgroundColor)
            {
                IsChangeBackgroundColor = false;

                //GameObject[] MyGuiBackgrounds = GameObject.FindGameObjectsWithTag("GuiBackground", true);
                GameObject[] MyGuiBackgrounds = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(UnityEngine.GameObject));
                Debug.LogError("Found: " + MyGuiBackgrounds.Length + " Objects!");
                foreach (GameObject BackgroundObject in MyGuiBackgrounds)
                {
                    if (BackgroundObject.tag == MyBackgroundTag)
                    {
                        //Debug.Log ("Altering: " + BackgroundObject.name);
                        if (BackgroundObject.GetComponent<RawImage>())
                            BackgroundObject.GetComponent<RawImage>().color = NewBackgroundColor;
                        if (BackgroundObject.GetComponent<Outline>())
                        {
                            BackgroundObject.GetComponent<Outline>().effectColor = NewBackgroundOutlineColor;
                            BackgroundObject.GetComponent<Outline>().effectDistance = new Vector2(NewBackgroundOutlineSize, NewBackgroundOutlineSize);
                        }
                    }
                }
            }

            if (IsReplaceNextImage)
            {
                IsReplaceNextImage = false;
                GameObject[] MyNextButtons = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(UnityEngine.GameObject));
                //Debug.LogError("Found: " + MyGuiBackgrounds.Length + " Objects!");
                foreach (GameObject NextObject in MyNextButtons)
                {
                    if (NextObject.name == ButtonNextName)
                    {
                        NextObject.GetComponent<RawImage>().texture = MyNextButtonTexture;
                    }
                }
            }

            if (IsReplaceFonts)
            {
                IsReplaceFonts = false;
                GameObject[] MyFonts = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(UnityEngine.GameObject));
                //Debug.LogError("Found: " + MyGuiBackgrounds.Length + " Objects!");
                foreach (GameObject NextObject in MyFonts)
                {
                    if (NextObject.GetComponent<Text>())
                    {
                        NextObject.GetComponent<Text>().font = MyFontStyle;
                        NextObject.GetComponent<Text>().color = MyFontColor;

                        if (NextObject.GetComponent<Outline>())
                        {
                            NextObject.GetComponent<Outline>().effectColor = MyFontOutlineColor;
                        }
                    }
                }
            }
        }
    }
}