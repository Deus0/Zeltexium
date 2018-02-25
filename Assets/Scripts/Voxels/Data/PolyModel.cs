using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MakerGuiSystem;
using Zeltex;
using Newtonsoft.Json;

// To Do: Model specific rules - ie is model cube above, or model pillar? If so cull certain sides.
// Example 1 - if pillar is this model and the above one, cull top and bottom sides
// Example 2 - if pillar is this and cube is above, only cull the pillar top
// Example 3 - If pillar is this and pillar is also below and in front, make a mesh that extends to both of these (think of piping)

namespace Zeltex.Voxels
{

    /// <summary>
    /// Used for Caching Texture Coordinates
    /// </summary>
    [System.Serializable()]
    public class IndexedCoordinates : SerializableDictionaryBase<Int3, List<Vector2>> 
    {
        [JsonIgnore]
        public bool IsEditing;
    }

    /// <summary>
    /// The main polygonal mesh data for a voxel
    /// Also contains rules needed for placing in the voxel grid
    /// </summary>
    [System.Serializable]
	public class PolyModel : ElementCore
    {
        #region Variables
        //public string Name = "Empty";
        [JsonIgnore]
        public static int BufferLength = 0; // 1
        [JsonProperty]
        public List<MeshData> MyModels = new List<MeshData>();
        // Each model has multiple texture maps
        [JsonProperty]
		public List<PolyTextureMap> TextureMaps = new List<PolyTextureMap>();
        // Voxel Rules
        [JsonProperty]
        public bool[] Solidity = new bool[6];   // determine if its solid on sides

        // Ignoring
        [JsonIgnore]
        private TileMap CachedTilemap;
        [JsonIgnore]
        private IndexedCoordinates CachedCoordinates = new IndexedCoordinates();
        [JsonIgnore]
        private List<Vector3> AllVerts = new List<Vector3>();
        [JsonIgnore]
        private List<int> AllTriangles = new List<int>();
        [JsonIgnore]
        private List<Vector2> MyTextureCoordinates = new List<Vector2>();
        [JsonIgnore]
        private int i;
        [JsonIgnore]
        private int IndexBegin;
        [JsonIgnore]
        private int EndIndex;
        #endregion

        #region Spawning
        public PolyModelHandle MyPoly;

        public override void Spawn()
        {
            if (MyPoly == null)
            {
                GameObject NewItem = new GameObject();
                NewItem.name = Name;// + "-Handler";
                MyPoly = NewItem.AddComponent<PolyModelHandle>();
                MyPoly.LoadVoxelMesh(this, 0);
            }
            else
            {
                Debug.LogError("Trying to spawn when handler already exists for: " + Name);
            }
        }

        public override void DeSpawn()
        {
            if (MyPoly)
            {
                MyPoly.gameObject.Die();
            }
        }

        public override bool HasSpawned()
        {
            return (MyPoly != null);
        }
        #endregion

        #region Initiation
        public PolyModel()
        {
            //TextureMaps.Add(new PolyTextureMap());
            GenerateCubeMesh();
            AllSidesSolid();
            //AddNewTextureMap();
            //GenerateTextureMap("");
        }
        #endregion

        #region TextureMap

        public bool RenameTexture(string OldTextureName, string NewTextureName)
        {
            bool DidUpdate = false;
            Debug.Log("Renaming texture: " + OldTextureName + " to " + NewTextureName);
            for (int i = 0; i < TextureMaps.Count; i++)
            {
                for (int j = 0; j < TextureMaps[i].Coordinates.Count; j++)
                {
                    if (Zeltex.Util.ScriptUtil.RemoveWhiteSpace(TextureMaps[i].Coordinates[j].TileName) == Zeltex.Util.ScriptUtil.RemoveWhiteSpace(OldTextureName))
                    {
                        TextureMaps[i].Coordinates[j].TileName = NewTextureName;
                        DidUpdate = true;
                    }
                }
            }
            if (DidUpdate)
            {
                OnModified();
            }
            return DidUpdate;
        }

