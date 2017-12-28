﻿using System.Collections.Generic;
using System.Reflection;
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
		[Header("Data")]
		public string MapName = "Zelnugg";
        [SerializeField, HideInInspector]
        private List<ElementFolder> ElementFolders = new List<ElementFolder>();
        //public List<StringFolder> StringFolders = new List<StringFolder>();
        //private List<string> MyResourceNames = new List<string>();

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
        //[SerializeField, HideInInspector]
        //private bool IsInitialized;
        [Header("Events")]
        // Invoked when: Cleared, Add new file, Loaded files
        [Tooltip("Invoked when the file size changes")]
        public UnityEvent OnUpdatedResources = new UnityEvent();
        public UnityEvent OnBeginLoading = new UnityEvent();
        public UnityEvent OnEndLoading = new UnityEvent();
		public DataGUI MyGui;
        #endregion

        #region Mono

		public void DrawGui() 
		{
			MyGui.DrawGui ();
		}
		public List<ElementFolder> GetFolders()
		{
			return ElementFolders;
		}

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
			if (Application.isPlaying)
			{
				UniversalCoroutine.CoroutineManager.StartCoroutine(LoadAllRoutine2());
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

        private System.Collections.IEnumerator LoadAllRoutine2()
        {
            OnBeginLoading.Invoke();
            InitializeFolders();
            DataManager.Get().MapName = PlayerPrefs.GetString(DataManager.Get().ResourcesName, "Zelnugg");
            LogManager.Get().Log("Loading Map [" + DataManager.Get().MapName + "]");
            MakeStreaming();
#if !UNITY_ANDROID || UNITY_EDITOR
            yield return UniversalCoroutine.CoroutineManager.StartCoroutine(LoadAllRoutine());
#endif
            if (Application.isEditor == false)
            {
#if !UNITY_ANDROID || UNITY_EDITOR
                MakePersistent();
                ElementFolder MyFolder = GetElementFolder(DataFolderNames.Saves);
                if (MyFolder != null)
                {
                    yield return UniversalCoroutine.CoroutineManager.StartCoroutine(MyFolder.LoadAllElements());
                    OnUpdatedResources.Invoke();
                }
#endif
            }
            OnEndLoading.Invoke();
            yield return null;
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
            FolderNames.Add(DataFolderNames.PolyModels);
            FolderNames.Add(DataFolderNames.Voxels);
            FolderNames.Add(DataFolderNames.Dialogues);
            FolderNames.Add(DataFolderNames.PolyModels);
            return FolderNames;
        }

        /// <summary>
        /// Clears and re adds all the folders
        /// </summary>
        public void InitializeFolders()
        {
            ElementFolders.Clear();

            // Element Folders
            ElementFolders.Add(ElementFolder.Create(DataFolderNames.VoxelMeta, "zel"));
            ElementFolders.Add(ElementFolder.Create(DataFolderNames.PolyModels, "zel"));
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
            DataFolder<Element> MyFolder = GetElementFolder(FolderName);
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
            //RenameName = MapName;
            InitializeFolders();
            yield return UniversalCoroutine.CoroutineManager.StartCoroutine(LoadAllElements());
            IsLoaded = true;
            yield return null;
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


		private List<string> GetStatisticsList()
		{
			List<string> MyStatistics = new List<string>();
			int TotalCount = ElementFolders.Count;   //SpellFolders.Count + ItemFolders.Count + StatFolders.Count +  TextureFolders.Count + AudioFolders.Count + 
			MyStatistics.Add("DataManager -:- Element Types: " + TotalCount);

			for (int i = 0; i < ElementFolders.Count; i++)
			{
				MyStatistics.Add("[" + (i + 1) + "] " + ElementFolders[i].FolderName + ": " + ElementFolders[i].Data.Count);
			}
			return MyStatistics;
		}


		#region ImportVox
		private Int3 VoxelIndex = Int3.Zero();
		private int VoxelIndex2 = 0;
		private Int3 VoxelIndex3 = Int3.Zero();
		private Color VoxelColor;
		private string ImportVoxelData = "";

		public System.Collections.IEnumerator LoadVoxFile(Voxels.VoxelModel MyModel)
		{
			yield return UniversalCoroutine.CoroutineManager.StartCoroutine(LoadVoxFile());
			MyModel.VoxelData = ImportVoxelData;
			MyModel.OnModified();
		}

		public System.Collections.IEnumerator LoadVoxFile(Voxels.World SpawnedWorld = null)
		{
			yield return null;
			#if UNITY_EDITOR// || UNITY_STANDALONE_WIN
			System.Windows.Forms.OpenFileDialog MyDialog = new System.Windows.Forms.OpenFileDialog();
			System.Windows.Forms.DialogResult MyResult = MyDialog.ShowDialog();
			string FilePath = MyDialog.FileName;
			if (MyResult == System.Windows.Forms.DialogResult.OK && FilePath != null && FilePath.Length > 0)
			{
				MVMainChunk MyVoxelMainChunk = MVImporter.LoadVOX(FilePath, null);
				if (MyVoxelMainChunk != null)
				{
					//if (SpawnedWorld != null)
					{
					Debug.Log("Loading world from .Vox: " + MyVoxelMainChunk.voxelChunk.sizeX + ", " + MyVoxelMainChunk.voxelChunk.sizeY + ", " + MyVoxelMainChunk.voxelChunk.sizeZ);
					Int3 NewWorldSize = new Int3(
					Mathf.CeilToInt(MyVoxelMainChunk.voxelChunk.sizeX / Voxels.Chunk.ChunkSize) + 1,
					Mathf.CeilToInt(MyVoxelMainChunk.voxelChunk.sizeY / Voxels.Chunk.ChunkSize) + 1,
					Mathf.CeilToInt(MyVoxelMainChunk.voxelChunk.sizeZ / Voxels.Chunk.ChunkSize) + 1);
					Voxels.VoxelData Data = null;
					if (SpawnedWorld)
					{
						SpawnedWorld.IsChunksCentred = false;
						yield return UniversalCoroutine.CoroutineManager.StartCoroutine(SpawnedWorld.SetWorldSizeRoutine(NewWorldSize));
						/*while (SpawnedWorld.IsWorldLoading())
						{
						yield return null;
						}*/
					}
					else
					{
						// Create new voxel Data here
						Data = new Voxels.VoxelData(
						Voxels.Chunk.ChunkSize * Mathf.CeilToInt((float) MyVoxelMainChunk.voxelChunk.sizeX / Voxels.Chunk.ChunkSize),
						Voxels.Chunk.ChunkSize * Mathf.CeilToInt((float)MyVoxelMainChunk.voxelChunk.sizeY / Voxels.Chunk.ChunkSize),
						Voxels.Chunk.ChunkSize * Mathf.CeilToInt((float)MyVoxelMainChunk.voxelChunk.sizeZ / Voxels.Chunk.ChunkSize));
					}
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
										SpawnedWorld.UpdateBlockTypeMass(
										"Color",
										VoxelIndex,
										VoxelColor);
									}
									else
									{
										Data.SetVoxelTypeColorRaw(VoxelIndex, 1, VoxelColor);
									}
									/*MassUpdateVoxelIndex = MyBlockType;
									MassUpdateVoxelName = MyWorld.MyLookupTable.GetName(MyBlockType);
									MassUpdateColor = Color.white;
									MassUpdatePosition.Set(LoadingVoxelIndex);
									UpdateBlockTypeLoading();*/
								}
								else
								{
									if (SpawnedWorld)
									{
										//Debug.Log(VoxelIndex.ToString() + " -TO- Air");
										SpawnedWorld.UpdateBlockTypeMass("Air", VoxelIndex);
									}
					/*else
					{
					Data.SetVoxelTypeRaw(VoxelIndex, 0);
					}*/
								}
							}
						}
					}
					if (SpawnedWorld)
					{
						Debug.Log("Vox Import OnMassUpdate for: " + SpawnedWorld.name);
						Voxels.WorldUpdater.Get().Clear(SpawnedWorld);
						SpawnedWorld.OnMassUpdate();
						Voxels.WorldUpdater.Get().IsUpdating = false;
						//SpawnedWorld.ForceRefresh();
					}
					else
					{
						int VoxelType = 0;
						System.Text.StringBuilder ImportVoxelDataBuilder = new System.Text.StringBuilder();
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
										//ImportVoxelDataBuilder.AppendLine(1 + " " + VoxelColor.r + " " + VoxelColor.g + " " + VoxelColor.b);
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
						ImportVoxelData = ImportVoxelDataBuilder.ToString();
						Debug.LogError(Data.GetSize().GetVector().ToString() + " - Imported voxel data:\n" + ImportVoxelData);
					}

					/*if (MyVoxelMainChunk.alphaMaskChunk != null)
					{
					Debug.Log("Checking Alpha from .Vox: " + MyVoxelMainChunk.alphaMaskChunk.sizeX + ", " + MyVoxelMainChunk.alphaMaskChunk.sizeY + ", " + MyVoxelMainChunk.alphaMaskChunk.sizeZ);
					for (int x = 0; x < MyVoxelMainChunk.alphaMaskChunk.sizeX; ++x)
					{
					for (int y = 0; y < MyVoxelMainChunk.alphaMaskChunk.sizeY; ++y)
					{
					for (int z = 0; z < MyVoxelMainChunk.alphaMaskChunk.sizeZ; ++z)
					{
					Debug.Log(MyVoxelMainChunk.alphaMaskChunk.voxels[x, y, z].ToString());
					}
					}
					}
					}
					else
					{
					Debug.Log("Alpha Mask is null");
					}*/
				}
				/*else
				{
				Debug.LogError("[World in Viewer] is null.");
				}**/
				// for our voxel data, set it to 
				}
				else
				{
					Debug.LogError("[MyVoxelMainChunk] is null.");
				}
			}
			else
			{
			Debug.LogError("[MVVoxModel] Invalid file path");
			}
			//yield break;
			#endif
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
					Mesh MyMesh = ObjImport.ImportFile(MyDialog.FileName);
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
			Mesh mesh = MyMeshFilter.mesh;

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
			System.Windows.Forms.OpenFileDialog MyDialog = new System.Windows.Forms.OpenFileDialog();
			System.Windows.Forms.DialogResult MyResult = MyDialog.ShowDialog();
			if (MyResult == System.Windows.Forms.DialogResult.OK)
			{
				byte[] bytes = FileUtil.LoadBytes(MyDialog.FileName);
				MyZexel.LoadImage(bytes);
			}
			else
			{
				Debug.LogError("Failure to open file.");
			}
			#endif
		}

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

		private System.Collections.IEnumerator ImportSoundRoutine(string FileName, Sound.Zound MyZound)
		{
			WWW MyWavLoader = new WWW("file://" + FileName);
			yield return MyWavLoader;
			LatestPlayed = MyWavLoader.GetAudioClip();
			MyZound.UseAudioClip(LatestPlayed);
		}
		#endregion




		public AudioClip LatestPlayed;

		public void PlayClip(AudioClip clip)
		{
			gameObject.GetComponent<AudioSource>().PlayOneShot(clip);
		} // PlayClip()

		public bool GetIsLoaded() 
		{
			return IsLoaded;
		}



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
		public ElementFolder GetElementFolder(string FolderName)
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
					if (FolderName == DataFolderNames.VoxelMeta)
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