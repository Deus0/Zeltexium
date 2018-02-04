using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Zeltex.Characters;
using Zeltex.Combat;
using Zeltex.Voxels;
using Zeltex.Guis;
using Zeltex.Skeletons;

namespace Zeltex.AI
{
    /// <summary>
    /// Moves bot to a new position.
    /// movement will start slowing down like breaking, which reduces the curving in the pathed movement
    /// To DO:
    ///     - Need to move the attacking content into a new class BotAttacker
    ///     - Give different types of movements
    ///     - Reimplement flocking
    /// </summary>
    public partial class Bot : MonoBehaviour 
	{
        #region Variables
        private Rigidbody MyRigidbody;
		[Header("Debug")]
        [SerializeField]
        private bool DebugPosition = true;
        [SerializeField]
        private bool DebugVelocity = false;
        [SerializeField]
        private Color32 DebugVelocityColor = Color.green;
        [SerializeField]
        private Color32 DebugForceColor = Color.blue;
        [SerializeField]
        private Color32 DebugPositionColor = Color.yellow;
        [SerializeField]
        private Color32 DebugMoveToPositionColor = Color.cyan;
        [SerializeField]
        private Color32 DebugAttackColor = Color.red;
        [SerializeField]
        private bool IsDebugGui;

        [Header("Movement")]
        [SerializeField]
        private int TargetPositionIndex = 0;
        [SerializeField]
        private List<Vector3> TargetPositions = new List<Vector3>();
        [SerializeField]
        private Vector3 LookAtPosition;
        [Header("Positioning")]
        [SerializeField]
        private GameObject TargetObject;
        private float DistanceToTarget;
        private Vector3 Thresh = new Vector3(0.5f, 1f, 0.5f);
        private float ThreshObject = 2f;
        [Header("Forces")]
        //[SerializeField] private float Speed = 1f;	// slightly faster then people lol
		//[SerializeField] private float TurnSpeed = 3f;
        [SerializeField]
        private Vector3 MovementForce = new Vector3();
        //[SerializeField] private Vector3 LimitMovementForce = new Vector3(1,1,1);
        [SerializeField]
        private Vector3 TargetRotation = new Vector3();
        //[SerializeField] private float MaxRotation = 5;
        private float MinimumForce = 0.15f;
        private float SlowDownDistance = 2f;
        [Header("States")]
        [SerializeField]
        private bool IsMoveTo = false;
        //[SerializeField]
        //private bool CanAttack;
        //[SerializeField]
        //private bool CanRam = false;
        [SerializeField]
        private bool IsSlowDown = true;
        //private bool IsJump;
        private bool IsLookTowards = false;
        [Header("Events")]
        public UnityEvent OnReachTarget = new UnityEvent();
        //private float DesiredDirectionOffset = 0;
        // Icon
        [SerializeField]
        private List<GameObject> PositionIcons = new List<GameObject>();
        private float SinAddition = 0;
        private float SinSpeed = 0;
        // Path Finding
        private BotPathFinder PathFinder = new BotPathFinder();
        private World MyWorld;
        private float LastRefreshedPath;
        private Int3 LastObjectPosition;
        [Header("Materials")]
        [SerializeField]
        private Material TargetPositionMaterial;
        [Header("Materials")]
        [SerializeField]
        private Material TargetObjectMaterial;
        private Character MyCharacter;
       // private BasicController MyController;
        //private Mover MyController;
        private SkeletonHandler MySkeleton;
        // a little bit of whiskers
        private bool IsWhiskers = true;
        public float RotationSpeed = 360f;
        //private float SlowRotationDistance = 1f;
        private bool IsMovementDisplayed = false;
        private Transform WaypointParent;
        #endregion

        #region Mono

        public void RefreshRigidbody()
        {
            MyRigidbody = GetComponent<Rigidbody>();
        }

        private void OnGUI()
        {
            if (IsDebugGui)
            {
                GUILayout.Label("Target: " + (TargetPositionIndex + 1) + " out of " + TargetPositions.Count);
                //GUILayout.Label("State: " + MyBot.MyMovementState.ToString());
                //GUILayout.Label("Attacking: " + CanAttack.ToString());
            }
        }

