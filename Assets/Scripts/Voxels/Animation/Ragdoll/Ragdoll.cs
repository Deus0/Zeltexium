using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Zeltex.AI;
using Zeltex.Util;
using Zeltex.Voxels;
using Zeltex.Characters;
using Zeltex.Skeletons;

// Need to decouple this script
// to do:
//		record animation curves of the body falling
//			stop recording curve when the body stops moving
//		reverse the curve to make it get back up when its ressurected

// merge with explode script - that use for voxels
// so i can blow up parts of the body itself

// add in function for removing just parts of the body
// an example would be // remove body parts that intersect with a sphere at x location - or that intersect with a raycast

// Ideally on death, activate a post processing effect script
//		and activate particle system
//		activate sounds of dying


// another one would be, convert only the bottom parts of the body, ie feet, then calves etc, until it is just a head

namespace Zeltex.Physics 
{
    /// <summary>
    /// Rag doll is a system of bones but connected using joints instead of transforms.
    /// </summary>
	public class Ragdoll : MonoBehaviour
    {
        public EditorAction ActionRagdoll;
        public EditorAction ActionReverseRagdoll;
        private SkeletonHandler MySkeleton;
        public bool IsBodyPartsItems = false;
        public bool IsApplyJoints = true;
        private float ExplosionForce = 100f;
        private float DownExplosionForce = 20f;
        private static float ExplosionPauseTime = 0.25f;
        private static float ExplosionPower = 24;
        private Characters.Character MyCharacter;

        private void Awake()
        {
            if (transform.parent)
            {
                MyCharacter = transform.parent.GetComponent<Characters.Character>();
            }
        }

        private void Update()
        {
            if (ActionRagdoll.IsTriggered())
            {
                RagDoll();
            }
            if (ActionReverseRagdoll.IsTriggered())
            {
                // Stop Dying and reverse
                if (MyCharacter)
                {
                    MyCharacter.StopDeath();
                }
                ReverseRagdoll();
            }
        }

        private Zeltine RagdollHandle;
        // gets any child body with a mesh
        // keeps its mesh and position/rotation
        // creates a new body in that position with that, and a rigidbody
        public void RagDoll()
        {
            RagdollHandle = RoutineManager.Get().StartCoroutine(RagDollRoutine());
        }

        private Vector3 RagdolledPosition;
        private Quaternion RagdolledRotation;
        private IEnumerator RagDollRoutine()
        {
            RagdolledPosition = MyCharacter.transform.position;
            RagdolledRotation = MyCharacter.transform.rotation;
            CapsuleCollider MyCapsule = transform.parent.GetComponent<CapsuleCollider>();
            Rigidbody MyRigidbody = transform.parent.gameObject.GetComponent<Rigidbody>();
            SkeletonAnimator MyAnimator = gameObject.GetComponent<SkeletonAnimator>();
            MySkeleton = gameObject.GetComponent<SkeletonHandler>();
            UnityEngine.Networking.NetworkTransform MyNetworkTransform = transform.parent.gameObject.GetComponent<UnityEngine.Networking.NetworkTransform>();
            if (MyCapsule)
            {
                MyCapsule.enabled = false;
            }
            if (MyNetworkTransform)
            {
                MyNetworkTransform.enabled = false;
                //Destroy(MyNetworkTransform);
            }
            if (MyRigidbody)
            {
                MyRigidbody.isKinematic = true;
            }
            if (MyAnimator)
            {
                MyAnimator.Stop();
            }

            BasicController MyController = transform.parent.gameObject.GetComponent<BasicController>();
            if (MyController)
            {
                MyController.enabled = false;
            }
            Mover MyMover = transform.parent.gameObject.GetComponent<Mover>();
            if (MyMover)
            {
                MyMover.enabled = false;
            }
            //MyAnimator.Stop(); // the bone lines
            //Debug.Log("Reversing Bone Positions.");
            for (int i = 0; i < MySkeleton.GetBones().Count; i++)
            {
                MySkeleton.GetBones()[i].SetBodyCubePosition();    // reverse transform positions in bone structure
            }
            yield return null;
            //for (int i = MySkeleton.MyBones.Count - 1; i >= 0; i--)
            for (int i = 0; i < MySkeleton.GetBones().Count; i++)
            {
                //Vector3 BeforePosition = MySkeleton.MyBones[i].MyTransform.position;
                RemoveBone(MySkeleton.GetBones()[i], transform);
                /*float TimeStarted = Time.time;
                while (Time.time - TimeStarted <= 3f)
                {
                    yield return null;
                }
                Vector3 AfterPosition = MySkeleton.MyBones[i].MyTransform.position;
                Debug.LogError(MySkeleton.MyBones[i].MyTransform.name + ": " + i + " before : " + BeforePosition.ToString() + " --- " + AfterPosition.ToString());*/
            }
            yield return null;
            if (IsApplyJoints)
            {
                ApplyJoints();
            }
            float TimeBegun = Time.time;
            while (Time.time - TimeBegun <= ExplosionPauseTime)
            {
                yield return null;
            }
            for (int i = 0; i < MySkeleton.GetBones().Count; i++)
            {
                Transform BoneTransform = MySkeleton.GetBones()[i].MyTransform;
                if (BoneTransform)
                {
                    Rigidbody BoneRigidbody = BoneTransform.GetComponent<Rigidbody>();
                    if (BoneRigidbody != null)
                    {
                        BoneRigidbody.isKinematic = false;
                        BoneRigidbody.AddExplosionForce(ExplosionForce, transform.position, ExplosionPower * MySkeleton.GetSkeleton().GetBounds().extents.magnitude);
                        BoneRigidbody.AddForce(DownExplosionForce * -Vector3.up);
                    }
                }
            }
            // finally release kinematics
        }

