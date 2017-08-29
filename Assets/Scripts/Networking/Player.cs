using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Zeltex.Networking
{
    /// <summary>
    /// Bam!
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
                    Destroy(this);
                    return;
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Zeltex.LogManager.Get().Log("Local Player! Spawning Bullet", "Client");
            }

        }

        [Command]
        public void CmdCreateBullet(Vector3 SpawnPosition, Quaternion SpawnRotation, Vector3 RandomForce)
        {
            Zeltex.LogManager.Get().Log("Server Command! Spawning Bullet", "Server");
            GetComponent<Zeltex.Combat.Shooter>().ServerCreateBullet(SpawnPosition, SpawnRotation, RandomForce);
        }

        #region Pools

        [Command]
        public void CmdReturnObjectToPool(ReadyMessageData Data)
        {
            if (Data.SpawnedObject)
            {
                Zeltex.LogManager.Get().Log("CmdRegisterNewObject Success: " + Data.Spawner.name + ":" + Data.SpawnedObject.name, "PoolsReturning");
                //Data.Spawner.gameObject.GetComponent(Data.MyType).SendMessage(Data.ServerFunctionName, Data);
                Data.Spawner.gameObject.GetComponent<PoolSpawner>().ServerReadyObject(Data);
            }
            else
            {
                Zeltex.LogManager.Get().Log("CmdRegisterNewObject Failed", "PoolsReturning");
            }
        }

        [Command]
        public void CmdReadyObject(ReadyMessageData Data)
        {
            if (Data.SpawnedObject)
            {
                Zeltex.LogManager.Get().Log("CmdReadyObject Success: " + Data.Spawner.name + ":" + Data.SpawnedObject.name, "PoolsReadying");
                //Data.Spawner.gameObject.GetComponent(Data.MyType).SendMessage(Data.ServerFunctionName, Data);
                Data.Spawner.gameObject.GetComponent<PoolSpawner>().ServerReadyObject(Data);

            }
            else
            {
                Zeltex.LogManager.Get().Log("CmdReadyObject Failed", "PoolsReadying");
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
                    Zeltex.LogManager.Get().Log("CmdSynchPool Success [PoolIndex: " + Data.PoolIndex + "]", "SynchPools");
                    MyPoolSpawner.ServerSynchPool(Data);
                }
                else
                {
                    Zeltex.LogManager.Get().Log("CmdSynchPool Failure as no pool spawner component", "SynchPools");
                }
            }
            else
            {
                Zeltex.LogManager.Get().Log("CmdSynchPool Failure as no pool spawner gameObject", "SynchPools");
            }
        }
        #endregion
    }

}