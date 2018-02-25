using UnityEngine;
using System.Collections;

namespace Zeltex.WorldUtilities
{
    public class AnimateCameraPosition : MonoBehaviour
    {
        public float AnimationTime = 3f;
        public UnityEngine.Events.UnityEvent OnFinishAnimation = null;
        public UnityEngine.Events.UnityEvent OnFinishReverseAnimation = null;
        Vector3 OldPosition;
        Vector3 NewPosition;
        Quaternion OldRotation;
        Quaternion NewRotation;
        float TimeStarted;
        bool IsAnimating = false;
        bool IsReverse = false;

        // Use this for initialization
        void Awake()
        {
            NewPosition = transform.position;
            NewRotation = transform.rotation;
        }

        // Update is called once per frame
        void Update()
        {
            if (IsAnimating)
            {
                float TimeSinceBegan = Time.time - TimeStarted;
                if (TimeSinceBegan <= AnimationTime)
                {
                    transform.position = Vector3.Lerp(OldPosition, NewPosition, TimeSinceBegan / AnimationTime);
                    transform.rotation = Quaternion.Lerp(OldRotation, NewRotation, TimeSinceBegan / AnimationTime);
                }
                else {
                    IsAnimating = false;    // end animation
                    transform.position = NewPosition;
                    if (IsReverse)
                    {
                        SwapThings();
                        if (OnFinishReverseAnimation != null)
                            OnFinishReverseAnimation.Invoke();
                    }
                    else {
                        if (OnFinishAnimation != null)
                            OnFinishAnimation.Invoke();
                    }
                }
            }
        }
        public void ReverseAnimation()
        {
            TimeStarted = Time.time;
            IsAnimating = true;
            IsReverse = true;
            SwapThings();
        }
        public void SwapThings()
        {
            Vector3 TempPosition = OldPosition;
            OldPosition = NewPosition;
            NewPosition = TempPosition;
            Quaternion TempRotation = OldRotation;
            OldRotation = NewRotation;
            NewRotation = TempRotation;
        }
        public void Animate(GameObject OldCameraPosition)
        {
            transform.position = OldCameraPosition.transform.position;
            transform.rotation = OldCameraPosition.transform.rotation;
            OldPosition = OldCameraPosition.transform.position;
            OldRotation = OldCameraPosition.transform.rotation;
            TimeStarted = Time.time;
            IsAnimating = true;
            IsReverse = false;
        }
    }
}