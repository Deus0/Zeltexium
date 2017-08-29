using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Util;

namespace Zeltex.Voxels
{
    /// <summary>
    /// The main class for voxel models. It is a collection of chunked voxels.
    /// </summary>
    [ExecuteInEditMode]
	public partial class World : MonoBehaviour
    {
        #region Variables
        public static int TextureResolution = 16;
        public static bool IsMipMaps = false;
        public bool IsDebug = false;

        [Header("Actions")]
        public EditorAction ApplyActionCube = new EditorAction();
        public EditorAction ActionGatherChunks = new EditorAction();
        public string VoxelActionName = "Color";
        public List<GameObject> ActionGrayBoxes;
        public EditorAction ActionLoadModel = new EditorAction();
        public string LoadModelName = "Head";

        [Header("Needed References")]
        public WorldUpdater MyUpdater;
        public VoxelManager MyDataBase;
        public GameObject MyVoxelDestroyParticles;

        [Header("Data")]
        public List<Material> MyMaterials = new List<Material>();
        public VoxelLookupTable MyLookupTable = new VoxelLookupTable(); // lookup table used for voxel indexes
        [SerializeField]    //, HideInInspector
        public ChunkDictionary MyChunkData = new ChunkDictionary();

        [Header("Items")]
        [SerializeField]
        public bool IsGameWorld = false;
        [SerializeField]
        public bool IsDropItems = false;
        [SerializeField]
        public bool IsDropParticles = false;
        [SerializeField]
        private bool IsColliders = true;
        [SerializeField]
        public bool IsConvex = false;

        [Header("Mesh Building")]
        [SerializeField]
        public bool IsLighting;             // IsLighting in general, otherwise everything will be the same brightness
        //[SerializeField] public bool IsSmoothedLighting;     // smooths the lighting based on distance
       // [HideInInspector]
        public Vector3 CentreOffset;
        [HideInInspector]
        public Vector3 MyLossyScale;   // used for centre offsettings
        public bool IsReBuildAllMeshes;
        // Lighting
        [HideInInspector]
        public float PropogationDecreaseRate = 0.78f;
        [HideInInspector]
        public int SunBrightness = 255;
        [HideInInspector]
        public int DefaultBrightness = 0;

        [Header("Roaming")]
        public VoxelTerrain MyVoxelTerrain;
        public bool IsHeight = false;
        #endregion

        #region Mono

        public void Enable()
        {
            SetState(true);
        }

        public void Disable()
        {
            SetState(false);
        }


        void Awake()
        {
            MyLossyScale = transform.lossyScale;
        }

