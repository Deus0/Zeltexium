using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Zeltex.Skeletons;
using Zeltex.Characters;
using Zeltex;

namespace Zeltex.AI
{
    /// <summary>
    /// Controls movement by altering rigidbody
    /// </summary>
    public class BasicController : NetworkBehaviour
    {
        #region Variables
        [Header("Debug")]
        [SerializeField]
        private float RaycastModifier = 1.1f;       // used for ground checks
        [SerializeField]
        private bool IsDebugLines;
        private static float TimeToChange = 0.05f;
        // references
        private Rigidbody MyRigidbody;
        private SkeletonAnimator MyAnimator;
        private SkeletonHandler MySkeleton;
        private Bounds MyBounds;
        [Header("Options")]
        [SerializeField]
        private BotMovementSettings Settings;
        [SerializeField]
        private bool IsGravity = true;
        [SerializeField]
        private bool IsAirControl;
        //[Header("Debugging")]
        private bool IsJump;
        private bool IsGrounded;
        private bool CanFly;
        // Hidden
        private bool IsWalking; // used to animate walk cycles
        [HideInInspector]
        public bool IsJumping; // True when jumping action is in effect
        [Header("Variables")]
        [SerializeField]
        private float MovementSpeed = 2.0F;
        [SerializeField]
        private float JumpSpeed = 8.0F;
        [SerializeField]
        private Vector3 GravitationalForce = new Vector3(0, -20, 0);
        [SerializeField]
        private float RotationForce = 1f;
        //[SerializeField]
        private Vector3 MoveForce = Vector3.zero;
        // privates
        private float MovementX;
        private float MovementZ;
        private float MovementY;
        private Vector3 MyVelocity;
        private Vector3 TargetRotation;
        private float JumpTime = 0.16f;
        private float HangTime = 0.12f;
        private float MaxVelocityChange = 1;
        // Animation Controller
        private float TimeLastChangedState;
        private float AnimationWalkSpeed = 2.8f;
        // Running
        [HideInInspector]
        public bool IsRunning;
        private float RunMultiplier = 4;
        private CapsuleCollider MyCapsule;
        private bool IsRotate;
        private NetworkIdentity MyNetworkIdentity;
        private Character MyCharacter;
        [SerializeField]
        private bool DebugRotationInstant;
        #endregion

        #region Mono
        void Start()
        {
            MyNetworkIdentity = GetComponent<NetworkIdentity>();
            RefreshRigidbody();
        }

        public void RefreshRigidbody()
        {
            MyRigidbody = GetComponent<Rigidbody>();
            if (GetComponent<Bot>())
            {
                GetComponent<Bot>().RefreshRigidbody();
            }
            MyCapsule = GetComponent<CapsuleCollider>();
            RefreshCapsuleBounds();
            MyCharacter = gameObject.GetComponent<Character>();
            if (MyCharacter)
            {
                MySkeleton = MyCharacter.GetSkeleton();
                if (MySkeleton)
                {
                    MySkeleton.GetSkeleton().OnLoadSkeleton.AddEvent(RefreshCapsuleBounds);
                }
            }
        }

        private void RefreshCapsuleBounds()
        {
            if (MyCapsule)
            {
                MyBounds = MyCapsule.bounds;
            }
        }

        void FixedUpdate()
        {
            if (MyRigidbody && MyNetworkIdentity.localPlayerAuthority)
            {
                ApplyRotation();
                UpdateMovement();
                ApplyGravity();
            }
        }

        void Update()
        {
            if (MyNetworkIdentity.localPlayerAuthority && MyRigidbody)
            {
                ModifyAnimator();
            }
        }
        #endregion

        #region Movement
        /// <summary>
        /// Forces a jump!
        /// </summary>
        public void Jump()
        {
            if (enabled && IsJumping == false)
            {
                IsJumping = true;
                StartCoroutine(JumpRoutine());
            }
        }
        /// <summary>
        /// Set current frames input
        /// </summary>
        public void Input(float MovementX_, float MovementZ_, bool IsJump_)
        {
            IsJump = IsJump_;
            MovementX = MovementX_;
            MovementZ = MovementZ_;
            // if getting input keep upright
            //var rot = Quaternion.FromToRotation(transform.up, Vector3.up);
            //MyRigidbody.AddTorque(new Vector3(rot.x, rot.y, rot.z), ForceMode.VelocityChange);
        }

        public void InputY(float InputY_)
        {
            MovementY = InputY_;
        }

        /// <summary>
        /// Set movement speed of character
        /// </summary>
        public void SetMovementSpeed(float NewSpeed)
        {
            MovementSpeed = NewSpeed;
            MovementSpeed = Mathf.Clamp(MovementSpeed, 0, 200);
        }

        public float GetMovementSpeed()
        {
           return MovementSpeed;
        }

