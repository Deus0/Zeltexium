using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Combat;
using System.IO;

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// Makes spells! BAm!
    /// </summary>
    public class SpellMaker : ElementMakerGui
    {
        #region Variables
        [Header("SpellMaker")]
        //public List<Spell> MySpells = new List<Spell>();
        //public List<string> MyData = new List<string>();
        public ColourPicker MyColorPicker;
        //public SoundMaker MySoundMaker;
       // public Zeltex.Skeletons.SkeletonMaker MySkeletonMaker;
        //public ClassMaker MyClassMaker;
        private static SpellMaker MySpellMaker;
        #endregion

        #region DataManager

        protected override void SetFilePaths()
        {
            DataManagerFolder = "Spells";
        }
        
        /// <summary>
        /// gets selected data
        /// </summary>
        public Spell GetSelectedSpell()
        {
            return DataManager.Get().GetElement(DataManagerFolder, GetSelectedIndex()) as Spell;
        }
        /// <summary>
        /// gets a data by a name
        /// </summary>
        public Spell GetSpell(string SpellName)
        {
            return DataManager.Get().GetElement(DataManagerFolder, SpellName) as Spell;
        }

        /// <summary>
        /// Index of where it is added
        /// </summary>
        protected override void AddData()
        {
            Spell NewSpell = new Spell();
            DataManager.Get().AddElement(DataManagerFolder, NewSpell);
        }
        #endregion

        #region IndexController

        /// <summary>
        /// Load Gui with Input
        /// Only gets called if index is updated
        /// </summary>
        public override void OnUpdatedIndex(int NewIndex)
        {
            base.OnUpdatedIndex(NewIndex);
            Spell MySpell = GetSelectedSpell();
            if (MySpell != null)
            {
                Debug.LogError("Spell Being Set: " + NewIndex);
                // Main
                GetInput("NameInput").text = MySpell.Name;
                GetToggle("SummonToggle").isOn = MySpell.IsSummon;
                GetInput("SummonNameInput").text = MySpell.MinionName;
                GetInput("ClassNameInput").text = MySpell.MinionClass;
                GetInput("RaceNameInput").text = MySpell.MinionRace;
                GetToggle("AddStatToggle").isOn = MySpell.IsAddStat;
                GetInput("AddStatNameInput").text = MySpell.AddStatName;
                GetInput("AddStatValueInput").text = "" + MySpell.AddStatValue;
                GetToggle("UseStatToggle").isOn = MySpell.IsUseStat;
                GetInput("UseStatNameInput").text = MySpell.StatUseName;
                GetInput("UseStatValueInput").text = "" + MySpell.StatCost;
                // Function
                GetToggle("StickToggle").isOn = MySpell.IsStick;
                GetToggle("ExplosionToggle").isOn = MySpell.IsExplosion;
                GetInput("ExplosionSizeInput").text = "" + MySpell.ExplosionSize;
                GetToggle("SeekToggle").isOn = MySpell.IsSeek;
                GetInput("SeekDistanceInput").text = "" + MySpell.SeekDistance;
                GetInput("SeekDelayInput").text = "" + MySpell.SeekDelay;
                GetToggle("ProjectileToggle").isOn = MySpell.IsProjectile;
                GetInput("FireRateInput").text = "" + MySpell.FireRate;
                GetInput("AccuracyInput").text = "" + MySpell.Randomness;
                GetInput("ForceInput").text = "" + MySpell.BulletForce;
                GetInput("LifeInput").text = "" + MySpell.LifeTime;
                GetInput("SizeInput").text = "" + MySpell.Size;
                MyColorPicker.SetColor(MySpell.ColorTint);
                StartCoroutine(UpdateDropdownLinks());
                CancelInvokes();
            }
        }

        private IEnumerator UpdateDropdownLinks()
        {
            yield return null;
            // Remove listeners
            GetDropdown("SpawnSoundDropdown").onValueChanged.RemoveAllListeners();
            GetDropdown("ImplodeSoundDropdown").onValueChanged.RemoveAllListeners();
            GetDropdown("ExplodeSoundDropdown").onValueChanged.RemoveAllListeners();
            GetDropdown("ClassDropdown").onValueChanged.RemoveAllListeners();
            GetDropdown("RaceDropdown").onValueChanged.RemoveAllListeners();
            yield return null;
            // set dropdown values
            int SpawnSoundFileIndex = DataManager.Get().GetFileIndex(DataFolderNames.Sounds, GetSelectedSpell().SpawnSoundName) + 1;
            Debug.LogError("Value setting SpawnSoundDropdown: " + SpawnSoundFileIndex);
            GetDropdown("SpawnSoundDropdown").value = SpawnSoundFileIndex;
            int ImplodeSoundFileIndex = DataManager.Get().GetFileIndex(DataFolderNames.Sounds, GetSelectedSpell().ImplodeSoundName);
            GetDropdown("ImplodeSoundDropdown").value = ImplodeSoundFileIndex + 1;
            int ExplodeSoundFileIndex = DataManager.Get().GetFileIndex(DataFolderNames.Sounds, GetSelectedSpell().ExplodeSoundName);
            GetDropdown("ExplodeSoundDropdown").value = ExplodeSoundFileIndex + 1;
            //int ClassIndex = DataManager.Get().GetFileIndex(DataFolderNames.Classes, GetSelectedSpell().MinionClass);
            //GetDropdown("ClassDropdown").value = ClassIndex + 1;
            int RaceIndex = DataManager.Get().GetFileIndex(DataFolderNames.Skeletons, GetSelectedSpell().MinionRace);
            GetDropdown("RaceDropdown").value = RaceIndex + 1;
            yield return null;
            // Re add listeners
            GetDropdown("SpawnSoundDropdown").onValueChanged.AddEvent(delegate { UseInput(GetDropdown("SpawnSoundDropdown")); });
            GetDropdown("ImplodeSoundDropdown").onValueChanged.AddEvent(delegate { UseInput(GetDropdown("ImplodeSoundDropdown")); });
            GetDropdown("ExplodeSoundDropdown").onValueChanged.AddEvent(delegate { UseInput(GetDropdown("ExplodeSoundDropdown")); });
            GetDropdown("ClassDropdown").onValueChanged.AddEvent(delegate { UseInput(GetDropdown("ClassDropdown")); });
            GetDropdown("RaceDropdown").onValueChanged.AddEvent(delegate { UseInput(GetDropdown("RaceDropdown")); });
        }
        #endregion

        #region UI

        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(InputField MyInputField)
        {
            base.UseInput(MyInputField);
            if (MyInputField.name == "SummonNameInput")
            {
                GetSelectedSpell().SetName(MyInputField.text);
            }
            else if (MyInputField.name == "ClassNameInput")
            {
                GetSelectedSpell().SetClass(MyInputField.text);
            }
            else if (MyInputField.name == "RaceNameInput")
            {
                GetSelectedSpell().SetRace(MyInputField.text);
            }
            else if (MyInputField.name == "AddStatNameInput")
            {
                GetSelectedSpell().SetAddStatName(MyInputField.text);
            }
            else if (MyInputField.name == "AddStatValueInput")
            {
                GetSelectedSpell().AddStatValue = float.Parse(MyInputField.text);
                MyInputField.text = GetSelectedSpell().AddStatValue + "";
            }
            else if (MyInputField.name == "UseStatNameInput")
            {
                GetSelectedSpell().StatUseName = MyInputField.text;
            }
            else if (MyInputField.name == "UseStatValueInput")
            {
                GetSelectedSpell().StatCost = float.Parse(MyInputField.text);
                MyInputField.text = GetSelectedSpell().StatCost + "";
            }
            // Function
            else if (MyInputField.name == "ExplosionSizeInput")
            {
                GetSelectedSpell().ExplosionSize = float.Parse(MyInputField.text);
                MyInputField.text = GetSelectedSpell().ExplosionSize + "";
            }
            else if (MyInputField.name == "SeekDistanceInput")
            {
                GetSelectedSpell().SeekDistance = float.Parse(MyInputField.text);
                MyInputField.text = GetSelectedSpell().SeekDistance + "";
            }
            else if (MyInputField.name == "SeekDelayInput")
            {
                GetSelectedSpell().SeekDelay = float.Parse(MyInputField.text);
                MyInputField.text = GetSelectedSpell().SeekDelay + "";
            }
            else if (MyInputField.name == "FireRateInput")
            {
                GetSelectedSpell().FireRate = float.Parse(MyInputField.text);
                MyInputField.text = GetSelectedSpell().FireRate + "";
            }
            else if (MyInputField.name == "AccuracyInput")
            {
                GetSelectedSpell().Randomness = float.Parse(MyInputField.text);
                MyInputField.text = GetSelectedSpell().Randomness + "";
            }
            else if (MyInputField.name == "ForceInput")
            {
                GetSelectedSpell().BulletForce = float.Parse(MyInputField.text);
                MyInputField.text = GetSelectedSpell().BulletForce + "";
            }
            else if (MyInputField.name == "LifeInput")
            {
                GetSelectedSpell().LifeTime = float.Parse(MyInputField.text);
                MyInputField.text = GetSelectedSpell().LifeTime + "";
            }
            else if (MyInputField.name == "SizeInput")
            {
                GetSelectedSpell().Size = float.Parse(MyInputField.text);
                MyInputField.text = GetSelectedSpell().Size + "";
            }
        }

        public override void UseInput(Button MyButton)
        {
            base.UseInput(MyButton);
            if (GetSelectedSpell() != null)
            {
                if (MyButton.name == "PlaySpawnSound")
                {
                    //GetComponent<AudioSource>().PlayOneShot(DataManager.Get().GetSound(DataFolderNames.Sounds, GetSelectedSpell().SpawnSoundName));
                }
                else if (MyButton.name == "PlayImplodeSound")
                {
                   // GetComponent<AudioSource>().PlayOneShot(DataManager.Get().GetSound(DataFolderNames.Sounds, GetSelectedSpell().ImplodeSoundName));
                }
                else if (MyButton.name == "PlayExplodeSound")
                {
                    //GetComponent<AudioSource>().PlayOneShot(DataManager.Get().GetSound(DataFolderNames.Sounds, GetSelectedSpell().ExplodeSoundName));
                }
            }
            else
            {
                Debug.LogError("Selected Spell is null.");
            }
        }
        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(Toggle MyToggle)
        {
            if (MyToggle.name == "SummonToggle")
            {
                GetSelectedSpell().IsSummon = MyToggle.isOn;
            }
            else if (MyToggle.name == "AddStatToggle")
            {
                GetSelectedSpell().IsAddStat = MyToggle.isOn;
            }
            else if (MyToggle.name == "UseStatToggle")
            {
                GetSelectedSpell().IsUseStat = MyToggle.isOn;
            }
            else if (MyToggle.name == "StickToggle")
            {
                GetSelectedSpell().IsStick = MyToggle.isOn;
            }
            else if (MyToggle.name == "ExplosionToggle")
            {
                GetSelectedSpell().IsExplosion = MyToggle.isOn;
            }
            else if (MyToggle.name == "SeekToggle")
            {
                GetSelectedSpell().IsSeek = MyToggle.isOn;
            }
            else if (MyToggle.name == "ProjectileToggle")
            {
                GetSelectedSpell().IsProjectile = MyToggle.isOn;
            }
            else if (MyToggle.name == "ChainToggle")
            {
                GetSelectedSpell().IsChain = MyToggle.isOn;
            }
        }

        /// <summary>
        /// use a drop down as input!
        /// </summary>
        public override void UseInput(Dropdown MyDropdown)
        {
            base.UseInput(MyDropdown);
            if (MyDropdown.name == "SpawnSoundDropdown")
            {
                Debug.LogError("Use Input SpawnSoundDropdown: " + MyDropdown.value + " / " + MyDropdown.options.Count);
                GetSelectedSpell().SetSpawnSoundName(MyDropdown.options[MyDropdown.value].text);
            }
            else if (MyDropdown.name == "ImplodeSoundDropdown")
            {
                GetSelectedSpell().SetImplodeSoundName(MyDropdown.options[MyDropdown.value].text);
            }
            else if (MyDropdown.name == "ExplodeSoundDropdown")
            {
                GetSelectedSpell().SetExplodeSoundName(MyDropdown.options[MyDropdown.value].text);
            }
            else if (MyDropdown.name == "ClassDropdown")
            {
                GetSelectedSpell().SetClass(MyDropdown.options[MyDropdown.value].text);
            }
            else if(MyDropdown.name == "RaceDropdown")
            {
                GetSelectedSpell().SetRace(MyDropdown.options[MyDropdown.value].text);
            }
        }
        /// <summary>
        /// Fill the drop downs with data
        /// </summary>
        public override void FillDropdown(Dropdown MyDropdown)
        {
            List<string> MyDropdownNames = new List<string>();
            if (MyDropdown.name == "SpawnSoundDropdown" || MyDropdown.name == "ImplodeSoundDropdown" || MyDropdown.name == "ExplodeSoundDropdown")
            {
                MyDropdownNames.Add("None");
                MyDropdownNames.AddRange(DataManager.Get().GetNames(DataFolderNames.Sounds));
                FillDropDownWithList(MyDropdown, MyDropdownNames);
            }
            else if (MyDropdown.name == "RaceDropdown")
            {
                MyDropdownNames.Add("None");
                MyDropdownNames.AddRange(DataManager.Get().GetNames(DataFolderNames.Skeletons));
                FillDropDownWithList(MyDropdown, MyDropdownNames);
            }
            else if (MyDropdown.name == "ClassDropdown")
            {
                MyDropdownNames.Add("None");
                //MyDropdownNames.AddRange(DataManager.Get().GetNames(DataFolderNames.Classes));
                FillDropDownWithList(MyDropdown, MyDropdownNames);
            }
        }
        /// <summary>
        /// Sets the colour of the spell
        /// </summary>
        /// <param name="MyColor"></param>
        public void SetColor(Color32 MyColor)
        {
            GetSelectedSpell().ColorTint = MyColor;
        }
        #endregion

        #region ImportExport
        /// <summary>
        /// Export the file using webgl
        /// </summary>
        public void Export()
        {
            //FileUtil.Export(GetSelectedName(), FileExtension, FileUtil.ConvertToSingle(GetSelected().GetScript()));
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
                Spell NewData = new Spell();
                NewData.RunScript(MyScript);
                AddData(UploadFileName, NewData);
            }
        }
        /// <summary>
        /// Add a new voxel to the game!
        /// </summary>
        public void AddData(string MyName, Spell NewData)
        {
            /*for (int i = 0; i < MyNames.Count; i++)
            {
                if (MyNames[i] == MyName)
                {
                    return;
                }
            }
            AddName(MyName);
            MySpells.Add(NewData);
            MyIndexController.SetMaxSelected(GetSize());*/
        }
        #endregion
    }
}