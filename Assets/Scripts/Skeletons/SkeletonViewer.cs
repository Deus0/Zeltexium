using UnityEngine;
using System.Collections;
using Zeltex.Skeletons;

namespace Zeltex.Guis
{
    /// <summary>
    /// The painting mode for skeleton tools
    /// </summary>
   /* public enum SkeletonBrushMode
    {
        Select,
        Move,
        Scale,
        Rotate,
        Create
    }*/
    // upon clicking it - will find position in image - through camera - ray cast bone position
    // when selecting new bone position - will show timeline points for that bone - using custom animator
    /// <summary>
    /// Skeleton viewing - extending object viewer
    ///      - Bone Collecting
    ///      - Creating Bone LineRenders and Joint Cubes
    ///      - Resizing of Bones
    ///      - Moving of joints
    ///      - Debugging bones
    ///      - Creating new bones
    /// Controls:
    ///     - Control click to pan
    ///     - Mouse wheel to zoom
    ///     - Click to add new bone
    /// </summary>
    public class SkeletonViewer : ObjectViewer
    {
        #region Variables
        [Header("SkeletonViewer")]
        public SkeletonHandler MySpawnedSkeleton;
        #endregion

        #region ZelGui
        /// <summary>
        /// Called by Gui's ZelGui OnBegin
        /// </summary>
        public override void OnBegin()
        {
            base.OnBegin();
            MySpawnedSkeleton = GetSpawn().GetComponent<SkeletonHandler>();
            //StopCoroutine(OnLoad());
            //StartCoroutine(OnLoad());
        }
        private IEnumerator OnLoad()
        {
            if (MySpawnedSkeleton.GetSkeleton() != null)
            {
                while (MySpawnedSkeleton.GetSkeleton().IsLoadingSkeleton())
                {
                    yield return null;
                }
                //RefreshCamera();
            }
            //ObjectViewer.SetLayerRecursive(MySpawnedSkeleton.gameObject, ViewerLayer);
        }

        public void RefreshCamera()
        {
            // now zoom in properly on camera
            Bounds SkeletonBounds = MySpawnedSkeleton.GetSkeleton().GetEditorBounds();
            Vector3 NewPosition = new Vector3(
                0,
                0,
                GetDistanceToCamera(MyCamera, SkeletonBounds) * 1.5f);  //GetSpawn().transform.lossyScale.z * 
            MyCamera.transform.position = MySpawnedSkeleton.GetSkeleton().GetTransform().TransformPoint(SkeletonBounds.center + NewPosition);
            MyCamera.transform.LookAt(MySpawnedSkeleton.GetSkeleton().GetTransform());
        }

        public float GetDistanceToCamera(Camera MyCamera, Bounds MyBounds)
        {
            float CameraDistance = 0;
            if (MyBounds.extents.x > MyBounds.extents.y)
            {
                CameraDistance = (MyBounds.size.x) / (float)Mathf.Tan(MyCamera.fieldOfView / 2f);
            }
            else
            {
                CameraDistance = (MyBounds.size.y) / (float)Mathf.Tan(MyCamera.fieldOfView / 2f);
            }
            return CameraDistance;
        }

        /// <summary>
        /// Called by Gui's ZelGui OnEnd
        /// </summary>
        public override void OnEnd()
        {
            Clear();
        }
        #endregion

        #region Utility
        /// <summary>
        /// Utility function to check if 2 colours are equal
        /// </summary>
        public static bool AreColorsEqual(Color MyColor1, Color MyColor2)
        {
            return (MyColor1.r == MyColor2.r
                    && MyColor1.g == MyColor2.g
                    && MyColor1.b == MyColor2.b
                    && MyColor1.a == MyColor2.a);
        }
        #endregion
    }
}