        /// <summary>
        /// Applies joints between all the bones
        /// </summary>
        private void ApplyJoints()
        {
            for (int i = 0; i < MySkeleton.GetBones().Count; i++)
            {
                Transform ChildPart = MySkeleton.GetBones()[i].MyTransform;
                Transform ParentPart = MySkeleton.GetBones()[i].ParentTransform;
                if (ParentPart != null && ParentPart != transform)
                {
                    HingeJoint MyJoint = ChildPart.gameObject.GetComponent<HingeJoint>();
                    if (MyJoint == null)
                    {
                        MyJoint = ChildPart.gameObject.AddComponent<HingeJoint>();
                    }
                    MyJoint.breakForce = Mathf.Infinity;
                    MyJoint.connectedBody = ParentPart.GetComponent<Rigidbody>();
                    MyJoint.enableCollision = true;
                    MyJoint.useSpring = true;
                    MyJoint.autoConfigureConnectedAnchor = false;
                    for (int j = 0; j < MySkeleton.GetBones().Count; j++)
                    {
                        if (MySkeleton.GetBones()[i].ParentTransform == MySkeleton.GetBones()[j].MyTransform) // find parent bone transform
                        {
                            //MyJoint.anchor = MySkeleton.MyBones[j].MyJointCube.transform.localPosition;
                            //MyJoint.anchor = MySkeleton.MyBones[j].MyTransform.localPosition;
                            // get parent Bone
                            MyJoint.anchor = MySkeleton.GetBones()[j].GetJointPosition();
                            MyJoint.connectedAnchor = MySkeleton.GetBones()[j].GetJointPosition();
                            break;
                        }
                    }
                    Vector3 OldLocal = ChildPart.localPosition;
                    ChildPart.localPosition -= OldLocal;
                    for (int j = 0; j < ChildPart.childCount; j++)
                    {
                        ChildPart.GetChild(j).localPosition += OldLocal;
                    }
                }
                else
                {
                    //ChildPart.GetComponent<Rigidbody>().freezeRotation = true;
                }
            }
        }