        /// <summary>
        /// Called every Frame
        /// Finds a target and steers towards it
        /// It also attacks things
        /// </summary>
        private void UpdateMovement()
        {
            if (MyWorld == null)
            {
				if (MyCharacter)
				{
                    MyWorld = MyCharacter.GetInWorld();
				}
				else
				{
					Debug.LogError("Bot [" + name + "] does not have a character component.");
				}
            }
            if (MyRigidbody)
            {
                FindTarget();
                if (DebugVelocity)
                {
                    Debug.DrawLine(transform.position, transform.position + (GetMovementForce()), DebugForceColor);
                    Debug.DrawLine(transform.position, transform.position + (MyRigidbody.velocity), DebugVelocityColor);
                }
                if (DebugPosition && IsMoveTo)
                {
                    if (Attacking)
                    {
                        Debug.DrawLine(GetTargetPosition(), GetTargetPosition() + new Vector3(0, 1, 0), DebugAttackColor);
                    }
                    else
                    {
                        Debug.DrawLine(GetTargetPosition(), GetTargetPosition() + new Vector3(0, 1, 0), DebugPositionColor);
                    }
                }
                if (DebugPosition)
                {
                    Debug.DrawLine(transform.position, GetTargetPosition(), DebugMoveToPositionColor);
                    //Debug.DrawLine(transform.position, transform.position + transform.forward, DebugForwardDirection);
                }
                //MovementUpdate();
            }
            //if (GetTargetPosition().y - 0.2f > transform.position.y)
            {
                //IsJump = true;
            }
            //else
            {
                //IsJump = false;
            }
            RefreshPathTargetObject();
            UpdatePositionIcon();
        }

        /// <summary>
        /// Clear all the movement data
        /// </summary>
        public void Clear()
        {
            //Debug.LogError("Clearing for: " + name);
            ClearPositionIcons();
            TargetPositionIndex = 0;
            TargetPositions.Clear();
            IsMoveTo = false;
            Attacking = false;
            TargetObject = null;
        }
        #endregion

        #region PositionIcons

        /// <summary>
        /// Every frame animate the target icons
        /// </summary>
        private void UpdatePositionIcon()
        {
            if (IsMovementDisplayed)
            {
                if (PositionIcons.Count != TargetPositions.Count)
                {
                    Debug.LogError("Positions noot equal to icons: " + TargetPositions.Count + ":" + PositionIcons.Count);
                    CreatePositionIcons();
                    Debug.LogError("Positions now equal to icons: " + TargetPositions.Count + ":" + PositionIcons.Count);
                }
                /*if (TargetObject)
                {
                    Bounds TargetBounds = TargetObject.transform.GetChild(0).gameObject.GetComponent<Skeleton>().GetBounds();
                    PositionIcons[i].transform.position = TargetObject.transform.position + new Vector3(0, TargetBounds.extents.y + TargetBounds.center.y + 0.2f, 0);
                }*/
                if (TargetObject)
                {
                    if (TargetPositions.Count != 0)
                    {
                        TargetPositions[TargetPositions.Count - 1] = TargetObject.transform.position;
                    }
                }
                for (int i = 0; i < PositionIcons.Count; i++)
                {
                    PositionIcons[i].transform.localScale = (0.10f + 0.02f * Mathf.Sin(SinSpeed * Time.time + SinAddition)) * (new Vector3(1, 1, 1));
                    PositionIcons[i].transform.position = TargetPositions[i] + 0.02f * (new Vector3(0, Mathf.Sin(SinSpeed * Time.time - SinAddition), 0));
                    if (i != 0)
                    {
                        PositionIcons[i].GetComponent<LineRenderer>().startWidth = 0.1f + 0.04f * Mathf.Sin(SinSpeed * Time.time + SinAddition);
                        PositionIcons[i].GetComponent<LineRenderer>().endWidth = 0.1f + 0.04f * Mathf.Sin(SinSpeed * Time.time + SinAddition);
                        PositionIcons[i].GetComponent<LineRenderer>().SetPosition(0, TargetPositions[i - 1] + 0.02f * (new Vector3(0, Mathf.Sin(SinSpeed * Time.time - SinAddition), 0)));
                        PositionIcons[i].GetComponent<LineRenderer>().SetPosition(1, TargetPositions[i] + 0.02f * (new Vector3(0, Mathf.Sin(SinSpeed * Time.time - SinAddition), 0)));
                    }
                }
            }
        }

