using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Util;

namespace Zeltex.Voxels
{

    [System.Serializable()]
    public class ChunkDictionary : SerializableDictionaryBase<Int3, Chunk>
    {

    }

    /// <summary>
    /// World Resizing part of the world
    /// </summary>
    public partial class World : MonoBehaviour
    {
        [Header("Grid Properties")]
        [SerializeField]
        public bool IsCentreWorld;          // centres the entire mesh via bounds, used by item manager, 
        [SerializeField]
        public Vector3 VoxelScale = new Vector3(1, 1, 1);
        [Tooltip("The size of the worlds chunks")]
        [SerializeField]
        private Int3 WorldSize = Int3.Zero();
        [Tooltip("The offset of the worlds chunks")]
        [SerializeField]
        public Int3 PositionOffset = Int3.Zero();    // start off with nothing
        // repositioning
       // private List<Vector3> MoveToList = new List<Vector3>();
       // [SerializeField]
       // private List<Chunk> ChunkRepositionList = new List<Chunk>();
        //private Chunk RoamingChunk;
        // testing values, default is true
        public static bool TestingIsRefreshOldChunks = true;   // wipes chunks when repositioned
        public static bool TestingRefreshSides = true;         // refreshes chunk sides when reposiioned
        public static bool TestingCreateTerrain = true;        // creates Terrain when repositioned
        public static bool TestingCreatePlane = false;          // creates planes when repositioned

        public delegate void OnSizeFinishedEvent(World ResizedWorld);

        [Tooltip("Models won't be centred, but worlds will.")]
        //[SerializeField]
        public bool IsChunksCentred = false;
        //private bool IsResizingWorld;

        Vector3 WorldTotalSize = Vector3.zero;
        Vector3 OldCentreOffset = Vector3.zero;

        #region Resizing
        /// <summary>
        /// calls the resize routine
        /// </summary>
        public void SetWorldSize(Vector3 NewLimit, OnSizeFinishedEvent OnResized = null)
        {
            UniversalCoroutine.CoroutineManager.StartCoroutine(SetWorldSizeRoutine(NewLimit.ToInt3(), OnResized));
        }
        /// <summary>
        /// calls the resize routine
        /// </summary>
        public void SetWorldSize(Int3 NewLimit, OnSizeFinishedEvent OnResized = null)
        {
            UniversalCoroutine.CoroutineManager.StartCoroutine(SetWorldSizeRoutine(NewLimit, OnResized));
        }

        /// <summary>
        /// Centre the mesh data verts by the world size
        /// </summary>
        public void CalculateCentre()
        {
            if (IsCentreWorld)
            {
                // now get world size
                WorldTotalSize = GetWorldSize();
                WorldTotalSize.x = WorldTotalSize.x / MyLossyScale.x;
                WorldTotalSize.y = WorldTotalSize.y / MyLossyScale.y;
                WorldTotalSize.z = WorldTotalSize.z / MyLossyScale.z;
                OldCentreOffset.Set(CentreOffset.x, CentreOffset.y, CentreOffset.z);
                CentreOffset = WorldTotalSize / 2f;
                if (float.IsNaN(CentreOffset.x) || float.IsNaN(CentreOffset.y) || float.IsNaN(CentreOffset.z))
                {
                    CentreOffset = Vector3.zero;
                }
            }
            else
            {
                CentreOffset = Vector3.zero;
            }
        }

        /// <summary>
        /// Set the worlds size, takes time
        /// </summary>
        public IEnumerator SetWorldSizeRoutine(Int3 NewSize, OnSizeFinishedEvent OnResized = null)
        {
            yield return UniversalCoroutine.CoroutineManager.StartCoroutine(SetWorldSizeRoutine(NewSize, Int3.Zero(), OnResized));
        }