        /// <summary>
        /// Detatch the bone from the skeleton heirarchy and attach it to the root bone
        /// </summary>
		private void RemoveBone(Bone MyBone, Transform MyRoot)
        { 
            Transform MyBoneTransform = MyBone.MyTransform;
            if (MyBone != null && MyBoneTransform && MyBone.VoxelMesh)
            {
                Vector3 BeforeScale = new Vector3(0.5f, 0.5f, 0.5f);
                if (MyBone.VoxelMesh != null && MyBone.VoxelMesh.localScale != Vector3.zero)
                {
                    BeforeScale = MyBone.VoxelMesh.localScale;
                }
                MyBoneTransform.transform.SetParent(null);

                if (MyBone.VoxelMesh != null)
                {
                    World MyWorld = MyBone.VoxelMesh.GetComponent<World>();
                    if (MyWorld)
                    {
                        MyWorld.SetConvex(true);
                    }
                }

                Rigidbody MyRigid = MyBoneTransform.gameObject.GetComponent<Rigidbody>();
                if (MyRigid == null)
                {
                    MyRigid = MyBone.MyTransform.gameObject.AddComponent<Rigidbody>();
                    MyRigid.isKinematic = true;
                    MyRigid.useGravity = false;

                    Gravity MyGrav = MyBoneTransform.gameObject.AddComponent<Gravity>();
                    MyGrav.GravityForce = new Vector3(0, -0.5f, 0);
                }

                Items.ItemHandler MyItemInstance = MyBoneTransform.gameObject.GetComponent<Items.ItemHandler>();
                if (MyItemInstance == null)
                {
                    MyItemInstance = MyBone.MyTransform.gameObject.AddComponent<Items.ItemHandler>();
                    MyItemInstance.SetItem(MyBone.MyItem);
                }
            }
        }

        public void AttachBone(Bone MyBone)
        {
            //Debug.LogError("Removing bone: " + MyBone.MyTransform.name);
            if (MyBone != null)
            {
                MyBone.MyTransform.SetParent(MyBone.ParentTransform);

                if (MyBone.VoxelMesh != null)
                {
                    World MyWorld = MyBone.VoxelMesh.GetComponent<World>();
                    if (MyWorld)
                    {
                        MyWorld.SetConvex(true);
                    }
                }
            }

            Rigidbody MyRigid = MyBone.MyTransform.gameObject.GetComponent<Rigidbody>();
            if (MyRigid != null)
            {
                // Remove gravity first as it depends on rigidbody!
                Gravity MyGrav = MyBone.MyTransform.gameObject.GetComponent<Gravity>();
                if (MyGrav)
                {
                    Destroy(MyGrav);
                }
                Destroy(MyRigid);
            }

            Items.ItemHandler MyItemInstance = MyBone.MyTransform.gameObject.GetComponent<Items.ItemHandler>();
            if (MyItemInstance != null)
            {
                Destroy(MyItemInstance);
            }
        }

        public void ReverseRagdoll(float ReverseTime = 5)
        {
            RagdollHandle = RoutineManager.Get().StartCoroutine(RagdollHandle, ReverseRagdollRoutine(ReverseTime));
        }
        
