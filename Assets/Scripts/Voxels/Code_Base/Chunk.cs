using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Zeltex.Characters;
using Zeltex.Util;

namespace Zeltex.Voxels
{
    [System.Serializable]
    public class ChunkEditorActionBlock
    {
        public EditorAction RefreshMesh = new EditorAction();


        public void Update(Chunk MyChunk)
        {

        }
    }
    /// <summary>
    /// Chunks Hold 16x16x16 Voxels
    /// They also handle the updates of the mesh
    /// </summary>
    [ExecuteInEditMode]
    public partial class Chunk : MonoBehaviour
    {
        #region Variables
        //public Thread MyThread;
        public static int ChunkSize = 16;   // size for our chunks
        public ChunkEditorActionBlock Actions = new ChunkEditorActionBlock();
        // references
        [SerializeField, HideInInspector]
        private World MyWorld;
        // Data
        [SerializeField]//, HideInInspector]
        public VoxelLookupTable MyLookupTable = new VoxelLookupTable(); // lookup table used for voxel indexes
        [SerializeField, HideInInspector]
        private VoxelData MyVoxels = new VoxelData(); // main voxel data - split up in chunks!
		public Int3 Position = Int3.Zero();  // main position data for each chunk!
        [SerializeField, HideInInspector]
        private List<Chunk> SurroundingChunks = new List<Chunk>();
        // States
        private bool HasSaved = true;
        public List<Character> MyCharacterSpawns = new List<Character>();

        [Header("Debug")]
        [SerializeField]
        private bool HasStartedUpdating;
        [SerializeField]
        private bool WasMassUpdated;
        [SerializeField]
        private bool IsUpdatingRender = false;
        [SerializeField]
        private bool IsBuildingMesh = false;
        //private bool HasUpdated = false;
        // Spawn Data
        private List<Int3> VoxelDropPositions = new List<Int3>();
        private List<int> VoxelDropTypes = new List<int>();
        private List<Color> VoxelDropColors = new List<Color>();
        private MeshFilter MyMeshFilter;
        private MeshRenderer MyMeshRenderer;
        private MeshCollider MyMeshCollider;
        private Int3 VoxelsRawPosition = Int3.Zero();
        [SerializeField]
        private bool IsDirty = true;    // start as dirty until saved by level manager

        public bool IsDirtyTrigger()
        {
            if (IsDirty)
            {
                IsDirty = false;
                return true;
            }
            return false;
        }
        #endregion

        #region Utility
#if UNITY_EDITOR
        private void Update()
        {
            if (Actions.RefreshMesh.IsTriggered())
            {
               // WasMassUpdated = true;
                RefreshAll();
                //OnMassUpdate();
            }
        }
#endif
        /// <summary>
        /// Refresh all the component links
        /// </summary>
        public void RefreshComponents()
        {
            MyMeshRenderer = null;
            MyMeshFilter = null;
            MyMeshCollider = null;
            GetMeshRenderer();
            GetMeshFilter();
            GetMeshCollider();
        }

        /// <summary>
        /// Repurposes a chunk for later use
        /// </summary>
        public void OnChunkReset()
        {
            MyMeshFilter.sharedMesh.Clear();
        }

        public MeshRenderer GetMeshRenderer()
        {
            if (MyMeshRenderer == null)
            {
                MyMeshRenderer = gameObject.GetComponent<MeshRenderer>();
            }
            if (MyMeshRenderer == null)
            {
                MyMeshRenderer = gameObject.AddComponent<MeshRenderer>();
            }
            MyMeshRenderer.enabled = IsMeshVisible;
            return MyMeshRenderer;
        }

        public MeshFilter GetMeshFilter()
        {
            if (MyMeshFilter == null)
            {
                MyMeshFilter = gameObject.GetComponent<MeshFilter>();
            }
            if (MyMeshRenderer == null)
            {
                MyMeshFilter = gameObject.AddComponent<MeshFilter>();
            }
            return MyMeshFilter;
        }

        public MeshCollider GetMeshCollider()
        {
            if (MyWorld.HasCollision())
            {
                if (MyMeshCollider == null)
                {
                    MyMeshCollider = gameObject.GetComponent<MeshCollider>();
                }
                if (MyMeshCollider == null)
                {
                    MyMeshCollider = gameObject.AddComponent<MeshCollider>();
                }
            }
            return MyMeshCollider;
        }


