using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Zeltex.AI;
using Zeltex.Util;
using Zeltex.Skeletons;
using Zeltex.Guis.Maker;
using Zeltex.Physics;

namespace Zeltex.Characters
{

    /// <summary>
    /// Managers all the characters spawned in the map
    /// Spawns characters!
    /// Pools the characters
    /// </summary>
    public class CharacterManager : PoolBase<Character>
    {
        #region Variables
        public static bool IsAutoSave = true;
        private static string DefaultPlayerName = "Prefabs/Character";
        private static string DefaultCharacterName = "New_Character";
		private bool IsUpdatingCharacter = false;   // is the manager currently updating a character
        //private Vector3 HidePosition = new Vector3(0, -300, 0);

        public new static CharacterManager Get()
        {
            if (MyManager == null)
            {
                MyManager = GameObject.Find("CharacterPool").GetComponent<CharacterManager>();
            }
            return MyManager as CharacterManager;
        }

        #endregion

        public void DrawDebug()
        {
            GUILayout.Label("Character Pools: " + GetSize());
            if (GUILayout.Button("Spawn Pools"))
            {
                SpawnPools();
            }
            if (GUILayout.Button("Synch Pool"))
            {
                SynchPool();
            }
            if (Pools.Count > 0)
            {
                GUILayout.Label("Characters: " + Pools[0].SpawnedObjects.Count);
                GUILayout.Label("-----=====-----=====-----=====-----=====-----");
                int PoolIndex = 0;
                for (int i = 0; i < Pools[PoolIndex].SpawnedObjects.Count; i++)
                {
                    if (Pools[0].SpawnedObjects[i])
                    {
                        if (GUILayout.Button("\t" + i + ":\t" + Pools[PoolIndex].SpawnedObjects[i].name
                             + " [" + Pools[PoolIndex].SpawnedObjects[i].GetComponent<NetworkIdentity>().netId + "]"))
                        {
                            Camera.main.GetComponent<Possess>().SetCharacter(Pools[PoolIndex].SpawnedObjects[i]);
                        }
                    }
                }
                GUILayout.Label("-----=====-----=====-----=====-----=====-----");
                string CharacterName = "";
                for (int i = 0; i < Pools[PoolIndex].PoolObjects.Count; i++)
                {
                    if (Pools[PoolIndex].PoolObjects[i])
                    {
                        CharacterName = "\t" + i + ":\t" + Pools[PoolIndex].PoolObjects[i].name
                             + ":" + Pools[PoolIndex].PoolObjects[i].GetComponent<NetworkIdentity>().netId;
                        if (GUILayout.Button(CharacterName))
                        {
                            // Spawn Character, turn into normal one
                            //Character SpawnedOne = GetPoolObject(PoolIndex, i);
                            //SpawnedOne.transform.position = new Vector3(0, 5, 0);
                        }
                    }
                }
                GUILayout.Label("-----=====-----=====-----=====-----=====-----");
            }
        }

        #region Mono

        protected override void Start()
        {
            base.Start();
            LogManager.Get().AddDraw(DrawDebug, "CharacterManager");
        }

        /// <summary>
        /// Sets all the characters movement states
        /// </summary>
        public void SetMovement(bool NewState)
        {
            for (int i = 0; i < Pools[0].SpawnedObjects.Count; i++)
            {
                if (Pools[0].SpawnedObjects[i])
                {
                    Pools[0].SpawnedObjects[i].SetMovement(NewState);
                    if (NewState)
                    {
                        Bot MyBot = Pools[0].SpawnedObjects[i].GetComponent<Bot>();
                        if (MyBot)
                        {
                            MyBot.Wander();
                        }
                    }
                }
            }
        }

        public void SetAggression(bool NewAngryState)
        {
            Debug.Log("Setting " + Pools[0].SpawnedObjects.Count + " to kill each other.");
            for (int i = 0; i < Pools[0].SpawnedObjects.Count; i++)
            {
                if (Pools[0].SpawnedObjects[i])
                {
                    Bot MyBot = Pools[0].SpawnedObjects[i].GetComponent<Bot>();
                    if (MyBot)
                    {
                        MyBot.GetData().IsAggressive = NewAngryState;
                        if (NewAngryState)
                        {
                            // if kill!!
                            MyBot.Wander();
                        }
                    }
                }
            }
        }
        #endregion

