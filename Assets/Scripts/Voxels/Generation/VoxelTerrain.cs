using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Zeltex.Util;
using System.Collections.Generic;
using Zeltex.Guis.Maker;
using UniversalCoroutine;

namespace Zeltex.Voxels 
{
    /// <summary>
    /// Generate the terrain for the world
    /// </summary>
    [ExecuteInEditMode]
	public class VoxelTerrain : MonoBehaviour
    {
        [Header("Actions")]
        [SerializeField]
        private EditorAction IsGenerate;
        [SerializeField]
        private EditorAction IsResize;
        [SerializeField]
        private Int3 NewWorldSize;
        [SerializeField]
        private World TargetWorld;
        public bool IsGenerateTilemap;

        // Terrain Generation Settings
        [Header("Terrain")]
		[SerializeField]
        private TerrainMetaData MyTerrainMetaData = new TerrainMetaData();
        [SerializeField]
        private RawImage MyImage;
        //[SerializeField]
        //private Int3 DefaultMapSize = new Int3(7, 2, 7);
        // terrain generation
        private Int3 MyWorldPosition = Int3.Zero();
        private float HeightMapNoise = 0;
        private float StretchedNoise = 0;
        //private World MyWorld;
        private Chunk TerrainChunk;
        private Int3 ChunkIndex = Int3.Zero();

        public void Update()
        {
            if (TargetWorld)
            {
                if (IsGenerate.IsTriggered())
                {
                    CreateTerrainWorld(TargetWorld);
                }
                if (IsResize.IsTriggered())
                {
                    CoroutineManager.StartCoroutine(TargetWorld.SetWorldSizeRoutine(NewWorldSize));
                }
                if (IsGenerateTilemap)
                {
                    IsGenerateTilemap = false;
                    VoxelManager.Get().GenerateTileMap();
                }
            }
        }

        #region File
        public static string BeginTag = "/BeginTerrain";
        public static string EndTag = "/EndTerrain";
        //private static string SplitterTag = ",";

        public List<string> GetScript()
        {
            List<string> Data = new List<string>();
            Data.Add(BeginTag);

            Data.Add(EndTag);
            return Data;
        }

        public bool RunScript(List<string> Data)
        {
            // Clear
            // Set Defaults
            bool DidRead = false;
            bool IsReading = false;
            for (int i = 0; i < Data.Count; i++)
            {
                if (IsReading == false)
                {
                    if (Data[i] == BeginTag)
                    {
                        IsReading = true;
                        DidRead = true;
                    }
                }
                else
                {
                    if (Data[i] == EndTag)
                    {
                        IsReading = false;
                        break;
                    }
                    else
                    {

                    }
                }
            }
            return DidRead;
        }
        #endregion

        #region WorldUpdates

        /// <summary>
        /// Create terrain over world
        /// </summary>
        public void CreateFlatLand(World MyWorld)
		{
            StartCoroutine(CreateFlatlandRoutine(MyWorld));
        }

        private IEnumerator CreateFlatlandRoutine(World MyWorld)
		{
            MyTerrainMetaData.SetNames(VoxelManager.Get());    // set meta data
			Debug.Log("Creating Flatland of height: " + MyTerrainMetaData.BaseHeight + " with block: " + MyTerrainMetaData.DirtName);
            if (MyWorld.GetWorldSizeChunks() == Int3.Zero())
            {
                yield return MyWorld.SetWorldSizeRoutine(new Int3(7, 1, 7));
            }
            foreach (Int3 MyKey in MyWorld.MyChunkData.Keys)
            {
                Chunk MyChunk = MyWorld.MyChunkData[MyKey];
                for (int i = 0; i < Chunk.ChunkSize; i++)
                {
                    for (int j = 0; j < Chunk.ChunkSize; j++)
                    {
                        for (int k = 0; k < Chunk.ChunkSize; k++)
                        {
                            Int3 MyWorldPosition = MyChunk.Position * Chunk.ChunkSize + new Int3(i, j, k);    // block position in world
                            if (MyWorldPosition.y <= MyTerrainMetaData.BaseHeight)
                            {
                                MyWorld.UpdateBlockTypeMass(MyTerrainMetaData.DirtName, MyWorldPosition);
                            }
							else
							{
								MyWorld.UpdateBlockTypeMass("Air", MyWorldPosition);
							}
                        }
                    }
                }
            }
            MyWorld.OnMassUpdate();
            yield break;
        }

