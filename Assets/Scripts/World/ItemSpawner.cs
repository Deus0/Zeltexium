using UnityEngine;
using System.Collections.Generic;
using Zeltex.Characters;
using Zeltex.Util;
using Zeltex.AI;
using Zeltex.Guis;
using Zeltex.Combat;
using Zeltex.Items;

namespace Zeltex.WorldUtilities
{
    /// <summary>
    /// Spawns characters in a zone.
    /// To use script, attach to a zone object.
    /// </summary>
	public class ItemSpawner : MonoBehaviour
    {
        private bool IsUseArea = true;
        [Header("Event")]
        public EventObject OnSpawn;
        [Header("Spawn Rate")]
        private int MaximumSpawns = 1;
		public float SpawnRate = 15;
		private float LastSpawned = 0f;
        [Header("Spawn Type")]
        public GameObject MyPrefab;
        public string PrefabName = "Props/HealthPickup";
        //public bool IsUseIndexes = true;
        //public int ClassIndex = 0;
        //public int RaceIndex = 0;
        //public string MyClassName = "Minion";
        //public string MyRaceName = "Human";
        public List<GameObject> MySpawns = new List<GameObject>();

        void Start()
        {
            LastSpawned = Time.time - SpawnRate;    // delay it
        }

        void Update()
        {
            Spawn();
        }

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
			MySpawns.Clear ();
		}
        /// <summary>
        /// Spawns the minions on a timer, only if they are player owned.
        /// </summary>
        void Spawn()
        {
            if (MySpawns.Count < MaximumSpawns && Time.time - LastSpawned >= SpawnRate)
            {
                LastSpawned = Time.time;
                GameObject MyItemObject = Instantiate(MyPrefab, transform.position, Quaternion.identity);
                MyItemObject.GetComponent<ItemObject>().OnDestroyed.AddEvent(Remove);
                MySpawns.Add(MyItemObject);
                /*else
                {
                }*/
            }
        }
        public void Remove(GameObject MyCharacter)
        {
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
/*public void SpawnMinion(Vector3 SpawnPosition)
{
    if (MySpawns.Count > 0)
    for (int i = MySpawns.Count-1; i >= 0; i--)
    {
        if (MySpawns [i] == null)
            MySpawns.RemoveAt (i);
    }
    LastSpawned = Time.time;
    Quaternion NewAngle = Quaternion.identity;
    NewAngle.eulerAngles = new Vector3 (0, Random.Range (0, 360), 0);
    GameObject NewSpawn;
    if (PhotonNetwork.connected)
    {
        NewSpawn = PhotonNetwork.InstantiateSceneObject (MySpawnPrefab.name, SpawnPosition, NewAngle, 0, null);
        DefaultMinionName = NewSpawn.name;
        gameObject.GetComponent<PhotonView>().RPC(
            "UpdateMinion",
            PhotonTargets.All);
    }
    else
    {
        NewSpawn = (GameObject)Instantiate (MySpawnPrefab, SpawnPosition, NewAngle);
        UpdateMinionName(NewSpawn);
    }
    //MySpawns.Add (NewSpawn);
    //OnSpawn.Invoke (NewSpawn);
}

[PunRPC]
public void UpdateMinion()
{
    GameObject MyMinion = GameObject.Find ("MinionSpawner(Clone)");
    if (MyMinion) 
    {
        UpdateMinionName(MyMinion);
    }
}
// also called when the client connects
public void UpdateMinionName(GameObject MyMinion) 
{
    if (MyMinion && MyMinion.name == "MinionSpawner(Clone)") 
    {
        MySpawns.Add (MyMinion);
        string NewName = "Minion" + MySpawns.Count;
        GameObject MinionCharacter = MyMinion.gameObject.transform.GetChild (0).gameObject;
        MinionCharacter.name = NewName;
        MyMinion.name = NewName + "_Spawner";

        if (PhotonNetwork.connected)
        {
            CharacterStats MyStats = MinionCharacter.GetComponent<CharacterStats>();
            if (MyStats)
                MyStats.SynchAllStats();
        }

        GuiSystem.GuiManager MyGui = MinionCharacter.GetComponent<GuiSystem.GuiManager> ();
        if (MyGui)
            MyGui.UpdateLabel ();
        OnSpawn.Invoke (MyMinion);
    }
}*/
