using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zeltex.Generators;
using Zeltex.Util;
using Zeltex.Combat;

namespace Zeltex
{

    /// <summary>
    /// Generic class to load data from a file
    /// Initializes in editor too
    /// </summary>
    public partial class DataManager : ManagerBase<DataManager>
    {

        #region Generic

        /// <summary>
        /// returns the name of an index
        /// </summary>
        public string GetName(string FolderName, int Index)
        {
            ElementFolder ElementFolder = GetElementFolder(FolderName);
            if (ElementFolder != null)
            {
                return ElementFolder.GetName(Index);
            }
            return "";
        }

        /// <summary>
        /// returns the size of a folder
        /// </summary>
        public int GetSize(string FolderName)
        {
            ElementFolder ElementFolder = GetElementFolder(FolderName);
            if (ElementFolder != null)
            {
                return ElementFolder.Data.Count;
            }
            return 0;
        }
        #endregion

        #region Elements

        public List<ElementFolder> GetElementFolders()
        {
            return ElementFolders;
        }

        /// <summary>
        /// returns true if the folder has been modified
        /// </summary>
        public bool IsFolderModified(string FolderName)
        {
            ElementFolder ElementFolder = GetElementFolder(FolderName);
            if (ElementFolder != null)
            {
                bool IsModified = false;
                List<Element> MyElements = ElementFolder.GetData();
                for (int i = 0; i < MyElements.Count; i++)
                {
                    if (MyElements[i].CanSave())
                    {
                        IsModified = true;
                        break;
                    }
                }
                return IsModified;
            }
            return false;
        }

