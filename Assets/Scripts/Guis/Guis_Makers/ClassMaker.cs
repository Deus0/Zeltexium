using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Zeltex.Util;
using Zeltex.Quests;
using Zeltex.Combat;

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// Gui Handler for Classes
    /// </summary>
    public class ClassMaker : MakerGui
    {
        private static ClassMaker MyClassMaker;

        public static ClassMaker Get()
        {
            if (MyClassMaker == null)
            {
                MyClassMaker = GameObject.Find("GameManager").GetComponent<MapMaker>().MyClassEditor;
            }
            return MyClassMaker;
        }

        #region DataManager

        /// <summary>
        /// Sets the file path for the files
        /// </summary>
        protected override void SetFilePaths()
        {
            DataManagerFolder = "Classes";
        }

        public List<string> GetData(string ClassName)
        {
            return FileUtil.ConvertToList(Get(ClassName));
        }

        public List<string> GetData(int ClassIndex)
        {
            return FileUtil.ConvertToList(Get(ClassIndex));
        }
        #endregion

        #region DataToUI

        /// <summary>
        /// Load Gui with Input
        /// Only gets called if index is updated
        /// </summary>
        public override void OnUpdatedIndex(int NewIndex)
        {
            base.OnUpdatedIndex(NewIndex);
            //Debug.Log("Updating Index in ItemMaker.");
            GetInput("NameInput").text = GetSelectedName();
            GetInput("ScriptInput").text = GetSelected();
            CancelInvokes();
        }

        /// <summary>
        /// Fill in a dropdown
        /// </summary>
        public override void FillDropdown(Dropdown MyDropdown)
        {
            //TextureMaker MyTextureMaker = TextureMaker.Get();
            List<string> DropdownNames = new List<string>();
            if (MyDropdown.name == "StatsDropdown")
            {
                for (int i = 0; i < DataManager.Get().GetSizeElements("Stats"); i++)
                {
                    DropdownNames.Add((DataManager.Get().GetElement("Stats", i) as Stat).Name);//MyStatsMaker.Get(i).Name);
                }
            }
            else if (MyDropdown.name == "ItemsDropdown")
            {
                DropdownNames.AddRange(DataManager.Get().GetNames("ItemMeta"));
                //for (int i = 0; i < MyItemMaker.GetSize(); i++)
                //{
                //DropdownNames.Add(MyItemMaker.MyInventory.MyItems[i].Name);
                // }
            }
            else if (MyDropdown.name == "DialoguesDropdown")
            {
                DropdownNames.AddRange(DataManager.Get().GetNames("Dialogues"));
                /*for (int i = 0; i < MyDialogueMaker.MyDialogueHandler.MyTree.GetSize(); i++)
                {
                    DropdownNames.Add(MyDialogueMaker.MyDialogueHandler.MyTree.GetDialogue(i).Name);
                }*/
            }
            else if (MyDropdown.name == "QuestsDropdown")
            {
                DropdownNames.AddRange(DataManager.Get().GetNames("Quests"));
                /* for (int i = 0; i < MyQuestMaker.MyQuestLog.GetSize(); i++)
                 {
                     DropdownNames.Add(MyQuestMaker.MyQuestLog.GetQuest(i).Name);
                 }*/
            }
            if (DropdownNames.Count > 0)
            {
                FillDropDownWithList(MyDropdown, DropdownNames);
            }
        }
        #endregion

        #region UseInput

        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(InputField MyInputField)
        {
            if (MyInputField.name == "NameInput")
            {
                MyInputField.text = Rename(MyInputField.text);
            }
            else if (MyInputField.name == "ScriptInput")
            {
                DataManager.Get().Set(DataManagerFolder, GetSelectedIndex(), MyInputField.text);
            }
        }

        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(Dropdown MyDropdown)
        {
            if (MyDropdown.name == "DataTypeDropdown")
            {
                GetDropdown("StatsDropdown").gameObject.SetActive(false);
                GetDropdown("ItemsDropdown").gameObject.SetActive(false);
                GetDropdown("DialoguesDropdown").gameObject.SetActive(false);
                GetDropdown("QuestsDropdown").gameObject.SetActive(false);
                //MyDialogueDataDropDown.gameObject.SetActive(false);
                if (MyDropdown.value == 0)
                {
                    GetDropdown("StatsDropdown").gameObject.SetActive(true);
                }
                else if (MyDropdown.value == 1)
                {
                    GetDropdown("ItemsDropdown").gameObject.SetActive(true);
                }
                else if (MyDropdown.value == 2)
                {
                    GetDropdown("DialoguesDropdown").gameObject.SetActive(true);
                }
                else if (MyDropdown.value == 3)
                {
                    GetDropdown("QuestsDropdown").gameObject.SetActive(true);
                }
            }
            else if (MyDropdown.name == "StatsDropdown")
            {
				GetLabel("DataLabelText").text = (DataManager.Get().GetElement("Stats", MyDropdown.value) as Stat).GetDescription();//MyStatsMaker.MyStats.GetStat(MyDropdown.value).GetDescription();
            }
            else if (MyDropdown.name == "ItemsDropdown")
            {
                //GetLabel("DataLabelText").text = MyItemMaker.MyInventory.GetItem(MyDropdown.value).GetDescriptionLabel();
            }
            else if (MyDropdown.name == "DialoguesDropdown")
            {
                GetLabel("DataLabelText").text = "";
            }
            else if (MyDropdown.name == "QuestsDropdown")
            {
                GetLabel("DataLabelText").text = (DataManager.Get().GetElement("Quests", MyDropdown.value) as Quest).GetDescriptionText();
            }
        }

        /// <summary>
        /// Used for generically updating buttons
        /// </summary>
        public override void UseInput(Button MyButton)
        {
            base.UseInput(MyButton);
            if (MyButton.name == "AddStatButton")
            {
                GetInput("ScriptInput").text += "\n/GiveStat " + GetDropdown("StatsDropdown").options[GetDropdown("StatsDropdown").value].text;
            }
            else if (MyButton.name == "AddItemButton")
            {
                GetInput("ScriptInput").text += "\n/GiveItem " + GetDropdown("ItemsDropdown").options[GetDropdown("ItemsDropdown").value].text;
            }
            else if (MyButton.name == "AddDialogueButton")
            {
                GetInput("ScriptInput").text += "\n/GiveDialogue " + GetDropdown("DialoguesDropdown").options[GetDropdown("DialoguesDropdown").value].text;
            }
            else if (MyButton.name == "AddQuestButton")
            {
                GetInput("ScriptInput").text += "\n/GiveQuest " + GetDropdown("QuestsDropdown").options[GetDropdown("QuestsDropdown").value].text;
            }
        }
        #endregion
    }

}



