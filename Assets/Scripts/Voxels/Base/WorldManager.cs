using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Zeltex;
using Zeltex.Util;
using Zeltex.Characters;
using Zeltex.Guis;
using Zeltex.Guis.Maker;

namespace Zeltex.Voxels
{ 
    /// <summary>
    /// Reads a file in a routine
    /// </summary>
    public class FileReaderRoutiner 
    {
        public string Result = "";
        private static float YieldRate = (20f / 1000f);
        private float LastYield;
        private float TimeSince;
        private string ThisLine = "";
        private System.Text.StringBuilder MyBuilder;
        private StreamReader MyStreamReader;
        //private bool IsReading = true;
        private string FilePath;

        public FileReaderRoutiner(string NewFilePath) 
        {
            MyBuilder = new System.Text.StringBuilder();
            LastYield = Time.realtimeSinceStartup;
            TimeSince = Time.realtimeSinceStartup - LastYield;
            ThisLine = "";
            FilePath = NewFilePath;
        }

        public IEnumerator Run() 
        {
            using (MyStreamReader = new StreamReader(FilePath))
            {
                while (MyStreamReader.Peek() > -1)
                {
                    ThisLine = MyStreamReader.ReadLine();
                    MyBuilder.AppendLine(ThisLine);
                    TimeSince = Time.realtimeSinceStartup - LastYield;
                    if (TimeSince >= YieldRate)
                    {
                        LastYield = Time.realtimeSinceStartup;
                        yield return null;
                    }
                }
            }
            Result = MyBuilder.ToString();
        }
    }

    [Serializable]
    public class WorldManagerActions
    {
        [SerializeField]
        public EditorAction ActionClearAll;
        [SerializeField]
        public EditorAction IsSpawn;
        [SerializeField]
        public EditorAction IsGenerate;
        [SerializeField]
        public EditorAction IsClear;
        [SerializeField]
        public EditorAction ActionLoadLevel = new EditorAction();
        [SerializeField]
        public EditorAction ActionSaveLevel = new EditorAction();
        [Header("Options")]
        [SerializeField]
        public Int3 ActionPosition;
        [SerializeField]
        public World ActionWorld;
    }
    /// <summary>
    /// Manages the worlds
    /// Spawns a world
    /// Level Maker Uses this when loading a world
    /// </summary>
    [ExecuteInEditMode]
    public class WorldManager : ManagerBase<WorldManager>
    {
        #region Variables++
        public static string ChunkFileExtention = "chn";
        public static string CharacterFileExtension = "chr";
        //public static string SaveGameName = "";

        [Header("Settings")]
        public Vector3 SpawnedWorldScale = 0.5f * (new Vector3(1, 1, 1));

        [Header("Actions")]
        public WorldManagerActions WorldActions;

        [Header("Debug")]
        [SerializeField]
        private bool IsDebug;
        [SerializeField]
        private KeyCode DebugKey = KeyCode.F3;
        private bool IsDebugGui = false;

        [Header("References")]
        public VoxelTerrain MyTerrainGenerator;
        public Transform WorldsParent;
        public VoxelManager MyVoxelManager;
        public WorldUpdater WorldUpdater;

        [Header("Data")]
        public Vector3 SpawnWorldScale = new Vector3(1, 1, 1);
        public List<World> MyWorlds = new List<World>();
        //private bool IsLoading;
        public bool IsDisableCharactersOnLoad;
        [SerializeField]
        public string LoadLevelName;
        [Header("Voxel Roam Settings")]
        public Int3 RoamPosition = Int3.Zero();
        public Int3 RoamSize = new Int3(4, 2, 4);

