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

        #region Spawning

        public override void Spawn()
        {
            if (MyZone == null)
            {
                GameObject NewWorld = new GameObject();
                NewWorld.name = Name;// + "-Handler";
                MyZone = NewWorld.AddComponent<Zone>();
                MyZone.SetData(this);
            }
            else
            {
                Debug.LogError("Trying to spawn when handler already exists for: " + Name);
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