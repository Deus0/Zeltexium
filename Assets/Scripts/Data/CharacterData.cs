using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Items;
using Zeltex.Combat;
using Zeltex.Quests;
using Zeltex.Dialogue;
using Zeltex.Skeletons;
using Zeltex.Guis.Characters;
using Newtonsoft.Json;
using Zeltex.Characters;

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
        [JsonProperty]
        public string Class = "";
        [JsonProperty]
        public string Race = "";

        [JsonProperty]
        public CharacterStats MyStats = new CharacterStats();

        [JsonProperty]
        public Inventory Skillbar = new Inventory();
        [JsonProperty]
        public Inventory Equipment = new Inventory();
        [JsonProperty]
        public Inventory Backpack = new Inventory();

        [JsonProperty]
        public QuestLog MyQuestLog = new QuestLog();
        [JsonProperty]
        public DialogueTree MyDialogue = new DialogueTree();
        [JsonProperty]
        public Skeleton MySkeleton = new Skeleton();
        [JsonProperty]
        public List<Zanimation> MyAnimations = new List<Zanimation>();
        [JsonProperty]
        public CharacterGuis MyGuis = new CharacterGuis();
        [JsonProperty]
        public AI.BotMeta BotData = new AI.BotMeta();

        // respawn data
        [JsonProperty]
        public bool CanRespawn = true;  // can player respawn after death

        // Level Data
        [JsonProperty]
        public string LevelInsideOf = "";
        [JsonProperty]
        public Vector3 LevelPosition;
        [JsonProperty]
        public Vector3 LevelRotation;
        [JsonIgnore]
        public Character MyCharacter;

        public void SetCharacter(Character NewCharacter)
        {
            MyCharacter = NewCharacter;
            if (MyCharacter)
            {
                MyCharacter.transform.position = LevelPosition;
                MyCharacter.transform.eulerAngles = LevelRotation;
            }
        }

        public void Update()
        {
            if (MyCharacter)
            {
                LevelPosition = MyCharacter.transform.position;
                LevelRotation = MyCharacter.transform.eulerAngles;
            }
        }

        public void Clear()
        {
            MyStats.Clear();
            MyQuestLog.Clear();
            MyDialogue.Clear();
            Skillbar.Clear();
            Backpack.Clear();
            Equipment.Clear();
        }

        public void OnInitialized()
        {
            for (int i = Skillbar.MyItems.Count; i < 5; i++)
            {
                Skillbar.MyItems.Add(new Item());
            }
            for (int i = Backpack.MyItems.Count; i < 20; i++)
            {
                Backpack.MyItems.Add(new Item());
            }
        }
    }

}