        private void Update()
        {
            if (IsDebug)
            {
                if (Input.GetKeyDown(DebugKey))
                {
                    IsDebugGui = !IsDebugGui;
                }
            }
            if (WorldActions.ActionClearAll.IsTriggered())
            {
                // if loading, make sure to cancel it first
                for (int i = 0; i < MyWorlds.Count; i++)
                {
                    if (MyWorlds[i])
                    {
                        MyWorlds[i].gameObject.Die();
                    }
                }
                MyWorlds.Clear();
            }
            if (WorldActions.IsSpawn.IsTriggered())
            {
                WorldActions.ActionWorld = SpawnWorld();
            }
            if (WorldActions.IsClear.IsTriggered())
            {
                if (WorldActions.ActionWorld)
                {
                    Clear();
                }
            }
            if (WorldActions.ActionLoadLevel.IsTriggered())
            {
                if (WorldActions.ActionWorld)
                {
                    UniversalCoroutine.CoroutineManager.StartCoroutine(LoadLevel(WorldActions.ActionWorld, GetLoadedLevel(), WorldActions.ActionPosition));
                }
                else
                {
                    UniversalCoroutine.CoroutineManager.StartCoroutine(LoadLevel(SpawnWorld(), GetLoadedLevel(), WorldActions.ActionPosition));
                }
            }
            if (WorldActions.ActionSaveLevel.IsTriggered())
            {
                SaveLevel(GetLoadedLevel());
            }
        }

        private Level GetLoadedLevel()
        {
            return DataManager.Get().GetElement(DataFolderNames.Levels, LoadLevelName) as Level;
        }
        #endregion

        #region Worlds

        public void Clear()
        {
            for (int i = 0; i < MyWorlds.Count;i++)
            {
                if (MyWorlds[i])
                {
                    MyWorlds[i].gameObject.Die();
                }
            }
            MyWorlds.Clear();
        }

        /// <summary>
        /// Spawns a new gameobject and converts it to a world
        /// </summary>
        public World SpawnWorld()
        {
            GameObject NewWorldObject = new GameObject();
            World NewWorld = ConvertToWorld(NewWorldObject);
            if (NewWorld == null)
            {
                Debug.LogError("World Manager failed to spawn new world.");
            }
            return NewWorld;
        }

        /// <summary>
        /// Used by a viewer to load a world
        /// </summary>
        public World ConvertToWorld(GameObject NewGameObject)
        {
            LayerManager.Get().SetLayerWorld(NewGameObject);
            World MyWorld = NewGameObject.AddComponent<World>();
            MyWorld.name = "World";
            if (WorldsParent == null)
            {
                WorldsParent = GameObject.Find("Worlds").transform;
            }
            MyWorld.transform.SetParent(WorldsParent);
            MyWorld.transform.localScale = SpawnWorldScale;
            MyWorld.MyVoxelTerrain = MyTerrainGenerator;
            MyWorld.VoxelScale = SpawnedWorldScale;
            MyWorlds.Add(MyWorld);
            return MyWorld;
        }

        /// <summary>
        /// Destroys a planet
        /// </summary>
        public void Remove(World DoomedWorld)
        {
            if (DoomedWorld)
            {
                if (MyWorlds.Contains(DoomedWorld) == true)
                {
                    MyWorlds.Remove(DoomedWorld);
                }
                DoomedWorld.gameObject.Die();
            }
        }
        #endregion

        #region LevelLoading

        public void LoadLevel(Level NewLevel)
        {
            UniversalCoroutine.CoroutineManager.StartCoroutine(LoadLevel(SpawnWorld(), NewLevel));
        }

        public IEnumerator LoadLevelWorldless(Level NewLevel, System.Action OnLoadChunk = null)
        {
            yield return (LoadLevel(SpawnWorld(), NewLevel, OnLoadChunk));
        }

        public IEnumerator LoadLevel(World MyWorld, Level NewLevel, System.Action OnLoadChunk = null)
        {
            yield return (LoadLevel(MyWorld, NewLevel, Int3.Zero(), OnLoadChunk));
        }

