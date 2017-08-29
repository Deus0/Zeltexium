using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Zeltex.Util;
using Zeltex.Voxels;
using Zeltex.Combat;
using Zeltex.Characters;
using UniversalCoroutine;

namespace Zeltex.Skeletons
{
    /// <summary>
    /// Skeleton Class
    ///        - Create Bone
    ///        - Load Skeleton
    /// Uses VoxelManagers Voxel Data for all its meshes
    /// TODO:
    ///     Mutation values - turn off for edit mode - turn back on
    ///     Basically its a modifier that can be reverted - like 3ds max modifiers
    /// </summary>
    [ExecuteInEditMode]
    public partial class Skeleton : MonoBehaviour
    {
        #region Variables
        static float SkeletonLoadDelay = 0.0f;
        [Header("EditorActions")]
        [SerializeField]
        private EditorAction ActionLoad = new EditorAction();
        [SerializeField]
        private EditorAction ActionClear = new EditorAction();
        [SerializeField]
        private EditorAction ActionExportAsFBX = new EditorAction();
        [SerializeField]
        private EditorAction ActionRestoreDefaultPose = new EditorAction();
        [SerializeField]
        private EditorAction ActionSetDefaultPose = new EditorAction();
        
        [Header("Data")]
        public string SkeletonName = "Skeleton_0";
        [SerializeField]
        public Transform MyCameraBone;
        [SerializeField]
        public Transform MyBoneHead;
        private SkeletonAnimator MyAnimator;
        //public List<Transform> MyBones = new List<Transform>();
        public List<Bone> MyBones = new List<Bone>();
        [SerializeField]
        private Bounds MyBounds;
        [Header("Bones")]
        public Material BoneMaterial;
        public float BoneSize = 0.1f;   // cube meshes
        public Color32 BoneColor = new Color32(53, 83, 83, 255);
        [Header("Visibility")]
        public bool IsShowJoints = true;
        public bool IsShowBones = true;
        public bool IsShowMeshes = true;
        public bool IsAnimating = true;
        public bool IsJointsColliders = true;
        public bool IsMeshColliders = false;
        public bool IsConvexMeshes = false;
        [Header("Events")]
        public UnityEvent OnLoadSkeleton;

        [Header("Joints")]
        [SerializeField]
        private Material JointMaterial;
        [SerializeField]
        private Color32 JointColor = new Color32(48, 90, 59, 255);
        [SerializeField]
        private float JointSize = 0.15f;

        // Generation - Move this to SkeletonMutator Script
        private int BoneColorMutation = 20;
        private Vector3 OriginalCameraPosition;
        private bool IsLoading = false;
        private static Vector3 VoxelScale = new Vector3(0.05f, 0.05f, 0.05f);

        public bool IsReCalculateBounds;
        [SerializeField]
        private GameObject DefaultBody;
        private bool IsRenderCapsule = false;
        private UniversalCoroutine.Coroutine LoadRoutine;
        #endregion

        #region Mono

        public void Awake()
        {
            MyAnimator = GetComponent<SkeletonAnimator>();
        }

        /// <summary>
        /// Resets the bone colour back to the original
        /// </summary>
        public void RestoreBoneColor(Bone MyBone)
        {
            MyBone.MyJointCube.GetComponent<MeshRenderer>().material.color = JointColor;
        }

        void Update ()
        {
            if (IsAnimating)
            {
                Animate();
            }
            if (IsReCalculateBounds)
            {
                IsReCalculateBounds = false;
                CalculateBounds();
            }
            if (ActionLoad.IsTriggered())
            {
                string Script = DataManager.Get().Get(DataFolderNames.Skeletons, 0);
                IsLoading = false;
                if (LoadRoutine != null)
                {
                    this.UniStopCoroutine(LoadRoutine);
                }
                LoadRoutine = CoroutineManager.StartCoroutine(RunScriptRoutine(FileUtil.ConvertToList(Script)));
                //Load();
            }
            if (ActionClear.IsTriggered())
            {
                CoroutineManager.StartCoroutine(Clear());
            }
            if (ActionExportAsFBX.IsTriggered())
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                System.Windows.Forms.SaveFileDialog MyDialog = new System.Windows.Forms.SaveFileDialog();
                System.Windows.Forms.DialogResult MyResult = MyDialog.ShowDialog();
                if (MyResult == System.Windows.Forms.DialogResult.OK)
                {
                    //Mesh MyMesh = ObjImport.ImportFile(MyDialog.FileName);
                    UnityFBXExporter.FBXExporter.ExportGameObjToFBX(gameObject, MyDialog.FileName);
                    //byte[] bytes = FileUtil.LoadBytes(MyDialog.FileName);
                    //MyZexel.LoadImage(bytes);
                }
#endif
            }

            if (ActionRestoreDefaultPose.IsTriggered())
            {
                RestoreDefaultPose();
            }
            if (ActionSetDefaultPose.IsTriggered())
            {
                SetDefaultPose();
            }
        }


