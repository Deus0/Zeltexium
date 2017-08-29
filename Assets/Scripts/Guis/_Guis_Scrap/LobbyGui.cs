using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Guis;
using Zeltex.Util;

/*namespace Zeltex.Guis.Players
{
    /// <summary>
    /// Used to show a list of servers that the client can connect to.
    /// </summary>
	public class LobbyGui : GuiList
    {
        #region Variables
        //public Transform LevelSelectGui;
        //public Transform CharacterSelectGui;
        public PlayerGui MyPlayerGui;
        [Header("UI")]
        public GameObject ConnectingLabel;
        public GameObject JoinServerButton;
        public GameObject CreateServerButton;
        // Privates
        private string Version = "Version 1";
        private string RoomName = "Room";
        private List<string> MyRooms = new List<string>();
		private List<string> LastGatheredRooms = new List<string>();
		private bool IsRefreshingList = false;
        private int MaxWaitTime = 10;
        private int WaitedTime = 0;
        #endregion

        public void NextGui()
        {
            if (PhotonNetwork.offlineMode == true)  // if offline mode, switch this gui
            {
                CreateOfflineRoom();
                MyPlayerGui.GoTo("MapSelect");
            }
            else // otherwise open this gui
            {
                MyPlayerGui.GoTo("Lobby");
            }
        }

        #region GuiList
        /// <summary>
        /// Gets called by ZelGui on begin event
        /// </summary>
        public void OnBegin()   // starts joining lobby
        {
            JoinServerButton.SetActive(false);
            CreateServerButton.gameObject.SetActive(false);
            ConnectingLabel.SetActive(true);
            ForceRefresh();
        }
        public List<string> GetServerRooms()
        {
            return MyRooms;
        }

        /// <summary>
        /// Updates buttons depending on list states
        /// </summary>
        void RefreshJoinServerButton()
        {
            if (MyRooms.Count > 0)
            {
                if (MyList.activeSelf)
                {
                    JoinServerButton.SetActive(true);
                }
            }
            else
            {
                JoinServerButton.SetActive(false);
            }
        }

        void CheckEvents()
        {
            if (OnActivateEvent.GetPersistentEventCount() == 0)
            {
                OnActivateEvent.AddEvent(JoinRoom);	// when click go it will join the room
            }
        }
        override public void StartRefresh()
        {
            if (!IsRefreshingList)
            {
                IsRefreshingList = true;
                CheckEvents();
                // first clear any servers i have in the list
                Clear();
                StartCoroutine(JoinLobby());
                StartCoroutine(OnGoingRefresh());
            }
        }
        override public void StopRefresh()
        {
            StopCoroutine(OnGoingRefresh());
            StopCoroutine(JoinLobby());
            IsRefreshingList = false;
        }
        // Handles updating and only update what has changed!
        override public void RefreshList()
        {
            StartRefresh();
        }
        /// <summary>
        /// keeps refreshing the server list
        /// </summary>
        IEnumerator OnGoingRefresh()
        {
            while (!PhotonNetwork.insideLobby)
            {
                yield return new WaitForSeconds(1f);
            }
            while (transform.parent.gameObject.activeSelf && PhotonNetwork.insideLobby)  // keep checking rooms when in lobby
            {
				MyRooms = GetRoomsList ();
				if (!FileUtil.AreListsTheSame(MyRooms,LastGatheredRooms)) 
				{
                    //Debug.Log("Refreshing Servers List. At [" + Time.realtimeSinceStartup + "]");
					Clear ();
                    RefreshJoinServerButton();
					for (int i = 0; i < MyRooms.Count; i++) 
					{
                        yield return null;
						Add(MyRooms [i]);
                    }
                }
				LastGatheredRooms = MyRooms;
				yield return new WaitForSeconds (1f);
			}
			IsRefreshingList = false;
        }
        #endregion

        #region UserInput
        /// <summary>
        /// Updates the room used for the server
        /// </summary>
        /// <param name="NewName"></param>
        public void UpdateRoomName(string NewName)
        {
            RoomName = NewName;
        }
        /// <summary>
        /// Used with an offline button
        /// </summary>
        /// <param name="NewState"></param>
        public void SetOfflineMode(bool NewState)
        {
            PhotonNetwork.offlineMode = NewState;
        }
        #endregion

        #region PhotonFunctions
        /// <summary>
        /// joins the lobby
        /// </summary>
        public void OnJoinedLobby()
        {
            Debug.Log("Joining Lobby: " + PhotonNetwork.lobby.Name + " at " + Time.time);
            // reactivate main buttons
            if (MyList.activeSelf)
            {
                CreateServerButton.SetActive(true);
            }
            ConnectingLabel.SetActive(false);
        }
        #endregion

        #region PhotonNetworking
        public void CreateOfflineRoom()
        {
            PhotonNetwork.CreateRoom("Offline");
        }
        /// <summary>
        /// Gets a list of server rooms
        /// </summary>
        public static List<string> GetRoomsList()
        {
            List<string> MyRooms = new List<string>();

            foreach (RoomInfo game in PhotonNetwork.GetRoomList())
            {
                if (game.visible)
                    MyRooms.Add(game.name);
            }
            return MyRooms;
        }
        /// <summary>
        /// joins a photon room
        /// </summary>
        public void JoinRoom(int RoomIndex)
        {
            string RoomName = SelectedName;
            Debug.Log("Joining Room: " + RoomName + " at " + Time.time);
            PhotonNetwork.JoinRoom(RoomName);
        }
        /// <summary>
        /// Connects to photon network, then joins a lobby.
        /// </summary>
        private IEnumerator JoinLobby()
        {
            WaitedTime = 0;
            if (!PhotonNetwork.connected)
            {
                PhotonNetwork.ConnectUsingSettings(Version);
            }
            while (!PhotonNetwork.connected)
            {
                yield return new WaitForSeconds(1f);
                WaitedTime++;
                if (WaitedTime >= MaxWaitTime)
                {
                    yield break;
                }
            }
            PhotonNetwork.JoinLobby();
            Debug.Log("Finished Joined Lobby at [" + Time.realtimeSinceStartup + "]");
        }
        /// <summary>
        /// Creates a networking room
        /// </summary>
        public void CreateRoom()
        {
            Debug.Log("Creating room: " + RoomName);
            ExitGames.Client.Photon.Hashtable MyRoomProperties =
                new ExitGames.Client.Photon.Hashtable() { { "Map", "NewMap" } };
            string[] NewProperties = new string[] { "Map" };
            RoomOptions MyRoomOptions = new RoomOptions();
            MyRoomOptions.customRoomProperties = MyRoomProperties;
            MyRoomOptions.customRoomPropertiesForLobby = NewProperties;
            PhotonNetwork.CreateRoom(RoomName, MyRoomOptions, null);
        }
        /// <summary>
        /// Disconnects from the room
        /// leaves the lobby
        /// disconnects from server
        /// </summary>
        public void Disconnect()
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LeaveLobby();
            PhotonNetwork.Disconnect();
        }

        #endregion
    }
}*/
