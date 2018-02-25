using UnityEngine;
using System.Collections;

namespace Zeltex.AI
{
    /// <summary>
    /// A simple camera Movement Script
    /// </summary>
    public class CameraSmoother : MonoBehaviour
    {
        public float LerpSpeed = 2;
        public float RotationalLerpSpeed = 10;
        private Vector3 PositionOffset = new Vector3();
        private bool IsZoomedOut = false;
        private Transform Target;
        private Transform Follower;
        Vector3 MyPosition;

        // Update is called once per frame
        void Update ()
        {
            if (Target)
            {
                Follow();
                if (Input.GetKeyDown(KeyCode.H))
                {
                    if (IsZoomedOut)
                    {
                        PositionOffset = new Vector3();
                    }
                    else
                    {
                        PositionOffset = new Vector3(0, 0.6f, -0.8f);
                    }
                    IsZoomedOut = !IsZoomedOut;
                    Target.GetComponent<MeshRenderer>().enabled = IsZoomedOut;
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.H))
                {
                    Target = transform.parent.parent;
                    Follower = transform.parent;
                    MyPosition = Follower.position;
                    Target.GetComponent<MeshRenderer>().enabled = false;
                    //Follower.parent = null; // detatch so smoothing works
                }
            }
        }
        void Follow()
        {
            MyPosition = Vector3.Lerp(MyPosition, Target.position + Target.TransformDirection(PositionOffset), Time.deltaTime * LerpSpeed);
            Follower.position = MyPosition;
            // Target.rotation
            Vector3 LookDirection = (Target.position - Follower.position).normalized;
            if (!IsZoomedOut)
            {
                LookDirection = Target.forward;
            }
            Follower.rotation = Quaternion.Lerp(Follower.rotation, Quaternion.LookRotation(LookDirection), Time.deltaTime * RotationalLerpSpeed);
        }
    }
}