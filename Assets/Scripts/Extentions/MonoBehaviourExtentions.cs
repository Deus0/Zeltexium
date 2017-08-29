using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex
{
    public static class MonoBehaviourExtension
    {
        public static void Die(this MonoBehaviour MyBehaviour)
        {
            if (MyBehaviour != null)
            {
                if (Application.isEditor && !Application.isPlaying)
                {
                    GameObject.DestroyImmediate(MyBehaviour.gameObject);
                }
                else
                {
                    GameObject.Destroy(MyBehaviour.gameObject);
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
    }

}