using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Zeltex.Voxels
{
    /// <summary>
    /// Voxel Update part of the world
    /// </summary>
    public partial class World : MonoBehaviour
    {
        #region UpdateVariables
        [Header("DroppingVoxels")]
        private List<Int3> VoxelPositionsMass = new List<Int3>();
        private List<int> VoxelTypesMass = new List<int>();
        private List<Color> VoxelColorsMass = new List<Color>();
        [Header("Painter")]
        public bool IsUseUpdater = true;
        public bool IsPaintOver;            // is paint over current voxels, instead of building new ones

        // for mass updating
        private Chunk MassUpdateChunk;
        private bool DidUpdate;
        private int VoxelIndex;
        private Color PreviousColor;
        private int PreviousType;
        private Voxel MyVoxel;
        #endregion

        #region MassUpdates

        public void UpdateBlockTypeMassArea(string VoxelName, Int3 Position, float VoxelSize, Color VoxelColor)
        {
            UpdateBlockTypeMassArea(VoxelName, Position, new Vector3(VoxelSize, VoxelSize, VoxelSize), VoxelColor);
        }

        /// <summary>
        /// Creates brush shapes! this one is simple cube shape!
        /// </summary>
        public void UpdateBlockTypeMassArea(string VoxelName, Int3 Position, Vector3 VoxelSize, Color VoxelColor)
        {
            List<Int3> Positions = new List<Int3>();
            Debug.LogError (gameObject.name + " Updating block at: " + Position.GetVector().ToString() + 
                ": with size: " + VoxelSize + " with colour: " + VoxelColor.ToString() + ":" + VoxelName);
            if (VoxelSize.x == 0 && VoxelSize.y == 0 && VoxelSize.z == 0)
            {
                Positions.Add(Position);
            }
            else
            {   // use float size around voxel grid
                for (float i = -VoxelSize.x; i <= VoxelSize.x; i += 1)
                {
                    for (float j = -VoxelSize.y; j <= VoxelSize.y; j += 1)
                    {
                        for (float k = -VoxelSize.z; k <= VoxelSize.z; k += 1)
                        {
                            Positions.Add(Position + new Int3(i, j, k));
                        }
                    }
                }
            }
            //Debug.LogError("Updating Blocks, with posiitons count: " + Positions.Count);
            for (int i = 0; i < Positions.Count; i++)
            {
                if (IsPaintOver)
                {
                    if (GetVoxelType(Positions[i]) != 0)
                    {
                        UpdateBlockTypeMass(VoxelName, Positions[i], VoxelColor);
                    }
                }
                else
                {
                    UpdateBlockTypeMass(VoxelName, Positions[i], VoxelColor);
                }
            }
            OnMassUpdate();
        }

        /// <summary>
        /// Update a voxel at a position
        /// </summary>
        public void UpdateBlockTypeMass(string VoxelName, Int3 WorldPosition)
        {
            UpdateBlockTypeMass(VoxelName, WorldPosition, Color.white);
        }

        /// <summary>
        /// Breaks up voxel updates from a mass into a single update
        /// </summary>
        public void UpdateBlockTypeMass(string VoxelName, Int3 WorldPosition, Color NewColor, bool IsTerrainGeneration = false)
        {
            MassUpdateChunk = GetChunkWorldPosition(WorldPosition);;
            if (MassUpdateChunk != null)
            {
                Debug.LogError("Updating: MassUpdateChunk" + MassUpdateChunk.name);
                MyVoxel = GetVoxel(WorldPosition);
                if (MyVoxel != null)
                {
                    PreviousType = MyVoxel.GetVoxelType();// GetVoxelType(WorldPosition);
                    PreviousColor = MyVoxel.GetColor();
                    VoxelIndex = MyLookupTable.GetIndex(VoxelName);
                    DidUpdate = MassUpdateChunk.UpdateBlockTypeMass(WorldPosition, VoxelIndex, NewColor);
                    Debug.LogError("DidUpdate" + DidUpdate);
                    if (!IsTerrainGeneration && (DidUpdate && PreviousType != 0 && VoxelIndex == 0)) // because air does not drop things! And cannot drop things if not air!
                    {
                        if (IsDropItems || IsDropParticles)
                        {
                            VoxelPositionsMass.Add(WorldPosition);
                            VoxelTypesMass.Add(PreviousType);
                            VoxelColorsMass.Add(PreviousColor);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Chunk is null at: " + WorldPosition.ToString());
            }
        }

        /// <summary>
        /// Called to mass update voxels in the world
        /// </summary>
        public void OnMassUpdate()
        {
            Debug.LogError("Creating " + VoxelPositionsMass.Count + " Voxels in world!");
            if (VoxelPositionsMass.Count > 0)
            {
                //Debug.Log("Creating " + VoxelPositionsMass.Count + " Voxels in world!");
                if (Application.isPlaying)
                {
                    for (int i = 0; i < VoxelPositionsMass.Count; i++)
                    {
                        Chunk MyChunk = GetChunkWorldPosition(VoxelPositionsMass[i]);
                        if (MyChunk != null)
                        {
                            MyChunk.AddDropVoxel(VoxelPositionsMass[i], VoxelTypesMass[i], VoxelColorsMass[i]);   // okay ill fix this later
                        }
                    }
                }
                VoxelPositionsMass.Clear();
                VoxelTypesMass.Clear();
                VoxelColorsMass.Clear();
            }
            foreach (Int3 MyKey in MyChunkData.Keys)
            {
                MassUpdateChunk = MyChunkData[MyKey];
                if (MassUpdateChunk)
                {
                    MassUpdateChunk.OnMassUpdate();
                }
            }
        }
        #endregion
        
        #region UpdateVoxels
        /// <summary>
        /// Update every Voxel to a single VoxelType
        /// </summary>
        /// <param name="VoxelType"></param>
        public void UpdateAll(string VoxelName)
        {
            for (int i = 0; i < WorldSize.x * Chunk.ChunkSize; i++)
            {
                for (int j = 0; j < WorldSize.y * Chunk.ChunkSize; j++)
                {
                    for (int k = 0; k < WorldSize.z * Chunk.ChunkSize; k++)
                    {
                        UpdateBlockTypeMass(VoxelName, new Int3(i, j, k));
                    }
                }
            }
            OnMassUpdate();
        }

        public bool UpdateBlockType(string VoxelName, Vector3 BlockPosition)
        {
            return UpdateBlockType(VoxelName, BlockPosition, 0, new Color(255, 255, 255));
        }
        public bool UpdateBlockType(string VoxelName, Vector3 BlockPosition, float VoxelSize)
        {
            return UpdateBlockType(VoxelName, BlockPosition, VoxelSize, new Color(255, 255, 255));
        }

        public bool UpdateBlockType(string VoxelName, Vector3 BlockPosition, float VoxelSize, Color VoxelColor)
        {
            UpdateBlockTypeMassArea(
                VoxelName,
                new Int3(BlockPosition),
                VoxelSize,
                VoxelColor);
            return true;
            //return UpdateBlockTypeSize (BlockPosition.x, BlockPosition.y, BlockPosition.z, NewType,SetSize);
        }

        public void ReplaceType(int OldType, int NewType)
        {
            foreach (Int3 MyKey in MyChunkData.Keys)
            {
                Chunk MyChunk = MyChunkData[MyKey];
                MyChunk.ReplaceType(OldType, NewType);
            }
        }
        #endregion

        #region VoxelUpdatesDepreciated
        /// <summary>
        /// Update a voxel at a position - Uses Index - Depreciated, please use string identifier
        /// </summary>
        public void UpdateBlockTypeMass(int VoxelIndex, Int3 WorldPosition)
        {
            string VoxelName = MyLookupTable.GetName(VoxelIndex);
            UpdateBlockTypeMass(VoxelName, WorldPosition, Color.white);
        }
        #endregion

        #region NetworkUpdates
        /// <summary>
        /// Updates the block
        /// </summary>
        /*private bool UpdateBlockTypeMassAreaNetwork(string VoxelName, float x, float y, float z, float VoxelSize, float MyTintR, float MyTintG, float MyTintB)
        {
            Color VoxelColor = new Color(MyTintR, MyTintG, MyTintB);
            //Debug.Log("Painting with color: " + MyTint.ToString() + " : " + MyTintR + ":" + MyTintG + ":" + MyTintB);
            UpdateBlockTypeMassArea(VoxelName, new Int3(x, y, z), VoxelSize, VoxelColor);
            return true;
        }*/
        #endregion

        #region UpdateUtility
        public void UpdateBlockType(RaycastHit MyHit, string VoxelName, float VoxelSize)
        {
            bool IsAir = (VoxelName == "Air");
            Vector3 BlockPosition = RayHitToBlockPosition(MyHit, IsAir);
            UpdateBlockType(VoxelName, BlockPosition, VoxelSize);
        }
        public void UpdateBlockType(RaycastHit MyHit, string VoxelName, float VoxelSize, Color VoxelColor)
        {
            bool IsAir = (VoxelName == "Air");
            Vector3 BlockPosition = RayHitToBlockPosition(MyHit, IsAir);
            UpdateBlockType(VoxelName, BlockPosition, VoxelSize, VoxelColor);
        }
        #endregion
    }

}