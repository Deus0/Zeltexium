using System;
using UnityEngine;

namespace Zeltex.AI
{
    [RequireComponent(typeof (Rigidbody))]
    public class MyController : MonoBehaviour
	{
		private Rigidbody m_RigidBody;
		[Tooltip("Used to spherecast movement")]
		public CapsuleCollider m_Capsule;
		private float m_YRotation;
		private Vector3 m_GroundContactNormal;
		private bool m_Jump, m_PreviouslyGrounded, m_Jumping;
		public bool m_IsGrounded;

        [Serializable]
        public class MovementSettings
        {
            public float ForwardSpeed = 8.0f;   // Speed when walking forward
            public float BackwardSpeed = 4.0f;  // Speed when walking backwards
            public float StrafeSpeed = 4.0f;    // Speed when walking sideways
            public float RunMultiplier = 2.0f;   // Speed when sprinting
	        public KeyCode RunKey = KeyCode.LeftShift;
            public float JumpForce = 30f;
            public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
            [HideInInspector] public float CurrentTargetSpeed = 8f;

#if !MOBILE_INPUT
            private bool m_Running;
#endif

            public void UpdateDesiredTargetSpeed(Vector2 input)
            {
	            if (input == Vector2.zero) return;
				if (input.x > 0 || input.x < 0)
				{
					//strafe
					CurrentTargetSpeed = StrafeSpeed;
				}
				if (input.y < 0)
				{
					//backwards
					CurrentTargetSpeed = BackwardSpeed;
				}
				if (input.y > 0)
				{
					//forwards
					//handled last as if strafing and moving forward at the same time forwards speed should take precedence
					CurrentTargetSpeed = ForwardSpeed;
				}
#if !MOBILE_INPUT
	            if (Input.GetKey(RunKey))
	            {
		            CurrentTargetSpeed *= RunMultiplier;
		            m_Running = true;
	            }
	            else
	            {
		            m_Running = false;
	            }
#endif
            }

#if !MOBILE_INPUT
            public bool Running
            {
                get { return m_Running; }
            }
#endif
        }


        [Serializable]
        public class AdvancedSettings
        {
            public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
            public float stickToGroundHelperDistance = 0.5f; // stops the character
            public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
            public bool airControl; // can the user control the direction that is being moved in the air
        }


        public Transform cam;
        public MovementSettings movementSettings = new MovementSettings();
		//public MouseLook mouseLook = new MouseLook();
        public AdvancedSettings advancedSettings = new AdvancedSettings();


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

        public bool Running
        {
            get
            {
 #if !MOBILE_INPUT
				return movementSettings.Running;
#else
	            return false;
#endif
            }
        }

		//PhotonTransformView m_TransformView;
		private void Start()
		{
            m_RigidBody = GetComponent<Rigidbody>();
			if (m_Capsule == null)
           		m_Capsule = GetComponent<CapsuleCollider>();
		}
		public void UpdateCamera(Transform NewCamera) {
			cam = NewCamera;
			//mouseLook.Init (transform, cam);
		}

		private void Update()
        {
			
			{
				RotateView ();

				//if (CrossPlatformInputManager.GetButtonDown ("Jump") && !m_Jump)
                if (Input.GetKeyDown(KeyCode.Space) && !m_Jump)
                {
					m_Jump = true;
				}
				ApplyMovementForce(DesiredMovement);
				/*MyPhoton.RPC("ApplyMovementForce",
				             PhotonTargets.All,
				             DesiredMovement);*/
			}

			//ApplyMovementForce ();
		}
		
		/*void ApplySynchronizedValues()
		{
			m_TransformView.SetSynchronizedValues( m_CurrentMovement, m_CurrentTurnSpeed );
		}*/
		Vector2 input;	
		Vector3 DesiredMovement;

		private void HandleInput() {
			DesiredMovement = new Vector3 ();
			input = GetInput ();
			
			if ((Mathf.Abs (input.x) > float.Epsilon || Mathf.Abs (input.y) > float.Epsilon) && (advancedSettings.airControl || m_IsGrounded)) {
				// always move along the camera forward as it is the direction that it being aimed at
				Vector3 DirectionForward = transform.forward;	// cam.transform.forward
				Vector3 DirectionRight = transform.right;	// cam.transform.right
				DesiredMovement = DirectionForward * input.y + DirectionRight * input.x;
				// this thing! wut!
				DesiredMovement = Vector3.ProjectOnPlane (DesiredMovement, m_GroundContactNormal).normalized;
				
				DesiredMovement.x = DesiredMovement.x * movementSettings.CurrentTargetSpeed;
				DesiredMovement.z = DesiredMovement.z * movementSettings.CurrentTargetSpeed;
				DesiredMovement.y = DesiredMovement.y * movementSettings.CurrentTargetSpeed;

				//MyPhotonTransform.SetSynchronizedValues(DesiredMovement, movementSettings.CurrentTargetSpeed);
			}
		}

