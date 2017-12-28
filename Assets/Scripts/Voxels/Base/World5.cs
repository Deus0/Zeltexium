using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex.Voxels
{
    /// <summary>
    /// Positioning voxel stuff
    /// </summary>
    public partial class World : MonoBehaviour
    {
        #region Getters

        /// <summary>
        /// Gets a chunk at a chunk position. Using Integers.
        /// </summary>
        public Chunk GetChunk(Int3 ChunkPosition)
        {
            if (MyChunkData.ContainsKey(ChunkPosition))
            {
                return MyChunkData[ChunkPosition];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the voxel name at a position
        /// </summary>
        public string GetVoxelName(Int3 BlockPosition)
        {
            Voxel MyVoxel = GetVoxel(BlockPosition);
            if (MyVoxel == null)
            {
                return "Air";
            }
            else
            {
                return MyLookupTable.GetName(MyVoxel.GetVoxelType());
            }
        }


        /// <summary>
        /// Gets the type of a voxel
        /// </summary>
        public int GetVoxelType(Int3 BlockPosition)
        {
            Voxel MyVoxel = GetVoxel(BlockPosition);
            if (MyVoxel == null)
            {
                return 0;
            }
            else
            {
                return MyVoxel.GetVoxelType();
            }
        }

        /// <summary>
        /// Returns the colour of a voxel
        /// </summary>
        public Color GetVoxelColor(Int3 WorldPosition)
        {
            Chunk MyChunk = GetChunkWorldPosition(WorldPosition);
            if (MyChunk == null)
            {
                return Color.white;
            }
            else
            {
                Int3 MyBlockPosition = WorldToBlockInChunkPosition(WorldPosition);
                return MyChunk.GetVoxelColor(MyBlockPosition);
            }
        }

        /// <summary>
        /// Get a voxel at a position in the world
        /// </summary>
        public Voxel GetVoxel(Int3 WorldPosition)
        {
            // if world position, get its chunk position
            MyChunk = GetChunkWorldPosition(WorldPosition);
            if (MyChunk != null)
            {
                return MyChunk.GetVoxel(WorldToBlockInChunkPosition(WorldPosition));// InChunkPosition.x, InChunkPosition.y, InChunkPosition.z);
            }
            else
            {
                return null;
            }
        }
        #endregion
        
        #region Positioning

        /// <summary>
        /// Gets chunk using a world position
        /// </summary>
        public Chunk GetChunkWorldPosition(Int3 MyWorldPosition)
        {
            Int3 ChunkPosition = WorldToChunkPosition(MyWorldPosition);
            if (MyChunkData.ContainsKey(ChunkPosition))
            {
                return MyChunkData[ChunkPosition];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// The chunk position of the block
        /// </summary>
        public Int3 WorldToChunkPosition(Int3 WorldPosition)
        {
            int ChunkPositionX = (WorldPosition.x / Chunk.ChunkSize);
            int ChunkPositionY = (WorldPosition.y / Chunk.ChunkSize);
            int ChunkPositionZ = (WorldPosition.z / Chunk.ChunkSize);
            if (WorldPosition.x < 0 && WorldPosition.x % Chunk.ChunkSize != 0)
            {
                ChunkPositionX--;
            }
            if (WorldPosition.y < 0 && WorldPosition.y % Chunk.ChunkSize != 0)
            {
                ChunkPositionY--;
            }
            if (WorldPosition.z < 0 && WorldPosition.z % Chunk.ChunkSize != 0)
            {
                ChunkPositionZ--;
            }
            Int3 MyChunkPosition = new Int3(ChunkPositionX, ChunkPositionY, ChunkPositionZ);
            return MyChunkPosition;
        }

        /// <summary>
        /// The position inside the chunk - 0 to 15
        /// </summary>
        public Int3 WorldToBlockInChunkPosition(Int3 WorldPosition)
        {
            Int3 InChunkPosition = new Int3();
            if (WorldPosition.x < 0)
            {
                InChunkPosition.x = Chunk.ChunkSize - 1 - (Mathf.CeilToInt(Mathf.Abs(WorldPosition.x + 1)) % Chunk.ChunkSize); // Reversing minus direction!
            }
            else
            {
                InChunkPosition.x = Mathf.RoundToInt(WorldPosition.x) % Chunk.ChunkSize;
            }
            if (WorldPosition.y < 0)
            {
                InChunkPosition.y = Chunk.ChunkSize - (Mathf.CeilToInt(Mathf.Abs(WorldPosition.y)) % Chunk.ChunkSize);
            }
            else
            {
                InChunkPosition.y = Mathf.RoundToInt(WorldPosition.y) % Chunk.ChunkSize;
            }
            if (WorldPosition.z < 0)
            {
                InChunkPosition.z = Chunk.ChunkSize - 1 - (Mathf.CeilToInt(Mathf.Abs(WorldPosition.z + 1)) % Chunk.ChunkSize);
            }
            else
            {
                InChunkPosition.z = Mathf.RoundToInt(WorldPosition.z) % Chunk.ChunkSize;
            }
            return InChunkPosition;
        }
        #endregion

        #region RealWorld

        public Vector3 GetUnit()
        {
            Vector3 VoxelUnitWorld = new Vector3(1, 1, 1);
            VoxelUnitWorld.x *= VoxelScale.x;
            VoxelUnitWorld.y *= VoxelScale.y;
            VoxelUnitWorld.z *= VoxelScale.z;
            VoxelUnitWorld.x *= transform.lossyScale.x;
            VoxelUnitWorld.y *= transform.lossyScale.y;
            VoxelUnitWorld.z *= transform.lossyScale.z;
            return VoxelUnitWorld;
        }

        /// <summary>
        /// This umm is wrong, Int3 world position needs to be purely block coordinates.
        /// </summary>
        public Int3 GetChunkPosition(Int3 WorldPosition)
        {
            return GetChunkPosition(WorldPosition.GetVector());
        }

        /// <summary>
        /// Converts a world position to a chunk position
        /// </summary>
        public Int3 GetChunkPosition(Vector3 WorldPosition)
        {
            Vector3 BlockPosition = WorldPosition;
            // gets the local position in the world
            BlockPosition = transform.InverseTransformPoint(BlockPosition);
            //Debug.LogError("Local position in world: " + WorldPosition.ToString() + " - to - " + BlockPosition.ToString());
            // Divide by Voxel Scale
            // Divide By CHunkSize and round to get our chunk position
            float ChunkSize = (float)(Chunk.ChunkSize);
            int ChunkX = Mathf.FloorToInt(BlockPosition.x / (ChunkSize * VoxelScale.x));
            int ChunkY = Mathf.FloorToInt(BlockPosition.y / (ChunkSize * VoxelScale.y));
            int ChunkZ = Mathf.FloorToInt(BlockPosition.z / (ChunkSize * VoxelScale.z));
            Int3 NewChunkPosition = new Int3(ChunkX, ChunkY, ChunkZ);
            //Debug.LogError("Found Chunk Posiiton: " + NewChunkPosition.GetVector().ToString());
            return NewChunkPosition;
        }
        #endregion

        #region RealAndBlockConversion

        /// <summary>
        /// Block Position into real position
        /// </summary>
        public Vector3 BlockToRealPosition(Vector3 BlockPosition)
        {
            // Scale
            BlockPosition.x *= VoxelScale.x;
            BlockPosition.y *= VoxelScale.y;
            BlockPosition.z *= VoxelScale.z;
            // offset
            BlockPosition -= CentreOffset;
            // transforms
            BlockPosition = transform.TransformPoint(BlockPosition);
            BlockPosition += transform.TransformDirection(GetUnit() / 2f);
            return BlockPosition;
        }

        /// <summary>
        /// A real world position to the block position in this world
        /// </summary>
        public Int3 RealToBlockPosition(Vector3 WorldPosition)
        {
            Vector3 BlockPosition = WorldPosition;
            // transforms
            BlockPosition = transform.InverseTransformPoint(BlockPosition);
            // offset
            BlockPosition += CentreOffset;
            // scales
            BlockPosition.x /= VoxelScale.x;
            BlockPosition.y /= VoxelScale.y;
            BlockPosition.z /= VoxelScale.z;
            BlockPosition = new Vector3(Mathf.FloorToInt(BlockPosition.x), Mathf.FloorToInt(BlockPosition.y), Mathf.FloorToInt(BlockPosition.z));
            return BlockPosition.ToInt3();
        }
        #endregion

        #region RayHit
        /// <summary>
        /// Gets the type of a voxel using a ray position and normal
        /// </summary>
        public int GetVoxelTypeRay(Vector3 WorldPosition, Vector3 Normal)
        {
            WorldPosition = RayHitToBlockPosition(WorldPosition, Normal);
            return GetVoxelType(new Int3(WorldPosition));
        }
        /// <summary>
        /// Returns the colour of a voxel using a ray position and normal
        /// </summary>
        public Color GetVoxelColorRay(Vector3 WorldPosition, Vector3 Normal)
        {
            WorldPosition = RayHitToBlockPosition(
                Normal,
                WorldPosition,
                true,
                (transform.lossyScale.sqrMagnitude / 3f) * VoxelScale.x);
            return GetVoxelColor(new Int3(WorldPosition));
        }

        public static Vector3 RayHitToBlockPosition(RaycastHit MyHit)
        {
            return RayHitToBlockPosition(MyHit, true);
        }

        public Vector3 RayHitToBlockPosition(Vector3 WorldPosition, Vector3 Normal)
        {
            return RayHitToBlockPosition(
                Normal,
                WorldPosition,
                true,
                (transform.lossyScale.sqrMagnitude / 3f) * VoxelScale.x);
        }
        public Vector3 RayHitToBlockPosition(Vector3 WorldPosition, Vector3 Normal, bool IsBuild)
        {
            return RayHitToBlockPosition(
                Normal,
                WorldPosition,
                IsBuild,
                (transform.lossyScale.sqrMagnitude / 3f) * VoxelScale.x);
        }

        public static Vector3 RayHitToBlockPosition(RaycastHit MyHit, bool Direction)
        {
            World MyWorld;
            if (MyHit.collider.GetComponent<Chunk>())
            {
                MyWorld = MyHit.collider.GetComponent<Chunk>().GetWorld();
            }
            else
            {
                MyWorld = MyHit.collider.GetComponent<World>();
            }
            if (MyWorld == null)
            {
                Debug.LogError("World is Null..");
                return Vector3.zero;
            }
            else
            {
                return MyWorld.RayHitToBlockPosition(MyHit.normal, MyHit.point, Direction,
                                                (MyHit.collider.transform.lossyScale.sqrMagnitude / 3f) * MyWorld.VoxelScale.x);
            }
        }
        public Vector3 RayHitToBlockPosition(Vector3 Normal, Vector3 MyHitPosition, bool Direction, float BlockScale)
        {
            if (Direction)
            {
                MyHitPosition -= Normal * BlockScale / 2f;
            }
            else
            {
                MyHitPosition += Normal * BlockScale / 2f;
            }
            Vector3 BlockPosition = RealToBlockPosition(MyHitPosition).GetVector();
            return BlockPosition;
        }
        #endregion
    }
}

//Debug.LogError("2 BlockPosition: " + BlockPosition.ToString());
/*int ChunkX = Mathf.FloorToInt(BlockPosition.x / Chunk.ChunkSize);
int ChunkY = Mathf.FloorToInt(BlockPosition.y / Chunk.ChunkSize);
int ChunkZ = Mathf.FloorToInt(BlockPosition.z / Chunk.ChunkSize);
int PosX = Mathf.FloorToInt(BlockPosition.x) % Chunk.ChunkSize;
int PosY = Mathf.FloorToInt(BlockPosition.y) % Chunk.ChunkSize;
int PosZ = Mathf.FloorToInt(BlockPosition.z) % Chunk.ChunkSize;
if (BlockPosition.x < 0)
{
    ChunkX = Mathf.RoundToInt(BlockPosition.x / Chunk.ChunkSize);
    ChunkX--;
    //PosX = Mathf.CeilToInt(BlockPosition.x) % Chunk.ChunkSize;
}
if (BlockPosition.y < 0)
{
    ChunkY--;
    //ChunkY = Mathf.CeilToInt(BlockPosition.y / Chunk.ChunkSize);
    //PosY = Mathf.CeilToInt(BlockPosition.y) % Chunk.ChunkSize;
}
if (BlockPosition.z < 0)
{
    //ChunkZ--;
    ChunkZ = Mathf.RoundToInt(BlockPosition.x / Chunk.ChunkSize);
    ChunkZ--;
    //ChunkZ = Mathf.CeilToInt(BlockPosition.z / Chunk.ChunkSize);
    //PosZ = Mathf.CeilToInt(BlockPosition.z) % Chunk.ChunkSize;
}
//Debug.LogError("3 Inside Chunk Position: " + (new Vector3(PosX, PosY, PosZ)).ToString());
//Chunk MyChunk = MyWorld.GetChunk (ChunkX, ChunkY, ChunkZ);
BlockPosition = new Vector3(ChunkX * Chunk.ChunkSize + PosX,
                            ChunkY * Chunk.ChunkSize + PosY,
                            ChunkZ * Chunk.ChunkSize + PosZ);*/
//Debug.LogError ("Clicking thingie: " + BlockPosition.ToString ());