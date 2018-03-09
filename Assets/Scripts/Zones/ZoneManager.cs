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

        public new static ZoneManager Get()
        {
            if (MyManager == null)
            {
                MyManager = ManagersManager.Get().GetManager<ZoneManager>(ManagerNames.ZoneManager);
            }
            return MyManager as ZoneManager;
        }

        public void Add(Zone NewZone)
        {
            if (Zones.Contains(NewZone) == false)
            {
                NewZone.transform.SetParent(ZoneManager.Get().transform);
                Zones.Add(NewZone);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < Zones.Count; i++)
            {
                if (Zones[i])
                {
                    Zones[i].gameObject.Die();
                }
            }
            Zones.Clear();
        }

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

        public void ReturnObject(Zone MyZone)
        {
            if (MyZone)
            {
                MyZone.gameObject.Die();
            }
        }
    }

}