        /// <summary>
        /// From triangle indexes, returns a list of the vectors!
        /// </summary>
        public List<Vector2> GetUVs(List<int> MyIndexes, int TextureMapIndex)
        {
            List<Vector2> MyUVs = new List<Vector2>();
            List<Vector2> TotalUVs = GetTextureMapCoordinates(TextureMapIndex, new TileMap(1, 1, 16, 16));
            for (int i = 0; i < MyIndexes.Count; i++)
            {
                if (MyIndexes[i] >= 0 && MyIndexes[i] < TotalUVs.Count)
                {
                    MyUVs.Add(TotalUVs[MyIndexes[i]]);
                }
                else
                {
                    Debug.LogError("Index " + MyIndexes[i] + " is out of bounds: " + TotalUVs.Count);
                }
            }
            return MyUVs;
        }

        /// <summary>
        /// Create a new texture map and add it to the list
        /// </summary>
        public void NewTextureMap()
        {
            TextureMaps.Add(GenerateTextureMap(""));
            OnModified();
        }

        public void SetTextureMapTile(int TextureMapIndex, string TextureName)
        {
            TextureMaps[TextureMapIndex].SetName(TextureName); // set first tile to new name
            OnModified();
        }

        public void SetTextureMapTile(int TextureMapIndex, string TextureName, int VertIndex)
        {
            TextureMaps[TextureMapIndex].SetName(TextureName, VertIndex); // set first tile to new name
            OnModified();
        }

        private void AddNewTextureMap()
        {
			PolyTextureMap NewMap = new PolyTextureMap();
            //MeshData MyMesh = GetCombinedMesh(0);
            TextureMaps.Add(NewMap);
            OnModified();
        }

        /// <summary>
        /// Generate a new texture map for the model
        /// </summary>
		private PolyTextureMap GenerateTextureMap(string TileName)
        {
            //NewMap = GenerateTextureMap(NewMap, "");
			PolyTextureMap NewMap = new PolyTextureMap(); // clear the map
            MeshData MyMesh = GetCombinedMesh(0);
            for (int i = 0; i < MyMesh.Verticies.Count; i++)
            {
                NewMap.Add(new Vector2(), TileName);
            }
            for (int i = 0; i < 6; i++)
            {
                List<Vector2> NewQuads = new List<Vector2>();
                MeshData.AddQuadUVs(
                        0,  // tile index - TileIndex
                        1,  // tile resolution - 8
                        World.TextureResolution,  // texture resolution
                        i,  // side index
                        NewQuads  // coordinates to add to GetCoordinates(TileName)
                        );
                for (int j = 0; j < 4; j++)
                {
                    int k = i * 4 + j;
                    NewMap.Set(k, NewQuads[j]);
                }
            }
            return NewMap;
        }

        /// <summary>
        /// Updates the texture maps uvs to a quad
        /// </summary>
        public void GenerateTextureMap(string TileName, int Index) //int SideIndex, 
        {
            if (TextureMaps.Count == 0)
            {
                AddNewTextureMap();
            }
            TextureMaps[Index] = GenerateTextureMap(TileName);
            OnModified();
        }

        [JsonIgnore]
        private Int3 CachedPosition;
        /// <summary>
        /// Get the uvs for the entire model!
        /// </summary>
        public List<Vector2> GetTextureMapCoordinates(int TextureMapIndex, TileMap MyInfo)
        {
            if (TextureMapIndex >= 0 && TextureMapIndex < TextureMaps.Count)
            {
                if (CachedTilemap != MyInfo)
                {
                    // When new tilemap, clear cached polymodel texture coordinates
                    CachedTilemap = MyInfo;
                    CachedCoordinates.Clear();
                }
                CachedPosition = new Int3(-1, TextureMapIndex, 0);
                if (CachedCoordinates.ContainsKey(CachedPosition) == false)
                {
                    // Get new coordinates here
                    CachedCoordinates.Add(CachedPosition, TextureMaps[TextureMapIndex].GetCoordinates(MyInfo));
                }
                return CachedCoordinates[CachedPosition];
            }
            else
            {
                return new List<Vector2>();
            }
        }

