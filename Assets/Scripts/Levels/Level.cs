using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Zeltex.Voxels;
using Newtonsoft.Json;
using Zeltex.Characters;
using Zeltex.Util;

namespace Zeltex
{
    /// <summary>
    /// Each level contains one of these
    ///     -> Lights, positions, directions
    ///     -> Environment settings: Fog, Colour of background
    ///     -> Music to play
    ///     -> World settings - endless, generate settings etc
    /// </summary>
    [Serializable]
    public class Level : ElementCore
    {
        // The initial script for debugging
        [SerializeField, JsonProperty]
        private Int3 MyWorldSize = new Int3(0, 0, 0);
        [SerializeField, JsonProperty]
        private bool IsInfinite;
        [SerializeField, JsonProperty]
        private bool IsGenerateTerrain;
        [SerializeField, JsonProperty]
        private Vector3 SpawnPoint;
        [SerializeField, JsonIgnore]
        private World MyWorld;
        // Characters linked to a level
        [SerializeField, JsonIgnore]
        protected List<Character> MyCharacters = new List<Character>();
        [SerializeField, JsonIgnore]
        public List<Zone> Zones = new List<Zone>();
        [SerializeField, JsonIgnore]
        private FilePathType MyFilePathType = FilePathType.StreamingPath;
        [JsonIgnore]
        public static string ChunkFileExtention = "chn";
        [JsonIgnore]
        public static string CharacterFileExtension = "chr";
        [JsonIgnore]
        private static string ChunkFileExtentionPlusDot = "." + ChunkFileExtention;
        [JsonIgnore]
        public static string CharacterFileExtensionPlusDot = "." + CharacterFileExtension;
        [JsonIgnore]
        private List<string> CharactersInChunk = new List<string>();    // used when loading character files
        [JsonIgnore]
        private List<string> ZonesInChunk = new List<string>();
        [JsonIgnore]
        public int CharactersCount;
        [JsonIgnore]
        public List<string> CharacterNames = new List<string>();
        [JsonIgnore]
        public List<Character> Characters = new List<Character>();

        #region Overrides
        public bool CanSpawnCharacterInEditor(int CharacterIndex)
        {
            return Characters[CharacterIndex] == null;
        }

        public void SpawnCharacterInEditor(int CharacterIndex)
        {
            string CharacterFilePath = GetCharacterFilePath(CharacterNames[CharacterIndex]);
            string Script = FileUtil.Load(CharacterFilePath);
            CharacterData Data = Element.Load<CharacterData>(CharacterNames[CharacterIndex], Script);
            Characters[CharacterIndex] = (Data.Spawn(this));
        }
        public void DespawnCharacterInEditor(int CharacterIndex)
        {
            if (Characters[CharacterIndex])
            {
                Characters[CharacterIndex].gameObject.Die();
            }
        }

        public override void OnLoad()
        {
            base.OnLoad();
            // Also load the total number of characters in folder
            string CharacterFolderPath = GetFolderPathExtra("Characters");
#if UNITY_EDITOR
            if (FileManagement.DirectoryExists(CharacterFolderPath))
            {
                CharacterNames.Clear();
                Characters.Clear();
                CharacterNames.AddRange(FileManagement.ListFiles(CharacterFolderPath, new string[] { ".chr" }, true, true));
                for (int i = CharacterNames.Count - 1; i >= 0; i--)
                {
                    CharacterNames[i] = Path.GetFileNameWithoutExtension(CharacterNames[i]);
                    Characters.Add(null);
                }
                CharactersCount = CharacterNames.Count;
            }
#endif
            string ChunkFolderPath = GetFolderPathExtra("Chunks");
            string ZoneFolderPath = GetFolderPathExtra("Zones");
            // Create any folder paths that don't exist
            if (FileManagement.DirectoryExists(CharacterFolderPath) == false)
            {
                FileManagement.CreateDirectory(CharacterFolderPath, true);
            }
            if (FileManagement.DirectoryExists(ChunkFolderPath) == false)
            {
                FileManagement.CreateDirectory(ChunkFolderPath, true);
            }
            if (FileManagement.DirectoryExists(ZoneFolderPath) == false)
            {
                FileManagement.CreateDirectory(ZoneFolderPath, true);
            }
        }

        public Vector3 GetSpawnPoint()
        {
            return SpawnPoint;
        }

