using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Util;
using Zeltex.Dialogue;
using Zeltex.Characters;
using Zeltex.Items;
using Zeltex;
using SimpleJSON;
using Newtonsoft.Json;

namespace Zeltex.Quests 
{
    [System.Serializable]
    public class QuestBlock : Element
    {
        public string Description = "No Description.";
    }
    /// <summary>
    /// An agreement between characters to meet certain conditions. Met with either rewards or punishment.
    /// </summary>
	[System.Serializable]
	public class Quest : Element
	{
        #region Variables
        public static string QuestNameColor = "#d1d9e1";
		public string Description;		// 
		//public string Condition;		// a command code for the quests condition
		public int ConditionIndex = 0;
		public List<Condition> MyConditions = new List<Condition>();
		public List<Reward> MyRewards = new List<Reward>();
        public QuestBlock MyQuestBlock;

        [SerializeField]
        private bool IsCompleted = false;
		public float TimeCompleted = 0f;
		public bool IsOrderedConditions = false;
		public bool IsHandedIn = false;
		public float TimeHandedIn = 0f;

        // reference to quest partipants
        [JsonIgnore]
        public Character QuestGiver = null;
        [JsonIgnore]
        public Character QuestTaker = null;
        #endregion

        #region Modifiers

        /// <summary>
        /// Sets the description of a quest
        /// </summary>
        public void SetDescription(string NewDescription)
        {
            if (Description != NewDescription)
            {
                Description = NewDescription;
                OnModified();
            }
        }
        #endregion

        #region UI
        public string GetLabelText() 
		{
			string MyLabelText = "";
			if (HasCompleted ())
				MyLabelText += "[X] ";
			else
				MyLabelText += "[O] ";
			MyLabelText += Name;
			return MyLabelText;
		}

		public string GetDescriptionText() 
		{
			string MyDescription = "";
			string QuestGiverName = "Invalid";
			if (QuestGiver)
				QuestGiverName = QuestGiver.name;
			MyDescription += "Quest Giver <color=" + QuestNameColor + ">[" + QuestGiverName + "]</color>\n";
			MyDescription += Description;
			if (MyConditions.Count > 0) 
			{
				MyDescription += "\nConditions";
				for (int i = 0; i < MyConditions.Count; i++) 
				{
					MyDescription += "\n\t";
					MyDescription += MyConditions [i].GetDescriptionText ();
				}
			} else {
				MyDescription += "\nNo Conditions.";
			}
			if (MyRewards.Count > 0) 
			{
				MyDescription += "\nRewards:";
				for (int i = 0; i < MyRewards.Count; i++)
				{
					MyDescription += "\n\t";
					MyDescription += MyRewards [i].GetDescriptionText ();
				}
			} 
			else 
			{
				MyDescription += "\nNo Rewards.";
			}
			if (IsHandedIn) 
			{
				MyDescription += "\nHanded in on: " + TimeHandedIn;
			}
			return MyDescription;
		}
        #endregion

        #region QuestLog
        public bool HasGivenQuestOut()
		{
			if (QuestTaker == null)
				return false;
			else
				return true;
		}

		public void HandIn() 
		{
			if (!IsHandedIn) 
			{
				CompleteQuest ();	// incase it isn't complete
				IsHandedIn = true;
				TimeHandedIn = Time.time;
			}
		}

		public void CompleteQuest()
		{
			if (!IsCompleted) 
			{
				IsCompleted = true;
				TimeCompleted = Time.time;
			}
		}

        public bool HasGivenToSomeone()
        {
            return (QuestTaker != null);
        }

        public bool HasCompleted()
        {
            return IsCompleted;
        }
        /// <summary>
        /// checks if quest has been completed
        /// </summary>
        public void CheckCompleted()
        {
            if (IsAllConditionsTrue())
                IsCompleted = true;
            else
                IsCompleted = false;
        }
        #endregion

        #region Conditions
        public bool IsAllConditionsTrue()
        {
            for (int i = 0; i < MyConditions.Count; i++)
            {
                if (MyConditions[i].HasCompleted() == false)
                    return false;
            }
            return true;
        }

