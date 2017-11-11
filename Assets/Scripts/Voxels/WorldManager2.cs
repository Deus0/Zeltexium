using System.Collections;
using UnityEngine;
using Zeltex.Characters;
using Zeltex.Guis.Maker;
using Zeltex.Guis;
using Zeltex.Util;
using System;

namespace Zeltex.Voxels
{
    /// <summary>
    /// Character loading part of the world manager
    /// </summary>
    public partial class WorldManager : ManagerBase<WorldManager>
    {
        private Character MyCharacter;

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
                yield return UniversalCoroutine.CoroutineManager.StartCoroutine(CreateMainCharacter(MyGame, OnLoadChunk));
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
                    Debug.LogError("Did not find Character spawned: " + MyGame.CharacterName);
                }
                else
                {
                    Debug.LogError("Set Save Game Character to: " + MyGame.MyCharacter.name);
                }
            }
        }

        private IEnumerator CreateMainCharacter(SaveGame MyGame = null, Action OnLoadChunk = null)
        {
            // then load bot with script
            Character MyCharacter = CharacterManager.Get().GetPoolObject();
            if (MyCharacter != null)
            {
                // GetClass Script
                CharacterData Data = DataManager.Get().GetElement(DataFolderNames.Characters, 0) as CharacterData;
                yield return UniversalCoroutine.CoroutineManager.StartCoroutine(MyCharacter.SetDataRoutine(Data));
                if (OnLoadChunk != null)
                {
                    OnLoadChunk.Invoke();
                }
                MyGame.GetLevel().AddCharacter(MyCharacter);
                MyGame.SetCharacter(MyCharacter);
            }
            else
            {
                Debug.LogError("Character Pooled Object is null inside LoadNewSaveGame function");
            }
        }

        /// <summary>
        /// Creates new save game using a racename and classname
        /// </summary>
        /*public IEnumerator LoadNewSaveGame(SaveGame MyGame, System.Action OnLoadChunk = null)    //Level MyLevel, string RaceName, string ClassName, string StartingLocation = ""
        {
            yield return UniversalCoroutine.CoroutineManager.StartCoroutine(GameManager.Get().BeginGameRoutineMain());
            LoadedSaveGame = MyGame;
            // Load the level
            if (MyGame != null)
            {
                yield return UniversalCoroutine.CoroutineManager.StartCoroutine(LoadLevelWorldless(MyGame.GetLevel(), OnLoadChunk));
            }
            else
            {
                Debug.LogError("MyGame is null LoadNewSaveGame");
            }

            // Creates a new character
            if (MyGame != null && MyGame.CharacterName == "")
            {
                // then load bot with script
                Character MyCharacter = CharacterManager.Get().GetPoolObject();
                if (MyCharacter != null)
                {
                    // GetClass Script
                    CharacterData Data = DataManager.Get().GetElement(DataFolderNames.Characters, 0) as CharacterData;
                    yield return UniversalCoroutine.CoroutineManager.StartCoroutine(MyCharacter.SetDataRoutine(Data));
                    MyGame.SetCharacter(MyCharacter);
                    if (OnLoadChunk != null)
                    {
                        OnLoadChunk.Invoke();
                    }
                    MyGame.GetLevel().AddCharacter(MyCharacter);
                }
                else
                {
                    Debug.LogError("Character Pooled Object is null inside LoadNewSaveGame function");
                }
            }
            else
            {
                Debug.LogError("Either MyGame is null or CharacterName isn't Nothing");
                // Set character to levels loaded character
                ///MyGame.SetCharacter(MyCharacter);
            }
        }*/
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
            World WorldInsideOf = MainCharacter.GetWorldInsideOf();
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
    }
}