        public LevelHandler GetLevelHandler()
        {
            if (MyWorld)
            {
                return MyWorld.gameObject.GetComponent<LevelHandler>();
            }
            return null;
        }

        private void SetDefaults()
        {
            MyWorldSize = new Int3(7, 2, 7);
            IsInfinite = false;
            IsGenerateTerrain = false;
        }

        #endregion

        #region Data

        public void SetWorldSize(Vector3 NewSize)
        {
            SetWorldSize(new Int3(NewSize));
        }

        /// <summary>
        /// Sets the new world size
        /// </summary>
        public void SetWorldSize(Int3 NewSize)
        {
            if (MyWorldSize != NewSize)
            {
                MyWorldSize = NewSize;
                OnModified();
            }
        }

        /// <summary>
        /// Sets world as infinite
        /// </summary>
        public void SetWorldAsInfinite(bool NewInfiniteState)
        {
            if (IsInfinite != NewInfiniteState)
            {
                IsInfinite = NewInfiniteState;
                OnModified();
            }
        }

        /// <summary>
        /// Sets world to generate terrain
        /// </summary>
        public void SetIsGenerateTerrain(bool NewState)
        {
            if (IsGenerateTerrain != NewState)
            {
                IsGenerateTerrain = NewState;
                OnModified();
            }
        }
        #endregion

        #region Getters

        public bool GenerateTerrain()
        {
            return IsGenerateTerrain;
        }

        /// <summary>
        /// Blarg
        /// </summary>
        public Int3 GetWorldSize()
        {
            return MyWorldSize;
        }

        public bool Infinite()
        {
            return IsInfinite;
        }
        #endregion

        #region Spawning
        public bool IsSpawning;

        public override void Spawn()
        {
            if (!IsSpawning)
            {
                Debug.Log(Name + " Is Spawning in game.");
                RoutineManager.Get().StartCoroutine(SpawnRoutine());
            }
        }

        public IEnumerator SpawnRoutine()
        {
            IsSpawning = true;
            yield return LoadLevelWorldless();
            IsSpawning = false;
        }

        public override void DeSpawn()
        {
            ZoneManager.Get().Clear();
            Guis.Characters.CharacterGuiManager.Get().ReturnAllObjects();
            for (int i = 0; i < MyCharacters.Count; i++)
            {
                CharacterManager.Get().ReturnObject(MyCharacters[i]);
            }
            WorldManager.Get().Remove(MyWorld);
        }

        public override bool HasSpawned()
        {
            return (MyWorld != null);
        }

        public void SetWorld(World NewWorld)
        {
            MyWorld = NewWorld;
        }

        public World GetWorld()
        {
            if (MyWorld == null)
            {
                MyWorld = WorldManager.Get().GetWorld(Name);
            }
            return MyWorld;
        }

        public int GetCharactersCount()
        {
            return CharactersCount;
        }

        public void AddCharacter(Character NewCharacter)
        {
            if (MyCharacters.Contains(NewCharacter) == false)
            {
                MyCharacters.Add(NewCharacter);
                CharactersCount = MyCharacters.Count;
            }
        }

        public int GetRealCharactersCount()
        {
            return MyCharacters.Count;
        }

        public Character GetCharacter(int CharacterIndex)
        {
            return MyCharacters[CharacterIndex];
        }
        #endregion

        #region Zones

        public void SaveZones(string SaveGameName = "", bool IsForceSaveAll = false)
        {
            for (int i = 0; i < Zones.Count; i++)
            {
                SaveZone(Zones[i], SaveGameName, IsForceSaveAll);
            }
        }

        public void SaveZone(Zone MyZone, string SaveGameName = "", bool IsForceSaveAll = false)
        {
            if (MyZone)
            {
                ZoneData MyData = MyZone.GetData();
                MyData.OnPreSave();
                if (MyData.CanSave() || IsForceSaveAll)
                {
                    string FilePath = GetDataFilePath(MyData.Name, "Zones", SaveGameName);
                    MyData.MoveOnSave();
                    string SerializedData = MyData.GetSerial();
                    Debug.Log("Saving Character to Path: " + FilePath);
                    Util.FileUtil.Save(FilePath, SerializedData);
                    MyData.OnSaved();
                }
                else
                {
                    Debug.Log("Cannot save character as it has not been modified: " + MyZone.name);
                }
            }
        }
        #endregion

