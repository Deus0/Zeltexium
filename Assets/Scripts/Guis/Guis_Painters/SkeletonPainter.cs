using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zeltex.Skeletons;
using Zeltex.Characters;
using Zeltex.Util;
using Zeltex.Voxels;
using Zeltex.AnimationUtilities;
using Zeltex.Generators;
using Zeltex.Physics;

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// The type of brush the skeleton painter will use
    /// </summary>
    public enum SkeletonPaintType
    {
        None,       // nothing!
        Link,       // Select a Skeleton!!
        Select,     // Selects a transform!
        Move,       // move the transform
        Scale,      // scale the transform
        Rotate,     // rotate the transform
        Zoom,       // Zoom in and out from skeleton
        Pan,        // pan the camera, moves position
        Orbit,       // orbits the camera around skeleton
        Spawn      // spawn a new joint
    }
    /// <summary>
    /// Paint on any skeleton in the map
    /// Edit their bones, transforms, meta data
    /// Edit their textures - for when their bodies are selected
    /// Edit them in the level editor or inside the skeleton viewer
    /// Functions:
    ///     Select Skeleton Bone
    ///     Moves Skeleton Bone/Mesh - on mouse click, begin moving, when release, stop moving
    /// To Do:
    ///     When moving - have a editingDefaultPose toggle on
    ///     A button that restores default pose at any time
    ///     When saving - saves the default bone poses
    ///     
    ///     Select Skeleton In Map
    ///     MeshColliders on/off in map skeletons (this will be a problem for collisions)
    ///     Select Skeleton
    ///     Render Selection Cube around Selected bone / Mesh
    ///     Render Selection lines around Selected Skeleton - when linking a skeleton
    ///     
    ///     Rotate Bone
    ///     Scale Bone
    ///     Turn Independent moving/scaling/rotation on(will counter scale children)
    ///     Create Bone at select position around selected bone, or just add to root bone of selected skeleton
    /// 
    ///     Bone Pose - restore pose on time = 0
    ///     Give each animation a totaltime, updating animation can update the total time
    /// </summary>
    public class SkeletonPainter : GuiBasic
    {
        #region Variables
        [Header("SkeletonPainter")]
        public SkeletonPaintType PaintType;         // interaction state
        // selected things
        [SerializeField]
        private SkeletonHandler MySkeleton;                 // skeleton that we are editing
        [SerializeField]
        private Bone SelectedBone;
        [Header("Options")]
        public Color32 SelectedJointColor = Color.green;
        public Color32 SelectedMeshColor = new Color32(200, 255, 200, 255);
        [Header("References")]
        [SerializeField]
        private AnimationTimeline MyTimeline;
        // public ModelMaker MyModelMaker;
        // Internal Options
        private bool IsEditDefaultPose;             // also effects teh default bone positions
        //private bool IsGridSnap;                    // grid snap starts off
        // internal states
        private bool IsIndependentMode;
        // -Ray Casting-
        [SerializeField]
        private bool DidRayHitSkeleton;
        [SerializeField]
        private ObjectViewer MySkeletonViewer;
        [SerializeField]
        private bool DidRayHitViewer;
        private RaycastHit MyHit;
        // Transforms
        //[SerializeField]
        private Transform SelectedTransform;
        private float SelectedDistance;
        // -Moving-
        private bool IsMoving;
        private Vector3 MovementOffset = Vector3.zero;
        // -Rotating-
        private bool IsRotating;    // the rotation of a transform in update function
        // -Scale-
        private bool IsUniformScale = true;
        private bool IsScaling;     // is scaling depending on mouse position difference
        //private Vector3 OriginalScale = new Vector3(1, 1, 1);
        private Vector3 LastMousePosition;
        static float FloatingPointRounder = 100000f;
        // Copy Paste
        private List<string> MyCopyData = new List<string>();
        // Camera Tools
        // connect with camera class which has the functions there (using input etc)

        #endregion

        #region Mono
        private void Start()
        {
            SelectBone(null, true);
            SelectSkeleton(null, true);
            if (MyTimeline)
            {
                MyTimeline.OnBegin();
            }
        }

        private void Update()
        {
            if (PaintType != 0)
            {
                Raycast();
            }
            if ((DidHitGui == false || DidRayHitViewer == true) && DidRayHitSkeleton == false)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (PaintType == SkeletonPaintType.Spawn && MySkeleton != null)
                    {
                        Transform BoneTransform = null;
                        if (SelectedBone != null)
                        {
                            BoneTransform = SelectedBone.MyTransform;
                        }
                        Bone MyBone = MySkeleton.GetSkeleton().CreateBone(BoneTransform);
                        if (MyBone != null)
                        {
                            SelectedTransform = BoneTransform;
                            Ray MyRay = GetRay(Input.mousePosition);
                            SelectedDistance = Vector3.Distance(MyRay.origin, MyBone.MyTransform.position);
                            SelectedTransform.position = MyRay.origin + MyRay.direction * SelectedDistance;
                            MovementOffset.Set(0, 0, 0);
                            SelectBone(MyBone);
                            IsMoving = true;
                        }
                        else
                        {
                            Debug.LogError("Could not create bone.");
                        }
                        /*if (SelectedBone.MyTransform)
                        {
                        }
                        else
                        {
                        }*/
                    }
                }
            }
            UpdateTransformPosition();
            UpdateTransformScale();
            UpdateTransformRotation();
            AlterBrush();
            HandleHotkeys();
            /*if (MySkeleton == null)
            {
                SetPaintTypeInternal(SkeletonPaintType.Link);
            }*/
            if (PaintType == SkeletonPaintType.Spawn && MySkeleton != null && SelectedBone == null)
            {

            }
            LastMousePosition = Input.mousePosition;
        }
        #endregion

        #region Input

        private void HandleHotkeys()
        {
            if (MySkeleton != null && !GUIUtil.IsInputFieldFocused())
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    SetPaintTypeInternal(SkeletonPaintType.None);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    SetPaintTypeInternal(SkeletonPaintType.Link);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    SetPaintTypeInternal(SkeletonPaintType.Select);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    SetPaintTypeInternal(SkeletonPaintType.Move);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    SetPaintTypeInternal(SkeletonPaintType.Scale);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    SetPaintTypeInternal(SkeletonPaintType.Rotate);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha7))
                {
                    SetPaintTypeInternal(SkeletonPaintType.Pan);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha8))
                {
                    SetPaintTypeInternal(SkeletonPaintType.Zoom);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha9))
                {
                    SetPaintTypeInternal(SkeletonPaintType.Orbit);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    SetPaintTypeInternal(SkeletonPaintType.Spawn);
                }
            }
        }

        /// <summary>
        /// Called when mouse dragging - click starts when initially clicked on the transform object
        /// </summary>
        /*protected void OnDragged(Vector3 MousePosition, Vector3 MousePositionDifference)
        {
            if (MySkeleton != null)
            {
                if (PaintType == SkeletonPaintType.Move)
                {
                    //ObjectFollowMouse(MousePosition);
                    //OnUpdateTransform();
                }
                else if (PaintType == SkeletonPaintType.Scale)
                {
                    //ScaleSelected(MousePositionDifference);
                    //OnUpdateTransform();
                }
                else if (PaintType == SkeletonPaintType.Rotate)
                {
                    //ObjectRotateMouse(MousePositionDifference);
                    //OnUpdateTransform();
                }
            }
        }*/
        #endregion

        #region Raycasting
        bool DidHitGui;
        /// <summary>
        /// First raycast for guis
        /// Then raycast for worlds
        /// </summary>
        private void Raycast()
        {
            // set highlighting to false
            DidRayHitSkeleton = false;
            DidRayHitViewer = false;
            DidHitGui = RaycastViewer();   // did ray hit any gui
            if (DidHitGui == false)
            {
                //RaycastWorld();
            }
            if (Input.GetMouseButtonDown(0) && DidRayHitSkeleton == false)
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
                if (MyResults[i].gameObject.GetComponent<ObjectViewer>())
                {
                    DidRayHitViewer = true;
                    if (MySkeletonViewer != MyResults[i].gameObject.GetComponent<ObjectViewer>())
                    {
                        MySkeletonViewer = MyResults[i].gameObject.GetComponent<ObjectViewer>();
                        // Have to raycast
                        Debug.Log("New SkeletonViewer Selected: " + MySkeletonViewer.name);
                    }
                    DidRayHitSkeleton = MySkeletonViewer.GetRayHitInViewer(Input.mousePosition, out MyRay, out MyHit);
                    if (DidRayHitSkeleton)
                    {
                        OnSkeletonHit(MyRay, MyHit);
                    }
                   /* else
                    {
                        Debug.LogError("Ray missed skeleton.");
                    }*/
                    break;
                }
            }
            return (MyResults.Count != 0);
        }

        public static GameObject FindRootSkeleton(Transform MyObject)
        {
            if (MyObject == null)
            {
                return null;
            }
            if (MyObject.gameObject.GetComponent<SkeletonHandler>())
            {
                return MyObject.gameObject;
            }
            else
            {
                return FindRootSkeleton(MyObject.parent);
            }
        }
        /// <summary>
        /// When a skeleton is hit with a ray, check input
        /// </summary>
        private void OnSkeletonHit(Ray MyRay, RaycastHit MyHit)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // only select these two!
                Chunk HitChunk = MyHit.collider.gameObject.GetComponent<Chunk>();
                if (HitChunk)
                {
                    Character MyCharacterObject = MyHit.collider.transform.FindRootCharacter();
                    if (MyCharacterObject)
                    {
                        // move mesh
                        SelectedTransform = HitChunk.GetWorld().transform;
                    }
                    else
                    {
                        GameObject MySkeleton = FindRootSkeleton(MyHit.collider.transform);
                        if (MySkeleton)
                        {
                            SelectedTransform = HitChunk.GetWorld().transform;
                        }
                    }
                }
                else if (MyHit.collider.gameObject.name.Contains("Joint"))
                {
                    // move joint
                    SelectedTransform = MyHit.collider.gameObject.transform.parent;
                }
                else
                {
                    SelectedTransform = null;
                    //return (MyResults.Count != 0);  // return cannot move!
                }
                if (PaintType == SkeletonPaintType.Link)
                {
                    Character MyCharacterObject = MyHit.collider.transform.FindRootCharacter();
                    if (MyCharacterObject)
                    {
                        Debug.LogError("Selecting Skeleton");
                        SelectSkeleton(MyCharacterObject.GetSkeleton());//MySkeletonViewer.GetSpawn().GetComponent<Skeleton>());
                        //HighlightCharacter(MyCharacterObject.transform);
                    }
                    else
                    {
                        GameObject MySkeleton = FindRootSkeleton(MyHit.collider.transform);
                        if (MySkeleton)
                        {
                            //SelectedTransform = HitChunk.GetWorld().transform;
                            SelectSkeleton(MySkeleton.GetComponent<SkeletonHandler>());
                        }
                        else
                        {
                            Debug.LogError("Hit object: " + MyHit.collider.transform.name + " has no character or skeleton root");
                        }
                    }
                    // Get Selected Skeleton from selected Transform
                }
                else if (PaintType == SkeletonPaintType.Select)
                {
                    if (MySkeleton) // can only select bones if skeleton selected
                    {
                        if (MyHit.collider.gameObject.GetComponent<Chunk>())
                        {
                            // hit mesh
                            SelectMesh(MyHit.collider.gameObject);
                        }
                        else if (MyHit.collider.gameObject.GetComponent<CapsuleCollider>())
                        {
                            SelectCapsule(MyHit.collider.gameObject);
                        }
                        else
                        {
                            // hit bone
                            SelectJoint(MyHit.collider.gameObject);
                        }
                    }
                }
                else if (PaintType == SkeletonPaintType.Move)
                {
                    //Camera MyCamera = GetSkeletonCamera();
                    SelectedDistance = Vector3.Distance(MyRay.origin, MyHit.point);
                    MovementOffset = SelectedTransform.transform.position - MyHit.point; //(MyCamera.transform.position + MyCamera.transform.forward * SelectedDistance)
                    // IsMoving is true until mouse press up
                    IsMoving = true; 
                    //Vector3 TestPosition = (MyRay.origin + MyRay.direction * SelectedDistance);
                    //Debug.LogError("Compare - [" +
                    //    TestPosition.x + "," + TestPosition.y + "," + TestPosition.z + "] : [" +
                    //    MyHit.point.x + "," + MyHit.point.y + "," + MyHit.point.z + "]");
                }
                else if (PaintType == SkeletonPaintType.Rotate)
                {
                    GetCamera();
                    SelectedDistance = Vector3.Distance(MyRay.origin, MyHit.collider.transform.position);
                    IsRotating = true;    // until mouse press up
                }
                else if (PaintType == SkeletonPaintType.Scale)
                {
                    GetCamera();
                    SelectedDistance = Vector3.Distance(MyRay.origin, MyHit.collider.transform.position);
                    //OriginalScale = SelectedTransform.localScale;
                    IsScaling = true;
                }
            }
        }

        private Vector3 RoundPosition(Vector3 Input)
        {
            return new Vector3(Mathf.RoundToInt(Input.x * FloatingPointRounder) / FloatingPointRounder,
                               Mathf.RoundToInt(Input.y * FloatingPointRounder) / FloatingPointRounder,
                               Mathf.RoundToInt(Input.z * FloatingPointRounder) / FloatingPointRounder);
        }

        /// <summary>
        /// Raycast world for skeleton!
        /// </summary>
        private void RaycastWorld()
        {

        }
        #endregion

        #region Mouse
        /// <summary>
        /// internally set paint type
        /// </summary>
        private void SetPaintTypeInternal(SkeletonPaintType NewType)
        {
            GetDropdown("PaintTypeDropdown").value = (int)NewType;
            SetPaintType(NewType);
        }
        /// <summary>
        ///  set paint type
        /// </summary>
        /// <param name="NewPaintType"></param>
        private void SetPaintType(SkeletonPaintType NewPaintType)
        {
            if (PaintType != NewPaintType)
            {
                /*if (PaintType == VoxelPaintType.Select)
                {
                    DeselectAll();
                }*/
                PaintType = NewPaintType;
            }
        }

        private void AlterBrush()
        {
            if (PaintType != SkeletonPaintType.None)
            {
                if (PaintType == SkeletonPaintType.Select)
                {
                    MouseCursor.Get().SetMouseIcon("SkeletonPainterSelect");
                }
                else if (PaintType == SkeletonPaintType.Link)
                {
                    MouseCursor.Get().SetMouseIcon("SkeletonPainterLink");
                }

                else if (PaintType == SkeletonPaintType.Move)
                {
                    MouseCursor.Get().SetMouseIcon("SkeletonPainterMove");
                }
                else if (PaintType == SkeletonPaintType.Scale)
                {
                    MouseCursor.Get().SetMouseIcon("SkeletonPainterScale");
                }
                else if (PaintType == SkeletonPaintType.Rotate)
                {
                    MouseCursor.Get().SetMouseIcon("SkeletonPainterRotate");
                }

                else if (PaintType == SkeletonPaintType.Spawn)
                {
                    MouseCursor.Get().SetMouseIcon("SkeletonPainterSpawn");
                }

                else if (PaintType == SkeletonPaintType.Zoom)
                {
                    MouseCursor.Get().SetMouseIcon("SkeletonPainterZoom");
                }
                else if (PaintType == SkeletonPaintType.Pan)
                {
                    MouseCursor.Get().SetMouseIcon("SkeletonPainterPan");
                }
                else if (PaintType == SkeletonPaintType.Orbit)
                {
                    MouseCursor.Get().SetMouseIcon("SkeletonPainterOrbit");
                }
            }
            else
            {
                MouseCursor.Get().SetMouseIcon("DefaultMouse");
            }
        }
        #endregion

        #region Linking

        /// <summary>
        /// When selecting new skeleton
        /// Update the bones list
        /// Update statistics (bones, meshes, IKs)
        /// </summary>
        private void SelectSkeleton(SkeletonHandler NewSkeleton, bool IsForceSelect = false)
        {
            if (MySkeleton != NewSkeleton || IsForceSelect)
            {
                ReleaseSkeleton();
                MySkeleton = NewSkeleton;
                // Turn off AnimationController! (make one of these!)
                FillList(GetListHandler("BonesList"));  // refresh bones list
                // gui for skeleton data
                if (MySkeleton)
                {
                    OnSelectedSkeleton();
                }
                else
                {
                    OnDeSelectedSkeleton();
                }
                SetSkeletonCapsuleUI();
                // Update statistics!
                UpdateStatistics();
                // Disable movement! make kinematic!
            }
        }

        /// <summary>
        /// Sets the to null, while releasing the old one
        /// </summary>
        private void ReleaseSkeleton()
        {
            if (MySkeleton != null)
            {
                // Deselect skeleton
                // for example, hide bones! hide grid!
                // Restore bot movement!
                MySkeleton.GetComponent<GridOverlay>().SetState(false);
                MySkeleton.GetSkeleton().SetXRay(false);
                MySkeleton.GetSkeleton().SetMeshVisibility(true);
                MySkeleton.GetSkeleton().SetBoneVisibility(false);
                MySkeleton.GetSkeleton().SetJointColliders(false);
                MySkeleton.GetSkeleton().SetCapsuleCollider(false);
                MySkeleton.GetSkeleton().SetMeshColliders(true);
                MySkeleton.GetSkeleton().SetConvex(true);
                if (MySkeleton.transform.parent != null)
                {
                    CapsuleCollider MyCapsule = MySkeleton.transform.parent.gameObject.GetComponent<CapsuleCollider>();
                    if (MyCapsule)
                    {
                        MyCapsule.enabled = true;
                    }
                }
                Zanimator OldAnimator = MySkeleton.GetComponent<Zanimator>();
                if (OldAnimator && MyTimeline)
                {
                    MyTimeline.RemoveAnimator(OldAnimator);
                    GetInput("TimeInput").interactable = false;
                    GetInput("TimeInput").text = "";
                }
                MySkeleton = null;
            }
        }

        private void OnSelectedSkeleton()
        {
            Zanimator MyAnimator = MySkeleton.GetComponent<Zanimator>();
            if (GetInput("TimeInput"))
            {
                GetInput("TimeInput").interactable = true;
                if (MyAnimator)
                {
                    GetInput("TimeInput").text = "" + MyAnimator.GetTimeLength();
                }
            }
            if (MySkeleton.GetComponent<GridOverlay>())
            {
                MySkeleton.GetComponent<GridOverlay>().SetState(GetToggle("GridLinesToggle").isOn);
            }
            MySkeleton.GetSkeleton().ShowJoints();
            MySkeleton.GetSkeleton().SetXRay(GetToggle("XRayToggle").isOn);
            MySkeleton.GetSkeleton().SetMeshVisibility(GetToggle("MeshVisibilityToggle").isOn);
            MySkeleton.GetSkeleton().SetBoneVisibility(GetToggle("BoneVisibilityToggle").isOn);
            
            //Debug.LogError(MySkeleton.name + " is getting colliders set: " + SelectionType.ToString());
            int CollisionLayer = GetDropdown("SelectionDropdown").value;
            MySkeleton.GetSkeleton().SetConvex(false);
            MySkeleton.GetSkeleton().SetJointColliders(CollisionLayer == 0);    // value is 0
            MySkeleton.GetSkeleton().SetMeshColliders(CollisionLayer == 1);        // value is 1
            MySkeleton.GetSkeleton().SetCapsuleCollider(CollisionLayer == 2);
            if (MySkeleton.transform.parent != null)
            {
                CapsuleCollider MyCapsule = MySkeleton.transform.parent.gameObject.GetComponent<CapsuleCollider>();
                if (MyCapsule)
                {
                    MyCapsule.enabled = false;
                }
            }

            if (MyTimeline)
            {
                MyTimeline.SetAnimator(MyAnimator);
                MyTimeline.ForceUpdated();
                GetInput("TimeInput").interactable = true;
            }
            if (MyAnimator)
            {
                MyAnimator.Stop();
            }
            SetPaintTypeInternal(SkeletonPaintType.Select);
            GetButton("CopyButton").interactable = true;
            GetButton("PasteButton").interactable = (MyCopyData.Count != 0);
        }

        private void OnDeSelectedSkeleton()
        {
            if (GetInput("TimeInput"))
            {
                GetInput("TimeInput").interactable = false;
                GetInput("TimeInput").text = "";
            }
            GetButton("CopyButton").interactable = false;
            GetButton("PasteButton").interactable = false;
            SelectBone(null);
        }

        /// <summary>
        /// provides statistics on a selected skeleton
        /// </summary>
        private void UpdateStatistics()
        {
            List<string> MyStatistics = new List<string>();
            if (MySkeleton)
            {
                MyStatistics.Add(MySkeleton.name);
                MyStatistics.Add("Bones Count [" + MySkeleton.GetBones().Count + "]");
            }
            else
            {
                MyStatistics.Add("No Skeleton Selected");
            }
            GetLabel("StatisticsText").text = FileUtil.ConvertToSingle(MyStatistics);
        }
        #endregion

        #region Bone

        /// <summary>
        /// Selects a bone using the joint object attached to the bone transform
        /// </summary>
        private void SelectJoint(GameObject MyObject)
        {
            bool WasFound = false;
            if (MyObject.name.Contains("Joint"))
            {
                // make sure use parent bone for this, instead of the helper mesh [Joint X]
                MyObject = MyObject.transform.parent.gameObject;
                Debug.Log("Selected new joint: " + MyObject.name);
                for (int i = 0; i < MySkeleton.GetBones().Count; i++)
                {
                    if (MyObject.transform == MySkeleton.GetBones()[i].MyTransform)   // Selecting Bone
                    {
                        SelectBone(MySkeleton.GetBones()[i]);
                        WasFound = true;
                        break;
                    }
                }
                if (WasFound == false)
                {
                     Debug.LogError("No Joint Selected: " + MyObject.name);
                    //SelectedBone = null;
                    //SelectBone(new Bone());
                }
            }
        }

        /// <summary>
        /// return the selected bone
        /// </summary>
        public Transform GetSelectedBone()
        {
            if (SelectedBone != null)
            {
                return SelectedBone.MyTransform;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Select a bone by a name
        /// </summary>
        private void SelectBoneByName(string MyName)
        {
            if (MySkeleton)
            {
                for (int i = 0; i < MySkeleton.GetBones().Count; i++)
                {
                    if (MySkeleton.GetBones()[i].Name == MyName)
                    {
                        SelectBone(MySkeleton.GetBones()[i]);
                        break;
                    }
                }

            }
        }

        /// <summary>
        /// Select a new bone!
        /// </summary>
        private void SelectBone(Bone NewBone, bool IsForceSelect = false)
        {
            if (SelectedBone != NewBone || IsForceSelect)
            {
                ReleaseBone();
                SelectedBone = NewBone;
                if (SelectedBone != null && SelectedBone.MyTransform != null)
                {
                    OnSelectedBone();
                }
                else
                {
                    OnDeselectedBone();
                }
            }
        }

        private void ReleaseBone()
        {
            if (SelectedBone != null)
            {
                if (MySkeleton && SelectedBone.MyJointCube)
                {
                    MySkeleton.GetSkeleton().RestoreBoneColor(SelectedBone);
                }
                SelectedBone = null;
            }
        }

        private void OnSelectedBone()
        {
            if (SelectedBone.MyJointCube)
            {
                SelectedBone.MyJointCube.GetComponent<MeshRenderer>().material.color = SelectedJointColor;
            }
            /*InputBoneName.interactable = true;
            InputTag.interactable = true;
            InputTag.text = SelectedBone.Tag;
            InputBoneName.text = SelectedBone.Name;*/
            MyTransformHandler.Set(SelectedBone.MyTransform);
            MyTimeline.RefreshCurveTicks();
            GetButton("SpawnSphereButton").interactable = true;
            GetButton("SpawnModelButton").interactable = true;
            GetButton("DeleteSelectedMeshButton").interactable = SelectedBone.HasMesh();
        }

        [SerializeField]
        private TransformHandler MyTransformHandler;
        private void SelectCapsule(GameObject MyObject)
        {
            if (MyObject == MySkeleton.GetSkeleton().GetCapsule() && MySkeleton.GetSkeleton().GetCapsuleRenderer())
            {
                //MySkeleton.GetCapsuleRenderer().enabled = true;
                MySkeleton.GetSkeleton().GetCapsuleRenderer().material.color = SelectedJointColor;
                //MyTransformHandler.Set(MyObject.transform);
            }
            //MyObject.GetComponent<MeshRenderer>().material.color = MySkeleton.BoneColor;
        }

        private void OnDeselectedBone()
        {
            // interactable = false for bone transforms
            GetButton("SpawnSphereButton").interactable = false;
            GetButton("SpawnModelButton").interactable = false;
            GetButton("DeleteSelectedMeshButton").interactable = false;
        }

        /// <summary>
        /// Returns a skeleton if one is selected
        /// </summary>
        public SkeletonHandler GetSelectedSkeleton()
        {
            return MySkeleton;
        }
        #endregion

        #region BoneMesh

        /// <summary>
        /// Selects a mesh
        /// </summary>
        private void SelectMesh(GameObject MyObject)
        {
            if (MyObject.GetComponent<Chunk>() && MyObject.GetComponent<World>() == null)
            {
                MyObject = MyObject.GetComponent<Chunk>().GetWorld().gameObject;
            }
           // World MyWorld = MyObject.GetComponent<World>();
            bool WasFound = false;
            for (int i = 0; i < MySkeleton.GetBones().Count; i++)
            {
                if (MyObject.transform == MySkeleton.GetBones()[i].VoxelMesh) // Selecting mesh
                {
                    SelectBone(MySkeleton.GetBones()[i]);
                    WasFound = true;
                    break;
                }
            }
            if (WasFound == false)
            {
                SelectBone(new Bone());
            }
        }


        /// <summary>
        /// Create a sphere onto the selected bone
        /// </summary>
        private void DestroyBoneMesh()
        {
            if (SelectedBone != null && SelectedBone.HasMesh())
            {
                SelectedBone.DestroyAttachedMesh();
                GetButton("DeleteSelectedMeshButton").interactable = false;
            }
        }

        /// <summary>
        /// Create a sphere onto the selected bone
        /// </summary>
        private void CreateSphereOnMesh()
        {
            if (SelectedBone != null)
            {
                string MySphereData = PolyModelGenerator.Get().GetSphere();
                SelectedBone.CreateMesh(MySphereData);
                //MySkeleton.GetSkeleton().CreateMesh(SelectedBone, MySphereData);
                GetButton("DeleteSelectedMeshButton").interactable = true;
            }
        }

        private void CreateModelOnMesh()
        {
            if (SelectedBone != null)
            {
                //string VoxelData = DataManager.Get().Get(DataFolderNames.PolyModels, GetDropdown("PolyModelDropdown").value);
                //MySkeleton.GetSkeleton().CreateMesh(SelectedBone, VoxelData);
                //SelectedBone.CreateMesh(VoxelData);
                //GetButton("DeleteSelectedMeshButton").interactable = true;
            }
        }
        #endregion

        #region TransformPosition

        /// <summary>
        /// Uses mouse movement to reposition transform
        /// </summary>
        private void UpdateTransformPosition()
        {
            if (IsMoving)
            {
                if (Input.GetMouseButtonUp(0) || PaintType != SkeletonPaintType.Move || SelectedTransform == null)
                {
                    IsMoving = false;
                }
                else
                {
                    Vector2 MousePosition = GetLocalMousePosition(Input.mousePosition);
                    Camera MyCamera = GetCamera();
                    MouseMoveTransform(SelectedTransform, MyCamera, MousePosition);
                }
            }
        }

        /// <summary>
        /// Move the transform using a screenspace position and a camera
        /// </summary>
        private void MouseMoveTransform(Transform MyTransform, Camera MyCamera, Vector2 MousePosition)
        {
            // use viewer for world position if i have to, otherwise use normal camera for mouse position in world
            // base the distance from camera based on the object clicked on, move along the camera xy plane
            // finally set the new position of the bone t othe same distance, but in the new mouse position inside the camera screen space
            Ray CameraRay = MyCamera.ScreenPointToRay(MousePosition);
            Vector3 NewTransformPosition = CameraRay.origin + CameraRay.direction * SelectedDistance + MovementOffset;
            NewTransformPosition = new Vector3(
                Mathf.RoundToInt(NewTransformPosition.x * FloatingPointRounder) / FloatingPointRounder,
                Mathf.RoundToInt(NewTransformPosition.y * FloatingPointRounder) / FloatingPointRounder,
                Mathf.RoundToInt(NewTransformPosition.z * FloatingPointRounder) / FloatingPointRounder);
            //Debug.LogError("When Moving: " + NewTransformPosition.ToString());
            if (IsIndependentMode)
            {
                Vector3 DifferencePosition = NewTransformPosition - MyTransform.position;
                // add this to all children
                for (int i = 0; i < MyTransform.childCount; i++)
                {
                    if (IsBone(MyTransform.GetChild(i).gameObject))
                       // MyTransform.GetChild(i).name.Contains("Joint") == false &&
                        //MyTransform.GetChild(i).name.Contains("Mesh") == false)
                    {
                        MyTransform.GetChild(i).position -= DifferencePosition;
                    }
                }
            }
            MyTransform.position = NewTransformPosition;
        }
        #endregion

        #region TransformScale
        /// <summary>
        /// When Scalining call this function
        /// </summary>
        private void UpdateTransformScale()
        {
            if (IsScaling)
            {
                if (Input.GetMouseButtonUp(0) || PaintType != SkeletonPaintType.Scale || SelectedTransform == null)
                {
                    IsScaling = false;
                }
                else
                {
                    Vector3 MouseDelta = Input.mousePosition - LastMousePosition;
                    //MouseDelta.x = (Mathf.RoundToInt(MouseDelta.x / 5)) * 5;
                    //MouseDelta.y = (Mathf.RoundToInt(MouseDelta.y / 5)) * 5;
                    if (MouseDelta != new Vector3())
                    {
                        ScaleSelected(SelectedTransform);
                    }

                }
            }
        }
        /// <summary>
        /// Scale a transform!
        /// </summary>
        void ScaleSelected(Transform MyTransform)
        {
            Vector3 WorldMouseDelta = GetWorldMouseDelta();
            Vector3 ScaleValue = WorldMouseDelta;
            ScaleValue.x = Mathf.Abs(ScaleValue.x); ScaleValue.y = Mathf.Abs(ScaleValue.y); ScaleValue.z = Mathf.Abs(ScaleValue.z);
            ScaleValue += new Vector3(1, 1, 1); // base scale
            if (WorldMouseDelta.x > 0)
            {
                ScaleValue.x = 1 / ScaleValue.x;    // reverse direction
            }
            if (WorldMouseDelta.y > 0)
            {
                ScaleValue.y = 1 / ScaleValue.y;    // reverse direction
            }
            if (WorldMouseDelta.z > 0)
            {
                ScaleValue.z = 1 / ScaleValue.z;    // reverse direction
            }

            // transform direction!
            if (IsUniformScale)
            {
                ScaleValue.x = (ScaleValue.x + ScaleValue.y + ScaleValue.z) / 3f;
                ScaleValue.y = ScaleValue.x;
                ScaleValue.z = ScaleValue.x;
            }
            // GetChildren
            List<Transform> MyChildren = new List<Transform>();
            if (IsIndependentMode)
            {
                for (int i = 0; i < MyTransform.childCount; i++)
                {
                    Transform Child = MyTransform.GetChild(i);
                    if (IsBone(Child.gameObject))
                    {
                        MyChildren.Add(Child);
                    }
                }
            }
            // Scale transform
            MyTransform.localScale = new Vector3(
                        MyTransform.localScale.x * ScaleValue.x,
                        MyTransform.localScale.y * ScaleValue.y,
                        MyTransform.localScale.z * ScaleValue.z
                );
            // Scale children by reverse
            if (IsIndependentMode)
            {
                for (int i = 0; i < MyChildren.Count; i++)
                {
                    Transform Child = MyChildren[i];
                    Child.localScale = new Vector3(
                        Child.localScale.x / ScaleValue.x,
                        Child.localScale.y / ScaleValue.y,
                        Child.localScale.z / ScaleValue.z
                        );
                    //MyChildren[i].localScale /= ScaleValue;
                }
            }
        }
        #endregion

        #region TransformRotation

        private void UpdateTransformRotation()
        {
            if (IsRotating)
            {
                if (Input.GetMouseButtonUp(0) || PaintType != SkeletonPaintType.Rotate || SelectedTransform == null)
                {
                    IsRotating = false;
                }
                else
                {
                    Vector3 MouseDelta = Input.mousePosition - LastMousePosition;
                    if (MouseDelta != new Vector3())
                    {
                        RotateSelected(SelectedTransform);
                    }

                }
            }
        }

        private void RotateSelected(Transform MyTransform)
        {
            Camera SkeletonCamera = GetCamera();
            float RotationSpeed = 360;
            Vector3 WorldMouseDelta = GetWorldMouseDelta();
            WorldMouseDelta *= RotationSpeed;
            List<Transform> Children = new List<Transform>();
            List<Quaternion> ChildrenRotations = new List<Quaternion>();
            List<Vector3> ChildrenPositions = new List<Vector3>();
            if (IsIndependentMode)
            {
                for (int i = 0; i < MyTransform.childCount; i++)
                {
                    Transform Child = MyTransform.GetChild(i);
                    if (IsBone(Child.gameObject))
                    {
                        Children.Add(Child);
                        ChildrenRotations.Add(Child.rotation);
                        ChildrenPositions.Add(Child.position);
                    }
                }
            }
            //Vector3 UpDirection = SkeletonCamera.transform.InverseTransformDirection(MyTransform.up);
            //Vector3 RightDirection = SkeletonCamera.transform.InverseTransformDirection(MyTransform.right);
            //Vector3 UpDirection = (MyTransform.up);
            //Vector3 RightDirection = MyTransform.right;
            //WorldMouseDelta = MyTransform.InverseTransformDirection(WorldMouseDelta);
            Vector3 UpDirection = SkeletonCamera.transform.up;
            Vector3 LeftDirection = -SkeletonCamera.transform.right;
            MyTransform.Rotate(UpDirection, WorldMouseDelta.x);
            MyTransform.Rotate(LeftDirection, WorldMouseDelta.y);
            // draw debug lines for the rotation mouse input - the mouse world position + the Vector that i am using for rotation
            // Scale children by reverse
            if (IsIndependentMode)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].rotation = ChildrenRotations[i];
                    Children[i].position = ChildrenPositions[i];
                }
            }
        }
        #endregion

        #region UI

        /// <summary>
        /// Inputfields
        /// </summary>
        public override void UseInput(InputField MyInputField)
        {
            base.UseInput(MyInputField);
            if (MySkeleton)
            {
                if (MyInputField.name == "TimeInput")
                {
                    float NewInput = float.Parse(MyInputField.text);
                    if (NewInput >= 0 && NewInput < 15)
                    {
                        MySkeleton.GetComponent<Zanimator>().SetTimeLength(NewInput);
                    }
                    MyInputField.text = "" + MySkeleton.GetComponent<Zanimator>().GetTimeLength();
                    MyTimeline.OnUpdatedTotalTime(MySkeleton.GetComponent<Zanimator>().GetTimeLength());
                }
                else if (MyInputField.name == "CapsuleInputX")
                {
                    float NewInput = float.Parse(MyInputField.text);
                    MyInputField.text = NewInput.ToString();
                    MySkeleton.GetSkeleton().SetCapsuleCenter(new Vector3(NewInput, MySkeleton.GetSkeleton().GetCapsuleCenter().y, MySkeleton.GetSkeleton().GetCapsuleCenter().z));
                }
                else if (MyInputField.name == "CapsuleInputY")
                {
                    float NewInput = float.Parse(MyInputField.text);
                    MyInputField.text = NewInput.ToString();
                    MySkeleton.GetSkeleton().SetCapsuleCenter(new Vector3(MySkeleton.GetSkeleton().GetCapsuleCenter().x, NewInput, MySkeleton.GetSkeleton().GetCapsuleCenter().z));
                }
                else if (MyInputField.name == "CapsuleInputZ")
                {
                    float NewInput = float.Parse(MyInputField.text);
                    MyInputField.text = NewInput.ToString();
                    MySkeleton.GetSkeleton().SetCapsuleCenter(new Vector3(MySkeleton.GetSkeleton().GetCapsuleCenter().x, MySkeleton.GetSkeleton().GetCapsuleCenter().y, NewInput));
                }
                else if (MyInputField.name == "CapsuleHeightInput")
                {
                    float NewInput = float.Parse(MyInputField.text);
                    MyInputField.text = NewInput.ToString();
                    MySkeleton.GetSkeleton().SetCapsuleHeight(NewInput);
                }
                else if (MyInputField.name == "CapsuleRadiusInput")
                {
                    float NewInput = float.Parse(MyInputField.text);
                    MyInputField.text = NewInput.ToString();
                    MySkeleton.GetSkeleton().SetCapsuleRadius(NewInput);
                }
            }
        }

        private void SetSkeletonCapsuleUI()
        {
            if (MySkeleton)
            {
                GetInput("CapsuleInputX").text = MySkeleton.GetSkeleton().GetCapsuleCenter().x + "";
                GetInput("CapsuleInputY").text = MySkeleton.GetSkeleton().GetCapsuleCenter().y + "";
                GetInput("CapsuleInputZ").text = MySkeleton.GetSkeleton().GetCapsuleCenter().z + "";
                GetInput("CapsuleHeightInput").text = MySkeleton.GetSkeleton().GetCapsuleHeight() + "";
                GetInput("CapsuleRadiusInput").text = MySkeleton.GetSkeleton().GetCapsuleRadius() + "";
            }
            else
            {
                GetInput("CapsuleInputX").text = "";
                GetInput("CapsuleInputY").text = "";
                GetInput("CapsuleInputZ").text = "";
                GetInput("CapsuleHeightInput").text = "";
                GetInput("CapsuleRadiusInput").text = "";
            }
        }

        /// <summary>
        /// Use a drop down as input
        /// </summary>
        public override void UseInput(Dropdown MyDropdown)
        {
            base.UseInput(MyDropdown);
            if (MyDropdown.name == "PaintTypeDropdown")
            {
                SetPaintType((SkeletonPaintType)MyDropdown.value);
            }
            else if (MyDropdown.name == "SelectionDropdown")
            {
                if (MySkeleton)
                {
                    /*if (MyDropdown.value == 0)
                    {
                        MySkeleton.SetColliders(false);
                        MySkeleton.SetJointColliders(false);
                    }*/
                    if (MyDropdown.value == 0)
                    {
                        MySkeleton.GetSkeleton().SetMeshColliders(false);
                        MySkeleton.GetSkeleton().SetJointColliders(true);
                        MySkeleton.GetSkeleton().SetCapsuleCollider(false);
                    }
                    else if (MyDropdown.value == 1)
                    {
                        MySkeleton.GetSkeleton().SetMeshColliders(true);
                        MySkeleton.GetSkeleton().SetJointColliders(false);
                        MySkeleton.GetSkeleton().SetCapsuleCollider(false);
                    }
                    // Collider
                    else if (MyDropdown.value == 2)
                    {
                        MySkeleton.GetSkeleton().SetMeshColliders(false);
                        MySkeleton.GetSkeleton().SetJointColliders(false);
                        MySkeleton.GetSkeleton().SetCapsuleCollider(true);
                    }
                }
            }
        }

        /// <summary>
        /// Use toggle on the viewer
        /// </summary>
        public override void UseInput(Toggle MyToggle)
        {
            if (MyToggle.name == "GridSnapToggle")
            {
                //IsGridSnap = MyToggle.isOn;
            }
            else
            {
                if (MySkeleton)
                {
                    if (MyToggle.name == "GridLinesToggle")
                    {
                        MySkeleton.GetComponent<GridOverlay>().SetState(MyToggle.isOn);
                    }
                    else if (MyToggle.name == "XRayToggle")
                    {
                        // for bones, set all the shaders differently
                        MySkeleton.GetSkeleton().SetXRay(MyToggle.isOn);
                    }
                    if (MyToggle.name == "MeshVisibilityToggle")
                    {
                        MySkeleton.GetComponent<Skeleton>().SetMeshVisibility(MyToggle.isOn);
                    }
                    else if (MyToggle.name == "BoneVisibilityToggle")
                    {
                        MySkeleton.GetSkeleton().SetBoneVisibility(MyToggle.isOn);
                    }
                    else if (MyToggle.name == "IndependentModeToggle")
                    {
                        IsIndependentMode = MyToggle.isOn;
                    }
                }
            }
        }

        public override void UseInput(Button MyButton)
        {
            base.UseInput(MyButton);
            if (MyButton.name == "NoneButton")
            {
                SetPaintTypeInternal(SkeletonPaintType.None);
            }
            else if (MyButton.name == "LinkButton")
            {
                SetPaintTypeInternal(SkeletonPaintType.Link);
            }
            else if (MyButton.name == "SelectButton")
            {
                SetPaintTypeInternal(SkeletonPaintType.Select);
            }

            else if (MyButton.name == "MoveButton")
            {
                SetPaintTypeInternal(SkeletonPaintType.Move);
            }
            else if (MyButton.name == "ScaleButton")
            {
                SetPaintTypeInternal(SkeletonPaintType.Scale);
            }
            else if (MyButton.name == "RotateButton")
            {
                SetPaintTypeInternal(SkeletonPaintType.Rotate);
            }

            else if (MyButton.name == "SpawnButton")
            {
                SetPaintTypeInternal(SkeletonPaintType.Spawn);
            }

            else if (MyButton.name == "ZoomButton")
            {
                SetPaintTypeInternal(SkeletonPaintType.Zoom);
            }
            else if (MyButton.name == "PanButton")
            {
                SetPaintTypeInternal(SkeletonPaintType.Pan);
            }
            else if (MyButton.name == "OrbitButton")
            {
                SetPaintTypeInternal(SkeletonPaintType.Orbit);
            }
            else
            {
                if (MySkeleton)
                {
                    if (MyButton.name == "CreateMeshButton")
                    {
                        if (MySkeleton != null && SelectedBone != null)
                        {
                            int MeshIndex = GetDropdown("PolyModelDropdown").value;
                            //SelectedBone.CreateMesh(DataManager.Get().Get(DataFolderNames.PolyModels, MeshIndex));  //MyModelMaker.Get(MeshIndex)
                        }
                    }
                    else if (MyButton.name == "ExplodeButton")
                    {
                        if (MySkeleton.GetComponent<VoxelExplosion>() == null)
                        {
                            MySkeleton.gameObject.AddComponent<VoxelExplosion>().Explodes();
                        }
                        else
                        {
                            MySkeleton.GetComponent<VoxelExplosion>().Explodes();
                        }
                    }
                    else if (MyButton.name == "RagdollButton")
                    {
                        if (MySkeleton.GetComponent<Ragdoll>() == null)
                        {
                            MySkeleton.gameObject.AddComponent<Ragdoll>().RagDoll();
                        }
                        else
                        {
                            MySkeleton.GetComponent<Ragdoll>().RagDoll();
                        }
                    }
                    else if (MyButton.name == "RestoreDefaultPose")
                    {
                        MySkeleton.GetSkeleton().RestoreDefaultPose();
                    }
                    else if (MyButton.name == "SetDefaultPose")
                    {
                        MySkeleton.GetSkeleton().SetDefaultPose();
                    }
                    else if (MyButton.name == "CopyButton")
                    {
                        Copy();
                    }
                    else if (MyButton.name == "PasteButton")
                    {
                        Paste();
                    }
                    // Bone Buttons
                    else if (SelectedBone != null)
                    {
                        if (MyButton.name == "SpawnSphereButton")
                        {
                            CreateSphereOnMesh();
                        }
                        else if (MyButton.name == "SpawnModelButton")
                        {
                            CreateModelOnMesh();
                        }
                        else if (MyButton.name == "DeleteSelectedMeshButton")
                        {
                            DestroyBoneMesh();
                        }
                    }
                }
            }
        }

        public void Copy()
        {
            //if (MySkeleton)
            {
                //MyCopyData = MySkeleton.GetComponent<Skeleton>().GetScriptList();
                GetButton("PasteButton").interactable = true;
            }
        }

        public void Paste()
        {
            //if (MySkeleton)
            {
                if (MyCopyData.Count > 0)
                {
                    Debug.Log("Pasting copied Voxels: " + MyCopyData.Count);
                    //MySkeleton.RunScript(MyCopyData);
                    MyCopyData.Clear();
                    GetButton("PasteButton").interactable = false;
                }
            }
        }

        /// <summary>
        /// Fill the drop downs with data
        /// </summary>
        public override void FillDropdown(Dropdown MyDropdown)
        {
            List<string> MyDropdownNames = new List<string>();
            if (MyDropdown.name == "PolyModelDropdown")
            {
                MyDropdownNames.AddRange(DataManager.Get().GetNames(DataFolderNames.PolyModels));
                FillDropDownWithList(MyDropdown, MyDropdownNames);
            }
        }

        public override void UseInput(GuiList MyList)
        {
            if (MyList.name == "BonesList")
            {
                SelectBoneByName(MyList.GetSelectedName());   // select a bone by a name
            }
        }

        public override void FillList(GuiList MyList)
        {
            if (MyList.name == "BonesList")
            {
                MyList.Clear();
                if (MySkeleton)
                {
                    for (int i = 0; i < MySkeleton.GetBones().Count; i++)
                    {
                        MyList.Add(MySkeleton.GetBones()[i].Name);
                    }
                }
            }
        }
        #endregion

        #region Utility

        /// <summary>
        /// Returns the distance from the camera
        /// </summary>
        private float GetDistanceFromCamera(Transform MyTransform)
        {
            Vector3 LocalPosition = GetCamera().transform.InverseTransformPoint(MyTransform.position);
            return LocalPosition.z;
        }

        /// <summary>
        /// Checks if an object is a bone
        /// </summary>
        private bool IsBone(GameObject MyGameObject)
        {
            return (MyGameObject.name.Contains("Bone"));
        }

        /// <summary>
        /// Returns the 3d mouse position using Selected Distance
        /// </summary>
        Vector3 GetWorldMouseDelta()
        {
            return GetWorldMouseDelta(SelectedDistance);
        }

        /// <summary>
        /// Returns the 3d mouse position
        /// </summary>
        Vector3 GetWorldMouseDelta(float Distance)
        {
            // Get Rays
            Ray CameraRay = GetRay(Input.mousePosition);
            Ray LastCameraRay = GetRay(LastMousePosition);
            // Get Position along ray
            Vector3 MouseWorldPosition = CameraRay.origin + CameraRay.direction * Distance;
            Vector3 LastMouseWorldPosition = LastCameraRay.origin + LastCameraRay.direction * SelectedDistance;
            // Inverse mouse position, to make local to Camera Rotation
            Camera SkeletonCamera = GetCamera();
            MouseWorldPosition = SkeletonCamera.transform.InverseTransformDirection(MouseWorldPosition);
            LastMouseWorldPosition = SkeletonCamera.transform.InverseTransformDirection(LastMouseWorldPosition);
            // Now get the delta of the two vectors
            Vector3 WorldMouseDelta = LastMouseWorldPosition - MouseWorldPosition;
            return WorldMouseDelta;
        }

        /// <summary>
        /// Converts a mouse position to a local one, incase of a viewer
        /// </summary>
        Vector2 GetLocalMousePosition(Vector2 MousePosition)
        {
            //Vector2 MousePosition = Input.mousePosition;
            if (MySkeletonViewer)
            {
                MousePosition = MySkeletonViewer.GetLocalMousePosition(MousePosition);
            }
            return MousePosition;
        }

        /// <summary>
        /// Returns a camera + mouse position ray
        /// </summary>
        Ray GetRay(Vector2 MousePosition)
        {
            return GetCamera().ScreenPointToRay(GetLocalMousePosition(MousePosition));
        }
        Ray GetRayOrtho(Vector2 MousePosition)
        {
            Camera MyCamera = GetCamera();
            MyCamera.orthographic = true;
            Ray MyRay = MyCamera.ScreenPointToRay(GetLocalMousePosition(MousePosition));
            MyCamera.orthographic = false;
            return MyRay;
        }

        /// <summary>
        /// Gets the camera of the selected viewer
        /// </summary>
        Camera GetCamera()
        {
            Camera MyCamera = Camera.main;
            if (MySkeletonViewer)
            {
                MyCamera = MySkeletonViewer.GetRenderCamera();
                //Debug.Log("MousePosition: " + Input.mousePosition.ToString() + ": - LocalMousePosition: " + MousePosition.ToString());
            }
            return MyCamera;
        }

        /// <summary>
        /// gets ths screen size of the selected viewer
        /// </summary>
        Vector2 GetScreenSize()
        {
            Vector2 ScreenSize = new Vector2(Screen.width, Screen.height);
            if (MySkeletonViewer)
            {
                ScreenSize = MySkeletonViewer.GetComponent<RectTransform>().GetSize();
            }
            return ScreenSize;
        }
        #endregion
    }
}