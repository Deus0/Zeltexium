using System.Collections.Generic;
using UnityEngine;
using Zeltex.Items;
using Zeltex.Combat;
using Zeltex.Util;
using System.IO;
using Zeltex.Voxels;
using System.Linq;
using Zeltex.Quests;

namespace Zeltex
{

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
        public ElementEvent ModifiedEvent = new ElementEvent();
        [Tooltip("When the file has saved - Update any connected UI to indicate a file change"), HideInInspector]
        public ElementEvent SavedEvent = new ElementEvent();
        [Tooltip("When the file has been renamed - Update any connected UI to indicate a file change"), HideInInspector]
        public ElementEvent RenamedEvent = new ElementEvent();
        [Tooltip("When the file has been renamed - Update any connected UI to indicate a file change"), HideInInspector]
        public ElementEvent MovedEvent = new ElementEvent();

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
                if (FolderName == DataFolderNames.PolygonModels)
                {
                    VoxelManager.Get().ClearModels();
                }
                else if (FolderName == DataFolderNames.VoxelMeta)
                {
                    VoxelManager.Get().ClearMetas();
                }
                else if (FolderName == DataFolderNames.VoxelDiffuseTextures)
                {
                    VoxelManager.Get().DiffuseTextures.Clear();
                }
                else if (FolderName == DataFolderNames.VoxelNormalTextures)
                {
                    VoxelManager.Get().NormalTextures.Clear();
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

        public List<Texture2D> LoadAllTextures()
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

		/// <summary>
		/// Other
		/// </summary>
		public string SetName(int FileIndex, string NewName)
		{
			return SetName(GetName(FileIndex), NewName);
		}

        /// <summary>
        /// Returns the new name
        ///     - If the old name and new name is the same, return
        ///     - While the name is in the database, incremenet a number on it
        /// </summary>
        public string SetName(string OldName, string NewName)
        {
            if (OldName == NewName)
            {
                return NewName;
            }
            string OriginalName = NewName;
            int NameTryCount = 1;
            while (Data.ContainsKey(NewName))
            {
                NameTryCount++;
                NewName = OriginalName + " " + NameTryCount;
                if (NameTryCount >= 100000)
                {
                    return "Wow, you broke the system.";
                }
            }
            // Cannot rename, have to remove and re add!
            if (Data.ContainsKey(NewName) == false)
            {
                string OldFileName = GetFolderPath() + OldName + "." + FileExtension;
                string NewFileName = GetFolderPath() + NewName + "." + FileExtension;
                // Delete file if exists
                if (File.Exists(OldFileName))
                {
                    Debug.LogError("Moving file: " + OldFileName + " to " + NewFileName);
                    File.Move(OldFileName, NewFileName);
                }

                T MyValue = Data[OldName];
                Data.Remove(OldName);
                Data.Add(NewName, MyValue);
                return NewName;
            }
            else
            {
                return OldName;
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
                if (FolderName == DataFolderNames.PolygonModels)
                {
                    VoxelManager.Get().AddModelRaw(NewElement as VoxelModel);
                }
                else if (FolderName == DataFolderNames.VoxelMeta)
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
    }
}