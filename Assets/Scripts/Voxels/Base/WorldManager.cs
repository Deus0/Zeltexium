using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Zeltex;
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
        private SaveGame LoadedSaveGame;
        #endregion

        #region NormalStuff

        private void Update()
        {
            if (IsDebug)
            {
                if (Input.GetKeyDown(DebugKey))
                {
                    IsDebugGui = !IsDebugGui;
                }
            }
            HandleActions();
        }

        private void HandleActions()
        {
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
                    UniversalCoroutine.CoroutineManager.StartCoroutine(GetLoadedLevel().LoadLevel(WorldActions.ActionWorld, WorldActions.ActionPosition));
                }
                else
                {
                    UniversalCoroutine.CoroutineManager.StartCoroutine(GetLoadedLevel().LoadLevel(SpawnWorld(), WorldActions.ActionPosition));
                }
            }
            if (WorldActions.ActionSaveLevel.IsTriggered())
            {
                GetLoadedLevel().SaveLevel();
            }
        }

        private Level GetLoadedLevel()
        {
            return DataManager.Get().GetElement(DataFolderNames.Levels, LoadLevelName) as Level;
        }

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
            MyWorld.transform.SetParent(transform);
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

        public int GetSize()
        {
            return MyWorlds.Count;
        }

        public World GetWorld(string WorldName)
        {
            for (int i = 0; i < GetSize(); i++)
            {
                if (MyWorlds[i] && MyWorlds[i].name == WorldName)
                {
                    return MyWorlds[i];
                }
            }
            return null;
        }

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
                //if (MyCharacter)
                //{
                //    GUILayout.Label("Main Character: " + MyCharacter.name);
                //}
            }
        }
        //private Character MyCharacter;

        public static new WorldManager Get()
        {
            if (MyManager == null)
            {
                MyManager = GameObject.Find("Worlds").GetComponent<WorldManager>();
            }
            return MyManager;
        }

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
                    yield return MyLevel.LoadLevel(NewWorld, NewPosition);
                }
                // move character to level
                MainCharacter.transform.position = SpawnPositionFinder.FindClosestPositionInChunk(NewWorld, NewPosition);
                MainCharacter.SetWorld(NewWorld);
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
                LoadedLevel.SaveChunks(LoadedSaveGame.Name);
                // Next Any character changes - CharacterData - make sure to refresh their transforms
                LoadedLevel.SaveCharacters(LoadedSaveGame.Name);
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

    }
}
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