        /// <summary>
        /// Changes a world to a single chunk or a group of chunks, but can force the refresh
        /// </summary>
        public IEnumerator SetWorldSizeRoutine(Int3 NewSize, Int3 NewPositionOffset, OnSizeFinishedEvent OnResized = null)
        {
            yield return (SetPositionOffset(NewPositionOffset));
            Int3 OldLimit = Int3.Zero();
            OldLimit.Set(WorldSize);
            float TimeBegun = Time.realtimeSinceStartup;
            WorldUpdater.Get().Clear(this);  // Stop Updating if Resizing!
            if (NewSize != null && NewSize != OldLimit)
            {
                Debug.Log("Resizing [" + name + "] from: " + OldLimit.ToString() + " to " + NewSize.ToString());    // it's nice to know this
                WorldSize = new Int3(NewSize);   // update limit
                CalculateCentre();
                //MyChunkData.Clear();    // clear our dictionary
                // assuming they are changing size... Refresh the chunks here
                if (NewSize == Int3.Zero())
                {
                    yield return UniversalCoroutine.CoroutineManager.StartCoroutine(ClearChunks());
                }
                else if (!IsSingleChunk(NewSize) && !IsSingleChunk(OldLimit))  // both multi chunks
                {
                    yield return UniversalCoroutine.CoroutineManager.StartCoroutine(ResizeMultiChunkWorld(OldLimit));
                }
                else if (!IsSingleChunk(NewSize) && IsSingleChunk(OldLimit))   // Going from SingleChunk to MultiChunk Mode
                {
                    yield return UniversalCoroutine.CoroutineManager.StartCoroutine(ConvertToMultiChunkWorld());
                }
                else if (IsSingleChunk(NewSize) && !IsSingleChunk(OldLimit))  // if transforming from multi to singular
                {
                    yield return UniversalCoroutine.CoroutineManager.StartCoroutine(ConvertToSingleChunkWorld());
                }
                else if (IsSingleChunk(NewSize) && IsSingleChunk(OldLimit))   // if both single chunk
                {
                    Debug.LogWarning(name + " is Going from Single Chunk to Single Chunk.");
                }
            }
            if (OnResized != null)
            {
                Debug.Log("Invoking Delegate for world " + name + " resizing :)");
                OnResized.Invoke(this);
            }
            //yield return null;
            //Debug.LogError("Resized World: " + OldLimit.ToString() + ":" + NewLimit.ToString() + ": Time:" + (Time.realtimeSinceStartup - TimeBegun));
        }

        private IEnumerator ResizeMultiChunkWorld(Int3 OldSize)
        {
            Debug.Log(name + " is Going from Multi Chunk to Multi Chunk.");
            //Debug.Log("Resizing world.");
            yield return UniversalCoroutine.CoroutineManager.StartCoroutine(OnResizeRoutine());   // load new world - creates chunks and adds to chunk positions
            Debug.Log(name + " is Finished Going from Multi Chunk to Multi Chunk.");
            if (OldSize != Int3.Zero())
            {
                foreach (Int3 MyKey in MyChunkData.Keys)
                {
                    Chunk MyChunk = MyChunkData[MyKey];
                    MyChunk.RefreshAll();// need to refresh as the centre position has changed
                }
            }
        }