        /// <summary>
        /// Create terrain over world
        /// </summary>
        public void CreateTerrainWorld(World MyWorld)
        {
            CoroutineManager.StartCoroutine(CreateTerrainWorldRoutine(MyWorld));
        }

        public IEnumerator CreateTerrainWorldRoutine(World MyWorld, bool IsBuildMeshes = true)
        {
            foreach (Int3 MyKey in MyWorld.MyChunkData.Keys)
            {
                TerrainChunk = MyWorld.MyChunkData[MyKey];
                yield return (CreateTerrain());
                if (IsBuildMeshes)
                {
                    yield return TerrainChunk.BuildChunkMesh();
                }
            }
            yield return null;
        }
        #endregion

        /// <summary>
        /// Run by VoxelTerrain!
        /// </summary>
        public IEnumerator CreateTerrain(Chunk MyChunk)
        {
            TerrainChunk = MyChunk;
            yield return CreateTerrain();
        }

        #region TerrainGeneration

        /// <summary>
        /// Create the terrain inside a chunk, using some meta data
        /// </summary>
        private IEnumerator CreateTerrain()
        {
           // MyWorld = TerrainChunk.GetWorld();
            MyTerrainMetaData.SetNames(VoxelManager.Get());    // set meta data
            if (TerrainChunk != null)
            {
                /*for (int i = 0; i < Chunk.ChunkSize; i++)
                {
                    for (int j = 0; j < Chunk.ChunkSize; j++)
                    {
                        for (int k = 0; k < Chunk.ChunkSize; k++)
                        {
                            Int3 MyWorldPosition = MyChunk.Position * Chunk.ChunkSize;    // block position in world
                        }
                    }
                }*/
                TerrainChunk.MassUpdateColor = Color.white;
                for (ChunkIndex.x = 0; ChunkIndex.x < Chunk.ChunkSize; ChunkIndex.x++)
                {
                    for (ChunkIndex.y = 0; ChunkIndex.y < Chunk.ChunkSize; ChunkIndex.y++)
                    {
                        for (ChunkIndex.z = 0; ChunkIndex.z < Chunk.ChunkSize; ChunkIndex.z++)
                        {
                            TerrainChunk.MassUpdatePosition = ChunkIndex;
                            //MyChunk.UpdateBlockTypeMass(i, j, k, 0);
                            MyWorldPosition = TerrainChunk.Position * Chunk.ChunkSize;
                            MyWorldPosition.Add(ChunkIndex);
                            MyWorldPosition.Add(MyTerrainMetaData.WorldOffset);

                            //WorldPositionY = TerrainChunk.Position.y * Chunk.ChunkSize + ChunkIndex.y;

                            HeightMapNoise = SimplexNoise.Noise(      MyWorldPosition.x * MyTerrainMetaData.Frequency,
                                                                            0,//MyPosition.y * MyMeta.Frequency,
                                                                            MyWorldPosition.z * MyTerrainMetaData.Frequency);
                            StretchedNoise = HeightMapNoise * MyTerrainMetaData.Amplitude;
                            StretchedNoise += MyTerrainMetaData.BaseHeight;

                            if (StretchedNoise <= MyTerrainMetaData.MinimumHeight)
                            {
                                StretchedNoise = MyTerrainMetaData.MinimumHeight;
                            }
                            // chunk update is local to chunk
                            // world update function is local to world
                            StretchedNoise = Mathf.RoundToInt(StretchedNoise);
                            if (MyWorldPosition.y == 0)
                            {
                                TerrainChunk.MassUpdateVoxelName = MyTerrainMetaData.BedrockName;
                                TerrainChunk.UpdateBlockTypeMassTerrain();
                               // MyWorld.UpdateBlockTypeMass(MyTerrainMetaData.BedrockName, MyWorldPosition, Color.white, true);
                            }
                            else if (MyWorldPosition.y <= StretchedNoise)
                            {// height of land
                                if (MyWorldPosition.y != StretchedNoise)
                                {
                                    //MyWorld.UpdateBlockTypeMass(MyTerrainMetaData.DirtName, MyWorldPosition, Color.white, true);
                                    TerrainChunk.MassUpdateVoxelName = MyTerrainMetaData.DirtName;
                                    TerrainChunk.UpdateBlockTypeMassTerrain();
                                }
                                else
                                {
                                    //MyWorld.UpdateBlockTypeMass(MyTerrainMetaData.GrassName, MyWorldPosition, Color.white, true); // top block
                                    TerrainChunk.MassUpdateVoxelName = MyTerrainMetaData.GrassName;
                                    TerrainChunk.UpdateBlockTypeMassTerrain();
                                    if (MyTerrainMetaData.IsTrees)
                                    {
                                        if (ChunkIndex.x >= MyTerrainMetaData.LeavesSize && ChunkIndex.x < Chunk.ChunkSize - MyTerrainMetaData.LeavesSize &&
                                            ChunkIndex.z >= MyTerrainMetaData.LeavesSize && ChunkIndex.z < Chunk.ChunkSize - MyTerrainMetaData.LeavesSize &&
                                            ((float)Random.Range(1, 100)) / 100f >= MyTerrainMetaData.TreeFrequencyCutoff)
                                        {
                                            CreateTree();
                                        }
                                    }
                                }
                            }
                            /*else
                            {
                                MyWorld.UpdateBlockTypeMass("Air", MyWorldPosition);
                            }*/
                        }
                    }
                    if ((ChunkIndex.x + 1) % 2 == 0)
                    {
                        yield return null;
                    }
                }
            }
            TerrainChunk.TriggerMassUpdate();
        }
        /*if (MyChunk.Position.x >= 0)
        {
            MyWorldPosition.x += i;
        }
        else
        {
            MyWorldPosition.x += Chunk.ChunkSize - i;
        }
        if (MyChunk.Position.y >= 0)
        {
            MyWorldPosition.y += j;
        }
        else
        {
            MyWorldPosition.y += Chunk.ChunkSize - j;
        }
        if (MyChunk.Position.z >= 0)
        {
            MyWorldPosition.z += k;
        }
        else
        {
            MyWorldPosition.z += Chunk.ChunkSize - k;
        }*/

