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
                RefreshCamera();
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

        #region UI
        /*public void UpdateTag()
        {
            if (SelectedBone == null)
                return;
            SelectedBone.Tag = InputTag.text;
        }

        public void UpdateBoneName()
        {
            if (SelectedBone == null)
                return;
            SelectedBone.Name = InputBoneName.text;
        }*/
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

        #region Creation
        /*
        /// <summary>
        /// Create a bone at mouse position
        /// </summary>
        void CreateBoneAtPosition(Vector2 MousePosition)
        {
            Ray MyRay;
            bool DidHit = GetRayInViewer(MousePosition, out MyRay);
            if (DidHit)
            {
                GameObject NewBone = CreateBone();
                if (SelectedBone != null)
                {
                    Vector3 MyLastPosition = SelectedBone.MyTransform.position;
                    NewBone.transform.position = MouseToBonePosition(MousePosition, MyLastPosition);
                }
                else
                {
                    NewBone.transform.position = MouseToBonePosition(MousePosition, 1);
                }
                SetSelectedObject(NewBone);
            }
        }
        /// <summary>
        /// Create a bone
        /// </summary>
        public GameObject CreateBone()
        {
            Transform MyParent;
            if (SelectedBone != null)
            {
                MyParent = (SelectedBone.MyTransform);
            }
            else
            {
                MyParent = (GetSpawn().transform);
            }
            //Debug.LogError("Bone Parent: " + MyParent.name + " - is null: " + (SelectedBone == null));
            Bone NewBone = MySpawnedSkeleton.CreateBone(MyParent);
            MySpawnedSkeleton.MyBones.Add(NewBone);
            return NewBone.MyTransform.gameObject;
        }
        /// <summary>
        /// Destroy a bone!
        /// </summary>
        public void DestroySelectedBone()
        {
            if (SelectedBone != null)
            {
                GetSpawn().GetComponent<Skeleton>().Remove(SelectedBone.MyTransform);
            }
        }*/
        #endregion
    }
}




//public SkeletonBrushMode MyMode;
//public Color32 TintedWorldColor = new Color32(200, 255, 200, 155);
//public Color32 SelectedBoneColor = new Color32(15, 255, 15, 155);
//public float GrowSpeed = 1.01f;
//[SerializeField]
//private Bone SelectedBone = null;
// private bool IsMovingCreatedBone = false;
// [Header("Events")]
//public UnityEvent OnSelectBone;
//[Header("Bone Input")]
//public InputField InputBoneName;
//public InputField InputTag;
//private bool IsIndependentMovement = false; // if true will not move child bones
//private bool IsIndependentScaling = false;  // if true, scaling will be done per bone rather then using parents
// last minute voxel editing
// [Header("Voxel Edit")]
// public bool IsMeshSelectMode;
// public bool IsVoxelEditMode = false;
//public int UpdateType = 22;
// public float UpdateSize = 0;
//public Color32 MyTintingColor = Color.white;
// oruvates
/*if (MyMode == SkeletonBrushMode.Create)
{
    if (Input.GetMouseButtonDown(0))
    {
        if (IsMouseInViewer(Input.mousePosition))
        {
            CreateBoneAtPosition(Input.mousePosition);
            // keep moving until release the mouse!
            IsMovingCreatedBone = true;
        }
    }
}*/

/*public void RescaleBone(int BoneIndex, float ScaleVariance)
{
    if (BoneIndex < 0 || BoneIndex >= MySpawnedSkeleton.MyBones.Count)
        return;
    RescaleTransform(MySpawnedSkeleton.MyBones[BoneIndex].MyTransform, 1f + ScaleVariance);
}*/
//void RescaleTransform(Transform MyTransform, float NewScale)
//{
//}
/// <summary>
/// Switches the mode of the viewer
/// </summary>
/*public void UseInput(Dropdown MyDropdown)
{
    if (MyDropdown.name == "BrushDropdown")
    {
        MyMode = (SkeletonBrushMode)MyDropdown.value;
    }
}*/
/// <summary>
/// Called when mouse dragging in the viewer
/// </summary>
/* protected override void OnDragged(Vector3 MousePosition, Vector3 MousePositionDifference)
 {
     if (SelectedObject != null)
     {
         if (MyMode == SkeletonBrushMode.Move)
         {
             ObjectFollowMouse(MousePosition);
             OnUpdateTransform();
         }
         else if (MyMode == SkeletonBrushMode.Scale)
         {
             ScaleSelected(MousePositionDifference);
             OnUpdateTransform();
         }
         else if (MyMode == SkeletonBrushMode.Rotate)
         {
             ObjectRotateMouse(MousePositionDifference);
             OnUpdateTransform();
         }
     }
 }
 /// <summary>
 /// Scale a transform!
 /// </summary>
 void ScaleSelected(Vector2 MouseDifference)
 {
     float ScaleValue = -MouseDifference.x + MouseDifference.y;
     float MyGrowSpeed = GrowSpeed;
     if (ScaleValue > 0)
     {
         MyGrowSpeed = 1 / GrowSpeed;
     }
     // GetChildren
     Transform ScaledTransform = SelectedBone.MyTransform;
     List<Transform> MyChildren = new List<Transform>();
     if (IsIndependentScaling)
     {
         for (int i = 0; i < ScaledTransform.childCount; i++)
         {
             if (ScaledTransform.GetChild(i).gameObject.tag == "Bone" || ScaledTransform.GetChild(i).gameObject.tag == "BonePart")
             {
                 MyChildren.Add(ScaledTransform.GetChild(i));
             }
         }

     }
     // Scale transform
     ScaledTransform.localScale *= MyGrowSpeed;
     // Scale children by reverse
     if (IsIndependentScaling)
     {
         for (int i = 0; i < MyChildren.Count; i++)
         {
             MyChildren[i].localScale /= MyGrowSpeed;
         }
     }
 }

 /// <summary>
 ///  for joints, only move them not their children bone joints
 /// </summary>
 protected override void OnMoveGameObject(GameObject MovedObject, Vector3 NewPosition, Vector3 DifferencePosition)
 {
     if (IsIndependentMovement)
         for (int i = 0; i < SelectedBone.MyTransform.childCount; i++)
         {
             Transform MyChild = SelectedBone.MyTransform.GetChild(i);
             if (MyChild.gameObject.tag == "Bone")
             {
                 MyChild.position += -DifferencePosition;
             }
         }
 }*/
