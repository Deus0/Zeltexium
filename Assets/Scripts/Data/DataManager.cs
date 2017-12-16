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
        [Header("Events")]
        // Invoked when: Cleared, Add new file, Loaded files
        [Tooltip("Invoked when the file size changes")]
        public UnityEvent OnUpdatedResources = new UnityEvent();
        public UnityEvent OnBeginLoading = new UnityEvent();
        public UnityEvent OnEndLoading = new UnityEvent();
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
            UniversalCoroutine.CoroutineManager.StartCoroutine(LoadAllRoutine2());
        }

        public List<Element> GetElements(string FolderName)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null)
            {
                return MyFolder.GetData();
            }
            return new List<Element>();
        }

        private System.Collections.IEnumerator LoadAllRoutine2()
        {
            OnBeginLoading.Invoke();
            InitializeFolders();
            DataManager.Get().MapName = PlayerPrefs.GetString(DataManager.Get().ResourcesName, "Zelnugg");
            LogManager.Get().Log("Loading Map [" + DataManager.Get().MapName + "]");
            MakeStreaming();
            yield return UniversalCoroutine.CoroutineManager.StartCoroutine(LoadAllRoutine());
            if (Application.isEditor == false)
            {
                MakePersistent();
                ElementFolder MyFolder = GetElementFolder(DataFolderNames.Saves);
                if (MyFolder != null)
                {
                    yield return UniversalCoroutine.CoroutineManager.StartCoroutine(MyFolder.LoadAllElements());
                    OnUpdatedResources.Invoke();
                }
            }
            OnEndLoading.Invoke();
        }

        System.Collections.IEnumerator ClearConsole()
        {
            // wait until console visible
            while (!Debug.developerConsoleVisible)
            {
                yield return null;
            }
            yield return null; // this is required to wait for an additional frame, without this clearing doesn't work (at least for me)

            // Debug.ClearDeveloperConsole();
#if UNITY_EDITOR
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.SceneView));
            System.Type logEntries = assembly.GetType("UnityEditorInternal.LogEntries");
            System.Reflection.MethodInfo clearConsoleMethod = logEntries.GetMethod("Clear");
            clearConsoleMethod.Invoke(new object(), null);
#endif
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

        public string GetMapName()
        {
            if (MapName == "")
            {
                MapName = "Zelnugg";
            }
            return MapName;
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

        public string GetMapPath(string NewMapName)
        {
            if (NewMapName != "")
            {
                return GetResourcesPath() + NewMapName + "/";
            }
            else return GetResourcesPath();
        }

        public void MakeStreaming()
        {
            MyFilePathType = FilePathType.StreamingPath;
        }

        public void MakePersistent()
        {
            MyFilePathType = FilePathType.PersistentPath;
        }

        public string GetResourcesPath()
        {
            return GetResourcesPath(MyFilePathType);
        }

        /// <summary>
        /// Folder Path of maps
        /// </summary>
        public string GetResourcesPath(FilePathType MyType)
        {
            if (MyType == FilePathType.PersistentPath)
            {
                // "C:/Users/Marz/AppData/LocalLow/Zeltex/Zeltex/";//  + " / ";// + "/Resources/";
                MapFolderPath = Application.persistentDataPath + "/";
            }
            else if (MyType == FilePathType.StreamingPath)
            {
                // mac or OS
#if UNITY_EDITOR || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
                MapFolderPath = Application.dataPath + "/StreamingAssets/";

#elif UNITY_ANDROID
                //On Android, use:
                 MapFolderPath = "jar:file://" + Application.dataPath + "!/assets/";
#elif UNITY_IOS
                //On iOS, use:
                 MapFolderPath = Application.dataPath + "/Raw";
#elif UNITY_WEBGL
                MapFolderPath = Application.streamingAssetsPath + "/";
#endif
            }
            else
            {
                MapFolderPath = Application.dataPath + "/";
                MapFolderPath += "Resources/";
#if UNITY_EDITOR
                MapFolderPath += "ResourcePacks/";
#endif
            }
            if (!FileManagement.DirectoryExists(MapFolderPath, true, true))
            {
                FileManagement.CreateDirectory(MapFolderPath, true);
                Debug.Log("Created Resouces FolderPath [" + MapFolderPath + "]");
            }
            return MapFolderPath;
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
                if (FileManagement.DirectoryExists(MapPath))
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
            if (!FileManagement.DirectoryExists(GetMapPath(), true, true))
            {
                FileManagement.CreateDirectory(GetMapPath(), true);
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
            if (!FileManagement.DirectoryExists(DirectoryPath, true, true))
            {
                Debug.Log("Created directory [" + DirectoryPath + "]");
                FileManagement.CreateDirectory(DirectoryPath, true);
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
            StringFolders.Clear();
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
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.Saves, "zel"));

                ElementFolders.Add(ElementFolder.Create(DataFolderNames.Sounds, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.Musics, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.VoxelDiffuseTextures, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.VoxelNormalTextures, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.ItemTextures, "zel"));
                ElementFolders.Add(ElementFolder.Create(DataFolderNames.StatTextures, "zel"));

                // level scripts, each level contains a folder full of chunks and characters too
                RefreshGuiMapNames();
            }
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
            Files.AddRange(FileManagement.ListDirectories(GetResourcesPath()));
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
                    //if (FileManagement.DirectoryExists(GetMapPath()))
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
		}

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