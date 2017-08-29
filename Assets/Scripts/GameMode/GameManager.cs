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
        private GameObject MainMenu;
        public UnityEvent OnBeginGame = new UnityEvent();
        public UnityEvent OnEndGame = new UnityEvent();
        public AudioClip OnBeginAudio;
        private bool IsEnding;

        // Use this for initialization
        void Start()
        {
            DataManager.Get().MapName = PlayerPrefs.GetString(DataManager.Get().ResourcesName, "");
            if (DataManager.Get().MapName != "")
            {
                DataManager.Get().LoadAll();
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

        private void Update()
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
        }

        public void BeginGame()
        {
            if (!Game.GameMode.IsPlaying)
            {
                Game.GameMode.IsPlaying = true;
                Zeltex.Networking.NetworkManager.Get().HostGame();  // hosting is the main way to play now
                StartCoroutine(BeginGameRoutine());
            }
            else
            {
                Debug.LogError("Already playing game.");
            }
        }

        private IEnumerator BeginGameRoutine()
        {
            yield return new WaitForSeconds(0.5f);
            PoolsManager.Get().SpawnPools.Invoke();
            //Camera.main.gameObject.SetActive(false);
            CameraManager.Get().EnableGameCamera();
            //CameraManager.Get().GetMainCamera().GetComponent<Possess>().SetCharacter(Characters.CharacterManager.Get().GetSpawn(0, 0));
            if (OnBeginAudio)
            {
                GetComponent<AudioSource>().PlayOneShot(OnBeginAudio);
            }
            OnBeginGame.Invoke();
        }

        public void EndGame()
        {
            if (Game.GameMode.IsPlaying)
            {
                Game.GameMode.IsPlaying = false;
                PoolsManager.Get().ClearPools.Invoke();
                Zeltex.Networking.NetworkManager.Get().StopHosting();
                OnEndGame.Invoke();
                CameraManager.Get().EnableMainMenuCamera();
                GuiSpawner.Get().EnableGui("MainMenu");
            }
        }

        private void RefreshMainMenuListener()
        {
            MainMenu = GuiSpawner.Get().GetGui("MainMenu");
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
    }

}