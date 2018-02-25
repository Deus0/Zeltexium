using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Zeltex
{
    public delegate void GetPoolObjectDelgate2(Characters.Character SpawnedObject);
    /// <summary>
    /// NetworkIdentity SpawnedObject, int PoolIndex, string Type
    /// </summary>
    public class ReadyMessageData
    {
        public NetworkIdentity SpawnedObject;
        public NetworkIdentity Spawner;
        public int PoolIndex;
        public string ServerFunctionName;
        // The function used at the end
        public string PoolBaseFunctionName;
        public NetworkIdentity ExtraData;
        //public GetPoolObjectDelgate2 OnGetPoolObject = null;
    }

    /// <summary>
    /// NetworkIdentity SpawnedObject, int PoolIndex, string Type
    /// </summary>
    public class SynchPoolMessageData
    {
        public NetworkIdentity Spawner;
        public int ClientConnectionID;
        // -1 if all pools
        public int PoolIndex;
        public string BaseFunctionName;
    }

    public class PoolSpawner : NetworkBehaviour
    {
        private Component MyPool;
        private System.Type PoolType;

        public void Initialize(System.Type PoolBaseType)
        {
            PoolType = PoolBaseType;
            MyPool = GetComponent(PoolBaseType);
        }
        
        #region SpawningPool
        public T SpawnPoolObject<T>(GameObject Prefab, Component MyPool, int PoolIndex = 0) where T : Component
        {
            if (Prefab)
            {
                bool IsNetworkPrefab = false;
                if (UnityEngine.Networking.NetworkServer.active)
                {
                    IsNetworkPrefab = Prefab.GetComponent<UnityEngine.Networking.NetworkIdentity>();
                    if (IsNetworkPrefab)
                    {
                        UnityEngine.Networking.ClientScene.RegisterPrefab(Prefab);
                    }
                }
                GameObject PoolObject = Instantiate(Prefab);
                T PoolComponent = PoolObject.GetComponent<T>();
                if (UnityEngine.Networking.NetworkServer.active && IsNetworkPrefab)
                {
                    UnityEngine.Networking.NetworkServer.Spawn(PoolObject);
                }
                if (Application.isPlaying)  // in editor, there are no pools just spawning
                {
                    InitialReturnObjectToPool(PoolObject, MyPool, PoolIndex);
                }
                else
                {
                    GetComponent(PoolType).SendMessage("EditorOnlyAddToPool", PoolObject.GetComponent<NetworkIdentity>());
                }
                return PoolComponent;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region ReturnObjectToPool

        /// <summary>
        /// This initially sends a message as the server or client
        /// Then it bounces back and forth to make sure the function is called on every machine
        /// </summary>
        public void InitialReturnObjectToPool(GameObject SpawnedObject, Component MyPool, int PoolIndex = 0)
        {
            ReadyMessageData Data = new ReadyMessageData();
            Data.SpawnedObject = SpawnedObject.GetComponent<NetworkIdentity>();
            Data.Spawner = gameObject.GetComponent<NetworkIdentity>();
            Data.PoolIndex = PoolIndex;
            Data.ServerFunctionName = "ServerReturnObjectToPool";
            Data.PoolBaseFunctionName = "NetworkReturnObject";
            if (Data.SpawnedObject != null)
            {
                if (UnityEngine.Application.isPlaying)
                {
                    if (isServer)
                    {
                        ServerReturnObjectToPool(Data);
                    }
                    else
                    {
                        ClientReturnObjectToPool(Data);
                    }
                }
                // editor only spawning objects
                else
                {
                    GetComponent(PoolType).SendMessage("NetworkReturnObject", Data);
                }
            }
        }

        [Client] // called only on client
        public void ClientReturnObjectToPool(ReadyMessageData Data)
        {
            LogManager.Get().Log("Client: ClientReturnObjectToPool " + name, "PoolRegistering");
            if (Zeltex.Networking.Player.Get())
            {
                Zeltex.Networking.Player.Get().CmdReturnObjectToPool(Data);
            }
        }

        /// <summary>
        /// Spawns the bullet on the network
        /// This gets called only on the server
        /// </summary>
        [Server]
        public void ServerReturnObjectToPool(ReadyMessageData Data)
        {
            LogManager.Get().Log("Server: RegisterNewObject " + name, "PoolRegistering");
            //RegisterNewObject(Data);
            RpcReturnObjectToPool(Data);
        }

        /// <summary>
        /// Called on every client
        /// </summary>
        [ClientRpc]
        public void RpcReturnObjectToPool(ReadyMessageData Data)
        {
            GameObject NewObject = Data.SpawnedObject.gameObject;
            LogManager.Get().Log(
                ("ClientAll: Success - RegisterNewObject " + name + ":" + Data.PoolIndex
                + " - NewObject? " + NewObject.name),
                "PoolRegistering");
            MyPool.SendMessage(Data.PoolBaseFunctionName, Data);
        }
        #endregion

        #region InitialReadyObject

        [Client] // called only on client
        public void ClientReadyObject(ReadyMessageData Data)
        {
            LogManager.Get().Log("Client: ClientReadyObject " + Data.SpawnedObject.netId, "PoolsReadying");
            if (Networking.Player.Get())
            {
                Networking.Player.Get().CmdReadyObject(Data);
            }
        }

        /// <summary>
        /// Spawns the bullet on the network
        /// This gets called only on the server
        /// </summary>
        [Server]
        public void ServerReadyObject(ReadyMessageData Data)
        {
            LogManager.Get().Log("Server: ServerReadyObject " + Data.SpawnedObject.netId, "PoolsReadying");
            //RegisterNewObject(Data);
            RpcReadyObject(Data);
        }

        /// <summary>
        /// Called on every client
        /// </summary>
        [ClientRpc]
        public void RpcReadyObject(ReadyMessageData Data)
        {
            if (Data != null && Data.SpawnedObject != null)
            {
                LogManager.Get().Log("ClientAll: Success - RpcReadyObject " +
                    Data.SpawnedObject.netId +
                    ": - PoolIndex: " + Data.PoolIndex
                    + " - NewObject? " + Data.SpawnedObject.name,
                    "PoolsReadying");
                MyPool.SendMessage(Data.PoolBaseFunctionName, Data);
            }
            else
            {
                Debug.LogError(name + " ~ Cannot ready object as data or spawned object is null.");
            }
        }
        #endregion

        #region SynchPool

        public bool IsServer()
        {
            return isServer;
        }
        /*if (!isServer)
        {
            ClientRquestSynchPool();
        }*/
        /*Characters.Character[] SceneCharacters = GameObject.FindObjectsOfType<Characters.Character>();
        if (SceneCharacters.Length > 0)
        {
            for (int i = 0; i < SceneCharacters.Length; i++)
            {
                if (SceneCharacters[i])
                {
                    InitialiRegisterNewObject(SceneCharacters[i].gameObject, MyPool);
                }
            }
        }*/

        /// <summary>
        /// First called by client, and asks server to synch
        /// </summary>
        [Client] // called only on client
        public void ClientSynchPool(SynchPoolMessageData Data)
        {
            LogManager.Get().Log("[PoolSpawner] Client: ClientSynchPool " + name, "SynchPools");
            if (Zeltex.Networking.Player.Get())
            {
                Zeltex.Networking.Player.Get().CmdSynchPool(Data);
            }
        }

        /// <summary>
        /// Server recieves the synch request
        /// </summary>
        [Server]
        public void ServerSynchPool(SynchPoolMessageData Data)
        {
            LogManager.Get().Log("[PoolSpawner] Server: Sending Pool Data to " + Data.Spawner.ToString() + " [PoolIndex: " + Data.PoolIndex + "]", "SynchPools");
            MyPool.SendMessage(Data.BaseFunctionName, Data);
        }

        // Then these are sent from PoolBase to synch individual objects

        [TargetRpc]
        public void TargetReturnObjectToPool(NetworkConnection Target, ReadyMessageData Data)
        {
            LogManager.Get().Log("[PoolSpawner] Client: TargetReturnObjectToPool in " + name, "SynchPools");
            MyPool.SendMessage(Data.PoolBaseFunctionName, Data);
        }

        [TargetRpc]
        public void TargetReadyObject(NetworkConnection Target, ReadyMessageData Data)
        {
            LogManager.Get().Log("[PoolSpawner] Client: TargetReadyObject in " + name, "SynchPools");
            MyPool.SendMessage(Data.PoolBaseFunctionName, Data);
        }
        #endregion
    }
}