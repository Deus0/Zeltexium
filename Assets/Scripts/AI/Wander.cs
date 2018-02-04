using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Characters;
using Zeltex.Voxels;
using Zeltex.Combat;

/*	Class: Wander
 * 	Purpose: Every x seconds, it will chose a new wander target, based on its current angle, with a random addition to its angle
 *  
 * */
namespace Zeltex.AI
{
    /// <summary>
    /// Makes the bot wander around randomly
    /// </summary>
    [System.Serializable]
	public class Wander : BotBehaviour
    {
        #region Variables
        private bool IsWandering;
        [Header("Options")]
		public float WanderCooldown = 8f;
		public float WanderRange = 2.5f;
		public float WanderRotateVariation = 1f;
        public int WanderChecks = 50;
        public LayerMask WanderLayer;
        //bool IsWandering = true;
        //public bool IsRandomJumping = true;
		[Header("Limiting Wander")]
		public bool IsLimitWander = false;
		public Vector3 WanderInitialPosition;
		public Vector3 WanderSize = new Vector3 (6, 6, 6);
		// references / internal states
		private float WanderLastTime = 0f;
        private Vector3 WanderPosition = new Vector3();
        private float LastAttackCheck = 0;  // check for attacking
        //private Character MyCharacter = null;
        [Header("Attacking")]
        public float CheckForAttackCooldown = 0.5f;
        public float AttackCheckRange = 5f;
        // Components
        private Bot MyBot;
        private float LastJumpTime;
        private bool DebugPosition = true;
        private Mover MyMover;
        private float WanderIncreasePerCheck = 2f;

        //private bool IsWanderTimeUp;
        #endregion

        public override void Initiate(Transform MyTransform)
        {
            base.Initiate(MyTransform);
            Name = "Wander";
            MyBot = MyTransform.GetComponent<Bot>();
            MyMover = MyTransform.GetComponent<Mover>();
        }
        /// <summary>
        /// When entering the wander state
        /// </summary>
        public override void Begin()
        {
            IsWandering = false;
            WanderLastTime = Time.time - WanderCooldown;
            //Debug.Log(BotTransform.name + " has started to wander " + WanderLastTime);
        }

        // Update is called once per frame
        public override void Update(Bot TargetBot)
		{
            if (MyBot == null)
            {
                MyBot = TargetBot;
                BotTransform = TargetBot.transform;
                MyMover = TargetBot.GetComponent<Mover>();
            }
            if (IsWandering == false)
            {
                WanderAround();
            }
            else
            {
                // begin again if time is pass a certain amount
                if (IsWanderTimeUp())
                {
                    // finish early
                    IsWandering = false;
                }
            }
            CheckForAttack();
            if (DebugPosition)
            {
                Debug.DrawLine(WanderPosition, WanderPosition + new Vector3(0, 3, 0), Color.green);
            }
        }

        /*private World GetWorld()
        {
            if (MyCharacter == null)
            {
                MyCharacter = GetComponent<Character>();
            }
            return MyCharacter.GetWorldInsideOf();
        }*/

       /* private void EnableWander()
        {
            WanderLastTime = Time.time;
            CanWander = true;
        }*/

        private bool IsWanderTimeUp()
        {
            float TimePassed = Time.time - WanderLastTime;
            bool IsWanderUp = (TimePassed >= WanderCooldown && MyBot.IsAttacking() == false);
            if (IsWanderUp)
            {
                WanderLastTime = Time.time;
            }
            return IsWanderUp;
        }
        /// <summary>
        /// Main wander update method
        /// </summary>
        private void WanderAround()
        {
            if (IsWanderTimeUp())
            {
                IsWandering = true;
                WanderPosition = GetNewWanderPosition();
                MyBot.MoveToPositionRaw(WanderPosition);
                MyBot.OnReachTarget.RemoveAllEvents();
                MyBot.OnReachTarget.AddEvent(Begin);
            }
        }

        private void RandomJump()
        {
            /*if (IsRandomJumping)
            {
                float IsJump = Random.Range(1, 100);
                if (IsJump > 90)
                {
                    GetComponent<BasicController>().Jump();
                }
            }*/
        }

