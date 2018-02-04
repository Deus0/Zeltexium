using System.Collections.Generic;
using UnityEngine;
using Zeltex.Util;
using Newtonsoft.Json;
using UnityEngine.Events;

namespace Zeltex.Combat
{
    /// <summary>
    /// A group of stats. Primarily used in game mechanics.
    /// </summary>
	[System.Serializable]
	public class Stats : Element
    {
        [JsonProperty, SerializeField]
        public bool IsAlive = true;
        // List of stats
        public List<Stat> Data = new List<Stat>();

        //[Header("Events")]
        [JsonIgnore, HideInInspector]
        public UnityEvent OnNewStats = new UnityEvent();
        [JsonIgnore, HideInInspector]
        public UnityEvent OnUpdateStats = new UnityEvent();

        #region Init

        public override void OnLoad()
        {
            base.OnLoad();
            for (int i = 0; i < Data.Count; i++)
            {
                Data[i].ParentElement = (this);
                Data[i].OnLoad();    // any sub stats will be set as well
            }
        }
        #endregion

        #region List
        /// <summary>
        /// Clears all Stats
        /// </summary>
        public void Clear() 
		{
			Data.Clear ();
        }

        /// <summary>
        /// Removes a stat at an index
        /// </summary>
        public void Remove(Stat MyStat)
        {
            Data.Remove(MyStat);
        }

        /// <summary>
        /// Removes a stat at an index
        /// </summary>
        public void RemoveStat(int i)
        {
            Data.RemoveAt(i);
        }

        /// <summary>
        /// Adds a stat to the list
        /// If already in the list, will combine them
        /// </summary>
        public virtual bool Add(Stat MyStat)
        {
            // if already in list, add their values together
            for (int i = 0; i < Data.Count; i++)
            {
                if (Data[i].Name == MyStat.Name)
                //if (Data[i].GetStatType() == MyStat.GetStatType())// if in list, increase the value of the stat
                {
                    Data[i].Add(MyStat.GetValue());
                    return false;
                }
            }
            // if not using the new variable, it will override the base stats when creating temp ones
            Data.Add(new Stat(MyStat)); // if not in list, add to list
            return true;
        }

        public virtual Stat AddStat(string StatName, float NewAddition)
        {
            Stat MyStat = GetStat(StatName);
            if (MyStat != null)
            {
                MyStat.Add(NewAddition);
            }
            return MyStat;
        }

        public virtual void AddStatState(string StatName, float NewAddition)
        {
            Stat MyStat = GetStat(StatName);
            if (MyStat != null)
            {
                MyStat.AddState(NewAddition);
                OnUpdateStats.Invoke();
            }
        }
        #endregion

        #region Setters
        public void SetStat(string StatName, float NewValue) 
		{
			for (int i = 0; i < Data.Count; i++)
            {
				if (StatName == Data[i].Name)
                {
					Data[i].SetState(NewValue);
					return;
				}
			}
		}

		public void SetStat(string StatName, float NewState, float NewMax) 
		{
			for (int i = 0; i < Data.Count; i++)
			{
				if (StatName == Data[i].Name) 
				{
					Data[i].SetState(NewState);
					Data[i].SetValue(NewMax);
					return;
				}
			}
		}
        #endregion

        #region Getters

        /// <summary>
        /// Returns true if stat exists.
        /// </summary>
        public bool HasStat(Stat MyStat)
        {
            for (int i = 0; i < Data.Count; i++)
            {
                if (Data[i].Name == MyStat.Name) // if in list, increase the value of the stat
                {
                    return true;
                }
            }
            return false;
        }

        public Stat GetStat(int i)
        {
            if (i >= 0 && i < Data.Count)
            {
                return Data[i];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the stat with a name
        /// or returns null
        /// </summary>
		public Stat GetStat(string StatName)
        {
            StatName = ScriptUtil.RemoveWhiteSpace(StatName);
            for (int i = 0; i < Data.Count; i++)
            {
                if (StatName == ScriptUtil.RemoveWhiteSpace(Data[i].Name))
                {
                    return Data[i];
                }
            }
            return null;
        }

        /// <summary>
        /// returns the size of stats.
        /// </summary>
        public int GetSize()
        {
			return Data.Count;
        }
        #endregion

        #region File
        public static bool IsBeginTag(string MyLine)
        {
            return (MyLine.Contains("/stats") || MyLine.Contains("/characterstats"));
        }
        public static bool IsEndTag(string MyLine)
        {
            return (MyLine.Contains("/endstats"));
        }
        /// <summary>
        /// Returns the script list.
        /// </summary>
       /* public virtual List<string> GetScriptList(bool IsCharacterStats)
        {
            List<string> MyScript = new List<string>();
            if (GetSize() == 0)
            {
                return MyScript;
            }

            if (IsCharacterStats)
            {
                MyScript.Add("/characterstats");
            }
            else
            {
                MyScript.Add("/stats");
            }
            for (int i = 0; i < GetSize(); i++)
            {
                MyScript.AddRange(GetStat(i).GetScriptList());
            }
            MyScript.Add("/endstats");
            return MyScript;
        }

        /// <summary>
        /// Returns the script list in a single string.
        /// </summary>
        public string GetScript(bool IsCharacterStats)
        {
            return FileUtil.ConvertToSingle(GetScriptList(IsCharacterStats));
        }

        /// <summary>
        /// Runs the script list, loading the stats.
        /// </summary>
        public virtual void RunScript(List<string> StringData)
        {
            bool IsReadingStats = false;
            int IndexBegin = -1;
            int IndexEnd = -1;
            for (int i = 0; i < StringData.Count; i++)
            {
                if (IsEndTag(StringData[i]))
                {
                    IsReadingStats = false;
                    IndexEnd = i;
                    if (IndexBegin != -1)
                    {
                            Stat NewStat = new Stat();
                            List<string> MyStatScript = StringData.GetRange(IndexBegin, IndexEnd - IndexBegin);
                            NewStat.RunScript(MyStatScript);
                            //Debug.LogError("Last Stat: " + FileUtil.ConvertToSingle(MyStatScript));
                            Add(NewStat);
                    }
                }
                if (IsReadingStats)
                {
                    StringData[i] = ScriptUtil.RemoveWhiteSpace(StringData[i]);
                    if (StringData[i] != "")
                    {
                        if (Stat.IsBeginTag(StringData[i])) //!ScriptUtil.IsCommand(StringData[i]) || ScriptUtil.GetCommand(StringData[i]).Contains("/Buff"))
                        {
                            IndexEnd = i;
                            if (IndexBegin != -1)
                            {
                                Stat NewStat = new Stat();
                                List<string> MyStatScript = StringData.GetRange(IndexBegin, IndexEnd - IndexBegin);
                                NewStat.RunScript(MyStatScript);
                                Add(NewStat);
                            }
                            // start reading from this index
                            IndexBegin = i;
                        }	// if has / at the front!
                    }
                }
                if (IsBeginTag(StringData[i]))
                {
                    IsReadingStats = true;  // begin reading in stat data
                }
            }
        }*/
        #endregion
    }
}