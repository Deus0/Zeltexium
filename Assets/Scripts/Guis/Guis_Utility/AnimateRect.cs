using UnityEngine;
using Zeltex.Util;
using Zeltex.Guis;

namespace Zeltex.Guis
{
    [System.Serializable]
    public enum AnimateDirection
    {
        Right,
        Left,
        Up,
        Down
    }
    /// <summary>
    /// Animates the rect by sliding it. Used for extra buttons on guis.
    /// </summary>
    public class AnimateRect : MonoBehaviour
    {
        public AnimateDirection MyDirection = AnimateDirection.Right;
        private float TimeStarted = -1;
        private float OriginalAnimationLength = 1f;
        private float AnimationLength = 1f;
        private Vector2 BeginPosition;
        private Vector2 EndPosition;
        private Vector2 MyPosition;
        private Vector2 TargetOffset;
        private bool IsForward;
        private float TimePassed;

        void Start()
        {
            BeginPosition = gameObject.GetComponent<RectTransform>().anchoredPosition;
            //EndPosition = BeginPosition + gameObject.GetComponent<RectTransform>().TransformPoint(new Vector3(100, 0, 0));
            if (MyDirection == AnimateDirection.Right)
                EndPosition = BeginPosition + new Vector2(gameObject.GetComponent<RectTransform>().GetWidth(), 0);
            else if (MyDirection == AnimateDirection.Left)
                EndPosition = BeginPosition + new Vector2(-gameObject.GetComponent<RectTransform>().GetWidth(), 0);
            else if (MyDirection == AnimateDirection.Up)
                EndPosition = BeginPosition + new Vector2(0, gameObject.GetComponent<RectTransform>().GetHeight());
            else if (MyDirection == AnimateDirection.Down)
                EndPosition = BeginPosition + new Vector2(0, -gameObject.GetComponent<RectTransform>().GetHeight());
            gameObject.GetComponent<RectTransform>().anchoredPosition = EndPosition;
            IsForward = true;
        }

        public void Begin()
        {
            IsForward = !IsForward; // flip direction
            //Debug.Log("Toggling: " + IsForward);
            MyPosition = gameObject.GetComponent<RectTransform>().anchoredPosition;
            if (TimeStarted == -1)
            {
                AnimationLength = OriginalAnimationLength;
            }
            else
            {
                AnimationLength = OriginalAnimationLength - TimePassed; //  reverse time left
            }
            TimeStarted = Time.time;
        }
	    public void End()
        {
            TimeStarted = -1;
        }
	    // Update is called once per frame
	    void Update ()
        {
            Animate();

        }
        void Animate()
        {
            if (TimeStarted != -1)
            {
                TimePassed = Time.time - TimeStarted;
                /*if (transform.parent.GetComponent<Orbitor>())
                {
                    TargetOffset = transform.parent.GetComponent<Orbitor>().GetScreenPosition();
                }*/
                if (IsForward)
                {
                    gameObject.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(MyPosition, EndPosition + TargetOffset, TimePassed / AnimationLength);
                }
                else
                {
                    gameObject.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(MyPosition, BeginPosition + TargetOffset, TimePassed / AnimationLength);
                }
                if (TimePassed >= AnimationLength)
                {
                    End();
                }
            }
        }
    }
}
