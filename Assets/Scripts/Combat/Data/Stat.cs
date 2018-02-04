using UnityEngine;
using System.Collections.Generic;
using Zeltex.Util;
using Newtonsoft.Json;

namespace Zeltex.Combat
{
    /// <summary>
    /// The type of stat data kept
    /// </summary>
    [System.Serializable]
    public enum StatType
    {
        Base,
        State,
        Regen,
        Modifier,
        TemporaryRegen,
        TemporaryModifier
    }
    /// <summary>
    /// Class:  Stat
    /// Author: Marz
    ///     - Base Stat - name, value
    ///     - State Stat - name, value, statevalue
    ///     - Regen Stat - name, regenvalue, regencooldown
    ///     - Modifier    - name, value, modifiedname, value
    ///     - Tempoary Regen  - regenstats, expiretime
    ///     - Temporary Modifier  -modifierstats, expiretime
    ///     
    /// Base
    ///	    - [string] [value] - Joyness 5
    /// State
    ///	    - [string] [value] [value] - Health 50/100
    /// Modifier
    ///      - [string] [value] [string] [value] - Strength 10, Health x10
    /// Regen
    ///      - [string] [string] [value] [value] - HealthRegen,Health,0,1
    /// Buff - Temporary Modifier
    ///      - [string] [value] [value] - Strength, 10, 30s
    /// To DO:
    /// Damage Over Time - Temporary Regen
    ///      - [string] [string] [value] [timeTick] [timeMax] - Burn, Health, 3, 4s, 20s
    /// </summary>
    [System.Serializable]
	public class Stat : Element
	{
        #region Variables
        [JsonIgnore]
        private static string DescriptionColorTag = "<color=#00cca4>";
        [JsonIgnore]
        private static string NameColorTag = "<color=#474785>";
        [JsonIgnore]
        private static string StatVariableColorTag = "<color=#989a33>";
        [JsonIgnore]
        private static float GameRegenModifier = 0.1f;
        // UI
        public string Description = "";		// used for tooltip
        // Data
        [SerializeField, JsonProperty]
        protected StatType MyType = StatType.Base;
        [SerializeField, JsonProperty]
        protected List<float> Value = new List<float> ();				// the default of the value
		[SerializeField, JsonProperty]
        protected List<string> Modifiers = new List<string> ();		// the default of the value

        public Zexel MyZexel = new Zexel();
        [JsonIgnore]
        protected Texture2D MyTexture;     // The texture used in the gui
        #endregion

        public StatType GetStatType()
        {
            return MyType;
        }

        public List<float> GetValues()
        {
            return Value;
        }

        #region Initiation

        /// <summary>
        /// Default Initiator
        /// </summary>
        public Stat() { }

		/// <summary>
		/// Default Initiator
		/// </summary>
		public Stat(StatType NewStatType)
		{
            MyType = NewStatType;
			if (MyType == StatType.Base)
			{
				CreateBase(Name, 0);
			}
			else if (MyType == StatType.State)
			{
				CreateState(Name, 0, 0);
			}
			else if (MyType == StatType.Regen)
			{
				CreateRegen(Name, "", 0, 1);
            }
            else if (MyType == StatType.Modifier)
            {
                CreateModifier(Name, 0, "", 1);
            }
            else if (MyType == StatType.TemporaryModifier)
            {
                CreateBuff(Name, 1, "", 15);
            }
            else if (MyType == StatType.TemporaryRegen)
            {
                CreateDot(Name, 1, "", 1, 15);
            }
        }

		/// <summary>
		/// Cloning a stat
		/// </summary>
		public Stat(Stat NewStat)
        {
            Name = NewStat.Name;
            Description = NewStat.Description;
            MyType = NewStat.MyType;
            for (int i = 0; i < NewStat.Value.Count; i++)
            {
                Value.Add(NewStat.Value[i]);
            }
            for (int i = 0; i < NewStat.Modifiers.Count; i++)
            {
                Modifiers.Add(NewStat.Modifiers[i]);
            }
            MyTexture = NewStat.MyTexture;
        }