        private IEnumerator ReverseRagdollRoutine(float ReverseTime)
        {
            if (MySkeleton)
            {
                CapsuleCollider MyCapsule = transform.parent.GetComponent<CapsuleCollider>();
                if (MyCapsule)
                {
                    MyCapsule.enabled = true;
                }
                Rigidbody MyRigidbody = transform.parent.gameObject.GetComponent<Rigidbody>();
                if (MyRigidbody)
                {
                    MyRigidbody.isKinematic = false;
                }
                UnityEngine.Networking.NetworkTransform MyNetworkTransform = transform.parent.gameObject.GetComponent<UnityEngine.Networking.NetworkTransform>();
                if (MyNetworkTransform)
                {
                    MyNetworkTransform.enabled = true;
                }
                SkeletonAnimator MyAnimator = gameObject.GetComponent<SkeletonAnimator>();
                if (MyAnimator != null)
                {
                    MyAnimator.enabled = true;
                }
                for (int i = 0; i < MySkeleton.GetBones().Count; i++)
                {
                    AttachBone(MySkeleton.GetBones()[i]);
                }
                MySkeleton.GetSkeleton().RestoreDefaultPose(ReverseTime);
                Vector3 StartPosition = MyCharacter.transform.position;
                Quaternion StartRotation = MyCharacter.transform.rotation;
                float TimeStarted = Time.time;
                float LerpTime = 0;
                while (Time.time - TimeStarted <= ReverseTime)
                {
                    LerpTime = ((Time.time - TimeStarted) / ReverseTime);
                    MyCharacter.transform.position = Vector3.Lerp(StartPosition, RagdolledPosition, LerpTime);
                    MyCharacter.transform.rotation = Quaternion.Lerp(StartRotation, RagdolledRotation, LerpTime);
                    yield return null;
                }

                BasicController MyController = transform.parent.gameObject.GetComponent<BasicController>();
                if (MyController)
                {
                    MyController.RefreshRigidbody();
                    MyController.enabled = true;
                }
                Mover MyMover = transform.parent.gameObject.GetComponent<Mover>();
                if (MyMover)
                {
                    MyMover.RefreshRigidbody();
                    MyMover.enabled = true;
                }
            }
        }
    }
    /*public void ConnectParts(GameObject ParentPart, GameObject ChildPart)
    {
        if (ParentPart.GetComponent<Rigidbody>())
        {   // FixedJoint
            HingeJoint MyJoint = ChildPart.GetComponent<HingeJoint> ();
            if (MyJoint == null)
                MyJoint = ChildPart.AddComponent<HingeJoint>();
           // MyJoint.enableProjection = true;
            MyJoint.breakForce = Mathf.Infinity;
            MyJoint.connectedBody = ParentPart.GetComponent<Rigidbody> ();
        }
    }*/
    /*public static class ConfigurableJointExtensions
    {
        /// <summary>
        /// Sets a joint's targetRotation to match a given local rotation.
        /// The joint transform's local rotation must be cached on Start and passed into this method.
        /// </summary>
        public static void SetTargetRotationLocal(this ConfigurableJoint joint, Quaternion targetLocalRotation, Quaternion startLocalRotation)
        {
            if (joint.configuredInWorldSpace)
            {
                Debug.LogError("SetTargetRotationLocal should not be used with joints that are configured in world space. For world space joints, use SetTargetRotation.", joint);
            }
            SetTargetRotationInternal(joint, targetLocalRotation, startLocalRotation, Space.Self);
        }

        /// <summary>
        /// Sets a joint's targetRotation to match a given world rotation.
        /// The joint transform's world rotation must be cached on Start and passed into this method.
        /// </summary>
        public static void SetTargetRotation(this ConfigurableJoint joint, Quaternion targetWorldRotation, Quaternion startWorldRotation)
        {
            if (!joint.configuredInWorldSpace)
            {
                Debug.LogError("SetTargetRotation must be used with joints that are configured in world space. For local space joints, use SetTargetRotationLocal.", joint);
            }
            SetTargetRotationInternal(joint, targetWorldRotation, startWorldRotation, Space.World);
        }

        static void SetTargetRotationInternal(this ConfigurableJoint joint, Quaternion targetRotation, Quaternion startRotation, Space space)
        {
            // Calculate the rotation expressed by the joint's axis and secondary axis
            var right = joint.axis;
            var forward = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
            var up = Vector3.Cross(forward, right).normalized;
            Quaternion worldToJointSpace = Quaternion.LookRotation(forward, up);

            // Transform into world space
            Quaternion resultRotation = Quaternion.Inverse(worldToJointSpace);

            // Counter-rotate and apply the new local rotation.
            // Joint space is the inverse of world space, so we need to invert our value
            if (space == Space.World)
            {
                resultRotation *= startRotation * Quaternion.Inverse(targetRotation);
            }
            else {
                resultRotation *= Quaternion.Inverse(targetRotation) * startRotation;
            }

            // Transform back into joint space
            resultRotation *= worldToJointSpace;

            // Set target rotation to our newly calculated rotation
            joint.targetRotation = resultRotation;
        }
    }*/
}
//public bool IsDebugMode = false;
//public KeyCode MyExplodeKey;
//public KeyCode RagdollKey;
//public KeyCode MyReviveKey;

