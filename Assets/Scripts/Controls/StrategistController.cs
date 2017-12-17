using UnityEngine;
using Zeltex.Combat;
using Zeltex.Voxels;
using Zeltex.AI;
using Zeltex.Guis;
using Zeltex.Skeletons;
using Zeltex.WorldUtilities;
using Zeltex.Characters;

/*
		Player class
			Any extra things on top of the Characters functionality
			Keyboard input

	To convert to player: - All other classes with input
		Toggle keys
		GuiManager Keys
		TestMovement, MouseLocker, MouseLook classes
		CameraMovement
*/

/// <summary>
/// Handles most of the games logic:
///     Game Modes
///     Player Controllers
///     Game Rules
/// </summary>
namespace Zeltex
{
    /// <summary>
    /// Strategist Controller
    /// Connects with Character and Movement classes.
    /// This class (should) only has Input
    /// To Do:
    ///     Add Gui Things for move to position (little cube, lerping moving a little)
    ///     Add Gui things above characters i am following, or attacking (green or red)
    ///     Switch between first person camera position and third and birds eye (top down) views
    ///     Show lines for path (along the ground)
    ///     Fly Mode button
    ///     Gui Button to toggle menu
    ///     Fix Bot Movement
    ///         Needs to turn to face the right direction
    ///         Needs to Only Move Forwards direction
    ///     Add in gui - for bot state
    ///     Add in gui for player options
    ///         click on player - will it follow, attack, talk to, defend, etc
    ///         click on tree - inspect, move to, chop down
    ///     Then after this: Animations for attacking, sword chopping - use proper class names for adventure mode
    /// </summary>
	public class StrategistController : Possess
    {
        #region Variables
        public Vector3 LastHitPosition;
        public World LastHitWorld;
        public LayerMask MyLayer;
        public bool IsFriendly;
        public Vector3 PositionOffset = new Vector3(0, 1.0f, -0.1f);
        public Vector3 RotationOffset = new Vector3(60, 0, 0);
        public float MaxDistance = 15;
        #endregion

        protected override void SetSettings()
        {
            IsHideHeadMesh = false;
            IsDisableBot = false;
            CameraPositionOffset = PositionOffset;
            CameraRotationOffset = RotationOffset;
        }
        public override void SetInput(bool IsInput_)
        {
            base.SetInput(IsInput_);
            if (MyCrosshair)
            {
                MyCrosshair.TurnOff();
            }
        }

        #region Input
        /// <summary>
        /// The main input function
        /// </summary>
        protected override void HandleInput() 
		{
            base.HandleInput();
            // if player is local to this machine!
            if (MyCharacter)
            {
                // if raycast on ground, make bot move to it
                if (Input.GetMouseButtonDown(0))
                {
                    CommandBot();
                }
            }
        }

        /// <summary>
        /// Get the rayhit object
        /// </summary>
        public GameObject GetRayHitObject()
        {
            RaycastHit MyRaycast;
            Ray MyRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            //Debug.DrawLine(MyRay.origin, MyRay.origin + MyRay.direction * 2, Color.red, 5);
            //Debug.Break();
            if (UnityEngine.Physics.Raycast(MyRay.origin, MyRay.direction, out MyRaycast, MaxDistance, MyLayer))  // Camera.main.transform.position, Camera.main.transform.forward   // 10, 
            {
                LastHitPosition = MyRaycast.point;
                if (MyRaycast.collider.gameObject.GetComponent<World>())
                {
                    LastHitWorld = MyRaycast.collider.gameObject.GetComponent<World>();
                }
                else if (MyRaycast.collider.gameObject.GetComponent<Chunk>())
                {
                    LastHitWorld = MyRaycast.collider.gameObject.GetComponent<Chunk>().GetWorld();
                }
                else
                {
                    LastHitWorld = null;
                }
                return MyRaycast.collider.gameObject;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// The main action!
        /// </summary>
		public void CommandBot()
        {
            if (Character.IsRayHitGui() == false)
            {
                GameObject MyHitObject = GetRayHitObject();
                if (MyHitObject)
                {
                    Bot MyBot = MyCharacter.GetComponent<Bot>();
                    if (LastHitWorld != null)
                    {
                        MyBot.MoveToPosition(LastHitPosition + new Vector3(0, LastHitWorld.transform.localScale.y * LastHitWorld.VoxelScale.y, 0) / 2f);
                        // instead i should use BotBrain class to tell it to do something, it will change the state
                        return;
                    }
                    else if (MyHitObject.gameObject.tag == "World")
                    {
                        MyBot.MoveToPosition(LastHitPosition + new Vector3(0, 0.4f, 0));
                        return;
                    }
                    else
                    {
                        Debug.LogError("World Not hit: " + MyHitObject.name);
                    }

                    Character MyHitCharacter = MyHitObject.GetComponent<Character>();
                    if (MyHitCharacter)
                    {
                        if (IsFriendly)//Input.GetKey(KeyCode.LeftControl))
                        {
                            MyBot.FollowTarget(MyHitCharacter.gameObject);
                        }
                        else
                        {
                            if (MyBot.gameObject == MyHitCharacter.gameObject)
                            {
                                //MyBot.Wait();
                                //MyBot.FollowTarget(gameObject);
                            }
                            else
                            {
                                MyBot.WasHit(MyHitCharacter);
                            }

                        }
                    }
                }
                else
                {
                    Debug.LogError("Nothing hit.");
                }
            }

        }
        #endregion
    }
}
