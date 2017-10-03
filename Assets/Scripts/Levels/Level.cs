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
        // The initial script for debugging
        [SerializeField, JsonProperty]
        private Int3 MyWorldSize = new Int3(0, 0, 0);
        [SerializeField, JsonProperty]
        private bool IsInfinite;
        [SerializeField, JsonProperty]
        private bool IsGenerateTerrain;

        [SerializeField, JsonIgnore]
        private string MyScript = "";
        // The world loaded for the level
        [SerializeField, JsonIgnore]
        private World MyWorld;
        /// <summary>
        /// Characters linked to a level
        /// </summary>
        [SerializeField, JsonIgnore]
        protected List<Character> MyCharacters = new List<Character>();
        [SerializeField, JsonProperty]
        protected int CharactersCount = 0;

        #region Overrides

        private void SetDefaults()
        {
            MyWorldSize = new Int3(7, 2, 7);
            IsInfinite = false;
            IsGenerateTerrain = false;
        }

        public override string GetScript()
        {
            string Script = "";
            if (IsGenerateTerrain)
            {
                Script += "GenerateTerrain" + "\n";
            }
            if (IsInfinite)
            {
                Script += "Infinite" + "\n";
            }
            if (Script != "")
            {
                Script.Remove(Script.Length - 1);
            }
            Debug.LogError("Saving level: " + Script);
            return Script;
        }

        public override void RunScript(string Script)
        {
            SetDefaults();
            string[] Data = Script.Split('\n');
            for (int i = 0; i< Data.Length; i++)
            {
                if (Data[i].Contains("GenerateTerrain"))
                {
                    IsGenerateTerrain = true;
                }
                else if (Data[i].Contains("Infinite"))
                {
                    IsInfinite = true;
                }
            }
            MyScript = Script;
            //Debug.LogError("Run script of level: " + Script);
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

        public void SetWorld(World NewWorld)
        {
            MyWorld = NewWorld;
        }

        public World GetWorld()
        {
            return MyWorld;
        }

        public string GetFolderPath()
        {
            string FolderPath = DataManager.GetFolderPath(DataFolderNames.Levels + "/") + Name + "/";
            if (System.IO.Directory.Exists(FolderPath) == false)
            {
                Debug.Log("Creating Directory: " + FolderPath);
                System.IO.Directory.CreateDirectory(FolderPath);
            }
            return FolderPath;
        }

        public static string ChunkFileExtension = "chn";
        public static string CharacterFileExtension = "chr";
        public void SaveOpenCharacters(bool IsForceSaveAll = false)
        {
            for (int i = 0; i < MyCharacters.Count;i++)
            {
                if (MyCharacters[i])
                {
                    CharacterData MyData = MyCharacters[i].GetData();
                    if (MyData.CanSave() || IsForceSaveAll)
                    {
                        string SerializedCharacterData = MyData.GetSerial();
                        string CharacterPath = GetFilePath(MyCharacters[i]);
                        Util.FileUtil.Save(CharacterPath, SerializedCharacterData);
                        MyData.OnSaved();
                    }
                }
            }
        }

        public string GetFilePath(Character MyCharacter)
        {
            return DataManager.GetFolderPath(DataFolderNames.Levels + "/") + Name + "/" + MyCharacter.name + "." + CharacterFileExtension;
        }

        public void SaveOpenChunks(bool IsForceSaveAll = false)
        {
            if (GetWorld())
            {
                foreach (KeyValuePair<Int3, Chunk> MyChunkDataPair in GetWorld().MyChunkData)
                {
                    Chunk MyChunk = MyChunkDataPair.Value;
                    if (MyChunk && (MyChunk.IsDirtyTrigger() || IsForceSaveAll))
                    {
                        string ChunkData = Util.FileUtil.ConvertToSingle(MyChunk.GetScript());
                        string ChunkPath = GetFilePath(MyChunk);
                        Debug.Log("Saving chunk to: " + ChunkPath);
                        Util.FileUtil.Save(ChunkPath, ChunkData);
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

        public string GetFilePath(Chunk MyChunk)
        {
            return DataManager.GetFolderPath(DataFolderNames.Levels + "/") + Name + "/" + "Chunk_" + MyChunk.Position.x + "_" + MyChunk.Position.y + "_" + MyChunk.Position.z + "." + ChunkFileExtension;
        }

        public int GetCharactersCount()
        {
            return CharactersCount;
        }

        public void AddCharacter(Character NewCharacter)
        {
            MyCharacters.Add(NewCharacter);
        }
    }

}