        /// <summary>
        /// Creates all the position icons
        /// </summary>
        private void CreatePositionIcons()
        {
            if (IsMovementDisplayed)
            {
                if (WaypointParent == null)
                {
                    WaypointParent = GameObject.Find("WayPoints").transform;
                }
                ClearPositionIcons();
                Transform WayPoints = WaypointParent.transform;
                for (int i = 0; i < TargetPositions.Count; i++)
                {
                    GameObject PositionIcon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    PositionIcon.GetComponent<BoxCollider>().Die();
                    PositionIcon.layer = LayerManager.Get().GetWaypointLayer();
                    PositionIcon.transform.SetParent(WayPoints);
                    PositionIcon.name = (i + 1) + ": " + gameObject.name + " [" + TargetPositions[i].x + ", " + TargetPositions[i].y + ", " + TargetPositions[i].z + "]";
                    PositionIcon.transform.localScale = 0.1f * (new Vector3(1, 1, 1));
                    PositionIcons.Add(PositionIcon);
                    if (i != 0)
                    {
                        LineRenderer MyLineRenderer = PositionIcon.AddComponent<LineRenderer>();
                        MyLineRenderer.startWidth = 0.1f;
                        MyLineRenderer.endWidth = 0.1f;
                        MyLineRenderer.SetPosition(0, TargetPositions[i - 1]);
                        MyLineRenderer.SetPosition(1, TargetPositions[i]);
                    }
                }
                LoadTargetPositionMaterial();
            }
        }

        /// <summary>
        /// Clears our position icons
        /// </summary>
        private void ClearPositionIcons()
        {
            for (int i = 0; i < PositionIcons.Count; i++)
            {
                PositionIcons[i].Die();
            }
            PositionIcons.Clear();
        }

        private void SetPositionIconsMaterial(Material MyMaterial)
        {
            for (int i = 0; i < PositionIcons.Count; i++)
            {
                PositionIcons[i].GetComponent<MeshRenderer>().sharedMaterial = MyMaterial;
                if (PositionIcons[i].GetComponent<LineRenderer>())
                {
                    PositionIcons[i].GetComponent<LineRenderer>().sharedMaterial = MyMaterial;
                }
            }
        }

        private void LoadTargetPositionMaterial()
        {
            SetPositionIconsMaterial(TargetPositionMaterial);
        }

        private void LoadTargetObjectMaterial()
        {
            SetPositionIconsMaterial(TargetObjectMaterial);
        }
        #endregion

		#region TargetPosition

		/// <summary>
		/// Refreshes the path if the bot has moved
		/// </summary>
		private System.Collections.IEnumerator RefreshPathTargetObject()
		{
			if (TargetObject != null)
			{
				if (Time.time - LastRefreshedPath >= 0.5f)
				{
					LastRefreshedPath = Time.time;
					if (MyWorld)
					{
						Int3 CurrentObjectPosition = new Int3(MyWorld.RealToBlockPosition(TargetObject.transform.position));
						if (LastObjectPosition != CurrentObjectPosition)
						{
							LastObjectPosition = CurrentObjectPosition;
							Vector3 TargetWorldPosition = MyWorld.BlockToRealPosition(LastObjectPosition.GetVector()) + MyWorld.GetUnit() / 2f;
							yield return StartCoroutine(RefreshPath(TargetWorldPosition));
							LoadTargetObjectMaterial();
						}
					}
				}
			}
		}

