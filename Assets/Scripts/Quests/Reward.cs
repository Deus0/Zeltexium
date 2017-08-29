using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Zeltex.Quests
{
    /// <summary>
    /// A reward for completing a quest
    /// Other rewards might be stats
    /// </summary>
    [System.Serializable]
    public class Reward
    {
        public bool IsInventory;
        public int ItemQuantity;
        public string ItemName;

        public string GetDescriptionText()
        {
            if (IsInventory)
            {
                return ItemName + " x" + ItemQuantity;
            }
            return "";
        }
        public List<string> GetScriptList()
        {
            List<string> MyScript = new List<string>();

            return MyScript;
        }
        public void RunScript(List<string> MyScript)
        {

        }
    }
}