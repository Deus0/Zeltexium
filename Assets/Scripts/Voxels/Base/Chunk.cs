using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Zeltex.Characters;
using Zeltex.Util;

namespace Zeltex.Voxels
{
    /// <summary>
    /// Editor actions chunks can perform
    /// </summary>
    [System.Serializable]
    public class ChunkEditorActionBlock
    {
        public EditorAction RefreshMesh = new EditorAction();


        public void Update(Chunk MyChunk)
        {
            if (RefreshMesh.IsTriggered())
            {
                MyChunk.RefreshAll();
            }
        }
    }

    /// <summary>
    /// Chunks Hold 16x16x16 Voxels
    /// They also handle the updates of the mesh
    /// </summary>
    [ExecuteInEditMode]
    public class Chunk : MonoBehaviour
    {
        private static string AirName = "Air";
        public static int ChunkSize = 16;   // size for our chunks
         
        // World Stuff
        [SerializeField, HideInInspector]
        private World MyWorld;
        // World Position data for each chunk!
        public Int3 Position = Int3.Zero();
        [SerializeField, HideInInspector]
        private List<Chunk> SurroundingChunks = new List<Chunk>();
        // Voxel Stuff
        // The main table for voxels
        [SerializeField]
        public VoxelLookupTable MyLookupTable = new VoxelLookupTable(); // lookup table used for voxel indexes
        // main voxel data - split up in chunks!
        [SerializeField, HideInInspector]
        private VoxelData MyVoxels = new VoxelData(Chunk.ChunkSize, Chunk.ChunkSize, Chunk.ChunkSize);
        // Characters linked to this chunk
        public List<Character> MyCharacterSpawns = new List<Character>();

        // Building Meshes
        private bool HasChanged = false;    // Save Variable
        private bool WasMassUpdated;
        private bool IsUpdatingRender = false;
        private bool IsBuildingMesh = false;
        private bool HasStartedUpdating;
        private List<MeshData> ChunkMeshes = new List<MeshData>(); // a mesh data per material! This is cleared after the building is done

        // Spawn Voxels
        private List<Int3> VoxelDropPositions = new List<Int3>();
        private List<int> VoxelDropTypes = new List<int>();
        private List<Color> VoxelDropColors = new List<Color>();

        // Components
        private MeshFilter MyMeshFilter;
        private MeshRenderer MyMeshRenderer;
        private MeshCollider MyMeshCollider;

        // Mutation!
        private static bool IsMutateColor = false;
        private static Color MutateColorAddition = new Color(0.88f, 0.4f, 0.58f, 1f);
        private static float MutateColorVariance = 0.14f;

        #region CachedVariables

        #region MeshUpdates

        private bool HasInitiated;

        public ChunkEditorActionBlock Actions = new ChunkEditorActionBlock();
        #endregion

        #region TerrainUpdates
        [HideInInspector]
        public string MassUpdateVoxelName;
        [HideInInspector]
        public Int3 MassUpdatePosition = Int3.Zero();
        [HideInInspector]
        public Color MassUpdateColor;
        private Voxel MassUpdateVoxel;
        private int MassUpdateVoxelIndex;
        private int PreviousIndex;
        private string PreviousVoxelName;
        #endregion
        #endregion

        #region Saving
        public bool CanSave()
        {
            return HasChanged;
        }
        public void OnModified()
        {
            if (!HasChanged)
            {
                HasChanged = true;
                // Modified Event
                //MyWorld.OnModified();
                // which calls MyLevel/SaveGame onModified
            }
        }
        public void OnSaved()
        {
            if (HasChanged)
            {
                HasChanged = false;
                // Trigger event
            }
        }
        #endregion

        #region Mono
#if UNITY_EDITOR
        private void Update()
        {
            Actions.Update(this);
        }
#endif
        #endregion

        #region Utility

