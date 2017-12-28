using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Zeltex.Voxels
{
    /// <summary>
    /// Level Part of the world
    /// </summary>
    public partial class World : MonoBehaviour
    {
        // File
        #region MetaFile
        //public GameObject MySpawnZone;

        /// <summary>
        /// Gets the script for the worlds meta data.
        /// </summary>
        public List<string> GetScriptMeta()
        {
            List<string> MyScriptList = new List<string>();
            Int3 MapSize = WorldSize;
            MyScriptList.Add("" + MapSize.x);
            MyScriptList.Add("" + MapSize.y);
            MyScriptList.Add("" + MapSize.z);
            MyScriptList.AddRange(MyLookupTable.GetScript());
            if (MyVoxelTerrain)
            {
                MyScriptList.AddRange(MyVoxelTerrain.GetScript());
            }
            return MyScriptList;
        }

        /// <summary>
        /// Load the meta information from a single file
        /// </summary>
        public void LoadLevel(Level MyLevel)
        {
            StopAllCoroutines();
            StartCoroutine(LoadLevelRoutine(MyLevel, Int3.Zero()));
        }

        /// <summary>
        /// Runs the Meta Data script. Loading things like map size.
        /// </summary>
        public IEnumerator LoadLevelRoutine(Level MyLevel, Int3 PositionOffset)
        {
            if (MyLevel != null)
            {
                if (PositionOffset == null)
                {
                    PositionOffset = Int3.Zero();
                }
                if (MyLevel.GetWorldSize() == null)
                {
                    Debug.LogError("MyLevel.GetWorldSize() is null");
                    MyLevel.SetWorldSize(Int3.Zero());
                }

                Debug.Log("Loading level with offset: " + PositionOffset.GetVector().ToString()
                    + " with size: " + MyLevel.GetWorldSize().GetVector().ToString());
                //Vector3 MapSize = new Vector3(float.Parse(MyScript[0]), float.Parse(MyScript[1]), float.Parse(MyScript[2]));
                yield return UniversalCoroutine.CoroutineManager.StartCoroutine(SetWorldSizeRoutine(MyLevel.GetWorldSize(), PositionOffset));
                if (MyLevel.GenerateTerrain())
                {
                    yield return UniversalCoroutine.CoroutineManager.StartCoroutine(MyVoxelTerrain.CreateTerrainWorldRoutine(this));
                }
                if (MyLevel.Infinite())
                {
                    Debug.Log("Setting " + name + " as infinite.");
                    if (UnityEngine.Application.isPlaying)
                    {
                        VoxelFreeRoam.Get().BeginRoaming();
                    }
                    IsHeight = true;
                }
            }
            else
            {
                Debug.LogError("Level is null");
            }
            //List<string> RestOfScript = MyScript.GetRange(3, MyScript.Count - 3);
            /*MyLookupTable.RunScript(GetMaxVoxelCount(), RestOfScript);
            if (MyVoxelTerrain)
            {
                bool DidRead = MyVoxelTerrain.RunScript(RestOfScript);
                //Debug.LogError("Did have Voxel Terrain? " + DidRead.ToString());
                if (DidRead)
                {
                    yield return MyVoxelTerrain.CreateTerrainWorldRoutine(this);
                }
            }
            else
            {
                Debug.LogError("World " + name + " has no voxel terrain.");
            }*/
        }
        // add a zone management system
        //GameObject MySpawnPosition = GameObject.Find("SpawnZone");
        /*if (MySpawnZone)
        {
            MySpawnZone.GetComponent<SpawnPositionFinder>().FindNewPosition();  // find new position
            MyScriptList.Add("" + MySpawnZone.GetComponent<SpawnPositionFinder>().IsRandom);
            MyScriptList.Add("" + MySpawnZone.transform.position.x);
            MyScriptList.Add("" + MySpawnZone.transform.position.y);
            MyScriptList.Add("" + MySpawnZone.transform.position.z);
        }
        else
        {
            MyScriptList.Add("" + false);
            MyScriptList.Add("");
            MyScriptList.Add("");
            MyScriptList.Add("");
        }*/
        //GameObject MySpawnPosition = GameObject.Find("SpawnZone");
        /*if (MySpawnZone)
        {
            MySpawnZone.GetComponent<SpawnPositionFinder>().IsRandom = bool.Parse(MyScript[3]);
            Vector3 SpawnPosition = new Vector3(
                float.Parse(MyScript[4]),
                float.Parse(MyScript[5]),
                float.Parse(MyScript[6]));
            MySpawnZone.transform.position = SpawnPosition;
        }*/
        #endregion
    }
}