        #region Saving

        public void SaveCharacters(string SaveFolderName = "", bool IsForceSaveAll = false)
        {
            for (int i = 0; i < MyCharacters.Count; i++)
            {
                MyCharacters[i].GetData().SetCharacter(MyCharacters[i], false);
                SaveCharacter(MyCharacters[i], SaveFolderName, IsForceSaveAll);
            }
        }

        public void SaveCharacter(Character MyCharacter, string SaveFolderName = "", bool IsForceSaveAll = false)
        {
            if (MyCharacter)
            {
                CharacterData MyData = MyCharacter.GetData();
                MyData.SetCharacter(MyCharacter, false);    // just incase doesn't get set earlier
                MyData.RefreshTransform();
                if (MyData.CanSave() || IsForceSaveAll)
                {
                    string CharacterPath = GetFilePath(MyCharacter, SaveFolderName);
                    if (CharacterPath != MyData.LoadPath)
                    {
                        FileManagement.DeleteFile(MyData.LoadPath, true);
                    }
                    string SerializedCharacterData = MyData.GetSerial();
                    Debug.Log("Saving Character to Path: " + CharacterPath);
                    Util.FileUtil.Save(CharacterPath, SerializedCharacterData);
                    MyData.OnSaved();
                }
                else
                {
                    Debug.Log("Cannot save character as it has not been modified: " + MyCharacter.name);
                }
            }
        }

        public void SaveChunks(string SaveFolderName = "", bool IsForceSaveAll = false)
        {
            if (GetWorld())
            {
                foreach (KeyValuePair<Int3, Chunk> MyChunkDataPair in GetWorld().MyChunkData)
                {
                    Chunk MyChunk = MyChunkDataPair.Value;
                    if (MyChunk && (MyChunk.CanSave() || IsForceSaveAll))
                    {
                        string ChunkData = Util.FileUtil.ConvertToSingle(MyChunk.GetScript());
                        string ChunkPath = GetFilePath(MyChunk, SaveFolderName);
                        Debug.Log("Saving chunk to: " + ChunkPath);
                        Util.FileUtil.Save(ChunkPath, ChunkData);
                        MyChunk.OnSaved();
                    }
                    else
                    {
                        Debug.Log("Could not save Chunk: " + MyChunkDataPair.Key.ToString());
                    }
                }
            }
            else
            {
                Debug.LogError("    Level has no world.");
            }
        }
        #endregion

        #region Paths

        public string GetDataFilePath(string DataName, string DataTypeName, string SaveGameName = "")
        {
            return GetFolderPathExtra(DataTypeName, SaveGameName) + DataName + ".zel";
        }

        public void SetFilePathType(FilePathType NewType)
        {
            MyFilePathType = NewType;
        }

        public string GetFilePath(Chunk MyChunk, string SaveFolderName = "")
        {
            return GetFolderPath(SaveFolderName) + "Chunk_" + MyChunk.Position.x + "_" +
                MyChunk.Position.y + "_" + MyChunk.Position.z + ChunkFileExtentionPlusDot;
        }

        public string GetFilePath(Character MyCharacter, string SaveFolderName = "")
        {
            return GetCharacterFilePath(MyCharacter.name);
        }

        public string GetCharacterFilePath(string CharacterName, string SaveFolderName = "")
        {
            return GetFolderPathExtra("Characters", SaveFolderName) + CharacterName + CharacterFileExtensionPlusDot;
        }

        /// <summary>
        /// The extra subfolder, used for chunks, characters, zones
        /// </summary>
        public string GetFolderPathExtra(string ExtraFolderName = "Characters", string SaveFolderName = "")
        {
            return GetFolderPath(SaveFolderName) + ExtraFolderName + "/";
        }


