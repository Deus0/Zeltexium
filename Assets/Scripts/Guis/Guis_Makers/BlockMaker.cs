using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using Zeltex.Util;
using Zeltex.Voxels;
using Zeltex.Guis;

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// Handles making blocks 
    /// Meta data saved in alphabetical order as blocks use their id to get meta from the VoxelTileGenerator
    /// </summary>
    public class BlockMaker : ElementMakerGui
    {
        #region Variables
        [Header("BlockMaker")]  // idk why i have 3 references.. lol
        public VoxelManager MyVoxelManager;
        public VoxelPolygonViewer MyBlockViewer;
        //public TextureMaker MyTextureManager;
        //public PolygonMaker MyModelMaker;
        #endregion

        #region Files
        protected override void SetFilePaths()
        {
            DataManagerFolder = DataFolderNames.Voxels;
        }

        #endregion

        #region Data
        /*public int GetIndex(string MyName)
        {
            for (int i = 0; i < MyDatabase.Data.Count; i++)
            {
                if (MyName == MyDatabase.GetMeta(i).Name)
                {
                    return i;
                }
            }
            return 0;
        }*/

        public VoxelMeta GetSelectedVoxel()
        {
            return DataManager.Get().GetElement(DataManagerFolder, GetSelectedIndex()) as VoxelMeta;
        }
        #endregion

        // index control 
        #region IndexController
        
        /// <summary>
        /// Called by ZelGui class when turning on gui
        /// </summary>
        public override void OnBegin()
        {
            if (MyVoxelManager == null)
            {
                MyVoxelManager = VoxelManager.Get();
            }
            MyBlockViewer.OnBegin();
            base.OnBegin();
        }
        /// <summary>
        /// Called by ZelGui class when turning on gui
        /// </summary>
        public override void OnEnd()
        {
            base.OnEnd();
            MyBlockViewer.OnEnd();
        }
        /// <summary>
        /// Load Gui with Input
        /// Only gets called if index is updated
        /// </summary>
        public override void OnUpdatedIndex(int NewIndex)
        {
            base.OnUpdatedIndex(NewIndex);
            //Debug.Log("Updating Index in Block Maker.");
            GetInput("NameInput").text = GetSelectedVoxel().Name;
            GetLabel("VoxelNameLabel").text = GetSelectedVoxel().Name;
            GetInput("DescriptionInput").text = GetSelectedVoxel().GetDescription();
            GetInput("CommandsInput").text = "";
            for (int i = 0; i < GetSelectedVoxel().Commands.Count; i++)
            {
                GetInput("CommandsInput").text += GetSelectedVoxel().Commands[i] + "\n";
            }
            GetDropdown("TextureMapDropdown").onValueChanged = new Dropdown.DropdownEvent();
            GetDropdown("ModelDropdown").onValueChanged = new Dropdown.DropdownEvent();
            // for air disactivate the things! Default air block!
            if (NewIndex == 0)
            {
                GetDropdown("ModelDropdown").interactable = false;
                GetDropdown("TextureMapDropdown").interactable = false;
                GetInput("CommandsInput").interactable = false;
                GetDropdown("ModelDropdown").value = 0;
                GetDropdown("TextureMapDropdown").value = 0;
                GetInput("DescriptionInput").interactable = false;
                GetInput("DescriptionInput").text = "Air has no flavour.";
                GetInput("NameInput").interactable = false;
                MyBlockViewer.ClearMesh();
            }
            else
            {
                GetInput("NameInput").interactable = true;
                GetDropdown("ModelDropdown").interactable = true;
                GetDropdown("TextureMapDropdown").interactable = true;
                GetInput("CommandsInput").interactable = true;
                GetInput("DescriptionInput").interactable = true;
                GetDropdown("ModelDropdown").value = MyVoxelManager.GetModelIndex(GetSelectedVoxel().ModelID);
                //Debug.LogError("Setting voxel type to: " + GetSelected().TextureMapID);
                GetDropdown("TextureMapDropdown").value = GetSelectedVoxel().TextureMapID;
                MyBlockViewer.LoadVoxelMesh(GetSelectedVoxel());
            }
            //GetDropdown("TextureMapDropdown").onValueChanged = new Dropdown.DropdownEvent();
            GetDropdown("TextureMapDropdown").onValueChanged.AddEvent(delegate { UseInput(GetDropdown("TextureMapDropdown")); });
            GetDropdown("ModelDropdown").onValueChanged.AddEvent(delegate { UseInput(GetDropdown("ModelDropdown")); });
            CancelInvokes();
        }

        /// <summary>
        /// Index of where it is added
        /// </summary>
        protected override void AddData()
		{
			VoxelMeta NewVoxel = new VoxelMeta();
            DataManager.Get().AddElement(DataManagerFolder, NewVoxel);
        }

        /// <summary>
        /// When an index is removed
        /// </summary>
       /* protected override void RemovedData(int Index)
        {
            MyVoxelManager.RemoveMeta(Index);
        }*/
        #endregion

        #region UI
        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(InputField MyInputField)
        {
            base.UseInput(MyInputField);
            /*if (MyInputField.name == "NameInput")
            {
                //Rename(MyInputField.text);
                //MyInputField.text = GetSelectedName();
                //GetLabel("VoxelNameLabel").text = MyInputField.text;
            }
            else*/
            if (MyInputField.name == "DescriptionInput")
            {
                GetSelectedVoxel().SetDescription(MyInputField.text);
            }
            else if (MyInputField.name == "CommandsInput")
            {
                string[] MyCommands = MyInputField.text.Split('\n');
                GetSelectedVoxel().Commands.Clear();
                GetSelectedVoxel().Commands.AddRange(MyCommands);
            }
        }
        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(Dropdown MyDropdown)
        {
            if (MyDropdown.name == "ModelDropdown")
            {
                GetSelectedVoxel().ModelID = MyDropdown.options[MyDropdown.value].text;
                PolyModel MyModel = DataManager.Get().GetElement(DataFolderNames.PolyModels, GetSelectedVoxel().ModelID) as PolyModel;
                if (MyModel == null)
                {
                    GetSelectedVoxel().SetModelID("Block");
                    MyModel = VoxelManager.Get().GetModel("Block");
                }
                if (MyModel != null)
                {
                    GetSelectedVoxel().SetTextureMap(Mathf.Clamp(GetSelectedVoxel().TextureMapID, 0, MyModel.TextureMaps.Count - 1));
                }
                MyBlockViewer.LoadMeta(GetSelected());
                FillTextureMapDropdown();
            }
            if (MyDropdown.name == "TextureMapDropdown")
            {
                GetSelectedVoxel().SetTextureMap(MyDropdown.value);
                MyBlockViewer.LoadMeta(GetSelected());
            }
        }
        public override void FillDropdown(Dropdown MyDropdown)
        {
            List<string> MyNames = new List<string>();
            if (MyDropdown.name == "ModelDropdown")
            {
                for (int i = 0; i < MyVoxelManager.MyModels.Count; i++)
                {
                    MyNames.Add(MyVoxelManager.GetModel(i).Name);// MyModelMaker.MyDataBase.MyModels[i].name);
                }
                FillDropDownWithList(MyDropdown, MyNames);
            }
            if (MyDropdown.name == "TextureMapDropdown")
            {
                FillTextureMapDropdown();
            }
        }
        private void FillTextureMapDropdown()
        {
            List<string> MyNames = new List<string>();
            string ModelName = "Block";
            if (GetSelectedVoxel() != null)
            {
                ModelName = GetSelectedVoxel().ModelID;
            }
            PolyModel MyModel = DataManager.Get().GetElement(DataFolderNames.PolyModels, ModelName) as PolyModel;
            if (MyModel == null)
            {
                Debug.LogError(ModelName + " is null.");
                MyModel = VoxelManager.Get().GetModel("Block");
            }
            if (MyModel != null)
            {
                for (int i = 0; i < MyModel.TextureMaps.Count; i++)
                {
                    //MyNames.Add(MyModel.TextureMaps[i].GetTilemapNames());
                    MyNames.Add(MyModel.Name + "_TextureMap_" + i);
                }
            }
            FillDropDownWithList(GetDropdown("TextureMapDropdown"), MyNames);
        }
        #endregion

        #region Import

        public override void UseInput(Button MyButton)
        {
            if (MyButton.name == "ExportButton")
            {
                //FileUtil.Export(GetSelectedName(), FileExtension, GetSelected().GetScript());
            }
        }
        /// <summary>
        /// called on mouse down
        /// </summary>
        public void Import()
        {
            //FileUtil.Import(name, "Upload", FileExtension);
        }
        /// <summary>
        /// Called from javascript, uploading a model data
        /// </summary>
        public void Upload(string MyData)
        {
            string UploadFileName = "";
            for (int i = 0; i < MyData.Length; i++)
            {
                if (MyData[i] == '\n')
                {
                    UploadFileName = MyData.Substring(0, i);
                    UploadFileName = Path.GetFileNameWithoutExtension(UploadFileName);
                    MyData = MyData.Substring(i + 1);
                    break;
                }
            }
            if (UploadFileName != "")
            {
                Debug.Log("Uploading new voxel:" + UploadFileName + ":" + MyData.Length);
                VoxelMeta NewMeta = new VoxelMeta();
                NewMeta.RunScript(MyData);
                AddData(UploadFileName, NewMeta);
            }
        }
        /// <summary>
        /// Add a new voxel to the game!
        /// </summary>
        public void AddData(string MyName, VoxelMeta NewMeta)
        {
            if (MyVoxelManager.MyMetas.ContainsKey(MyName) == false)
            {
                AddName(MyName);
                NewMeta.Name = MyName;
                DataManager.Get().AddElement(DataManagerFolder, NewMeta);
                MyIndexController.SetMaxSelected(GetSize());
            }
            else
            {
                Debug.LogError(MyName + " already exists in database.");
            }
        }
        #endregion
    }
}


