using UnityEngine;
using System.Collections;

namespace Zeltex.AI
{
    /// <summary>
    /// A simple camera Movement Script
    /// </summary>
    public class CameraMoveTo : MonoBehaviour
    {
        public Transform MoveToPosition;
        public float DistanceTo = 1;
        float TimeStarted;
        float Speed = 1;
        float TotalTimeToTravel;

	    // Update is called once per frame
	    void Update ()
        {
            MoveTo();
        }

        /// <summary>
        /// Lerps the camera to a new position
        /// </summary>
        private void MoveTo()
        {
            if (MoveToPosition)
            {
                Vector3 DirectionVector = (transform.position - MoveToPosition.transform.position).normalized;
                DirectionVector = Vector3.up;
                Vector3 TargetPosition = MoveToPosition.transform.position + DirectionVector * DistanceTo;
                float TimeLerp = (Time.time - TimeStarted) / TotalTimeToTravel;
                transform.position = Vector3.Lerp(transform.position, TargetPosition, TimeLerp);
                transform.LookAt(MoveToPosition.position);
                float Distance = Vector3.Distance(MoveToPosition.transform.position, transform.position);
                if (Distance <= DistanceTo * 1.1f)
                {
                    MoveToPosition = null;
                }
            }
        }

        /// <summary>
        /// Locks the target of the camera MoveTo to a transform.
        /// </summary>
        /// <param name="NewTarget"></param>
        public void MoveTo(Transform NewTarget)
        {
            MoveToPosition = NewTarget;
            TimeStarted = Time.time;
            float Distance = Vector3.Distance(MoveToPosition.transform.position, transform.position);
            TotalTimeToTravel = Distance / Speed;
        }
    }
}