        /// <summary>
        /// Clear all the values and names
        /// </summary>
        private void Clear()
        {
            Value.Clear();
            Modifiers.Clear();
        }
        
        public void SetValuesAsType(StatType NewType)
        {
            MyType = NewType;
            Clear();
            if (MyType == StatType.Base)
            {
                Value.Add(1);
            }
            else if (MyType == StatType.State)
            {
                Value.Add(10);    // state first!
                Value.Add(10);  // max second!
            }
            else if (MyType == StatType.Regen)
            {
                Value.Add(1);
                Value.Add(1);
                Value.Add(Time.time);   // last ticked
                Modifiers.Add("");
            }
            else if (MyType == StatType.Modifier)
            {
                Value.Add(1);
                Value.Add(1);
                Modifiers.Add("");
            }
            else if (MyType == StatType.TemporaryModifier)
            {
                Value.Add(1);       // value
                Value.Add(12);          // time lasting for
                Value.Add(Time.time);           // Time Begun
                Modifiers.Add("");
            }
            else if (MyType == StatType.TemporaryRegen)
            {
                Modifiers.Add("");    // stat modified per tick
                Value.Add(1);              // value addition
                Value.Add(15);          // time lasting for
                Value.Add(Time.time);           // Time Begun
                Value.Add(1);            // TickTime
                Value.Add(Time.time);           // Time Last Ticked
            }
            OnModified();
        }

        /// <summary>
        /// Creates a base value stat.
        /// </summary>
        public void CreateBase(string NewName, float NewValue)
        {
            if (MyType != StatType.Base)
            {
                Name = NewName;
                SetValuesAsType(StatType.Base);
                Value[0] = (NewValue);
                OnModified();
            }
            else
            {
                CheckStatsValues();
            }
        }

        /// <summary>
        /// Creates a stat modifier type.
        /// </summary>
        public void CreateModifier(string NewName, float NewValue, string ModifierType, float MultiplierValue)
        {
            if (MyType != StatType.Modifier)
            {
                Name = NewName;
                SetValuesAsType(StatType.Modifier);
                Value[0] = (NewValue);
                Modifiers[0] = (ModifierType);
                Value[1] = (MultiplierValue);
                OnModified();
            }
            else
            {
                CheckStatsValues();
            }
        }
        /// <summary>
        /// Creates a buff type.
        /// [string]  [value] [string] [value]- Rage, 30s, Strength, +10
        /// </summary>
        public void CreateBuff(string NewName, float AdditionValue, string ModifierType, float TimeLength)
        {
            if (MyType != StatType.TemporaryModifier)
            {
                Name = NewName;
                SetValuesAsType(StatType.TemporaryModifier);
                Value[0] = (AdditionValue);       // value
                Value[1] = (TimeLength);          // time lasting for
                Value[2] = (Time.time);           // Time Begun
                Modifiers[0] = (ModifierType);
                OnModified();
            }
            else
            {
                CheckStatsValues();
            }
        }
        /// <summary>
        /// Creates a dot type.
        ///      - [string] [value]  [string] [timeTick] [timeMax] - Burn, 5dmg, Health, 4s, 20s
        /// </summary>
        public void CreateDot(string NewName, float Damage, string ModifierType, float TimeTick, float TimeLength)
        {
            if (MyType != StatType.TemporaryRegen)
            {
                Name = NewName;
                SetValuesAsType(StatType.TemporaryRegen);
                Value[0] = (Damage);              // value addition
                Value[1] = (TimeLength);          // time lasting for
                Value[2] = (Time.time);           // Time Begun
                Value[3] = (TimeTick);            // TickTime
                Value[4] = (Time.time);           // Time Last Ticked
                Modifiers[0] = (ModifierType);    // stat modified per tick
                OnModified();
            }
            else
            {
                CheckStatsValues();
            }
        }