/// <summary>
/// Loads all the data
/// </summary>
/* public override void LoadAll()
 {
     base.LoadAll();
     Clear();
     List<string> MyFiles = FileUtil.GetFilesOfType(GetFilePath(), FileExtension);
     MyFiles = FileUtil.SortAlphabetically(MyFiles);
     //Debug.LogError("Loading " + MyFiles.Count + " Blocks from: " + GetFilePath());
     for (int i = 0; i < MyFiles.Count; i++)
     {
         string MyScript = FileUtil.Load(MyFiles[i]);
         VoxelMeta MyData = new VoxelMeta(MyScript);
         MyVoxelManager.Data.Add(MyData.Name, MyData);
         AddName(MyData.Name);
     }
     MyIndexController.SetMaxSelected(GetSize());
 }

 /// <summary>
 /// Saves all the quests
 /// </summary>
 public override void SaveAll()
 {
     string FolderPath = GetFilePath();
     Debug.Log("Saving all in Block Maker [" + FolderPath + "] Saving all of " + MyVoxelManager.Data.Count + " Voxels.");
     for (int i = 0; i < MyVoxelManager.Data.Count; i++)
     {
         VoxelMeta MyData = MyVoxelManager.GetMeta(i);
         List<string> MyScript = FileUtil.ConvertToList(MyData.GetScript());
         FileUtil.Save(FolderPath + MyData.Name + "." + FileExtension, FileUtil.ConvertToSingle(MyScript));   //"VoxelMeta_" + i +
     }
 }*/
