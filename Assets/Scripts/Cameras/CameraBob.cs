using System;
using UnityEngine;
using UnityStandardAssets.Utility;

namespace Zeltex
{
    public class CameraBob : MonoBehaviour
    {
        public CurveControlledBob motionBob = new CurveControlledBob();
        public LerpControlledBob jumpAndLandingBob = new LerpControlledBob();
		private Mover MyMover;
        public float StrideInterval;
        [Range(0f, 1f)] public float RunningStrideLengthen;

       // private CameraRefocus m_CameraRefocus;
        private bool m_PreviouslyGrounded;
        private Transform MyCamera;
        private Transform CameraBone;

        private float FollowVelocity = 0;
        private Vector3 FollowPosition;
        private Quaternion FollowRotation;
        public float FollowVelocityLerpSpeed = 3;
        public float LerpSpeed = 17; 
        public float RunLerpSpeed = 17;
        public float RotateLerpSpeed = 14;

        void Update()
        {
            if (!GameManager.IsCameraFixedUpdate)
            {
                UpdateBob(Time.deltaTime);
            }
        }

        void FixedUpdate()
        {
            if (GameManager.IsCameraFixedUpdate)
            {
                UpdateBob(Time.fixedDeltaTime);
            }
        }

		public void Initialise(Mover NewMover) 
		{
			MyMover = NewMover;
            CameraBone = NewMover.GetCameraBone();
            MyCamera = CameraBone.GetChild(NewMover.GetCameraBone().transform.childCount - 1);;
            motionBob.Setup(MyCamera, StrideInterval);
            //CameraBone.transform.SetParent(null);
            FollowPosition = CameraBone.transform.position;
            FollowRotation = CameraBone.transform.rotation;
            MyCamera.transform.SetParent(null);
            FollowVelocity = MyMover.Velocity.magnitude;
		}

        private void UpdateBob(float LerpTime) 
        {
            if (MyMover)
            {
                bool IsMovingOnGround = MyMover.Velocity.magnitude > 0 && MyMover.Grounded;
                if (IsMovingOnGround || !MyMover.Grounded)
                {
                    LerpTime *= RunLerpSpeed;
                }
                else
                {
                    LerpTime *= LerpSpeed;
                }
                FollowVelocity = Mathf.Lerp(FollowVelocity, MyMover.Velocity.magnitude, Time.deltaTime * FollowVelocityLerpSpeed);
                FollowPosition = Vector3.Lerp(FollowPosition, CameraBone.transform.position, LerpTime);
                FollowRotation = Quaternion.Lerp(FollowRotation, CameraBone.transform.rotation, RotateLerpSpeed * Time.deltaTime);
                Vector3 BobbedCameraPosition = FollowPosition;
                BobbedCameraPosition += motionBob.DoHeadBob(FollowVelocity);
                if (IsMovingOnGround)
                {
                    BobbedCameraPosition.y = BobbedCameraPosition.y - jumpAndLandingBob.Offset();
                }
                else
                {
                    BobbedCameraPosition.y -= jumpAndLandingBob.Offset();
                }
                MyCamera.position = BobbedCameraPosition;
                MyCamera.rotation = FollowRotation;

                if (!m_PreviouslyGrounded && MyMover.Grounded)
                {
                    StartCoroutine(jumpAndLandingBob.DoBobCycle());
                }

                m_PreviouslyGrounded = MyMover.Grounded;
            }
        }
    }
}
