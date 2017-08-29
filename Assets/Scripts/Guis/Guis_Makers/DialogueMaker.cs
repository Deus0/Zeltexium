using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Dialogue;
using System.IO;
using Zeltex.Util;

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// The main handler for dialogue data editing in game
    /// </summary>
    public class DialogueMaker : MakerGui
    {
        #region Variables
        [Header("Data")]
        public DialogueHandler MyDialogueHandler;
        //public List<string> MyData = new List<string>();
        //public List<string> MyDataNames = new List<string>();

        [Header("Index Control")]
        //public int SelectedLeafIndex;  // per tree branch
        //public int SelectedSpeechIndex;    // per speech line
        public IndexController LeafController;
        public IndexController SpeechController;
        public IndexController ActionController;
        public int SelectedConditionIndex = 0;
        #endregion

        #region ZelGui
        /// <summary>
        /// Called by ZelGui class when turning on gui
        /// </summary>
        public override void OnBegin()
        {
            base.OnBegin();
            InitiateLeafController();
            InitiateSpeechController();
            InitiateActionController();
            Load();
        }
        #endregion

        #region File
        /// <summary>
        /// Sets the file path for the files
        /// </summary>
        protected override void SetFilePaths()
        {
            DataManagerFolder = "Dialogues";
        }
        /// <summary>
        /// Load just a singular tree. 
        /// Converting the string script into the dialogue tree format.
        /// </summary>
        public override void Load()
        {
            Debug.Log("Loading In DialogueMaker: " + GetSize());
            if (GetSize() > 0)
            {
                MyDialogueHandler.MyTree.Clear();
				string MyScript = GetSelected();
				Debug.Log("Loading " + MyScript);
				MyDialogueHandler.MyTree.RunScript(FileUtil.ConvertToList(MyScript));
                FillDropdown(GetDropdown("NextIndexDropDown"));
                FillDropdown(GetDropdown("NextIndex2DropDown"));
            }
        }
        #endregion

        #region Data
        public string GetScript(string MyName)
        {
            /*for (int i = 0; i < MyNames.Count; i++)
            {
                if (MyNames[i] == MyName)
                {
                    return MyData[i];
                }
            }*/
            return "";
        }
        /// <summary>
        /// Clears all the data
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            MyDialogueHandler.MyTree.Clear();
        }
        /// <summary>
        /// Returns the selected dialogue Data
        /// </summary>
        DialogueData GetSelectedDialogue()
        {
            //Debug.LogError("Index is " + LeafController.SelectedIndex + " out of " + MyDialogueHandler.MyTree.GetSize());
            if (LeafController.SelectedIndex >= 0 && LeafController.SelectedIndex < GetSizeLeaf())
            {
                return MyDialogueHandler.MyTree.Get(LeafController.SelectedIndex);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Returns the selected dialogue condition
        /// </summary>
        public DialogueCondition GetSelectedCondition()
        {
            if (MyDialogueHandler.MyTree.GetSize() > 0 && GetSelectedDialogue().MyConditions.Count > 0 && 
                SelectedConditionIndex >= 0 && SelectedConditionIndex < GetSelectedDialogue().MyConditions.Count)
            {
                return GetSelectedDialogue().MyConditions[SelectedConditionIndex];
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region UI
        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(InputField MyInputField)
        {
            if (MyInputField.name == "LeafName")
            {
                string OldLeafName = GetSelectedDialogue().Name;
				GetSelectedDialogue().Name = MyInputField.text;
                // rename any old conditions to the new name
                for (int i = 0; i < MyDialogueHandler.MyTree.GetSize(); i++)
                {
                    DialogueData ThisLeaf = MyDialogueHandler.MyTree.GetDialogue(i);
                    for (int j = 0; j < ThisLeaf.MyConditions.Count; j++)
                    {
                        DialogueCondition MyCondition = ThisLeaf.MyConditions[j];
                        MyCondition.UpdateIndexes(OldLeafName, MyInputField.text);
                    }
                }
                FillDropdown(GetDropdown("NextIndexDropDown"));
                FillDropdown(GetDropdown("NextIndex2DropDown"));
            }
            else if (MyInputField.name == "NameInput")
            {
                MyInputField.text = Rename(MyInputField.text);
            }
            else if (MyInputField.name == "SpeechInput")
            {
                SetSelectedSpeechLine(MyInputField.text);
            }
            else if (MyInputField.name == "SpeakerInput")
            {
                SetSelectedSpeaker(MyInputField.text);
            }
        }

        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(Dropdown MyDropdown)
        {
            if (MyDropdown.name == "TextureDropdown")
            {
                /*string TextureName = MyDropdown.options[MyDropdown.value].text;
                Texture MyTexture = MyItemTextureManager.GetTexture(TextureName);
                if (MyTexture != null)
                {
                    GetSelected().SetTexture(MyTexture as Texture2D);
                    TextureImage.texture = GetSelected().GetTexture();
                }*/
            }
            else if (MyDropdown.name == "ActionDropdown")
            {
                GetSelectedDialogue().Actions[ActionController.GetSelectedIndex()].Name = MyDropdown.options[MyDropdown.value].text;
                // First turn all input off
                GetDropdown("QuestDropdown").gameObject.SetActive(false);
                // now turn input on per part of it
                if (GetSelectedDialogue().Actions[ActionController.GetSelectedIndex()].Name == "GiveQuest")
                {
                    GetDropdown("QuestDropdown").gameObject.SetActive(true);
                }
            }
            else if (MyDropdown.name == "QuestDropdown")
            {
				GetSelectedDialogue().Actions[ActionController.GetSelectedIndex()].Input1 = MyDropdown.options[MyDropdown.value].text;  // quest name
            }
        }
        public override void FillDropdown(Dropdown MyDropdown)  // when adding leaf, remember to refill drop down for next indexes
        {
            List<string> MyNames = new List<string>();
            if (MyDropdown.name == "ConditionsDropDown")    // fill with conditions
            {
                for (int i = 0; i < DialogueConditions.MyCommands.Length; i++)
                {
                    MyNames.Add(DialogueConditions.MyCommands[i]);
                }
            }
            else if (MyDropdown.name == "NextIndexDropDown" || MyDropdown.name == "NextIndex2DropDown") // fill with dialogue names
            {
				if (MyDialogueHandler)
				{
					for (int i = 0; i < MyDialogueHandler.MyTree.GetSize(); i++)
					{
						MyNames.Add(MyDialogueHandler.MyTree.Get(i).Name);
					}
				}
            }
            else if (MyDropdown.name == "QuestDropdown")
            {
                MyNames.Add("None");
               // MyNames.AddRange(MyQuestMaker.MyNames);
            }
            if (MyNames.Count > 0)
            {
                FillDropDownWithList(MyDropdown, MyNames);
            }
        }
        #endregion

        #region TreeIndexController
        /// <summary>
        /// When updated Tree Index
        /// </summary>
        public override void OnUpdatedIndex(int NewIndex)
        {
            Debug.Log(" Dialogue Maker: Updating Index to [" + NewIndex + "]");
            base.OnUpdatedIndex(NewIndex);
            if (MyIndexController.GetOldIndex() != NewIndex && MyIndexController.GetOldIndex() >= 0 && MyIndexController.GetOldIndex() < GetSize())   // will not work when opening for first time
            {
                string DialogueScript = FileUtil.ConvertToSingle(MyDialogueHandler.MyTree.GetScriptList());
                DataManager.Get().Set(DataManagerFolder, GetSelectedIndex(), DialogueScript);
                //Debug.Log("Storing Model as script to: " + MyIndexController.GetOldIndex() + " as moving to " + ":\n" + MyData[MyIndexController.GetOldIndex()]);
            }
            GetInput("NameInput").text = MyDialogueHandler.MyTree.Name;// MyNames[NewIndex];
            Load();
            LeafController.SetMaxSelected(GetSize());
            LeafController.ForceSelect(0);
        }

		/// <summary>
		/// Index of where it is added
		/// </summary>
		//protected override void AddData()
		//{
		//MyData.Add("");
		//AddName("Tree " + Mathf.RoundToInt(Random.Range(1,10000)));
		//MyDialogueHandler.MyTree.Clear();
		// base.OnAdd(NewIndex);
		//}

        #endregion

        #region SpeechController
        private void InitiateSpeechController()
        {
            if (SpeechController)
            {
                SpeechController.OnIndexUpdated.RemoveAllListeners();
                SpeechController.OnIndexUpdated.AddEvent(OnUpdatedIndexSpeech);
                SpeechController.OnAdd.AddEvent(OnAddSpeech);
                SpeechController.OnRemove.AddEvent(OnRemoveSpeech);
                SpeechController.OnListEmpty.AddEvent(OnListEmptySpeech);
                SpeechController.SetMaxSelected(GetSizeSpeech());
                SpeechController.OnBegin();
            }
        }
        private string GetSelectedSpeaker()
        {
            return GetSelectedDialogue().SpeechLines[SpeechController.SelectedIndex].Speaker;
        }
        private string GetSelectedSpeechLine()
        {
            return GetSelectedDialogue().SpeechLines[SpeechController.SelectedIndex].Speech;
        }
        private void SetSelectedSpeechLine(string NewSpeech)
        {
			GetSelectedDialogue().SpeechLines[SpeechController.SelectedIndex].Speech = NewSpeech;
        }
        private void SetSelectedSpeaker(string NewSpeaker)
        {
			GetSelectedDialogue().SpeechLines[SpeechController.SelectedIndex].Speaker = NewSpeaker;
        }
        /// <summary>
        /// return size of data (Leaf count)
        /// </summary>
        public int GetSizeSpeech()
        {
            if ( GetSelectedDialogue() == null) //GetSelected() == null ||
            {
                return 0;
            }
            else
            {
                return GetSelectedDialogue().SpeechLines.Count;
            }
        }
        /// <summary>
        /// Adds a new leaf onto the tree
        /// </summary>
        public void OnAddSpeech(int Index)
        {
			GetSelectedDialogue().SpeechLines.Add(new SpeechLine());
            SpeechController.SetMaxSelected(GetSizeSpeech());
            SpeechController.SelectIndex(GetSizeSpeech() - 1);
        }
        /// <summary>
        /// Removes the selected leaf
        /// </summary>
        public void OnRemoveSpeech(int Index)
        {
			GetSelectedDialogue().SpeechLines.RemoveAt(Index);
            SpeechController.SetMaxSelected(GetSizeSpeech());
            SpeechController.SelectIndex(Index - 1);
        }
        private void OnListEmptySpeech()
        {

        }
        /// <summary>
        /// When leaf has been updated
        /// </summary>
        public void OnUpdatedIndexSpeech(int NewIndex)
        {
            if (GetSelectedDialogue() != null)
            {
                GetInput("SpeechInput").text = GetSelectedSpeechLine();
                GetInput("SpeakerInput").text = GetSelectedSpeaker();
            }
        }
        #endregion

        #region LeafController
        private void InitiateLeafController()
        {
            if (LeafController)
            {
                LeafController.OnIndexUpdated.RemoveAllListeners();
                LeafController.OnIndexUpdated.AddEvent(OnUpdatedIndexLeaf);
                LeafController.OnAdd.AddEvent(OnAddLeaf);
                LeafController.OnRemove.AddEvent(OnRemoveLeaf);
                LeafController.OnListEmpty.AddEvent(OnListEmptyLeaf);
                LeafController.SetMaxSelected(GetSizeLeaf());
                LeafController.OnBegin();
            }
        }
        /// <summary>
        /// return size of data (Leaf count)
        /// </summary>
        public int GetSizeLeaf()
        {
            return MyDialogueHandler.MyTree.GetSize();
        }
        /// <summary>
        /// Adds a new leaf onto the tree
        /// </summary>
        public void OnAddLeaf(int Index)
        {
            MyDialogueHandler.MyTree.Add(
                new DialogueData(
                "" + (GetSizeLeaf() - 1),
                "" + GetSizeLeaf() )
            );
            LeafController.SetMaxSelected(GetSizeLeaf());
            LeafController.SelectIndex(GetSizeLeaf() - 1);
            FillDropdown(GetDropdown("NextIndexDropDown"));
            FillDropdown(GetDropdown("NextIndex2DropDown"));
        }
        /// <summary>
        /// Removes the selected leaf
        /// </summary>
        public void OnRemoveLeaf(int Index)
        {
            MyDialogueHandler.MyTree.RemoveAt(Index);
            LeafController.SetMaxSelected(GetSizeLeaf());
            LeafController.SelectIndex(LeafController.SelectedIndex - 1);
        }
        private void OnListEmptyLeaf()
        {

        }
        /// <summary>
        /// When leaf has been updated
        /// </summary>
        public void OnUpdatedIndexLeaf(int NewIndex)
        {
            if (GetSelectedDialogue() != null)
            {
                GetInput("LeafName").text = GetSelectedDialogue().Name;
                SpeechController.SetMaxSelected(GetSizeSpeech());
                SpeechController.ForceSelect(0);
                SelectedConditionIndex = 0;
                OnUpdatedCondition();
            }
        }
        #endregion

        #region ActionController
        private void InitiateActionController()
        {
            if (ActionController)
            {
                ActionController.OnIndexUpdated.RemoveAllListeners();
                ActionController.OnIndexUpdated.AddEvent(OnUpdatedIndexAction);
                ActionController.OnAdd.AddEvent(OnAddAction);
                ActionController.OnRemove.AddEvent(OnRemoveAction);
                ActionController.OnListEmpty.AddEvent(OnListEmptyAction);
                ActionController.SetMaxSelected(GetSizeAction());
                ActionController.OnBegin();
            }
        }
        private DialogueAction GetSelectedAction()
        {
            return GetSelectedDialogue().Actions[ActionController.SelectedIndex];
        }
        /// <summary>
        /// return size of data (Leaf count)
        /// </summary>
        public int GetSizeAction()
        {
            if (GetSelectedDialogue() != null && GetSelectedDialogue().Actions != null)
            {
                return GetSelectedDialogue().Actions.Count;
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// Adds a new leaf onto the tree
        /// </summary>
        public void OnAddAction(int Index)
        {
			GetSelectedDialogue().Actions.Add(new DialogueAction());
            ActionController.SetMaxSelected(GetSizeAction());
            ActionController.SelectIndex(GetSizeAction() - 1);
            GetDropdown("QuestDropdown").interactable = true;
            GetDropdown("ActionDropdown").interactable = true;
        }
        /// <summary>
        /// Removes the selected leaf
        /// </summary>
        public void OnRemoveAction(int Index)
        {
			GetSelectedDialogue().Actions.RemoveAt(Index);
            ActionController.SetMaxSelected(GetSizeAction());
            ActionController.SelectIndex(Index - 1);
        }
        private void OnListEmptyAction()
        {
            GetDropdown("QuestDropdown").interactable = false;
            GetDropdown("ActionDropdown").interactable = false;
        }
        /// <summary>
        /// When leaf has been updated
        /// </summary>
        public void OnUpdatedIndexAction(int NewIndex)
        {
            if (GetSelectedDialogue() != null)
            {
                Dropdown MyActionDropdown = GetDropdown("ActionDropdown");
                DialogueAction MyAction = GetSelectedAction();
                MyActionDropdown.value = 0;
                for (int i = 0; i < MyActionDropdown.options.Count; i++)
                {
                    if (MyActionDropdown.options[i].text == MyAction.Name)
                    {
                        MyActionDropdown.value = i;
                        break;
                    }
                }
                if (MyAction.Name == "GiveQuest")
                {
                    Dropdown MyQuestDropdown = GetDropdown("QuestDropdown");
                    MyQuestDropdown.value = 0;
                    /*for (int i = 0; i < MyQuestMaker.MyNames.Count; i++)
                    {
                        if (MyQuestMaker.MyNames[i] == MyAction.Input1)
                        {
                            MyQuestDropdown.value = i + 1;
                            break;
                        }
                    }*/
                }
                //GetInput("SpeechInput").text = GetSelectedSpeechLine();
                //GetInput("SpeakerInput").text = GetSelectedSpeaker();
            }
        }
        #endregion

        #region Condition
        public void NextCondition()
        {
            SelectedConditionIndex++;
            OnUpdatedCondition();
        }
        public void PreviousCondition()
        {
            SelectedConditionIndex--;
            OnUpdatedCondition();
        }
        // updates the data
        public void NewCondition()
        {
            if (GetSelectedDialogue() != null)
            {
			    GetSelectedDialogue().MyConditions.Add(new DialogueCondition("default", ""));
                SelectedConditionIndex = GetSelectedDialogue().MyConditions.Count - 1;
                OnUpdatedCondition();
            }
        }
        public void RemoveCondition()
        {
            if (GetSelectedDialogue().MyConditions.Count > 0)
            {
				GetSelectedDialogue().MyConditions.RemoveAt(SelectedConditionIndex);
                SelectedConditionIndex--;
                OnUpdatedCondition();
            }
        }

        /// <summary>
        /// Called when loading new Condition
        /// </summary>
        void OnUpdatedCondition()
        {
            if (GetSelectedDialogue() != null)
            {
                Dropdown NextDropDown = GetDropdown("NextIndexDropDown");
                Dropdown NextDropDown2 = GetDropdown("NextIndex2DropDown");
                Dropdown ConditionsDropDown = GetDropdown("ConditionsDropDown");

                ConditionsDropDown.interactable = false;
                SelectedConditionIndex = Mathf.Clamp(SelectedConditionIndex, 0, GetSelectedDialogue().MyConditions.Count - 1);
                if (SelectedConditionIndex == -1)
                {
                    SelectedConditionIndex = 0;
                }
                if (GetSelectedDialogue().MyConditions.Count == 0)
                {
                    GetLabel("IndexLabelCondition").text = "-";
                    GetButton("PreviousCondition").interactable = false;
                    GetButton("NextCondition").interactable = false;
                    ConditionsDropDown.value = 0;
                    ConditionsDropDown.CancelInvoke();
                    NextDropDown.interactable = false;
                    NextDropDown2.interactable = false;
                    NextDropDown2.gameObject.SetActive(false);
                    return;
                }
                else
                {
                    GetLabel("IndexLabelCondition").text = "" + (1 + SelectedConditionIndex);
                    NextDropDown.interactable = true;
                    NextDropDown2.interactable = true;

                    ConditionsDropDown.value = GetConditionsValue(GetSelectedCondition().Command);
                    StartCoroutine(DoTheThen());

                    if (GetSelectedCondition().NextIndexes.Count > 0)
                    {
                        NextDropDown2.gameObject.SetActive(true);
                        NextDropDown.value = GetDialogueValue(GetSelectedCondition().NextIndexes[0]);
                        NextDropDown.CancelInvoke();
                        NextDropDown2.value = GetDialogueValue(GetSelectedCondition().NextIndexes[1]);
                        NextDropDown2.CancelInvoke();
                    }
                    else
                    {
                        NextDropDown2.gameObject.SetActive(false);
                        NextDropDown.value = GetDialogueValue(GetSelectedCondition().NextIndex);
                        NextDropDown.CancelInvoke();
                    }
                    // Index Buttons
                    GetButton("PreviousCondition").interactable = (SelectedConditionIndex != 0);
                    GetButton("NextCondition").interactable = (SelectedConditionIndex != GetSelectedDialogue().MyConditions.Count - 1);
                }
            }
        }   
        IEnumerator DoTheThen()
        {
            yield return new WaitForSeconds(0.1f);
            GetDropdown("ConditionsDropDown").interactable = true;
        }
        public void OnUpdatedCondition(int NewCondition)
        {
            Dropdown NextDropDown = GetDropdown("NextIndexDropDown");
            Dropdown NextDropDown2 = GetDropdown("NextIndex2DropDown");
            Dropdown ConditionsDropDown = GetDropdown("ConditionsDropDown");
            string NewCommand = ConditionsDropDown.options[NewCondition].text;
            DialogueCondition MySelectedCondition = GetSelectedCondition();
            if (MySelectedCondition != null && MySelectedCondition.Command != NewCommand)
            {
                string OldCommand = GetSelectedCondition().Command;
                if (OldCommand == "options")
                {
                    GetSelectedCondition().NextIndexes.Clear();
                    NextDropDown2.gameObject.SetActive(false);
                }
                GetSelectedCondition().Command = NewCommand;
                if (NewCommand == "options")
                {
                    GetSelectedCondition().NextIndexes.Clear();
                    GetSelectedCondition().NextIndexes.Add(GetSelectedCondition().NextIndex);
                    GetSelectedCondition().NextIndexes.Add("End");
                    NextDropDown2.gameObject.SetActive(true);
                    //FillWithDialogueNames(MyNextDropDown2);
                    NextDropDown2.value = GetDialogueValue(GetSelectedCondition().NextIndexes[1]);
                }
            }
        }
        public int GetConditionsValue(string MyConditionName)
        {
            for (int i = 0; i < DialogueConditions.MyCommands.Length; i++)
            {
                if (DialogueConditions.MyCommands[i] == MyConditionName)
                {
                    return i;
                }
            }
            if ("End" == MyConditionName)
            {
                return DialogueConditions.MyCommands.Length;
            }
            return 0;
        }
        public int GetDialogueValue(string MyDialogueName)
        {
            if (MyDialogueName == "End")
            {
                return MyDialogueHandler.MyTree.GetSize();
            }
            for (int i = 0; i < MyDialogueHandler.MyTree.GetSize(); i++)
            {
                if (MyDialogueName == MyDialogueHandler.MyTree.Get(i).Name)
                {
                    return i;
                }
            }
            return 0;
        }
        public void UpdateNextIndex(string MyInput)
        {
            if (GetSize() > 0)
                if (GetSelectedCondition() != null)
                    GetSelectedCondition().NextIndex = (MyInput);
        }
        public void UpdateNextIndex2(string MyInput)
        {
            if (GetSize() > 0)
                if (GetSelectedCondition() != null)
                    GetSelectedCondition().NextIndexes[1] = (MyInput);
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
                //DialogueData NewData = new Quest();
               // NewData.RunScript(FileUtil.ConvertToList(MyScript));
                AddData(UploadFileName, MyScript);
            }
        }
        /// <summary>
        /// Add a new voxel to the game!
        /// </summary>
        public void AddData(string MyName, string NewData)
        {
            /*for (int i = 0; i < MyNames.Count; i++)
            {
                if (MyNames[i] == MyName)
                {
                    return;
                }
            }*/
            AddName(MyName);
            //MyData.Add(NewData);
            MyIndexController.SetMaxSelected(GetSize());
        }
        #endregion
    }
}


/// <summary>
/// Loads all the data
/// </summary>
/*public override void LoadAll()
{
    base.LoadAll();
    Clear();
    string MyPath = FileUtil.GetFolderPath(FolderName);
    if (Directory.Exists(MyPath))
    {
        //Debug.Log("Loading all Meta Data for Blocks. [" + Time.realtimeSinceStartup + "]");
        List<string> MetaFiles = FileUtil.GetFilesOfType(MyPath, FileExtension);
        //MetaFiles.Sort(CompareListByName);
        MetaFiles = FileUtil.SortAlphabetically(MetaFiles);
        //Debug.Log("Total Meta Files [" + MetaFiles.Count + "] at time [" + Time.realtimeSinceStartup + "]");
        for (int i = 0; i < MetaFiles.Count; i++)
        {
            if (File.Exists(MetaFiles[i]))
            {
                AddName(Path.GetFileNameWithoutExtension(MetaFiles[i]));
                string LoadedMeta = FileUtil.Load(MetaFiles[i]);
                MyData.Add(LoadedMeta);
            }
        }
    }
    else
    {
        // Debug.LogError("No Meta Data path for blocks [" + VoxelMetaFilePath + "]");
    }
}
/// <summary>
/// Saves the current tree
/// </summary>
public override void Save()
{
    Debug.Log("Saving New Dialogue: " + GetSelected().Name);
    List<string> MyScript = MyDialogueHandler.MyTree.GetScriptList();
    string MyScriptSingle = FileUtil.ConvertToSingle(MyScript);
    FileUtil.Save(
        FileUtil.GetFolderPath(FolderName) + "DialogueTree_" + MyIndexController.SelectedIndex + "." + FileExtension,
        MyScriptSingle);
}
/// <summary>
/// Saves all the DialogueTrees
/// </summary>
public override void SaveAll()
{
    if (MyData.Count > 0)
    {
        List<string> MyScript = MyDialogueHandler.MyTree.GetScriptList();
        MyData[MyIndexController.SelectedIndex] = FileUtil.ConvertToSingle(MyScript);
        string MyFolderName = FileUtil.GetFolderPath(FolderName);
        for (int i = 0; i < MyData.Count; i++)
        {
            string FilePart = MyNames[i] + "." + FileExtension;
            string FileName = MyFolderName + FilePart;
            File.WriteAllText(FileName, MyData[i]);
        }
    }
}*/
