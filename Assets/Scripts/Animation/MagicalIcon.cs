using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex.Animations
{
    public class MagicalIcon : MonoBehaviour
    {
        public AnimationCurve GrowCurve;
        private Vector3 OriginalScale;
        [SerializeField]
        private Vector3 ScaleAmplification = new Vector3(0.1f, 0.1f, 0.1f);
        [SerializeField]
        private float GrowSpeed = 1f;
        private float GrowTime;

        // Use this for initialization
        void Start()
        {
            OriginalScale = transform.localScale;
        }

        // Update is called once per frame
        void Update()
        {
            GrowTime = (Time.time * GrowSpeed) % 1f;
            transform.localScale = OriginalScale + new Vector3(
                ScaleAmplification.x * GrowCurve.Evaluate(GrowTime),
                ScaleAmplification.y * GrowCurve.Evaluate(GrowTime),
                ScaleAmplification.z * GrowCurve.Evaluate(GrowTime));
        }
    }
}
