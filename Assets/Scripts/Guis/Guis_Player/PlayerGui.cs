using UnityEngine;
using System.Collections.Generic;
using Zeltex.Guis;

namespace Zeltex.Guis.Players
{
    /// <summary>
    /// Just a manual script for the players gui
    /// </summary>
    public class PlayerGui : MonoBehaviour
    {
        #region Variables
        [Header("Debug")]
        public bool DebugMode;
        public string BeginGui = "DownloadMenu";
        //[Header("Audio")]
        //private AudioSource MySource;
        //public AudioClip MyButtonSound;

        //public KeyCode MyEscapeKey = KeyCode.Escape;
        [Header("Reference")]
        public GameObject CurrentGui;
        // main ones
        [Header("Main")]
        public GameObject MainMenu;
        public GameObject DownloadMenu;
        public GameObject Options;
        public GameObject Tools;     // individual texture drawer
        // selectors
        [Header("Selectors")]
        public GameObject Lobby;
        public GameObject MapSelect;
        public GameObject CharacterSelect;
        public GameObject GameModeGui;          // rules to win the game, spawning of things
                                                // editors
                                                //		 editors can be in gui form or in scripting form -
                                                //		scripting will offer more features as its easier to implement them in code
        [Header("Editors")]
        public GameObject CharacterMaker;
        public GameObject MapMaker;
        public GameObject ClassMaker;
        public GameObject ItemMaker;        // Items meta data
        public GameObject BlockMaker;       // blocks Meta Data
        public GameObject TextureMaker;     // individual texture drawer

        [Header("In Game")]
        public GameObject PauseMenu;
        public GameObject EndGameGui;
        public GameObject RespawnGui;
        public GameObject Tutorial;
        public GameObject Console;

        private List<GameObject> PlayerGuis = new List<GameObject>();
        #endregion

        void Start()
        {
            //Debug.Log("Begining player gui.");
            GoTo(BeginGui, false);
            //GoTo("MainMenu", false);
            //Tutorial.SetActive (true);
        }
        // Update is called once per frame
        /*void Update()
        {
            if (Input.GetKeyDown(MyEscapeKey))
            {
                if (PauseMenu.GetComponent<ZelGui>().GetActive())
                    GoTo("UnPause");
                else
                    GoTo("Pause");
            }
        }*/

        public void Close()
        {
            GoTo("Nothing");
        }
        public void GoTo(string GuiName)
        {
            GoTo(GuiName, true);
        }
        public ZelGui MainMenuBackground;
        public void GoTo(string GuiName, bool IsSound)
        {
            /*if (MySource == null)
            {
                MySource = gameObject.GetComponent<AudioSource>();
            }*/
            if (PlayerGuis.Count == 0)
            {
                PlayerGuis = GetPlayerGuis();
            }
            /*if (MySource && IsSound && MyButtonSound)
            {
                MySource.pitch = Mathf.Pow(2f, 2*Mathf.RoundToInt(Random.Range(-12,12)) / 12f);
                MySource.PlayOneShot(MyButtonSound);
            }*/

            if (GuiName == "UnPause" || GuiName == "Nothing")
            {
                if (PauseMenu && PauseMenu.activeSelf)
                {
                    CurrentGui = null;
                }
                MainMenuBackground.TurnOff();
                DisableGuis();
                return;
            }
            else if (GuiName == "ExitGame")
            {
                Application.Quit();
            }

            GameObject OldGui = CurrentGui;
            for (int i = 0; i < PlayerGuis.Count; i++)
            {
                if (GuiName == PlayerGuis[i].name)
                {
                    CurrentGui = PlayerGuis[i];
                }
            }

            if (CurrentGui &&
                OldGui != CurrentGui)
            {
                DisableGuis();
                //CurrentGui.SetActive(true);
                if (CurrentGui.GetComponent<ZelGui>())
                {
                    //Debug.Log("Turning on gui: " + CurrentGui.name + " at " + Time.realtimeSinceStartup);
                    CurrentGui.GetComponent<ZelGui>().TurnOn();
                }
                if (OldGui)
                {
                    //Debug.Log("Giving: " + CurrentGui.name + " the position of " + OldGui.name + " at " + Time.realtimeSinceStartup);
                    //CurrentGui.GetComponent<Orbitor>().SetScreenPosition(OldGui.GetComponent<Orbitor>().GetScreenPosition());
                    //CurrentGui.transform.position = OldGui.transform.position;
                }
            }
        }

        private void DisableGuis()
        {
            for (int i = 0; i < PlayerGuis.Count; i++)
            {
                PlayerGuis[i].GetComponent<ZelGui>().TurnOff();
                //PlayerGuis[i].SetActive(false);
            }
        }

        public List<GameObject> GetPlayerGuis()
        {
            List<GameObject> MyPlayerGuis = new List<GameObject>();
            // Main
            if (MainMenu)
                MyPlayerGuis.Add(MainMenu);
            if (DownloadMenu)
                MyPlayerGuis.Add(DownloadMenu);
            if (Tools)
                MyPlayerGuis.Add(Tools);
            if (Options)
                MyPlayerGuis.Add(Options);
            // Selectors
            if (Lobby)
                MyPlayerGuis.Add(Lobby);
            if (MapSelect)
                MyPlayerGuis.Add(MapSelect);
            if (CharacterSelect)
                MyPlayerGuis.Add(CharacterSelect);
            if (GameModeGui)
                MyPlayerGuis.Add(GameModeGui);
            // makers
            if (MapMaker)
                MyPlayerGuis.Add(MapMaker);
            if (CharacterMaker)
                MyPlayerGuis.Add(CharacterMaker);
            /*if (ClassMaker)
                MyPlayerGuis.Add(ClassMaker);
            if (ItemMaker)
                MyPlayerGuis.Add(ItemMaker);
            if (BlockMaker)
                MyPlayerGuis.Add(BlockMaker);
            if (TextureMaker)
                MyPlayerGuis.Add(TextureMaker);
            if (TextureMapSelect)
                MyPlayerGuis.Add(TextureMapSelect);*/

            if (PauseMenu)
                MyPlayerGuis.Add(PauseMenu);
            if (Tutorial)
                MyPlayerGuis.Add(Tutorial);
            if (EndGameGui)
                MyPlayerGuis.Add(EndGameGui);
            if (RespawnGui)
                MyPlayerGuis.Add(RespawnGui);
            return MyPlayerGuis;
        }
    }
}