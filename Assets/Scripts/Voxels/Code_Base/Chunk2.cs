using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex;

namespace Zeltex.Voxels
{
    /// <summary>
    /// Mesh building of chunks!
    /// </summary>
    public partial class Chunk : MonoBehaviour
    {
        #region MeshBUildingVariables
        private List<MeshData> ChunkMeshes = new List<MeshData>(); // a mesh data per material! This is cleared after the building is done
        private bool IsCheckingForUpdate = false;
        private TileMap MyTilemap = new TileMap(8, 8, 16, 16);
        public bool IsMeshVisible = true;

        // meshing stuff
        private bool IsAir;
        private bool IsRebuild;
        private Int3 MeshingBlockPosition = new Int3();
        private int MeshingMaterialIndex = 0;
        private Vector3 VoxelVertexOffset = new Vector3();
        // Calculating the sides of voxels
        private bool[] MySides = new bool[] { false, false, false, false, false, false };
        private Voxel VoxelAbove;
        private Voxel VoxelBelow;
        private Voxel VoxelFront;
        private Voxel VoxelBehind;
        private Voxel VoxelLeft;
        private Voxel VoxelRight;
        private VoxelModel SidesCalculationModel;
        private VoxelMeta SidesCalculationMeta;
        // Calculating solidity
        private bool[] MySolids = new bool[27];
        private bool IsSolidAbove = false;
        private bool IsSolidBelow = false;
        private bool IsSolidFront = false;
        private bool IsSolidBehind = false;
        private bool IsSolidLeft = false;
        private bool IsSolidRight = false;
        private VoxelModel ModelOther;
        private VoxelMeta MetaOther;
        // calculating the voxel model data
        private Voxel MeshingVoxel;
        private VoxelMeta CalculateVoxelModelMeta;
        private List<Vector2> NewUVs = new List<Vector2>();
        private MeshData DataBasePartModel;
        private int CalculateSolidsIndex = 0;
        private int MeshingType = 0;
        private Int3 AbovePosition = Int3.Zero();
        private Int3 BelowPosition = Int3.Zero();
        private Int3 FrontPosition = Int3.Zero();
        private Int3 BehindPosition = Int3.Zero();
        private Int3 LeftPosition = Int3.Zero();
        private Int3 RightPosition = Int3.Zero();

        private bool HasInitiated;
        private Int3 BuildVoxelIndex = Int3.Zero();
        private List<CombineInstance> CombiningMeshList = new List<CombineInstance>();
        private int VertCount = 0;
        private Int3 CreateMeshVoxelIndex = Int3.Zero();
        private int MaterialIndex;
        private Voxel CreateMeshVoxel;
        private int CreateMeshChunkIndex;
        // Turns the voxels into meshes
        #region MeshUpdates
        private float TimeBegun;
        private static string AirName = "Air";
        private Color VoxelColor;
        #endregion

        #region Utility

