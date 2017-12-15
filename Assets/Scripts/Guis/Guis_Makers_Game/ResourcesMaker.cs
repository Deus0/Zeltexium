using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zeltex.Util;
using Zeltex.Generators;

namespace Zeltex.Guis.Maker
{
    // Creating a new game:
    // First Terrain
    // it has just the seed input, for basic
    // Can add in Advanced Terrain Settings later
    // Game starts in a town, generate the town too and the maps

    /// <summary>
    /// Handles player saves, after begin game screen it comes here
    ///  Save game has:
    ///     Level I was last in (load it)
    ///     Characters I can play as (one for now)
    ///     [Future] Edits to the levels i've made
    ///     [Future] Level can be just be generation options (level blueprints)
    /// </summary>
    public class ResourcesMaker : GuiBasic
    {
        [Header("Resources")]
        public UnityEvent OnConfirmEvent;    // on confirm, normally, close the gui, and open the main menu
        public TabManager MainTabs;
        public TabManager EditTabs;
        public ResourcesFileHandler MyFileHandler;
        private string BeforeCreationResoucesName;
        private string BeforeEditedResourcesName;
        private UnityAction OnUpdatedResources;
        private string BeforeEnableResoucesName;

        #region ZelGui

        private void OnEnable()
        {
            BeforeEnableResoucesName = DataManager.Get().MapName;
        }

        /// <summary>
        /// Finish using the ResourcesMaker
        /// </summary>
        private void OnConfirm()
        {
            OnConfirmEvent.Invoke();
        }

        public override void OnBegin()
        {
            //GetToggle("PersistentPathToggle").isOn = (FileUtil.MyFilePathType == FilePathType.PersistentPath);
            GetDropdown("PathTypeDropdown").value = (int)DataManager.Get().MyFilePathType;
            RefreshList();
            OnUpdatedResourceStatistics();
            OnUpdatedResources = OnUpdatedResourceStatistics;
            DataManager.Get().OnUpdatedResources.RemoveListener(OnUpdatedResources);
            DataManager.Get().OnUpdatedResources.AddEvent(OnUpdatedResources);
            MainTabs.EnableTab("EditTab");
            EditTabs.EnableTab("Tab1");
            MyFileHandler.OpenFoldersList();
        }

        public override void OnEnd()
        {
            base.OnEnd();
            DataManager.Get().OnUpdatedResources.RemoveListener(OnUpdatedResources);
        }

        private void OnUpdatedResourceStatistics()
        {
            Debug.Log("Updating resources statistics.");
            string MyStatistics = DataManager.Get().GetStatistics();
            GetLabel("CreationResourceStatistics").text = MyStatistics;
            GetLabel("EditingResourceStatistics").text = MyStatistics;
        }

        #endregion

        #region Resource Folders

        /// <summary>
        /// Gets a list of the folders in the resources path and adds them to the list
        /// TODO: Sort them by last accessed
        /// </summary>
        private void RefreshList()
        {
            GuiList MyList = GetListHandler("MyList");
            List<string> MyNames = GetResourceNames();
            MyList.Clear();
            MyList.AddRange(MyNames);
            if (DataManager.Get().MapName != "")
            {
                MyList.Select(DataManager.Get().MapName);
            }
            else
            {
                MyList.Select(0);
            }
            GetLabel("ResourcesDirectory").text = DataManager.Get().GetResourcesPath();
        }

        public static List<string> GetResourceNames()
        {
            List<string> MyNames = new List<string>();
            string MyFolderPath = DataManager.Get().GetResourcesPath();    // get folder path
            string[] MyDirectories = FileManagement.ListDirectories(MyFolderPath);
            for (int i = 0; i < MyDirectories.Length; i++)
            {
                string MyPath = Path.GetFileName(MyDirectories[i]);
                if (MyPath != "Unity")
                {
                    MyNames.Add(MyPath);
                }
            }
            return MyNames;
        }

        #endregion

        #region Files

        private void LoadResources()
        {
            // on load
            DataManager.Get().UnloadAll();
            DataManager.Get().LoadResources(GetList("MyList").GetSelectedName());
            OnConfirm();
        }