        /// <summary>
        /// Resize the world!
        /// </summary>
        public IEnumerator OnResizeRoutine()
        {
            //IsResizingWorld = true;
            // For all chunks, if position outside of limit remove
            List<Int3> MyChunkKeys = new List<Int3>();

            foreach (Int3 KeyInKeys in MyChunkData.Keys)
            {
                MyChunkKeys.Add(KeyInKeys);
            }
            // Clear all chunks outside the limit
            Int3 MyKey = Int3.Zero();
            Chunk MyChunk;
            for (int i = 0; i < MyChunkKeys.Count; i++)
            {
                MyKey = MyChunkKeys[i];
                if (MyChunkData.ContainsKey(MyKey))
                {
                    MyChunk = MyChunkData[MyKey];
                    if (MyChunk != null)
                    {
                        if (IsChunksCentred)
                        {
                            if (MyChunk.Position.x < PositionOffset.x || MyChunk.Position.x >= PositionOffset.x + WorldSize.x ||
                                MyChunk.Position.y < PositionOffset.y || MyChunk.Position.y >= PositionOffset.y + WorldSize.y ||
                                MyChunk.Position.z < PositionOffset.z || MyChunk.Position.z >= PositionOffset.z + WorldSize.z)    // if chunk outside new bounds
                            {
                                DestroyChunk(MyChunk.Position);
                            }
                        }
                        else
                        {
                            if (MyChunk.Position.x < 0 || MyChunk.Position.x >= WorldSize.x ||
                                MyChunk.Position.y < 0 || MyChunk.Position.y >= WorldSize.y ||
                                MyChunk.Position.z < 0 || MyChunk.Position.z >= WorldSize.z)    // if chunk outside new bounds
                            {
                                DestroyChunk(MyChunk.Position);
                            }
                        }
                    }
                }
            }
            if (!IsSingleChunk())
            {
                Int3 ChunkPosition = Int3.Zero();
                Int3 ChunkSpawnPosition = Int3.Zero();
                for (ChunkPosition.x = 0; ChunkPosition.x < WorldSize.x; ChunkPosition.x++)
                {
                    for (ChunkPosition.z = 0; ChunkPosition.z < WorldSize.z; ChunkPosition.z++)
                    {
                        for (ChunkPosition.y = 0; ChunkPosition.y < WorldSize.y; ChunkPosition.y++)
                        {
                            if (IsChunksCentred)
                            {
                                ChunkSpawnPosition.Set(
                                    ChunkPosition.x + PositionOffset.x - WorldSize.x / 2,
                                    ChunkPosition.y,
                                    ChunkPosition.z + PositionOffset.z - WorldSize.z / 2);
                            }
                            else
                            {
                                ChunkSpawnPosition.Set(
                                    ChunkPosition.x + PositionOffset.x,
                                    ChunkPosition.y,
                                    ChunkPosition.z + PositionOffset.z);
                            }
                            CreateChunk(ChunkSpawnPosition);// new Int3(PositionOffset.x, 0, PositionOffset.z) + new Int3(i, j, k) - new Int3(WorldSize.x, 0, WorldSize.z) / 2f);
                           
                        }
                    }
                }
            }
            yield return null;
            //IsResizingWorld = false;
        }

        /// <summary>
        /// Converts from a single world to a multi world
        /// </summary>
        private IEnumerator ConvertToMultiChunkWorld()
        {
            Debug.Log(name + " is Going from Single Chunk to Multi Chunk.");
            // clear this as the chunk is technically being removed
            MyChunkData.Clear();
            // destroy the things
            MeshCollider MyMeshCollider = gameObject.GetComponent<MeshCollider>();
            MeshFilter MyMeshFilter = gameObject.GetComponent<MeshFilter>();
            MeshRenderer MyMeshRenderer = gameObject.GetComponent<MeshRenderer>();
            Chunk MyChunk = gameObject.GetComponent<Chunk>();
            RemoveVoxelAssets(MyChunk);
            // Spawns/Removes new chunks based on size of world
            yield return OnResizeRoutine();
            Chunk NewChunk = GetChunk(new Int3(0, 0, 0));
            if (NewChunk != null)
            {
                if (MyChunk != null)
                {
                    CopyChunk(MyChunk, NewChunk);
                }
                NewChunk.RefreshAll();
            }
            else
            {
                Debug.LogError("Error Creating new chunks: " + MyChunkData.Values.Count);
            }
            if (MyChunk != null)
            {
                MonoBehaviourExtension.Kill(MyChunk);
                MonoBehaviourExtension.Kill(MyMeshFilter);
                MonoBehaviourExtension.Kill(MyMeshRenderer);
                MonoBehaviourExtension.Kill(MyMeshCollider);
            }
        }

