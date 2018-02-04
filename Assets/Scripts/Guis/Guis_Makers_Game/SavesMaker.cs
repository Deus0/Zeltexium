using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using MakerGuiSystem;
using Zeltex.Guis;
using Zeltex.Util;
using Zeltex.Guis.Maker;

namespace Zeltex.Saves
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
        public static string DefaultLevelName = "SaveGame.sav";
        private string StartingLocation = "";
        public UnityEvent OnConfirm;
        public TabManager MyTabManager;
        private SaveGame NewGame;
        public SaveGameViewer MySaveGameViewer;

        #region ZelGui

        private void Start()
        {
            Initiate();
        }

        public void Initiate()
        {
            /*string MyFolderPath = DataManager.GetFolderPath(DataFolderNames.Saves + "/");    // get folder path
            string[] MyDirectories = FileManagement.ListDirectories(MyFolderPath);*/
            GuiList SavesList = GetListHandler("SavesList");
            // Debug.Log("Saves folder path is:" + MyFolderPath + " with " + MyDirectories.Length + " Saves!");
            List<string> SaveGameNames = DataManager.Get().GetNames(DataFolderNames.Saves);
            SavesList.DeSelect();
            if (SaveGameNames.Count > 0)
            {
                SavesList.Clear();
                for (int i = 0; i < SaveGameNames.Count; i++)
                {
                    SavesList.Add(SaveGameNames[i]);
                }
                SavesList.Select(0);
                MySaveGameViewer.RefreshUI(DataManager.Get().GetElement(DataFolderNames.Saves, 0) as SaveGame);
                MyTabManager.EnableTab("ViewSaveGameTab");
            }
            else
            {
                GetButton("PlayGameButton").interactable = false;
                GetButton("CancelCreationButton").interactable = false;
                BeginCreatingNewGame();
                MySaveGameViewer.RefreshUI(null, true);
            }
        }

        #endregion

        #region UI

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
                BeginPlayingGame();
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

        public override void UseInput(GuiList MyList)
        {
            base.UseInput(MyList);
            GetButton("PlayGameButton").interactable = (MyList.GetSelected() >= 0);
            GetButton("EraseGameButton").interactable = (MyList.GetSelected() >= 0);
        }
        #endregion

        #region Save

        /// <summary>
        /// Begin creating a new game
        /// </summary>
        private void BeginCreatingNewGame()
        {
            // Name of save game
            NewGame = new SaveGame();
            NewGame.SetName(NameGenerator.GenerateVoxelName());
            GetInput("NameInput").text = NewGame.Name;

            Dropdown LevelsDropdown = GetDropdown("LevelSelectionDropdown");
            if (LevelsDropdown)
            {
                FillDropDownWithList(LevelsDropdown, DataManager.Get().GetNames(DataFolderNames.Levels));
                if (LevelsDropdown.options.Count > 0)
                {
                    StartingLocation = LevelsDropdown.options[LevelsDropdown.value].text;
                }
            }
            NewGame.SetLevel(StartingLocation);

            MyTabManager.EnableTab("CreationTab");
        }

        /// <summary>
        /// Creates a new game save file!
        /// </summary>
        private IEnumerator CreateNewGame()
        {
            NewGame.SetLevel(StartingLocation);
            GuiSpawner.Get().DestroySpawn("MainMenu");
            OnConfirm.Invoke();
            GetComponent<ZelGui>().TurnOff();
            GameManager.Get().MyLoadingGui.SetPercentage(0);
            //GetButton("PlayGameButton").interactable = true;
            DataManager.Get().AddElement(DataFolderNames.Saves, NewGame);
            DataManager.Get().SaveElement(NewGame);

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
                }
                else
                {
                    Debug.LogError("Tried to create a duplicate savegame [" + SaveGameName + "]");
                }
            }
            catch (DirectoryNotFoundException e)
            {
                Debug.LogError("DirectoryNotFoundException at: " + MyDirectory + "\n" + e.ToString());
            }
            //SetSaveGameName(SaveGameName);
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
            yield return UniversalCoroutine.CoroutineManager.StartCoroutine(
                Voxels.WorldManager.Get().LoadSaveGameRoutine(
                NewGame, 
                () =>
                {
                    ChunksLoaded++;
                    GameManager.Get().MyLoadingGui.SetPercentage( ChunksLoaded / MaxLoading);
                }));
            GameManager.Get().MyLoadingGui.SetPercentage(1f);

            for (int i = 0; i < 120; i++)
            {
                yield return null;
            }
            OnLoadedGame(NewGame);
            DataManager.Get().SaveElement(NewGame);
        }



        /// <summary>
        /// Play the game
        /// </summary>
        private void BeginPlayingGame()
        {
            GuiList MyList = GetListHandler("SavesList");
            int SelectedSaveGame = MyList.GetSelected();
            if (SelectedSaveGame >= 0)
            {
                MySaveGameViewer.RefreshUI(DataManager.Get().GetElement(DataFolderNames.Saves, SelectedSaveGame) as SaveGame);
                MyTabManager.EnableTab("ViewSaveGameTab");
            }
        }

        public void CancelSelectedSaveGame()
        {
            MyTabManager.EnableTab("SelectionTab");
        }

        public void EnterSaveGame()
        {
            UniversalCoroutine.CoroutineManager.StartCoroutine(PlayGameRoutine());
            OnConfirm.Invoke();
        }

		/// <summary>
		/// Play the current game
		/// </summary>
        public IEnumerator PlayGameRoutine()
        {
            // Load Characters list
            // Load Character
            GuiList MyList = GetListHandler("SavesList");
            if (MyList.GetSelected() >= 0 && MyList.GetSize() > 0)
            {
                string SaveGameName = MyList.GetSelectedName();
                SaveGame MyGame = DataManager.Get().GetElement(DataFolderNames.Saves, SaveGameName) as SaveGame;
                if (MyGame != null)
                {
                    GetComponent<ZelGui>().TurnOff();
                    GameManager.Get().MyLoadingGui.SetPercentage(0);
                    Level MyLevel = MyGame.GetLevel();// //DataManager.Get().GetElement(DataFolderNames.Levels, LevelName) as Level;

                    if (MyLevel != null)
                    {
                        float ChunksLoaded = 0;
                        float MaxLoading = (MyLevel.GetWorldSize().x * MyLevel.GetWorldSize().y * MyLevel.GetWorldSize().z)
                             + MyLevel.GetCharactersCount() - 2;
                        if (MyGame.CharacterName == "")
                        {
                            // if save game has no character
                            MaxLoading++;
                        }
                        MaxLoading += MyGame.GetLevel().GetCharactersCount();
                       // SetSaveGameName(SaveGameName);   // set the SaveGameName somewhere for when saving
                        yield return UniversalCoroutine.CoroutineManager.StartCoroutine(
                            Voxels.WorldManager.Get().LoadSaveGameRoutine(
                                MyGame,
                                () => {
                                    ChunksLoaded++;
                                    GameManager.Get().MyLoadingGui.SetPercentage(ChunksLoaded / MaxLoading);
                                }));
                        GameManager.Get().MyLoadingGui.SetPercentage(1f);

                        yield return null;
                        OnLoadedGame(MyGame);
                    }
                    else
                    {
                        Debug.LogError("Failure to find level: " + MyGame.LevelName);
                        // go to default level now with character
                        GameManager.Get().MyLoadingGui.TurnOff();
                    }
                }
                else
                {
                    Debug.LogError("Could not find save game with name: " + SaveGameName);
                }
            }
        }

        public void OnLoadedGame(SaveGame MyGame)
        {
            // Camera and possession
            if (MyGame.MyCharacter)
            {
                Camera MyCamera = CameraManager.Get().SpawnGameCamera();
                MyCamera.transform.SetParent(MyGame.MyCharacter.GetCameraBone());
                MyCamera.transform.localPosition = Vector3.zero;
                MyCamera.transform.localEulerAngles = Vector3.zero;
                CameraManager.Get().EnableGameCamera();
                Possess.PossessCharacter(MyGame.MyCharacter, MyCamera);
            }
            else
            {
                Debug.LogError("Save Game [" + MyGame.Name + "] has no character: " + MyGame.CharacterName);
            }

            List<Characters.Character> MyCharacters = Characters.CharacterManager.Get().GetSpawned();
            Debug.Log("Allowing all [" + MyCharacters.Count + "] bots to wander.");
            for (int i = 0; i < Characters.CharacterManager.Get().GetSize(); i++)
            {
                Characters.Character MyCharacter = MyCharacters[i];
                if (MyCharacter)
                {
                    AI.Bot MyBot = MyCharacter.gameObject.GetComponent<AI.Bot>();
                    if (MyBot == null && MyCharacter.IsPlayer == false)
                    {
                        MyBot = MyCharacter.gameObject.AddComponent<AI.Bot>();
                    }
                    if (MyBot)
                    {
                        MyBot.OnBeginGame();
                    }
                    else
                    {
                        Debug.LogError("Could not begin game for bot: " + MyCharacter.name);
                    }
                }
                else
                {
                    Debug.LogError("Could not begin game for bot: " + i);
                }
            }
            GameManager.Get().MyLoadingGui.TurnOff();
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
                DataManager.Get().RemoveElement(DataFolderNames.Saves, SaveGameName);
            }
        }
        #endregion

    }
}