using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Zeltex.Networking
{
    /// <summary>
    /// Main commander for networking, acts between networking functions
    /// </summary>
    class Player : NetworkBehaviour
    {
        private static Player Instance;

        public static Player Get()
        {
            return Instance;
        }

        void Update()
        {
            if (Instance == null)
            {
                if (isLocalPlayer)
                {
                    Instance = this;
                }
                else
                {
                    this.Die();
                    return;
                }
            }
        }

        [Command]
        public void CmdCreateBullet(Vector3 SpawnPosition, Quaternion SpawnRotation, Vector3 RandomForce)
        {
            LogManager.Get().Log("Server Command! Spawning Bullet", "Server");
            GetComponent<Combat.Shooter>().ServerCreateBullet(SpawnPosition, SpawnRotation, RandomForce);
        }

        #region Pools

        [Command]
        public void CmdReturnObjectToPool(ReadyMessageData Data)
        {
            if (Data.SpawnedObject)
            {
                LogManager.Get().Log("CmdRegisterNewObject Success: " + Data.Spawner.name + ":" + Data.SpawnedObject.name, "PoolsReturning");
                //Data.Spawner.gameObject.GetComponent(Data.MyType).SendMessage(Data.ServerFunctionName, Data);
                Data.Spawner.gameObject.GetComponent<PoolSpawner>().ServerReadyObject(Data);
            }
            else
            {
                LogManager.Get().Log("CmdRegisterNewObject Failed", "PoolsReturning");
            }
        }

        [Command]
        public void CmdReadyObject(ReadyMessageData Data)
        {
            if (Data.SpawnedObject)
            {
                LogManager.Get().Log("CmdReadyObject Success: " + Data.Spawner.name + ":" + Data.SpawnedObject.name, "PoolsReadying");
                //Data.Spawner.gameObject.GetComponent(Data.MyType).SendMessage(Data.ServerFunctionName, Data);
                Data.Spawner.gameObject.GetComponent<PoolSpawner>().ServerReadyObject(Data);

            }
            else
            {
                LogManager.Get().Log("CmdReadyObject Failed", "PoolsReadying");
            }
        }

        /// <summary>
        /// Sends from the client to the server to: SynchPools
        /// </summary>
        [Command]
        public void CmdSynchPool(SynchPoolMessageData Data)
        {
            if (Data.Spawner && Data.Spawner.gameObject)
            {
                PoolSpawner MyPoolSpawner = Data.Spawner.gameObject.GetComponent<PoolSpawner>();
                if (MyPoolSpawner)
                {
                    LogManager.Get().Log("CmdSynchPool Success [PoolIndex: " + Data.PoolIndex + "]", "SynchPools");
                    MyPoolSpawner.ServerSynchPool(Data);
                }
                else
                {
                    LogManager.Get().Log("CmdSynchPool Failure as no pool spawner component", "SynchPools");
                }
            }
            else
            {
                LogManager.Get().Log("CmdSynchPool Failure as no pool spawner gameObject", "SynchPools");
            }
        }
        #endregion
    }

}