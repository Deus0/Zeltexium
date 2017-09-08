using UnityEngine;
using System.Collections;
using Zeltex.Characters;
using System.Collections.Generic;
using System.IO;
using Zeltex.Util;
using Zeltex.AI;
using Zeltex.Voxels;
using Zeltex.Guis.Maker;

namespace Zeltex.Guis.Players
{
    /// <summary>
    /// Selecting a character to play out of a list.
    /// selectable are those spawned in the map.
    /// for bigger maps may need to expand this to ones inside directories.
    /// </summary>
    public class CharacterSelectGui : GuiList 
	{
		public bool IsRefreshingList = false;
		List<string> MyCharacters = new List<string>();
		List<string> LastGatheredCharacters = new List<string>();
        public GameObject ConfirmButton;
        public GameObject DeleteCharacterButton;
       // public PlayerSpawner MySpawner;

        string CharacterName = "";

        public void OnBegin()
        {
            SetButtons(false);
            ForceRefresh();
		}

		public override void Select(int NewSelectedIndex)
		{
            base.Select(NewSelectedIndex);
            SetButtons(true);
        }
        public void SetButtons(bool NewState)
        {
            if (NewState)
            {
                if (MyCharacters.Count == 0)
                    DeSelect();
                if (!IsGuiSelected()) // if turning them on make sure its selected
                    return;
            }
            ConfirmButton.SetActive(NewState);
            DeleteCharacterButton.SetActive(NewState);
        }
        public static List<string> GetCharactersListWorld(World MyWorld) 
		{
			return GetCharactersList (MapMaker.SaveFileName);
		}
        public static string GetFolderPath()
        {
            return DataManager.GetFolderPath("Characters");
        }
        public static List<string> GetCharactersList(string MyVoxels)
		{
			List<string> MyCharacters = new List<string> ();
            string MyFolderPath = GetFolderPath();
            Debug.Log("Getting characters from: " + MyFolderPath);
            DirectoryInfo MyFileDirectory = new DirectoryInfo (MyFolderPath);
			if (MyFileDirectory == null)
			{	// if there is no directory 
				Debug.Log("Directory of " + MyVoxels + " was not found.");
				return MyCharacters;
			}
			//var MyFiles = info.GetFiles();
			var MyFiles = MyFileDirectory.GetFiles ();
			for (int i = 0; i < MyFiles.Length; i++) 
			{
				if (MyFiles[i].Name.Contains(".chr") && !MyFiles[i].Name.Contains(".meta")) 
				{
					string NewCharacterName = MyFiles[i].Name.Replace(".chr", "");
					int MyChunkIndex = NewCharacterName.IndexOf(")")+1;
					NewCharacterName = NewCharacterName.Substring(MyChunkIndex);
					MyCharacters.Add(NewCharacterName);
				}
			}
			return MyCharacters;
		}

        void CheckEvents()
        {
           // if (OnSelectEvent.GetPersistentEventCount() == 0)
            {
                // RefreshListeners();
                //OnSelectEvent.AddEvent(delegate { SetButtons(true); });
                OnActivateEvent.RemoveAllListeners();
                OnActivateEvent.AddEvent(SpawnPlayer);
            }
        }
		// Handles updating and only update what has changed!
		override public void RefreshList() 
		{
			Debug.Log ("Refreshing Characters List. At " + Time.time + ".");
            StartRefresh();
        }

        override public void StartRefresh()
        {
            Debug.Log("Refreshing list. At " + Time.time + ".");
            if (!IsRefreshingList)
            {
                IsRefreshingList = true;
                CheckEvents();
                StopCoroutine(OnGoingRefresh());
                StartCoroutine(OnGoingRefresh());
            }
        }
        override public void StopRefresh()
        {
            StopCoroutine(OnGoingRefresh());
            IsRefreshingList = false;
        }

        public List<string> GetPlayableCharacters()
        {
            List<string> MyCharacterNames = new List<string>();
            List<Character> MyCharacters = CharacterManager.Get().GetSpawned();
            for (int i = 0; i < MyCharacters.Count; i++)
            {
                if (MyCharacters[i])
                {
                    MyCharacterNames.Add(MyCharacters[i].name);
                    /*List<string> MyPermissions = MyCharacters[i].GetComponent<Character>().PlayerPermissions;
                    for (int j = 0; j < MyPermissions.Count; j++)
                    {
                        if (PhotonNetwork.player.name == MyPermissions[j])
                        {
                            MyCharacterNames.Add(MyCharacters[i].name);
                        }
                    }*/
                }
            }
            return MyCharacterNames;
        }

