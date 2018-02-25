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
            Zanimator MyAnimator = gameObject.GetComponent<Zanimator>();
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
            if (MyBone != null)
            {
                MyBone.AttachBoneToParent(false);

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
                    MyGrav.Die();
                }
                MyRigid.Die();
            }

            Items.ItemHandler MyItemInstance = MyBone.MyTransform.gameObject.GetComponent<Items.ItemHandler>();
            if (MyItemInstance != null)
            {
                MyItemInstance.Die();
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
                Zanimator MyAnimator = gameObject.GetComponent<Zanimator>();
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
}