        Int3 TreePosition = Int3.Zero();
        Int3 LeafPosition = Int3.Zero();
        float DistanceToMid = 0;
        /// <summary>
        /// Create a tree at a position
        /// </summary>
        public void CreateTree()
        {
            TreePosition.Set(ChunkIndex);
            for (TreePosition.y = 1; TreePosition.y <= ChunkIndex.y + MyTerrainMetaData.TreeHeight; TreePosition.y++)
            {
                TerrainChunk.MassUpdateVoxelName = MyTerrainMetaData.WoodName;
                TerrainChunk.MassUpdatePosition = TreePosition;
                TerrainChunk.UpdateBlockTypeMassTerrain();

                //TerrainChunk.GetWorld().UpdateBlockTypeMass(MyTerrainMetaData.WoodName, 
                //    WorldPosition + new Int3(0, z, 0), Color.white, true);// i, j + z, k, MyMeta.TreeBlock);  // make tree
            }
            CreateTreeLeaves();    // i, j + z, k
        }

        /// <summary>
        /// Create tree leaves from a position
        /// </summary>
        public void CreateTreeLeaves()   //int i, int j, int k
        {
            for (LeafPosition.x = TreePosition.x - MyTerrainMetaData.LeavesSize; LeafPosition.x <= TreePosition.x + MyTerrainMetaData.LeavesSize; LeafPosition.x++)
            {
                for (LeafPosition.y = TreePosition.y - MyTerrainMetaData.LeavesSize; LeafPosition.y <= TreePosition.y + MyTerrainMetaData.LeavesSize; LeafPosition.y++)
                {
                    for (LeafPosition.z = TreePosition.z - MyTerrainMetaData.LeavesSize; LeafPosition.z <= TreePosition.z + MyTerrainMetaData.LeavesSize; LeafPosition.z++)
                    {
                        DistanceToMid = Vector3.Distance(LeafPosition.GetVector(), TreePosition.GetVector());
                        if (DistanceToMid <= MyTerrainMetaData.LeavesSize)
                        {
                            TerrainChunk.MassUpdateVoxelName = MyTerrainMetaData.LeafName;
                            TerrainChunk.MassUpdatePosition = LeafPosition;
                            TerrainChunk.UpdateBlockTypeMassTerrain();
                            //MyChunk.GetWorld().UpdateBlockTypeMass(MyMeta.LeafName, LeafPosition, Color.white, true);  // make tree leaves
                        }
                    }
                }
            }
        }
        #endregion