        /// <summary>
        /// Gets the layer mask the skeleton is using
        /// </summary>
        private void SetLayerMask(GameObject MyObject)
        {
            LayerManager.Get().SetLayerSkeleton(MyObject);
            /*if (transform.parent == null)
            {
                return gameObject.layer;
            }
            else
            {
                return transform.parent.gameObject.layer;
            }*/
        }

        public SkeletonAnimator GetAnimator()
        {
            return MyAnimator;
        }
        #endregion

        #region Bounds

        /// <summary>
        /// Gets the skeletons bounds
        /// </summary>
        public Bounds GetBounds()
        {
            if (MyBounds == null)
            {
                CalculateBounds();
            }
            if (MyBounds.size == Vector3.zero)
            {
                MyBounds.size = new Vector3(0.5f, 0.5f, 0.5f);
            }
            return MyBounds;
        }

        /// <summary>
        /// Calculates the skeleton Bounds
        /// </summary>
        private Bounds CalculateBounds()
        {
            Debug.Log("Calculating bounds for: " + name + " with " + MyBones.Count + " bones.");
            //Vector3 MyPosition = transform.position;
            //transform.position += new Vector3(30, 30, 30);
            //Debug.LogError("Moving position to " + transform.position.ToString());
            MyBounds = new Bounds(transform.position, Vector3.zero);
            for (int i = 0; i < MyBones.Count; i++)
            {
                if (MyBones[i].VoxelMesh)
                {
                    World MyWorld = MyBones[i].VoxelMesh.GetComponent<World>();
                    MyBounds = AddWorldToBounds(MyWorld, MyBounds);
                }
                else
                {
                    Debug.Log(i + " has no mesh.");
                }
            }
            MyBounds.center -= transform.position;
            Debug.Log("MyBounds: " + MyBounds.extents.ToString() + ":" + MyBounds.center.ToString());
            //transform.position = MyPosition;
            MyBounds.center = new Vector3(0.009f, 0.174f, 0.009f);
            MyBounds.size = new Vector3(0.26f, 0.91f, 0.26f);
            return MyBounds;
        }

        public Bounds GetEditorBounds()
        {
            Bounds MyBounds = new Bounds(transform.position, Vector3.zero);
            for (int i = 0; i < MyBones.Count; i++)
            {
                if (MyBones[i].VoxelMesh)
                {
                    World MyWorld = MyBones[i].VoxelMesh.GetComponent<World>();
                    MyBounds = AddWorldToBounds(MyWorld, MyBounds);
                }
                if (MyBones[i].MyJointCube)
                {
                    MeshRenderer MyRenderer = MyBones[i].MyJointCube.GetComponent<MeshRenderer>();
                    MeshFilter MyMeshFilter = MyBones[i].MyJointCube.GetComponent<MeshFilter>();
                    if (MyRenderer && MyMeshFilter.mesh && MyMeshFilter.mesh.vertexCount > 0)
                    {
                        MyBounds.Encapsulate(MyRenderer.bounds.min);
                        MyBounds.Encapsulate(MyRenderer.bounds.max);
                    }
                }
                if (MyBones[i].VoxelMesh)
                {
                    MeshRenderer MyRenderer = MyBones[i].VoxelMesh.GetComponent<MeshRenderer>();
                    MeshFilter MyMeshFilter = MyBones[i].VoxelMesh.GetComponent<MeshFilter>();
                    if (MyMeshFilter && MyMeshFilter.mesh && MyMeshFilter.mesh.vertexCount > 0)
                    {
                        MyBounds.Encapsulate(MyRenderer.bounds.min);
                        MyBounds.Encapsulate(MyRenderer.bounds.max);
                    }
                }
            }
            MyBounds.center -= transform.position;
            MyBounds.center = new Vector3(0.008549809f, 0.1749992f, 0.008110046f);
            MyBounds.size = new Vector3(0.1282765f, 0.4583321f, 0.1282749f);
            return MyBounds;
        }

        /// <summary>
        /// Adds the worlds to bounds
        /// </summary>
        private Bounds AddWorldToBounds(World MyWorld, Bounds MyBounds)
        {
            if (MyWorld.IsSingleChunk())
            {
                MeshRenderer MyBodyRenderer = MyWorld.gameObject.GetComponent<MeshRenderer>();
                MeshFilter MyMeshFilter = MyWorld.gameObject.GetComponent<MeshFilter>();
                if (MyMeshFilter.mesh && MyMeshFilter.mesh.vertexCount > 0)
                {
                    Debug.Log(MyWorld.name + " has " + MyMeshFilter.mesh.vertexCount + " verts. MyBodyRenderer.bounds.size: " 
                        + MyBodyRenderer.bounds.size.ToString() + " - and position: " + MyBodyRenderer.bounds.center.ToString());
                    MyBounds.Encapsulate(MyBodyRenderer.bounds);
                    //MyBounds.Encapsulate(MyBodyRenderer.bounds.min);
                    //MyBounds.Encapsulate(MyBodyRenderer.bounds.max);
                }
                else
                {
                    Debug.Log(MyWorld.name + " has no mesh.");
                }
            }
            else
            {
                foreach (Int3 MyKey in MyWorld.MyChunkData.Keys)
                {
                    Chunk MyChunk = MyWorld.MyChunkData[MyKey];
                    MeshRenderer MyBodyRenderer = MyChunk.GetComponent<MeshRenderer>();
                    MeshFilter MyMeshFilter = MyChunk.GetComponent<MeshFilter>();
                    if (MyMeshFilter.mesh && MyMeshFilter.mesh.vertexCount > 0)
                    {
                        MyBounds.Encapsulate(MyBodyRenderer.bounds);
                        // MyBounds.Encapsulate(MyBodyRenderer.bounds.min);
                        //MyBounds.Encapsulate(MyBodyRenderer.bounds.max);
                    }
                    /*else if (MyMeshFilter.mesh.vertexCount == 0)
                    {
                        Debug.LogError(MyChunk.name + " has no verticies");
                    }*/
                }
            }
            return MyBounds;
        }

