using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using Zeltex.Voxels;
using Zeltex.Items;

namespace Zeltex.Skeletons
{

    /// <summary>
    /// A skeleton is made up of a collection of bones!
    /// Bones also have meshes added on them
    /// </summary>
    [System.Serializable]
    public class Bone : Element
    {
        #region JsonVariables
        [Header("Names")]
        // The link to the bones parent
        [JsonProperty, SerializeField]
        public string ParentName = "";
        // Obsolete, soon to be removed - replace with item name for quickloading
        [JsonProperty, SerializeField]
        public string MeshName = "";
        // Item has polymodel or voxelmodel inside it
        [JsonProperty, SerializeField]
        public Item MyItem = new Item();
        // Tag is used for sorting bones or something
        [JsonProperty, SerializeField]
        public string Tag = "";

        [Header("Defaults")]
        // Default Pose
        [JsonProperty, SerializeField]
		private Vector3 DefaultPosition = Vector3.zero;
        [JsonProperty, SerializeField]
		private Vector3 DefaultScale = new Vector3(1, 1, 1);
        [JsonProperty, SerializeField]
		private Vector3 DefaultRotation = Vector3.zero;
        [JsonProperty, SerializeField]
		private Vector3 MeshDefaultPosition = Vector3.zero;
        [JsonProperty, SerializeField]  
		private Vector3 MeshDefaultScale = new Vector3(0.2f, 0.2f, 0.2f);
        [JsonProperty, SerializeField]
        private Vector3 MeshDefaultRotation = Vector3.zero;
        // Set size limit to the maxiumum size an item can be inside it
        [JsonProperty]
        public Vector3 SizeLimit = new Vector3(0.1f, 0.1f, 0.1f);
        #endregion

        #region Spawned
        [Header("Spawned")]
        [JsonIgnore]
        public Transform MyTransform;
        [JsonIgnore]
        public Transform ParentTransform;
        [SerializeField, JsonIgnore]
        private Bone ParentBone;
        //public Transform MyLineRender;
        // joint! its a cube at the end of bones!
        [JsonIgnore]
        public Transform MyJointCube;
        // The lines between joints! using a cube type mesh for this!
        [JsonIgnore]
        public Transform BodyCube;
        // mesh attached to the transform
        [JsonIgnore]
        public Transform VoxelMesh;
        [JsonIgnore]
        public float BoneLength;
        //public float BoneThickness = 0.1f;
        [JsonIgnore]
        Vector3 MyDifferenceVector;
        [JsonIgnore]
        Vector3 OriginalJointPosition;

        [JsonIgnore]
        private const string BoneTag = "Bone";
        [JsonIgnore]
        private static float JointSize = 0.1f;
        [JsonIgnore]
        private static float BoneSize = 0.1f;
        [JsonIgnore]
        private static Color32 JointColor = Color.green;
        [JsonIgnore]
        private static Color32 BoneColor = Color.cyan;
        [JsonIgnore]
        private static float BoneColorMutation = 15;
        [JsonIgnore]
        private static Vector3 VoxelScale = new Vector3(0.05f, 0.05f, 0.05f);
        [JsonIgnore]
        private Zeltine ActivateCoroutine;
        #endregion

        public override void OnLoad() 
        {
            base.OnLoad();
            if (MyItem != null)
            {
                MyItem.ParentElement = this;
                MyItem.OnLoad();
            }
        }

		public void CheckForChange()
		{
			if (MyTransform)
			{
				// Check parent
				if (ParentName != ParentTransform.name)
				{
					ParentName = ParentTransform.name;
					OnMoved();
				}

				// Check Transform
				if (MyTransform.localPosition != DefaultPosition)
				{
					DefaultPosition = MyTransform.localPosition;
					OnModified();
				}
				if (MyTransform.localEulerAngles != DefaultRotation)
				{
					DefaultRotation = MyTransform.localEulerAngles;
					OnModified();
				}
				if (MyTransform.localScale != DefaultScale)
				{
					DefaultScale = MyTransform.localScale;
					OnModified();
				}

				// Check mesh
				if (VoxelMesh)
				{
					if (VoxelMesh.localPosition != MeshDefaultPosition)
					{
						MeshDefaultPosition = VoxelMesh.localPosition;
						OnModified();
					}
					if (VoxelMesh.localEulerAngles != MeshDefaultRotation)
					{
						MeshDefaultRotation = VoxelMesh.localEulerAngles;
						OnModified();
					}
					if (VoxelMesh.localScale != MeshDefaultScale)
					{
						MeshDefaultScale = VoxelMesh.localScale;
						OnModified();
					}
				}
			}
		}

