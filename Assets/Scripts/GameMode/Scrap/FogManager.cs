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
        public float AnimateLength = 15f;
        private float InGameFog = 0.2f;
        private float MainMenuFog = 0.1f;
        private bool IsInGameFog;
        private float TimeBegunFog;
        private float FogAddition = 0.04f;
        private float FogTimeScale = 0.5f;
        private float MyFog = 0;
        public float FogLerpSpeed = 1f;

        void Start()
        {
            GameManager.Get().OnBeginGame.AddEvent(EnableInGameFog);
            GameManager.Get().OnEndGame.AddEvent(EnableMainMenuFog);
            EnableMainMenuFog();
        }

        private float GetInGameFog()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer && Application.isEditor == false)
            {
                return InGameFog / 2f;
            }
            else
            {
                return InGameFog;
            }
        }
        private float GetMainMenuFog()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer && Application.isEditor == false)
            {
                return MainMenuFog / 2f;
            }
            else
            {
                return MainMenuFog;
            }
        }

        private void EnableInGameFog()
        {
            TimeBegunFog = Time.time;
            IsInGameFog = true;
            EnableFog(GetInGameFog());
        }

        private void EnableMainMenuFog()
        {
            TimeBegunFog = Time.time;
            IsInGameFog = false;
            EnableFog(GetMainMenuFog());
        }

        void Update()
        {
            AnimateFog();
        }

        /// <summary>
        /// Begins fog animation, enabling it
        /// </summary>
        public void EnableFog(float NewFogDensity)
        {
            RenderSettings.fog = true;
            RenderSettings.fogDensity = NewFogDensity;
        }

        public void DisableFog()
        {
            RenderSettings.fog = false;
        }

        void AnimateFog()
        {
            float NewFog;
            if (IsInGameFog)
            {
                NewFog = GetInGameFog() + FogAddition * Mathf.Sin((TimeBegunFog - Time.time) * FogTimeScale);
            }
            else
            {
                NewFog = GetMainMenuFog() + FogAddition * Mathf.Sin((TimeBegunFog - Time.time) * FogTimeScale);
            }
            MyFog = Mathf.Lerp(MyFog, NewFog, Time.deltaTime * FogLerpSpeed);
            RenderSettings.fogDensity = MyFog;
            /*float TimePassed = Time.time - TimeBegin;
            if (TimePassed <= AnimateLength)
            {
                RenderSettings.fogDensity = Mathf.Lerp(0, 0.2f, TimePassed / AnimateLength);
            }*/
        }
    }
}