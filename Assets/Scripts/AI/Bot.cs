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
        public bool IsAggressive = false;
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
        // StateData
        public bool Attacking;
        // Components
        [SerializeField]
        public BotBehaviour CurrentBehaviour;
        [SerializeField]
        public List<BotBehaviour> MyBehaviours = new List<BotBehaviour>();
        [SerializeField]
        public Wander MyWander;
        [SerializeField]
        public BotBehaviour MyWaiting;

        private float TimeStartedWaiting = 0;
        [SerializeField]
        private EditorAction ActionAddWander = new EditorAction();
        [SerializeField]
        private EditorAction ActionAddWaiting = new EditorAction();
        [SerializeField]
        private EditorAction ActionClearBehaviours = new EditorAction();
        private bool CanMove = false;

        public BotMeta GetData()
        {
            return MyCharacter.GetData().BotData;
        }

        public void OnBeginGame()
        {
            CanMove = true;
            Initiate();
            BeginBehaviour("Wander");
        }

        public void OnEndGame()
        {
            CanMove = false;
        }

        void Start()
        {
            Initiate();
        }

        private bool HasInitiated;
        private void Initiate()
        {
            if (HasInitiated == false)
            {
                HasInitiated = true;
                if (MyWaiting == null)
                {
                    MyWaiting = new BotBehaviour();
                    MyBehaviours.Add(MyWaiting);
                }
                if (MyWander == null)
                {
                    MyWander = new Wander();
                    MyBehaviours.Add(MyWander);
                }
                MyCharacter = GetComponent<Character>();
                MySkeleton = MyCharacter.GetSkeleton();
                RefreshRigidbody();
                // The beginning state is paused
                for (int i = 0; i < MyBehaviours.Count; i++)
                {
                    MyBehaviours[i].Initiate(transform);
                }
                if (CurrentBehaviour == null)// || CurrentBehaviour.Name == MyWander.Name)
                {
                    CurrentBehaviour = MyWaiting;
                }
                //BeginBehaviour(CurrentBehaviour.Name);
                SinAddition = Random.Range(0.0f, 1.0f);
                SinSpeed = Random.Range(0.9f, 1.1f);
                TargetRotation = transform.eulerAngles;
            }
        }

        private void Update()
        {
            if (Application.isEditor == false || (Application.isEditor && Application.isPlaying))
            {
                if (CanMove)
                {
                    if (CurrentBehaviour != null)
                    {
                        CurrentBehaviour.Update(this);
                    }
                    UpdateMovement();
                }
            }
            HandleActions();
        }

        private void HandleActions()
        {
            if (ActionAddWaiting.IsTriggered())
            {
                //if (MyWaiting == null)
                MyBehaviours.Remove(MyWaiting);
                MyBehaviours.Add(new BotBehaviour());
                MyWaiting = MyBehaviours[MyBehaviours.Count - 1] as BotBehaviour;
                MyWaiting.Name = "Waiting";
                CurrentBehaviour = MyWaiting;
            }
            if (ActionAddWander.IsTriggered())
            {
                MyBehaviours.Remove(MyWander);
                MyBehaviours.Add((new Wander()) as BotBehaviour);
                MyWander = MyBehaviours[MyBehaviours.Count - 1] as Wander;
                CurrentBehaviour = MyWander;
                MyWander.Name = "Wander";
            }
            if (ActionClearBehaviours.IsTriggered())
            {
                MyBehaviours.Clear();
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
            if (CurrentBehaviour != null)
            {
                CurrentBehaviour.Exit();
            }
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
        public void WasHit(Character AttackingCharacter)
        {
            if (enabled)
            {
                if (AttackingCharacter != gameObject)
                {
                    AttackTarget(AttackingCharacter);
                }
            }
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