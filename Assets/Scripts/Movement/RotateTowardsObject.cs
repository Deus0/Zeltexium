using UnityEngine;
using System.Collections;


namespace Zeltex.AI
{
    public class RotateTowardsObject : MonoBehaviour
    {
        public float AnimationSpeed = 1f;
        private GameObject MyTarget;

        public void RotateTowards(GameObject NewObject)
        {
            MyTarget = NewObject;
        }

        public void Stop()
        {

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (MyTarget)
            {
                transform.LookAt(MyTarget.transform.position);
            }
        }
    }
}
