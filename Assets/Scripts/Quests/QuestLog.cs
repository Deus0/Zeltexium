using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using Zeltex.Items;
using Zeltex.Util;
using Zeltex.Characters;
using Newtonsoft.Json;

/*	Completed quests are now kept in the quest log
 * 	They will be in the same list, with the boolean IsComplete set to true
 * 		When they are handed in for rewards, their boolean IsHandedIn is set to true
 * 	Otherwise the are just incomplete
 * 		
 * 
*/
namespace Zeltex.Quests
{
    /// <summary>
    /// Holds a list of quests
    /// </summary>
    [System.Serializable]
	public class QuestLog : Element
    {
        [Header("Debug")]
        public bool DebugGui;

		[Header("Quests")]
		[Tooltip("Limits the amount of unfinished quests. Set to -1 for unlimited.")]
		public int QuestLimit = -1;
		[Tooltip("These quests can be given to other characters to complete.")]
		public List<Quest> MyQuests = new List<Quest>();
		//[Tooltip("Link to QuestLogGuiHandler class. Empty if the character has no quest gui.")]
		//public QuestLogGuiHandler MyQuestLogGui;

		[Header("Sounds")]
		[Tooltip("Sound Plays when completing a quest"), JsonIgnore]
		public AudioClip OnCompleteQuestSound;
		[Tooltip("Sound Plays when handing in a quest"), JsonIgnore]
		public AudioClip OnHandInQuestSound;
		[Tooltip("Sound Plays when Beginning in a quest"), JsonIgnore]
		public AudioClip BeginQuestSound;

        [Header("Events")]
        [Tooltip("Called when a quest is added or removed."), JsonIgnore]
        public UnityEvent OnAddQuest;
        [Tooltip("Called when a quest is completed."), JsonIgnore]
        public UnityEvent OnCompletedQuest;

        [JsonIgnore]
        private Character MyCharacter;
        [JsonIgnore]
        private AudioSource MySource;

        // Use this for initialization
        public void Initialise(Character NewCharacter)
        {
            MyCharacter = NewCharacter;
            MySource = MyCharacter.GetComponent<AudioSource>();
            if (MySource == null)
            {
                MySource = MyCharacter.gameObject.AddComponent<AudioSource>();
            }
        }

        void OnGUI()
        {
            if (DebugGui && MyCharacter)
            {
                GUILayout.Label("QuestLog of [" + MyCharacter.name + "]");
                GUILayout.Label("Number of Quests [" + MyQuests.Count + "]");
                for (int i = 0; i < MyQuests.Count; i++)
                {
                    GUILayout.Label("\t" + i + " - Quest[" + MyQuests[i].Name + "]");
                }
            }
        }

        public void RunScript(List<string> MyData)
        {
            /*bool IsReading = false;
            int BeginIndex = -1;
            for (int i = 0; i < MyData.Count; i++)
            {
                if (MyData[i] == "/QuestLog")
                {
                    IsReading = true;
                }
                else if (MyData[i] == "/EndQuestLog")
                {
                    Quest NewQuest = new Quest();
                    NewQuest.RunScript(MyData.GetRange(BeginIndex, i - 1 - BeginIndex));
                    Add(NewQuest);
                    IsReading = false;
                }
                if (IsReading)
                {
                    if (MyData[i].Contains("/quest "))
                    {
                        if (BeginIndex == -1)
                        {
                            BeginIndex = i;
                        }
                        else
                        {
                            Quest NewQuest = new Quest();
                            NewQuest.RunScript(MyData.GetRange(BeginIndex, i - 1 - BeginIndex));
                            Add(NewQuest);
                        }
                    }
                }
            }*/
        }

        /*public List<string> GetScriptList()
        {
            List<string> MyScript = new List<string>();
            MyScript.Add("/QuestLog");
            for (int i = 0; i < MyQuests.Count; i++)
            {
                MyScript.AddRange(MyQuests[i].GetScriptList());
            }
            MyScript.Add("/EndQuestLog");
            return MyScript;
        }*/


        public void Clear()
        {
            MyQuests.Clear();
        }

        public int GetSize()
        {
            return MyQuests.Count;
        }

        public Quest Get(int i)
        {
            if (i >= 0 && i < MyQuests.Count)
                return MyQuests[i];
            else return null;
        }

