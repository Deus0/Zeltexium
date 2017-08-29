using UnityEngine;
using Zeltex.Util;

namespace Zeltex.Voxels
{
    /// <summary>
    /// Creates a Sphere shape in the world.
    /// </summary>
    public class VoxelPrimitives : ManagerBase<VoxelPrimitives>
    {
        [Header("Sphere")]
        public int VoxelType = 1;
        [SerializeField]
        private bool IsCreateNoiseSphere;
        [SerializeField]
        private float SphereRadius = 24f;
        [SerializeField]
        private Vector3 SphereOffset = new Vector3();
        [SerializeField]
        private float SphereNoiseCutoff = 0.5f;
        [SerializeField]
        private float SphereMultiplier = 2f;
        public World MyWorld;

        public void SetVoxelType(int VoxelType_)
        {
            VoxelType = VoxelType_;
            if (VoxelType == 0) // make sure not air
            {
                VoxelType = 1;
            }
        }

        public void Fill()
        {
            Fill(VoxelType);
        }
        public void Fill(int FillType)
        {
            foreach (Int3 MyKey in MyWorld.MyChunkData.Keys)
            {
                Chunk MyChunk = MyWorld.MyChunkData[MyKey];
                for (int i = 0; i < Chunk.ChunkSize; i++)
                {
                    for (int j = 0; j < Chunk.ChunkSize; j++)
                    {
                        for (int k = 0; k < Chunk.ChunkSize; k++)
                        {
                            Int3 WorldPosition = MyChunk.Position * Chunk.ChunkSize + new Int3(i, j, k);
                            MyChunk.UpdateBlockTypeMass(WorldPosition, FillType);
                        }
                    }
                }
            }
            MyWorld.OnMassUpdate();
        }
        public void CreateNoiseSphere()
        {
            Chunk MyChunk = MyWorld.MyChunkData[new Int3(0, 0, 0)];
            for (float i = -SphereRadius; i <= SphereRadius; i++)
            {
                for (float j = -SphereRadius; j <= SphereRadius; j++)
                {
                    for (float k = -SphereRadius; k <= SphereRadius; k++)
                    {
                        float MyNoise = SimplexNoise.Noise(i, j, k);
                        MyNoise = (MyNoise / SphereMultiplier +
                            SphereMultiplier * (
                            (SphereRadius - Vector3.Distance(
                                new Vector3(0, 0, 0),
                                new Vector3(i, j, k))
                                ) / SphereRadius))
                            / 2f;    // 0 - 1 * 0 to 8
                        Int3 BlockPosition = new Int3(SphereOffset.x + i, SphereOffset.y + j, SphereOffset.z + k);
                        if (MyNoise >= 1 - SphereNoiseCutoff)
                        {
                            MyWorld.UpdateBlockTypeMass(VoxelType, BlockPosition);
                        }
                        else
                        {
                            MyWorld.UpdateBlockTypeMass(0, BlockPosition);
                        }
                    }
                }
            }
            MyWorld.OnMassUpdate();
        }
        public void CreateCube()
        {
            CreateCube(MyWorld.GetWorldBlockSize().ToInt3() / 2f, MyWorld.GetWorldBlockSize().ToInt3() * 0.7f);
        }
        public void CreateCube(Int3 MyPosition, Int3 Size)    //, bool IsCentred
        {
            foreach (var MyChunk in MyWorld.MyChunkData)
            {
                if (MyChunk.Value)
                {
                    for (float i = -Size.x; i <= Size.x; i++)
                    {
                        for (float j = -Size.y; j <= Size.y; j++)
                        {
                            for (float k = -Size.z; k <= Size.z; k++)
                            {
                                /*float MyNoise = SimplexNoise.Noise(i, j, k);
                                MyNoise = (MyNoise / SphereMultiplier +
                                    SphereMultiplier * (
                                    (SphereRadius - Vector3.Distance(
                                        new Vector3(0, 0, 0),
                                        new Vector3(i, j, k))
                                        ) / SphereRadius))
                                    / 2f;    // 0 - 1 * 0 to 8
                                Int3 BlockPosition = new Int3( SphereOffset.x + i,  SphereOffset.y + j, SphereOffset.z + k);*/
                                Int3 BlockPosition = MyPosition + new Int3(i,j,k);
                                //if (MyNoise >= 1 - SphereNoiseCutoff)
                                {
                                    MyWorld.UpdateBlockTypeMass(VoxelType, BlockPosition);
                                }
                            }
                        }
                    }
                }
            }
            MyWorld.OnMassUpdate();
        }

        public void CreateSphere()
        {
            CreateSphere(MyWorld.GetWorldBlockSize() / 2f, MyWorld.GetWorldBlockSize() / 2f);
        }
        public void CreateSphere(Vector3 Position, Vector3 Size)
        {
            string VoxelName = MyWorld.MyDataBase.GetMeta(VoxelType).Name;
            Debug.Log("Creating Sphere at: " + Position.ToString() + ":" + Size.ToString() + " with type [" + VoxelName + "]");
            foreach (var MyChunk in MyWorld.MyChunkData)
            {
                if (MyChunk.Value)
                {
                    for (float i = -Size.x; i < Size.x; i++)
                    {
                        for (float j = -Size.y; j < Size.y; j++)
                        {
                            for (float k = -Size.z; k < Size.z; k++)
                            {
                                Int3 ThisPosition = new Int3(Position) + new Int3(i, j, k);
                                float Distance = Vector3.Distance(Position, ThisPosition.GetVector());
                                if (Distance < Size.x)
                                {
                                    MyWorld.UpdateBlockTypeMass(VoxelName, ThisPosition);
                                    //Debug.Log("Voxel: " + ThisPosition.ToString() + ":" + MyWorld.GetVoxelType(ThisPosition));
                                }
                                else
                                {
                                    MyWorld.UpdateBlockTypeMass("Air", ThisPosition);
                                }
                            }
                        }
                    }
                }
            }
            MyWorld.OnMassUpdate();
            //Debug.Log("After Sphere Creation:\n" + FileUtil.ConvertToSingle(MyWorld.GetScript()));
        }
        /// <summary>
        /// Create a basic tree
        /// </summary>
        public void CreateTree()
        {

        }
    }
}
