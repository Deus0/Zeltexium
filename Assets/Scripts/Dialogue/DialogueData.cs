using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using Zeltex.Quests;	// atm conditions use the quest log, need to abstract this outside of the system
using Zeltex.Util;
using Zeltex.Characters;
using Zeltex;

// conditions programmed in
//	/first
//	/noquest
//	/unfinishedquest
//	/hascompletedquest
//	/handedinquest
//	/options

/*
	note
	/questcheck 5,8,9
	goes to
	/noquest 5
	/unfinishedquest 8
	/completedquest 9

	and
	/questname [quest_name]
	
 */

/*
	Stores data of the dialogue lines
	Has a function to convert command lines into dialogue data
*/


namespace Zeltex.Dialogue
{
    /// <summary>
    /// A single block of dialogue data.
    /// It's main purpose is to alter the flow of the dialogue tree.
    /// As well as add in functions on the end of the speech.
    /// </summary>
	[System.Serializable]
	public class DialogueData
    {
        #region Variables
        public string Name = "";
		[Header("Functions")]
		// a function to activate	- used for mostly preset functions like exiting the chat
		[Tooltip("Preset function calls! Just need their name!")]
		[SerializeField]
        public UnityEvent OnNextLine = new UnityEvent ();
		[SerializeField]
        public EventObjectString OnNextLine2 = new EventObjectString();
		public string InputString = "";			// this should just be first parameter for function
        public List<DialogueAction> Actions = new List<DialogueAction>();       // index for each responsee

        // All conditions for next dialogue data
        [Header("Conditions")]
        public List<DialogueCondition> MyConditions = new List<DialogueCondition>();        // index for each responsee

        // All Dialogue entries
        [Header("Speech")]
		public int SpeechIndex = 0;	// index of speech it is up to
		public List<SpeechLine> SpeechLines = new List<SpeechLine>();                           // maybe the npc has different speech he can chose as well
        private int ListIndex = 0;  // just used to
        public static string SpeakerName1 = "Character";
        public static string SpeakerName2 = "Player";
        public static string EndOfDialogue = "EndOfDialogue";
        #endregion

        #region Initiators
        public DialogueData()
        {

        }
        public DialogueData(string MyLabel, string NextLabel)
        {
            Name = MyLabel;
            AddCondition("default", NextLabel);
        }
        /// <summary>
        /// Creates a dialogue data with the default condition to go to the next one
        /// </summary>
        /*public DialogueData(int NextCount)
        {
            Name = NextCount + "";
            ListIndex = NextCount;
            AddDefaultCondition();
        }*/
        #endregion

        #region Getters

        public bool HasOptions()
        {
            for (int i = 0; i < MyConditions.Count; i++)
            {
                if (MyConditions[i].Command == "options")
                    return true;
            }
            return false;
        }

