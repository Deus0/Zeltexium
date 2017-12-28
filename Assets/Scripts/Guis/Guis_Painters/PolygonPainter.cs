using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zeltex.Voxels;
using Zeltex.Util;

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// The mode of the voxel model handler
    /// </summary>
    public enum PolygonEditMode
    {
        None,
        Link,
        Move,
        Rotate,
        Scale
    }
    /// <summary>
    /// Edit any polygon in the map
    /// </summary>
    public class PolygonPainter : GuiBasic
    {
        [Header("PolygonPainter")]
        [SerializeField]
        private IndexController TextureMapController;
        [SerializeField]
        private PolyModelHandle SelectedModel;
        // Raycasting
        private bool DidHitGui;
        private bool DidHitModel;
        private ObjectViewer MyViewer;
        private RaycastHit MyHit;
        private PolygonEditMode HandlerMode = 0;
        private GameObject MovingHandler;
        private float MoveDelta = 1f;
        private float MoveDistance = 2f;

        private void Awake()
        {
            InitiateTextureMapController();
        }

        private void Update()
        {
            FillAllContainers();
            Raycast();
            if (MovingHandler)
            {
                if (Input.GetMouseButton(0) && SelectedModel && MyViewer)
                {
                    //CurrentMousePosition = //MyViewer.GetLocalMousePosition(Input.mousePosition);
                    Ray MyRay;
                    MyViewer.GetRayInViewer(Input.mousePosition, out MyRay);
                    CurrentMousePosition = MyRay.origin + MoveDistance * MyRay.direction;
                    if (Input.GetMouseButtonDown(0))
                    {
                        LastMousePosition = CurrentMousePosition;
                    }
                    if (HandlerMode == PolygonEditMode.Move)//Input.GetKeyDown(KeyCode.LeftControl))
                    {
                        SelectedModel.OnMoveGameObject(MovingHandler, (CurrentMousePosition - LastMousePosition) * MoveDelta);
                    }
                    LastMousePosition = CurrentMousePosition;
                }
                else
                {
                    MovingHandler = null;
                }
            }
        }

        private void SelectPolygonMakerOne()
        {
            SelectModel(GuiSpawner.Get().GetGui("PolygonMaker").GetComponent<PolygonMaker>().MyViewer.GetSpawn().GetComponent<PolyModelHandle>());
        }

        // link the polygon painter to a mesh or viewer
        #region Linking

        /// <summary>
        /// 
        /// </summary>
        private void SelectModel(PolyModelHandle NewModel)
        {
            if (NewModel != SelectedModel)
            {
                // deselect selected model
                if (SelectedModel)
                {
                    SelectedModel.UpdateHandlerMode(PolyModelHandleMode.None);
                    SelectedModel.GetComponent<MeshCollider>().enabled = true;
                }

                SelectedModel = NewModel;
                if (SelectedModel)
                {
                    Debug.LogError("Selected: " + SelectedModel.name);
                    TextureMapController.SetMaxSelected(GetSizeTextureMap());
                    TextureMapController.SelectIndex(0);
                    TextureMapController.Enable();
                    if (GetInput("NameInput"))
                    {
                        GetInput("NameInput").text = SelectedModel.name;
                        GetInput("NameInput").interactable = true;
                    }
                    SelectedModel.UpdateHandlerMode((PolyModelHandleMode)GetDropdown("HandlerDropdown").value);
                }
                else
                {
                    // set guis to inactive
                    TextureMapController.Disable();
                    if (GetInput("NameInput"))
                    {
                        GetInput("NameInput").text = "";
                        GetInput("NameInput").interactable = false;
                    }
                }
            }
        }
        #endregion

        // raycast against that polygon
        #region Raycasting

        /// <summary>
        /// First raycast for guis
        /// Then raycast for worlds
        /// </summary>
        private void Raycast()
        {
            // set highlighting to false
                DidHitModel = false;
                DidHitGui = RaycastViewer();   // did ray hit any gui
                if (DidHitGui == false)
                {
                    //RaycastWorld();
                }
            /*if (Input.GetMouseButtonDown(0) && DidRayHitSkeleton == false)
            {
                if (DidHitGui == false || DidRayHitViewer == true)
                {
                    if (PaintType == SkeletonPaintType.Link)
                    {
                        SelectSkeleton(null);
                    }
                    else if (PaintType == SkeletonPaintType.Select)
                    {
                        SelectBone(new Bone());
                    }
                }
            }*/
        }
        private float AreaSelectionRadius = 0.1f;
        /// <summary>
        /// Returns whether it hit a gui
        /// </summary>
        private bool RaycastViewer()
        {
            Ray MyRay;
            //Create the PointerEventData with null for the EventSystem
            PointerEventData MyPointerEvent = new PointerEventData(EventSystem.current);
            //Set required parameters, in this case, mouse position
            MyPointerEvent.position = Input.mousePosition;
            //Create list to receive all results
            List<RaycastResult> MyResults = new List<RaycastResult>();
            //Raycast it
            EventSystem.current.RaycastAll(MyPointerEvent, MyResults);
            //Debug.LogError("Raycast results: " + MyResults.Count + " at position: " + MyPointerEvent.position.ToString());
            for (int i = 0; i < MyResults.Count; i++)
            {
                if (MyResults[i].gameObject.GetComponent<ObjectViewer>())
                {
                    //DidRayHitViewer = true;
                    if (MyViewer != MyResults[i].gameObject.GetComponent<ObjectViewer>())
                    {
                        MyViewer = MyResults[i].gameObject.GetComponent<ObjectViewer>();
                        // Have to raycast
                        Debug.Log("New SkeletonViewer Selected: " + MyViewer.name);
                    }
                    bool DidRayHitModel = MyViewer.GetRayHitInViewer(Input.mousePosition, out MyRay, out MyHit);
                    if (DidRayHitModel)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            PolyModelHandle MyModel = MyHit.collider.gameObject.GetComponent<PolyModelHandle>();
                            if (MyModel && HandlerMode == PolygonEditMode.Link)
                            {
                                SelectModel(MyModel);
                            }
                            if (SelectedModel && HandlerMode == PolygonEditMode.Move)// && HandlerMode == PolygonEditMode.)
                            {
                                SelectedModel.SelectHandler(MyHit.collider.gameObject, GetToggle("AreaSelectToggle").isOn);
                                MovingHandler = MyHit.collider.gameObject;
                                MoveDistance = Vector3.Distance(MyViewer.GetRenderCamera().transform.position, MyHit.point);
                            }
                        }
                    }
                    else
                    {
                        if (Input.GetMouseButtonDown(0) && HandlerMode == PolygonEditMode.Link)
                        {
                            //Debug.LogError("Ray missed skeleton.");
                            SelectModel(null);
                        }
                    }
                    break;
                }
            }
            return (MyResults.Count != 0);
        }
        
        private Vector3 LastMousePosition;
        private Vector3 CurrentMousePosition;
        #endregion

        #region verticies

        #endregion

        #region TextureIndexController

        /// <summary>
        /// 
        /// </summary>
        private PolyModel GetSelectedModel()
        {
            if (SelectedModel)
            {
                return SelectedModel.MyModel;
            }
            else
            {
                return null;
            }
        }

        private int GetSizeTextureMap()
        {
            if (GetSelectedModel() != null)
            {
                return GetSelectedModel().TextureMaps.Count;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Link functions with the index controller
        /// </summary>
        private void InitiateTextureMapController()
        {
            if (TextureMapController)
            {
                TextureMapController.OnIndexUpdated.RemoveAllListeners();
                TextureMapController.OnIndexUpdated.AddEvent(OnUpdatedIndexTextureMap);
                TextureMapController.OnAdd.RemoveAllListeners();
                TextureMapController.OnAdd.AddEvent(OnAddTextureMap);
                TextureMapController.OnRemove.RemoveAllListeners();
                TextureMapController.OnRemove.AddEvent(OnRemoveTextureMap);
                TextureMapController.OnListEmpty.RemoveAllListeners();
                TextureMapController.OnListEmpty.AddEvent(OnListEmptyTextureMap);
                TextureMapController.SetMaxSelected(GetSizeTextureMap());
                TextureMapController.OnBegin();
                TextureMapController.Disable();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private int GetSelectedTextureMap()
        {
            return TextureMapController.SelectedIndex;
        }

        /// <summary>
        /// When loaded new texture map
        /// </summary>
        private void OnUpdatedIndexTextureMap(int Index)
        {
            //RefreshViewer();
            //UpdateStatistics();
            if (SelectedModel)
            {
                SelectedModel.TextureMapIndex = Index;
                SelectedModel.OnMassUpdatedModel();
            }
        }

        /// <summary>
        /// Adds a texture map
        /// </summary>
        private void OnAddTextureMap(int Index)
        {
            if (GetSelectedModel() != null)
            {
                GetSelectedModel().NewTextureMap();
                TextureMapController.SetMaxSelected(GetSelectedModel().TextureMaps.Count);
                TextureMapController.SelectIndex(GetSelectedModel().TextureMaps.Count - 1);
            }
            else
            {
                Debug.LogWarning("Cannot add texture map without model selected.");
            }
            //RefreshViewer();
            //UpdateStatistics();
        }

        /// <summary>
        /// Removes a texture map
        /// </summary>
        private void OnRemoveTextureMap(int Index)
        {
            GetSelectedModel().TextureMaps.RemoveAt(Index);
            //RefreshViewer();
            //UpdateStatistics();
            TextureMapController.SetMaxSelected(GetSelectedModel().TextureMaps.Count);
            TextureMapController.SelectIndex(TextureMapController.SelectedIndex - 1);
        }

        /// <summary>
        /// When no texture maps
        /// </summary>
        private void OnListEmptyTextureMap()
        {

        }
        #endregion

        #region Input

        public override void UseInput(Button MyButton)
        {
            if (GetSelectedModel() != null)
            {
                if (MyButton.name == "AddTextureButton")
                {
                    // Get the new tile name
                    Dropdown MyDropdown = GetDropdown("TexturesDropdown");
                    if (GetImage("TextureMapViewer"))
                    {
                        GetImage("TextureMapViewer").texture = (DataManager.Get().GetElement(DataFolderNames.VoxelDiffuseTextures, MyDropdown.value) as Zexel).GetTexture();

                    }
                    string TextureName = DataManager.Get().GetName(DataFolderNames.VoxelDiffuseTextures, MyDropdown.value);// VoxelManager.Get().DiffuseTextures[MyDropdown.value].name;
                    // Update the selected face
                    if (SelectedModel.HasFaceSelected()) // Bam!
                    {
                        List<int> Triangles = SelectedModel.GetSelectedTriangles();
                        for (int i = 0; i < Triangles.Count; i++)
                        {
                            GetSelectedModel().SetTextureMapTile(GetSelectedTextureMap(), TextureName, Triangles[i]);
                        }
                    }
                    else // generate for all of them!
                    {
                        GetSelectedModel().GenerateTextureMap(TextureName, GetSelectedTextureMap());
                        SelectedModel.OnMassUpdatedModel();
                        Debug.LogError("Generating texture " + TextureName + " for " + GetSelectedModel().Name + " of texture index: " + GetSelectedTextureMap());
                    }
                }
            }
            else
            {
                Debug.LogError("No Model Selected.");
            }
        }

        public override void UseInput(Dropdown MyDropdown)
        {
            if (MyDropdown.name == "HandlerDropdown")
            {
                if (SelectedModel)
                {
                    SelectedModel.UpdateHandlerMode((PolyModelHandleMode)MyDropdown.value);
                }
            }
            else if (MyDropdown.name == "PolygonEditModeDropdown")
            {
                HandlerMode = (PolygonEditMode)MyDropdown.value;
                if (SelectedModel != null)
                {
                    SelectedModel.GetComponent<MeshCollider>().enabled = (HandlerMode == PolygonEditMode.Link);
                }
            }
            else if (MyDropdown.name == "TexturesDropdown")
            {
                GetImage("TexturesImage").texture = 
                    (DataManager.Get().GetElement(DataFolderNames.VoxelDiffuseTextures, MyDropdown.value) as Zexel).GetTexture();// VoxelManager.Get().DiffuseTextures[MyDropdown.value];
            }
        }

        public override void UseInput(Toggle MyToggle)
        {
            if (MyToggle.name == "AreaSelectToggle")
            {
                //IsAreaSelect = MyToggle.isOn;
            }
        }
        
        /// <summary>
        /// Fill the drop downs with data
        /// </summary>
        public override void FillDropdown(Dropdown MyDropdown)
        {
            Debug.LogError("Filling Dropdown in " + name);
            if (MyDropdown != null)
            {
                List<string> MyDropdownNames = new List<string>();
                /*if (MyDropdown.name == "ImportDropdown")
                {
                    List<string> MyFiles = FileUtil.GetFilesOfType(FileUtil.GetFolderPath("ModelImports/"), "obj");
                    for (int i = 0; i < MyFiles.Count; i++)
                    {
                        MyDropdownNames.Add(Path.GetFileNameWithoutExtension(MyFiles[i]));
                    }
                    FillDropDownWithList(MyDropdown, MyDropdownNames);
                }*/
                if (MyDropdown.name == "TexturesDropdown")
                {
                    for (int i = 0; i < DataManager.Get().GetSize(DataFolderNames.VoxelDiffuseTextures); i++)
                    {
                        MyDropdownNames.Add(DataManager.Get().GetName(DataFolderNames.VoxelDiffuseTextures, i));
                    }
                    FillDropDownWithList(MyDropdown, MyDropdownNames);
                }
            }
        }
        #endregion

        #region Statistics

        /// <summary>
        /// Generates the Solidity Values for a voxel model and update the UI.
        /// </summary>
        public void GenerateSolidity()
        {
            PolyModel MyModel = GetSelectedModel();
            if (MyModel != null)
            {
                MyModel.GenerateSolidity();
                string MySolidity = "";
                for (int i = 0; i < MyModel.Solidity.Length; i++)
                {
                    string MyBool = "[X]";
                    if (!MyModel.Solidity[i])
                        MyBool = "[0]";
                    if (i == 0)
                        MySolidity += " Top";
                    else if (i == 1)
                        MySolidity += " Bottom";
                    else if (i == 2)
                        MySolidity += " Back";
                    else if (i == 3)
                        MySolidity += " Front";
                    else if (i == 4)
                        MySolidity += " Left";
                    else if (i == 5)
                        MySolidity += " Right";
                    if (i != 5)
                        MySolidity += MyBool + "\n";
                    else
                        MySolidity += MyBool;
                }
                if (GetLabel("SolidityLabel"))
                {
                    GetLabel("SolidityLabel").text = MySolidity;
                }
            }
        }
        #endregion
    }
}