        #region SceneCharacterActions

        /// <summary>
        /// Lets all the bots move
        /// </summary>
        public void SetBotMovement(bool NewState)
		{
			for (int i = Pools[0].SpawnedObjects.Count - 1; i >= 0; i--)
			{
				if (Pools[0].SpawnedObjects[i])
				{
					Bot MyBot = Pools[0].SpawnedObjects[i].GetComponent<Bot>();
					if (MyBot)
					{
						if (NewState)
						{
							MyBot.Wander();  // unpause
						}
						else
						{
							MyBot.Wait();   // pause
						}
					}
				}
			}
		}

        /// <summary>
        /// Kills all the characters
        /// </summary>
        public void Cull()
        {
            for (int i = Pools[0].SpawnedObjects.Count - 1; i >= 0; i--)
            {
                if (Pools[0].SpawnedObjects[i])
                {
                    Pools[0].SpawnedObjects[i].OnDeath();
                }
            }
        }

        /// <summary>
        /// Clears loaded characters
        /// </summary>
        public void Clear()
		{
            StopAllCoroutines();
            if (Pools.Count > 0)
            {
                for (int i = Pools[0].SpawnedObjects.Count - 1; i >= 0; i--)
                {
                    if (Pools[0].SpawnedObjects[i])
                    {
                        ReturnObject(Pools[0].SpawnedObjects[i]);
                    }
                }
            }
            else
            {
                Debug.LogError("No Character Pools, cannot clear.");
            }
            //SpawnedObjects.Clear();
        }
        /// <summary>
        /// Clears loaded characters
        /// </summary>
        public void ClearAllButMainCharacter()
        {
            Character MainCharacter = Camera.main.gameObject.GetComponent<Player>().GetCharacter();
            StopAllCoroutines();
            for (int i = Pools[0].SpawnedObjects.Count - 1; i >= 0; i--)
            {
                if (Pools[0].SpawnedObjects[i] && Pools[0].SpawnedObjects[i] != MainCharacter)
                {
                    ReturnObject(Pools[0].SpawnedObjects[i]);
                }
            }
            Pools[0].SpawnedObjects.Clear();
            Pools[0].SpawnedObjects.Add(MainCharacter);
        }

        /// <summary>
        /// Gets the spawned Character names
        /// </summary>
        public new List<string> GetNames()
		{
			List<string> MyNames = new List<string>();
			for (int i = 0; i < Pools[0].SpawnedObjects.Count; i++)
			{
				if (Pools[0].SpawnedObjects[i])
                {
                    MyNames.Add(Pools[0].SpawnedObjects[i].name);
                }
			}
			return MyNames;
		}

        /*public void Add(Character NewCharacter)
		{
			if (MyCharacters.Contains(NewCharacter) == false)
			{
				MyCharacters.Add(NewCharacter);
			}
		}

		public void Remove(Character MyCharacter)
		{
			MyCharacters.Remove(MyCharacter);
		}*/
        #endregion
        /*public GameObject GetPlayer()
        {
            return Camera.main.gameObject;
        }*/

        #region Spawning

