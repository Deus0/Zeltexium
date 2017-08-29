using System.Collections;
using UnityEngine;
using Zeltex.Characters;
using Zeltex.Guis.Maker;
using Zeltex.Guis;
using Zeltex.Util;

namespace Zeltex.Voxels
{
    /// <summary>
    /// Character loading part of the world manager
    /// </summary>
    public partial class WorldManager : ManagerBase<WorldManager>
    {
        private Character MyCharacter;

        #region LevelWithCharacters

        public void LoadSaveGame(Level MyLevel, string CharacterScript, string StartingLocation = "")
        {
            StartCoroutine(LoadSaveGameRoutine(MyLevel, CharacterScript));
        }

        /// <summary>
        /// Used by SaveGameMaker to load a level with a character script
        /// </summary>
        public IEnumerator LoadSaveGameRoutine(Level MyLevel, string CharacterScript, string StartingLocation = "")
        {
            // fade out and begin loading
			if (ImageFader.Get())
			{
				ImageFader.Get().FadeOut(1f);
			}

            World SpawnedWorld = SpawnWorld();

            // Load Character First
            Character MyCharacter = CharacterManager.Get().GetPoolObject();
            if (MyCharacter)
            {
                yield return MyCharacter.RunScriptRoutine(FileUtil.ConvertToList(CharacterScript));
            }

            // Next Load the level
            // yield return LoadLevelWorldless(MyLevel, MyCharacter.GetChunkPosition());
            // get camera distance

            //Int3 CameraDistance = new Int3(10, 4, 10);
           // yield return SpawnedWorld.SetWorldSizeRoutine(CameraDistance, MyCharacter.GetChunkPosition());
            // Next Load the level
            yield return LoadLevel(SpawnedWorld, MyLevel, MyCharacter.GetChunkPosition());

			if (ImageFader.Get())
			{
				// while until fading finishes if loads too fast
				while (ImageFader.Get().IsFading)
				{
					yield return null;
				}
			}
            Possess.PossessCharacter(MyCharacter);
            //MyCharacter.SetMovement(false);
            GuiSpawner.Get().DestroySpawn("MainMenu");
			if (ImageFader.Get())
			{
				ImageFader.Get().FadeIn(1.5f);
				while (ImageFader.Get().IsFading)
				{
					yield return null;
				}
			}
            //MyCharacter.SetMovement(true);
        }

        /// <summary>
        /// Creates new save game using a racename and classname
        /// </summary>
        public IEnumerator LoadNewSaveGame(Level MyLevel, string RaceName, string ClassName, string StartingLocation = "")
        {
            // fade out and begin loading
			if (ImageFader.Get())
			{
				ImageFader.Get().FadeOut(1f);
			}
            // Load the level
            yield return LoadLevelWorldless(MyLevel);
            // then load bot with script
            Character MyCharacter = CharacterManager.Get().GetPoolObject();
            // GetClass Script
            string ClassScript = DataManager.Get().Get("Classes", ClassName);
            string RaceScript = DataManager.Get().Get("Skeletons", RaceName);
            yield return MyCharacter.GetSkeleton().Load(RaceName, FileUtil.ConvertToList(RaceScript));
            yield return MyCharacter.RunScriptRoutine(ClassName, FileUtil.ConvertToList(ClassScript));
            Vector3 NewPosition = SpawnPositionFinder.FindNewPositionChunkBoundaries(MyWorlds[MyWorlds.Count - 1]);
            MyCharacter.transform.position = NewPosition;

            // while until fading finishes if loads too fast
            int FramesCount = 0;
			if (ImageFader.Get())
			{
				while (ImageFader.Get().IsFading)
				{
					yield return null;
					FramesCount++;
					MyCharacter.transform.position = NewPosition;
					if (FramesCount >= 5000)
					{
						Debug.LogError("Waited past 5000 frames");
						break;
					}
				}
			}
            WorldManager.Get().SaveGame(MyCharacter);
            Possess.PossessCharacter(MyCharacter);

            GuiSpawner.Get().DestroySpawn("MainMenu");
			if (ImageFader.Get())
			{
				ImageFader.Get().FadeIn(1.5f);
				while (ImageFader.Get().IsFading)
				{
					yield return null;
				}
			}
			MyCharacter.transform.position = NewPosition;
            if (StartingLocation != "")
            {
                MyCharacter.transform.position = new Vector3(666, MyCharacter.transform.position.y, 666);
            }
            //MyCharacter.SetMovement(true);
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
                NewPosition = new Int3((int)Random.Range(-3000, 3000), 8, (int)Random.Range(-3000, 3000));
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

        /// <summary>
        /// Saves the current game
        /// </summary>
        public void SaveGame(Character MyCharacter)
        {
            if (MyWorlds.Count > 0)
            {
                World WorldToSave = MyWorlds[MyWorlds.Count - 1];   // should be a character->GetWorldIn function
                string LevelName = WorldToSave.name;
                string LevelScript = "/Level " + LevelName + "\n";
                LevelScript += FileUtil.ConvertToSingle(MyCharacter.GetScript());
                string FilePath = FileUtil.GetFolderPath(DataFolderNames.Saves + "/") + SaveGameName + "/" + SavesMaker.DefaultLevelName;
                Debug.LogError("Saving [" + SaveGameName + "] with character [" + MyCharacter.name + "] to [" + FilePath + "]:\n" + LevelScript);
                FileUtil.Save(FilePath, LevelScript);
            }
            else
            {
                Debug.LogError("No worlds to save.");
            }
        }

        #endregion
    }
}