        public void SetMeshVisibility(bool NewVisibility)
        {
            if (IsMeshVisible != NewVisibility)
            {
                IsMeshVisible = NewVisibility;
                if (MyMeshRenderer)
                {
                    MyMeshRenderer.enabled = IsMeshVisible;
                }
                else
                {
                    Debug.LogError("Mesh Renderer not found in [" + name + ".");
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
        #endregion

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

        /// <summary>
        /// Runs on a thread - starts the build process of the mesh data
        /// BUilds a different mesh per material!
        /// </summary>
        public IEnumerator BuildChunkMesh()
        {
            Debug.Log("Building " + name);
            OnBuildingMesh();
            int i = 0;
            if (ChunkMeshes.Count != MyWorld.MyMaterials.Count)
            {
                ChunkMeshes.Clear();
                for (i = 0; i < MyWorld.MyMaterials.Count; i++)
                {
                    ChunkMeshes.Add(new MeshData());    // create a place where we can store our voxel mesh
                }
            }
            //Debug.LogError("Updated Mesh:[" + name + "] BuildChunkMesh Function " + MyWorld.MyMaterials.Count);
            for (i = 0; i < MyWorld.MyMaterials.Count; i++)     // for each material, build the mesh of each chunk
            {
                MeshingMaterialIndex = i;
                yield return UniversalCoroutine.CoroutineManager.StartCoroutine(BuildChunkMeshPerMaterial());
            }
            IsUpdatingRender = true;
            IsBuildingMesh = false; // tell updater it has finished building chunk mesh!
            yield return UniversalCoroutine.CoroutineManager.StartCoroutine(UpdateChunk());
            Debug.Log("Finished Building " + name);
        }
        
        /// <summary>
        /// Converts a world into a mesh data. Limited at 1 chunk per world for this.
        /// TODO: Somewhere here is a 100ms GC.Collect call - due to allocation/deallocation of memory
        /// </summary>
        private IEnumerator BuildChunkMeshPerMaterial()
        {
            //int TimesPaused = 0;
            TimeBegun = Time.realtimeSinceStartup;
            if (MyWorld && MyWorld.MyDataBase)
            {
                for (BuildVoxelIndex.x = 0; BuildVoxelIndex.x < ChunkSize; BuildVoxelIndex.x++)
                {
                    for (BuildVoxelIndex.y = 0; BuildVoxelIndex.y < ChunkSize; BuildVoxelIndex.y++)
                    {
                        for (BuildVoxelIndex.z = 0; BuildVoxelIndex.z < ChunkSize; BuildVoxelIndex.z++)
                        {
                            MeshingVoxel = MyVoxels.GetVoxelRaw(BuildVoxelIndex);
                            MeshingType = MeshingVoxel.GetVoxelType();
                            IsAir = (MeshingType == 0);
                            if (IsAir == false)
                            {
                                if (MeshingVoxel.GetHasUpdated() == true || MyWorld.IsReBuildAllMeshes)
                                {
                                    //Debug.LogError("Updating voxel mesh: " + BuildVoxelIndex.ToString() + " --- " + MeshingType);
                                    //float TimeStarted = Time.realtimeSinceStartup;  // how long did rebuild take?
                                    MeshingBlockPosition.Set(BuildVoxelIndex);
                                    BuildMeshVoxel();
                                    MeshingVoxel.OnBuiltMesh(); // as the mesh has been rebuilt!
                                }
                            }
                        }
                    }
                    yield return null;
                }
            }
            else
            {
                Debug.LogError("No Longer supporting mesh building without a database");
            }
            yield return null;
        }

        /// <summary>
        /// Converts a voxel into a mesh.
        /// Each mesh is built depending on it's surrounding side voxels (6 in total).
        /// </summary>
        private void BuildMeshVoxel()//, ref MeshData ChunkMeshData)
        {
            if (MyWorld.MyLookupTable.ContainsMeshIndex(MeshingType) == false)
            {
                Debug.LogError("Lookup table does not have type: " + MeshingType
                    + " - Attempting to auto generate it");
                string DataManagerVoxelName = "";
                for (int i = 0; i < DataManager.Get().GetSizeElements(DataFolderNames.VoxelMeta); i++)
                {
                    DataManagerVoxelName = DataManager.Get().GetName(DataFolderNames.VoxelMeta, i);
                    if (MyWorld.MyLookupTable.ContainsVoxel(DataManagerVoxelName) == false)
                    {
                        MyWorld.MyLookupTable.AddName(DataManagerVoxelName, MeshingType);
                        break;
                    }
                }
            }
            // Check if can place type
            if (MyWorld.MyLookupTable.ContainsMeshIndex(MeshingType) == true)
            {
				if (MeshingVoxel.GetVoxelType() != 0)
				{
                    // Get the voxel data to build the model, based on its side voxels
                    CalculateSides();
                    CalculateSolids();
                    CalculateLights();
                    CalculateVoxelModel();
                }
            }
            else
            {
                //Debug.LogError("Lookup table does not have type: " + MeshingType);
            }
            //Debug.Log("Finished building mesh data for voxel [" + VoxelIndex + "]: " + i + ":" + j + ":" + k + " with " + ThisVoxel.MyMeshData.Verticies.Count + " verticies");
        }
        
        /// <summary>
        /// Get the sides of the voxel! True if is to draw!
        /// </summary>
        private void CalculateSides()
        {
            SidesCalculationMeta = GetWorld().GetVoxelMeta(MeshingVoxel.GetVoxelType());
			if (SidesCalculationMeta != null)
			{
                SidesCalculationModel = SidesCalculationMeta.GetModel();

                AbovePosition.Set(MeshingBlockPosition.Above());
                BelowPosition.Set(MeshingBlockPosition.Below());
                FrontPosition.Set(MeshingBlockPosition.Front());
                BehindPosition.Set(MeshingBlockPosition.Behind());
                LeftPosition.Set(MeshingBlockPosition.Left());
                RightPosition.Set(MeshingBlockPosition.Right());

                VoxelAbove = GetVoxel(AbovePosition);
                VoxelBelow = GetVoxel(BelowPosition);
                VoxelFront = GetVoxel(FrontPosition);
                VoxelBehind = GetVoxel(BehindPosition);
                VoxelLeft = GetVoxel(LeftPosition);
                VoxelRight = GetVoxel(RightPosition);

			    // Solids
			    IsSolidAbove = false;
			    IsSolidBelow = false;
			    IsSolidFront = false;
			    IsSolidBehind = false;
			    IsSolidLeft = false;
			    IsSolidRight = false;
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
        }
        
        /// <summary>
        /// Get the solid sides around the voxel - used for side culling
        /// </summary>
        private void CalculateSolids()
        {
            CalculateSolidsIndex = 0;  // 0 to 26 - 27 size
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
        /// For a voxel, create a mesh for it!
        /// </summary>
        private void CalculateVoxelModel()
        {
            CalculateVoxelModelMeta = GetWorld().GetVoxelMeta(MeshingVoxel.GetVoxelType());
            if (CalculateVoxelModelMeta != null && MeshingMaterialIndex == CalculateVoxelModelMeta.MaterialID)
            {
                if (CalculateVoxelModelMeta.GetModel() != null)
                {
                    MeshingVoxel.MyMeshData.Clear();
                    for (int SideIndex = 0; SideIndex < MySides.Length; SideIndex++)     // for all 6 sides of the voxel
                    {
                        if (MySides[SideIndex])
                        {
                            // first add mesh verticies
                            DataBasePartModel = GetWorld().MyDataBase.GetMeshData(CalculateVoxelModelMeta.ModelID, SideIndex);
                            MeshingVoxel.MyMeshData.AddDataMesh = DataBasePartModel;
                            MeshingVoxel.MyMeshData.Add();
                            // Add the range of uvs just for the rendered verticies

                            NewUVs.Clear();
                            NewUVs = CalculateVoxelModelMeta.GetModel().GetTextureMapCoordinates(
                                CalculateVoxelModelMeta.TextureMapID,
                                SideIndex,
                                MyTilemap);
                            MeshingVoxel.MyMeshData.TextureCoordinates.AddRange(NewUVs);
                        }
                    }
                    MeshingVoxel.MyMeshData.Colors.Clear();
                    VoxelColor = MeshingVoxel.GetColor();
                    for (int z = 0; z < MeshingVoxel.MyMeshData.Verticies.Count; z++)
                    {
                        MeshingVoxel.MyMeshData.Colors.Add(VoxelColor);
                    }

                    // Multiply by voxel scale - both the model data and the grid positioning!
                    MeshingVoxel.MyMeshData.MultiplyVerts(MyWorld.VoxelScale);
                    // Add the grid position to each voxel model!
                    VoxelVertexOffset.x = MyWorld.VoxelScale.x * MeshingBlockPosition.x;
                    VoxelVertexOffset.y = MyWorld.VoxelScale.y * MeshingBlockPosition.y;
                    VoxelVertexOffset.z = MyWorld.VoxelScale.z * MeshingBlockPosition.z;
                    MeshingVoxel.MyMeshData.AddToVertex(VoxelVertexOffset);
                }
                else
                {
                    Debug.LogError("Model is null>");
                }
            }
            else
            {
                Debug.LogError("Meta is null>");
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
           // Debug.LogError("Updating chunk " + name);
            yield return UniversalCoroutine.CoroutineManager.StartCoroutine(UpdateMesh());       // updates the mesh
           // Debug.LogError("Updating chunk " + name + " finish UpdateMesh");
            //MyMeshes.Clear();
            // Spawn stuff now!
            SpawnCharactersOnChunk();   // sets characters spawned to active
            DropVoxels();               // after mesh is built and updated
            //Debug.Log("Finished loading chunk. " + Time.time);
            ChunkMeshes.Clear();
            IsUpdatingRender = false;
            HasStartedUpdating = false;
            if (!HasInitiated)
            {
                HasInitiated = true;
            }
        }

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
        /// Method used to update mesh renderer mesh.
        /// Combines the ChunkMeshes into one mesh, using different materials
        /// </summary>
        private IEnumerator UpdateMesh()
        {
            float TimeStarted = Time.realtimeSinceStartup;
            RefreshComponentLinks();
            yield return UniversalCoroutine.CoroutineManager.StartCoroutine(CreateMesh(MyMeshFilter));
            MyMeshRenderer.materials = MyWorld.MyMaterials.ToArray();
            // colliders
            if (MyMeshCollider)
            {
                MyMeshCollider.sharedMesh = null;
                MyMeshCollider.sharedMesh = MyMeshFilter.sharedMesh;
            }
        }

        /// <summary>
        /// Create teh mesh data
        /// </summary>
        private IEnumerator CreateMesh(MeshFilter MyMeshFilter)
        {
            if (MyMeshFilter.sharedMesh == null)
            {
                //DestroyImmediate(MyMeshFilter.sharedMesh);
                MyMeshFilter.sharedMesh = new Mesh();
            }
            // Set up meshes
            for (CreateMeshChunkIndex = 0; CreateMeshChunkIndex < ChunkMeshes.Count; CreateMeshChunkIndex++)
            {
                ChunkMeshes[CreateMeshChunkIndex].Clear();
                for (CreateMeshVoxelIndex.x = 0; CreateMeshVoxelIndex.x < Chunk.ChunkSize; CreateMeshVoxelIndex.x++)
                {
                    for (CreateMeshVoxelIndex.y = 0; CreateMeshVoxelIndex.y < Chunk.ChunkSize; CreateMeshVoxelIndex.y++)
                    {
                        for (CreateMeshVoxelIndex.z = 0; CreateMeshVoxelIndex.z < Chunk.ChunkSize; CreateMeshVoxelIndex.z++)
                        {
                            CreateMeshVoxel = MyVoxels.GetVoxel(CreateMeshVoxelIndex.x, CreateMeshVoxelIndex.y, CreateMeshVoxelIndex.z);
                            ChunkMeshes[CreateMeshChunkIndex].AddDataMesh = CreateMeshVoxel.MyMeshData;
                            ChunkMeshes[CreateMeshChunkIndex].Add();
                        }
                    }
                    if ((CreateMeshVoxelIndex.x + 1) % 4 == 0)
                    {
                        yield return null;
                    }
                }
                ChunkMeshes[CreateMeshChunkIndex].AddToVertex(-MyWorld.CentreOffset);
            }
            //Debug.Log(MyWorld.name + "'s chunk has vertex count of: " + ChunkMeshes[0].Verticies.Count + ":" + MyWorld.WorldSize.ToString());
            MyMeshFilter.mesh.Clear();
            //Debug.LogError("Updated Mesh: ChunkMeshes: " + ChunkMeshes.Count +
            //    " - Materials: " + GetWorld().MyMaterials.Count);
            if (ChunkMeshes.Count > 0)
            {
                VertCount = 0;
                CombiningMeshList.Clear();
                // Combine all the materials together
                for (MaterialIndex = 0; MaterialIndex < GetWorld().MyMaterials.Count; MaterialIndex++)
                {
                    CombineInstance NewCombineInstance = new CombineInstance();
                    NewCombineInstance.mesh = new Mesh();
                    yield return null;
                    NewCombineInstance.transform = transform.localToWorldMatrix;
                    CombiningMeshList.Add(NewCombineInstance);
                    CombiningMeshList[MaterialIndex].mesh.vertices = ChunkMeshes[MaterialIndex].GetVerticies();
                    CombiningMeshList[MaterialIndex].mesh.triangles = ChunkMeshes[MaterialIndex].GetTriangles();
                    CombiningMeshList[MaterialIndex].mesh.uv = ChunkMeshes[MaterialIndex].GetTextureCoordinates();
                    CombiningMeshList[MaterialIndex].mesh.colors32 = ChunkMeshes[MaterialIndex].GetColors().ToArray();
                    VertCount += ChunkMeshes[MaterialIndex].Verticies.Count;
                }
                //Debug.LogError("Before CombineMeshes.");
                MyMeshFilter.sharedMesh.CombineMeshes(CombiningMeshList.ToArray(), false, false);
                //Debug.LogError("After CombineMeshes.");
                //Debug.LogError("Updated Mesh:[" + name + "] Vertexes: " + MyMeshFilter.sharedMesh.vertexCount);
                MyMeshFilter.sharedMesh.subMeshCount = CombiningMeshList.Count;
                MyMeshFilter.sharedMesh.name = name + " Mesh";
                MyMeshFilter.sharedMesh.RecalculateNormals();
                //MyMeshFilter.sharedMesh.RecalculateBounds();
                MyMeshFilter.sharedMesh.RecalculateTangents();
                for (MaterialIndex = 0; MaterialIndex < CombiningMeshList.Count; MaterialIndex++)
                {
                    MonoBehaviourExtension.Kill(CombiningMeshList[MaterialIndex].mesh);
                }
                CombiningMeshList.Clear();
                //Debug.LogError("Before UploadMeshData.");
                MyMeshFilter.sharedMesh.UploadMeshData(false);
                //Debug.LogError("Finished UploadMeshData.");
            }
            else
            {
                Debug.LogError("Chunk Mesh is null inside: " + GetWorld().name + ":" + name);
            }
            MyVoxels.Reset();
        }
        #endregion
    }

}
/*Mesh CombinedMeshes = new Mesh ();
CombinedMeshes.CombineMeshes (MeshList.ToArray (), true, false);
CombinedMeshes.name = "_Collision"; // gameObject.name + 
MyMeshCollider.sharedMesh = CombinedMeshes;	// should be a combination of the solid parts!*/
/// <summary>
/// Called when single voxel is updated
/// </summary>
/* public void Updated()
 {
     Debug.LogError(name + " - MyWorld: " + Position.GetVector().ToString());
     //IsUpdatingRender = true;
     // HasUpdatedLights = false;
     // i should only check for this every .1 second
     HasSaved = false;
     //Debug.LogError(name + " - MyWorld: ");
     //Debug.LogError(name + " - MyWorld: " + MyWorld.name);
     if (MyWorld.IsUseUpdater)
     {
         //MyWorld.MyUpdater.AddLights(this);
         MyWorld.MyUpdater.Add(this);
     }
     //IsCheckingForUpdate = false;
 }*/

//{
//if ((Time.realtimeSinceStartup - TimeBegun) * 1000 >= 0.005f)    // greater then 10 milliseconds
//{
//    TimeBegun = Time.realtimeSinceStartup;
//TimesPaused++;
//Debug.Log("Times paused at: " + TimeBegun);
//yield return new WaitForSeconds(1000 / 120f);
//yield return new WaitForSecondsRealtime(60f / 2000f);   // wait half a  frame if taken a frame
//System.GC.Collect();
//yield return new WaitForSecondsRealtime(0.005f);
//}
// }