        public void GatherChunks()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Chunk ChildChunk = transform.GetChild(i).gameObject.GetComponent<Chunk>();
                if (ChildChunk)
                {
                    if (MyChunkData.ContainsKey(ChildChunk.Position) == false)
                    {
                        MyChunkData.Add(ChildChunk.Position, ChildChunk);
                    }
                }
            }
        }
        private void Update()
        {
            if (ApplyActionCube.IsTriggered())
            {
                GatherChunks();
                GameObject ActionCube;
                for (int i = 0; i < ActionGrayBoxes.Count; i++)
                {
                    ActionCube = ActionGrayBoxes[i];
                    if (ActionCube != null)
                    {
                        Debug.LogError("Apply voxel [" + VoxelActionName + "] to: " + RealToBlockPosition(ActionCube.transform.position).ToString());
                        BoxCollider MyCollider = ActionCube.GetComponent<BoxCollider>();
                        if (MyCollider)
                        {
                            UpdateBlockTypeMassArea(VoxelActionName, RealToBlockPosition(ActionCube.transform.position),
                                new Vector3(
                                    (MyCollider.size.x * MyCollider.transform.lossyScale.x - 1f) / 2f,
                                    (MyCollider.size.y * MyCollider.transform.lossyScale.y - 1f) / 2f,
                                    (MyCollider.size.z * MyCollider.transform.lossyScale.z - 1f) / 2f)
                                , Color.white);
                        }
                        else
                        {
                            UpdateBlockTypeMassArea(VoxelActionName, RealToBlockPosition(ActionCube.transform.position), 0, Color.white);
                        }
                    }
                }
            }
            if (ActionGatherChunks.IsTriggered())
            {
                GatherChunks();
            }
            if (ActionLoadModel.IsTriggered())
            {
                WorldModel MyElement = DataManager.Get().GetElement(DataFolderNames.VoxelModels, LoadModelName) as WorldModel;
                if (MyElement != null)
                {
                    Debug.LogError("Loading:\n" + MyElement.VoxelData);
                    RunScript(FileUtil.ConvertToList(MyElement.VoxelData));
                }
                else
                {
                    Debug.LogError("Could not load " + LoadModelName);
                }

            }
        }
        /// <summary>
        /// Debug stuff!
        /// </summary>
        /*void OnGUI()
        {
            if (IsDebug)
            {
                if (MyChunkData == null)
                {
                    GUILayout.Label(name + " has null chunk data.");
                }
                else
                {
                    GUILayout.Label("Dictionary List: " + MyChunkData.Values.Count + ":" + MyChunkData.Keys.Count);
                    foreach (Int3 MyKey in MyChunkData.Keys)
                    {
                        GUILayout.Label("   Key: " + MyKey.GetVector().ToString());
                    }
                }
                List<string> MyData = GetStatistics();
                for (int i = 0; i < MyData.Count; i++)
                {
                    GUILayout.Label(MyData[i]);
                }
            }
        }*/
        // not bug
        #endregion

        #region ChunkManagement

        /// <summary>
        /// Creates a chunk at a chunk position.
        /// </summary>
        public Chunk CreateChunk(Int3 ChunkPosition, GameObject ChunkObject = null)
        {
            if (MyChunkData.ContainsKey(ChunkPosition) == false)
            {
                if (ChunkObject == null)
                {
                    ChunkObject = new GameObject();
                    ChunkObject.layer = gameObject.layer;
                    //LayerManager.Get().SetLayerWorld(ChunkObject);
                    ChunkObject.transform.SetParent(transform, false);
                    ChunkObject.transform.localPosition = GetChunkLocalPosition(ChunkPosition);
                    ChunkObject.transform.localRotation = Quaternion.identity;
                    ChunkObject.transform.localScale = new Vector3(1, 1, 1);
                    ChunkObject.name = "Chunk:" + ChunkPosition.x + ":" + ChunkPosition.y + ":" + ChunkPosition.z;
                }
                Chunk NewChunk = ChunkObject.AddComponent<Chunk>();
                NewChunk.Position.Set(ChunkPosition);
                NewChunk.SetWorld(this);
                MeshFilter MyMeshFilter = NewChunk.GetMeshFilter();
                MeshRenderer MyMeshRenderer = NewChunk.GetMeshRenderer();
                //Debug.Log("Creating new chunk: " + NewObject.name + " at time " + Time.time);
                MeshCollider MyMeshCollider = NewChunk.GetMeshCollider();
                NewChunk.SetMeshVisibility(IsMeshVisible);
                if (MyMeshCollider && IsColliders)
                {
                    MyMeshCollider.convex = IsConvex;
                }
                MyChunkData.Add(NewChunk.Position, NewChunk);
                MyLookupTable.OnAddChunk(NewChunk); // adds air blocks to the world count
                return NewChunk;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Removes voxel assets from a chunk, used when destroying or repooling a chunk
        /// </summary>
        private void RemoveVoxelAssets(Chunk MyChunk)
        {
            // Remove any voxel assets  
            if (MyChunk)
            {
                for (int i = 0; i < MyChunk.transform.childCount; i++)
                {
                    Transform ChildTransform = MyChunk.transform.GetChild(i);
                    if (ChildTransform.name.Contains("Line") == false && ChildTransform.GetComponent<Chunk>() == null)
                    {
                        MonoBehaviourExtension.Kill(ChildTransform.gameObject);  // destroy any torches/voxel assets
                    }
                }
            }
        }

        /// <summary>
        /// Destroys a chunk at a chunk position. Also Removes the chunk data from the list
        /// </summary>
        public bool DestroyChunk(Int3 MyChunkPosition)
        {
            Chunk MyChunk = null;
            if (MyChunkData.TryGetValue(MyChunkPosition, out MyChunk))
            {
                if (MyChunk.gameObject != gameObject)
                {
                    // Destroy the mesh too before destroying the chunk!
                    if (MyChunk.GetComponent<MeshFilter>().mesh)
                    {
                        MonoBehaviourExtension.Kill(MyChunk.GetComponent<MeshFilter>().mesh);
                    }
                    // atm it is saved when it is removed
                    MyLookupTable.OnRemoveChunk(MyChunk); // Reduces the count by the amount in this chunk
                                                          // SaveChunk 
                    MonoBehaviourExtension.Kill(MyChunk.gameObject);
                    //Debug.Log("Deleting chunk: " + MyChunk.name + ":" + MyChunkData.Values.Count);
                    MyChunkData.Remove(MyChunkPosition);
                    //Debug.Log("after Deleting chunk: :" + MyChunkData.Values.Count);
                    return true;
                }
                else
                {
                    Debug.LogError("Trying to destroy world by destroying chunk in: " + name);
                    return false;
                }
            }
            else
            {
                Debug.Log("Could not find chunk to destroy: " + MyChunkPosition.GetVector().ToString() + ": size:" + MyChunkData.Values.Count);
            }
            return false;
        }

        /// <summary>
        /// Copy ChunkA to ChunkB
        /// </summary>
        public Chunk CopyChunk(Chunk MyChunkA, Chunk MyChunkB)
        {
            if (MyChunkA != null && MyChunkB != null)
            {
                MeshCollider MyMeshCollider = MyChunkA.GetComponent<MeshCollider>();
                MeshFilter MyMeshFilter = MyChunkA.GetComponent<MeshFilter>();
                MeshRenderer MyMeshRenderer = MyChunkA.GetComponent<MeshRenderer>();
                MeshFilter NewMeshFilter = MyChunkB.GetComponent<MeshFilter>();
                MeshCollider NewMeshCollider = MyChunkB.GetComponent<MeshCollider>();
                MeshRenderer NewMeshRenderer = MyChunkB.GetComponent<MeshRenderer>();
                MyChunkB = FileUtil.GetCopyOf(MyChunkB, MyChunkA);
                NewMeshFilter = FileUtil.GetCopyOf(NewMeshFilter, MyMeshFilter);
                NewMeshCollider = FileUtil.GetCopyOf(NewMeshCollider, MyMeshCollider);
                NewMeshRenderer = FileUtil.GetCopyOf(NewMeshRenderer, MyMeshRenderer);
                NewMeshRenderer.material = MyMeshRenderer.material;
                NewMeshFilter.sharedMesh = (Mesh)Instantiate(MyMeshFilter.sharedMesh);
                NewMeshCollider.sharedMesh = NewMeshFilter.sharedMesh;
                MyChunkB.RefreshComponents();
                return MyChunkB;
            }
            return null;
        }

        /// <summary>
        /// Removes all the chunks! Should remove chunk data too
        /// </summary>
        public IEnumerator ClearChunks()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Chunk MyChunk = transform.GetChild(i).gameObject.GetComponent<Chunk>();
                if (MyChunk)
                {
                    if (DestroyChunk(MyChunk.Position))
                    {

                    }
                    else
                    {
                        MonoBehaviourExtension.Kill(transform.GetChild(i).gameObject);
                    }
                }
            }
            yield return null;
        }

        /// <summary>
        /// Clears the voxels!!
        /// Move to Zeltex.Game classes.
        /// Clears Items, Characters, zones etc
        /// </summary>
		public void Clear()
        {
            float BeginTime = Time.realtimeSinceStartup;
            //MyUpdater.Reset();      // it should only clear chunks from this world...
            foreach (Int3 MyKey in MyChunkData.Keys)
            {
                Chunk MyChunk = MyChunkData[MyKey];
                MyChunk.Clear();
            }
            Debug.Log("World " + name + " took [" + (Time.realtimeSinceStartup - BeginTime) + "] to Clear!");
        }
        #endregion

        #region Getters

        public Int3 GetWorldSizeChunks()
        {
            return new Int3(WorldSize);
        }

        /// <summary>
        /// Gets the worlds real size in unity units
        /// </summary>
        public Vector3 GetWorldSize()
        {
            return new Vector3(
                WorldSize.x * VoxelScale.x * Chunk.ChunkSize * MyLossyScale.x,
                WorldSize.y * VoxelScale.y * Chunk.ChunkSize * MyLossyScale.y,
                WorldSize.z * VoxelScale.z * Chunk.ChunkSize * MyLossyScale.z);
        }

        /// <summary>
        /// Gets the entire block size of the world
        /// </summary>
        public Vector3 GetWorldBlockSize()
        {
            return new Vector3(
                WorldSize.x * Chunk.ChunkSize,
                WorldSize.y * Chunk.ChunkSize,
                WorldSize.z * Chunk.ChunkSize);
        }

        /// <summary>
        /// Gets the local position of a chunk, used to position chunks in the world
        /// </summary>
        private Vector3 GetChunkLocalPosition(Int3 MyChunkPosiiton)
        {
            return new Vector3(
                MyChunkPosiiton.x * Chunk.ChunkSize * VoxelScale.x,
                MyChunkPosiiton.y * Chunk.ChunkSize * VoxelScale.y,
                MyChunkPosiiton.z * Chunk.ChunkSize * VoxelScale.z
                );
        }

        /// <summary>
        /// returns true if the world is still loading
        /// </summary>
        public bool IsWorldLoading()
        {
            foreach (Int3 MyKey in MyChunkData.Keys)
            {
                Chunk MyChunk = MyChunkData[MyKey];
                if (MyChunk.IsUpdatingChunk())
                {
                    return true;
                }
            }
            //Debug.LogError("None of chunks is loading");
            return false;
        }

        /// <summary>
        /// Is the world a single chunk, otherwise its a multi chunk
        /// </summary>
        public bool IsSingleChunk()
        {
            return (WorldSize.x == 1 && WorldSize.y == 1 && WorldSize.z == 1);
        }

        /// <summary>
        /// Is the world a single chunk, otherwise its a multi chunk
        /// </summary>
        private bool IsSingleChunk(Int3 Size)
        {
            return (Size.x == 1 && Size.y == 1 && Size.z == 1);
        }

        /// <summary>
        /// For rendering debugging
        /// </summary>
        private List<string> GetPolygonStatistics()
        {
            List<string> MyData = new List<string>();
            int VertCount = 0;
            int TriangleCount = 0;
            foreach (Int3 MyKey in MyChunkData.Keys)
            {
                //MyChunkData[MyKey]
                Mesh ChunkMesh = MyChunkData[MyKey].GetComponent<MeshFilter>().mesh;
                VertCount += ChunkMesh.vertexCount;
                TriangleCount += ChunkMesh.triangles.Length;
            }
            MyData.Add("Total Verticies [" + VertCount + "]");
            MyData.Add("Total Triangle Points [" + TriangleCount + "]");
            return MyData;
        }

        /// <summary>
        /// Gets the statistics of the world
        /// </summary>
        public List<string> GetStatistics()
        {
            List<string> MyData = new List<string>();
            MyData.Add("Voxel Model [" + name + "]");
            MyData.Add("Size [" + WorldSize.ToString() + "]");
            // Polygon Data
            MyData.Add("Polygonal Data:");
            MyData.AddRange(GetPolygonStatistics());
            // Voxel Data
            MyData.Add("Voxels Total [" + GetMaxVoxelCount() + "]");
            MyData.AddRange(MyLookupTable.GetStatistics());
            MyData.Add("Chunk Lookup tables:");
            foreach (Int3 MyKey in MyChunkData.Keys)
            {
                MyData.AddRange(MyChunkData[MyKey].MyLookupTable.GetStatistics());
                break;
            }
            return MyData;
        }
        #endregion

        #region Setters

        /// <summary>
        /// Sets the lighting
        /// </summary>
        public void SetLighting(bool NewLighting)
        {
            IsLighting = NewLighting;
        }

        /// <summary>
        /// Hides/Unhides the chunks
        /// </summary>
        private void SetState(bool NewState)
        {
            if (enabled != NewState)
            {
                enabled = false;
                SetChunksVisibility(NewState);
                if (NewState == false)
                {
                    SetCollidersRaw(NewState);
                }
                else
                {
                    SetCollidersRaw(NewState && IsColliders);
                }
                List<Characters.Character> MyCharacters = Characters.CharacterManager.Get().GetSpawned();
                for (int i = 0; i < MyCharacters.Count; i++)
                {
                    if (MyCharacters[i] && MyCharacters[i].GetWorldInsideOf() == this)
                    {
                        MyCharacters[i].SetMovement(NewState);
                    }
                }
            }
        }
        #endregion
    }
}

/// <summary>
/// Load world chunks in a routine.
/// Assuming world is already cleared
/// </summary>
/*public IEnumerator RefreshRoutine(float TimeDelay)
{
    if (IsSingleChunk() == false)   // single chunks don't get refreshed
    {
        // load the chunks
        for (int i = 0; i < WorldSize.x; i++)
        {
            for (int j = 0; j < WorldSize.y; j++)
            {
                for (int k = 0; k < WorldSize.z; k++)
                {
                    CreateChunk(new Int3(i, j, k));
                    yield return new WaitForSeconds(TimeDelay);
                }
            }
        }
    }
}*/