        /// <summary>
        /// Load the level ! Base function for loading level meta into a world!
        /// </summary>
        public IEnumerator LoadLevel(World MyWorld, Level NewLevel, Int3 PositionOffset, System.Action OnLoadChunk = null, SaveGame SavedGame = null)
        {
            float TimeBegunLoading = Time.realtimeSinceStartup;
            //IsLoading = true;
            if (PositionOffset == null)
            {
                Debug.LogError("PositionOffset is null inside LoadLevel function");
                PositionOffset = Int3.Zero();
            }
            if (MyWorld != null)
            {
                if (NewLevel != null)
                {
                    LevelHandler MyLevelHandler = MyWorld.gameObject.GetComponent<LevelHandler>();
                    if (MyLevelHandler == null)
                    {
                        MyLevelHandler = MyWorld.gameObject.AddComponent<LevelHandler>();
                    }
                    MyLevelHandler.MyLevel = NewLevel;
                    NewLevel.SetWorld(MyWorld);
                    MyWorld.name = NewLevel.Name;

                    // First load chunks
                    yield return MyWorld.SetWorldSizeRoutine(RoamSize, RoamPosition, null, true);
                    // Then Generate Terrain
                    if (NewLevel.GenerateTerrain())
                    {
                        yield return GetComponent<VoxelTerrain>().CreateTerrainWorldRoutine(MyWorld, false);
                    }
                    // Finally Load the chunks from edited data, as well as characters, zones, items etc
                    yield return LoadChunksInLevel(MyWorld, NewLevel, OnLoadChunk, SavedGame);
                }
                else
                {
                    Debug.LogError("Level is null inside LoadLevel function");
                    //yield return UniversalCoroutine.CoroutineManager.StartCoroutine(MyTerrainGenerator.CreateTerrainWorldRoutine(MyWorld));
                    //MyWorld.name = "Error";
                }
            }
            else
            {
                Debug.LogError("World is null inside LoadLevel function");
            }
            //IsLoading = false;
            //Debug.LogError("Time taken to load level: " + NewLevel.Name + " - " + (Time.realtimeSinceStartup - TimeBegunLoading));
        }

        //private List<string> CharacterFiles = new List<string>();
        //private List<string> FilesInFolder = new List<string>();
        private static string ChunkFileExtentionPlusDot = "." + ChunkFileExtention;
        //private static string CharacterFileExtensionPlusDot = "." + CharacterFileExtension;
        private IEnumerator LoadChunksInLevel(World MyWorld, Level NewLevel, System.Action OnLoadChunk = null, SaveGame SavedGame = null)
        {
            // load chunks
            string FolderPath = NewLevel.GetFolderPath();
            // Load Overwritten Character File Names
            Int3 ChunkPosition = Int3.Zero();
            for (ChunkPosition.x = -RoamSize.x + RoamPosition.x; ChunkPosition.x < RoamSize.x + RoamPosition.x; ChunkPosition.x++)
            {
                for (ChunkPosition.y = -RoamSize.y + RoamPosition.y; ChunkPosition.y < RoamSize.y + RoamPosition.y; ChunkPosition.y++)
                {
                    for (ChunkPosition.z = -RoamSize.z + RoamPosition.z; ChunkPosition.z < RoamSize.z + RoamPosition.z; ChunkPosition.z++)
                    {
                        yield return LoadChunk(NewLevel, SavedGame, ChunkPosition, FolderPath, MyWorld, OnLoadChunk);
                    }
                }
            }
            Chunk MyChunk;
            for (ChunkPosition.x = -RoamSize.x + RoamPosition.x; ChunkPosition.x < RoamSize.x + RoamPosition.x; ChunkPosition.x++)
            {
                for (ChunkPosition.y = -RoamSize.y + RoamPosition.y; ChunkPosition.y < RoamSize.y + RoamPosition.y; ChunkPosition.y++)
                {
                    for (ChunkPosition.z = -RoamSize.z + RoamPosition.z; ChunkPosition.z < RoamSize.z + RoamPosition.z; ChunkPosition.z++)
                    {
                        MyChunk = MyWorld.GetChunk(ChunkPosition);
                        if (MyChunk)
                        {
                            yield return MyChunk.BuildChunkMesh();
                            yield return MyChunk.ActivateCharacters();
                        }
                    }
                }
            }
            
            //Debug.Log("Loading Level from path: " + FolderPath + " - With files count of " + ChunkFiles.Count);
            //string ChunkData = "";
            //Int3 ChunkPosition = Int3.Zero();
            //Chunk MyChunk = null;
            /* string[] ChunkPositionStrings = null;
             if (ChunkFiles.Count > 0)
             {
                 for (int i = 0; i < ChunkFiles.Count; i++)
                 {
                     string FullFilePath = ChunkFiles[i];
                     string FileName = Path.GetFileName(FullFilePath);
                     //ChunkData = FileManagement.ReadAllLines(FileName, //FileUtil.Load(FileName);
                     //Debug.Log("Loading Chunk: " + FileName + "\n" + ChunkData);
                     ChunkPositionStrings = (FileName.Substring(0, FileName.Length - 4)).Split('_');
                     try
                     {
                         if (ChunkPositionStrings.Length >= 4)
                         {
                             ChunkPosition.Set(int.Parse(ChunkPositionStrings[1]), int.Parse(ChunkPositionStrings[2]), int.Parse(ChunkPositionStrings[3]));
                             //Debug.Log("Chunk [" + FileName + "] is at position: " + ChunkPosition.GetVector().ToString());
                             MyChunk = MyWorld.GetChunk(ChunkPosition);
                         }
                         else
                         {
                             Debug.LogError("MyInput.Length is " + ChunkPositionStrings.Length + " in 'LoadChunksInLevel'");
                         }
                     }
                     catch(System.FormatException e)
                     {
                         Debug.LogError("Problem reading chunks in level: FOrmatException in 'LoadChunksInLevel'\n" + e.ToString());
                     }
                 }
             }*/

            //yield return LoadCharactersFromFiles(NewLevel, CharacterFiles, OnLoadChunk);
        }