        /// <summary>
        /// Finds a new wander position in the map
        /// </summary>
        public Vector3 GetNewWanderPosition()
        {
            float Randomness;
            //Vector3 WanderDirection = new Vector3(WanderRange * Mathf.Cos(Randomness), 0f, WanderRange * Mathf.Sin(Randomness));
            Vector3 WanderDirection;

            Vector3 WanderTheta;
            RaycastHit MyHit;
            Ray MyRay;
            //World MyWorld = WorldManager.Get().MyWorlds[0];
            //RaycastHit MyHit;
            for (int i = 0; i < WanderChecks; i++)    // 20 ray checks to get wander position
            {
                Randomness = Random.Range(-WanderRotateVariation - i * WanderIncreasePerCheck, WanderRotateVariation + i * WanderIncreasePerCheck);     // Randomly change wander theta
                WanderDirection = Mathf.Sin(Randomness) * BotTransform.right + Mathf.Cos(Randomness) * BotTransform.forward;// + Mathf.Sin(Randomness) * BotTransform.up;      // a circle around the user, where randomness is the point along the circle
                WanderTheta = BotTransform.position + WanderDirection * WanderRange;
                if (IsLimitWander)
                {
                    WanderTheta.x = Mathf.Clamp(WanderTheta.x, WanderInitialPosition.x - WanderSize.x, WanderInitialPosition.x + WanderSize.x);
                    WanderTheta.y = Mathf.Clamp(WanderTheta.y, WanderInitialPosition.y - WanderSize.y, WanderInitialPosition.y + WanderSize.y);
                    WanderTheta.z = Mathf.Clamp(WanderTheta.z, WanderInitialPosition.z - WanderSize.z, WanderInitialPosition.z + WanderSize.z);
                }
                //Int3 WanderPositionInt = new Int3(MyWorld.RealToBlockPosition(WanderTheta));
                //Vector3 NewWanderPosition = MyWorld.BlockToRealPosition(WanderPositionInt.GetVector()) + MyWorld.GetUnit() / 2f;

                //Voxel VoxelWander = MyWorld.GetVoxel(WanderPositionInt);
                //Voxel VoxelWanderBelow = MyWorld.GetVoxel(WanderPositionInt.Below());

                /*if (VoxelWander != null && VoxelWanderBelow != null && VoxelWander.GetVoxelType() == 0 && VoxelWanderBelow.GetVoxelType() != 0)
                {
                    return NewWanderPosition;
                }*/
                Vector3 Normal = (WanderTheta - MyBot.transform.position).normalized;
                Normal.y = 0;
                MyRay = new Ray(MyBot.transform.position, Normal);
                if (UnityEngine.Physics.Raycast(MyRay, out MyHit, WanderRange, MyMover.GroundLayer))
                {
                    //Debug.LogError(i + " ~ Hitting " + MyHit.point.ToString());
                }
                else
                {
                    //Debug.LogError(i + " ~ Not Hitting " + WanderTheta.ToString());
                    return WanderTheta;
                }
            }
            return BotTransform.position;
        }

        /// <summary>
        /// Quick check for attacking
        /// </summary>
        void CheckForAttack()
        {
            if (MyBot != null && MyBot.GetData() != null
                && MyBot.GetData().IsAggressive && MyBot.IsAttacking() == false && Time.time - LastAttackCheck >= CheckForAttackCooldown)
            {
                LastAttackCheck = Time.time;
                //Debug.Log(BotTransform.name + " is checking for an attack.");
                bool IsAttack = false;
                List<Character> MyCharacters = CharacterManager.Get().GetSpawned();
                if (MyCharacters.Count > 0)
                {
                    Character TargetCharacter = MyCharacters[Random.Range(0, MyCharacters.Count - 1)];
                    if (TargetCharacter != null && TargetCharacter.gameObject != BotTransform.gameObject)
                    {
                        if (Vector3.Distance(BotTransform.position, TargetCharacter.transform.position) < AttackCheckRange)
                        {
                            if (TargetCharacter.IsAlive())
                            {
                                IsAttack = true;
                            }
                        }
                    }
                    if (IsAttack)
                    {
                        MyBot.AttackTarget(TargetCharacter);
                    }
                }
            }
        }
    }
}

/*if (!Physics.Raycast(transform.position, WanderDirection, out MyHit, WanderRange, WanderLayer))
{
    return WanderTheta;
}*/
/*else
{
    GetComponent<BasicController>().Jump();
}*/
/*if (Physics.Raycast(transform.position, WanderDirection, out MyHit, WanderRange))
{
        Debug.DrawLine(transform.position, MyHit.point, Color.red, WanderCooldown);
    MyMovement.MoveToPosition(MyHit.point - WanderDirection * 0.05f);
}
else
{
    if (DebugPosition)
        Debug.DrawLine(transform.position, transform.position + WanderDirection * WanderRange, Color.green, WanderCooldown);
    MyMovement.MoveToPosition(WanderTheta);
}*/
