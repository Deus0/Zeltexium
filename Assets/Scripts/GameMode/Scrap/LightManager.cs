using UnityEngine;
using System.Collections;

namespace Zeltex.Game
{
    /// <summary>
    /// Manages directional lighting
    /// </summary>
    public class LightManager : MonoBehaviour
    {
        #region Variables
        // Level stuff
        public bool IsFog = true;
        public Light MySunlight;
        public Color LoadedColor = new Color(28 / 255f, 6 / 255f, 10 / 255f);
        public float LoadedIntensity = 0.4f;
        public Color MainMenuColor = new Color(210 / 255f, 255 / 255f, 255 / 255f);
        public float MainMenuIntesity = 0.7f;
        #endregion

        #region Mono
        void Start()
        {
            //Debug.Log("StartUp [" + Time.realtimeSinceStartup + "] Setting Lights in Map Maker.");
            RenderSettings.fog = IsFog;
            SetLights(false);
        }
        #endregion

        #region Lights
        public void UpdateLightIntensity(float MyIntensity)
        {
            //RenderSettings.ambientLight = Color.white * MyIntensity;
            //RenderSettings.fogColor = Color.white * MyIntensity;
            //Camera.main.backgroundColor = Color.white * MyIntensity;
            StopCoroutine(LerpLights(MyIntensity, Color.white * MyIntensity));
            StartCoroutine(LerpLights(MyIntensity, Color.white * MyIntensity));
        }
        public void EnableLights()
        {
            SetLights(true);
        }
        public void DisableLights()
        {
            SetLights(false);
        }
        /// <summary>
        /// Sets the lights to different modes
        /// </summary>
        private void SetLights(bool IsLoaded)
        {
            float NewIntensity = LoadedIntensity;
            Color32 NewColor = LoadedColor;
            if (IsLoaded == false)
            {
                NewIntensity = MainMenuIntesity;
                NewColor = MainMenuColor;
            }
            StopCoroutine(LerpLights(NewIntensity, NewColor));
            StartCoroutine(LerpLights(NewIntensity, NewColor));
        }
        public IEnumerator LerpLights(float NewIntensity, Color32 NewColor)
        {
            float LerpTime = 10;
            float TimeBegin = Time.realtimeSinceStartup;
            float TimePassed = Time.realtimeSinceStartup - TimeBegin;
            float LerpValue = TimePassed / LerpTime;
            bool IsLooping = true;
            float OriginalIntensity = RenderSettings.ambientIntensity;
            Color32 OriginalColor = RenderSettings.fogColor;
            while (IsLooping)
            {
                TimePassed = Time.realtimeSinceStartup - TimeBegin;
                LerpValue = TimePassed / LerpTime;
                Color32 LerpedColor = Color.Lerp(OriginalColor, NewColor, LerpValue);
                MySunlight.intensity = Mathf.Lerp(OriginalIntensity, NewIntensity, LerpValue);
                RenderSettings.ambientIntensity = MySunlight.intensity;
                //MySunlight.color = LerpedColor;
                RenderSettings.fogColor = LerpedColor;
                Camera.main.backgroundColor = LerpedColor;
                //Debug.Log("New Lerp value: " + LerpValue + ": TimePassed: " + TimePassed);
                if (TimePassed >= LerpTime)
                {
                    //Debug.Log("Ending Lerp!");
                    IsLooping = false;
                    yield break;
                }
                yield return null;
            }
        }
        #endregion
    }
}