        /// <summary>
        /// Loads chunk and characters in it
        /// Also Loads zones, item Objects, and anything else stored in it
        /// </summary>
        private IEnumerator LoadChunk(Level NewLevel, SaveGame MySaveGame, Int3 ChunkPosition, string FolderPath, World MyWorld, System.Action OnLoadChunk) 
        {
            string InnerChunkFileName = "Chunks/" + "Chunk_" + ChunkPosition.x + "_" + ChunkPosition.y + "_" + ChunkPosition.z + ChunkFileExtentionPlusDot;
            string ChunkFileName = FolderPath + InnerChunkFileName;
            bool DoesContainChunkFile = FileManagement.FileExists(ChunkFileName, true, true);
            if (MySaveGame != null)
            {
                string ChunkFileNameSaveGame = DataManager.GetFolderPath(DataFolderNames.Saves + "/") + NewLevel.Name + "/" + InnerChunkFileName;
                bool DoesSaveGameFileExist = FileManagement.FileExists(ChunkFileNameSaveGame, true, true);
                if (DoesSaveGameFileExist)
                {
                    ChunkFileName = ChunkFileNameSaveGame;
                }
            }
            if (DoesContainChunkFile == false)
            {
                //Debug.LogError("Chunk at " + ChunkPosition.GetVector().ToString() + " Does not exist. " + ChunkFileName);
                yield break;
            }
            Chunk MyChunk = MyWorld.GetChunk(ChunkPosition);
            yield return LoadChunkFromFile(MyChunk, ChunkFileName);
            // Load Characters for this chunk here:
            yield return LoadCharactersFromFiles(NewLevel, CharactersInChunk, OnLoadChunk);
            if (OnLoadChunk != null) 
            {
                OnLoadChunk.Invoke();
            }
        }
        List<string> CharactersInChunk = new List<string>();

