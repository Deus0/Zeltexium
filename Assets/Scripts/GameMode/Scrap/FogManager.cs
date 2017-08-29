using UnityEngine;
using System.Collections;

namespace Zeltex.Game
{
    /// <summary>
    /// manages fog
    /// tweens fog
    /// </summary>
    public class FogManager : MonoBehaviour
    {
        float TimeBegin;
        public float AnimateLength = 15f;

        void Start()
        {
            DisableFog();
        }

        void Update()
        {
            AnimateFog();
        }
        /// <summary>
        /// Begins fog animation, enabling it
        /// </summary>
        public void EnableFog()
        {
            RenderSettings.fog = true;
            TimeBegin = Time.time;
            RenderSettings.fogDensity = 0;
        }
        public void DisableFog()
        {
            RenderSettings.fog = false;
        }

        void AnimateFog()
        {
            float TimePassed = Time.time - TimeBegin;
            if (TimePassed <= AnimateLength)
            {
                RenderSettings.fogDensity = Mathf.Lerp(0, 0.2f, TimePassed / AnimateLength);
            }
        }
    }
}