/*public string GetClassName(string FileName)
{
    if (FileName.Contains("Classes/"))
    {
        int IndexBegin = FileName.IndexOf("Classes/") + "Classes/".Length;
        FileName = FileName.Substring(IndexBegin);
    }
    return FileName;
}*/

/// <summary>
/// Until I implement the statmaker gui properly
/// </summary>
/*public List<string> GetStatNames()
{
    List<string> MyStatNames = new List<string>();
    MyStatNames.Add("Level");
    MyStatNames.Add("Experience");
    MyStatNames.Add("Health");
    MyStatNames.Add("Mana");
    MyStatNames.Add("Energy");
    MyStatNames.Add("HealthRegen");
    MyStatNames.Add("ManaRegen");
    MyStatNames.Add("EnergyRegen");
    MyStatNames.Add("Strength");
    MyStatNames.Add("Vitality");
    MyStatNames.Add("Intelligence");
    MyStatNames.Add("Wisdom");
    MyStatNames.Add("Agility");
    MyStatNames.Add("Dexterity");
    return MyStatNames;
}*/


/*public void UpdateStatistics()
{
    List<string> MyDebugClassScript = new List<string>();

    // Convert script into its complete script

    List<string> MyStats = ScriptUtil.GetQuestLogSection(FileUtil.ConvertToList(MyScriptText.text));
    MyDebugClassScript.Add("Stats: " + MyStats.Count);
    List<string> MyItems = ScriptUtil.GetQuestLogSection(FileUtil.ConvertToList(MyScriptText.text));
    MyDebugClassScript.Add("Items: " + MyItems.Count);
    List<string> MyQuestParts = ScriptUtil.GetQuestLogSection(FileUtil.ConvertToList(MyScriptText.text));
    MyDebugClassScript.Add("Quests: " + MyQuestParts.Count);
    List<string> MyDialogueParts = ScriptUtil.GetDialogueSection(FileUtil.ConvertToList(MyScriptText.text));
    MyDebugClassScript.Add("Dialogue: " + MyDialogueParts.Count);

    string DebugClass = "";
    for (int i = 0; i < MyDebugClassScript.Count; i++)
        DebugClass += (MyDebugClassScript[i] + "\n");
    MyStatusText.text = DebugClass;
}*/

/// <summary>
/// Assumes one world, remove this eventually.
/// </summary>
/// <param name="IsWorldPath"></param>
/// <returns></returns>
/*public static string GetSaveFolderPath(bool IsWorldPath)
{
    string FilePath = "";
    if (IsWorldPath)
    {
        if (MapMaker.SaveFileName != "")
        {
            FilePath = FileUtil.GetWorldFolderPath(MapMaker.SaveFileName);  //MyWorld.GetComponent<VoxelSaver>().SaveFileName
        }
        else
        {
            return "";
        }
    }
    else
    {
        FilePath = FileUtil.GetDefaultFolderPath();   // default classes folder
    }
    FilePath += FolderName;
    if (!Directory.Exists(FilePath))
        Directory.CreateDirectory(FilePath);
    return FilePath;
}*/
