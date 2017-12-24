using UnityEngine;
using System.Collections.Generic;
using Zeltex.Game;

namespace Zeltex.Guis.Characters
{
    /// <summary>
    /// A quick list of guis the character can enable and disable
    /// </summary>
    [ExecuteInEditMode]
    public class MenuGui : MonoBehaviour
    {
        //public EditorAction ActionRefreshElements;
        public CharacterGuis MyGuiManager;
        public Color32 OffColor;
        public Color32 OnColor;

        /*public void Update()
        {
            if (ActionRefreshElements.IsTriggered())
            {
                RefreshElements();
            }
        }*/

        /*public void RefreshElements()
        {
            Clear();
            List<string> MyGuis = CharacterGuiManager.Get().GetNames();//MyGuiManager.GetNames();
            for (int i = 0; i < MyGuis.Count; i++)
            {
                AddElement(MyGuis[i]);
            }
        }*/

        /*public void AddElement(string GuiName)
        {
            if ((Contains(GuiName) == false)
                && GuiName != "Menu" 
                && GuiName != "Dialogue"
                && GuiName != "ItemPickup"
                && GuiName != "Tooltip")
            {
                Add(GuiName);
                GuiListElement MyToolTip = MyGuis[MyGuis.Count - 1].GetComponent<GuiListElement>();
                //MyToolTip.OnClick.AddEvent(delegate { Toggle(GuiName, MyToolTip); });//MyZelGui.Toggle);
                MyToolTip.OnClickListElement.AddEvent<string, GuiListElement>(Toggle);
                MyToolTip.SetColors(OffColor, OnColor);
                MyToolTip.MyGuiListElementData.IsToolTip = false;
            }
        }*/

        public void Toggle(string GuiName, GuiListElement MyToolTip)
        {
            ZelGui MyZelGui = MyGuiManager.GetZelGui(GuiName);
            Debug.Log("Is " + GuiName + " ZelGui ? " + (MyZelGui != null));
            if (MyZelGui != null)
            {
                // destroy
                MyToolTip.OnToggledOff();
                //MyGuiManager.Remove(MyZelGui);
                MyZelGui.Toggle();
            }
            else
            {
                // create
                MyToolTip.OnToggledOn();
                MyGuiManager.Spawn(GuiName);
            }
        }

        public void EndGame()
        {
            GameManager.Get().EndGame();
        }

        public void SaveGame()
        {
            Voxels.WorldManager.Get().SaveGame(MyGuiManager.GetCharacter());
        }

        public void ResumeGame()
        {
            GameManager.Get().ResumeGame();
            GetComponent<ZelGui>().TurnOff();
        }
        
        public void ToggleGui(string GuiName)
        {
            ZelGui MyZelGui = MyGuiManager.GetZelGui(GuiName);
            Debug.Log("ToggleGui - Is " + GuiName + " ZelGui ? " + (MyZelGui != null));
            if (MyZelGui != null)
            {
                MyZelGui.Toggle();
            }
            else
            {
                ZelGui MyGui = MyGuiManager.Spawn(GuiName);
                if (MyGui)
                {
                    MyGui.TurnOn();
                }
                else
                {
                    Debug.LogError("Could not spawn: " + GuiName);
                }
            }
        }
    }
}