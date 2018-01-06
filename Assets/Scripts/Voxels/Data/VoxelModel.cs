using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Util;

namespace Zeltex.Voxels
{
    /// <summary>
    /// The main polygonal mesh data for a voxel
    /// Also contains rules needed for placing in the voxel grid
    /// </summary>
    [System.Serializable]
    public class VoxelModel : Element
    {
        public string VoxelData = "";
		[Newtonsoft.Json.JsonIgnore]
		public bool IsLoadingFromFile;

		public VoxelModel() 
		{

		}

        public VoxelModel(string NewData)
        {
            VoxelData = NewData;
        }

		public void UseWorld(World MyWorld) 
		{
			VoxelData = FileUtil.ConvertToSingle(MyWorld.GetScript());
			Name = MyWorld.name;
		}
    }
}
