using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Util;
using Zeltex.Guis.Maker;
using Zeltex.Items;
using Zeltex.AI;
using Zeltex.Generators;
using Zeltex.Physics;

namespace Zeltex.Voxels
{
    [System.Serializable()]
    public class ChunkDictionary : SerializableDictionaryBase<Int3, Chunk>
    {

    }

    [System.Serializable]
    public class WorldEditorActionBlock
    {
        public EditorAction ApplyActionCube = new EditorAction();
        public EditorAction ImportVox = new EditorAction();

        public EditorAction WorldActionExpandForward = new EditorAction();
        public EditorAction WorldActionExpandBack = new EditorAction();
        public EditorAction WorldActionExpandLeft = new EditorAction();
        public EditorAction WorldActionExpandRight = new EditorAction();
        public EditorAction WorldActionExpandUp = new EditorAction();
        public EditorAction WorldActionExpandDown = new EditorAction();


        public void Update(World MyWorld)
        {

        }
    }

    /// <summary>
    /// The main class for voxel models. It is a collection of chunked voxels.
    /// </summary>
    [ExecuteInEditMode]
	public class World : MonoBehaviour
    {
        #region Variables
        [HideInInspector]
        public float YieldRate = (256f / 1000f);
        private float LastYield;
        public static int TextureResolution = 16;
        public static bool IsMipMaps = false;
        public bool IsDebug = false;
        public bool IsMeshVisible = true;
        [Header("Actions")]
        public WorldEditorActionBlock Actions = new WorldEditorActionBlock();
        public EditorAction ActionGatherChunks = new EditorAction();
        public string VoxelActionName = "Color";
        public List<GameObject> ActionGrayBoxes;
        public EditorAction ActionLoadModel = new EditorAction();
        public string LoadModelName = "Head";
        public EditorAction ActionPushModel = new EditorAction();
        public string PushModelName = "Body";
        [Header("Needed References")]
        public GameObject MyVoxelDestroyParticles;
        [Header("Data")]
        //public List<Material> MyMaterials = new List<Material>();
        public VoxelLookupTable MyLookupTable = new VoxelLookupTable(); // lookup table used for voxel indexes
        [SerializeField]    //, HideInInspector
        public ChunkDictionary MyChunkData = new ChunkDictionary();
        public TileMap MyTilemap = new TileMap(8, 8, 16, 16);
        private List<Material> MyMaterials = new List<Material>();


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
        [HideInInspector]
        public bool IsAddOutline = true;
        private UnityEvent OnLoadWorld = new UnityEvent();

        #region OtherVariables
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
        public static bool TestingIsRefreshOldChunks = true;   // wipes chunks when repositioned
        public static bool TestingRefreshSides = true;         // refreshes chunk sides when reposiioned
        public static bool TestingCreateTerrain = true;        // creates Terrain when repositioned
        public static bool TestingCreatePlane = false;          // creates planes when repositioned
        public delegate void OnSizeFinishedEvent(World ResizedWorld);
        [Tooltip("Models won't be centred, but worlds will.")]
        public bool IsChunksCentred = false;
        Vector3 WorldTotalSize = Vector3.zero;
        Vector3 OldCentreOffset = Vector3.zero;
        #endregion

        #region RoamingVariables
        private Chunk MyChunk;
        private int RadiusX;
        private int RadiusZ;
        //private float TimeBegin;
        //private Int3 PositionChange;
        //private Int3 NewPositionInt = new Int3();
        // private List<Int3> PositionOffsets = new List<Int3>();
        private Int3 FillInChunksPosition = Int3.Zero();
        private List<Int3> ChunkKeys;
        [Header("Debug")]
        [SerializeField]
        private List<Chunk> ChunkPool = new List<Chunk>();
        private bool isSettingPositionOffset;
        private Int3 NewPositionOffset = Int3.Zero();
        private bool HasNewPositionOffset;
        #endregion

        #region UpdateVariables
        private int CharactersInChunkCount;
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
        #endregion

        #region Mono
        public IEnumerator YieldTimer() 
        {
            if (Time.realtimeSinceStartup - LastYield >= YieldRate)
            {
                LastYield = Time.realtimeSinceStartup;
                yield return null;
            }
        }

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

        public List<Material> GetMaterials()
        {
            if (MyMaterials.Count == 0)
            {
                MyMaterials = VoxelManager.Get().MyMaterials;
            }
            return MyMaterials;
        }

        private void Update()
        {
            Actions.Update(this);
            if (Actions.ApplyActionCube.IsTriggered())
            {
                GatherChunks();
                GameObject ActionCube;
                for (int i = 0; i < ActionGrayBoxes.Count; i++)
                {
                    ActionCube = ActionGrayBoxes[i];
                    ApplyActionCube(ActionCube, VoxelActionName);
                }
            }
            if (ActionGatherChunks.IsTriggered())
            {
                GatherChunks();
            }
            if (ActionLoadModel.IsTriggered())
            {
                VoxelModel MyElement = DataManager.Get().GetElement(DataFolderNames.VoxelModels, LoadModelName) as VoxelModel;
                if (MyElement != null)
                {
                    //Debug.LogError("Loading:\n" + MyElement.VoxelData);
                    RunScript(FileUtil.ConvertToList(MyElement.VoxelData));
                }
                else
                {
                    Debug.LogError("Could not load " + LoadModelName);
                }

            }
            if (Actions.ImportVox.IsTriggered())
            {
                RoutineManager.Get().StartCoroutine(DataManager.Get().LoadVoxFile(false, this));
            }
            if (ActionPushModel.IsTriggered())
            {
                VoxelModel NewModel = new VoxelModel();
                NewModel.UseScript(FileUtil.ConvertToSingle(GetScript()));
                NewModel.Name = PushModelName;
                DataManager.Get().AddElement(DataFolderNames.VoxelModels, NewModel);
            }
        }