		private void ApplyMovementForce(Vector3 DesiredSpeed) 
		{
			if (m_RigidBody.velocity.sqrMagnitude <
				(movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed)) // if not at maximum velocity, increase it!
			{
				m_RigidBody.AddForce (DesiredSpeed * SlopeMultiplier (), ForceMode.Impulse);
			}
		}
			
		private void HandleJump() {
			if (m_IsGrounded) {
				m_RigidBody.drag = 5f;
				
				if (m_Jump) {
					m_RigidBody.drag = 0f;
					m_RigidBody.velocity = new Vector3 (m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
					m_RigidBody.AddForce (new Vector3 (0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
					m_Jumping = true;
				}
				
				if (!m_Jumping && Mathf.Abs (input.x) < float.Epsilon && Mathf.Abs (input.y) < float.Epsilon && m_RigidBody.velocity.magnitude < 1f) {
					m_RigidBody.Sleep ();
				}
			} else {
				m_RigidBody.drag = 0f;
				if (m_PreviouslyGrounded && !m_Jumping) {
					StickToGroundHelper ();
				}
			}
			m_Jump = false;
		}

		private void FixedUpdate()
		{

            {
				GroundCheck ();
				HandleInput();
				HandleJump();
			}

        }


        private float SlopeMultiplier()
        {
            float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
            return movementSettings.SlopeCurveModifier.Evaluate(angle);
        }


        private void StickToGroundHelper()
        {
			RaycastHit hitInfo;
			float CheckDistance = m_Capsule.radius + advancedSettings.groundCheckDistance;
			if (Physics.SphereCast(transform.position, m_Capsule.radius-advancedSettings.groundCheckDistance, Vector3.down, out hitInfo, CheckDistance))
           // if (Physics.SphereCast(transform.position, m_Capsule.radius, Vector3.down, out hitInfo,
			//                       m_Capsule.radius + advancedSettings.stickToGroundHelperDistance))
            {
                if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
                {
                    m_RigidBody.velocity = Vector3.ProjectOnPlane(m_RigidBody.velocity, hitInfo.normal);
                }
            }
        }


        private Vector2 GetInput()
        {

            Vector2 input = new Vector2(0, 0);
                   // x = CrossPlatformInputManager.GetAxis("Horizontal"),
                    //y = CrossPlatformInputManager.GetAxis("Vertical")
            if (Input.GetKeyDown(KeyCode.W))
            {
                input.y = 1;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                input.y = -1;
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                input.x = -1;
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                input.x = 1;
            }
            movementSettings.UpdateDesiredTargetSpeed(input);
            return input;
        }


        private void RotateView()
        {
			if (cam)
            {
				// get the rotation before it's changed
				float oldYRotation = transform.eulerAngles.y;

				//mouseLook.LookRotation (transform, cam.transform);

				if (m_IsGrounded || advancedSettings.airControl) {
					// Rotate the rigidbody velocity to match the new direction that the character is looking
					Quaternion velRotation = Quaternion.AngleAxis (transform.eulerAngles.y - oldYRotation, Vector3.up);
					m_RigidBody.velocity = velRotation * m_RigidBody.velocity;
				}
			}
        }
		void OnTriggerEnter() {
			m_IsGrounded = true;
			m_GroundContactNormal = Vector3.up;
			//Debug.LogError ("Trigger Enter " + Time.time);
		}
		void OnTriggerStay() {
			m_IsGrounded = true;
			m_GroundContactNormal = Vector3.up;
			//Debug.LogError ("Trigger Enter " + Time.time);
		}
		void OnTriggerExit() {
			m_IsGrounded = false;
			//Debug.LogError ("Trigger Exit " + Time.time);
		}
		public LayerMask GroundCheckLayer;
        /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
        private void GroundCheck()
        {
            /*m_PreviouslyGrounded = m_IsGrounded;
            RaycastHit hitInfo;
			float CheckDistance = advancedSettings.groundCheckDistance*2f;	//m_Capsule.radius + 
			if (Physics.SphereCast(transform.position, m_Capsule.radius-advancedSettings.groundCheckDistance, Vector3.down, out hitInfo, CheckDistance, GroundCheckLayer))
			//if (Physics.Raycast(transform.position, Vector3.down, out hitInfo,
			//                       CheckDistance))
            {
				Debug.DrawLine(transform.position, hitInfo.point, Color.green);
                m_IsGrounded = true;
                m_GroundContactNormal = hitInfo.normal;
            }
            else
			{
				Debug.DrawLine(transform.position, transform.position+Vector3.down*CheckDistance, Color.red);
                m_IsGrounded = false;
                m_GroundContactNormal = Vector3.up;
            }
			*/
            if (//!m_PreviouslyGrounded &&
			    m_IsGrounded && m_Jumping)
            {
                m_Jumping = false;
            }
        }
    }
}