//public bool IsAddPartColliders = true;
//public bool IsAttachMainCamera = false;
//[Tooltip("Used for character interaction")]
//public List<MyEvent> OnInteract;
//[Tooltip("When the body is converted into a ragdoll")]
//public UnityEvent OnRagdoll;
//[Tooltip("When the body is converted into a ragdoll")]
//public UnityEvent OnRevive;
//[Tooltip("If a Camera is attached, reattach it to the ragdoll")]
//public UnityEvent OnReattachCamera;
//[Tooltip("How long is the paused after death before falling")]
//public float TimeFrozen = 1f;
//[Tooltip("Artificial Gravity after death")]
//public Vector3 GravityForce = new Vector3 (0, -2, 0);
//public float ReverseTimeDilation = 0.2f;
//public bool IsTesting = false;
//private float TimeSpawned = 0f;

//GameObject RootSpawn;	// spawned new body main
//protected List<GameObject> MySpawnedBodyParts = new List<GameObject> ();
//public Vector3 ForceOnExplosion = new Vector3 ();
//public bool IsForce = false;
//public float ForceStrength = 10f;

//public bool CanTimeBeReversed = false;
// public Vector3 ForceDirection;

/*public void RemoveBodyPart(GameObject MyBodyPart) 
{
    if (MyBodyPart != null)
    for (int i = 0; i < MySpawnedBodyParts.Count; i++) {
        if (MySpawnedBodyParts[i] == MyBodyPart) 
        {
            if (IsAttachMainCamera) 
            {
                if (MyBodyPart.name.Contains("Head")) 
                {
                    if (MyBodyPart.transform.childCount > 0) 
                    {
                        Debug.LogError("Setting up camera");
                        Camera.main.gameObject.transform.SetParent(MyBodyPart.transform);
                    }
                }
            }
            MySpawnedBodyParts.RemoveAt(i);
            DestroyImmediate (MyBodyPart);
        } else {
            Destroy (MyBodyPart);
        }
    }
}*/

/*void Start() 
{
    GatherBodyParts ();
    for (int i = 0; i < MyBodyParts.Count; i++)
    {
        if (IsAddPartColliders && MyBodyParts [i].GetComponent<MeshCollider> () == null)
        {
            MeshCollider MyBoxCollider = MyBodyParts [i].AddComponent<MeshCollider> ();
            //MyBoxCollider.isTrigger = true;
            MyBoxCollider.convex = true;
        }
        BodyPart MyBodyPart = MyBodyParts [i].GetComponent<BodyPart> ();
        if (MyBodyPart == null)
            MyBodyPart = MyBodyParts [i].AddComponent<BodyPart> ();
        MyBodyPart.MyParent = gameObject;
        MyBodyPart.RagdollBrain = this;
    }
    RootSpawn = gameObject;
}*/

// Update is called once per frame
/*void Update()
{
    AwakenBody();
    if (IsDebugMode)
    {
        if (Input.GetKeyDown(MyReviveKey))
        {
            //ReverseDeath();
        }
        else if (Input.GetKeyDown(MyExplodeKey))
        {
            Explode();
        } else if (Input.GetKeyDown(RagdollKey))
        {
            RagDoll();
        }
    }
}*/

// Toggles the attachtness of a body part
/*public void ActivateBodyPart(GameObject MyBodyPart)
{
    BodyPart MyData = MyBodyPart.GetComponent<BodyPart> ();
    if (MyData.IsRigidBody) {
        ReattachBodyPart (MyBodyPart);
    } else {
        DetatchBodyPart(MyBodyPart);
    }
}*/

/*public void DetatchBodyPart(GameObject MyBodyPart)
{
    //MySpawnedBodyParts.Clear ();
    List<GameObject> DetatchedBodyParts = new List<GameObject> ();
    if (MyBodyPart.activeSelf)
    {
            //DetatchedBodyParts.Add (FirstPart);	// should be a 
        List<GameObject> MyList = FindChildren (MyBodyPart);
            //Debug.LogError ("But Body part has: " + MyBodyPart.transform.childCount + " Children!");
        for (int j = 0; j < MyList.Count; j++)
        {
            GameObject NewPart = CreateNewBodyPart (MyList [j]);
            if (NewPart != null) 
            {
                DetatchedBodyParts.Add (NewPart); 
            }
        }
        //Debug.LogError ("Detatched " + MyList.Count + " children!");
        TimeSpawned = Time.time;
        ConnectUpBodyParts (DetatchedBodyParts);
    }
}*/

