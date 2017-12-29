using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using Zeltex.Util;
using Zeltex.AI;
using Zeltex.Items;
using Zeltex.Characters;
using Newtonsoft.Json;

namespace Zeltex.Combat
{
	/// <summary>
    /// Stats attached to the character.
    /// Things to do:
    ///     Talent Points - Add them to attributes
    ///     When an attribute is added or decreased - add to the corresponding base stats
    /// </summary>
    [System.Serializable]
	public class CharacterStats
    {
        #region Variables
        [JsonIgnore]
        public EditorAction ActionLoadStats = new EditorAction();
        [JsonIgnore]
        public string ActionStatsName = "";
        // statics
        [JsonIgnore]
        public static float ExperienceMultiplier = 1.5f;
        [JsonIgnore]
        private static float BaseSpeed = 12.5f;
        [JsonIgnore]
        private static float SpeedMultiplier = 2.5f;

        public bool DebugGui = false;
        [Header("References")]
        [JsonIgnore]
        public AudioSource MySource;
        [JsonIgnore, HideInInspector]
        public Inventory MyEquipment;                   // my equipment inventory
        [JsonIgnore]
        [HideInInspector]
        public Stats EquipmentStats = new Stats();      // this is equal to all the inventory items
        [JsonIgnore]
        private Character MyCharacter;
        [JsonIgnore]
        private BasicController MyBasicController;

        [Header("Gameplay")]
        [JsonIgnore, SerializeField]
        private bool IsInflictHeightDamage = true;
        [JsonIgnore, HideInInspector]
        private Vector3 MyPosition;
        [JsonIgnore, HideInInspector]
        private float LastInflictedDamage;

        [JsonIgnore]//, HideInInspector]
        public EventObject OnDeath = new EventObject();

        [Header("Assets")]
        [JsonIgnore]
        public AudioClip OnDeathSound;
        [JsonIgnore]
        public GameObject LevelUpEffectsPrefab;
        [JsonIgnore]
        public Font PopupFont;
        public Stats MyStats;
        #endregion

        #region Mono
        public CharacterStats()
        {

        }

        /// <summary>
        /// Sets the character and refreshes it
        /// </summary>
        public void SetCharacter(Character NewCharacter)
        {
            if (MyCharacter == null || MyCharacter != NewCharacter)
            {
                MyCharacter = NewCharacter;
                if (MyCharacter)
                {
                    MyStats = MyCharacter.GetData().MyStats;
                    MySource = MyCharacter.GetComponent<AudioSource>();
                    MyBasicController = MyCharacter.GetComponent<BasicController>();
                    MyPosition = MyCharacter.transform.position;
                }
                else
                {
                    MyStats = null;
                }
                if (MyStats != null)
                {
                    for (int i = 0; i < MyStats.GetSize(); i++)
                    {
                        MyStats.GetStat(i).ResetTimer();
                    }
                }
            }
        }

        public void UpdateScript()
        {
            if (Alive())
            {
                RegenStats();
                InflictOutOfBoundsDamage();
            }
        }

        /// <summary>
        /// The debug gui
        /// </summary>
        public void OnGUI()
        {
            if (DebugGui)
            {
                GUI.contentColor = Color.magenta;  // Apply Red color to Button
                List<string> MyDebugList = GetDebugStats();
                for (int i = 0; i < MyDebugList.Count; i++)
                {
                    GUILayout.Label(MyDebugList[i]);
                }
            }
        }

        /// <summary>
        /// A list of strings used to debug.
        /// </summary>
        public List<string> GetDebugStats()
        {
            List<string> MyStatInfo = new List<string>();
            MyStatInfo.Add(MyCharacter.name + " has " + MyStats.GetSize() + " stats.");
            for (int i = 0; i < MyStats.GetSize(); i++)
            {
                MyStatInfo.Add((i + 1) + " : " + MyStats.GetStat(i).GuiString());
                MyStatInfo.Add("\t" + MyStats.GetStat(i).GetDescription());
                string Values = "";
                for (int j = 0; j < MyStats.GetStat(i).GetValues().Count; j++)
                {
                    Values += MyStats.GetStat(i).GetValues()[j] + ", ";
                }
                MyStatInfo.Add("\t" + Values);
            }
            return MyStatInfo;
        }
        
