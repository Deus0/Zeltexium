using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Zeltex.Guis;
using Zeltex.Util;
using System.IO;
using Zeltex.Voxels;
using Zeltex.Characters;
using Zeltex.Items;
using ZeltexTools;
using Zeltex;

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// Make and edit levels!
	/// TODO:
	///		Spawn a world, away from game world, and edit it in the viewer
	///		Swap between main camera and viewer cameras
    /// </summary>
    public class LevelMaker : GuiBasic
    {
        #region Variables
        public static string FileExtension = "chn";
        public static string CharacterFileExtension = "chr";
        private bool IsCloseOnLoad = false;
        private string LoadedLevelName = "";
        [Header("Reference")]
        private World MyWorld;
        public ObjectViewer MyViewer;   // the level viewer
        Vector3 OriginalCameraPosition;
        [SerializeField]
        private Character MyPlayer;
        [SerializeField]
        private TabManager MyTabManager;
        #endregion

        #region ZelGui

        public override void OnBegin()
        {
            MyWorld = WorldManager.Get().ConvertToWorld(MyViewer.GetSpawn());
            RefreshList();
            Load();
            VoxelFreeRoam MyFreeRoam = MyViewer.GetRenderCamera().gameObject.AddComponent<VoxelFreeRoam>();
            MyFreeRoam.MyWorld = MyViewer.GetSpawn().GetComponent<World>();
            MyFreeRoam.enabled = false;
            MyFreeRoam.BeginRoaming();
        }

        public override void OnEnd()
        {
            WorldManager.Get().ReturnObject(MyWorld);
            Clear();
        }

        public static List<string> GetNames()
        {
            string MyFolderPath = DataManager.GetFolderPath(DataFolderNames.Levels + "/");
            string[] MyInfo = Directory.GetDirectories(MyFolderPath);
            List<string> MyNames = new List<string>();
            MyNames.AddRange(MyInfo);
            return MyNames;
        }

        public void RefreshList()
        {
            string MyFolderPath = DataManager.GetFolderPath(DataFolderNames.Levels + "/");    // get folder path
            string[] MyInfo = Directory.GetDirectories(MyFolderPath);
            Debug.Log("Levels folder path is:" + MyFolderPath + " with " + MyInfo.Length + " Levels!");
            GuiList MyList = GetListHandler("LevelsList");
            MyList.Clear();
            MyList.AddRange(DataManager.Get().GetNames(DataFolderNames.Levels));
            MyList.Select(0);
        }
        #endregion

        #region Pathing

        public string GetSelectedName()
        {
            return GetListHandler("LevelsList").GetSelectedName();
        }

        public string GetLoadedPath()
        {
            string MyLevel = LoadedLevelName;// MyLoadedLevel.text;// MyGuiList.GetSelectedGuiName();
            return DataManager.GetFolderPath(DataFolderNames.Levels + "/") + MyLevel + "/";
        }

        public string GetSelectedPath()
        {
            string MyLevel = GetListHandler("LevelsList").GetSelectedName();
            return DataManager.GetFolderPath(DataFolderNames.Levels + "/") + MyLevel + "/";
        }

        public string GetFilePath(Chunk MyChunk)
        {
            return GetLoadedPath() + "Chunk_" + MyChunk.Position.x + "_" + MyChunk.Position.y + "_" + MyChunk.Position.z + "." + FileExtension;
        }

		/// <summary>
		/// 
		/// </summary>
        public string GetMetaPath()
        {
            return GetLoadedPath() + LoadedLevelName + ".wmt";
        }
        #endregion

        #region WorldSpawning

        private void Update()
        {
            if (MyPlayer && Input.GetKeyDown(KeyCode.Escape))
            {
                StopPlaying();
            }
        }

        public void PlayLevel()
        {
            OriginalCameraPosition = Camera.main.transform.position;
            Vector3 SpawnPosition = SpawnPositionFinder.FindNewPosition(MyWorld);
            MyPlayer = WorldManager.SpawnPlayer(SpawnPosition, MyWorld);
            SetChildren(false);
        }

        public void PlayFromCameraPosition()
        {
            OriginalCameraPosition = Camera.main.transform.position;
            MyPlayer = WorldManager.SpawnPlayer(MyViewer.GetRenderCamera().transform.position, MyWorld);
            SetChildren(false);
        }

        private void SetChildren(bool NewState)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(NewState);
            }
        }

        public void StopPlaying()
        {
            if (MyPlayer != null)
            {
                Debug.LogError("Stop level maker playing state.");
                // detatch camera
                Possess[] MyControllers = Camera.main.gameObject.GetComponents<Possess>();
                for (int i = 0; i < MyControllers.Length; i++)
                {
                    if (MyControllers[i].enabled)
                    {
                        MyControllers[i].SetCharacter(null);    // un possess
                        break;
                    }
                }
                Camera.main.transform.position = OriginalCameraPosition;
                GetComponent<Orbitor>().TeleportToTarget();
                SetChildren(true);
                //ObjectViewer.SetLayerRecursive(transform.gameObject, MyNormalLayerMask);
                Camera.main.cullingMask = LayerManager.Get().ViewerLayer;
                Billboard.IsLookAtMainCamera = false;
                MyPlayer.OnDeath();
				MyPlayer = null;
			}
        }
		#endregion

		#region New

		public void Add()
        {
            string NewLevelName = "Level" + Mathf.RoundToInt(Random.Range(1, 10000));
			SetLoadedLevelName(NewLevelName);
            string MyFolderPath = DataManager.GetFolderPath(DataFolderNames.Levels + "/");
            MyFolderPath += NewLevelName + "/";
            Directory.CreateDirectory(MyFolderPath);
            GetListHandler("LevelsList").Add(NewLevelName);
            StartCoroutine(AddRoutine());
        }

        private IEnumerator AddRoutine()
        {
            SetButtons(false);
			// and now modify map!
			GetInput("InputSizeX").text = "3";
			GetInput("InputSizeY").text = "2";
			GetInput("InputSizeZ").text = "3";
            yield return MyWorld.SetWorldSizeRoutine(new Int3(3, 2, 3));
            //yield return MyTerrainGenerator.CreateTerrainRoutine(0f);
            yield return SaveRoutine();
            SetButtons(true);
        }
        #endregion

        #region Clear

        /// <summary>
        /// Unload the level !
        /// </summary>
        private void Clear()
        {
            MyWorld.SetWorldSize(Vector3.zero);
            ClearMap();
            GetListHandler("LevelsList").Select(-1);
			SetLoadedLevelName("");
        }

        /// <summary>
        /// Clear map data
        /// </summary>
        private void ClearMap()
        {
            // Clear map extras
            CharacterManager.Get().Clear();
            ItemManager.Get().Clear();
        }
        #endregion

        #region Loading

		private void SetLoadedLevelName(string NewLevelName)
		{
			LoadedLevelName = NewLevelName;
			GetLabel("LoadedLevelName").text = NewLevelName;
			GetInput("NameInput").text = NewLevelName;
			GetInput("NameInput").interactable = (NewLevelName == "");
		}

        /// <summary>
        /// Load the level !
        /// </summary>
        public void Load()
        {
            StopCoroutine(LoadLevel());
            StartCoroutine(LoadLevel());
        }

        private IEnumerator LoadLevel()
        {
            SetButtons(false);
            MyTabManager.EnableTab("Loading");
            ClearMap();
            Level MyLevel = DataManager.Get().GetElement(DataFolderNames.Levels, GetListHandler("LevelsList").GetSelected()) as Level;
            //string LevelName = GetListHandler("LevelsList").GetSelectedName();
            WorldManager.Get().IsDisableCharactersOnLoad = true;
            yield return MyLevel.LoadLevel(MyWorld);
            WorldManager.Get().IsDisableCharactersOnLoad = false;
            SetLoadedLevelName(MyLevel.Name);
            OnFinishLoading();
            // Change tab
            MyTabManager.EnableTab("Level");
        }

		/// <summary>
		/// when finished loading the level
		/// </summary>
        private void OnFinishLoading()
        {
            if (GetInput("InputSizeX"))
            {
                GetInput("InputSizeX").text = "" + MyWorld.GetWorldSizeChunks().x;
                GetInput("InputSizeY").text = "" + MyWorld.GetWorldSizeChunks().y;
                GetInput("InputSizeZ").text = "" + MyWorld.GetWorldSizeChunks().z;
            }
            SetButtons(true);
			// if in game!
            if (IsCloseOnLoad)
            {
                IsCloseOnLoad = false;
                ZelGui MyZelGui = GetComponent<ZelGui>();
                MyZelGui.SetChildStates(true);
                MyZelGui.TurnOff();
                //MyGuiManager.GoTo("CharacterMaker");
            }
        }

        #endregion

        #region Saving

		private bool IsLevelLoaded()
		{
			return (LoadedLevelName != "None" && LoadedLevelName != "");
		}

        public void Save()
        {
            if (IsLevelLoaded())
            {
                StartCoroutine(SaveRoutine());
            }
        }


        public string GetFilePath(GameObject MyCharacter)
        {
            Int3 ChunkPosition = MyWorld.GetChunkPosition(MyCharacter.transform.position);
            return GetLoadedPath() + "Character_" + MyCharacter.name +
                 "_Chunk_" + ChunkPosition.x + "_" + ChunkPosition.y + "_" + ChunkPosition.z +
                 "." + CharacterFileExtension;
        }

        private IEnumerator SaveRoutine()
        {
            SetButtons(false);	// disable while saving
            List<string> MyMetaData = MyWorld.GetScriptMeta();
            string MetaPath = GetMetaPath();
            FileUtil.Save(MetaPath, FileUtil.ConvertToSingle(MyMetaData));

            // Saves any chunks that have changed in the world
            foreach (Int3 MyKey in MyWorld.MyChunkData.Keys)
            {
                Chunk MyChunk = MyWorld.MyChunkData[MyKey];
                //if (MyChunk.GetHasSaved() == false)
                {
                    MyChunk.OnSaved();
                    string MyFilePath = GetFilePath(MyChunk);
					string MyScriptSingle = FileUtil.ConvertToSingle(MyChunk.GetScript());
					Debug.Log("Saving chunk at location: " + MyFilePath);
                    FileUtil.Save(MyFilePath, MyScriptSingle);
                }
			}
			Debug.Log("Finished saving " + MyWorld.MyChunkData.Count + " Chunks in world " + MyWorld.name);

			// Save any characters that have changed in the world
			List<Character> MyCharacters = CharacterManager.Get().GetSpawned();
            /*for (int i = 0; i < MyCharacters.Count; i++)
            {
                List<string> MyScript = MyCharacters[i].GetScript();
                string MyFilePath = GetFilePath(MyCharacters[i].gameObject);
                float TimeBegin = Time.realtimeSinceStartup;
                string MyScriptSingle = FileUtil.ConvertToSingle(MyScript);
                FileUtil.Save(MyFilePath, MyScriptSingle);
                Debug.Log("Saved [" + MyCharacters[i].name + "], Taken [" + (Time.realtimeSinceStartup - TimeBegin) + "]");
                yield return null;
            }*/
            yield return null;
            Debug.Log("Finished saving " + MyCharacters.Count + " characters in world " + MyWorld.name);
            SetButtons(true);
        }
        #endregion

        #region Delete

		/// <summary>
		/// Delete the current level
		/// </summary>
        public void Delete()
        {
			string SelectedName = GetListHandler("LevelsList").GetSelectedName();
			if (SelectedName != "")
			{
				string MyFolderPath = GetSelectedPath();
				Debug.Log("Deleting Level from path: " + MyFolderPath);
				if (Directory.Exists(MyFolderPath))
				{
					Directory.Delete(MyFolderPath, true);
					GetListHandler("LevelsList").RemoveSelected();
				}
				if (IsLevelLoaded() && LoadedLevelName == SelectedName)
				{
					Clear();
				}
			}
            //StartCoroutine(RefreshList());
        }

        public void DeleteCharacter(Character MyCharacter)
        {
			MyCharacter.OnDeath();
			//CharacterManager.Get().Remove(MyCharacter);
            string MyFilePath = GetFilePath(MyCharacter.gameObject);
            Debug.Log("Deleting Character at path: " + MyFilePath);
            if (File.Exists(MyFilePath))
            {
                FileUtil.Delete(MyFilePath);
            }
        }
        #endregion

        #region UI

        /// <summary>
        /// Generic UseInput function
        /// </summary>
        public override void UseInput(Button MyButton)
        {
            if (MyButton.name == "Play")
            {
                PlayLevel();
			}
			else if (MyButton.name == "LoadButton")
			{
				Load();
			}
			else if (MyButton.name == "SaveButton")
			{
				Save();
			}
			else if (MyButton.name == "AddButton")
			{
				Add();
			}
			else if (MyButton.name == "DeleteButton")
			{
				Delete();
            }
            else if (MyButton.name == "TerrainButton")
            {
                WorldManager.Get().GetComponent<VoxelTerrain>().CreateTerrainWorld(MyWorld);
            }
            else if (MyButton.name == "FlatlandButton")
            {
                WorldManager.Get().GetComponent<VoxelTerrain>().CreateFlatLand(MyWorld);
            }
        }

        /// <summary>
        /// Input field
        /// </summary>
        public override void UseInput(InputField MyInput)
        {
            if (MyInput.name == "NameInput")
            {
				string OldLevelName = GetLabel("LoadedLevelName").text;
				string NewLevelName = MyInput.text;

				if (NewLevelName != "" && NewLevelName != OldLevelName)
                {
                    string MyFolderPath = DataManager.GetFolderPath(DataFolderNames.Levels + "/");
                    MyFolderPath += OldLevelName + "/";
                    string NewFolderPath = DataManager.GetFolderPath(DataFolderNames.Levels + "/") + NewLevelName + "/";
                    if (Directory.Exists(MyFolderPath) == true && Directory.Exists(NewFolderPath) == false)
                    {
                        string FileName = MyFolderPath + OldLevelName + ".wmt";   // rename meta file
                        if (File.Exists(FileName))
                        {
                            string FileName2 = MyFolderPath + NewLevelName + ".wmt";   // rename meta file
                            File.Move(FileName, FileName2);
                        }
                        Directory.Move(MyFolderPath, NewFolderPath);
                        // Rename the wmt file
                        GetListHandler("LevelsList").Rename(OldLevelName, NewLevelName);
                        LoadedLevelName = NewLevelName;
						GetLabel("LoadedLevelName").text = NewLevelName;
                    }
                    else
                    {
                        MyInput.text = OldLevelName;
                    }
                }
            }
        }

        /// <summary>
        /// Turn off the level list handler buttons
        /// </summary>
        private void SetButtons(bool NewState)
        {
            GetButton("SaveButton").interactable = NewState;
			GetButton("LoadButton").interactable = NewState;
			GetButton("AddButton").interactable = NewState;
			GetButton("DeleteButton").interactable = NewState;
			GetButton("ResizeButton").interactable = NewState;
        }
        #endregion

        #region Utility

        /// <summary>
        /// Called by level maker
        /// </summary>
        public void Resize()
        {
            float SizeX = float.Parse(GetInput("InputSizeX").text);
            float SizeY = float.Parse(GetInput("InputSizeY").text);
            float SizeZ = float.Parse(GetInput("InputSizeZ").text);
            MyWorld.SetWorldSize(new Vector3(SizeX, SizeY, SizeZ));
        }
        #endregion
    }
}

/*

        public IEnumerator ConvertToSingle(List<string> MyList, int DelayModulus, float DelayTime)
        {
            string MyOutput = "";
            for (int j = 0; j < MyList.Count; j++)
            {
                MyOutput += MyList[j] + '\n';
                if (j % DelayModulus == 0)
                {
                    yield return new WaitForSeconds(DelayTime);
                }
            }
            yield return MyOutput;
        }

        public IEnumerator ConvertToList(string MyScript, int DelayModulus, float DelayTime)
        {
            float TimeBegin = Time.realtimeSinceStartup;
            List<string> MyOutput = new List<string>();
            string LatestInput = "";
            for (int i = 0; i < MyScript.Length; i++)
            {
                if (MyScript[i] == '\n')
                {
                    MyOutput.Add(LatestInput);
                    LatestInput = "";
                }
                else
                {
                    LatestInput += MyScript[i];
                }
                if (i % DelayModulus == 0)
                {
                    yield return new WaitForSeconds(DelayTime);
                }
            }
            Debug.Log("Converted to list - Taken: " + (Time.realtimeSinceStartup - TimeBegin));
            yield return MyOutput;
        }

    */