        // on going rrefresh of characters in map while this list is open
        IEnumerator OnGoingRefresh() 
		{
			//Zeltex.Voxels.World MyWorld = GameObject.FindObjectOfType<Zeltex.Voxels.World>();
			while (transform.parent.gameObject.activeSelf)
			{
				//MyCharacters = GetCharactersListWorld(MyWorld);
				//MyCharacters = CharacterManager.GatherCharacterNames();
                MyCharacters = GetPlayableCharacters();
				if (!FileUtil.AreListsTheSame(MyCharacters, LastGatheredCharacters)) 
				{
					//Debug.LogError (Time.time + " Refreshing Character List.");
					Clear ();
                    SetButtons(false);
                    DeSelect();
                    for (int i = 0; i < MyCharacters.Count; i++)
					{
                        Add(MyCharacters[i]);
                    }
                    // make sure to refresh buttons
                    SetButtons(true);   // will only be true if gui selected and list count is greater then 0
                }
				LastGatheredCharacters = MyCharacters;
				yield return new WaitForSeconds (2f);
			}
			IsRefreshingList = false;
		}
        
        public void SpawnPlayer(int CharacterNameIndex)
        {
            CharacterName = SelectedName;
        }
        // called when game match starts
        public void SpawnPlayer()
        {
            if (CharacterName != "")
            {
                Debug.LogError("Converting character: " + CharacterName + " to a player.");
                //Transform MyCharacter = GetSelectedCharacter(CharacterName);
                Transform MyCharacter = GetSelectedCharacter();
                //Debug.Log ("Updating Character as Player: " + CharacterName);
                if (MyCharacter)
                {
                    // Convert to player
                    //MyCharacter.GetComponent<Character>().ConvertToPlayer();
                }
                else
                {
                    Debug.LogError("Could not find player spawner in scene. It might not of loaded yet.");
                    // maybe forcefully load the character!
                }
            }
        }
        public void OnCreateCharacter()
        {
            CharacterName = ""; // don't spawn a character
        }
        // called on confirm button
        void SpawnPlayer(string NewCharacterName)
		{
            CharacterName = NewCharacterName;
		}

		public static Transform GetSelectedCharacter(string CharacterName) 
		{
			GameObject MyCharacter = GameObject.Find(CharacterName);
			if (MyCharacter)
				return MyCharacter.transform;
			else
				return null;
		}

		/*public static string GetCharacterFileName(Transform MyTransform)
		{
			GameObject MyWorld = GameObject.Find ("World");
			Vector3 ChunkPosition = Zeltex.Voxels.World.GetChunkPosition(MyWorld.GetComponent<Zeltex.Voxels.World>(),transform.position);
			return ChunkPosition.ToString() + MyTransform.name + ".chr";
		}*/

        public void CameraMoveTo()
        {
            Transform MyCharacter = GetSelectedCharacter();
            Camera.main.gameObject.GetComponent<CameraMoveTo>().MoveTo(MyCharacter);
        }
        
		public void DeleteCharacterFiles(string WorldName, string CharacterName) 
		{
            string FolderPath = GetFolderPath();
            Debug.Log("Deleting files from " + FolderPath);
            if (Directory.Exists(FolderPath))
            {
                DirectoryInfo MyFileDirectory = new DirectoryInfo(FolderPath);
                FileInfo[] MyFiles = MyFileDirectory.GetFiles();
                for (int i = 0; i < MyFiles.Length; i++)
                {
                    if (MyFiles[i].Name.Contains(CharacterName + ".chr"))// && !MyFiles[i].Name.Contains(".meta")) 
                    {
                        //Debug.LogError("Deleting file: " + MyFiles [i].FullName);
                        File.Delete(MyFiles[i].FullName);
                    }
                }
            }
		}
        public Transform GetSelectedCharacter()
        {
            string CharacterName = GetSelectedName();
            GameObject MyCharacter = GameObject.Find(CharacterName);
            return MyCharacter.transform;
        }
		public void RemovePlayer() 
		{
            //if (MyWorld == null)
			//	return;
            Transform MyCharacter = GetSelectedCharacter();

            //string CharacterName = GetSelectedGuiName ();
            //string MyWorldName = MyWorld.GetComponent<Zeltex.Voxels.VoxelSaver>().SaveFileName;
            Debug.Log ("Deleting character: " + CharacterName);

			//GameObject MyCharacter = GameObject.Find(CharacterName);
			Debug.Log ("Did Find: " + (MyCharacter != null));
			if (MyCharacter)
            {
                //MyCharacter.GetComponent<Character>().Delete();
                Destroy (MyCharacter.gameObject);
                // refresh list
                RefreshList();
            }
		}
	}
}