        /// <summary>
        /// If Character is out of bounds, will kill them off
        /// </summary>
        private void InflictOutOfBoundsDamage()
        {
            if (IsInflictHeightDamage && MyPosition != MyCharacter.transform.position)
            {
                MyPosition = MyCharacter.transform.position;
                if (MyCharacter.GetWorldInsideOf())
                {
                    if (MyPosition.y <= MyCharacter.GetWorldInsideOf().transform.position.y - 4)
                    {
                        if (Time.time - LastInflictedDamage >= 1f)
                        {
                            LastInflictedDamage = Time.time;
                            MyStats.AddStat("Health", -2f);  // decrease health!
                        }
                    }
                    else
                    {
                        LastInflictedDamage = Time.time;
                    }
                }
            }
        }
        #endregion

        // Things like regen, dots, and buffs. Stats that tick with time.
        #region TimedUpdates
        /// <summary>
        /// Regeneration updates
        /// </summary>
        public void RegenStats()
        {
            bool HasUpdatedStats = false;
            for (int i = MyStats.GetSize() - 1; i >= 0; i--)
            {
                if (MyStats.GetStat(i).GetStatType() == StatType.Regen)
                {
                    bool DidRegen = CheckRegen(i);
                    if (DidRegen)
                    {
                        HasUpdatedStats = true;
                    }
                }
                else if (MyStats.GetStat(i).GetStatType() == StatType.TemporaryRegen)
                {
                    bool DidDot = CheckDot(i);
                    if (DidDot)
                    {
                        HasUpdatedStats = true;
                    }
                }
                else if (MyStats.GetStat(i).GetStatType() == StatType.TemporaryModifier)
                {
                    bool DidBuff = CheckBuff(i);
                    if (DidBuff)
                    {
                        HasUpdatedStats = true;
                    }
                }
            }
            if (HasUpdatedStats)
            {
                MyStats.OnUpdateStats.Invoke();    // update gui of stats
            }
        }

        /// <summary>
        /// Returns true if regened a stat
        /// </summary>
        private bool CheckRegen(int i)
        {
            Stat RegenStat = MyStats.GetStat(i);
            if (RegenStat.HasTicked())
            {
                Stat ModifiedStat = MyStats.GetStat(RegenStat.GetModifyStatName());
                if (ModifiedStat != null)
                {
                    bool DidRegen = ModifiedStat.AddState(RegenStat.GetRegenValueModified());
                    if (DidRegen)
                    {
                        return true;
                    }
                }
                else
                {
                    Debug.LogWarning("Regen stat: " + RegenStat.Name + " could not find modifier stat: " + RegenStat.GetModifyStatName());
                }
            }
            return false;
        }

