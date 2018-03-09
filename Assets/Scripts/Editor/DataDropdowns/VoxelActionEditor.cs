using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Zeltex.Voxels;

namespace Zeltex
{

    [CustomPropertyDrawer(typeof(VoxelAction))]
    public class VoxelActionEditor : ElementActionEditor<VoxelMeta, VoxelAction>
    {
        protected override void SetName(string NewName)
        {
            MyAction.DataName = NewName;
        }
    }
}