        private bool HasDefault()
        {
            for (int i = 0; i < MyConditions.Count; i++)
            {
                if (MyConditions[i].Command == "default")
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Has the dialogue section ended
        /// </summary>
        public bool HasEnded()
        {
            if (SpeechIndex >= SpeechLines.Count - 1)
            {
                SpeechIndex = 0; // reset for next time
                return true;
            }
            else
            {
                return false;
            }
        }

        // if no speech!
        public string GetSpeaker()
        {
            return GetSpeechSpeaker(SpeechIndex);
        }

        public string[] GetOptionsLines(int OptionsCount)
        {
            string[] NewLine = new string[OptionsCount];
            for (int i = 0; i < OptionsCount; i++)
            {
                NewLine[i] = GetSpeechLine(SpeechIndex);
                SpeechIndex++;
            }
            return NewLine;
        }
        public string GetSpeechLines()
        {
            string MySpeech = "";
            for (int i = 0; i < SpeechLines.Count; i++)
            {
                MySpeech += SpeechLines[i].Speech + '\n';
            }
            return MySpeech;
        }
        public string GetSpeechLine()
        {
            return GetSpeechLine(SpeechIndex);
        }

        public string GetSpeechLine(int Index)  // if its the first line  
        {
            if (Index >= 0 && Index < SpeechLines.Count)
            {
                return SpeechLines[Index].Speech;
            }
            else
            {
                return "Invalid Index";
            }
        }

        public string GetSpeechSpeaker(int Index)
        {
            if (Index >= 0 && Index < SpeechLines.Count)
            {
                return SpeechLines[Index].Speaker;
            }
            else
            {
                return "Invalid Index";
            }
        }

        public string GetAllSpeech(string SpeakerName, bool IsIndexed)
        {
            string NewLines = "";
            for (int i = 0; i < SpeechLines.Count; i++)
            {
                if (GetSpeechSpeaker(i) == SpeakerName)
                {
                    if (IsIndexed)
                        NewLines += "[" + i + "] ";
                    NewLines += GetSpeechLine(i) + "\n";
                }
            }
            return NewLines;
        }

        public int GetOptionsCount()
        {
            if (GetSpeaker() == "Player")
            {
                DialogueCondition MyCondition = GetCondition("options");
                if (MyCondition != null)
                    return MyCondition.NextIndexes.Count;
            }
            return 0;
        }
        #endregion

        #region Speech
        /// <summary>
        /// Is There no speech in this block?
        /// </summary>
        /// <returns></returns>
        public bool IsEmptyChat()
        {
            return (SpeechLines.Count == 0);
        }
        /// <summary>
        /// Go to the next line of speech
        /// </summary>
        public void NextLine()
        {
            SpeechIndex++;
        }
        /// <summary>
        /// When speech lines go to far, reset them for next time
        /// </summary>
        public void Reset()
        {
            SpeechIndex = 0;
        }
        public void AddSpeechLine(string Talker, string Speech)
        {

            SpeechLine NewSpeechLine = new SpeechLine(Talker, Speech);
            SpeechLines.Add(NewSpeechLine);
        }
        /// <summary>
        /// Crreate a new speech line
        /// </summary>
        /*public void AddSpeechLine()
        {
            SpeechLine NewSpeechLine = new SpeechLine();
            SpeechLines.Add(NewSpeechLine);
        }*/
        #endregion

        #region Conditions

        public void SetDefault(string MyNextDialogue)
        {
            SetCondition("default", MyNextDialogue);
        }
        /*void AddDefaultCondition()
        {
            AddCondition("default", "" + (ListIndex + 1));
        }*/
        public void AddCondition(string NewCommand)
        {
            AddCondition(NewCommand, "" + ListIndex);
        }

        public void AddCondition(string NewCommand, int MyNextIndex)
        {
            AddCondition(NewCommand, MyNextIndex + "");
        }

        public void AddCondition(string NewCommand, string MyPointer)
        {
            // first check if its already in list
            for (int i = 0; i < MyConditions.Count; i++)
            {
                if (MyConditions[i].Command == NewCommand)
                {
                    MyConditions[i].NextIndex = MyPointer; // sets it instead
                    return;
                }
            }
            DialogueCondition MyCondition = new DialogueCondition(NewCommand, MyPointer);
            //MyNext.Add (MyNextIndex);
            MyConditions.Add(MyCondition);
        }
        public void AddOptions(List<string> MyNextIndex)
        {
            AddCondition("options", MyNextIndex);
        }
        public void AddCondition(string NewCommand, List<string> MyNextIndex)
        {
            if (MyNextIndex.Count == 0)
                return;
            // first check if its already in list
            for (int i = 0; i < MyConditions.Count; i++)
            {
                if (MyConditions[i].Command == NewCommand)
                    return;
            }
            DialogueCondition MyCondition = new DialogueCondition(NewCommand, MyNextIndex[0]);
            MyCondition.AddNext(MyNextIndex);
            //MyNext.Add (MyNextIndex);
            MyConditions.Add(MyCondition);
        }
        public void RemoveCondition(int ConditionIndex)
        {
            //MyNext.RemoveAt (ConditionIndex);
            MyConditions.RemoveAt(ConditionIndex);
        }

        // conditions
        public string GetDefaultNext()
        {
            return GetConditionNext("default");
        }
        public DialogueCondition GetCondition(string MyConditionName)
        {
            for (int i = 0; i < MyConditions.Count; i++)
            {
                if (MyConditions[i].Command == MyConditionName)
                {
                    return MyConditions[i];
                }
            }
            return null;
        }
        public string GetConditionNext(string MyCommand)
        {
            DialogueCondition MyCondition = GetCondition(MyCommand);
            if (MyCondition != null)
                return MyCondition.NextIndex;
            else
                return "";
        }
        public void SetCondition(string MyCommand, string MyNextThing)
        {
            for (int i = 0; i < MyConditions.Count; i++)
            {
                if (MyConditions[i].Command == MyCommand)
                {
                    MyConditions[i].NextIndex = MyNextThing;
                    return;
                }
            }
        }
        /// <summary>
        /// Activates a condition thing!
        /// </summary>
        public void ActivateOptionsCondition(string MyLine)
        {
            Debug.Log("Activating Options Condition");
            MyLine = ScriptUtil.RemoveCommand(MyLine);
            //MyLine = SpeechFileReader.RemoveCommand(MyLine, "/options ");
            List<int> MyInts = ScriptUtil.GetInts(MyLine);

            //MyNext.RemoveAt(MyNext.Count-1);	// remove the last added one, and replace with custom ones
            List<string> MyNexts = new List<string>();
            for (int j = 0; j < MyInts.Count; j++)
            {
                MyNexts.Add("" + (MyInts[j] - 1));
            }
            AddCondition("options", MyNexts);
        }
        #endregion

        #region OnFinishDialogueSection

        /// <summary>
        ///  Uses conditions to find the next index in the tree
        ///  The input are the two characters talk and the options the characters have chosen
        /// </summary>
        public string GetNextDialogueName(bool IsFirst, Character MyCharacter, Character OtherCharacter, int OptionsIndex)
        {
            if (MyCharacter && OtherCharacter)
            {
                OnNextLine.Invoke();
                OnNextLine2.Invoke(OtherCharacter.gameObject, InputString);
                for (int i = 0; i < Actions.Count; i++)
                {
                    if (Actions[i].Name == "GiveQuest")
                    {
                        //QuestMaker MyQuestMaker = QuestMaker.Get();
                        Quest MyQuest = DataManager.Get().GetElement("Quests", Actions[i].Input1) as Quest;
                        MyCharacter.GetComponent<QuestLog>().Add(MyQuest);
                    }
                }
            }
            // Then check Conditions!
            // if first time talking condition
            for (int i = 0; i < MyConditions.Count; i++)
            {
                if (MyConditions[i].Command == "first")
                {
                    if (IsFirst)
                    {
                        return (MyConditions[i].NextIndex);
                    }
                }
                else if (MyConditions[i].Command == "noquest")
                {
                    if (!OtherCharacter.GetComponent<QuestLog>().HasQuest(InputString))
                    {
                        return (MyConditions[i].NextIndex);
                    }
                }
                else if (MyConditions[i].Command == "unfinishedquest")
                {
                    if (!OtherCharacter.GetComponent<QuestLog>().HasUnfinishedQuest(InputString))
                    {
                        return (MyConditions[i].NextIndex);
                    }
                }
                else if (MyConditions[i].Command == "hascompletedquest")
                {
                    if (!OtherCharacter.GetComponent<QuestLog>().HasCompletedQuest(InputString))
                    {
                        OtherCharacter.GetComponent<QuestLog>().HandInQuest(InputString, OtherCharacter);
                        return (MyConditions[i].NextIndex);
                    }
                }
                else if (MyConditions[i].Command == "handedinquest")
                {
                    if (!OtherCharacter.GetComponent<QuestLog>().HasHandedInQuest(InputString))
                    {
                        return (MyConditions[i].NextIndex);
                    }
                }
                else if (MyConditions[i].Command == "options")
                {
                    if (MyConditions[i].NextIndexes.Count == 0)
                    {
                        return MyConditions[i].NextIndex;
                    }
                    else
                    {
                        OptionsIndex = Mathf.Clamp(OptionsIndex, 0, MyConditions[i].NextIndexes.Count - 1);
                        return MyConditions[i].NextIndexes[OptionsIndex];
                    }
                }
            }
            // if all else are false, go to default
            for (int i = 0; i < MyConditions.Count; i++)
            {
                if (MyConditions[i].Command == "default")
                {
                    //Debug.LogError("Now Going To New Line " + MyConditions[i].NextIndex);
                    return MyConditions[i].NextIndex;
                }
            }
            return EndOfDialogue;
        }

        #endregion
        // need to add a variable for each response
        // or a unity function reference here, so one can be like, 'i would like to trade'->open trade window
        // string utilities

        #region File
        // Reads in a list of strings, ie Reads in the Script!
        // Takes in a reference to the characters game object
        public void RunScript(List<string> SavedData)
        {
            //Debug.LogError("Reading DialogueData: " + SavedData.Count);
            for (int i = 0; i < SavedData.Count; i++)
            {
                string MyLine = SavedData[i];
                string Other = ScriptUtil.RemoveCommand(SavedData[i]);
                //Debug.LogError("Reading: " + SavedData[i]);

                if (MyLine.Contains("/id "))
                {
                    Name = Other;
                }
                // -----SPEECH-----
                else if (!ScriptUtil.IsCommand(MyLine)) // if empty line - add as player dialogue
                {
                    //Debug.LogError("No Command in: " + SavedData[i]);
                    string CleanLine = ScriptUtil.RemoveWhiteSpace(SavedData[i]);
                    if (!FileUtil.IsEmptyLine(CleanLine))
                    {
                        SpeechLine NewSpeechLine = new SpeechLine("Player", CleanLine);
                        SpeechLines.Add(NewSpeechLine);
                    }
                }
                else if (SavedData[i].Contains("/" + SpeakerName1))  // adding new charactername
                {
                    AddSpeechLine(SpeakerName1, Other);
                    // if (SpeechDialogue != "") MyDialogueGroup.CreateNewDialogueLine();	// original idea was to just split them into new dialogue lines
                }
                else if (SavedData[i].Contains("/" + SpeakerName2))  // adding new charactername
                {
                    AddSpeechLine(SpeakerName2, Other);
                    // if (SpeechDialogue != "") MyDialogueGroup.CreateNewDialogueLine();	// original idea was to just split them into new dialogue lines
                }

                // -----Conditions!-----
                else if (DialogueConditions.IsCondition(this, SavedData[i]))
                {

                }
                else if (SavedData[i].Contains(DialogueAction.BeginTag))
                {
                    Actions.Add(new DialogueAction(Other));
                }
                // -----Data-----

                // if player has finished a quest, rewards them and removes the quest here!
                // else if (SavedData[i].Contains("/questname "))
                //{
                //    InputString = Other;
                // }
                //Debug.LogError("Reading new line [" + SavedData[i]);
            }
            /*if (!HasDefault())
            {
                AddDefaultCondition();
            }*/
        }
        public List<string> GetScriptList()
        {
            List<string> MyScripts = new List<string>();
            MyScripts.Add("/id " + Name);
            for (int i = 0; i < SpeechLines.Count; i++)
            {
                MyScripts.Add("/" + SpeechLines[i].Speaker + " " + SpeechLines[i].Speech);
            }
            for (int i = 0; i < MyConditions.Count; i++)
            {
                MyScripts.AddRange(MyConditions[i].GetScriptList());
            }
            for (int i = 0; i < Actions.Count; i++)
            {
                MyScripts.Add(Actions[i].GetScript());
            }
            return MyScripts;
        }
        #endregion
    }
}
/*

*/
/// <summary>
///  when there is options it will forcefully end the speech
/// </summary>
/*public void End()
{
    SpeechIndex = SpeechLines.Count-1;
}*/

/*public bool ListContains(List<string> MyList, string SeekedWord)
{
    for (int i = 0; i < MyList.Count; i++)
    {
        if (MyList[i] == SeekedWord)
            return true;
    }
    return false;
}*/
