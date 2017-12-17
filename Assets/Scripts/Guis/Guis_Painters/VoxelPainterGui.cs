using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeltexTools;
using Zeltex.Util;
using Zeltex.Voxels;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zeltex.Items;
using Zeltex.AnimationUtilities;
using Zeltex.Skeletons;

namespace Zeltex.Guis.Maker
{
    public enum VoxelPaintType
    {
        None,
        Build,
        Erase,
        PickColor,
        PickVoxel,
        Paint,
        Select,     // Select Voxels!
        PaintVoxel,
        PaintColor,
        Fill,
        RectangleSelection,
        Link
        //Move        // Move Voxels!
    }
    /// <summary>
    /// Paint the voxels
    /// The user will select a world, then edit it
    /// Upon selecting a new world, it will update the dropdown menu for its voxel types it uses\
    /// To Do:
    ///     Select Tool - including operations for them
    ///     Select Outer Edges Function - Selects all voxels on the edge, touching air
    ///     Move Tool - Move voxels in a direction using a gizmo type thing - just buttons will do for now
    ///     
    /// </summary>
    public class VoxelPainterGui : GuiBasic
    {
        #region Variables
        private static Vector3 MaxWorldSize = new Vector3(8, 8, 8);
        public bool IsDebug;
        private World MyWorld;      // the VoxelSystem we are editing
        private VoxelViewer MyViewer;   // if editing a viewer
        private SkeletonViewer MySkeletonViewer;   // if editing a viewer
        private ObjectViewer MyObjectViewer;   // if editing a viewer
        [Header("References")]
        public VoxelPolygonViewer MyVoxelModelViewer;
       // public VoxelPrimitives MyVoxelPrimitives;
        [Header("Paint Brush")]
        public VoxelPaintType PaintType;
        //public int PaintType = 0;
        public int PaintSize = 0;
        public string VoxelName = "Air";
        public bool IsPaintOver;
        public Color MyTintingColor;
        [Header("Events")]
        public MyEventColor32 OnChangeColor = new MyEventColor32();
        public EventString OnChangeVoxelType = new EventString();
        [Header("Selection")]
        public Material HighlightedMaterial;
        public Material SelectedMaterial;
        private GameObject HighLightedCube;
        private List<GameObject> SelectionCubes = new List<GameObject>();
        //private GameObject SelectionCube;
        private RaycastHit MyHit;
        [SerializeField]
        private bool DidRayHit;
        //private Vector3 HitNormal;
        private bool IsSpray;
        public Vector3 HitNormal;
        public Vector3 LastHitBlockPosition;
        public int SelectionDepth = 0;
        private int SelectionNegativeDepth = -5;
        private List<Int3> SelectedPositions = new List<Int3>();
        private Int3 FirstHitBlockPosition;
        private bool IsFirstClick = true;
        #endregion

        #region Mono

        private void Awake()
        {
            //OnBegin();
            // create cube
            HighLightedCube = (GameObject)GameObject.CreatePrimitive(PrimitiveType.Cube);
            HighLightedCube.name = "HighlightCube";
            Destroy(HighLightedCube.GetComponent<BoxCollider>());
            HighLightedCube.GetComponent<MeshRenderer>().material = HighlightedMaterial;
            HighLightedCube.SetActive(false);
            VoxelName = VoxelManager.Get().GetMetaName(1);
        }

        void OnDisable()
        {
            Destroy(HighLightedCube);
            DeselectAll();
        }
        // Update is called once per frame
        void Update()
        {
            if (PaintType != 0)
            {
                Raycast();
                UpdateHighlighted();
                // Handle input
                if (DidRayHit && ((!IsSpray && Input.GetMouseButtonDown(0)) || (IsSpray && Input.GetMouseButton(0))))
                {
                    HandleWorldInteraction(MyHit.point, MyHit.normal);
                }
                AlterBrush();
            }
        }

        void OnGUI()
        {
            if (IsDebug && MyWorld)
            {
                GUILayout.Label("Position: " + LastHitBlockPosition.ToString());
                GUILayout.Label("Chunk Position: " + MyWorld.GetChunkWorldPosition(new Int3(LastHitBlockPosition)).Position.GetVector().ToString());
            }
        }
        #endregion

        #region Selection

        private void DeselectAll()
        {
            for (int i = 0; i < SelectionCubes.Count; i++)
            {
                Destroy(SelectionCubes[i]);
            }
            SelectionCubes.Clear();
            SelectedPositions.Clear();
        }

        public Vector3 SelectionCubeToBlockPosition(GameObject SelectionCube)
        {
            string[] Input = SelectionCube.name.Split('_');
            return new Vector3(float.Parse(Input[1]), float.Parse(Input[2]), float.Parse(Input[3]));
        }

        private string BlockPositionToSelectionCubeName(Int3 BlockPosition)
        {
            return "SelectionCube_" + BlockPosition.x + "_" + BlockPosition.y + "_" + BlockPosition.z;
        }