        private void EditResources()
        {
            BeforeEditedResourcesName = DataManager.Get().MapName;
            MyFileHandler.OpenFoldersList();
            string LoadName = GetList("MyList").GetSelectedName();
            GetLabel("ResourcesName").text = LoadName;
            Debug.Log("Editing Resources: New: " + LoadName + " - Previous: " + BeforeEditedResourcesName);
            if (DataManager.Get().MapName != LoadName)
            {
                DataManager.Get().UnloadAll();
                DataManager.Get().LoadResources(LoadName);
            }
            GetInput("EditingNameInput").text = DataManager.Get().MapName;
            MainTabs.EnableTab("EditTab");
        }

        private void CancelEdited()
        {
            MainTabs.EnableTab("SelectionTab");
            Debug.Log("Canceling editing: Current: " + DataManager.Get().MapName + " - Previous: " + BeforeEditedResourcesName);
            if (DataManager.Get().MapName != BeforeEditedResourcesName)
            {
                DataManager.Get().UnloadAll();
                DataManager.Get().LoadResources(BeforeEditedResourcesName);
            }
        }

        private void DuplicateResources()
        {
            string Name1 = GetList("MyList").GetSelectedName();
            string Name2 = NameGenerator.GenerateVoxelName();
            string Path1 = DataManager.Get().GetResourcesPath() + Name1 + "/";
            string Path2 = DataManager.Get().GetResourcesPath() + Name2 + "/";
            if (Directory.Exists(Path1) && !Directory.Exists(Path2))
            {
                DirectoryCopy(Path1, Path2);
                GetList("MyList").Add(Name2);
            }
        }

        private void DuplicateResourcesToPersistent()
        {
            if (DataManager.Get().MyFilePathType == FilePathType.StreamingPath)
            {
                string Name1 = GetList("MyList").GetSelectedName();
                string Name2 = NameGenerator.GenerateVoxelName();
                string Path1 = DataManager.Get().GetResourcesPath() + Name1 + "/";
                FileUtil.SetPersistentPath(FilePathType.PersistentPath);
                string Path2 = DataManager.Get().GetResourcesPath() + Name2 + "/";
                if (Directory.Exists(Path1) && !Directory.Exists(Path2))
                {
                    DirectoryCopy(Path1, Path2);
                    //GetList("MyList").Add(Name2);
                    RefreshList();
                }
                else
                {
                    FileUtil.SetPersistentPath(FilePathType.StreamingPath);
                    GetList("MyList").Select(Name2);
                }
            }
        }

        private void ConfirmCreate()
        {
            // load current loaded one
            DataManager.Get().SaveAll();
            MainTabs.EnableTab("SelectionTab");
            GetList("MyList").Add(DataManager.Get().MapName);
            GetList("MyList").Select(DataManager.Get().MapName);
        }

        private void CancelCreate()
        {
            DataManager.Get().EraseResouces(DataManager.Get().MapName);
            DataManager.Get().UnloadAll();
            DataManager.Get().LoadResources(BeforeCreationResoucesName);
            MainTabs.EnableTab("SelectionTab");
        }

        private void CreateResources()
        {
            BeforeCreationResoucesName = DataManager.Get().MapName;
            // change name to ..! blank load!
            string NewName = "Resources" + Random.Range(1, 100000);
            if (DataManager.Get().CreateResources(NewName))
            {
                MainTabs.EnableTab("CreationTab");
                GetInput("NameInput").text = NewName;
            }
        }

        #endregion


        #region UI

        public override void UseInput(Dropdown MyDropdown)
        {
            base.UseInput(MyDropdown);
            if (MyDropdown.name == "PathTypeDropdown")
            {
                if (FileUtil.SetPersistentPath((FilePathType)MyDropdown.value))
                {
                    RefreshList();
                }
            }
        }

        public override void UseInput(InputField MyInputField)
        {
            if (MyInputField.name == "NameInput")
            {
                MyInputField.text = DataManager.Get().RenameResources(MyInputField.text);
            }
            else if (MyInputField.name == "EditingNameInput")
            {
                MyInputField.text = DataManager.Get().RenameResources(MyInputField.text);
            }
        }