/*
/// <summary>
/// Removes selected mesh
/// </summary>
public void DestroyMeshOnSelected()
{
    if (SelectedBone != null)
    {
        if (IsMeshSelectMode && SelectedBone.VoxelMesh)
        {
            //Debug.LogError(SelectedBone.Name + " - Destroying mesh [" + SelectedBone.VoxelMesh.name + "]");
            Destroy(SelectedBone.VoxelMesh.gameObject);
        }
        else
        {
            DestroySelectedBone();
        }
    }
}
/// <summary>
/// returns selected Transform
/// </summary>
/// <returns></returns>
/* public Transform GetSelectedBone()
 {
     if (SelectedBone == null)
     {
         return null;
     }
     else
     {
         return SelectedBone.MyTransform;
     }
 }
/// <summary>
/// Sets previous selection colours to normal
/// </summary>
 private void Deselect(GameObject MyObject)
 {
     if (MyObject != null)
     {
         if (MyObject.name.Contains("VoxelMesh"))
         {
             World SelectedWorld = MyObject.GetComponent<World>();
             SetWorldColor(SelectedWorld, Color.white);
         }
         else
         {
             if (MyObject.transform.childCount > 0)
             {
                 MyObject.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = MySpawnedSkeleton.JointColor;
             }
         }
     }
 }

/// <summary>
/// Called from ObjectViewer, when selecting a new object!
/// Split this up into selecting different objects
/// </summary>
 protected override void OnSelectedNewObject(GameObject NewObject)
 {
     SetSelectedObject(NewObject);    // update with mesh and not bone transform
 }
 /// <summary>
 /// Sets the new object
 /// </summary>
 protected override void SetSelectedObject(GameObject MyObject)
 {
     Deselect(SelectedObject);
     if (MyObject.GetComponent<Chunk>())
     {
         MyObject = MyObject.GetComponent<Chunk>().GetWorld().gameObject;
     }
     else if (MyObject.name.Contains("Joint"))
     {
         MyObject.GetComponent<MeshRenderer>().material.color = SelectedBoneColor;
         MyObject = MyObject.transform.parent.gameObject;
     }
     Debug.Log("Selected new object: " + MyObject.name);
     base.SetSelectedObject(MyObject);
     World MyWorld = null;
     if (SelectedObject.GetComponent<World>())
     {
         MyWorld = SelectedObject.GetComponent<World>();
         SetWorldColor(MyWorld, TintedWorldColor);
     }
     SelectedBone = null;
     for (int i = 0; i < MySpawnedSkeleton.MyBones.Count; i++)
     {
         if (SelectedObject.transform == MySpawnedSkeleton.MyBones[i].MyTransform)   // Selecting Bone
         {
             SelectedBone = MySpawnedSkeleton.MyBones[i];
             break;
         }
         else if (SelectedObject.transform == MySpawnedSkeleton.MyBones[i].VoxelMesh) // Selecting mesh
         {
             SelectedBone = MySpawnedSkeleton.MyBones[i];
             break;
         }
     }
     if (SelectedBone != null)
     {
         InputBoneName.interactable = true;
         InputTag.interactable = true;
         InputTag.text = SelectedBone.Tag;
         InputBoneName.text = SelectedBone.Name;
         OnSelectBone.Invoke();
     }
 }

private void SetWorldColor(World MyWorld, Color32 MyColor)
 {
     if (MyWorld != null)
     {
         //Debug.Log("Coloring: " + MyWorld.name);
         if (MyWorld.IsSingleChunk())
         {
             MyWorld.gameObject.GetComponent<MeshRenderer>().material.color = MyColor;
         }
         else
         {
             foreach (Int3 MyKey in MyWorld.MyChunkData.Keys)
             {
                 Chunk MyChunk = MyWorld.MyChunkData[MyKey];
                 MyChunk.GetComponent<MeshRenderer>().material.color = MyColor;
             }
         }
     }
     else
     {
         SelectedObject.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = MyColor;
     }
 }

/// <summary>
/// Called when selecting nothing!
/// </summary>
protected override void DeselectObject()
 {
     if (MyMode != SkeletonBrushMode.Create)
     {
         Deselect(SelectedObject);
         base.DeselectObject();
         SelectedBone = null;
         InputBoneName.interactable = false;
         InputTag.interactable = false;
         //InputTag.interactable = false;
     }
 }*/
