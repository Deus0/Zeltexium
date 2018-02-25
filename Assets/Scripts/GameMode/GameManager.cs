using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zeltex.Util;
using Zeltex.Guis;

namespace Zeltex
{
    /// <summary>
    /// Managers the players game
    /// </summary>
    public class GameManager : ManagerBase<GameManager>
    {
        public static bool IsCameraFixedUpdate = true;
        public static bool IsOrbitorFixedUpdate = true;
        //private GameObject MainMenu;
        public UnityEvent OnBeginGame = new UnityEvent();
        public UnityEvent OnEndGame = new UnityEvent();
        public AudioClip OnBeginAudio;
        private bool IsEnding;
        public bool IsAllHaveStatsBar = true;
        public int TargetFrameRate = -1;

        public static new GameManager Get()
        {
            if (MyManager == null)
            {
                MyManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            return MyManager;
        }
        // Use this for initialization
        void Start()
        {
            if (TargetFrameRate != -1)
            {
                Application.targetFrameRate = TargetFrameRate;
            }
            /*GameObject MainMenu = GuiSpawner.Get().SpawnGui("MainMenu");
            if (MainMenu)
            {
                RefreshMainMenuListener();
            }*/
            /*else
            {
                StartMenuButton.onClick.RemoveAllListeners();
                StartMenuButton.onClick.AddEvent(
                    delegate
                    {
                        GuiSpawner.Get().DestroySpawn(MainMenu);
                        GuiSpawner.Get().SpawnGui("ResourcesMaker");
                    });
            }*/
        }

        /*private void Update()
        {
            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                Time.timeScale *= 1.1f;
                Debug.Log("TimeScale: " + Time.timeScale.ToString());
            }
            if (Input.GetKeyDown(KeyCode.PageDown))
            {
                Time.timeScale /= 1.1f;
                Debug.Log("TimeScale: " + Time.timeScale.ToString());
            }
        }*/

        /// <summary>
        /// Begin editing the resources
        /// </summary>
        public void BeginResourcesEditing()
        {
            GuiSpawner.Get().DestroySpawn(GuiSpawner.Get().GetGui("MainMenu"));
            GuiSpawner.Get().SpawnGui("ResourcesMaker");
        }

        /// <summary>
        /// Begin editing the resources
        /// </summary>
        public void BeginSettings()
        {
            GuiSpawner.Get().DestroySpawn(GuiSpawner.Get().GetGui("MainMenu"));
            GuiSpawner.Get().SpawnGui("Settings");
        }

        /// <summary>
        /// Begin normal game mode
        /// </summary>
        public void BeginGame()
        {
            if (!Game.GameMode.IsPlaying)
            {
                Game.GameMode.IsPlaying = true;
                StartCoroutine(BeginGameRoutine());
            }
            else
            {
                Debug.LogError("Already playing game.");
            }
        }

        /*public IEnumerator BeginGameRoutineMain()
        {
            if (!Game.GameMode.IsPlaying)
            {
                Game.GameMode.IsPlaying = true;
                Debug.Log("Beginning to play game.");
                yield return (BeginGameRoutine());
            }
            else
            {
                Debug.LogError("Already playing game.");
            }
        }*/

        /// <summary>
        /// To do - do this step after chosing a savegame or host
        /// </summary>
        /// <returns></returns>
        private IEnumerator BeginGameRoutine()
        {
            Debug.Log("Beginning to play game.");
            GuiSpawner.Get().DestroySpawn(GuiSpawner.Get().GetGui("MainMenu"));
            GuiSpawner.Get().SpawnGui("SaveGames");
            yield return null;
        }

        /// <summary>
        /// Hosts networking game
        /// </summary>
        /// <returns></returns>
        public IEnumerator HostGame()
        {
            Networking.NetworkManager.Get().HostGame();  // hosting is the main way to play now
            // Wait For Host Game to do its thing!
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            bool HasSpawned = false;
            System.Action OnFinishLoading = () =>
            {
                HasSpawned = true;
            };
            PoolsManager.Get().SpawnPools(OnFinishLoading);
            while (!HasSpawned)
            {
                yield return null;
            }
            OnBeginGame.Invoke();
            // finish!
        }

        public void PlayCharacter()
        {
            CameraManager.Get().EnableGameCamera();
            if (OnBeginAudio)
            {
                GetComponent<AudioSource>().PlayOneShot(OnBeginAudio);
            }
        }

        public void EndGame()
        {
            if (Game.GameMode.IsPlaying)
            {
                Game.GameMode.IsPlaying = false;
                Voxels.WorldManager.Get().Clear();
                Characters.CharacterManager.Get().Clear();
                PoolsManager.Get().ClearAllPools();
                Zeltex.Networking.NetworkManager.Get().StopHosting();
                OnEndGame.Invoke();
                CameraManager.Get().EnableMainMenuCamera();
                GuiSpawner.Get().SpawnGui("MainMenu");
            }
        }

        private void RefreshMainMenuListener()
        {
            GameObject MainMenu = GuiSpawner.Get().GetGui("MainMenu");
            if (MainMenu)
            {
                Button StartMenuButton = MainMenu.transform.Find("Header").Find("StartButton").GetComponent<Button>();
                StartMenuButton.onClick.RemoveAllListeners();
                StartMenuButton.onClick.AddEvent(MainMenu_StartButtonClicked);
            }
            else
            {
                Debug.LogError("Could not spawn main menu.");
            }
        }

        public void MainMenu_StartButtonClicked()
        {
            GameObject MainMenu = GuiSpawner.Get().GetGui("MainMenu");
            if (MainMenu)
            {
                GuiSpawner.Get().Disable(MainMenu);
            }
            if (DataManager.Get().MapName != "")
            {
                GuiSpawner.Get().SpawnGui("SaveGames");
            }
            else
            {
                GuiSpawner.Get().SpawnGui("ResourcesMaker");
            }
        }

        /// <summary>
        /// Ends the game
        /// Wipes map
        /// And characters
        /// </summary>
        /*public void EndGame()
        {
            if (IsEnding == false)
            {
                IsEnding = true;
                StartCoroutine(EndGameRoutine());
            }
            //Zeltex.Voxels.WorldManager.Get();
        }

        private IEnumerator EndGameRoutine()
        {
            Characters.CharacterManager.Get().Cull();
            yield return new WaitForSeconds(3f);
            Characters.CharacterManager.Get().Clear();
            Items.ItemManager.Get().Clear();
            Voxels.WorldManager.Get().Clear();
            GuiSpawner.Get().SpawnGui("MainMenu");
            RefreshListener();
            IsEnding = false;
        }*/
        Player PausingPlayer;

        public void PauseGame(Player MyPlayer)
        {
           // Time.timeScale = 0;
            PausingPlayer = MyPlayer;
            PausingPlayer.SetMouse(false);
        }

        public void ResumeGame()
        {
            //Time.timeScale = 1;
            PausingPlayer.SetMouse(true);
        }
    }

}