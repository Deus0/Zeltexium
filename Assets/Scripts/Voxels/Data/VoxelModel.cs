using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Util;
using Newtonsoft.Json;

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
		[JsonIgnore]
		public bool IsLoadingFromFile;

        public void UseScript(string NewData)
        {
            VoxelData = NewData;
        }

        public void UseWorld(World MyWorld) 
		{
			VoxelData = FileUtil.ConvertToSingle(MyWorld.GetScript());
			Name = MyWorld.name;
        }
        #region Spawning
        public World MyWorld;

        public override void Spawn()
        {
            if (MyWorld == null)
            {
                GameObject NewWorld = new GameObject();
                NewWorld.name = Name + "-Handler";
                MyWorld = NewWorld.AddComponent<World>();
                MyWorld.VoxelScale = new Vector3(0.01f, 0.01f, 0.01f);
                MyWorld.RunScript(FileUtil.ConvertToList(VoxelData));
            }
            else
            {
                Debug.LogError("Trying to spawn when handler already exists for: " + Name);
            }
        }

        public override void DeSpawn()
        {
            if (MyWorld)
            {
                MyWorld.gameObject.Die();
            }
        }

        public override bool HasSpawned()
        {
            return (MyWorld != null);
        }
        #endregion
    }
}
