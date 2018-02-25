using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Characters.FirstPerson;
using Zeltex.AI;

namespace Zeltex
{
    public class Mover : MonoBehaviour
    {
        public bool IsPlayer;
        public float GroundedDrag = 5;
        [SerializeField]
        private Transform CameraTransform;
        public MouseLook mouseLook = new MouseLook();
        public AdvancedSettings advancedSettings = new AdvancedSettings();
        private Rigidbody m_RigidBody;
        private CapsuleCollider m_Capsule;
        private float m_YRotation;
        private Vector3 m_GroundContactNormal;
        private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;
        private Bot MyBot;
        [SerializeField]
        public LayerMask GroundLayer;
        private Vector2 MovementInput = Vector2.zero;
        private Vector2 RotationInput = Vector2.zero;
		//private float OriginalDrag;
		private CameraBob MyBob;
        public float JumpForce = 26f;
        // This is used for multiplying angle facing with speed
        public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));

        public void RefreshRigidbody()
        {
            m_RigidBody = GetComponent<Rigidbody>();
        }

        public Vector3 Velocity
        {
            get { return m_RigidBody.velocity; }
        }

        public bool Grounded
        {
            get { return m_IsGrounded; }
        }

        public bool Jumping
        {
            get { return m_Jumping; }
        }

        private void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();
            MyBot = GetComponent<AI.Bot>();
        }

        /// <summary>
        /// Rigidbody updates with Fixed Update, so movement needs to as well
        /// </summary>
        private void FixedUpdate()
        {
            if (m_Capsule != null)
            {
                CheckCameraBob();
                RotateView();
                UpdateInput();
                GroundCheck();
                UpdateMovementForce();
                UpdateGroundMovement();
            }
        }

        public void SetBot(Bot NewBot)
        {
            MyBot = NewBot;
        }

        private void CheckCameraBob() 
        {
            if (IsPlayer && MyBob == null)
            {
                //Camera MyCamera = NewCameraBone.GetComponentInChildren<Camera>();
                Camera MyCamera = CameraManager.Get().GetMainCamera();
                if (MyCamera)
                {
                    MyBob = MyCamera.transform.gameObject.GetComponent<CameraBob>();
                    if (MyBob)
                    {
                        MyBob.Initialise(this);
                    }
                }
            }
        }

        public void SetCameraBone(Transform NewCameraBone)
        {
            CameraTransform = NewCameraBone;
            mouseLook.Init(transform, CameraTransform);
            MyBob = null;
            CheckCameraBob();
        }

        public Transform GetCameraBone() 
        {
            return CameraTransform;
        }

        private Vector2 GetInput()
        {
            Vector2 input = new Vector2
            {
                x = CrossPlatformInputManager.GetAxis("Horizontal"),
                y = CrossPlatformInputManager.GetAxis("Vertical")
            };
            //movementSettings.UpdateDesiredTargetSpeed(input);
            return input;
        }

		private void UpdateInput() 
		{
			if (IsPlayer && CrossPlatformInputManager.GetButtonDown("Jump") && !m_Jump)
			{
				m_Jump = true;
			}
		}

        public float SpeedMultiplier = 1.14f;
        private void UpdateMovementForce() 
        {
            if (IsPlayer)
            {
                MovementInput = GetInput();
            }
            else
            {
                if (MyBot)
                {
                    MovementInput = MyBot.GetInput();
                }
            }
            if ((Mathf.Abs(MovementInput.x) > float.Epsilon || Mathf.Abs(MovementInput.y) > float.Epsilon) && (advancedSettings.airControl || m_IsGrounded))
            {
                // always move along the camera forward as it is the direction that it being aimed at
                //Vector3 desiredMove = transform.forward * MovementInput.y + transform.right * MovementInput.x;
                Vector3 desiredMove;
                if (CameraTransform)
                {
                    desiredMove = transform.forward * MovementInput.y + transform.right * MovementInput.x;
                }
                else
                {
                    desiredMove = CameraTransform.transform.forward * MovementInput.y + CameraTransform.transform.right*MovementInput.x;
                }
                //desiredMove = Vector3.ProjectOnPlane(desiredMove, Vector3.down).normalized;   //m_GroundContactNormal

                desiredMove.x = desiredMove.x * SpeedMultiplier;
                desiredMove.z = desiredMove.z * SpeedMultiplier;
                desiredMove.y = desiredMove.y * SpeedMultiplier;
                if (m_RigidBody.velocity.sqrMagnitude < SpeedMultiplier * SpeedMultiplier)
                {
                    m_RigidBody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.VelocityChange);
                    //m_RigidBody.AddForce(desiredMove, ForceMode.Impulse);
                }
            }
        }

        private void UpdateGroundMovement()
        {
            if (m_IsGrounded)
            {
                m_RigidBody.drag = GroundedDrag;

                if (m_Jump)
                {
                    m_RigidBody.drag = 0f;
                    m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
                    m_RigidBody.AddForce(new Vector3(0f, JumpForce, 0f), ForceMode.Impulse);
                    m_Jumping = true;
                }

                if (!m_Jumping && Mathf.Abs(MovementInput.x) < float.Epsilon && Mathf.Abs(MovementInput.y) < float.Epsilon && m_RigidBody.velocity.magnitude < 1f)
                {
                    m_RigidBody.Sleep();
                }
            }
            else
            {
                m_RigidBody.drag = 0f;
                if (m_PreviouslyGrounded && !m_Jumping)
                {
                    StickToGroundHelper();
                }
            }
            m_Jump = false;
        }


        private float SlopeMultiplier()
        {
            float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
            return SlopeCurveModifier.Evaluate(angle);
        }


        private void StickToGroundHelper()
        {
            RaycastHit hitInfo;
            if (UnityEngine.Physics.SphereCast(
                                    transform.position,
                                    m_Capsule.radius * (1.0f - advancedSettings.shellOffset), 
                                    Vector3.down, out hitInfo,
                                   // ((m_Capsule.height / 2f) - m_Capsule.radius) +
                                   //advancedSettings.stickToGroundHelperDistance,
                                   (m_Capsule.height / 2f),
                                   GroundLayer))
                                   //, QueryTriggerInteraction.Ignore))// Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
                {
                    m_RigidBody.velocity = Vector3.ProjectOnPlane(m_RigidBody.velocity, hitInfo.normal);
                }
            }
        }

        public void RotateCamera(Vector3 TargetRotation)
        {
            if (CameraTransform)
            {
                CameraTransform.eulerAngles = TargetRotation;
            }
            else
            {
                transform.eulerAngles = TargetRotation;
            }
        }
        
        private void RotateView()
        {
            //avoids the mouse looking if the game is effectively paused
            if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

            // get the rotation before it's changed
            float oldYRotation = transform.eulerAngles.y;

            if (IsPlayer)
            {
                RotationInput.Set(
                    CrossPlatformInputManager.GetAxis("Mouse Y") * mouseLook.YSensitivity, 
                    CrossPlatformInputManager.GetAxis("Mouse X") * mouseLook.XSensitivity);
                if (CameraTransform)
                {
                    mouseLook.LookRotation(transform, CameraTransform, RotationInput);
                }
                else
                {
                    mouseLook.LookRotation(transform, transform, RotationInput);
                }
                mouseLook.UpdateCursorLock();
            }
            else if (MyBot)
            {
                //Vector3 TargetRotation = MyBot.GetTargetRotation();
                mouseLook.SetRotation(transform, CameraTransform, MyBot.GetTargetRotation());
            }

            if (m_IsGrounded || advancedSettings.airControl)
            {
                // Rotate the rigidbody velocity to match the new direction that the character is looking
                Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
                m_RigidBody.velocity = velRotation * m_RigidBody.velocity;
            }
        }

        public bool IsDebugRay;
        /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
        private void GroundCheck()
        {
            m_PreviouslyGrounded = m_IsGrounded;
            RaycastHit hitInfo;
            float RayDistance = 1.02f * ((m_Capsule.height / 2f) + Mathf.Abs(m_Capsule.center.y));
            if (IsDebugRay)
            {
                Debug.DrawLine(transform.position, transform.position - transform.up * RayDistance, Color.red);
            }
            if (UnityEngine.Physics.Raycast(transform.position, -transform.up, out hitInfo, RayDistance, GroundLayer))
            //if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
           //                        ((m_Capsule.height / 2f) - m_Capsule.radius) + advancedSettings.groundCheckDistance, GroundLayer, QueryTriggerInteraction.Ignore))   // Physics.AllLayers
            {
                m_IsGrounded = true;
                m_GroundContactNormal = hitInfo.normal;
            }
            else
            {
                m_IsGrounded = false;
                m_GroundContactNormal = Vector3.up;
            }
            if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping)
            {
                m_Jumping = false;
            }
        }

        [Serializable]
        public class AdvancedSettings
        {
            public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
            public float stickToGroundHelperDistance = 0.5f; // stops the character
            public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
            public bool airControl; // can the user control the direction that is being moved in the air
            [Tooltip("set it to 0.1 or more if you get stuck in wall")]
            public float shellOffset; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
        }
    }
}