        void UpdateMovement()
        {
            float Height = MyBounds.extents.y * RaycastModifier;
            float Width = MyBounds.extents.x;
            float Depth = MyBounds.extents.z;
            Vector3 Position0 = transform.position + MyBounds.center;
            Vector3 Position1 = Position0 + transform.TransformDirection(new Vector3(Width, 0, Depth));
            Vector3 Position2 = Position0 + transform.TransformDirection(new Vector3(Width, 0, -Depth));
            Vector3 Position3 = Position0 + transform.TransformDirection(new Vector3(-Width, 0, Depth));
            Vector3 Position4 = Position0 + transform.TransformDirection(new Vector3(-Width, 0, -Depth));
            LayerMask MyLayer = LayerManager.Get().GetWorldsLayer();
            IsGrounded = (UnityEngine.Physics.Raycast(Position0, GravitationalForce.normalized, Height, MyLayer)
                || UnityEngine.Physics.Raycast(Position1, GravitationalForce.normalized, Height, MyLayer)
                || UnityEngine.Physics.Raycast(Position2, GravitationalForce.normalized, Height, MyLayer)
                || UnityEngine.Physics.Raycast(Position3, GravitationalForce.normalized, Height, MyLayer)
                || UnityEngine.Physics.Raycast(Position4, GravitationalForce.normalized, Height, MyLayer));
            if (IsDebugLines)
            {
                Debug.DrawLine(Position1, Position1 + GravitationalForce.normalized * Height, Color.red);
                Debug.DrawLine(Position2, Position2 + GravitationalForce.normalized * Height, Color.red);
                Debug.DrawLine(Position3, Position3 + GravitationalForce.normalized * Height, Color.red);
                Debug.DrawLine(Position4, Position4 + GravitationalForce.normalized * Height, Color.red);
            }
            // Apply movement forces if on ground
            if (IsGrounded || !IsGravity || IsAirControl)
            {
                //Feed moveDirection with input.
                MoveForce = new Vector3(MovementX, MovementY, MovementZ);
                MoveForce = MySkeleton.GetSkeleton().GetCameraBone().TransformDirection(MoveForce);//  transform.TransformDirection(MoveForce);
                //Multiply it by speed.
                MoveForce *= MovementSpeed;
                if (IsRunning)
                {
                    MoveForce *= RunMultiplier;
                }
                // reset input
                MovementX *= 0.9f;
                MovementZ *= 0.9f;
                MovementY *= 0.9f;
            }
            //Vector3 velocity = MyRigidbody.velocity;
            MyVelocity = MyRigidbody.velocity;
            Vector3 DesiredForce = (MoveForce * Time.deltaTime - MyVelocity);
            DesiredForce.x = Mathf.Clamp(DesiredForce.x, -MaxVelocityChange, MaxVelocityChange);
            DesiredForce.z = Mathf.Clamp(DesiredForce.z, -MaxVelocityChange, MaxVelocityChange);
            DesiredForce.y = Mathf.Clamp(DesiredForce.y, -MaxVelocityChange, MaxVelocityChange);
            //MyRigidbody.velocity = DesiredForce;
            MyRigidbody.AddForce(DesiredForce, ForceMode.VelocityChange);
        }
        #endregion

        #region Gravity

        public void ToggleFly()
        {
            CanFly = !CanFly;
        }

        public bool IsFlyer()
        {
            return CanFly;
        }

        /// <summary>
        /// Apply gravity and jumping
        /// </summary>
        private void ApplyGravity()
        {
            if (IsGravity && IsGrounded == false)
            {
                MyRigidbody.AddForce(GravitationalForce);   // * Time.deltaTime
                if (CanFly)
                {
                    MyRigidbody.AddForce(- 0.9f * GravitationalForce);   // * Time.deltaTime
                }
            }
            //Jumping
            if (IsJumping == false && IsJump)// && (IsGrounded || CanFly)) // 
            {
                Jump();
                //MoveForce.y = jumpSpeed;
            }
        }

        public void SetRotationSpeed(float NewSpeed)
        {
            RotationForce = NewSpeed;
        }

        /*public void SetRotation(Quaternion NewRotation)
        {
            TargetRotation = NewRotation.eulerAngles;
            if (GetComponent<Player>())
            {
                GetComponent<Player>().SetRotation(TargetRotation);
            }
        }*/


        /// <summary>
        /// Purely for the body
        /// </summary>
        /*public void InputRotation(Vector3 RotationForce_)
        {
            Quaternion MyRotation = transform.rotation;
            transform.Rotate(RotationForce_ * Time.deltaTime * 60f);
            TargetRotation = transform.forward;
            transform.rotation = MyRotation;
        }*/

