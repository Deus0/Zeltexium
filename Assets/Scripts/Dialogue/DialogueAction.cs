using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Zeltex.Dialogue
{
    /// <summary>
    /// An action that contains function information for the dialogue system
    /// Example: /Action GiveItem(TopHat)
    /// </summary>
    [System.Serializable]
    public class DialogueAction
    {
        public static string BeginTag = "/Action";
        public string Name = "";    // GiveItem, GiveQuest, AttackCharacter, etc
        public string Input1 = "";

        public DialogueAction()
        {
            // new empty action
        }

        public DialogueAction(string Data)
        {
            int IndexOf = Data.IndexOf('(');
            //int IndexOf2 = Data.IndexOf(')');
            Name = Data.Substring(0, IndexOf);
            Input1 = Data.Substring(IndexOf + 1, Data.Length - 1 - (IndexOf + 1));
            Debug.Log("Created new Action:" + Name + ":" + Input1);
        }

        public string GetScript()
        {
            return BeginTag + " " + Name + "(" + Input1 + ")";
        }
    }
}