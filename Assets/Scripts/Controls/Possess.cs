using UnityEngine;
using Zeltex.Combat;
using Zeltex.AI;
using Zeltex.Skeletons;
using Zeltex.WorldUtilities;
using Zeltex.Characters;
using Zeltex.Guis;
using Zeltex.Guis.Characters;
using Zeltex.Guis.Players;
using Zeltex.Cameras;

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

namespace Zeltex
{

    /// <summary>
    /// Base Player Class - Used to take possession of a character
    /// No input at all
    /// </summary>
    [ExecuteInEditMode]
    public class Possess : MonoBehaviour
    {
        #region Variables
        [SerializeField]
        private EditorAction ActionPossess;
        [SerializeField]
        private Character ActionCharacter;
        static Transform CamerasParent;
        // References
        protected Character LastCharacter;              // Who I was controlling
        [SerializeField]
        protected Character MyCharacter;                // who i am controlling
        [Header("Possession")]
        [Tooltip("Makes the current transform the player")]
        [SerializeField]
        private bool IsAttachedToCharacter;
        // Parts of the character
        [SerializeField]
        protected Transform CameraBone = null;          // The camera used for the player
        [SerializeField]
        protected Transform BodyBone = null;            // The body that moves around
        [SerializeField]
        protected Mover MyController;
        [SerializeField]
        private bool IsParentOfAllCameras;
        [SerializeField, HideInInspector]
        protected CharacterGuis MyGuiManager;
        [SerializeField, HideInInspector]
        protected Skillbar MySkillbar;
        [SerializeField, HideInInspector]
        protected SkeletonHandler MySkeleton;
        [SerializeField, HideInInspector]
        protected ZelGui MyCrosshair;
        [SerializeField, HideInInspector]
        protected CameraMovement MyCameraMovement;
        // States
        protected bool IsInput = true;
        protected bool IsFrozen = false;
        protected bool CanDisableMouse;         // for things like first / third person controller, disable the mouse
        // Possession
        protected int CharacterIndex = 0;
        // Shortcut keys - Debugging
        //public ZelGui MyVoxelPainter;
        //public ZelGui MyCharacterPainter;
        // Settings
        protected bool IsHideHeadMesh;
        protected bool IsDisableBot = true;
        protected Vector3 CameraPositionOffset;
        protected Vector3 CameraRotationOffset;

        private KeyCode ConsoleKey = KeyCode.BackQuote;
        private KeyCode VoxelPainterKey = KeyCode.Alpha0;
        private KeyCode SkeletonPainterKey = KeyCode.Alpha9;
        #endregion

        #region Mono
        private void Awake()
        {
            MyCameraMovement = GetComponent<CameraMovement>();
        }

        protected virtual void Start()
        {
            SetSettings();
        }

        void Update ()
		{
            if (Application.isPlaying)
            {
                HandleInput();
                LockMouse();
            }
            if (ActionPossess.IsTriggered())
            {
                SetCharacter(ActionCharacter);
            }
        }

        protected virtual void SetSettings()
        {
            if (IsAttachedToCharacter)
            {
                if (MyCharacter == null)
                {
                    if (GetComponent<BasicController>() == null)
                    {
                        gameObject.AddComponent<BasicController>();
                    }
                    if (GetComponent<Skillbar>() == null)
                    {
                        gameObject.AddComponent<Skillbar>();
                    }
                    if (GetComponent<Character>() == null)
                    {
                        gameObject.AddComponent<Character>();
                    }
                    SetCharacter(gameObject.GetComponent<Character>());
                }
            }
            IsHideHeadMesh = true;
        }
        #endregion

        #region MouseLock

        void LockMouse()
        {
            Cursor.visible = !IsInput;
            if (IsInput)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }
        #endregion

        #region MoreTakeOver

        /// <summary>
        /// Take over the next character in the map!
        /// </summary>
        protected void TakeOverNext()
        {
            CharacterIndex++;
            if (CharacterIndex >= CharacterManager.Get().GetSize())
            {
                CharacterIndex = 0;
            }
            if (CharacterManager.Get().GetSize() > 0)
            {
                Character MyCharacter = CharacterManager.Get().GetSpawn(CharacterIndex);
                if (MyCharacter)
                {
                    SetCharacter(MyCharacter);
                }
                else
                {
                    Debug.LogError("Index: " + CharacterIndex + " has no character.");
                }
            }
            else
            {
                Debug.LogError("No Characters to take over.");
            }
        }
        #endregion

        #region Character