        private void RefreshComponentLinks()
        {
            if (MyWorld.IsSingleChunk())
            {
                MyMeshFilter = MyWorld.GetComponent<MeshFilter>();
                MyMeshRenderer = MyWorld.GetComponent<MeshRenderer>();
                MyMeshCollider = MyWorld.GetComponent<MeshCollider>();
            }
            else
            {
                MyMeshFilter = gameObject.GetComponent<MeshFilter>();
                MyMeshRenderer = gameObject.GetComponent<MeshRenderer>();
                MyMeshCollider = gameObject.GetComponent<MeshCollider>();
            }
            if (MyMeshFilter == null || MyMeshRenderer == null)// || MyMeshCollider == null)
            {
                Debug.LogError("Chunk Missing Component - Position: " + Position.GetVector().ToString());
            }
        }
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
            if (MyMeshFilter && MyMeshFilter.sharedMesh)
            {
                MyMeshFilter.sharedMesh.Clear();
            }
            else
            {
                //Debug.LogError("MyMeshFilter or MyMeshFilter.sharedMesh is null inside " + name);
            }
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
                /*if (MyWorld.IsAddOutline)
                {
                    gameObject.AddComponent<cakeslice.Outline>();
                }*/
            }
            MyMeshRenderer.enabled = MyWorld.IsMeshVisible;
            return MyMeshRenderer;
        }

        public MeshFilter GetMeshFilter()
        {
            if (MyMeshFilter == null)
            {
                MyMeshFilter = gameObject.GetComponent<MeshFilter>();
            }
            if (MyMeshFilter == null)
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
            Int3 VoxelsRawPosition = Int3.Zero();
            for (VoxelsRawPosition.x = 0; VoxelsRawPosition.x < ChunkSize; VoxelsRawPosition.x++)
            {
                for (VoxelsRawPosition.y = 0; VoxelsRawPosition.y < ChunkSize; VoxelsRawPosition.y++)
                {
                    for (VoxelsRawPosition.z = 0; VoxelsRawPosition.z < ChunkSize; VoxelsRawPosition.z++)
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
                if (VoxelDropPositions[i] != null)
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
                return MyVoxels.GetVoxel(InChunkPosition);
            }
            else
            {
                // Assuming that these position values are relative to chunk Position, like the one on the edge of this chunk
                return MyWorld.GetVoxel(InChunkPosition + Position * ChunkSize);
            }
        }

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
            Int3 UpdateBlockTypeMassChunkPosition = MyWorld.WorldToBlockInChunkPosition(WorldPosition);
            bool WasUpdated = false;
            if (IsInRange(UpdateBlockTypeMassChunkPosition))
            {
                int UpdateBlockTypeMassPreviousIndex = MyVoxels.GetVoxelType(UpdateBlockTypeMassChunkPosition);
                if (NewColor == Color.white)
                {
                    WasUpdated = MyVoxels.SetVoxelType(this, UpdateBlockTypeMassChunkPosition, Type);
                }
                else
                {
                    Type = 1;
                    WasUpdated = MyVoxels.SetVoxelColor(this, UpdateBlockTypeMassChunkPosition, NewColor);
                }
                if (WasUpdated)
                {
                    WasMassUpdated = true;
                    OnModified();
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

        #region Loading

        public void SetDefaultVoxelNames()
        {
            MyWorld.MyLookupTable.Clear();
            for (int i = 0; i < DataManager.Get().GetSize(DataFolderNames.Voxels); i++)
            {
                MyWorld.MyLookupTable.AddName(DataManager.Get().GetName(DataFolderNames.Voxels, i));
            }
        }
        /// <summary>
        /// Loading the data for a particular chunk, need to call onMassUpdate after all the updates
        /// Used by WorldManager
        /// </summary>
        public IEnumerator RunScript(string Data, bool IsUpdateMesh = true)
        {
            if (Data == null)
            {
                Debug.LogError("Error Running script in " + name);
                yield break;
            }
            //int UpdatedBlocks = 0;
            Color MutationColor = new Color(
                Random.Range(0, MutateColorAddition.r),
                Random.Range(0, MutateColorAddition.g),
                Random.Range(0, MutateColorAddition.b),
                1f);
            string[] MyInput;
            int MyBlockType;
            //bool DidUpdate;
            Color MyColor;
            int ScriptLineIndex = 0;
            if (MyWorld == null)
            {
                Debug.LogError(name + " didnt have a World");
                MyWorld = GetComponent<World>();
                RefreshComponents();
            }
            if (MyWorld.MyLookupTable == null)
            {
                Debug.LogError(name + " didnt have a LookupTable");
                MyWorld.MyLookupTable = new VoxelLookupTable();
            }
            bool DidUpdateChunk = false;
            bool DidUpdateVoxel = false;
            MyWorld.MyLookupTable.AddName("Color");
            string[] MyLines = Data.Split('\n');
            Int3 LoadingVoxelIndex = Int3.Zero();

            for (LoadingVoxelIndex.x = 0; LoadingVoxelIndex.x < Chunk.ChunkSize; LoadingVoxelIndex.x++)
            {
                for (LoadingVoxelIndex.y = 0; LoadingVoxelIndex.y < Chunk.ChunkSize; LoadingVoxelIndex.y++)
                {
                    for (LoadingVoxelIndex.z = 0; LoadingVoxelIndex.z < Chunk.ChunkSize; LoadingVoxelIndex.z++)
                    {
                        if (ScriptLineIndex < MyLines.Length)
                        {
                            MyInput = MyLines[ScriptLineIndex].Split(' ');
                            if (MyInput.Length == 1)
                            {
                                try
                                {
                                    try
                                    {
                                        MyBlockType = int.Parse(MyLines[ScriptLineIndex]);
                                    } 
                                    catch (System.FormatException) 
                                    {
                                        Debug.LogError("Chunk line is in wrong format at line: " + ScriptLineIndex + " - " + MyLines[ScriptLineIndex]);
                                        MyBlockType = 0;
                                        yield break;
                                    }
                                    //if (MyBlockType != 0)
                                    {
                                        MassUpdateVoxelIndex = MyBlockType;
                                        if (MyWorld.MyLookupTable != null)
                                        {
                                            MassUpdateVoxelName = MyWorld.MyLookupTable.GetName(MyBlockType);
                                        }
                                        MassUpdatePosition.Set(LoadingVoxelIndex);
                                        DidUpdateVoxel = UpdateBlockTypeLoading();
                                        if (DidUpdateChunk == false && DidUpdateVoxel)
                                        {
                                            DidUpdateChunk = true;
                                        }
                                    }
                                }
                                catch (System.FormatException e)
                                {
                                    Debug.LogError(e.ToString());
                                }
                                catch (System.NullReferenceException e)
                                {
                                    Debug.LogError(e.ToString());
                                }
                            }
                            else if (MyInput.Length == 4)
                            {
                                try
                                {
                                    MyBlockType = int.Parse(MyInput[0]);
                                    //if (MyBlockType != 0)
                                    {
                                        MyColor.r = int.Parse(MyInput[1]) / 255f;
                                        MyColor.g = int.Parse(MyInput[2]) / 255f;
                                        MyColor.b = int.Parse(MyInput[3]) / 255f;

                                        if (IsMutateColor)
                                        {
                                            MyColor.r = Mathf.Clamp((MyColor.r + Random.Range(-MutateColorVariance, MutateColorVariance) + MutationColor.r) / 2f, 0, 1);
                                            MyColor.g = Mathf.Clamp((MyColor.g + Random.Range(-MutateColorVariance, MutateColorVariance) + MutationColor.g) / 2f, 0, 1);
                                            MyColor.b = Mathf.Clamp((MyColor.b + Random.Range(-MutateColorVariance, MutateColorVariance) + MutationColor.b) / 2f, 0, 1);
                                        }
                                        MassUpdateColor.r = MyColor.r;
                                        MassUpdateColor.g = MyColor.g;
                                        MassUpdateColor.b = MyColor.b;
                                        if (MyWorld.MyLookupTable != null)
                                        {
                                            MassUpdateVoxelName = MyWorld.MyLookupTable.GetName(MyBlockType);
                                        }
                                        MassUpdateVoxelIndex = 1;
                                        MassUpdatePosition.Set(LoadingVoxelIndex);
                                        DidUpdateVoxel = UpdateBlockColorLoading();
                                        if (DidUpdateChunk == false && DidUpdateVoxel)
                                        {
                                            DidUpdateChunk = true;
                                        }
                                    }
                                }
                                catch (System.FormatException e)
                                {
                                    Debug.LogError(e.ToString());
                                }
                            }
                            ScriptLineIndex++;
                        }
                        //Debug.LogError("Running script for: " + name + "'s VoxelDatas:" + MyVoxels.GetVoxelRaw(DebugVoxelIndex).GetVoxelType().ToString());
                    }
                }
                if (DidUpdateChunk)
                {
                    yield return MyWorld.YieldTimer();
                }
            }

            WasMassUpdated = true;
            if (DidUpdateChunk && IsUpdateMesh)
            {
                yield return BuildChunkMesh();
            }
        }

        public void DebugChunkVoxels()
        {
            Debug.LogError(name + " is debugging voxel types.");
            Int3 DebugVoxelIndex = Int3.Zero();
            for (DebugVoxelIndex.x = 0; DebugVoxelIndex.x < ChunkSize; DebugVoxelIndex.x++)
            {
                for (DebugVoxelIndex.y = 0; DebugVoxelIndex.y < ChunkSize; DebugVoxelIndex.y++)
                {
                    for (DebugVoxelIndex.z = 0; DebugVoxelIndex.z < ChunkSize; DebugVoxelIndex.z++)
                    {
                        //if (MyVoxels.GetVoxelRaw(VoxelIndex).GetVoxelType() != 0)
                        {
                            Debug.LogError(name + "'s VoxelDatas:" + MyVoxels.GetVoxelRaw(DebugVoxelIndex).GetVoxelType().ToString());   // + VoxelIndex.GetVector().ToString() +
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a list of data for a Chunk
        /// </summary>
        public List<string> GetScript()
        {
            List<string> MyData = new List<string>();
            MyData.AddRange(GetInstancedList());
            for (int i = 0; i < ChunkSize; i++)
            {
                for (int j = 0; j < ChunkSize; j++)
                {
                    for (int k = 0; k < ChunkSize; k++)
                    {
                        Voxel MyVoxel = GetVoxel(new Int3(i, j, k));
                        VoxelColor MyVoxelColor = MyVoxel as VoxelColor;
                        if (MyVoxelColor == null)
                        {
                            MyData.Add("" + MyVoxel.GetVoxelType());
                        }
                        else
                        {
                            Color MyColor = MyVoxelColor.GetColor();
                            int Red = (int)(255 * MyColor.r);
                            int Green = (int)(255 * MyColor.g);
                            int Blue = (int)(255 * MyColor.b);
                            MyData.Add("" + MyVoxelColor.GetVoxelType() + " " + Red + " " + Green + " " + Blue);
                        }
                    }
                }
            }
            return MyData;
        }


        /// <summary>
        /// Returns a list of data for a Chunk
        /// </summary>
        public string GetSerial()
        {
            string Data = "";
            Int3 Position = Int3.Zero();
            Voxel MyVoxel;
            VoxelColor MyVoxelColor;
            Color MyColor = Color.white;
            for (Position.x = 0; Position.x < ChunkSize; Position.x++)
            {
                for (Position.y = 0; Position.y < ChunkSize; Position.y++)
                {
                    for (Position.z = 0; Position.z < ChunkSize; Position.z++)
                    {
                        MyVoxel = MyVoxels.GetVoxelRaw(Position);
                        MyVoxelColor = MyVoxel as VoxelColor;
                        if (MyVoxelColor == null)
                        {
                            Data += ("" + MyVoxel.GetVoxelType()) + "\n";
                        }
                        else
                        {
                            MyColor = MyVoxelColor.GetColor();
                            Data += ("" + MyVoxelColor.GetVoxelType() +
                                " " + (int)(255 * MyColor.r)
                                + " " + (int)(255 * MyColor.g) 
                                + " " + (int)(255 * MyColor.b)) + "\n";
                        }
                    }
                }
            }
            return Data;
        }
        #endregion

        #region TerrainUpdates
        /// <summary>
        /// Uses: MassUpdatePosition, MassUpdateVoxelName, MassUpdateColor, 
        /// Breaks up voxel updates from a mass into a single update
        /// </summary>
        public void UpdateBlockTypeMassTerrain()
        {
            MassUpdateVoxelIndex = MyWorld.MyLookupTable.GetIndex(MassUpdateVoxelName);
            UpdateBlockTypeLoading();
        }

        private bool UpdateBlockTypeLoading()
        {
            PreviousIndex = MyVoxels.GetVoxelType(MassUpdatePosition);
            bool DidUpdate = MyVoxels.SetVoxelType(
                this,
                MassUpdatePosition,
                MassUpdateVoxelIndex);
            if (DidUpdate)
            {
                // get names
                PreviousVoxelName = MyWorld.MyLookupTable.GetName(PreviousIndex);
                MyWorld.MyLookupTable.OnReplace(PreviousVoxelName, MassUpdateVoxelName);
                MyLookupTable.OnReplace(PreviousVoxelName, MassUpdateVoxelName);
            }
            return DidUpdate;
        }


        private bool UpdateBlockColorLoading()
        {
            PreviousIndex = MyVoxels.GetVoxelType(MassUpdatePosition);
            bool DidUpdate = MyVoxels.SetVoxelColor(
                this,
                MassUpdatePosition,
                MassUpdateColor);
            if (DidUpdate)
            {
                // get names
                PreviousVoxelName = MyWorld.MyLookupTable.GetName(PreviousIndex);
                MyWorld.MyLookupTable.OnReplace(PreviousVoxelName, MassUpdateVoxelName);
                MyLookupTable.OnReplace(PreviousVoxelName, MassUpdateVoxelName);
            }
            return DidUpdate;
        }
        //Debug.Log("[" + name + "] Updated voxel in world position: " + WorldPosition.ToString() 
        //    + "\n" + InChunkPositionX + ":" + InChunkPositionY + ":" + InChunkPositionZ
        //    + "\n" + PreviousIndex + " to " + Type + " of color " + NewColor.ToString());
        #endregion

        #region MeshBUildingVariables

        #region Utility

        private bool ChunkVisibility = false;

        public void SetMeshVisibility(bool NewVisibility)
        {
            if (ChunkVisibility != NewVisibility)
            {
                ChunkVisibility = NewVisibility;
                if (MyMeshRenderer)
                {
                    MyMeshRenderer.enabled = ChunkVisibility;
                }
            }
        }

        /// <summary>
        /// Gets the mid point of a chunk
        /// </summary>
        public Vector3 GetMidPoint()
        {
            return transform.TransformPoint(new Vector3(ChunkSize, ChunkSize, ChunkSize) / 2f);
        }

        /// <summary>
        /// Has the chunk loaded yet
        /// </summary>
        public bool HasLoaded()
        {
            if (!HasInitiated || IsUpdatingRender || WasMassUpdated)
            {
                if (WasMassUpdated)
                {
                    OnMassUpdate();
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        public VoxelData GetVoxelData()
        {
            return MyVoxels;
        }

        public void SetVoxelData(VoxelData NewVoxels)
        {
            MyVoxels = NewVoxels;
            OnMassUpdate();
        }


        /// <summary>
        /// Returns true if the chunk is still building mesh
        /// Used in chunk updater for queing the chunks
        /// </summary>
        public bool GetIsBuildingMesh()
        {
            return IsBuildingMesh;
        }

        /// <summary>
        /// Used in world, to check if the world is loading any chunks
        /// </summary>
        public bool IsUpdatingChunk()
        {
            return (HasStartedUpdating || WasMassUpdated || IsBuildingMesh || IsUpdatingRender);    // is building or updating mesh
        }
        #endregion

        public void PreWorldBuilderBuildMesh()
        {
            WasMassUpdated = true;
        }
        /// <summary>
        /// Runs on a thread - starts the build process of the mesh data
        /// BUilds a different mesh per material!
        /// </summary>
        public IEnumerator BuildChunkMesh()
        {
            //Debug.LogError("BuildChunkMesh: " + WasMassUpdated + " in " + name);
            if (WasMassUpdated)
            {
                WasMassUpdated = false;
                OnBuildingMesh();
                int i = 0;
                if (ChunkMeshes.Count != MyWorld.GetMaterials().Count)
                {
                    ChunkMeshes.Clear();
                    for (i = 0; i < MyWorld.GetMaterials().Count; i++)
                    {
                        ChunkMeshes.Add(new MeshData());    // create a place where we can store our voxel mesh
                    }
                }
                //Debug.LogError("Updated Mesh:[" + name + "] BuildChunkMesh Function " + MyWorld.MyMaterials.Count);
                for (i = 0; i < MyWorld.GetMaterials().Count; i++)     // for each material, build the mesh of each chunk
                {
                    yield return BuildChunkMeshPerMaterial(i);
                }
                IsUpdatingRender = true;
                IsBuildingMesh = false; // tell updater it has finished building chunk mesh!
                yield return UpdateChunk();
            }
        }

        /// <summary>
        /// Converts a world into a mesh data. Limited at 1 chunk per world for this.
        /// TODO: Somewhere here is a 100ms GC.Collect call - due to allocation/deallocation of memory
        /// </summary>
        private IEnumerator BuildChunkMeshPerMaterial(int MaterialIndex)
        {
            if (MyWorld)
            {
                //List<Thread> QuedThreads = new List<Thread>();
                List<Thread> MyThreads = new List<Thread>();
                Int3 BuildVoxelIndex = Int3.Zero();
                for (BuildVoxelIndex.x = 0; BuildVoxelIndex.x < ChunkSize; BuildVoxelIndex.x++)
                {
                    for (BuildVoxelIndex.y = 0; BuildVoxelIndex.y < ChunkSize; BuildVoxelIndex.y++)
                    {
                        for (BuildVoxelIndex.z = 0; BuildVoxelIndex.z < ChunkSize; BuildVoxelIndex.z++)
                        {
                            Voxel MeshingVoxel = MyVoxels.GetVoxelRaw(BuildVoxelIndex);
                            if (MeshingVoxel != null)
                            {
                                int VoxelType = MeshingVoxel.GetVoxelType();
                                if (VoxelType != 0)
                                {
                                    if (MeshingVoxel.GetHasUpdated() == true || MyWorld.IsReBuildAllMeshes)
                                    {
                                        VoxelMeta ThisMeta = GetWorld().GetVoxelMeta(VoxelType);
                                        Int3 PositionBuilding = new Int3(BuildVoxelIndex.x, BuildVoxelIndex.y, BuildVoxelIndex.z);
                                        CalculatePolyModel(MeshingVoxel, PositionBuilding, VoxelType, ThisMeta,
                                            CalculateSides(MeshingVoxel, PositionBuilding, VoxelType, ThisMeta),
                                            MaterialIndex);
                                        MeshingVoxel.OnBuiltMesh(); // as the mesh has been rebuilt!
                                        /*MyThreads.Add(new Thread(
                                        () =>
                                        {
                                        }));
                                        MyThreads[MyThreads.Count - 1].Start();*/
                                    }
                                }
                            }
                        }
                    }
                    yield return MyWorld.YieldTimer();
                }
                while (MyThreads.Count > 0)
                {
                    /*if (QuedThreads.Count > 0)
                    {
                        for (int i = 0; i < Mathf.Max(0, 200 - QuedThreads.Count); i++)
                        {
                            MyThreads.Add(QuedThreads[QuedThreads.Count - 1]);
                            QuedThreads.RemoveAt(QuedThreads.Count - 1);
                            MyThreads[MyThreads.Count - 1].Start();
                        }
                    }*/
                    for (int i = MyThreads.Count - 1; i >= 0; i--)
                    {
                        if (MyThreads[i].IsAlive == false)
                        {
                            MyThreads.RemoveAt(i);
                        }
                        if (i % 50 == 0)
                        {
                            yield return MyWorld.YieldTimer();
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("No Longer supporting mesh building without a database");
            }
        }

        /// <summary>
        /// Get the sides of the voxel! True if is to draw!
        /// </summary>
        private bool[] CalculateSides(Voxel MeshingVoxel, Int3 MeshingBlockPosition, int VoxelType, VoxelMeta SidesCalculationMeta)
        {
            bool[] MySides = new bool[] { false, false, false, false, false, false };
            if (SidesCalculationMeta != null)
            {
                PolyModel ModelOther;
                VoxelMeta MetaOther;

                Int3 AbovePosition = (MeshingBlockPosition.Above());
                Int3 BelowPosition=(MeshingBlockPosition.Below());
                Int3 FrontPosition=(MeshingBlockPosition.Front());
                Int3 BehindPosition=(MeshingBlockPosition.Behind());
                Int3 LeftPosition=(MeshingBlockPosition.Left());
                Int3 RightPosition=(MeshingBlockPosition.Right());

                Voxel VoxelAbove = GetVoxel(AbovePosition);
                Voxel VoxelBelow = GetVoxel(BelowPosition);
                Voxel VoxelFront = GetVoxel(FrontPosition);
                Voxel VoxelBehind = GetVoxel(BehindPosition);
                Voxel VoxelLeft = GetVoxel(LeftPosition);
                Voxel VoxelRight = GetVoxel(RightPosition);

                // Solids
                bool IsSolidAbove = false;
                bool IsSolidBelow = false;
                bool IsSolidFront = false;
                bool IsSolidBehind = false;
                bool IsSolidLeft = false;
                bool IsSolidRight = false;
                PolyModel SidesCalculationModel = SidesCalculationMeta.GetModel();
                if (SidesCalculationModel != null)
                {
                    IsSolidAbove = SidesCalculationModel.IsSolid((int)Direction.Up);
                    IsSolidBelow = SidesCalculationModel.IsSolid((int)Direction.Down);
                    IsSolidFront = SidesCalculationModel.IsSolid((int)Direction.Forward);
                    IsSolidBehind = SidesCalculationModel.IsSolid((int)Direction.Back);
                    IsSolidLeft = SidesCalculationModel.IsSolid((int)Direction.Left);
                    IsSolidRight = SidesCalculationModel.IsSolid((int)Direction.Right);
                }

                // Y Axis
                if (VoxelAbove == null || IsSolidAbove == false)
                {
                    MySides[0] = true;
                }
                else    // else check if voxel above is air or non solid (like a torch)
                {
                    MetaOther = GetWorld().GetVoxelMeta(VoxelAbove.GetVoxelType());
                    ModelOther = MetaOther.GetModel();
                    MySides[0] = (MetaOther.Name == AirName || ModelOther.IsSolid((int)Direction.Down) == false);
                }

                if (VoxelBelow == null || IsSolidBelow == false)
                {
                    MySides[1] = true;
                }
                else
                {
                    MetaOther = GetWorld().GetVoxelMeta(VoxelBelow.GetVoxelType());
                    ModelOther = MetaOther.GetModel();
                    MySides[1] = (MetaOther.Name == AirName || ModelOther.IsSolid((int)Direction.Up) == false);
                }

                // Z Axis
                if (VoxelFront == null || IsSolidBehind == false)
                {
                    MySides[2] = true;
                }
                else
                {
                    MetaOther = GetWorld().GetVoxelMeta(VoxelFront.GetVoxelType());
                    ModelOther = MetaOther.GetModel();
                    MySides[2] = (MetaOther.Name == AirName || ModelOther.IsSolid((int)Direction.Forward) == false);
                }

                if (VoxelBehind == null || IsSolidFront == false)
                {
                    MySides[3] = true;
                }
                else
                {
                    MetaOther = GetWorld().GetVoxelMeta(VoxelBehind.GetVoxelType());
                    ModelOther = MetaOther.GetModel();
                    MySides[3] = (MetaOther.Name == AirName || ModelOther.IsSolid(((int)Direction.Back)) == false);
                }

                // X Axis
                if (VoxelRight == null || IsSolidLeft == false)
                {
                    MySides[4] = true;
                }
                else
                {
                    MetaOther = GetWorld().GetVoxelMeta(VoxelRight.GetVoxelType());
                    ModelOther = MetaOther.GetModel();
                    MySides[4] = (MetaOther.Name == AirName || ModelOther.IsSolid((int)Direction.Right) == false);
                }

                if (VoxelLeft == null || IsSolidRight == false)
                {
                    MySides[5] = true;
                }
                else
                {
                    MetaOther = GetWorld().GetVoxelMeta(VoxelLeft.GetVoxelType());
                    ModelOther = MetaOther.GetModel();
                    MySides[5] = (MetaOther.Name == AirName || ModelOther.IsSolid(((int)Direction.Left)) == false);
                }
            }
            return MySides;
        }

        /// <summary>
        /// Get the solid sides around the voxel - used for side culling
        /// </summary>
        private void CalculateSolids(Voxel MeshingVoxel, Int3 MeshingBlockPosition)
        {
            bool[] MySolids = new bool[27];
            int CalculateSolidsIndex = 0;  // 0 to 26 - 27 size
            for (int a = MeshingBlockPosition.x - 1; a <= MeshingBlockPosition.x + 1; a++)
            {
                for (int b = MeshingBlockPosition.y - 1; b <= MeshingBlockPosition.y + 1; b++)
                {
                    for (int c = MeshingBlockPosition.z - 1; c <= MeshingBlockPosition.z + 1; c++)
                    {
                        MySolids[CalculateSolidsIndex] = (MeshingVoxel.GetVoxelType() != 0); // if not air
                        CalculateSolidsIndex++;
                    }
                }
            }
        }

        public List<Chunk> GetSurroundingChunks()
        {
            List<Chunk> MyChunks = new List<Chunk>();
            Chunk OtherChunk = MyWorld.GetChunk(Position.Left());
            if (OtherChunk != null)
            {
                MyChunks.Add(OtherChunk);
            }
            OtherChunk = MyWorld.GetChunk(Position.Right());
            if (OtherChunk != null)
            {
                MyChunks.Add(OtherChunk);
            }
            OtherChunk = MyWorld.GetChunk(Position.Front());
            if (OtherChunk != null)
            {
                MyChunks.Add(OtherChunk);
            }
            OtherChunk = MyWorld.GetChunk(Position.Behind());
            if (OtherChunk != null)
            {
                MyChunks.Add(OtherChunk);
            }
            OtherChunk = MyWorld.GetChunk(Position.Above());
            if (OtherChunk != null)
            {
                MyChunks.Add(OtherChunk);
            }
            OtherChunk = MyWorld.GetChunk(Position.Below());
            if (OtherChunk != null)
            {
                MyChunks.Add(OtherChunk);
            }
            return MyChunks;
        }

        /// <summary>
        /// For a voxel, create a mesh for it!
        /// </summary>
        private void CalculatePolyModel(Voxel MyVoxel, Int3 MeshingBlockPosition, int VoxelType,
                                        VoxelMeta CalculatePolyModelMeta, bool[] MySides, int MaterialIndex)
        {
            if ((CalculatePolyModelMeta != null &&
                MaterialIndex == CalculatePolyModelMeta.MaterialID)
                || CalculatePolyModelMeta.GetModel() != null)
            {
                MyVoxel.MyMeshData.Clear();
                for (int SideIndex = 0; SideIndex < MySides.Length; SideIndex++)     // for all 6 sides of the voxel
                {
                    if (MySides[SideIndex])
                    {
                        // first add mesh verticies
                        MyVoxel.MyMeshData.AddDataMesh = CalculatePolyModelMeta.GetModel().GetModel(SideIndex);
                        MyVoxel.MyMeshData.Add();
                        // Add the range of uvs just for the rendered verticies
                        MyVoxel.MyMeshData.TextureCoordinates.AddRange(
                            CalculatePolyModelMeta.GetModel().GetTextureMapCoordinates(
                                CalculatePolyModelMeta.TextureMapID,
                                SideIndex,
                                MyWorld.MyTilemap));
                    }
                }
                MyVoxel.MyMeshData.Colors.Clear();
                Color VoxelColor = MyVoxel.GetColor();
                for (int z = 0; z < MyVoxel.MyMeshData.Verticies.Count; z++)
                {
                    MyVoxel.MyMeshData.Colors.Add(VoxelColor);
                }

                // Multiply by voxel scale - both the model data and the grid positioning!
                MyVoxel.MyMeshData.MultiplyVerts(MyWorld.VoxelScale);
                // Add the grid position to each voxel model!
                Vector3 VoxelVertexOffset = new Vector3(
                    MyWorld.VoxelScale.x * MeshingBlockPosition.x,
                    MyWorld.VoxelScale.y * MeshingBlockPosition.y,
                    MyWorld.VoxelScale.z * MeshingBlockPosition.z);
                MyVoxel.MyMeshData.AddToVertex(VoxelVertexOffset);
            }
            else
            {
                Debug.LogError("Model is null>");
            }
        }

        /*if (ThisVoxel.MyMeshData.TextureCoordinates.Count != ThisVoxel.MyMeshData.Verticies.Count)
        {
            //Debug.LogError("Voxel is out of UV bounds: " + ThisVoxel.MyMeshData.TextureCoordinates.Count + ":" + ThisVoxel.MyMeshData.Verticies.Count
            //    + " - for meta: " + MyMeta.Name + " with model " + MyMeta.ModelID + " and textureID: " + MyMeta.TextureMapID);
            ThisVoxel.MyMeshData.TextureCoordinates.Clear();
            for (int i = 0; i < ThisVoxel.MyMeshData.Verticies.Count; i++)
            {
                ThisVoxel.MyMeshData.TextureCoordinates.Add(new Vector2(0, 0));
            }
        }*/
        #endregion

        #region VoxelToMeshRenderer

        /// <summary>
        /// Called in WorldUpdater when buuilding mesh starts
        /// </summary>
        public void OnBuildingMesh()
        {
            // Refresh things to build mesh with
            IsBuildingMesh = true;  // started to build
        }

        /// <summary>
        /// Updates the meshes using the voxel data -> Called by Chunk Updater. 
        /// </summary>
        public IEnumerator UpdateChunk()
        {
            yield return UpdateMesh();       // updates the mesh
            SpawnCharactersOnChunk();   // sets characters spawned to active
            DropVoxels();               // after mesh is built and updated
            ChunkMeshes.Clear();
            IsUpdatingRender = false;
            HasStartedUpdating = false;
            if (!HasInitiated)
            {
                HasInitiated = true;
            }
        }

        //private Color VoxelColor;
        /// <summary>
        /// Method used to update mesh renderer mesh.
        /// Combines the ChunkMeshes into one mesh, using different materials
        /// </summary>
        private IEnumerator UpdateMesh()
        {
            Int3 CreateMeshVoxelIndex = Int3.Zero();
            Mesh NewMesh = new Mesh();
            Voxel CreateMeshVoxel;
            MeshData CreateMeshMeshData;
            // Set up meshes
            for (int CreateMeshChunkIndex = 0; CreateMeshChunkIndex < ChunkMeshes.Count; CreateMeshChunkIndex++)
            {
                CreateMeshMeshData = ChunkMeshes[CreateMeshChunkIndex];
                if (CreateMeshMeshData != null)
                {
                    CreateMeshMeshData.Clear();
                    for (CreateMeshVoxelIndex.x = 0; CreateMeshVoxelIndex.x < Chunk.ChunkSize; CreateMeshVoxelIndex.x++)
                    {
                        for (CreateMeshVoxelIndex.y = 0; CreateMeshVoxelIndex.y < Chunk.ChunkSize; CreateMeshVoxelIndex.y++)
                        {
                            for (CreateMeshVoxelIndex.z = 0; CreateMeshVoxelIndex.z < Chunk.ChunkSize; CreateMeshVoxelIndex.z++)
                            {
                                CreateMeshVoxel = MyVoxels.GetVoxel(CreateMeshVoxelIndex);
                                if (CreateMeshVoxel != null)
                                {
                                    CreateMeshMeshData.AddDataMesh = CreateMeshVoxel.MyMeshData;
                                    CreateMeshMeshData.Add();
                                }
                            }
                        }
                        yield return MyWorld.YieldTimer();
                    }
                    CreateMeshMeshData.AddToVertex(-MyWorld.CentreOffset);
                }
            }
            //Debug.Log(MyWorld.name + "'s chunk has vertex count of: " + ChunkMeshes[0].Verticies.Count + ":" + MyWorld.WorldSize.ToString());
            //Debug.LogError("Updated Mesh: ChunkMeshes: " + ChunkMeshes.Count +
            //    " - Materials: " + GetWorld().MyMaterials.Count);
            if (ChunkMeshes.Count > 0)
            {
                int VertCount = 0;
                List<CombineInstance> CombiningMeshList = new List<CombineInstance>();
                // Combine all the materials together
                for (int MaterialIndex = 0; MaterialIndex < MyWorld.GetMaterials().Count; MaterialIndex++)
                {
                    CreateMeshMeshData = ChunkMeshes[MaterialIndex];
                    if (CreateMeshMeshData != null)
                    {
                        CombineInstance NewCombineInstance = new CombineInstance();
                        NewCombineInstance.mesh = new Mesh();
                        NewCombineInstance.transform = transform.localToWorldMatrix;
                        CombiningMeshList.Add(NewCombineInstance);
                        CombiningMeshList[MaterialIndex].mesh.vertices = CreateMeshMeshData.GetVerticies();
                        CombiningMeshList[MaterialIndex].mesh.triangles = CreateMeshMeshData.GetTriangles();
                        CombiningMeshList[MaterialIndex].mesh.uv = CreateMeshMeshData.GetTextureCoordinates();
                        CombiningMeshList[MaterialIndex].mesh.colors32 = CreateMeshMeshData.GetColors().ToArray();
                        VertCount += CreateMeshMeshData.Verticies.Count;
                        yield return MyWorld.YieldTimer();
                    }
                }
                //Debug.LogError("Before CombineMeshes.");
                NewMesh.CombineMeshes(CombiningMeshList.ToArray(), false, false);
                //Debug.LogError("After CombineMeshes.");
                //Debug.LogError("Updated Mesh:[" + name + "] Vertexes: " + MyMeshFilter.sharedMesh.vertexCount);
                NewMesh.subMeshCount = CombiningMeshList.Count;
                NewMesh.name = name + " Mesh";
                NewMesh.RecalculateNormals();
                //MyMeshFilter.sharedMesh.RecalculateBounds();
                NewMesh.RecalculateTangents();
                for (int MaterialIndex = 0; MaterialIndex < CombiningMeshList.Count; MaterialIndex++)
                {
                    MonoBehaviourExtension.Kill(CombiningMeshList[MaterialIndex].mesh);
                }
                //Debug.LogError("Before UploadMeshData.");
                NewMesh.UploadMeshData(false);
                //Debug.LogError("Finished UploadMeshData.");
                if (MyMeshFilter.sharedMesh != null)
                {
                    DestroyImmediate(MyMeshFilter.sharedMesh);
                }
                MyMeshFilter.sharedMesh = NewMesh;
            }
            else
            {
                Debug.LogError("Chunk Mesh is null inside: " + GetWorld().name + ":" + name);
            }
            MyVoxels.Reset();
            MyMeshRenderer.sharedMaterials = MyWorld.GetMaterials().ToArray();
            // colliders
            if (MyMeshCollider)
            {
                MyMeshCollider.sharedMesh = null;
                MyMeshCollider.sharedMesh = MyMeshFilter.sharedMesh;
            }
        }
        #endregion
        
        // Propogates lighting accross Voxels
        #region Lighting

        /// <summary>
        /// Calculate the lights per voxel
        /// </summary>
        private void CalculateLights()
        {
            //int[] MyLights = new int[27];
            //GetLights(ThisVoxel, i, j, k, MaterialType, ref MyLights);
            // build the actual model
        }

        /// <summary>
        /// Get the lights around the voxel
        /// </summary>
        private void GetLights(Voxel ThisVoxel, int i, int j, int k, int MaterialType, ref int[] MyLights)
        {
            if (MyWorld.IsLighting)
            {
                //MyLights = ThisVoxel.GetSurroundingLights(this, i, j, k, MaterialType);
            }
            else
            {
                //MyLights = ThisVoxel.GetBasicLights(this, i, j, k, MaterialType);
            }
            for (int a = 0; a < MyLights.Length; a++)
            {
                MyLights[a] = 255;
            }
        }

        /// <summary>
        /// Chunk lighting!
        /// </summary>
        public static MeshData AddBasicLights(MeshData MyVoxelMesh, int[] MyLights, int SideIndex)
        {
            byte ThisLight = (byte)MyLights[SideIndex];
            MyVoxelMesh.AddQuadColours(ThisLight);
            return MyVoxelMesh;
        }
        /// <summary>
        /// A smoothed version of lighting
        /// </summary>
        public static MeshData AddSmoothedLights(MeshData MyVoxelMesh, int[] MyLights)
        {
            Vector3[] MyVerticies = MyVoxelMesh.GetVerticies();
            int[] MyTriangles = MyVoxelMesh.GetTriangles();
            //Color32[] MyColors = MyVoxelMesh.GetColors();
            for (int z = 0; z < MyTriangles.Length; z += 3)
            {
                Vector3 Vertex1 = MyVerticies[MyTriangles[z]];
                Vector3 Vertex2 = MyVerticies[MyTriangles[z + 1]];
                Vector3 Vertex3 = MyVerticies[MyTriangles[z + 2]];
                // average the light value out by this
                int LightsAdded1 = 0;
                int LightsAdded2 = 0;
                int LightsAdded3 = 0;
                // if light source hits triangle, add light to it
                int LightStrength1 = 0;
                int LightStrength2 = 0;
                int LightStrength3 = 0;
                int LightIndex = 0;

                //List<int> LightAddedIndex = new List<int>();
                //List<Vector3> LightPositions = new List<Vector3>();
                for (int j = -1; j <= 1; j++)
                    for (int i = -1; i <= 1; i++)
                        for (int k = -1; k <= 1; k++)
                        {
                            int LightBrightness = MyLights[LightIndex];
                            LightIndex++;
                            Vector3 LightPosition = new Vector3(i, j, k) + new Vector3(0.5f, 0.5f, 0.5f);   // to make the lights in the middle of cubes
                            Vector3 TriangleMidPoint = (Vertex1 + Vertex2 + Vertex3) / 3f;
                            // Get Direction of Light Souce to Triangle
                            Vector3 LightDirection = (TriangleMidPoint - LightPosition).normalized;

                            // if light hits triangle, add the light
                            //if (DoesIntersect (Vertex1, Vertex2, Vertex3, LightPosition, LightDirection)) 
                            if (DoesIntersectPlane(Vertex1, Vertex2, Vertex3, LightPosition))
                            {
                                float LightDistance1 = Vector3.Distance(LightPosition, Vertex1);
                                if (LightDistance1 < 1)
                                {
                                    LightsAdded1++;
                                    LightStrength1 += Mathf.RoundToInt(LightDistance1 * ((float)LightBrightness));
                                    //LightAddedIndex.Add (LightIndex);
                                    //LightPositions.Add (LightPosition);
                                }
                                float LightDistance2 = Vector3.Distance(LightPosition, Vertex2);
                                if (LightDistance2 < 1)
                                {
                                    LightsAdded2++;
                                    LightStrength2 += Mathf.RoundToInt(LightDistance2 * ((float)LightBrightness));
                                }
                                float LightDistance3 = Vector3.Distance(LightPosition, Vertex3);
                                if (LightDistance3 < 1)
                                {
                                    LightsAdded3++;
                                    LightStrength3 += Mathf.RoundToInt(LightDistance3 * ((float)LightBrightness));
                                }
                                //Debug.LogError ("Light intersection at: " + LightPosition.ToString ());
                            }
                        }

                if (LightsAdded1 != 0)
                {
                    LightStrength1 /= LightsAdded1;
                    //if (SideIndex == 0)
                    {
                        //Debug.LogError ("Lights1 added: " + LightStrength1);
                        //for (int i = 0; i < LightAddedIndex.Count; i++)
                        //  Debug.LogError ("Side [" + SideIndex + "] Light Index [" + LightAddedIndex[i] + "] And light position [" + LightPositions[i] + "]");
                        //Debug.Break ();
                    }
                }
                if (LightsAdded2 != 0)
                {
                    LightStrength2 /= LightsAdded2;
                    //Debug.LogError ("Lights2 added: " + LightStrength2);
                }
                if (LightsAdded3 != 0)
                {
                    LightStrength3 /= LightsAdded3;
                    //Debug.LogError ("Lights3 added: " + LightStrength3);
                }
                MyVoxelMesh.SetColor(MyTriangles[z], new Color32((byte)LightStrength1, (byte)LightStrength1, (byte)LightStrength1, 255));
                MyVoxelMesh.SetColor(MyTriangles[z + 1], new Color32((byte)LightStrength2, (byte)LightStrength2, (byte)LightStrength2, 255));
                MyVoxelMesh.SetColor(MyTriangles[z + 2], new Color32((byte)LightStrength3, (byte)LightStrength3, (byte)LightStrength3, 255));

            }
            return MyVoxelMesh;
        }

        public static bool DoesIntersectPlane(Vector3 Vertex1, Vector3 Vertex2, Vector3 Vertex3, Vector3 RayOrigin)
        {
            Plane MyPlane = new Plane(Vertex1, Vertex2, Vertex3);
            return MyPlane.GetSide(RayOrigin);
        }
        /// <summary>
        /// Set the brightness at a position
        /// </summary>
        public void SetBrightness(int i, int j, int k, float Brightness)
        {
            SetBrightness(i, j, k, Mathf.RoundToInt(Brightness));
        }
        /// <summary>
        /// Sets the brightness at a position using integers
        /// </summary>
        public void SetBrightness(int i, int j, int k, int Brightness)
        {
            /*if (GetVoxel (i, j, k).SetBrightness (this, i, j, k, Brightness)) 
            {
                //if (IsPrimaryChange)
                HasChangedAt (i, j, k);
            }*/
        }
        /// <summary>
        /// Sets the brightness at a position with mass
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="k"></param>
        /// <param name="Brightness"></param>
        public void SetBrightnessMass(int i, int j, int k, float Brightness)
        {
            SetBrightnessMass(i, j, k,
                Mathf.RoundToInt(Brightness));
        }
        public void SetBrightnessMass(int i, int j, int k, int Brightness)
        {
            //GetVoxel (i, j, k).SetBrightnessUnChanged (this, i, j, k, Brightness);
        }
        /// <summary>
        /// Sets all lights to maximum brightness
        /// </summary>
        public void ResetLighting()
        {
            for (int i = 0; i < ChunkSize; i++)
            {
                for (int j = 0; j < ChunkSize; j++)
                {
                    for (int k = 0; k < ChunkSize; k++)
                    {
                        SetBrightnessMass(i, j, k, MyWorld.SunBrightness);
                    }
                }
            }
        }
        /// <summary>
        /// Casts sunlight from the top most block, down to the bottom
        /// maybe later do directional sunlight?
        /// </summary>
        public void Sunlight()
        {
            //Debug.LogError("Updating Sunlight!");
            for (int i = 0; i < ChunkSize; i++)
                for (int k = 0; k < ChunkSize; k++)
                {
                    bool IsDarkness = false;
                    for (int j = ChunkSize - 1; j >= 0; j--)
                    {
                        Voxel ThisVoxel = GetVoxel(new Int3(i, j, k));
                        //bool HasChanged = false;
                        if (IsDarkness)
                        {
                            /*if (ThisVoxel.GetBlockIndex() == 6)   // if water
                            {
                                SetBrightnessMass(i, j, k, MyWorld.SunBrightness * MyWorld.PropogationDecreaseRate);
                            }
                            else
                            {*/
                            SetBrightnessMass(i, j, k, MyWorld.DefaultBrightness);
                            //}
                            //HasChanged = GetVoxel (i, j, k).SetBrightness (this, i, j, k, DefaultBrightness);
                        }
                        else
                        {
                            if (ThisVoxel.GetVoxelType() == 0)
                            {
                                SetBrightnessMass(i, j, k, MyWorld.SunBrightness);
                                //HasChanged = GetVoxel (i, j, k).SetBrightness (this, i, j, k, SunBrightness);
                            }
                            else
                            {
                                IsDarkness = true;
                                SetBrightnessMass(i, j, k, MyWorld.DefaultBrightness);
                                //HasChanged = GetVoxel (i, j, k).SetBrightness (this, i, j, k, DefaultBrightness);
                            }
                        }

                    }
                }
        }

        /// <summary>
        /// Starts the light propogation.
        /// </summary>
        public void PropogateLights()
        {
            Debug.Log("Propogating lights in " + name);
            //IsInLightQue = false;
            //IsRunningLightsThread = true;
            int Buffer = 1;
            if (MyWorld.IsLighting)
            {
                Sunlight();
                for (int i = -Buffer; i < ChunkSize + Buffer; i++)
                {
                    for (int j = -Buffer; j < ChunkSize + Buffer; j++)
                    {
                        for (int k = -Buffer; k < ChunkSize + Buffer; k++)
                        {
                            Voxel ThisVoxel = GetVoxel(new Int3(i, j, k));
                            if (ThisVoxel != null)
                            {
                                int Brightness = 255;// (int)ThisVoxel.GetLight ();
                                int BlockIndex = ThisVoxel.GetVoxelType();
                                if (BlockIndex == 0 || BlockIndex == 6)
                                {
                                    //PropogateLight (i, j, k, MyBrightnss, 0);
                                    PropogateLight(i + 1, j, k, Brightness, 1);
                                    PropogateLight(i - 1, j, k, Brightness, 1);
                                    PropogateLight(i, j + 1, k, Brightness, 1);
                                    PropogateLight(i, j - 1, k, Brightness, 1);
                                    PropogateLight(i, j, k + 1, Brightness, 1);
                                    PropogateLight(i, j, k - 1, Brightness, 1);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < ChunkSize; i++)
                    for (int j = 0; j < ChunkSize; j++)
                        for (int k = 0; k < ChunkSize; k++)
                        {
                            //Voxel ThisVoxel = GetVoxel(new Int3(i, j, k));
                            //ThisVoxel.SetBrightness(this, i, j, k, MyWorld.SunBrightness);
                        }
            }
        }

        /// <summary>
        /// Propogates the light, using celluar automato, to move lighting accross voxels.
        /// </summary>
        public void PropogateLight(int i, int j, int k, int Brightness, int PropogationCount)
        {
            /*Voxel ThisVoxel = GetVoxel (i, j, k);
            Brightness = Mathf.RoundToInt (Brightness* MyWorld.PropogationDecreaseRate);
            if (ThisVoxel != null)
            if (PropogationCount <= 8
                && Brightness > MyWorld.DefaultBrightness
                && ThisVoxel.GetVoxelType() == 0
                && Brightness > GetVoxel (i, j, k).GetLight ()
                // && IsInRange (i) && IsInRange (j) && IsInRange (k)       // is within this chunk
                )
            {
                    SetBrightnessMass(i,j,k, Brightness);
                PropogationCount++;
                PropogateLight (i + 1, j, k, Brightness,PropogationCount);
                PropogateLight (i - 1, j, k, Brightness,PropogationCount);
                PropogateLight (i, j + 1, k, Brightness,PropogationCount);
                PropogateLight (i, j - 1, k, Brightness,PropogationCount);  
                PropogateLight (i, j, k + 1, Brightness,PropogationCount);
                PropogateLight (i, j, k - 1, Brightness,PropogationCount);
            } else {
                //Debug.LogError("Blarg!: " + Brightness);
            }*/
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
            Debug.LogError(name + " - MassUpdated: " + WasMassUpdated);
            if (WasMassUpdated)
            {
                //Debug.LogError(name + " was mass updated: " + Time.realtimeSinceStartup);
                WasMassUpdated = false;
                HasStartedUpdating = true;
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
            for (int ChunkIndex = 0; ChunkIndex < SurroundingChunks.Count; ChunkIndex++)
            {
                if (World.TestingRefreshSides)
                {
                    SurroundingChunks[ChunkIndex].WasMassUpdated = true;
                    SurroundingChunks[ChunkIndex].SetAllUpdatedSides();
                }
            }
        }
        /// <summary>
        /// I should keep these as reference
        /// </summary>
        private void RefreshSurroundingChunks()
        {
            SurroundingChunks.Clear();
            // front
            Chunk OtherChunk = GetWorld().GetChunk(new Int3(Position.x, Position.y, Position.z + 1));
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
            WorldUpdater.Get().Add(this);
        }
        #endregion

        #region Instanced
        [Header("Instanced")]
        public List<Character> MyCharacters = new List<Character>();
        public List<Zone> MyZones = new List<Zone>();
        public List<Items.ItemHandler> Items = new List<Items.ItemHandler>();

        public IEnumerator ActivateCharacters()
        {
            for (int i = 0; i < MyCharacters.Count; i++)
            {
                if (MyCharacters[i])
                {
                    yield return MyCharacters[i].ActivateCharacter();
                }
            }
        }

        public void AddTransform(Transform NewTransform)
        {
            Character NewCharacter = NewTransform.gameObject.GetComponent<Character>();
            if (NewCharacter && MyCharacters.Contains(NewCharacter) == false)
            {
                MyCharacters.Add(NewCharacter);
                return;
            }
            Zone NewZone = NewTransform.gameObject.GetComponent<Zone>();
            if (NewCharacter && MyZones.Contains(NewZone) == false)
            {
                MyZones.Add(NewZone);
                return;
            }
        }

        public void RemoveTransform(Transform OldTransform)
        {
            Character OldCharacter = OldTransform.gameObject.GetComponent<Character>();
            if (OldCharacter && MyCharacters.Contains(OldCharacter) == true)
            {
                MyCharacters.Remove(OldCharacter);
                return;
            }
            Zone OldZone = OldTransform.gameObject.GetComponent<Zone>();
            if (OldZone && MyZones.Contains(OldZone) == true)
            {
                MyZones.Remove(OldZone);
                return;
            }
        }

        private List<string> GetInstancedList()
        {
            List<string> MyData = new List<string>();
            if (MyCharacters.Count > 0)
            {
                MyData.Add("/Characters");
                for (int i = 0; i < MyCharacters.Count; i++)
                {
                    MyData.Add(MyCharacters[i].GetData().Name);
                }
                MyData.Add("/EndCharacters");
            }
            if (MyZones.Count > 0)
            {
                MyData.Add("/Zones");
                for (int i = 0; i < MyZones.Count; i++)
                {
                    MyData.Add(MyZones[i].GetData().Name);
                }
                MyData.Add("/EndZones");
            }
            if (Items.Count > 0)
            {
                MyData.Add("/Items");
                for (int i = 0; i < MyZones.Count; i++)
                {
                    MyData.Add(Items[i].MyItem.Name);
                }
                MyData.Add("/EndItems");
            }
            return MyData;
        }
        #endregion
    }
}
