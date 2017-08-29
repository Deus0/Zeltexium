using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Zeltex
{
    public class PoolsManager : ManagerBase<PoolsManager>
    {
        public UnityEvent SpawnPools;
        public UnityEvent SynchPools;
        public UnityEvent ClearPools;
    }

}