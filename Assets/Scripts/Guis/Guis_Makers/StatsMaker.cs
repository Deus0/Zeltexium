using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Combat;
using Zeltex.Util;
using System.IO;
using UnityEngine.UI;
using Zeltex.Guis;

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// Edits a list of stats
    /// </summary>
    public class StatsMaker : ElementMakerGui
    {
        #region DataManager

        protected override void SetFilePaths()
        {
            DataManagerFolder = "Stats";
        }

        /// <summary>
        /// Index of where it is added
        /// </summary>
        protected override void AddData()
		{
			Debug.Log("Adding new stat.");
			Stat MyStat = new Stat();
			MyStat.CreateBase("Stat", 0);
			DataManager.Get().AddElement(DataManagerFolder, MyStat);
		}

        public Stat GetStat(int Index)
        {
            return DataManager.Get().GetElement(DataManagerFolder, Index) as Stat;
        }

        public Stat GetStat(string StatName)
        {
            return DataManager.Get().GetElement(DataManagerFolder, StatName) as Stat;
        }

        public Stat GetSelectedStat()
        {
            return DataManager.Get().GetElement(DataManagerFolder, GetSelectedIndex()) as Stat;
        }
        #endregion

        // index control 
        #region IndexController

        public static StatsMaker Get()
        {
            return GameObject.Find("StatsMaker").GetComponent<StatsMaker>();
        }
        /// <summary>
        /// Load Gui with Input
        /// Only gets called if index is updated
        /// </summary>
        public override void OnUpdatedIndex(int NewIndex)
        {
            base.OnUpdatedIndex(NewIndex);
            //Debug.Log("Updating Index in StatsMaker: " + NewIndex);
            GetInput("NameInput").text = GetSelectedName();
            GetLabel("NameLabelText").text = GetSelectedName();
            GetInput("DescriptionInput").text = GetSelectedStat().Description;
            GetImage("TextureImage").texture = GetSelectedStat().GetTexture();
            GetDropdown("TextureDropdown").value = 0;
            //GetInput("States").text = "" + GetSelected().GetValue();
            //GetInput("RegenModifierInput").text = "";
            //GetInput("StateInput").text = "" + "";

            GetDropdown("StatTypeDropdown").onValueChanged = new Dropdown.DropdownEvent();
            GetInput("StatsInput1").interactable = false;
            GetInput("StatsInput2").interactable = false;
            GetInput("StatsInput3").interactable = false;
            GetInput("StatsInput4").interactable = false;
            GetInput("StatsInput1").text = "";
            GetInput("StatsInput2").text = "";
            GetInput("StatsInput3").text = "";
            GetInput("StatsInput4").text = "";
            // reset the things
            GetLabel("Label1").text = "";
            GetLabel("Label2").text = "";
            GetLabel("Label3").text = "";
            GetLabel("Label4").text = "";
            FillInputs();
            GetDropdown("StatTypeDropdown").onValueChanged.AddEvent(delegate { UseInput(GetDropdown("StatTypeDropdown")); });
        }

        private void FillInputs()
        {
            // [string] [value] - Joyness 5
            if (GetSelectedStat().GetStatType() == StatType.Base)
            {
                GetDropdown("StatTypeDropdown").value = 0;
                GetLabel("Label1").text = "Value";
                GetInput("StatsInput1").interactable = true;
                GetInput("StatsInput1").text = "" + GetSelectedStat().GetValue();
            }
            // [string] [value] [value] - Health 50/100
            else if (GetSelectedStat().GetStatType() == StatType.State)
            {
                GetDropdown("StatTypeDropdown").value = 1;
                GetLabel("Label1").text = "State";
                GetLabel("Label2").text = "Value";
                GetInput("StatsInput1").interactable = true;
                GetInput("StatsInput2").interactable = true;
                GetInput("StatsInput1").text = "" + GetSelectedStat().GetState();
                GetInput("StatsInput2").text = "" + GetSelectedStat().GetMaxState();
            }
            // [string] [string] [value] [value] - HealthRegen,Health,0,1
            else if (GetSelectedStat().GetStatType() == StatType.Regen)
            {
                GetDropdown("StatTypeDropdown").value = 2;
                GetLabel("Label1").text = "Stat";
                GetLabel("Label2").text = "Value";
                GetLabel("Label3").text = "Rate";
                GetInput("StatsInput1").interactable = true;
                GetInput("StatsInput2").interactable = true;
                GetInput("StatsInput3").interactable = true;
                GetInput("StatsInput1").text = GetSelectedStat().GetModifyStatName();
                GetInput("StatsInput2").text = "" + GetSelectedStat().GetRegenValue();
                GetInput("StatsInput3").text = "" + GetSelectedStat().GetRegenRate();
            }
            // [string] [value] [string] [value] - Strength 10, Health x10
            else if (GetSelectedStat().GetStatType() == StatType.Modifier)
            {
                GetDropdown("StatTypeDropdown").value = 3;
                GetLabel("Label1").text = "Value";
                GetLabel("Label2").text = "Stat";
                GetLabel("Label3").text = "Multiplier";
                GetInput("StatsInput1").interactable = true;
                GetInput("StatsInput2").interactable = true;
                GetInput("StatsInput3").interactable = true;
                GetInput("StatsInput1").text = "" + GetSelectedStat().GetValue();
                GetInput("StatsInput2").text = "" + GetSelectedStat().GetModifyStatName();
                GetInput("StatsInput3").text = "" + GetSelectedStat().GetModifierValue();
            }
        }
        #endregion

        #region UI
        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(InputField MyInputField)
        {
            if (MyInputField.name == "NameInput")
            {
                MyInputField.text = Rename(MyInputField.text);
                GetLabel("NameLabelText").text = GetSelectedName();
            }
            else if (MyInputField.name == "DescriptionInput")
            {
				GetSelectedStat().SetDescription(MyInputField.text);
			}
			else
			{
				if (GetSelectedStat().GetStatType() == StatType.Base)
				{
					if (MyInputField.name == "StatsInput1")
					{
						GetSelectedStat().SetValue(float.Parse(MyInputField.text));
					}
				}
				else if (GetSelectedStat().GetStatType() == StatType.State)
				{
					if (MyInputField.name == "StatsInput1")
					{
						GetSelectedStat().SetState(float.Parse(MyInputField.text));
					}
					else if (MyInputField.name == "StatsInput2")
					{
						GetSelectedStat().SetMax(float.Parse(MyInputField.text));
					}
				}
				else if (GetSelectedStat().GetStatType() == StatType.Regen)
				{
					if (MyInputField.name == "StatsInput1")
					{
						GetSelectedStat().SetModifier(MyInputField.text); // stat name
					}
					else if (MyInputField.name == "StatsInput2")
					{
						GetSelectedStat().SetRegenValue(float.Parse(MyInputField.text)); //Value
					}
					else if (MyInputField.name == "StatsInput3")
					{
						GetSelectedStat().SetRegenCooldown(float.Parse(MyInputField.text));   // cooldown/rate
					}
				}
				else if (GetSelectedStat().GetStatType() == StatType.Modifier)
				{
					if (MyInputField.name == "StatsInput1")
					{
						GetSelectedStat().SetValue(float.Parse(MyInputField.text)); // stat value
					}
					else if (MyInputField.name == "StatsInput2")
					{
						GetSelectedStat().SetModifier(MyInputField.text);   // stat modified name
					}
					else if (MyInputField.name == "StatsInput3")
					{
						GetSelectedStat().SetModifierValue(float.Parse(MyInputField.text));   // stat multiplier
					}
					else if (MyInputField.name == "StatsInput4")
					{
						GetSelectedStat().SetModifier(MyInputField.text);
					}
				}
			}
        }
        
        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(Dropdown MyDropdown)
        {
            if (MyDropdown.name == "TextureDropdown")  //
            {
                if (MyDropdown.value > 0)
                {
					//GetSelectedStat().SetTexture(DataManager.Get().GetTexture("StatTextures", MyDropdown.value - 1));
                   // GetImage("TextureImage").texture = GetSelectedStat().GetTexture();
                }
            }
			// change the type of stat
            else if (MyDropdown.name == "StatTypeDropdown")  //
            {
                if (MyDropdown.value == 0)
                {
					GetSelectedStat().CreateBase(GetSelectedStat().Name, GetSelectedStat().GetValue());
                }
                else if (MyDropdown.options[MyDropdown.value].text == "State")
                {
					GetSelectedStat().CreateState(GetSelectedStat().Name, GetSelectedStat().GetValue(), GetSelectedStat().GetValue());
                }
                else if (MyDropdown.options[MyDropdown.value].text == "Regen")
                {
					GetSelectedStat().CreateRegen(GetSelectedStat().Name,  "", GetSelectedStat().GetValue(), 0);
                }
                else if (MyDropdown.options[MyDropdown.value].text == "Modifier")
                {
                    GetSelectedStat().CreateModifier(GetSelectedStat().Name, GetSelectedStat().GetValue(), "", 1);
                }
                else if (MyDropdown.options[MyDropdown.value].text == "Buff")
                {
                    GetSelectedStat().CreateBuff(GetSelectedStat().Name, GetSelectedStat().GetValue(), "", 1);
                }
                else if (MyDropdown.options[MyDropdown.value].text == "Dot")
                {
                    GetSelectedStat().CreateDot(GetSelectedStat().Name, GetSelectedStat().GetValue(), "", 1, 3);
                }
                OnUpdatedIndex(MyIndexController.SelectedIndex);    // refresh inputs
            }
        }

        /// <summary>
        /// Fill the drop downs with data
        /// </summary>
        public override void FillDropdown(Dropdown MyDropdown)
        {
            List<string> MyDropdownNames = new List<string>();
            if (MyDropdown.name == "TextureDropdown")
            {
                MyDropdownNames.Add("Custom");
                MyDropdownNames.AddRange(DataManager.Get().GetNames("StatTextures"));
                FillDropDownWithList(MyDropdown, MyDropdownNames);
            }
        }
        #endregion

        #region Import
        /// <summary>
        /// Export the file using webgl
        /// </summary>
        public void Export()
        {
            //FileUtil.Export(GetSelectedName(), FileExtension, GetSelected().GetScript());
        }
        /// <summary>
        /// Import Data using Webgl
        /// Called on mouse down - instead of mouse up like normal buttons
        /// </summary>
        public void Import()
        {
           // FileUtil.Import(name, "Upload", FileExtension);
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
                Stat NewData = new Stat();
                NewData.RunScript(FileUtil.ConvertToList(MyScript));
                AddData(UploadFileName, NewData);
            }
        }
        /// <summary>
        /// Add a new voxel to the game!
        /// </summary>
        public void AddData(string MyName, Stat NewData)
        {
            /*for (int i = 0; i < MyNames.Count; i++)
            {
                if (MyNames[i] == MyName)
                {
                    return;
                }
            }
            AddName(MyName);
            MyStats.Add(NewData);
            MyIndexController.SetMaxSelected(GetSize());*/
        }
        #endregion
    }
}