        /// <summary>
        /// Gets chunk ready to reuse for loading
        /// </summary>
        public void Reset()
        {
            SetAllVoxelsRaw(0);
            MyLookupTable.InitializeChunkTable();
        }

        public int GetHighestPoint(int i, int k)
        {
            for (int j = Chunk.ChunkSize - 1; j >= 0; j--)
            {
                if (GetVoxelType(new Int3(i, j, k)) != 0)
                {
                    return j;
                }
            }
            return -1;
        }

        public void SetAllVoxelsRaw(int NewIndex)
        {
            for (VoxelsRawPosition.x = 0; VoxelsRawPosition.x < Chunk.ChunkSize; VoxelsRawPosition.x++)
            {
                for (VoxelsRawPosition.y = 0; VoxelsRawPosition.y < Chunk.ChunkSize; VoxelsRawPosition.y++)
                {
                    for (VoxelsRawPosition.z = 0; VoxelsRawPosition.z < Chunk.ChunkSize; VoxelsRawPosition.z++)
                    {
                        MyVoxels.SetVoxelTypeRaw(VoxelsRawPosition, NewIndex);
                    }
                }
            }
        }
        public void AddDropVoxel(Int3 VoxelPosition, int VoxelType, Color VoxelColor)
        {
            VoxelDropPositions.Add(VoxelPosition);
            VoxelDropTypes.Add(VoxelType);
            VoxelDropColors.Add(VoxelColor);
        }
        public void DropVoxels()
        { 
            for (int i = VoxelDropPositions.Count - 1; i >= 0; i--)
            {
                if (VoxelDropPositions[i] != null && VoxelDropColors[i] != null)
                {
                    MyWorld.CreateBlockAtLocation(VoxelDropPositions[i].GetVector(), VoxelDropTypes[i], VoxelDropColors[i]);
                }
            }
            VoxelDropPositions.Clear();
            VoxelDropTypes.Clear();
            VoxelDropColors.Clear();
        }
        /// <summary>
        /// gets the world
        /// </summary>
        public World GetWorld() 
		{
            if (MyWorld == null)
            {
                MyWorld = GetComponent<World>();
                if (MyWorld == null && transform.parent != null)
                {
                    MyWorld = transform.parent.GetComponent<World>();
                }
            }
			return MyWorld;
		}

        /// <summary>
        /// Sets the world
        /// </summary>
		public void SetWorld(World NewWorld)
		{
			MyWorld = NewWorld;
		}

        /// <summary>
        /// Checks to see if a position index is inside the chunk size range.
        /// </summary>
		public static bool IsInRange(int a) 
		{
			return (a >= 0 && a < ChunkSize);
        }
        /// <summary>
        /// Checks to see if a position index is inside the chunk size range.
        /// </summary>
		public static bool IsInRange(Int3 MyInt3)
        {
            return (MyInt3.x >= 0 && MyInt3.x < ChunkSize
                && MyInt3.y >= 0 && MyInt3.y < ChunkSize
                && MyInt3.z >= 0 && MyInt3.z < ChunkSize);
        }

        /// <summary>
        /// Spawns any character data in the chunks
        /// Actually atm it just turns them on
        /// </summary>
        public void SpawnCharactersOnChunk()
        {
            // activate any characters that were saved into the mesh
            for (int i = MyCharacterSpawns.Count - 1; i >= 0; i--)
            {
                if (MyCharacterSpawns[i])
                {
                    MyCharacterSpawns[i].SetMovement(true);
                }
            }
            MyCharacterSpawns.Clear();
        }

        #endregion

