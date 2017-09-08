using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using MakerGuiSystem;
using Zeltex.Guis;
using Zeltex.Util;

namespace Zeltex.Guis.Maker
{
    [System.Serializable]
    public class SaveGame
    {
        public string LevelName;        // level that character was last in
        public string CharacterScript;  // A string version of our character
    }
    // Creating a new game:
    // First Terrain
    // it has just the seed input, for basic
    // Can add in Advanced Terrain Settings later
    // Game starts in a town, generate the town too and the maps

    /// <summary>
    /// Handles player saves, after begin game screen it comes here
    ///  Save game has:
    ///     Level I was last in (load it)
    ///     Characters I can play as (one for now)
    ///     [Future] Edits to the levels i've made
    ///     [Future] Level can be just be generation options (level blueprints)
    /// </summary>
    public class SavesMaker : GuiBasic
    {
        public List<string> MyNames;
        public static string DefaultLevelName = "SaveGame.sav";
        private string StartingLocation = "";

        #region ZelGui

        public override void OnBegin()
        {
            string MyFolderPath = DataManager.GetFolderPath(DataFolderNames.Saves + "/");    // get folder path
            string[] MyDirectories = Directory.GetDirectories(MyFolderPath);
            GuiList SavesList = GetListHandler("SavesList");
            Debug.Log("Saves folder path is:" + MyFolderPath + " with " + MyDirectories.Length + " Saves!");
            if (MyDirectories.Length != 0)
            {
                GetButton("PlayGameButton").interactable = true;
                SavesList.Clear();
                for (int i = 0; i < MyDirectories.Length; i++)
                {
                    string SaveGameName = Path.GetFileName(MyDirectories[i]);
                    SavesList.Add(SaveGameName);
                    MyNames.Add(SaveGameName);
                }
                SavesList.Select(0);
            }
            else
            {
                GetButton("PlayGameButton").interactable = false;
                GetButton("CancelCreationButton").interactable = false;
                BeginCreatingNewGame();
            }
        }

        #endregion

        #region UI

        public UnityEvent OnConfirm;
        public TabManager MyTabManager;

        public override void UseInput(InputField MyInputField)
        {
            if (MyInputField.name == "NameInput")
            {
                if (MyInputField.text == "")
                {
                    MyInputField.text = "Game" + Random.Range(1, 100000);
                }
            }
        }

        public override void UseInput(Button MyButton)
        {
            base.UseInput(MyButton);
            if (MyButton.name == "NewGameButton")
            {
                // go to new game tab!
                BeginCreatingNewGame();
            }
            else if (MyButton.name == "CancelCreationButton")
            {
                MyTabManager.EnableTab("SelectionTab");
            }
            else if (MyButton.name == "ConfirmCreationButton")
            {
                StartCoroutine(CreateNewGame());
            }
            else if (MyButton.name == "PlayGameButton")
            {
                // chose character tab!
                PlayGame();
                OnConfirm.Invoke();
            }
            else if (MyButton.name == "EraseGameButton")
            {
                EraseSelected();
                // are you sure tab! Yes, No!
            }
            else if (MyButton.name == "EraseYesButton")
            {
                // Delete the selected level
            }
            else if (MyButton.name == "EraseNoButton")
            {
                // Go Back to main tab!
            }
        }
        
        public override void UseInput(Dropdown MyDropdown)
        {
            if (MyDropdown.name == "LevelSelectionDropdown")
            {
                StartingLocation = MyDropdown.options[MyDropdown.value].text;
            }
            else if (MyDropdown.name == "ControllerDropdown")
            {
                PlayerManager.Get().SetController(MyDropdown.options[MyDropdown.value].text);
            }
        }
        #endregion

        #region Save

        /// <summary>
        /// Begin creating a new game
        /// </summary>
        private void BeginCreatingNewGame()
        {
            MyTabManager.EnableTab("CreationTab");
            GetInput("NameInput").text = "Game" + Random.Range(1, 100000);
        }

        /// <summary>
        /// Used when loading a saved game
        /// </summary>
        private void SetSaveGameName(string NewName)
        {
            Voxels.WorldManager.SaveGameName = NewName;
        }