		private System.Collections.IEnumerator RefreshPath(Vector3 TargetPosition)
        {
            TargetPositions.Clear();
            TargetPositionIndex = 0;
            if (MyWorld != null)
            {
				yield return StartCoroutine(PathFinder.FindPath(MyWorld, transform.position, 
					TargetPosition, 
					MyCharacter.GetData().MySkeleton.GetBounds()));
				TargetPositions = PathFinder.MyPathWorld;
            }
            else
            {
                TargetPositions.Clear();
                TargetPositions.Add(TargetPosition);
				yield return null;
            }
			CreatePositionIcons();
			LoadTargetPositionMaterial();
        }

        /// <summary>
        /// Walk towards a target position
        /// </summary>
        void WalkTowards(Vector3 Position)
        {
            RotateTowards(Position);
            MoveForwards(Position);
        }

        /// <summary>
        /// Sets the bot to move to a position
        /// </summary>
        public void MoveToPosition(Vector3 TargetPosition)
        {
            StopFollowing();
            IsMoveTo = true;
            IsLookTowards = false;
            LookAtPosition = TargetPosition;
			StartCoroutine(RefreshPath(TargetPosition));
        }

        public void MoveToPositionRaw(Vector3 TargetPosition)
        {
            StopFollowing();
            IsMoveTo = true;
            IsLookTowards = false;
            LookAtPosition = TargetPosition;
            TargetPositions.Clear();
            TargetPositionIndex = 0;
            if (TargetPositions.Count == 0)
            {
                TargetPositions = new List<Vector3>();
            }
            TargetPositions.Clear();
            TargetPositions.Add(TargetPosition);
            CreatePositionIcons();
            LoadTargetPositionMaterial();
        }
        /// <summary>
        /// Sets the bot to look at a position
        /// </summary>
		public void LookAt(Vector3 NewPosition)
        {
            StopFollowing();
            IsMoveTo = false;
            IsLookTowards = true;
            LookAtPosition = NewPosition;
        }

        private Vector3 GetTargetLookAtPosition()
        {
            if (TargetObject != null)
            {
                return TargetObject.transform.position;
            }
            else
            {
                return LookAtPosition;
            }
        }
        #endregion

        #region TargetObject