        #region ApplyMaps
        /// <summary>
        /// Using the texture, it will generate the terrain based on it
        /// </summary>
        public void CreateTerrainFromTextureMap(World MyWorld)
        {
            Debug.Log("Building Terrain from texture map.");
            MyTerrainMetaData.SetNames(VoxelManager.Get());    // set meta data
            Color[] MyColors = (MyImage.texture as Texture2D).GetPixels(0);
            for (int i = 0; i < MyImage.texture.width; i++)
            {
                for (int j = 0; j < MyImage.texture.height; j++)
                {
                    //int MyType = Mathf.RoundToInt((MyColors[ZeltexTools.TextureEditor.GetPixelIndex(i, j, MyImage.texture.width)].b * 255f) / 5);
                    int MyHeight = Mathf.RoundToInt((MyColors[TextureEditor.GetPixelIndex(i, j, MyImage.texture.width)].r * 255f) / 15);
                    //Debug.Log("New Type: " + newType + " - new height: " + MyHeight);
                    for (int k = MyHeight; k >= 0; k--)
                    {
                        //Vector3 MyWorldPosition = MyChunk.Position.GetVector() * Chunk.ChunkSize;    // block position in world
                        //Debug.Log("New Dirt at position: " + i + ":" + j + ":" + k + ": " + MyTerrainMetaData.DirtName);
                        MyWorld.UpdateBlockTypeMass(MyTerrainMetaData.DirtName, new Int3(i, k, j));
                    }
                }
            }
            MyWorld.OnMassUpdate();
        }
        #endregion

        #region TextureMaps

        /// <summary>
        /// Creates a texture for tree locations
        /// </summary>
        public void CreateTreeMap()
        {

        }

        /// <summary>
        /// Create new colours
        /// </summary>
        private void OnUpdateTerrain(World MyWorld)
        {
            Texture2D MyTexture = new Texture2D(
                Mathf.FloorToInt(MyWorld.GetWorldBlockSize().x),
                Mathf.FloorToInt(MyWorld.GetWorldBlockSize().z));
            MyTexture.filterMode = FilterMode.Point;
            Color[] MyColors = MyTexture.GetPixels(0);
            // get voxels on each high point
            for (int i = 0; i < Chunk.ChunkSize * MyTexture.width; i++)
            {
                for (int k = 0; k < Chunk.ChunkSize * MyTexture.height; k++)
                {
                    for (int j = Mathf.FloorToInt(Chunk.ChunkSize * MyWorld.GetWorldBlockSize().y - 1); j >= 0; j--)
                    {
                        int MyVoxelType = MyWorld.GetVoxelType(new Int3(i, j, k));
                        if (MyVoxelType != 0)
                        {
                            MyColors[TextureEditor.GetIndex(MyTexture, new Vector2(i, k))] = new Color(
                                    (MyVoxelType * 30) / 255f,
                                    (j * 10) / 255f,
                                    55 / 255f);
                            break;
                        }
                    }
                }
            }
            // change voxels to colours
            MyTexture.SetPixels(MyColors);
            MyTexture.Apply();
            MyImage.texture = MyTexture;
        }

        /// <summary>
        /// Creates a texture for terrain
        /// </summary>
        public void CreateTerrainMap(World MyWorld)
        {
            Texture2D MyTexture = new Texture2D(
                Mathf.FloorToInt(MyWorld.GetWorldBlockSize().x),
                Mathf.FloorToInt(MyWorld.GetWorldBlockSize().z));
            MyTexture.filterMode = FilterMode.Point;
            Color[] MyColors = MyTexture.GetPixels(0);
            // get voxels on each high point
            for (int i = 0; i < Chunk.ChunkSize * MyTexture.width; i++)
            {
                for (int k = 0; k < Chunk.ChunkSize * MyTexture.height; k++)
                {
                    Vector3 MyWorldPosition = new Vector3(i, 0, k);    // block position in world
                    Vector3 MyPosition = MyTerrainMetaData.WorldOffset + MyWorldPosition;  // used for noise
                    float HeightMapNoise = SimplexNoise.Noise(MyPosition.x * MyTerrainMetaData.Frequency, 0, MyPosition.z * MyTerrainMetaData.Frequency);
                    float MyNoise = HeightMapNoise * MyTerrainMetaData.Amplitude;
                    MyNoise += MyTerrainMetaData.BaseHeight;
                    if (MyNoise <= MyTerrainMetaData.MinimumHeight)
                    {
                        MyNoise = MyTerrainMetaData.MinimumHeight;
                    }
                    int MyVoxelType = MyTerrainMetaData.GetDirtType();
                    MyColors[TextureEditor.GetIndex(MyTexture, new Vector2(i, k))] =
                        new Color((MyNoise * 15) / 255f,
                                    (MyNoise * 10) / 255f,
                                    (MyVoxelType * 5) / 255f);
                }
            }
            // change voxels to colours
            MyTexture.SetPixels(MyColors);
            MyTexture.Apply();
            MyImage.texture = MyTexture;
        }
        #endregion