        public void ApplyActionCube(GameObject ActionCube, string VoxelActionName)
        {
            ApplyActionCube(ActionCube, VoxelActionName, Color.white);
        }
        public void ApplyActionCube(GameObject ActionCube, string VoxelActionName, Color32 VoxelColor)
        {
            if (ActionCube != null)
            {
                Debug.Log("Apply voxel [" + VoxelActionName + "] to: " + RealToBlockPosition(ActionCube.transform.position).ToString());
                BoxCollider MyCollider = ActionCube.GetComponent<BoxCollider>();
                if (MyCollider)
                {
                    UpdateBlockTypeMassArea(
                        VoxelActionName, 
                        RealToBlockPosition(ActionCube.transform.position),
                        new Vector3(
                            (MyCollider.size.x * MyCollider.transform.lossyScale.x - 1f) / 2f,
                            (MyCollider.size.y * MyCollider.transform.lossyScale.y - 1f) / 2f,
                            (MyCollider.size.z * MyCollider.transform.lossyScale.z - 1f) / 2f),
                        VoxelColor);
                }
                else
                {
                    UpdateBlockTypeMassArea(VoxelActionName, RealToBlockPosition(ActionCube.transform.position), 0, Color.white);
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
                    //Debug.Log("Creating new chunk: " + NewObject.name + " at time " + Time.time);
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
                NewChunk.GetMeshFilter();
                NewChunk.GetMeshRenderer();
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
                        ChildTransform.gameObject.Die();  // destroy any torches/voxel assets
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
                    Mesh MyMesh = MyChunk.GetComponent<MeshFilter>().sharedMesh;
                    if (MyMesh != null)
                    {
                        MonoBehaviourExtension.Kill(MyMesh);
                    }
                    // atm it is saved when it is removed
                    MyLookupTable.OnRemoveChunk(MyChunk); // Reduces the count by the amount in this chunk
                                                          // SaveChunk 
                    MyChunk.gameObject.Die();
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
                NewMeshRenderer.sharedMaterial = MyMeshRenderer.sharedMaterial;
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
                yield return YieldTimer();
            }
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
                Mesh ChunkMesh = MyChunkData[MyKey].GetComponent<MeshFilter>().sharedMesh;
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
                    if (MyCharacters[i] && MyCharacters[i].GetInWorld() == this)
                    {
                        MyCharacters[i].SetMovement(NewState);
                    }
                }
            }
        }
        #endregion

        #region FileUtility

        /// <summary>
        /// Resets chunks to use for loading
        /// </summary>
        private void Reset()
        {
            foreach (Int3 Key in MyChunkData.Keys)
            {
                if (MyChunkData.ContainsKey(Key))
                {
                    Chunk MyChunk = MyChunkData[Key];
                    if (MyChunk)
                    {
                        MyChunk.Reset(); // set them all to air since i've updated them
                    }
                }
            }
        }

        /// <summary>
        /// Set all the chunks voxels to 0 Air without triggering updates!
        /// </summary>
        public void SetAllVoxelsRaw()
        {
            foreach (Int3 Key in MyChunkData.Keys)
            {
                if (MyChunkData.ContainsKey(Key))
                {
                    Chunk MyChunk = MyChunkData[Key];
                    if (MyChunk)
                    {
                        MyChunk.SetAllVoxelsRaw(0);
                    }
                }
            }
        }


        public void SetAllVoxelsUpdated()
        {
            foreach (Int3 Key in MyChunkData.Keys)
            {
                if (MyChunkData.ContainsKey(Key))
                {
                    Chunk MyChunk = MyChunkData[Key];
                    if (MyChunk)
                    {
                        MyChunk.SetAllUpdated(true);
                    }
                }
            }
        }


        private void HideChunks()
        {
            SetChunksVisibility(false);
        }

        /// <summary>
        /// Turn all the renders off! Used when loading in new world
        /// </summary>
        private void SetChunksVisibility(bool NewState)
        {
            // first hide the chunks
            foreach (Int3 Key in MyChunkData.Keys)
            {
                if (MyChunkData.ContainsKey(Key))
                {
                    Chunk MyChunk = MyChunkData[Key];
                    if (MyChunk)
                    {
                        if (MyChunk != null)
                        {
                            MyChunk.GetComponent<MeshRenderer>().enabled = NewState;
                        }
                    }
                }
            }
        }

        public int GetMaxVoxelCount()
        {
            int VoxelCount = Mathf.RoundToInt(WorldSize.x * WorldSize.y * WorldSize.z);    // total world size
            VoxelCount *= Chunk.ChunkSize * Chunk.ChunkSize * Chunk.ChunkSize;
            return VoxelCount;
        }
        #endregion

        #region Saving

        /// <summary>
        /// Saves all the chunks for a model maker
        /// </summary>
        public IEnumerator SaveRoutine(string FilePath, ModelMaker MyManager)
        {
            List<string> MyScriptList = new List<string>();
            if (IsSingleChunk())
            {
                MyScriptList.AddRange(GetChunk(new Int3()).GetScript());
                yield return YieldTimer();
            }
            else
            {
                MyScriptList.Add("/World " + WorldSize.x + " " + WorldSize.y + " " + WorldSize.z);
                foreach (Int3 MyKey in MyChunkData.Keys)
                {
                    Chunk MyChunk = MyChunkData[MyKey];
                    MyScriptList.Add("/Chunk " + MyKey.x + " " + MyKey.y + " " + MyKey.z);
                    MyScriptList.AddRange(MyChunk.GetScript());
                    yield return YieldTimer();
                }
            }
        }

        /// <summary>
        /// Gets an entire world script.
        /// </summary>
        public List<string> GetScript()
        {
            return GetScriptList();
        }

        /// <summary>
        /// Returns a list of data for a world
        /// </summary>
        public List<string> GetScriptList()
        {
            List<string> MyData = new List<string>();
            MyData.AddRange(MyLookupTable.GetScript());
            if (IsSingleChunk())
            {
                MyData.AddRange(GetChunk(new Int3()).GetScript());
            }
            else
            {
                MyData.Add("/World " + WorldSize.x + " " + WorldSize.y + " " + WorldSize.z);
                foreach (Int3 MyKey in MyChunkData.Keys)
                {
                    Chunk MyChunk = MyChunkData[MyKey];
                    MyData.Add("/Chunk " + MyKey.x + " " + MyKey.y + " " + MyKey.z);
                    MyData.AddRange(MyChunk.GetScript());
                }
            }
            return MyData;
        }


        #endregion

        #region Loading


        public void AddOnLoad(UnityEngine.Events.UnityAction OnLoadAction)
        {
            OnLoadWorld.RemoveListener(OnLoadAction);
            OnLoadWorld.AddEvent(OnLoadAction);
        }
        public void RemoveOnLoad(UnityEngine.Events.UnityAction OnLoadAction)
        {
            OnLoadWorld.RemoveListener(OnLoadAction);
        }

        /// <summary>
        /// Loads the world script on a seperate routine
        /// </summary>
        public void RunScript(List<string> MyData, System.Action OnFinishLoading = null)
        {
            RoutineManager.Get().StartCoroutine(RunScriptRoutine(MyData, OnFinishLoading));
        }