        public Bone(Skeleton NewSkeleton, string BoneName)
        {
			ParentElement = NewSkeleton;
            Name = BoneName;
            //Activate();
        }

		public VoxelModel GetVoxelModel()
		{
            if (MyItem != null && MyItem.MyModel != null)
			{
                if (MyItem.MyModel.Name != "Empty" && MyItem.MyModel.Name != "")
				{
                    return MyItem.MyModel;
				}
			}
			if (MeshName != "")
			{
				return DataManager.Get().GetElement(DataFolderNames.VoxelModels, MeshName) as VoxelModel;
			}
			return null;
		}

        public PolyModel GetPolyModel() 
        {
            if (MyItem != null && MyItem.MyPolyModel != null)
            {
                if (MyItem.MyPolyModel.Name != "Empty" && MyItem.MyPolyModel.Name != "")
                {
                    return MyItem.MyPolyModel;
                }
            }
            return null;
        }

        public VoxelModel GetSpawnedVoxelModel() 
        {
            if (VoxelMesh)
            {
                // Generate voxel model off world
                Voxels.World MyWorld = VoxelMesh.transform.gameObject.GetComponent<Voxels.World>();
                if (MyWorld)
                {
                    Voxels.VoxelModel WorldModel = new Voxels.VoxelModel();
                    WorldModel.UseWorld(MyWorld);
                    return WorldModel;
                }
            }
            return null;
        }

        public void Rip()
        {
            if (MyTransform)
            {
                Name = MyTransform.name;
                DefaultPosition = MyTransform.localPosition;
                DefaultRotation = MyTransform.localEulerAngles;
                DefaultScale = MyTransform.localScale;
                // refresh parent
                ParentTransform = MyTransform.parent;
                if (ParentTransform)
                {
                    ParentName = ParentTransform.name;
                }
                else
                {
                    ParentName = "";
                }
                if (VoxelMesh)
                {
                    MeshName = VoxelMesh.name;
                    MeshDefaultPosition = VoxelMesh.localPosition;
                    MeshDefaultRotation = VoxelMesh.localEulerAngles;
                    MeshDefaultScale = VoxelMesh.localScale;
                }
                else
                {
                    MeshName = "";  // don't load a mesh
                    MeshDefaultPosition = Vector3.zero;
                    MeshDefaultRotation = Vector3.zero;
                    MeshDefaultScale =  new Vector3(1f, 1f, 1f);
                }
                // or rip a custom mesh if required
            }
        }

        public void Reactivate() 
        {
            Deactivate();
            ActivateSingle();
        }

        /// <summary>
        /// Create the Scene representation
        /// </summary>
        public void ActivateSingle()
        {
            if (ActivateCoroutine != null)
            {
				RoutineManager.Get().StopCoroutine(ActivateCoroutine);
            }
			ActivateCoroutine = RoutineManager.Get().StartCoroutine(ActivateRoutine());
        }

		public void SetMesh(VoxelModel NewModel)
		{
            if (MyItem == null || MyItem.Name == "" || MyItem.Name == "Empty")
            {
                MyItem = new Item();
                MyItem.SetName(NewModel.Name);
            }
            if (MyItem != null)
            {
                MyItem.MyModel = NewModel;
                if (MyItem.MyModel != null)
                {
                    NewModel.ParentElement = this;
                    MyItem.MyModel.OnModified();
                }
            }
		}

		public void SetMeshName(string NewMeshName)
		{
			if (NewMeshName != MeshName)
			{
				MeshName = NewMeshName;
				OnModified();
			}
		}