        /// <summary>
        /// Loads a chunk from a file
        /// </summary>
        private IEnumerator LoadChunkFromFile(Chunk MyChunk, string FullFilePath) 
        {
            if (MyChunk)
            {
                MyChunk.SetDefaultVoxelNames();
                //Debug.LogError("Loading ChunkName [" + FileName + "] - at " + MyChunk.name + " of size: " + MyScript.Count);
                //MyChunk.RunScript(FileUtil.ConvertToList(ChunkData))
                string MyScript;
                //MyStrings.AddRange(FileManagement.ReadAllLines(ChunkFiles[i], false, true, true));
                if (FullFilePath.Contains("://") || FullFilePath.Contains(":///"))
                {
                    WWW UrlRequest = new WWW(FullFilePath);
                    yield return (UrlRequest);  // UniversalCoroutine.CoroutineManager.StartCoroutine
                    //Scripts.Add(UrlRequest.text);
                    MyScript = (UrlRequest.text);
                }
                else
                {
                    // Read File Asynch
                    FileReaderRoutiner MyFileReader = new FileReaderRoutiner(FullFilePath);
                    yield return MyFileReader.Run();
                    MyScript = MyFileReader.Result as string;
                    //Debug.LogError("Read script asynchornously.\n " + MyScript);
                }
                CharactersInChunk.Clear();
                string[] MyScriptSplit = MyScript.Split('\n');
                // If Contains Characters on first line of chunk
                if (MyScriptSplit[0].Contains("/Characters"))
                {
                    // each line until /EndCharacters
                    for (int i = 1; i < MyScriptSplit.Length; i++)
                    {
                        // Until line is EndCharacters
                        if (MyScriptSplit[i].Contains("/EndCharacters"))
                        {
                            // Make sure to cut script
                            string[] NewScriptSplit = new string[MyScriptSplit.Length - i - 1];
                            Array.Copy(MyScriptSplit, i + 1, NewScriptSplit, 0, NewScriptSplit.Length);
                            MyScript = FileUtil.ConvertToSingle(NewScriptSplit);
                            break;
                        }
                        else
                        {
                            CharactersInChunk.Add(ScriptUtil.RemoveWhiteSpace(MyScriptSplit[i]));
                        }
                    }
                }
                yield return MyChunk.RunScript(MyScript, false);
                // Surrounding Chunks
                /*List<Chunk> SurroundingChunks = MyChunk.GetSurroundingChunks();
                for (int i = 0; i < SurroundingChunks.Count; i++)
                {
                    SurroundingChunks[i].WasUpdated();
                    SurroundingChunks[i].SetAllUpdatedSides();
                    yield return SurroundingChunks[i].BuildChunkMesh();
                }*/
            }
            else
            {
                Debug.LogError("While loading world: " + MyChunk.GetWorld().name + " - Could not find chunk");
            }
        }

        /// <summary>
        /// Converts level data paths into overwritten save game paths
        /// </summary>
        /*private void LoadSaveGamePaths(World MyWorld, Level NewLevel, SaveGame SavedGame, System.Action OnLoadChunk = null)
        {
            string LevelFolderPath = NewLevel.GetFolderPath();
            if (SavedGame != null)
            {
                string SaveGameFolderPath = DataManager.GetFolderPath(DataFolderNames.Saves + "/") + SavedGame.Name + "/" + NewLevel.Name + "/";
                if (FileManagement.DirectoryExists(SaveGameFolderPath))
                {
                    for (int i = 0; i < FilesInFolder.Count; i++)
                    {
                        string FileName = Path.GetFileName(FilesInFolder[i]);
                        string SaveGameFileName = SaveGameFolderPath + FileName;
                        bool DoesSaveExist = FileManagement.FileExists(SaveGameFileName, true, true);
                        //Debug.Log("Checking for file: " + FileName + " - " + SaveGameFileName + " --- " + DoesSaveExist);
                        if (DoesSaveExist)
                        {
                            FilesInFolder[i] = SaveGameFileName;
                        }
                        else
                        {
                            FilesInFolder[i] = LevelFolderPath + FilesInFolder[i];
                        }
                    }
                    string[] FilesInSaveFolder = FileManagement.ListFiles(SaveGameFolderPath, true, true);
                    List<string> NewFolderFiles = new List<string>();
                    for (int j = 0; j < FilesInSaveFolder.Length; j++)
                    {
                        if (FilesInSaveFolder[j].Contains(ChunkFileExtention) || FilesInSaveFolder[j].Contains(CharacterFileExtension)
                            && FilesInSaveFolder[j].Contains("meta") == false)
                        {
                            bool DoesInNormalFiles = false;
                            string FileName = Path.GetFileName(FilesInSaveFolder[j]);
                            for (int i = 0; i < FilesInFolder.Count; i++)
                            {
                                if (FilesInFolder[i].Contains(ChunkFileExtention) || FilesInFolder[i].Contains(CharacterFileExtension)
                                    && FilesInFolder[j].Contains("meta") == false)
                                {
                                    string FileName2 = Path.GetFileName(FilesInFolder[i]);
                                    if (FileName == FileName2)
                                    {
                                        DoesInNormalFiles = true;
                                        break;
                                    }
                                }
                            }
                            if (DoesInNormalFiles == false)
                            { 
                                NewFolderFiles.Add(SaveGameFolderPath + FilesInSaveFolder[j]);
                                //Debug.Log("Adding NEW Save Folder File " + NewFolderFiles[NewFolderFiles.Count - 1]);
                            }
                        }
                    }
                    FilesInFolder.AddRange(NewFolderFiles);
                    // if any new character or chunk files
                    return;
                }
            }
            // If no save game or save game path doesn't exist
            for (int i = 0; i < FilesInFolder.Count; i++)
            {
                FilesInFolder[i] = LevelFolderPath + FilesInFolder[i];
            }
        }*/