        /// <summary>
        /// Save all the elements in a folder, the ui implementation
        /// </summary>
        public void SaveElements(string FolderName)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null)
            {
                List<Element> MyData = MyFolder.GetData();
                //Debug.LogError(FolderName + " is saving: " + MyData.Count);
                for (int i = 0; i < MyData.Count; i++)
                {
                    if (MyData[i].CanSave())
                    {
                        string Script = "";
                        if (IsJSONFormat)
                        {
                            Script = Newtonsoft.Json.JsonConvert.SerializeObject(MyData[i]);//MyData[i].GetScript();
                        }
                        else
                        {
                            Script = MyData[i].GetScript();
                        }
                        MyFolder.SaveFile(i, Script);
                        MyData[i].OnSaved();
                    }
                }
            }
            else
            {
                Debug.LogError(FolderName + " was not found.");
            }
        }

        /// <summary>
        /// Get the quest folder
        /// </summary>
        private ElementFolder GetElementFolder(string FolderName)
        {
            for (int i = 0; i < ElementFolders.Count; i++)
            {
                if (ElementFolders[i].FolderName == FolderName)
                {
                    return ElementFolders[i];
                }
            }
            return null;
        }

        private void LoadFolder(string FolderName)
        {
            Debug.LogError("Loading: " + FolderName);
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null)
            {
                UniversalCoroutine.CoroutineManager.StartCoroutine(MyFolder.LoadAllElements());
                Debug.LogError("Loading SUCCESS: " + FolderName);
                OnUpdatedResources.Invoke();
            }
        }

        /// <summary>
        /// Loads all the data!
        /// </summary>
        private System.Collections.IEnumerator LoadAllElements()
        {
            Debug.Log("Loading all elements for [" + MapName + "]");
            for (int i = 0; i < ElementFolders.Count; i++)
            {
                yield return UniversalCoroutine.CoroutineManager.StartCoroutine(ElementFolders[i].LoadAllElements());
            }
            OnUpdatedResources.Invoke();
        }

        public Element RevertElement(Element MyElement)
        {
            ElementFolder MyFolder = GetElementFolder(MyElement.GetFolder());
            if (MyFolder != null)
            {
                MyElement = MyElement.Revert();
                MyFolder.Set(MyElement.Name, MyElement);
            }
            return MyElement;
        }

        public void LoadElement(Element MyElement)
        {
            ElementFolder MyFolder = GetElementFolder(MyElement.GetFolder());
            if (MyFolder != null)
            {
                MyFolder.LoadElement(MyElement);
            }
        }

        /// <summary>
        /// Save all the elements!
        /// </summary>
        private void SaveAllElements()
        {
            for (int i = 0; i < ElementFolders.Count; i++)
            {
                ElementFolders[i].SaveAllElements();
            }
        }

        public void SaveElement(string ElementFolderName, int ElementIndex)
        {
            ElementFolder MyFolder = GetElementFolder(ElementFolderName);
            if (MyFolder != null)
            {
                Element MyElement = MyFolder.Get(ElementIndex);
                SaveElement(MyElement);
            }
        }
        public void SaveElement(string ElementFolderName, string ElementName)
        {
            ElementFolder MyFolder = GetElementFolder(ElementFolderName);
            if (MyFolder != null)
            {
                Element MyElement = MyFolder.Get(ElementName);
                SaveElement(MyElement);
            }
        }

        public void SaveElement(Element MyElement)
        {
            ElementFolder MyFolder = GetElementFolder(MyElement.GetFolder());
            if (MyFolder != null
                && MyElement.CanSave())
            {
                MyFolder.SaveFile(MyElement.Name, MyElement.GetSerial());
                MyElement.OnSaved();
            }
        }

        /// <summary>
        /// Get an item  from a folder, using an index
        /// </summary>
        public Element GetElement(string FolderName, int Index)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null)
            {
                return MyFolder.Get(Index);
            }
            return null;
        }

        /// <summary>
        /// Get an item  from a folder, using an index
        /// </summary>
        public Element GetElement(string FolderName, string FileName)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null && MyFolder.Data.ContainsKey(FileName))
            {
                return MyFolder.Get(FileName);
            }
            else
            {
                Debug.LogError("Could not find folder with name: " + FolderName);
            }
            return null;
        }

        /// <summary>
        /// returns the element if its in the folder
        /// </summary>
        public int GetElementIndex(string FolderName, Element MyElement)
        {
            int MyIndex = -1;
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null)
            {
                List<Element> MyData = MyFolder.GetData();
                for (int i = 0; i < MyData.Count; i++)
                {
                    if (MyData[i] == MyElement)
                    {
                        MyIndex = i;
                        break;
                    }
                }
            }
            return MyIndex;
        }

        public void SetElement(string FolderName, Element NewElement)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null && NewElement != null)
            {
                if (MyFolder.SetElement(NewElement.Name, NewElement))
                {
                    NewElement.MyFolder = MyFolder;
                    OnUpdatedResources.Invoke();
                }
            }
        }

        /// <summary>
        /// Add a texture
        /// </summary>
        public void AddElement(string FolderName, Element NewElement)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null && NewElement != null)
            {
                if (MyFolder.Add(NewElement.Name, NewElement))
                {
                    NewElement.MyFolder = MyFolder;
                    OnUpdatedResources.Invoke();
                }
            }
        }

        /// <summary>
        /// Removes a particular data
        /// </summary>
        public void RemoveElement(string FolderName, int FileIndex)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null)
            {
                MyFolder.Remove(FileIndex);
                if (FolderName == DataFolderNames.VoxelMeta)
                {
                    Voxels.VoxelManager.Get().RemoveMeta(FileIndex);
                }
                if (FolderName == DataFolderNames.PolygonModels)
                {
                    Voxels.VoxelManager.Get().RemoveModel(FileIndex);
                }
                OnUpdatedResources.Invoke();
            }
        }

        /// <summary>
        /// Removes a particular data
        /// </summary>
        public void RemoveElement(string FolderName, string FileName)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null)
            {
                int IndexOf = MyFolder.IndexOf(FileName);
                if (IndexOf != -1)
                {
                    MyFolder.Remove(IndexOf);
                    if (FolderName == DataFolderNames.VoxelMeta)
                    {
                        Voxels.VoxelManager.Get().RemoveMeta(IndexOf);
                    }
                    if (FolderName == DataFolderNames.PolygonModels)
                    {
                        Voxels.VoxelManager.Get().RemoveModel(IndexOf);
                    }
                    OnUpdatedResources.Invoke();
                }
            }
        }

        /// <summary>
        /// returns the size of a folder
        /// </summary>
        public int GetSizeElements(string FolderName)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null)
            {
                return MyFolder.Data.Count;
            }
            else
            {
                return 0;
            }
        }
        #endregion
    }
}