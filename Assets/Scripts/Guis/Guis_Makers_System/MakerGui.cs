using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Zeltex.Util;
using Zeltex.Guis;
using UnityEngine.Events;
using Zeltex;

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// Base class for all the maker guis
    /// Adds in a file system!
    /// </summary>
    public class MakerGui : GuiBasic
    {
        #region Variables
        [Header("MakerGui")]
        public IndexController MyIndexController;    // specifically linked to the file
        public GuiList MyList;
        public UnityEvent OnUpdateSize = new UnityEvent();
        protected string DataManagerFolder = "";
        #endregion

        #region Mono
        protected virtual void Start()
        {
        }
        #endregion

        #region DataManager

        /// <summary>
        /// Save the folder!
        /// </summary>
        public virtual void SaveAll()
        {
            DataManager.Get().SaveElements(DataManagerFolder);
        }

        /// <summary>
        /// use this to set datamanager folder name
        /// </summary>
        protected virtual void SetFilePaths()
        {
            DataManagerFolder = "Error"; // ??
        }
        /// <summary>
        /// Add basic data for a string
        /// </summary>
        protected virtual void AddData()
        {
            //DataManager.Get().AddElement(DataManagerFolder,);
        }

        /// <summary>
        /// Remove a single data!
        /// </summary>
        protected virtual void RemovedData(int Index)
        {
            DataManager.Get().RemoveElement(DataManagerFolder, Index);
        }

        /// <summary>
        /// Clears all the data - used from UI
        /// </summary>
        public void ClearData()
        {
            DataManager.Get().Clear(DataManagerFolder);
        }

        /// <summary>
        /// Save the current file!
        /// </summary>
        public void SaveSelected()
        {
            //DataManager.Get().Save(DataManagerFolder, GetSelectedIndex());
        }

        /// <summary>
        /// Get the amount of files!
        /// </summary>
        public virtual int GetSize()
        {
            return DataManager.Get().GetSize(DataManagerFolder);
        }
        #endregion

        #region DataManagerData

        /// <summary>
        /// Get the selected Data
        /// </summary>
        public virtual string GetSelected()
        {
            //Debug.LogError("Getting Selected: " + DataManagerFolder);
            //return DataManager.Get().Get(DataManagerFolder, GetSelectedIndex());
            return "";
        }
        /// <summary>
        /// Get a data by a name!
        /// </summary>
        public string Get(string FileName)
        {
            //return DataManager.Get().Get(DataManagerFolder, FileName);
            return "";
        }

        protected void Set(string MyScript, int Index)
        {
            //DataManager.Get().Set(DataManagerFolder, Index, MyScript);
        }

        /// <summary>
        /// Get a file at an index
        /// </summary>
        public string Get(int FileIndex)
        {
            //return DataManager.Get().Get(DataManagerFolder, FileIndex);
            return "";
        }

        #endregion

        #region DataManagerNaming

        /// <summary>
        /// Get a name at an index
        /// </summary>
        public string GetName(int Index)
        {
            return DataManager.Get().GetName(DataManagerFolder, Index);
        }

        /// <summary>
        /// Rename a file!
        /// </summary>
        protected string Rename(string NewName)
        {
            // rename file
            string OldName = GetSelectedName();
            NewName = DataManager.Get().SetName(DataManagerFolder, GetSelectedIndex(), NewName);
            if (OldName != NewName)
            {
                MyList.Rename(OldName, NewName);
                Debug.Log("Renamed " + OldName + " - to - " + NewName);
            }
            else
            {
                Debug.LogWarning("Failed to rename " + OldName);
            }
            return NewName;
        }

        /// <summary>
        /// Gets the name selected
        /// </summary>
        public string GetSelectedName()
        {
            return DataManager.Get().GetName(DataManagerFolder, GetSelectedIndex());
        }
        #endregion

        #region UI
        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(Button MyButton)
        {
            if (MyButton.name == "SaveButton")
            {
                SaveAll();
            }
        }
        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(InputField MyInputField)
        {
            if (MyInputField.name == "NameInput")
            {
                MyInputField.text = Rename(MyInputField.text);
            }
        }
        #endregion

        #region FileAccess
        public virtual void Load()
        {
            Debug.Log("Calling MakerGui Load function.");
        }
        public virtual void Save()
        {
            Debug.Log("Calling MakerGui Save function.");
        }
        public virtual void LoadAll()
        {
            SetFilePaths();
        }
        public virtual IEnumerator LoadAllRoutine()
        {
            SetFilePaths();
            yield break;
        }


        /// <summary>
        /// Deletes the current file
        /// </summary>
        protected virtual void DeleteFile()
        {
            DeleteFile(GetSelectedName());
        }
        public void DeleteFile(string MyName)
        {
            //string MyFileName = FileUtil.GetFolderPath(FolderName) + MyName + "." + FileExtension;
            //FileUtil.Delete(MyFileName);
        }
        /// <summary>
        /// Used just for texture maker
        /// </summary>
        public void DeleteFile(string PathName, string MyName)
        {
            //string MyFileName = FileUtil.GetFolderPath(PathName) + MyName + "." + FileExtension;
            //FileUtil.Delete(MyFileName);
        }
        #endregion

        #region IndexController
        /// <summary>
        /// Select an index remotely
        /// </summary>
        public void Select(int NewIndex)
        {
            if (MyIndexController && MyIndexController.gameObject.activeSelf)
            {
                MyIndexController.ForceSelect(NewIndex);
            }
            else
            {
                OnUpdatedIndex(NewIndex);
            }
        }

        /// <summary>
        /// this is where gui data should be updated with the loaded data
        /// </summary>
        public virtual void OnUpdatedIndex(int NewIndex)
        {
            SetInputs(true);
            MyList.IsSelectable = true;
            MyList.Select(NewIndex);
            if (GetInput("NameInput"))
            {
                GetInput("NameInput").text = GetSelectedName();
            }
        }

        /// <summary>
        /// Creates a new data in maker
        /// </summary>
        public void New()
        {
            if (MyIndexController)
            {
                MyIndexController.Add();
            }
        }

        /// <summary>
        /// Index of where it is added
        /// </summary>
        public void OnAdd(int NewIndex)
        {
            AddData();  // first add data!
            if (MyIndexController)
            {
                MyIndexController.SetMaxSelected(GetSize());
                MyIndexController.SelectIndex(MyIndexController.MaxSelected - 1);
            }
            AddName(GetSelectedName()); // add to gui list
            OnUpdateSize.Invoke();
        }

        /// <summary>
        /// this is where gui data should be updated with the loaded data
        /// </summary>
        public void OnRemove(int NewIndex)
        {
			MyList.RemoveAt(NewIndex);
            MyIndexController.RemovedOldIndex();
			RemovedData(NewIndex);
			MyIndexController.SetMaxSelected(GetSize());
            MyIndexController.SelectIndex(MyIndexController.SelectedIndex - 1);
            OnUpdateSize.Invoke();
        }

        /// <summary>
        /// Link up the Gui with the indexController
        /// </summary>
        private void InitiateIndexController()
        {
            if (MyIndexController)
            {
                MyIndexController.OnIndexUpdated.RemoveAllListeners();
                MyIndexController.OnIndexUpdated.AddEvent(OnUpdatedIndex);
                MyIndexController.OnAdd.RemoveAllListeners();
                MyIndexController.OnAdd.AddEvent(OnAdd);
                MyIndexController.OnRemove.RemoveAllListeners();
                MyIndexController.OnRemove.AddEvent(OnRemove);
                MyIndexController.OnListEmpty.RemoveAllListeners();
                MyIndexController.OnListEmpty.AddEvent(OnListEmpty);
                MyIndexController.SetMaxSelected(GetSize());
                if (MyList)
                {
                    MyList.IsSelectable = true;
					MyList.Clear();
					for (int i = 0; i < GetSize(); i++)
					{
						MyList.Add(GetName(i));
					}
					MyList.Select(GetSelectedIndex());
                }
                MyIndexController.OnBegin();
            }
        }

        /// <summary>
        /// When list is empty, set interactivity of input off
        /// </summary>
        public virtual void OnListEmpty()
        {
            SetInputs(false);
            for (int i = 0; i < MyLabels.Count; i++)
            {
                if (MyLabels[i])
                {
                    MyLabels[i].text = "";
                }
            }
        }
        ///
        #endregion

        #region Data

        /// <summary>
        /// Adds name to files list
        /// </summary>
        protected void AddName(string NewName)
        {
            if (MyList)
            {
                MyList.Add(NewName);
            }
        }

        public int GetSelectedIndex()
        {
            return MyIndexController.SelectedIndex;
        }

        /// <summary>
        /// Clears the Maker gui of all elements
        /// </summary>
        public virtual void Clear()
        {
            if (MyList)
            {
                MyList.Clear();
            }
            ClearData();
            OnUpdateSize.Invoke();
        }
        /// <summary>
        /// Override in texture maker, as it contains multiple folders
        /// </summary>
        public virtual void Delete()
        {
            Clear();
        }

        #endregion

        #region ZelGui
        /// <summary>
        /// Called on ZelGuis OnBegin function
        /// </summary>
        public override void OnBegin()
        {
            base.OnBegin();         // fill dropdowns first
            SetFilePaths();
            InitiateIndexController();
        }
        /// <summary>
        /// Called on ZelGuis OnEnd function
        /// </summary>
        public override void OnEnd()
        {
            base.OnEnd();
            MyIndexController.OnEnd();
        }
        #endregion
    }
}

/*
    Put these into a gui maker class
    

        // if parts of the gui is missing, spawn them
        // Components: Canvas, Orbitor, Billboard, ZelGui, GraphicRaycaster
        // Also adds the ZelGui Events OnBegin, OnEnd if they are not there
        public void CheckGui()
        {

        }
        // gui spawning things
        public void SpawnBackground()
        {

        }
        public void SpawnLabel()
        {

        }
        public void SpawnCloseButton()
        {

        }
        // Has a label for the index, and forward and back buttons
        public void SpawnIndexLabel()
        {

        }

    */
    