        private void SingleSelectVoxel()
        {
            DeselectAll();
            SelectVoxel();
        }

        private void DeSelectVoxel()
        {
            for (int i = 0; i < SelectionCubes.Count; i++)
            {
                if (SelectionCubeToBlockPosition(SelectionCubes[i]) == LastHitBlockPosition)
                {
                    Destroy(SelectionCubes[i]);
                    SelectionCubes.RemoveAt(i);
                    SelectedPositions.RemoveAt(i);
                    break;
                }
            }
        }

        private void SelectRectangleVoxels()
        {
            if (!Input.GetKey(KeyCode.LeftControl))
            {
                DeselectAll();
            }
            int DirectionX = 0;
            if (FirstHitBlockPosition.x > LastHitBlockPosition.x)
            {
                DirectionX = -1;
            }
            else if (FirstHitBlockPosition.x < LastHitBlockPosition.x)
            {
                DirectionX = 1;
            }
            int DirectionY = 0;
            if (FirstHitBlockPosition.y > LastHitBlockPosition.y)
            {
                DirectionY = -1;
            }
            else if (FirstHitBlockPosition.y < LastHitBlockPosition.y)
            {
                DirectionY = 1;
            }
            int BeginZ = FirstHitBlockPosition.z;
            if (BeginZ < LastHitBlockPosition.z)
            {
                BeginZ = Mathf.RoundToInt(LastHitBlockPosition.z);
            }
            int DirectionZ = 1;

            bool IsFinishedX = false;
            for (int i = FirstHitBlockPosition.x; IsFinishedX == false; i += DirectionX)
            {
                bool IsFinishedY = false;
                for (int j = FirstHitBlockPosition.y; IsFinishedY == false; j += DirectionY)
                {
                    for (int k = SelectionNegativeDepth; k <= SelectionDepth; k++)
                    {
                        Int3 BlockPosition = new Int3(i, j, BeginZ) - (new Int3(k * HitNormal));
                        SelectPosition(BlockPosition);
                    }
                    if (DirectionY > 0) // if j++
                    {
                        IsFinishedY = (j >= LastHitBlockPosition.y);
                    }
                    else
                    {
                        IsFinishedY = (j <= LastHitBlockPosition.y);
                    }
                }
                if (DirectionX > 0)
                {
                    IsFinishedX = (i >= LastHitBlockPosition.x);
                }
                else
                {
                    IsFinishedX = (i <= LastHitBlockPosition.x);
                }
            }
        }

        private void SelectVoxel()
        {
            for (int i = SelectionNegativeDepth; i <= SelectionDepth; i++)
            {
                //Debug.Log("LastHitBlockPosition: " + LastHitBlockPosition.ToString());
                Int3 BlockPosition = new Int3(LastHitBlockPosition - i * HitNormal);
                SelectPosition(BlockPosition);
            }
        }
        
        private void SelectVoxels(List<Int3> MyVoxels)
        {
            for (int i = 0; i < MyVoxels.Count; i++)
            {
                SelectPosition(MyVoxels[i]);
            }
        }

        private void SelectPosition(Int3 SelectionPosition)
        {
            if (SelectedPositions.Contains(SelectionPosition) == false)
            {
                if (MyWorld.GetVoxelType(SelectionPosition) != 0)
                {
                    Debug.Log("Selecting new Voxel: " + SelectionPosition.x + ":" + SelectionPosition.y + ":" + SelectionPosition.z);
                    //GameObject SelectionCube = CreateSelectionCube(SelectionPosition);
                    GameObject SelectionCube = (GameObject)GameObject.CreatePrimitive(PrimitiveType.Cube);
                    SelectionCube.name = BlockPositionToSelectionCubeName(SelectionPosition);//SelectionCubes.Count;
                    Destroy(SelectionCube.GetComponent<BoxCollider>());
                    SelectionCube.GetComponent<MeshRenderer>().material = SelectedMaterial;
                    SelectionCube.SetActive(true);
                    SelectionCube.layer = HighLightedCube.layer;
                    SelectionCube.transform.SetParent(MyWorld.transform);
                    SelectionCube.transform.position = MyWorld.BlockToRealPosition(SelectionPosition.GetVector());
                    Debug.Log("SelectionCube.transform.position: " + SelectionCube.transform.localPosition.ToString());
                    SelectionCube.transform.rotation = MyWorld.transform.rotation;
                    SelectionCube.transform.localScale = MyWorld.VoxelScale * 1.01f;
                    SelectedPositions.Add(SelectionPosition);
                    SelectionCubes.Add(SelectionCube);
                }
            }
        }

        private List<Int3> GetSelectedBlockPositions()
        {
            List<Int3> MyBlockPositions = new List<Int3>();
            if (SelectionCubes.Count != 0)
            {
                for (int i = 0; i < SelectionCubes.Count; i++)
                {
                    MyBlockPositions.Add(new Int3(SelectionCubeToBlockPosition(SelectionCubes[i])));
                }
            }
            else
            {

            }
            return MyBlockPositions;
        }

