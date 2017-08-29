using UnityEngine;
using System.Collections.Generic;
using Zeltex.Util;
using Zeltex.Characters;

/// <summary>
/// A system of classes designed for movements.
/// </summary>
namespace Zeltex.AI
{
    /// <summary>
    /// Ai Settings for a bot
    /// </summary>
    [System.Serializable]
    public class BotMeta : Element
    {
        public bool IsAggressive = true;
        public bool CanWander = true;


    }
    /// <summary>
    /// The state at which the bot is moving at.
    /// Can only be doing one thing at a time.
    /// </summary>
	public enum MovementState
    {
		Waiting,
        MoveToPosition,
		Following,
        Watching,
        Wander,
		Flee,
        Attacking,
        Speaking,
        Other,
        Paused
    };

    /// <summary>
    /// Attached to character class.
    /// Controls all the movement ai scripts.
    /// To Do:
    ///     - Save a history of states
    ///     - Evolve further decisions based on this memory
    /// </summary>
    [ExecuteInEditMode]
	public partial class Bot : MonoBehaviour
	{
        public BotMeta Data;
        // StateData
        public bool Attacking;
        // Components
        [SerializeField]
        public BotBehaviour CurrentBehaviour;
        [SerializeField]
        public List<BotBehaviour> MyBehaviours = new List<BotBehaviour>();
        [SerializeField]
        public Wander MyWander;
        private float TimeStartedWaiting;

        void Start()
        {
            //if (MyBehaviours.Count == 0)
            {
            }
            MyBehaviours.Clear();
            MyBehaviours.Add((new Wander()) as BotBehaviour);
            MyWander = MyBehaviours[0] as Wander;
            MyCharacter = GetComponent<Character>();
            MySkeleton = MyCharacter.GetSkeleton();
            RefreshRigidbody();
            // The beginning state is paused
            for (int i = 0; i < MyBehaviours.Count; i++)
            {
                MyBehaviours[i].Initiate(transform);
            }
            BeginBehaviour("Wander");
            SinAddition = Random.Range(0.0f, 1.0f);
            SinSpeed = Random.Range(0.9f, 1.1f);
        }

        private void Update()
        {
            if (Application.isEditor == false || (Application.isEditor && Application.isPlaying))
            {
                if (CurrentBehaviour != null)
                {
                    CurrentBehaviour.Update();
                }
                UpdateMovement();
            }
        }

        public BotBehaviour GetBehaviour(string BehaviourName)
        {
            for (int i = 0; i < MyBehaviours.Count; i++)
            {
                if (MyBehaviours[i].Name == BehaviourName)
                {
                    return MyBehaviours[i];
                }
            }
            return null;
        }

        public bool BeginBehaviour(string BehaviourName)
        {
            BotBehaviour NewBehaviour = GetBehaviour(BehaviourName);
            //if (MyBehaviour != null)
            //{
                if (CurrentBehaviour != null)
                {
                    // exit?
                    CurrentBehaviour.Exit();
                }
                CurrentBehaviour = NewBehaviour;
                if (CurrentBehaviour != null)
                {
                    CurrentBehaviour.Begin();
                }
                return true;
            /*}
            else
            {
                Debug.LogError(BehaviourName + " does not exist in bot.");
                return false;
            }*/
        }

        /// <summary>
        /// Changes the state if goals are not being met.
        /// </summary>
        private void AutoChangeState()
        {
            /*if (MyMovementState == MovementState.Waiting)
            {
                if (Time.time - TimeStartedWaiting >= 10f)
                {
                    Wander();
                }
            }*/
        }

        public void EnableBot()
        {
            Debug.Log(name + " is enabling bot components.");
            //MyMovement = gameObject.GetComponent<BotMovement>();
            /*if (MyMovement == null)
            {
                MyMovement = gameObject.AddComponent<BotMovement>();
            }*/
            enabled = true;
        }

        /// <summary>
        /// Disables all
        /// </summary>
        public void Disable()
        {
            Debug.Log("Disabling bot: " + name);
            CurrentBehaviour.Exit();
            enabled = false;
        }

        /*public void RemoveBotComponents()
        {
            CurrentBehaviour.Exit();
            if (MyMovement)
            {
                ObjectUtil.DestroyThing(MyMovement);
            }
            ObjectUtil.DestroyThing(this);
        }*/

        #region Actions

        /// <summary>
        /// New Target
        /// </summary>
        /*public void FollowTarget(GameObject NewTarget)
        {

        }*/

        public void Wander()
        {
            
        }

        public void Wait()
        {

        }
        #endregion

        #region TriggerStates

        /// <summary>
        /// When hit in combat
        /// </summary>
        public void WasHit(GameObject NewTarget)
        {
            /*if (enabled)
            {
                if (NewTarget != gameObject)
                {
                    ChangeState(MovementState.Attacking);
                    if (MyMovement)
                    {
                        MyMovement.AttackTarget(NewTarget.GetComponent<Character>());
                    }
                }
            }*/
        }

        /// <summary>
        /// Movement Actions
        /// Commanded to move to a position
        /// </summary>
        /*public void MoveToPosition(Vector3 NewPosition)
        {

        }*/
        /// <summary>
        /// pause until told not to
        /// </summary>
        public void Pause()
        {

        }

        // Summoning
        public void BegunSummoning(GameObject Summoner)
        {

        }

        public void WasSummoned(GameObject Summoner)
        {

        }

        // speech
        public void BegunSpeech(GameObject OtherCharacter)
        {

        }
        #endregion
    }
}