        public string GetFolderPath(string SaveFolderName = "")
        {
            if (SaveFolderName == "")
            {
                string FolderPath = DataManager.Get().GetResourcesPath(MyFilePathType) + DataManager.Get().GetMapName() + "/" + (DataFolderNames.Levels + "/") + Name + "/";
                /*if (FileManagement.DirectoryExists(FolderPath, true, true) == false)    // 
                {
                    Debug.Log("Creating Directory for Level [" + Name + "]: " + FolderPath);
                    FileManagement.CreateDirectory(FolderPath, true);
                }
                //else
                {
                    // Debug.LogError("Getting Directory Path for Level [" + Name + "]: " + FolderPath);
                }*/
                return FolderPath;
            }
            else
            {
                //Debug.Log("Level: " + Name + " is Getting Save File Path: " + MyFilePathType.ToString());
                string FolderPath = DataManager.Get().GetResourcesPath(MyFilePathType) + DataManager.Get().GetMapName() + "/" + (DataFolderNames.Saves + "/") + SaveFolderName + "/" + Name + "/";
                /*if (FileManagement.DirectoryExists(FolderPath, true, true) == false)    // 
                {
                    Debug.Log("Creating Directory for Save Path [" + Name + "]: " + FolderPath);
                    FileManagement.CreateDirectory(FolderPath, true);
                }
                //else
                {
                    //Debug.Log("Getting Directory Path for Level [" + Name + "]: " + FolderPath);
                }*/
                return FolderPath;
            }
        }
        #endregion


        #region LevelLoading

        public IEnumerator LoadLevelWorldless(Action<int> OnLoadChunk = null)
        {
            yield return (LoadLevel(WorldManager.Get().SpawnWorld(), OnLoadChunk));
        }

        public IEnumerator LoadLevel(World MyWorld, Action<int> OnLoadChunk = null)
        {
            yield return (LoadLevel(MyWorld, Int3.Zero(), OnLoadChunk));
        }

        /// <summary>
        /// Load the level ! Base function for loading level meta into a world!
        /// </summary>
        public IEnumerator LoadLevel(World MyWorld, Int3 PositionOffset, Action<int> OnLoadChunk = null, SaveGame SavedGame = null)
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
                LevelHandler MyLevelHandler = MyWorld.gameObject.GetComponent<LevelHandler>();
                if (MyLevelHandler == null)
                {
                    MyLevelHandler = MyWorld.gameObject.AddComponent<LevelHandler>();
                }
                MyLevelHandler.MyLevel = this;
                SetWorld(MyWorld);
                MyWorld.name = Name;