        public IEnumerator ActivateRoutine()
        {
            if (MyTransform)
            {
                Deactivate();
            }
            // create bone with Name
            //Attach to bone of name 'Parent'
            GameObject NewBoneObject = new GameObject();
            LayerManager.Get().SetLayerSkeleton(NewBoneObject);
            NewBoneObject.name = Name;
            NewBoneObject.tag = BoneTag;
            //NewBone.transform.position = MyCamera.transform.position + MyCamera.transform.forward * 2f;
            //NewBoneObject.transform.position = new Vector3();
            //NewBoneObject.transform.SetParent(FindParentBone(MySkeleton.GetTransform()), false);
            NewBoneObject.transform.localPosition = DefaultPosition;
            NewBoneObject.transform.localEulerAngles = DefaultRotation;
            NewBoneObject.transform.localScale = DefaultScale;

            MyTransform = NewBoneObject.transform;
            ParentTransform = NewBoneObject.transform.parent;

            //CreateJointMesh();
            //CreateBoneMesh();
			VoxelModel Model = GetVoxelModel();
            if (Model != null)
            {
                yield return CreateMeshRoutine(Model); //RoutineManager.Get().StartRoutine
                OnSpawnedMesh();
                if (VoxelMesh)
                {
                    World VoxelWorld = VoxelMesh.GetComponent<World>();
                    if (VoxelWorld)
                    {
                        VoxelWorld.SetColliders((ParentElement as Skeleton).IsMeshColliders);
                        VoxelWorld.SetMeshVisibility(true);
                    }
                }
                else
                {
                    Debug.LogError("Failure Creating Voxel world for bone: " + Name);
                }
            }
            else if (GetPolyModel() != null)
            {
                // Spawn poly model!
                PolyModel MyPolyModel = GetPolyModel();
                yield return CreateMeshRoutine(MyPolyModel);
                OnSpawnedMesh();
            }
        }

        private void OnSpawnedMesh() 
        {
            if (VoxelMesh)
            {
                VoxelMesh.localPosition = MeshDefaultPosition;
                VoxelMesh.localEulerAngles = MeshDefaultRotation;
                VoxelMesh.localScale = MeshDefaultScale;
            }
        }

		public void ShowMesh()
		{
			if (VoxelMesh)
            {
				World VoxelWorld = VoxelMesh.GetComponent<Voxels.World> ();
				if (VoxelWorld) {
					VoxelWorld.SetMeshVisibility (true);
				}
			}
		}

		public void HideMesh()
		{
			if (VoxelMesh)
			{
				Voxels.World VoxelWorld = VoxelMesh.GetComponent<Voxels.World>();
				if (VoxelWorld)
				{
					VoxelWorld.SetMeshVisibility(false);
				}
			}
		}

        /// <summary>
        /// Attaches the bone to the proper parent
        /// </summary>
        public void AttachBoneToParent(bool IsReposition = true)
        {
            FindParentBone();
            if (ParentBone != null)
            {
                if (ParentBone.MyTransform != null)
                {
                    MyTransform.SetParent(ParentBone.MyTransform);
                }
                else
                {
                    MyTransform.SetParent((ParentElement as Skeleton).GetTransform());
                    ParentTransform = MyTransform.parent;
                }
            }
            else
            {
                MyTransform.SetParent((ParentElement as Skeleton).GetTransform());
            }
            if (IsReposition)
            {
                // MyTransform.SetParent(ParentBone.MyTransform, false);
                MyTransform.localPosition = DefaultPosition;
                MyTransform.localEulerAngles = DefaultRotation;
            }
            MyTransform.localScale = DefaultScale;
        }
        private IEnumerator ParentBoneWhenSpawned()
        {
            while (true)
            {
                if (ParentBone.MyTransform != null)
                {
                    MyTransform.SetParent(ParentBone.MyTransform, false);
                    MyTransform.localPosition = DefaultPosition;
                    MyTransform.localEulerAngles = DefaultRotation;
                    MyTransform.localScale = DefaultScale;
                    ParentTransform = MyTransform.parent;
                    yield break;
                }
                yield return null;
            }
        }
        
        private void FindParentBone()
        {
            ParentBone = null; // default
            for (int i = 0; i < (ParentElement as Skeleton).MyBones.Count; i++)
            {
				if ((ParentElement as Skeleton).MyBones[i].Name == ParentName)
                {
					ParentBone = (ParentElement as Skeleton).MyBones[i];
                    break;
                }
            }
        }
        private Transform FindParentBone(Transform MyParent)
        {
            for (int i = 0; i < MyParent.childCount; i++)
            {
                if (MyParent.GetChild(i).name == ParentName)
                {
                    return MyParent.GetChild(i);
                }
                Transform MyChildParent = FindParentBone(MyParent.GetChild(i));
                if (MyChildParent.name == ParentName)
                {
                    return MyChildParent;
                }
            }
            return MyParent;
        }