        public void InputTargetRotation(Vector3 NewTargetRotation)
        {
            Quaternion MyRotation = transform.rotation;
            transform.eulerAngles = NewTargetRotation;
            TargetRotation = transform.forward;
            transform.rotation = MyRotation;
        }

        public void SetRotationVelocity(Vector3 NewAngularVelocity)
        {
            if (MyRigidbody == null)
            {
                MyRigidbody = GetComponent<Rigidbody>();
            }
            if (MyRigidbody != null)
            {
                MyRigidbody.angularVelocity = NewAngularVelocity;
            }
        }

        public void SetRotationState(bool NewRotationState)
        {
            IsRotate = NewRotationState;
        }

        private void ApplyRotation()
        {
            if (IsRotate)
            {
                Vector3 FromRotation = transform.forward;
                Vector3 ToRotation = TargetRotation.normalized;
                float Threshold = 0.05f;
                if (!(ToRotation.x >= FromRotation.x - Threshold && ToRotation.x <= FromRotation.x + Threshold
                    && ToRotation.y >= FromRotation.y - Threshold && ToRotation.y <= FromRotation.y + Threshold
                    && ToRotation.z >= FromRotation.z - Threshold && ToRotation.z <= FromRotation.z + Threshold))
                {
                    //get the angle between transform.forward and target delta
                    float AngleDifference = Vector3.Angle(FromRotation, ToRotation);
                    // get its cross product, which is the axis of rotation to
                    // get from one vector to the other
                    Vector3 CrossProduct = Vector3.Cross(FromRotation, ToRotation);

                    // apply torque along that axis according to the magnitude of the angle.
                    if (IsDebugLines)
                    {
                        Debug.DrawLine(transform.position, transform.position + transform.forward * 2f, Color.green);
                        Debug.DrawLine(transform.position, transform.position + TargetRotation.normalized * 2f, Color.red);
                    }
                    if (!DebugRotationInstant)
                    {
                        MyRigidbody.AddTorque(AngleDifference * CrossProduct * RotationForce);
                    }
                    else
                    {
                        transform.eulerAngles = ToRotation;
                    }
                }
            }
        }

        public IEnumerator JumpRoutine()
        {
            bool WasGravity = IsGravity;
            IsGravity = false;
            float TimeStart = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - TimeStart < JumpTime)
            {
                if (MyRigidbody)
                {
                    MyRigidbody.AddForce(-JumpSpeed * GravitationalForce.normalized);
                }
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForSeconds(HangTime);
            IsGravity = WasGravity;
            IsJumping = false;
            IsJump = false;
        }

        /// <summary>
        /// Keeps floats between -180 and 180
        /// </summary>
        public static Vector3 ApplyMagicToRotation(Vector3 InputRotation)
        {
            if (InputRotation.y >= 360)
            {
                InputRotation.y -= 360;
            }
            if (InputRotation.y < 0)
            {
                InputRotation.y += 360;
            }
            return InputRotation;
        }

        public void SetGravity(bool NewGravity)
        {
            IsGravity = NewGravity;
        }
        #endregion

        #region AnimationController

        /// <summary>
        /// Update the animator
        /// </summary>
        private void ModifyAnimator()
        {
            if (MyRigidbody && MyAnimator)
            {
                //float Smallest = 0.05f;
                if (MovementX != 0 || MovementZ != 0)
                //if (!(MyRigidbody.velocity.z >= -Smallest && MyRigidbody.velocity.z <= Smallest
                //    && MyRigidbody.velocity.x >= -Smallest && MyRigidbody.velocity.x <= Smallest))
                {
                    MyAnimator.SetSpeed(AnimationWalkSpeed * (Mathf.Abs(MyRigidbody.velocity.x) + Mathf.Abs(MyRigidbody.velocity.z)));
                    if (IsWalking)
                    {
                        if (Time.time - TimeLastChangedState > TimeToChange && MyAnimator.IsPlaying(0))
                        {
                            //Debug.Log("Velocity: " + GetCurrentVelocity().ToString());
                            MyAnimator.Play(2); // walk animation
                        }
                    }
                    else
                    {
                        IsWalking = true;
                        TimeLastChangedState = Time.time;
                    }
                }
                else
                {
                    if (IsWalking == false)
                    {
                        if (Time.time - TimeLastChangedState > TimeToChange && MyAnimator.IsPlaying(2))
                        {
                            TimeLastChangedState = Time.time;
                            MyAnimator.Play(0); // idle animation
                            MyAnimator.SetSpeed(2.2f);
                        }
                    }
                    else
                    {
                        IsWalking = false;
                        TimeLastChangedState = Time.time;
                    }
                }
            }
            else
            {
                if (MyRigidbody == null)
                {
                    Debug.LogError("No rigidbody on controller.");
                }
            }
        }
        #endregion
    }
}