                // First load chunks
                yield return MyWorld.SetWorldSizeRoutine(WorldManager.Get().RoamSize, WorldManager.Get().RoamPosition, null, true);
                // Then Generate Terrain
                if (GenerateTerrain())
                {
                    yield return WorldManager.Get().GetComponent<VoxelTerrain>().CreateTerrainWorldRoutine(MyWorld, false);
                }
                // Finally Load the chunks from edited data, as well as characters, zones, items etc
                yield return LoadChunksInLevel(MyWorld, OnLoadChunk, SavedGame);
            }
            else
            {
                Debug.LogError("World is null inside LoadLevel function");
            }
            //IsLoading = false;
        }

        private IEnumerator LoadChunksInLevel(World MyWorld, Action<int> OnLoadChunk = null, SaveGame SavedGame = null)
        {
            Int3 RoamSize = WorldManager.Get().RoamSize;
            Int3 RoamPosition = WorldManager.Get().RoamPosition;
            
            // load chunks
            string FolderPath = GetFolderPath();
            // Load Overwritten Character File Names
            Int3 ChunkPosition = Int3.Zero();
            float LoadCount = 0;
            for (ChunkPosition.x = -RoamSize.x + RoamPosition.x; ChunkPosition.x < RoamSize.x + RoamPosition.x; ChunkPosition.x++)
            {
                for (ChunkPosition.y = -RoamSize.y + RoamPosition.y; ChunkPosition.y < RoamSize.y + RoamPosition.y; ChunkPosition.y++)
                {
                    for (ChunkPosition.z = -RoamSize.z + RoamPosition.z; ChunkPosition.z < RoamSize.z + RoamPosition.z; ChunkPosition.z++)
                    {
                        yield return LoadChunk(SavedGame, ChunkPosition, FolderPath, MyWorld);
                        if (OnLoadChunk != null)
                        {
                            LoadCount++;
                            OnLoadChunk.Invoke((int)(LoadCount / (float)((RoamSize.x * 2) * (RoamSize.y * 2) * (RoamSize.z * 2) * 2) * 100));
                        }
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
                        if (OnLoadChunk != null)
                        {
                            LoadCount++;
                            OnLoadChunk.Invoke((int)(LoadCount / (float)((RoamSize.x * 2) * (RoamSize.y * 2) * (RoamSize.z * 2) * 2) * 100));
                        }
                    }
                }
            }
            //yield return LoadCharactersFromFiles(NewLevel, CharacterFiles, OnLoadChunk);
        }

        /// <summary>
        /// Loads chunk and characters in it
        /// Also Loads zones, item Objects, and anything else stored in it
        /// </summary>
        private IEnumerator LoadChunk(SaveGame MySaveGame, Int3 ChunkPosition, string FolderPath, World MyWorld)
        {
            string InnerChunkFileName = "Chunks/" + "Chunk_" + ChunkPosition.x + "_" + ChunkPosition.y + "_" + ChunkPosition.z + ChunkFileExtentionPlusDot;
            string ChunkFileName = FolderPath + InnerChunkFileName;
            bool DoesContainChunkFile = FileManagement.FileExists(ChunkFileName, true, true);
            if (MySaveGame != null)
            {
                string ChunkFileNameSaveGame = DataManager.GetFolderPath(DataFolderNames.Saves + "/") + Name + "/" + InnerChunkFileName;
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
            yield return LoadCharactersFromFiles(CharactersInChunk);
            yield return LoadZonesInChunk(ZonesInChunk);
        }

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
                MyScript = TakeAwayNames(MyScript, CharactersInChunk, "Characters");
                MyScript = TakeAwayNames(MyScript, ZonesInChunk, "Zones");
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

        private string TakeAwayNames(string MyScript, List<string> MyList, string ObjectType)
        {
            MyList.Clear();
            string[] MyScriptSplit = MyScript.Split('\n');
            // If Contains Characters on first line of chunk
            if (MyScriptSplit[0].Contains("/" + ObjectType))
            {
                // each line until /EndCharacters
                for (int i = 1; i < MyScriptSplit.Length; i++)
                {
                    // Until line is EndCharacters
                    if (MyScriptSplit[i].Contains("/End" + ObjectType))
                    {
                        // Make sure to cut script
                        string[] NewScriptSplit = new string[MyScriptSplit.Length - i - 1];
                        Array.Copy(MyScriptSplit, i + 1, NewScriptSplit, 0, NewScriptSplit.Length);
                        MyScript = FileUtil.ConvertToSingle(NewScriptSplit);
                        break;
                    }
                    else
                    {
                        MyList.Add(ScriptUtil.RemoveWhiteSpace(MyScriptSplit[i]));
                    }
                }
            }
            return MyScript;
        }


        private IEnumerator LoadZonesInChunk(List<string> ZoneNames, Action OnLoadChunk = null)
        {
            string Script = "";
            for (int i = 0; i < ZoneNames.Count; i++)
            {
                string ZonePath = GetDataFilePath(ZoneNames[i], "Zones");
                if (FileManagement.FileExists(ZonePath, true, true))
                {
                    if (ZonePath.Contains("://") || ZonePath.Contains(":///"))
                    {
                        WWW UrlRequest = new WWW(ZonePath);
                        yield return (UrlRequest);
                        Script = UrlRequest.text;
                    }
                    else
                    {
                        FileReaderRoutiner MyFileReader = new FileReaderRoutiner(ZonePath);
                        yield return MyFileReader.Run();
                        Script = MyFileReader.Result as string;
                    }
                    if (Script != null)
                    {
                        ZoneData NewData = JsonConvert.DeserializeObject(Script, typeof(ZoneData)) as ZoneData;
                        if (NewData != null)
                        {
                            NewData.SpawnInLevel(this);
                        }
                    }
                }
                yield return null;
            }
        }

        private IEnumerator LoadCharactersFromFiles(List<string> CharacterFiles, Action OnLoadChunk = null)
        {
            for (int i = 0; i < CharacterFiles.Count; i++)
            {
                Character MyCharacter = CharacterManager.Get().GetPoolObject();
                if (MyCharacter)
                {
                    MyCharacter.name = CharacterFiles[i];
                    // Set full file path
                    CharacterFiles[i] = GetFilePath(MyCharacter);
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
                                yield return MyCharacter.SetDataRoutine(NewData, this, false, false, (!Application.isPlaying)); // activate if in the editor and not playing
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

        public void SaveLevel()
        {
            string FolderPath = GetFolderPath();
            World LevelWorld = GetWorld();
            if (LevelWorld != null)
            {
                SetWorld(LevelWorld);
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
                Debug.LogError("Could not save level " + Name + "'s World.");
            }
        }
        #endregion
    }

}