        /// <summary>
        /// Follow a target - friendly
        /// </summary>
        public void FollowTarget(GameObject NewTarget)
        {
            TargetPositions.Clear();
            Attacking = false;
            TargetObject = NewTarget;
            IsMoveTo = true;
            IsLookTowards = false;
			LastRefreshedPath = Time.time;
			if (MyWorld)
			{
				LastObjectPosition = new Int3(MyWorld.RealToBlockPosition(TargetObject.transform.position));
				RefreshPath(MyWorld.BlockToRealPosition(LastObjectPosition.GetVector()) + MyWorld.GetUnit() / 2f);
			}
			LoadTargetObjectMaterial();
        }
        /// <summary>
        /// Look at an object
        /// </summary>
        public void LookAt(GameObject NewTarget)
        {
            IsMoveTo = false;
            IsLookTowards = true;
            TargetObject = NewTarget;
        }
        /// <summary>
        /// Stop following behaviour
        /// </summary>
        public void StopFollowing()
        {
            TargetObject = null;
            IsMoveTo = false;
            if (gameObject.GetComponent<Shooter>())
            {
                gameObject.GetComponent<Shooter>().StopAiming();
            }
        }
        /// <summary>
        /// Is facing an object
        /// </summary>
        public bool IsFacingObject(Transform OtherObject, float AngleThreshold)
        {
            if (MyCharacter.GetSkeleton().GetSkeleton().GetCameraBone() != null)
            {
                return (Vector3.Angle(MyCharacter.GetForwardDirection(), OtherObject.transform.position - transform.position) < AngleThreshold);
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region Attacking
        private Skillbar MySkillbar;

        /// <summary>
        /// Attacks the character
        /// </summary>
        void Attack()
		{
            // aim head bone up down at target
            if (MyCharacter.GetSkeleton().GetSkeleton().GetCameraBone())
            {
                MyCharacter.GetSkeleton().GetSkeleton().GetCameraBone().LookAt(TargetObject.transform);  // should be max though direction!
                // head bone should follow camera bone!
            }
            //Debug.Log(name + " is trying to attack!");
            if (DistanceToTarget < 8f)
            {
                if (IsFacingObject(TargetObject.transform, 60f))  // if distance within spell range 	- and line of sight range - ie some will be aoe spells
                {
                    if (MyCharacter && MyCharacter.GetSkillbar())
                    {
                        MySkillbar = MyCharacter.GetSkillbar();
                        Debug.Log(name + " is attacking!");
                        if (TargetObject.GetComponent<Character>().IsAlive())
                        {
                            MySkillbar.SwitchToAttackSpell();  // if not already switched
                            if (gameObject.GetComponent<Shooter>())
                            {
                                gameObject.GetComponent<Shooter>().Activate();
                            }
                            else
                            {
                                //Debug.LogError("Skillbar size - with " + MySkillbar.GetSelectedIndex());
                                //Debug.LogError("Skill [" + MySkillbar.GetSelectedItem().Name + "] has no shooter"); // run away!
                            }
                        }
                        else
                        {
                            StopAttacking();
                        }
                    }
                    else
                    {
                        Debug.LogError(name + " has no skillbar");
                    }
                }
            }
            else
            {
                // if distance less then 8f, stop following
                StopAttacking();
            }
        }

		private void StopAttacking()
		{
            Attacking = false;
			TargetObject = null;
			ClearPositionIcons();
			TargetPositions.Clear();
			TargetPositionIndex = 0;
            if (MyCharacter.GetSkeleton().GetSkeleton().GetCameraBone())
            {
                MyCharacter.GetSkeleton().GetSkeleton().GetCameraBone().transform.localEulerAngles = Vector3.zero;
            }
            //OnReachTarget.Invoke(); // finish doing what it was meant to after wandering?
            GetComponent<Bot>().Wander();
        }

        /// <summary>
        /// Follow a target - and kill it
        /// </summary>
        public void AttackTarget(Character NewTarget)
        {
            if (NewTarget)
            {
                FollowTarget(NewTarget.gameObject);
                Attacking = true;
                //MyBot.ChangeState(MovementState.Attacking);
            }
        }

        /// <summary>
        /// checks if bot is attacking
        /// </summary>
        public bool IsAttacking()
        {
            if (TargetObject == null)
            {
                Attacking = false;
            }
            return Attacking;
        }
        #endregion

        #region Movement
        private Vector2 MyInput = Vector2.zero;
        private Vector2 RotationInput = Vector2.zero;

        public Vector2 GetInput()
        {
            MyInput.Set(GetMovementForce().x, GetMovementForce().z);
            return MyInput;
        }

        public Vector3 GetTargetRotation()
        {
            return TargetRotation;
        }

        public Vector2 GetRotationInput()
        {
            Vector3 BeforeRotation = transform.eulerAngles;
            transform.LookAt(GetTargetPosition());
            TargetRotation = transform.eulerAngles;
            transform.eulerAngles = BeforeRotation;
            RotationInput.Set(TargetRotation.normalized.x, TargetRotation.normalized.y);
            return RotationInput;
        }

        /// <summary>
        /// Gets the current movement force being applied to the bot
        /// </summary>
		public Vector3 GetMovementForce() 
		{
            /*MovementForce.x = Mathf.Clamp(MovementForce.x, -LimitMovementForce.x, LimitMovementForce.x);
            MovementForce.y = Mathf.Clamp(MovementForce.y, -LimitMovementForce.y, LimitMovementForce.y);
            MovementForce.z = Mathf.Clamp(MovementForce.z, -LimitMovementForce.z, LimitMovementForce.z);*/
            return ((MovementForce).normalized);//);    //transform.TransformVector
        }
        /// <summary>
        /// Stops the movement instantly
        /// </summary>
		public void StopMovement()
		{
			MovementForce.x = 0;
			MovementForce.z = 0;
            TargetRotation = transform.eulerAngles;
		}
        #endregion

        // The desired velocity, and the acceleration towards that
        #region Steering
        /// <summary>
        /// Steers towards a desired position!
        /// </summary>
        void MoveForwards(Vector3 Position)
        {
            Vector3 MyVelocity = GetComponent<Rigidbody>().velocity;
            //Vector3 MyVelocity = gameObject.GetComponent<Rigidbody>().velocity;
            float DistanceToTarget = Vector3.Distance(Position, transform.position);

            //Vector3 DesiredDirection = (Position - transform.position).normalized;
            //DesiredDirectionOffset = Vector3.Angle(DesiredDirection, transform.forward);

            float DesiredForce = 1f;
            if (IsSlowDown)
            {
                // the closer i am to target, the slower i go
                if (DistanceToTarget < SlowDownDistance)
                {
                    //MyController.SetRotationSpeed(12f);
                    if (Attacking)
                    {
                        DesiredForce *= SlowDownDistance / DistanceToTarget;
                    }
                    else
                    {
                        DesiredForce *= (DistanceToTarget) / SlowDownDistance;
                    }
                }
                else
                {
                    //MyController.SetRotationSpeed(3f);
                }
            }

            float SteerForce = (DesiredForce - MyVelocity.z);
            // Otherwise it gets stuck from the drag of the rigidbody being greater then its movement force
            if (SteerForce < MinimumForce)
            {
                SteerForce = MinimumForce;
            }
            MovementForce.z = SteerForce;
            Whiskers();
        }

        private void Whiskers()
        {
            if (IsWhiskers)
            {
                RaycastHit MyHit;
                MovementForce.x = 0;
                Bounds MyBounds = MySkeleton.GetSkeleton().GetBounds();
                float BoundsX = (MyBounds.center.x + MyBounds.extents.x) * 1.03f;
                //float DesiredForceX = 0;
                if (UnityEngine.Physics.Raycast(transform.position, transform.right, out MyHit, BoundsX, LayerManager.Get().WorldsLayer))
                {
                    //Debug.Log(name + " hit " + MyHit.collider.name);
                    MovementForce.x -= 1f;
                }
                if (UnityEngine.Physics.Raycast(transform.position, -transform.right, out MyHit, BoundsX, LayerManager.Get().WorldsLayer))
                {
                    //Debug.Log(name + " hit " + MyHit.collider.name);
                    MovementForce.x += 1f;
                }
                if (UnityEngine.Physics.Raycast(transform.position, transform.forward, out MyHit, (MyBounds.center.z + MyBounds.extents.z) * 1.03f, LayerManager.Get().WorldsLayer))
                {
                    MovementForce.z *= -1;
                }
                //float SteerForceX = (DesiredForceX - MyVelocity.x);
                //MovementForce.x = SteerForce;
            }
        }

        /// <summary>
        ///  Oh! This isnt strafe movement, but rotational speed
        /// </summary>
        private void RotateTowards(Vector3 Position)
        {
            //Vector3 TargetDirection = (transform.position - Position).normalized;
            Quaternion BeforeRotation = transform.rotation;
            transform.LookAt(Position);
            TargetRotation = transform.eulerAngles;
            transform.rotation = BeforeRotation;

            /*if (DistanceToTarget <= SlowRotationDistance)
            {
                TargetRotation.x = Mathf.MoveTowardsAngle(transform.eulerAngles.x, TargetRotation.x, Time.deltaTime * RotationSpeed / 2f);
                TargetRotation.y = Mathf.MoveTowardsAngle(transform.eulerAngles.y, TargetRotation.y, Time.deltaTime * RotationSpeed / 2f);
            }
            else
            {
                TargetRotation.x = Mathf.MoveTowardsAngle(transform.eulerAngles.x, TargetRotation.x, Time.deltaTime * RotationSpeed);
                TargetRotation.y = Mathf.MoveTowardsAngle(transform.eulerAngles.y, TargetRotation.y, Time.deltaTime * RotationSpeed);
            }*/
        }
        #endregion

        #region Pathing

        /// <summary>
        /// Gets the next position in the list
        /// </summary>
        Vector3 GetTargetPosition()
        {
            if (TargetObject)
            {
                return TargetObject.transform.position;
            }
            else if (TargetPositions.Count > 0 && TargetPositionIndex < TargetPositions.Count)
            {
                return TargetPositions[TargetPositionIndex];
            }
            else
            {
                return transform.position;
            }
        }
        
        private bool IsFinalTarget()
        {
            return (TargetPositionIndex == TargetPositions.Count - 1);
        }

        /// <summary>
        /// Invoked when the character has reached a target
        /// </summary>
        private void OnMovedToTarget()
        {
            // reached final target!
            if (TargetPositionIndex == TargetPositions.Count - 1 || TargetPositions.Count == 0)
            {
                //Debug.Log(name + " has reached final target at " + Time.time);
                IsMoveTo = false;
                if (TargetObject == null && Attacking == false)
                {
                    OnReachTarget.Invoke();
                }
            }
            else
            {
                TargetPositionIndex++;
            }
        }
        #endregion

        // AI positioning towards its targets
        #region positioning
        
        /// <summary>
        /// Sets the position to walk to
        /// </summary>
        void FindTarget()
		{
			if (MyRigidbody)
            {
                if (TargetObject != null && Attacking && !TargetObject.GetComponent<Character>().IsAlive())
                {
                    StopAttacking();
                }
                // move to target
                else if (TargetObject)
				{
					DistanceToTarget = Vector3.Distance (transform.position, GetTargetPosition());
					if (DistanceToTarget > ThreshObject)// || (CanAttack && CanRam))
					{
						WalkTowards(GetTargetPosition());
					}
					else
					{
                        // if close to target, just rotate torwards
						//StopMovement();
						RotateTowards(GetTargetPosition());
                        OnMovedToTarget();
                    }
                    // while moving/strafing/rotating towards, attack if a target
                    if (Attacking)
                    {
                        Attack();
                    }
                }
                // move to a position
                else if (IsMoveTo) 
				{
                    Vector3 TargetPosition = GetTargetPosition();
                    Vector3 DistanceToTarget = new Vector3(Mathf.Abs(transform.position.x - TargetPosition.x),
                        Mathf.Abs(transform.position.y - TargetPosition.y), Mathf.Abs(transform.position.z - TargetPosition.z));
                    bool HasReachedTarget = DistanceToTarget.x < Thresh.x && DistanceToTarget.y < Thresh.y && DistanceToTarget.z < Thresh.z;
                    //float DistanceToTarget = Vector3.Distance (transform.position, );
                    if (HasReachedTarget)
                    {
                        OnMovedToTarget();
					}
					else
                    {
                        WalkTowards(TargetPosition);
                    }
				}
				// Look towards a position / target object
				else if (IsLookTowards)
                {
                    RotateTowards(GetTargetLookAtPosition());
				}
				else
				{
					// Slow Down
					StopMovement();
				}
			}
		}

        #endregion
    }
}

/*Vector3 MyForwardDirection = transform.TransformDirection(Vector3.forward);
float ForwardDot = Vector3.Dot(MyForwardDirection, TargetDirection);
Vector3 MyRightDirection = transform.TransformDirection(Vector3.left);
float RightDot = Vector3.Dot(MyRightDirection, TargetDirection);
float TotalDot = Mathf.Abs(ForwardDot) + Mathf.Abs(RightDot);
if (transform.rotation.eulerAngles.normalized.y < TargetDirection.normalized.y)
{
    RotationalForce.y = (RightDot / TotalDot);
}
else if (transform.rotation.eulerAngles.normalized.y > TargetDirection.normalized.y)
{
    RotationalForce.y = - (RightDot / TotalDot);
}*/
//RotationalForce.y = Mathf.Clamp(RotationalForce.y, -MaxRotation, MaxRotation);
//float ThisThresh = Thresh;  // normal thresh
/*if (IsFinalTarget() == false)
{
    ThisThresh *= 4;    // 4 times! if not final target!
}*/