        /// <summary>
        /// Get the UVs for a particular side
        /// TODO: Store UVs for a particular side instead of storing all of them
        /// </summary>
        public List<Vector2> GetTextureMapCoordinates(int TextureMapIndex, int SideIndex, TileMap MyInfo)
        {
            if (TextureMapIndex >= 0 && TextureMapIndex < TextureMaps.Count)
            {
                if (CachedTilemap != MyInfo)
                {
                    // When new tilemap, clear cached polymodel texture coordinates
                    CachedTilemap = MyInfo;
                    CachedCoordinates.Clear();
                    //Debug.LogError("Resetting Cached Coordinates! for " + Name);
                }
                CachedPosition = new Int3(SideIndex, TextureMapIndex, 0);
                if (CachedCoordinates.ContainsKey(CachedPosition) == false)
                {
                    MyTextureCoordinates = TextureMaps[TextureMapIndex].GetCoordinates(MyInfo);
                    if (MyTextureCoordinates.Count == 0)
                    {
                        //Debug.LogError("Generating Coordinates.");
                        for (i = 0; i < GetAllVerts().Count; i++)
                        {
                            MyTextureCoordinates.Add(new Vector2());
                        }
                    }
                    List<Vector2> MyTextureCoordinates2 = new List<Vector2>();
                    IndexBegin = 0;
                    if (SideIndex != 0)
                    {
                        for (i = 0; i < SideIndex; i++)
                        {
                            IndexBegin += MyModels[i].Verticies.Count;
                        }
                    }
                    EndIndex = IndexBegin + MyModels[SideIndex].Verticies.Count;
                    //Debug.Log("Adding Texture UVs: " + IndexBegin + " to " + EndIndex + " out of " + MyTextureCoordinates.Count);
                    for (i = IndexBegin; i < EndIndex; i++)
                    {
                        MyTextureCoordinates2.Add(MyTextureCoordinates[i]);
                    }
                    // Get new coordinates here
                    CachedCoordinates.Add(CachedPosition,  MyTextureCoordinates2);
                }
                return CachedCoordinates[CachedPosition];
            }
            else
            {
                //Debug.LogError("Error getting texture coordinates from model: " + Name + ", " + TextureMapIndex + " out of " + TextureMaps.Count);
                return new List<Vector2>();
            }
        }
        #endregion

        #region CombinedMesh

        public MeshData GetCombinedMesh(int TextureMapIndex)
        {
            //TextureMaker MyTextureMaker = TextureMaker.Get();
            //MeshData NewMesh = new MeshData();
            /*for (int SideIndex = 0; SideIndex < MyModels.Count; SideIndex++)
            {
                NewMeshData.Add(ref new MeshData(MyModels[SideIndex]));
                NewMesh.Add(ref NewMeshData);
            }*/
            MeshData NewMesh = new MeshData(MyModels);
            NewMesh.TextureCoordinates.AddRange(GetTextureMapCoordinates(TextureMapIndex, new TileMap()));
            return NewMesh;
        }

        public void UseMesh(Mesh MyMesh)
        {
            MyModels[0] = new MeshData(MyMesh);
            for (int i = 1; i < MyModels.Count; i++)
            {
                MyModels[i].Clear();
            }
            GenerateSolidity();
        }
        #endregion

        #region GettersAndSetters

        public List<Vector3> GetAllVerts()
        {
            if (AllVerts.Count == 0)
            {
                for (int i = 0; i < MyModels.Count; i++)
                {
                    AllVerts.AddRange(MyModels[i].Verticies);
                }
            }
            return AllVerts;
        }

		public List<int> GetAllTriangles()
		{
			if (AllTriangles.Count == 0)
			{
				for (int i = 0; i < MyModels.Count; i++)
				{
					AllTriangles.AddRange(MyModels[i].Triangles);
				}
			}
			return AllTriangles;
		}
        #endregion

        #region Files

