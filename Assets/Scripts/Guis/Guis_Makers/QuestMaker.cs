using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Zeltex.Quests;
using Zeltex.Util;

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// Edits the quests
    /// To Do:
    ///     - fix renaming to alter file if exists 
    ///     - make sure renamed version isn't same quest name
    ///     - fix deleting
    /// </summary>
    public class QuestMaker : ElementMakerGui
    {
        public QuestLog MyQuestLog;

        #region DataManager

        protected override void SetFilePaths()
        {
            DataManagerFolder = "Quests";
        }

        /// <summary>
        /// Gets the selected quest
        /// </summary>
        public Quest GetSelectedQuest()
        {
            return DataManager.Get().GetElement(DataManagerFolder, GetSelectedIndex()) as Quest;
        }

        /// <summary>
        /// Create a new quest
        /// </summary>
        protected override void AddData()
        {
            base.AddData();
            Quest MyQuest = new Quest();
            MyQuest.Name = "Q" + Random.Range(1, 10000);
            DataManager.Get().AddElement(DataManagerFolder, MyQuest);
        }
        #endregion

        // direct quest data access
        #region Data

        /// <summary>
        /// Clears the data
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            MyQuestLog.Clear();
        }

        public Condition GetSelectedCondition()
        {
            if (GetSelectedQuest().MyConditions.Count == 0)
            {
                GetSelectedQuest().MyConditions.Add(new Condition());
            }
            return GetSelectedQuest().MyConditions[0];
        }
        #endregion

        // index control 
        #region IndexController

        /// <summary>
        /// Load Gui with Input
        /// Only gets called if index is updated
        /// </summary>
        public override void OnUpdatedIndex(int NewIndex)
        {
            base.OnUpdatedIndex(NewIndex);
            if (GetSelectedQuest() != null)
            {
                //Debug.Log("Updating Index in questmaker.");
                GetInput("NameInput").text = GetSelectedQuest().Name;
                GetInput("DescriptionInput").text = GetSelectedQuest().Description;
                GetInput("ObjectNameInput").text = GetSelectedCondition().ObjectName;
                GetInput("QuantityInput").text = "" + GetSelectedCondition().ItemQuantity;
                bool WasFound = false;
                for (int i = 1; i < GetDropdown("ConditionsDropdown").options.Count; i++)   // skip nothing
                {
                    if (GetSelectedCondition().ConditionType == GetDropdown("ConditionsDropdown").options[i].text)
                    {
                        GetDropdown("ConditionsDropdown").value = i;
                        WasFound = true;
                        break;
                    }
                }
                if (WasFound == false)
                {
                    GetDropdown("ConditionsDropdown").value = 0;
                }
                GetDropdown("ConditionsDropdown").CancelInvoke();
            }
            else
            {
                Debug.LogError("No Quest");
            }
        }
        #endregion

        // Particular UI stuff
        #region UI

        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(InputField MyInputField)
        {
            if (MyInputField.name == "NameInput")
            {
                MyInputField.text = Rename(MyInputField.text);
                //GetSelectedQuest().Name = MyInputField.text; // rename the quest too!
            }
            else if (MyInputField.name == "DescriptionInput")
            {
                GetSelectedQuest().SetDescription(MyInputField.text);
            }
            else if (MyInputField.name == "ObjectNameInput")
            {
                GetSelectedCondition().ObjectName = MyInputField.text;
            }
            else if (MyInputField.name == "QuantityInput")
            {
                GetSelectedCondition().ItemQuantity = int.Parse(MyInputField.text);
            }
        }
        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(Dropdown MyDropDown)
        {
            //Debug.Log("QuestMaker UseInput: " + MyDropDown.name);
            if (MyDropDown.name == "ConditionsDropdown")
            {
                string DropdownLabel = MyDropDown.options[MyDropDown.value].text;
                //Debug.Log("Adding quest condtition of type: " + DropdownLabel);
                GetSelectedCondition().ConditionType = DropdownLabel;
                //GetSelected().SetTexture(MyItemTextureManager.MyTextures[MyDropDown.value] as Texture2D);
                //TextureImage.texture = GetSelected().GetTexture();
            }
        }
        #endregion

        #region ImportExport
        /// <summary>
        /// Export the file using webgl
        /// </summary>
        public void Export()
        {
            //FileUtil.Export(GetSelectedName(), FileExtension, FileUtil.ConvertToSingle(GetSelected().GetScriptList()));
        }
        /// <summary>
        /// Import Data using Webgl
        /// Called on mouse down - instead of mouse up like normal buttons
        /// </summary>
        public void Import()
        {
            //FileUtil.Import(name, "Upload", FileExtension);
        }
        /// <summary>
        /// Called from javascript, uploading a model data
        /// </summary>
        public void Upload(string MyScript)
        {
            string UploadFileName = "";
            for (int i = 0; i < MyScript.Length; i++)
            {
                if (MyScript[i] == '\n')
                {
                    UploadFileName = MyScript.Substring(0, i);
                    UploadFileName = Path.GetFileNameWithoutExtension(UploadFileName);
                    MyScript = MyScript.Substring(i + 1);
                    break;
                }
            }
            if (UploadFileName != "")
            {
                Debug.Log("Uploading new voxel:" + UploadFileName + ":" + MyScript.Length);
                Quest NewData = new Quest();
                NewData.RunScript(FileUtil.ConvertToList(MyScript));
                AddData(UploadFileName, NewData);
            }
        }
        /// <summary>
        /// Add a new voxel to the game!
        /// </summary>
        public void AddData(string MyName, Quest NewData)
        {
            /*for (int i = 0; i < MyNames.Count; i++)
            {
                if (MyNames[i] == MyName)
                {
                    return;
                }
            }
            AddName(MyName);
            MyQuestLog.Add(NewData);
            MyIndexController.SetMaxSelected(GetSize());*/
        }
        #endregion
    }
}


/// <summary>
/// Index of where it is added
/// </summary>
///protected override void AddData()
//{
//	DataManager.Get().AddQuest(DataManagerFolder);
/* Quest MyQuest = new Quest();
MyQuest.MyConditions.Add(new Condition());
MyQuest.Name = "New Quest " + Mathf.RoundToInt(Random.Range(1, 10000));
MyQuestLog.Add(MyQuest);    //"New Quest"
AddName(MyQuest.Name);
base.OnAdd(NewIndex);
// }

        /// <summary>
        /// Called when list is empty
        /// </summary>
        /*public override void OnListEmpty()
        {
            base.OnListEmpty();
        }*/