		public Quest Get(string QuestName) 
		{
			for (int i = 0; i < MyQuests.Count; i++)
            {
				// need to do this check when reading the quest from files - have not tested yet
				MyQuests [i].Name = ScriptUtil.CheckStringForLastChar(MyQuests [i].Name);	
				QuestName = ScriptUtil.CheckStringForLastChar(QuestName);
				if (MyQuests [i].Name == QuestName)
                {
					return MyQuests[i];
				}
			}
			return null;
		}
		
		// sets the quest to completed!
		public void SignOffQuest(Character MyFinishingCharacter, string QuestName) 
		{
			if (MyFinishingCharacter.GetQuestLog() != null)
            {
                MyFinishingCharacter.GetQuestLog().CompleteQuest(QuestName);
			}
		}

		// sets the quest to completed!
		public bool CompleteQuest(string QuestName) 
		{
			Quest MyQuest = Get (QuestName);
			if (MyQuest != null)
            {
				if (!MyQuest.HasCompleted() && !MyQuest.IsHandedIn)
                {
					MyQuest.CompleteQuest ();
                    HandleCompletedQuest();
                    OnAddQuest.Invoke();
                    return true;
				}
			}
            else
            {
				Debug.Log("Could not find Quest: " + QuestName + " to complete.");
			}
			return false;
		}

        /// <summary>
        /// When loading a quest from the class maker
        /// </summary>
        public void Add(Quest NewQuest)
        {
            if (NewQuest != null)
            {
                //NewQuest.QuestGiver = MyCharacter;
                NewQuest.QuestTaker = MyCharacter;
                if (!MyQuests.Contains(NewQuest))
                {
                    //Debug.Log("[AddQuest] Adding new Quest in " + name);
                    MyQuests.Add(NewQuest);
                    OnAddQuest.Invoke();
                }
                Guis.ZelGui MyQuestBeginGui = MyCharacter.GetGuis().Spawn("QuestBegin");
                if (MyQuestBeginGui)
                {
                    Guis.QuestBeginGui MyQuestBegin = MyQuestBeginGui.GetComponent<Guis.QuestBeginGui>();
                    if (MyQuestBegin)
                    {
                        MyQuestBegin.Initialize(NewQuest);
                    }
                }
                else
                {
                    Debug.LogError("Could not spawn quest begin gui");
                }
            }
            else
            {
                Debug.LogError("Trying to add null quest to " + MyCharacter.name);
            }
        }

        public void Remove(int MyIndex)
        {
            if (MyIndex >= 0 && MyIndex < MyQuests.Count)
            {
                MyQuests.RemoveAt(MyIndex);
            }
        }

		public int GetUncompletedQuests() 
		{
			int UncompletedQuestsCount = 0;
			for (int i = 0; i < MyQuests.Count; i++)
            {
				if (!MyQuests[i].IsHandedIn)
                {
					UncompletedQuestsCount++;
				}
			}
			return UncompletedQuestsCount;
		}

        public Quest GetQuest(int i)
        {
            return MyQuests[i];
        }

        public int GetQuestIndex(string QuestName) 
		{
			QuestName = ScriptUtil.CheckStringForLastChar (QuestName);
			for (int i = 0; i < MyQuests.Count; i++)
            {
				MyQuests[i].Name = ScriptUtil.CheckStringForLastChar (MyQuests[i].Name);
				if (MyQuests[i].Name == QuestName)
                {
					return i;
				}
			}
			return -1;
		}

