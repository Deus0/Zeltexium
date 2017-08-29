using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex.Util
{
    /// <summary>
    ///  a custom webgl button
    /// </summary>
    public class WebglHandler : MonoBehaviour
    {
        bool IsFullScreen;
        int OriginalWidth;
        int OriginalHeight;
        bool CanToggle = true;
        
        //Application.ExternalCall("SetFullscreen", 1);
        void Start()
        {
            OriginalWidth = Screen.width;
            OriginalHeight = Screen.height;
            // if not webgl, hide button
#if UNITY_WEBGL

#else
            gameObject.SetActive(false);
#endif
#if UNITY_EDITOR
            gameObject.SetActive(false);
#endif
        }

        void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                CanToggle = true;
            }
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                IsFullScreen = false;
                Screen.fullScreen = false;
                Debug.Log("Escaping Toggling mode: " + IsFullScreen);
            }
        }
        public void ToggleScreen()
        {
            if (CanToggle)
            {
                CanToggle = false;
#if UNITY_WEBGL
                if (IsFullScreen == false) 
                {
                    IsFullScreen = true;
                    Screen.fullScreen = true;
                    Application.ExternalEval("SetFullscreen(1)");
                    Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, true, 30);
                }
                else 
                {
                    Screen.fullScreen = false;
                    IsFullScreen = false;
                    Application.ExternalEval("SetFullscreen(0)");
                    //Screen.SetResolution(OriginalWidth, OriginalHeight, true, 30);
                }
#endif
                Debug.Log("Toggling screen mode: " + IsFullScreen);
            }
        }
    }
}