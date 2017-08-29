/*using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace PlayerGuiSystem
{
    //[ExecuteInEditMode]
    public class ClassMaker : MonoBehaviour
    {
        [Header("Debug")]
        public bool DebugLog;
        public bool DebugMode;
        [Header("Actions")]
        public bool IsGetWorldPath = false;
        public bool IsGetPath = false;
        public bool IsLoadScript = false;
        public bool IsClearScript = false;
        public bool IsGetClassesList = false;
        [Header("Data")]
        public string MyFilePath = "";
        public string CharacterClassName = "Minion";
        public string CharacterScript;
        int SelectedIndex = 0;
        public List<string> CharacterClassNames;
        [Header("References")]
        public InputField MyScriptText;
        public Text IndexLabel;
        public InputField NameInput;
        // hidden references
        private string WorldName;
        private GameObject MyWorld;
        bool IsUsingWorldPath = false;

        public void OnBegin()
        {
            RefreshFilePath();
            RefreshCharacterClassNames();
            SelectedIndex = 0;
            OnChangedClass();
        }
        void Log(string NewLog)
        {
            if (DebugLog)
            {
                Debug.Log(NewLog);
            }
        }
        // Update is called once per frame
        void Update()
        {
            if (IsGetClassesList)
            {
                IsGetClassesList = false;
                RefreshCharacterClassNames();
            }
            if (IsGetPath)
            {
                IsGetPath = false;
                MyFilePath = GetSaveFolderPath(false);
            }
            if (IsGetWorldPath)
            {
                IsGetWorldPath = false;
                MyFilePath = GetSaveFolderPath(true);
            }
            if (IsLoadScript)
            {
                IsLoadScript = false;
                Load();
            }
            if (IsClearScript)
            {
                IsClearScript = false;
                ClearScript();
            }
        }
        public void UseWorldPath()
        {
            IsUsingWorldPath = true;
            RefreshFilePath();
            RefreshCharacterClassNames();
        }
        public void UseDefaultPath()
        {
            IsUsingWorldPath = false;
            RefreshFilePath();
            RefreshCharacterClassNames();
        }

        void OnGUI()
        {
            if (DebugMode)
            {
                GUILayout.Label("FilePath [" + MyFilePath + "]");
            }
        }

        public void NewCharacterClass()
        {
            SelectedIndex = CharacterClassNames.Count;
            CharacterClassNames.Add("New Class");
            OnChangedClass();
            SaveScript();   // saves New Class to a file
        }

        public void OnChangedClass()
        {
            SelectedIndex = Mathf.Clamp(SelectedIndex, 0, CharacterClassNames.Count-1);
            if (IndexLabel)
            {
                if (CharacterClassNames.Count != 0)
                    IndexLabel.text = "[" + (SelectedIndex + 1) + "," + CharacterClassNames.Count + "]";
                else
                    IndexLabel.text = "[-/-]";
            }

            if (CharacterClassNames.Count != 0)
                CharacterClassName = CharacterClassNames[SelectedIndex];
            else
                CharacterClassName = "New Class";

            if (NameInput)
            {
                NameInput.text = CharacterClassName;
            }
            Load();
        }

        public void NextCharacterClass()
        {
            SelectedIndex++;
            OnChangedClass();
        }

        public void PreviousCharacterClass()
        {
            SelectedIndex--;
            OnChangedClass();
        }
        public void Delete()
        {
            // stub function for later
            DeleteFile();
            RefreshCharacterClassNames();   // otherwise it still selects the deleted class name
            OnChangedClass();
        }
        void DeleteFile()
        {
            FileUtil.DeleteText(GetFullFileName());
        }

        public void UpdateClassName(string NewClassName)
        {
            if (NewClassName == "")
                NewClassName = "New Class"; // need to add (i) iterations to the names
            if (CharacterClassName != NewClassName)
            {
                // get old file - delete it
                DeleteFile();
                // change name
                CharacterClassName = NewClassName;
                // save it to new file
                SaveScript();
                // update ui
                RefreshCharacterClassNames();
                // make sure the clss is still selected
                for (int i = 0; i < CharacterClassNames.Count; i++)
                {
                    if (CharacterClassNames[i] == CharacterClassName)
                    {
                        SelectedIndex = i;
                        break;
                    }
                }
                OnChangedClass();   // refreshes gui
            }
        }


        List<string> GetCharacterClassList(string DirectoryPath)
        {
            MyFilePath = GetSaveFolderPath(IsUsingWorldPath);   
            if (CharacterClassNames.Count == 0)
                RefreshCharacterClassNames();
            return CharacterClassNames;
        }

        // Class Name
        public string GetSelectedClassName(int NewIndex)
        {
            if (CharacterClassNames.Count == 0)
                return "New Class";
            NewIndex = Mathf.Clamp(NewIndex, 0, CharacterClassNames.Count - 1);
            return CharacterClassNames[NewIndex];
        }

        public string GetSelectedClassName()
        {
            return CharacterClassName;
        }

        // FILE PATH
        void RefreshFilePath()
        {
            MyFilePath = GetSaveFolderPath(IsUsingWorldPath);
        }
        public string GetFullFileName(int NewIndex)
        {
            return GetFullFileName(GetSelectedClassName(NewIndex));
        }
        public string GetFullFileName()
        {
            return GetFullFileName(CharacterClassName);
        }
        public string GetFullFileName(string MyClassName)
        {
            RefreshFilePath();
            string FullFileName = MyFilePath + MyClassName + ".txt";
            return FullFileName;
        }
        // used by RefreshFilePath
        public static string GetSaveFolderPath(bool IsWorldPath)
        {
            string FilePath = "";
            if (IsWorldPath)
            {
                //if (MyWorld == null)
                string WorldName = "";
                GameObject MyWorld = GameObject.Find("World");
                if (MyWorld)
                    WorldName = MyWorld.GetComponent<Zeltex.Voxels.VoxelSaver>().SaveFileName;
                if (MyWorld && WorldName != "")
                    FilePath = FileUtil.GetWorldFolderPath(WorldName);  //MyWorld.GetComponent<VoxelSaver>().SaveFileName
                else
                    return "";
            }
            else
            {
                FilePath = FileUtil.GetDefaultFolderPath();   // default classes folder
            }
            FilePath += "Classes/";
            if (!Directory.Exists(FilePath))
                Directory.CreateDirectory(FilePath);
            return FilePath;
        }
        // File Path for the list of names
        private void RefreshCharacterClassNames()
        {
            RefreshFilePath();
            RefreshCharacterClassNames(MyFilePath);
        }

        private void RefreshCharacterClassNames(string DirectoryPath)
        {
            CharacterClassNames.Clear();
            if (!Directory.Exists(DirectoryPath))
                return;
            string[] MyFiles = Directory.GetFiles(DirectoryPath);
            for (int i = 0; i < MyFiles.Length; i++)
            {
                if (MyFiles[i].Length >= 5 && MyFiles[i].Substring(MyFiles[i].Length - 4) == ".txt")
                {
                    int IndexBegin = MyFiles[i].IndexOf("Classes/") + "Classes/".Length;
                    string NewClassName = MyFiles[i].Substring(IndexBegin);
                    if (NewClassName.Contains(".txt"))
                        NewClassName = NewClassName.Remove(NewClassName.IndexOf(".txt"));
                    CharacterClassNames.Add(NewClassName);
                }
            }
        }

        // Script UI Interface
        void ClearScript()
        {
            if (MyScriptText)
            {
                MyScriptText.text = "";
            }
        }

        public void Load()
        {
            if (MyScriptText)
            {
                Log("[" + Time.time + "] Loading script: " + GetFullFileName());// + Time.realTimeSinceStartup);
                CharacterScript = FileUtil.LoadText(GetFullFileName());
                //Debug.LogError("Loading new Script [" + CharacterScript.Length + "] - [" + ((int)CharacterScript[CharacterScript.Length-1]) + "]");
                MyScriptText.text = CharacterScript;
                //Debug.Log("Text is - " + CharacterScript);
            }
        }
        public void SaveScript()
        {
            Debug.Log("Saving script: " + GetFullFileName());
            if (MyScriptText)
            {
                string MyScript = MyScriptText.text;
                //Debug.LogError("Saving new Script [" + MyScript.Length + "] - [" + ((int)MyScript[MyScript.Length-1]) + "]");
                //Debug.LogError((int)('\n') + ":");
                FileUtil.SaveText(GetFullFileName(CharacterClassName), MyScript);
            }
        }

        public void CloneDefaultScriptsToWorld()
        {
            string DefaultScriptsPath = GetSaveFolderPath(false);
            string WorldScriptsPath = GetSaveFolderPath(true);
            //GetCharacterClassList - list 1

            string[] MyFiles = Directory.GetFiles(DefaultScriptsPath);
            for (int i = 0; i < MyFiles.Length; i++)
            {
                if (MyFiles[i].Length >= 5 && MyFiles[i].Substring(MyFiles[i].Length - 4) == ".txt")    // if character file
                {
                    int IndexBegin = MyFiles[i].IndexOf("Classes/") + "Classes/".Length;
                    string FileName = MyFiles[i].Substring(IndexBegin);
                    File.Copy(MyFiles[i], WorldScriptsPath + FileName);
                }
            }
        }
        // Static Functions - used by summoning spell atm to update a character
        public static string GetFileName(string MyClassName)
        {
            string FullFileName = GetSaveFolderPath(true) + MyClassName + ".txt";
            return FullFileName;
        }
        public static List<string> GetClassNames()
        {
            List<string> MyClassNames = new List<string>();
            string DirectoryPath = GetSaveFolderPath(true);
            string[] MyFiles = Directory.GetFiles(DirectoryPath);
            for (int i = 0; i < MyFiles.Length; i++)
            {
                if (MyFiles[i].Length >= 5 && MyFiles[i].Substring(MyFiles[i].Length - 4) == ".txt")
                {
                    int IndexBegin = MyFiles[i].IndexOf("Classes/") + "Classes/".Length;
                    string NewClassName = MyFiles[i].Substring(IndexBegin);
                    if (NewClassName.Contains(".txt"))
                        NewClassName = NewClassName.Remove(NewClassName.IndexOf(".txt"));
                    MyClassNames.Add(NewClassName);
                }
            }
            return MyClassNames;
        }

        public void LoadDefaultScript()
        {
            string MyScript =
@"/id 1
/Character Yes Sir. Reporting for duty.
/Player Carry on.
/id 2
/Character ... You're not going to kill me, sir?
/Player Carry on....*Sharpens Blade*

/characterstats
Level,1
/statdata An overall indication of power.
Experience,0,10
/statdata Experience is giving for completing quests or slaying monsters.
Health,0,0
/statdata Lets you not die for longer
Mana,0,0
/statdata Allows you to manapulate magic
Energy,0,0
/statdata Lets you run through mountains
HealthRegen,Health,0,1
/statdata Regrows you into a stronger man
ManaRegen,Mana,0,1
/statdata Regrows you into a stronger man
EnergyRegen,Energy,0,1
/statdata Regrows you into a stronger man
Strength,5,Health,5
/statdata Lets you move mountains.
Vitality,5,HealthRegen,2
/statdata Let's you overcome unfavourable odds.
Intelligence,5,Mana,5
/statdata Strengthens your brainmuscle.
Wisdom,5,ManaRegen,2
/statdata Opens new paths for your future.
Agility,5,Energy,5
/statdata Makes your body move more efficiently.
Dexterity,5,EnergyRegen,2
/statdata Allows you to run for longer.
/endstats

/item Command
/description Let's you remember who you are. One who commands others.
/commands
/Commander
/endcommands
/texture DarkBall

/item Fireball
/description Magic flows through your veins, controlling it, transforming it into fire, you lunge it at your enemies.
/commands
/Fireball
/endcommands
/texture Burn

/item Dirt
/description A common material found on earth
/commands
/Block
/endcommands
/texture dirt

/item Sheild
/description Using mana to sheild your body
/commands
/Sheild
/endcommands
/texture sheild

/item Summoner
/description After unlocking the secrets of magic, you have gained the ability to use spatial magic. You use it to call forth the power of a level 1 demonic goblin.
/commands
/Summoner
/endcommands
/texture DemonGoblin
";
            MyScriptText.text = MyScript;
        }
    }

}
*/