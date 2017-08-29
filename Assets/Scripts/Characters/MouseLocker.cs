using UnityEngine;
using Zeltex.AI;
using Zeltex.Guis;

namespace Zeltex.Characters 
{
	/// <summary>
    /// Controls cursor and fps controls
    /// </summary>
	public class MouseLocker : MonoBehaviour 
	{
        /*#region Variables
        //public BasicController MyPlayerController;
		private Transform MyCrosshair3D;
		//public Texture2D MyCursorTexture;
        //private Transform CameraBone;
        private Transform MyPlayer;
        public bool IsFPSEnabled = true;
        bool IsFrozen = false;
        #endregion
        /// <summary>
        /// Locks onto a new character.
        /// </summary>
        public void SetController(Transform MyCharacter)
		{
			MyPlayer = MyCharacter;
			if (MyPlayer == null)
			{
				//MyPlayerController = null;
				MyCrosshair3D = null;
				return;
			}
		}

		public void ToggleMouse()
        {
            if (IsFrozen == false)
            {
                SetPlayerInput(!IsFPSEnabled);
            }
		}
        /// <summary>
        /// Called by CameraMovement script
        /// </summary>
        /// <param name="NewMouse"></param>
        public void SetPlayerInput(bool NewMouse)
        {
            if (IsFrozen == false)
            {
                SetPlayerInput(NewMouse, false);
            }
		}

        /// <summary>
        /// Sets the new player input
        /// </summary>
		public void SetPlayerInput(bool NewMouse, bool IsDisablePlayer) 
		{
			IsFPSEnabled = NewMouse;
            gameObject.GetComponent<MouseCursor>().SetState(!IsFPSEnabled);
			//Debug.LogError ("Toggling mouse in mouse locker!");
			if (MyPlayer)
			{
				Player PlayerComponent = MyPlayer.GetComponent<Player> ();
				if (PlayerComponent) 
				{
					PlayerComponent.SetInput (IsFPSEnabled);
					if (IsDisablePlayer)
                    {
                        PlayerComponent.enabled = IsFPSEnabled;
                    }
				}
			}
			if (MyCrosshair3D)
			{
				if (IsFPSEnabled)
                {
                    MyCrosshair3D.GetComponent<ZelGui>().TurnOn();
                }
				else
                {
                    MyCrosshair3D.GetComponent<ZelGui>().TurnOff();
                }
			}
		}
        public void FreezeMouse()
        {
            IsFrozen = true;
            SetPlayerInput(false, false);
        }
        public void UnFreezeMouse()
        {
            IsFrozen = false;
            SetPlayerInput(true, false);
        }*/
    }
}