        public Condition GetCurrentCondition()
		{
            if (ConditionIndex < 0 || ConditionIndex >= MyConditions.Count || MyConditions.Count == 0)  // if outside bounds or no conditions
                return new Condition();
			if (ConditionIndex < MyConditions.Count)
				return MyConditions [ConditionIndex];
			else
				return MyConditions [MyConditions.Count-1];
        }

        public void IncreaseConditionIndex()
        {
            if (GetCurrentCondition().HasCompleted())
            {
                ConditionIndex++;
                if (ConditionIndex >= MyConditions.Count)
                    ConditionIndex = MyConditions.Count - 1;
            }
        }
        /// <summary>
        ///  WHen player enters a zone
        /// </summary>
        public bool OnZone(string CheckZone, string Action)
        {
            bool DidTrigger = false;
            //Debug.Log (Name + " -Checking Zone for: " + CheckZone);
            for (int i = 0; i < MyConditions.Count; i++)
            {
                if (!MyConditions[i].HasCompleted() && MyConditions[i].ConditionType.Contains("Zone"))
                {
                    DidTrigger = MyConditions[i].OnZone(CheckZone, Action);
                }
            }
            if (DidTrigger)
            {
                IncreaseConditionIndex();
                CheckCompleted();
            }
            return DidTrigger;
        }

        // tells all the conditions that items have changed, so a check is done
        /// <summary>
        /// To Do:
        /// Make event only for the item
        /// </summary>
        public bool OnAddItem(Character MyCharacter)
        {
           // Debug.Log("Inside Quest, OnAddItem for: " + MyCharacter.name);
            if (IsHandedIn)
            {
                return false;
            }
            bool DidTrigger = false;
            //Zeltex.Items.Inventory MyInventory = MyCharacter.gameObject.GetComponent<Zeltex.Items.Inventory> ();
            for (int i = 0; i < MyConditions.Count; i++)
            {
                if (MyConditions[i].ConditionType.Contains("Inventory"))
                {
                    if (CheckInventory(MyConditions[i], MyCharacter))
                    {
                        DidTrigger = true;
                    }
                }
            }

            if (DidTrigger)
            {
                IncreaseConditionIndex();
                CheckCompleted();
            }
            return DidTrigger;
        }

        /// <summary>
        /// Called on Add Item in inventory
        /// </summary>
		public bool CheckInventory(Condition MyCondition, Character MyCharacter) 
		{
            //Debug.Log("Inside Quest, checking Inventory for Character: " + MyCharacter.name);
			// inventory condition checks
			if (MyCondition.IsInventory()) 
			{
				Inventory MyInventory = MyCharacter.GetBackpackItems();
				if (MyInventory != null) 
				{
                    Item MyItem = MyInventory.GetItem(MyCondition.ObjectName);
                    if (MyItem != null)
                    {
                        int MyQuantity = MyItem.GetQuantity();
                        //Debug.Log("Character has " + MyQuantity + " items of " + MyCondition.ObjectName);
                        if (MyQuantity != MyCondition.ItemQuantityState)
                        {
                            MyCondition.ItemQuantityState = MyQuantity;
                            MyCondition.ItemQuantityState = Mathf.Clamp(MyCondition.ItemQuantityState, 0, MyCondition.ItemQuantity);
                            return true;
                        }   // has updated quest variables
                    }
                    else
                    {
                        MyCondition.ItemQuantityState = 0;
                        // Debug.LogError("No item of: " + MyCondition.ObjectName + " in inventory.");
                    }
				}
			}
			return false;
        }
        #endregion

        #region Data

        /// <summary>
        /// Get the data of the element
        /// </summary>
        public override string GetScript()
        {
            return FileUtil.ConvertToSingle(GetScriptList());
        }

        /// <summary>
        /// Loads the data of the element
        /// </summary>
        public override void RunScript(string Script)
        {
            Quest NewQuest = JsonConvert.DeserializeObject<Quest>(Script);
            Name = NewQuest.Name;
            Description = NewQuest.Description;
        }

        /// returns a list of strings to save the quest
        /// </summary>
        /// <returns></returns>
        public List<string> GetScriptList()
        {
            var serializedObject = JsonConvert.SerializeObject(this);
            return FileUtil.ConvertToList(serializedObject);
            /*List<string> MyScript = new List<string>();
            MyScript.Add("/quest " + Name);
            MyScript.Add("/description " + Description);
            for (int i = 0; i < MyConditions.Count; i++)
            {
                MyScript.AddRange(MyConditions[i].GetScriptList());
            }
            for (int i = 0; i < MyRewards.Count; i++)
            {
                MyScript.AddRange(MyRewards[i].GetScriptList());
            }
            return MyScript;*/
        }

