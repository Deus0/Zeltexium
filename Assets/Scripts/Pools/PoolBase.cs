using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Zeltex
{
    public delegate void GetPoolObjectDelgate<T>(T SpawnedObject);
    /// <summary>
    /// The base of all manager classes
    ///     -ReturnObjectToPool
    ///     -GetPoolObject - sets them to true
    ///     -SpawnPool
    ///     -SynchPool  (for new players)
    ///     -Possession - Should be set to be owned by the client of possessed
    /// Notes:
    ///     The characters are all set to be owned by the server
    /// </summary>
    [RequireComponent(typeof(PoolSpawner))]
    public class PoolBase<T> : ManagerBase<PoolBase<T>> where T : Component
    {
        [Header("Pool")]
        [SerializeField]
        private List<int> MaxPools = new List<int>();
        [SerializeField]
        private List<GameObject> PoolPrefabs = new List<GameObject>();
        [Header("PoolDebug")]
        [SerializeField]
        private int DebugChildCount;
        [SerializeField]
        protected List<SpawnedPool<T>> MyPools = new List<SpawnedPool<T>>();
        protected PoolSpawner MyPoolSpawner;
        private bool IsPoolsSpawned;

        public virtual List<SpawnedPool<T>> Pools
        {
            get
            {
                return MyPools;
            }
            set
            {
                MyPools = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            RefreshPoolSpawner();
        }

        private void RefreshPoolSpawner()
        {
            if (MyPoolSpawner == null)
            {
                MyPoolSpawner = GetComponent<PoolSpawner>();
                MyPoolSpawner.Initialize(this.GetType().ToString());
            }
        }

        private void CreatePoolObjects()
        {
            if (Pools.Count != PoolPrefabs.Count)
            {
                for (int i = 0; i < PoolPrefabs.Count; i++)
                {
                    CreatePoolObject();
                }
            }
        }

        protected virtual void Start()
        {
            CreatePoolObjects();
            StartCoroutine(WaitForGameManager());
        }

        public GameObject GetPrefab(int PoolIndex)
        {
            if (PoolIndex >= 0 && PoolIndex < PoolPrefabs.Count)
            {
                return PoolPrefabs[PoolIndex];
            }
            else
            {
                return null;
            }
        }

        IEnumerator WaitForGameManager()
        {
            yield return null;
            yield return null;
            yield return null;
            // Just do this once
            //GameManager.Get().OnBeginGame.AddEvent(SpawnPools);
            //GameManager.Get().OnEndGame.AddEvent(ClearPools);
            PoolsManager.Get().SpawnPools.AddEvent(SpawnPools);
            PoolsManager.Get().ClearPools.AddEvent(ClearPools);
            PoolsManager.Get().SynchPools.AddEvent(SynchPools);
        }

        #region Getters

        public List<T> GetSpawned(int PoolIndex = 0)
        {
            if (Pools.Count == 0 ||
                PoolIndex < 0 || PoolIndex >= Pools.Count)
            {
                return new List<T>();
            }
            else
            {
                return Pools[PoolIndex].SpawnedObjects;
            }
        }

        public int GetSize()
        {
            if (Pools.Count == 0)
            {
                return 0;
            }
            else
            {
                return Pools[0].SpawnedObjects.Count;
            }
        }

        public T GetSpawn(int Index, int PoolIndex = 0)
        {
            if (Pools.Count > 0 && Index >= 0 && Index < Pools[PoolIndex].SpawnedObjects.Count)
            {
                return Pools[PoolIndex].SpawnedObjects[Index];
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region PoolNames
        protected bool ContainsPool(string PoolName)
        {
            int PoolIndex = GetPoolIndex(PoolName);
            return PoolIndex != -1;
        }

        protected T SpawnPoolObject(string PoolName)
        {
            int PoolIndex = GetPoolIndex(PoolName);
            if (PoolIndex != -1)
            {
                return SpawnPoolObject(PoolIndex);
            }
            else
            {
                return null;
            }
        }

        public int GetPoolIndex(string PoolName)
        {
            for (int i = 0; i < PoolPrefabs.Count; i++)
            {
                if (i < PoolPrefabs.Count)
                {
                    if (PoolPrefabs[i].name == PoolName)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        #endregion

        protected virtual void CreatePoolObject()
        {
            LogManager.Get().Log("Creating new pool at: " + Pools.Count, "PoolsBase");
            Pools.Add(new SpawnedPool<T>());
        }

        protected void ClearPools()
        {
            for (int i = 0; i < Pools.Count; i++)
            {
                Pools[i].ClearPool();
            }
        }

        public void SpawnPools()
        {
            if (!IsPoolsSpawned)
            {
                IsPoolsSpawned = true;
                if (MyPoolSpawner == null)
                {
                    MyPoolSpawner = GetComponent<PoolSpawner>();
                }
                //Debug.LogError("Pool: Spawning " + PoolPrefabs.Count + " in " + name);
                // Check Map if already exists?
                /*T[] SceneCharacters = GameObject.FindObjectsOfType<T>();
                if (SceneCharacters.Length > 0)
                {
                    for (int i = 0; i < SceneCharacters.Length; i++)
                    {
                        if (SceneCharacters[i])
                        {
                            MyPoolSpawner.InitialiRegisterNewObject(SceneCharacters[i].gameObject, this);
                        }
                    }
                }*/
                CreatePoolObjects();
                for (int i = 0; i < PoolPrefabs.Count; i++)
                {
                    SpawnPool(i);
                }
            }
        }

        /// <summary>
        /// Spawn a pool of characters at start of game
        /// </summary>
        private void SpawnPool(int PoolIndex = 0)
        {
            int PoolMax = 1;
            if (PoolIndex < MaxPools.Count)
            {
                PoolMax = MaxPools[PoolIndex];
            }
            for (int i = 0; i < PoolMax; i++)
            {
                SpawnPoolObject(i, PoolIndex);
            }
            DebugChildCount = transform.childCount;
        }

        public List<string> GetNames()
        {
            List<string> GuiNames = new List<string>();
            for (int i = 0; i < PoolPrefabs.Count; i++)
            {
                if (PoolPrefabs[i])
                {
                    GuiNames.Add(PoolPrefabs[i].name);
                }
            }
            return GuiNames;
        }

        #region SpawnObject
        protected virtual T SpawnPoolObject(int SpawnIndex, int PoolIndex = 0)
        {
            if (MyPoolSpawner == null)
            {
                Debug.LogError(name + " has null MyPoolSpawner");
                return null;
            }
            if (PoolPrefabs.Count > 0 && PoolPrefabs[PoolIndex])
            {
                return MyPoolSpawner.SpawnPoolObject<T>(PoolPrefabs[PoolIndex].gameObject, this, PoolIndex);
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region SynchPool

        /// <summary>
        /// Synches all available pools
        /// </summary>
        public void SynchPools()
        {
            CreatePoolObjects();
            for (int i = 0; i < PoolPrefabs.Count; i++)
            {
                SynchPool(i);
            }
        }

        /// <summary>
        /// Synches the spawn pool
        /// </summary>
        public void SynchPool(int PoolIndex = 0)
        {
            // if client
            //LogManager.Get().Log("Initial Synch Pool in PoolBase", "SynchPools");

            LogManager.Get().Log("[PoolBase] InitialSynchPool Pool type: " + this.GetType()
                 + ":" + ClientScene.readyConnection.connectionId, "SynchPools");
            if (!MyPoolSpawner.IsServer())
            {
                SynchPoolMessageData Data = new SynchPoolMessageData();
                Data.ClientConnectionID = ClientScene.readyConnection.connectionId;
                Data.Spawner = gameObject.GetComponent<NetworkIdentity>();
                Data.PoolIndex = PoolIndex;
                Data.BaseFunctionName = "FinalSynchPool";
                MyPoolSpawner.ClientSynchPool(Data);
            }
        }

        public List<T> GetPoolObjects(int PoolIndex = 0)
        {
            return MyPools[PoolIndex].PoolObjects;
        }
        
        public void FinalSynchPool(SynchPoolMessageData OldData)
        {
            StartCoroutine(FinalSynchPoolRoutine(OldData));
        }

        private IEnumerator FinalSynchPoolRoutine(SynchPoolMessageData OldData)
        {
            LogManager.Get().Log("[PoolBase] Server: FinalSynchPool in PoolBase "
                + "\n[PoolIndex: " + OldData.PoolIndex + "]",
                "SynchPools");
            NetworkConnection ClientConnection = null;
            for (int i = 0; i < NetworkServer.connections.Count; i++)
            {
                if (NetworkServer.connections[i].connectionId == OldData.ClientConnectionID)
                {
                    ClientConnection = NetworkServer.connections[i];
                }
            }
            if (ClientConnection != null)
            {
                LogManager.Get().Log("Final ServerSynchPool - Synching Pool Objects", "SynchPools");
                List<T> PoolObjects = GetPoolObjects(OldData.PoolIndex);
                for (int i = 0; i < PoolObjects.Count; i++)
                {
                    if (PoolObjects[i])
                    {
                        ReadyMessageData Data = new ReadyMessageData();
                        Data.SpawnedObject = PoolObjects[i].gameObject.GetComponent<NetworkIdentity>();
                        LogManager.Get().Log("[" + i + "] FinalSynchPool: " + Data.SpawnedObject.netId +
                            " - is null? " + (Data.SpawnedObject == null),
                            "SynchPools");
                        Data.Spawner = gameObject.GetComponent<NetworkIdentity>();
                        Data.PoolIndex = OldData.PoolIndex;
                        Data.PoolBaseFunctionName = "NetworkReturnObject";
                        // Observe(networking) inactive pool objects
                        System.Reflection.MethodInfo OnCheckObserver = typeof(NetworkIdentity).GetMethod("OnCheckObserver",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        System.Reflection.MethodInfo AddObserver = typeof(NetworkIdentity).GetMethod("AddObserver",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if ((bool)OnCheckObserver.Invoke(Data.SpawnedObject, new object[] { ClientConnection }))
                        {
                            AddObserver.Invoke(Data.SpawnedObject, new object[] { ClientConnection });
                        }

                        MyPoolSpawner.TargetReturnObjectToPool(ClientConnection, Data);
                        yield return null;
                    }
                    else
                    {
                        LogManager.Get().Log(i + "FinalSynchPool: Failure", "SynchPools");
                    }
                }
                yield return null;

                LogManager.Get().Log("Final ServerSynchPool - Synching Spawned Objects", "SynchPools");
                List<T> SpawnedObjects = GetSpawned(OldData.PoolIndex);
                for (int i = 0; i < SpawnedObjects.Count; i++)
                {
                    ReadyMessageData Data = new ReadyMessageData();
                    Data.SpawnedObject = SpawnedObjects[i].GetComponent<NetworkIdentity>();
                    Data.Spawner = gameObject.GetComponent<NetworkIdentity>();
                    Data.PoolIndex = OldData.PoolIndex;
                    Data.PoolBaseFunctionName = "NetworkReadyObject";
                    MyPoolSpawner.TargetReadyObject(ClientConnection, Data);
                    yield return null;
                }
            }
            else
            {
                LogManager.Get().Log("Final FAILED ServerSynchPool in PoolBase", "SynchPools");
            }
            yield return null;
        }
        #endregion

        #region ReturnObjectToPool


        public void NetworkReturnObject(ReadyMessageData Data)
        {
            /*if (Data.PoolIndex < Pools.Count
                || Pools.Count == 0)
            {
                CreatePoolObject();
            }*/
            CreatePoolObjects();
            if (Data.PoolIndex >= 0 && Data.PoolIndex < Pools.Count)
            {
                if (Data.SpawnedObject != null)
                {
                    T PoolObject = Data.SpawnedObject.gameObject.GetComponent<T>();
                    if (Pools[Data.PoolIndex].CanReturnObject(PoolObject))
                    {
                        LogManager.Get().Log("Adding Pool Object to pool [" + Data.PoolIndex + "] in " + name, "PoolRegistering");
                        ReturnObject(PoolObject, Data.PoolIndex);
                    }
                    else
                    {
                        LogManager.Get().Log("Failure [NetworkReturnObject] due to Component not found: " + name, "PoolRegistering");
                    }
                }
                else
                {
                    LogManager.Get().Log("Failure to Register new Spawn due to SpawnedObject is null. Spawner: " + Data.Spawner.name, "PoolRegistering");
                }
            }
            else
            {
                LogManager.Get().Log("Failure to Register new Spawn due to pool index out of bounds in " + name
                        + "\n" + Data.PoolIndex + " out of " + Pools.Count, "PoolRegistering");
            }
        }
        
        public void ReturnObject(T PoolObject, string PoolPrefabName)
        {
            ReturnObject(PoolObject, GetPoolIndex(PoolPrefabName));
        }

        public virtual void ReturnObject(T PoolObject, int PoolIndex = 0)
        {
            //for (int i = 0; i < Pools.Count; i++)
            if (PoolIndex >= 0 && PoolIndex < Pools.Count)
            {
                //if (Pools[PoolIndex].SpawnedObjects.Contains(PoolObject))
                PoolObject.transform.SetParent(transform);
                Pools[PoolIndex].ReturnObject(PoolObject);
            }
        }
        #endregion

        #region ReadyPoolObject

        public T GetPoolObject(string PoolName, NetworkIdentity ExtraData = null)//GetPoolObjectDelgate<T> OnGetPoolObject = null)
        {
            int PoolIndex = GetPoolIndex(PoolName);
            if (PoolIndex != -1)
            {
                return GetPoolObject(PoolIndex, -1, ExtraData);
            }
            else
            {
                //Debug.LogError("PoolIndex is -1");
                return null;
            }
        }

        public T GetPoolObject(int PoolIndex = 0, int ObjectIndex = -1, NetworkIdentity ExtraData = null)// GetPoolObjectDelgate<T> OnGetPoolObject = null)
        {
            if (Application.isPlaying)
            {
                if (Pools.Count > 0)
                {
                    T PoolComponent = Pools[PoolIndex].GetPoolObject(PoolIndex, ObjectIndex);

                    if (PoolComponent)
                    {
                        ReadyMessageData Data = new ReadyMessageData();
                        Data.SpawnedObject = PoolComponent.gameObject.GetComponent<NetworkIdentity>();
                        Data.Spawner = gameObject.GetComponent<NetworkIdentity>();
                        Data.PoolIndex = PoolIndex;
                        Data.ExtraData = ExtraData;
                        Data.ServerFunctionName = "ServerReadyObject";
                        Data.PoolBaseFunctionName = "ReadyObject";
                        LogManager.Get().Log("InitialReadyObject " + Data.SpawnedObject.netId, "PoolsReadying");
                        if (MyPoolSpawner.IsServer())
                        {
                            MyPoolSpawner.ServerReadyObject(Data);
                        }
                        else
                        {
                            MyPoolSpawner.ClientReadyObject(Data);
                        }
                    }
                    return PoolComponent;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return GetEditorObject(PoolIndex, ExtraData);
            }
        }

        /// <summary>
        /// offline version of objects
        /// </summary>
        protected virtual T GetEditorObject(int PoolIndex = 0, NetworkIdentity ExtraData = null)
        {
            //Debug.Log("Getting Pool Object in Edit mode.");
            RefreshPoolSpawner();
            T PoolComponent = SpawnPoolObject(0, PoolIndex);
            if (PoolComponent != null)
            {
                ReadyMessageData Data = new ReadyMessageData();
                Data.SpawnedObject = PoolComponent.gameObject.GetComponent<NetworkIdentity>();
                Data.Spawner = gameObject.GetComponent<NetworkIdentity>();
                Data.PoolIndex = PoolIndex;
                Data.ExtraData = ExtraData;
                ReadyObject(Data);
            }
            else
            {
                Debug.LogError("Spawned Pool Object is null in: " + name);
            }
            return PoolComponent;
        }

        /// <summary>
        /// Final ready object function
        /// </summary>
        public virtual void ReadyObject(ReadyMessageData Data)
        {
            T PoolObject = Data.SpawnedObject.gameObject.GetComponent<T>();
            if (PoolObject)
            {
                if (Data.PoolIndex >= 0 && Data.PoolIndex < Pools.Count)
                {
                    Pools[Data.PoolIndex].ReadyObject(PoolObject);
                }
                if (LogManager.Get())
                {
                    LogManager.Get().Log("ReadyObject (Final) " + PoolObject.GetComponent<NetworkIdentity>().netId, "PoolsReadying");
                }
                PoolObject.gameObject.SetActive(true);
                PoolObject.gameObject.hideFlags = HideFlags.None;
                PoolObject.name = PoolPrefabs[Data.PoolIndex].name;
                //ActivateDelegate(PoolObject);
            }
        }
        #endregion
    }

    [System.Serializable]
    public class SpawnedPool<T> where T : Component
    {
        public static bool IsHidePoolObjects = true;
        [SerializeField]
        public List<T> SpawnedObjects = new List<T>();
        [SerializeField]
        public List<T> PoolObjects = new List<T>();

        public void RemoveNullCharacters()
        {
            for (int i = SpawnedObjects.Count - 1; i >= 0; i--)
            {
                if (SpawnedObjects[i] == null)
                {
                    Debug.LogError("Error at " + i + " Spawn is null.");
                    SpawnedObjects.RemoveAt(i);
                }
            }
        }

        public void Remove(int Index)
        {
            if (Index >= 0 && Index < SpawnedObjects.Count)
            {
                ReturnObject(SpawnedObjects[Index]);
            }
        }

        public void ClearPool()
        {
            for (int i = 0; i < PoolObjects.Count; i++)
            {
                if (PoolObjects[i] != null)
                {
                    GameObject.Destroy(PoolObjects[i].gameObject);
                }
            }
            PoolObjects.Clear();
        }

        public void ReadyObject(T PoolComponent)
        {
            if (PoolComponent)
            {
                if (PoolObjects.Contains(PoolComponent) == true)
                {
                    PoolObjects.Remove(PoolComponent);
                }
                if (SpawnedObjects.Contains(PoolComponent) == false)
                {
                    SpawnedObjects.Add(PoolComponent);
                }
            }
        }

        public T GetPoolObject(int PoolIndex, int ObjectIndex = -1)
        {
            if (PoolObjects.Count > 0)
            {
                if (ObjectIndex == -1)
                {
                    ObjectIndex = PoolObjects.Count - 1;
                }
                return PoolObjects[ObjectIndex];
            }
            else
            {
                // if increase pool, increase it
                return null;
            }
        }


        public void ReturnObject(T PoolObject)
        {
            if (CanReturnObject(PoolObject))
            {
                if (SpawnedObjects.Contains(PoolObject))
                {
                    SpawnedObjects.Remove(PoolObject);
                }
                PoolObject.gameObject.SetActive(false);
                PoolObject.name = "PoolObject [" + (PoolObjects.Count - 1) + "]";
                if (IsHidePoolObjects)
                {
                    PoolObject.gameObject.hideFlags = HideFlags.HideInHierarchy;
                }
                PoolObjects.Add(PoolObject);
            }
        }

        public bool CanReturnObject(T PoolObject)
        {
            if (PoolObject == null)
            {
                return false;
            }
            return PoolObjects.Contains(PoolObject) == false;
        }
    }
}
/* if (PoolPrefabs[PoolIndex])// && PoolPrefabs[PoolIndex].GetComponent<T>() != null)
 {
     bool IsNetworkPrefab = PoolPrefabs[PoolIndex].GetComponent<UnityEngine.Networking.NetworkIdentity>();
     if (IsNetworkPrefab)
     {
         UnityEngine.Networking.ClientScene.RegisterPrefab(PoolPrefabs[PoolIndex].gameObject);
     }
     MyPoolSpawner.();
     GameObject PoolObject = Instantiate(PoolPrefabs[PoolIndex].gameObject);
     T PoolComponent = PoolObject.GetComponent<T>();
     if (IsNetworkPrefab)
     {
         UnityEngine.Networking.NetworkServer.Spawn(PoolObject);
     }

     if (Network.isServer)
     {
         ServerRegisterNewObject(PoolIndex, PoolObject.GetComponent<NetworkIdentity>());
     }
     else
     {
         ClientRegisterNewObject(PoolIndex, PoolObject.GetComponent<NetworkIdentity>());
     }
     return PoolComponent;
 }
 else
 {
     return null;
 }*/
//private Dictionary<int, GetPoolObjectDelgate<T>> DelegatesGetPoolObject = new Dictionary<int, GetPoolObjectDelgate<T>>();

/*private void AddDelegate(int NetworkID, GetPoolObjectDelgate<T> OnGetPoolObject)
{
    if (OnGetPoolObject != null)
    {
        if (DelegatesGetPoolObject.ContainsKey(NetworkID))
        {
            DelegatesGetPoolObject[NetworkID] = OnGetPoolObject;
        }
        else
        {
            DelegatesGetPoolObject.Add(NetworkID, OnGetPoolObject);
    }
    }
}

private void ActivateDelegate(T PoolObject)
{
    int NetworkID = (int)PoolObject.GetComponent<NetworkIdentity>().netId.Value;
    if (DelegatesGetPoolObject.ContainsKey(NetworkID))
    {
        if (DelegatesGetPoolObject[NetworkID] != null)
        {
            DelegatesGetPoolObject[NetworkID].Invoke(PoolObject);
        }
        DelegatesGetPoolObject.Remove(NetworkID);
    }
}*/



/*public T GetPoolObject(int PoolIndex = 0)
{
    // check for nulls until found non null
    RemoveNullCharacters();
    if (PoolObjects.Count > 0)
    {
        int Index = PoolObjects.Count - 1;
        GameObject PoolObject = PoolObjects[Index].gameObject;
        PoolObjects.RemoveAt(Index);
        T PoolComponent = PoolObject.GetComponent<T>();
        SpawnedObjects.Add(PoolComponent);
        return PoolComponent;
    }
    else
    {
        // if increase pool, increase it
        return null;
    }
}*/
