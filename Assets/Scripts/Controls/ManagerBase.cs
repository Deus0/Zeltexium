using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Zeltex
{
    /// <summary>
    /// the base of all manager classes
    /// </summary>
    public class ManagerBase<T> : MonoBehaviour
    {
        public static T MyManager;

        protected virtual void Awake()
        {
            MyManager = gameObject.GetComponent<T>();
        }

        public static T Get()
        {
            return MyManager;
        }
    }

}