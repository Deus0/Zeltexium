using UnityEngine;
using System.Collections;

namespace Zeltex.Util
{
    /// <summary>
    /// Simply debugs the frames per second.
    /// </summary>
    public class FPSDebugger : MonoBehaviour 
    {
        public bool IsDebug;
        public bool IsExtra = false;
        public KeyCode MyClearKey;
	    float CurrentTime = 0.0f;
        float LowestTime = -1;
        float HighestTime = -1;
        public Color32 NormalColor = Color.green;
        GUIStyle style;
        //float LastTime;
        //float LowestDelta = 1000;
        //float HighestDelta = 0;
        public Color32 LowFpsColor = Color.red;
        public Color32 MedFpsColor = Color.yellow;
        public Color32 HighFpsColor = Color.green;
        public Color32 SuperHighFpsColor = Color.grey;
        float LastWiped;
        [Header("FrameRate")]
        public int TargetFramRate = 30;
        public float LowestFps = 8;
        public float MediumFps = 15;
        public float GoodFps = 25;
        private static FPSDebugger instance;

        private void Awake()
        {
            instance = this;
        }

        public static FPSDebugger Get()
        {
            return instance;
        }

        void Start()
        {
		    //Application.targetFrameRate = TargetFramRate;
            int w = Screen.width, h = Screen.height;
            style = new GUIStyle();
            style.alignment = TextAnchor.UpperRight;
            style.fontSize = h * 2 / 100;
            LastWiped = Time.time;
        }

	    void Update()
	    {
            if (IsDebug)
            {
                if (Input.GetKeyDown(MyClearKey))
                {
                    ClearDebug();
                }
                CurrentTime = Time.deltaTime;
                //CurrentTime += (Time.deltaTime - CurrentTime) * 0.1f;
                //LastTime = Time.deltaTime;
                if (Time.time - LastWiped >= 20)
                {
                    LastWiped = Time.time;
                    ClearDebug();
                }
            }
        }
	
	    void OnGUI()
	    {
            if (IsDebug)
            {
                DebugFPS();
                if (IsExtra)
                {
                    // DebugDelta();
                }
            }
        }
        private void ClearDebug()
        {
            HighestTime = -1;
            LowestTime = -1;
            //LastTime = 0;
            //HighestDelta = 0;
            //LowestDelta = 1000;
        }
        private void DebugFPS()
        {
            if (CurrentTime == 0)
                return;
            int w = Screen.width, h = Screen.height;
            if (CurrentTime > HighestTime || HighestTime == -1)
            {
                HighestTime = CurrentTime;
            }
            if (CurrentTime < LowestTime || LowestTime == -1)
            {
                LowestTime = CurrentTime;
            }
            float fps = 1.0f / CurrentTime;
            if (fps < LowestFps)
                style.normal.textColor = LowFpsColor;
            else if (fps < MediumFps)
                style.normal.textColor = MedFpsColor;
            else if (fps < GoodFps)
                style.normal.textColor = HighFpsColor;
            else
                style.normal.textColor = SuperHighFpsColor;

            Rect rect = new Rect(0, 0, w, h * 2 / 100);
            //	style.normal.textColor = new Color (0.0f, 0.0f, 0.5f, 1.0f);
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", CurrentTime * 1000.0f, fps);
            GUI.Label(rect, text, style);
            if (IsExtra)
            {
                rect.y += 20;
                style.normal.textColor = NormalColor;
                float LowestTime2 = LowestTime * 1000f;
                float HighestTime2 = HighestTime * 1000f;
                GUI.Label(rect, string.Format("Low: {0:0.0} ms ({1:0.} fps)", LowestTime2, 1f / LowestTime), style);
                rect.y += 20;
                GUI.Label(rect, string.Format("High: {0:0.0} ms ({1:0.} fps)", HighestTime2, 1f / HighestTime), style);
            }
        }
        /*private void DebugDelta()
        {
            int w = Screen.width, h = Screen.height;
            Rect rect = new Rect(0, 0, w, h * 2 / 100);
            rect.y += 20;
            rect.y += 20;
            rect.y += 20;
            GUI.Label(rect, string.Format("Delta: {0:0.000} ms", 1000 * LastTime), style);
            rect.y += 20;
            if (LastTime < LowestDelta)
            {
                LowestDelta = LastTime;
            }
            if (LastTime > HighestDelta)
            {
                HighestDelta = LastTime;
            }
            GUI.Label(rect, string.Format("LowDelta: {0:0} ms", 1000 * LowestDelta), style);
            rect.y += 20;
            GUI.Label(rect, string.Format("HighDelta: {0:0} ms", 1000 * HighestDelta), style);
            rect.y += 20;
        }*/
    }
}