		// when another quest holder is issueing the quest out
		public void GiveCharacterQuest(Character CharacterQuestTaker, string QuestName) 
		{
			QuestLog QuestTaker = CharacterQuestTaker.GetComponent<QuestLog> ();
			if (QuestTaker == null)
				return;
			//Debug.Log (name + " is giving ["+QuestName+"] to " + QuestTaker.name + " at " + Time.time);
			// First check if the SecondQuestLog(player) can recieve a quest
			if (GetUncompletedQuests () >= QuestLimit && QuestLimit != -1) 
			{
				Debug.Log ("When " + MyCharacter.name + " was giving ["+QuestName+"] it failed. Limit Reached");
				return;
			}
			
			int QuestIndex = GetQuestIndex(QuestName);
			if (QuestIndex == -1) 
			{
				Debug.LogError ("When " + MyCharacter.name + " was giving ["+QuestName+"] it failed. No Quest to give with that name.");
				//if (MyQuests.Count > 0) 
				{
					//Debug.LogError("Quest to find's length is[" + QuestName.Length + "] and QuestLog Quest's length is [" + MyQuests[0].Name.Length + "]");
					//Debug.LogError("Quest to find's name is [" + QuestName + "] and QuestLog Quest's name is: [" + MyQuests[0].Name + "]");
					//Debug.LogError("Number " +(QuestName.Length-2)+" [" + QuestName[QuestName.Length-2] + "] [" + ((int)QuestName[QuestName.Length-2]) + "]");
					//Debug.LogError("Number " +(QuestName.Length-1)+" [" + QuestName[QuestName.Length-1] + "] [" + ((int)QuestName[QuestName.Length-1]) + "]");
				}
				return;
			}
			
			if (QuestIndex < MyQuests.Count && QuestIndex >= 0)
            {
				Quest NewQuest = MyQuests [QuestIndex];
				if (NewQuest.HasGivenQuestOut())
                {
					Debug.Log ("When " + MyCharacter.name + " was giving ["+QuestName+"] it failed. Quest has already been given out.");
					return;
				}

				if (!QuestTaker.MyQuests.Contains (NewQuest)) 
				{
                    //Debug.Log("[GiveCharacterQuest] Adding new Quest in " + name);
					QuestTaker.MyQuests.Add (NewQuest);
					QuestTaker.MyQuests[QuestTaker.MyQuests.Count-1].OnAddItem(CharacterQuestTaker);	// basically make sure it's been checked at beginning of add
					if (QuestTaker.BeginQuestSound != null)
                    {
						MySource.PlayOneShot (QuestTaker.BeginQuestSound);
						//Debug.LogError("PLaying sound");
					}
					//Debug.LogError("Adding Quest");
					//Debug.LogError("Adding quest: " + MyCharacter.MyQuests[QuestIndex].Name);
					NewQuest.QuestGiver = MyCharacter;
					NewQuest.QuestTaker = CharacterQuestTaker;

                    OnAddQuest.Invoke();
                    //HandleAddQuest ();  // handle give quest instead
					//QuestTaker.HandleAddQuest();
					return;
				}
                else
                {
					Debug.LogError ("When " + MyCharacter.name + " was giving ["+QuestName+"] it failed. " + CharacterQuestTaker.name + " already has that quest.");
				}
			}
		}

		// sets the variables of the quest to [given rewards out = true]
		public bool HandOutQuest(string QuestName)
        {
			Quest MyQuest = Get (QuestName);
			if (MyQuest == null)
				return false;
			MyQuest.HandIn ();
			return true;
		}

		// when player is handing in a quest
		public bool HandInQuest(string QuestName, Character QuestGiver)
        {
			Quest MyQuest = Get (QuestName);
            QuestLog MyQuestGiver = QuestGiver.GetQuestLog();

			if (MyQuest == null)	// if doesn't have the quest
				return false;

			if (MyQuest.HasCompleted() && !MyQuest.IsHandedIn)
			{
				//Debug.LogError (name + " Removeing quest: " + QuestIndex);
				// now trade rewards over
				
				if (!MyQuestGiver.HandOutQuest(QuestName)) 
				{
					//	if quest giver no longer has that quest contract
					//return false;
				}
				MyQuest.HandIn();
                OnAddQuest.Invoke();

                if (OnHandInQuestSound != null)
					MySource.PlayOneShot (OnHandInQuestSound);
				
				for (int i = 0; i < MyQuest.MyConditions.Count; i++)
                {
					if (MyQuest.MyConditions[i].IsInventory())
                    {
						Inventory MyInventory = MyCharacter.GetBackpackItems();
                        Inventory MyInventory2 = QuestGiver.GetBackpackItems();
                        if (MyInventory != null && MyInventory2 != null)
                        {
							MyInventory.GiveItem(MyInventory2, MyQuest.MyConditions[i].ObjectName, MyQuest.MyConditions[i].ItemQuantity);
						}
					}
				}
				for (int i = 0; i < MyQuest.MyRewards.Count; i++) {
					Reward MyReward = MyQuest.MyRewards[i];
					if (MyReward.IsInventory) 
					{
						Debug.Log(QuestGiver.name + " is rewarding: " + MyCharacter.name);
						Inventory MyInventory = MyCharacter.GetBackpackItems();
						Inventory MyInventory2 = QuestGiver.GetBackpackItems();
						if (MyInventory != null && MyInventory2 != null)
                        {
							MyInventory2.GiveItem(MyInventory, MyReward.ItemName, MyReward.ItemQuantity);
						}
					}
				}
				return true;
			}
			return false;
		}

