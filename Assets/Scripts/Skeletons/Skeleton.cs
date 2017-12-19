using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Zeltex.Util;
using Zeltex.Voxels;
using Zeltex.Combat;
using Zeltex.Characters;
using UniversalCoroutine;
using Newtonsoft.Json;

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
    //[ExecuteInEditMode]
    [System.Serializable]
    public class Skeleton : Element
    {
        #region Variables
        [Header("EditorActions")]
        [SerializeField, JsonIgnore]
        private EditorAction ActionLoad = new EditorAction();
        [SerializeField, JsonIgnore]
        private EditorAction ActionClear = new EditorAction();
        [SerializeField, JsonIgnore]
        private EditorAction ActionExportAsFBX = new EditorAction();
        [SerializeField, JsonIgnore]
        private EditorAction ActionRestoreDefaultPose = new EditorAction();
        [SerializeField, JsonIgnore]
        private EditorAction ActionSetDefaultPose = new EditorAction();
        [SerializeField, JsonIgnore]
        public EditorAction ActionGenerateSkeleton = new EditorAction();

        [SerializeField, JsonIgnore]
        public EditorAction ActionActivateSkeleton = new EditorAction();
        [SerializeField, JsonIgnore]
        public EditorAction ActionDeactivateSkeleton = new EditorAction();
        [SerializeField, JsonIgnore]
        public EditorAction ActionRipSkeleton = new EditorAction();

        [Header("Data")]
        //[SerializeField, JsonProperty]
        //public string SkeletonName = "Skeleton_0";
        [SerializeField, JsonProperty]
        public List<Bone> MyBones = new List<Bone>();
        [SerializeField, JsonIgnore, HideInInspector]
        private Bounds MyBounds;
        [SerializeField, JsonIgnore, HideInInspector]    // object spawned in world
        private SkeletonHandler SpawnedSkeleton;

        [Header("Bones")]
        [SerializeField, JsonIgnore]
        public Material BoneMaterial;
        [SerializeField, JsonIgnore]
        public float BoneSize = 0.1f;   // cube meshes
        [SerializeField, JsonIgnore]
        public Color32 BoneColor = new Color32(53, 83, 83, 255);

        [Header("Visibility")]
        [SerializeField, JsonIgnore]
        public bool IsShowJoints = true;
        [SerializeField, JsonIgnore]
        public bool IsShowBones = true;
        [SerializeField, JsonIgnore]
        public bool IsShowMeshes = true;
        [SerializeField, JsonIgnore]
        public bool IsAnimating = true;
        [SerializeField, JsonIgnore]
        public bool IsJointsColliders = true;
        [SerializeField, JsonIgnore]
        public bool IsMeshColliders = true;
        [SerializeField, JsonIgnore]
        public bool IsConvexMeshes = true;

        [Header("Events")]
        [SerializeField, JsonIgnore]
        public UnityEvent OnLoadSkeleton;

        // Linked Body Parts
        [SerializeField, JsonIgnore]
        public Transform MyCameraBone;
        [SerializeField, JsonIgnore]
        public Transform MyBoneHead;

        /*[Header("Joints")]
        [SerializeField, JsonIgnore]
        private Material JointMaterial;
        [SerializeField, JsonIgnore]
        private Color32 JointColor = new Color32(48, 90, 59, 255);
        [SerializeField, JsonIgnore]
        private float JointSize = 0.15f;

        // Generation - Move this to SkeletonMutator Script
        [JsonIgnore]
        private int BoneColorMutation = 20;*/
        [JsonIgnore]
        private Vector3 OriginalCameraPosition;
        [JsonIgnore]
        private bool IsLoading = false;

        [JsonIgnore]
        public bool IsReCalculateBounds;
        [JsonIgnore]
        private GameObject DefaultBody;
        [JsonIgnore]
        private bool IsRenderCapsule = false;
        [JsonIgnore]
        private UniversalCoroutine.Coroutine LoadRoutine;
        [JsonIgnore]
        static float SkeletonLoadDelay = 0.0f;
        #endregion

        public SkeletonAnimator GetAnimator()
        {
            return SpawnedSkeleton.GetAnimator();
        }
        public Transform GetTransform()
        {
            return SpawnedSkeleton.transform;
        }
        public void SetSkeletonHandler(SkeletonHandler MySkeletonHandler)
        {
            SpawnedSkeleton = MySkeletonHandler;
        }
        #region Mono

        /// <summary>
        /// Resets the bone colour back to the original
        /// </summary>
        public void RestoreBoneColor(Bone MyBone)
        {
            MyBone.SetJointColor();
            MyBone.SetBoneColor();
        }

        public void Update()
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
                    UniversalCoroutine.CoroutineManager.StopCoroutine(LoadRoutine);
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
#if UNITY_EDITOR //|| UNITY_STANDALONE_WIN
                System.Windows.Forms.SaveFileDialog MyDialog = new System.Windows.Forms.SaveFileDialog();
                System.Windows.Forms.DialogResult MyResult = MyDialog.ShowDialog();
                if (MyResult == System.Windows.Forms.DialogResult.OK)
                {
                    //Mesh MyMesh = ObjImport.ImportFile(MyDialog.FileName);
                    UnityFBXExporter.FBXExporter.ExportGameObjToFBX(SpawnedSkeleton.gameObject, MyDialog.FileName);
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

            if (ActionGenerateSkeleton.IsTriggered())
            {
                RunScript(FileUtil.ConvertToList(Generators.SkeletonGenerator.Get().GenerateBasicSkeleton(NameGenerator.GenerateVoxelName())));
            }

            if (ActionActivateSkeleton.IsTriggered())
            {
                Activate();
            }
            if (ActionDeactivateSkeleton.IsTriggered())
            {
                Deactivate();
            }
            if (ActionRipSkeleton.IsTriggered())
            {
                RipSkeleton();
            }
            
        }

        public void RipSkeleton()
        {
            for (int i = 0; i < MyBones.Count; i++)
            {
                MyBones[i].SetSkeleton(this);
                MyBones[i].Rip();
            }
        }

        private UniversalCoroutine.Coroutine ActivateCoroutine;
        public void Activate()
        {
            if (ActivateCoroutine != null)
            {
                UniversalCoroutine.CoroutineManager.StopCoroutine(ActivateCoroutine);
            }
            ActivateCoroutine = UniversalCoroutine.CoroutineManager.StartCoroutine(ActivateRoutine());
        }

        public IEnumerator ActivateRoutine()
        {
            for (int i = 0; i < MyBones.Count; i++)
            {
                MyBones[i].SetSkeleton(this);
                yield return UniversalCoroutine.CoroutineManager.StartCoroutine(MyBones[i].ActivateRoutine());
            }
        }

        public void Deactivate()
        {
            for (int i = 0; i < MyBones.Count; i++)
            {
                MyBones[i].Deactivate();
            }
        }


        /// <summary>
        /// Gets the layer mask the skeleton is using
        /// </summary>
       /* private void SetLayerMask(GameObject MyObject)
        {
            LayerManager.Get().SetLayerSkeleton(MyObject);
        }*/
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
            if (SpawnedSkeleton)
            {
                Debug.Log("Calculating bounds for: " + SpawnedSkeleton.name + " with " + MyBones.Count + " bones.");
                //Vector3 MyPosition = SpawnedSkeleton.transform.position;
                //transform.position += new Vector3(30, 30, 30);
                //Debug.LogError("Moving position to " + SpawnedSkeleton.transform.position.ToString());
                MyBounds = new Bounds(SpawnedSkeleton.transform.position, Vector3.zero);
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
                MyBounds.center -= SpawnedSkeleton.transform.position;
                Debug.Log("MyBounds: " + MyBounds.extents.ToString() + ":" + MyBounds.center.ToString());
                //transform.position = MyPosition;
                //MyBounds.center = new Vector3(0.009f, 0.174f, 0.009f);
                //MyBounds.size = new Vector3(0.26f, 0.91f, 0.26f);
            }
            return MyBounds;
        }

        public Bounds GetEditorBounds()
        {
            if (SpawnedSkeleton)
            {
                Bounds MyBounds = new Bounds(SpawnedSkeleton.transform.position, Vector3.zero);
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
                MyBounds.center -= SpawnedSkeleton.transform.position;
                MyBounds.center = new Vector3(0.008549809f, 0.1749992f, 0.008110046f);
                MyBounds.size = new Vector3(0.1282765f, 0.4583321f, 0.1282749f);
            }
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

        public void CalculateCapsule()
        {
            CalculateBounds();
            CapsuleCollider MyCapsule = GetCapsule();
            if (MyCapsule)
            {
                float MyHeight = MyBounds.size.y;// * 0.98f;
                if (MyHeight == 0)
                {
                    MyHeight = 0.1f;
                }
                MyCapsule.height = MyHeight;// MyBounds.extents.y * 2;
                float MyRadius = (MyBounds.extents.x + MyBounds.size.z) / 4f;//3.6f;
                if (MyRadius == 0)
                {
                    MyRadius = 0.1f;
                }
                //Vector3 MyCenter = MyBounds.center - SpawnedSkeleton.transform.position;
                MyCapsule.center = MyBounds.center;
                //MyCapsule.center = new Vector3(MyCapsule.center.x, -MyCapsule.center.y * 2f, MyCapsule.center.z);
                if (MyBounds.extents.x > MyBounds.extents.z)
                {
                    MyCapsule.radius = MyBounds.extents.x;
                }
                else
                {
                    MyCapsule.radius = MyBounds.extents.z;
                }
            }
        }
        /// <summary>
        /// Updates the Bounding box of the skeleton
        /// Called when skeleton updates
        /// </summary>
        void UpdateBounds()
        {
            //Debug.LogError("Updating bounds " + Time.time);
            if (SpawnedSkeleton.transform.parent != null)
            {
                CalculateBounds();
                AttachCameraToHead();
                /*
                Transform MySheild = SpawnedSkeleton.transform.parent.Find("Sheild");
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
            /*if (MyCameraBone == null && SpawnedSkeleton && SpawnedSkeleton.transform)
            {
                MyCameraBone = SpawnedSkeleton.transform.parent.Find("CameraBone");
            }*/
            if (MyCameraBone == null)
            {
                for (int i = 0; i < MyBones.Count; i++)
                {
                    if (MyBones[i].Name.Contains("Camera"))
                    {
                        MyCameraBone = MyBones[i].MyTransform;
                        break;
                    }
                }
            }
            return MyCameraBone;
        }

        /// <summary>
        /// Set new camera bone for skeleton
        /// </summary>
        public void SetCameraBone(Transform NewCameraBone)
        {
            MyCameraBone = NewCameraBone;
            Shooter MyShooter = SpawnedSkeleton.transform.parent.gameObject.GetComponent<Shooter>();
            if (MyShooter)
            {
                MyShooter.HotSpotTransform = MyCameraBone;
            }

            SpawnedSkeleton.transform.parent.gameObject.GetComponent<Character>().SetGuisTarget(MyCameraBone);
            /*BasicController PlayerController = SpawnedSkeleton.transform.parent.gameObject.GetComponent<BasicController>();
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
            MyBoneHead = SpawnedSkeleton.transform;
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
            /*if (MyCameraBone == null && SpawnedSkeleton.transform.parent.FindChild("CameraBone") != null)   // only do this once
            {
                SetCameraBone(SpawnedSkeleton.transform.parent.FindChild("CameraBone"));
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
                /*if (SpawnedSkeleton.transform.parent.gameObject.GetComponent<Character>().IsPlayer)
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
                MyCameraBone.SetParent(SpawnedSkeleton.transform.parent);
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
            return CreateBone(SpawnedSkeleton.transform);
        }

        /// <summary>
        /// Create A Bone, with a joint and a line
        /// </summary>
        public Bone CreateBone(Transform BoneParent = null)
        {
            if (BoneParent == null)
            {
                BoneParent = SpawnedSkeleton.transform;
            }
            Bone NewBone = new Bone(this, GenerateUniqiueBoneName());
            //Debug.LogError("Creating bone: " + MyBones.Count);
            MyBones.Add(NewBone);
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
                    MyBones[i].CreateJointMesh();
                    /*MyBones[i].MyJointCube = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                    SetLayerMask(MyBones[i].MyJointCube.gameObject);
                    // Destroy collider if no joint shown
                    if (!IsJointsColliders && MyBones[i].MyJointCube.GetComponent<BoxCollider>())
                    {
                        MonoBehaviourExtension.Kill(MyBones[i].MyJointCube.GetComponent<BoxCollider>());
                    }
                    MyBones[i].MyJointCube.name = "Joint " + i;
                    MyBones[i].MyJointCube.tag = "BonePart";
                    MyBones[i].MyJointCube.transform.localScale = new Vector3(JointSize, JointSize, JointSize);
                    MyBones[i].MyJointCube.transform.SetParent(MyBones[i].MyTransform, false);*/
                }
                if (IsShowBones && MyBones[i].BodyCube == null && MyBones[i].ParentTransform != SpawnedSkeleton.transform)
                {
                    MyBones[i].CreateBoneMesh();
                    /*MyBones[i].BodyCube = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                    SetLayerMask(MyBones[i].BodyCube.gameObject);
                    if (MyBones[i].BodyCube.GetComponent<BoxCollider>())
                    {
                        MonoBehaviourExtension.Kill(MyBones[i].BodyCube.GetComponent<BoxCollider>());
                    }
                    MyBones[i].BodyCube.name = "BoneMesh " + i;
                    MyBones[i].BodyCube.tag = "BonePart";
                    MyBones[i].BodyCube.transform.localScale = new Vector3(BoneSize, BoneSize, BoneSize);
                    MyBones[i].BodyCube.transform.SetParent(MyBones[i].MyTransform, false);*/
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
                        SpawnedSkeleton.GetAnimator().DeleteAllKeysFromAllAnimations(MyBone);
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

        private Zeltine RestoratingPoseHandle;
        public void RestoreDefaultPose(float TimeTaken)
        {
            RestoratingPoseHandle = RoutineManager.Get().StartCoroutine(RestoratingPoseHandle, RestoreDefaultPoseRoutine(TimeTaken));
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

        public void DestroyBodyCubes()
        {
            for (int i = 0; i < MyBones.Count; i++)
            {
                if (MyBones[i].MyTransform && MyBones[i].ParentTransform)
                {
                    Bone MyBone = MyBones[i];
                    if (MyBone.BodyCube)    // bone mesh
                    {
                        MyBone.BodyCube.gameObject.Die();
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
                MyBones[i].SetBoneColor();
                MyBones[i].SetJointColor();
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
            if (SpawnedSkeleton.transform.parent && SpawnedSkeleton.transform.parent.GetComponent<Character>())
            {
                CapsuleCollider = SpawnedSkeleton.transform.parent.gameObject.GetComponent<CapsuleCollider>();
            }
            else
            {
                if (CapsuleCollider == null)
                {
                    GameObject CapsuleObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    CapsuleObject.name = SpawnedSkeleton.name + "_Capsule";
                    CapsuleObject.layer = SpawnedSkeleton.gameObject.layer;
                    CapsuleObject.transform.SetParent(SpawnedSkeleton.transform);
                    CapsuleObject.transform.position = SpawnedSkeleton.transform.position;
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
                CapsuleObject.name = SpawnedSkeleton.name + "_CapsuleRenderer";
                CapsuleObject.layer = SpawnedSkeleton.gameObject.layer;
                CapsuleObject.transform.SetParent(SpawnedSkeleton.transform);
                CapsuleObject.transform.position = SpawnedSkeleton.transform.position;
                GameObject.Destroy(CapsuleObject.GetComponent<CapsuleCollider>());
                CapsuleRenderer = CapsuleObject.GetComponent<MeshRenderer>();
                Material JointMaterial = new Material(Shader.Find("Standard"));
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
            Debug.Log("Setting SKeleton [" + SpawnedSkeleton.name + "]'s Mesh Colliders: " + IsColliders);
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
            if (SpawnedSkeleton.transform.parent)
            {
                Debug.LogError("Setting Joint Colliders of " + SpawnedSkeleton.transform.parent.name + " to " + IsColliders);
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
        /// Searches SpawnedSkeleton.transform tree for bones with a tag
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



        #region File

        /// <summary>
        /// Returns the skeletons data
        /// </summary>
        public static List<string> GetSkeletonSection(List<string> MyLines)
        {
            //List<string> MySections = SplitSections(MyFiles);
            List<string> MySkeletonScript = new List<string>();
            int BeginIndex = -1;
            for (int i = 0; i < MyLines.Count; i++)
            {
                if (MyLines[i].Contains("/BeginSkeleton"))
                {
                    BeginIndex = i;
                }
                else if (MyLines[i].Contains("/EndSkeleton"))
                {
                    if (BeginIndex != -1)
                    {
                        MySkeletonScript = MyLines.GetRange(BeginIndex, i - BeginIndex + 1);
                        //Debug.LogError("SkeletonScript: " + MySkeletonScript.Count + ":" + MySkeletonScript[MySkeletonScript.Count-1]);
                        Debug.LogError("Skeleton Section: \n" + FileUtil.ConvertToSingle(MySkeletonScript));
                        return MySkeletonScript;
                    }
                    break;  // end search here
                }
            }
            Debug.LogError("Could not find skeleton section:\n" + FileUtil.ConvertToSingle(MyLines));
            return MySkeletonScript;
        }

        /// <summary>
        /// Returns a list of strings (commands and data) to read in from a file.
        /// </summary>
        public List<string> GetScriptList()
        {
            List<string> Data = new List<string>();
            Skeleton MySkeleton = SpawnedSkeleton.transform.GetComponent<Skeleton>();
            // for each bone, save its index, and the index of its parent bone
            // save its position, rotation, scale
            Data.Add("/BeginSkeleton " + Name);
            GetCapsule();
            if (CapsuleCollider)
            {
                Data.Add("/Capsule");
                Data.Add(CapsuleCollider.height + "");
                Data.Add(CapsuleCollider.radius + "");
                Data.Add(CapsuleCollider.center.x + "");
                Data.Add(CapsuleCollider.center.y + "");
                Data.Add(CapsuleCollider.center.z + "");
            }
            for (int i = 0; i < MySkeleton.MyBones.Count; i++)
            { // - MySkeletonPosition.x
                Bone MyBone = MySkeleton.MyBones[i];
                Transform ThisJoint = MyBone.MyTransform;
                if (ThisJoint)
                {
                    Data.Add("/Bone");
                    Data.Add("" + GetParentIndex(MySkeleton.MyBones[i]));
                    if (MyBone.Name != "")
                    {
                        Data.Add("/Name");
                        Data.Add(MyBone.Name);
                    }
                    if (MyBone.Tag != "")
                    {
                        Data.Add("/Tag");
                        Data.Add(MyBone.Tag);
                    }
                    if (ThisJoint.localPosition != new Vector3(0, 0, 0))
                    {
                        Data.Add("/Position");
                        Data.Add("" + (ThisJoint.localPosition.x));  // position
                        Data.Add("" + (ThisJoint.localPosition.y));  // position
                        Data.Add("" + (ThisJoint.localPosition.z));  // position
                        Data.Add("/EndPosition");
                    }
                    if (ThisJoint.localRotation != Quaternion.identity)
                    {
                        Data.Add("/Rotation");
                        Data.Add("" + ThisJoint.localRotation.x);  // rotation
                        Data.Add("" + ThisJoint.localRotation.y);  // rotation
                        Data.Add("" + ThisJoint.localRotation.z);  // rotation
                        Data.Add("" + ThisJoint.localRotation.w);  // rotation
                        Data.Add("/EndRotation");
                    }
                    if (ThisJoint.localScale != new Vector3(1, 1, 1))
                    {
                        Data.Add("/Scale");
                        Data.Add("" + ThisJoint.localScale.x);  // scale
                        Data.Add("" + ThisJoint.localScale.y);  // scale
                        Data.Add("" + ThisJoint.localScale.z);  // scale
                        Data.Add("/EndScale");
                    }
                    if (MyBone.VoxelMesh)
                    {
                        Data.Add("/VoxelMesh");
                        // World script
                        List<string> MyMeshScript = MyBone.VoxelMesh.GetComponent<World>().GetScript();
                        Data.AddRange(MyMeshScript);
                        // Mesh Name
                        Data.Add("/EndVoxelMesh");
                        if (MyBone.VoxelMesh.localPosition != new Vector3(0, 0, 0))
                        {
                            Data.Add("/MeshPosition");
                            Data.Add("" + (MyBone.VoxelMesh.localPosition.x));  // position
                            Data.Add("" + (MyBone.VoxelMesh.localPosition.y));  // position
                            Data.Add("" + (MyBone.VoxelMesh.localPosition.z));  // position
                            Data.Add("/EndMeshPosition");
                        }
                        if (MyBone.VoxelMesh.localRotation != Quaternion.identity)
                        {
                            Data.Add("/MeshRotation");
                            Data.Add("" + (MyBone.VoxelMesh.localRotation.x));
                            Data.Add("" + (MyBone.VoxelMesh.localRotation.y));
                            Data.Add("" + (MyBone.VoxelMesh.localRotation.z));
                            Data.Add("" + (MyBone.VoxelMesh.localRotation.w));
                            Data.Add("/EndMeshRotation");
                        }
                        if (MyBone.VoxelMesh.localScale != new Vector3(1, 1, 1))
                        {
                            Data.Add("/MeshScale");
                            Data.Add("" + (MyBone.VoxelMesh.localScale.x));  // position
                            Data.Add("" + (MyBone.VoxelMesh.localScale.y));  // position
                            Data.Add("" + (MyBone.VoxelMesh.localScale.z));  // position
                            Data.Add("/EndMeshScale");
                        }
                    }
                    Data.Add("/EndBone");
                }
            }
            // Skeleton Data, save all these things as default pose!
            SkeletonAnimator MySkeletonAnimator = SpawnedSkeleton.GetComponent<SkeletonAnimator>();
            Data.AddRange(MySkeletonAnimator.GetScript());
            Data.Add("/EndSkeleton");
            return Data;
        }

        /// <summary>
        /// Runs the script
        /// </summary>
        public void RunScript(List<string> Data)
        {
            if (SpawnedSkeleton)
            {
                if (LoadRoutine != null)
                {
                    UniversalCoroutine.CoroutineManager.StopCoroutine(LoadRoutine);
                    IsLoading = false;
                }
                LoadRoutine = UniversalCoroutine.CoroutineManager.StartCoroutine(RunScriptRoutine(Data));
            }
        }

        /// <summary>
        /// Loads the skeleton in a routine and gives it a name
        /// </summary>
        public IEnumerator Load(string RaceName, List<string> Data)
        {
            SpawnedSkeleton.transform.parent.gameObject.GetComponent<Character>().SetRace(RaceName);
            yield return UniversalCoroutine.CoroutineManager.StartCoroutine(RunScriptRoutine(Data));
        }

        /// <summary>
        /// Is loading skeleton
        /// </summary>
        public bool IsLoadingSkeleton()
        {
            return IsLoading;
        }

        public void ForceStopLoad()
        {
            if (IsLoading)
            {
                IsLoading = false;
                if (LoadRoutine != null)
                {
                    UniversalCoroutine.CoroutineManager.StopCoroutine(LoadRoutine);
                }
            }
        }

        /// <summary>
        /// Loads the skeleton in a routine
        /// </summary>
        public IEnumerator RunScriptRoutine(List<string> Data)
        {
            if (!IsLoading)
            {
                IsLoading = true;
                float LoadSkeletonBeginTime = Time.realtimeSinceStartup;
                yield return (Clear()); //CharacterManager.Get().StartCoroutine
                MyBones.Clear();
                Bone MyBone = new Bone();
                bool IsReading = false;
                Debug.Log("Loading skeleton : " + SpawnedSkeleton.transform.name + "\n " + FileUtil.ConvertToSingle(Data));

                #region SkeletonData
                for (int i = 0; i < Data.Count; i++)
                {
                    if (!IsReading)   // nothing
                    {
                        //SkeletonName = name;
                        if (Data[i].Contains("/BeginSkeleton"))
                        {
                            string NewSkeletonName = ScriptUtil.RemoveCommand(Data[i]);
                            if (NewSkeletonName != "/BeginSkeleton" && NewSkeletonName != "")
                            {
                                Name = NewSkeletonName;
                                IsReading = true;
                            }
                            else
                            {
                                Debug.LogError("Line: " + Data[i] + " contains /BeginSkeleton with no skeleton name");
                            }
                        }
                    }
                    else
                    {
                        if (Data[i] == "/EndSkeleton")
                        {
                            IsReading = false;
                            //Debug.LogError("Ending skeleton on line: " + i);
                            break;
                        }
                        else if (Data[i] == "/Name")
                        {
                            MyBone.Name = Data[i + 1];
                            i += 1;
                        }
                        else if (Data[i] == "/Bone")
                        {
                            int ParentIndex = int.Parse(Data[i + 1]);
                            MyBone = CreateBone();
                            //MyBone = CreateBoneLoading(ParentIndex);
                            Debug.Log("Loading skeleton Bone: " + SpawnedSkeleton.transform.name + ":" + ParentIndex);
                            //yield return new WaitForSeconds(SkeletonLoadDelay);
                            i += 1;
                        }
                        else if (Data[i] == "/Capsule")
                        {
                            float CapsuleHeight = float.Parse(Data[i + 1]);
                            float CapsuleRadius = float.Parse(Data[i + 2]);
                            Vector3 CapsuleCenter = Vector3.zero;
                            CapsuleCenter.x = float.Parse(Data[i + 3]);
                            CapsuleCenter.y = float.Parse(Data[i + 4]);
                            CapsuleCenter.z = float.Parse(Data[i + 5]);
                            GetCapsule();
                            if (CapsuleCollider)
                            {
                                CapsuleCollider.height = CapsuleHeight;
                                CapsuleCollider.radius = CapsuleRadius;
                                CapsuleCollider.center = CapsuleCenter;
                            }
                            i += 5;
                        }
                        else if (Data[i].Contains("/BeginSkeletonAnimator"))
                        {
                            //Debug.LogError("Loading Skeleton Animator " + i);
                            for (int j = i; j < Data.Count; j++)
                            {
                                if (Data[j].Contains("/EndSkeletonAnimator"))
                                {
                                    //Debug.LogError("End Skeleton Animator " + j);
                                    int Index1 = i + 1;
                                    int Index2 = j - 1;
                                    int ElementCount = (Index2 - Index1) + 1;
                                    List<string> MyScript = Data.GetRange(Index1, ElementCount);
                                    //Debug.LogError("Script:\n" + FileUtil.ConvertToSingle(MyScript));
                                    float TimeBegin = Time.realtimeSinceStartup;
                                    if (SpawnedSkeleton.GetAnimator() != null)
                                    {
                                        SpawnedSkeleton.GetAnimator().RunScript(MyScript);
                                    }
                                    else
                                    {
                                        Debug.LogError(SpawnedSkeleton.name + " does not have an animator.");
                                    }
                                    //Debug.Log("Time taken to load animation: " + (Time.realtimeSinceStartup - TimeBegin));
                                    i = j;
                                    //yield return new WaitForSeconds(SkeletonLoadDelay);
                                    break;
                                }
                            }
                        }
                        /*else if (Zeltex.Util.ScriptUtil.RemoveWhiteSpace(Data[i]) == "/VoxelMesh")
                        {
                            Debug.Log("Creating VoxelMesh");
                        }*/
                        else if (MyBone.MyTransform)
                        {
                            if (Data[i] == "/Tag")
                            {
                                MyBone.Tag = Data[i + 1];
                                i += 1;
                            }
                            else if (Data[i] == "/Position")
                            {
                                float PositionX = float.Parse(Data[i + 1]);
                                float PositionY = float.Parse(Data[i + 2]);
                                float PositionZ = float.Parse(Data[i + 3]);
                                MyBone.MyTransform.localPosition = new Vector3(PositionX, PositionY, PositionZ);
                                i += 3;
                                //Debug.LogError("Setting new position for bone " + (MyBones.Count-1) + " - " + MyBones[MyBones.Count - 1].position.ToString());
                            }
                            else if (Data[i] == "/Rotation")
                            {
                                float RotationX = float.Parse(Data[i + 1]);
                                float RotationY = float.Parse(Data[i + 2]);
                                float RotationZ = float.Parse(Data[i + 3]);
                                float RotationW = float.Parse(Data[i + 4]);
                                MyBone.MyTransform.localRotation = new Quaternion(RotationX, RotationY, RotationZ, RotationW);
                                i += 4;
                            }
                            else if (Data[i] == "/Scale")
                            {
                                try
                                {
                                    MyBone.MyTransform.localScale.Set(
                                        float.Parse(Data[i + 1]),
                                        float.Parse(Data[i + 2]),
                                        float.Parse(Data[i + 3]));
                                }
                                catch (System.Exception e)
                                {
                                    Debug.LogError(e.ToString());
                                }
                                i += 3;
                            }
                            else if (ScriptUtil.RemoveWhiteSpace(Data[i]) == "/VoxelMesh")
                            {
                                Debug.Log("Loading VoxelMesh: " + SpawnedSkeleton.transform.name + ":" + i);
                                for (int j = i + 1; j < Data.Count; j++)
                                {
                                    if (Data[j] == "/EndVoxelMesh")
                                    {
                                        int Index1 = i + 1;
                                        int Index2 = j - 1;
                                        int ElementCount = (Index2 - Index1) + 1; // from i to j-1
                                        List<string> MyMeshScript = Data.GetRange(Index1, ElementCount);
                                        if (MyBones.Count > 0)
                                        {
                                            Debug.Log("Loading skeleton Mesh: " + SpawnedSkeleton.transform.name + ":" + MyMeshScript.Count + "\n " + FileUtil.ConvertToSingle(MyMeshScript));
                                            //yield return CoroutineManager.StartCoroutine(MyBone.CreateMeshRoutine(MyMeshScript));
                                            /*yield return CoroutineManager.StartCoroutine(CreateMeshRoutine(
                                                MyBone,
                                                MyMeshScript,
                                                false));    //CharacterManager.Get().StartCoroutine*/
                                        }
                                        i = j;  // skuo to endVoxelMesh line
                                        break;
                                    }
                                }
                                //yield return new WaitForSeconds(SkeletonLoadDelay);
                            }
                            else if (Data[i] == "/MeshPosition")
                            {
                                float PositionX = float.Parse(Data[i + 1]);
                                float PositionY = float.Parse(Data[i + 2]);
                                float PositionZ = float.Parse(Data[i + 3]);
                                if (MyBone.VoxelMesh != null)
                                {
                                    MyBone.VoxelMesh.transform.localPosition = new Vector3(PositionX, PositionY, PositionZ);
                                }
                                i += 3;
                            }
                            else if (Data[i] == "/MeshRotation")
                            {
                                float RotationX = float.Parse(Data[i + 1]);
                                float RotationY = float.Parse(Data[i + 2]);
                                float RotationZ = float.Parse(Data[i + 3]);
                                float RotationW = float.Parse(Data[i + 4]);
                                if (MyBone.VoxelMesh != null)
                                {
                                    MyBone.VoxelMesh.transform.localRotation =
                                        new Quaternion(RotationX, RotationY, RotationZ, RotationW);
                                }
                                i += 4;
                            }
                            else if (Data[i] == "/MeshScale")
                            {
                                float PositionX = float.Parse(Data[i + 1]);
                                float PositionY = float.Parse(Data[i + 2]);
                                float PositionZ = float.Parse(Data[i + 3]);
                                if (MyBone.VoxelMesh != null)
                                {
                                    MyBone.VoxelMesh.transform.localScale = new Vector3(PositionX, PositionY, PositionZ);
                                }
                                i += 3;
                            }
                            else if (Data[i] == "/EndBone")
                            {
                                MyBone = new Bone();    // empty is
                            }
                        }
                    }
                }
                #endregion
                SetDefaultPose();
                RefreshCapsule();
                if (MyBones.Count == 0)
                {
                    // Create default mesh or something? a transparent cube?
                    CreateBone(); // a default bone
                    // Create a default mesh too - a cube
                }
                SetMeshColliders(IsMeshColliders);
                SetMeshVisibility(true);
                UpdateBounds();
                if (DefaultBody)
                {
                    DefaultBody.SetActive(MyBones.Count == 0);
                }
                IsLoading = false;
            }
            else
            {
                Debug.LogWarning("Trying to load skeleton while already loading.");
            }
            //Debug.Log("Finished Loading Skeleton: " + (Time.realtimeSinceStartup - LoadSkeletonBeginTime));
        }

        /// <summary>
        /// Creates a new bone for the loading skeleton
        /// </summary>
        /*private Bone CreateBoneLoading(int ParentIndex)
        {
            GameObject NewBone = new GameObject();
            LayerManager.Get().SetLayerSkeleton(NewBone);
            NewBone.name = "Bone " + MyBones.Count;
            NewBone.tag = "Bone";
            if (ParentIndex >= 0 && ParentIndex < MyBones.Count &&
                MyBones[ParentIndex] != null && MyBones[ParentIndex].MyTransform != null)
            {
                NewBone.transform.SetParent(MyBones[ParentIndex].MyTransform, false);
            }
            else
            {
                NewBone.transform.SetParent(SpawnedSkeleton.transform, false);
            }
            NewBone.transform.position = NewBone.transform.parent.position;
            NewBone.transform.rotation = NewBone.transform.parent.rotation;
            NewBone.transform.localScale = new Vector3(1, 1, 1);
            // Lists
            Bone MyBone = CreateBoneData(NewBone.transform.parent, NewBone.transform, MyBones.Count - 1);
            MyBone.MyTransform = NewBone.transform;
            MyBone.ParentTransform = NewBone.transform.parent;
            MyBones.Add(MyBone);  //ParentBone
            return MyBone;
        }*/

        #endregion


        /// <summary>
        /// Gets a debug string for the voxels
        /// </summary>
        private string GetVoxelDebugInfo(List<string> MyData)
        {
            string MyDebugInfo = "";
            List<int> VoxelCount = new List<int>();
            for (int i = 0; i < 25; i++)
            {
                VoxelCount.Add(0);
            }
            for (int i = 0; i < MyData.Count; i++)
            {
                string[] MyInput = MyData[i].Split(' ');
                if (MyInput.Length == 1)
                {
                    VoxelCount[int.Parse(MyInput[0])] += 1;
                }
                else
                {
                    VoxelCount[22] += 1;
                }
            }
            for (int i = 0; i < VoxelCount.Count; i++)
            {
                MyDebugInfo += "Voxel type [" + i + "] with count of [" + VoxelCount[i] + "]\n";
            }
            return MyDebugInfo;
        }
    }
}