        /// <summary>
        /// Converts from a multi world to a single world
        /// </summary>
        private IEnumerator ConvertToSingleChunkWorld()
        {
            float TimeBegin = Time.realtimeSinceStartup;
            Chunk OldChunk = GetChunk(Int3.Zero());  // at 0,0,0
            MyChunkData.Clear();
            Chunk NewChunk = CreateChunk(Int3.Zero(), gameObject);
            if (OldChunk != null)
            {
                NewChunk = CopyChunk(OldChunk, NewChunk);
                yield return ClearChunks();  // destroy all child objects of the world 
            }
            MyChunkData.Clear();
            MyChunkData.Add(new Int3(0, 0, 0), NewChunk);

            if (OldChunk != null)
            {
                //Debug.Log("Adding to " + NewChunk.GetMeshFilter().mesh.name + " verts: " + OldCentreOffset.ToString());
                Vector3[] vertices = NewChunk.GetMeshFilter().mesh.vertices;
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] += OldCentreOffset;
                }
               // Debug.Log("Minusing to verts: " + CentreOffset.ToString());
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] -= CentreOffset;
                }
                NewChunk.GetMeshFilter().mesh.vertices = vertices;
                NewChunk.GetMeshFilter().mesh.RecalculateBounds();
                NewChunk.GetMeshFilter().mesh.UploadMeshData(false);
            }
            else
            {
                //Debug.LogError("Old chunk is null!");
            }
        }
        #endregion

        #region Roaming
        // new algorithim:
        // chuck all chunks into pool
        // repurpose them later after meshing stops
        // if run out, repurpose them now
        // grab a chunk from a pool when move to new positions in offset
        // make sure it loads on startup
        // save chunk changes, mark chunks as dirty, if removing a chunk and is dirty, save it to disk
        private Chunk MyChunk;
        private int RadiusX;
        private int RadiusZ;
        private float TimeBegin;
        //private Int3 PositionChange;
        private Int3 NewPositionInt = new Int3();
       // private List<Int3> PositionOffsets = new List<Int3>();
        private Int3 FillInChunksPosition = Int3.Zero();
        private List<Int3> ChunkKeys;
        [Header("Debug")]
        [SerializeField]
        private List<Chunk> ChunkPool = new List<Chunk>();
        private bool isSettingPositionOffset;
        private Int3 NewPositionOffset = Int3.Zero();
        private bool HasNewPositionOffset;

        /// <summary>
        /// Called from VoxelRoam when entering a new chunk!
        /// </summary>
        public IEnumerator SetPositionOffset(Int3 ChunkPosition)
        {
            if (!isSettingPositionOffset)
            {
                ChunkPosition.y = 0;
                //Debug.LogError("Setting world " + name + " offset " + ChunkPosition.ToString());
                //int queNumber = IsRepositioningChunks;
                //IsRepositioningChunks++;
                if (ChunkPosition != PositionOffset)
                {
                    isSettingPositionOffset = true;
                    //PositionChange = ChunkPosition - PositionOffset;
                    PositionOffset.Set(ChunkPosition);
                    PositionOffset.y = 0; // for now this doesnt matter
                    //Debug.LogError("New PositionOffset: " + PositionOffset.ToString());
                    RecycleChunks();
                    yield return FillInChunks();
                    isSettingPositionOffset = false;
                    if (HasNewPositionOffset)
                    {
                        HasNewPositionOffset = false;
                        //Debug.LogError("Doubling PositionOffset: " + NewPositionOffset.ToString());
                        yield return SetPositionOffset(NewPositionOffset);
                    }
                    else
                    {
                        //Debug.LogError("Finished offseting world.");
                    }
                }
                else
                {
                    //Debug.LogError("The same PositionOffset: " + ChunkPosition.ToString());
                }
            }
            else
            {
                if (PositionOffset != ChunkPosition)
                {
                    //Debug.LogError("Setting PositionOffset while already loading offset: " + ChunkPosition.ToString());
                    NewPositionOffset.Set(ChunkPosition);
                    HasNewPositionOffset = true;
                }
            }
        }

        private int CharactersInChunkCount;
        private void AddChunkToPool(Chunk MyChunk)
        {
            if (MyChunk != null && MyChunkData.ContainsKey(MyChunk.Position))
            {
                // if any characters in chunk, kill them instantly
                CharactersInChunkCount = Characters.CharacterManager.Get().GetSize();
                //Debug.LogError("Checking for characters: " + CharactersInChunkCount);
                for (int i = 0; i < CharactersInChunkCount; i++)
                {
                    Characters.Character MyCharacter = Characters.CharacterManager.Get().GetSpawn(i);
                    if (MyCharacter.GetChunkInsideOf() == MyChunk)
                    {
                        MyCharacter.OnDeath();
                    }
                    /*else
                    {
                        Debug.LogError("Character not inside of chunk: " + MyCharacter.name);
                    }*/
                }
                MyChunk.OnChunkReset();
                if (ChunkPool.Contains(MyChunk) == false)
                {
                    ChunkPool.Add(MyChunk);
                }
                MyChunkData.Remove(MyChunk.Position);
            }
        }

        private List<Int3> GetChunkDataKeys()
        {
            List<Int3> ChunkKeys = new List<Int3>();
            foreach (Int3 MyKey in MyChunkData.Keys)
            {
                ChunkKeys.Add(MyKey);
            }
            return ChunkKeys;
        }

        public Vector3 GetHalfSize()
        {
            return new Vector3(WorldSize.x - 1, WorldSize.y, WorldSize.z - 1) / 2f;
        }
        private void RecycleChunks()
        {
            TimeBegin = Time.realtimeSinceStartup;
            RadiusX = Mathf.RoundToInt(GetHalfSize().x);// (WorldSize.x - 1) / 2f);
            RadiusZ = Mathf.RoundToInt(GetHalfSize().z);// (WorldSize.z - 1) / 2f);

            // if chunk outside of bounds, add to reposition list

            // brute force
            ChunkKeys = GetChunkDataKeys();
            for (int i = 0; i < ChunkKeys.Count; i++)
            {
                if (MyChunkData.ContainsKey(ChunkKeys[i]))
                {
                    MyChunk = MyChunkData[ChunkKeys[i]];
                    if (MyChunk.Position.x < PositionOffset.x - RadiusX
                        || MyChunk.Position.x > PositionOffset.x + RadiusX
                        || MyChunk.Position.z < PositionOffset.z - RadiusZ
                        || MyChunk.Position.z > PositionOffset.z + RadiusZ)
                    {
                        AddChunkToPool(MyChunk);//, MyChunk.Position.GetVector() + new Vector3(WorldSize.x, 0, 0));
                    }
                }
            }
        }

        private IEnumerator FillInChunks()
        {
            //Debug.Log("Moving position to: " + ChunkPosition.GetVector().ToString() + " with change of: " + PositionChange.GetVector().ToString());

            if (VoxelDebugger.IsDebug)
            {
                VoxelDebugger.Get().ClearMoveChunks();
            }
            ChunkKeys = GetChunkDataKeys();
            PositionOffset.y = 0;
            for (FillInChunksPosition.x = PositionOffset.x - RadiusX; FillInChunksPosition.x <= PositionOffset.x + RadiusX; FillInChunksPosition.x++)
            {
                for (FillInChunksPosition.y = 0; FillInChunksPosition.y < PositionOffset.y + WorldSize.y; FillInChunksPosition.y++)
                {
                    for (FillInChunksPosition.z = PositionOffset.z - RadiusX; FillInChunksPosition.z <= PositionOffset.z + RadiusX; FillInChunksPosition.z++)
                    {
                        if (MyChunkData.ContainsKey(FillInChunksPosition) == false)
                        {
                            if (ChunkPool.Count == 0)
                            {
                                break;
                            }
                            else
                            {
                                // grab a chunk from pool, remove it
                                Chunk ReusedChunk = ChunkPool[0];
                                ChunkPool.RemoveAt(0);
                                // re apply chunk
                                yield return UniversalCoroutine.CoroutineManager.StartCoroutine(RepositionChunk(ReusedChunk, FillInChunksPosition));
                                yield return UniversalCoroutine.CoroutineManager.StartCoroutine(MyVoxelTerrain.CreateTerrain(ReusedChunk));
                                ReusedChunk.ForceRefreshSurroundingChunks();
                                ReusedChunk.OnMassUpdate();

                            }
                        }
                    }
                }
            }

            // Does all that stuff without pause
            //Debug.Log(name + ":Reposition Time[" + Mathf.RoundToInt((Time.realtimeSinceStartup - TimeBegin) * 1000) + "] - [" + PositionChange.ToString() + "] using: " + MyChunkData.Keys.Count + " Chunks!");
            //yield return UpdateMovedChunks();
        }

        /// <summary>
        /// When loading, reuse an old chunk in a new position!
        /// This is problem! for old chunks just hide until rebuilding!
        /// </summary>
        private IEnumerator RepositionChunk(Chunk MyChunk, Int3 NewPosition)   // Vector3 PositionA, 
        {
            if (MyChunk != null && MyChunkData.ContainsKey(NewPosition) == false && MyChunk.Position != NewPosition)
            {
                MyChunk.Position.Set(NewPosition);
                MyChunk.name = "Chunk:" + MyChunk.Position.x + ":" + MyChunk.Position.y + ":" + MyChunk.Position.z;
                MyChunk.transform.localPosition = GetChunkLocalPosition(MyChunk.Position);
                MyLookupTable.OnRemoveChunk(MyChunk);   // remove all the lookup data of this chunk
                MyLookupTable.OnAddChunk(MyChunk);      // add all the air data
                yield return UniversalCoroutine.CoroutineManager.StartCoroutine(MyChunk.GetVoxelData().SetAllVoxelsToAir()); // set all to air
                MyChunkData.Add(MyChunk.Position, MyChunk);
            }
            else
            {
                if (MyChunk == null)
                {
                    Debug.LogError("Chunk is null at: " + NewPosition.ToString());
                }
                if (MyChunkData.ContainsKey(NewPosition))
                {
                    Debug.LogError("Cannot add to dictionary as position already exists: " + NewPosition.ToString());
                }
                if (MyChunk.Position == NewPosition)
                {
                    Debug.LogError("Positions the same for chunks: " + NewPosition.ToString());
                }
            }
        }

        /// <summary>
        /// When chunk has to be loaded with generation
        /// </summary>
        public IEnumerator OnNewChunkRoam()
        {
            yield return null;
            if (TestingCreateTerrain)
            {
                //yield return UniversalCoroutine.CoroutineManager.StartCoroutine(MyVoxelTerrain.CreateTerrain(RoamingChunk));
            }
            else if (TestingCreatePlane)
            {
                //MyVoxelTerrain.CreatePlane(RoamingChunk, 8, 2);
            }
        }
        #endregion
    }
}


