using UnityEngine;
using System.Collections;

namespace Zeltex.Util
{
    // Game Object Utilities
    public static class ObjectUtil
    {
        // utility
        public static void DestroyThing(GameObject ObjectToDestroy)
        {
    #if UNITY_EDITOR
			    //DestroyImmediate (ObjectToDestroy);
				    if (UnityEditor.EditorApplication.isPlaying)
					    GameObject.Destroy (ObjectToDestroy);
				    else
					    GameObject.DestroyImmediate (ObjectToDestroy);
    #else
            GameObject.Destroy(ObjectToDestroy);
    #endif
        }
        public static void DestroyThing(Component ObjectToDestroy)
        {
    #if UNITY_EDITOR
				    if (UnityEditor.EditorApplication.isPlaying)
					    GameObject.Destroy (ObjectToDestroy);
				    else
					    GameObject.DestroyImmediate (ObjectToDestroy);
    #else
            GameObject.Destroy(ObjectToDestroy);
    #endif
        }
    }
}