        /*public override string GetScript()
        {
            List<string> MyScript = new List<string>();
            for (int i = 0; i < MyModels.Count; i++)
            {
                MyScript.Add("/BeginMesh");
                MyScript.AddRange(MyModels[i].GetScript());
                MyScript.Add("/EndMesh");
            }
            for (int i = 0; i < TextureMaps.Count; i++)
            {
                MyScript.Add("/BeginTextureMap");
                MyScript.AddRange(TextureMaps[i].GetScript());
                MyScript.Add("/EndTextureMap");
            }
            return Zeltex.Util.FileUtil.ConvertToSingle(MyScript);
        }

        public void RunScript(List<string> MyScript)
        {
            RunScript(Util.FileUtil.ConvertToSingle(MyScript));
        }

        public override void RunScript(string MyScript)
        {
            List<string> MyScriptList = Zeltex.Util.FileUtil.ConvertToList(MyScript);
            MyModels.Clear();
            TextureMaps.Clear();
            for (int i = 0; i < MyScriptList.Count; i++)
            {
                if (MyScriptList[i] == "/BeginMesh")
                {
                    int BeginIndex = i + 1;
                    for (int j = BeginIndex; j < MyScriptList.Count; j++)
                    {
                        if (MyScriptList[j] == "/EndMesh")
                        {
                            int EndIndex = j - 1;
                            int RangeCount = EndIndex - BeginIndex + 1;
                            //Debug.LogError("Getting Range " + (RangeCount + BeginIndex) + " <= " + MyScript.Count);
                            MeshData NewMesh = new MeshData();
                            if (RangeCount > 0)
                            {
                                List<string> MeshScript = MyScriptList.GetRange(BeginIndex, RangeCount);// start after end mesh
                                NewMesh.RunScript(MeshScript);
                            }
                            MyModels.Add(NewMesh);
                            i = j;
                            break;
                        }
                    }
                }
                if (MyScriptList[i] == "/BeginTextureMap")
                {
                    int BeginIndex = i + 1;
                    for (int j = BeginIndex; j < MyScriptList.Count; j++)
                    {
                        if (MyScriptList[j] == "/EndTextureMap")
                        {
                            int EndIndex = j - 1;
                            int RangeCount = EndIndex - BeginIndex + 1;
                            PolyTextureMap MyTextureMap = new PolyTextureMap();
                            if (RangeCount > 0)
                            {
                                List<string> TextureMapScript = MyScriptList.GetRange(BeginIndex, RangeCount);
                                MyTextureMap.RunScript(TextureMapScript);
                            }
                            TextureMaps.Add(MyTextureMap);
                            i = j;
                            break;
                        }
                    }
                }
            }
        }*/
        #endregion

        public bool UpdateAtPosition(Vector3 OldPosition, Vector3 NewPosition)
        {
            bool DidUpdate = false;
            //Debug.LogError("Updating Model: " + OldPosition.ToString() + " to " + NewPosition.ToString());
            for (int i = 0; i < MyModels.Count; i++)
            {
                for (int j = 0; j < MyModels[i].Verticies.Count; j++)
                {
                    if (MyModels[i].Verticies[j] == OldPosition)
                    {
                        MyModels[i].Verticies[j] = NewPosition;
                        DidUpdate = true;
                    }
                }
            }
            return DidUpdate;
        }

        public bool UpdateAtPosition(int VertPosition, Vector3 NewPosition)
        {
            bool DidUpdate = false;
            int VertCount = 0;
            //Debug.LogError("Updating Model: " + OldPosition.ToString() + " to " + NewPosition.ToString());
            for (int i = 0; i < MyModels.Count; i++)
            {
                for (int j = 0; j < MyModels[i].Verticies.Count; j++)
                {
                    if (VertCount == VertPosition)
                    {
                        MyModels[i].Verticies[j] = NewPosition;
                        return true;
                    }
                    VertCount++;
                }
            }
            return DidUpdate;
        }

        public void UpdateWithPositions(List<GameObject> Objects)
        {
            List<Vector3> Positions = new List<Vector3>();
            for (int i = 0; i < Objects.Count; i++)
            {
                Positions.Add(Objects[i].transform.localPosition + new Vector3(0.5f, 0.5f, 0.5f));
            }
            UpdateWithPositions(Positions);
        }

