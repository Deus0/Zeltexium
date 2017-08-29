using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex.Animations
{
    public class CameraIdle : MonoBehaviour
    {
        public Vector3 SinAmplitude = new Vector3(0.05f, 0.2f, 0.05f);
        public Vector3 NoiseAmplitude = new Vector3(0.03f, 0.03f, 0.03f);
        public float TimeScale = 0.2f;
        private Vector3 Rotation;
        private Vector3 NewRotation;
        private Vector3 Noise;

        private void Start()
        {
            Rotation = transform.localRotation.eulerAngles;
        }

        // Update is called once per frame
        void Update()
        {
            Noise.Set(
                Mathf.PerlinNoise(NoiseAmplitude.x, TimeScale * Time.time), 
                Mathf.PerlinNoise(NoiseAmplitude.y, TimeScale * Time.time), 
                Mathf.PerlinNoise(NoiseAmplitude.z, TimeScale * Time.time));
            NewRotation.Set(
                Rotation.x + (SinAmplitude.x * Noise.x) * Mathf.Sin(TimeScale * Time.time),
                Rotation.y + (SinAmplitude.y * Noise.y) * Mathf.Sin(TimeScale * Time.time),
                Rotation.z + (SinAmplitude.z * Noise.z) * Mathf.Sin(TimeScale * Time.time));
            transform.localRotation = Quaternion.Euler(NewRotation);
        }
    }

}