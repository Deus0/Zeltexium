/// <summary>
/// Data manager. Holds all of the games assets in one place.
/// </summary>
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Zeltex.Util;
using Zeltex.Voxels;

namespace Zeltex
{
    /// <summary>
    /// Used for Caching Texture Coordinates
    /// </summary>
    [System.Serializable()]
    public class ElementFolderDictionary : SerializableDictionaryBase<string, ElementFolder> 
    {
    }
    /// <summary>
    /// Generic class to load data from a file
    /// Initializes in editor too
    /// </summary>
    [ExecuteInEditMode]
    public partial class DataManager : ManagerBase<DataManager>
    {
        #region Variables
		[Header("Data")]
		public string MapName = "Zelnugg";
        [SerializeField, HideInInspector]
        private ElementFolderDictionary ElementFolders = new ElementFolderDictionary();

		[SerializeField, HideInInspector]
		public string ResourcesName = "ResourcesName";
		[SerializeField, HideInInspector]
		public string MapFolderPath = "";
		[SerializeField, HideInInspector]
		public string PathTypeKey = "PathType";
		[SerializeField, HideInInspector]
        public FilePathType MyFilePathType = FilePathType.StreamingPath;
        [SerializeField, HideInInspector]
        private bool IsJSONFormat = true;
        [SerializeField, HideInInspector]
        private bool IsLoaded;
        private bool IsLoadingAll;
        [Header("Events")]
        // Invoked when: Cleared, Add new file, Loaded files
        [Tooltip("Invoked when the file size changes")]
        public UnityEvent OnUpdatedResources = new UnityEvent();
        public UnityEvent OnBeginLoading = new UnityEvent();
        public UnityEvent OnEndLoading = new UnityEvent();
		public DataGUI MyGui;
        public int FormatType = 0;
        public bool WasPlayingOnLoad = false;
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
			MyGui = gameObject.GetComponent<DataGUI>();
			if (Application.isPlaying && IsLoaded == false)
			{
				RoutineManager.Get().StartCoroutine(LoadAllBeginning());
			}
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

        private IEnumerator LoadAllBeginning()
        {
            WasPlayingOnLoad = true;
            OnBeginLoading.Invoke();
            InitializeFolders();
            MapName = PlayerPrefs.GetString(ResourcesName, "Zelnugg");
            LogManager.Get().Log("Loading Map [" + DataManager.Get().MapName + "]");
            MakeStreaming();
#if !UNITY_ANDROID || UNITY_EDITOR
            yield return LoadAllRoutine();
#endif
            if (Application.isEditor == false)
            {
#if !UNITY_ANDROID && !UNITY_EDITOR
                MakePersistent();
                ElementFolder MyFolder = GetElementFolder(DataFolderNames.Saves);
                if (MyFolder != null)
                {
                    yield return (MyFolder.LoadAllElements());
                    OnUpdatedResources.Invoke();
                }
#endif
            }
            OnEndLoading.Invoke();
            yield return null;
        }

        public void DrawGui()
        {
            MyGui.DrawGui();
        }
        public List<ElementFolder> GetFolders()
        {
            return ElementFolders.GetValues();
        }

        public List<ElementFolder> GetElementFolders()
        {
            return ElementFolders.GetValues();
        }