        /// <summary>
        /// Creates a new game save file!
        /// </summary>
        private IEnumerator CreateNewGame()
        {
            GetComponent<ZelGui>().SetChildStates(false);
            GetButton("PlayGameButton").interactable = true;
            GuiList SavesList = GetListHandler("SavesList");
            string SaveGameName = GetInput("NameInput").text;
            string MyDirectory = DataManager.GetFolderPath(DataFolderNames.Saves + "/") + SaveGameName + "/";
            if (Directory.Exists(MyDirectory) == false)
            {
                Directory.CreateDirectory(MyDirectory);
                SavesList.Add(SaveGameName);
                MyNames.Add(SaveGameName);
            }
            else
            {
                Debug.LogError("wow... tried to create a duplicate savegame!");
            }
            SetSaveGameName(SaveGameName);
            // Go to Character Create Screen and Level Select
            // wait just pick first one for now...!

            string LevelName = "";
            List<string> LevelNames = DataManager.Get().GetNames(DataFolderNames.Levels);
            if (LevelNames.Count > 0)
            {
                LevelName = LevelNames[0];
            }
            string RaceName = DataManager.Get().GetName(DataFolderNames.Skeletons, 0);
            //string ClassName = DataManager.Get().GetName(DataFolderNames.Classes, 0);
            Level MyLevel = DataManager.Get().GetElement(DataFolderNames.Levels, LevelName) as Level;
            yield return Voxels.WorldManager.Get().LoadNewSaveGame(
                MyLevel, 
                RaceName, 
                "",
                StartingLocation);
            OnConfirm.Invoke();
            GuiSpawner.Get().DestroySpawn(gameObject);
        }

		/// <summary>
		/// Play the current game
		/// </summary>
        public void PlayGame()
        {
            // Load Characters list
            // Load Character
            GuiList MyList = GetListHandler("SavesList");
            if (MyList.GetSelected() >= 0 && MyList.GetSize() > 0)
            {
                string SaveGameName = MyList.GetSelectedName();
                string SaveGamePath = DataManager.GetFolderPath(DataFolderNames.Saves + "/") + SaveGameName + "/" + DefaultLevelName;
                string SaveGameScript = FileUtil.Load(SaveGamePath);
                // remove first line
                List<string> CharacterScript = FileUtil.ConvertToList(SaveGameScript);
                string LevelName = CharacterScript[0];
                CharacterScript.RemoveAt(0);
                LevelName = ScriptUtil.RemoveCommand(LevelName);    // get level name /Level [LevelName]
                string CharacterScriptSingle = FileUtil.ConvertToSingle(CharacterScript);
                // Load Level
                //Debug.LogError("Loading save game from: " + SaveGamePath + "\n" + SaveGameScript);
                //Debug.LogError("Loading Level: " + LevelName + " with:\n" + CharacterScriptSingle);
                Level MyLevel = DataManager.Get().GetElement(DataFolderNames.Levels, LevelName) as Level;
                if (MyLevel != null)
                {
                    GetComponent<ZelGui>().SetChildStates(false);
                    SetSaveGameName(SaveGameName);   // set the SaveGameName somewhere for when saving
                    Voxels.WorldManager.Get().LoadSaveGame(MyLevel, CharacterScriptSingle);
                    // Load Character using script
                    //Debug.LogError("Loading Character:\n" + CharacterScriptSingle);
                    GuiSpawner.Get().DestroySpawn(gameObject);
                }
                else
                {
                    Debug.LogError("Failure to find level: " + LevelName);
                    // go to default level now with character
                }
            }
        }

		/// <summary>
		/// Plays from the beginning level with a new character
		/// Loads new character gui
		/// </summary>
		private void PlayNewGame(string LevelName)
		{

		}

        private void EraseSelected()
        {
            GuiList MyList = GetListHandler("SavesList");
            if (MyList.GetSelected() >= 0 && MyList.GetSize() > 0)
            {
                string SaveGameName = MyList.GetSelectedName();
                string LevelPath = DataManager.GetFolderPath(DataFolderNames.Saves + "/") + SaveGameName + "/";
                if (Directory.Exists(LevelPath))
                {
                    Directory.Delete(LevelPath, true);
                }
                MyList.RemoveSelected();
            }
        }
        #endregion

    }
}