        #region Extra
        public void CreatePlane(World MyWorld, float Height, int MyType)
        {
            foreach (Int3 MyKey in MyWorld.MyChunkData.Keys)
            {
                Chunk MyChunk = MyWorld.MyChunkData[MyKey];
                CreatePlane(MyChunk, Height, MyType);
            }
        }

        //string VoxelName;
        //string VoxelName2;
        private VoxelData TerrainVoxelData;
        int VoxelIndexX;
        int VoxelIndexY;
        int VoxelIndexZ;
        Int3 VoxelPosition;
        /// <summary>
        /// Add random pillars to these planes
        /// Use voxel data to try and reduce the collection data (in heap stack)
        /// </summary>
        public void CreatePlane(Chunk MyChunk, float Height, int MyType)
        {
            //MyWorld = MyChunk.GetWorld();
            // TODO: GetMetaByType("Dirt") or "Wood" or "Leaf"
            // Create biome datas that players can edit
            int VoxelIndex1 = MyType;
            int VoxelIndex2 = 4;
            //VoxelName = MyWorld.MyDataBase.GetMetaName(VoxelIndex1);
            //VoxelName2 = MyWorld.MyDataBase.GetMetaName(VoxelIndex2);
            TerrainVoxelData = MyChunk.GetVoxelData();// new VoxelData();
            for (VoxelIndexX = 0; VoxelIndexX < Chunk.ChunkSize; VoxelIndexX++)
            {
                for (VoxelIndexY = 0; VoxelIndexY < Chunk.ChunkSize; VoxelIndexY++)
                {
                    for (VoxelIndexZ = 0; VoxelIndexZ < Chunk.ChunkSize; VoxelIndexZ++)
                    {
                        VoxelPosition = new Int3(VoxelIndexX, VoxelIndexY, VoxelIndexZ);
                        MyWorldPosition = MyChunk.Position * Chunk.ChunkSize + VoxelPosition;    // block position in world
                        if(MyWorldPosition.y <= Height)
                        {
                            if (VoxelIndexX == 0 || VoxelIndexX == Chunk.ChunkSize - 1 || VoxelIndexZ == 0 || VoxelIndexZ == Chunk.ChunkSize - 1)
                            {
                                TerrainVoxelData.SetVoxelTypeRaw(VoxelPosition, VoxelIndex2);
                                //MyChunk.GetWorld().UpdateBlockTypeMass(VoxelName2, MyWorldPosition, Color.white, true);
                            }
                            else
                            {
                                TerrainVoxelData.SetVoxelTypeRaw(VoxelPosition, VoxelIndex1);
                               // MyChunk.GetWorld().UpdateBlockTypeMass(VoxelName, MyWorldPosition, Color.white, true);
                            }
                        }
                        else
                        {
                            TerrainVoxelData.SetVoxelTypeRaw(VoxelPosition, 0);
                            //MyChunk.GetWorld().UpdateBlockTypeMass("Air", MyWorldPosition, Color.white, true);
                            //TerrainVoxelData.SetBlock("Air", new Int3(i, j, k));
                        }
                    }
                }
            }
            MyChunk.SetVoxelData(TerrainVoxelData);
        }
        #endregion

