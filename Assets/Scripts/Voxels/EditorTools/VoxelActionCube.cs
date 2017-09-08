using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex.Voxels
{
    [ExecuteInEditMode]
    public class VoxelActionCube : MonoBehaviour
    {
        public EditorAction Build;
        public World ActionWorld;
        public string VoxelActionName;
        public Color VoxelActionColor;

        // Update is called once per frame
        void Update()
        {
            if (ActionWorld && Build.IsTriggered())
            {
                ActionWorld.ApplyActionCube(gameObject, VoxelActionName, VoxelActionColor);
            }
        }
    }

}