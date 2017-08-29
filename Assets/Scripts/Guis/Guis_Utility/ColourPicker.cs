using UnityEngine;
using UnityEngine.UI;
using Zeltex.Util;

namespace Zeltex.Guis
{
    /// <summary>
    /// Basic Gui Element to pick colours
    /// </summary>
    public class ColourPicker : MonoBehaviour
    {
        //public TextureEditor MyTextureMaker;
        //public int ColorToChange = 0;   // 0 for primary, 1 for secondary
        #region Variables
        public Color32 MyColor = new Color32(255,255,255,255);
        public GameObject MyColorShower;
        public Slider GreySlider;
        public Slider RedSlider;
        public Slider GreenSlider;
        public Slider BlueSlider;
        public Slider AlphaSlider;
        public InputField GreyInput;
        public InputField RedInput;
        public InputField GreenInput;
        public InputField BlueInput;
        public InputField AlphaInput;
        public MyEventColor32 OnChangeColor = new MyEventColor32();
        bool IsIgnoreValueChanges = false;
        #endregion

        #region Mono
        public void OnBegin()
        {
            //Debug.Log("OnAwake ColourPicker.");
            SetColor(MyColor);
        }
        #endregion

        public Color32 GetColor()
        {
            return MyColor;
        }

        public void SetColor(Color32 NewColor)
        {
            if (!IsIgnoreValueChanges)
            {
                IsIgnoreValueChanges = true;
                MyColor = NewColor;
                RefreshGui();
                OnChangeColor.Invoke(MyColor);
                IsIgnoreValueChanges = false;
            }
        }
        /// <summary>
        /// Update the sliders and the colour shower
        /// </summary>
        public void RefreshGui()
        {
            //Debug.Log("Refreshing Gui");
            MyColorShower.GetComponent<RawImage>().color = MyColor;
            GreySlider.value = ((MyColor.r + MyColor.g + MyColor.b)/3f) / 255f;
            GreySlider.CancelInvoke();
            RedSlider.value = MyColor.r / 255f;
            RedSlider.CancelInvoke();
            GreenSlider.value = MyColor.g / 255f;
            GreenSlider.CancelInvoke();
            BlueSlider.value = MyColor.b / 255f;
            BlueSlider.CancelInvoke();
            AlphaSlider.value = MyColor.a / 255f;
            if (GreyInput)
            {
                GreyInput.text = "" + (GreySlider.value * 255);
                RedInput.text = "" + (MyColor.r);
                GreenInput.text = "" + (MyColor.g);
                BlueInput.text = "" + (MyColor.b);
                AlphaInput.text = "" + (MyColor.a);
            }
        }

        public void UseInput(Slider MySlider)
        {
            if (MySlider.name == "RedSlider")
            {
                SetRed(MySlider.value);
            }
            else if (MySlider.name == "BlueSlider")
            {
                SetBlue(MySlider.value);
            }
            else if (MySlider.name == "GreenSlider")
            {
                SetGreen(MySlider.value);
            }
            else if (MySlider.name == "AlphaSlider")
            {
                SetAlpha(MySlider.value);
            }
            else if (MySlider.name == "GreyScaleSlider")
            {
                SetSaturation(MySlider.value);
            }
        }
        public void UseInput(InputField MyInput)
        {
            int MyInputInt = Mathf.RoundToInt(float.Parse(MyInput.text));
            MyInput.text = "" + MyInputInt;
            float MyInputFloat = (float)MyInputInt / 255;
            if (MyInput.name == "RedInput")
            {
                SetRed(MyInputFloat);
            }
            else if (MyInput.name == "BlueInput")
            {
                SetBlue(MyInputFloat);
            }
            else if (MyInput.name == "GreenInput")
            {
                SetGreen(MyInputFloat);
            }
            else if (MyInput.name == "AlphaInput")
            {
                SetAlpha(MyInputFloat);
            }
            else if (MyInput.name == "GreyInput")
            {
                SetSaturation(MyInputFloat);
            }
        }

        public void ChangeRed(InputField MyInput)
        {
            SetColor(new Color32((byte)int.Parse(MyInput.text), MyColor.g, MyColor.b, MyColor.a));
        }

        public void ChangeBlue(InputField MyInput)
        {
            SetColor(new Color32(MyColor.r, MyColor.g, (byte)int.Parse(MyInput.text), MyColor.a));
        }

        public void ChangeGreen(InputField MyInput)
        {
            SetColor(new Color32(MyColor.r, (byte)int.Parse(MyInput.text), MyColor.b, MyColor.a));
        }