        /// <summary>
        /// Check if dot effects stats
        /// </summary>
        public bool CheckDot(int i)
        {
            Stat DotStat = MyStats.GetStat(i);
            if (MyStats.GetStat(i).HasTicked())
            {
                Stat ModifiedStat = MyStats.GetStat(DotStat.GetModifyStatName());
                if (ModifiedStat != null)
                {
                    bool DidRegen = ModifiedStat.AddState(DotStat.GetDotValue());
                    if (DidRegen)
                    {
                        OnUpdateStat(ModifiedStat);
                        return true;
                    }
                }
            }
            if (DotStat.HasExpired())
            {
                MyStats.Remove(DotStat);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if buff has expired
        /// </summary>
        public bool CheckBuff(int i)
        {
            Stat StatBuffer = MyStats.GetStat(i);
            if (StatBuffer.HasExpired())
            {
                //Debug.LogError("Checking Health: " + GetStat("Health").GetValue());
                // Reverse modification
                Stat BuffedStat = MyStats.GetStat(StatBuffer.GetModifyStatName());
                if (BuffedStat != null)
                {
                    float DecreaseModifierValue = -StatBuffer.GetValue();
                    Debug.Log(StatBuffer.Name + " is Debuffing - Modifier stat is: " + BuffedStat.Name + " was reduced by " + DecreaseModifierValue);
                    AddModifier(BuffedStat, DecreaseModifierValue);
                }
                MyStats.Remove(StatBuffer);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a modifier to the characters stats
        /// </summary>
        public void AddModifier(Stat MyModifierStat, float AdditionValue)
        {
            MyModifierStat.Add(AdditionValue);
            OnUpdateStat(MyModifierStat);
            // Apply Reverse Modifier - for example if strength was just decreased by 1, remove 5 health
            if (MyModifierStat.GetStatType() == StatType.Modifier)
            {
                Stat MyBaseStat = MyStats.GetStat(MyModifierStat.GetModifyStatName());
                if (MyBaseStat != null)
                {
                    float DecreaseValue = AdditionValue * MyModifierStat.GetModifierValue();
                    Debug.Log("Decreasing Base Stat: " + MyBaseStat.Name + " by [" + DecreaseValue + "] " +" from [" + MyBaseStat.GetValue() + "]");
                    MyBaseStat.Add(DecreaseValue);
                    OnUpdateStat(MyBaseStat);
                }
            }
        }
        #endregion

        // Updates to stats
        #region Updates
        /// <summary>
        /// Update a stat
        /// </summary>
        public void UpdateStat(string StatName, float State)
        {
            Stat MyStat = MyStats.GetStat(StatName);
            if (MyStat.GetState() != State)
            {
                MyStat.SetState(State);
                OnUpdateStat(MyStat);
                MyStats.OnUpdateStats.Invoke();
            }
        }

        /// <summary>
        /// Called when a stat is updated
        /// </summary>
        void OnAddStat(Stat MyStat)
        {
            // for base, or state, check if any modifiers work on them
            // if they do, take their base value first, then apply modifiers for their temp stats...
            if (MyStat.GetStatType() == StatType.TemporaryModifier)
            {
                Stat MyModifiedStat = MyStats.GetStat(MyStat.GetModifyStatName());
                //Debug.LogError("Adding: " + TempStats.GetStat(i).Name + " to " + MyStat.Name);
                if (MyModifiedStat != null)
                {
                    MyModifiedStat.Add(MyStat.GetValue());
                    //MyModifyStat.AddState(TempStats.GetStat(i).GetAdditionValue());    // normal operation, add it
                }
            }
            //Debug.LogError ("State: " + BaseStats.Data [i].GuiString ());
            else if (MyStat.GetStatType() == StatType.Modifier)
            {
                Stat MyModifiedStat = MyStats.GetStat(MyStat.GetModifyStatName());
                if (MyModifiedStat != null)
                {
                    MyModifiedStat.Add(MyStat.GetValue() * MyStat.GetModifierValue());
                }
            }
        }

        /// <summary>
        /// Called only when the stat is modified! (need their old values)
        /// </summary>
        public void OnUpdateStat(Stat MyStat)
        {
            if (MyStat.Name == "Speed")
            {
                UpdateMovementSpeed(MyStat.GetValue());
            }
            else if (MyStat.Name == "Health")
            {
                CheckForDeath(MyStat);
            }
        }

        void OnUpdateTempStats()
        {
            for (int i = 0; i < MyStats.GetSize(); i++)
            {
                OnUpdateStat(MyStats.GetStat(i));
            }
            MyStats.OnNewStats.Invoke();
        }
        #endregion

        #region StatsList

        /// <summary>
        /// Add a stat to the list
        /// </summary>
        public void AddStat(Stat NewStat)
        {
            if (MyStats.IsAlive)
            {
                Stat MyStat = MyStats.GetStat(NewStat.Name);
                if (MyStat == null)
                {
                    MyStat = NewStat;
                    Add(MyStat);
                    MyStat.ResetTimer();
                }
                else
                {
                    MyStat.AddState(NewStat.GetState());
                }
                OnUpdateStat(MyStat);
                MyStats.OnUpdateStats.Invoke();
            }
        }

        /// <summary>
        /// Adds a stat
        /// </summary>
        public bool Add(Stat MyStat)
        {
            bool DidAdd = MyStats.Add(MyStat);
            if (DidAdd)
            {
                OnAddStat(MyStat);
                OnUpdateStat(MyStat);
                MyStats.OnUpdateStats.Invoke();
            }
            return DidAdd;
        }
        #endregion

        #region Getters
        /// <summary>
        /// Returns true if stat exists
        /// </summary>
        public bool HasStat(string StatName, float StatNeeded)
        {
            if (!MyStats.IsAlive)
                return false;
            Stat MyStat = MyStats.GetStat(StatName);
            if (MyStat != null)
                return (MyStat.GetState() >= StatNeeded);
            else
                return false;
        }
        /// <summary>
        /// returns the value of a stat
        /// </summary>
        public float GetStatValue(string StatName)
        {
            Stat MyStat = MyStats.GetStat(StatName);
            if (MyStat != null)
            {
                return MyStat.GetState();
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the base value of a stat
        /// </summary>
        public Stat GetStatBase(string StatName)
        {
            return MyStats.GetStat(StatName);
        }
        #endregion

        #region File

        /*public List<string> GetScriptList()
        {
            return MyStats.GetScriptList(true);
        }

        public override void RunScript(List<string> MyData)
        {
            base.RunScript(MyData);
            for (int i = 0; i < GetSize(); i++)
            {
                OnUpdateStat(GetStat(i));
            }
        }*/
        #endregion

        // Stats effecting gameplay
        #region Gameplay
        /// <summary>
        /// Dies when Health reaches 0 (for most)
        /// </summary>
        public bool Alive()
        {
            return MyStats.IsAlive;
        }

        public void RestoreFullHealth()
        {
            MyStats.IsAlive = true;
            Stat MyHealth = MyStats.GetStat("Health");
            if (MyHealth != null)
            {
                MyHealth.SetState(MyHealth.GetMaxState());
                MyStats.OnUpdateStats.Invoke();
            }
        }
        /// <summary>
        /// Is the character dead
        /// </summary>
        public bool IsDead()
        {
            Stat MyHealth = MyStats.GetStat("Health");
            if (MyHealth != null)
            {
                if (MyHealth.GetState() <= 0)
                {
                    Die();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check for the characters death
        /// </summary>
        void CheckForDeath(Stat MyStat)
        {
            if (MyStats.IsAlive)
            {
                if (MyStat.GetState() <= 0)
                {
                    Die();  // dies by dots
                }
            }
        }

        private void Die()
        {
            if (MyStats.IsAlive)
            {
                MyStats.IsAlive = false;
                if (MySource && OnDeathSound)
                {
                    //MySource.PlayOneShot(OnDeathSound);
                    Sounds.SoundManager.CreateNewSound(MyCharacter.transform.position, OnDeathSound, 0.5f);
                }
                AnimationUtilities.StatPopUp.CreateTextPopup(MyCharacter.transform.position, "Dead", PopupFont, Color.magenta);     // Popups
                if (MyCharacter)
                {
                    MyCharacter.OnDeath();
                }
                OnDeath.Invoke(MyCharacter.gameObject);
            }
        }

        /// <summary>
        /// Updates movement speed depending on speed stat
        /// </summary>
        void UpdateMovementSpeed(float NewSpeed)
        {
            if (MyBasicController)
            {
                MyBasicController.SetMovementSpeed(BaseSpeed + NewSpeed * SpeedMultiplier);
            }
        }

        /// <summary>
        /// Add experience to the character
        /// </summary>
        public void AddExperience(float NewExperience)
        {
            Stat BaseCurrentExperience = GetStatBase("Experience");
            Stat CurrentExperience = MyStats.GetStat("Experience");
            // Level up hack
            NewExperience = CurrentExperience.GetValue() - CurrentExperience.GetState();
            if (CurrentExperience != null)
            {
                //Debug.LogError ("Experience: Value[" + CurrentExperience.GetValue () + "] and State: ["+ CurrentExperience.GetState () + "]");
                // On level up
                if (CurrentExperience.GetState() + NewExperience >= CurrentExperience.GetValue())
                {
                    //Debug.LogError ("Leveling UP!");
                    MyStats.SetStat("Experience",
                                    (CurrentExperience.GetState() + NewExperience) - CurrentExperience.GetValue(),  // left over experience
                                    CurrentExperience.GetValue() * ExperienceMultiplier);                          // Exponentially rise the max exp
                    OnLevelUp();
                }
                else
                {
                    MyStats.AddStat("Experience", NewExperience);
                    //Debug.LogError ("Not Leveling UP! " + (CurrentExperience.GetValue ()-CurrentExperience.GetState ()) + " More to go!");
                }

            }
        }

        /// <summary>
        /// Called when character is leveling up!
        /// </summary>
        private void OnLevelUp()
        {
            // for fun
            //transform.localScale *= 1.1f;  // grow bigger! RAWR
            Debug.Log(MyCharacter.name + " is leveling up!");
            MyStats.AddStat("Level", 1);           // an indictator of overall power
            if (LevelUpEffectsPrefab)
            {
                GameObject MyLevelingEffects = GameObject.Instantiate(LevelUpEffectsPrefab, MyCharacter.transform.position, MyCharacter.transform.rotation, MyCharacter.transform);
                GameObject.Destroy(MyLevelingEffects, 3);
            }
            // auto spend skill points if bot
            /*if (MyCharacter.IsPlayer == false)
            {
				if (GetStat("Strength") == null)
				{
					Stat NewStat = new Stat(StatType.Modifier);
					NewStat.Name = "Strength";
					AddStat(NewStat);
				}
                GetStat("Strength").Add(3);
                //List<Stat> AttributeStats = GetAttributeStats();
                //AttributeStats[Random.Range(0, AttributeStats.Count - 1)].Add(1);   // add random point
            }
            else*/
            {
                // Give skill point
                MyStats.AddStat("SkillPoints", 1);	// to use on attributes
                // turn on stats level up gui, turn off all others
                // refresh stats that i can use skill points on
            }
        }

        /// <summary>
        /// Get a list of its attributes
        /// </summary>
        List<Stat> GetAttributeStats()
        {
            List<Stat> AttributeStats = new List<Stat>();
            for (int i = 0; i < MyStats.GetSize(); i++)
            {
                if (MyStats.GetStat(i).GetStatType() == StatType.Modifier)
                {
                    AttributeStats.Add(MyStats.GetStat(i));
                }
            }
            return AttributeStats;
        }
        #endregion

        #region Equipment
        public void SetEquipment(Inventory NewEquipment)
        {
            if (MyEquipment != NewEquipment)
            {
                // link player character stats to equipment inventory
                MyEquipment = NewEquipment;
                // get inventory
                // on add item, update equipment stats
                //EquipmentInventory.OnAddItem.AddEvent(MyStats.OnEquipNewItem);   // this needs to happen before the other events
            }
        }
        /// <summary>
        /// Take away the old items stats
        /// Add the new items stats
        /// </summary>
        public void OnEquipNewItem(Item OldItem, Item NewItem)
        {
            /*EquipmentStats.Clear(); // refresh all stats
            // for all items in equipment add stats
            for (int i = 0; i < MyEquipment.MyItems.Count; i++)
            {
                for (int j = 0; j < MyEquipment.MyItems[i].MyStats.GetSize(); j++)
                {
                    EquipmentStats.Add(MyEquipment.MyItems[i].MyStats.GetStat(j));
                }
            }
            // now add equipment to temp stats
            for (int i = 0; i < EquipmentStats.GetSize(); i++)
            {
                TempStats.Add(EquipmentStats.GetStat(i));
            }
            OnUpdateTempStats();*/
        }
        #endregion
    }
}



/*public void RefreshStats()
{
    MyStats.Clear();
    for (int i = 0; i < BaseStats.GetSize(); i++)
    {
        Stat NewStat = BaseStats.GetStat(i);
        TempStats.Add(NewStat);
        OnUpdateStat(NewStat);
    }
    OnUpdateTempStats();
}*/
