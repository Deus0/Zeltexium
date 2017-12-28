using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Skeletons;
using MakerGuiSystem;
using Zeltex.Voxels;
using Zeltex.Util;
using Zeltex.Guis.Maker;
/*
namespace MakerGuiSystem
{ 
    
    /// <summary>
    /// Downloads the map from the host inside the client
    ///     Sends messages to the host
    ///     Recieves messages from clients if host
    ///     For downloading/uploading map data
    /// </summary>
    public class MapDownloader : MonoBehaviour
    {
        public GameObject MyCharacterSyncher;
        public World MyWorld;
        //public ClassMaker MyClassMaker;
        public bool IsDownloadingMap = false;
        bool IsDownloadingPolyModel;
        bool IsDownloadingVoxelMeta;
        bool IsDownloadingSkeletons;
        bool IsDownloadingClasses;
        bool IsDownloadingVoxelTextures;
        //public SkeletonManager MySkeletonManager;
        public MapMaker MyMapMaker;


        /// <summary>
        /// 
        /// </summary>
        public void LoadWorld() // loads from host, sends data requests
        {
            IsDownloadingMap = true;
            //Debug.LogError("Loading World From host: " + PhotonNetwork.masterClient.name);
            if (PhotonNetwork.isMasterClient)
            {
                Debug.LogError("Attempting to load map from host..");
                return;
            }
            // Need to load meta data
            //MapMaker.LoadWorldMeta(MyWorld.gameObject);
            //OnPreLoadWorld.Invoke(SelectedName);   // loads all game data

            StartCoroutine(LoadInTime());
        }

        IEnumerator LoadInTime()
        {
            // VoxelMeta
            /*IsDownloadingVoxelMeta = true;
            DownloadVoxelMeta();
            while (IsDownloadingVoxelMeta)
            {
                yield return null;
            }
            // Classes
            IsDownloadingClasses = true;
            DownloadClasses();
            while (IsDownloadingClasses)
            {
                yield return null;
            }
            // Dialogues
            // ItemMeta
            // Quests
            // Block Textures
            IsDownloadingVoxelTextures = true;
            DownloadVoxelTextures();
            while (IsDownloadingVoxelTextures)
            {
                yield return null;
            }
            // Item Textures
            // Skeletons
            IsDownloadingSkeletons = true;
            DownloadSkeletons();
            while (IsDownloadingSkeletons)
            {
                yield return null;
            }

            // PolyModel
            IsDownloadingPolyModel = true;
            DownloadPolyModels();
            while (IsDownloadingPolyModel)
            {
                yield return null;
            }

            // PolyModels
        
            // Now Load Chunks
            yield return new WaitForSeconds(0.1f);
            //MyWorld.GetComponent<VoxelSaver>().LoadFromFile(MapMaker.SaveFileName);
            // Wait until downloading has finished!

            // after Chunks are loaded, synch characters
            yield return new WaitForSeconds(0.1f);
            // Synch all characters
            MyCharacterSyncher.GetComponent<PhotonView>().RPC("SynchCharacterData",
                PhotonTargets.Others,
                PhotonNetwork.player.ID
            );

            IsDownloadingMap = false;
        }

        void DownloadVoxelMeta()
        {
            Debug.LogError("Downloading Voxel Meta.");
            MyWorld.GetComponent<Zeltex.Voxels.VoxelManager>().Data.Clear();
            gameObject.GetComponent<PhotonView>().RPC("RequestVoxelMeta",
                    PhotonNetwork.masterClient, // just sent to master client for the data
                    PhotonNetwork.player.ID);
        }
        // This is send just to the host
        [PunRPC]
        public void RequestVoxelMeta(int PlayerID)
        {
            Debug.LogError("Request For Meta Data from: " + PlayerID);
            PhotonPlayer MyPlayerRequested = PhotonPlayer.Find(PlayerID);
            List<VoxelMeta> MyData = MyWorld.GetComponent<VoxelManager>().Data;
            //Debug.LogError("Sending Data - " + MyData.Count + " - to: " + MyPlayerRequested.name);
            for (int i = 0; i < MyData.Count; i++)
            {
                string MyScript = MyData[i].GetScript();
                //Debug.LogError("    Sending Data: " + MyScript);
                gameObject.GetComponent<PhotonView>().RPC("RecieveVoxelMeta",
                    MyPlayerRequested,  // send just to the player that requested the data
                    i,
                    MyScript
                    );
            }
            gameObject.GetComponent<PhotonView>().RPC("RecieveVoxelMeta",
                MyPlayerRequested,  // send just to the player that requested the data
                -1,
                "End"
                );
        }

        [PunRPC]
        public void RecieveVoxelMeta(int i, string MyScript)
        {
            //Debug.LogError("RecieveVoxelMeta: " + i);
            //Debug.LogError("    Data: " + MyMetaScript);
            if (MyScript == "End")
            {
                IsDownloadingVoxelMeta = false;
            }
            else
            {
                MyWorld.GetComponent<Zeltex.Voxels.VoxelManager>().Data.Add(new Zeltex.Voxels.VoxelMeta(MyScript));
            }
        }

        // Voxel Models

        void DownloadPolyModels()
        {
            Debug.LogError("Downloading Voxel Models.");
            MyWorld.GetComponent<Zeltex.Voxels.VoxelManager>().MyModels.Clear();
            gameObject.GetComponent<PhotonView>().RPC("RequestPolyModel",
                    PhotonNetwork.masterClient, // just sent to master client for the data
                    PhotonNetwork.player.ID);
        }
        // This is send just to the host
        [PunRPC]
        public void RequestPolyModel(int PlayerID)
        {
            //Debug.LogError("Request For Meta Data from: " + PlayerID);
            /*PhotonPlayer MyPlayerRequested = PhotonPlayer.Find(PlayerID);
            List<PolyModel> MyData = MyWorld.GetComponent<VoxelManager>().MyModels;
            //Debug.LogError("Sending PolyModelData - " + MyData.Count + " - to: " + MyPlayerRequested.name);
            for (int i = 0; i < MyData.Count; i++)
            {
                string MyScript = FileUtil.ConvertToSingle(MyData[i].GetScript());
                //Debug.LogError("    Sending Data: " + MyScript);
                gameObject.GetComponent<PhotonView>().RPC("RecievePolyModel",
                    MyPlayerRequested,  // send just to the player that requested the data
                    i,
                    MyScript
                    );
            }
            gameObject.GetComponent<PhotonView>().RPC("RecievePolyModel",
                MyPlayerRequested,  // send just to the player that requested the data
                -1,
                "End"
                );
        }
        [PunRPC]
        public void RecievePolyModel(int i, string MyScript)
        {
            //Debug.LogError("Recieve PolyModelData: " + i);
            if (MyScript == "End")
            {
                IsDownloadingPolyModel = false;
            }
            else
            {
                //Debug.LogError("    Data: " + MyMetaScript);
                Zeltex.Voxels.PolyModel MyModel = new Zeltex.Voxels.PolyModel();
                MyModel.RunScript(FileUtil.ConvertToList(MyScript));
                MyModel.GenerateSolidity();
                MyWorld.GetComponent<Zeltex.Voxels.VoxelManager>().AddModel(MyModel);
            }
        }

        // Voxel Textures
        void DownloadVoxelTextures()
        {
            Debug.LogError("Downloading Voxel Textures.");
            MyMapMaker.MyTextureManager.Clear();
            gameObject.GetComponent<PhotonView>().RPC("RequestVoxelTextures",
                    PhotonNetwork.masterClient, // just sent to master client for the data
                    PhotonNetwork.player.ID);
        }
        // This is send just to the host
        [PunRPC]
        public void RequestVoxelTextures(int PlayerID)
        {
            //Debug.LogError("Request For Meta Data from: " + PlayerID);
            PhotonPlayer MyPlayerRequested = PhotonPlayer.Find(PlayerID);
            List<Texture2D> MyData = MyMapMaker.MyTextureManager.GetData();
            //Debug.LogError("Sending PolyModelData - " + MyData.Count + " - to: " + MyPlayerRequested.name);
            for (int i = 0; i < MyData.Count; i++)
            {
                byte[] MyScript = MyData[i].EncodeToPNG();
                //Debug.LogError("    Sending Data: " + MyScript);
                gameObject.GetComponent<PhotonView>().RPC("RecieveVoxelTextures",
                    MyPlayerRequested,  // send just to the player that requested the data
                    i,
                    MyData[i].width,
                    MyData[i].height,
                    MyScript
                    );
            }
            gameObject.GetComponent<PhotonView>().RPC("RecieveVoxelTextures",
                MyPlayerRequested,  // send just to the player that requested the data
                -1,
                0,
                0,
                null
                );
        }

        public void RecieveVoxelTextures(int i, int width, int height, byte[] MyScript)
        {
            //Debug.LogError("Recieve Voxel Texture: " + i);
            if (i == -1)
            {
                IsDownloadingVoxelTextures = false;
                //MyMapMaker.MyTextureManager.GenerateTileMap();
            }
            else
            {
                //Debug.LogError("    Data: " + MyMetaScript);
                Texture2D MyTexture = new Texture2D(width, height);
                MyTexture.LoadImage(MyScript);
                //MyWorld.GetComponent<VoxelManager>().MyTextureManager.MyTextures.Add(MyTexture);
            }
        }

        // Classes
        void DownloadClasses()
        {
            /*IsDownloadingClasses = true;
            Debug.LogError("Downloading Classes.");
            MyMapMaker.MyClassEditor.MyData.Clear();
            //MyMapMaker.MyClassEditor.MyNames.Clear();
            gameObject.GetComponent<PhotonView>().RPC("RequestClasses",
                    PhotonNetwork.masterClient, // just sent to master client for the data
                    PhotonNetwork.player.ID);
        }
        // This is send just to the host
        [PunRPC]
        public void RequestClasses(int PlayerID)
        {
            //Debug.LogError("Request For Meta Data from: " + PlayerID);
            PhotonPlayer MyPlayerRequested = PhotonPlayer.Find(PlayerID);
            List<string> MyData = MyMapMaker.MyClassEditor.MyData;
            //List<string> MyNames = MyMapMaker.MyClassEditor.MyNames;
            //Debug.LogError("Sending Classes - " + MyData.Count + " - to: " + MyPlayerRequested.name);
            for (int i = 0; i < MyData.Count; i++)
            {
                //Debug.LogError("    Sending Data: " + MyScript);
                gameObject.GetComponent<PhotonView>().RPC("RecieveClasses",
                    MyPlayerRequested,  // send just to the player that requested the data
                    i,
                    MyData[i]//,
                    //MyNames[i]
                    );
            }
            gameObject.GetComponent<PhotonView>().RPC("RecieveClasses",
                MyPlayerRequested,  // send just to the player that requested the data
                -1,
                "",
                ""
                );
        }
        [PunRPC]
        public void RecieveClasses(int i, string MyScript, string MyName)
        {
            //Debug.LogError("Recieve Voxel Texture: " + i);
            if (i == -1)
            {
                IsDownloadingClasses = false;
            }
            else
            {
                //Debug.LogError("    Data: " + MyMetaScript);
                MyMapMaker.MyClassEditor.MyData.Add(MyScript);
               // MyMapMaker.MyClassEditor.MyNames.Add(MyName);
            }
        }

        // Skeletons

        void DownloadSkeletons()
        {
            IsDownloadingSkeletons = true;
            Debug.LogError("Downloading Skeletons.");
            //MyMapMaker.MySkeletonManager.MyData.Clear();
            //MyMapMaker.MySkeletonManager.MyNames.Clear();
            gameObject.GetComponent<PhotonView>().RPC("RequestSkeletons",
                    PhotonNetwork.masterClient, // just sent to master client for the data
                    PhotonNetwork.player.ID);
        }

        // This is send just to the host
        [PunRPC]
        public void RequestSkeletons(int PlayerID)
        {
            //Debug.LogError("Request For Meta Data from: " + PlayerID);
            PhotonPlayer MyPlayerRequested = PhotonPlayer.Find(PlayerID);
            List<string> MyData = MyMapMaker.MySkeletonManager.MyData;
            //List<string> MyDataNames = MyMapMaker.MySkeletonManager.MyNames;
            //Debug.LogError("Sending Classes - " + MyData.Count + " - to: " + MyPlayerRequested.name);
            for (int i = 0; i < MyData.Count; i++)
            {
                //Debug.LogError("    Sending Data: " + MyScript);
                gameObject.GetComponent<PhotonView>().RPC("RecieveSkeletons",
                    MyPlayerRequested,  // send just to the player that requested the data
                    i,
                    //MyDataNames[i],
                    MyData[i]
                    );
            }
            gameObject.GetComponent<PhotonView>().RPC("RecieveSkeletons",
                MyPlayerRequested,  // send just to the player that requested the data
                -1,
                "",
                ""
                );
        }

        public void RecieveSkeletons(int i, string MyName, string MyScript)
        {
            //Debug.LogError("Recieve Voxel Texture: " + i);
            if (i == -1)
            {
                IsDownloadingSkeletons = false;
            }
            else
            {
                //Debug.LogError("    Data: " + MyMetaScript);
                //MyMapMaker.MySkeletonManager.MyNames.Add(MyName);
                //MyMapMaker.MySkeletonManager.MyData.Add(MyScript);
            }
        }
    }
}*/

// when client joins a room - called from photon network
/*void OnJoinedRoom()
{
    //if (enabled)
    {
        //Debug.Log("At [" + Time.time + "] Player " + PhotonNetwork.playerName + " has Joined the room " + 
        //    PhotonNetwork.room.name + " of master " + PhotonNetwork.room.masterClientId);

        if (PhotonNetwork.masterClient != PhotonNetwork.player)
        {   // if local player is not master client, synch the data(characters and chunks)
            // synch map data
            LoadWorld();
        }
        else
        {
            //Debug.LogError("Not Synching data, " + PhotonNetwork.player.name + " is master client.");
        }
    }
} */
