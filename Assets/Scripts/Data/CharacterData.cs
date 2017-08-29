using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Items;
using Zeltex.Combat;
using Zeltex.Quests;
using Zeltex.Dialogue;

namespace Zeltex
{
    /// <summary>
    /// Holds all the data for a character
    ///     Skillbar
    ///     Stats
    ///     SkeletonData
    /// </summary>
    [System.Serializable]
    public class CharacterData : Element
    {
        [Header("Data")]
        public string Class = "";
        public string Race = "";
        public Inventory Skillbar = new Inventory();
        public Inventory Equipment = new Inventory();
        public CharacterStats MyStats = new CharacterStats();
        public QuestLog MyQuestLog = new QuestLog();
        public DialogueTree MyDialogue = new DialogueTree();
        // respawn data
        public bool CanRespawn = true;  // can player respawn after death
    }

}