/*public void ReverseDeath() 
{
    for (int i = 0; i < MySpawnedBodyParts.Count; i++) {
        ReattachBodyPart(MySpawnedBodyParts[i]);
    }
}*/
// need to check
/*public void ReattachBodyPart(GameObject MyBodyPart) 
{
    BodyPart BodyPartComponent = MyBodyPart.GetComponent<BodyPart>();
    if (CanTimeBeReversed) {
        ReverseMovement MyReverseMovement = MyBodyPart.GetComponent<ReverseMovement> ();
        //Debug.LogError("Reattaching BodyParts v1 ");
        if (MyReverseMovement) {
            MyReverseMovement.Reverse ();
            //Debug.LogError("Reattaching BodyParts");
            MyReverseMovement.OnEndReverse.AddListener (// this ain't working atm! debug more!
            delegate {
                //Debug.LogError("Ending Reverse Movement: " + BodyPartComponent.name);
                if (BodyPartComponent)
                if (BodyPartComponent.OriginalBodyPart)
                    BodyPartComponent.OriginalBodyPart.SetActive (true);
                //if (MyBodyPart.transform.childCount > 0)
                //	Debug.LogError("Wut it contains child: " + MyBodyPart.transform.childCount);
                RemoveBodyPart (MyBodyPart);
                if (MySpawnedBodyParts.Count == 0) {
                    OnRevive.Invoke ();
                }
            });
        }
    } else {	// use general lerping to get bone back on

    }
}*/

// this happens on all parts, need it to just happen on some!
/*public void AwakenBody() 
{
    for (int i = MySpawnedBodyParts.Count-1; i >= 0; i--)
    { 
        if (MySpawnedBodyParts[i] != null) {
        if (MySpawnedBodyParts [i].GetComponent<BodyPart> ().TimeSpawned != 0)
        if (Time.time - MySpawnedBodyParts [i].GetComponent<BodyPart> ().TimeSpawned >= TimeFrozen)
                {
            if (MySpawnedBodyParts [i] != null) 
            if (MySpawnedBodyParts [i].GetComponent<Rigidbody> ()) {
                MySpawnedBodyParts [i].GetComponent<Rigidbody> ().isKinematic = false;
                if (IsForce)
                            {
                    MySpawnedBodyParts [i].GetComponent<Rigidbody> ().velocity = ForceOnExplosion;	// reset velocity
                                ForceOnExplosion = ForceDirection * ForceStrength;
                    }
                    if (CanTimeBeReversed) {
                        ReverseMovement MyReverseMovement = MySpawnedBodyParts [i].GetComponent<ReverseMovement> ();
                        if (MyReverseMovement == null) 
                        { 
                                MyReverseMovement =MySpawnedBodyParts [i].AddComponent<ReverseMovement> ();
                        }
                        MyReverseMovement.ReverseTimeDilation = ReverseTimeDilation;
                    }
            }
            MySpawnedBodyParts [i].GetComponent<BodyPart> ().TimeSpawned = 0;
        }
        } else {
            MySpawnedBodyParts.RemoveAt (i);
        }
    }
}*/
/*void AutoReverse(ReverseMovement MyReverseMovement)
{
    if (CanTimeBeReversed) {
        float AutoReverseTime = -1;
        if (AutoReverseTime == -1) {
            AutoReverseTime = MyReverseMovement.MutateReverseTime ();
        } else {
            MyReverseMovement.SetAutoReverseTime (AutoReverseTime);
        }
    }
}*/
//StartCoroutine(SecondPart(RagdollType));
//Skeleton MySkeleton = gameObject.GetComponent<Skeleton>();
/* Transform MyRootBone = MySkeleton.transform.GetChild(0);

 for (int i = 0; i < MySkeleton.MyBones.Count; i++)
 {
     MySkeleton.MyBones[i].SetBodyCubePosition();    // reverse transform positions in bone structure
 }
 for (int i = 0; i < MySkeleton.MyBones.Count; i++)
 {
     Transform ChildPart = MySkeleton.MyBones[i].MyTransform;
     Transform ParentPart = MySkeleton.MyBones[i].ParentTransform;
     if (ChildPart != MyRootBone)
     {
         HingeJoint MyJoint = ChildPart.gameObject.AddComponent<HingeJoint>();
         MyJoint.breakForce = Mathf.Infinity;
         MyJoint.connectedBody = ParentPart.GetComponent<Rigidbody>();
         MyJoint.enableCollision = true;
         MyJoint.useSpring = true;
     }
 }

 if (OnRagdoll != null)
 {
     OnRagdoll.Invoke();
 }
 TimeSpawned = Time.time;*/
