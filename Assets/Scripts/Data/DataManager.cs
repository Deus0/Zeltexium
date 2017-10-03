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
    [ExecuteInEditMode]
    public partial class DataManager : ManagerBase<DataManager>
    {
        #region Variables
        [Header("Debug")]
        public bool IsDebugGui;
        private KeyCode DebugOpenKey = KeyCode.F1;
        public bool IsLoad;     // loads all the data
        [Header("Data")]
        [SerializeField]
        private List<ElementFolder> ElementFolders = new List<ElementFolder>();
        public List<StringFolder> StringFolders = new List<StringFolder>();
        
        private string RenameName = "Null";
        //private List<string> MyResourceNames = new List<string>();
        [Header("Events")]
        // Invoked when: Cleared, Add new file, Loaded files
        [Tooltip("Invoked when the file size changes")]
        public UnityEvent OnUpdatedResources = new UnityEvent();

        public string ResourcesName = "ResourcesName";
        public string MapName = "Zelnugg";
        public string MapFolderPath = "";
        public string PathTypeKey = "PathType";
        public FilePathType MyFilePathType = FilePathType.StreamingPath;
        [SerializeField, HideInInspector]
        private bool IsJSONFormat = true;
        [SerializeField, HideInInspector]
        private bool IsLoaded;
        //[SerializeField, HideInInspector]
        //private bool IsInitialized;
        #endregion

        #region Mono

        public new static DataManager Get()
        {
            if (MyManager == null)
            {
                GameObject LayerManagerObject = GameObject.Find("DataManager");
                if (LayerManagerObject)
                {
                    MyManager = LayerManagerObject.GetComponent<DataManager>();
                }
                else
                {
                    Debug.LogError("Could not find [DataManager].");
                }
            }
            return MyManager;
        }

        private void Start()
        {
            InitializeFolders();
            DataManager.Get().MapName = PlayerPrefs.GetString(DataManager.Get().ResourcesName, "Zelnugg");
            LogManager.Get().Log("Loading Map [" + DataManager.Get().MapName + "]");
            //if (DataManager.Get().MapName != "")
            {
                DataManager.Get().LoadAll();
            }
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(DebugOpenKey))
            {
                IsDebugGui = !IsDebugGui;
            }
            if (IsLoad)
            {
                IsLoad = false;
                LoadAll();
            }
        }
        #endregion

        #region Path

        public static string GetFolderPath(string SubFolderName)
        {
            return DataManager.Get().GetFolderPathNS(SubFolderName);
        }

        /// <summary>
        /// The main function for the files folder path
        /// </summary>
        public string GetFolderPathNS(string SubFolderName)//, string FileExtension)
        {
            string FolderPath = "";// = FileUtil.GetSaveFolderPath(, MyWorld);
            FolderPath = GetMapPathNS() + SubFolderName;// + "/";
            //Debug.Log("Returning new FolderPath: " + FolderPath);
            return FolderPath;
        }

        public static string GetMapPath()
        {
            return DataManager.Get().GetMapPathNS();
        }
        /// <summary>
        /// Folder path of current map
        /// </summary>
        public string GetMapPathNS()
        {
            string CurrentMapPath = GetResourcesPath();
            if (MapName == "")
            {
                MapName = "Zelnugg";
            }
            if (MapName != "")
            {
                CurrentMapPath += DataManager.Get().MapName + "/";
            }
            return CurrentMapPath;
        }

        public static string GetMapPath(string NewMapName)
        {
            if (NewMapName != "")
            {
                return GetResourcesPath() + NewMapName + "/";
            }
            else return GetResourcesPath();
        }

        /// <summary>
        /// Folder Path of maps
        /// </summary>
        public static string GetResourcesPath()
        {
            if (DataManager.Get().MyFilePathType == FilePathType.PersistentPath)
            {
                // "C:/Users/Marz/AppData/LocalLow/Zeltex/Zeltex/";//  + " / ";// + "/Resources/";
                DataManager.Get().MapFolderPath = Application.persistentDataPath + "/";
            }
            else if (DataManager.Get().MyFilePathType == FilePathType.StreamingPath)
            {
                // mac or OS
#if UNITY_EDITOR || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
                DataManager.Get().MapFolderPath = Application.dataPath + "/StreamingAssets/";

#elif UNITY_ANDROID
                //On Android, use:
                 DataManager.Get().MapFolderPath = "jar:file://" + Application.dataPath + "!/assets/";
#elif UNITY_IOS
                //On iOS, use:
                 DataManager.Get().MapFolderPath = Application.dataPath + "/Raw";
#elif UNITY_WEBGL
                DataManager.Get().MapFolderPath = Application.streamingAssetsPath + "/";
#endif
            }
            else
            {
                DataManager.Get().MapFolderPath = Application.dataPath + "/";
                DataManager.Get().MapFolderPath += "Resources/";
#if UNITY_EDITOR
                DataManager.Get().MapFolderPath += "ResourcePacks/";
#endif
            }
            if (!System.IO.Directory.Exists(DataManager.Get().MapFolderPath))
            {
                System.IO.Directory.CreateDirectory(DataManager.Get().MapFolderPath);
                Debug.Log("Created Resouces FolderPath [" + DataManager.Get().MapFolderPath + "]");
            }
            return DataManager.Get().MapFolderPath;
        }
        #endregion


        #region Initialize

        /// <summary>
        /// Erases resources
        /// </summary>
        public bool EraseResouces(string ResourcesName)
        {
            if(ResourcesName != "")
            {
                string ResourcesPath = GetResourcesPath();
                string MapPath = ResourcesPath + ResourcesName + "/";
                Debug.Log("Attempting to Erasing ResourcesPack [" + ResourcesName + "] " + MapPath);
                if (System.IO.Directory.Exists(MapPath))
                {
                    Debug.Log("Erasing Resources [" + MapPath + "]");
                    System.IO.Directory.Delete(MapPath, true);
                    string MapPathMeta = ResourcesPath + ResourcesName + ".meta";
                    if (System.IO.File.Exists(MapPathMeta))
                    {
                        System.IO.File.Delete(MapPathMeta);
                    }
                    return true;
                }
                else
                {
                    Debug.Log("Resources Directory did not exist [" + MapPath + "]");
                }
            }
            else
            {
                Debug.Log("Could not erase Resources of null");
            }
            return false;
        }

        /// <summary>
        /// Creates a new resouces pack
        /// </summary>
        public bool CreateResources(string ResourcesName)
        {
            if (IsLoaded)
            {
                UnloadAll();
            }
            if (IsLoaded == false)
            {
                CreateDirectories();
                MapName = ResourcesName;
                RenameName = ResourcesName;
                IsLoaded = true;
                OnUpdatedResources.Invoke();
                return true;
            }
            else
            {
                MapName = "";
            }
            return false;
        }

        private void CreateDirectories()
        {
            if (!System.IO.Directory.Exists(GetMapPath()))
            {
                System.IO.Directory.CreateDirectory(GetMapPath());
            }
            Debug.Log("Creating Resouces directory [" + GetMapPath() + "]");
            List<string> FolderNames = GetFolderNames();
            List<string> DirectoryPaths = new List<string>();
            for (int i = 0; i < FolderNames.Count; i++)
            {
                DirectoryPaths.Add(GetFolderPath(FolderNames[i]));
            }
            for (int i = 0; i < DirectoryPaths.Count; i++)
            {
                CreateDirectory(DirectoryPaths[i]);
            }
        }

        public string GetStatistics()
        {
            return FileUtil.ConvertToSingle(GetStatisticsList());
        }

        private void CreateDirectory(string DirectoryPath)
        {
            if (!System.IO.Directory.Exists(DirectoryPath))
            {
                Debug.Log("Created directory [" + DirectoryPath + "]");
                System.IO.Directory.CreateDirectory(DirectoryPath);
            }
            else
            {
                //Debug.LogError("Somehow path existed before creation: " + DirectoryPath);
            }
        }



        public List<string> GetFolderNames()
        {
            List<string> FolderNames = new List<string>();
            //FolderNames.Add(DataFolderNames.Classes);
            FolderNames.Add(DataFolderNames.Items);
            FolderNames.Add(DataFolderNames.Recipes);
            FolderNames.Add(DataFolderNames.Quests);
            FolderNames.Add(DataFolderNames.Spells);
            FolderNames.Add(DataFolderNames.Stats);
            FolderNames.Add(DataFolderNames.VoxelMeta);
            
            FolderNames.Add(DataFolderNames.Characters);
            FolderNames.Add(DataFolderNames.StatGroups);
            FolderNames.Add(DataFolderNames.Inventorys);
            FolderNames.Add(DataFolderNames.QuestLogs);
            FolderNames.Add(DataFolderNames.DialogueTrees);

            FolderNames.Add(DataFolderNames.VoxelDiffuseTextures);
            FolderNames.Add(DataFolderNames.VoxelNormalTextures);
            FolderNames.Add(DataFolderNames.StatTextures);
            FolderNames.Add(DataFolderNames.ItemTextures);

            FolderNames.Add((DataFolderNames.Levels));
            FolderNames.Add((DataFolderNames.Saves));

            FolderNames.Add(DataFolderNames.Sounds);
            FolderNames.Add(DataFolderNames.Skeletons);
            FolderNames.Add(DataFolderNames.VoxelModels);
            FolderNames.Add(DataFolderNames.Voxels);
            FolderNames.Add(DataFolderNames.Dialogues);
            FolderNames.Add(DataFolderNames.PolygonModels);
            return FolderNames;
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeFolders()
        {
            if (ElementFolders.Count == 0)
            {
                Debug.LogError("Element Folders are 0.");
                //IsInitialized = false;
            }
            //if (!IsInitialized)
            {
                //IsInitialized = true;
                //MyFilePathType = (FilePathType)PlayerPrefs.GetInt(PathTypeKey, 0);

                ElementFolders.Clear();

                // Element Folders
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.VoxelMeta, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.PolygonModels, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.VoxelModels, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.Skeletons, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.Zanimations, "zel"));

                ElementFolders.Add(ElementFolder.Create(DataFolderNames.Items, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.Recipes, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.Inventorys, "zel"));

                ElementFolders.Add(ElementFolder.Create(DataFolderNames.Stats, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.StatGroups, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.Spells, "zel"));

                ElementFolders.Add(ElementFolder.Create(DataFolderNames.Quests, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.QuestLogs, "zel"));

                ElementFolders.Add(ElementFolder.Create(DataFolderNames.Dialogues, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.DialogueTrees, "zel"));

                ElementFolders.Add(ElementFolder.Create(DataFolderNames.Characters, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.Levels, "zel"));

                ElementFolders.Add(ElementFolder.Create(DataFolderNames.Sounds, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.Musics, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.VoxelDiffuseTextures, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.VoxelNormalTextures, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.ItemTextures, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.StatTextures, "zel"));

                // level scripts, each level contains a folder full of chunks and characters too
                RefreshGuiMapNames();
            }
           // StringFolders.Clear();
            //StringFolders.Add(StringFolder.Create(DataFolderNames.Skeletons, "skl"));
            //AudioFolders.Clear();
            //TextureFolders.Clear();
        }
        public Texture2D TestTexture;
        public Texture2D TestTexture2;

        /// <summary>
        /// Unloads all data
        /// </summary>
        public bool UnloadAll()
        {
            if (IsLoaded)
            {
                IsLoaded = false;
                ClearAll();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// A way to clear the resources without setting the map to unloaded
        /// </summary>
        public void ClearAll()
        {
            /*foreach (DataFolder<string> MyFolder in StringFolders)
            {
                MyFolder.Clear();
            }
            foreach (DataFolder<Texture2D> MyFolder in TextureFolders)
            {
                MyFolder.Clear();
            }
            foreach (DataFolder<AudioClip> MyFolder in AudioFolders)
            {
                MyFolder.Clear();
            }*/
            foreach (ElementFolder MyFolder in ElementFolders)
            {
                MyFolder.Clear();
            }
            OnUpdatedResources.Invoke();
        }

        public string RenameResources(string NewName)
        {
            if (MapName == NewName)
            {
                return NewName;
            }
            // get list of current resources
            List<string> Files = new List<string>();
            Files.AddRange(System.IO.Directory.GetDirectories(GetResourcesPath()));
            for (int i = 0; i < Files.Count; i++)
            {
                Files[i] = System.IO.Path.GetFileNameWithoutExtension(Files[i]);
            }
            string OriginalNewName = NewName;
            int NameTryCount = 1;
            while (Files.Contains(NewName))
            {
                NameTryCount++;
                NewName = OriginalNewName + " " + NameTryCount;
                if (NameTryCount >= 100000)
                {
                    return MapName;
                }
            }
            string DirectoryA = GetMapPath(MapName);
            MapName = NewName;
            string DirectoryB = GetMapPath(MapName);
            Debug.Log("MOving directory from [" + DirectoryA + "] to [" + DirectoryB + "]");
            System.IO.Directory.Move(DirectoryA, DirectoryB);
            return NewName;
        }
        #endregion

        #region DataFolder

        /// <summary>
        /// Clear a particular folder
        /// </summary>
        public void Clear(string FolderName)
        {
            DataFolder<string> MyFolder = Get(FolderName);
            if (MyFolder != null)
            {
                MyFolder.Clear();
            }
        }

        /// <summary>
        /// Adds the data
        /// </summary>
        public void Add(string FolderName)
        {
            for (int i = 0; i < StringFolders.Count; i++)
            {
                if (StringFolders[i].FolderName == FolderName)
                {
                    StringFolders[i].New("New" + Random.Range(1,100000));
                    //DataFolders[i].MyNames.Add("");
                    //DataFolders[i].MyData.Add("");
                }
            }
        }

        /// <summary>
        /// Adds the data, used in voxel manager for new meta
        /// </summary>
        public void AddEmptyString(string FolderName, string NewName)
        {
            for (int i = 0; i < StringFolders.Count; i++)
            {
                if (StringFolders[i].FolderName == FolderName)
                {
                    StringFolders[i].New(NewName);
                    //DataFolders[i].MyNames.Add(NewName);
                    //DataFolders[i].MyData.Add("");
                }
            }
        }
        #endregion

        #region FileAccess

        /// <summary>
        /// Loads a resources pack!
        /// </summary>
        public void LoadResources(string NewName)
        {
            MapName = NewName;
            LoadAll();
        }

        /// <summary>
        /// Loads all the folders
        /// </summary>
        public void LoadAll()
        {
            if (IsLoaded == false)
            {
                LogManager.Get().Log("Loading From map path: " + GetMapPath() + " : " + MapName, "DataManager");
                if (string.IsNullOrEmpty(MapName) == false)
                {
                    //if (System.IO.Directory.Exists(GetMapPath()))
                    {
                        UniversalCoroutine.CoroutineManager.StartCoroutine(LoadAllRoutine());
                    }
                    //else
                    {
                        //Debug.LogWarning(MapName + " Directory Path does not exist.");
                    }
                }
                else
                {
                    Debug.LogError("ResourcesName is null");
                }
            }
            else
            {
                LogManager.Get().Log("Cannot load as already loaded resources.: " + GetResourcesPath(), "DataManager");
                //Debug.Log("Cannot load as already loaded resources.");
            }
        }

        private System.Collections.IEnumerator LoadAllRoutine()
        {
            //CreateDirectories();    // do the thing
            PlayerPrefs.SetString(ResourcesName, MapName);
            Debug.Log("Loading Resources from folder [" + MapName + "]");
            RenameName = MapName;
            InitializeFolders();
            yield return UniversalCoroutine.CoroutineManager.StartCoroutine(LoadAllElements());
            LoadAllStrings();
            IsLoaded = true;
            yield return null;
        }

        /// <summary>
        /// Save all the folders
        /// </summary>
        public void SaveAll()
        {
            Debug.Log("Saving " + StringFolders.Count + " Folders");
            SaveAllStrings();
            SaveAllElements();
			//SaveAllTextures();
			//SaveAllAudio();
		}

		/// <summary>
		/// Save all the folders
		/// </summary>
		/*public void SaveAll(string FolderName)
		{
			Debug.Log("Saving " + StringFolders.Count + " Folders");
            DataFolder<string> MyFolder = Get(FolderName);
            if (MyFolder != null)
			{
				//MyFolder.SaveFile();
			}
		}*/

		/// <summary>
		/// Delete all the folders
		/// </summary>
		public void DeleteAll()
		{
			for (int i = 0; i < StringFolders.Count; i++)
			{
				List<string> MyNames = StringFolders[i].GetNames();
				for (int j = 0; j < MyNames.Count; j++)
				{
					StringFolders[i].DeleteFile(MyNames[j]);
				}
			}
			for (int i = 0; i < ElementFolders.Count; i++)
			{
				List<string> MyNames = ElementFolders[i].GetNames();
				for (int j = 0; j < MyNames.Count; j++)
				{
					ElementFolders[i].DeleteFile(MyNames[j]);
				}
			}
			/*for (int i = 0; i < TextureFolders.Count; i++)
			{
				List<string> MyNames = TextureFolders[i].GetNames();
				for (int j = 0; j < MyNames.Count; j++)
				{
					TextureFolders[i].DeleteFile(MyNames[j]);
				}
			}
			for (int i = 0; i < AudioFolders.Count; i++)
			{
				List<string> MyNames = AudioFolders[i].GetNames();
				for (int j = 0; j < MyNames.Count; j++)
				{
					AudioFolders[i].DeleteFile(MyNames[j]);
				}
			}*/
		}
        #endregion

        #region Naming

        public int GetFileIndex(string FolderName, string FileName)
        {
            List<string> FileNames = GetNames(FolderName);
            //Debug.LogError("Looking for: " + FileName + " : " + FolderName);
            for (int i = 0; i < FileNames.Count; i++)
            {
                if (FileNames[i] == FileName)
                {
                    return i;
                }
            }
            return -1;
        }

        public List<string> GetNames(string FolderName)
        {
            DataFolder<string> MyFolder = Get(FolderName);
            if (MyFolder != null)
            {
                return MyFolder.GetNames();
            }
            ElementFolder ElementFolder = GetElementFolder(FolderName);
            if (ElementFolder != null)
            {
                return ElementFolder.GetNames();
            }
            return new List<string>();
        }


        /// <summary>
        /// Renames a file from a folder, at an index
        /// </summary>
        public string SetName(string FolderName, int Index, string NewName)
        {
            DataFolder<string> MyFolder = Get(FolderName);
            if (MyFolder != null)
            {
                //Debug.LogError("Getting name: " + Index + " Inside " + FolderName);
                return MyFolder.SetName(Index, NewName);
            }
            ElementFolder ElementFolder = GetElementFolder(FolderName);
            if (ElementFolder != null)
            {
                Element MyElement = ElementFolder.Get(Index);
                if (MyElement != null)
                {
                    string OldName = ElementFolder.GetName(Index);
                    string NewName2 = ElementFolder.SetName(Index, NewName);
                    if (OldName != NewName2)
                    {
                        MyElement.OnModified();
                    }
                    return NewName2;
                }
            }
            return "";  // no name given
        }
        #endregion
    }
}

