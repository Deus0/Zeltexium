using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Zeltex
{

    /// <summary>
    /// Names of all Zeltex Managers
    /// </summary>
    public static class ManagerNames
    {
        public static string CharacterManager = "Characters";
        public static string CharacterGuiManager = "CharacterGuis";
        public static string CameraManager = "Cameras";
        public static string SoundManager = "Sounds";
        public static string ZoneManager = "Zones";
        public static string WayPointManager = "WayPoints";
        public static string BulletManager = "Bullets";
        public static string ItemManager = "Items";
    }

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