        // inputs
        #region UI
        public void UpdateInputDirt(InputField MyInput)
        {
            UpdateInput(MyInput, "Dirt");
        }
        public void UpdateInputGrass(InputField MyInput)
        {
            UpdateInput(MyInput, "Grass");
        }
        public void UpdateInputBedrock(InputField MyInput)
        {
            UpdateInput(MyInput, "Bedrock");
        }
        public void UpdateInputTree(InputField MyInput)
        {
            UpdateInput(MyInput, "Tree");
        }
        public void UpdateInputLeaf(InputField MyInput)
        {
            UpdateInput(MyInput, "Leaf");
        }
        public void UpdateInput(InputField MyInput, string MyType)
        {
            int NewBlockType = int.Parse(MyInput.text);
            if (MyType == "Dirt")
            {
                MyTerrainMetaData.BlockType1 = NewBlockType;
            }
            else if (MyType == "Grass")
            {
                MyTerrainMetaData.BlockType2 = NewBlockType;
            }
            else if (MyType == "Bedrock")
            {
                MyTerrainMetaData.BottomFloorBlock = NewBlockType;
            }
            else if (MyType == "Tree")
            {
                MyTerrainMetaData.TreeBlock = NewBlockType;
            }
            else if (MyType == "Leaf")
            {
                MyTerrainMetaData.LeafBlock = NewBlockType;
            }
        }
        public void UpdateAmplitude(string NewAmp)
        {
            float NewAmplitude = float.Parse(NewAmp);
            MyTerrainMetaData.Amplitude = NewAmplitude;
        }

        public void UpdateBaseHeight(string NewAmp)
        {
            float NewAmplitude = float.Parse(NewAmp);
            MyTerrainMetaData.BaseHeight = NewAmplitude;
        }

        public void UpdateBaseFrequency(string NewAmp)
        {
            float NewAmplitude = float.Parse(NewAmp);
            MyTerrainMetaData.Frequency = NewAmplitude;
        }
        /// <summary>
        /// Used in UI
        /// </summary>
        //public void CreateTerrain()
        //{
            //StartCoroutine(CreateTerrainRoutine(0.2f));
            //CreateTerrain (MyWorld, MyTerrainMetaData);
        //}
        #endregion
    }
}


/*public void CreateTrees(Chunk MyChunk, TerrainMetaData MyMeta)
{
    if (MyChunk != null)
    {
    //Vector3 HillPosition = new Vector3(16, 16, 16);
        for (int i = MyMeta.LeavesSize; i < Chunk.ChunkSize - MyMeta.LeavesSize; i++)
        {
            for (int j = MyMeta.LeavesSize; j < Chunk.ChunkSize - MyMeta.LeavesSize; j++)
            {
                for (int k = MyMeta.LeavesSize; k < Chunk.ChunkSize - MyMeta.LeavesSize; k++)
                {
                    Vector3 WorldPosition = MyMeta.WorldOffset + MyChunk.Position.GetVector() * Chunk.ChunkSize + new Vector3(i, j, k);
                    float HeightMapNoise = SimplexNoise.Noise(
                                    WorldPosition.x * MyMeta.Frequency,
                                    WorldPosition.y * MyMeta.Frequency,
                                    WorldPosition.z * MyMeta.Frequency
                                    );
                    float StretchedNoise = HeightMapNoise * MyMeta.Amplitude;
                    StretchedNoise += MyMeta.BaseHeight;
                    if (StretchedNoise <= MyMeta.MinimumHeight)
                    {
                        StretchedNoise = MyMeta.MinimumHeight;
                    }

                    int WorldPositionY = MyChunk.Position.y * Chunk.ChunkSize + j;
                    // chunk update is local to chunk
                    // world update function is local to world
                    StretchedNoise = Mathf.RoundToInt(StretchedNoise);
                    if (WorldPositionY == StretchedNoise)
                    {
                        float IsTree = SimplexNoise.Noise(      WorldPosition.x * MyMeta.TreeFrequency,
                                                                WorldPosition.y * MyMeta.TreeFrequency,
                                                                WorldPosition.z * MyMeta.TreeFrequency);
                        if (IsTree >= MyMeta.TreeFrequencyCutoff)
                        {
                            Voxel VoxelRight = MyChunk.GetVoxel(i + 1, j, k);
                            Voxel VoxelLeft = MyChunk.GetVoxel(i - 1, j, k);
                            Voxel VoxelFront = MyChunk.GetVoxel(i, j, k + 1);
                            Voxel VoxelBack = MyChunk.GetVoxel(i, j, k - 1);
                            if (VoxelRight != null 
                                && VoxelLeft != null 
                                && VoxelFront != null 
                                && VoxelBack != null
                                && VoxelRight.GetVoxelType() != MyMeta.TreeBlock 
                                && VoxelLeft.GetVoxelType() != MyMeta.TreeBlock &&
                                    VoxelFront.GetVoxelType() != MyMeta.TreeBlock 
                                    && VoxelBack.GetVoxelType() != MyMeta.TreeBlock)
                            {
                                CreateTree(MyMeta, WorldPosition, MyChunk);
                            }
                        }
                    }
                }
            }
        }
    }
}*/
//StretchedNoise += Vector3.Distance(MyWorldPosition, HillPosition);
//4*Vector3.Distance(Position.GetVector()*Chunk.ChunkSize + new Vector3(i,j,k),
//new Vector3(HillPosition.x, Position.y*Chunk.ChunkSize+j, HillPosition.z))/HillPosition.y;	// percentage of total height of hill?
/*float MyBasicNoise2 = SimplexNoise.Noise.Generate(MyPosition.x*MyMeta.Frequency2,
                                              MyPosition.y * MyMeta.Frequency2,  
                                              MyPosition.z * MyMeta.Frequency2);*/
