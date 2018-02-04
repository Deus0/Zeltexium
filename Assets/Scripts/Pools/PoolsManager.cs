using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Reflection;

namespace Zeltex
{
    public class PoolsManager : ManagerBase<PoolsManager>
    {
        //public UnityEvent SpawnPools;
        public UnityEvent SynchPools;
        public UnityEvent ClearPools;
        public List<Component> MyPools = new List<Component>();
        public Guis.LoadingGui MyLoadGui;

        public void SpawnPools(System.Action OnFinishedSpawning)
        {
            RoutineManager.Get().StartCoroutine(SpawnPoolsRoutine(OnFinishedSpawning));
        }

        public IEnumerator SpawnPoolsRoutine(System.Action OnFinishedSpawning)
        {
            MyLoadGui.TurnOn("Loading Objects");
            for (int i = 0; i < MyPools.Count; i++)
            {
                Type PoolType = MyPools[i].GetType();
                bool HasSpawned = false;
                System.Action OnFinishThisSpawn = () =>
                {
                    HasSpawned = true;
                };
                MethodInfo MyMethod = PoolType.GetMethod("SpawnPools");
                object[] ParametersArray = new object[] { OnFinishThisSpawn };
                //Debug.LogError(i + " Invoking SpawnPools with type: " + PoolType.ToString());
                MyMethod.Invoke(MyPools[i], ParametersArray);
                while (!HasSpawned) 
                {
                    yield return null;
                }
            }
            if (OnFinishedSpawning != null) 
            {
                OnFinishedSpawning.Invoke();
            }
            MyLoadGui.TurnOff();
        }
    }
}