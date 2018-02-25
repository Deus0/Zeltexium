using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Characters;
using Zeltex.AI;
using Zeltex.Voxels;  // for position finder
using Zeltex.Util;

namespace Zeltex.WorldUtilities
{
    /// <summary>
    /// Spawns characters in a zone.
    /// To use script, attach to a zone object.
    /// </summary>
	public class ZoneSpawner : Zone
    {
        #region Variables
        [Header("Spawn")]
        [SerializeField]
        private GameObject SpawnPrefab;
        [SerializeField]
        private bool IsSpawnCharacter;
        [Header("Spawn Rate")]
        [SerializeField]
        private int MaximumSpawns = 3;
        [SerializeField]
        private float SpawnRate = 15;
		private float LastSpawned = 0f;
        [Header("SpawnCharacter")]
        [SerializeField]
        private string MyClassName = "Minion";
        [SerializeField]
        private string MyRaceName = "Human";
        [SerializeField]
        private bool IsUseIndexes = false;
        //[SerializeField]
        //private int ClassIndex = 0;
        [SerializeField]
        private int RaceIndex = 0;
        [Header("Positioning")]
        public SpawnPositionFinder MySpawnPositionFinder;
        [Header("Spawned")]
        [SerializeField]
        private List<GameObject> MySpawns = new List<GameObject>();
        [Header("Event")]
        public EventObject OnSpawn;
        private UnityEngine.Events.UnityAction<GameObject> OnCharacterReturnToPool;

        #endregion

        #region Mono
        void Start()
        {
            LastSpawned = Time.time;    // delay it
        }

        public void SetWorld(World NewWorld)
        {
            MySpawnPositionFinder.MyWorld = NewWorld;
        }

        void Update()
        {
            if (Game.GameMode.IsPlaying)
            {
                if (isServer)
                {
                    ServerSpawn();
                    //ServerSpawn(GetSpawnPosition(), HotSpotTransform.rotation,
                    //    new Vector3(+Random.Range(-MySpell.Randomness, MySpell.Randomness), +Random.Range(-MySpell.Randomness, MySpell.Randomness), +Random.Range(-MySpell.Randomness, MySpell.Randomness)));
                }
            }
        }
        #endregion
        
        /// <summary>
        /// Clears the minions summoned by this spawner.
        /// </summary>
        public void Clear()
        {
			for (int i = 0; i < MySpawns.Count; i++)
            {
				if (MySpawns[i])
                {
                    DestroyImmediate(MySpawns[i]);
                }
			}
			MySpawns.Clear();
		}

        /// <summary>
        /// Spawns the minions on a timer, only if they are player owned.
        /// </summary>
        [Server]
        void ServerSpawn()
        {
            // only spawn if is host and if connected
            if (MySpawns.Count < MaximumSpawns && 
                Time.time - LastSpawned >= SpawnRate)
            {
                LastSpawned = Time.time;
                Vector3 NewSpawnPosition = transform.position;
                if (MySpawnPositionFinder)
                {
                    GetComponent<SpawnPositionFinder>().IsRandom = true;
                    NewSpawnPosition = GetComponent<SpawnPositionFinder>().FindNewPosition();
                }
                else
                {
                    NewSpawnPosition = GetRandomPosition();
                }
                StartCoroutine(Spawn(NewSpawnPosition));
            }
        }

        IEnumerator Spawn(Vector3 Position)
        {
            Debug.Log("Spawning new character in " + name + " - Class: " + MyClassName + " with: " + MyRaceName);
            if (IsSpawnCharacter && CharacterManager.Get())
            {
                yield return SpawnCharacter(Position);
            }
            else if (SpawnPrefab)
            {
                GameObject Spawn = Instantiate(SpawnPrefab, Position, Quaternion.identity);
                MySpawns.Add(Spawn);
                UnityEngine.Networking.NetworkServer.Spawn(Spawn);
                OnSpawn.Invoke(Spawn);
            }
            yield return null;
        }

        private IEnumerator SpawnCharacter(Vector3 Position)
        {
            Character MySummonedObject = CharacterManager.Get().GetPoolObject();

            if (MySummonedObject)
            {
                MySpawns.Add(MySummonedObject.gameObject);
                MySummonedObject.ForceInitialize();
                MySummonedObject.name = NameGenerator.GenerateVoxelName();
                MySummonedObject.Teleport(Position);
                MySummonedObject.transform.rotation = Quaternion.identity;
                if (IsUseIndexes)
                {
                    if (RaceIndex >= 0 && RaceIndex < DataManager.Get().GetSize(DataFolderNames.Skeletons))
                    {
                        MyRaceName = DataManager.Get().GetName(DataFolderNames.Skeletons, RaceIndex);
                    }
                    /*if (ClassIndex >= 0 && ClassIndex < DataManager.Get().GetSize(DataFolderNames.Classes))
                    {
                        MyClassName = DataManager.Get().GetName(DataFolderNames.Classes, ClassIndex);
                    }*/
                }
                yield return null;
                //yield return CharacterManager.Get().UpdateSpawned(MySummonedObject, MyRaceName, MyClassName, "");
                //MySpawns[MySpawns.Count - 1].GetComponent<CharacterStats>().OnDeath.AddEvent(Remove);
                MySummonedObject.GetComponent<Bot>().Wander();
                OnCharacterReturnToPool = OnReturnToPool;
                MySummonedObject.OnReturnToPool.RemoveEvent<GameObject>(OnCharacterReturnToPool);
                MySummonedObject.OnReturnToPool.AddEvent<GameObject>(OnCharacterReturnToPool);
                OnSpawn.Invoke(MySummonedObject.gameObject);
            }
        }

        public void OnReturnToPool(GameObject MyCharacter)
        {
            if (MySpawns.Count >= MaximumSpawns)
            {
                LastSpawned = Time.time;    // delay it
            }
            for (int i = 0; i < MySpawns.Count; i++)
            {
                if (MySpawns[i] == MyCharacter)
                {
                    MySpawns.RemoveAt(i);
                    break;
                }
            }
        }
	}
}