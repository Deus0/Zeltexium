using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zeltex.Items;
using Zeltex.Combat;
using Zeltex.Util;
using System.IO;
using Zeltex.Voxels;
using System.Linq;
using Zeltex.Quests;
using Newtonsoft.Json;

namespace Zeltex
{
	[System.Serializable]
	public class ElementFolderEvent : UnityEvent<ElementFolder> { }

    [System.Serializable()]
    public class DataDictionary<T> : SerializableDictionaryBase<string, T>
    {
        public bool ContainsValue(T MyElement)
        {
            return Values.Contains(MyElement);
        }
    }

    [System.Serializable()]
    public class ElementDictionary : SerializableDictionaryBase<string, Element>
    {
        public bool ContainsValue(Element MyElement)
        {
            return Values.Contains(MyElement);
        }
    }
    /// <summary>
    /// A reference to a folder
    /// </summary>
    [System.Serializable]
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

        public System.Collections.IEnumerator DeserializeRoutine(List<string> Names, List<string> Scripts)
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
                        NewElement = Element.Load(Names[i], this as ElementFolder, Script);
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
                    if (FolderName == DataFolderNames.PolyModels)
                    {
                        if (VoxelManager.Get())
                        {
                            VoxelManager.Get().AddModelRaw(NewElement as PolyModel);
                        }
                    }
                    else if (FolderName == DataFolderNames.Voxels)
                    {
                        if (VoxelManager.Get())
                        {
                            VoxelManager.Get().AddMetaRaw(NewElement as VoxelMeta);
                        }
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

        public static ElementFolder Create(string NewFolderName, string NewFileExtension)
        {
            ElementFolder NewFolder = new ElementFolder();
            NewFolder.Set(NewFolderName, NewFileExtension);
            return NewFolder;
        }

        #region FileAccess

        public void Clear()
		{
            //Debug.LogError("[DataFolder] Clearing " + FolderName);
            if (VoxelManager.Get())
            {
                if (FolderName == DataFolderNames.PolyModels)
                {
                    VoxelManager.Get().ClearModels();
                }
                else if (FolderName == DataFolderNames.Voxels)
                {
                    VoxelManager.Get().ClearMetas();
                }
                else if (FolderName == DataFolderNames.VoxelDiffuseTextures)
                {
                    if (VoxelManager.Get().DiffuseTextures != null)
                    {
                        VoxelManager.Get().DiffuseTextures.Clear();
                    }
                }
                else if (FolderName == DataFolderNames.VoxelNormalTextures)
                {
                    if (VoxelManager.Get().NormalTextures != null)
                    {
                        VoxelManager.Get().NormalTextures.Clear();
                    }
                }
            }
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

        /*private void LoadAllNames()
        {
            Clear();
            List<string> MyFiles = FileUtil.GetFilesOfType(GetFolderPath(), FileExtension);
            Debug.Log("[" + FolderName + "] Found: " + MyFiles.Count + "\nFolderPath: " + GetFolderPath() + " --- " + FileExtension);
            MyFiles = FileUtil.SortAlphabetically(MyFiles);
            List<string> MyNames = new List<string>();
            for (int i = 0; i < MyFiles.Count; i++)
            {
                MyNames.Add(Path.GetFileNameWithoutExtension(MyFiles[i]));
                Data.Add(MyNames[i], default(Element));
            }
            Debug.Log("[" + FolderName + "] Found " + MyNames.Count + ". ~~" + MyFiles.Count + "\nFolderPath: " + GetFolderPath());
        }

        public List<string> LoadAllStrings()
		{
            LoadAllNames();
            List<string> MyScripts = new List<string>();
            foreach (var MyKey in Data.Keys)
            {
                if (Data.ContainsKey(MyKey))
                {
                    string LoadPath = GetFolderPath() + MyKey + "." + FileExtension;
                    //Debug.LogError("Loading: " + LoadPath);
                    MyScripts.Add(FileUtil.Load(LoadPath));
                }
			}
			return MyScripts;
		}*/

        /*public List<Texture2D> LoadAllTextures()
        {
            LoadAllNames();
            List<Texture2D> MyTextures = new List<Texture2D>();

            foreach (var MyKey in Data.Keys)
            {
                if (Data.ContainsKey(MyKey))
                {
                    byte[] BytesData = FileUtil.LoadBytes(GetFolderPath() + MyKey + "." + FileExtension);
                    Texture2D NewTexture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                    NewTexture.filterMode = FilterMode.Point;
                    NewTexture.wrapMode = TextureWrapMode.Clamp;
                    NewTexture.LoadImage(BytesData);// as Texture2D;
                    NewTexture.name = MyKey;
                    MyTextures.Add(NewTexture);
                }
            }

            if (VoxelManager.Get())
            {
                if (FolderName == DataFolderNames.VoxelDiffuseTextures)
                {
                    VoxelManager MyVoxelManager = VoxelManager.Get();
                    MyVoxelManager.DiffuseTextures.Clear();
                    MyVoxelManager.DiffuseTextures.AddRange(MyTextures);
                    MyVoxelManager.GenerateTileMap();
                }
                else if (FolderName == DataFolderNames.VoxelNormalTextures)
                {
                    VoxelManager MyVoxelManager = VoxelManager.Get();
                    MyVoxelManager.NormalTextures.Clear();
                    MyVoxelManager.NormalTextures.AddRange(MyTextures);
                }
            }
            return MyTextures;
        }

        public List<AudioClip> LoadAllAudio()
        {
            LoadAllNames();
            List<AudioClip> MySounds = new List<AudioClip>();
            //foreach (var KeyVarPair in Data)
            foreach (var MyKey in Data.Keys)
            {
                if (Data.ContainsKey(MyKey))
                {
                    string LoadPath = "file:///" + GetFolderPath() + MyKey + "." + FileExtension;
                    WWW MyLoader = new WWW(LoadPath);
                    while (!MyLoader.isDone) { }
                    MySounds.Add(MyLoader.GetAudioClip(false, false, AudioType.WAV));
                }
            }
            return MySounds;
        }*/

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
                catch(System.IO.IsolatedStorage.IsolatedStorageException e)
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
                NewName += Random.Range(1, 10000);
            }
            if (Data.Keys.Contains(NewName) == false)
            {
                if (FolderName == DataFolderNames.PolyModels)
                {
                    VoxelManager.Get().AddModelRaw(NewElement as PolyModel);
                }
                else if (FolderName == DataFolderNames.Voxels)
                {
                    VoxelManager.Get().AddMetaRaw(NewElement as VoxelMeta);
                }
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

        public bool Remove(string FileName)
        {
            if (Data.ContainsKey(FileName))
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

    }
}