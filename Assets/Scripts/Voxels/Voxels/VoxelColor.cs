using UnityEngine;
using System.Collections;

namespace Zeltex.Voxels
{
    /// <summary>
    /// A tinted voxel
    /// </summary>
    [System.Serializable]
    public class VoxelColor : Voxel
    {
        Color MyColor;

        public VoxelColor(Voxel MyVoxel)
        {
            Type = MyVoxel.GetVoxelType();
            //Light = MyVoxel.Light;
            HasUpdated = MyVoxel.GetHasUpdated();
            MyMeshData = MyVoxel.MyMeshData;
        }

        public VoxelColor(VoxelColor MyVoxelColor)
        {
            Type = MyVoxelColor.GetVoxelType();
            //Light = MyVoxel.Light;
            HasUpdated = MyVoxelColor.GetHasUpdated();
            MyMeshData = MyVoxelColor.MyMeshData;
            MyColor = MyVoxelColor.GetColor();
        }

        public VoxelColor(int NewType)
        {
            Type = NewType;
            HasUpdated = true;
            MyMeshData = new MeshData();
        }
        public VoxelColor(int NewType, MeshData NewMeshData)
        {
            Type = NewType;
            MyMeshData = NewMeshData;
        }
        public VoxelColor(int NewType, Color MyColor)
        {
            Type = NewType;
            HasUpdated = true;
            MyMeshData = new MeshData();
            SetColor(MyColor);
        }
        public void SetColor(Color MyTint)
        {
            if (MyTint != MyColor)
            {
                HasUpdated = true;  // make sure updates
                MyColor = MyTint;
            }
        }
        public bool SetType(
            Chunk MyChunk,
            int x,
            int y,
            int z, int
            NewType,
            Color MyTint)
        {
            /*Debug.LogError("[SetType] Voxel being set to " + MyTint.ToString());
            byte NewR = (byte)((int)(MyTint.r));
            byte NewG = (byte)MyTint.g;
            byte NewB = (byte)MyTint.b;
            Debug.LogError("[SetType] MyTintColor1 RGB= " + NewR.ToString() + "," + NewG.ToString() + "," + NewB.ToString());*/
            if (MyChunk &&
                (Type != NewType || MyColor != MyTint))
            {
                Type = NewType;
                //UpdateSurroundings(MyChunk, x, y, z);
                /*R = NewR;
                G = NewG;
                B = NewB;*/
                MyColor = MyTint;
                return true;
            }
            return false;
        }
        public Color GetColor()
        {
            return MyColor;
        }
    }
}