/*


        #region Stats
        private void LoadAllStats()
        {
            for (int i = 0; i < StatFolders.Count; i++)
            {
                List<Stat> MyData = StatFolders[i].LoadAllStats();
                for (int j = 0; j < MyData.Count; j++)
                {
                    StatFolders[i].Set(j, MyData[j]);
                }
            }
        }

        private DataFolder<Stat> GetStatFolder(string FolderName)
        {
            for (int i = 0; i < StatFolders.Count; i++)
            {
                if (StatFolders[i].FolderName == FolderName)
                {
                    return StatFolders[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Get an item  from a folder, using an index
        /// </summary>
        public Stat GetStat(string FolderName, int Index)
        {
            DataFolder<Stat> MyFolder = GetStatFolder(FolderName);
            if (MyFolder != null)
            {
                return MyFolder.Get(Index);
            }
            return null;
        }
        /// <summary>
        /// Get an item  from a folder, using an index
        /// </summary>
        public Stat GetStat(string FolderName, string FileName)
        {
            DataFolder<Stat> MyFolder = GetStatFolder(FolderName);
            if (MyFolder != null)
            {
                return MyFolder.Get(FileName);
            }
            return null;
        }

        /// <summary>
        /// Add a texture
        /// </summary>
        public void AddStat(string FolderName, Stat NewStat)
        {
            DataFolder<Stat> MyFolder = GetStatFolder(FolderName);
            if (MyFolder != null && NewStat != null)
            {
                MyFolder.Add(NewStat.Name, NewStat);
            }
        }

        /// <summary>
        /// Removes a particular data
        /// </summary>
        public void RemoveStat(string FolderName, int FileIndex)
        {
            DataFolder<Stat> MyFolder = GetStatFolder(FolderName);
            if (MyFolder != null)
            {
                MyFolder.Remove(FileIndex);
            }
        }

        /// <summary>
        /// returns the size of a folder
        /// </summary>
        public int GetSizeStats(string FolderName)
        {
            DataFolder<Stat> MyFolder = GetStatFolder(FolderName);
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

        #region Items
        private void LoadAllItems()
        {
            for (int i = 0; i < ItemFolders.Count; i++)
            {
                List<Item> MyData = ItemFolders[i].LoadAllItems();
                for (int j = 0; j < MyData.Count; j++)
                {
                    ItemFolders[i].Set(j, MyData[j]);
                }
            }
        }
        private DataFolder<Item> GetItemFolder(string FolderName)
        {
            for (int i = 0; i < ItemFolders.Count; i++)
            {
                if (ItemFolders[i].FolderName == FolderName)
                {
                    return ItemFolders[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Get an item  from a folder, using an index
        /// </summary>
        public Item GetItem(string FolderName, int Index)
        {
            DataFolder<Item> MyFolder = GetItemFolder(FolderName);
            if (MyFolder != null)
            {
                Item MyItem = MyFolder.Get(Index);
                if (MyItem == null)
                {
                    //Debug.LogError("[DataManager] Item " + Index + " is null");
                }
                return MyItem;
            }
            return null;
        }

        /// <summary>
        /// Add a texture
        /// </summary>
        public void AddItem(string FolderName, Item NewItem)
        {
            DataFolder<Item> MyFolder = GetItemFolder(FolderName);
            if (MyFolder != null && NewItem != null)
            {
                MyFolder.Add(NewItem.Name, NewItem);
            }
        }

        /// <summary>
        /// Removes a particular data
        /// </summary>
        public void RemoveItem(string FolderName, int FileIndex)
        {
            DataFolder<Item> MyFolder = GetItemFolder(FolderName);
            if (MyFolder != null)
            {
                MyFolder.Remove(FileIndex);
            }
        }

        /// <summary>
        /// returns the size of a folder
        /// </summary>
        public int GetSizeItems(string FolderName)
        {
            DataFolder<Item> MyFolder = GetItemFolder(FolderName);
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

 #region Quests

/// <summary>
/// Get the quest folder
/// </summary>
private DataFolder<Quest> GetQuestFolder(string FolderName)
{
    for (int i = 0; i < QuestFolders.Count; i++)
    {
        if (QuestFolders[i].FolderName == FolderName)
        {
            return QuestFolders[i];
        }
    }
    return null;
}

private void LoadAllQuests()
{
    for (int i = 0; i < QuestFolders.Count; i++)
    {
        List<Quest> MyData = QuestFolders[i].LoadAllQuests();
        for (int j = 0; j < MyData.Count; j++)
        {
            QuestFolders[i].Set(j, MyData[j]);
        }
    }
}
/// <summary>
/// Get an item  from a folder, using an index
/// </summary>
public Quest GetQuest(string FolderName, int Index)
{
    DataFolder<Quest> MyFolder = GetQuestFolder(FolderName);
    if (MyFolder != null)
    {
        return MyFolder.Get(Index);
    }
    return null;
}

/// <summary>
/// Get an item  from a folder, using an index
/// </summary>
public Quest GetQuest(string FolderName, string FileName)
{
    DataFolder<Quest> MyFolder = GetQuestFolder(FolderName);
    if (MyFolder != null && MyFolder.Data.ContainsKey(FileName))
    {
        return MyFolder.Get(FileName);
    }
    return null;
}

/// <summary>
/// Add a texture
/// </summary>
public void AddQuest(string FolderName, Quest NewQuest)
{
    DataFolder<Quest> MyFolder = GetQuestFolder(FolderName);
    if (MyFolder != null && NewQuest != null)
    {
        MyFolder.Add(NewQuest.Name, NewQuest);
    }
}

/// <summary>
/// Removes a particular data
/// </summary>
public void RemoveQuest(string FolderName, int FileIndex)
{
    DataFolder<Quest> MyFolder = GetQuestFolder(FolderName);
    if (MyFolder != null)
    {
        MyFolder.Remove(FileIndex);
    }
}

/// <summary>
/// returns the size of a folder
/// </summary>
public int GetSizeQuests(string FolderName)
{
    DataFolder<Quest> MyFolder = GetQuestFolder(FolderName);
    if (MyFolder != null)
    {
        return MyFolder.Data.Count;
    }
    else
    {
        return 0;
    }
}
#endregion*/

