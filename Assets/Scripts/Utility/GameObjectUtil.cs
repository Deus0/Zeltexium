using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex.Util
{
    /// <summary>
    /// Just a few Object utility functions that I use alot
    /// </summary>
    public static class GameObjectUtil
    {
        /// <summary>
        /// Set the layers of the character to be viewer only by the viewer camera.
        /// </summary>
        public static void SetLayerRecursive(this GameObject Object, LayerMask Layer)
        {
            Object.layer = Mathf.RoundToInt(Mathf.Log(Layer.value, 2));
            for (int i = 0; i < Object.transform.childCount; i++)
            {
                SetLayerRecursive(Object.transform.GetChild(i).gameObject, Layer);
            }
        }

        /// <summary>
        /// Similar to the above function but using int
        /// </summary>
        public static void SetLayerRecursiveInt(this GameObject Object, int Layer)
        {
            Object.layer = Mathf.RoundToInt(Mathf.Log(1 << Layer, 2));   //1 << 
            for (int i = 0; i < Object.transform.childCount; i++)
            {
                SetLayerRecursive(Object.transform.GetChild(i).gameObject, Layer);
            }
        }
    }

}