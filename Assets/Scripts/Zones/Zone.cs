using UnityEngine;
using UnityEngine.Networking;
using Zeltex.Util;
using System.Collections.Generic;

namespace Zeltex
{
    /// <summary>
    /// Base for a zone
    /// </summary>
    public class Zone : NetworkBehaviour
    {
        protected NetworkIdentity MyNetworkIdentity;
        [Header("In the Zone")]
        [SerializeField]
        protected ZoneData Data;
        [SerializeField]
        protected bool IsSpawner = false;
        public List<Transform> Spawns = new List<Transform>();
        private float TimeSinceSpawned;

        public ZoneData GetData()
        {
            return Data;
        }

        public void SetData(ZoneData NewData)
        {
            if (Data != NewData)
            {
                Debug.Log("Setting new data for zone: " + name);
                Data = NewData;
                // initialize data
                if (Data != null)
                {
                    Data.SetZone(this);
                    IsSpawner = (Data.Type == "Spawner");
                }
                else
                {
                    // default variables
                    IsSpawner = false;
                }
            }
            else
            {
                Debug.LogError("Cannot set data for zone.");
            }
        }

        // Use this for initialization
        void Awake()
        {
            //MyMeshRenderer = GetComponent<MeshRenderer>();
            MyNetworkIdentity = GetComponent<NetworkIdentity>();
            TimeSinceSpawned = Time.time + 5;
        }

        protected Vector3 GetRandomPosition()
        {
            Vector3 MySize = transform.lossyScale;
            return transform.position + new Vector3(
                        Random.Range(-MySize.x / 2f, MySize.x / 2f),
                        Random.Range(-MySize.y / 2f, MySize.y / 2f),
                        Random.Range(-MySize.z / 2f, MySize.z / 2f));
            /*Bounds MyBounds = MyMeshRenderer.bounds;
            return transform.position + new Vector3(
                Random.Range(MyBounds.min.x, MyBounds.max.x),
                Random.Range(MyBounds.min.y, MyBounds.max.y),
                Random.Range(MyBounds.min.z, MyBounds.max.z));*/
        }

        private void Update()
        {
            if (IsSpawner)
            {
                UpdateSpawning();
            }
        }

        private bool CanSpawn()
        {
            return (Spawns.Count > 0 && Time.time - TimeSinceSpawned >= 15);
        }

        private void UpdateSpawning()
        {
            if (CanSpawn())
            {
                CharacterData CharacterData = DataManager.Get().GetElement(DataFolderNames.Characters, 0).Clone<CharacterData>();
                Characters.Character NewCharacter = CharacterData.Spawn(Data.Position.InLevel);
                Spawns.Add(NewCharacter.transform);
                NewCharacter.transform.position = transform.position;
            }
        }
    }

}