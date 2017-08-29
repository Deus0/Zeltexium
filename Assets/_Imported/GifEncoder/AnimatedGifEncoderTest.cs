using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GifEncoder;
using System;
using System.IO;
using Zeltex.Skeletons;

namespace Zeltex.Guis
{
    /// <summary>
    /// A class that encodes gifs
    /// </summary>
    public class AnimatedGifEncoderTest : MonoBehaviour
    {
        public bool IsDebug;
        public bool IsSkeletonViewer = false;
        public SkeletonViewer MyViewer;
        public Camera MyRenderCamera;
        public int SecondsCount = 2;
        public int FramesPerSecond = 24;
        private AnimatedGifEncoder gifEncoder;
        bool IsCapturing;
        public string FileName = "Output";
        //public RenderTexture renderTexture;
        public List<Texture2D> MyTextures;
        public bool IsTimedMode = false;
        int FramesCount = 0;
        int ProcessCount = 0;
        bool IsProcessing = false;
        float LastCaptureTime;

        public void Update()
        {
            if (IsCapturing == false)
            {
                if (IsDebug && Input.GetKeyDown(KeyCode.G))
                {
                    StartCapturing();
                }
            }
            else
            {
                if (IsTimedMode == false && IsDebug && Input.GetKeyDown(KeyCode.G))
                {
                    FinishCapturing();
                }
            }
        }
        void OnPostRender()
        {
            if (IsCapturing)
            {
                float TimePassed = Time.time - LastCaptureTime;
                float TimePerCapture = 1f / ((float)FramesPerSecond);
                if (TimePassed >= TimePerCapture)
                {
                    LastCaptureTime = Time.time;
                    CaptureGif();
                }
            }
        }

        void OnGUI()
        {
            if (IsCapturing)
            {
                GUILayout.Label("Capturing: " + FramesCount + " / " + (FramesPerSecond * SecondsCount));
                if (IsProcessing)
                {
                    GUILayout.Label("Processing: " + ProcessCount);
                }
                else
                {
                    GUILayout.Label("Finished processing gif: " + FileName + ".gif");
                }
            }
        }

        private void StartCapturing()
        {
            if (IsSkeletonViewer)
            {
                MyRenderCamera = MyViewer.GetRenderCamera();
            }
            StartedProcess = false;
            LastCaptureTime = Time.time;
            MyTextures.Clear();
            IsCapturing = true;
            FramesCount = 0;
            // Create a GIF encoder:
            string MyFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), FileName + ".gif");
            int i = 0;
            while (File.Exists(MyFileName))
            {
                MyFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), FileName + "_" + i + ".gif");
                i++;
            }
            this.gifEncoder = new AnimatedGifEncoder(MyFileName);
            this.gifEncoder.SetDelay(1000 / 30);
        }
        bool StartedProcess = false;
        public void FinishCapturing()
        {
            if (StartedProcess == false)
            {
                StartedProcess = true;
                IsProcessing = true;
                StartCoroutine(ProcessGif());
            }
        }
        public IEnumerator ProcessGif()
        {
            for (int i = 0; i < MyTextures.Count; i++)
            {
                ProcessCount++;
                // Add the current frame to the GIF:
                this.gifEncoder.AddFrame(MyTextures[i]);
                yield return new WaitForSeconds(0.1f);
            }
            this.gifEncoder.Finish();
            IsCapturing = false;
            yield return new WaitForSeconds(2f);
            IsProcessing = false;
        }
        private void CaptureGif()
        {
            if (IsTimedMode && FramesCount > SecondsCount * FramesPerSecond)
            {
                FinishCapturing();
            }
            else if (IsProcessing == false)
            {
                FramesCount++;
                RenderTexture MyRenderTexture = MyRenderCamera.targetTexture;
                MyRenderCamera.Render();
                RenderTexture.active = MyRenderTexture;
                MyTextures.Add(new Texture2D(MyRenderTexture.width, MyRenderTexture.height, TextureFormat.RGB24, false));
                MyTextures[MyTextures.Count - 1].ReadPixels(new Rect(0, 0, MyRenderTexture.width, MyRenderTexture.height), 0, 0);
                MyTextures[MyTextures.Count - 1].Apply();
            }
        }

        /*private void Quit()
        {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
        }*/
    }

}