        // Sets voxel variables
        #region SettersAndGetters
        public bool GetHasSaved()
        {
            return HasSaved;
        }
        public void OnSaved()
        {
            HasSaved = true;
        }
        /// <summary>
        /// Uses the mass update system to replace any voxel of a certain type by another
        /// </summary>
        public void ReplaceType(int OldType, int NewType)
        {
            for (int i = 0; i < ChunkSize; i++)
            {
                for (int j = 0; j < ChunkSize; j++)
                {
                    for (int k = 0; k < ChunkSize; k++)
                    {
                        if (GetVoxel(new Int3(i, j, k)).GetVoxelType() == OldType)
                        {
                            UpdateBlockTypeMass(new Int3(i, j, k), NewType);
                        }
                    }
                }
            }
            OnMassUpdate();
        }
        /// <summary>
        /// Uses the mass update system to replace any voxel of a certain type by another
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < ChunkSize; i++)
            {
                for (int j = 0; j < ChunkSize; j++)
                {
                    for (int k = 0; k < ChunkSize; k++)
                    {
                        if (GetVoxel(new Int3(i, j, k)).GetVoxelType() != 0)
                        {
                            UpdateBlockTypeMass(new Int3(i, j, k), 0);
                        }
                    }
                }
            }
            OnMassUpdate();
        }
        /// <summary>
        /// Returns the voxel type at a postiion. If no voxel it will return a type of 0.
        /// </summary>
        public int GetVoxelType(Int3 BlockPosition)
        {
            Voxel MyVoxel = GetVoxel(BlockPosition);
            if (MyVoxel != null)
            {
                return MyVoxel.GetVoxelType();
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns the voxel Colour. Requires the inside chunk position. (0 to 15)
        /// </summary>
        public Color GetVoxelColor(Int3 BlockPosition)
        {
            return MyVoxels.GetVoxelColorColor(BlockPosition);
        }

        /// <summary>
        /// Get a voxel at local chunk position
        /// </summary>
        public Voxel GetVoxel(Int3 InChunkPosition)
        {
            //Int3 InChunkPosition = new Int3(BlockPosition);
            //Int3 InChunkPosition = MyWorld.WorldToBlockInChunkPosition(BlockPosition);
            if (IsInRange(InChunkPosition.x) && IsInRange(InChunkPosition.y) && IsInRange(InChunkPosition.z))
            {
                return MyVoxels.GetVoxel(InChunkPosition.x, InChunkPosition.y, InChunkPosition.z);
            }
            else
            {
                //Debug.LogError("Is MyWorld: " + (MyWorld == null));
                // Assuming that these position values are relative to chunk Position, like the one on the edge of this chunk
                WorldPosition = InChunkPosition + Position * ChunkSize;
                return MyWorld.GetVoxel(WorldPosition);
            }
        }

        private Int3 WorldPosition;
        private int UpdateBlockTypeMassPreviousIndex;
        private Int3 UpdateBlockTypeMassChunkPosition;

        /// <summary>
        /// Uses raw type
        /// </summary>
        public bool UpdateBlockTypeMass(Int3 WorldPosition, int Type)
        {
            return UpdateBlockTypeMass(WorldPosition, Type, Color.white);
        }

        /// <summary>
        /// Update the voxels using an index relating to the LookupTable
        /// Only update voxels from inside worlds, using world positions
        /// Input is world position and type
        /// need to add colours
        /// </summary>
        public bool UpdateBlockTypeMass(Int3 WorldPosition, int Type, Color NewColor)
        {
            if (MyWorld == null)
            {
                MyWorld = transform.parent.GetComponent<World>();
            }
            UpdateBlockTypeMassChunkPosition = MyWorld.WorldToBlockInChunkPosition(WorldPosition);
            bool WasUpdated = false;
            if (IsInRange(UpdateBlockTypeMassChunkPosition))
            {
                UpdateBlockTypeMassPreviousIndex = MyVoxels.GetVoxelType(UpdateBlockTypeMassChunkPosition);
                WasUpdated = MyVoxels.SetVoxelType(this, UpdateBlockTypeMassChunkPosition, Type, NewColor);
                if (WasUpdated)
                {
                    WasMassUpdated = true;
                    IsDirty = true;
                    //Debug.Log("[" + name + "] Updated voxel in world position: " + WorldPosition.ToString() 
                    //    + "\n" + InChunkPositionX + ":" + InChunkPositionY + ":" + InChunkPositionZ
                    //    + "\n" + PreviousIndex + " to " + Type + " of color " + NewColor.ToString());
                    // get names
                    string PreviousVoxelName = GetWorld().MyLookupTable.GetName(UpdateBlockTypeMassPreviousIndex);
                    string NewVoxelName = GetWorld().MyLookupTable.GetName(Type);
                    GetWorld().MyLookupTable.OnReplace(PreviousVoxelName, NewVoxelName);
                    MyLookupTable.OnReplace(PreviousVoxelName, NewVoxelName);
                    // surrounding chunks!
                    OnUpdatedAtPosition(UpdateBlockTypeMassChunkPosition);
                }
            }
            else
            {
                Debug.LogError("Could not load: " + WorldPosition.ToString());
            }
            return WasUpdated;
        }

        /// <summary>
        /// On updated at a position it will update all the surrounding chunks
        /// </summary>
        private void OnUpdatedAtPosition(Int3 InChunkPosition)
        {
            //InChunkPositionX = Mathf.RoundToInt(WorldPosition.x) % Chunk.ChunkSize;
            Voxel VoxelLeft = GetVoxel(new Int3(InChunkPosition.x - 1, InChunkPosition.y, InChunkPosition.z));
            Voxel VoxelRight = GetVoxel(new Int3(InChunkPosition.x + 1, InChunkPosition.y, InChunkPosition.z));
            Voxel VoxelBelow = GetVoxel(new Int3(InChunkPosition.x, InChunkPosition.y - 1, InChunkPosition.z));
            Voxel VoxelAbove = GetVoxel(new Int3(InChunkPosition.x, InChunkPosition.y + 1, InChunkPosition.z));
            Voxel VoxelBehind = GetVoxel(new Int3(InChunkPosition.x, InChunkPosition.y, InChunkPosition.z - 1));
            Voxel VoxelFront = GetVoxel(new Int3(InChunkPosition.x, InChunkPosition.y, InChunkPosition.z + 1));
            if (VoxelLeft != null)
            {
                VoxelLeft.OnUpdated();
            }
            if (VoxelRight != null)
            {
                VoxelRight.OnUpdated();
            }
            if (VoxelBelow != null)
            {
                VoxelBelow.OnUpdated();
            }
            if (VoxelAbove != null)
            {
                VoxelAbove.OnUpdated();
            }
            if (VoxelBehind != null)
            {
                VoxelBehind.OnUpdated();
            }
            if (VoxelFront != null)
            {
                VoxelFront.OnUpdated();
            }

            if (InChunkPosition.x == 0)
            {
                SetChunkMassUpdated(MyWorld.GetChunk(new Int3(Position.x - 1, Position.y, Position.z)));
            }
            else if (InChunkPosition.x == ChunkSize - 1)
            {
                SetChunkMassUpdated(MyWorld.GetChunk(new Int3(Position.x + 1, Position.y, Position.z)));
            }

            //InChunkPositionY = Mathf.RoundToInt(WorldPosition.y) % Chunk.ChunkSize;
            if (InChunkPosition.y == 0)
            {
                SetChunkMassUpdated(MyWorld.GetChunk(new Int3(Position.x, Position.y - 1, Position.z)));
            }
            else if (InChunkPosition.y == ChunkSize - 1)
            {
                SetChunkMassUpdated(MyWorld.GetChunk(new Int3(Position.x, Position.y + 1, Position.z)));
            }

            //InChunkPositionZ = Mathf.RoundToInt(WorldPosition.z) % Chunk.ChunkSize;
            if (InChunkPosition.z == 0)
            {
                SetChunkMassUpdated(MyWorld.GetChunk(new Int3(Position.x, Position.y, Position.z - 1)));
            }
            else if (InChunkPosition.z == ChunkSize - 1)
            {
                SetChunkMassUpdated(MyWorld.GetChunk(new Int3(Position.x, Position.y, Position.z + 1)));
            }
        }

        // Mass update surrounding chunk too!
        void SetChunkMassUpdated(Chunk NewChunk)
        {
            if (NewChunk != null)
            {
                NewChunk.WasMassUpdated = true;
                if (MyWorld.IsDebug)
                {
                    //Debug.Log("Chunk")
                    Debug.Log("[" + name + "] Is Updating: " + NewChunk.name);
                }
            }
        }
        #endregion

        // coordinates Voxel updates with WorldUpdater Class
        #region MassUpdating

        /// <summary>
        /// Was the chunk updated
        /// </summary>
        public bool WasUpdated()
        {
            return WasMassUpdated;
        }

        /// <summary>
        /// Not sure why but had to use this
        /// </summary>
        public void TriggerMassUpdate()
        {
            WasMassUpdated = true;
        }

        /// <summary>
        /// Adds all updated chunks to the updater class
        /// Called when updated many voxels at once
        /// </summary>
        public void OnMassUpdate()
        {
           // Debug.LogError(name + " - MassUpdated: " + WasMassUpdated);
            if (WasMassUpdated)
            {
                //Debug.LogError(name + " was mass updated: " + Time.realtimeSinceStartup);
                WasMassUpdated = false;
                HasStartedUpdating = true;
                if (HasSaved == true)
                {
                    HasSaved = false;
                    // Add to save event
                }
                IsUpdatingRender = true;
                //HasUpdated = false;
                WorldUpdater.Get().Add(this);
                //if (World.TestingRefreshSides)
                {
                    RefreshSurroundingChunks();
                    if (SurroundingChunks.Count == 0)
                    {
                        //Debug.LogError("Chunk " + name + " Has no surrounding chunks!");
                    }
                    for (int i = 0; i < SurroundingChunks.Count; i++)
                    {
                        if (SurroundingChunks[i].WasMassUpdated == true)
                        {
                            SurroundingChunks[i].WasMassUpdated = false;
                            //Debug.Log("[" + name + "] Surrounding Chunk updated " + SurroundingChunks[i].name);
                            WorldUpdater.Get().Add(SurroundingChunks[i]);
                        }
                        else
                        {
                            //Debug.Log("[" + name + "] Surrounding Chunk did not update " + SurroundingChunks[i].name);
                        }
                    }
                }
            }
            else
            {
                //Debug.LogError(name + " was not updated: " + Time.realtimeSinceStartup);
            }
        }

        int ChunkIndex;
        /// <summary>
        /// Force the chunks surrounding to update as well
        /// </summary>
        public void ForceRefreshSurroundingChunks()
        {
            if (World.TestingRefreshSides)
            {
                SetAllUpdatedSides();
            }
            //RefreshSurroundingChunks();
            for (ChunkIndex = 0; ChunkIndex < SurroundingChunks.Count; ChunkIndex++)
            {
                if (World.TestingRefreshSides)
                {
                    SurroundingChunks[ChunkIndex].WasMassUpdated = true;
                    SurroundingChunks[ChunkIndex].SetAllUpdatedSides();
                }
            }
        }

        private Chunk OtherChunk;
        /// <summary>
        /// I should keep these as reference
        /// </summary>
        private void RefreshSurroundingChunks()
        {
            SurroundingChunks.Clear();
            // front
            OtherChunk = GetWorld().GetChunk(new Int3(Position.x, Position.y, Position.z + 1));
            if (OtherChunk && SurroundingChunks.Contains(OtherChunk) == false)
            {
                SurroundingChunks.Add(OtherChunk);
            }
            // back
            OtherChunk = GetWorld().GetChunk(new Int3(Position.x, Position.y, Position.z - 1));
            if (OtherChunk && SurroundingChunks.Contains(OtherChunk) == false)
            {
                SurroundingChunks.Add(OtherChunk);
            }

            // right
            OtherChunk = GetWorld().GetChunk(new Int3(Position.x + 1, Position.y, Position.z));
            if (OtherChunk && SurroundingChunks.Contains(OtherChunk) == false)
            {
                SurroundingChunks.Add(OtherChunk);
            }
            // left
            OtherChunk = GetWorld().GetChunk(new Int3(Position.x - 1, Position.y, Position.z));
            if (OtherChunk && SurroundingChunks.Contains(OtherChunk) == false)
            {
                SurroundingChunks.Add(OtherChunk);
            }

            // above
            OtherChunk = GetWorld().GetChunk(new Int3(Position.x, Position.y + 1, Position.z));
            if (OtherChunk && SurroundingChunks.Contains(OtherChunk) == false)
            {
                SurroundingChunks.Add(OtherChunk);
            }
            // below
            OtherChunk = GetWorld().GetChunk(new Int3(Position.x, Position.y - 1, Position.z));
            if (OtherChunk && SurroundingChunks.Contains(OtherChunk) == false)
            {
                SurroundingChunks.Add(OtherChunk);
            }
        }

        /// <summary>
        /// Set all voxels to updated
        /// </summary>
        private void SetAllUpdated()
        {
            SetAllUpdated(false);
        }

        /// <summary>
        /// Sets all the update states of voxels to false
        /// </summary>
        public void SetAllUpdated(bool HasUpdated)
        {
            MyVoxels.SetHasUpdated(HasUpdated);
            WasMassUpdated = HasUpdated;
        }

        /// <summary>
        /// Sets all the side voxels to updated, forcing them to rebuild, counts as a mass update
        /// </summary>
        public void SetAllUpdatedSides()
        {
            MyVoxels.OnSidesUpdates();
            WasMassUpdated = true;  // make sure it updates
        }

        /// <summary>
        /// Used to start updating the chunk, when world is resizing!
        /// </summary>
        public void RefreshAll()
        {
            Int3 RefreshAllPosition = Int3.Zero();
            Voxel MyVoxel;
            for (RefreshAllPosition.x = 0; RefreshAllPosition.x < ChunkSize; RefreshAllPosition.x++)
            {
                for (RefreshAllPosition.y = 0; RefreshAllPosition.y < ChunkSize; RefreshAllPosition.y++)
                {   
                    for (RefreshAllPosition.z = 0; RefreshAllPosition.z < ChunkSize; RefreshAllPosition.z++)
                    {
                        MyVoxel = GetVoxel(RefreshAllPosition);
                        MyVoxel.OnUpdated();
                        MyVoxel.MyMeshData.Clear();
                    }
                }
            }
            WasMassUpdated = true;  // make sure it updates
            IsUpdatingRender = true;
            //HasUpdated = false;
            GetWorld().MyUpdater.Add(this);
        }
        #endregion

    }
}
/// <summary>
/// This function is called when a voxel changes its type, lighting or colour. It is used purely for mesh updating. 
/// Tells the chunks that meshes or lights need updating.
/// if the voxel location is on the edge of the chunk, it will call the chunk next to it to be updated.
/// </summary>
/*public void HasChangedAt(Int3 BlockPosition)//, bool IsLightsOnly)
{
    if (BlockPosition.x == 0)
    {
        Chunk ChunkLeft = MyWorld.GetChunk(Position.Left());
        if (ChunkLeft != null)
        {
            ChunkLeft.GetVoxel(BlockPosition.RightSide()).SetHasUpdated(true);
            ChunkLeft.Updated();
        }
    }
    else if (BlockPosition.x == ChunkSize - 1)
    {
        Chunk ChunkRight = MyWorld.GetChunk(Position.Right());
        if (ChunkRight != null)
        {
            ChunkRight.GetVoxel(BlockPosition.LeftSide()).SetHasUpdated(true);
            ChunkRight.Updated();
        }
    }
    if (BlockPosition.z == 0)
    {
        Chunk OtherChunk = MyWorld.GetChunk(Position.Behind());
        if (OtherChunk != null)
        {
            OtherChunk.GetVoxel(BlockPosition.FrontSide()).SetHasUpdated(true);
            OtherChunk.Updated();
        }
    }
    else if (BlockPosition.z == ChunkSize - 1)
    {
        Chunk OtherChunk = MyWorld.GetChunk(Position.Front());
        if (OtherChunk != null)
        {
            OtherChunk.GetVoxel(BlockPosition.BehindSide()).SetHasUpdated(true);
            OtherChunk.Updated();
        }
    }
    if (BlockPosition.y == 0)
    {
        Chunk OtherChunk = MyWorld.GetChunk(Position.Below());
        if (OtherChunk != null)
        {
            OtherChunk.GetVoxel(BlockPosition.BelowSide()).SetHasUpdated(true);
            OtherChunk.Updated();
        }
    }
    else if (BlockPosition.y == ChunkSize - 1)
    {
        Chunk OtherChunk = MyWorld.GetChunk(Position.Above());
        if (OtherChunk != null)
        {
            OtherChunk.GetVoxel(BlockPosition.AboveSide()).SetHasUpdated(true);
            OtherChunk.Updated();
        }
    }
}*/
