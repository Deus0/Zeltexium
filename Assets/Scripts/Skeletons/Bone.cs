using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

namespace Zeltex.Skeletons
{

    /// <summary>
    /// A skeleton is made up of a collection of bones!
    /// </summary>
    [System.Serializable]
    public class Bone
    {
        [SerializeField, HideInInspector]
        private Skeleton MySkeleton;
        [Header("Names")]
        [JsonProperty, SerializeField]
        public string Name = "";
        [JsonProperty, SerializeField]
        public string ParentName = "";
        [JsonProperty, SerializeField]
        public string Tag = "";
        [Header("Defaults")]
        // Default Pose
        [JsonProperty, SerializeField]
        private Vector3 DefaultPosition;
        [JsonProperty, SerializeField]
        private Vector3 DefaultScale;
        [JsonProperty, SerializeField]
        private Vector3 DefaultRotation;
        [JsonProperty, SerializeField]
        private Vector3 MeshDefaultPosition;
        [JsonProperty, SerializeField]
        private Vector3 MeshDefaultScale;
        [JsonProperty, SerializeField]
        private Vector3 MeshDefaultRotation;

        [JsonIgnore]
        public Transform MyTransform;
        [JsonIgnore]
        public Transform ParentTransform;
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

        public void Rip()
        {
            if (MyTransform)
            {
                Name = MyTransform.name;
                if (ParentTransform)
                {
                    ParentName = ParentTransform.name;
                }
                else
                {
                    ParentName = "";
                }
            }
        }

        /// <summary>
        /// Create the Scene representation
        /// </summary>
        public void Activate()
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
            NewBoneObject.transform.SetParent(FindParentBone(MySkeleton.GetTransform()), false);
            NewBoneObject.transform.localPosition = DefaultPosition;
            NewBoneObject.transform.localEulerAngles = DefaultRotation;
            NewBoneObject.transform.localScale = DefaultScale;

            //Bone NewBone = CreateBoneData(BoneParent, NewBoneObject.transform, MyBones.Count);
            MyTransform = NewBoneObject.transform;
            ParentTransform = NewBoneObject.transform.parent;
            //NewBoneObject.transform.localScale = new Vector3(1, 1, 1);
            //NewBone.Name = GenerateUniqiueBoneName();
            //SetLayerMask(NewBoneObject);
            CreateJointMesh();
            CreateBoneMesh();
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
            MySkeleton = NewSkeleton;
        }
        public Bone(Skeleton NewSkeleton, string BoneName)
        {
            MySkeleton = NewSkeleton;
            Name = BoneName;
            Activate();
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
            if (BodyCube == null && ParentTransform != MySkeleton.GetTransform())
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
                    MyJointCube.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
                }
                else
                {
                    MyJointCube.GetComponent<MeshRenderer>().material = new Material(JointMaterial);
                }
                MyJointCube.GetComponent<MeshRenderer>().material.SetFloat("_Mode", 2);
                MyJointCube.GetComponent<MeshRenderer>().material.color = JointColor;
            }
        }

        public void SetBoneColor()
        {
            if (BodyCube)
            {
                Material BoneMaterial = new Material(Shader.Find("Standard"));
                BodyCube.GetComponent<MeshRenderer>().material = new Material(BoneMaterial);
                BodyCube.GetComponent<MeshRenderer>().material.SetFloat("_Mode", 2);
                byte Red = (byte)(BoneColor.r + (int)Random.Range(-BoneColorMutation, BoneColorMutation));
                byte Green = (byte)(BoneColor.g + (int)Random.Range(-BoneColorMutation, BoneColorMutation));
                byte Blue = (byte)(BoneColor.b + (int)Random.Range(-BoneColorMutation, BoneColorMutation));
                BodyCube.GetComponent<MeshRenderer>().material.color = new Color32(Red, Green, Blue, BoneColor.a);
            }
        }


        #region Mesh

        /// <summary>
        /// Used by skeleton manager
        /// </summary>
        public void CreateMesh(string MeshData)
        {
            UniversalCoroutine.CoroutineManager.StartCoroutine(CreateMeshRoutine(Zeltex.Util.FileUtil.ConvertToList(MeshData)));
        }

        /// <summary>
        /// Create a mesh in the timer
        /// </summary>
        public IEnumerator CreateMeshRoutine(System.Collections.Generic.List<string> MeshData, bool IsMeshVisible = false)
        {
            if (VoxelMesh != null)
            {
                MonoBehaviourExtension.Kill(VoxelMesh.gameObject);
                VoxelMesh = null;
            }
            // TODO - Make this pooled skeleton meshes
            GameObject NewMeshObject = new GameObject();
            LayerManager.Get().SetLayerSkeleton(NewMeshObject);
            NewMeshObject.name = "VoxelMesh [" + Name + "]";
            NewMeshObject.tag = "BonePart";
            // World Stuff
            Voxels.World MyWorld = NewMeshObject.GetComponent<Voxels.World>();
            if (MyWorld == null)
            {
                MyWorld = NewMeshObject.AddComponent<Voxels.World>();
                MyWorld.IsChunksCentred = false;
            }
            Voxels.WorldUpdater WorldUpdater = Voxels.WorldUpdater.Get();
            if (WorldUpdater)
            {
                MyWorld.MyUpdater = WorldUpdater;
            }
            else
            {
                Debug.LogError("WorldUpdater object not found.");
            }

            if (Voxels.VoxelManager.Get())
            {
                MyWorld.MyDataBase = Voxels.VoxelManager.Get();
                MyWorld.MyMaterials = Voxels.VoxelManager.Get().MyMaterials;
            }
            else
            {
                Debug.LogError("Voxel Manager is null");
            }

            MyWorld.SetColliders(false);
            MyWorld.SetConvex(MySkeleton.IsConvexMeshes);
            MyWorld.SetMeshVisibility(IsMeshVisible);

            MyWorld.VoxelScale = VoxelScale;
            MyWorld.IsCentreWorld = true;
            MyWorld.IsDropParticles = true;

            NewMeshObject.transform.SetParent(MyTransform);
            NewMeshObject.transform.position = MyTransform.position;
            NewMeshObject.transform.rotation = MyTransform.rotation;
            NewMeshObject.transform.localScale.Set(1, 1, 1);
            VoxelMesh = NewMeshObject.transform;

            //yield return null;
            yield return UniversalCoroutine.CoroutineManager.StartCoroutine(MyWorld.RunScriptRoutine(MeshData));
            Debug.LogError("-----------------------------------");
            //Debug.LogError("Waited : " + WaitCount + " for mesh to load.");
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
                GameObject.Destroy(VoxelMesh.gameObject);
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
            OriginalJointPosition = MyTransform.localPosition;
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
