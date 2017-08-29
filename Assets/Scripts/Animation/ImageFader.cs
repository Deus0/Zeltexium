using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Zeltex
{
    /// <summary>
    /// A quick screen fader
    /// </summary>
    public class ImageFader : MonoBehaviour
    {
        private static ImageFader Instance;
        [SerializeField]
        private bool IsFadeInAtStart = true;
        [SerializeField]
        private float LerpTime = 3;
        [SerializeField]
        private float PauseTime = 3;
        [SerializeField]
        private RawImage EventImage;
        [SerializeField]
        private Color BeginColor = Color.black;
        [SerializeField]
        private Color EndColor = new Color(0, 0, 0, 0);
        [SerializeField]
        private Color BeginFontColor = Color.cyan;
        private bool IsReverse;
        public Text LoadingText;
        public bool IsFading;

        public static ImageFader Get()
        {
            return Instance;
        }

        void Start()
        {
            Instance = this;
            if (IsFadeInAtStart)
            {
                FadeIn();
            }
        }

        /// <summary>
        /// Used by loading function
        /// </summary>
        public void FadeOut(float FadeOutTime)
        {
            LerpTime = FadeOutTime;
            PauseTime = 0;
            LoadingText.enabled = true;
            LoadingText.color = EndColor;
            FadeOut();
        }

        public void FadeIn(float FadeOutTime)
        {
            LerpTime = FadeOutTime;
            PauseTime = 0;
            FadeIn();
        }


        // normal fades
        public void FadeIn()
        {
            IsReverse = false;
            StopCoroutine(LerpImage());
            StartCoroutine(LerpImage());
        }

        public void FadeOut()
        {
            IsReverse = true;
            StopCoroutine(LerpImage());
            StartCoroutine(LerpImage());
        }

        public IEnumerator LerpImage()
        {
            IsFading = true;
            float TimeBegin;
            float TimePassed;
            float LerpValue;
            if (LoadingText.enabled && !IsReverse)
            {
                TimeBegin = Time.realtimeSinceStartup;
                TimePassed = 0;
                while (TimePassed < LerpTime)
                {
                    LerpValue = TimePassed / LerpTime;
                    LoadingText.color = Color.Lerp(BeginFontColor, EndColor, LerpValue);
                    yield return null;
                    TimePassed = Time.realtimeSinceStartup - TimeBegin;
                }
                LoadingText.enabled = false;
            }

            // normal fader
            TimeBegin = Time.realtimeSinceStartup + PauseTime;
            TimePassed = Time.realtimeSinceStartup - TimeBegin;
            while (TimePassed < LerpTime)
            {
                LerpValue = TimePassed / LerpTime;
                //Debug.Log("New Lerp value: " + LerpValue + ": TimePassed: " + TimePassed);
                if (IsReverse == false)
                {
                    EventImage.color = Color.Lerp(BeginColor, EndColor, LerpValue);
                }
                else
                {
                    EventImage.color = Color.Lerp(EndColor, BeginColor, LerpValue);
                }
                yield return null;
                TimePassed = Time.realtimeSinceStartup - TimeBegin;
            }
            if (IsReverse == false)
            {
                EventImage.color = EndColor;
            }
            else
            {
                EventImage.color = BeginColor;
            }

            // Fade in text after image
            if (LoadingText.enabled && IsReverse)
            {
                TimeBegin = Time.realtimeSinceStartup;
                TimePassed = 0;
                while (TimePassed < LerpTime)
                {
                    LerpValue = TimePassed / LerpTime;
                    LoadingText.color = Color.Lerp(EndColor, BeginFontColor, LerpValue);
                    yield return null;
                    TimePassed = Time.realtimeSinceStartup - TimeBegin;
                }
            }
            IsFading = false;
            //gameObject.SetActive(false);
        }
    }

}