        private IEnumerator LoadCharactersFromFiles(Level NewLevel, List<string> CharacterFiles, Action OnLoadChunk = null)
        {
            for (int i = 0; i < CharacterFiles.Count; i++)
            {
                Character MyCharacter = CharacterManager.Get().GetPoolObject();
                if (MyCharacter)
                {
                    MyCharacter.name = CharacterFiles[i];
                    // Set full file path
                    CharacterFiles[i] = NewLevel.GetFilePath(MyCharacter);
                    if (FileManagement.FileExists(CharacterFiles[i], true, true))
                    {
                        // First get script
                        string CharacterScript = "";
                        if (CharacterFiles[i].Contains("://") || CharacterFiles[i].Contains(":///"))
                        {
                            WWW UrlRequest = new WWW(CharacterFiles[i]);
                            yield return (UrlRequest);
                            CharacterScript = UrlRequest.text;
                        }
                        else
                        {
                            FileReaderRoutiner MyFileReader = new FileReaderRoutiner(CharacterFiles[i]);
                            yield return MyFileReader.Run();
                            CharacterScript = MyFileReader.Result as string;
                            //CharacterFile = File.ReadAllText(CharacterFiles[i]);
                        }
                        if (CharacterScript != null)
                        {
                            CharacterData NewData = Newtonsoft.Json.JsonConvert.DeserializeObject(CharacterScript, typeof(CharacterData)) as CharacterData;
                            if (NewData != null)
                            {
                                yield return MyCharacter.SetDataRoutine(NewData, NewLevel, false, false, false);
                                NewData.LoadPath = CharacterFiles[i];
                                if (OnLoadChunk != null)
                                {
                                    OnLoadChunk.Invoke();
                                }
                            }
                        }
                        else
                        {
                            Debug.LogError(CharacterFiles[i] + " has null string loaded.");
                        }
                    }
                    else
                    {
                        Debug.LogError(CharacterFiles[i] + " is not an existing file!");
                    }
                }
                else
                {
                    Debug.LogError("Character Pool was empty in WorldManager:LoadChunksInLevel");
                    break;
                }
            }
        }
        