        public void UpdateWithPositions(List<Vector3> Positions)
        {
            if (Positions.Count == AllVerts.Count)
            {
                int PositionsIndex = 0;
                for (int i = 0; i < MyModels.Count; i++)
                {
                    for (int j = 0; j < MyModels[i].Verticies.Count; j++)
                    {
                        MyModels[i].Verticies[j] = Positions[PositionsIndex];
                        PositionsIndex++;
                    }
                }
            }
            OnModified();
        }

        //NewMeshData.AddQuadUVs(TextureIndex,8, World.TextureResolution, i);
        public bool IsSolid(int SideIndex)
        {
            return Solidity[SideIndex];
        }

        public void AllSidesSolid()
        {
            Solidity = new bool [6] { true, true, true, true, true, true };
        }
        public void Clear()
        {
            MyModels.Clear();
        }
        public MeshData GetModel(int SideIndex)
        {
            if (SideIndex >= 0 && SideIndex < MyModels.Count)
            {
                return MyModels[SideIndex];
            }
            else
            {
                return new MeshData();
            }
        }


        // If One of the sides is a complete plane that is parralel to grid lines, that side is solid
        // this needs to be dependent on other models - so the more models the more this will take
        // for ones that arn't generated - simply don't cull any sides
        public void GenerateSolidity()
        {
            List<MeshData> MyMeshes = GetCubeSides();
            // for all meshes, see if it alligns with grid
            for (int i = 0; i < MyModels.Count; i++)
            {
                if (i == 2 || i == 3)
                {
                    if (i == 2)
                        Solidity[2] = AreMeshDataEqual(MyModels[3], MyMeshes[3]);
                    else
                        Solidity[3] = AreMeshDataEqual(MyModels[2], MyMeshes[2]);
                }
                else if (i == 4 || i == 5)
                {
                    if (i == 4)
                        Solidity[4] = AreMeshDataEqual(MyModels[5], MyMeshes[5]);
                    else
                        Solidity[5] = AreMeshDataEqual(MyModels[4], MyMeshes[4]);
                }
                else
                {
                    if (i == 0)
                        Solidity[0] = AreMeshDataEqual(MyModels[1], MyMeshes[1]);
                    else
                        Solidity[1] = AreMeshDataEqual(MyModels[0], MyMeshes[0]);
                    //Solidity[i] = AreMeshDataEqual(MyModels[i], MyMeshes[i]);
                }
            }
        }

        bool AreMeshDataEqual(MeshData MyData1, MeshData MyData2)
        {
            if (MyData1.Verticies.Count != MyData2.Verticies.Count)
                return false;
            if (MyData1.Triangles.Count != MyData2.Triangles.Count)
                return false;
            for (int i = 0; i < MyData1.Verticies.Count; i++)
            {
                if (MyData1.Verticies[i] != MyData2.Verticies[i])
                    return false;
            }
            for (int i = 0; i < MyData1.Triangles.Count; i++)
            {
                if (MyData1.Triangles[i] != MyData2.Triangles[i])
                    return false;
            }
            return true;
        }
        // Half the cube essentially
        public void GenerateTriangleMesh()
        {

        }

        public void MovePosition(Vector3 AdditionVector)
        {
            for (int i = 0; i < MyModels.Count; i++)
            {
                MyModels[i].AddToVertex(AdditionVector);
            }
        }

