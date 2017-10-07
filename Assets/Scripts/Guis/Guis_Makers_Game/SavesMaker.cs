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
            string[] MyDirectories = FileManagement.ListDirectories(MyFolderPath);
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
                UniversalCoroutine.CoroutineManager.StartCoroutine(CreateNewGame());
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

        private SaveGame NewGame;

        /// <summary>
        /// Begin creating a new game
        /// </summary>
        private void BeginCreatingNewGame()
        {
            MyTabManager.EnableTab("CreationTab");
            GetInput("NameInput").text = "New Game";
            NewGame = new SaveGame();
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
            GuiSpawner.Get().DestroySpawn("MainMenu");
            OnConfirm.Invoke();
            GetComponent<ZelGui>().TurnOff();
            LoadingGui.Get().SetPercentage(0);
            LoadingGui.Get().MyZel.TurnOn();
            //GetButton("PlayGameButton").interactable = true;
            DataManager.Get().AddElement(DataFolderNames.Saves, NewGame as Element);

            GuiList SavesList = GetListHandler("SavesList");
            string SaveGameName = GetInput("NameInput").text;
            string MyDirectory = DataManager.GetFolderPath(DataFolderNames.Saves + "/") + SaveGameName + "/";
            Debug.Log("Creating New Save Game at: [" + MyDirectory + "]");
            try
            {
                if (FileManagement.DirectoryExists(MyDirectory, true, true) == false)
                {
                    FileManagement.CreateDirectory(MyDirectory, true);
                    SavesList.Add(SaveGameName);
                    MyNames.Add(SaveGameName);
                }
                else
                {
                    Debug.LogError("Tried to create a duplicate savegame [" + SaveGameName + "]");
                }
            }
            catch (System.IO.DirectoryNotFoundException e)
            {
                Debug.LogError("DirectoryNotFoundException at: " + MyDirectory);
            }
            SetSaveGameName(SaveGameName);
            // Go to Character Create Screen and Level Select
            // wait just pick first one for now...!
            float ChunksLoaded = 0;
            float MaxLoading = (NewGame.GetLevel().GetWorldSize().x * NewGame.GetLevel().GetWorldSize().y * NewGame.GetLevel().GetWorldSize().z)
                 + NewGame.GetLevel().GetCharactersCount() - 2;
            if (NewGame.CharacterName == "")
            {
                // if save game has no character
                MaxLoading++;
            }
            MaxLoading += NewGame.GetLevel().GetCharactersCount();
            yield return UniversalCoroutine.CoroutineManager.StartCoroutine(Voxels.WorldManager.Get().LoadNewSaveGame(
                NewGame, 
                () =>
                {
                    ChunksLoaded++;
                    LoadingGui.Get().SetPercentage( ChunksLoaded / MaxLoading);
                }));
            LoadingGui.Get().SetPercentage(1f);
            Camera MyCamera = CameraManager.Get().SpawnGameCamera();
            if (NewGame.MyCharacter)
            {
                MyCamera.transform.SetParent(NewGame.MyCharacter.GetCameraBone());
            }
            MyCamera.transform.localPosition = Vector3.zero;
            MyCamera.transform.localEulerAngles = Vector3.zero;
            CameraManager.Get().EnableGameCamera();
            Possess.PossessCharacter(NewGame.MyCharacter, MyCamera);

            NewGame.Save();

            for (int i = 0; i < 120; i++)
            {
                yield return null;
            }
            LoadingGui.Get().MyZel.TurnOff();
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
                GuiSpawner.Get().DestroySpawn(gameObject);
                string SaveGameName = MyList.GetSelectedName();
                SaveGame MyGame = DataManager.Get().GetElement(DataFolderNames.Saves, SaveGameName) as SaveGame;

                //string SaveGamePath = DataManager.GetFolderPath(DataFolderNames.Saves + "/") + SaveGameName + "/" + DefaultLevelName;
                //string SaveGameScript = FileUtil.Load(SaveGamePath);
                // remove first line
                //List<string> CharacterScript = FileUtil.ConvertToList(SaveGameScript);
                //string LevelName = CharacterScript[0];
                //CharacterScript.RemoveAt(0);
                //LevelName = ScriptUtil.RemoveCommand(LevelName);    // get level name /Level [LevelName]
                //string CharacterScriptSingle = FileUtil.ConvertToSingle(CharacterScript);
                // Load Level
                //Debug.LogError("Loading save game from: " + SaveGamePath + "\n" + SaveGameScript);
                //Debug.LogError("Loading Level: " + LevelName + " with:\n" + CharacterScriptSingle);
                Level MyLevel = MyGame.GetLevel();// //DataManager.Get().GetElement(DataFolderNames.Levels, LevelName) as Level;

                if (MyLevel != null)
                {
                    SetSaveGameName(SaveGameName);   // set the SaveGameName somewhere for when saving
                    Voxels.WorldManager.Get().LoadSaveGame(MyGame);// MyLevel, CharacterScriptSingle);
                    // Load Character using script
                    //Debug.LogError("Loading Character:\n" + CharacterScriptSingle);
                }
                else
                {
                    Debug.LogError("Failure to find level: " + MyGame.LevelName);
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