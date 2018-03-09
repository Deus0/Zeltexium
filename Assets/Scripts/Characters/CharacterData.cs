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
using Zeltex.Voxels;

namespace Zeltex
{

    /// <summary>
    /// Holds all the data for a character
    ///     Skillbar
    ///     Stats
    ///     SkeletonData
    /// </summary>
    [System.Serializable]
    public class CharacterData : ElementCore
    {
        [Header("Data")]
        [JsonProperty]
        public string Class;
        [JsonProperty]
        public string Race;

        [JsonProperty]
        public Zexel MyZexel;
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

        // LevelTransform
        public LevelTransform Position;

        [JsonIgnore]
        public Inventory Equipment;
        [JsonIgnore]
        public Character MyCharacter;
        [JsonIgnore]
        public CharacterStats MyStatsHandler;
        // The path loaded file from disc
        [JsonIgnore]
        public string LoadPath = "";

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
            MyZexel = null;
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
            if (MyZexel != null)
            {
                MyZexel.ParentElement = this;
                MyZexel.OnLoad();
            }
            if (Position != null)
            {
                Position.ParentElement = this;
                Position.OnLoad();
            }
        }

        public void SetCharacter(Character NewCharacter, Level NewInLevel = null, bool IsSetTransform = true)
        {
            MyCharacter = NewCharacter;
            if (Position == null)
            {
                Position = new LevelTransform();
            }
            Position.SetLevel(NewInLevel);
            if (NewInLevel != null)
            {
                NewInLevel.AddCharacter(NewCharacter);
            }
            if (IsSetTransform)
            {
                Position.AttachComponent(MyCharacter);
            }
            else
            {
                Position.CheckTransformUpdated();
            }
        }

        #region Positioning

        public void RefreshTransform(bool IsInitial = false)
        {
            if (Position != null)
            {
                Position.CheckTransformUpdated();
            }
            else
            {
                Debug.LogError(Name + " Has no set character.");
            }
        }
        #endregion


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

        public void SetInLevel(Level NewInLevel)
        {
            Position.SetLevel(NewInLevel);
        }


        #region EditorSpawning

        public override void Spawn()
        {
            GameObject NewCharacter = new GameObject();
            NewCharacter.name = Name;
            MyCharacter = NewCharacter.AddComponent<Character>();
            MyCharacter.SetData(this, null, false, false);
        }

        public Character Spawn(Level MyLevel)
        {
            GameObject NewCharacter = new GameObject();
            NewCharacter.name = Name;
            MyCharacter = NewCharacter.AddComponent<Character>();
            MyCharacter.SetData(this, MyLevel, false, false);
            return MyCharacter;
        }

        public override void DeSpawn()
        {
            if (MyCharacter)
            {
                MyCharacter.Die();
            }
        }

        public override bool HasSpawned()
        {
            return (MyCharacter != null);
        }
        #endregion
    }

}