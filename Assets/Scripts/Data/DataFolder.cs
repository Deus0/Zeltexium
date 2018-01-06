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
    /// <summary>
    /// A reference to a folder
    /// </summary>
    [System.Serializable]
	public class DataFolder<T>
	{
		[Header("Data")]
		public string FolderName = "None";                              // The name of the folder, used to save a list of files
		public string FileExtension = "err";                            // the particular type of file used to save the data
        public DataDictionary<T> RealData = new DataDictionary<T>();
        public virtual DataDictionary<T> Data
        {
            get { return RealData; }
            set { RealData = value; }
        }

        [Header("Events")]
        [Tooltip("When the file has modified - Update any connected UI to indicate a file change"), HideInInspector]
		public ElementFolderEvent ModifiedEvent = new ElementFolderEvent();
        [Tooltip("When the file has saved - Update any connected UI to indicate a file change"), HideInInspector]
		public ElementFolderEvent SavedEvent = new ElementFolderEvent();
        [Tooltip("When the file has been renamed - Update any connected UI to indicate a file change"), HideInInspector]
		public ElementFolderEvent RenamedEvent = new ElementFolderEvent();
        [Tooltip("When the file has been renamed - Update any connected UI to indicate a file change"), HideInInspector]
		public ElementFolderEvent MovedEvent = new ElementFolderEvent();

        public void Set(string NewFolderName, string NewFileExtension)
        {
            FolderName = NewFolderName;
            FileExtension = NewFileExtension;
        }

        public static DataFolder<T> Create(string NewFolderName, string NewFileExtension)
        {
            DataFolder<T> NewFolder = new DataFolder<T>();
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

        private void LoadAllNames()
        {
            Clear();
            List<string> MyFiles = FileUtil.GetFilesOfType(GetFolderPath(), FileExtension);
            Debug.Log("[" + FolderName + "] Found: " + MyFiles.Count + "\nFolderPath: " + GetFolderPath() + " --- " + FileExtension);
            MyFiles = FileUtil.SortAlphabetically(MyFiles);
            List<string> MyNames = new List<string>();
            for (int i = 0; i < MyFiles.Count; i++)
            {
                MyNames.Add(Path.GetFileNameWithoutExtension(MyFiles[i]));
                Data.Add(MyNames[i], default(T));
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
		}

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

		public List<T> GetData()
        {
            List<T> MyData = new List<T>();
            try
            {
                if (Data != null)
                {
                    foreach (var KeyVarPair in Data)
                    {
                        if (KeyVarPair.Value != null)
                        {
                            MyData.Add(KeyVarPair.Value);
                        }
                    }
                }
            }
            catch (System.ObjectDisposedException e)
            {
                Debug.LogError("Bug Found in DataFolder: " + e.ToString());
            }
            return MyData;
        }

        public List<string> GetNames()
        {
            List<string> MyNames = new List<string>();
            foreach (var Key in Data.Keys)
            {
                MyNames.Add(Key);
            }
            return MyNames;
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
				Data.Add(NewName, default(T));
			}
			else
			{
				//Debug.LogError("Cannot add another " + NewName + " to " + FolderName);
			}
        }

        public void ReAdd(string ElementName, T MyElement)
        {
            if (Data.ContainsValue(MyElement))
            {
                Data.Remove(ElementName);
                Data.Add(ElementName, MyElement);
            }
        }

        public bool SetElement(string NewName, T NewElement)
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
        public bool Add(string NewName, T NewElement)
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
        public T Get(string FileName)
        {
            if (Data.ContainsKey(FileName))
            {
                return Data[FileName];
            }
            else
            {
                return default(T);
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
        public T Get(int FileIndex)
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
            Debug.LogError("Could not find file " + FileIndex + " inside of " + FolderName);
            return default(T);
        }

        /// <summary>
        /// Set a value by file name
        /// </summary>
        public void Set(string FileName, T NewData)
        {
            if (Data.ContainsKey(FileName))
            {
                Data[FileName] = NewData;
            }
        }

        /// <summary>
        /// set a generic value by index
        /// </summary>
        public void Set(int FileIndex, T NewData)
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
				foreach (KeyValuePair<string, T> MyValuePair in Data)
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