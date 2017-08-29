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

	    void Update ()
        {
            FollowMouse();
        }
        /// <summary>
        /// Using Orbitor to position the gui, it makes the gui follow the mouse.
        /// </summary>
        private void FollowMouse()
        {
            Vector2 NewMousePosition = new Vector2(Mathf.RoundToInt(Input.mousePosition.x), Mathf.RoundToInt(Input.mousePosition.y));
            if (MousePosition.x != NewMousePosition.x || MousePosition.y != NewMousePosition.y)
            {
                MyPositioner.SetScreenPosition(RectUpdater.MousePositionToScaledScreenPosition(NewMousePosition));
                MousePosition = NewMousePosition;
            }
        }
    }
}
