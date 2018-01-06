using System;
using UnityEngine;
using UnityStandardAssets.Utility;

namespace Zeltex
{
    public class CameraBob : MonoBehaviour
    {
		public Transform Camera;
        public CurveControlledBob motionBob = new CurveControlledBob();
        public LerpControlledBob jumpAndLandingBob = new LerpControlledBob();
		private Mover MyMover;
        public float StrideInterval;
        [Range(0f, 1f)] public float RunningStrideLengthen;

       // private CameraRefocus m_CameraRefocus;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;


        private void Start()
        {
       //     m_CameraRefocus = new CameraRefocus(Camera, transform.root.transform, Camera.transform.localPosition);
        }

		public void Initialise(Mover NewMover) 
		{
			MyMover = NewMover;
			motionBob.Setup(Camera.parent, StrideInterval);
			m_OriginalCameraPosition = Camera.parent.localPosition;
		}


        private void LateUpdate()
        {
            if (MyMover)
            {
                UpdateBob();
            }
        }

        private void UpdateBob() 
        {
            Vector3 newCameraPosition;
            if (MyMover.Velocity.magnitude > 0 && MyMover.Grounded)
            {
                Camera.transform.parent.localPosition = motionBob.DoHeadBob(MyMover.Velocity.magnitude*(MyMover.Running ? RunningStrideLengthen : 1f));
                newCameraPosition = Camera.parent.localPosition;
                newCameraPosition.y = Camera.parent.localPosition.y - jumpAndLandingBob.Offset();
            }
            else
            {
                newCameraPosition = Camera.parent.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - jumpAndLandingBob.Offset();
            }
            Camera.parent.localPosition = newCameraPosition;

            if (!m_PreviouslyGrounded && MyMover.Grounded)
            {
                StartCoroutine(jumpAndLandingBob.DoBobCycle());
            }

            m_PreviouslyGrounded = MyMover.Grounded;
        }
    }
}