		// Quest Condition Checking
		private void HandleCompletedQuest()
        {
			if (OnCompleteQuestSound != null)
				MySource.PlayOneShot (OnCompleteQuestSound);
			OnCompletedQuest.Invoke();
		}

		public void OnTalkTo(GameObject MyCharacter)
		{
			for (int i = 0; i < MyQuests.Count; i++) // maybe pass into the quest, Object name and Event Type (Seth PickedUp Item, Seth TalkedTo Lotus, etc)
			{
				if (MyQuests[i].GetCurrentCondition().IsTalkTo())
                {
					if (MyQuests[i].GetCurrentCondition().ObjectName == MyCharacter.name) 
					{
						bool HasFinished = MyQuests[i].HasCompleted();
						MyQuests[i].GetCurrentCondition().Complete();
						MyQuests[i].CheckCompleted();
						
						if (!HasFinished && MyQuests[i].HasCompleted()) 
						{
							HandleCompletedQuest();
						}
					}
				}
			}
		}

		public void OnAddItem() 
		{
            //Debug.LogError("Calling QuestLog OnAddItem: " + name);
			// Check quests for item type conditions
			for (int i = 0; i < MyQuests.Count; i++) 
			{
				if (MyQuests[i].QuestTaker == MyCharacter)
				{	// only check if i am the one doing the quest
					bool HasFinished = MyQuests[i].HasCompleted();

					bool DidTrigger = MyQuests[i].OnAddItem(MyCharacter);

					if (!HasFinished && MyQuests[i].HasCompleted()) 
					{
						HandleCompletedQuest();
					}
					
					if (DidTrigger)
                    {
						OnAddQuest.Invoke();
					}
				}
			}
		}

		public void OnZone(string ZoneName, string Action)
        {
			//Debug.LogError ("Handing Leaving of: " + ZoneName);
			for (int i = 0; i < MyQuests.Count; i++)
            {
				bool HasFinished = MyQuests[i].HasCompleted();

				bool DidTrigger = MyQuests[i].OnZone(ZoneName, Action);

				if (!HasFinished && MyQuests[i].HasCompleted()) 
				{
					HandleCompletedQuest();
				}
				if (DidTrigger)
                {
					OnAddQuest.Invoke();
				}
            }
        }

        // Checks

        /// <summary>
        /// Returns true if the user has the quest
        /// </summary>
        public bool HasQuest(string QuestName)
        {
			Quest MyQuest = Get (QuestName);
			return (MyQuest != null);
		}

        /// <summary>
        ///  returns true if the user has handed in the quest
        /// </summary>
        public bool HasHandedInQuest(string QuestName)
        {
			Quest MyQuest = Get (QuestName);
			if (MyQuest == null)
				return false;
			return (MyQuest.IsHandedIn);
		}

        /// <summary>
        ///  returns true if the user has unfinishquest
        /// </summary>
        public bool HasUnfinishedQuest(string QuestName) 
		{
			Quest MyQuest = Get (QuestName);
			if (MyQuest == null)
            {
				return false;
			} 
			else 
			{
				return !MyQuest.HasCompleted();
			}
		}

        /// <summary>
        ///  returns true if the user has the completed quest and not handed it in
        /// </summary>
        public bool HasCompletedQuest(string QuestName) 
		{
			Quest MyQuest = Get (QuestName);
			if (MyQuest == null)
            {
				return false;
			} 
			else 
			{
				if (MyQuest.HasCompleted() && !MyQuest.IsHandedIn)
                {
                    return true;
                }
				else
                {
                    return false;
                }
			}
		}
	}
}