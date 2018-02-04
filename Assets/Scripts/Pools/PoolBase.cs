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
        //[SerializeField]
        //private int DebugChildCount;
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
            //if (MyPoolSpawner == null)
            {
                MyPoolSpawner = GetComponent<PoolSpawner>();
                MyPoolSpawner.Initialize(this.GetType());
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
            StartCoroutine(StartAfterGameManagerStart());
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

        IEnumerator StartAfterGameManagerStart()
        {
            yield return null;
            yield return null;
            yield return null;
            PoolsManager.Get().ClearPools.AddEvent(ClearPools);
            PoolsManager.Get().SynchPools.AddEvent(SynchPools);
            PoolsManager.Get().MyPools.Add(this);
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
        
        /// <summary>
        /// Returns all spawned objects to the pools
        /// </summary>
        public void ReturnAllObjects()
        {
            if (Pools.Count > 0)
            {
                for (int j = 0; j < Pools.Count; j++)
                {
                    for (int i = Pools[j].PoolObjects.Count - 1; i >= 0; i--)
                    {
                        if (Pools[j].PoolObjects[i] == null)
                        {
                            Pools[j].PoolObjects.RemoveAt(i);
                        }
                        else
                        {
                            ReturnObject(Pools[j].PoolObjects[i]);
                        }
                    }
                }
            }
        }

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

        public void SpawnPools(System.Action OnFinishedSpawning) 
        {
            //Debug.LogError("Spawning Pools for " + name);
            RoutineManager.Get().StartCoroutine(SpawnPoolsRoutine(OnFinishedSpawning));
        }

        float LastYield;
        float YieldRate =  (1f / 1000f);
        private IEnumerator SpawnPoolsRoutine(System.Action OnFinishedSpawning)
        {
            if (!IsPoolsSpawned)
            {
                IsPoolsSpawned = true;
                if (MyPoolSpawner == null)
                {
                    MyPoolSpawner = GetComponent<PoolSpawner>();
                }
                CreatePoolObjects();
                LastYield = Time.realtimeSinceStartup;
                for (int i = 0; i < PoolPrefabs.Count; i++)
                {
                    yield return SpawnPool(i);
                    if (Time.realtimeSinceStartup - LastYield >= YieldRate)
                    {
                        LastYield = Time.realtimeSinceStartup;
                        yield return null;
                    }
                }
            }
            if (OnFinishedSpawning != null)
            {
                OnFinishedSpawning.Invoke();
            }
        }

        /// <summary>
        /// Spawn a pool of characters at start of game
        /// </summary>
        public IEnumerator SpawnPool(int PoolIndex = 0) 
        {
            int PoolMax = 1;
            if (PoolIndex < MaxPools.Count)
            {
                PoolMax = MaxPools[PoolIndex];
            }
            //Debug.Log("Spawning Pool[" + PoolIndex + "] at: " + Time.time);
            for (int i = 0; i < PoolMax; i++)
            {
                //Debug.Log("Spawning Pool Object [" + i + "] at: " + Time.time);
                SpawnPoolObject(i, PoolIndex);
                if (Time.realtimeSinceStartup - LastYield >= YieldRate)
                {
                    //Debug.Log("Yielding at Pool Object [" + i + "] at: " + Time.time);
                    LastYield = Time.realtimeSinceStartup;
                    yield return null;
                }
            }
            //DebugChildCount = transform.childCount;
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
                Debug.LogError("Prefabs Count is zero or " + PoolIndex + " has a null prefab inside [" + name + "]");
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
            if (PoolObject != null && PoolIndex >= 0 && PoolIndex < Pools.Count)
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
            if (Application.isPlaying == false)
            {
                RefreshPoolSpawner();
            }
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

        /// <summary>
        /// Editor only adding objects to pool
        /// </summary>
        public void EditorOnlyAddToPool(NetworkIdentity PoolObject)
        {
            PoolObject.transform.SetParent(transform);
            if (MyPools.Count == 0)
            {
                CreatePoolObjects();
            }
            CreatePoolObjects();
            MyPools[0].SpawnedObjects.Add(PoolObject.GetComponent<T>());
        }
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
                    PoolObjects[i].gameObject.Die();
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
            if (UnityEngine.Application.isPlaying)
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
            else
            {
                // In editor mode just kill it!
                if (PoolObject)
                {
                    if (SpawnedObjects.Contains(PoolObject))
                    {
                        SpawnedObjects.Remove(PoolObject);
                    }
                    PoolObject.gameObject.Die();
                }
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