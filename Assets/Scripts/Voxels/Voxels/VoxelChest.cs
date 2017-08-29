using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Items;

namespace Zeltex.Voxels
{
    /// <summary>
    /// A voxel with an inventory!
    /// </summary>
    [System.Serializable]
    public class VoxelChest : Voxel
    {
        [SerializeField]
        public Inventory ChestItems;


    }
}