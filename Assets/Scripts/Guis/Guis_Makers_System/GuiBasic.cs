using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Guis;

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// Base class for all the guis with many inputs
    /// </summary>
    public class GuiBasic : MonoBehaviour
    {
        #region Variables
        [Header("UI")]
        public List<InputField> MyInputs;
        public List<Text> MyLabels;
        public List<RawImage> MyImages;
        public List<Dropdown> MyDropdowns;
        public List<Button> MyButtons;
        public List<Toggle> MyToggles;
        public List<GuiList> MyLists;
        #endregion

        #region InputUtil
        /// <summary>
        /// Sets our inputs off
        /// </summary>
        protected void SetInputs(bool NewState)
        {
            for (int i = 0; i < MyInputs.Count; i++)
            {
                if (MyInputs[i])
                {
                    MyInputs[i].interactable = NewState;
                    if (!NewState)
                    {
                        MyInputs[i].text = "";
                    }
                }
                else
                {
                    //Debug.LogError(name + " has a null input at: " + i);
                }
            }
            for (int i = 0; i < MyDropdowns.Count; i++)
            {
                if (MyDropdowns[i])
                {
                    MyDropdowns[i].interactable = NewState;
                }
            }
            for (int i = 0; i < MyButtons.Count; i++)
            {
                if (MyButtons[i])
                {
                    MyButtons[i].interactable = NewState;
                }
            }
            for (int i = 0; i < MyToggles.Count; i++)
            {
                if (MyToggles[i])
                {
                    MyToggles[i].interactable = NewState;
                }
            }
        }

        protected void CancelInvokes()
        {
            for (int i = 0; i < MyInputs.Count; i++)
            {
                if (MyInputs[i])
                {
                    MyInputs[i].CancelInvoke();
                }
            }
            for (int i = 0; i < MyToggles.Count; i++)
            {
                if (MyToggles[i])
                {
                    MyToggles[i].CancelInvoke();
                }
            }
        }
        #endregion

        #region FillInput
        private bool HasFilledContainers;
        /// <summary>
        /// Fills in all the containers of the gui
        /// </summary>
        protected void FillAllContainers()
        {
            if (!HasFilledContainers)
            {
                HasFilledContainers = true;
                for (int i = 0; i < MyDropdowns.Count; i++)
                {
                    FillDropdown(MyDropdowns[i]);
                }
                for (int i = 0; i < MyLists.Count; i++)
                {
                    FillList(MyLists[i]);
                }
            }
        }

        /// <summary>
        /// fills in all the dropdowns
        /// </summary>
        public virtual void FillDropdown(Dropdown MyDropdown)
        {

        }

        /// <summary>
        /// Fills in all the lists
        /// </summary>
        public virtual void FillList(GuiList MyList)
        {

        }

        /// <summary>
        /// Fills in a dropdown with a list of strings
        /// </summary>
        public static void FillDropDownWithList(Dropdown MyDropDown, List<string> Data)
        {
            MyDropDown.value = 0;
            MyDropDown.ClearOptions();
            List<Dropdown.OptionData> MyOptions = new List<Dropdown.OptionData>();
            for (int i = 0; i < Data.Count; i++)
            {
                MyOptions.Add(new Dropdown.OptionData(Data[i]));
            }
            MyDropDown.AddOptions(MyOptions);
            MyDropDown.RefreshShownValue();
        }

        #endregion

        #region UseInput

        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public virtual void UseInput(InputField MyInputField)
        {

        }

        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public virtual void UseInput(Dropdown MyDropdown)
        {

        }

        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public virtual void UseInput(Toggle MyToggle)
        {

        }

        /// <summary>
        /// Used for generically updating buttons
        /// </summary>
        public virtual void UseInput(Button MyButton)
        {

        }

        /// <summary>
        /// Used for generically updating buttons
        /// </summary>
        public virtual void UseInput(GuiList MyList)
        {

        }

        #endregion

        #region GetInput
        /// <summary>
        /// Gets an inputfield connected to the gui
        /// </summary>
        protected InputField GetInput(string MyInputName)
        {
            for (int i = 0; i < MyInputs.Count; i++)
            {
                if (MyInputs[i] && MyInputs[i].name == MyInputName)
                {
                    return MyInputs[i];
                }
            }
           // Debug.LogError("Did not find input [" + MyInputName + "] in: " + name + " - " + MyInputs.Count);
            return null;
        }

        protected Dropdown GetDropdown(string MyInputName)
        {
            for (int i = 0; i < MyDropdowns.Count; i++)
            {
                if (MyDropdowns[i] && MyDropdowns[i].name == MyInputName)
                {
                    return MyDropdowns[i];
                }
            }
            //Debug.LogError("Did not find dropdown [" + MyInputName + "] in: " + name + " - " + MyDropdowns.Count);
            return null;
        }
        /// <summary>
        /// Gets an inputfield connected to the gui
        /// </summary>
        protected Text GetLabel(string LabelName)
        {
            for (int i = 0; i < MyLabels.Count; i++)
            {
                if (MyLabels[i] && MyLabels[i].name == LabelName)
                {
                    return MyLabels[i];
                }
            }
            //Debug.LogError("Did not find label [" + LabelName + "] in: " + name + " - " + MyLabels.Count);
            return null;
        }

        protected RawImage GetImage(string ImageName)
        {
            for (int i = 0; i < MyImages.Count; i++)
            {
                if (MyImages[i] && MyImages[i].name == ImageName)
                {
                    return MyImages[i];
                }
            }
            //Debug.LogError("Did not find image [" + ImageName + "] in: " + name + " - " + MyImages.Count);
            return null;
        }
        protected Button GetButton(string MyName)
        {
            for (int i = 0; i < MyButtons.Count; i++)
            {
                if (MyButtons[i].name == MyName)
                {
                    return MyButtons[i];
                }
            }
            //Debug.LogError("Did not find button [" + MyName + "] in: " + name + " - " + MyButtons.Count);
            return null;
        }
        protected Toggle GetToggle(string MyName)
        {
            for (int i = 0; i < MyToggles.Count; i++)
            {
                if (MyToggles[i].name == MyName)
                {
                    return MyToggles[i];
                }
            }
            //Debug.LogError("Did not find button [" + MyName + "] in: " + name + " - " + MyToggles.Count);
            return null;
        }


        protected GuiList GetList(string MyName)
        {
            return GetListHandler(MyName);
        }

        protected GuiList GetListHandler(string MyName)
        {
            for (int i = 0; i < MyLists.Count; i++)
            {
                if (MyLists[i].name == MyName)
                {
                    return MyLists[i];
                }
            }
            //Debug.LogError("Did not find List [" + MyName + "] in: " + name + " - " + MyToggles.Count);
            return null;
        }
        #endregion


        #region ZelGui
        /// <summary>
        /// Called on ZelGuis OnBegin function
        /// </summary>
        public virtual void OnBegin()
        {
            FillAllContainers();
        }
        /// <summary>
        /// Called on ZelGuis OnEnd function
        /// </summary>
        public virtual void OnEnd()
        {

        }
        #endregion

    }
}