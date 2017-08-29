using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Util;
using System.IO;
using Zeltex;
using Zeltex.Characters;

namespace Zeltex.Voxels
{ 
    /// <summary>
    /// Manages the worlds
    /// Spawns a world
    /// Level Maker Uses this when loading a world
    /// </summary>
    [ExecuteInEditMode]
    public partial class WorldManager : ManagerBase<WorldManager>
    {
        #region Variables++
        public static string ChunkFileExtention = "chn";
        public static string CharacterFileExtension = "chr";
        public static string SaveGameName = "";

        [Header("Actions")]
        [SerializeField]
        private EditorAction IsSpawn;
        [SerializeField]
        private EditorAction IsGenerate;
        [SerializeField]
        private EditorAction IsClear;
        [SerializeField]
        private EditorAction ActionLoadLevel;
        [SerializeField]
        private EditorAction ActionSaveLevel;
        [SerializeField]
        private Int3 ActionPosition;
        [SerializeField]
        private World ActionWorld;
        [SerializeField]
        private string LoadLevelName;

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
        private bool IsLoading;
        public bool IsDisableCharactersOnLoad;

        private void Update()
        {
            if (IsDebug)
            {
                if (Input.GetKeyDown(DebugKey))
                {
                    IsDebugGui = !IsDebugGui;
                }
            }
            if (IsSpawn.IsTriggered())
            {
                ActionWorld = SpawnWorld();
            }
            if (IsClear.IsTriggered())
            {
                if (ActionWorld)
                {
                    MonoBehaviourExtension.Kill(ActionWorld.gameObject);
                }
            }
            if (ActionLoadLevel.IsTriggered())
            {
                if (ActionWorld)
                {
                    UniversalCoroutine.CoroutineManager.StartCoroutine(LoadLevel(ActionWorld, GetLoadedLevel(), ActionPosition));
                }
                else
                {
                    UniversalCoroutine.CoroutineManager.StartCoroutine(LoadLevel(SpawnWorld(), GetLoadedLevel(), ActionPosition));
                }
            }
            if (ActionSaveLevel.IsTriggered())
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
            for (int i =0; i < MyWorlds.Count;i++)
            {
                if (MyWorlds[i])
                {
                    Destroy(MyWorlds[i].gameObject);
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
            MyWorld.MyUpdater = WorldUpdater;
            MyWorld.MyDataBase = MyVoxelManager;
            MyWorld.MyMaterials = MyVoxelManager.MyMaterials;
            MyWorld.MyVoxelTerrain = MyTerrainGenerator;
            MyWorlds.Add(MyWorld);
            return MyWorld;
        }

        /// <summary>
        /// Destroys a planet
        /// </summary>
        public void Destroy(World DoomedWorld)
        {
            if (DoomedWorld)
            {
                if (MyWorlds.Contains(DoomedWorld) == true)
                {
                    MyWorlds.Remove(DoomedWorld);
                }
                Destroy(DoomedWorld.gameObject);
            }
        }
        #endregion

        #region LevelLoading

        public void LoadLevel(Level NewLevel)
        {
            UniversalCoroutine.CoroutineManager.StartCoroutine(LoadLevel(SpawnWorld(), NewLevel));
        }

        public IEnumerator LoadLevelWorldless(Level NewLevel)
        {
            yield return UniversalCoroutine.CoroutineManager.StartCoroutine(LoadLevel(SpawnWorld(), NewLevel));
        }

        public IEnumerator LoadLevel(World MyWorld, Level NewLevel)
        {
            yield return UniversalCoroutine.CoroutineManager.StartCoroutine(LoadLevel(MyWorld, NewLevel, Int3.Zero()));
        }

        /// <summary>
        /// Load the level ! Base function for loading level meta into a world!
        /// </summary>
        public IEnumerator LoadLevel(World MyWorld, Level NewLevel, Int3 PositionOffset)
        {
            IsLoading = true;
            if (NewLevel != null)
            {
                Debug.Log("Loading Level: " + NewLevel.Name);
                yield return UniversalCoroutine.CoroutineManager.StartCoroutine(MyWorld.LoadLevelRoutine(NewLevel, PositionOffset));
                MyWorld.name = NewLevel.Name;
                yield return UniversalCoroutine.CoroutineManager.StartCoroutine(LoadChunksInLevel(MyWorld, NewLevel));
                NewLevel.SetWorld(MyWorld);
            }
            else
            {
                Debug.LogError("Level is null, loading random world.");
                //yield return UniversalCoroutine.CoroutineManager.StartCoroutine(MyTerrainGenerator.CreateTerrainWorldRoutine(MyWorld));
                //MyWorld.name = "Error";
            }
            IsLoading = false;
        }

        private IEnumerator LoadChunksInLevel(World MyWorld, Level NewLevel)
        {
            // load chunks
            string FolderPath = NewLevel.GetFolderPath();
            List<string> MyFiles = FileUtil.GetFilesOfType(FolderPath, ChunkFileExtention);
            if (MyFiles.Count == 0)
            {
                MyFiles = FileUtil.GetFilesOfType(FolderPath, "");
            }
            Debug.Log("Loading Level from path: " + FolderPath + " - With files count of " + MyFiles.Count);
            if (MyFiles.Count > 0)
            {
                for (int i = 0; i < MyFiles.Count; i++)
                {
                    string FileName = MyFiles[i];
                    //Debug.LogError("Loading Chunk: " + FileName);
                    List<string> MyScript = FileUtil.ConvertToList(FileUtil.Load(FileName));
                    FileName = Path.GetFileName(FileName);
                    string[] MyInput = (FileName.Substring(0, FileName.Length - 4)).Split('_');
                    int ChunkPositionX = int.Parse(MyInput[1]);
                    int ChunkPositionY = int.Parse(MyInput[2]);
                    int ChunkPositionZ = int.Parse(MyInput[3]);
                    Chunk MyChunk = MyWorld.GetChunk(new Int3(ChunkPositionX, ChunkPositionY, ChunkPositionZ));
                    if (MyChunk)
                    {
                        MyChunk.SetDefaultVoxelNames();
                        Debug.LogError("Loading ChunkName [" + FileName + "] - at " + MyChunk.name + " of size: " + MyScript.Count);
                        yield return UniversalCoroutine.CoroutineManager.StartCoroutine(MyChunk.RunScript(MyScript));
                    }
                    else
                    {
                        Debug.LogError("Could not find chunk: " + ChunkPositionX + ":" + ChunkPositionY + ":" + ChunkPositionZ);
                    }
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

        /// <summary>
        /// Loads all the characters in the level - side characters
        /// </summary>
        private IEnumerator LoadCharactersInLevel(World MyWorld, string MyFolderPath)
        {
            List<string> MyCharacterFiles = FileUtil.GetFilesOfType(MyFolderPath, CharacterFileExtension);
            //Debug.LogError("Inside Level [" + LevelName + "] with characters count of: " + MyCharacterFiles.Count);
            for (int i = 0; i < MyCharacterFiles.Count; i++)
            {
                string FileName = MyCharacterFiles[i];
                string MyFile = FileUtil.Load(FileName);
                bool ActiveState = gameObject.activeSelf;

                List<string> MyScript = FileUtil.ConvertToList(MyFile);
                FileName = Path.GetFileName(FileName);
                int ChunkIndex = FileName.IndexOf("Chunk_");
                int IndexOfName = "Character_".Length;
                int IndexNameEnds = ChunkIndex - 1;
                string CharacterName = FileName.Substring(IndexOfName, IndexNameEnds - IndexOfName);
                FileName = FileName.Substring(ChunkIndex, FileName.Length - 4 - ChunkIndex);
                string[] MyInput = FileName.Split('_');
                int ChunkPositionX = int.Parse(MyInput[1]);
                int ChunkPositionY = int.Parse(MyInput[2]);
                int ChunkPositionZ = int.Parse(MyInput[3]);
                //Debug.Log("Loading Character in chunk position: " + ChunkPositionX + ":" + ChunkPositionY + ":" + ChunkPositionZ);
                Int3 ChunkPosition = new Int3(ChunkPositionX, ChunkPositionY, ChunkPositionZ);
                //Debug.LogError("Loading Character from world: " + MyWorld.name + " at " + MyWorld.transform.position.ToString());
                Chunk MyChunk = MyWorld.GetChunk(ChunkPosition);
                if (MyChunk != null)
                {
                    Character MyCharacter = CharacterManager.Get().GetPoolObject();
                    if (MyCharacter)
                    {
                        MyCharacter.InWorld = MyWorld;
                        float TimeBegun = Time.realtimeSinceStartup;
                        yield return UniversalCoroutine.CoroutineManager.StartCoroutine(MyCharacter.RunScriptRoutine(MyScript));
                        if (IsDisableCharactersOnLoad)
                        {
                            //MyCharacter.SetMovement(false);
                        }
                        //Debug.Log("Loaded character - Taken: " + (Time.realtimeSinceStartup - TimeBegun));
                        MyCharacter.name = CharacterName;
                        if (MyChunk.WasUpdated())
                        {
                            //MyCharacter.SetMovement(false);
                            MyChunk.MyCharacterSpawns.Add(MyCharacter);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Chunk Null at: " + ChunkPosition.GetVector().ToString());
                }
                yield return null;
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