        public override void UseInput(Button MyButton)
        {
            base.UseInput(MyButton);
            if (MyButton.name == "CreateButton")
            {
                CreateResources();
            }
            else if (MyButton.name == "CancelCreateButton")
            {
                CancelCreate();
            }
            else if (MyButton.name == "ConfirmCreateButton")
            {
                ConfirmCreate();
            }

            else if (MyButton.name == "GenerateButton")
            {
                MapGenerator.Get().GenerateMapAndSave();
            }
            else if (MyButton.name == "DuplicateButton")
            {
                DuplicateResources();
            }
            else if (MyButton.name == "DuplicateToPersistent")
            {
                DuplicateResourcesToPersistent();
            }
            else if (MyButton.name == "EraseButton")
            {
                // are you sure tab! Yes, No!
                if (DataManager.Get().EraseResouces(GetList("MyList").GetSelectedName()))
                {
                    GetList("MyList").RemoveSelected();
                }
            }
            else if (MyButton.name == "EraseYesButton")
            {
                // Delete the selected level
            }
            else if (MyButton.name == "EraseNoButton")
            {
                // Go Back to main tab!
            }

            else if (MyButton.name == "EditButton")
            {
                EditResources();
            }
            else if (MyButton.name == "BackEdited")
            {
                CancelEdited();
            }
            else if (IsMakerButton(MyButton))
            {
            }
            else if (MyButton.name == "LoadButton")
            {
                LoadResources();
            }
            else if (MyButton.name == "PlayEdited")
            {
                OnConfirm();
            }
            else if (MyButton.name == "SaveAllButton")
            {
                DataManager.Get().SaveAll();
            }
        }
        
        private bool IsMakerButton(Button MyButton)
        {
            bool IsMaker = MyButton.name.Contains("Maker");

            if (IsMaker)
            {
                GuiSpawner.Get().SpawnMakerGui(MyButton.name);
            }
            return IsMaker;
        }

        #endregion

        #region Utility
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
        #endregion
    }
}

/* if (MyButton.name == "TextureMaker")
 {
     PreviousMakerGui = GuiSpawner.Get().SpawnGui("TextureMaker");
 }
 else if (MyButton.name == "PolygonMaker")
 {
     PreviousMakerGui = GuiSpawner.Get().SpawnGui("PolygonMaker");
 }
 else if (MyButton.name == "ModelMaker")
 {
     PreviousMakerGui = GuiSpawner.Get().SpawnGui("ModelMaker");
 }
 else if (MyButton.name == "SkeletonMaker")
 {
     PreviousMakerGui = GuiSpawner.Get().SpawnGui("SkeletonMaker");
 }
 else if (MyButton.name == "SoundMaker")
 {
     PreviousMakerGui = GuiSpawner.Get().SpawnGui("SoundMaker");
 }
 else if (MyButton.name == "LevelMaker")
 {
     PreviousMakerGui = GuiSpawner.Get().SpawnGui("LevelMaker");
 }
 else if (MyButton.name == "ClassMaker")
 {
     PreviousMakerGui = GuiSpawner.Get().SpawnGui("ClassMaker");
 }
 else if (MyButton.name == "DialogueMaker")
 {
     PreviousMakerGui = GuiSpawner.Get().SpawnGui("DialogueMaker");
 }
 else if (MyButton.name == "ItemMaker")
 {
     PreviousMakerGui = GuiSpawner.Get().SpawnGui("ItemMaker");
 }
 else if (MyButton.name == "QuestMaker")
 {
     PreviousMakerGui = GuiSpawner.Get().SpawnGui("QuestMaker");
 }
 else if (MyButton.name == "RecipeMaker")
 {
     PreviousMakerGui = GuiSpawner.Get().SpawnGui("RecipeMaker");
 }
 else if (MyButton.name == "SpellMaker")
 {
     PreviousMakerGui = GuiSpawner.Get().SpawnGui("SpellMaker");
 }
 else if (MyButton.name == "StatsMaker")
 {
     PreviousMakerGui = GuiSpawner.Get().SpawnGui("StatsMaker");
 }
 else if (MyButton.name == "VoxelMaker")
 {
     PreviousMakerGui = GuiSpawner.Get().SpawnGui("VoxelMaker");
 }
 else
 {
     return false;
 }*/
/// <summary>
/// Creates a new Resources Pack!
/// </summary>
/*public void Create()
{
    GuiList MyList = GetListHandler("MyList");
    string NewName = "Resources" + Random.Range(1, 100000);
    string NewDirectory = FileUtil.GetResourcesPath() + NewName + "/";
    if (Directory.Exists(NewDirectory) == false)
    {
        Directory.CreateDirectory(NewDirectory);
        MyList.Add(NewName);
    }
    MyList.Select(MyList.GetSize() - 1);
    FileUtil.MapName = NewName;
}*/

/* public void CreateDefault()
 {
     Create();
     MapGenerator.Get().GenerateMapAndSave();
 }*/