        public void SaveLevel(Level MyLevel)
        {
            if (MyLevel != null)
            {
                string FolderPath = MyLevel.GetFolderPath();
                World LevelWorld = MyLevel.GetWorld();
                if (LevelWorld == null)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        if (transform.GetChild(i).name == MyLevel.Name)
                        {
                            World MyWorld = transform.GetChild(i).GetComponent<World>();
                            if (MyWorld)
                            {
                                MyLevel.SetWorld(transform.GetChild(i).GetComponent<World>());
                                LevelWorld = MyLevel.GetWorld();
                                break;
                            }
                        }
                    }
                }
                if (LevelWorld)
                {
                    LevelWorld.GatherChunks();
                    string BetweenChar = "_";
                    if (LevelWorld)
                    {
                        foreach (KeyValuePair<Int3, Chunk> MyValueKeyPair in LevelWorld.MyChunkData)
                        {
                            Chunk MyChunk = MyValueKeyPair.Value;
                            if (MyChunk)
                            {
                                string FilePath = FolderPath + "Chunk" +
                                    BetweenChar + MyChunk.Position.x.ToString() +
                                    BetweenChar + MyChunk.Position.y.ToString() +
                                    BetweenChar + MyChunk.Position.z.ToString() +
                                    "." + ChunkFileExtention;
                                string ChunkData = MyChunk.GetSerial();
                                Debug.Log("Saving chunk to: " + FilePath);
                                FileUtil.Save(FilePath, ChunkData);
                            }
                        }
                    }
                    //UniversalCoroutine.CoroutineManager.StartCoroutine(SaveGame(SpawnWorld(), NewLevel));
                }
                else
                {
                    Debug.LogError("Could not save level " + MyLevel.Name + "'s World.");
                }
            }
            else
            {
                Debug.LogError("Level is null. Cannot save.");
            }
        }
        #endregion

        private void OnGUI()
        {
            if (IsDebugGui)
            {
                if (GUILayout.Button("ClearAll"))
                {
                    Clear();
                    CharacterManager.Get().ClearAllButMainCharacter();
                }
                List<string> LevelNames = DataManager.Get().GetNames(DataFolderNames.Levels);
                for (int i = 0; i < LevelNames.Count; i++)
                {
                    if (GUILayout.Button(LevelNames[i]))
                    {
                        GoToLevel(DataManager.Get().GetElement(DataFolderNames.Levels, LevelNames[i]) as Level);
                    }
                }
                if (MyCharacter)
                {
                    GUILayout.Label("Main Character: " + MyCharacter.name);
                }
            }
        }
        private Character MyCharacter;

        public static new WorldManager Get()
        {
            if (MyManager == null)
            {
                MyManager = GameObject.Find("Worlds").GetComponent<WorldManager>();
            }
            return MyManager;
        }

        #region LoadSaveGames

        public void LoadSaveGame(Action OnLoadChunk = null, SaveGame MyGame = null)//Level MyLevel, string CharacterScript, string StartingLocation = "")
        {
            UniversalCoroutine.CoroutineManager.StartCoroutine(LoadSaveGameRoutine(MyGame, OnLoadChunk));// MyLevel, CharacterScript));
        }

        /// <summary>
        /// Used by SaveGameMaker to load a level with a character script
        /// </summary>
        public IEnumerator LoadSaveGameRoutine(SaveGame MyGame = null, Action OnLoadChunk = null)//Level MyLevel, string CharacterScript, string StartingLocation = "")
        {
            LoadedSaveGame = MyGame;
            World SpawnedWorld = SpawnWorld();

            // Next Load the level
            if (MyCharacter != null)
            {
                yield return LoadLevel(SpawnedWorld, MyGame.GetLevel(), MyCharacter.GetChunkPosition());
            }
            else
            {
                yield return LoadLevel(SpawnedWorld, MyGame.GetLevel(), Int3.Zero(), OnLoadChunk, MyGame);
            }

            // Creates a new character
            if (MyGame.CharacterName == "")
            {
                yield return (CreateMainCharacter(MyGame, OnLoadChunk));
            }
            else
            {
                bool WasFound = false;
                Character LevelCharacter = null;
                // Set character to levels loaded character
                for (int i = 0; i < MyGame.GetLevel().GetRealCharactersCount(); i++)
                {
                    LevelCharacter = MyGame.GetLevel().GetCharacter(i);
                    if (LevelCharacter && LevelCharacter.GetData().Name == MyGame.CharacterName)
                    {
                        MyGame.SetCharacter(LevelCharacter);
                        WasFound = true;
                        break;
                    }
                }
                if (WasFound == false)
                {
                    Debug.Log("Could not find Main Character in Level: " + MyGame.CharacterName);
                    yield return (CreateMainCharacter(MyGame, OnLoadChunk));
                }
                else
                {
                    Debug.Log("Set Save Game Character to: " + MyGame.MyCharacter.name);
                }
            }
        }

        /// <summary>
        /// Creates the main character
        /// Currently uses 0 - which happens to be Alzo
        /// </summary>
        private IEnumerator CreateMainCharacter(SaveGame MyGame = null, Action OnLoadChunk = null)
        {
            // then load bot with script
            Character MyCharacter = CharacterManager.Get().GetPoolObject();
            if (MyCharacter != null)
            {
                // GetClass Script
                CharacterData Data = DataManager.Get().GetElement(DataFolderNames.Characters, 0) as CharacterData;
                MyCharacter.transform.position = MyGame.GetLevel().GetSpawnPoint();
                yield return (MyCharacter.SetDataRoutine(Data, MyGame.GetLevel()));
                if (OnLoadChunk != null)
                {
                    OnLoadChunk.Invoke();
                }
                MyGame.GetLevel().AddCharacter(MyCharacter);
                MyGame.SetCharacter(MyCharacter);
            }
            else
            {
                Debug.LogError("=====-=-----------==========------");
                Debug.LogError("Character Pooled Object is null inside LoadNewSaveGame function");
                Debug.LogError("=====-=-----------==========------");
            }
        }
        #endregion


        #region Character

        /// <summary>
        /// Moves the main character to a new level
        /// </summary>
        public void GoToLevel(Level MyLevel)
        {
            StopAllCoroutines();
            // Get Main Character
            Character MainCharacter = Camera.main.gameObject.GetComponent<Player>().GetCharacter();
            // Spawn second level
            StartCoroutine(MoveCharacterToLevel(MainCharacter, MyLevel));
        }

        /// <summary>
        /// Moves the character to a new level
        /// </summary>
        public IEnumerator MoveCharacterToLevel(Character MainCharacter, Level MyLevel)
        {
            // fade out and begin loading
            Int3 NewPosition = Int3.Zero();
            if (MyLevel.Infinite())
            {
                // chose an area in the level
                NewPosition = new Int3((int)UnityEngine.Random.Range(-3000, 3000), 8, (int)UnityEngine.Random.Range(-3000, 3000));
                // when world spawns at this point, the position finder will find closest point in chunk

            }
            else
            {

            }
            World WorldInsideOf = MainCharacter.GetInWorld();
            if (WorldInsideOf.name != MyLevel.Name)
            {
                // Load the level
                World NewWorld = null;
                for (int i = 0; i < MyWorlds.Count; i++)
                {
                    if (MyWorlds[i].name == MyLevel.Name)
                    {
                        NewWorld = MyWorlds[i];
                        break;
                    }
                }
                if (NewWorld == null)
                {
                    NewWorld = SpawnWorld();
                    NewWorld.transform.position = new Vector3(0, (MyWorlds.Count - 1) * 1000, 0);  // move up a level!
                    yield return LoadLevel(NewWorld, MyLevel, NewPosition);
                }
                // move character to level
                MainCharacter.transform.position = SpawnPositionFinder.FindClosestPositionInChunk(NewWorld, NewPosition);
                MainCharacter.SetWorld(NewWorld);
                MyCharacter = MainCharacter;
            }
        }

        /// <summary>
        /// Spawns a player and uses the main camera to possess it
        /// </summary>
        public static Character SpawnPlayer(Vector3 SpawnPosition, World InWorld)
        {
            Character MyPlayer = CharacterManager.Get().GetPoolObject();

            if (MyPlayer)
            {
                Billboard.IsLookAtMainCamera = true;
                MyPlayer.transform.position = SpawnPosition;
                // Possess player
                Possess.PossessCharacter(MyPlayer.GetComponent<Character>());
                // set character to be on this layer
                // Hide levelmaker!
            }
            return MyPlayer;
        }

        private SaveGame LoadedSaveGame;
        /// <summary>
        /// Saves the current game
        /// </summary>
        public void SaveGame(Character MyCharacter)
        {
            if (LoadedSaveGame != null)
            {
                // Save the loaded save game
                Level LoadedLevel = LoadedSaveGame.GetLevel();
                // First chunk Changes
                LoadedLevel.SaveOpenChunks(LoadedSaveGame.Name);
                // Next Any character changes - CharacterData - make sure to refresh their transforms
                LoadedLevel.SaveOpenCharacters(LoadedSaveGame.Name);
            }
            else
            {
                Debug.LogError("No LoadedSaveGame in WorldManager");
            }
            if (MyWorlds.Count > 0)
            {
                World WorldToSave = MyWorlds[MyWorlds.Count - 1];   // should be a character->GetWorldIn function
                //string LevelName = WorldToSave.name;
                // string LevelScript = "/Level " + LevelName + "\n";
                //LevelScript += FileUtil.ConvertToSingle(MyCharacter.GetScript());
                // string FilePath = DataManager.GetFolderPath(DataFolderNames.Saves + "/") + SaveGameName + "/" + SavesMaker.DefaultLevelName;
                //Debug.LogError("Saving [" + SaveGameName + "] with character [" + MyCharacter.name + "] to [" + FilePath + "]:\n" + LevelScript);
                //FileUtil.Save(FilePath, LevelScript);
            }
            else
            {
                Debug.LogError("No worlds to save.");
            }
        }

        #endregion

        /*private void OnGUI()
        {
            if (IsLoading)
            {
                GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginArea(new Rect((Screen.width / 2) - 50, (Screen.height / 2), 100, 100));
                GUILayout.Label("Loading world");
                GUILayout.EndArea();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndArea();
            }
        }*/

    }
}