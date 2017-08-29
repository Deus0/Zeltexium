using UnityEngine;
using System.Collections;

namespace Zeltex.Skeletons
{

    /// <summary>
    /// A skeleton is made up of a collection of bones!
    /// </summary>
    [System.Serializable]
    public class Bone
    {
        public string Name = "";
        public string Tag = "";
        public Transform MyTransform;
        public Transform ParentTransform;
        //public Transform MyLineRender;
        // joint! its a cube at the end of bones!
        public Transform MyJointCube;
        // The lines between joints! using a cube type mesh for this!
        public Transform BodyCube;
        // mesh attached to the transform
        public Transform VoxelMesh;
        public float BoneLength;
        //public float BoneThickness = 0.1f;
        Vector3 MyDifferenceVector;
        Vector3 OriginalJointPosition;
        // Default Pose
        [SerializeField]
        private Vector3 DefaultPosition;
        [SerializeField]
        private Vector3 DefaultScale;
        [SerializeField]
        private Vector3 DefaultRotation;
        [SerializeField, HideInInspector]
        private Vector3 MeshDefaultPosition;
        [SerializeField, HideInInspector]
        private Vector3 MeshDefaultScale;
        [SerializeField, HideInInspector]
        private Vector3 MeshDefaultRotation;

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
