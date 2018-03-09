using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex.Voxels
{
    [ExecuteInEditMode]
    public class VoxelActionCube : MonoBehaviour
    {
        public EditorAction Build = new EditorAction();
        public EditorAction Refresh = new EditorAction();
        public World ActionWorld;
        public VoxelAction VoxelActionName;
        public Color VoxelActionColor;
        public PolyModelHandle MyHandle;

        private void RefreshHandle()
        {
            if (MyHandle == null)
            {
                MyHandle = gameObject.GetComponent<PolyModelHandle>();
                if (MyHandle == null)
                {
                    MyHandle = gameObject.AddComponent<PolyModelHandle>();
                }
            }
            VoxelMeta MyVoxel = DataManager.Get().GetElement(DataFolderNames.Voxels, VoxelActionName.DataName) as VoxelMeta;
            if (MyVoxel != null)
            {
                MyHandle.LoadVoxelMesh(DataManager.Get().GetElement(DataFolderNames.PolyModels, MyVoxel.ModelID).Clone<PolyModel>(), MyVoxel.TextureMapID);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (ActionWorld && Build.IsTriggered())
            {
                ActionWorld.ApplyActionCube(gameObject, VoxelActionName.DataName, VoxelActionColor);
            }
            if (Refresh.IsTriggered())
            {
                RefreshHandle();
            }
        }
    }

}