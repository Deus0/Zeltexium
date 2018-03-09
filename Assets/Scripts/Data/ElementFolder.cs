using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Voxels;
using Zeltex.Util;
using Newtonsoft.Json;
using UnityEngine.Events;

namespace Zeltex
{
    [Serializable]
    public class ElementFolderEvent : UnityEvent<ElementFolder> { }

    [Serializable()]
    public class DataDictionary<T> : SerializableDictionaryBase<string, T>
    {
        public bool ContainsValue(T MyElement)
        {
            return Values.Contains(MyElement);
        }
    }

    [Serializable()]
    public class ElementDictionary : SerializableDictionaryBase<string, Element>
    {
        public bool ContainsValue(Element MyElement)
        {
            return Values.Contains(MyElement);
        }
    }

    [Serializable]
    public partial class ElementFolder : object, ISerializationCallbackReceiver
    {
        [Header("Data")]
        public string FolderName = "None";                              // The name of the folder, used to save a list of files
        public string FileExtension = "err";                            // the particular type of file used to save the data
        [HideInInspector]
        public ElementDictionary Data = new ElementDictionary();
        public List<Element> CachedData = new List<Element>();

        [Header("Events")]
        [Tooltip("When the file has modified - Update any connected UI to indicate a file change"), HideInInspector]
        public ElementFolderEvent ModifiedEvent = new ElementFolderEvent();
        [Tooltip("When the file has saved - Update any connected UI to indicate a file change"), HideInInspector]
        public ElementFolderEvent SavedEvent = new ElementFolderEvent();
        [Tooltip("When the file has been renamed - Update any connected UI to indicate a file change"), HideInInspector]
        public ElementFolderEvent RenamedEvent = new ElementFolderEvent();
        [Tooltip("When the file has been renamed - Update any connected UI to indicate a file change"), HideInInspector]
        public ElementFolderEvent MovedEvent = new ElementFolderEvent();


        public void OnBeforeSerialize()
        {
            //Serialize();
        }

        public void OnAfterDeserialize()
        {
            //Deserialize();
        }
        List<string> SerializdData = new List<string>();
        List<string> SerializedNames = new List<string>();
        public void Serialize()
        {
            SerializdData.Clear();
            SerializedNames.Clear();
            List<Element> MyElements = GetData();
            List<string> MyNames = GetNames();
            if (MyElements == null)
            {
                Debug.LogError(FolderName + " has null elements!");
                return;
            }
            int Size = MyElements.Count;
            for (int i = 0; i < Size; i++)
            {
                if (MyElements[i] != null)
                    SerializdData.Add(MyElements[i].GetSerial());
            }
            for (int i = 0; i < Size; i++)
            {
                if (MyNames[i] != null)
                    SerializedNames.Add(MyNames[i]);
            }
        }

        public void Deserialize()
        {
            Clear();
            RoutineManager.Get().StartCoroutine(DeserializeRoutine(SerializedNames, SerializdData));
        }

        public IEnumerator DeserializeRoutine(List<string> Names, List<string> Scripts)
        {
            float LastYield = Time.realtimeSinceStartup;
            string Script = "";
            JsonSerializerSettings MySettings = new JsonSerializerSettings();
            MySettings.Formatting = DataManager.Get().GetFormat();
            for (int i = 0; i < Scripts.Count; i++)
            {
                Script = Scripts[i];
                Element NewElement = null;// = new Element();
                bool IsThreading = true;
                System.Threading.Thread LoadThread = new System.Threading.Thread(
                    () =>
                    {
                        NewElement = Element.Load(Names[i], Script, this as ElementFolder);
                        IsThreading = false;
                    });
                LoadThread.Start();
                while (IsThreading)
                {
                    yield return null;
                }
                if (NewElement != null)
                {
                    NewElement.Name = Names[i];
                    NewElement.MyFolder = this;
                    NewElement.ResetName();
                    NewElement.OnLoad();
                    if (DataFolderNames.GetDataType(FolderName) == typeof(Level))
                    {
                        // Do the thing!
                        Level MyLevel = NewElement as Level;
                        MyLevel.SetFilePathType(DataManager.Get().MyFilePathType);
                    }
                    if (Data.ContainsKey(NewElement.Name) == false)
                    {
                        Data.Add(NewElement.Name, NewElement);
                        CachedData.Add(NewElement);
                        NewElement.MyFolder = (this as ElementFolder);
                    }
                    else
                    {
                        Debug.LogError("Attempting to add duplicate element: " + NewElement.Name + " to folder " + FolderName);
                    }
                }
                if (Time.realtimeSinceStartup - LastYield >= (16f / 1000f))
                {
                    LastYield = Time.realtimeSinceStartup;
                    yield return null;
                }
            }
            SerializdData.Clear();
            SerializedNames.Clear();
        }