        /// <summary>
        /// Set the mesh that the character uses for a head
        /// </summary>
        public void SetHeadMesh(bool NewState)
        {
            if (NewState || (NewState == false && IsHideHeadMesh))
            {
                if (MyCharacter && MyCharacter.GetSkeleton() && MyCharacter.GetSkeleton().GetSkeleton().MyBoneHead &&
                    MyCharacter.GetSkeleton().GetSkeleton().MyBoneHead.GetComponent<MeshRenderer>())
                {
                    MyCharacter.GetSkeleton().GetSkeleton().MyBoneHead.GetComponent<MeshRenderer>().enabled = NewState;
                }
            }
        }

        /// <summary>
        /// Set the bot on or off
        /// </summary>
        private void SetBot(bool NewState)
        {
            Bot CharacterBot = MyCharacter.GetComponent<Bot>();
            if (CharacterBot)
            {
                //CharacterBot.enabled = NewState;
                if (NewState)
                {
                    CharacterBot.EnableBot();
                }
                else
                {
                    if (IsDisableBot)
                    {
                        CharacterBot.Disable();
                    }
                    else
                    {
                        CharacterBot.Wait(); // stop wandering!
                    }
                    CharacterBot.Disable();
                }
            }
        }

        /// <summary>
        /// Detatch player from character!
        /// </summary>
        public void RemoveCharacter(bool IsKillAlso = false)
        {
            if (MyCharacter)
            {
                if (MyCameraMovement)
                {
                    MyCameraMovement.enabled = true;
                }
                LastCharacter = MyCharacter;
                Debug.Log("Player is now removing control from [" + MyCharacter.name + "]");
                MyCharacter.SetPlayer(false);
                SetHeadMesh(true);
                SetBot(true);
                if (IsParentOfAllCameras)
                {
                    transform.SetParent(CameraManager.Get().transform, true);
                }
                MySkillbar = null;
                // reset these targets back before i depossess, so their guis aim at themselves and not camera
                if (MyGuiManager != null)
                {
                    SetGuiTargets(CameraBone, MySkeleton);
                }
                MyGuiManager = null;
                MySkeleton = null;
                CameraBone = null;
                if (MyController != null)
                {
                    //MyController.SetMovementSpeed(MyController.GetMovementSpeed() / 1.2f);
                }
                MyController = null;
                BodyBone = null;
                SetInput(false);
                if (IsKillAlso)
                {
                    MyCharacter.OnDeath();
                }
                MyCharacter = null;
            }
        }

        private void SetGuiTargets(Transform NewCameraBone, SkeletonHandler NewSkeleton)
        {
            //bool NewState = (MySkeleton == null);   // if no skeleton (possession by human camera), turn them on
            if (MyGuiManager != null)   //!NewState && 
            {
                MyCrosshair = MyGuiManager.GetZelGui("Crosshair");
            }

            ZelGui MyLabel = MyGuiManager.GetZelGui("Label");
            if (MyLabel)
            {
                MyLabel.GetComponent<Orbitor>().SetTarget(CameraBone, MySkeleton);
                //MyLabel.SetState(NewState);
            }

            ZelGui MySkillBar = MyGuiManager.GetZelGui("SkillBar");
            if (MySkillBar)
            {
                MySkillBar.GetComponent<Orbitor>().SetTarget(CameraBone, MySkeleton);
                //MySkillBar.SetState(NewState);
            }

            ZelGui MyItemPickup = MyGuiManager.GetZelGui("ItemPickup");
            if (MyItemPickup)
            {
                MyItemPickup.GetComponent<Orbitor>().SetTarget(CameraBone, MySkeleton);
            }

            ZelGui MyTooltip = MyGuiManager.GetZelGui("Tooltip");
            if (MyTooltip)
            {
                MyTooltip.GetComponent<Orbitor>().SetTarget(CameraBone, MySkeleton);
            }
        }

        /// <summary>
        /// Used in console
        /// </summary>
        public Character GetCharacter()
        {
            return MyCharacter;
        }
        