/*#region Recipes

/// <summary>
/// Get the quest folder
/// </summary>
private DataFolder<Recipe> GetRecipeFolder(string FolderName)
{
    for (int i = 0; i < RecipeFolders.Count; i++)
    {
        if (RecipeFolders[i].FolderName == FolderName)
        {
            return RecipeFolders[i];
        }
    }
    return null;
}
/// <summary>
/// Get an item  from a folder, using an index
/// </summary>
public Recipe GetRecipe(string FolderName, int Index)
{
    DataFolder<Recipe> MyFolder = GetRecipeFolder(FolderName);
    if (MyFolder != null)
    {
        return MyFolder.Get(Index);
    }
    return null;
}

/// <summary>
/// Get an item  from a folder, using an index
/// </summary>
public Recipe GetRecipe(string FolderName, string FileName)
{
    DataFolder<Recipe> MyFolder = GetRecipeFolder(FolderName);
    if (MyFolder != null && MyFolder.Data.ContainsKey(FileName))
    {
        return MyFolder.Get(FileName);
    }
    return null;
}

/// <summary>
/// Add a texture
/// </summary>
public void AddRecipe(string FolderName, Recipe NewQuest)
{
    DataFolder<Recipe> MyFolder = GetRecipeFolder(FolderName);
    if (MyFolder != null && NewQuest != null)
    {
        MyFolder.Add(NewQuest.Name, NewQuest);
    }
}

/// <summary>
/// Removes a particular data
/// </summary>
public void RemoveRecipe(string FolderName, int FileIndex)
{
    DataFolder<Recipe> MyFolder = GetRecipeFolder(FolderName);
    if (MyFolder != null)
    {
        MyFolder.Remove(FileIndex);
    }
}

/// <summary>
/// returns the size of a folder
/// </summary>
public int GetSizeRecipes(string FolderName)
{
    DataFolder<Recipe> MyFolder = GetRecipeFolder(FolderName);
    if (MyFolder != null)
    {
        return MyFolder.Data.Count;
    }
    else
    {
        return 0;
    }
}
#endregion*/
/* #region Spells

 private void LoadAllSpells()
 {

     for (int i = 0; i < SpellFolders.Count; i++)
     {
         List<Spell> MyData = SpellFolders[i].LoadAllSpells();
         for (int j = 0; j < MyData.Count; j++)
         {
             SpellFolders[i].Set(j, MyData[j]);
         }
     }
 }
 private DataFolder<Spell> GetSpellFolder(string FolderName)
 {
     for (int i = 0; i < SpellFolders.Count; i++)
     {
         if (SpellFolders[i].FolderName == FolderName)
         {
             return SpellFolders[i];
         }
     }
     return null;
 }
 /// <summary>
 /// Get an item  from a folder, using an index
 /// </summary>
 public Spell GetSpell(string FolderName, int Index)
 {
     DataFolder<Spell> MyFolder = GetSpellFolder(FolderName);
     if (MyFolder != null)
     {
         return MyFolder.Get(Index);
     }
     return null;
 }
 /// <summary>
 /// Get an item  from a folder, using an index
 /// </summary>
 public Spell GetSpell(string FolderName, string FileName)
 {
     DataFolder<Spell> MyFolder = GetSpellFolder(FolderName);
     if (MyFolder != null && MyFolder.Data.ContainsKey(FileName))
     {
         return MyFolder.Get(FileName);
     }
     return null;
 }

 /// <summary>
 /// Add a texture
 /// </summary>
 public void AddSpell(string FolderName, Spell NewSpell)
 {
     DataFolder<Spell> MyFolder = GetSpellFolder(FolderName);
     if (MyFolder != null && NewSpell != null)
     {
         MyFolder.Add(NewSpell.Name, NewSpell);
     }
 }

 /// <summary>
 /// Removes a particular data
 /// </summary>
 public void RemoveSpell(string FolderName, int FileIndex)
 {
     DataFolder<Spell> MyFolder = GetSpellFolder(FolderName);
     if (MyFolder != null)
     {
         MyFolder.Remove(FileIndex);
     }
 }

 /// <summary>
 /// returns the size of a folder
 /// </summary>
 public int GetSizeSpells(string FolderName)
 {
     DataFolder<Spell> MyFolder = GetSpellFolder(FolderName);
     if (MyFolder != null)
     {
         return MyFolder.Data.Count;
     }
     else
     {
         return 0;
     }
 }
 #endregion*/

