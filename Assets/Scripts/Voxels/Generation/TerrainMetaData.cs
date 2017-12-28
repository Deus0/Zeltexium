using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Zeltex.Voxels
{
    /// <summary>
    /// Contains the data needed for terrain generation.
    /// </summary>
	[System.Serializable]
    public class TerrainMetaData : Element
    {
        [SerializeField]
        public float BaseHeight = 16;
        [SerializeField]
        public float MinimumHeight = 1;
        [SerializeField]
        public float Amplitude = 0.5f;
        [SerializeField]
        public float Frequency = 0.05f;
        [SerializeField]
        public Vector3 WorldOffset = new Vector3();
        [Header("Dirt")]
        public int BlockType1 = 2;
        public int BlockType2 = 3;
        public int BottomFloorBlock = 4;
        [Header("Trees")]
        public bool IsTrees = true;
        public int TreeBlock = 4;
        public int LeafBlock = 5;
        public int TreeHeight = 4;
        public int LeavesSize = 1;
        public float TreeFrequency = 0.1f;
        public float TreeFrequencyCutoff = 0.95f;
        [Header("BlockNames")]
        [JsonIgnore]
        public string DirtName = "Air";
        [JsonIgnore]
        public string GrassName = "Air";
        [JsonIgnore]
        public string BedrockName = "Air";
        [JsonIgnore]
        public string WoodName = "Air";
        [JsonIgnore]
        public string LeafName = "Air";

        public TerrainMetaData()
        {

        }

        // later add random stuff here
        public int GetDirtType()
        {
            return BlockType1;
        }
        public int GetGrasstype()
        {
            return BlockType2;
        }

        public void SetNames(VoxelManager MyVoxelManager)
        {
            DirtName = MyVoxelManager.GetMetaName(GetDirtType());
            GrassName = MyVoxelManager.GetMetaName(GetGrasstype());
            BedrockName = MyVoxelManager.GetMetaName(BottomFloorBlock);
            WoodName = MyVoxelManager.GetMetaName(TreeBlock);
            LeafName = MyVoxelManager.GetMetaName(LeafBlock);
        }
    }
}
