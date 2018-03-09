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
    public class Skeleton : ElementCore
    {
		#region Variables
		[Header("Data")]
		[SerializeField, JsonProperty]
		public List<Bone> MyBones = new List<Bone>();
		[SerializeField, JsonIgnore, HideInInspector]
		private Bounds MyBounds;
		[SerializeField, JsonIgnore, HideInInspector]    // object spawned in world
		private SkeletonHandler SpawnedSkeleton;

        [Header("Bones")]
        [SerializeField, JsonIgnore]
        public Material BoneMaterial;
		[JsonIgnore, HideInInspector]
        public float BoneSize = 0.02f;   // cube meshes
		[JsonIgnore, HideInInspector]
        public Color32 BoneColor = new Color32(53, 83, 83, 255);

        [Header("Visibility")]
		[JsonIgnore, HideInInspector]
		public bool IsShowJoints = false;
		[JsonIgnore, HideInInspector]
		public bool IsShowBones = false;
		[JsonIgnore, HideInInspector]
		public bool IsShowMeshes = true;
		[JsonIgnore, HideInInspector]
		public bool IsAnimating = true;
		[JsonIgnore, HideInInspector]
		public bool IsJointsColliders = false;
		[JsonIgnore, HideInInspector]
        public bool IsMeshColliders = true;

        [Header("Events")]
        [SerializeField, JsonIgnore]
        public UnityEvent OnLoadSkeleton;

        // Linked Body Parts
        [SerializeField, JsonIgnore]
        public Transform MyCameraBone;
        [SerializeField, JsonIgnore]
        public Transform MyBoneHead;

		[JsonIgnore, HideInInspector]
		private Vector3 OriginalCameraPosition;
		[JsonIgnore, HideInInspector]
        private bool IsLoading = false;

		[JsonIgnore, HideInInspector]
        public bool IsReCalculateBounds;
        [JsonIgnore]
		private GameObject DefaultBody;
		[JsonIgnore, HideInInspector]
        private bool IsRenderCapsule = false;
        [JsonIgnore]
		private Zeltine LoadRoutine = null;

		private Zeltine ActivateCoroutine;
		private Zeltine RestoratingPoseHandle;
		private CapsuleCollider MyCapsuleCollider;
		private MeshRenderer CapsuleRenderer;
        [JsonIgnore]
        private static float MinimumHeight = 0.04f;
        #endregion

        public override void OnLoad() 
        {
            base.OnLoad();
            for (int i = 0; i < MyBones.Count; i++)
            {
                MyBones[i].ParentElement = this;
                MyBones[i].OnLoad();
            }
        }

		public Bone GetBone(Transform BoneTransform)
		{
			for (int i = 0; i < MyBones.Count; i++)
			{
				if (MyBones[i].MyTransform == BoneTransform)
				{
					return MyBones[i];
				}
			}
			return null;
		}

        public Zanimator GetAnimator()
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

		public void CheckTransforms()
		{
			if (HasSpawned())
			{
				for (int i = 0; i < MyBones.Count; i++)
				{
					MyBones[i].CheckForChange();
				}
			}
		}

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
        }

		public void ExportAsFBX() 
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

        public void RipSkeleton()
        {
            for (int i = 0; i < MyBones.Count; i++)
            {
                MyBones[i].SetSkeleton(this);
                MyBones[i].Rip();
            }
        }

        public void Activate(System.Action OnCompleteActivation = null)
        {
            if (ActivateCoroutine != null)
            {
				RoutineManager.Get().StopCoroutine(ActivateCoroutine);
            }
			ActivateCoroutine = RoutineManager.Get().StartCoroutine(ActivateRoutine(OnCompleteActivation));
        }

        public IEnumerator ActivateRoutine(System.Action OnCompleteActivation = null, bool IsCalculateCapsule = true)
        {
            if (!IsActivated)
            {
                IsActivated = true;
                //Debug.Log("Skeleton " + Name + " Is activating the bones: " + MyBones.Count);
                for (int i = 0; i < MyBones.Count; i++)
                {
                    MyBones[i].SetSkeleton(this);   //RoutineManager.Get().StartRoutine
                    yield return MyBones[i].ActivateRoutine();
                }
                for (int i = 0; i < MyBones.Count; i++)
                {
                    MyBones[i].AttachBoneToParent();
                }
                if (IsCalculateCapsule)
                {
                    CalculateCapsule();
                }
                if (OnCompleteActivation != null)
                {
                    OnCompleteActivation.Invoke();
                }
            }
        }

        protected bool IsActivated = false;

        public void Deactivate()
        {
            if (IsActivated)
            {
                IsActivated = false;
                for (int i = 0; i < MyBones.Count; i++)
                {
                    MyBones[i].Deactivate();
                }
            }
        }

        public void Reactivate() 
        {
            Deactivate();
            Activate();
        }
        #endregion

        #region Bounds

        /// <summary>
        /// Gets the skeletons bounds
        /// </summary>
        public Bounds GetBounds()
        {
            if (MyBounds.size == Vector3.zero)
            {
                CalculateBounds();
                if (MyBounds.size == Vector3.zero)
                {
                    MyBounds.size = new Vector3(0.5f, 0.5f, 0.5f);
                }
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
                MyBounds = new Bounds(SpawnedSkeleton.transform.position, Vector3.zero);
                for (int i = 0; i < MyBones.Count; i++)
                {
                    if (MyBones[i].VoxelMesh)
                    {
                        World MyWorld = MyBones[i].VoxelMesh.GetComponent<World>();
                        MyBounds = AddWorldToBounds(MyWorld, MyBounds);
                    }
                }
                MyBounds.center -= SpawnedSkeleton.transform.position;
            }
            return MyBounds;
        }

        /// <summary>
        /// Adds the worlds to bounds
        /// </summary>
        private Bounds AddWorldToBounds(World MyWorld, Bounds MyBounds)
        {
            if (MyWorld)
            {
                if (MyWorld.IsSingleChunk())
                {
                    MeshRenderer MyBodyRenderer = MyWorld.gameObject.GetComponent<MeshRenderer>();
                    MeshFilter MyMeshFilter = MyWorld.gameObject.GetComponent<MeshFilter>();
                    if (MyMeshFilter.sharedMesh && MyMeshFilter.sharedMesh.vertexCount > 0)
                    {
                        MyBounds.Encapsulate(MyBodyRenderer.bounds);
                    }
                }
                else
                {
                    foreach (Int3 MyKey in MyWorld.MyChunkData.Keys)
                    {
                        Chunk MyChunk = MyWorld.MyChunkData[MyKey];
                        MeshRenderer MyBodyRenderer = MyChunk.GetComponent<MeshRenderer>();
                        MeshFilter MyMeshFilter = MyChunk.GetComponent<MeshFilter>();
                        if (MyMeshFilter.sharedMesh && MyMeshFilter.sharedMesh.vertexCount > 0)
                        {
                            MyBounds.Encapsulate(MyBodyRenderer.bounds);
                        }
                    }
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
                float MyHeight = MyBounds.size.y + 0.12f;// * 0.98f;
                if (MyHeight < MinimumHeight)
                {
                    MyHeight = MinimumHeight;
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
            MyBoneHead = SpawnedSkeleton.transform;
            //Bone HeadBone = GetBoneWithTag("Head");
            if (MyBoneHead != null && MyCameraBone != null)
            {
                //Debug.Log("Moving Camera Bone to: " + MyBoneHead.transform.position.ToString());
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
            // Find bone with tag Head
            //float CameraDistanceZ = 0;

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
            /*if (MyBoneHead != null && MyBoneHead.GetComponent<MeshRenderer>())
            {
                MyBoneHead.GetComponent<MeshRenderer>().enabled = false;
            }*/
        }
        #endregion

        #region Bones
        public void AddBoneWithPoly(string PolyName, int TextureMapIndex)
        {
            PolyModel MyModel = DataManager.Get().GetElement(DataFolderNames.PolyModels, PolyName) as PolyModel;
            if (MyModel != null)
            {
                MyModel = MyModel.Clone<PolyModel>();
                AddBoneWithPoly(MyModel, TextureMapIndex);
            }
            else
            {
                Debug.LogError("Could not find model of name: " + PolyName);
            }
        }
        /// <summary>
        /// Creates a new bone with a voxel model - by converting the model into an item
        /// </summary>
        public void AddBoneWithPoly(PolyModel NewBoneModel, int TextureMapIndex)
        {
            Bone NewBone = CreateBone(SpawnedSkeleton.transform);
            NewBone.SetItem(NewBoneModel.GenerateItem(TextureMapIndex));
            NewBone.ActivateSingle();
        }

        /// <summary>
        /// Imports a mesh from DataManager and uses it for the bone
        /// </summary>
        /// <param name="MeshName"></param>
        public void AddBoneWithMesh(string MeshName)
        {
            VoxelModel MyModel = DataManager.Get().GetElement(DataFolderNames.VoxelModels, MeshName) as VoxelModel;
            if (MyModel != null)
            {
                MyModel = MyModel.Clone<VoxelModel>();
                AddBoneWithModel(MyModel);
            }
            else
            {
                Debug.LogError("Could not find model of name: " + MeshName);
            }
        }

        /// <summary>
        /// Creates a new bone with a voxel model - by converting the model into an item
        /// </summary>
        public void AddBoneWithModel(VoxelModel NewBoneModel)
        {
            Bone NewBone = CreateBone(SpawnedSkeleton.transform);
            NewBone.MyItem = NewBoneModel.GenerateItem();
            NewBone.ActivateSingle();
        }

        public void AddBoneWithItem(string NewItem)
        {
            Items.Item MyItem = DataManager.Get().GetElement(DataFolderNames.Items, NewItem) as Items.Item;
            if (MyItem != null)
            {
                MyItem = MyItem.Clone<Items.Item>();
                AddBoneWithItem(MyItem);
            }
            else
            {
                Debug.LogError("Could not find item of name: " + NewItem);
            }
        }

        public void AddBoneWithItem(Items.Item NewBoneModel)
        {
            Bone NewBone = CreateBone(SpawnedSkeleton.transform);
            NewBone.MyItem = NewBoneModel;
            NewBone.ActivateSingle();
        }


        public void Clear()
		{
			RoutineManager.Get().StartCoroutine(ClearRoutine());
		}
        /// <summary>
        /// Clears the skeleton in a routine
        /// </summary>
        public IEnumerator ClearRoutine()
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
                    MyBones[i].MyTransform.gameObject.Die();
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
        /// Create A Bone, with a joint and a line
        /// </summary>
        public Bone CreateBone(Transform BoneParent = null)
		{
			if (SpawnedSkeleton)
			{
				if (BoneParent == null)
				{
					BoneParent = SpawnedSkeleton.transform;
				}
				Bone NewBone = new Bone(this, GenerateUniqiueBoneName());
				if (BoneParent)
				{
					NewBone.ParentName = BoneParent.name;
				}
				MyBones.Add(NewBone);
				return NewBone;
			}
			else
			{
				Debug.LogError("No Skeleton is spawned for: " + Name);
				return null;
			}
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
                }
                if (IsShowBones && MyBones[i].BodyCube == null && MyBones[i].ParentTransform != SpawnedSkeleton.transform)
                {
                    MyBones[i].CreateBoneMesh();
                }
                if (MyBones[i].MyJointCube != null)
                {
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
                        //Debug.Log("Removing bone: " + MyBone.name + " - at index: " + i);
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
                MyBone.gameObject.Die(); // after removing all children/grandchildren, will destroy the object
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
                //Debug.LogError("Bone " + i + " going from " + OldPositions[i].ToString() + " TO " + NewPositions[i].ToString());
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

        /// <summary>
        /// Gets a capsule, if a character exists it gets it off that, otherwise gets it off the skeleton
        /// </summary>
        public CapsuleCollider GetCapsule()
        {
			if (SpawnedSkeleton)
			{
				if (SpawnedSkeleton.transform.parent && SpawnedSkeleton.transform.parent.GetComponent<Character>())
				{
                    MyCapsuleCollider = SpawnedSkeleton.transform.parent.gameObject.GetComponent<CapsuleCollider>();
				}
				else
				{
					if (MyCapsuleCollider == null)
					{
						GameObject CapsuleObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
						CapsuleObject.name = SpawnedSkeleton.name + "_Capsule";
						CapsuleObject.layer = SpawnedSkeleton.gameObject.layer;
						CapsuleObject.transform.SetParent(SpawnedSkeleton.transform);
						CapsuleObject.transform.position = SpawnedSkeleton.transform.position;
                        MyCapsuleCollider = CapsuleObject.GetComponent<CapsuleCollider>();
                        CapsuleObject.GetComponent<MeshRenderer>().Die();
						CapsuleObject.GetComponent<MeshFilter>().Die();
                        GetCapsuleRenderer();
					}
				}
			}
            return MyCapsuleCollider;
        }
        /// <summary>
        /// Gets a capsule, if a character exists it gets it off that, otherwise gets it off the skeleton
        /// </summary>
        public MeshRenderer GetCapsuleRenderer()
        {
            if (CapsuleRenderer == null)
            {
                GameObject CapsuleObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                CapsuleObject.GetComponent<CapsuleCollider>().Die();    // Destroy the naturally created object
                CapsuleObject.name = SpawnedSkeleton.name + "_CapsuleRenderer";
                CapsuleObject.layer = SpawnedSkeleton.gameObject.layer;
                CapsuleObject.transform.SetParent(SpawnedSkeleton.transform);
                CapsuleObject.transform.position = SpawnedSkeleton.transform.position;
                CapsuleRenderer = CapsuleObject.GetComponent<MeshRenderer>();
                Material JointMaterial = new Material(Shader.Find("Standard"));
                CapsuleRenderer.sharedMaterial = JointMaterial;
                CapsuleRenderer.enabled = IsRenderCapsule;
            }
            return CapsuleRenderer;
        }

        private void RefreshCapsule()
        {
            GetCapsule();
            if (MyCapsuleCollider)
            {
                SetCapsuleRadius(MyCapsuleCollider.radius);
                SetCapsuleHeight(MyCapsuleCollider.height);
                SetCapsuleCenter(MyCapsuleCollider.center);
            }
        }

        public void SetCapsuleHeight(float NewHeight)
        {
            if (MyCapsuleCollider)
            {
                MyCapsuleCollider.height = NewHeight;
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
            if (MyCapsuleCollider)
            {
                MyCapsuleCollider.radius = NewRadius;
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
            if (MyCapsuleCollider)
            {
                MyCapsuleCollider.center = NewCenterPosition;
                if (CapsuleRenderer)
                {
                    CapsuleRenderer.transform.localPosition = NewCenterPosition;
                }
            }
        }
        public Vector3 GetCapsuleCenter()
        {
            if (MyCapsuleCollider)
            {
                return MyCapsuleCollider.center;
            }
            else
            {
                return Vector3.zero;
            }
        }

        public float GetCapsuleHeight()
        {
            if (MyCapsuleCollider)
            {
                return MyCapsuleCollider.height;
            }
            else
            {
                return 0;
            }
        }
        public float GetCapsuleRadius()
        {
            if (MyCapsuleCollider)
            {
                return MyCapsuleCollider.radius;
            }
            else
            {
                return 0;
            }
        }

        public void SetCapsuleCollider(bool NewState)
        {
            GetCapsule();
            if (MyCapsuleCollider)
            {
                MyCapsuleCollider.enabled = NewState;
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
            //Debug.Log("Setting SKeleton [" + SpawnedSkeleton.name + "]'s Mesh Colliders: " + IsColliders);
            for (int i = 0; i < MyBones.Count; i++)
            {
                if (MyBones[i].VoxelMesh)
                {
                    World MyWorld = MyBones[i].VoxelMesh.GetComponent<World>();
                    if (MyWorld)
                    {
                        MyWorld.SetColliders(IsColliders);
                    }
                    //else
                    {
                        //Debug.LogError(MyBones[i].Name + " has no world on its mesh object.");
                    }
                }
            }
         }

        /// <summary>
        /// Sets the collision of the joints
        /// </summary>
        public void SetJointColliders(bool IsColliders)
        {
            //if (SpawnedSkeleton.transform.parent)
            {
                //Debug.LogError("Setting Joint Colliders of " + SpawnedSkeleton.transform.parent.name + " to " + IsColliders);
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
                    MyBones[i].MyJointCube.gameObject.GetComponent<MeshRenderer>().sharedMaterial.shader = MyShader;
                }
                if (MyBones[i].BodyCube)
                {
                    MyBones[i].BodyCube.gameObject.GetComponent<MeshRenderer>().sharedMaterial.shader = MyShader;
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

		#region EditorSpawning
		public override void Spawn() 
		{
			GameObject NewSkeleton = new GameObject();
			NewSkeleton.name = Name;
			SpawnedSkeleton = NewSkeleton.AddComponent<SkeletonHandler>();
			SpawnedSkeleton.SetSkeletonData(this);
			Activate();
		}

		public override void DeSpawn() 
		{
			if (SpawnedSkeleton)
			{
				Deactivate();
				SpawnedSkeleton.gameObject.Die();
			}
		}

		public override bool HasSpawned() 
		{
			return (SpawnedSkeleton != null);
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
                        //Debug.LogError("Skeleton Section: \n" + FileUtil.ConvertToSingle(MySkeletonScript));
                        return MySkeletonScript;
                    }
                    break;  // end search here
                }
            }
            //Debug.LogError("Could not find skeleton section:\n" + FileUtil.ConvertToSingle(MyLines));
            return MySkeletonScript;
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
					RoutineManager.Get().StopCoroutine(LoadRoutine);
                }
            }
        }
        #endregion
        
        public void ScanHeirarchyForUpdates()
        {
            if (SpawnedSkeleton != null && IsActivated)
            {
                for (int i = MyBones.Count - 1; i >= 0; i--)
                {
                    if (MyBones[i].MyTransform)
                    {
                        if (MyBones[i].Name != MyBones[i].MyTransform.name)
                        {
                            MyBones[i].Name = MyBones[i].MyTransform.name;
                        }
                        Transform ParentBone = MyBones[i].MyTransform.transform.parent;
                        if (ParentBone == SpawnedSkeleton.transform
                            && MyBones[i].ParentName != "Body")
                        {
                            MyBones[i].ParentName = "Body";
                        }
                        else if (MyBones[i].ParentName != ParentBone.name)
                        {
                            MyBones[i].ParentName = MyBones[i].MyTransform.transform.parent.name;
                        }
                        MyBones[i].SetDefaultPose();
                    }
                    else
                    {
                        // Remove bones if they are deleted
                        MyBones.RemoveAt(i);
                        Debug.Log("Bone has been Deleted at: " + i);
                    }
                }
            }
            else
            {
                Debug.LogError(Name + " has no spawned body.");
            }
        }

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