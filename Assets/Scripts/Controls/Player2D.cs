using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex
{
    /// <summary>
    /// Main controller class.
    /// Connects with Character and Movement classes.
    /// This class (should) only has Input
    /// To Do:
    ///     - Seperate out rotation variables into a camera class?
    /// </summary>
	public class Player2D : Possess
    {

        #region PossessOverrides

        protected override void Start()
        {
            base.Start();
            IsInput = true;
        }

        protected override void SetSettings()
        {
            IsHideHeadMesh = true;
            IsDisableBot = true;
        }

        /// <summary>
        /// The main input function
        /// </summary>
        protected override void HandleInput()
        {
            base.HandleInput();
            // if player is local to this machine!
            if (MyCharacter && IsInput)// && gameObject.GetComponent<PhotonView>().owner == PhotonNetwork.player)
            {
                /*float MovementX = Input.GetAxis("Horizontal");
                MyController.Input(MovementX, 0, 
                    Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space) || Input.GetAxis("Jump") != 0);
                if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space) || Input.GetAxis("Jump") != 0)
                {
                    Debug.LogError("JUMP");
                }*/
            }
        }

        /// <summary>
        /// Sets fps controls on - fps gui etc
        /// </summary>
        public override void SetInput(bool IsInput_)
        {
            base.SetInput(IsInput_);
            /*if (MyController)
            {
                MyController.SetRotationVelocity(Vector3.zero);
                MyController.SetRotationState(IsInput);
            }*/
        }
        #endregion
    }
}