        public void SetSaturation(float GreyValue)
        {
            byte Value = (byte)Mathf.RoundToInt(GreyValue * 255);  // new saturation value
            SetColor(new Color32(Value, Value, Value, MyColor.a));
            /*float Average = (MyColor.r + MyColor.g + MyColor.b) / 3f;

            float RatioR = MyColor.r / Average;   // ratio of r compared to total
            int ValueR = (byte)Mathf.RoundToInt(RatioR * Value);
            float RatioG = MyColor.g / Average;
            int ValueG = (byte)Mathf.RoundToInt(RatioG * Value);
            float RatioB = MyColor.b / Average;
            int ValueB = (byte)Mathf.RoundToInt(RatioB * Value);
            ValueR = Mathf.Clamp(ValueR, 0, 1);
            ValueG = Mathf.Clamp(ValueG, 0, 1);
            ValueB = Mathf.Clamp(ValueB, 0, 1);
            SetColor(new Color32((byte)ValueR, (byte)ValueG, (byte)ValueB, MyColor.a));*/
        }
        public void SetRed(float RedValue)
        {
            byte Value = (byte)Mathf.RoundToInt(RedValue * 255);
            //Debug.Log(Time.time + " - Set new red: " + Value.ToString());
            //Debug.Log("Set new red: " + MyColor.ToString());
            SetColor(new Color32(Value, MyColor.g, MyColor.b, MyColor.a));
            //Debug.Log("Set new red: " + MyColor.ToString());
        }
        public void SetGreen(float RedValue)
        {
            byte Value = (byte)Mathf.RoundToInt(RedValue * 255);
            SetColor(new Color32(MyColor.r, Value, MyColor.b, MyColor.a));
        }
        public void SetBlue(float RedValue)
        {
            byte Value = (byte)Mathf.RoundToInt(RedValue * 255);
            SetColor(new Color32(MyColor.r, MyColor.g, Value, MyColor.a));
        }
        public void SetAlpha(float AlphaValue)
        {
            byte Value = (byte)Mathf.RoundToInt(AlphaValue * 255);
            SetColor(new Color32(MyColor.r, MyColor.g, MyColor.b, Value));
        }

        public void IncreaseColor(int red, int green, int blue, int transparency)
        {
            Color32 NewColor = MyColor; //MySelectObject.GetComponent<RawImage> ().color;
            int NewRed = (NewColor.r + red);
            int NewGreen = (green + NewColor.g);
            int NewBlue = (blue + NewColor.b);
            int NewAlpha = (transparency + NewColor.a);
            NewRed = Mathf.Clamp(NewRed, 0, 255);
            NewGreen = Mathf.Clamp(NewGreen, 0, 255);
            NewBlue = Mathf.Clamp(NewBlue, 0, 255);
            NewAlpha = Mathf.Clamp(NewAlpha, 0, 255);
            NewColor.r = (byte)(NewRed);
            NewColor.g = (byte)(NewGreen);
            NewColor.b = (byte)(NewBlue);
            NewColor.a = (byte)(NewAlpha);
            SetColor(NewColor);
        }
        public void IncreaseRed(float RedIncrease) { IncreaseColor(Mathf.RoundToInt(RedIncrease * 255), 0, 0, 0); }
        public void IncreaseGreen(float RedIncrease) { IncreaseColor(0, Mathf.RoundToInt(RedIncrease * 255), 0, 0); }
        public void IncreaseBlue(float RedIncrease) { IncreaseColor(0, 0, Mathf.RoundToInt(RedIncrease * 255), 0); }
        public void IncreaseRed(int RedIncrease) { IncreaseColor(RedIncrease, 0, 0, 0); }
        public void IncreaseGreen(int RedIncrease) { IncreaseColor(0, RedIncrease, 0, 0); }
        public void IncreaseBlue(int RedIncrease) { IncreaseColor(0, 0, RedIncrease, 0); }
    }
}
/*

        void Start()
        {
            if (MyTextureMaker)
            {
                if (ColorToChange == 0)
                {
                    MyColor = MyTextureMaker.GetMainColor();
                }
                else
                {
                    MyColor = MyTextureMaker.GetSecondaryColor();
                }

            }
            //RefreshGui();
            if (MyTextureMaker)
             {
                 if (ColorToChange == 0)
                 {
                     MyTextureMaker.UpdatePrimaryColor(NewColor);
                 }
                 else
                 {
                     MyTextureMaker.UpdateSecondaryColor(NewColor);
                 }
             }
        }
*/