//#region FilePaths
/*public string GetFilePath()
{
    SetFilePaths();
    return FileUtil.GetFolderPath(FolderName);
}
public string GetFilePath(string MyName)
{
    SetFilePaths();
    return FileUtil.GetFolderPath(FolderName) + MyName + "." + FileExtension;
}
public string GetFilePath(int MyIndex)
{
    SetFilePaths();
    return FileUtil.GetFolderPath(FolderName) + MyNames[MyIndex] + "." + FileExtension;
}
/// <summary>
/// Returns a list of the file names in the folder path
/// </summary>
protected List<string> GetFileNames()
{
    string FilePath = GetFilePath();
    return FileUtil.GetFilesOfType(FilePath, FileExtension);
}*/
//#endregion

/*
 * ItemTextures
    StatTextures
    VoxelTexturesNormals
    VoxelTexturesDiffuse
    png

    Sounds
    Music
    wav

    PolygonModels
    vmd

    Stats
    sts

    VoxelMeta
    vmt

    VoxelModels
    vxm

    ItemMeta
    itm

    Classes
    txt

    Dialogues
    dlg

    Quests
    qst

    Spells
    spl

    Skeletons
    skl
 * 
 * */

/*public string Rename(string MyInput)
{
    // make sure name doesn't already exist
    string OriginalName = MyInput;
    bool HasFoundNewName = false;
    int Checks = 1;
    while (HasFoundNewName == false)
    {
        if (MyNames.Contains(MyInput))  //ScriptUtil.RemoveWhiteSpace(
        {
            Checks++;
            MyInput = OriginalName + " (" + Checks + ")";
        }
        else
        {
            HasFoundNewName = true;
        }
    }
    return MyInput;
}*/
/// <summary>
/// Deletes the current file
/// </summary>
/*protected virtual void DeleteFile()
{
    //DeleteFile(GetSelectedName());
}
public void DeleteFile(string MyName)
{
    string MyFileName = FileUtil.GetFolderPath(FolderName) + MyName + "." + FileExtension;
    FileUtil.Delete(MyFileName);
}
/// <summary>
/// Used just for texture maker
/// </summary>
public void DeleteFile(string PathName, string MyName)
{
    string MyFileName = FileUtil.GetFolderPath(PathName) + MyName + "." + FileExtension;
    FileUtil.Delete(MyFileName);
}*/
//#endregion