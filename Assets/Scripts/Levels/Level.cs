using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Voxels;
using Newtonsoft.Json;
using Zeltex.Characters;

namespace Zeltex
{
    /// <summary>
    /// Each level contains one of these
    ///     -> Lights, positions, directions
    ///     -> Environment settings: Fog, Colour of background
    ///     -> Music to play
    ///     -> World settings - endless, generate settings etc
    /// </summary>
    [System.Serializable]
    public class Level : Element
    {
        public static string ChunkFileExtension = "chn";
        public static string CharacterFileExtension = "chr";
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
        [SerializeField, JsonProperty]
        protected int CharactersCount = 0;
        [SerializeField, JsonIgnore]
        private Zeltex.Util.FilePathType MyFilePathType = Zeltex.Util.FilePathType.StreamingPath;

        #region Overrides

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
            yield return WorldManager.Get().LoadLevelWorldless(this);
            IsSpawning = false;
        }

        public override void DeSpawn()
        {
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
                //OnModified();
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

        #region Saving

        public void SaveOpenCharacters(string SaveFolderName = "", bool IsForceSaveAll = false)
        {
            for (int i = 0; i < MyCharacters.Count; i++)
            {
                MyCharacters[i].GetData().SetCharacter(MyCharacters[i], false);
                SaveCharacterToLevel(MyCharacters[i], SaveFolderName, IsForceSaveAll);
            }
        }

        public void SaveCharacterToLevel(Character MyCharacter, string SaveFolderName = "", bool IsForceSaveAll = false)
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

        public void SaveOpenChunks(string SaveFolderName = "", bool IsForceSaveAll = false)
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

        public void SetFilePathType(Zeltex.Util.FilePathType NewType)
        {
            MyFilePathType = NewType;
        }

        public string GetFilePath(Chunk MyChunk, string SaveFolderName = "")
        {
            if (SaveFolderName == "")
            {
                return GetFolderPath() + "/" + "Chunk_" + MyChunk.Position.x + "_" + MyChunk.Position.y + "_" + MyChunk.Position.z + "." + ChunkFileExtension;    //DataManager.GetFolderPath(DataFolderNames.Levels + "/") + Name + "/" 
            }
            else
            {
                return GetSaveFolderPath(SaveFolderName) + "Chunk_" + MyChunk.Position.x + "_" + MyChunk.Position.y + "_" + MyChunk.Position.z + "." + ChunkFileExtension;
            }
        }

        public string GetFilePath(Character MyCharacter, string SaveFolderName = "")
        {
            if (SaveFolderName == "")
            {
                return GetFolderPath() + "Characters/" +
                    //"Chunk_" + MyCharacter.GetChunkPosition().x + "_" + MyCharacter.GetChunkPosition().y + "_" + MyCharacter.GetChunkPosition().z + "_" + 
                    MyCharacter.name + "." + CharacterFileExtension;
            }
            else
            {
                return GetSaveFolderPath(SaveFolderName) + "Characters/" +
                    //"Chunk_" + MyCharacter.GetChunkPosition().x + "_" + MyCharacter.GetChunkPosition().y + "_" + MyCharacter.GetChunkPosition().z + "_" + 
                    MyCharacter.name + "." + CharacterFileExtension;
            }
        }

        /*public string GetCharacterFilePath(string CharacterName, string SaveFolderName = "")
        {
            if (SaveFolderName == "")
            {
                return GetFolderPath() + "Characters/" + CharacterName + "." + CharacterFileExtension;
            }
            else
            {
                return GetSaveFolderPath(SaveFolderName) + "Characters/" + CharacterName + "." + CharacterFileExtension;
            }
        }*/

        public string GetSaveFolderPath(string SaveFolderName)
        {
            //Debug.Log("Level: " + Name + " is Getting Save File Path: " + MyFilePathType.ToString());
            string FolderPath = DataManager.Get().GetResourcesPath(MyFilePathType) + DataManager.Get().GetMapName() + "/" + (DataFolderNames.Saves + "/") + SaveFolderName + "/" + Name + "/";
            if (FileManagement.DirectoryExists(FolderPath, true, true) == false)    // 
            {
                Debug.Log("Creating Directory for Save Path [" + Name + "]: " + FolderPath);
                FileManagement.CreateDirectory(FolderPath, true);
            }
            //else
            {
                //Debug.Log("Getting Directory Path for Level [" + Name + "]: " + FolderPath);
            }
            return FolderPath;
        }

        public string GetFolderPath()
        {
            string FolderPath = DataManager.Get().GetResourcesPath(MyFilePathType) + DataManager.Get().GetMapName() + "/" + (DataFolderNames.Levels + "/") + Name + "/";
            if (FileManagement.DirectoryExists(FolderPath, true, true) == false)    // 
            {
                Debug.Log("Creating Directory for Level [" + Name + "]: " + FolderPath);
                FileManagement.CreateDirectory(FolderPath, true);
            }
            //else
            {
               // Debug.LogError("Getting Directory Path for Level [" + Name + "]: " + FolderPath);
            }
            return FolderPath;
        }
        #endregion
    }

}