        public MeshData GetFace(Vector3 MyDifference)
        {
            return GetFace(MyDifference, Quaternion.identity);
        }
        public MeshData GetFace(Vector3 MyDifference, Quaternion MyRotation)
        {
            MeshData MyMeshData = new MeshData();
            float x = 0.5f;
            float y = 0.5f;
            float z = 0.5f;
            float Radius = 0.5f;
            MyMeshData.AddVertex(new Vector3(-Radius, 0, Radius));
            MyMeshData.AddVertex(new Vector3(Radius, 0, Radius));
            MyMeshData.AddVertex(new Vector3(Radius, 0, -Radius));
            MyMeshData.AddVertex(new Vector3(-Radius, 0, -Radius));
            MyMeshData.AddQuadTriangles();
            for (int i = 0; i < MyMeshData.Verticies.Count; i++)
            {
                MyMeshData.Verticies[i] = MyRotation * MyMeshData.Verticies[i];
            }
            for (int i = 0; i < MyMeshData.Verticies.Count; i++)
            {
                MyMeshData.Verticies[i] = MyMeshData.Verticies[i]+MyDifference+new Vector3(x, y, z);
            }
            return MyMeshData;
        }
        public List<MeshData> GetCubeSides()
        {
            List<MeshData> MyMeshes = new List<MeshData>();
            // Create mesh sides
            MeshData MeshFaceUp = GetFace(new Vector3(0, 0.5f, 0));
            MeshData MeshFaceDown = GetFace(new Vector3(0, -0.5f, 0), Quaternion.Euler(new Vector3(180, 0, 0)));
            MeshData MeshFaceNorth = GetFace(new Vector3(0, 0, 0.5f), Quaternion.Euler(new Vector3(90, 0, 0)));
            MeshData MeshFaceSouth = GetFace(new Vector3(0, 0, -0.5f), Quaternion.Euler(new Vector3(-90, 0, 0)));
            MeshData MeshFaceEast = GetFace(new Vector3(0.5f, 0, 0), Quaternion.Euler(new Vector3(0, 0, -90)));
            MeshData MeshFaceWest = GetFace(new Vector3(-0.5f, 0, 0), Quaternion.Euler(new Vector3(0, -0, 90)));
            // Add them in a list to use in the grid
            MyMeshes.Add(MeshFaceUp);
            MyMeshes.Add(MeshFaceDown);
            MyMeshes.Add(MeshFaceNorth);
            MyMeshes.Add(MeshFaceSouth);
            MyMeshes.Add(MeshFaceEast);
            MyMeshes.Add(MeshFaceWest);
            return MyMeshes;
        }
		// i should switch the 6 sides to just a plane being rotated
		public void GenerateCubeMesh() 
		{
            MyModels.Clear();
            // Create mesh sides
            MeshData MeshFaceUp = GetFace(new Vector3(0, 0.5f, 0));
            MeshData MeshFaceDown = GetFace(new Vector3(0, -0.5f, 0), Quaternion.Euler(new Vector3(180,0,0)));
            MeshData MeshFaceNorth = GetFace(new Vector3(0, 0, 0.5f), Quaternion.Euler(new Vector3(90, 0, 0)));
            MeshData MeshFaceSouth = GetFace(new Vector3(0, 0, -0.5f), Quaternion.Euler(new Vector3(-90, 0, 0)));
            MeshData MeshFaceEast = GetFace(new Vector3(0.5f, 0, 0), Quaternion.Euler(new Vector3(0, 0, -90)));
            MeshData MeshFaceWest = GetFace(new Vector3(-0.5f, 0, 0), Quaternion.Euler(new Vector3(0, -0, 90)));
            // Add them in a list to use in the grid
            MyModels.Add(MeshFaceUp);
            MyModels.Add(MeshFaceDown);
            MyModels.Add(MeshFaceNorth);
            MyModels.Add(MeshFaceSouth);
            MyModels.Add(MeshFaceEast);
            MyModels.Add (MeshFaceWest);
            OnModified();
        }