        /// <summary>
        /// Updates the Bounding box of the skeleton
        /// Called when skeleton updates
        /// </summary>
        void UpdateBounds()
        {
            //Debug.LogError("Updating bounds " + Time.time);
            if (transform.parent != null)
            {
                CalculateBounds();
                AttachCameraToHead();
                /*CapsuleCollider MyCollider = transform.parent.GetComponent<CapsuleCollider>();
                float MyHeight = MyBounds.size.y * 0.9f;
                if (MyHeight == 0)
                {
                    MyHeight = 0.1f;
                }
                float MyRadius = (MyBounds.extents.x + MyBounds.size.z) / 4f;//3.6f;
                if (MyRadius == 0)
                {
                    MyRadius = 0.1f;
                }
                float MyStep = 0.01f * MyHeight;
                Vector3 MyCenter = MyBounds.center - transform.position;
                GameObject MyCapsuleObject = transform.parent.gameObject;
                if (MyCapsuleObject)
                {
                    CapsuleCollider MyCapsule = MyCapsuleObject.GetComponent<CapsuleCollider>();
                    if (MyCapsule)
                    {
                        MyCapsule.center = MyBounds.center;
                        MyCapsule.height = MyBounds.extents.y * 2;
                        if (MyBounds.extents.x > MyBounds.extents.z)
                        {
                            MyCapsule.radius = MyBounds.extents.x;
                        }
                        else
                        {
                            MyCapsule.radius = MyBounds.extents.x;
                        }
                        StartCoroutine(RefreshCapsule(MyCapsule.GetComponent<CapsuleCollider>()));
                    }
                }
                Transform MySheild = transform.parent.Find("Sheild");
                if (MySheild)
                {
                    float BiggestNumber = MyBounds.extents.x;
                    if (MyBounds.extents.y > BiggestNumber)
                    {
                        BiggestNumber = MyBounds.extents.y;
                    }
                    if (MyBounds.extents.z > BiggestNumber)
                    {
                        BiggestNumber = MyBounds.extents.z;
                    }
                    BiggestNumber *= 2;
                    BiggestNumber *= 1.1f;
                    MySheild.localScale = new Vector3(BiggestNumber, BiggestNumber, BiggestNumber);
                }*/
            }
            //SetColliders(IsMeshColliders);
        }

        /// <summary>
        /// Sometimes capsules dont update properly. So i do this in a routine
        /// </summary>
        /*IEnumerator RefreshCapsule(CapsuleCollider MyCapsule)
		{
			if (MyCapsule)
			{
				MyCapsule.enabled = false;
				yield return new WaitForSeconds(0.01f);
				MyCapsule.enabled = true;
			}
        }*/
        #endregion

        #region CameraBone

        /// <summary>
        /// The original Camera position is stored for later use
        /// </summary>
        public Vector3 GetOriginalCameraPosition()
        {
            return OriginalCameraPosition;
        }

        /// <summary>
        /// Gets the camera bone of the skeleton. The camera is attached to it.
        /// </summary>
        public Transform GetCameraBone()
        {
            if (MyCameraBone == null)
            {
                MyCameraBone = transform.parent.Find("CameraBone");
            }
            return MyCameraBone;
        }

        /// <summary>
        /// Set new camera bone for skeleton
        /// </summary>
        public void SetCameraBone(Transform NewCameraBone)
        {
            MyCameraBone = NewCameraBone;
            Shooter MyShooter = transform.parent.gameObject.GetComponent<Shooter>();
            if (MyShooter)
            {
                MyShooter.HotSpotTransform = MyCameraBone;
            }

			transform.parent.gameObject.GetComponent<Character>().SetGuisTarget(MyCameraBone);
            /*BasicController PlayerController = transform.parent.gameObject.GetComponent<BasicController>();
            if (PlayerController)
            {
                PlayerController.UpdateCamera(MyCameraBone);
            }*/
        }