        public Newtonsoft.Json.Formatting GetFormat()
        {
            if (FormatType == 0)
            {
                return Newtonsoft.Json.Formatting.None;
            }
            else
            {
                return Newtonsoft.Json.Formatting.Indented;
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
#endif
#if UNITY_ANDROID
                //On Android, use:
				MapFolderPath = "jar:file://" + Application.dataPath + "!/assets/";
#endif
#if UNITY_IOS
		                //On iOS, use:
				MapFolderPath = Application.dataPath + "/Raw";
#endif
#if UNITY_WEBGL
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
                    Directory.Delete(MapPath, true);
                    string MapPathMeta = ResourcesPath + ResourcesName + ".meta";
                    if (File.Exists(MapPathMeta))
                    {
                        File.Delete(MapPathMeta);
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
                //RenameName = ResourcesName;
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

            FolderNames.Add(DataFolderNames.VoxelDiffuseTextures);
            FolderNames.Add(DataFolderNames.VoxelNormalTextures);
            FolderNames.Add(DataFolderNames.StatTextures);
            FolderNames.Add(DataFolderNames.ItemTextures);

            FolderNames.Add(DataFolderNames.Sounds);
            FolderNames.Add(DataFolderNames.Musics);
            
            FolderNames.Add(DataFolderNames.Voxels);
            FolderNames.Add(DataFolderNames.Items);
            FolderNames.Add(DataFolderNames.Recipes);
            FolderNames.Add(DataFolderNames.Quests);
            FolderNames.Add(DataFolderNames.Spells);
            FolderNames.Add(DataFolderNames.Stats);
            FolderNames.Add(DataFolderNames.Dialogues);

            FolderNames.Add(DataFolderNames.PolyModels);
            FolderNames.Add(DataFolderNames.VoxelModels);
            FolderNames.Add(DataFolderNames.Skeletons);

            FolderNames.Add(DataFolderNames.Characters);
            FolderNames.Add(DataFolderNames.StatGroups);
            FolderNames.Add(DataFolderNames.Inventorys);
            FolderNames.Add(DataFolderNames.QuestLogs);
            FolderNames.Add(DataFolderNames.DialogueTrees);

            FolderNames.Add((DataFolderNames.Levels));
            FolderNames.Add((DataFolderNames.Saves));
            return FolderNames;
        }

        /// <summary>
        /// Clears and re adds all the folders
        /// </summary>
        public void InitializeFolders()
        {
            ElementFolders.Clear();

            // Element Folders
            ElementFolders.Add(DataFolderNames.Voxels, ElementFolder.Create(DataFolderNames.Voxels, "zel"));
            ElementFolders.Add(DataFolderNames.PolyModels, ElementFolder.Create(DataFolderNames.PolyModels, "zel"));
            ElementFolders.Add(DataFolderNames.VoxelModels, ElementFolder.Create(DataFolderNames.VoxelModels, "zel"));
            ElementFolders.Add(DataFolderNames.Skeletons, ElementFolder.Create(DataFolderNames.Skeletons, "zel"));
            ElementFolders.Add(DataFolderNames.Zanimations, ElementFolder.Create(DataFolderNames.Zanimations, "zel"));

            ElementFolders.Add(DataFolderNames.Items, ElementFolder.Create(DataFolderNames.Items, "zel"));
            ElementFolders.Add(DataFolderNames.Recipes, ElementFolder.Create(DataFolderNames.Recipes, "zel"));
            ElementFolders.Add(DataFolderNames.Inventorys, ElementFolder.Create(DataFolderNames.Inventorys, "zel"));

            ElementFolders.Add(DataFolderNames.Stats, ElementFolder.Create(DataFolderNames.Stats, "zel"));
            ElementFolders.Add(DataFolderNames.StatGroups, ElementFolder.Create(DataFolderNames.StatGroups, "zel"));
            ElementFolders.Add(DataFolderNames.Spells, ElementFolder.Create(DataFolderNames.Spells, "zel"));

            ElementFolders.Add(DataFolderNames.Quests, ElementFolder.Create(DataFolderNames.Quests, "zel"));
            ElementFolders.Add(DataFolderNames.QuestLogs, ElementFolder.Create(DataFolderNames.QuestLogs, "zel"));

            ElementFolders.Add(DataFolderNames.Dialogues, ElementFolder.Create(DataFolderNames.Dialogues, "zel"));
            ElementFolders.Add(DataFolderNames.DialogueTrees, ElementFolder.Create(DataFolderNames.DialogueTrees, "zel"));

            ElementFolders.Add(DataFolderNames.Characters, ElementFolder.Create(DataFolderNames.Characters, "zel"));
            ElementFolders.Add(DataFolderNames.Levels, ElementFolder.Create(DataFolderNames.Levels, "zel"));
            ElementFolders.Add(DataFolderNames.Saves, ElementFolder.Create(DataFolderNames.Saves, "zel"));

            ElementFolders.Add(DataFolderNames.Sounds, ElementFolder.Create(DataFolderNames.Sounds, "zel"));
            ElementFolders.Add(DataFolderNames.Musics, ElementFolder.Create(DataFolderNames.Musics, "zel"));
            ElementFolders.Add(DataFolderNames.VoxelDiffuseTextures, ElementFolder.Create(DataFolderNames.VoxelDiffuseTextures, "zel"));
            ElementFolders.Add(DataFolderNames.VoxelNormalTextures, ElementFolder.Create(DataFolderNames.VoxelNormalTextures, "zel"));
            ElementFolders.Add(DataFolderNames.ItemTextures, ElementFolder.Create(DataFolderNames.ItemTextures, "zel"));
            ElementFolders.Add(DataFolderNames.StatTextures, ElementFolder.Create(DataFolderNames.StatTextures, "zel"));

            // level scripts, each level contains a folder full of chunks and characters too
            //RefreshGuiMapNames();
        }

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
            Debug.Log("Clearing folders: " + ElementFolders.Count);
            foreach (KeyValuePair<string, ElementFolder> MyPair in ElementFolders)
            {
                MyPair.Value.Clear();
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

        public int GetSize()
        {
            return ElementFolders.Count;
        }
        public ElementFolder GetFolder(int Index)
        {
            return ElementFolders.GetValue(Index);
        }
        #region DataFolder

        /// <summary>
        /// Clear a particular folder
        /// </summary>
        public void Clear(string FolderName)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null)
            {
                MyFolder.Clear();
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

        private Zeltine MyLoadingRoutine;
        public void CancelLoading()
        {
            if (MyLoadingRoutine != null || IsLoadingAll == true)
            {
                Debug.Log("Cancelling Loading Data!");
                RoutineManager.Get().StopCoroutine(MyLoadingRoutine);
                MyLoadingRoutine = null;
                ClearAll();
                IsLoaded = false;
                IsLoadingAll = false;
            }
        }
        /// <summary>
        /// Loads all the folders
        /// </summary>
        public void LoadAll(Action OnFinishLoading = null)
        {
            if (IsLoaded == false)
            {
                LogManager.Get().Log("Loading From map path: " + GetMapPath() + " : " + MapName, "DataManager");
                if (string.IsNullOrEmpty(MapName) == false)
                {
                    //if (FileManagement.DirectoryExists(GetMapPath()))
                    {
                        IsLoadingAll = true;
                        MyLoadingRoutine = RoutineManager.Get().StartCoroutine(LoadAllRoutine(OnFinishLoading));
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

        private IEnumerator LoadAllRoutine(System.Action OnFinishLoading = null)
        {
            PlayerPrefs.SetString(ResourcesName, MapName);
            Debug.Log("Loading Resources from folder [" + MapName + "]");
            //RenameName = MapName;
            InitializeFolders();
            yield return (LoadAllElements());
            IsLoaded = true;
            yield return null;
            if (OnFinishLoading != null)
            {
                OnFinishLoading.Invoke();
            }
            MyLoadingRoutine = null;
            IsLoadingAll = false;
        }
		
		public bool IsLoading() 
		{
			return IsLoadingAll;
		}

        /// <summary>
        /// Save all the folders
        /// </summary>
        public void SaveAll()
        {
            Debug.Log("Saving " + ElementFolders.Count + " Folders");
            SaveAllElements();
		}

		/// <summary>
		/// Delete all the folders
		/// </summary>
		public void DeleteAll()
        {
            foreach (KeyValuePair<string, ElementFolder> MyPair in ElementFolders)
			{
                ElementFolder MyFolder = MyPair.Value;
                List<string> MyNames = MyFolder.GetNames();
				for (int j = 0; j < MyNames.Count; j++)
				{
                    MyFolder.DeleteFile(MyNames[j]);
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
            if (FolderName != null && ElementFolders.ContainsKey(FolderName) &&  ElementFolders[FolderName] != null)
            {
                return ElementFolders[FolderName].GetNames();
            }
            return new List<string>();
        }


        /// <summary>
        /// Renames a file from a folder, at an index
        /// </summary>
        public string SetName(string FolderName, int Index, string NewName)
        {
            ElementFolder ElementFolder = GetElementFolder(FolderName);
            if (ElementFolder != null)
            {
                Element MyElement = ElementFolder.Get(Index);
                if (MyElement != null)
                {
                    MyElement.SetName(NewName);
                    /*string OldName = ElementFolder.GetName(Index);
                    string NewName2 = ElementFolder.Get(Index).SetName(NewName);
                    if (OldName != NewName2)
                    {
                        MyElement.OnModified();
                    }*/
                    //return NewName2;
                    return MyElement.Name;
                }
            }
            return "";  // no name given
        }
        #endregion
		
		#region Other

		private List<string> GetStatisticsList()
		{
			List<string> MyStatistics = new List<string>();
			int TotalCount = ElementFolders.Count;   //SpellFolders.Count + ItemFolders.Count + StatFolders.Count +  TextureFolders.Count + AudioFolders.Count + 
			MyStatistics.Add("DataManager -:- Element Types: " + TotalCount);

            foreach (KeyValuePair<string, ElementFolder> MyPair in ElementFolders)
			{
                MyStatistics.Add(MyPair.Key + ": " + MyPair.Value.Data.Count);
			}
			return MyStatistics;
        }
		
        public AudioClip LatestPlayed;
		
        public void PlayClip(AudioClip clip)
        {
            gameObject.GetComponent<AudioSource>().PlayOneShot(clip);
        }
		
        public bool GetIsLoaded()
        {
            return IsLoaded;
        }

       // #region ImportVox
        private Int3 VoxelIndex = Int3.Zero();
		private int VoxelIndex2 = 0;
		//private Int3 VoxelIndex3 = Int3.Zero();
		private Color VoxelColor;
		private List<string> ImportedVoxelNames = new List<string>();
		private List<string> ImportVoxelDatas = new List<string>();
		public List<VoxelModel> ImportedModels = new List<VoxelModel>();

		public List<VoxelModel> GetImportedVoxelModels() 
		{
			List<VoxelModel> ImportedModels2 = new List<VoxelModel>();
			ImportedModels2.AddRange(ImportedModels);
			ImportedModels.Clear();
			return ImportedModels2;
		}
		
		public IEnumerator LoadVoxFiles()
		{
			ImportedModels.Clear();
			VoxelModel MyModel = new VoxelModel();
			MyModel.IsLoadingFromFile = true;
			ImportedModels.Add(MyModel);
			yield return (LoadVoxFile(true));
			if (ImportVoxelDatas.Count > 0)
			{
				MyModel.VoxelData = ImportVoxelDatas[ImportVoxelDatas.Count - 1];
				for (int i = 0; i < ImportVoxelDatas.Count; i++) 
				{
					if (ImportedModels.Count -1 < i)
					{
						ImportedModels.Add(new Voxels.VoxelModel());
					}
					ImportedModels[i].VoxelData = ImportVoxelDatas[i];
					ImportedModels[i].SetName(ImportedVoxelNames[i]);
				}
			}
			else
			{
				MyModel.VoxelData = "";
				MyModel.SetName("");
			}
			for (int i = 0; i < ImportedModels.Count; i++) 
			{
				ImportedModels[i].IsLoadingFromFile = false;
			}
		}

		/// <summary>
		/// Imports a vox file as a new voxel model
		/// </summary>
		/// <returns>The vox.</returns>
		public VoxelModel ImportVox()
		{
			Voxels.VoxelModel NewModel = new Voxels.VoxelModel();
			RoutineManager.Get().StartCoroutine(LoadVoxFile(NewModel));
			return NewModel;
        }

        public void ImportVox(VoxelModel MyModel)
        {
            RoutineManager.Get().StartCoroutine(LoadVoxFile(MyModel));
        }

		public IEnumerator LoadVoxFile(VoxelModel MyModel)
		{
			MyModel.IsLoadingFromFile = true;
			yield return LoadVoxFile(false);
			if (ImportVoxelDatas.Count > 0)
			{
                MyModel.VoxelData = ImportVoxelDatas[ImportVoxelDatas.Count - 1];
                MyModel.SetName(ImportedVoxelNames[ImportedVoxelNames.Count - 1]);
			}
			else
			{
				MyModel.VoxelData = "";
                Debug.LogError("Could not get data for importing model: " + MyModel.Name);
			}
			MyModel.IsLoadingFromFile = false;
            MyModel.OnModified();
		}

		public IEnumerator LoadVoxFile(bool IsMultiFiles = true, Voxels.World SpawnedWorld = null)
		{
			ImportVoxelDatas.Clear();
            ImportedVoxelNames.Clear();
			yield return null;
			#if UNITY_EDITOR// || UNITY_STANDALONE_WIN
				System.Windows.Forms.OpenFileDialog MyDialog = new System.Windows.Forms.OpenFileDialog();
				if (MyDialog == null) 
				{
					yield break;
				}
				MyDialog.Multiselect = IsMultiFiles;
                if (IsMultiFiles)
                {
                    MyDialog.Title = "Import Vox Files";
                }
                else
                {
                    MyDialog.Title = "Import Vox";
                }

				System.Windows.Forms.DialogResult MyResult = MyDialog.ShowDialog();
				//string FilePath = MyDialog.FileName;
				string [] FilePaths = MyDialog.FileNames;
				if (MyResult == System.Windows.Forms.DialogResult.OK)
				{
                    if (FilePaths.Length > 0) 
                    {
                        for (int i = 0; i < FilePaths.Length; i++) 
                        {
                            yield return ImportVoxFileSingle(FilePaths[i], SpawnedWorld);
                        }
                    }
                    else if (MyDialog.FileName != null) 
                    {
                        yield return ImportVoxFileSingle(MyDialog.FileName, SpawnedWorld);
                    }
                    else 
                    {
                        Debug.LogError("[MVVoxModel] Invalid file path");
                    }
				}
				else
				{
					Debug.LogError("[MVVoxModel] Invalid file path");
				}
				MyDialog.Dispose();
			#endif
		}
		
		public IEnumerator ImportVoxFileSingle(string FilePath, Voxels.World SpawnedWorld) 
		{
            Debug.Log("Importing Vox File at: " + FilePath + " with Spawned World? " + (SpawnedWorld != null).ToString());
			string VoxelModelName = Path.GetFileNameWithoutExtension(FilePath);
			ImportedVoxelNames.Add(VoxelModelName);
			MVMainChunk MyVoxelMainChunk = MVImporter.LoadVOX(FilePath, null);
			if (MyVoxelMainChunk == null && MyVoxelMainChunk.voxelChunk == null)
			{
				Debug.LogError(FilePath + " Has an invalid vox file.");
				yield break;
			}

			Voxels.VoxelData Data = null;
			// First get the size of the voxels
			Int3 NewWorldSize = new Int3(
                Mathf.CeilToInt(MyVoxelMainChunk.voxelChunk.sizeX / (float)Voxels.Chunk.ChunkSize),
                Mathf.CeilToInt(MyVoxelMainChunk.voxelChunk.sizeY / (float)Voxels.Chunk.ChunkSize),
                Mathf.CeilToInt(MyVoxelMainChunk.voxelChunk.sizeZ / (float)Voxels.Chunk.ChunkSize));
            Debug.Log("Loading world from .Vox: " + MyVoxelMainChunk.voxelChunk.sizeX + ", " + MyVoxelMainChunk.voxelChunk.sizeY + ", " + MyVoxelMainChunk.voxelChunk.sizeZ 
                + " --- " + NewWorldSize.GetVector().ToString());
			if (SpawnedWorld)
			{
				if (SpawnedWorld)
				{
					SpawnedWorld.IsChunksCentred = false;
					yield return SpawnedWorld.SetWorldSizeRoutine(NewWorldSize);
					SpawnedWorld.name = VoxelModelName;
				}
			}
			else
			{
				// Create new voxel Data here
				Data = new Voxels.VoxelData(Voxels.Chunk.ChunkSize * NewWorldSize.x, Voxels.Chunk.ChunkSize * NewWorldSize.y, Voxels.Chunk.ChunkSize * NewWorldSize.z);
			}

			// Read the data
			for (VoxelIndex.x = 0; VoxelIndex.x < MyVoxelMainChunk.voxelChunk.sizeX; VoxelIndex.x++)
			{
				for (VoxelIndex.y = 0; VoxelIndex.y < MyVoxelMainChunk.voxelChunk.sizeY; VoxelIndex.y++)
				{
					for (VoxelIndex.z = 0; VoxelIndex.z < MyVoxelMainChunk.voxelChunk.sizeZ; VoxelIndex.z++)
					{
						VoxelIndex2 = (int)MyVoxelMainChunk.voxelChunk.voxels[VoxelIndex.x, VoxelIndex.y, VoxelIndex.z];
						//VoxelIndex3.Set(VoxelIndex.x - MyVoxelMainChunk.voxelChunk.sizeX / 2,
						//    VoxelIndex.y, VoxelIndex.z - MyVoxelMainChunk.voxelChunk.sizeZ / 2);
						if (VoxelIndex2 > 0)
						{
							//Debug.Log(MyVoxelMainChunk.voxelChunk.voxels[x, y, z].ToString());
							// minus 1 off the pallete to get the real index
							VoxelColor = MyVoxelMainChunk.palatte[VoxelIndex2 - 1];
							if (SpawnedWorld)
							{
    							//Debug.Log(VoxelIndex.ToString() + " -TO- " + VoxelColor.ToString());
    							SpawnedWorld.UpdateBlockTypeMass("Color", VoxelIndex, VoxelColor);
							}
							else
							{
							    Data.SetVoxelTypeColorRaw(VoxelIndex, 1, VoxelColor);
							}
						}
						else
						{
							if (SpawnedWorld)
							{
								SpawnedWorld.UpdateBlockTypeMass("Air", VoxelIndex);
							}
						}
					}
				}
			}
			// if was loading to a world, update the meshes
			if (SpawnedWorld)
			{
				SpawnedWorld.OnMassUpdate();
			}
			else
			{
				int VoxelType = 0;
				System.Text.StringBuilder ImportVoxelDataBuilder = new System.Text.StringBuilder();
                if (!(NewWorldSize.x == 1 && NewWorldSize.y == 1 && NewWorldSize.z == 1))
                {
                    //Debug.Log("Importing voxel model of size: " + NewWorldSize.GetVector().ToString());
                    Int3 VoxelWorldIndex = Int3.Zero();
                    Int3 VoxelChunkPosition = Int3.Zero();
                    ImportVoxelDataBuilder.AppendLine("/World" + ' ' + NewWorldSize.x.ToString() + ' ' + NewWorldSize.y.ToString() + ' ' + NewWorldSize.z.ToString());
                    // break up data into chunks of 16x16x16
                    for (int i = 0; i < NewWorldSize.x; i++) 
                    {
                        for (int j = 0; j < NewWorldSize.y; j++) 
                        {
                            for (int k = 0; k < NewWorldSize.z; k++) 
                            {
                                VoxelChunkPosition.Set(i, j, k);
                                ImportVoxelDataBuilder.AppendLine("/Chunk" + ' ' + VoxelChunkPosition.x.ToString() + ' ' + VoxelChunkPosition.y.ToString() + ' ' + VoxelChunkPosition.z.ToString());
                                for (VoxelIndex.x = 0; VoxelIndex.x < Chunk.ChunkSize; VoxelIndex.x++)
                                {
                                    for (VoxelIndex.y = 0; VoxelIndex.y < Chunk.ChunkSize; VoxelIndex.y++)
                                    {
                                        for (VoxelIndex.z = 0; VoxelIndex.z < Chunk.ChunkSize; VoxelIndex.z++)
                                        {
                                            VoxelWorldIndex.Set(
                                                VoxelIndex.x + VoxelChunkPosition.x * Chunk.ChunkSize, 
                                                VoxelIndex.y + VoxelChunkPosition.y * Chunk.ChunkSize, 
                                                VoxelIndex.z + VoxelChunkPosition.z * Chunk.ChunkSize);
                                            VoxelType = Data.GetVoxelType(VoxelWorldIndex);
                                            if (VoxelType > 0)
                                            {
                                                VoxelColor = Data.GetVoxelColorColor(VoxelWorldIndex);
                                                int Red = (int)(255f * VoxelColor.r);
                                                int Green = (int)(255f * VoxelColor.g);
                                                int Blue = (int)(255f * VoxelColor.b);
                                                ImportVoxelDataBuilder.AppendLine("" + 1 + " " + Red + " " + Green + " " + Blue);
                                            }
                                            else
                                            {
                                                ImportVoxelDataBuilder.AppendLine(0.ToString());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else 
                {

                    for (VoxelIndex.x = 0; VoxelIndex.x < Data.GetSize().x; ++VoxelIndex.x)
                    {
                        for (VoxelIndex.y = 0; VoxelIndex.y < Data.GetSize().y; ++VoxelIndex.y)
                        {
                            for (VoxelIndex.z = 0; VoxelIndex.z < Data.GetSize().z; ++VoxelIndex.z)
                            {
                                VoxelType = Data.GetVoxelType(VoxelIndex);
                                if (VoxelType > 0)
                                {
                                    VoxelColor = Data.GetVoxelColorColor(VoxelIndex);
                                    int Red = (int)(255f * VoxelColor.r);
                                    int Green = (int)(255f * VoxelColor.g);
                                    int Blue = (int)(255f * VoxelColor.b);
                                    ImportVoxelDataBuilder.AppendLine("" + 1 + " " + Red + " " + Green + " " + Blue);
                                }
                                else
                                {
                                    ImportVoxelDataBuilder.AppendLine(0.ToString());
                                }
                            }
                        }
                    }
                }
				ImportVoxelDatas.Add(ImportVoxelDataBuilder.ToString());
			}
		}

		#endregion

		#region ImportsExports

		public void ImportPolygon(int FileIndex)
		{
			//ElementFolder MyFolder = GetElementFolder(FolderName);
			//if (MyFolder != null)
			#if UNITY_EDITOR// || UNITY_STANDALONE_WIN
			{
				System.Windows.Forms.OpenFileDialog MyDialog = new System.Windows.Forms.OpenFileDialog();
				System.Windows.Forms.DialogResult MyResult = MyDialog.ShowDialog();
				if (MyResult == System.Windows.Forms.DialogResult.OK)
				{
					//Mesh MyMesh = ObjImport.ImportFile(MyDialog.FileName);
					//byte[] bytes = FileUtil.LoadBytes(MyDialog.FileName);
					//MyZexel.LoadImage(bytes);
				}
				else
				{
					Debug.LogError("Failure to open file.");
				}
			}
			#endif
		}

		public void ImportPolygon2(Voxels.PolyModel MyModel) 
		{
			#if UNITY_EDITOR// || UNITY_STANDALONE_WIN
			System.Windows.Forms.OpenFileDialog MyDialog = new System.Windows.Forms.OpenFileDialog();
			System.Windows.Forms.DialogResult MyResult = MyDialog.ShowDialog();
			if (MyResult == System.Windows.Forms.DialogResult.OK)
			{
				// PolyModel SelectedModel = GetSelectedModel();
				Mesh MyMesh = ObjImport.ImportFile(MyDialog.FileName);
				MyMesh.RecalculateBounds();
				MyMesh.RecalculateNormals();
				MyModel.UseMesh(MyMesh);
				//MyViewer.GetSpawn().GetComponent<MeshFilter>().sharedMesh = MyMesh;
				//MyViewer.GetSpawn().GetComponent<PolyModelHandle>().RefreshMesh();
				//UpdateStatistics();
			}
			#endif
		}

		public void ExportZexel(Zexel MyZexel)
		{
			if (MyZexel.GetTexture() != null)
			{
				#if UNITY_EDITOR
				System.Windows.Forms.SaveFileDialog MySaveFileDialog = new System.Windows.Forms.SaveFileDialog();
				MySaveFileDialog.Filter = "*." + "png" + "| *." + "png";
				//OpenFileDialog open = new OpenFileDialog();
				MySaveFileDialog.Title = "Export a Texture";
				MySaveFileDialog.ShowDialog();
				if (MySaveFileDialog.FileName != "")
				{
					//byte[] MyBytes = System.Convert.FromBase64String(Data);
					FileUtil.SaveBytes(MySaveFileDialog.FileName, (MyZexel.GetTexture()).EncodeToPNG());
                }
#else
            Debug.LogError("Export Image not supported by platform.");
#endif
            }
        }

		public void ExportPolygon(Voxels.PolyModel MyModel)
		{
			#if UNITY_EDITOR// || UNITY_STANDALONE_WIN
			System.Windows.Forms.SaveFileDialog MyDialog = new System.Windows.Forms.SaveFileDialog();
			System.Windows.Forms.DialogResult MyResult = MyDialog.ShowDialog();
			if (MyResult == System.Windows.Forms.DialogResult.OK)
			{
				string MyMeshString = MeshToString(MyModel);
				FileUtil.Save(MyDialog.FileName, MyMeshString);
			}
			#endif
		}

		public static string MeshToString(Voxels.PolyModel MyModel)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			List<Vector3> Verticies = MyModel.GetAllVerts();
			List<Vector2> UVs = MyModel.GetTextureMapCoordinates(0, new TileMap(8, 8, 16, 16));
			//List<Vector3> Normals = MyModel.GetAllNormals();

			sb.Append("g ").Append(MyModel.Name).Append("\n");
			foreach (Vector3 v in Verticies)
			{
				sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
			}
			/*b.Append("\n");
			/foreach (Vector3 v in mesh.normals)
			{
				sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
			}
			sb.Append("\n");*/
			foreach (Vector3 v in UVs)
			{
				sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
			}
			Material[] mats = new Material[1];//MyMeshFilter.GetComponent<MeshRenderer>().sharedMaterials;
			mats[0] = Voxels.VoxelManager.Get().MyMaterials[0];
			for (int material = 0; material < mats.Length; material++)
			{
				sb.Append("\n");
				sb.Append("usemtl ").Append(mats[material].name).Append("\n");
				sb.Append("usemap ").Append(mats[material].name).Append("\n");

				int[] triangles = MyModel.GetAllTriangles().ToArray();
				for (int i = 0; i < triangles.Length; i += 3)
				{
					sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
						triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
				}
			}
			return sb.ToString();
		}

		public void ExportPolygon(MeshFilter MyMeshFilter)//int FileIndex)
		{
			#if UNITY_EDITOR// || UNITY_STANDALONE_WIN
			System.Windows.Forms.SaveFileDialog MyDialog = new System.Windows.Forms.SaveFileDialog();
			System.Windows.Forms.DialogResult MyResult = MyDialog.ShowDialog();
			if (MyResult == System.Windows.Forms.DialogResult.OK)
			{
				string MyMeshString = MeshToString(MyMeshFilter);
				FileUtil.Save(MyDialog.FileName, MyMeshString);
			}
			#endif
		}

		public static string MeshToString(MeshFilter MyMeshFilter)
		{
			Mesh mesh = MyMeshFilter.sharedMesh;

			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			sb.Append("g ").Append(mesh.name).Append("\n");
			foreach (Vector3 v in mesh.vertices)
			{
				sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
			}
			sb.Append("\n");
			foreach (Vector3 v in mesh.normals)
			{
				sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
			}
			sb.Append("\n");
			foreach (Vector3 v in mesh.uv)
			{
				sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
			}
			Material[] mats = MyMeshFilter.GetComponent<MeshRenderer>().sharedMaterials;
			for (int material = 0; material < mesh.subMeshCount; material++)
			{
				sb.Append("\n");
				sb.Append("usemtl ").Append(mats[material].name).Append("\n");
				sb.Append("usemap ").Append(mats[material].name).Append("\n");

				int[] triangles = mesh.GetTriangles(material);
				for (int i = 0; i < triangles.Length; i += 3)
				{
					sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
						triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
				}
			}
			return sb.ToString();
		}

		public void ImportImage(string FolderName, int FileIndex)
		{
			Zexel MyZexel = GetElement(FolderName, FileIndex) as Zexel;
			if (MyZexel != null)
			{
				ImportImage(MyZexel);
			}
		}

		public void ImportImage(Zexel MyZexel)
		{
			#if UNITY_EDITOR// || UNITY_STANDALONE_WIN
            string FilePath = OpenFile("png");
            if (FilePath != "")
			{
                MyZexel.LoadImage(FileUtil.LoadBytes(FilePath));
            }
            #endif
        }

        /// <summary>
        /// Opens a file of type
        /// </summary>
        public string OpenFile(string FileTypeExtension) 
        {
            #if UNITY_EDITOR
                return UnityEditor.EditorUtility.OpenFilePanel("Open a []", "", FileTypeExtension);
            #else
                return "";
            #endif
        }
        /*
         * if UNITY_EDITOR
                System.Windows.Forms.OpenFileDialog MyDialog = new System.Windows.Forms.OpenFileDialog();
                System.Windows.Forms.DialogResult MyResult = MyDialog.ShowDialog();
                if (MyResult == System.Windows.Forms.DialogResult.OK)
                {
                    return MyDialog.FileName;
                }
            #endif*/

		public void ImportZound(string FolderName, Sound.Zound MyZound)
		{
			ElementFolder MyFolder = GetElementFolder(FolderName);
			if (MyFolder != null)
			{
				#if UNITY_EDITOR// || UNITY_STANDALONE_WIN
				System.Windows.Forms.OpenFileDialog MyDialog = new System.Windows.Forms.OpenFileDialog();
				System.Windows.Forms.DialogResult MyResult = MyDialog.ShowDialog();
				if (MyResult == System.Windows.Forms.DialogResult.OK)
				{
					UniversalCoroutine.CoroutineManager.StartCoroutine(ImportSoundRoutine(MyDialog.FileName, MyZound));
				}
				else
				{
					Debug.LogError("Failure to open file.");
				}
				#endif
			}
			else
			{
				Debug.LogError("Failed to find folder: " + FolderName);
			}
		}

		private IEnumerator ImportSoundRoutine(string FileName, Sound.Zound MyZound)
		{
			WWW MyWavLoader = new WWW("file://" + FileName);
			yield return MyWavLoader;
			LatestPlayed = MyWavLoader.GetAudioClip();
			MyZound.UseAudioClip(LatestPlayed);
		}
		#endregion

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
                if (MyElements != null)
                {
                    for (int i = 0; i < MyElements.Count; i++)
                    {
                        if (MyElements[i].CanSave())
                        {
                            IsModified = true;
                            break;
                        }
                    }
                }
                else
                {
                    Debug.LogError("Folder " + FolderName + " Has null data..");
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
                MyFolder.SaveAllElements();
			}
			else
			{
				Debug.LogError(FolderName + " was not found.");
			}
		}

		/// <summary>
		/// Get the quest folder
		/// </summary>
		public ElementFolder GetElementFolder(string FolderName)
		{
            if (ElementFolders.ContainsKey(FolderName))
            {
                return ElementFolders[FolderName];
            }
			return null;
		}

		private void LoadFolder(string FolderName)
		{
			//Debug.LogError("Loading: " + FolderName);
			ElementFolder MyFolder = GetElementFolder(FolderName);
			if (MyFolder != null)
			{
				UniversalCoroutine.CoroutineManager.StartCoroutine(MyFolder.LoadAllElements());
				//Debug.LogError("Loading SUCCESS: " + FolderName);
				OnUpdatedResources.Invoke();
			}
		}

		/// <summary>
		/// Loads all the data!
		/// </summary>
		private IEnumerator LoadAllElements()
		{
            //Debug.Log("Loading all elements for [" + MapName + "]");
            foreach (KeyValuePair<string, ElementFolder> MyPair in ElementFolders)
			{
                yield return UniversalCoroutine.CoroutineManager.StartCoroutine(MyPair.Value.LoadAllElements());
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
            foreach (KeyValuePair<string, ElementFolder> MyPair in ElementFolders)
			{
                MyPair.Value.SaveAllElements();
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
				if (MyFolder.SetElement(NewElement))
				{
					OnUpdatedResources.Invoke();
				}
			}
		}

        /// <summary>
        /// Push the data as a new one!
        /// </summary>
        public void PushElement(string FolderName, Element NewElement)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null && NewElement != null)
            {
                NewElement = NewElement.Clone();
                if (MyFolder.Get(NewElement.Name) != null)
                {
                    if (MyFolder.SetElement(NewElement))
                    {
                        NewElement.MyFolder = MyFolder;
                        NewElement.OnModified();
                        OnUpdatedResources.Invoke();
                        Debug.Log(NewElement.Name + " is overwriting previous value in database folder: " + FolderName);
                    }
                    else
                    {
                        Debug.LogError(NewElement.Name + " failed to overwrite previous value in database folder: " + FolderName);
                    }
                }
                else
                {
                    MyFolder.AddElement(NewElement);
                    Debug.Log(NewElement.Name + " is being added to the database folder: " + FolderName);
                }
            }
        }

		public void AddNew(string FolderName)
		{
			ElementFolder MyFolder = GetElementFolder(FolderName);
			if (MyFolder != null)
			{
				MyFolder.AddNewElement();
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
				if (FolderName == DataFolderNames.Voxels)
				{
					Voxels.VoxelManager.Get().RemoveMeta(FileIndex);
				}
				if (FolderName == DataFolderNames.PolyModels)
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
					if (FolderName == DataFolderNames.Voxels)
					{
						Voxels.VoxelManager.Get().RemoveMeta(IndexOf);
					}
					if (FolderName == DataFolderNames.PolyModels)
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