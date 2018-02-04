using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Voxels;

namespace Zeltex.Skeletons
{
    public class VoxelExplosion : MonoBehaviour
    {
        public bool IsExplodeSkeleton;
        bool IsExplodeAll = false;
        Skeleton MySkeleton;
        List<Vector3> MyBlockPositions = new List<Vector3>();
        List<World> MyBlockWorlds = new List<World>();
        public float MinimumDelay = 0.25f;
        public float MaximumDelay = 1.25f;
        public float BlocksPerDropPercent = 0.003f;   // percentage of total blocks

        void Update()
        {
            if (IsExplodeSkeleton)
            {
                IsExplodeSkeleton = false;
                Explodes();
            }
        }

        public void Explodes()
        {
            MySkeleton = GetComponent<Skeleton>();
            CheckSkeleton(MySkeleton);
            StartCoroutine(Explode());
        }
        void CheckSkeleton(Skeleton MySkeleton)
        {
            for (int i = 0; i < MySkeleton.MyBones.Count; i++)
            {
                if (MySkeleton.MyBones[i].VoxelMesh)
                {
                    if (MySkeleton.MyBones[i].VoxelMesh.GetComponent<World>())
                    {
                        CheckWorld(MySkeleton.MyBones[i].VoxelMesh.GetComponent<World>());
                    }
                }
            }
        }
        void CheckWorld(World MyWorld)
        {
            // for every chunk
            foreach (Int3 MyKey in MyWorld.MyChunkData.Keys)
            {
                Chunk MyChunk = MyWorld.MyChunkData[MyKey];
                //Chunk MyChunk = MyWorld.MyChunkData.Values[a];
                for (int i = 0; i < Chunk.ChunkSize; i++)
                    for (int j = 0; j < Chunk.ChunkSize; j++)
                        for (int k = 0; k < Chunk.ChunkSize; k++)
                        {
                            Int3 ThisPosition = new Int3(i, j, k);
                            // if block exists and is not air, remove block
                            if (MyChunk.GetVoxel(ThisPosition).GetVoxelType() != 0)
                            {
                                Int3 WorldPosition = MyChunk.Position * Chunk.ChunkSize + ThisPosition;
                                if (IsExplodeAll)
                                {
                                    MyWorld.UpdateBlockTypeMass("Air", WorldPosition);   //UpdateBlockTypeSize
                                }
                                else
                                {
                                    MyBlockPositions.Add(WorldPosition.GetVector());
                                    MyBlockWorlds.Add(MyWorld);
                                }
                            }
                        }
            }
            if (IsExplodeAll)
            {
                MyWorld.OnMassUpdate();
            }
        }
        IEnumerator Explode()
        {
            // for every chunk
            int BlocksPerDrop = Mathf.CeilToInt(BlocksPerDropPercent * MyBlockPositions.Count);
            for (int i = MyBlockPositions.Count-1; i >= 0; i -= BlocksPerDrop)
            {
                List<World> WorldsUpdated = new List<World>();
                for (int j = 0; j < BlocksPerDrop; j++)
                {
                    int RemoveIndex = Mathf.RoundToInt(Random.Range(0, MyBlockPositions.Count - 1));
                    if (RemoveIndex >= 0 && RemoveIndex < MyBlockWorlds.Count)
                    {
                        // get random position
                        MyBlockWorlds[RemoveIndex].UpdateBlockTypeMass("Air", new Int3(MyBlockPositions[RemoveIndex]));   //UpdateBlockTypeSize
                        bool IsInList = false;
                        for (int k = 0; k < WorldsUpdated.Count; k++)
                        {
                            if (MyBlockWorlds[RemoveIndex] == WorldsUpdated[k])
                            {
                                IsInList = true;
                                break;
                            }
                        }
                        if (!IsInList)
                        {
                            WorldsUpdated.Add(MyBlockWorlds[RemoveIndex]);
                        }
                        MyBlockWorlds.RemoveAt(RemoveIndex);
                        MyBlockPositions.RemoveAt(RemoveIndex);
                    }
                }
                // For all worlds updated, propogate update
                for (int k = 0; k < WorldsUpdated.Count; k++)
                {
                    WorldsUpdated[k].OnMassUpdate();
                }
                WorldsUpdated.Clear();
                yield return new WaitForSeconds(Random.Range(MinimumDelay, MaximumDelay));
            }
        }
    }

}
/*for (int a = 0; a < MyWorld.MyChunkData.Values.Count; a++)
{
    Chunk MyChunk = MyWorld.MyChunkData.Values[a];
    for (int i = 0; i < Chunk.ChunkSize; i++)
        for (int j = 0; j < Chunk.ChunkSize; j++)
            for (int k = 0; k < Chunk.ChunkSize; k++)
            {
                // if block exists and is not air, remove block
                if (MyChunk.GetVoxel(i, j, k).GetBlockIndex() != 0)
                {
                    MyWorld.UpdateBlockTypeSize(new Vector3(
                        MyChunk.Position.x * Chunk.ChunkSize + i, 
                        MyChunk.Position.y * Chunk.ChunkSize + j, 
                        MyChunk.Position.z * Chunk.ChunkSize + k), 
                        0);
                    yield return new WaitForSeconds(Random.RandomRange(0.1f,0.5f));
                    //MyChunk.UpdateBlockTypeWorldPosition(new Vector3(i, j, k), 0);
                }
            }

    // mass update chunk

}*/