        /// <summary>
        /// Move the camera to be aligned with the head position
        /// Reference the head, for making transparent later
        /// </summary>
        private void AttachCameraToHead()
        {
            MyCameraBone = GetCameraBone(); // incase i havnt found it yet
            // Find bone with tag Head
            float CameraDistanceZ = 0;
            MyBoneHead = transform;
            Bone HeadBone = GetBoneWithTag("Head");

            /*if (HeadBone != null && HeadBone.VoxelMesh != null)
            {
                MyBoneHead = HeadBone.VoxelMesh.transform;//MyBones[i].MyTransform;
                if (MyBoneHead.GetComponent<MeshRenderer>())
                {
                    CameraDistanceZ = MyBoneHead.GetComponent<MeshRenderer>().bounds.size.z * 2f;// * (3 / 2f);
                }
            }
            else
            {   // by default make it the first bone
                if (MyBones.Count > 0)
                {
                    if (MyBones[0].VoxelMesh != null)
                    {
                        MyBoneHead = MyBones[0].VoxelMesh.transform;
                    }
                    else
                    {
                        MyBoneHead = MyBones[0].MyTransform;
                    }
                    if (MyBoneHead.GetComponent<MeshRenderer>())
                    {
                        CameraDistanceZ = MyBoneHead.GetComponent<MeshRenderer>().bounds.size.z * (3 / 2f);
                    }
                }
                else
                {
                    Debug.LogError("Skeleton " + name + " has no bones.");
                }
            }*/

            // Get Camera 
            /*if (MyCameraBone == null && transform.parent.FindChild("CameraBone") != null)   // only do this once
            {
                SetCameraBone(transform.parent.FindChild("CameraBone"));
            }*/
            // Reset camera bone every time its updated, also make sure the player knows its updated!
            // Attach camera to head!
            if (MyBoneHead != null && MyCameraBone != null)
            {
                Debug.Log("Moving Camera Bone to: " + MyBoneHead.transform.position.ToString());
                MyCameraBone.position = MyBoneHead.position;
                MyCameraBone.rotation = MyBoneHead.rotation;
                OriginalCameraPosition = MyCameraBone.localPosition;
                //MyCameraBone.SetParent(MyBoneHead);   // don't move this, just use the position of the bone!
                //MyCameraBone.localPosition = new Vector3(0, 0, CameraDistanceZ);// 0.35f); // size of head, i should change this!
                /*if (transform.parent.gameObject.GetComponent<Character>().IsPlayer)
                {
                    Camera.main.transform.gameObject.GetComponent<Player>().SetCameraBone(MyCameraBone);
                }*/
            }
            else
            {
                //Debug.LogError("No Bonehead found.");
            }
            /*if (MyBoneHead != null && MyBoneHead.GetComponent<MeshRenderer>())
            {
                MyBoneHead.GetComponent<MeshRenderer>().enabled = false;
            }*/
        }
        #endregion

        #region Bones
        
        /// <summary>
        /// Clears the skeleton in a routine
        /// </summary>
        public IEnumerator Clear()
        {
            if (MyCameraBone != null)
            {
                MyCameraBone.SetParent(transform.parent);
            }
            // Clears the currently loaded skeleton
            //Debug.LogError("Clearning: " + MyBones.Count + ":" + MyBones.Count + " Bones.");
            for (int i = MyBones.Count - 1; i >= 0; i--)
            {
                if (MyBones[i] != null && MyBones[i].MyTransform != null)
                {
                    MonoBehaviourExtension.Kill(MyBones[i].MyTransform.gameObject);
                    yield return null;
                    //yield return new WaitForSeconds(SkeletonLoadDelay);
                }
            }
            MyBones.Clear();
        }

