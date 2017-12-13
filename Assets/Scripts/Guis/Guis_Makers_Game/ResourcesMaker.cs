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
        private string BeforeCreationResoucesName;
        private string BeforeEditedResourcesName;
        private UnityAction OnUpdatedResources;
        private bool IsInFolder = false;
        private string SelectedFolderName = "";
        private string BeforeEnableResoucesName;
        // Keep track of the actions for file events
        private List<UnityAction<Element>> FileActionsModified = new List<UnityAction<Element>>();
        private List<UnityAction<Element>> FileActionsSaved = new List<UnityAction<Element>>();

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
            OpenFoldersList();
        }

        public override void OnEnd()
        {
            base.OnEnd();
            DataManager.Get().OnUpdatedResources.RemoveListener(OnUpdatedResources);
        }

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

        private void OnUpdatedResourceStatistics()
        {
            Debug.Log("Updating resources statistics.");
            string MyStatistics = DataManager.Get().GetStatistics();
            GetLabel("CreationResourceStatistics").text = MyStatistics;
            GetLabel("EditingResourceStatistics").text = MyStatistics;
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
            OpenFoldersList();
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

        #region FileList

        /// <summary>
        /// When user clicks on a list element
        /// </summary>
        public void OnClickFile(int FileIndex)
        {
            GuiList FilesList = GetList("FilesList");
            if (IsInFolder == false)
            {
                CloseFoldersList(FilesList);
                OpenFilesList(FilesList); 
            }
            else if (IsInFolder == true)
            {
                // return to main folders
                if (FileIndex == 0)
                {
                    CloseFilesList();
                    // Now Clear the list and add folders again
                    OpenFoldersList();
                }
                else if (FileIndex == 1)
                {
                    GameObject MakerGui = GuiSpawner.Get().SpawnGui(DataFolderNames.FolderToGuiName(SelectedFolderName));
                    if (MakerGui)
                    {
                        MakerGui MyMaker = MakerGui.GetComponent<MakerGui>();
                        TextureMaker MyTextureMaker = MyMaker.GetComponent<TextureMaker>();
                        if (MyTextureMaker)
                        {
                            MyTextureMaker.SetFolder(SelectedFolderName);
                        }
                        MyMaker.New();
                    }
                    else
                    {
                        Debug.LogError("Failed to open: " + SelectedFolderName);
                    }
                }
                else
                {
                    MakerGui MyMaker = GuiSpawner.Get().SpawnGui(DataFolderNames.FolderToGuiName(SelectedFolderName)).GetComponent<MakerGui>();
                    TextureMaker MyTextureMaker = MyMaker.GetComponent<TextureMaker>();
                    if (MyTextureMaker)
                    {
                        MyTextureMaker.SetFolder(SelectedFolderName);
                    }
                    MyMaker.Select(FileIndex - 2);
                    Debug.Log("Spawned maker: " + SelectedFolderName + " to select: " + (FileIndex - 2));
                }
            }
            Debug.Log("On Clicked file: " + FileIndex + "- IsInFolder: " + IsInFolder.ToString());
        }


        /// <summary>
        /// Adds the folder names and events
        /// </summary>
        private void OpenFoldersList()
        {
            IsInFolder = false;
            GuiList FilesList = GetList("FilesList");
            FilesList.Clear();  // clear any files remaining, or when on begin
            List<string> FolderNames = DataManager.Get().GetFolderNames();
            FilesList.AddRange(FolderNames);
            for (int i = 0; i < FolderNames.Count; i++)
            {
                // if modified element, add to folder
                GuiListElementFile MyElementFile = FilesList.GetCell(i).GetComponent<GuiListElementFile>();
                MyElementFile.SetModified(DataManager.Get().IsFolderModified(FolderNames[i]));
            }
            // Add listeners for each one of the files
            List<ElementFolder> ElementFolders = DataManager.Get().GetElementFolders();
            for (int i = 0; i < ElementFolders.Count; i++)
            {
                ElementFolder MyFolder = ElementFolders[i];
                GameObject ListElementObject = FilesList.GetCell(MyFolder.FolderName);
                if (ListElementObject != null)
                {
                    GuiListElementFile ListElement = ListElementObject.GetComponent<GuiListElementFile>();

                    UnityAction<Element> ModifiedAction = ListElement.OnModified;
                    MyFolder.ModifiedEvent.AddEvent(ModifiedAction);
                    FileActionsModified.Add(ModifiedAction);

                    UnityAction<Element> SavedAction = ListElement.OnSaved;
                    MyFolder.SavedEvent.AddEvent(SavedAction);
                    FileActionsSaved.Add(SavedAction);
                }
            }
        }

        /// <summary>
        /// Closes the files list
        /// </summary>
        private void CloseFoldersList(GuiList FilesList)
        {
            SelectedFolderName = FilesList.GetSelectedName();
            FilesList.Clear();
            // Remove listeners from element folders
            List<ElementFolder> ElementFolders = DataManager.Get().GetElementFolders();
            for (int i = 0; i < ElementFolders.Count; i++)
            {
                ElementFolder MyFolder = ElementFolders[i];
                if (MyFolder != null && i < FileActionsModified.Count)
                {
                    MyFolder.ModifiedEvent.RemoveListener(FileActionsModified[i]);
                    MyFolder.SavedEvent.RemoveListener(FileActionsSaved[i]);
                }
            }
            // Clear actions
            FileActionsModified.Clear();
            FileActionsSaved.Clear();
        }

        /// <summary>
        /// Creates all the elements for a folder of files
        /// </summary>
        private void OpenFilesList(GuiList FilesList)
        {
            FilesList.Clear();  // just incase
            IsInFolder = true;
            FilesList.Add("Back");
            FilesList.Add("New");
            // Add listeners too
            // Add in file tags to elements that are added
            int FilesCount = DataManager.Get().GetSize(SelectedFolderName);
            for (int i = 0; i < FilesCount; i++)
            {
                GameObject FileElement = FilesList.Add(DataManager.Get().GetName(SelectedFolderName, i));
                // for each element, add OnModified Events to the guis
                // also initiate the gui difference on it
                // Make GuiListElement for file? with the managing of these icons
                Element MyElement = DataManager.Get().GetElement(SelectedFolderName, i);
                GuiListElementFile ListElement = FileElement.GetComponent<GuiListElementFile>();
                if (MyElement != null && ListElement != null)
                {
                    UnityAction<Element> ModifiedAction = ListElement.OnModified;
                    MyElement.ModifiedEvent.AddEvent(ModifiedAction);
                    FileActionsModified.Add(ModifiedAction);
                    UnityAction<Element> SavedAction = ListElement.OnSaved;
                    MyElement.SavedEvent.AddEvent(SavedAction);
                    FileActionsSaved.Add(SavedAction);
                    ListElement.CheckElement(MyElement);
                }
            }
            Debug.Log("Opened folder: " + SelectedFolderName + " with " + FilesCount + " files.");
        }

        /// <summary>
        /// Closes all the files
        /// </summary>
        private void CloseFilesList()
        {
            // Clean up files list
            // ALso remove listeners from the current folder
            for (int i = 0; i < DataManager.Get().GetSize(SelectedFolderName); i++)
            {
                Element MyElement = DataManager.Get().GetElement(SelectedFolderName, i);
                if (MyElement != null)
                {
                    MyElement.ModifiedEvent.RemoveListener(FileActionsModified[i]);
                    MyElement.SavedEvent.RemoveListener(FileActionsSaved[i]);
                }
            }
            FileActionsModified.Clear();
            FileActionsSaved.Clear();
            SelectedFolderName = "";    // no folder selected now
        }

        public void OnModifiedElement(Element MyElement)
        {

        }
        #endregion

        #region UI

       /* public override void UseInput(Toggle MyToggle)
        {
            if (MyToggle.name == "PersistentPathToggle")
            {
                if (FileUtil.SetPersistentPath(MyToggle.isOn)) 
                {
                    RefreshList();
                }
            }
            else if (MyToggle.name == "StreamingPathToggle")
            {
                if (FileUtil.SetStreamingPath(MyToggle.isOn))
                {
                    RefreshList();
                }
            }
        }*/

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
            if (MyButton.name == "TextureMaker")
            {
                GuiSpawner.Get().SpawnGui("TextureMaker");
            }
            else if (MyButton.name == "PolygonMaker")
            {
                GuiSpawner.Get().SpawnGui("PolygonMaker");
            }
            else if (MyButton.name == "ModelMaker")
            {
                GuiSpawner.Get().SpawnGui("ModelMaker");
            }
            else if (MyButton.name == "SkeletonMaker")
            {
                GuiSpawner.Get().SpawnGui("SkeletonMaker");
            }
            else if (MyButton.name == "SoundMaker")
            {
                GuiSpawner.Get().SpawnGui("SoundMaker");
            }
            else if (MyButton.name == "LevelMaker")
            {
                GuiSpawner.Get().SpawnGui("LevelMaker");
            }
            else if (MyButton.name == "ClassMaker")
            {
                GuiSpawner.Get().SpawnGui("ClassMaker");
            }
            else if (MyButton.name == "DialogueMaker")
            {
                GuiSpawner.Get().SpawnGui("DialogueMaker");
            }
            else if (MyButton.name == "ItemMaker")
            {
                GuiSpawner.Get().SpawnGui("ItemMaker");
            }
            else if (MyButton.name == "QuestMaker")
            {
                GuiSpawner.Get().SpawnGui("QuestMaker");
            }
            else if (MyButton.name == "RecipeMaker")
            {
                GuiSpawner.Get().SpawnGui("RecipeMaker");
            }
            else if (MyButton.name == "SpellMaker")
            {
                GuiSpawner.Get().SpawnGui("SpellMaker");
            }
            else if (MyButton.name == "StatsMaker")
            {
                GuiSpawner.Get().SpawnGui("StatsMaker");
            }
            else if (MyButton.name == "VoxelMaker")
            {
                GuiSpawner.Get().SpawnGui("VoxelMaker");
            }
            else
            {
                return false;
            }
            return true;
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