/// <summary>
/// Reposition over time!
/// </summary>
/*IEnumerator UpdateMovedChunks()
{
    // Set All the Voxel Data
    for (ChunkIndex = 0; ChunkIndex < ChunkRepositionList.Count; ChunkIndex++)
    {
        if (ChunkRepositionList[ChunkIndex].Position.y == 1)
        {
            RoamingChunk = ChunkRepositionList[ChunkIndex];
            yield return OnNewChunkRoam();
        }
    }
    // do these next, so it does chunk updates properly
    for (ChunkIndex = 0; ChunkIndex < ChunkRepositionList.Count; ChunkIndex++)
    {
        if (ChunkRepositionList[ChunkIndex].Position.y == 0)
        {
            RoamingChunk = ChunkRepositionList[ChunkIndex];
            yield return OnNewChunkRoam();
        }
    }
    // Now Update them!
    for (ChunkIndex = 0; ChunkIndex < ChunkRepositionList.Count; ChunkIndex++)
    {
        ChunkRepositionList[ChunkIndex].ForceRefreshSurroundingChunks();
        ChunkRepositionList[ChunkIndex].OnMassUpdate();
    }
    yield return null;
}*/
//for (ChunkIndex = ChunkRepositionList.Count-1; ChunkIndex >= 0; ChunkIndex--)
/*while (ChunkRepositionList.Count > 0)
{
    if (VoxelDebugger.IsDebug)
    {
        VoxelDebugger.Get().AddMoveChunk("[" + (int)Time.realtimeSinceStartup + "] Chunk- [" + ChunkRepositionList[i].Position.GetVector().ToString() + " to " + MoveToList[i].ToString());
    }
    ChunkIndex = ChunkRepositionList.Count - 1;
    MoveChunk = ChunkRepositionList[ChunkIndex];
    MovePosition = MoveToList[ChunkIndex];
    ChunkRepositionList.RemoveAt(ChunkIndex);
    MoveToList.RemoveAt(ChunkIndex);
    yield return RepositionChunk(MoveChunk, MovePosition);
    yield return MyVoxelTerrain.CreateTerrain(MoveChunk);
    MoveChunk.ForceRefreshSurroundingChunks();
    MoveChunk.OnMassUpdate();
}*/

