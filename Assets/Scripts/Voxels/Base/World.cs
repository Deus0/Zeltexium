using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Util;
using Zeltex.Guis.Maker;

namespace Zeltex.Voxels
{
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
	public partial class World : MonoBehaviour
    {
        #region Variables
        public static int TextureResolution = 16;
        public static bool IsMipMaps = false;
        public bool IsDebug = false;

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

        [HideInInspector]
        public bool IsAddOutline = true;
        private UnityEvent OnLoadWorld = new UnityEvent();
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
                    Debug.LogError("Loading:\n" + MyElement.VoxelData);
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
                VoxelModel NewModel = new VoxelModel(Util.FileUtil.ConvertToSingle(GetScript()));
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
                yield return null;
            }
            else
            {
                MyScriptList.Add("/World " + WorldSize.x + " " + WorldSize.y + " " + WorldSize.z);
                foreach (Int3 MyKey in MyChunkData.Keys)
                {
                    Chunk MyChunk = MyChunkData[MyKey];
                    MyScriptList.Add("/Chunk " + MyKey.x + " " + MyKey.y + " " + MyKey.z);
                    MyScriptList.AddRange(MyChunk.GetScript());
                    yield return null;
                }
            }
            //string MyScript = FileUtil.ConvertToSingle(MyScriptList);
            //File.WriteAllText(FilePath, MyScript);
            //DataManager.Get().Set(DataFolderNames.PolyModels, MyManager.GetSelectedIndex(), MyScript);
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
        public void RunScript(List<string> MyData)
        {
            RoutineManager.Get().StartCoroutine(RunScriptRoutine(MyData));
        }

        /// <summary>
        /// Run the script, loading the world including its meta data
        /// Voxel Chunk Data
        /// Lookup table
        /// The size of the chunks
        /// Any modifiers on the chunk - ie noise grid
        /// </summary>
        public IEnumerator RunScriptRoutine(List<string> MyData)
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
                yield return (SetWorldSizeRoutine(NewWorldSize)); // this will refresh it
                MyLookupTable.RunScript(GetMaxVoxelCount(), LookupData);
                if (IsMultiChunk)   // if multiple chunks
                {
                    yield return (RunScriptMultiWorldRoutine(MyData));
                    // TODO: Fix this!
                    GetChunk(Int3.Zero()).SetAllUpdated(true);
                    GetChunk(Int3.Zero()).OnMassUpdate();
                }
                else
                {
                    Chunk WorldChunk = gameObject.GetComponent<Chunk>();
                    if (WorldChunk)
                    {
                        //Debug.LogError("Begun WorldChunk RunScript " + name);
                        yield return null;
                        //Debug.LogError("Loading chunkworld script from world: " + name);
                        yield return (WorldChunk.RunScript(MyData));
                        //Debug.LogError("Ended WorldChunk RunScript " + name);
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
                yield return UniversalCoroutine.CoroutineManager.StartCoroutine(SetWorldSizeRoutine(Int3.Zero())); // refresh! No more things!
            }
            OnLoadWorld.Invoke();
            // Debug.LogError("Ended World RunScriptRoutine " + name);
        }

        /// <summary>
        /// Runs the script in a routine
        /// </summary>
        private IEnumerator RunScriptMultiWorldRoutine(List<string> MyData)
        {
            IsUseUpdater = false;
            // Debug.LogError("Loading World on routine.");
            //List<Chunk> MyChunks = new List<Chunk>();
            for (int i = 1; i < MyData.Count; i++)
            {
                if (MyData[i].Contains("/Chunk"))
                {
                    string[] MyChunkMeta = MyData[i].Split(' ');
                    Chunk MyChunk = GetChunk(new Int3(int.Parse(MyChunkMeta[1]), int.Parse(MyChunkMeta[2]), int.Parse(MyChunkMeta[3])));
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
                    yield return (MyChunk.RunScript(MyChunkData));  //UniversalCoroutine.CoroutineManager.StartCoroutine
                    //MyChunks.Add(MyChunk);
                    i = NextIndex;
                    yield return null;
                }
            }
            //yield return MyUpdater.UpdateChunks(MyChunks);
            //Debug.LogError("Resizing World to 3:" + GetComponent<World>().WorldSize.ToString());
            IsUseUpdater = true;
        }

        #endregion
    }
}