        /// <summary>
        /// Run the script, loading the world including its meta data
        /// Voxel Chunk Data
        /// Lookup table
        /// The size of the chunks
        /// Any modifiers on the chunk - ie noise grid
        /// </summary>
        public IEnumerator RunScriptRoutine(List<string> MyData, System.Action OnFinishLoading = null)
        {
            //Debug.LogError("Begun World RunScriptRoutine " + name);
            if (MyData.Count > 0)
            {
                HideChunks();
                SetAllVoxelsRaw();  // clear the voxel indexes
                List<string> LookupData = new List<string>();
                bool HasLookupTable = (MyData[0] == VoxelLookupTable.BeginTag);
                if (HasLookupTable)
                {
                    // Use lookup table data at start of world
                    for (int i = 0; i < MyData.Count; i++)
                    {
                        if (MyData[i] == VoxelLookupTable.EndTag)
                        {
                            LookupData = MyData.GetRange(0, i + 1);
                            MyData.RemoveRange(0, i + 1);
                            break;
                        }
                    }
                    // Remove lookup table from list
                }
                Reset();
                bool IsMultiChunk = (MyData[0].Contains("/World"));
                Int3 NewWorldSize;
                if (IsMultiChunk)   // if multiple chunks
                {
                    //Debug.LogError("Loading Multi-Chunk with: " + MyData[0]);
                    string[] MyWorldMeta = MyData[0].Split(' ');
                    if (MyWorldMeta.Length == 4)
                    {
                        NewWorldSize = new Int3(int.Parse(MyWorldMeta[1]), int.Parse(MyWorldMeta[2]), int.Parse(MyWorldMeta[3]));
                    }
                    else
                    {
                        NewWorldSize = Int3.Zero();
                    }
                }
                else
                {
                    NewWorldSize = new Int3(1, 1, 1);
                }
                yield return SetWorldSizeRoutine(NewWorldSize); // this will refresh it
                MyLookupTable.RunScript(GetMaxVoxelCount(), LookupData);
                if (IsMultiChunk)   // if multiple chunks
                {
                    yield return RunScriptMultiWorldRoutine(MyData);
                }
                else
                {
                    Chunk WorldChunk = gameObject.GetComponent<Chunk>();
                    if (WorldChunk)
                    {
                        yield return WorldChunk.RunScript(FileUtil.ConvertToSingle(MyData));
                    }
                    else
                    {
                        Debug.LogError(name + " has no chunk.");
                    }
                }
                if (!HasLookupTable)
                {
                    MyLookupTable.Generate(this);
                }
            }
            else
            {
                //Debug.LogError("Loading world with no data");
                yield return SetWorldSizeRoutine(Int3.Zero()); // refresh! No more things!
            }
            OnLoadWorld.Invoke();
            if (OnFinishLoading != null)
            {
                OnFinishLoading();
            }
            // Debug.LogError("Ended World RunScriptRoutine " + name);
        }

        /// <summary>
        /// Runs the script in a routine
        /// </summary>
        private IEnumerator RunScriptMultiWorldRoutine(List<string> MyData)
        {
            IsUseUpdater = false;
            List<Chunk> UpdatedChunks = new List<Chunk>();
            Int3 ChunkPosition = Int3.Zero();
            for (int i = 1; i < MyData.Count; i++)
            {
                if (MyData[i].Contains("/Chunk"))
                {
                    string[] MyChunkMeta = MyData[i].Split(' ');
                    ChunkPosition.Set(int.Parse(MyChunkMeta[1]), int.Parse(MyChunkMeta[2]), int.Parse(MyChunkMeta[3]));
                    Chunk MyChunk = GetChunk(ChunkPosition);
                    UpdatedChunks.Add(MyChunk);
                    //i += Chunk.ChunkSize * Chunk.ChunkSize * Chunk.ChunkSize;
                    int NextIndex = i + 1;
                    // Find Next Chunk In Data
                    for (int j = i + 1; j < MyData.Count; j++)
                    {
                        if (MyData[j].Contains("/Chunk"))
                        {
                            NextIndex = j - 1;
                            break;
                        }
                        if (j == MyData.Count - 1)
                        {
                            NextIndex = MyData.Count - 1;
                        }
                    }
                    // now we have next index
                    List<string> MyChunkData = MyData.GetRange(i + 1, NextIndex - i);
                    //Debug.LogError(i + " Loading chunk: " + MyChunk.Position.GetVector().ToString() + " - " + MyChunkData.Count);
                    yield return MyChunk.RunScript(FileUtil.ConvertToSingle(MyChunkData), false);
                    //MyChunks.Add(MyChunk);
                    i = NextIndex;
                    yield return YieldTimer();
                }
            }
            for (int i = 0; i < UpdatedChunks.Count; i++)
            {
                yield return UpdatedChunks[i].BuildChunkMesh();
            }
            IsUseUpdater = true;
        }