        // Spawn Character
        // By default the spawned character is a bot
        /*public Character SpawnBot(Vector3 Position, string ClassName, string RaceName)
		{
			Character MySummonedObject = SpawnCharacter(Zeltex.NameGenerator.GenerateVoxelName(), Position, Quaternion.identity);
			StartCoroutine(UpdateSpawned(MySummonedObject, RaceName, ClassName, MySummonedObject.name));
			return MySummonedObject;
		}
		public Character SpawnBot(Vector3 Position, int ClassIndex, int RaceIndex)
		{
			float TimeBegun = Time.realtimeSinceStartup;
			Character MySummonedObject = SpawnCharacter(Zeltex.NameGenerator.GenerateVoxelName(), Position, Quaternion.identity);
			if (MySummonedObject)
			{
				string ClassName = Zeltex.DataManager.Get().GetName("Classes", RaceIndex);
				string RaceName = Zeltex.DataManager.Get().GetName("Skeletons", RaceIndex);
				StartCoroutine(UpdateSpawned(MySummonedObject, RaceName, ClassName, MySummonedObject.name));
			}
			return MySummonedObject;
		}

		public Character SpawnCharacter()
		{
			return SpawnCharacter(DefaultCharacterName, new Vector3(), Quaternion.identity);
		}

		public Character SpawnCharacter(string Name)
		{
			return SpawnCharacter(Name, new Vector3(), Quaternion.identity);
		}
		public Character SpawnCharacter(Vector3 Position)
		{
			return SpawnCharacter(NameGenerator.GenerateVoxelName(), Position, Quaternion.identity);
		}

		public Character SpawnCharacter(string Name, Vector3 Position)
		{
			return SpawnCharacter(Name, Position, Quaternion.identity);
		}*/

        /*public Character SpawnCharacter(string Name, Vector3 Position)
		{
			return SpawnCharacter(Name, Position, Quaternion.identity);
		}

		public Character SpawnCharacter(string Name, Vector3 Position, Quaternion Rotation)
		{
			return SpawnCharacter(Name, Position, Rotation);
		}*/

        #endregion

        #region UpdateSpawnedObjects
        // default player spawn

        /*public Character SpawnCharacter(string Name, Vector3 Position, Quaternion Rotation)
		{
			Character MyCharacter = GetPoolObject();
			// Spawn the character
			if (MyCharacter != null)
			{
				MyCharacter.transform.position = Position;
				MyCharacter.transform.rotation = Rotation;
				MyCharacter.Initialize(Name);
			}
			else
			{
				Debug.LogError("Character Spawned to null. Please check prefab links and network variables.");
			}
			return MyCharacter;
		}*/

        /*public void Register(Character MyCharacter)
        {
            if (Pools.Count > 0)
            {
                if (Pools[0].SpawnedObjects.Contains(MyCharacter) == false
                    && Pools[0].PoolObjects.Contains(MyCharacter) == false)
                {
                    Pools[0].SpawnedObjects.Add(MyCharacter);
                    MyCharacter.transform.SetParent(transform);
                }
            }
            else
            {
                //Debug.LogError("Pool Count is 0");
            }
        }*/

        /// <summary>
        /// 
        /// </summary>
        /*public IEnumerator UpdateSpawned(Character MyCharacter, string RaceName, string ClassName, string MyName)
		{
			if (MyCharacter)
            {
                MyCharacter.SetClassName(ClassName);
                if (MyName != "")
                {
                    MyCharacter.name = MyName;
                }
                while (IsUpdatingCharacter)
                {
                    // wait until
                    yield return null;
                }
                IsUpdatingCharacter = true;
                string SkeletonScript = Zeltex.DataManager.Get().Get(DataFolderNames.Skeletons, RaceName);
                string ClassScript = Zeltex.DataManager.Get().Get(DataFolderNames.Classes, ClassName);
                yield return MyCharacter.RunScriptRoutineWithSkeleton(ClassScript, SkeletonScript);
				IsUpdatingCharacter = false;
			}
			else
			{
				Debug.LogError("Character was not created. It is null");
			}
		}*/

        /// <summary>
        /// Spawn the character, loads its race and class script.
        /// </summary>
        /*public IEnumerator SummonCharacter(string RaceName, string ClassName, string MyName)
		{
            string MySkeletonScript = Zeltex.DataManager.Get().Get(Zeltex.DataFolderNames.Skeletons, RaceName);
            string MyClassScript = Zeltex.DataManager.Get().Get(Zeltex.DataFolderNames.Classes, ClassName);
            // Spawn the Summoned object
            Character MyCharacter = SpawnCharacter(
                Zeltex.NameGenerator.GenerateVoxelName(),
				new Vector3(-100, -100, -100),  // away from stuff 
				Quaternion.identity);
			if (MyName != "")
			{
				MyCharacter.name = MyName;
			}
			// Disable the movement
			yield return new WaitForSeconds(0.01f);
			//MyCharacter.DontRespawn();    // if its temporary
			yield return MyCharacter.RunScriptRoutineWithSkeleton(MyClassScript, MySkeletonScript);
			//MyCharacter.SetMovement(true);
		}*/
        #endregion