        /// <summary>
        /// Finds a bone with a t ag!
        /// </summary>
        private Bone GetBoneWithTag(string MyTag)
        {
            for (int i = 0; i < MyBones.Count; i++)
            {
                if (MyBones[i].Tag == MyTag)
                {
                    return MyBones[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Generates a unique name for a bone
        /// The name is used in the animations
        /// </summary>
        public string GenerateUniqiueBoneName()
        {
            bool HasFound = false;
            string PossibleName = "Bone";
            while (HasFound == false)
            {
                PossibleName = "Bone_" + Mathf.RoundToInt(Random.Range(1, 10000));
                bool DoesExist = false;
                for (int i = 0; i < MyBones.Count; i++)
                {
                    if (MyBones[i].Name == PossibleName)
                    {
                        DoesExist = true;
                        break;
                    }
                }
                if (DoesExist == false)
                {
                    HasFound = true;
                }
            }
            return PossibleName;
        }

        /// <summary>
        /// Spawns a bone attached to the skeleton
        /// </summary>
        public Bone CreateBone()
        {
            return CreateBone(transform);
        }

        /// <summary>
        /// Create A Bone, with a joint and a line
        /// </summary>
        public Bone CreateBone(Transform BoneParent = null)
        {
            if (BoneParent == null)
            {
                BoneParent = transform;
            }
            //Debug.LogError("Creating bone: " + MyBones.Count);
            GameObject NewBoneObject = new GameObject();
            SetLayerMask(NewBoneObject);
            NewBoneObject.tag = "Bone";
            //NewBone.transform.position = MyCamera.transform.position + MyCamera.transform.forward * 2f;
            NewBoneObject.transform.position = new Vector3();
            NewBoneObject.transform.SetParent(BoneParent, false);
            Bone NewBone = CreateBoneData(BoneParent, NewBoneObject.transform, MyBones.Count);
            NewBone.MyTransform = NewBoneObject.transform;
            NewBone.ParentTransform = BoneParent;
            NewBoneObject.transform.localScale = new Vector3(1, 1, 1);
            NewBone.Name = GenerateUniqiueBoneName();
            NewBoneObject.name = NewBone.Name;
            MyBones.Add(NewBone);
            return NewBone;
        }

        /// <summary>
        /// Create the bone data
        /// </summary>
        private Bone CreateBoneData(Transform BoneParent, Transform MyBone, int i)
        {
            //Debug.LogError(i + " - " + MyBones[i].name);
            //LayerMask ViewerLayer = GetLayerMask();// MyBoneParent.gameObject.layer;
            Bone NewBone = new Bone();
            if (JointMaterial == null)
            {
                JointMaterial = new Material(Shader.Find("Standard"));
            }
            if (BoneMaterial == null)
            {
                BoneMaterial = new Material(Shader.Find("Standard"));
            }
            // Create a joint cube for the bone
            if (IsShowJoints)
            {
                GameObject MyJointCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                SetLayerMask(MyJointCube);
                // Destroy collider if no joint shown
                if (!IsJointsColliders && MyJointCube.GetComponent<BoxCollider>())
                {
                    MonoBehaviourExtension.Kill(MyJointCube.GetComponent<BoxCollider>());
                }
                MyJointCube.name = "Joint " + i;
                MyJointCube.tag = "BonePart";
                MyJointCube.transform.localScale = new Vector3(JointSize, JointSize, JointSize);
                MyJointCube.transform.SetParent(MyBone, false);
                NewBone.MyJointCube = MyJointCube.transform;
            }
            if (BoneParent != transform)
            {
                // Create a bone mesh
                if (IsShowBones)
                {
                    GameObject BoneMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    SetLayerMask(BoneMesh);
                    if (BoneMesh.GetComponent<BoxCollider>())
                    {
                        MonoBehaviourExtension.Kill(BoneMesh.GetComponent<BoxCollider>());
                    }
                    BoneMesh.name = "BoneMesh " + i;
                    BoneMesh.tag = "BonePart";
                    BoneMesh.transform.localScale = new Vector3(BoneSize, BoneSize, BoneSize);
                    BoneMesh.transform.SetParent(MyBone, false);
                    NewBone.BodyCube = BoneMesh.transform;
                }
            }
            SetBoneColor(NewBone);
            return NewBone;
        }

        public void ShowJoints()
        {
            IsShowJoints = true;
            IsShowBones = true;
            IsJointsColliders = true;
            for (int i = 0; i < MyBones.Count; i++)
            {
                if (IsShowJoints && MyBones[i].MyJointCube == null)
                {
                    MyBones[i].MyJointCube = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                    SetLayerMask(MyBones[i].MyJointCube.gameObject);
                    // Destroy collider if no joint shown
                    if (!IsJointsColliders && MyBones[i].MyJointCube.GetComponent<BoxCollider>())
                    {
                        MonoBehaviourExtension.Kill(MyBones[i].MyJointCube.GetComponent<BoxCollider>());
                    }
                    MyBones[i].MyJointCube.name = "Joint " + i;
                    MyBones[i].MyJointCube.tag = "BonePart";
                    MyBones[i].MyJointCube.transform.localScale = new Vector3(JointSize, JointSize, JointSize);
                    MyBones[i].MyJointCube.transform.SetParent(MyBones[i].MyTransform, false);
                }
                if (IsShowBones && MyBones[i].BodyCube == null && MyBones[i].ParentTransform != transform)
                {
                    MyBones[i].BodyCube = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                    SetLayerMask(MyBones[i].BodyCube.gameObject);
                    if (MyBones[i].BodyCube.GetComponent<BoxCollider>())
                    {
                        MonoBehaviourExtension.Kill(MyBones[i].BodyCube.GetComponent<BoxCollider>());
                    }
                    MyBones[i].BodyCube.name = "BoneMesh " + i;
                    MyBones[i].BodyCube.tag = "BonePart";
                    MyBones[i].BodyCube.transform.localScale = new Vector3(BoneSize, BoneSize, BoneSize);
                    MyBones[i].BodyCube.transform.SetParent(MyBones[i].MyTransform, false);
                }
                if (MyBones[i].MyJointCube != null)
                {
                    Debug.Log("Created Box Collider");
                    BoxCollider MyBoxCollider = MyBones[i].MyJointCube.GetComponent<BoxCollider>();
                    if (!IsJointsColliders && MyBoxCollider)
                    {
                        MonoBehaviourExtension.Kill(MyBoxCollider);
                    }
                    else if (IsJointsColliders && MyBoxCollider == null)
                    {
                        MyBones[i].MyJointCube.gameObject.AddComponent<BoxCollider>();
                    }
                }
            }
        }

        /// <summary>
        /// Removes a target bone
        /// </summary>
        public void Remove(Transform MyBone)
        {
            if (MyBone != null)
            {
                for (int i = 0; i < MyBones.Count; i++)
                {
                    if (MyBones[i].MyTransform == MyBone)
                    {
                        Debug.Log("Removing bone: " + MyBone.name + " - at index: " + i);
                        MyBones.RemoveAt(i);
                        // also delete all the keyframes for this
                        MyAnimator.DeleteAllKeysFromAllAnimations(MyBone);
                        // if has children remove them too
                        List<Transform> MyChildren = FindChildren(MyBone);
                        for (int j = 0; j < MyChildren.Count; j++)
                        {
                            Remove(MyChildren[j]);  // removes all childrens children too
                        }
                        break;
                    }
                }
                MonoBehaviourExtension.Kill(MyBone.gameObject); // after removing all children/grandchildren, will destroy the object
            }
        }

        /// <summary>
        /// Finds the children of the bone
        /// </summary>
        private List<Transform> FindChildren(Transform MyBone)
        {
            List<Transform> MyChildren = new List<Transform>();
            for (int i = 0; i < MyBones.Count; i++)
            {
                if (MyBones[i].ParentTransform == MyBone)
                {
                    MyChildren.Add(MyBones[i].MyTransform);
                }
            }
            return MyChildren;
        }
        #endregion

        #region Posing
        /// <summary>
        /// Restores the default skeleton pose
        /// </summary>
        public void RestoreDefaultPose()
        {
            for (int i = 0; i < MyBones.Count; i++)
            {
                MyBones[i].RestoreDefaultPose();
            }
        }

        private UniversalCoroutine.Coroutine RestoratingPoseRoutineCoroutine;
        public void RestoreDefaultPose(float TimeTaken)
        {
            if (RestoratingPoseRoutineCoroutine != null)
            {
                UniversalCoroutine.CoroutineManager.StopCoroutine(RestoratingPoseRoutineCoroutine);
            }
            RestoratingPoseRoutineCoroutine = UniversalCoroutine.CoroutineManager.StartCoroutine(RestoreDefaultPoseRoutine(TimeTaken));
        }

        private IEnumerator RestoreDefaultPoseRoutine(float TimeTaken)
        {
            List<Vector3> OldPositions = new List<Vector3>();
            List<Vector3> NewPositions = new List<Vector3>();
            List<Quaternion> OldRotations = new List<Quaternion>();
            List<Quaternion> NewRotations = new List<Quaternion>();
            float TimeStarted = Time.time;
            for (int i = 0; i < MyBones.Count; i++)
            {
                OldPositions.Add(MyBones[i].MyTransform.localPosition);
                NewPositions.Add(MyBones[i].GetDefaultPosition());
                OldRotations.Add(MyBones[i].MyTransform.localRotation);
                NewRotations.Add(MyBones[i].GetDefaultRotationQ());
                Debug.LogError("Bone " + i + " going from " + OldPositions[i].ToString() + " TO " + NewPositions[i].ToString());
            }
            while (Time.time - TimeStarted <= TimeTaken)
            {
                for (int i = 0; i < MyBones.Count; i++)
                {
                    MyBones[i].MyTransform.localPosition = Vector3.Lerp(
                        OldPositions[i], 
                        NewPositions[i],
                        ((Time.time - TimeStarted) / TimeTaken));
                    MyBones[i].MyTransform.localRotation = Quaternion.Lerp(
                        OldRotations[i],
                        NewRotations[i],
                        ((Time.time - TimeStarted) / TimeTaken));
                }
                yield return null;
            }
            for (int i = 0; i < MyBones.Count; i++)
            {
                MyBones[i].MyTransform.localPosition = Vector3.Lerp(
                    OldPositions[i],
                    NewPositions[i],
                    ((Time.time - TimeStarted) / TimeTaken));
                MyBones[i].MyTransform.localRotation = Quaternion.Lerp(
                    OldRotations[i],
                    NewRotations[i],
                    ((Time.time - TimeStarted) / TimeTaken));
            }
        }

        /// <summary>
        /// Sets the default skeleton pose
        /// </summary>
        public void SetDefaultPose()
        {
            for (int i = 0; i < MyBones.Count; i++)
            {
                MyBones[i].SetDefaultPose();
            }
        }
        #endregion

        #region Visibility

        /// <summary>
        /// Animates the bones and joints
        /// </summary>
        public void Animate()
        {
            for (int i = 0; i < MyBones.Count; i++)
            {
                if (MyBones[i].MyTransform && MyBones[i].ParentTransform)
                {
                    Bone MyBone = MyBones[i];
                    if (MyBone.BodyCube)    // bone mesh
                    {
                        MyBone.BodyCube.transform.position = MyBone.GetMidPoint();
                        MyBone.BodyCube.transform.rotation = MyBone.GetBoneRotation();
                        MyBone.BodyCube.transform.localScale = MyBone.GetBoneScale(MyBone.MyTransform, BoneSize);
                    }
                }
            }
        }

        /// <summary>
        /// Resets the bone colours
        /// </summary>
        public void ResetColors()
        {
            for (int i = 0; i < MyBones.Count; i++)
            {
                SetBoneColor(MyBones[i]);
            }
        }

        /// <summary>
        /// sets the bone colours
        /// </summary>
        private void SetBoneColor(Bone MyBone)
        {
            Transform MyJointCube = MyBone.MyJointCube;
            if (MyJointCube)
            {
                if (JointMaterial == null)
                {
                    MyJointCube.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
                }
                else
                {
                    MyJointCube.GetComponent<MeshRenderer>().material = new Material(JointMaterial);
                }
                MyJointCube.GetComponent<MeshRenderer>().material.SetFloat("_Mode", 2);
                MyJointCube.GetComponent<MeshRenderer>().material.color = JointColor;
            }
            Transform BoneMesh = MyBone.BodyCube;
            if (BoneMesh)
            {
                if (BoneMaterial == null)
                    BoneMesh.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
                else
                    BoneMesh.GetComponent<MeshRenderer>().material = new Material(BoneMaterial);

                BoneMesh.GetComponent<MeshRenderer>().material.SetFloat("_Mode", 2);
                byte Red = (byte)(BoneColor.r + (int)Random.Range(-BoneColorMutation, BoneColorMutation));
                byte Green = (byte)(BoneColor.g + (int)Random.Range(-BoneColorMutation, BoneColorMutation));
                byte Blue = (byte)(BoneColor.b + (int)Random.Range(-BoneColorMutation, BoneColorMutation));
                BoneMesh.GetComponent<MeshRenderer>().material.color = new Color32(Red, Green, Blue, BoneColor.a);
            }
        }

        /// <summary>
        /// Set all the meshes visibility
        /// </summary>
        public void SetMeshVisibility(bool NewState)
        {
            for (int i = 0; i < MyBones.Count; i++)
            {
                if (MyBones[i].VoxelMesh)
                {
                    MyBones[i].VoxelMesh.gameObject.GetComponent<World>().SetMeshVisibility(NewState);
                }
            }
        }

        /// <summary>
        /// Set the bons visibility
        /// </summary>
        public void SetBoneVisibility(bool NewState)
        {
            for (int i = 0; i < MyBones.Count; i++)
            {
                if (MyBones[i].MyJointCube)
                {
                    MyBones[i].MyJointCube.gameObject.SetActive(NewState);
                }
                if (MyBones[i].BodyCube)
                {
                    MyBones[i].BodyCube.gameObject.SetActive(NewState);
                }
            }
        }
        #endregion

        #region Util

        private CapsuleCollider CapsuleCollider;
        private MeshRenderer CapsuleRenderer;

        /// <summary>
        /// Gets a capsule, if a character exists it gets it off that, otherwise gets it off the skeleton
        /// </summary>
        public CapsuleCollider GetCapsule()
        {
            if (transform.parent && transform.parent.GetComponent<Character>())
            {
                CapsuleCollider = transform.parent.gameObject.GetComponent<CapsuleCollider>();
            }
            else
            {
                if (CapsuleCollider == null)
                {
                    GameObject CapsuleObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    CapsuleObject.name = name + "_Capsule";
                    CapsuleObject.layer = gameObject.layer;
                    CapsuleObject.transform.SetParent(transform);
                    CapsuleObject.transform.position = transform.position;
                    CapsuleCollider = CapsuleObject.GetComponent<CapsuleCollider>();
                    MonoBehaviourExtension.Kill(CapsuleObject.GetComponent<MeshRenderer>());
                    MonoBehaviourExtension.Kill(CapsuleObject.GetComponent<MeshFilter>());
                    GetCapsuleRenderer();
                }
            }
            return CapsuleCollider;
        }
        /// <summary>
        /// Gets a capsule, if a character exists it gets it off that, otherwise gets it off the skeleton
        /// </summary>
        public MeshRenderer GetCapsuleRenderer()
        {
            if (CapsuleRenderer == null)
            {
                GameObject CapsuleObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                CapsuleObject.name = name + "_CapsuleRenderer";
                CapsuleObject.layer = gameObject.layer;
                CapsuleObject.transform.SetParent(transform);
                CapsuleObject.transform.position = transform.position;
                Destroy(CapsuleObject.GetComponent<CapsuleCollider>());
                CapsuleRenderer = CapsuleObject.GetComponent<MeshRenderer>();
                CapsuleRenderer.material = JointMaterial;
                CapsuleRenderer.enabled = IsRenderCapsule;
            }
            return CapsuleRenderer;
        }

        private void RefreshCapsule()
        {
            GetCapsule();
            if (CapsuleCollider)
            {
                SetCapsuleRadius(CapsuleCollider.radius);
                SetCapsuleHeight(CapsuleCollider.height);
                SetCapsuleCenter(CapsuleCollider.center);
            }
        }

        public void SetCapsuleHeight(float NewHeight)
        {
            if (CapsuleCollider)
            {
                CapsuleCollider.height = NewHeight;
                if (CapsuleRenderer)
                {
                    CapsuleRenderer.transform.localScale =
                        new Vector3(
                            CapsuleRenderer.transform.localScale.x,
                            NewHeight / 2f,
                            CapsuleRenderer.transform.localScale.z);
                }
            }
        }
        public void SetCapsuleRadius(float NewRadius)
        {
            if (CapsuleCollider)
            {
                CapsuleCollider.radius = NewRadius;
                if (CapsuleRenderer)
                {
                    CapsuleRenderer.transform.localScale =
                        new Vector3(
                            NewRadius * 2,
                            CapsuleRenderer.transform.localScale.y,
                            NewRadius * 2);
                }
            }
        }
        public void SetCapsuleCenter(Vector3 NewCenterPosition)
        {
            if (CapsuleCollider)
            {
                CapsuleCollider.center = NewCenterPosition;
                if (CapsuleRenderer)
                {
                    CapsuleRenderer.transform.localPosition = NewCenterPosition;
                }
            }
        }
        public Vector3 GetCapsuleCenter()
        {
            if (CapsuleCollider)
            {
                return CapsuleCollider.center;
            }
            else
            {
                return Vector3.zero;
            }
        }

        public float GetCapsuleHeight()
        {
            if (CapsuleCollider)
            {
                return CapsuleCollider.height;
            }
            else
            {
                return 0;
            }
        }
        public float GetCapsuleRadius()
        {
            if (CapsuleCollider)
            {
                return CapsuleCollider.radius;
            }
            else
            {
                return 0;
            }
        }

        public void SetCapsuleCollider(bool NewState)
        {
            GetCapsule();
            if (CapsuleCollider)
            {
                CapsuleCollider.enabled = NewState;
            }
            IsRenderCapsule = NewState;
            if (CapsuleRenderer)
            {
                CapsuleRenderer.enabled = IsRenderCapsule;
            }
        }

        /// <summary>
        /// Sets the meshes convex state
        /// </summary>
        public void SetConvex(bool NewState)
        {
            //Debug.Log("Setting SKeleton [" + name + "]'s Mesh Colliders: " + NewState);
            for (int i = 0; i < MyBones.Count; i++)
            {
                if (MyBones[i].VoxelMesh)
                {
                    World MyWorld = MyBones[i].VoxelMesh.GetComponent<World>();
                    MyWorld.SetConvex(NewState);
                }
            }
        }

        /// <summary>
        /// Used for skeleton editor, for selection of different things
        /// </summary>
        public void SetMeshColliders(bool IsColliders)
        {
            Debug.Log("Setting SKeleton [" + name + "]'s Mesh Colliders: " + IsColliders);
            for (int i = 0; i < MyBones.Count; i++)
            {
                if (MyBones[i].VoxelMesh)
                {
                    World MyWorld = MyBones[i].VoxelMesh.GetComponent<World>();
                    if (MyWorld)
                    {
                        MyWorld.SetColliders(IsColliders);
                    }
                    else
                    {
                        Debug.LogError(MyBones[i].Name + " has no world on its mesh object.");
                    }
                }
            }
         }

        /// <summary>
        /// Sets the collision of the joints
        /// </summary>
        public void SetJointColliders(bool IsColliders)
        {
            if (transform.parent)
            {
                Debug.LogError("Setting Joint Colliders of " + transform.parent.name + " to " + IsColliders);
            }
            for (int i = 0; i < MyBones.Count; i++)
            {
                if (MyBones[i].MyJointCube)
                {
                    BoxCollider MyBoxCollider = MyBones[i].MyJointCube.gameObject.GetComponent<BoxCollider>();
                    if (IsColliders && MyBoxCollider == null)
                    {
                        MyBones[i].MyJointCube.gameObject.AddComponent<BoxCollider>();
                    }
                    else if (!IsColliders && MyBoxCollider != null)
                    {
                        MonoBehaviourExtension.Kill(MyBoxCollider);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the shaders of the bones to XRay
        /// </summary>
        public void SetXRay(bool IsXRay)
        {
            Shader MyShader;
            if (IsXRay)
            {
                MyShader = Shader.Find("XRay");
            }
            else
            {
                MyShader = Shader.Find("Standard");
            }
            for (int i = 0; i < MyBones.Count; i++)
            {
                if (MyBones[i].MyJointCube)
                {
                    MyBones[i].MyJointCube.gameObject.GetComponent<MeshRenderer>().material.shader = MyShader;
                }
                if (MyBones[i].BodyCube)
                {
                    MyBones[i].BodyCube.gameObject.GetComponent<MeshRenderer>().material.shader = MyShader;
                }
            }
        }

        /// <summary>
        /// Searches transform tree for bones with a tag
        /// </summary>
        static List<Transform> GetBones2(Transform Parent, List<Transform> CollectedBones)
        {
            for (int i = 0; i < Parent.transform.childCount; i++)
            {
                Transform ChildBone = Parent.transform.GetChild(i);
                if (ChildBone.gameObject.tag == "Bone")
                {
                    CollectedBones.Add(ChildBone);
                }
                CollectedBones = GetBones2(ChildBone, CollectedBones);
            }
            return CollectedBones;
        }

        /// <summary>
        /// Returns parent index of a Bone
        /// </summary>
        private int GetParentIndex(Bone MyBone)
        {
            Transform BoneParent = MyBone.ParentTransform;
            for (int i = 0; i < MyBones.Count; i++)
            {
                if (BoneParent == MyBones[i].MyTransform)
                    return i;
            }
            return -1;
        }
        #endregion
    }
}