        #endregion

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
            yield return (SetWorldSizeRoutine(NewSize, Int3.Zero(), OnResized));
        }

        /// <summary>
        /// Changes a world to a single chunk or a group of chunks, but can force the refresh
        /// </summary>
        public IEnumerator SetWorldSizeRoutine(Int3 NewSize, Int3 NewPositionOffset, OnSizeFinishedEvent OnResized = null, bool IsSizeDoubled = false)
        {
            yield return (SetPositionOffset(NewPositionOffset));
            Int3 OldLimit = Int3.Zero();
            OldLimit.Set(WorldSize);
            float TimeBegun = Time.realtimeSinceStartup;
            WorldUpdater.Get().Clear(this);  // Stop Updating if Resizing!
            if (NewSize != null && NewSize != OldLimit)
            {
                WorldSize = new Int3(NewSize);   // update limit
                CalculateCentre();
                // assuming they are changing size... Refresh the chunks here
                if (NewSize == Int3.Zero())
                {
                    yield return ClearChunks();
                }
                else if (!IsSingleChunk(NewSize) && !IsSingleChunk(OldLimit))  // both multi chunks
                {
                    yield return ResizeMultiChunkWorld(OldLimit, IsSizeDoubled);
                }
                else if (!IsSingleChunk(NewSize) && IsSingleChunk(OldLimit))   // Going from SingleChunk to MultiChunk Mode
                {
                    yield return ConvertToMultiChunkWorld(IsSizeDoubled);
                }
                else if (IsSingleChunk(NewSize) && !IsSingleChunk(OldLimit))  // if transforming from multi to singular
                {
                    yield return ConvertToSingleChunkWorld();
                }
                else if (IsSingleChunk(NewSize) && IsSingleChunk(OldLimit))   // if both single chunk
                {
                    //Debug.LogWarning(name + " is Going from Single Chunk to Single Chunk.");
                }
            }
            if (OnResized != null)
            {
                //Debug.Log("Invoking Delegate for world " + name + " resizing :)");
                OnResized.Invoke(this);
            }
            //Debug.LogError("Resized World: " + OldLimit.ToString() + ":" + NewLimit.ToString() + ": Time:" + (Time.realtimeSinceStartup - TimeBegun));
        }

        private IEnumerator ResizeMultiChunkWorld(Int3 OldSize, bool IsSizeDoubled = false)
        {
            yield return OnResizeRoutine(IsSizeDoubled);   // load new world - creates chunks and adds to chunk positions
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
        public IEnumerator OnResizeRoutine(bool IsSizeDoubled = false)
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
                                yield return YieldTimer();
                            }
                        }
                        else
                        {
                            if (MyChunk.Position.x < 0 || MyChunk.Position.x >= WorldSize.x ||
                                MyChunk.Position.y < 0 || MyChunk.Position.y >= WorldSize.y ||
                                MyChunk.Position.z < 0 || MyChunk.Position.z >= WorldSize.z)    // if chunk outside new bounds
                            {
                                DestroyChunk(MyChunk.Position);
                                yield return YieldTimer();
                            }
                        }
                    }
                }
            }
            if (!IsSingleChunk())
            {
                Int3 ChunkPosition = Int3.Zero();
                Int3 ChunkSpawnPosition = Int3.Zero();
                Int3 ChunksBeginPosition = Int3.Zero();
                if (IsSizeDoubled)
                {
                    ChunksBeginPosition = -WorldSize;
                    ChunksBeginPosition.x++;
                    ChunksBeginPosition.z++;
                    ChunksBeginPosition.y = 0;
                }

                for (ChunkPosition.x = ChunksBeginPosition.x; ChunkPosition.x < WorldSize.x; ChunkPosition.x++)
                {
                    for (ChunkPosition.z = ChunksBeginPosition.z; ChunkPosition.z < WorldSize.z; ChunkPosition.z++)
                    {
                        for (ChunkPosition.y = ChunksBeginPosition.y; ChunkPosition.y < WorldSize.y; ChunkPosition.y++)
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
                            yield return YieldTimer();
                        }
                    }
                }
            }
            yield return YieldTimer();
            //IsResizingWorld = false;
        }

        /// <summary>
        /// Converts from a single world to a multi world
        /// </summary>
        private IEnumerator ConvertToMultiChunkWorld(bool IsSizeDoubled = false)
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
            yield return OnResizeRoutine(IsSizeDoubled);
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
                    if (MyCharacter.GetInChunk() == MyChunk)
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
            //TimeBegin = Time.realtimeSinceStartup;
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
            yield return YieldTimer();
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
            //Debug.LogError (gameObject.name + " Updating block at: " + Position.GetVector().ToString() + 
            //    ": with size: " + VoxelSize + " with colour: " + VoxelColor.ToString() + ":" + VoxelName);
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
            Debug.LogError(name + " - is Updating Blocks, with posiitons count: " + Positions.Count);
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
            MassUpdateChunk = GetChunkWorldPosition(WorldPosition);
            if (MassUpdateChunk != null)
            {
                Debug.LogError("Updating: MassUpdateChunk" + MassUpdateChunk.name);
                MyVoxel = GetVoxel(WorldPosition);
                if (MyVoxel != null)
                {
                    PreviousType = MyVoxel.GetVoxelType();// GetVoxelType(WorldPosition);
                    PreviousColor = MyVoxel.GetColor();
                }
                else
                {
                    PreviousType = 0;
                    PreviousColor = Color.white;
                }
                VoxelIndex = MyLookupTable.GetIndex(VoxelName);
                DidUpdate = MassUpdateChunk.UpdateBlockTypeMass(WorldPosition, VoxelIndex, NewColor);
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
            //else
            {
                //Debug.LogError("Chunk is null at: " + WorldPosition.ToString());
            }
        }

        /// <summary>
        /// Called to mass update voxels in the world
        /// </summary>
        public void OnMassUpdate()
        {
            Debug.LogError("Creating " + VoxelPositionsMass.Count + " Voxels in world! with MassUpdateChunk.");
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

        #region UpdateUtility
        /// <summary>
        /// Update a voxel at a position - Uses Index - Depreciated, please use string identifier
        /// </summary>
        public void UpdateBlockTypeMass(int VoxelIndex, Int3 WorldPosition)
        {
            string VoxelName = MyLookupTable.GetName(VoxelIndex);
            UpdateBlockTypeMass(VoxelName, WorldPosition, Color.white);
        }
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


        // File
        #region MetaFile
        //public GameObject MySpawnZone;

        /// <summary>
        /// Gets the script for the worlds meta data.
        /// </summary>
        public List<string> GetScriptMeta()
        {
            List<string> MyScriptList = new List<string>();
            Int3 MapSize = WorldSize;
            MyScriptList.Add("" + MapSize.x);
            MyScriptList.Add("" + MapSize.y);
            MyScriptList.Add("" + MapSize.z);
            MyScriptList.AddRange(MyLookupTable.GetScript());
            if (MyVoxelTerrain)
            {
                MyScriptList.AddRange(MyVoxelTerrain.GetScript());
            }
            return MyScriptList;
        }

        /// <summary>
        /// Load the meta information from a single file
        /// </summary>
        public void LoadLevel(Level MyLevel)
        {
            StopAllCoroutines();
            //StartCoroutine(LoadLevelRoutine(MyLevel, Int3.Zero()));
        }

        /// <summary>
        /// Runs the Meta Data script. Loading things like map size.
        /// </summary>
       /* public IEnumerator LoadLevelRoutine(Level MyLevel, Int3 PositionOffset)
        {
            if (MyLevel != null)
            {
                if (MyLevel.Infinite())
                {
                    //Debug.Log("Setting " + name + " as infinite.");
                    if (UnityEngine.Application.isPlaying)
                    {
                        VoxelFreeRoam.Get().BeginRoaming();
                    }
                    IsHeight = true;
                }
            }
            else
            {
                Debug.LogError("Level is null");
            }
        }*/
        #endregion



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

        public Int3 GetChunkPosition(Transform MyTransform)
        {
            return WorldToChunkPosition(RealToBlockPosition(MyTransform.position));
        }
        /// <summary>
        /// Uses a transform to get a chunk positions chunk
        /// </summary>
        public Chunk GetChunkTransform(Transform MyTransform)
        {
            return GetChunkWorldPosition(RealToBlockPosition(MyTransform.position));
        }

        /// <summary>
        /// Gets chunk using a world position (Voxel coordinates)
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

        #region DroppingVoxels
        /// <summary>
        /// Creates a voxel byitself in the world.
        /// </summary>
        public void CreateBlockAtLocation(Vector3 Position, int TypeRemoved, Color32 MyTint)
        {
            //Debug.LogError("Creating new particle at: " + Position.ToString() + " of type: " + TypeRemoved);
            if (TypeRemoved == 0)
            {
                //Debug.LogError("Trying to create an air block.");
                return; // don't create air blocks lol
            }
            Item NewItem = ItemGenerator.Get().GenerateItem(TypeRemoved);   //MyDataBase.GenerateItem(TypeRemoved);
            //GameObject NewMoveableVoxel = new GameObject();
            GameObject NewMoveableVoxel = ItemManager.Get().SpawnItem(transform, NewItem);
            if (NewMoveableVoxel == null)
            {
                return;
            }
            NewMoveableVoxel.layer = gameObject.layer;
            NewMoveableVoxel.name = DataManager.Get().GetName(DataFolderNames.Voxels, TypeRemoved);
            // Transform
            NewMoveableVoxel.transform.position = BlockToRealPosition(Position + (new Vector3(1, 1, 1)) / 2f);    // transform.TransformPoint(Position + VoxelScale/2f);
            NewMoveableVoxel.transform.rotation = transform.rotation;
            NewMoveableVoxel.transform.localScale = transform.lossyScale / 2f;
            if (IsDropParticles)
            {
                NewMoveableVoxel.transform.localScale = transform.lossyScale;
            }
            NewMoveableVoxel.transform.localScale = new Vector3(
                VoxelScale.x * NewMoveableVoxel.transform.localScale.x,
                VoxelScale.y * NewMoveableVoxel.transform.localScale.y,
                VoxelScale.z * NewMoveableVoxel.transform.localScale.z);
            // ColourTint
            if (NewMoveableVoxel.GetComponent<MeshFilter>() == null)
            {
                NewMoveableVoxel.AddComponent<MeshFilter>();
            }
            // Components
            // Destroy any previous colliders
            SphereCollider MySphereCollider = NewMoveableVoxel.GetComponent<SphereCollider>();
            if (MySphereCollider)
            {
                MySphereCollider.Die();
            }
            // Create Components
            MeshCollider MyCollider = NewMoveableVoxel.GetComponent<MeshCollider>();
            if (MyCollider == null)
            {
                NewMoveableVoxel.AddComponent<MeshCollider>();
            }
            if (NewMoveableVoxel.GetComponent<Rigidbody>() == null)
            {
                NewMoveableVoxel.AddComponent<Rigidbody>();
            }
            MeshRenderer MyMeshRenderer = NewMoveableVoxel.GetComponent<MeshRenderer>();
            if (MyMeshRenderer == null)
            {
                MyMeshRenderer = NewMoveableVoxel.AddComponent<MeshRenderer>();
            }
            MyCollider.convex = true;
            MyMeshRenderer.sharedMaterial = VoxelManager.Get().MyMaterials[0];
            PolyModelHandle MyModel = NewMoveableVoxel.AddComponent<PolyModelHandle>();
            MyModel.UpdateWithSingleVoxelMesh( TypeRemoved, MyTint);
            if (IsDropParticles)
            {
                NewMoveableVoxel.GetComponent<ParticleSystem>().Die();
                NewMoveableVoxel.GetComponent<ItemHandler>().Die();
                NewMoveableVoxel.Die(UnityEngine.Random.Range(1, 15));
                if (NewMoveableVoxel.GetComponent<Rigidbody>() != null)
                {
                    NewMoveableVoxel.GetComponent<Rigidbody>().isKinematic = false;
                    NewMoveableVoxel.GetComponent<Rigidbody>().useGravity = false;
                    NewMoveableVoxel.AddComponent<Gravity>().GravityForce = new Vector3(0, -0.05f, 0);
                }
            }
            else
            {
                if (MyVoxelDestroyParticles)
                {
                    GameObject MyParticles = (GameObject)Instantiate(MyVoxelDestroyParticles, NewMoveableVoxel.transform.position, NewMoveableVoxel.transform.rotation);
                    MyParticles.Die(3);
                }
            }
        }
        #endregion

        #region Utility
        string ChunkGetVoxelName;
        VoxelMeta ChunkGetMeta;
        /// <summary>
        /// Using the index of voxel data, it will use lookup table to get the name, then return the databasse's meta data
        /// </summary>
        public VoxelMeta GetVoxelMeta(int VoxelIndex)
        {
            ChunkGetVoxelName = MyLookupTable.GetName(VoxelIndex);
            ChunkGetMeta = DataManager.Get().GetElement(DataFolderNames.Voxels, ChunkGetVoxelName) as VoxelMeta;
            if (ChunkGetMeta == null)
            {
                Debug.LogError("Could not find: " + ChunkGetVoxelName + ":" + VoxelIndex);
                ChunkGetMeta = DataManager.Get().GetElement(DataFolderNames.Voxels, "Air") as VoxelMeta;
            }
            return ChunkGetMeta;
            //return //MyDataBase.GetMeta(VoxelName);
        }

        public bool HasCollision()
        {
            return IsColliders;
        }
        /// <summary>
        /// Set the colliders of the worl
        /// </summary>
        public void SetColliders(bool NewIsColliders)
        {
            if (IsColliders != NewIsColliders)
            {
                IsColliders = NewIsColliders;
                SetCollidersRaw(IsColliders);
            }
        }

        private void SetCollidersRaw(bool NewState)
        {
            if (NewState)
            {
                foreach (Int3 MyKey in MyChunkData.Keys)
                {
                    Chunk MyChunk = MyChunkData[MyKey];
                    if (MyChunk)
                    {
                        MeshCollider MyMeshCollider = MyChunk.gameObject.GetComponent<MeshCollider>();
                        if (MyMeshCollider == null)
                        {
                            MyMeshCollider = MyChunk.gameObject.AddComponent<MeshCollider>();
                            MyMeshCollider.convex = IsConvex;
                        }
                        MyMeshCollider.sharedMesh = MyChunk.gameObject.GetComponent<MeshFilter>().sharedMesh;
                    }
                }
            }
            else
            {
                foreach (Int3 MyKey in MyChunkData.Keys)
                {
                    Chunk MyChunk = MyChunkData[MyKey];
                    if (MyChunk)
                    {
                        MeshCollider MyMeshCollider = MyChunk.gameObject.GetComponent<MeshCollider>();
                        if (MyMeshCollider)
                        {
                            MyMeshCollider.Die();
                        }
                    }
                }
            }
        }

        public void SetConvex(bool NewState)
        {
            IsConvex = NewState;
            if (IsColliders)
            {
                //Debug.Log("Setting " + name + " to new convex state: " + NewState.ToString());
                foreach (Int3 MyKey in MyChunkData.Keys)
                {
                    Chunk MyChunk = MyChunkData[MyKey];
                    MeshCollider MyMeshCollider = MyChunk.gameObject.GetComponent<MeshCollider>();
                    if (MyMeshCollider)
                    {
                        MyMeshCollider.convex = IsConvex;
                    }
                }
            }
        }

        /// <summary>
        /// Flip the world around an axis
        /// </summary>
        public void Flip(string FlipType)
        {
            VoxelData MyVoxelData = new VoxelData(
                Mathf.RoundToInt(WorldSize.x * Chunk.ChunkSize),
                Mathf.RoundToInt(WorldSize.y * Chunk.ChunkSize),
                Mathf.RoundToInt(WorldSize.z * Chunk.ChunkSize));
            Int3 VoxelPosition = Int3.Zero();
            Voxel OldVoxel;
            for (VoxelPosition.x = 0; VoxelPosition.x < MyVoxelData.GetSize().x; VoxelPosition.x++)
            {
                for (VoxelPosition.y = 0; VoxelPosition.y < MyVoxelData.GetSize().y; VoxelPosition.y++)
                {
                    for (VoxelPosition.z = 0; VoxelPosition.z < MyVoxelData.GetSize().z; VoxelPosition.z++)
                    {
                        OldVoxel = GetVoxel(VoxelPosition);
                        VoxelColor OldVoxelTInted = OldVoxel as VoxelColor;
                        if (OldVoxelTInted == null)
                        {
                            if (FlipType == "FlipY")
                            {
                                MyVoxelData.SetVoxelRaw(
                                    new Int3(VoxelPosition.x, Mathf.FloorToInt(MyVoxelData.GetSize().y - 1 - VoxelPosition.y), VoxelPosition.z),
                                    new Voxel(OldVoxel));   // -1
                            }
                            else if (FlipType == "FlipX")
                            {
                                MyVoxelData.SetVoxelRaw(
                                    new Int3(Mathf.RoundToInt(MyVoxelData.GetSize().x - 1 - VoxelPosition.x), VoxelPosition.y, VoxelPosition.z), 
                                    new Voxel(OldVoxel));
                            }
                            else if (FlipType == "FlipZ")
                            {
                                MyVoxelData.SetVoxelRaw(
                                    new Int3(VoxelPosition.x, VoxelPosition.y, Mathf.RoundToInt(MyVoxelData.GetSize().z - 1 - VoxelPosition.z)),
                                    new Voxel(OldVoxel));
                            }
                        }
                        else
                        {
                            if (FlipType == "FlipY")
                            {
                                MyVoxelData.SetVoxelRaw(
                                    new Int3(VoxelPosition.x, Mathf.FloorToInt(MyVoxelData.GetSize().y - 1 - VoxelPosition.y), VoxelPosition.z),
                                    new VoxelColor(OldVoxelTInted));   // -1
                            }
                            else if (FlipType == "FlipX")
                            {
                                MyVoxelData.SetVoxelRaw(
                                    new Int3(Mathf.RoundToInt(MyVoxelData.GetSize().x - 1 - VoxelPosition.x), VoxelPosition.y, VoxelPosition.z),
                                    new VoxelColor(OldVoxelTInted));
        }
                            else if (FlipType == "FlipZ")
                            {
                                MyVoxelData.SetVoxelRaw(
                                    new Int3(VoxelPosition.x, VoxelPosition.y, Mathf.RoundToInt(MyVoxelData.GetSize().z - 1 - VoxelPosition.z)),
                                        new VoxelColor(OldVoxelTInted));
                            }
                        }
                    }
                }
            }
            string VoxelName = "Air";
            int VoxelType = 0;
            int OldVoxelType = 0;
            Color VoxelColor = Color.white;
            for (VoxelPosition.x = 0; VoxelPosition.x < MyVoxelData.GetSize().x; VoxelPosition.x++)
            {
                for (VoxelPosition.y = 0; VoxelPosition.y < MyVoxelData.GetSize().y; VoxelPosition.y++)
                {
                    for (VoxelPosition.z = 0; VoxelPosition.z < MyVoxelData.GetSize().z; VoxelPosition.z++)
                    {
                        VoxelType = MyVoxelData.GetVoxelType(VoxelPosition);
                        if (VoxelType != OldVoxelType)
                        {
                            VoxelName = MyLookupTable.GetName(VoxelType);
                        }
                        VoxelColor = MyVoxelData.GetVoxelColorColor(VoxelPosition);
                        UpdateBlockTypeMass(VoxelName, VoxelPosition, VoxelColor);
                        GetVoxel(VoxelPosition).OnUpdated();
                        OldVoxelType = VoxelType;
                    }
                }
            }
            OnMassUpdate();
        }

        public void SetMeshVisibility(bool NewVisibility)
        {
            if (IsMeshVisible != NewVisibility)
            {
                IsMeshVisible = NewVisibility;
                //Debug.Log("Setting world " + name + " to mesh visibility of " + NewVisibility);
                foreach (Int3 KeyInKeys in MyChunkData.Keys)
                {
                    MyChunkData[KeyInKeys].SetMeshVisibility(NewVisibility);
                }
            }
        }

        /// <summary>
        /// Perform an action on a group of blocks
        /// </summary>
        public void ApplyAction(string ActionType, List<Int3> BlockPositions)
        {
            ApplyAction(ActionType, BlockPositions, new Color(1, 1, 1));
        }

        /// <summary>
        /// Flip the world around an axis
        /// </summary>
        public void ApplyAction(string ActionType, List<Int3> BlockPositions, Color MyColor)
        {
            VoxelData MyVoxelData = new VoxelData(
                Mathf.RoundToInt(WorldSize.x * Chunk.ChunkSize),
                Mathf.RoundToInt(WorldSize.y * Chunk.ChunkSize),
                Mathf.RoundToInt(WorldSize.z * Chunk.ChunkSize));
            for (int a = 0; a < BlockPositions.Count; a++)
            {
                int i = BlockPositions[a].x;
                int j = BlockPositions[a].y;
                int k = BlockPositions[a].z;
                if (ActionType == "MoveLeft")
                {
                    MyVoxelData.SetVoxelRaw(new Int3(i + 1, j, k), new Voxel(GetVoxel(BlockPositions[a])));
                }
                else if (ActionType == "MoveRight")
                {
                    MyVoxelData.SetVoxelRaw(new Int3(i - 1, j, k), new Voxel(GetVoxel(BlockPositions[a])));
                }
                else if (ActionType == "MoveForward")
                {
                    MyVoxelData.SetVoxelRaw(new Int3(i, j, k - 1), new Voxel(GetVoxel(BlockPositions[a])));
                }
                else if (ActionType == "MoveBack")
                {
                    MyVoxelData.SetVoxelRaw(new Int3(i, j, k + 1), new Voxel(GetVoxel(BlockPositions[a])));
                }
                else if (ActionType == "MoveUp")
                {
                    MyVoxelData.SetVoxelRaw(new Int3(i, j + 1, k), new Voxel(GetVoxel(BlockPositions[a])));
                }
                else if (ActionType == "MoveDown")
                {
                    MyVoxelData.SetVoxelRaw(new Int3(i, j - 1, k), new Voxel(GetVoxel(BlockPositions[a])));
                }
                else if (ActionType == "Erase")
                {
                    UpdateBlockTypeMass("Air", BlockPositions[a]);
                }
                else if (ActionType == "Color")
                {
                    UpdateBlockTypeMass(GetVoxelName(BlockPositions[a]), BlockPositions[a], MyColor);
                }
                else if (ActionType == "CutToNewModel")
                {
                    Voxel MyVoxel = GetVoxel(BlockPositions[a]);
                    VoxelColor MyVoxelColor = MyVoxel as VoxelColor;
                    if (MyVoxelColor == null)
                    {
                        MyVoxelData.SetVoxelRaw(new Int3(i, j, k), new Voxel(MyVoxel));
                    }
                    else
                    {
                        MyVoxelData.SetVoxelRaw(new Int3(i, j, k), new VoxelColor(MyVoxelColor));
                    }
                }
            }
            Int3 MovePosition = Int3.Zero();
            if (ActionType.Contains("Move"))
            {
                // Now wipe previous ones
                for (int a = 0; a < BlockPositions.Count; a++)
                {
                    UpdateBlockTypeMass("Air", new Int3(BlockPositions[a]));
                }
                // Move them over!
                for (MovePosition.x = 0; MovePosition.x < WorldSize.x * Chunk.ChunkSize; MovePosition.x++)
                {
                    for (MovePosition.y = 0; MovePosition.y < WorldSize.y * Chunk.ChunkSize; MovePosition.y++)
                    {
                        for (MovePosition.z = 0; MovePosition.z < WorldSize.z * Chunk.ChunkSize; MovePosition.z++)
                        {
                            int MyType = MyVoxelData.GetVoxelType(MovePosition);
                            if (MyType != 0)
                            {
                                UpdateBlockTypeMass(MyType, MovePosition);
                            }
                        }
                    }
                }
            }
            if (ActionType.Contains("CutToNewModel"))
            {
                // Move them over!
                for (MovePosition.x = 0; MovePosition.x < WorldSize.x * Chunk.ChunkSize; MovePosition.x++)
                {
                    for (MovePosition.y = 0; MovePosition.y < WorldSize.y * Chunk.ChunkSize; MovePosition.y++)
                    {
                        for (MovePosition.z = 0; MovePosition.z < WorldSize.z * Chunk.ChunkSize; MovePosition.z++)
                        {
                            int MyType = MyVoxelData.GetVoxelType(MovePosition);
                            Color VoxelColor = MyVoxelData.GetVoxelColorColor(MovePosition);
                            UpdateBlockTypeMass(MyLookupTable.GetName(MyType), MovePosition, VoxelColor);
                        }
                    }
                }
            }
            OnMassUpdate();
        }

        public void GetNeighborsBySolid(Int3 BlockPosition, List<Int3> MyNeighbors)
        {
            //List<Int3> MyNeighbors = new List<Int3>();
            Voxel MyVoxel = GetVoxel(BlockPosition);
            if (MyVoxel != null)
            {
                // if the same type, and doesn't contain this position, flood fill from here
                if (MyVoxel.GetVoxelType() != 0 && MyNeighbors.Contains(BlockPosition) == false)
                {
                    MyNeighbors.Add(BlockPosition);
                    if (MyNeighbors.Contains(BlockPosition.Left()) == false)
                    {
                        GetNeighborsBySolid(new Int3(BlockPosition.Left()), MyNeighbors);
                    }
                    if (MyNeighbors.Contains(BlockPosition.Right()) == false)
                    {
                        GetNeighborsBySolid(new Int3(BlockPosition.Right()), MyNeighbors);
                    }
                    if (MyNeighbors.Contains(BlockPosition.Above()) == false)
                    {
                        GetNeighborsBySolid(new Int3(BlockPosition.Above()), MyNeighbors);
                    }
                    if (MyNeighbors.Contains(BlockPosition.Below()) == false)
                    {
                        GetNeighborsBySolid(new Int3(BlockPosition.Below()), MyNeighbors);
                    }
                    if (MyNeighbors.Contains(BlockPosition.Front()) == false)
                    {
                        GetNeighborsBySolid(new Int3(BlockPosition.Front()), MyNeighbors);
                    }
                    if (MyNeighbors.Contains(BlockPosition.Behind()) == false)
                    {
                        GetNeighborsBySolid(new Int3(BlockPosition.Behind()), MyNeighbors);
                    }
                }
            }
        }

        public void GetNeighborsByColor(Int3 BlockPosition, Color32 VoxelColor, List<Int3> MyNeighbors)
        {
            //List<Int3> MyNeighbors = new List<Int3>();
            Voxel MyVoxel = GetVoxel(BlockPosition);
            if (MyVoxel != null)
            {
                // if the same type, and doesn't contain this position, flood fill from here
                if (MyVoxel.GetColor() == VoxelColor && MyNeighbors.Contains(BlockPosition) == false)
                {
                    MyNeighbors.Add(BlockPosition);
                    if (MyNeighbors.Contains(BlockPosition.Left()) == false)
                    {
                        GetNeighborsByColor(new Int3(BlockPosition.Left()), VoxelColor, MyNeighbors);
                    }
                    if (MyNeighbors.Contains(BlockPosition.Right()) == false)
                    {
                        GetNeighborsByColor(new Int3(BlockPosition.Right()), VoxelColor, MyNeighbors);
                    }
                    if (MyNeighbors.Contains(BlockPosition.Above()) == false)
                    {
                        GetNeighborsByColor(new Int3(BlockPosition.Above()), VoxelColor, MyNeighbors);
                    }
                    if (MyNeighbors.Contains(BlockPosition.Below()) == false)
                    {
                        GetNeighborsByColor(new Int3(BlockPosition.Below()), VoxelColor, MyNeighbors);
                    }

                    if (MyNeighbors.Contains(BlockPosition.Front()) == false)
                    {
                        GetNeighborsByColor(new Int3(BlockPosition.Front()), VoxelColor, MyNeighbors);
                    }
                    if (MyNeighbors.Contains(BlockPosition.Behind()) == false)
                    {
                        GetNeighborsByColor(new Int3(BlockPosition.Behind()), VoxelColor, MyNeighbors);
                    }
                }
            }
        }

        /// <summary>
        /// Flip the world around an axis
        /// </summary>
        public void CropSelected(List<Int3> BlockPositions)
        {
            Int3 MinimumPosition = GetWorldBlockSize().ToInt3();
            Int3 MaximumPosition = Int3.Zero();
            int i, j, k;
            for (int a = 0; a < BlockPositions.Count; a++)
            {
                i = BlockPositions[a].x;
                j = BlockPositions[a].y;
                k = BlockPositions[a].z;
                if (i < MinimumPosition.x)
                {
                    MinimumPosition.x = i;
                }
                else if (i > MaximumPosition.x)
                {
                    MaximumPosition.x = i;
                }
                if (j < MinimumPosition.y)
                {
                    MinimumPosition.y = j;
                }
                else if (j > MaximumPosition.y)
                {
                    MaximumPosition.y = j;
                }
                if (k < MinimumPosition.z)
                {
                    MinimumPosition.z = k;
                }
                else if (k > MaximumPosition.z)
                {
                    MaximumPosition.z = k;
                }
            }
            //Debug.LogError("Max is: " + MaximumPosition.GetVector().ToString() + " - Min is: " + MinimumPosition.ToString());
            Int3 TotalSize = MaximumPosition - MinimumPosition;
            Int3 NewWorldSize = new Int3(
                Mathf.CeilToInt(TotalSize.x / (float)Chunk.ChunkSize),
                Mathf.CeilToInt(TotalSize.y / (float)Chunk.ChunkSize),
                Mathf.CeilToInt(TotalSize.z / (float)Chunk.ChunkSize));
            NewWorldSize.x = Mathf.Max(NewWorldSize.x, 1);
            NewWorldSize.y = Mathf.Max(NewWorldSize.y, 1);
            NewWorldSize.z = Mathf.Max(NewWorldSize.z, 1);
            // adjust to chunk size
            TotalSize.x = Chunk.ChunkSize * NewWorldSize.x;
            TotalSize.y = Chunk.ChunkSize * NewWorldSize.y;
            TotalSize.z = Chunk.ChunkSize * NewWorldSize.z;
            //Debug.LogError("NewWorldSize is: " + NewWorldSize.ToString() + " - TotalSize is: " + TotalSize.ToString());
            VoxelData MyVoxelData = new VoxelData(TotalSize.x, TotalSize.y, TotalSize.z);
            Voxel MyVoxel;
            VoxelColor MyVoxelColor;
            for (int a = 0; a < BlockPositions.Count; a++)
            {
                MyVoxel = GetVoxel(BlockPositions[a]);
                MyVoxelColor = MyVoxel as VoxelColor;
                i = BlockPositions[a].x - MinimumPosition.x;
                j = BlockPositions[a].y - MinimumPosition.y;
                k = BlockPositions[a].z - MinimumPosition.z;
                if (MyVoxelColor == null)
                {
                    MyVoxelData.SetVoxelRaw(new Int3(i, j, k), new Voxel(MyVoxel));
                }
                else
                {
                   // Debug.LogError("Setting: " + i + ":" + j + ":" + k + " to - : " + MyVoxelColor.GetVoxelType());
                    MyVoxelData.SetVoxelRaw(new Int3(i, j, k), new VoxelColor(MyVoxelColor));
                }
                UpdateBlockTypeMass("Air", new Int3(i,j,k));
            }
            Color VoxelColor = Color.white;
            int MyType = 0;
            int OldType = 0;
            string VoxelName = "Air";
            Int3 VoxelPosition = Int3.Zero();
            // Move them over!
            for (i = 0; i < TotalSize.x; i++)
            {
                for (j = 0; j < TotalSize.y; j++)
                {
                    for (k = 0; k < TotalSize.z; k++)
                    {
                        VoxelPosition.Set(i, j, k);
                        MyType = MyVoxelData.GetVoxelType(VoxelPosition);
                        if (MyType != OldType)
                        {
                            VoxelName = MyLookupTable.GetName(MyType);
                        }
                        VoxelColor = MyVoxelData.GetVoxelColorColor(VoxelPosition);
                        /*if (MyType != 0)
                        {
                            Debug.LogError("Setting: " + i + ":" + j + ":" + k + " to - : " + VoxelName);
                        }*/
                        UpdateBlockTypeMass(VoxelName, VoxelPosition, VoxelColor);
                        GetVoxel(VoxelPosition).OnUpdated();
                        OldType = MyType;
                    }
                }
            }
            StartCoroutine(SetWorldSizeRoutine(NewWorldSize, 
                (ResizedWorld) => 
                {
                    StartCoroutine(RefreshInTime(ResizedWorld));
                }));
        }

        public void ForceRefresh()
        {
            SetAllVoxelsUpdated();
            OnMassUpdate();
        }

        private System.Collections.IEnumerator RefreshInTime(World ResizedWorld)
        {
            yield return YieldTimer();
            //Debug.LogError("Refreshing world");
            ResizedWorld.SetAllVoxelsUpdated();
            ResizedWorld.OnMassUpdate();
        }
        #endregion

        #region Seek

        /// <summary>
        /// Find the closest position within voxel distance - of the same type as this position
        /// </summary>
        public Vector3 FindClosestVoxelPosition(Vector3 ThisPosition, int MaxVoxelDistance)
        {
            int VoxelIndex = GetVoxelType(new Int3(ThisPosition));
            //Debug.LogError("Looking for: " + ThisPosition.ToString() + ":" + GetVoxelType(ThisPosition) + " with Distance of: " + MaxVoxelDistance);
            return FindClosestVoxelPosition(ThisPosition, VoxelIndex, MaxVoxelDistance, 1);
        }
        /// <summary>
        /// Find the closest position within voxel distance - of the same type as this position
        /// </summary>
        public Vector3 FindClosestVoxelPosition(Vector3 ThisPosition, int VoxelIndex, int MaxVoxelDistance)
        {
            //Debug.LogError("Looking for: " + ThisPosition.ToString() + ":" + GetVoxelType(ThisPosition) + " with Distance of: " + MaxVoxelDistance);
            return FindClosestVoxelPosition(ThisPosition, VoxelIndex, MaxVoxelDistance, 1);
        }
        /// <summary>
        /// Find the closest position within voxel distance - of the same type as this position
        /// This is the recursive part of the function
        /// </summary>
        public Vector3 FindClosestVoxelPosition(Vector3 ThisPosition, int VoxelIndex, int MaxVoxelDistance, int VoxelDistance)
        {
            if (VoxelDistance > MaxVoxelDistance)
            {
                //Debug.LogError("Could not find voxel!");
                return ThisPosition;
            }
            for (int i = -VoxelDistance; i <= VoxelDistance; i++)
            {
                for (int j = -VoxelDistance; j <= VoxelDistance; j++)
                {
                    for (int k = -VoxelDistance; k <= VoxelDistance; k++)
                    {
                        if (i == -VoxelDistance || i == VoxelDistance ||
                            j == -VoxelDistance || j == VoxelDistance ||
                            k == -VoxelDistance || k == VoxelDistance)
                        {
                            Int3 OtherPosition = new Int3(ThisPosition) + new Int3(i, j, k);
                            int OtherType = GetVoxelType(OtherPosition);
                            if (VoxelIndex == OtherType)
                            {
                                //Debug.LogError("FoundVoxel!");
                                return OtherPosition.GetVector();
                            }
                        }
                    }
                }
            }
            VoxelDistance++;
            return FindClosestVoxelPosition(ThisPosition, VoxelIndex, MaxVoxelDistance, VoxelDistance);    // if failed
        }
        #endregion
    }
}