        #region Dying

        /*/// <summary>
        /// Called from character when it dies
        /// </summary>
        public void OnCharacterDeath(Character MyCharacter)
        {
            StopCoroutine(RemoveCharacterFromScene(MyCharacter));   // if already removing particular character
            StartCoroutine(RemoveCharacterFromScene(MyCharacter));
		}
		/// <summary>
		/// Removes the character from the scene after a time
		/// </summary>
		private IEnumerator RemoveCharacterFromScene(Character MyCharacter)
		{
			yield return new WaitForSeconds(Random.Range(10, 30));
			ReturnObject(MyCharacter);
		}*/
        #endregion

        #region Pooling
        
        protected override Character SpawnPoolObject(int SpawnIndex, int PoolIndex = 0)
        {
            Character MyCharacter = base.SpawnPoolObject(SpawnIndex);
            if (MyCharacter && LayerManager.Get())
            {
                LayerManager.Get().SetLayerCharacter(MyCharacter.gameObject);
            }
            return MyCharacter;
        }

        public override void ReturnObject(Character PoolObject, int PoolIndex = 0)
        {
            PoolObject.OnReturnToPool.Invoke(PoolObject.gameObject);
            Ragdoll MyRagdoll = PoolObject.GetSkeleton().GetComponent<Ragdoll>();
            if (MyRagdoll)
            {
                MyRagdoll.ReverseRagdoll();
            }
            base.ReturnObject(PoolObject);
        }

        public override void ReadyObject(ReadyMessageData Data)
        {
            base.ReadyObject(Data);
            Character PoolObject = Data.SpawnedObject.gameObject.GetComponent<Character>();
            if (PoolObject)
            {
                LogManager.Get().Log("ReadyObject (Final2) " + PoolObject.GetComponent<NetworkIdentity>().netId, "PoolsReadying");
                LayerManager.Get().SetLayerCharacter(PoolObject.gameObject);
                //PoolObject.ForceInitialize();
                PoolObject.transform.position =
                    new Vector3(Random.Range(-5, 5), 5, Random.Range(-5, 5));
            }
            //PoolObject.name = "Character_" + Random.Range(1, 10000);
        }
        #endregion

        #region SerializePools
        [SerializeField]
        protected new List<CharacterPool> MyPools = new List<CharacterPool>();

        public override List<SpawnedPool<Character>> Pools
        {
            get
            {
                base.MyPools.Clear();
                for (int i = 0; i < MyPools.Count; i++)
                {
                    base.MyPools.Add(MyPools[i] as SpawnedPool<Character>);
                }
                return base.MyPools;
            }
            set
            {
                base.MyPools.Clear();
                for (int i = 0; i < value.Count; i++)
                {
                    base.MyPools.Add(value[i] as CharacterPool);
                }
            }
        }

        protected override void CreatePoolObject()
        {
            //Debug.LogError("Creating Pool.");
            MyPools.Add(new CharacterPool());
        }

        [System.Serializable]
        public class CharacterPool : SpawnedPool<Character>
        {

        }
        #endregion
    }
}

/*public static class ZeltexGameObjectExtensions
{
	public static bool IsPrefab(this Transform This)
	{
		#if UNITY_EDITOR
		var TempObject = new GameObject();
		try
		{
			TempObject.transform.parent = This.parent;
			var OriginalIndex = This.GetSiblingIndex();
			This.SetSiblingIndex(int.MaxValue);
			if (This.GetSiblingIndex() == 0) return true;
			This.SetSiblingIndex(OriginalIndex);
			return false;
		}
		finally
		{
			Object.DestroyImmediate(TempObject);
		}
		#else
		return false;
		#endif
	}
}*/