//if (MyBasicNoise2 >= 0.6f) 
//	MyChunk.UpdateBlockTypeSaveFile(i,j,k, MyMeta.BlockType2);

/*float IsTree = SimplexNoise.Noise.Generate(MyPosition.x * MyMeta.TreeFrequency,
                                              MyPosition.y * MyMeta.TreeFrequency,
                                              MyPosition.z * MyMeta.TreeFrequency);
if (IsTree >= MyMeta.TreeFrequencyCutoff)
{
    Voxel VoxelRight = MyChunk.GetVoxel(i + 1, j, k);
    Voxel VoxelLeft = MyChunk.GetVoxel(i - 1, j, k);
    Voxel VoxelFront = MyChunk.GetVoxel(i, j, k + 1);
    Voxel VoxelBack = MyChunk.GetVoxel(i, j, k - 1);
    if (VoxelRight != null && VoxelLeft != null && VoxelFront != null && VoxelBack != null)
    if (VoxelRight.GetBlockIndex() != MyMeta.TreeBlock &&
        VoxelLeft.GetBlockIndex() != MyMeta.TreeBlock &&
       VoxelFront.GetBlockIndex() != MyMeta.TreeBlock &&
        VoxelBack.GetBlockIndex() != MyMeta.TreeBlock)
    {
        for (int z = 1; z <= MyMeta.TreeHeight; z++)
        {
            MyChunk.UpdateBlockTypeSaveFile(i, j + z, k, MyMeta.TreeBlock);  // make tree
            // now for top part
            if (z == MyMeta.TreeHeight)
            {
                for (int a = -MyMeta.LeavesSize; a <= MyMeta.LeavesSize; a++)
                    for (int b = -MyMeta.LeavesSize; b <= MyMeta.LeavesSize; b++)
                        for (int c = z - MyMeta.LeavesSize; c <= z+MyMeta.LeavesSize; c++)
                        {
                            if (!(a == 0 && b == 0 && c == z))
                            {
                                float DistanceToMid = Vector3.Distance(new Vector3(i, j+z, k), new Vector3(i + a, j + c, k + b));
                                if (DistanceToMid <= MyMeta.LeavesSize)
                                    MyChunk.UpdateBlockTypeSaveFile(i + a, j + c, k + b, MyMeta.LeafBlock);  // make tree
                            }
                        }
            }
        }
    }*/


/*float MyNoise = MyWorld.Amplitude*SimplexNoise.Noise.Generate(Position.x+i*MyWorld.Frequency,
                                                              Position.y+j*MyWorld.Frequency,
                                                              Position.z+k*MyWorld.Frequency);
MyNoise = (MyNoise+(1f-(j+1)/Chunk.ChunkSize)+
           ((Vector3.Distance(new Vector3(i,Chunk.ChunkSize-1,k),new Vector3(i,j,k)))/Chunk.ChunkSize))/3f;	// 0 - 1 * 0 to 8

if (MyNoise > 0.5f)
    UpdateBlockType(i,j,k, 1);*/
