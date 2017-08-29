using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Zeltex.Voxels
{
    /// <summary>
    /// The mode of the voxel model handler
    /// </summary>
    public enum VoxelModelHandleMode
    {
        None,
        Verts,
        Lines,
        Faces,
        Painting
    }
    /// <summary>
    /// Handles polygonal editing
    /// TODO: Move functions from voxelPolygonViewer into this
    /// </summary>
    [ExecuteInEditMode]
    public class VoxelModelHandle : MonoBehaviour
    {
        public VoxelModel MyModel;
        // the index of the loaded texture map
        public int TextureMapIndex;
        public UnityEvent OnUpdatedModelEvent;
        private VoxelModelHandleMode HandlerMode = 0;

        //public UVViewer MyUVViewer;
        //public PolygonMaker MyMaker;
        public Material VertMaterial;
        public Material FaceMaterial;
        //private int HandlerMode = 0;
        private Color32 NormalFaceColor = new Color32(0, 255, 76, 11);
        private Color32 SelectedFaceColor = new Color32(255, 76, 76, 68);
        private float VertSize = 0.03f;
        private string LoadedModelName;
        //private VoxelModel LoadedModel;
        private int LoadedTextureMap;
        private List<GameObject> MyHandlers = new List<GameObject>();
        private List<GameObject> SelectedHandlers = new List<GameObject>();
        [Header("Actions")]
        public bool IsLoadPolygon;
        public int ActionPolygonIndex;
        public int ActionTextureMapIndex;
        public bool IsSelectFace;
        public bool IsApplyVertHandlers;
        private Guis.UVViewer MyUVViewer;
        private Mesh MyMesh;

        public Mesh GetMesh()
        {
            return MyMesh;
        }

        private void Start()
        {
            MyModel.GenerateCubeMesh();
        }

        private void Update()
        {
            if (transform.childCount > 0)
            {
                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    // move random vertex up
                    MoveHandler(transform.GetChild(Random.Range(0, transform.childCount - 1)).gameObject, new Vector3(0, 0.1f, 0));
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    // move random vertex down
                    MoveHandler(transform.GetChild(Random.Range(0, transform.childCount - 1)).gameObject, new Vector3(0, -0.1f, 0));
                }
            }
            if (IsLoadPolygon)
            {
                IsLoadPolygon = false;
                LoadVoxelMesh(VoxelManager.Get().GetModel(ActionPolygonIndex), ActionTextureMapIndex);
            }
            if (IsSelectFace || Input.GetKeyDown(KeyCode.Alpha9))
            {
                IsSelectFace = false;
                if (MyHandlers.Count > 0)
                {
                    SelectHandler(MyHandlers[Random.Range(0, MyHandlers.Count - 1)]);
                }
            }
            if (IsApplyVertHandlers)
            {
                IsApplyVertHandlers = false;
                MyModel.UpdateWithPositions(MyHandlers);
                RefreshMesh();
            }
        }

        public void RefreshMesh()
        {
            LoadVoxelMesh(MyModel, TextureMapIndex);
        }

        /// <summary>
        /// Reloads the mesh of the model
        /// </summary>
        public void OnMassUpdatedModel()
        {
            LoadVoxelMesh(MyModel, TextureMapIndex);
        }
        /*protected override void HandleInput()
        {
            base.HandleInput();
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                UpdateHandlerMode(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                UpdateHandlerMode(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                UpdateHandlerMode(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                UpdateHandlerMode(3);
            }
        }*/

        #region Gizmos

        /// <summary>
        /// Clear all the mesh handlers as well as the references to them
        /// </summary>
        public void ClearHandlers()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
            MyHandlers.Clear();
            SelectedHandlers.Clear();
        }

        /// <summary>
        /// Updates the model handling mode
        /// </summary>
        public void UpdateHandlerMode(VoxelModelHandleMode NewMode)
        {
            if (HandlerMode != NewMode)
            {
                HandlerMode = NewMode;
                ClearHandlers();
                if (HandlerMode == VoxelModelHandleMode.None)
                {
                }
                if (HandlerMode == VoxelModelHandleMode.Verts)
                {
                    GenerateVertHandlers();
                }
                if (HandlerMode == VoxelModelHandleMode.Lines)
                {
                    GenerateLineHandlers();
                }
                if (HandlerMode == VoxelModelHandleMode.Faces)
                {
                    GenerateFaceHandlers();
                }
            }
        }

        #endregion

        #region VertHandlers

        /// <summary>
        /// Generates vert handlers for each vertex
        /// </summary>
        private void GenerateVertHandlers()
        {
            List<Vector3> MyVerts = MyModel.GetAllVerts();
            for (int i = 0; i < MyVerts.Count; i++)
            {
                GenerateVertHandler(MyVerts[i] - new Vector3(0.5f, 0.5f, 0.5f));
            }
        }

        /// <summary>
        /// Generates a new vert handler at a position
        /// </summary>
        public void GenerateVertHandler(Vector3 SpawnPosition)
        {
            // check if position already exist
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).transform.position == SpawnPosition)
                {
                    return;
                }
            }
            GameObject MyVertHandler = GameObject.CreatePrimitive(PrimitiveType.Cube);
            MyVertHandler.layer = gameObject.layer;
            MyVertHandler.name = "VertHandler_" + MyHandlers.Count;
            MyHandlers.Add(MyVertHandler);
            MyVertHandler.GetComponent<MeshRenderer>().material = VertMaterial;
            MyVertHandler.GetComponent<MeshRenderer>().material.color = NormalFaceColor;
            SpawnPosition = transform.TransformPoint(SpawnPosition);
            MyVertHandler.transform.position = SpawnPosition;
            MyVertHandler.transform.rotation = transform.rotation;
            MyVertHandler.transform.localScale = new Vector3(VertSize, VertSize, VertSize);
            MyVertHandler.transform.SetParent(transform);
        }
        #endregion

        #region LineHandlers
        /// <summary>
        /// Generates line handlers
        /// </summary>
        private void GenerateLineHandlers()
        {
            List<Vector3> MyVerts = MyModel.GetAllVerts();
            for (int i = 0; i < MyVerts.Count; i += 3)
            {
                Vector3 Position1 = MyVerts[i];
                Vector3 Position2 = MyVerts[i + 1];
                Vector3 Position3 = MyVerts[i + 2];
                GenerateLineHandler(Position1, Position2);
                GenerateLineHandler(Position2, Position3);
                GenerateLineHandler(Position3, Position1);
            }
        }

        /// <summary>
        /// Spawns a line handler
        /// </summary>
        /// <param name="Position1"></param>
        /// <param name="Position2"></param>
        private void GenerateLineHandler(Vector3 Position1, Vector3 Position2)
        {

        }
        #endregion

        #region FaceHandlers

        /// <summary>
        /// Generate handlers to select polygon faces!
        /// </summary>
        private void GenerateFaceHandlers()
        {
            ClearHandlers();
            // SelectedIndex = Mathf.Clamp(SelectedIndex, 0, MyWorld.MyModels.Count-1);
            //VoxelModel MyModel = MyVoxelManager.GetModel(LoadedModelName);
            List<Vector3> MyVerts = MyModel.GetAllVerts();
            for (int i = 0; i < MyVerts.Count; i += 4)
            {
                Vector3 Position1 = MyVerts[i] - new Vector3(0.5f, 0.5f, 0.5f);
                Vector3 Position2 = MyVerts[i + 1] - new Vector3(0.5f, 0.5f, 0.5f);
                Vector3 Position3 = MyVerts[i + 2] - new Vector3(0.5f, 0.5f, 0.5f);
                Vector3 Position4 = MyVerts[i + 3] - new Vector3(0.5f, 0.5f, 0.5f);
                GenerateFaceHandler(Position1, Position2, Position3, Position4);
            }
        }

        /// <summary>
        /// Create a mesh slightly facing away from the mesh
        /// Give it a special material
        /// </summary>
        private void GenerateFaceHandler(Vector3 Position1, Vector3 Position2, Vector3 Position3, Vector3 Position4)
        {
            GameObject NewFaceHandler = new GameObject();
            NewFaceHandler.name = "FaceHandler_" + MyHandlers.Count;
            MyHandlers.Add(NewFaceHandler);
            NewFaceHandler.layer = gameObject.layer;
            MeshRenderer MyMeshRenderer = NewFaceHandler.AddComponent<MeshRenderer>();
            MyMeshRenderer.material = FaceMaterial;
            MeshFilter MyMeshFilter = NewFaceHandler.AddComponent<MeshFilter>();
            MyMeshFilter.sharedMesh = new Mesh();
            MeshCollider MyMeshCollider = NewFaceHandler.AddComponent<MeshCollider>();
            MyMeshCollider.sharedMesh = MyMeshFilter.mesh;
            List<Vector3> MyVerts = new List<Vector3>();
            MyVerts.Add(Position1);
            MyVerts.Add(Position2);
            MyVerts.Add(Position3);
            MyVerts.Add(Position4);
            MyMeshFilter.mesh.vertices = MyVerts.ToArray();
            List<int> MyTriangles = new List<int>();
            MyTriangles.Add(0);
            MyTriangles.Add(1);
            MyTriangles.Add(2);
            MyTriangles.Add(0);
            MyTriangles.Add(2);
            MyTriangles.Add(3);
            MyMeshFilter.mesh.triangles = MyTriangles.ToArray();

            MyMeshFilter.mesh.RecalculateNormals();
            NewFaceHandler.transform.SetParent(transform);
            NewFaceHandler.transform.localPosition = Vector3.zero;
            NewFaceHandler.transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Has selected a face if the selected handlers count is greater then zero and if the mode is face mode
        /// </summary>
        public bool HasFaceSelected()
        {
            if (SelectedHandlers.Count > 0 && HandlerMode == VoxelModelHandleMode.Faces)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Loading
        /// <summary>
        /// Invoked when the properties of the model change
        /// </summary>
        private void OnUpdatedModel()
        {
            OnUpdatedModelEvent.Invoke();
        }

        /// <summary>
        /// Load in the voxel mesh
        /// </summary>
        public void LoadVoxelMesh(VoxelModel MyModel, int NewTextureMapIndex, bool IsRefreshHandlers)
        {
            LoadedModelName = MyModel.Name;
            TextureMapIndex = NewTextureMapIndex;
            //Debug.LogError("VoxelModelHandle [" + name + "] is loading [" + MyModel.Name + "] with texture map [" + TextureMapIndex + "]");
            List<string> SelectedHandlerNames = new List<string>();
            if (IsRefreshHandlers)
            {
                for (int i = 0; i < SelectedHandlers.Count; i++)
                {
                    SelectedHandlerNames.Add(SelectedHandlers[i].name);
                }
                UpdateHandlerMode(HandlerMode);
            }
            LoadedTextureMap = TextureMapIndex;
            UpdateWithSingleVoxelMesh(gameObject, MyModel.Name, TextureMapIndex);
            // after loaded - check if any handlers are out of bound, and reassign them if they arn't
            if (IsRefreshHandlers && HandlerMode == VoxelModelHandleMode.Verts)
            {
                OnLoadedVertMode();
            }
        }
        /// <summary>
        /// Clear the mesh!
        /// </summary>
        public void ClearMesh()
        {
            ClearHandlers();
            MeshFilter MyMeshFilter = gameObject.GetComponent<MeshFilter>();
            if (MyMeshFilter)
            {
                if (MyMeshFilter.sharedMesh == null)
                {
                    MyMeshFilter.sharedMesh = new Mesh(); // clear mesh
                }
                else
                {
                    MyMeshFilter.sharedMesh.Clear();
                }
            }
        }

        private void OnLoadedVertMode()
        {
            /*int VertCount = MyModel.GetAllVerts().Count;
            for (int i = 0; i < SelectedHandlerNames.Count; i++)
            {
                int VertIndex = HandlerNameToIndex(SelectedHandlerNames[i]);
                if (VertIndex >= 0 && VertIndex < VertCount)
                {
                    string MyName = "VertHandler_" + VertIndex;
                    for (int j = 0; j < MyHandlers.Count; j++)
                    {
                        if (MyHandlers[j].name == MyName)
                        {
                            SelectHandler(MyHandlers[j]);
                            break;
                        }
                    }
                }
            }*/
        }

        /// <summary>
        /// Load a new mesh
        /// </summary>
        public void LoadVoxelMesh(VoxelModel NewModel, int NewTextureMapIndex = 0)
        {
            MyModel = NewModel;
            TextureMapIndex = NewTextureMapIndex;
            if (MyModel != null)
            {
                //Debug.LogError("Loading Model: " + MyModel.Name + " : " + TextureMapIndex);
                MyModel.GenerateSolidity();
                LoadVoxelMesh(MyModel, TextureMapIndex, true);
            }
            else
            {
                Debug.LogError("Model is null.");
                ClearMesh();
            }
        }

        /// <summary>
        /// Update our mesh object with model and a texturemap
        /// </summary>
        public void UpdateWithSingleVoxelMesh(GameObject MyObject, string ModelIndex, int TextureIndex)
        {
            UpdateWithSingleVoxelMesh(MyObject, ModelIndex, TextureIndex, Color.white);
        }

        /// <summary>
        /// Update our mesh object with model and a texturemap and a colour
        /// </summary>
        public void UpdateWithSingleVoxelMesh(GameObject MyObject, string ModelIndex, int TextureIndex, Color32 MyTint)
        {
            if (gameObject == MyObject)
            {
                if (MyMesh)
                {
                    Destroy(MyMesh);
                }
            }
            Mesh MyCombinedMesh = GetSingleVoxelMesh(ModelIndex, TextureIndex, MyTint);
            // Debug.LogError("Inside [UpdateWithSingleVoxelMesh] with " + MyCombinedMesh.vertexCount + " vertex count. For: " + MyObject.name);
            MeshFilter MyFilter = MyObject.GetComponent<MeshFilter>();
            if (MyFilter)
            {
                MyFilter.sharedMesh = null;
                MyFilter.sharedMesh = MyCombinedMesh;
            }
            MeshCollider MyCollider = MyObject.GetComponent<MeshCollider>();
            if (MyCollider)
            {
                MyCollider.sharedMesh = null;
                MyCollider.sharedMesh = MyCombinedMesh;
            }
            MyMesh = MyCombinedMesh;
        }

        /// <summary>
        /// Returns the mesh using a meta
        /// </summary>
        public Mesh GetSingleVoxelMesh(int MetaIndex)
        {
            VoxelMeta MyMeta = VoxelManager.Get().GetMeta(MetaIndex);
            return GetSingleVoxelMesh(MyMeta.ModelID, MyMeta.TextureMapID, Color.white);
        }
        /// <summary>
        /// Using Meta Index instead of model and texture index
        /// </summary>
        public void UpdateWithSingleVoxelMesh(GameObject MyObject, int MetaIndex, Color32 MyTint)
        {
            VoxelMeta MyMeta = VoxelManager.Get().GetMeta(MetaIndex);
            if (MyMeta != null)
            {
                UpdateWithSingleVoxelMesh(MyObject, MyMeta.ModelID, MyMeta.TextureMapID, MyTint);
            }
        }

        /// <summary>
        /// Gets the mesh of a single voxel using the MyMetas
        /// </summary>
        public Mesh GetSingleVoxelMesh(string ModelName, int TextureIndex, Color32 ColorTint)
        {
            Mesh MyMesh = new Mesh();
            VoxelModel MyModel = VoxelManager.Get().GetModel(ModelName);
            if (MyModel != null)
            {
                MeshData MyMeshMyMetas = MyModel.GetCombinedMesh(TextureIndex);
                // Add in texture map uvs!
                // centre the mesh
                for (int i = 0; i < MyMeshMyMetas.Verticies.Count; i++)
                {
                    MyMeshMyMetas.Verticies[i] -= new Vector3(0.5f, 0.5f, 0.5f);
                }
                MyMesh.vertices = MyMeshMyMetas.GetVerticies();
                MyMesh.triangles = MyMeshMyMetas.GetTriangles();
                if (MyMeshMyMetas.GetTextureCoordinates().Length == MyMesh.vertices.Length)
                {
                    MyMesh.uv = MyMeshMyMetas.GetTextureCoordinates();
                }

                if (ColorTint != Color.white)
                {
                    MyMeshMyMetas.Colors.Clear();
                    for (int i = 0; i < MyMesh.vertices.Length; i++)
                    {
                        MyMeshMyMetas.Colors.Add(ColorTint);
                    }
                }
                MyMesh.colors32 = MyMeshMyMetas.Colors.ToArray();
                MyMesh.RecalculateNormals();
            }
            else
            {
                Debug.LogError("Could not find: " + ModelName);
            }
            return MyMesh;
        }
        #endregion

        #region Selection

        /// <summary>
        /// Update the vertexes! Called when moved a vertex handler!
        /// </summary>
        public void OnMoveGameObject(GameObject MovedObject, Vector3 DifferencePosition)
        {
            if (MovedObject.name.Contains("VertHandler") && DifferencePosition != new Vector3(0, 0, 0))
            {
                //List<Vector3> MyVerts = LoadedModel.GetAllVerts();  // the mesh verts!
                for (int i = 0; i < SelectedHandlers.Count; i++)
                {
                    MoveHandler(SelectedHandlers[i], DifferencePosition);
                }
                //RefreshModel();
            }
            /*else if (SelectedObject.name.Contains("FaceHandler"))
            {
                SelectedObject.transform.localPosition = Vector3.zero;
            }*/
        }

        private void MoveHandler(GameObject MyHandler, Vector3 DifferencePosition)
        {
            MyHandler.transform.localPosition += DifferencePosition;
            int VertPosition = HandlerNameToIndex(MyHandler.name);
            MyModel.UpdateAtPosition(
                           VertPosition,
                           MyHandler.transform.localPosition + new Vector3(0.5f, 0.5f, 0.5f)
                           );
        }

        public void UpdateTextureCoordinate(int Index, Vector2 NewPosition)
        {
            MyModel.TextureMaps[LoadedTextureMap].Set(Index, NewPosition);
        }

        private int HandlerNameToIndex(string MyName)
        {
            return int.Parse(MyName.Split('_')[1]);
        }

        /// <summary>
        /// Called when raycast selects a new object!
        /// </summary>
        public void SelectHandler(GameObject SelectedObject, bool IsAreaSelect = false, float AreaSelectionRadius = 0.1f)
        {
            if (SelectedObject.name.Contains("FaceHandler") || SelectedObject.name.Contains("VertHandler"))
            {
                if (SelectedObject.name.Contains("FaceHandler"))
                {
                    SelectedObject.transform.localPosition = Vector3.zero;
                }
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    if (IsAreaSelect)
                    {
                        AreaSelectHandler(SelectedObject.transform.position, AreaSelectionRadius);
                    }
                    else
                    {
                        SelectSingleHandler(SelectedObject);
                    }
                }
                else
                {
                    DeselectHandlers();
                    if (IsAreaSelect)
                    {
                        AreaSelectHandler(SelectedObject.transform.position, AreaSelectionRadius);
                    }
                    else
                    {
                        SelectSingleHandler(SelectedObject);
                    }
                }
            }
        }

        private void AreaSelectHandler(Vector3 SelectPosition, float AreaSelectionRadius = 0.1f)
        {
            List<GameObject> HandlersToSelect = new List<GameObject>();
            for (int i = 0; i < MyHandlers.Count; i++)
            {
                float DistanceToSelected = Vector3.Distance(SelectPosition, MyHandlers[i].transform.position);
                if (DistanceToSelected <= AreaSelectionRadius)
                {
                    HandlersToSelect.Add(MyHandlers[i]);
                }
            }
            for (int i = 0; i < HandlersToSelect.Count; i++)
            {
                SelectSingleHandler(HandlersToSelect[i]);
            }
        }
        private void DeselectHandlers()
        {
            for (int i = SelectedHandlers.Count - 1; i >= 0; i--)
            {
                if (SelectedHandlers[i] == null)
                {
                    SelectedHandlers.RemoveAt(i);
                }
                else
                {
                    SelectedHandlers[i].GetComponent<MeshRenderer>().material.color = NormalFaceColor;
                }
            }
            SelectedHandlers.Clear();
        }
        private bool DeselectHandler(GameObject MyHandler)
        {
            for (int i = SelectedHandlers.Count - 1; i >= 0; i--)
            {
                if (SelectedHandlers[i] == MyHandler)
                {
                    SelectedHandlers[i].GetComponent<MeshRenderer>().material.color = NormalFaceColor;
                    SelectedHandlers.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public void SelectSingleHandler(GameObject MyHandler)
        {
            bool IsFound = DeselectHandler(MyHandler);
            if (IsFound == false)
            {
                SelectedHandlers.Add(MyHandler);
                MyHandler.GetComponent<MeshRenderer>().material.color = SelectedFaceColor;
                if (MyUVViewer)
                {
                    OnSelectedHandlerUVViewer(MyHandler);
                }
                /*List<int> Triangles = GetSelectedTriangles();
                List<Vector2> TextureCoordinates = LoadedModel.GetTextureMapCoordinates(LoadedTextureMap);
                for (int i = 0; i < Triangles.Count; i++)
                {
                   // LoadedModel.SetTextureMapTile(SelectedTextureMap, TextureName, Triangles[i]);
                }*/
            }
        }

        private void OnSelectedHandlerUVViewer(GameObject MyHandler)
        {
            if (MyHandler.name.Contains("VertHandler"))
            {
                List<int> MyIndexes = GetSelectedTextureIndexes();
                if (MyModel.TextureMaps.Count > 0)
                {
                    MyUVViewer.GenerateHandlers(
                        MyModel.GetUVs(
                            MyIndexes, LoadedTextureMap),
                            MyIndexes,
                            MyModel.TextureMaps[LoadedTextureMap].GetTilemapNames(MyIndexes));
                }
                else
                {
                    MyUVViewer.ClearHandlers();
                }
            }
            else if (MyHandler.name.Contains("FaceHandler"))
            {
                List<int> MyIndexes = GetSelectedTriangles();
                MyUVViewer.GenerateHandlers(
                    MyModel.GetUVs(
                        MyIndexes, LoadedTextureMap),
                        MyIndexes,
                        MyModel.TextureMaps[LoadedTextureMap].GetTilemapNames(MyIndexes));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private int GetSelectedTextureMap()
        {
            return TextureMapIndex;
        }
        #endregion
        
        #region GetSelected
        /// <summary>
        /// Returns the selected triangles
        /// </summary>
        public List<int> GetSelectedTriangles()
        {
            List<int> MyTriangles = new List<int>();
            for (int i = 0; i < SelectedHandlers.Count; i++)
            {
                string[] NameInput = SelectedHandlers[i].name.Split('_');
                int FaceIndex = int.Parse(NameInput[1]);
                for (int j = 0; j < 4; j++)
                {
                    MyTriangles.Add(FaceIndex * 4 + j);
                }
            }
            return MyTriangles;
        }

        /*private List<Vector2> GetSelectedTextureCoordinates()
        {
            List<Vector2> MyUVs = new List<Vector2>();
            List<Vector2> TotalUVs = LoadedModel.GetTextureMapCoordinates(LoadedTextureMap, new TileMapInfo(1, 1, 16, 16));
            //Debug.Log("Getting uvs with " + SelectedHandlers.Count + " Selected handlers");
            for (int i = 0; i < SelectedHandlers.Count; i++)
            {
                int MyVertIndex = HandlerNameToIndex(SelectedHandlers[i].name);
                MyUVs.Add(TotalUVs[MyVertIndex]);
                //Debug.Log("getting uv " + i + "  with " + MyUVs[MyUVs.Count-1].ToString());
            }
            return MyUVs;
        }*/

        private List<int> GetSelectedTextureIndexes()
        {
            List<int> MyUVs = new List<int>();

            List<Vector2> TotalUVs = MyModel.GetTextureMapCoordinates(LoadedTextureMap, new TileMap(1, 1, 16, 16));
            //Debug.Log("Getting uvs with " + SelectedHandlers.Count + " Selected handlers");
            for (int i = 0; i < SelectedHandlers.Count; i++)
            {
                int MyVertIndex = HandlerNameToIndex(SelectedHandlers[i].name);
                MyUVs.Add(MyVertIndex);
                //Debug.Log("getting uv " + i + "  with " + MyUVs[MyUVs.Count-1].ToString());
            }
            return MyUVs;
        }
        #endregion

        #region Utility

        /// <summary>
        /// Rotate UVs
        /// </summary>
        public void RotateUVs(int SideIndex)
        {
            //GetSelected().MyModels[SideIndex].RotateTextureCoordinates();
            // Update the selected face
            if (HasFaceSelected()) // Bam!
            {
                List<int> Triangles = GetSelectedTriangles();
                VoxelTextureMap MyTextureMap = MyModel.TextureMaps[GetSelectedTextureMap()];
                for (int i = 0; i < Triangles.Count; i += 4)
                {
                    List<Vector2> MyUVs = new List<Vector2>();
                    for (int j = 0; j < 4; j++)
                    {
                        MyUVs.Add(MyTextureMap.Coordinates[Triangles[i + j]].MyCoordinate);
                    }
                    MeshData.RotateTextureCoordinates(MyUVs);
                    for (int j = 0; j < 4; j++)
                    {
                        MyTextureMap.Coordinates[Triangles[i + j]].MyCoordinate = MyUVs[j];
                    }
                    //GetSelected().SetTextureMapTile(SelectedTextureMap, TextureName, Triangles[i]);
                }
            }
            //RefreshViewer();
            //UpdateStatistics();
        }
        #endregion

        #region GeneratePrimitives
        /// <summary>
        /// Generate a cube primitive
        /// </summary>
        public void UpdateToCube()
        {
            MyModel.GenerateCubeMesh();
            OnUpdatedModel();
            //RefreshViewer();
            //UpdateStatistics();
        }
        #endregion
    }

}