using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex
{
    /// <summary>
    /// Just a simple trigger for editor actions
    /// </summary>
    [System.Serializable]
    public class VoxelModelAction
    {
        public string VoxelModelName = "";
        /*[SerializeField]
        private bool IsTrigger;

        public bool IsTriggered()
        {
            if (IsTrigger)
            {
                IsTrigger = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Trigger()
        {
            IsTrigger = true;
        }*/
    }
}