        /// <summary>
        /// Set the character - Attaches it to the player
        /// </summary>
        public virtual void SetCharacter(Character MyCharacter_)
        {
            if (MyCharacter)
            {
                RemoveCharacter();  // for bots mostly
            }
            MyCharacter = MyCharacter_;
            if (MyCharacter != null && MyCharacter.IsAlive())
            {
                Debug.Log("Player is now taking over [" + MyCharacter.name + "]");
                if (MyCameraMovement)
                {
                    MyCameraMovement.enabled = false;
                }
                MyCharacter.SetPlayer(true);
                MySkeleton = MyCharacter.GetSkeleton();
                if (MySkeleton)
                {
                    CameraBone = MySkeleton.GetSkeleton().GetCameraBone();
                    if (CameraBone)
                    {
                        transform.position = CameraBone.transform.position;  // MyPlayerSpawn.transform.position + MyPlayerSpawn.transform.TransformDirection (CameraOffset);
                        transform.rotation = CameraBone.transform.rotation;
                        // assuming player is on camera
                        transform.SetParent(CameraBone);
                    }
                }
                MySkillbar = MyCharacter.GetComponent<Skillbar>();
                MyController = MyCharacter.GetComponent<Mover>();
                BodyBone = MyCharacter.transform;
                if (!IsAttachedToCharacter)
                {
                    transform.localPosition = CameraPositionOffset;
                    transform.localEulerAngles = CameraRotationOffset;
                }
                SetHeadMesh(false);
                SetBot(false);

				MyGuiManager = MyCharacter.GetGuis();
				if (MyGuiManager != null)// && MyGuiManager.GetZelGui("Label"))
                {
                    MyGuiManager.Spawn("Menu");
                }
                SetInput(true); // turn mouse off
            }
        }

        /// <summary>
        /// Executed when the camera bone is updated from skeleton
        /// </summary>
        public void SetCameraBone(Transform CameraBone_) //Transform BodyBone_, 
        {
            //BodyBone = BodyBone_;
            CameraBone = CameraBone_;
        }
        #endregion

        #region State

        /// <summary>
        /// Disables the player
        /// </summary>
        public void DisableInput()
        {
            SetInput(false);
        }

        /// <summary>
        /// Sets fps controls on - fps gui etc
        /// </summary>
        public virtual void SetInput(bool IsInput_)
		{
			IsInput = IsInput_;
        }

        /// <summary>
        /// Toggles the mouse
        /// </summary>
        public void ToggleMouse()
        {
            if (IsFrozen == false)
            {
                SetInput(!IsInput);
            }
        }

        public void SetMouse(bool NewInput)
        {
            if (IsFrozen == false)
            {
                SetInput(NewInput);
            }
        }

        /// <summary>
        /// Freezes the player
        /// </summary>
        public void SetFreeze(bool NewFreeze)
        {
            IsFrozen = NewFreeze;
            SetInput(!NewFreeze);
        }
        /// <summary>
        /// Restores the player
        /// </summary>
        public void UnFreezeMouse()
        {
            SetFreeze(false);
        }
        #endregion

        #region Input
        /// <summary>
        /// basic input to debug the player
        /// </summary>
        protected void DebugInput()
        {
            if (GUIUtil.IsInputFieldFocused() == false)
            {
                // testing
                if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    if (MyCharacter != null)
                    {
                        RemoveCharacter();
                    }
                    else
                    {
                        SetCharacter(LastCharacter);
                    }
                }
                if (Input.GetKeyDown(KeyCode.Alpha7))
                {
                    TakeOverNext();
                }
                /* if (Input.GetKeyDown(KeyCode.Alpha8))
                 {
                     MyVoxelPainter.Toggle();
                 }
                 if (Input.GetKeyDown(KeyCode.Alpha9))
                 {
                     MyCharacterPainter.Toggle();
                 }*/
                ToggleGui(ConsoleKey, "Console");
                ToggleGui(VoxelPainterKey, "VoxelPainter");
                ToggleGui(SkeletonPainterKey, "SkeletonPainter");
            }
        }

        private void ToggleGui(KeyCode MyKey, string GuiName)
        {
            if (Input.GetKeyDown(MyKey))
            {
                if (GuiSpawner.Get().GetGui(GuiName))
                {
                    GuiSpawner.Get().DestroySpawn(GuiName);
                }
                else
                {
                    GuiSpawner.Get().SpawnGui(GuiName);
                }
            }
        }
        /// <summary>
        /// The main input function
        /// </summary>
        protected virtual void HandleInput()
        {
            DebugInput();

        }
        #endregion

        #region Statics

        public static void PossessCharacter(Character MyCharacter, Camera MyCamera = null)
        {
            if (MyCamera == null)
            {
                MyCamera = Camera.main;
            }
            if (MyCamera != null)
            {
                Possess[] MyControllers = MyCamera.gameObject.GetComponents<Possess>();
                for (int i = 0; i < MyControllers.Length; i++)
                {
                    if (MyControllers[i].enabled)
                    {
                        MyControllers[i].SetCharacter(MyCharacter);
                        break;
                    }
                }
            }
            else
            {
                Debug.LogError("No Main Camera.");
            }
        }
        #endregion
    }
}
