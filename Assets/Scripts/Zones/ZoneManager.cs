using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.WorldUtilities;

namespace Zeltex
{
    /// <summary>
    /// Manages zones
    /// TODO:
    /// Link Zone to world - Spawn objects just in that world
    /// </summary>
    public class ZoneManager : ManagerBase<ZoneManager>
    {
        [SerializeField]
        private List<Zone> Zones = new List<Zone>();
        private bool IsSpawnersEnabled = false;

        public ZoneSpawner GetZoneSpawner()
        {
            if (Zones.Count > 0)
            {
                return Zones[0] as ZoneSpawner;
            }
            else
            {
                return null;
            }
        }

        public bool ToggleSpawnersEnabled()
        {
            SetSpawnersEnabled(!IsSpawnersEnabled);
            return IsSpawnersEnabled;
        }

        public void SetSpawnersEnabled(bool NewState)
        {
            if (IsSpawnersEnabled != NewState)
            {
                IsSpawnersEnabled = NewState;
                for (int i = 0; i < Zones.Count; i++)
                {
                    ZoneSpawner MySpawner = Zones[i] as ZoneSpawner;
                    if (MySpawner)
                    {
                        MySpawner.enabled = IsSpawnersEnabled;
                    }
                }
            }
        }

        // WorldUtilities.ZoneSpawner.Get().SetWorld(this);
    }

}