        private void CheckStatsValues()
        {
            if (MyType == StatType.Base)
            {
                if (Value.Count != 1)
                {
                    Value.Clear();
                    Debug.LogError(MyType.ToString() + ": " + Name + " had wrong stats.");
                    Value.Add(0);// just incase
                    OnModified();
                }
                if (Modifiers.Count != 0)
                {
                    Modifiers.Clear();
                    OnModified();
                }
            }
            else if (MyType == StatType.State)
            {
                if (Value.Count != 2)
                {
                    Value.Clear();
                    Debug.LogError(MyType.ToString() + ": " + Name + " had wrong stats.");
                    Value.Add(0);// just incase
                    Value.Add(0);// just incase
                    OnModified();
                }
                if (Modifiers.Count != 0)
                {
                    Modifiers.Clear();
                    OnModified();
                }
            }
            else if(MyType == StatType.Modifier)
            {
                if (Value.Count != 2)
                {
                    Debug.LogError(MyType.ToString() + ": " + Name + " had wrong Values.");
                    Value.Clear();
                    Value.Add(0);// just incase
                    Value.Add(0);// just incase
                    OnModified();
                }
                if (Modifiers.Count != 1)
                {
                    Debug.LogError(MyType.ToString() + ": " + Name + " had wrong Modifiers.");
                    Modifiers.Clear();
                    Modifiers.Add("");
                    OnModified();
                }
            }
            else if(MyType == StatType.Regen)
            {
                if (Value.Count != 3)
                {
                    Debug.LogError(MyType.ToString() + ": " + Name + " had wrong Values.");
                    Value.Clear();
                    Value.Add(0);// just incase
                    Value.Add(0);// just incase
                    Value.Add(0);// just incase
                    OnModified();
                }
                if (Modifiers.Count != 1)
                {
                    Debug.LogError(MyType.ToString() + ": " + Name + " had wrong Modifiers.");
                    Modifiers.Clear();
                    Modifiers.Add("");
                    OnModified();
                }
            }
        }
        #endregion

        #region Stats

        /// <summary>
        /// Returns the value.
        /// </summary>
        public float GetValue()
        {
            CheckStatsValues();
            if (MyType == StatType.Base || MyType == StatType.TemporaryModifier)
            {
                return Value[0];
            }
            else
            {
                Debug.LogWarning("Trying to get a value from a variable that is not a [Base, TemporaryModifier] type. " + Name);
                return 0;
            }
		}

        /// <summary>
        /// Adds value to a variable depending on type.
        ///     - if state, will increase the Max too.
        /// </summary>
        public void Add(float AddValue)
        {
            //Debug.LogError(Name + " adding value: " + AddValue);
            if (Value.Count >= 1)
            {
                Value[0] += AddValue;
                OnModified();
            }
            if (MyType == StatType.State)
            {
                if (Value.Count < 2)
                {
                    Debug.LogError("Wht");
                    return;
                }
                Value[1] += AddValue;
            }
		}

		public void SetValue(float NewValue) 
		{
            if (MyType == StatType.Base)
            {
                Value[0] = NewValue;
                OnModified();
            }
            else if (MyType == StatType.Regen)
            {
                Value[0] = NewValue;
                OnModified();
            }
            else
            {
                Debug.Log(MyType.ToString() + " trying to set new value.");
            }
		}
        #endregion

        #region State

        /// <summary>
        /// Creates a State type variable.
        /// </summary>
        public void CreateState(string NewName, float NewState, float NewMax)
        {
            if (MyType != StatType.Modifier)
            {
                SetValuesAsType(StatType.State);
                Name = NewName;
                Value[0] = (NewState);    // state first!
                Value[1] = (NewMax);  // max second!
                OnModified();
            }
            else
            {
                CheckStatsValues();
            }
        }

        /// <summary>
        /// Set the max of a state variable
        /// </summary>
        public void SetMax(float NewMax)
        {
            if (MyType == StatType.State)
            {
                if (Value[1] != NewMax)
                {
                    Value[1] = NewMax;
                    OnModified();
                }
            }
            else
            {
                Debug.LogWarning("Trying to set the max of a variable that is not a state type. " + Name);
            }
        }

