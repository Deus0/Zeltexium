using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Items;
using Zeltex.Combat;
using Zeltex.Quests;
using Zeltex.Dialogue;
using Zeltex.Skeletons;
using Zeltex.Guis.Characters;
using Zeltex.Characters;
using Zeltex.AI;
using Newtonsoft.Json;

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
        public string Class;
        [JsonProperty]
        public string Race;

        [JsonProperty]
        public Stats MyStats;

        [JsonProperty]
        public Inventory Skillbar;
        [JsonProperty]
        public Inventory Backpack;

        [JsonProperty]
        public QuestLog MyQuestLog;
        [JsonProperty]
        public DialogueTree MyDialogue;
        [JsonProperty]
        public Skeleton MySkeleton;
        [JsonProperty]
        public List<Zanimation> MyAnimations;
        [JsonProperty]
        public CharacterGuis MyGuis;
        [JsonProperty]
        public BotMeta BotData;

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
        public Inventory Equipment;
        [JsonIgnore]
        public Character MyCharacter;
        [JsonIgnore]
        public CharacterStats MyStatsHandler;

        public CharacterData()
        {
            Class = "";
            Race = "";
            MyStats = new Stats();
            MyStatsHandler = new CharacterStats();
            Skillbar = new Inventory();
            Equipment = new Inventory();
            Backpack = new Inventory();
            MyQuestLog = new QuestLog();
            MyDialogue = new DialogueTree();
            MySkeleton = new Skeleton();
            MyAnimations = new List<Zanimation>();
            MyGuis = new CharacterGuis();
            BotData = new BotMeta();
            MyStats.ParentElement = this;
            Skillbar.ParentElement = this;
            Equipment.ParentElement = this;
            Backpack.ParentElement = this;
            MyQuestLog.ParentElement = this;
            MyDialogue.ParentElement = this;
            MySkeleton.ParentElement = this;
            MyGuis.ParentElement = this;
            BotData.ParentElement = this;
        }

        public override void OnLoad()
        {
            base.OnLoad();
            MyStats.ParentElement = this;
            Skillbar.ParentElement = this;
            Equipment.ParentElement = this;
            Backpack.ParentElement = this;
            MyQuestLog.ParentElement = this;
            MyDialogue.ParentElement = this;
            MySkeleton.ParentElement = this;
            MyGuis.ParentElement = this;
            BotData.ParentElement = this;
            MyStats.OnLoad();
            Skillbar.OnLoad();
            Equipment.OnLoad();
            Backpack.OnLoad();
            MyQuestLog.OnLoad();
            MyDialogue.OnLoad();
            MySkeleton.OnLoad();
            MyGuis.OnLoad();
            BotData.OnLoad();
            if (MyCharacter)
            {
                LevelPosition = MyCharacter.transform.position;
                LevelRotation = MyCharacter.transform.eulerAngles;
            }
        }

        public void SetCharacter(Character NewCharacter, bool IsSetTransform = true)
        {
            MyCharacter = NewCharacter;
            if (MyCharacter && IsSetTransform)
            {
                if (IsSetTransform)
                {
                    MyCharacter.transform.position = LevelPosition;
                    MyCharacter.transform.eulerAngles = LevelRotation;
                }
                else
                {
                    LevelPosition = MyCharacter.transform.position;
                    LevelRotation = MyCharacter.transform.eulerAngles;
                }
            }
        }

        public void RefreshTransform()
        {
            if (MyCharacter)
            {
                if (MyCharacter.transform.position.x != LevelPosition.x
                    || MyCharacter.transform.position.y != LevelPosition.y
                    || MyCharacter.transform.position.z != LevelPosition.z)
                {
                    LevelPosition = MyCharacter.transform.position;
                    OnModified();
                }
                if (MyCharacter.transform.eulerAngles.x != LevelRotation.x
                    || MyCharacter.transform.eulerAngles.y != LevelRotation.y
                    || MyCharacter.transform.eulerAngles.z != LevelRotation.z)
                {
                    LevelRotation = MyCharacter.transform.eulerAngles;
                    OnModified();
                }
            }
            else
            {
                Debug.LogError(Name + " Has no set character.");
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
                Item NewItem = new Item();
                NewItem.SetParentInventory(Skillbar);
                Skillbar.MyItems.Add(NewItem);
            }
            for (int i = Backpack.MyItems.Count; i < 20; i++)
            {
                Item NewItem = new Item();
                NewItem.SetParentInventory(Backpack);
                Backpack.MyItems.Add(NewItem);
            }
        }

        public Inventory GetEquipment() 
        {
            Equipment.Clear();
            for (int i = 0; i < MySkeleton.MyBones.Count; i++)
            {
                if (MySkeleton.MyBones[i].HasItem())
                {
                    MySkeleton.MyBones[i].MyItem.ParentElement = MySkeleton.MyBones[i];
                    Equipment.AddRaw(MySkeleton.MyBones[i].MyItem);
                    MySkeleton.MyBones[i].MyItem.SetParentInventory(Equipment);
                }
            }
            return Equipment;
        }
    }

}