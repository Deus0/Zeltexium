using UnityEngine;
using System.Collections;

namespace Zeltex.Voxels
{
    //[ExecuteInEditMode]
    public class VoxelPlane : MonoBehaviour
    {
        // Plane
        [Header("Plane Generation")]
        [SerializeField] private bool IsCreatePlane;
        //[SerializeField] private int CreatePlaneWidth = 5;
        //[SerializeField] private int CreatePlaneDepth = 5;
        //[SerializeField] private int PlanePosition = 0;
        // Cube
        [Header("Cube Generation")]
        [SerializeField] private bool IsCreateCube;
        //[SerializeField] private int CreateCubeHeight = 5;
        public float PillarSpawnChance = 90;
        public void CreateRoom(World MyWorld, float PlaneHeight, int PillarType, int GroundType, int WallType)
        {
            foreach (var MyChunk in MyWorld.MyChunkData)
            {
                if (MyChunk.Value)
                {
                    int PlaneOffsetX = Mathf.RoundToInt(MyWorld.GetWorldSizeChunks().x * Chunk.ChunkSize - PlaneHeight);
                    int PlaneOffsetY = Mathf.RoundToInt(MyWorld.GetWorldSizeChunks().y * Chunk.ChunkSize - PlaneHeight);
                    int PlaneOffsetZ = Mathf.RoundToInt(MyWorld.GetWorldSizeChunks().z * Chunk.ChunkSize - PlaneHeight);
                    GenerateRoom(MyChunk.Value, PlaneHeight, GroundType, WallType, 0, false, false);
                    GenerateRoom(MyChunk.Value, PlaneHeight, GroundType, WallType, PlaneOffsetY, false, false);
                    //Debug.LogError("PlaneOffsetX: " + PlaneOffsetX);
                    GenerateRoom(MyChunk.Value, PlaneHeight,  GroundType, WallType, 0, true, false);
                    GenerateRoom(MyChunk.Value, PlaneHeight,  GroundType, WallType, PlaneOffsetX, true, false);   // build opposite wall
                    GenerateRoom(MyChunk.Value, PlaneHeight,  GroundType, WallType, 0, false, true);
                    GenerateRoom(MyChunk.Value, PlaneHeight,  GroundType, WallType, PlaneOffsetZ, false, true);
                    if (MyChunk.Value.Position.y == 0)
                    {
                        for (int i = 1; i < Chunk.ChunkSize-1; i++)
                        {
                            for (int k = 1; k < Chunk.ChunkSize - 1; k++)
                            {
                                if (Random.Range(0, 100) > PillarSpawnChance)  // Random Pillars
                                {
                                    for (int j = 1; j < MyWorld.GetWorldBlockSize().y - 1; j++)
                                    {
                                        MyChunk.Value.UpdateBlockTypeMass(new Int3(i, j, k), PillarType);
                                    }
                                }
                                /*else
                                {
                                    MyChunk.Value.UpdateBlockTypeMass(i, Chunk.ChunkSize * 3 - 1, k, PillarType);
                                }*/
                            }
                        }
                    }
                    MyChunk.Value.OnMassUpdate();
                }
            }
        }

        public void GenerateRoom(Chunk MyChunk, float PlaneHeight, int GroundType, int WallType)
        {
            GenerateRoom(MyChunk, PlaneHeight, GroundType, WallType, 0, false, false);
        }

        public void GenerateRoom(Chunk MyChunk, float PlaneHeight, int GroundType, int WallType, int PlaneOffset, bool IsWallX, bool IsWallZ)
        {
            for (int i = 0; i < Chunk.ChunkSize; i++)
            {
                for (int j = 0; j < Chunk.ChunkSize; j++)
                {
                    for (int k = 0; k < Chunk.ChunkSize; k++)
                    {
                        //int MyBlockType = Mathf.FloorToInt(Random.RandomRange(UpdateType, UpdateType + 2));
                        Vector3 MyWorldPosition = MyChunk.Position.GetVector() * Chunk.ChunkSize + new Vector3(i, j, k);    // block position in world
                        if (!IsWallX && !IsWallZ && 
                            MyWorldPosition.y <= PlaneHeight + PlaneOffset && MyWorldPosition.y >= PlaneOffset)
                        {
                            MyChunk.UpdateBlockTypeMass(new Int3(i, j, k), GroundType);
                        }
                        // Walls
                        else if (IsWallX &&
                            MyWorldPosition.x <= PlaneHeight + PlaneOffset && MyWorldPosition.x >= PlaneOffset)
                        {
                            MyChunk.UpdateBlockTypeMass(new Int3(i, j, k), WallType);
                        }
                        else if (IsWallZ && MyWorldPosition.z <= PlaneHeight + PlaneOffset && MyWorldPosition.z >= PlaneOffset)
                        {
                            MyChunk.UpdateBlockTypeMass(new Int3(i, j, k), WallType);
                        }
                    }
                }
            }
        }

       /* public void CreatePlane(int y, int Type)
        {
            for (int x = 0; x < CreatePlaneWidth; x++)
            {
                for (int z = 0; z < CreatePlaneDepth; z++)
                {
                    MyWorld.UpdateBlockType(x, y, z, Type);
                }
            }
        }*/
    }
}
