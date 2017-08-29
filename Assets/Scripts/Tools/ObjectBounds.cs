using UnityEngine;
using System.Collections;

namespace Zeltex.Util
{
    //[ExecuteInEditMode]
    public class ObjectBounds : MonoBehaviour
    {
        [Header("Debug")]
        public bool DebugBounds = true;
        public bool IsGetBounds;
        public Bounds MyBounds;

        void Update()
        {
            if (IsGetBounds)
            {
                IsGetBounds = false;
                //MyMeshFilter = gameObject.GetComponent<MeshFilter>();
                //OriginalMesh = MyMeshFilter.sharedMesh;
                //MyBounds = OriginalMesh.bounds;
                MyBounds = new Bounds();
            }
        }

        void OnDrawGizmos()
        {
            if (DebugBounds)
            {
                Vector3 Position = transform.TransformPoint(MyBounds.center);
                Vector3 CubeSize = (new Vector3(MyBounds.size.x * transform.lossyScale.x,
                    MyBounds.size.y * transform.lossyScale.y,
                    MyBounds.size.z * transform.lossyScale.z));//transform.TransformDirection(MyBounds.size);
                Gizmos.color = Color.white;
                GizmoUtil.DrawCube(Position, CubeSize, transform.rotation);
            }
        }
    }
}