        /// <summary>
        /// Clear the bone's unity parts
        /// </summary>
        public void Deactivate()
        {
            if (MyTransform)
            {
                MyTransform.gameObject.Die();
            }
        }
        
        public void SetSkeleton(Skeleton NewSkeleton)
        {
			ParentElement = NewSkeleton;
        }

        public void CreateJointMesh()
        {
            if (MyJointCube)
            {
                bool IsJointsColliders = false;
                // Create a joint cube for the bone
                GameObject JointMeshObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                LayerManager.Get().SetLayerSkeleton(JointMeshObject);
                //SetLayerMask(MyJointCube);
                // Destroy collider if no joint shown
                if (!IsJointsColliders && JointMeshObject.GetComponent<BoxCollider>())
                {
                    MonoBehaviourExtension.Kill(JointMeshObject.GetComponent<BoxCollider>());
                }
                JointMeshObject.name = Name + "_Joint ";
                JointMeshObject.tag = BoneTag;
                JointMeshObject.transform.localScale = new Vector3(JointSize, JointSize, JointSize);
                JointMeshObject.transform.SetParent(MyTransform, false);
                MyJointCube = JointMeshObject.transform;
                SetJointColor();
            }
        }

        public void CreateBoneMesh()
        {
			if (BodyCube == null && ParentTransform != (ParentElement as Skeleton).GetTransform())
            {
                GameObject BoneMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
                LayerManager.Get().SetLayerSkeleton(BoneMesh);
                //SetLayerMask(BoneMesh);
                if (BoneMesh.GetComponent<BoxCollider>())
                {
                    MonoBehaviourExtension.Kill(BoneMesh.GetComponent<BoxCollider>());
                }
                BoneMesh.name = Name + "_BoneMesh ";
                BoneMesh.tag = BoneTag;
                BoneMesh.transform.localScale = new Vector3(BoneSize, BoneSize, BoneSize);
                BoneMesh.transform.SetParent(MyTransform, false);
                BodyCube = BoneMesh.transform;
                SetBoneColor();
            }
        }

        /// <summary>
        /// sets the bone colours
        /// </summary>
        public void SetJointColor()
        {
            if (MyJointCube)
            {
                Material JointMaterial = new Material(Shader.Find("Standard"));
                if (JointMaterial == null)
                {
                    MyJointCube.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
                }
                else
                {
                    MyJointCube.GetComponent<MeshRenderer>().sharedMaterial = new Material(JointMaterial);
                }
                MyJointCube.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Mode", 2);
                MyJointCube.GetComponent<MeshRenderer>().sharedMaterial.color = JointColor;
            }
        }

        public void SetBoneColor()
        {
            if (BodyCube)
            {
                Material BoneMaterial = new Material(Shader.Find("Standard"));
                MeshRenderer BodyMeshRenderer = BodyCube.GetComponent<MeshRenderer>();
                BodyMeshRenderer.sharedMaterial = new Material(BoneMaterial);
                BodyMeshRenderer.sharedMaterial.SetFloat("_Mode", 2);
                byte Red = (byte)(BoneColor.r + (int)Random.Range(-BoneColorMutation, BoneColorMutation));
                byte Green = (byte)(BoneColor.g + (int)Random.Range(-BoneColorMutation, BoneColorMutation));
                byte Blue = (byte)(BoneColor.b + (int)Random.Range(-BoneColorMutation, BoneColorMutation));
                BodyMeshRenderer.sharedMaterial.color = new Color32(Red, Green, Blue, BoneColor.a);
            }
        }


        #region Mesh

        public void CreateMesh(string MeshData)
        {
            VoxelModel NewMesh = new VoxelModel();
            NewMesh.UseScript(MeshData);
            UniversalCoroutine.CoroutineManager.StartCoroutine(CreateMeshRoutine(NewMesh));// Zeltex.Util.FileUtil.ConvertToList(MeshData)));
        }

        /// <summary>
        /// Used by skeleton manager
        /// </summary>
        public void CreateMesh(VoxelModel MeshData)
        {
			RoutineManager.Get().StartCoroutine(CreateMeshRoutine(MeshData));// Zeltex.Util.FileUtil.ConvertToList(MeshData)));
        }

