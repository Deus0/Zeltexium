using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Zeltex.Audio;
using Zeltex.Characters;
using Zeltex.Guis;
using Zeltex.Guis.Characters;
using Zeltex.Voxels;
using Zeltex.Items;
using Zeltex.Sounds;

namespace Zeltex.Editor
{
    /// <summary>
    /// Creates the level objects
    /// </summary>
    public class EditorCreateLevel : MonoBehaviour
    {

        // Add a menu item named "Do Something" to MyMenu in the menu bar.
        [MenuItem("Zeltex/CreateLevel")]
        static void DoSomething()
        {
            Debug.Log("Creating Level from Prefabs");
            SpawnManager<DataManager>();
            SpawnManager<GameManager>();
            SpawnManager<LightManager>();
            SpawnManager<CameraManager>();
            SpawnManager<CharacterManager>();
            SpawnManager<CharacterGuiManager>();
            SpawnManager<GuiSpawner>();
            SpawnManager<ZoneManager>();
            SpawnManager<WorldManager>();
            SpawnManager<ItemManager>();
            SpawnManager<BulletPool>();
            SpawnManager<SoundManager>();
            //SpawnManager<MusicManager>();
            //SpawnManager<PopupManager>();
            //SpawnManager<WaypointsManager>();
            //SpawnManager<ViewersManager>();
        }

        public static void SpawnManager<T>(string ManagerName = "")
        {
            GameObject MyManagerObject;
            string SeekName = typeof(T).Name;
            if (ManagerName != "")
            {
                MyManagerObject = GameObject.Find(ManagerName);
            }
            else
            {
                MyManagerObject = GameObject.Find(SeekName);
            }
            if (MyManagerObject == null)
            {
                if (SeekName.Contains("Manager"))
                {
                    SeekName = SeekName.Substring(0, SeekName.IndexOf("Manager"));
                    if (SeekName[SeekName.Length - 1] != 's')
                    {
                        SeekName += 's';
                    }
                }
                else if (SeekName.Contains("Pool"))
                {
                    SeekName = SeekName.Substring(0, SeekName.IndexOf("Pool"));
                    if (SeekName[SeekName.Length - 1] != 's')
                    {
                        SeekName += 's';
                    }
                }
                MyManagerObject = GameObject.Find(SeekName);
            }
            if (MyManagerObject == null)
            {
                Debug.Log("Creating a new " + SeekName);
                // After spawn parent to Spawns thing, maybe?
                string PrefabPath = "Prefabs/Managers/" + SeekName;
                GameObject MyPrefab = AssetDatabase.LoadAssetAtPath(PrefabPath, typeof(T)) as GameObject;
                if (MyPrefab)
                {
                    //PrefabUtility.InstantiatePrefab
                    Debug.LogError("Success Finding Prefab!");
                }
                else
                {
                    Debug.LogError("Could not find Manager at path: " + PrefabPath);
                }
            }
            else
            {
                Debug.Log(MyManagerObject.name + " Exists.");
            }
        }
    }
}