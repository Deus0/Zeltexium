using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex
{
    [System.Serializable]
    public class ElementAction
    {
        public string DataName = "";
    }

    [System.Serializable]
    public class VoxelAction : ElementAction { }

    /// <summary>
    /// Just a simple trigger for editor actions
    /// </summary>
    [System.Serializable]
    public class PolyModelAction
    {
        public string PolyName = "";
        public int TextureMapIndex = 0;
        // public string TextureMapName = "";
    }
}