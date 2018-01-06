using UnityEngine;

namespace Zeltex.Guis
{
    /// <summary>
    /// Basic script causing gui to follow mouse using Orbitor
    /// </summary>
    public class MouseFollower : MonoBehaviour
    {
        public Orbitor MyPositioner;
        private Vector2 MousePosition;

        //private void OnEnable()
        //{
            //Debug.LogError("Item Pickup enabled..");
        //}

        void Update ()
        {
            FollowMouse();
        }

        /// <summary>
        /// Using Orbitor to position the gui, it makes the gui follow the mouse.
        /// </summary>
        public void FollowMouse(bool IsForce = false)
        {
            Vector2 NewMousePosition = new Vector2(Mathf.RoundToInt(Input.mousePosition.x), Mathf.RoundToInt(Input.mousePosition.y));
            if (IsForce || (MousePosition.x != NewMousePosition.x || MousePosition.y != NewMousePosition.y))
            {
                MyPositioner.SetScreenPosition(RectUpdater.MousePositionToScaledScreenPosition(NewMousePosition));
                MousePosition = NewMousePosition;
            }
        }
    }
}