        private void MoveSelection(string MoveType)
        {
            for (int i = 0; i < SelectedPositions.Count; i++)
            {
                //Vector3 BlockPosition = SelectionCubeToBlockPosition(SelectionCubes[i]);
                Int3 NewPosition = SelectedPositions[i];
                if (MoveType == "MoveLeft")
                {
                    NewPosition += new Int3(1, 0, 0);
                }
                if (MoveType == "MoveRight")
                {
                    NewPosition += new Int3(-1, 0, 0);
                }
                if (MoveType == "MoveForward")
                {
                    NewPosition += new Int3(0, 0, -1);
                }
                if (MoveType == "MoveBack")
                {
                    NewPosition += new Int3(0, 0, 1);
                }
                if (MoveType == "MoveUp")
                {
                    NewPosition += new Int3(0, 1, 0);
                }
                if (MoveType == "MoveDown")
                {
                    NewPosition += new Int3(0, -1, 0);
                }
                SelectedPositions[i] = NewPosition;
                SelectionCubes[i].name = BlockPositionToSelectionCubeName(SelectedPositions[i]);
                SelectionCubes[i].transform.position = MyWorld.BlockToRealPosition(SelectedPositions[i].GetVector());
            }
        }
        #endregion

        #region Raycasting
        /// <summary>
        /// First raycast for guis
        /// Then raycast for worlds
        /// </summary>
        private void Raycast()
        {
            HighLightedCube.SetActive(false);
            DidRayHit = false;
            if (RaycastViewer() == false)
            {
                RaycastWorld();
            }
        }

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
                if (MyResults[i].gameObject.GetComponent<VoxelViewer>())
                {
                    if (MyViewer != MyResults[i].gameObject.GetComponent<VoxelViewer>())
                    {
                        MyViewer = MyResults[i].gameObject.GetComponent<VoxelViewer>();
                        Debug.Log("New Voxel Viewer Selected: " + MyViewer.name);
                    }
                    bool DidHit = MyViewer.GetRayHitInViewer(Input.mousePosition, out MyRay, out MyHit);
                    if (DidHit)
                    {
                        DidRayHit = true;
                        SelectWorld(MyViewer.GetSpawn().GetComponent<World>());
                    }
                    break;
                }
                else if (MyResults[i].gameObject.GetComponent<SkeletonViewer>())
                {
                    if (MySkeletonViewer != MyResults[i].gameObject.GetComponent<SkeletonViewer>())
                    {
                        MySkeletonViewer = MyResults[i].gameObject.GetComponent<SkeletonViewer>();
                        Debug.Log("New Skeleton Viewer Selected: " + MySkeletonViewer.name);
                    }
                    bool DidHit = MySkeletonViewer.GetRayHitInViewer(Input.mousePosition, out MyRay, out MyHit);
                    if (DidHit && MyHit.collider.gameObject.GetComponent<Chunk>())
                    {
                        SelectWorld(MyHit.collider.gameObject.GetComponent<Chunk>().GetWorld());
                        DidRayHit = true;
                    }
                    break;
                }
                else if (MyResults[i].gameObject.GetComponent<ObjectViewer>())
                {
                    ObjectViewer NewViewer = MyResults[i].gameObject.GetComponent<ObjectViewer>();
                    if (NewViewer.GetSpawn())
                    {
                        World ViewerWorld = NewViewer.GetSpawn().GetComponent<World>();
                        if (ViewerWorld)
                        {
                            if (MyObjectViewer != NewViewer)
                            {
                                MyObjectViewer = NewViewer;
                                Debug.Log("New Viewer Selected: " + MyObjectViewer.name);
                            }
                            bool DidHit = MyObjectViewer.GetRayHitInViewer(Input.mousePosition, out MyRay, out MyHit);
                            if (DidHit)
                            {
                                DidRayHit = true;
                                SelectWorld(MyObjectViewer.GetSpawn().GetComponent<World>());
                            }
                            break;
                        }
                        else
                        {
                            Debug.LogError(NewViewer.name + "'s Spawn has no world.");
                        }
                    }
                    else
                    {
                        Debug.LogError(NewViewer.name + " has no spawn.");
                    }
                }
            }
            return (MyResults.Count != 0);
        }
        /// <summary>
        /// Raycast against the world!
        /// </summary>
        private void RaycastWorld()
        {
            Ray MyRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (UnityEngine.Physics.Raycast(MyRay.origin, MyRay.direction, out MyHit))
            {
                Chunk MyChunk = MyHit.collider.gameObject.GetComponent<Chunk>();
                if (MyChunk)
                {
                    if (MyWorld != MyChunk.GetWorld())
                    {
                        SelectWorld(MyChunk.GetWorld());
                    }
                    DidRayHit = true;
                }
            }
        }
        /// <summary>
        /// Handles all the voxel/world input   //RaycastHit MyHit
        /// </summary>
        private void HandleWorldInteraction(Vector3 HitPoint, Vector3 HitNormal)
        {
            Vector3 LastHitBlockPosition = MyWorld.RayHitToBlockPosition(HitPoint, HitNormal);
            Vector3 MyNormal = MyWorld.transform.InverseTransformDirection(HitNormal);
            if (PaintType == VoxelPaintType.Build) // paint
            {
                MyWorld.UpdateBlockType(VoxelName, LastHitBlockPosition + MyNormal, PaintSize, MyTintingColor);
            }
            else if (PaintType == VoxelPaintType.Erase) // Erase
            {
                Debug.LogError("Erase:" + LastHitBlockPosition.ToString());
                MyWorld.UpdateBlockType("Air", LastHitBlockPosition, PaintSize, Color.white);
            }
            // left click
            else if (PaintType == VoxelPaintType.PickColor) // Color Picker
            {
                Color MyColor = MyWorld.GetVoxelColorRay(HitPoint, HitNormal);
                UpdateColor(MyColor);
            }
                //else if (Input.GetKey(KeyCode.LeftShift))
            else if (PaintType == VoxelPaintType.PickVoxel) // Block Picker
            {
                int NewType = MyWorld.GetVoxelTypeRay(HitPoint, HitNormal);
                string MyVoxelName = MyWorld.MyLookupTable.GetName(NewType);
                UpdateVoxelnternal(MyVoxelName);
            }
            else if (PaintType == VoxelPaintType.Paint)
            {
                MyWorld.IsPaintOver = true;
                MyWorld.UpdateBlockType(VoxelName, LastHitBlockPosition, PaintSize, MyTintingColor);
                MyWorld.IsPaintOver = false;
            }
            else if (PaintType == VoxelPaintType.Select)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    SelectVoxel();
                }
                else if (Input.GetKey(KeyCode.LeftShift))
                {
                    DeSelectVoxel();
                }
                else
                {
                    SingleSelectVoxel();
                }
                //HighLightedCube.transform.position = LastHitWorldPosition + MyWorld.transform.TransformDirection(VoxelUnit / 2f);// + PositionOffset;
            }
            else if (PaintType == VoxelPaintType.RectangleSelection)
            {
                if (IsFirstClick)
                {
                    // remember where first click was
                    FirstHitBlockPosition = new Int3(LastHitBlockPosition);
                    if (!Input.GetKey(KeyCode.LeftControl))
                    {
                        SingleSelectVoxel();
                    }
                    PreviousHitNormal = HitNormal;
                    IsFirstClick = !IsFirstClick;
                }
                else
                {
                    if (PreviousHitNormal == HitNormal)
                    {
                        SelectRectangleVoxels();
                        IsFirstClick = !IsFirstClick;
                    }
                }
            }
        }
        private Vector3 PreviousHitNormal;
        /// <summary>
        /// Updates the Highlighted cube
        /// </summary>
        private void UpdateHighlighted()
        {
            if (DidRayHit && MyWorld)
            {
                HighLightedCube.SetActive(true);
                HighLightedCube.layer = MyWorld.gameObject.layer;
                HitNormal = MyWorld.transform.InverseTransformDirection(MyHit.normal);  //Vector3
                LastHitBlockPosition = MyWorld.RayHitToBlockPosition(MyHit.point, MyHit.normal);
                bool IsAir = VoxelName == "Air";
                if (PaintType == VoxelPaintType.Build && IsAir == false)    // not equal air
                {
                    LastHitBlockPosition += HitNormal;
                }
                HighLightedCube.transform.position = MyWorld.BlockToRealPosition(LastHitBlockPosition);// + PositionOffset;
                HighLightedCube.transform.rotation = MyWorld.transform.rotation;
                HighLightedCube.transform.localScale = MyWorld.GetUnit() * 1.01f;
            }
        }

        /*Vector3 GetVoxelLossyScale()
        {
            return new Vector3(
                   MyWorld.VoxelScale.x * MyWorld.transform.lossyScale.x,
                   MyWorld.VoxelScale.y * MyWorld.transform.lossyScale.y,
                   MyWorld.VoxelScale.z * MyWorld.transform.lossyScale.z);
        }

        Vector3 BlockPositionToWorldPosition(Vector3 BlockPosition)
        {
            return BlockPositionToWorldPosition(new Int3(BlockPosition));
        }

        Vector3 BlockPositionToWorldPosition(Int3 BlockPosition)
        {
            Vector3 WorldPosition = MyWorld.BlockToRealPosition(BlockPosition.GetVector());
            Vector3 VoxelUnit = GetVoxelLossyScale();
            return WorldPosition + MyWorld.transform.TransformDirection(VoxelUnit / 2f);
        }*/
        #endregion

        #region WorldSelection
        /// <summary>
        /// Selecting a new world
        /// </summary>
        private void SelectWorld(World NewWorld)
        {
            if (MyWorld != NewWorld)
            {
                // Deactivate previous worlds lines
                if (MyWorld)
                {
                    Debug.Log("DeSelecting world: " + MyWorld.name);
                    if (MyWorld.GetComponent<GridOverlay>())
                    {
                        MyWorld.GetComponent<GridOverlay>().SetState(
                            false,
                            MyWorld.GetWorldBlockSize().ToInt3());
                    }
                    MyWorld.RemoveOnLoad(OnWorldLoad);
                }
                MyWorld = NewWorld;
                if (NewWorld != null)
                {
                    RefreshWorldData();
                    MyWorld.AddOnLoad(OnWorldLoad);
                }
                else
                {
                    GetLabel("StatisticsText").text = "No World Selected";
                    GetLabel("MySizeLabelX").text = "";
                    GetLabel("MySizeLabelY").text = "";
                    GetLabel("MySizeLabelZ").text = "";
                }
            }
        }

        public void OnWorldLoad()
        {
            RefreshWorldData();
        }

        /// <summary>
        /// when a new world is chosen, update all the data for it
        /// </summary>
        private void RefreshWorldData()
        {
            Debug.Log("Selecting new world: " + MyWorld.name);
            //GetLabel("StatisticsText").text = FileUtil.ConvertToSingle(MyWorld.GetStatistics());
            /*GridOverlay MyOverlay = MyWorld.GetComponent<GridOverlay>();
            if (MyOverlay)
            {
                MyOverlay.IsForce = true;
                MyOverlay.SetState(GetToggle("GridToggle").isOn, MyWorld.WorldSize * Chunk.ChunkSize);
            }*/
            OnUpdateSize();
            /*GetLabel("MySizeLabelX").text = MyWorld.WorldSize.x + "";
            GetLabel("MySizeLabelY").text = MyWorld.WorldSize.y + "";
            GetLabel("MySizeLabelZ").text = MyWorld.WorldSize.z + "";*/
            GetLabel("StatisticsText").text = FileUtil.ConvertToSingle(MyWorld.GetStatistics());
        }
        #endregion

        #region Setters

        /// <summary>
        /// Internally change the voxel type
        /// </summary>
        private void UpdateVoxelnternal(string VoxelName_)
        {
            VoxelName = VoxelName_;
            //MyVoxelPrimitives.VoxelName = VoxelName;
            GetDropdown("VoxelDropdown").value = 0; // default is air
            for (int i = 0; i < GetDropdown("VoxelDropdown").options.Count; i++)
            {
                if (GetDropdown("VoxelDropdown").options[i].text == VoxelName)
                {
                    GetDropdown("VoxelDropdown").value = i;
                    break;
                }
            }
            OnChangeVoxelType.Invoke(VoxelName);
        }

        /*private void UpdatePaintType()
        {
            PaintType = 0;
        }*/
        /// <summary>
        /// called by the colour picker ui
        /// </summary>
        /// <param name="NewColor"></param>
        public void UpdateTint(Color32 NewColor)
        {
            MyTintingColor = NewColor;
        }
        /// <summary>
        /// For internal tool use
        /// </summary>
        /// <param name="NewColor"></param>
        private void UpdateColor(Color32 NewColor)
        {
            MyTintingColor = NewColor;
            OnChangeColor.Invoke(NewColor);
        }
        #endregion

        #region Tools

        /// <summary>
        /// Sets the mouse icon to the type of paint brush
        /// </summary>
        private void AlterBrush()
        {
            if (PaintType != VoxelPaintType.None && HighLightedCube.activeSelf)// && (IsTextureHit || IsInRect))  // and inside the viewer!
            {
                if (PaintType == VoxelPaintType.Build)
                {
                    MouseCursor.Get().SetMouseIcon("VoxelBuild");
                }
                else if (PaintType == VoxelPaintType.Erase)
                {
                    MouseCursor.Get().SetMouseIcon("VoxelErase");
                }
                else if (PaintType == VoxelPaintType.Paint)
                {
                    MouseCursor.Get().SetMouseIcon("VoxelPaint");
                }
                else if (PaintType == VoxelPaintType.Select)
                {
                    MouseCursor.Get().SetMouseIcon("VoxelSelect");
                }
                else if (PaintType == VoxelPaintType.PickColor)
                {
                    MouseCursor.Get().SetMouseIcon("VoxelPickColor");
                }
                else if (PaintType == VoxelPaintType.PickVoxel)
                {
                    MouseCursor.Get().SetMouseIcon("VoxelPickIndex");
                }
                else if (PaintType == VoxelPaintType.Fill)
                {
                    MouseCursor.Get().SetMouseIcon("VoxelFill");
                }
                else if (PaintType == VoxelPaintType.RectangleSelection)
                {
                    MouseCursor.Get().SetMouseIcon("RectangleSelection");
                }
            }
            else
            {
                MouseCursor.Get().SetMouseIcon("DefaultMouse");
            }
        }

        /// <summary>
        /// Sets paint type as well as dropdown
        /// </summary>
        private void SetPaintTypeInternal(VoxelPaintType NewPaintType)
        {
            GetDropdown("PaintTypeDropdown").value = (int)NewPaintType;
            SetPaintType(NewPaintType);
        }

        /// <summary>
        /// Sets paint type
        /// </summary>
        private void SetPaintType(VoxelPaintType NewPaintType)
        {
            if (PaintType != NewPaintType)
            {
                if (PaintType == VoxelPaintType.Select)
                {
                    DeselectAll();
                }
                else if (PaintType == VoxelPaintType.RectangleSelection)
                {
                    IsFirstClick = true;
                }
                PaintType = NewPaintType;
            }
        }
        
        /// <summary>
        /// The buttons are used to swap to different paint types
        /// </summary>
        private bool UseInputTool(Button MyButton)
        {
            if (MyButton.name == "NoneButton")
            {
                SetPaintTypeInternal(VoxelPaintType.None);
            }
            else if (MyButton.name == "BuildButton")
            {
                SetPaintTypeInternal(VoxelPaintType.Build);
            }
            else if (MyButton.name == "PaintButton")
            {
                SetPaintTypeInternal(VoxelPaintType.Paint);
            }
            else if (MyButton.name == "EraseButton")
            {
                SetPaintTypeInternal(VoxelPaintType.Erase);
            }
            else if (MyButton.name == "PickColorButton")
            {
                SetPaintTypeInternal(VoxelPaintType.PickColor);
            }
            else if (MyButton.name == "PickVoxelButton")
            {
                SetPaintTypeInternal(VoxelPaintType.PickVoxel);
            }
            else if (MyButton.name == "SelectButton")
            {
                SetPaintTypeInternal(VoxelPaintType.Select);
            }
            else if (MyButton.name == "FillButton")
            {
                SetPaintTypeInternal(VoxelPaintType.Fill);
            }
            else if (MyButton.name == "RectangleSelectionButton")
            {
                SetPaintTypeInternal(VoxelPaintType.RectangleSelection);
            }
            else
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Input

        public override void UseInput(InputField MyInputField)
        {
            base.UseInput(MyInputField);
            if (MyInputField.name == "DepthInput")
            {
                SelectionDepth = int.Parse(MyInputField.text);
                MyInputField.text = SelectionDepth + "";
            }
        }

        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(Dropdown MyDropdown)
        {
            if (MyDropdown.name == "VoxelDropdown")
            {
                VoxelName = MyDropdown.options[MyDropdown.value].text;
                // Value of the drop down is the position in the entire database, so it will get hte proper voxel meta
                VoxelMeta MyVoxelMeta = VoxelManager.Get().GetMeta(MyDropdown.value + 1);
                MyVoxelModelViewer.LoadVoxelMesh(MyVoxelMeta);
                //MyWorld.GetComponent<VoxelPrimitives>().SetVoxelType(MyDropdown.value);
            }
            else if (MyDropdown.name == "PaintTypeDropdown")
            {
                SetPaintType((VoxelPaintType)MyDropdown.value);
            }
        }

        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(Toggle MyToggle)
        {
            if (MyToggle.name == "GridToggle")
            {
                if (MyWorld)
                {
                    if (MyWorld.GetComponent<GridOverlay>())
                    {
                        MyWorld.GetComponent<GridOverlay>().SetState(
                            MyToggle.isOn,
                            MyWorld.GetWorldBlockSize().ToInt3());
                    }
                    else
                    {
                        Debug.LogError(MyWorld.name + " does not have an overlay grid.");
                    }
                }
            }
            else if (MyToggle.name == "SprayToggle")
            {
                IsSpray = MyToggle.isOn;
            }
            else if (MyToggle.name == "PaintOverToggle")
            {
                IsPaintOver = MyToggle.isOn;
            }
        }

        private void CreateModelFromSelection()
        {

        }

        /// <summary>
        /// Use a button
        /// </summary>
        public override void UseInput(Button MyButton)
        {
            if (UseInputTool(MyButton))
            {
            }
            else if (MyButton.name == "DecreaseSizeButton")
            {
                PaintSize--;
                OnUpdatedPaintSize();
            }
            else if (MyButton.name == "IncreaseSizeButton")
            {
                PaintSize++;
                OnUpdatedPaintSize();
            }
            else if (MyButton.name == "IncreaseVoxelIndex")
            {
                GetDropdown("VoxelDropdown").value++;
                //MyVoxelName = MyWorld.MyLookupTable.GetName();
            }
            else if (MyButton.name == "DecreaseVoxelIndex")
            {
                GetDropdown("VoxelDropdown").value--;
            }
            // If selected
            else if (MyWorld)
            {
                if (MyButton.name == "ModelLoadButton")
                {
                    MyWorld.RunScript(FileUtil.ConvertToList(DataManager.Get().Get(DataFolderNames.VoxelModels, GetDropdown("ModelLoadDropdown").value)));
                }
                else if (MyButton.name == "CutToOtherModelButton")
                {
                   // CreateModelFromSelection();
                    MyWorld.ApplyAction("CutToNewModel", GetSelectedBlockPositions());
                    DeselectAll();
                }
                else if (MyButton.name == "DeleteSelectedButton")
                {
                    MyWorld.ApplyAction("Erase", GetSelectedBlockPositions());
                }
                else if (MyButton.name == "SelectNeighborsByColorButton")
                {
                    SelectNeighborsByColor();
                }
                else if (MyButton.name == "SelectNeighborsBySolidButton")
                {
                    SelectNeighborsBySolid();
                }
                else if (MyButton.name == "CropSelectedButton")
                {
                    CropSelected();
                }
                else if (MyButton.name == "ForceRefreshButton")
                {
                    MyWorld.ForceRefresh();
                }

                else if (MyButton.name == "SetColorButton")
                {
                    MyWorld.ApplyAction("Color", GetSelectedBlockPositions(), MyTintingColor);
                }
                else if (MyButton.name == "IncreaseWorldSizeX")
                {
                    if (MyWorld.GetWorldSizeChunks().x < MaxWorldSize.x)
                    {
                        MyWorld.SetWorldSize(
                            new Vector3(MyWorld.GetWorldSizeChunks().x + 1, MyWorld.GetWorldSizeChunks().y, MyWorld.GetWorldSizeChunks().z),
                            (ResizedWorld) =>
                            {
                                OnUpdateSize();
                            });
                    }
                }
                else if (MyButton.name == "DecreaseWorldSizeX")
                {
                    if (MyWorld.GetWorldSizeChunks().x != 1)
                    {
                        MyWorld.SetWorldSize(
                            new Vector3(MyWorld.GetWorldSizeChunks().x - 1, MyWorld.GetWorldSizeChunks().y, MyWorld.GetWorldSizeChunks().z),
                            (ResizedWorld) =>
                            {
                                OnUpdateSize();
                            });
                        ;
                    }
                }
                else if (MyButton.name == "IncreaseWorldSizeY")
                {
                    if (MyWorld.GetWorldSizeChunks().y < MaxWorldSize.y)
                    {
                        MyWorld.SetWorldSize(
                            new Vector3(MyWorld.GetWorldSizeChunks().x, MyWorld.GetWorldSizeChunks().y + 1, MyWorld.GetWorldSizeChunks().z),
                            (ResizedWorld) =>
                            {
                                OnUpdateSize();
                            });
                    }
                }
                else if (MyButton.name == "DecreaseWorldSizeY")
                {
                    if (MyWorld.GetWorldSizeChunks().y != 1)
                    {
                        MyWorld.SetWorldSize(
                            new Vector3(MyWorld.GetWorldSizeChunks().x, MyWorld.GetWorldSizeChunks().y - 1, MyWorld.GetWorldSizeChunks().z),
                            (ResizedWorld) =>
                            {
                                OnUpdateSize();
                            });
                    }
                }
                else if (MyButton.name == "IncreaseWorldSizeZ")
                {
                    if (MyWorld.GetWorldSizeChunks().z < MaxWorldSize.z)
                    {
                        MyWorld.SetWorldSize(
                            new Vector3(MyWorld.GetWorldSizeChunks().x, MyWorld.GetWorldSizeChunks().y, MyWorld.GetWorldSizeChunks().z + 1),
                            (ResizedWorld) =>
                            {
                                OnUpdateSize();
                            });
                    }
                }
                else if (MyButton.name == "DecreaseWorldSizeZ")
                {
                    if (MyWorld.GetWorldSizeChunks().z != 1)
                    {
                        MyWorld.SetWorldSize(
                            new Vector3(MyWorld.GetWorldSizeChunks().x, MyWorld.GetWorldSizeChunks().y, MyWorld.GetWorldSizeChunks().z - 1),
                            (ResizedWorld) =>
                            {
                                OnUpdateSize();
                            });
                    }
                }
                else if (MyButton.name == "FlipYButton")
                {
                    MyWorld.Flip("FlipY");
                }
                else if (MyButton.name == "FlipXButton")
                {
                    MyWorld.Flip("FlipX");
                }
                else if (MyButton.name == "FlipZButton")
                {
                    MyWorld.Flip("FlipZ");
                }
                else if (MyButton.name == "SphereButton")
                {
                    VoxelPrimitives.Get().MyWorld = MyWorld;
                    VoxelPrimitives.Get().CreateSphere();
                }
                else if (MyButton.name == "CubeButton")
                {
                    VoxelPrimitives.Get().MyWorld = MyWorld;
                    VoxelPrimitives.Get().CreateCube();
                }
                else if (MyButton.name == "ClearButton")
                {
                    MyWorld.Clear();
                }
                else if (MyButton.name == "MoveLeft")
                {
                    MyWorld.ApplyAction("MoveLeft", GetSelectedBlockPositions());
                    MoveSelection("MoveLeft");
                }
                else if (MyButton.name == "MoveRight")
                {
                    MyWorld.ApplyAction("MoveRight", GetSelectedBlockPositions());
                    MoveSelection("MoveRight");
                }
                else if (MyButton.name == "MoveForward")
                {
                    MyWorld.ApplyAction("MoveForward", GetSelectedBlockPositions());
                    MoveSelection("MoveForward");
                }
                else if (MyButton.name == "MoveBack")
                {
                    MyWorld.ApplyAction("MoveBack", GetSelectedBlockPositions());
                    MoveSelection("MoveBack");
                }
                else if (MyButton.name == "MoveUp")
                {
                    MyWorld.ApplyAction("MoveUp", GetSelectedBlockPositions());
                    MoveSelection("MoveUp");
                }
                else if (MyButton.name == "MoveDown")
                {
                    MyWorld.ApplyAction("MoveDown", GetSelectedBlockPositions());
                    MoveSelection("MoveDown");
                }
            }
        }

        /// <summary>
        /// Fill the drop downs with data
        /// </summary>
        public override void FillDropdown(Dropdown MyDropdown)
        {
            base.FillDropdown(MyDropdown);
            List<string> MyNames = new List<string>();
            if (MyDropdown.name == "VoxelDropdown")
            {
                // don't add air!
                for (int i = 1; i < VoxelManager.Get().MyMetas.Count; i++)
                {
                    MyNames.Add(VoxelManager.Get().GetMeta(i).Name);
                }
                FillDropDownWithList(MyDropdown, MyNames);
                MyDropdown.value = 0;
                if (VoxelManager.Get().MyMetas.Count > 0)
                {
                    VoxelMeta MyMeta = VoxelManager.Get().GetMeta(MyDropdown.value + 1);
                    MyVoxelModelViewer.LoadVoxelMesh(MyMeta);
                }
            }
            if (MyDropdown.name == "ModelLoadDropdown")
            {
                MyNames.AddRange(DataManager.Get().GetNames(DataFolderNames.VoxelModels));
                FillDropDownWithList(MyDropdown, MyNames);
            }
        }
        #endregion

        #region PaintController
        void OnUpdatedPaintSize()
        {
            PaintSize = Mathf.Clamp(PaintSize, 0, 5);
            GetLabel("PaintSizeLabel").text = "" + (1 + PaintSize);
        }
        #endregion

        #region SizeController
        /// <summary>
        /// Updates the grid and the UI Labels
        /// Remember to cconnect this with the world updating
        /// </summary>
        void OnUpdateSize()
        {
            /*if (MyWorld.GetComponent<GridOverlay>().GetState())
            {
                MyWorld.GetComponent<GridOverlay>().GenerateLines(MyWorld.WorldSize * Chunk.ChunkSize);
            }*/
            GridOverlay MyOverlay = MyWorld.GetComponent<GridOverlay>();
            if (MyOverlay)
            {
                MyOverlay.IsForce = true;
                MyOverlay.SetState(
                    GetToggle("GridToggle").isOn, 
                    MyWorld.GetWorldBlockSize().ToInt3());
            }
            GetLabel("MySizeLabelX").text = "[" + MyWorld.GetWorldSizeChunks().x + "]";
            GetLabel("MySizeLabelY").text = "[" + MyWorld.GetWorldSizeChunks().y + "]";
            GetLabel("MySizeLabelZ").text = "[" + MyWorld.GetWorldSizeChunks().z + "]";
        }
        #endregion

        #region Utility
        /// <summary>
        /// For each selected block, selects all neighbors of same colour
        /// </summary>
        private void SelectNeighborsByColor()
        {
            List<Int3> SelectedBlocks = GetSelectedBlockPositions();
            for (int i = 0; i < SelectedBlocks.Count; i++)
            {
                List<Int3> MyNeighbors = new List<Int3>();
                MyWorld.GetNeighborsByColor(SelectedBlocks[i], MyWorld.GetVoxel(SelectedBlocks[i]).GetColor(), MyNeighbors);
                SelectVoxels(MyNeighbors);
            }
        }
        /// <summary>
        /// For each selected block, selects all neighbors of same colour
        /// </summary>
        private void SelectNeighborsBySolid()
        {
            List<Int3> SelectedBlocks = GetSelectedBlockPositions();
            for (int i = 0; i < SelectedBlocks.Count; i++)
            {
                List<Int3> MyNeighbors = new List<Int3>();
                MyWorld.GetNeighborsBySolid(SelectedBlocks[i], MyNeighbors);
                SelectVoxels(MyNeighbors);
            }
        }

        public void CropSelected()
        {
            MyWorld.CropSelected(GetSelectedBlockPositions());
            DeselectAll();
        }
        #endregion
    }
}