/*if (PositionChange == new Int3(1, 0, 0))
{
    ChunkKeys = GetChunkDataKeys();
    // brute force
    for (int i = 0; i < ChunkKeys.Count; i++)
    {
        if (MyChunkData.ContainsKey(ChunkKeys[i]))
        {
            MyChunk = MyChunkData[ChunkKeys[i]];
            if (MyChunk.Position.x < PositionOffset.x - RadiusX)
            {
                if (ChunkRepositionList.Contains(MyChunk) == false)
                {
                    AddChunkToRepositionList(MyChunk, MyChunk.Position.GetVector() + new Vector3(WorldSize.x, 0, 0));
                }
            }
        }
        //yield return null;
    }
}
else if (PositionChange == new Int3(-1, 0, 0))
{
    ChunkKeys = GetChunkDataKeys();
    // brute force
    for (int i = 0; i < ChunkKeys.Count; i++)
    {
        if (MyChunkData.ContainsKey(ChunkKeys[i]))
        {
            MyChunk = MyChunkData[ChunkKeys[i]];
            if (MyChunk.Position.x > PositionOffset.x + RadiusX)
            {
                if (ChunkRepositionList.Contains(MyChunk) == false)
                {
                    AddChunkToRepositionList(MyChunk, MyChunk.Position.GetVector() + new Vector3(-WorldSize.x, 0, 0));
                }
            }
        }
        //yield return null;
    }
}
//Debug.Log("OuterLimitsZ: " + (ChunkPosition.z - RadiusZ) + ": to :" + (ChunkPosition.z + RadiusZ));
else if (PositionChange == new Int3(0, 0, 1))
{
    ChunkKeys = GetChunkDataKeys();
    //Debug.Log("Less Then OuterLimit Z: " + (ChunkPosition.z - RadiusZ));
    for (int i = 0; i < ChunkKeys.Count; i++)
    {
        if (MyChunkData.ContainsKey(ChunkKeys[i]))
        {
            MyChunk = MyChunkData[ChunkKeys[i]];
            if (MyChunk.Position.z < PositionOffset.z - RadiusZ)
            {
                if (ChunkRepositionList.Contains(MyChunk) == false)
                {
                    AddChunkToRepositionList(MyChunk, MyChunk.Position.GetVector() + new Vector3(0, 0, WorldSize.z));
                }
            }
        }
        //yield return null;
    }
}
else if (PositionChange == new Int3(0, 0, -1))
{
    ChunkKeys = GetChunkDataKeys();
    //Debug.Log("Greater then OuterLimit Z: " + (ChunkPosition.z + RadiusZ));
    //Debug.Log("Moved in X direction.");
    for (int i = 0; i < ChunkKeys.Count; i++)
    {
        if (MyChunkData.ContainsKey(ChunkKeys[i]))
        {
            MyChunk = MyChunkData[ChunkKeys[i]];
            if (MyChunk.Position.z > PositionOffset.z + RadiusZ)
            {
                if (ChunkRepositionList.Contains(MyChunk) == false)
                {
                    AddChunkToRepositionList(
                        MyChunk,
                        MyChunk.Position.GetVector() + new Vector3(0, 0, -WorldSize.z));
                }
            }
        }
        //yield return null;
    }
}*/
//MyChunk.GetMeshFilter().mesh.Clear();
//MoveToList.Add(NewPosition);
//PositionOffsets.Add(PositionChange);
/*if (ChunkRepositionList.Count > 0 && IsMovingChunks == false)
{
    yield return MoveChunks();
}*/
