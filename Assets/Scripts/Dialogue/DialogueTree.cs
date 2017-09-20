using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using Zeltex.Quests;
using Zeltex.Util;
using Zeltex.Characters;
using Newtonsoft.Json;

namespace Zeltex.Dialogue
{
    /// <summary>
    /// This contains all the speech data
    /// </summary>
	[System.Serializable]
	public class DialogueTree : Element
    {
        #region Variables
        [Tooltip("A list of the Dialogue Tree Branches.")]
        [SerializeField, JsonProperty]
        private List<DialogueData> MyDialogues = new List<DialogueData>();
        [Tooltip("The count how many times someone has talked to the character")]
		[SerializeField]
        public int ChattedCount = 0;							// 
		[Tooltip("The index that the speech is at.")]
		[SerializeField]
        private int DialogueIndex = 0;
        [HideInInspector]
        public UnityEvent OnEndTree = new UnityEvent();
        #endregion

        #region Initialize
        public DialogueData GetDialogue(int Index)
        {
            return MyDialogues[Index];
        }
        public DialogueData GetDialogue(string DialogueName)
        {
            for (int i = 0; i < MyDialogues.Count; i++)
            {
                if (MyDialogues[i].Name == DialogueName)
                {
                    return MyDialogues[i];
                }
            }
            return null;
        }
        public DialogueData Get(int index)
        {
            return (MyDialogues[index]);
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get the dialogue trees stats
        /// </summary>
        public List<string> GetStatistics()
        {
            List<string> MyData = new List<string>();
            MyData.Add("MyTree" + "'s DialogueTree Index: " + (GetIndex() + 1) + " of " + GetSize());
            MyData.Add("Chatted Count [" + ChattedCount + "]");
            DialogueData CurrentLine = GetCurrentDialogue();
            if (CurrentLine != null)
            {
                MyData.Add("Inside Dialogue " + CurrentLine.Name + " with " + CurrentLine.SpeechLines.Count +
                         " Speech lines and " + CurrentLine.MyConditions.Count + " Conditions and " + 0 + " Functions.");
                MyData.Add("SpeechIndex [" + (CurrentLine.SpeechIndex + 1) + "/" + CurrentLine.SpeechLines.Count + "]");

                MyData.Add("  [Speech Lines]");
                for (int i = 0; i < CurrentLine.SpeechLines.Count; i++)
                {
                    MyData.Add("\t Speech: " + CurrentLine.SpeechLines[i].GetLabelText());
                }

                MyData.Add("  [Conditions]");
                for (int i = 0; i < CurrentLine.MyConditions.Count; i++)
                {
                    MyData.Add("\t Condition: " + CurrentLine.MyConditions[i].GetLabel());
                }
            }
            else
            {
                MyData.Add("Current Speech Line is null");
            }
            return MyData;
        }
        #endregion

        #region File
        /// <summary>
        /// If split up using /id tags
        /// </summary>
        public void RunScript(List<string> MyData)
        {
            //Debug.LogError("[RunScript] Running DialogueTree Scripts of count: " +
            //    MyData.Count +  "\n" + FileUtil.ConvertToSingle(MyData));
            int LastId = -1;
            for (int i = 0; i < MyData.Count; i++)
            {
                if (MyData[i].Contains("/Dialogue"))
                {
                    //Debug.LogError("Dialogue Beginning at: " + i);
                    for (int j = i + 1; j < MyData.Count; j++)
                    {
                        if (MyData[j].Contains("/EndDialogue"))    // the last line
                        {
                            //Debug.LogError("Dialogue Ending at: " + j);
                            if (LastId != -1)
                            {
                                List<string> MyDialogueScript = MyData.GetRange(LastId, (j - 1) - LastId + 1);
                                DialogueData NewDialogue = new DialogueData();  //GetSize() + 1
                                NewDialogue.RunScript(MyDialogueScript);
                                MyDialogues.Add(NewDialogue);
                            }
                            break;
                        }
                        else if (MyData[j].Contains("/id"))
                        {
                            if (LastId != -1)
                            {
                                List<string> MyDialogueScript = MyData.GetRange(LastId, (j) - LastId);  // - 1
                                //Debug.Log("Reading in new leaf: " + FileUtil.ConvertToSingle(MyDialogueScript));
                                DialogueData NewDialogue = new DialogueData();//GetSize() + 1
                                NewDialogue.RunScript(MyDialogueScript);
                                MyDialogues.Add(NewDialogue);
                            }
                            LastId = j; // begin recording when it starts!
                        }
                    }
                }
                //Debug.Log("[RunScriptQuest] Adding new Quest in " + name);
            }
        }
        public List<string> GetScriptList()
        {
            List<string> MyScript = new List<string>();
            MyScript.Add("/Dialogue");
            for (int i = 0; i < MyDialogues.Count; i++)
            {
                MyScript.AddRange(MyDialogues[i].GetScriptList());
            }
            MyScript.Add("/EndDialogue");
            return MyScript;
        }
        #endregion

        #region SpeechProgression
        private bool IsFirstChat()
        {
            return (ChattedCount == 0);
        }

        /// <summary>
        /// Skips any underlying dialogue, goes through their conditions and actions though!
        /// </summary>
        private void CheckForEmptyDialogue(Character MyCharacter, Character OtherCharacter)
        {
			int MaxChecks = 1000;
			int ChecksNumber = 0;
			while (ChecksNumber < MaxChecks)
			{
				if (DialogueIndex >= 0 && DialogueIndex < MyDialogues.Count)
                {
					if (MyDialogues [DialogueIndex].IsEmptyChat ())
                    {
						Debug.Log ("Skipping Dialogue: " + DialogueIndex);
                        MyDialogues[DialogueIndex].GetNextDialogueName(IsFirstChat(), MyCharacter, OtherCharacter, 0); // no choice should be given if no options
					}
					else 
					{
                        break;
					}
					ChecksNumber++;
				}
                else
                {
                    break;
                }
			}
		}

        /// <summary>
        /// Moves the dialogue along
        /// </summary>
        public void ProgressDialogue(int OptionsIndex, Character MyCharacter, Character OtherCharacter)
        {
			DialogueData CurrentDialogue = GetCurrentDialogue ();
			if (CurrentDialogue != null)
            {
				if (CurrentDialogue.HasEnded())
                {
					Debug.Log ("Line has Ended. Moving to next Line. At " + (DialogueIndex + 1) + " out of " + (MyDialogues.Count));

                    // inside dialogueline is where 
                    //		-functions are activated
                    // 		-conditions are checked for dialoguetree
                    bool IsFirstChat = (ChattedCount == 0);
                    string NextDialogueName = CurrentDialogue.GetNextDialogueName(IsFirstChat, MyCharacter, OtherCharacter, OptionsIndex);
                    if (NextDialogueName != DialogueGlobals.EndOfDialogue)
                    {
                        DialogueIndex = GetDialogueIndex(NextDialogueName); // use identifier for next  dialogue - change index!
                        CheckForEmptyDialogue(MyCharacter, OtherCharacter);
                    }
                    else
                    {
                        DialogueIndex = MyDialogues.Count;
                    }
                    if (DialogueIndex >= MyDialogues.Count) 
					{
						OnEndTree.Invoke();
                    }
                }
				else 
				{
					CurrentDialogue.NextLine();
				}
			}
        }

        /// <summary>
        /// This is what causes loops, a jump to dialogue data statement
        /// </summary>
        public int GetDialogueIndex(string MyDialogueName)
        {
            for (int i = 0; i < MyDialogues.Count; i++)
            {
                if (MyDialogues[i].Name == MyDialogueName)
                {
                    return i;
                }
            }
            return -1;
        }

        public void End()
        {
			ChattedCount++;
			//DialogueIndex = 0;
		}

		public void GetNextDialogueLine(Character MyCharacter, Character OtherCharacter)
        {
			GetNextDialogueLine (0, MyCharacter, OtherCharacter);
		}

		public void GetNextDialogueLine(int OptionsIndex, Character MyCharacter, Character OtherCharacter)
        {
            string NextDialogueName = MyDialogues[DialogueIndex].GetNextDialogueName(
                        (ChattedCount == 0),
                        MyCharacter,
                        OtherCharacter,
                        OptionsIndex);

            DialogueIndex = GetDialogueIndex(NextDialogueName);
		}
        #endregion

        #region List
        public void Clear()
        {
            MyDialogues.Clear();
        }
        public void Reset()
        {
            Reset(null, null);
        }
        public void Reset(Character MyCharacter, Character OtherCharacter)
        {
            DialogueIndex = 0;
            CheckForEmptyDialogue(MyCharacter, OtherCharacter);
            DialogueData MyDialogue = GetCurrentDialogue();
            if (MyDialogue != null)
                MyDialogue.Reset();
        }


        /// <summary>
        /// Gets the current dialogue the tree is up to
        /// </summary>
        public DialogueData GetCurrentDialogue() 
		{
			if (DialogueIndex >= 0 && DialogueIndex < MyDialogues.Count)
            {
                return MyDialogues[DialogueIndex];
            }
			else
            {
                return null;
            }
        }

		public int GetIndex()
        {
			return DialogueIndex;
		}
        public bool IsEmpty()
        {
            return (MyDialogues.Count == 0);
        }
		public int GetSize()
        {
			return MyDialogues.Count;
		}
		public void Add(DialogueData NewData)
        {
			MyDialogues.Add(NewData);
		}
        public void RemoveAt(int MyIndex)
        {
            if (MyIndex >= 0 && MyIndex < MyDialogues.Count)
            {
                MyDialogues.RemoveAt(MyIndex);
            }
        }
        #endregion
    }
}