        public IEnumerator CreateMeshRoutine(PolyModel MyPolyModel)
        {
            if (MyPolyModel == null)
            {
                //Debug.LogError("MyPolyModel was null.");
                yield break;
            }

            GameObject NewMeshObject = CreateMeshPart();
            PolyModelHandle NewPolyHandle = NewMeshObject.AddComponent<PolyModelHandle>();
            NewPolyHandle.LoadVoxelMesh(MyPolyModel, MyItem.TextureMapIndex);
            yield return null;
        }

        private GameObject CreateMeshPart() 
        {
            if (MyTransform)
            {
                if (VoxelMesh != null)
                {
                    VoxelMesh.gameObject.Die();
                    VoxelMesh = null;
                }
                GameObject NewMeshObject = new GameObject();
                LayerManager.Get().SetLayerSkeleton(NewMeshObject);
                if (MeshName != "")
                {
                    NewMeshObject.name = MeshName;//"VoxelMesh [" + Name + "]";
                }
                else if (MyItem != null)
                {
                    NewMeshObject.name = MyItem.Name;//"VoxelMesh [" + Name + "]";
                }
                NewMeshObject.tag = "BonePart";

                NewMeshObject.transform.SetParent(MyTransform);
                NewMeshObject.transform.position = MyTransform.position;
                NewMeshObject.transform.rotation = MyTransform.rotation;
                NewMeshObject.transform.localScale.Set(1, 1, 1);
                VoxelMesh = NewMeshObject.transform;
                return NewMeshObject;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
		/// Create a mesh in the timer
		/// TODO - Make this pooled skeleton meshes
        /// </summary>
        public IEnumerator CreateMeshRoutine(VoxelModel MeshData, bool IsMeshVisible = false)    // System.Collections.Generic.List<string>
        {
			if (MeshData == null)
			{
				Debug.LogError("MeshData was null.");
				yield break;
			}
			//MeshName = MeshData.Name;
            GameObject NewMeshObject = CreateMeshPart();

            if (NewMeshObject)
            {
                // Add World parts
                World MyWorld = NewMeshObject.GetComponent<World>();
                if (MyWorld == null)
                {
                    MyWorld = NewMeshObject.AddComponent<World>();
                }

                MyWorld.IsChunksCentred = false;
                MyWorld.VoxelScale = VoxelScale;
                MyWorld.SetColliders(false);
                MyWorld.SetConvex(true);
                MyWorld.SetMeshVisibility(IsMeshVisible);
                MyWorld.IsCentreWorld = true;
                MyWorld.IsDropParticles = true;
                yield return MyWorld.RunScriptRoutine(Zeltex.Util.FileUtil.ConvertToList(MeshData.VoxelData));
            }
        }

        /// <summary>
        /// Returns the name of the mesh from the meshes gameobject name
        /// </summary>
        public static string GetMeshName(GameObject MyMeshObject)
        {
            if (MyMeshObject.name.Contains("MeshName"))
            {
                int MeshNameIndex = MyMeshObject.name.IndexOf("MeshName");
                string MyMeshName = MyMeshObject.name.Substring(MeshNameIndex);
                //Debug.LogError("MeshName: " + MyMeshName + " - Length: " + MyMeshName.Length);
                int MeshNameIndex2 = MyMeshName.IndexOf("(") + 1;
                int LengthOfName = MyMeshName.Length - MeshNameIndex2 - 1;
                //Debug.LogError("MeshNameStartIndex: " + MeshNameIndex2 + " - LengthOfName: " + LengthOfName);
                MyMeshName = MyMeshName.Substring(MeshNameIndex2, LengthOfName);
                return MyMeshName;
            }
            return "";
        }
        #endregion

        #region Initialization
        public Bone()
        {

        }

        public Bone(Transform New1, Transform New2)
        {
            Name = New1.name;
            MyTransform = New1;
            ParentTransform = New2;
        }
        #endregion

        #region Getters

        /// <summary>
        /// Each bone has a unique name stored in the transform!
        /// </summary>
        public string GetUniqueName()
        {
            if (MyTransform)
            {
                return MyTransform.name;
            }
            else
            {
                Debug.LogError("Bone missing transform!");
                return "";
            }
        }

        /// <summary>
        /// Has the bone got a mesh attached to it
        /// </summary>
        public bool HasMesh()
        {
            return (VoxelMesh != null);
        }

        public void DestroyAttachedMesh()
        {
            if (HasMesh())
            {
                VoxelMesh.gameObject.Die();
            }
        }
        #endregion
        
        public Vector3 GetDefaultPosition()
        {
            return DefaultPosition;
        }
        public Quaternion GetDefaultRotationQ()
        {
            return Quaternion.Euler(DefaultRotation);
        }
        /// <summary>
        /// Restores the bone positions to default
        /// </summary>
        public void RestoreDefaultPose()
        {
            if (DefaultScale == new Vector3(0,0,0))
            {
                DefaultScale = new Vector3(1, 1, 1);
            }
            MyTransform.localPosition = DefaultPosition;
            MyTransform.localScale = DefaultScale;
            MyTransform.localEulerAngles = DefaultRotation;
            if (VoxelMesh)
            {
                VoxelMesh.transform.localPosition = MeshDefaultPosition;
                VoxelMesh.transform.localScale = MeshDefaultScale;
                VoxelMesh.transform.localEulerAngles = MeshDefaultRotation;
            }
            else
            {
                MeshDefaultPosition = new Vector3();
                MeshDefaultScale = new Vector3(1, 1, 1);
                MeshDefaultRotation = new Vector3();
            }
        }
        /// <summary>
        /// Set default position for bone and mesh
        /// </summary>
        public void SetDefaultPose()
        {
            DefaultPosition = MyTransform.localPosition;
            DefaultScale = MyTransform.localScale;
            DefaultRotation = MyTransform.localEulerAngles;
            if (VoxelMesh)
            {
                MeshDefaultPosition = VoxelMesh.transform.localPosition;
                MeshDefaultScale = VoxelMesh.transform.localScale;
                MeshDefaultRotation = VoxelMesh.transform.localEulerAngles;
            }
            else
            {
                MeshDefaultPosition = new Vector3();
                MeshDefaultScale = new Vector3(1,1,1);
                MeshDefaultRotation = new Vector3();
            }
        }

        public Vector3 GetDifferenceVector()
        {
            if (ParentTransform != null && MyTransform != null)
            {
                MyDifferenceVector = ParentTransform.position - MyTransform.position;
            }
            return MyDifferenceVector;
        }

        public Vector3 GetScaledDifferenceVector()
        {
            Vector3 MyDifferenceVector = ParentTransform.position - MyTransform.position;
            MyDifferenceVector = new Vector3(
                MyDifferenceVector.x * MyTransform.parent.lossyScale.x,
                MyDifferenceVector.y * MyTransform.parent.lossyScale.y,
                MyDifferenceVector.z * MyTransform.parent.lossyScale.z
                );
            return MyDifferenceVector;
        }

        public Quaternion GetBoneRotation()
        {
            Quaternion MyRotation = Quaternion.FromToRotation(Vector3.up, GetDifferenceVector().normalized);
            return MyRotation;
        }

        public Vector3 GetJointPosition()
        {
            return OriginalJointPosition;
        }
        public void SetBodyCubePosition()
        {
            if (MyTransform)
            {
                OriginalJointPosition = MyTransform.localPosition;
            }
        }
        public Vector3 GetMidPoint()
        {
            return MyTransform.position + GetDifferenceVector() / 2f;
        }
        public float GetBoneLength()
        {
            float Distance = Vector3.Distance(MyTransform.position, ParentTransform.position);
            Distance /= MyTransform.lossyScale.y;
            BoneLength = Distance;
            return Distance;
        }

        public Vector3 GetBoneScale(Transform BoneTransform, float BoneThickness)
        {
            return new Vector3(BoneThickness * BoneTransform.lossyScale.x,   // * ParentTransform.localScale.x
                               GetBoneLength(),
                               BoneThickness * BoneTransform.lossyScale.z);  // * ParentTransform.localScale.z
        }

        public bool HasItem() 
        {
            return (SizeLimit != Vector3.zero);
        }
    }
}
/*GetDifferenceVector();
//Debug.DrawLine(MyTransform.position, MyTransform.position + MyTransform.forward, Color.red, 25);
//Debug.DrawLine(ParentTransform.position, ParentTransform.position + ParentTransform.forward, Color.blue, 25);
Vector3 OldLocal = MyTransform.localPosition;
MyTransform.localPosition -= OldLocal;
for (int i = 0; i < MyTransform.childCount; i++)
{
    MyTransform.GetChild(i).localPosition += OldLocal;
}*/
