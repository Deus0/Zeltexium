using UnityEngine;
using Zeltex.Combat;
using Zeltex.Voxels;
using Zeltex.Characters;
using System.Collections;
using Zeltex.AI;

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
    /// Main controller class.
    /// Connects with Character and Movement classes.
    /// This class (should) only has Input
    /// To Do:
    ///     - Seperate out rotation variables into a camera class?
    /// </summary>
    [ExecuteInEditMode]
    public class Player : Possess
    {
        #region Variables
        // Keys
        protected static KeyCode ActivateKey = KeyCode.Mouse0; // spells
        protected static KeyCode ActivateKey2 = KeyCode.Mouse1;
        protected static KeyCode InteractKey1 = KeyCode.E;    // interaction
        protected static KeyCode InteractKey2 = KeyCode.Q;
        protected static KeyCode DropItemKey = KeyCode.Z;
        protected static KeyCode ToggleMouseKey = KeyCode.C;
        protected static KeyCode ForwardKey = KeyCode.W;
        protected static KeyCode BackwardKey = KeyCode.S;
        protected static KeyCode LeftKey = KeyCode.A;
        protected static KeyCode RightKey = KeyCode.D;
        // Movement
        private Vector3 Movement;
        // Rotation
        [Header("Player")]
        [SerializeField]
        private bool IsRotateCamera = true;
        //private float rotationX = 0F;
        //private float rotationY = 0F;
        private float sensitivityX = 5;
        private float sensitivityY = 5;
        private float minimumX = -360F;
        private float maximumX = 360F;
        private float minimumY = -60F;
        private float maximumY = 60F;
        private Vector3 TargetRotation;
        private bool IsJump;
        private Skill[] MySkills;
        private float MouseScrollWheel;
        #endregion

        #region PossessOverrides

        /// <summary>
        /// Called in start function
        /// </summary>
        protected override void SetSettings()
        {
            base.SetSettings();
            IsHideHeadMesh = true;
            IsDisableBot = true;
            SetInput(IsInput);
        }

        /// <summary>
        /// The main input function
        /// </summary>
        protected override void HandleInput()
        {
            base.HandleInput();
            // if player is local to this machine!
            if (MyCharacter)// && gameObject.GetComponent<PhotonView>().owner == PhotonNetwork.player)
            {
                if (IsInput)
                {
                    HandleIngameInput();
                }
                if (Input.GetKeyDown(ToggleMouseKey) || Input.GetKeyDown(KeyCode.Escape))
                {
                    ToggleMouse();
                    if (MyGuiManager == null)
                    {
                        MyGuiManager = MyCharacter.MyGuis;
                    }
                    if (MyGuiManager != null)
                    {
                        MyGuiManager.Spawn("Menu");
                        if (IsInput == false)
                        {
                            MyGuiManager.EnableGui("Menu");
                            //Time.timeScale = 0;
                        }
                        else
                        {
                            MyGuiManager.DisableGui("Menu");
                            //Time.timeScale = 1f;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets fps controls on - fps gui etc
        /// </summary>
        public override void SetInput(bool IsInput_)
        {
            base.SetInput(IsInput_);
            //IsFPSEnabled = IsFirstPersonControl;
            //IsCursorActive = !IsInput;
            if (MyCrosshair == null)
            {
                if (MyCharacter.MyGuis == null)
                {
                    MyCharacter.MyGuis = new Guis.Characters.CharacterGuis();
                }
                MyCrosshair = MyCharacter.MyGuis.GetZelGui("Crosshair");
            }
            if (MyCrosshair)
            {
                if (IsInput)
                {
                    MyCrosshair.TurnOn();
                }
                else
                {
                    MyCrosshair.TurnOff();
                }
            }
            if (MyController == null)
            {
                MyController = MyCharacter.GetComponent<AI.BasicController>();
            }
            if (MyController)
            {
                MyController.SetRotationVelocity(Vector3.zero);
                MyController.SetRotationState(IsInput);
            }
            if (MyCharacter.GetComponent<Mover>())
            {
                MyCharacter.GetComponent<Mover>().enabled = IsInput;
            }
        }
        #endregion

        #region Input

        /// <summary>
        /// In game controls when controlling a character and input is enabled
        /// </summary>
        private void HandleIngameInput()
        {
            HandleControllerInput();
            RotateCamera();
            HandleActionKeys();
            SwitchSkillsInput();
            ActivateSkillsInput();
        }

        /// <summary>
        /// Action keys trigger something more !
        /// Like a spell, dropping an item, or talking to another character!
        /// </summary>
        private void HandleActionKeys()
        {
            if (Input.GetKeyDown(DropItemKey))
            {
                /*MySkillbar.GetSkillBar().DropItem(
                    MySkillbar.GetSelectedIndex(),
                    MySkeleton.MyCameraBone);*/
            }
            // Character Interaction Key - E
            if (Input.GetKeyDown(InteractKey1))// || Input.GetButtonDown("Fire4"))
            {
                MyCharacter.RayTrace();
            }
            else if (Input.GetKeyDown(InteractKey2))
            {
                MyCharacter.RayTrace(1);
            }
        }

        /// <summary>
        /// Switch which item is selected
        /// </summary>
        private void SwitchSkillsInput()
        {
            if (MyCharacter != null)
            {
                MySkillbar = MyCharacter.GetSkillbar();
                // Check SkillBar Input
                if (Input.GetKeyDown(KeyCode.LeftBracket))//Input.GetButtonDown("Mouse ScrollWheel Neg"))
                {
                    MySkillbar.SwitchItemUp();
                }
                if (Input.GetKeyDown(KeyCode.RightBracket))//Input.GetButtonDown("Mouse ScrollWheel Pos"))
                {
                    MySkillbar.SwitchItemDown();
                }
                MouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
                if (MouseScrollWheel > 0)
                {
                    MySkillbar.SwitchItemDown();
                }
                else if (MouseScrollWheel < 0)
                {
                    MySkillbar.SwitchItemUp();
                }

                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    MySkillbar.SwitchItemTo(0);
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    MySkillbar.SwitchItemTo(1);
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    MySkillbar.SwitchItemTo(2);
                }
                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    MySkillbar.SwitchItemTo(3);
                }
                if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    MySkillbar.SwitchItemTo(4);
                }
            }
            else
            {
                Debug.LogError(MyCharacter.name + " has no skillbar");
                MySkillbar = MyCharacter.GetComponent<Skillbar>();
            }
        }
        
        /// <summary>
        /// Activate an item
        /// </summary>
        private void ActivateSkillsInput()
        {
            if (//Input.GetKeyDown (ActivateKey) || Input.GetMouseButtonDown(0) ||
                Input.GetButtonDown("Fire1")) // || Input.GetButtonDown("Fire1")
            {
				if (MyCharacter.GetComponent<VoxelBrush> ()) 
				{
					MyCharacter.GetComponent<VoxelBrush> ().Activate ();
                }
                if (MyCharacter.GetComponent<BotCommander>())
                {
                    MyCharacter.GetComponent<BotCommander>().Activate();
                }

                MySkills = MyCharacter.GetComponents<Skill>();
                for (int i = MySkills.Length - 1; i >= 0; i--)
                {
                    MySkills[i].Activate();
                }
            }
			if (Input.GetKeyDown (ActivateKey2))    // || Input.GetButtonDown("Fire2")
            {
                if (MyCharacter.GetComponent<BotCommander>())
                {
                    MyCharacter.GetComponent<BotCommander>().Activate2();
                }
            }
			// for fire on hold skills
			if (Input.GetKey (ActivateKey)) // || Input.GetButtonDown("Fire3")
            {
				if (MyCharacter.GetComponent<Shooter> ()) 
				{
                    MyCharacter.GetComponent<Shooter> ().Activate3();
				}
			}
		}

        /// <summary>
        /// Move the controller around!
        /// </summary>
        private void HandleControllerInput()
        {
            if (MyController)
            {

                if (Input.GetKeyDown(KeyCode.F))
                {
                    MyController.ToggleFly();
                }
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    MyController.IsRunning = true;
                }
                else
                {
                    MyController.IsRunning = false;
                }
                Movement.x = Input.GetAxis("Horizontal");
                Movement.z = Input.GetAxis("Vertical");
                IsJump = Input.GetButton("Jump");
                if (Input.GetKey(KeyCode.W))
                {
                    Movement.z = 1;
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    Movement.z = -1;
                }
                else
                {
                    Movement.z *= 0.9f;
                }
                if (Input.GetKey(KeyCode.A))
                {
                    Movement.x = -1;
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    Movement.x = 1;
                }
                else
                {
                    Movement.x *= 0.9f;
                }
                MyController.Input(Movement.x, Movement.z, IsJump);
                if (MyController.IsFlyer())
                {
                    if (Input.GetKey(KeyCode.Q))
                    {
                        Movement.y = -1;
                    }
                    else if (Input.GetKey(KeyCode.E))
                    {
                        Movement.y = 1;
                    }
                    else
                    {
                        Movement.y *= 0.9f;
                    }
                }
                else
                {
                    Movement.y *= 0.9f;
                }
                MyController.InputY(Movement.y);
            }
        }
        #endregion

        #region Camera

        public void SetRotation(Vector3 NewRotation)
        {
            if (IsRotateCamera)
            {
                TargetRotation = NewRotation;
                //StartCoroutine(TemporarilyDisableRotation());
            }
        }

        private IEnumerator TemporarilyDisableRotation()
        {
            IsRotateCamera = false;
            yield return new WaitForSeconds(0.5f);
            IsRotateCamera = true;
        }

        private void RotateCamera()
        {
            if (IsRotateCamera && CameraBone != null && BodyBone != null && MyController != null)
            {
                // Read the mouse input axis
                //TargetRotation.x = MyController.transform.eulerAngles.x;
                //TargetRotation.y = ClampAngle(TargetRotation.y, minimumX, maximumX);
                TargetRotation.y += Input.GetAxis("Mouse X") * sensitivityX;
                TargetRotation.x -= Input.GetAxis("Mouse Y") * sensitivityY;
                TargetRotation.x = ClampAngle(TargetRotation.x, minimumY, maximumY);
                CameraBone.eulerAngles = TargetRotation;
                MyController.InputTargetRotation(TargetRotation);
                if (MySkeleton && MySkeleton.GetSkeleton().MyBoneHead)
                {
                    MySkeleton.GetSkeleton().MyBoneHead.rotation = CameraBone.rotation;
                }
            }
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle <= -360F)
            {
                angle += 360F;
            }
            if (angle >= 360F)
            {
                angle -= 360F;
            }
            return Mathf.Clamp(angle, min, max);
        }
        #endregion
    }
}
