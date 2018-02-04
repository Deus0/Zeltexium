using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex.Voxels
{
    /// <summary>
    /// Spawns an individual voxel
    /// Voxel Chest - animated
    /// Voxel Turret
    /// Voxel Sign
    /// Voxel Furnace
    /// </summary>
    public class VoxelHandle : MonoBehaviour
    {
        public VoxelMeta MyVoxel;
        public Voxel MyVoxelInstance;

        /// <summary>
        /// Load a voxel using a meta file
        /// </summary>
        public void Load(VoxelMeta NewVoxel)
        {
            MyVoxel = NewVoxel;
        }
    }
}