        /// <summary>
        /// Set the temporary value of a state stat
        /// </summary>
        public void SetState(float NewState)
        {
            if (MyType == StatType.State)
            {
                if (Value[0] != NewState)
                {
                    Value[0] = NewState;
                    OnModified();
                }
            }
            else
            {
                Debug.LogWarning("Trying to set the max of a variable that is not a state type. " + Name);
            }
        }

        /// <summary>
        /// Returns the state value of the stat.
        /// </summary>
        public float GetState()
        {
            if (MyType == StatType.State)
            {
                return Value[0];
            }
            else
            {
                Debug.LogWarning("Trying to get a state from a variable that is not a state type. " + Name);
                return 0;
            }
        }

        /// <summary>
        /// Add to the state value
        /// </summary>
        public bool AddState(float AddValue)
        {
            if (MyType == StatType.State)
            {
                float NewValue = Value[0] + AddValue;
                NewValue = Mathf.Clamp(NewValue, 0, GetMaxState());
                if (NewValue != Value[0])
                {
                    Value[0] = NewValue;
                    OnModified();
                    return true;    // value has changed
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Debug.LogWarning("Trying to add a state to a variable that is not a state type. " + Name);
                return false;   // weird error? no idea... how did Value List change? magic.
            }
        }

        /// <summary>
        /// Get the percentage of the state stat
        /// </summary>
        public float GetPercentage()
        {
            if (MyType == StatType.State)
            {
                if (Value[1] == 0)
                {
                    Debug.LogWarning("Stat has 0 as max value. " + Name);
                    Value[1] = 1;
                }
                return Value[0] / Value[1];
            }
            else
            {
                Debug.LogWarning("Trying to get a Percentage from a variable that is not a state type. " + Name);
                return 1;
            }
        }

        /// <summary>
        /// returns the max of state can be
        /// unless another variable then just returns the first value
        /// </summary>
        public float GetMaxState()
        {
            if (MyType == StatType.State)
            {
                return Value[1];
            }
            else
            {
                Debug.LogWarning("Trying to get a max from a variable that is not a state type. " + Name);
                return 1;
            }
        }
        #endregion

        #region Regen

        /// <summary>
        /// Creates a stat Regen type. 
        /// Values:
        ///     RegenRate
        ///     Cooldown
        ///     Current Time
        /// String:
        ///     Stat Name
        /// </summary>
        public void CreateRegen(string NewName, string RegenStatName, float RegenRate, float RegenCoolDown)
        {
            Name = NewName;
            SetValuesAsType(StatType.Regen);
            Value[0] = (RegenRate);
            Value[1] = (RegenCoolDown);
            Value[2] = (Time.time);   // last ticked
            Modifiers[0] = (RegenStatName);
        }


        /// <summary>
        /// The regen addition 
        /// </summary>
        public float GetRegenValue()
        {
            if (MyType == StatType.Regen)
            {
                return Value[0];
            }
            else
            {
                Debug.LogWarning("Trying to get new regen value of: " + MyType.ToString());
                return 0;
            }
        }

        /// <summary>
        /// Set a new regen value
        /// </summary>
        public void SetRegenValue(float NewValue)
        {
            if (MyType == StatType.Regen)
            {
                if (Value[0] != NewValue)
                {
                    Value[0] = NewValue;
                    OnModified();
                }
            }
            else
            {
                Debug.LogWarning("Trying to set new regen rate of: " + MyType.ToString());
            }
        }

        /// <summary>
        /// The modified regen value
        /// </summary>
        public float GetRegenValueModified()
        {
            return (GetRegenValue() * GameRegenModifier);
        }

        /// <summary>
        /// Set regeneration rate
        /// </summary>
        public void SetRegenCooldown(float NewValue)
        {
            if (MyType == StatType.Regen)
            {
                if (Value[1] != NewValue)
                {
                    Value[1] = NewValue;
                    OnModified();
                }
            }
            else
            {
                Debug.LogWarning("Trying to set new regen rate of: " + MyType.ToString());
            }
        }

        /// <summary>
        /// Get regeneration rate
        /// </summary>
        public float GetRegenRate()
        {
            if (MyType == StatType.Regen)
            {
                return Value[1];
            }
            else
            {
                return 0;
            }
        }
        #endregion

        #region Attributes 

        public void SetModifier(string ModifierName)
        {
            if (MyType == StatType.Modifier)
            {
                if (Modifiers[0] != ModifierName)
                {
                    Modifiers[0] = ModifierName;
                    OnModified();
                }
            }
            else if (MyType == StatType.Regen)
            {
                if (Modifiers[0] != ModifierName)
                {
                    Modifiers[0] = ModifierName;
                    OnModified();
                }
            }
            else
            {
                Debug.LogWarning("Trying to set modifier of: " + MyType.ToString());
            }
        }

        public void SetModifierValue(float ModifierName)
        {
            if (MyType == StatType.Modifier)
            {
                Value[1] = ModifierName;
            }
        }

        public float GetModifierValue() 
		{
			return Value [1];
		}

		public string GetModifyStatName() 
		{
            if (Modifiers.Count == 0)
            {
                Debug.LogError("Wut");
                return "";
            }
            else
            {
                return Modifiers[0];
            }
		}

		public void Add(string ValueType, float ValueAddition) 
		{
			if (ValueType == "Base")
            {
				Add (ValueAddition);
			}
            else if (ValueType == "State")
            {
				Add(ValueAddition);
			}
        }
        #endregion

        // Regen
        #region Timers
        
        /// <summary>
        /// Resets the time starting values
        /// </summary>
        public void ResetTimer()
        {
            if (MyType == StatType.Regen)
            {
                Value[2] = Time.time;
            }
            else if (MyType == StatType.TemporaryModifier)
            {
                SetPreviousTick(Time.time);
            }
            else if(MyType == StatType.TemporaryRegen)
            {
                Value[2] = Time.time;
                Value[4] = Time.time;
            }
        }

        /// <summary>
        /// Get the cooldown
        /// </summary>
        public float GetDotValue()
        {
            return Value[0];
        }

        private float GetPreviousTick()
        {
            if (MyType == StatType.Regen)
            {
                return Value[2];
            }
            else
            {
                return -10000;
            }
        }

        private void SetPreviousTick(float NewPreviousTime)
        {
            if (MyType == StatType.Regen)
            {
                Value[2] = NewPreviousTime;   // renew ticking
            }
        }
        /// <summary>
        /// Checking regen ticking!
        /// </summary>
		public bool HasTicked()
        {
            if (MyType == StatType.Regen)
            {
                if (Time.time - GetPreviousTick() > GetRegenRate())
                {
                    SetPreviousTick(Time.time);
                    // renew ticking
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (MyType == StatType.TemporaryRegen)
            {
                if (Time.time - Value[4] > Value[3])
                {
                    Value[4] = Time.time;   // last ticked time
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Is the dot over? should we remove it ?
        /// </summary>
        public bool HasExpired()
        {
            if (MyType == StatType.TemporaryModifier)
            {
                if (Time.time - Value[2] >= Value[1])
                {
                    //Value[2] = Time.time;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (MyType == StatType.TemporaryRegen)
            {
                if (Time.time - Value[2] >= Value[1])   // time has finished!
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region GettersAndSetters

        /// <summary>
        /// Used only for editor items. Loads texture from the item manager.
        /// </summary>
        public void LoadTexture(string MyTextureName)
        {
            //MyTexture = TextureMaker.Get().GetStatTexture(MyTextureName);
            MyTexture = (Zeltex.DataManager.Get().GetElement(DataFolderNames.StatTextures, MyTextureName) as Zexel).GetTexture();
            //Debug.LogError("Loaded Texture: " + MyTexture.name);
        }

        public void SetTexture(Texture2D NewTexture)
        {
            MyTexture = NewTexture;
        }

        public Texture2D GetTexture()
        {
            return MyTexture;
        }

        public void SetDescription(string NewDescription)
        {
            if (Description != NewDescription)
            {
                Description = NewDescription;
                OnModified();
            }
        }

        public string GetDescription()
        {
            return Description; // if null check stat manager!
        }
        #endregion

        #region File
        /// <summary>
        /// Returns true if it is a stat creator command
        /// </summary>
        public static bool IsBeginTag(string MyInput)
        {
            string MyCommand = ScriptUtil.GetCommand(MyInput);
            return (MyCommand.Contains("Base") ||
                MyCommand.Contains("State") ||
                MyCommand.Contains("Modifier") ||
                MyCommand.Contains("Regen") ||
                MyCommand.Contains("Buff") ||
                MyCommand.Contains("Dot"));
        }

        /// <summary>
        /// Run Script for specific types
        /// </summary>
        public void ActivateCommand(string MyCommand)
        {
            string Command = ScriptUtil.GetCommand(MyCommand);
            string MyInput = ScriptUtil.RemoveCommand(MyCommand);
            if (Command.Contains("/Description"))
            {
                SetDescription(MyInput);
            }
            else if (Command.Contains("/LoadTexture"))
            {
                LoadTexture(MyInput);
            }
        }

        /// <summary>
        /// Creates a dynamic stat based on a list of inputs, seperated by ','. /Base Level,1
        /// </summary>
        public void Initiate(string Script)
        {
            string StatType = ScriptUtil.GetCommand(Script);
            string MyInput = ScriptUtil.RemoveCommand(Script);
            //Debug.Log("New Stat: " + StatType + ":" + MyInput);
            Value.Clear();
            Modifiers.Clear();

            string[] MyStrings = MyInput.Split(','); // Parameters
            // Remove all white space/ improper characters in input names
            for (int j = 0; j < MyStrings.Length; j++)
            {
                MyStrings[j] = ScriptUtil.RemoveWhiteSpace(MyStrings[j]);
            }

            if (StatType == "/Base")
            {
                CreateBase(MyStrings[0], float.Parse(MyStrings[MyStrings.Length - 1]));
            }
            else if (StatType == "/State")
            {
                CreateState(MyStrings[0], float.Parse(MyStrings[MyStrings.Length - 2]), float.Parse(MyStrings[MyStrings.Length - 1]));
            }
            else if (StatType == "/Modifier")
            {   // string, float, string, float
                CreateModifier(MyStrings[0], float.Parse(MyStrings[1]), MyStrings[2], float.Parse(MyStrings[3]));
            }
            else if (StatType == "/Regen")
            {
                CreateRegen(MyStrings[0], MyStrings[1], float.Parse(MyStrings[2]), float.Parse(MyStrings[3]));
            }
            else if (StatType == "/Buff")
            {   // string, float, string, float
                CreateBuff(MyStrings[0], float.Parse(MyStrings[1]), MyStrings[2], float.Parse(MyStrings[3]));
            }
            else if (StatType == "/Dot")    // /Dot Burn, 5, Health, 3, 30
            {   // string, float, string, float
                CreateDot(MyStrings[0], float.Parse(MyStrings[1]), MyStrings[2], float.Parse(MyStrings[3]), float.Parse(MyStrings[4]));
            }
        }
        #endregion

        #region UI

        public string GetToolTipName()
        {
            return NameColorTag + Name + "</color>";
        }

        public string GetToolTipText()
        {
            string MyTooltip = DescriptionColorTag + Description + "</color>\n";
            if (MyType == StatType.Base)
            {
                MyTooltip += "\tValue: " + StatVariableColorTag + "[" + GetValue() + "]</color> ";
            }
            else if (MyType == StatType.State)
            {
                MyTooltip += "\tState: " + StatVariableColorTag + "[" + GetState() + "/" + GetMaxState() + "]</color> ";
            }
            else if (MyType == StatType.Regen)
            {
                if (Modifiers.Count == 0)
                    Modifiers.Add("Invalid");
                MyTooltip += "\tThe stat " + StatVariableColorTag + "[" + GetModifyStatName() + "]</color> will recover at a rate of " + StatVariableColorTag + "[" +
                    GetRegenValueModified() + "]</color> every " + StatVariableColorTag + "[" + GetRegenRate() + "]</color> seconds.";
            }
            else if(MyType == StatType.Modifier)
            {
                MyTooltip += "\t You will get an increase to " + StatVariableColorTag + "[" + Modifiers[0] + "]</color>  by a multiple of " +
                    StatVariableColorTag + "[" + Value[1] + "]</color>. Total Bonus " + StatVariableColorTag + "[" + (Value[0] * Value[1]) + "]</color>";
            }
            else if (MyType == StatType.TemporaryModifier)
            {
                MyTooltip += "\t You will get an increase to " +
                    StatVariableColorTag + "[" + Modifiers[0] + "]</color>  by an addition of " +
                    StatVariableColorTag + "[" + Value[0] + "]</color>. For " +
                    StatVariableColorTag + "[" + (Mathf.FloorToInt(Time.time - Value[2])) + "/" + Value[1] + "]</color> seconds.";
            }
            else if (MyType == StatType.TemporaryRegen)
            {
                MyTooltip += "\t You will get an increase to "
                     + StatVariableColorTag + "[" + Modifiers[0] + "]</color>  by an addition of "
                    + StatVariableColorTag + "[" + Value[0] + "]</color>."
                    + " It ticks every " + StatVariableColorTag + "[" + Value[3] + "]</color> seconds "
                    + " for " + StatVariableColorTag + "[" + (Mathf.FloorToInt(Time.time - Value[2])) + "/" + Value[1] + "]</color> seconds.";  // for x time
            }
            return MyTooltip;
        }

        private string GetStatTypeLabel()
        {
            return MyType.ToString();
        }

        public string GuiString()
        {
            if (MyType == StatType.Base)
            {
                if (Value.Count > 0)
                {
                    return (Name + " [" + Mathf.RoundToInt(Value[0]) + "]");
                }
                else
                {
                    if (Name != "Empty")
                    {
                        Debug.LogError(Name + " has no values...?");
                    }
                    return "Error";
                }
            }
            else if (MyType == StatType.State)
            {
                return (Name + " [" + Mathf.RoundToInt(GetState()) + "/" + Mathf.RoundToInt(GetMaxState()) + "]");
            }
            else if (MyType == StatType.Regen)
            {
                return (Name + " [" + Mathf.RoundToInt(Value[0]) + "]");
            }
            else if (MyType == StatType.Modifier)
            {
                return (Name + " [" + Mathf.RoundToInt(Value[0]) + "]" + " Modifies " + GetModifyStatName() + " with a multiplier of " + Mathf.RoundToInt(Value[1]));
            }
            else if (MyType == StatType.TemporaryRegen)
            {
                return (Name + " [" + Mathf.RoundToInt(Value[0]) + "]");
            }
            else if (MyType == StatType.TemporaryModifier)
            {
                return (Name + " [" + Mathf.RoundToInt(Value[0]) + "]");
            }
            else
            {
                return "Uknown";
            }
        }

        public string GetGuiString()
        {
            string MyGuiString = GuiString() + "\n";
            if (Description != "")
                MyGuiString += Description + "\n";
            return MyGuiString;
        }
        #endregion
    }
}

/// State
///	    - [string] [value] [value] - Health 50/100
/// Modifier
///      - [string] [value] [string] [value] - Strength 10, Health x10
/// Regen
///      - [string] [string] [value] [value] - HealthRegen,Health,0,1
/// Buff - Temporary Modifier
///      - [string] [value] [value] - Strength, 10, 30s
/// To DO:
/// Damage Over Time - Temporary Regen
///      - [string] [string] [value] [timeTick] [timeMax] - Burn, Health, 3, 4s, 20s
/*
/gamemode fun
/defaultstats
    //statregen [statname] [value] [regen] [rate]
    /statregen health 100 0.5 1
    /statregen mana 100 0.5 1
    /statregen energy 100 0.5 1
    //attribute [attributename](value) [statname](multiplier)
    /attribue strength(5) health(10)
    /attribue intelligence(5 mana(10)
    /attribue strength(5) energy(10)
*/
