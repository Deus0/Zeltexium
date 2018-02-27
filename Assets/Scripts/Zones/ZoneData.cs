using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Zeltex
{
    /// <summary>
    /// Each zone has a size, shape, position
    /// A zone has properties:
    ///     Spawning
    ///     Quest Objective
    ///     Gravity
    ///     VisualEnhancement (like post processing) - with toast UI 'You have entered a dark zone'
    ///     Stats Buff Zone 'all light stats will decreae by 50%'
    /// </summary>
    public class ZoneData : ElementCore
    {
        [JsonProperty]
        public Int3 Size = new Int3(1, 1, 1);
        [JsonProperty]
        public string Type = "Spawner";
        [JsonIgnore]
        public Zone MyZone;

        /// <summary>
        /// Called before saving, to check any property updates
        /// </summary>
        public void OnPreSave()
        {

        }

        #region Spawning

        public override void Spawn()
        {
            if (MyZone == null)
            {
                GameObject ZoneObject = new GameObject();
                ZoneObject.name = Name;// + "-Handler";
                MyZone = ZoneObject.AddComponent<Zone>();
                MyZone.SetData(this);
                ZoneManager.Get().Add(MyZone);
            }
            else
            {
                Debug.LogError("Trying to spawn when handler already exists for: " + Name);
            }
        }

        /// <summary>
        /// Spawns a zone in the level as a clone
        /// </summary>
        public void SpawnInLevel(string LevelName)
        {
            Level MyLevel = DataManager.Get().GetElement(DataFolderNames.Levels, LevelName) as Level;
            ZoneData InstancedZone = this.Clone<ZoneData>();
            InstancedZone.SpawnInLevel(MyLevel);
        }

        public void SpawnInLevel(Level SpawnLevel)
        {
            if (SpawnLevel != null && SpawnLevel.HasSpawned())
            {
                Spawn();
                SpawnLevel.Zones.Add(MyZone);
            }
        }

        public override void DeSpawn()
        {
            if (MyZone)
            {
                MyZone.gameObject.Die();
            }
        }

        public override bool HasSpawned()
        {
            return (MyZone != null);
        }
        #endregion
    }

}