/*

    
        /// <summary>
        /// Deletes the block from the data base and from file
        /// </summary>
        public void Delete()
        {
            string VoxelName = MyDatabase.Data[MyIndexController.SelectedIndex].Name;
            Debug.Log("Deleting Voxel [" + VoxelName + "]");
            string FullName = GetFilePath("" + MyIndexController.SelectedIndex);
            if (File.Exists(FullName))
            {
                File.Delete(FullName);
                FullName += ".meta";
                if (File.Exists(FullName))
                {
                    File.Delete(FullName);
                }
            }
            // Remove from database too
            MyDatabase.Data.RemoveAt(MyIndexController.SelectedIndex);
            OnUpdateSelected();
        }

        /// <summary>
        /// Creates a new block meta
        /// </summary>
        public string GetFilePath(int Index)
        {
            return FileUtil.GetFolderPath(FolderName) + "VoxelMeta_" + Index + "." + FileExtension;
            //return GetFolderPathStatic() + Name + "." + FileExtension;
        }

        public string GetFilePath(string Name)
        {
            return FileUtil.GetFolderPath(FolderName) + "VoxelMeta_" + Name + "." + FileExtension;
            //return GetFolderPathStatic() + Name + "." + FileExtension;
        }
        #endregion
        
        // The Data is a list of VoxelMeta
        #region Data
        /// <summary>
        /// Creates a new voxel meta and switches to it
        /// </summary>
        void New()
        {
            GetMetaData().Add(new VoxelMeta());
            SelectedIndex = GetMetaData().Count - 1;
            OnUpdateSelected();
        }

        /// <summary>
        /// returns the selected VoxelMeta
        /// </summary>
        public VoxelMeta GetSelectedMetaData()
        {
            int MetaDataCount = GetMetaData().Count;
            if (MetaDataCount == 0)
                return null;
            SelectedIndex = Mathf.Clamp(SelectedIndex, 0, MetaDataCount - 1);
            return GetMetaData()[SelectedIndex];
        }

        /// <summary>
        /// returns a list of Voxel Meta
        /// </summary>
        public List<VoxelMeta> GetMetaData()
        {
            return MyDatabase.Data;
        }

        #endregion

        // Move this into the index controller class
        #region IndexController
        public void Next()
        {
            SelectedIndex++;
            OnUpdateSelected();
        }
        public void Previous()
        {
            SelectedIndex--;
            OnUpdateSelected();
        }
        private void SetInteraction(bool NewState)
        {
            NameInput.interactable = NewState;
            DescriptionInput.interactable = NewState;
            ModelIDInput.interactable = NewState;
            TextureMapInput.interactable = NewState;
            CommandsInput.interactable = NewState;
        }
        /// <summary>
        /// Called when index changes
        /// to do:
        ///     - do the same thing as the model, where i turn off/on buttons
        ///     - use index controller instead
        /// </summary>
        void OnUpdateSelected()
        {
            int MetaDataCount = GetMetaData().Count;
            SelectedIndex = Mathf.Clamp(SelectedIndex, 0, MetaDataCount - 1);
            if (MetaDataCount == 0)
            {
                SetInteraction(false);
                SelectedLabel.text = "[---]";
                NameInput.text = "";
                DescriptionInput.text = "";
                ModelIDInput.text = "";
                TextureMapInput.text = "";
                CommandsInput.text = "";
            }
            else
            {
                SetInteraction(true);
                SelectedLabel.text = "[" + (SelectedIndex + 1) + " / " + MetaDataCount + "]";
                VoxelMeta MyMeta = GetSelectedMetaData();
                NameInput.text = MyMeta.Name;
                DescriptionInput.text = MyMeta.Description;
                ModelIDInput.text = "" + MyMeta.ModelID;
                TextureMapInput.text = "" + MyMeta.TextureMapID;
                CommandsInput.text = "";
                for (int i = 0; i < GetSelectedMetaData().Commands.Count; i++)
                {
                    CommandsInput.text += GetSelectedMetaData().Commands[i] + "\n";
                }
            }
        }
        #endregion

        // all these functions directly correspond from VoxelMeta -> Inputfield and vice versa
        #region GuiInput
        /// <summary>
        /// Update Voxel Name
        /// </summary>
        public void OnUpdatedName(InputField MyInput)
        {
            GetSelectedMetaData().Name = MyInput.text;
        }

        /// <summary>
        /// Update Voxel Description
        /// </summary>
        public void OnUpdatedDescription(InputField MyInput)
        {
            GetSelectedMetaData().Description = MyInput.text;
        }

        /// <summary>
        /// Update Voxel Model
        /// </summary>
        /// <param name="MyInput"></param>
        public void OnUpdatedModelID(InputField MyInput)
        {
            int NewModelID = int.Parse(MyInput.text);
            NewModelID = Mathf.Clamp(NewModelID, 0, MyDatabase.MyModels.Count - 1);
            GetSelectedMetaData().ModelID = NewModelID;
            MyInput.text = "" + NewModelID;
        }

        /// <summary>
        /// Update Voxel Texture Map
        ///     - To do: make actual texture maps, not just using auto gen ones
        /// </summary>
        public void OnUpdatedTextureMapID(InputField MyInput)
        {
            int NewModelID = int.Parse(MyInput.text);
            NewModelID = Mathf.Clamp(NewModelID, 0, MyDatabase.MyModels.Count - 1);
            GetSelectedMetaData().TextureMapID = NewModelID;
            MyInput.text = "" + NewModelID;
        }

        /// <summary>
        /// Update Voxel Commands
        ///     - To do: Make commands more
        ///     - use drop down to add, and a list of elements with a plus button
        /// </summary>
        public void OnUpdateCommands()
        {
            string[] MyCommands = CommandsInput.text.Split('\n');
            GetSelectedMetaData().Commands.Clear();
            GetSelectedMetaData().Commands.AddRange(MyCommands);
        }
        #endregion
/// <summary>
/// This is called via the ZelGui on begin event
/// </summary>
public void OnBegin()
{
    LoadAll();
}

/// <summary>
/// Save all the data
/// </summary>
public void SaveAll()
{
    for (int i = 0; i < MyDatabase.Data.Count; i++)
    {
        string MyFilePath = GetFilePath("" + i);
        File.WriteAllText(MyFilePath, MyDatabase.Data[i].GetScript());
    }
}

/// <summary>
/// Loads all the data
/// </summary>
public void LoadAll()
{
    //RefreshWorldPaths();
    string MyFolderPath = FileUtil.GetFolderPath(FolderName);
    if (Directory.Exists(MyFolderPath))
    {
        Debug.Log("Loading all Meta Data for Blocks. [" + Time.realtimeSinceStartup + "]");
        GetMetaData().Clear();
        List<string> MetaFiles = FileUtil.GetFilesOfType(MyFolderPath, FileExtension);
        //MetaFiles.Sort(CompareListByName);
        MetaFiles = FileUtil.SortAlphabetically(MetaFiles);
        Debug.Log("Total Meta Files [" + MetaFiles.Count + "] at time [" + Time.realtimeSinceStartup + "]");
        for (int i = 0; i < MetaFiles.Count; i++)
        {
            if (File.Exists(MetaFiles[i]))
            {
                string LoadedMeta = File.ReadAllText(MetaFiles[i]);
                GetMetaData().Add(new Zeltex.Voxels.VoxelMeta(LoadedMeta));
            }
        }
    }
    else
    {
        // Debug.LogError("No Meta Data path for blocks [" + VoxelMetaFilePath + "]");
    }
    OnUpdateSelected();
}
*/
