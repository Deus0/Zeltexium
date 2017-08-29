using UnityEngine;

namespace Zeltex.AI {
    public class PlayerController2D : MonoBehaviour
    {
	    private bool m_Jump;
	    [SerializeField] private float m_MaxSpeed = 10f;                    // The fastest the player can travel in the x axis.
	    [SerializeField] private float m_JumpForce = 400f;                  // Amount of force added when the player jumps.
	    [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;  // Amount of maxSpeed applied to crouching movement. 1 = 100%
	    [SerializeField] private bool m_AirControl = false;                 // Whether or not a player can steer while jumping;
	    [SerializeField] private LayerMask m_WhatIsGround;                  // A mask determining what is ground to the character

	    [SerializeField] private Vector3 GravityForce = new Vector3(0,-5,0);
	    //private Transform m_GroundCheck;    // A position marking where to check if the player is grounded.
	    [SerializeField] private float k_GroundedRadius = 1f; // Radius of the overlap circle to determine if grounded
	    public bool m_Grounded;            // Whether or not the player is grounded.
	    private Transform m_CeilingCheck;   // A position marking where to check for ceilings
	    const float k_CeilingRadius = .01f; // Radius of the overlap circle to determine if the player can stand up
	    //private Animator m_Anim;            // Reference to the player's animator component.
	    private Rigidbody MyRigidBody;
	    private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	
	    Vector3 GroundCheckPosition;
	    public int JumpInAirCount = 0;
	    public int MaxAirJumps = 2;

	    void Awake()
	    {
		    // Setting up references.
		    //m_GroundCheck = transform.Find("GroundCheck");
		    //m_CeilingCheck = transform.Find("CeilingCheck");
		    //m_Anim = GetComponent<Animator>();
		    MyRigidBody = GetComponent<Rigidbody>();
		    MyRigidBody.useGravity = false;
	    }
	
	    void Update()
	    {
		    if (!m_Jump)
		    {
                // Read the jump input in Update so button presses aren't missed
                m_Jump = Input.GetKeyDown(KeyCode.Space);
                //m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }
	    }
	    void FixedUpdate()
	    {
		    //MyRigidBody.AddForce (GravityForce);
		    m_Grounded = false;
		
		    // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		    // This can be done using layers instead but Sample Assets will not overwrite your project settings.
		    //if (m_GroundCheck != null) 
		    {

			    if (gameObject.GetComponent<ArtificialGravity>()) {
				    GroundCheckPosition = gameObject.GetComponent<ArtificialGravity>().GravityForce.normalized;
			    }
			    GroundCheckPosition.x *= transform.lossyScale.x;
			    GroundCheckPosition.y *= Mathf.Abs(transform.lossyScale.y);
			    //Vector3 GroundCheckPosition2 = (GroundCheckPosition);		//transform.parent.TransformDirection
			    //Debug.LogError("GroundCheckPosition: " + GroundCheckPosition2.ToString());
			    Collider[] colliders = Physics.OverlapSphere (transform.position+GroundCheckPosition, k_GroundedRadius*transform.lossyScale.y, m_WhatIsGround);	//
			    //Debug.DrawLine(GroundCheckPosition2, 
			    //               transform.position + transform.parent.TransformDirection(GroundCheckPosition/20f+new Vector3(0,0,-0.1f)), Color.red, 30f);
			    //Debug.LogError("colliders: " + colliders.Length);
			    for (int i = 0; i < colliders.Length; i++) {
				    if (colliders [i].gameObject != gameObject) 
				    if (colliders[i].tag == "Ground") {
					    m_Grounded = true;
					    JumpInAirCount = 0;
					    break;
				    }
			    }
			    //m_Anim.SetBool("Ground", m_Grounded);
		    }
		
		    // Set the vertical animation
		    //m_Anim.SetFloat("vSpeed", m_Rigidbody2D.velocity.y);

		    // Read the inputs.
		    bool crouch = Input.GetKey(KeyCode.LeftControl);
		    //float h = CrossPlatformInputManager.GetAxis("Horizontal");
		    //Debug.LogError ("Moving: " + h);
		    // Pass all parameters to the character control script.
		    //Move(h, crouch, m_Jump);
		    m_Jump = false;
	    }
	
	
	    public void Move(float move, bool crouch, bool jump)
	    {
		    // If crouching, check to see if the character can stand up
		    /*if (!crouch)
		    {
			    // If the character has a ceiling preventing them from standing up, keep them crouching
			    if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
			    {
				    crouch = true;
			    }
		    }*/
		
		    // Set whether or not the character is crouching in the animator
		    //m_Anim.SetBool("Crouch", crouch);
		
		    //only control the player if grounded or airControl is turned on
		    if (m_Grounded || m_AirControl)
		    {
			    // Reduce the speed if crouching by the crouchSpeed multiplier
			    move = (crouch ? move*m_CrouchSpeed : move);
			
			    // The Speed animator parameter is set to the absolute value of the horizontal input.
			    //m_Anim.SetFloat("Speed", Mathf.Abs(move));
			
			    // Move the character
			    MyRigidBody.velocity = transform.parent.TransformDirection(new Vector3(move*m_MaxSpeed, MyRigidBody.velocity.y, 0f));
			
			    // If the input is moving the player right and the player is facing left...
			    if (move > 0 && !m_FacingRight)
			    {
				    // ... flip the player.
				    Flip();
			    }
			    // Otherwise if the input is moving the player left and the player is facing right...
			    else if (move < 0 && m_FacingRight)
			    {
				    // ... flip the player.
				    Flip();
			    }
		    }
		    // If the player should jump...
		    if (m_Grounded && jump)
		    {
			    Debug.Log("Jumping");
			    DoJump(m_JumpForce);
		    }
		    else if (!m_Grounded && jump && JumpInAirCount < MaxAirJumps) {
			    JumpInAirCount++;
			    Debug.Log("AirJumping");
			    DoJump(m_JumpForce);
		    }
	    }

	    void DoJump(float JumpForce) {
		    // Add a vertical force to the player.
		    Vector3 JumpForce2 = -gameObject.GetComponent<ArtificialGravity>().GravityForce.normalized*JumpForce;
		    if (MyRigidBody.velocity.y < 0 && !m_Grounded)
		    {
			    MyRigidBody.velocity = new Vector3(MyRigidBody.velocity.x, 0, MyRigidBody.velocity.z);
			    //JumpForce2.y += -MyRigidBody.velocity.y*JumpForce;
			    //Debug.LogError ("Adding: " + (-MyRigidBody.velocity.y*JumpForce).ToString() + " To jump force.");
		    }
		    MyRigidBody.AddForce(JumpForce2);
		    m_Grounded = false;
	    }
	    private void Flip()
	    {
		    // Switch the way the player is labelled as facing.
		    m_FacingRight = !m_FacingRight;
		
		    // Multiply the player's x local scale by -1.
		    Vector3 theScale = transform.localScale;
		    theScale.x *= -1;
		    transform.localScale = theScale;
	    }
    }
}