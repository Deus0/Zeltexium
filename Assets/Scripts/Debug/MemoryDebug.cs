using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

namespace Zeltex.Util
{
    /// <summary>
    /// Debug memory useage!
    /// </summary>
    public class MemoryDebug : MonoBehaviour
    {
        string MyCPUUsage = "";
        string MyRamUseage = "";
        GUIStyle MyStyle;
        public string ProcessName;
        //Process MyProcess;
        public Color32 FontColor = Color.green;
        public GameObject MyWorld;
        public GameObject Characters;

        void Start()
        {
            Process MyProcess = Process.GetCurrentProcess();
            ProcessName = MyProcess.ProcessName;
            int w = Screen.width, h = Screen.height;
            MyStyle = new GUIStyle();
            MyStyle.alignment = TextAnchor.UpperRight;
            MyStyle.fontSize = h * 2 / 100;
            MyStyle.normal.textColor = FontColor;
            MyRect = new Rect(0, 0, w, h * 2 / 100);
        }
        Rect MyRect;
        void OnGUI()
        {
            /*GUILayout.Label("All " + FindObjectsOfType(typeof(UnityEngine.Object)).Length);
            GUILayout.Label("GameObjects " + FindObjectsOfType(typeof(GameObject)).Length);
            GUILayout.Label("Components " + FindObjectsOfType(typeof(Component)).Length);
            GUILayout.Label("Textures " + FindObjectsOfType(typeof(Texture)).Length);
            GUILayout.Label("AudioClips " + FindObjectsOfType(typeof(AudioClip)).Length);
            GUILayout.Label("Materials " + FindObjectsOfType(typeof(Material)).Length);
            GUILayout.Label("Worlds " + FindObjectsOfType(typeof(Zeltex.Voxels.World)).Length);
            GUILayout.Label("Characters " + FindObjectsOfType(typeof(Zeltex.Characters.Character)).Length);
            GUILayout.Label("Meshes " + FindObjectsOfType(typeof(Mesh)).Length);*/

            MyRect.y = 80;
            GUI.Label(MyRect, "Process: " + ProcessName, MyStyle);
            //MyRect.y += 20;
            //GUI.Label(MyRect, "CPU: " + MyCPUUsage + "%", MyStyle);
            
            MyRect.y += 20;
            GUI.Label(MyRect, "Total Memory: " + (UnityEngine.Profiling.Profiler.GetTotalReservedMemory() / 1000000).ToString() + "MB", MyStyle);
            //MyRect.y += 20;
            //GUI.Label(MyRect, "Allocated Memory: " + (UnityEngine.Profiling.Profiler.GetMonoHeapSize()/1000000).ToString() + "MB", MyStyle);

            MyRect.y += 20;
            GUI.Label(MyRect, "Allocated Memory: " + (UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory() / 1000000).ToString() + "MB", MyStyle);
            //MyRect.y += 20;
            //GUI.Label(MyRect, "Reserved Memory: " + (UnityEngine.Profiling.Profiler.GetTotalReservedMemory() / 1000000).ToString() + "MB", MyStyle);

            /*MyRect.y += 40;
            GUI.Label(MyRect, "World Memory: " + (UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(MyWorld) / 1000000).ToString() + "MB", MyStyle);
            MyRect.y += 20;
            GUI.Label(MyRect, "Characters Memory: " + (UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(Characters) / 1000000).ToString() + "MB", MyStyle);*/

        }

        void Update()
        {
            MyRamUseage = "" + (System.GC.GetTotalMemory(false)/1000000).ToString();
           // MyCPUUsage = (MyProcess.UserProcessorTime.TotalSeconds / MyProcess.TotalProcessorTime.TotalSeconds).ToString();
        }
    }
}
//MyCPUUsage = "" + MyCpuCounter.NextValue();
//MyRamUseage = MyRamCounter.NextValue();
//PerformanceCounter MyCpuCounter;
//PerformanceCounter MyRamCounter;
/*MyCpuCounter = new PerformanceCounter(
    "Processor",
     "% Processor Time",
    ProcessName); //"0,0");
MyRamCounter = new PerformanceCounter(
    "Memory", 
    "Available MBytes",
    ProcessName);*/
