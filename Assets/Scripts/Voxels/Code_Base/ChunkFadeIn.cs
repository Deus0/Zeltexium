using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex.Voxels
{
    /// <summary>
    /// Fades chunk in
    /// </summary>
    public class ChunkFadeIn : MonoBehaviour
    {
        MeshRenderer MyMeshRenderer;
        float TimeBegun;
        float FadeTime = 2.8f;

        void Start()
        {
            MyMeshRenderer = GetComponent<MeshRenderer>();
            TimeBegun = Time.time;
            //Debug.Log("Chunk [" + name + "] Has begun to fade in at " + TimeBegun);
        }
        public void Begin()
        {
            enabled = true;
            TimeBegun = Time.time;
            //Debug.Log("Chunk [" + name + "] Has begun to fade in at " + TimeBegun);
        }

        // Update is called once per frame
        void Update()
        {
            enabled = false;
            /*float TimePassed = Time.time - TimeBegun;
            float LerpValue = TimePassed / FadeTime;
            MyMeshRenderer.material.color = Color32.Lerp(
                new Color32(255, 255, 255, 0), 
                new Color32(255, 255, 255, 255), 
                LerpValue);
            if (TimePassed > FadeTime)
            {
                enabled = false;
            }*/
        }
    }

}