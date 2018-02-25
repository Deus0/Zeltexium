using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Zeltex.Characters;
using Zeltex.Voxels;

namespace Zeltex
{
    /// <summary>
    /// All the meta data of a save game
    ///     the Variation of the levels is saved in the SaveGame folder
    /// </summary>
    public class SaveGame : ElementCore
    {
        // The level the save game is last on, if blank loads the first level
        [JsonProperty]
        public string LevelName = "";
        // The character the player last used, if blank loads the chracter creator
        [JsonProperty]
        public string CharacterName = "";
        // A list of the levels the save game has played on
        [JsonProperty]
        public List<string> LevelNames = new List<string>();
        [JsonIgnore]
        public Character MyCharacter;
        [JsonIgnore]
        private Level MyLevel;

        public SaveGame()
        {
            // Set level to newest
            List<string> LevelNames = DataManager.Get().GetNames(DataFolderNames.Levels);
            if (LevelNames != null && LevelNames.Count > 0)
            {
                LevelName = LevelNames[0];
            }
            else if (LevelNames == null)
            {
                Debug.LogError("Cannot instantiate save game as level hass not been loaded yet.");
            }
        }

        public void SetLevel(string NewLevelName)
        {
            if (DataManager.Get().GetNames(DataFolderNames.Levels).Contains(NewLevelName))
            {
                Debug.Log("Setting Level of " + Name + " to level: " + LevelName);
                LevelName = NewLevelName;
            }
            else
            {
                Debug.LogError("Could not set save game level to: " + NewLevelName);
                LevelName = "";
            }
            SetLevelFromData();
        }

        public Level GetLevel()
        {
            SetLevelFromData();
            return MyLevel;
        }

        private void SetLevelFromData()
        {
            if (LevelName == "")
            {
                MyLevel = null;
            }
            else
            {
                MyLevel = DataManager.Get().GetElement(DataFolderNames.Levels, LevelName) as Level;
            }
        }

        public void SetCharacter(Character NewCharacter)
        {
            MyCharacter = NewCharacter;
            string OldCharacterName = CharacterName;
            if (MyCharacter)
            {
                CharacterName = MyCharacter.name;
            }
            else
            {
                CharacterName = "";
            }
            if (OldCharacterName != CharacterName)
            {
                OnModified();
            }
        }

        public string GetFolderPath()
        {
            return DataManager.GetFolderPath(DataFolderNames.Saves + "/") + Name + "/";
        }
        public override void Save(bool IsForce = false)
        {
            if (CanSave() || IsForce)
            {
                // create saveGame Directories here
                try
                {
                    string MyDirectory = GetFolderPath();
                    if (FileManagement.DirectoryExists(MyDirectory, true, true) == false)
                    {
                        FileManagement.CreateDirectory(MyDirectory, true);
                    }
                    string CharactersDirectory = MyDirectory + "/Characters";
                    if (FileManagement.DirectoryExists(CharactersDirectory, true, true) == false)
                    {
                        FileManagement.CreateDirectory(CharactersDirectory, true);
                    }
                    string ChunksDirectory = MyDirectory + "/Chunks";
                    if (FileManagement.DirectoryExists(ChunksDirectory, true, true) == false)
                    {
                        FileManagement.CreateDirectory(ChunksDirectory, true);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("DirectoryNotFoundException at: " + Name + "\n" + e.ToString());
                }
                base.Save(IsForce);
            }
        }

        public override void Delete()
        {
            if (CanDelete())
            {
                string MyDirectory = GetFolderPath();
                // Remove folder too
                if (FileManagement.DirectoryExists(MyDirectory, true, true) == true)
                {
                    Debug.Log("Deleting SaveGame Directory [" + MyDirectory + "]");
                    FileManagement.DeleteDirectory(MyDirectory, true);
                }
                base.Delete();
            }
        }

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
            yield return LoadSaveGameRoutine();//WorldManager.Get().LoadSaveGameRoutine(this);
            IsSpawning = false;
        }

        public override void DeSpawn()
        {
            if (MyLevel != null)
            {
                MyLevel.DeSpawn();
            }
        }

        public override bool HasSpawned()
        {
            return (MyLevel != null && MyLevel.HasSpawned());
        }
        #endregion

        public void LoadSaveGame(Action<int> OnLoadChunk = null)//Level MyLevel, string CharacterScript, string StartingLocation = "")
        {
            UniversalCoroutine.CoroutineManager.StartCoroutine(LoadSaveGameRoutine(OnLoadChunk));// MyLevel, CharacterScript));
        }

        /// <summary>
        /// Used by SaveGameMaker to load a level with a character script
        /// </summary>
        public IEnumerator LoadSaveGameRoutine(Action<int> OnLoadChunk = null)
        {
            World SpawnedWorld = WorldManager.Get().SpawnWorld();

            // Next Load the level
            yield return GetLevel().LoadLevel(SpawnedWorld, Int3.Zero(), OnLoadChunk, this);

            // Creates a new character
            if (CharacterName == "")
            {
                yield return CreateMainCharacter();
            }
            else
            {
                bool WasFound = false;
                Character LevelCharacter = null;
                // Set character to levels loaded character
                for (int i = 0; i < GetLevel().GetRealCharactersCount(); i++)
                {
                    LevelCharacter = GetLevel().GetCharacter(i);
                    if (LevelCharacter && LevelCharacter.GetData().Name == CharacterName)
                    {
                        SetCharacter(LevelCharacter);
                        WasFound = true;
                        break;
                    }
                }
                if (WasFound == false)
                {
                    Debug.Log("Could not find Main Character in Level: " + CharacterName);
                    yield return CreateMainCharacter();
                }
                else
                {
                    Debug.Log("Set Save Game Character to: " + MyCharacter.name);
                }
            }
        }

        /// <summary>
        /// Creates the main character
        /// Currently uses 0 - which happens to be Alzo
        /// </summary>
        private IEnumerator CreateMainCharacter()
        {
            // then load bot with script
            Character MyCharacter = CharacterManager.Get().GetPoolObject();
            if (MyCharacter != null)
            {
                // GetClass Script
                CharacterData Data = DataManager.Get().GetElement(DataFolderNames.Characters, 0) as CharacterData;
                MyCharacter.transform.position = GetLevel().GetSpawnPoint();
                yield return (MyCharacter.SetDataRoutine(Data, GetLevel()));
                GetLevel().AddCharacter(MyCharacter);
                SetCharacter(MyCharacter);
            }
            else
            {
                Debug.LogError("=====-=-----------==========------");
                Debug.LogError("Character Pooled Object is null inside LoadNewSaveGame function");
                Debug.LogError("=====-=-----------==========------");
            }
        }

    }
}