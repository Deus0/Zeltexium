using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex.Cameras
{
    public class CameraIdle : MonoBehaviour
    {
        public Vector3 SinAmplitude = new Vector3(0.05f, 0.2f, 0.05f);
        public Vector3 NoiseAmplitude = new Vector3(0.03f, 0.03f, 0.03f);
        public float TimeScale = 0.2f;
        private Vector3 Rotation;
        private Vector3 NewRotation;
        private Vector3 Noise;
        private CameraMovement MyCameraMover;
        private float TimeMovementStopped;
        private float TimeToStartIdling = 5f;
        private bool IsIdling = true;

        private void Start()
        {
            MyCameraMover = GetComponent<CameraMovement>();
            BeginIdling();
        }

        // Update is called once per frame
        void Update()
        {
            if (MyCameraMover == null)
            {
                UpdateIdleMovement();
            }
            else
            {
                if (MyCameraMover.IsCameraMoving() == false)
                {
                    if (IsIdling)
                    {
                        UpdateIdleMovement();   // normal
                    }
                    else
                    {
                        // Check if has stopped moving for over 5 seconds
                        if (Time.time - TimeMovementStopped >= TimeToStartIdling)
                        {
                            BeginIdling();
                        }
                    }
                }
                else
                {
                    TimeMovementStopped = Time.time;    // referenced when movement stops
                    IsIdling = false;
                }
            }
        }

        private void BeginIdling()
        {
            IsIdling = true;
            Rotation = transform.localRotation.eulerAngles;
            TimeBegun = Time.time;
        }
        float CurrentTime;
        float TimeBegun;
        private void UpdateIdleMovement()
        {
            CurrentTime = TimeScale * (Time.time - TimeBegun);
            Noise.Set(
                Mathf.PerlinNoise(NoiseAmplitude.x, CurrentTime),
                Mathf.PerlinNoise(NoiseAmplitude.y, CurrentTime),
                Mathf.PerlinNoise(NoiseAmplitude.z, CurrentTime));
            NewRotation.Set(
                Rotation.x + (SinAmplitude.x * Noise.x) * Mathf.Sin(CurrentTime),
                Rotation.y + (SinAmplitude.y * Noise.y) * Mathf.Sin(CurrentTime),
                Rotation.z + (SinAmplitude.z * Noise.z) * Mathf.Sin(CurrentTime));
            transform.localRotation = Quaternion.Euler(NewRotation);
        }
    }

}