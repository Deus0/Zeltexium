using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Skeletons;

namespace Zeltex.Voxels
{
    /// <summary>
    /// Debug lists for voxels!
    /// </summary>
    public class VoxelDebugger : MonoBehaviour
    {
        public static bool IsDebug = true;
        public static VoxelDebugger MyVoxelDebug = null;
        public List<string> EnteredNewChunks = new List<string>();
        public List<string> MovedChunks = new List<string>();

        public static VoxelDebugger Get()
        {
            if (MyVoxelDebug == null)
            {
                MyVoxelDebug = GameObject.Find("WorldUpdater").GetComponent<VoxelDebugger>();
            }
            return MyVoxelDebug;
        }

        public void AddNewChunk(string Data)
        {
            EnteredNewChunks.Add(Data);
            if (EnteredNewChunks.Count > 10)
            {
                EnteredNewChunks.RemoveAt(0);
            }
        }
        public void ClearMoveChunks()
        {
            MovedChunks.Clear();
        }
        public void AddMoveChunk(string Data)
        {
            MovedChunks.Add(Data);
            if (MovedChunks.Count > 15)
            {
                MovedChunks.RemoveAt(0);
            }
        }


    }
}