//IEnumerator SecondPart(int RagdollType)
//{
//   yield return new WaitForSeconds(0);
//GetComponent<BodyColours>().OnDeath();
//}

/*public void ConnectUpBodyParts()
{
    ConnectUpBodyParts (MySpawnedBodyParts, true);
}

public void ConnectUpBodyParts(List<GameObject> NewlyDetatchedParts)
{
    ConnectUpBodyParts (NewlyDetatchedParts, true);
}*/

/*public void ConnectUpBodyParts(List<GameObject> NewlyDetatchedParts, bool IsConnectAll)
{
    //Debug.LogError ("Connect parts with joints: " + NewlyDetatchedParts.Count);
    for (int i = 0; i < NewlyDetatchedParts.Count; i++)
    {
        int IsConnect = Random.Range (1,100);
        if (IsConnectAll || IsConnect < 66) 
        {
            if (MyParentIndexes[i] != -1) 
            {
                GameObject MyParent = GetParentBodyPart(i, NewlyDetatchedParts);
                if (MyParent != null) 
                {
                    ConnectParts(MyParent, MySpawnedBodyParts[i]);
                }
            }
        }
    }
}*/
/*MyBone.localScale = NewScale;
MyBone.transform.position = NewPosition;
MyBone.transform.rotation = NewRotation;*/
/*Vector3 NewPosition = MyBone.position;
Vector3 NewScale = MyBone.localScale;
Quaternion NewRotation = MyBone.rotation;*/
//MySpawnedBodyParts.Add (NewBodyPart);
// extra data
/*BodyPart MyBodyPart = NewBodyPart.AddComponent<BodyPart>();
MyBodyPart.MyParent = OldBodyPart.transform.gameObject;
MyBodyPart.RagdollBrain = this;
MyBodyPart.OriginalBodyPart = OldBodyPart;
MyBodyPart.TimeSpawned = Time.time;
MyBodyPart.IsRigidBody = true;*/

// transforms
/*for (int i = 0; i < OldBodyPart.transform.childCount; i++)
{
    if (OldBodyPart.transform.GetChild(i).gameObject.tag == "BonePart")
    {
        Transform MyChild = OldBodyPart.transform.GetChild(i).transform;
        if (MyChild.GetComponent<MeshRenderer>() && MyChild.GetComponent<MeshCollider>() == null)
        {
            MeshCollider MyMeshCollider = MyChild.gameObject.AddComponent<MeshCollider>();
            if (MyMeshCollider)
            {
                MyMeshCollider.sharedMesh = MyChild.GetComponent<MeshFilter>().sharedMesh;
                MyMeshCollider.convex = true;
            }
        }*/
/* if (MyChild.name.Contains("MeshName"))
 {
     //if (IsBodyPartsItems)
     {
         Zeltex.Items.ItemHandler MyItemHandler = MyChild.gameObject.AddComponent<Zeltex.Items.ItemHandler>();
         Item NewItem = new Zeltex.Items.Item();
         NewItem.SetQuantity(1);
         MyItemHandler.SetItem(NewItem);
     }
 }*/
//}
//}