        public void GenerateSquashedCubeMesh(Vector3 Scale) // scale it inwards
        {
            Scale /= 2f;
            MeshData MeshFaceUp = new MeshData();
            float x, y, z;
            x = 0; y = 0; z = 0;
            x += 0.5f; y += 0.5f; z += 0.5f;
            //float Radius = 0.5f;
            // face up
            MeshFaceUp.AddVertex(new Vector3(x - Scale.x, y + Scale.y, z + Scale.z));
            MeshFaceUp.AddVertex(new Vector3(x + Scale.x, y + Scale.y, z + Scale.z));
            MeshFaceUp.AddVertex(new Vector3(x + Scale.x, y + Scale.y, z - Scale.z));
            MeshFaceUp.AddVertex(new Vector3(x - Scale.x, y + Scale.y, z - Scale.z));
            MeshFaceUp.AddQuadTriangles();
            MyModels.Add(MeshFaceUp);

            MeshData MeshFaceDown = new MeshData();
            //public MeshData FaceDataDown(float x, float y, float z, MeshData meshData, int MaterialType, byte Brightness)
            {
                MeshFaceDown.AddVertex(new Vector3(x - Scale.x, y - Scale.y, z - Scale.z));
                MeshFaceDown.AddVertex(new Vector3(x + Scale.x, y - Scale.y, z - Scale.z));
                MeshFaceDown.AddVertex(new Vector3(x + Scale.x, y - Scale.y, z + Scale.z));
                MeshFaceDown.AddVertex(new Vector3(x - Scale.x, y - Scale.y, z + Scale.z));
                MeshFaceDown.AddQuadTriangles();
            }
            MyModels.Add(MeshFaceDown);

            MeshData MeshFaceNorth = new MeshData();
            //public MeshData FaceDataNorth(float x, float y, float z, MeshData meshData, int CycleNumber, byte Brightness)
            {
                MeshFaceNorth.AddVertex(new Vector3(x + Scale.x, y - Scale.y, z + Scale.z));
                MeshFaceNorth.AddVertex(new Vector3(x + Scale.x, y + Scale.y, z + Scale.z));
                MeshFaceNorth.AddVertex(new Vector3(x - Scale.x, y + Scale.y, z + Scale.z));
                MeshFaceNorth.AddVertex(new Vector3(x - Scale.x, y - Scale.y, z + Scale.z));
                MeshFaceNorth.AddQuadTriangles();
            }
            MyModels.Add(MeshFaceNorth);
            MeshData MeshFaceSouth = new MeshData();
            //public MeshData FaceDataSouth(float x, float y, float z, MeshData meshData, int CycleNumber, byte Brightness)
            {
                MeshFaceSouth.AddVertex(new Vector3(x - Scale.x, y - Scale.y, z - Scale.z));
                MeshFaceSouth.AddVertex(new Vector3(x - Scale.x, y + Scale.y, z - Scale.z));
                MeshFaceSouth.AddVertex(new Vector3(x + Scale.x, y + Scale.y, z - Scale.z));
                MeshFaceSouth.AddVertex(new Vector3(x + Scale.x, y - Scale.y, z - Scale.z));
                MeshFaceSouth.AddQuadTriangles();
            }
            MyModels.Add(MeshFaceSouth);

            MeshData MeshFaceEast = new MeshData();
            //public MeshData FaceDataEast(float x, float y, float z, MeshData meshData, int CycleNumber, byte Brightness)
            {
                MeshFaceEast.AddVertex(new Vector3(x + Scale.x, y - Scale.y, z - Scale.z));
                MeshFaceEast.AddVertex(new Vector3(x + Scale.x, y + Scale.y, z - Scale.z));
                MeshFaceEast.AddVertex(new Vector3(x + Scale.x, y + Scale.y, z + Scale.z));
                MeshFaceEast.AddVertex(new Vector3(x + Scale.x, y - Scale.y, z + Scale.z));
                MeshFaceEast.AddQuadTriangles();
            }
            MyModels.Add(MeshFaceEast);

            MeshData MeshFaceWest = new MeshData();
            //public MeshData FaceDataWest(float x, float y, float z, MeshData meshData, int CycleNumber, byte Brightness)
            {
                MeshFaceWest.AddVertex(new Vector3(x - Scale.x, y - Scale.y, z + Scale.z));
                MeshFaceWest.AddVertex(new Vector3(x - Scale.x, y + Scale.y, z + Scale.z));
                MeshFaceWest.AddVertex(new Vector3(x - Scale.x, y + Scale.y, z - Scale.z));
                MeshFaceWest.AddVertex(new Vector3(x - Scale.x, y - Scale.y, z - Scale.z));
                MeshFaceWest.AddQuadTriangles();
            }
            MyModels.Add(MeshFaceWest);

            Solidity = new bool[6] { false, false, false, false, false, false};
        }
    }
}