        public void Set(string NewFolderName, string NewFileExtension)
        {
            FolderName = NewFolderName;
            FileExtension = NewFileExtension;
        }

        public static ElementFolder Create(string NewFolderName, string NewFileExtension = "zel")
        {
            ElementFolder NewFolder = new ElementFolder();
            NewFolder.Set(NewFolderName, NewFileExtension);
            return NewFolder;
        }

        #region FileAccess

        public void Clear()
        {
            Data.Clear();
        }

        /// <summary>
        /// Gets the folder path on the current machine to the data folder
        /// </summary>
        public string GetFolderPath()
        {
            // Creates the folder path if doesn't exist!
            return DataManager.GetFolderPath(FolderName + "/");
        }

        public void SaveFile(int FileIndex, string Data)
        {
            string SavePath = GetFolderPath() + GetName(FileIndex) + "." + FileExtension;
            //Debug.LogError("Saving: " + SavePath + ":" + Data);
            FileUtil.Save(SavePath, Data);
        }

        public void SaveFile(string FileName, string Data)
        {
            string SavePath = GetFolderPath() + FileName + "." + FileExtension;
            Debug.Log("Saving: " + SavePath + ":" + Data);
            //FileManagement.SaveFile(SavePath, Data);
            string DirecetoryPath = Path.GetDirectoryName(SavePath);
            if (FileManagement.DirectoryExists(DirecetoryPath, true, true))
            {
                try
                {
                    FileManagement.SaveFile(SavePath, Data, false, true);
                }
                catch (System.IO.IsolatedStorage.IsolatedStorageException e)
                {
                    Debug.LogError(e.ToString());
                }
            }
            else
            {
                Debug.LogError("Cannot save path as directory does not exist: " + SavePath);
            }
        }

        public void SaveFileTexture(int FileIndex, Texture2D Data)
        {
            if (Data)
            {
                string SavePath = GetFolderPath() + GetName(FileIndex) + "." + FileExtension;
                FileUtil.SaveBytes(SavePath, Data.EncodeToPNG());
            }
            else
            {
                Debug.LogError(FolderName + " has no texture data at: " + FileIndex);
            }
        }

        public void SaveFileSound(int FileIndex, AudioClip Data)
        {
            if (Data)
            {
                string SavePath = GetFolderPath() + GetName(FileIndex) + "." + FileExtension;
                SavWav.Save(SavePath, Data);
            }
            else
            {
                Debug.LogError(FolderName + " has no Sound data at: " + FileIndex);
            }
        }
        #endregion

        #region Get

        public List<Element> GetData()
        {
            if (Data != null)
            {
                return Data.GetValues();
            }
            else
            {
                Debug.LogError("Data is null inside " + FolderName);
                return new List<Element>();
            }
        }

        public List<T> GetData<T>() where T : Element
        {
            if (Data != null)
            {
                List<T> MyT = new List<T>();
                foreach (KeyValuePair<string, Element> MyValuePair in Data)
                {
                    MyT.Add(MyValuePair.Value as T);
                }
                return MyT;
            }
            else
            {
                Debug.LogError("Data is null inside " + FolderName);
                return new List<T>();
            }
        }