        /// <summary>
        /// Runs the script to load the quest
        /// </summary>
        public void RunScript(List<string> SavedData)
        {
            /*for (int i = 0; i < SavedData.Count; i++)
            {
                string Other = ScriptUtil.RemoveCommand(SavedData[i]);
                if (SavedData[i].Contains("/quest "))
                {
                    string QuestName = Other;
                    Name = QuestName;
                }
                else if (SavedData[i].Contains("/description ")) // description
                {
                    Description = Other;
                }
                else if (SavedData[i].Contains("/LeaveZone "))  // leave zone
                {
                    Condition NewCondition = new Condition();
                    NewCondition.ConditionType = "LeaveZone";
                    NewCondition.ObjectName = Other;
                    MyConditions.Add(NewCondition);
                }
                else if (SavedData[i].Contains("/EnterZone "))  // enter zone
                {
                    Condition NewCondition = new Condition();
                    NewCondition.ConditionType = "EnterZone";
                    NewCondition.ObjectName = Other;
                    MyConditions.Add(NewCondition);
                }
                else if (SavedData[i].Contains("/TalkTo "))  // talk to npc
                {

                    Condition NewCondition = new Condition();
                    NewCondition.ConditionType = "TalkTo";
                    NewCondition.ObjectName = Other;
                    MyConditions.Add(NewCondition);
                }

                else if (SavedData[i].Contains("/Reward ")) // rewards
                {
                    List<string> MyRewardCommands = ScriptUtil.SplitCommands(Other);

                    if (MyRewardCommands.Count > 0)
                    {
                        Reward NewReward = new Reward();
                        NewReward.IsInventory = true;
                        try
                        {
                            string QuantityString = MyRewardCommands[MyRewardCommands.Count - 1];
                            NewReward.ItemQuantity = int.Parse(QuantityString);
                            string ItemName = Other.Remove(Other.IndexOf(QuantityString) - 1, QuantityString.Length + 1);
                            NewReward.ItemName = //SpeechUtilities.CheckStringForLastChar
                                (ItemName);
                        }
                        catch (System.FormatException e)
                        {
                            NewReward.ItemName = //SpeechUtilities.CheckStringForLastChar
                                (Other);
                            NewReward.ItemQuantity = 1;
                        }
                        MyRewards.Add(NewReward);
                    }
                }
                else if (SavedData[i].Contains("/Failure ")) // failure
                {
                    SavedData[i] = Other;

                }
                else if (SavedData[i].Contains("/Inventory ")) // items needed by npc
                {
                    SavedData[i] = Other;
                    List<string> MyItems = ScriptUtil.SplitCommands(SavedData[i]);

                    if (MyItems.Count > 0)
                    {
                        Condition NewCondition = new Condition();
                        NewCondition.ConditionType = "Inventory";
                        try
                        {
                            string QuantityString = MyItems[MyItems.Count - 1]; // get last command
                            NewCondition.ItemQuantity = int.Parse(QuantityString);  // convert it to int
                            SavedData[i] = SavedData[i].Remove(SavedData[i].IndexOf(QuantityString) - 1, QuantityString.Length + 1);
                            NewCondition.ObjectName = SavedData[i];
                        }
                        catch (System.FormatException e)
                        {
                            NewCondition.ObjectName = SavedData[i];
                            NewCondition.ItemQuantity = 1;
                        }
                        MyConditions.Add(NewCondition);
                    }
                }
            }*/
        }
        #endregion
    }
}

/*public void RunScript(string[] SavedData)
{
    List<string> MyScript = new List<string>();
    for (int i = 0; i < SavedData.Length; i++)
    {
        MyScript.Add(SavedData[i]);
    }
    RunScript(MyScript);
}*/
/// <summary>

//reading/writing section

/*public static string[] MyCommands = new string[] {  "quest",
                                                            "description",
                                                            "LeaveZone",
                                                            "EnterZone",
                                                            "Inventory",
                                                            "TalkTo",
                                                            "reward",
                                                            "failure"
                                                         };*/