using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zeltex.Guis.Maker;

namespace Zeltex.Guis
{
    /// <summary>
    /// Used to handle file guis of game resources
    /// </summary>
    public class ResourcesFileHandler : MonoBehaviour
    {
        [SerializeField]
        private GuiList FilesList;
        private bool IsInFolder = false;
        private string SelectedFolderName = "";
        // Keep track of the actions for file events
        private List<UnityAction<Element>> FileActionsModified = new List<UnityAction<Element>>();
        private List<UnityAction<Element>> FileActionsSaved = new List<UnityAction<Element>>();

        #region FileList

        private void Awake()
        {
            FilesList.OnActivateEvent.AddEvent<int>(OnClickFile);
        }

        /// <summary>
        /// When user clicks on a list element
        /// </summary>
        public void OnClickFile(int FileIndex)
        {
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
                    NewFile();
                }
                else
                {
                    OpenFile(FileIndex);
                }
            }
            Debug.Log("On Clicked file: " + FileIndex + "- IsInFolder: " + IsInFolder.ToString());
        }

        private void NewFile()
        {
            GameObject MakerGui = GuiSpawner.Get().SpawnMakerGui(DataFolderNames.FolderToGuiName(SelectedFolderName));
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

        private void OpenFile(int FileIndex)
        {
            MakerGui MyMaker = GuiSpawner.Get().SpawnMakerGui(DataFolderNames.FolderToGuiName(SelectedFolderName)).GetComponent<MakerGui>();
            TextureMaker MyTextureMaker = MyMaker.GetComponent<TextureMaker>();
            if (MyTextureMaker)
            {
                MyTextureMaker.SetFolder(SelectedFolderName);
            }
            MyMaker.Select(FileIndex - 2);
            Debug.Log("Spawned maker: " + SelectedFolderName + " to select: " + (FileIndex - 2));
        }

        /// <summary>
        /// Adds the folder names and events
        /// </summary>
        public void OpenFoldersList()
        {
            IsInFolder = false;
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
        public void CloseFoldersList(GuiList FilesList)
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
        public void OpenFilesList(GuiList FilesList)
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
        public void CloseFilesList()
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
    }
}