        public List<string> GetNames()
        {
            if (Data != null)
            {
                return Data.GetKeys();
            }
            else
            {
                Debug.LogError("Data is null inside " + FolderName);
                return new List<string>();
            }
        }
        #endregion

        #region Element

        /// <summary>
        /// add a generic default
        /// </summary>
        public void New(string NewName)
        {
            if (Data.ContainsKey(NewName) == false)
            {
                Data.Add(NewName, default(Element));

            }
            else
            {
                //Debug.LogError("Cannot add another " + NewName + " to " + FolderName);
            }
        }

        public void ReAdd(string ElementName, Element MyElement)
        {
            if (Data.ContainsValue(MyElement))
            {
                Data.Remove(ElementName);
                Data.Add(ElementName, MyElement);
            }
        }

        public bool SetElement(string NewName, Element NewElement)
        {
            if (Data.Keys.Contains(NewName))
            {
                Data[NewName] = NewElement;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Add a file a new file
        /// </summary>
        public bool Add(string NewName, Element NewElement)
        {
            if (Data.Keys.Contains(NewName) == true)
            {
                NewName += UnityEngine.Random.Range(1, 10000);
            }
            if (Data.Keys.Contains(NewName) == false)
            {
                Data.Add(NewName, NewElement);
                CachedData.Add(NewElement);
                return true;
            }
            else
            {
                Debug.LogError("Bam! Could not add:" + NewName);
                return false;
            }
        }

        /// <summary>
        /// Get a value by file name
        /// </summary>
        public Element Get(string FileName)
        {
            if (Data.ContainsKey(FileName))
            {
                return Data[FileName];
            }
            else
            {
                return default(Element);
            }
        }

        /// <summary>
        /// Gets the name at an index
        /// </summary>
        public string GetName(int FileIndex)
        {
            int Count = 0;
            foreach (var KeyVarPair in Data)
            {
                if (Count == FileIndex)
                {
                    return KeyVarPair.Key;
                }
                Count++;
            }
            return "";
        }
        /// <summary>
        /// Get a value by file name
        /// </summary>
        public Element Get(int FileIndex)
        {
            int Count = 0;
            foreach (var KeyVarPair in Data)
            {
                if (Count == FileIndex)
                {
                    return Data[KeyVarPair.Key];
                }
                Count++;
            }
            return default(Element);
        }

        /// <summary>
        /// Set a value by file name
        /// </summary>
        public void Set(string FileName, Element NewData)
        {
            if (Data.ContainsKey(FileName))
            {
                Data[FileName] = NewData;
            }
        }

        /// <summary>
        /// set a generic value by index
        /// </summary>
        public void Set(int FileIndex, Element NewData)
        {
            int Count = 0;
            foreach (var KeyVarPair in Data)
            {
                if (Count == FileIndex)
                {
                    Data[KeyVarPair.Key] = NewData;
                    break;
                }
                Count++;
            }
        }

        /// <summary>
        /// set a generic value by index
        /// </summary>
        public void Remove(int FileIndex)
        {
            int Count = 0;
            foreach (var KeyVarPair in Data)
            {
                if (Count == FileIndex)
                {
                    Data.Remove(KeyVarPair.Key);
                    CachedData.Remove(KeyVarPair.Value);
                    DeleteFile(KeyVarPair.Key);
                    break;
                }
                Count++;
            }
        }


        public bool CanRemove(string FileName)
        {
            return Data.ContainsKey(FileName);
        }

        public bool Remove(string FileName)
        {
            if (CanRemove(FileName))
            {
                CachedData.Remove(Data[FileName]);
                Data.Remove(FileName);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void DeleteFile(string FileName)
        {
            FileName = GetFolderPath() + FileName + "." + FileExtension;
            // Delete file if exists
            if (File.Exists(FileName))
            {
                Debug.LogError("Deleting file: " + FileName);
                File.Delete(FileName);
            }
            else
            {
                Debug.LogError("File did not exist: " + FileName);
            }
        }
        #endregion


        [JsonIgnore, HideInInspector]
        [Tooltip("Set to true when the element has been changed from the saved file")]
        public bool HasChanged = false;

        public bool CanSave()
        {
            return HasChanged;
        }

        public void OnSaved()
        {
            if (HasChanged)
            {
                // if finished saving all, check if any still dirty
                foreach (KeyValuePair<string, Element> MyValuePair in Data)
                {
                    Element MyElement = MyValuePair.Value as Element;
                    if (MyElement == null || MyElement.CanSave())
                    {
                        // Folder still dirty
                        return;
                    }
                }
                HasChanged = false;
                SavedEvent.Invoke(this as ElementFolder);
            }
        }

        public void OnModified()
        {
            if (!HasChanged)
            {
                HasChanged = true;
                ModifiedEvent.Invoke(this as ElementFolder);
            }
        }


        /*public new static ElementFolder Create(string NewFolderName, string NewFileExtension)
        {
            ElementFolder NewFolder = new ElementFolder();
            NewFolder.Set(NewFolderName, NewFileExtension);
            return NewFolder;
        }*/

        /// <summary>
        /// Data is set when reloading scripts
        /// </summary>
        public void SetData(List<Element> NewData)
        {
            Data.Clear();
            for (int i = 0; i < NewData.Count; i++)
            {
                Data.Add(NewData[i].Name, NewData[i]);
            }
        }
        /// <summary>
        /// Data is set when reloading scripts
        /// </summary>
        public void SetData<T>(List<T> NewData) where T : Element
        {
            Data.Clear();
            for (int i = 0; i < NewData.Count; i++)
            {
                Data.Add(NewData[i].Name, NewData[i]);
            }
        }

        public int IndexOf(string FileName)
        {
            if (Data.ContainsKey(FileName))
            {
                int FileCount = 0;
                foreach (KeyValuePair<string, Element> MyKeyValue in Data)
                {
                    if (MyKeyValue.Key == FileName)
                    {
                        return FileCount;
                    }
                    FileCount++;
                }
            }
            return -1;
        }

        public void Revert()
        {
            Clear();
            LoadAllElements();
        }

        public void ForceSaveAllElements()
        {
            ElementFolder This = this as ElementFolder;
            foreach (KeyValuePair<string, Element> MyKeyValue in This.Data)
            {
                MyKeyValue.Value.OnModified();
                MyKeyValue.Value.Save();
            }
        }

        public void SaveAllElements()
        {
            ElementFolder This = this as ElementFolder;
            foreach (KeyValuePair<string, Element> MyKeyValue in This.Data)
            {
                MyKeyValue.Value.Save();
            }
        }

        public void AddElement(Element NewElement)
        {
            Add(NewElement.Name, NewElement);
            NewElement.MyFolder = this;
            NewElement.ElementLink = FolderName;
            NewElement.ResetName();
            NewElement.OnModified();
        }

        public bool SetElement(Element NewElement)
        {
            bool DidSet = SetElement(NewElement.Name, NewElement);
            if (DidSet)
            {
                NewElement.MyFolder = this;
                NewElement.ElementLink = FolderName;
                NewElement.ResetName();
                NewElement.OnModified();
            }
            return DidSet;
        }

        public void AddNewElement()
        {
            Type type = DataFolderNames.GetDataType(FolderName);// System.Type.GetType("Foo.MyClass");
            object ElementObject = System.Activator.CreateInstance(type);
            Element NewElement = ElementObject as Element;
            NewElement.SetNameOfClone("Element" + UnityEngine.Random.Range(1, 10000));
            NewElement.MyFolder = this as ElementFolder;
            NewElement.MyFolder.Data.Add(NewElement.Name, NewElement);
            NewElement.MyFolder.CachedData.Add(NewElement);
        }

        public void LoadElement(Element MyElement)
        {
            MyElement = MyElement.Load();
        }
        
        public IEnumerator LoadAllElements()
        {
            Clear();
            List<string> FileNames;
            string ElementFolderPath = GetFolderPath();
            FileNames = new List<string>();
            if (ElementFolderPath != null)
            {
                if (FileManagement.DirectoryExists(ElementFolderPath, true, true))
                {
                    //
                    string[] FoundFiles = FileManagement.ListFiles(ElementFolderPath, new string[] { "." + FileExtension }, DataManager.Get().MyFilePathType == FilePathType.StreamingPath, true);//, true, true); // + "/"
                    if (FoundFiles != null)
                    {
                        //Debug.Log(ElementFolderPath + " has found " + FoundFiles.Length + " [" + FileExtension + "] Files.");
                        for (int i = 0; i < FoundFiles.Length; i++)
                        {
                            if (FoundFiles[i] != null)
                            {
                                LogManager.Get().Log("FoundFiles " + i + " " + FoundFiles[i], "DataManagerFiles");
                                try
                                {
                                    FileManagement.GetFileName(FoundFiles[i]);
                                    string FileExtention = FileManagement.GetFileExtension(FoundFiles[i]);
                                    string CulledFileName = FileManagement.GetFileName(FoundFiles[i]);
                                    if (CulledFileName.Contains(FileExtention))
                                    {
                                        FileNames.Add(ElementFolderPath + CulledFileName);
                                    }
                                    else
                                    {
                                        string FileName = CulledFileName + "." + FileExtension;
                                        FileNames.Add(ElementFolderPath + FileName);
                                    }
                                }
                                catch(FormatException e)
                                {
                                    Debug.LogError("[LoadAllElements] has error: " + e.ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        LogManager.Get().Log(ElementFolderPath + " has no files found.");
                    }
                }
                else
                {
                    Debug.LogError("[LoadAllElements] Directory Path does not exist, Recreating: " + ElementFolderPath);
                    FileManagement.CreateDirectory(ElementFolderPath, true);
                }
            }
            else
            {
                LogManager.Get().Log("Folder is null");
            }
            LogManager.Get().Log("[LoadAllElements] -=- [" + FolderName + "] Found: " + FileNames.Count + "\nFolderPath: " + ElementFolderPath + " --- " + FileExtension, "DataManager");
            List<string> Scripts = new List<string>();
            string LoadLog = "";
            for (int i = 0; i < FileNames.Count; i++)
            {
                LoadLog = "Loading Element file in : " + FolderName + " of - " + FileNames[i];
                LogManager.Get().Log(LoadLog, "DataManagerFiles");
                if (FileNames[i] != null && (FileNames[i].Contains("://") || FileNames[i].Contains(":///")))
                {
                    WWW UrlRequest = null;
                    try
                    {
                        UrlRequest = new WWW(FileNames[i]);
                    }
                    catch (FormatException e)
                    {
                        Debug.LogError("Error while loading file : UrlRequest: " + FileNames[i] + ": " + e.ToString());
                    }
                    if (UrlRequest != null)
                    {
                        yield return (UrlRequest);
                        Scripts.Add(UrlRequest.text);
                    }
                }
                else
                {
                    FileReaderRoutiner MyFileReader = new FileReaderRoutiner(FileNames[i]);
                    yield return MyFileReader.Run();
                    Scripts.Add(MyFileReader.Result);
                    //Scripts.Add(FileUtil.Load(FileNames[i]));
                }
            }
            for (int i = 0; i < FileNames.Count; i++)
            {
                FileNames[i] = Path.GetFileNameWithoutExtension(FileNames[i]);
            }
            yield return DeserializeRoutine(FileNames, Scripts);
        }
    }

}