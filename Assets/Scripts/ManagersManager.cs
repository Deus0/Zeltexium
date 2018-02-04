using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex
{
    /// <summary>
    /// The base of all
    /// </summary>
    public class ManagersManager : ManagerBase<ManagersManager>
    {
        public List<MonoBehaviour> MyManagers = new List<MonoBehaviour>();

        /// <summary>
        /// Get yer manager here!
        /// </summary>
        public T GetManager<T>(string ManagerName) where T : MonoBehaviour
        {
            for (int i = 0; i < MyManagers.Count; i++)
            {
                if (MyManagers[i].name == ManagerName)
                {
                    return MyManagers[i] as T;
                }
            }
            return null;
        }
    }

}