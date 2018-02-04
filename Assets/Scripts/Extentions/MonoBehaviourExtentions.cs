using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex
{
    public static class MonoBehaviourExtension
    {
        public static void Die(this Component MyComponent)
        {
            if (MyComponent != null)
            {
                if (Application.isEditor && !Application.isPlaying)
                {
                    GameObject.DestroyImmediate(MyComponent);
                }
                else
                {
                    GameObject.Destroy(MyComponent);
                }
            }
        }
        /// <summary>
        /// Destroys the mono behaviour
        /// </summary>
        public static void MonoDie(this MonoBehaviour MyBehaviour)
        {
            if (MyBehaviour != null)
            {
                if (Application.isEditor && !Application.isPlaying)
                {
                    GameObject.DestroyImmediate(MyBehaviour);
                }
                else
                {
                    GameObject.Destroy(MyBehaviour);
                }
            }
        }
        public static void Die(this MonoBehaviour MyBehaviour)
        {
            if (MyBehaviour != null)
            {
                if (Application.isEditor && !Application.isPlaying)
                {
                    GameObject.DestroyImmediate(MyBehaviour);
                }
                else
                {
                    GameObject.Destroy(MyBehaviour);
                }
            }
        }

        public static void Kill(UnityEngine.Object MyObject)
        {
            if (MyObject != null)
            {
                if (Application.isEditor && !Application.isPlaying)
                {
                    GameObject.DestroyImmediate(MyObject);
                }
                else
                {
                    GameObject.Destroy(MyObject);
                }
            }
        }

        public static void Die(this GameObject MyObject)
        {
            if (MyObject != null)
            {
                if (Application.isEditor && !Application.isPlaying)
                {
                    GameObject.DestroyImmediate(MyObject);
                }
                else
                {
                    GameObject.Destroy(MyObject);
                }
            }
        }

        public static void Die(this GameObject MyObject, float TimeToDie)
        {
            if (MyObject != null)
            {
                if (Application.isEditor && !Application.isPlaying)
                {
                    RoutineManager.Get().StartCoroutine(DieInTime(MyObject, TimeToDie));
                }
                else
                {
                    GameObject.Destroy(MyObject, TimeToDie);
                }
            }
        }

        public static IEnumerator DieInTime(GameObject MyObject, float TimeToDie)
        {
            float TimeBegun = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - TimeBegun <= TimeToDie)
            {
                yield return null;
            }
            GameObject.DestroyImmediate(MyObject);
        }

        /// <summary>
        /// Primarily used to clean for meshes, textures etc
        /// </summary>
        public static void Die(this Object MyObject)
        {
            if (MyObject != null)
            {
                if (Application.isEditor && !Application.isPlaying)
                {
                    GameObject.DestroyImmediate(MyObject);
                }
                else
                {
                    GameObject.Destroy(MyObject);
                }
            }
        }
    }

}