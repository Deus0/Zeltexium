using UnityEngine;
using System.Collections.Generic;
using Zeltex.Quests;
using Zeltex.Dialogue;
using Zeltex.Items;
using Zeltex.Combat;

/*
	Class responsible for reading and writing from dialogue files

		rename - CharacterScriptHandler
*/

namespace Zeltex.Util
{
    public static class ScriptUtil
    {
        public static string[] MyMainTags = new string[]
        {                                               "id", 				// dialogue
														"quest",			// quests
														"item", 			// inventory items
														"characterstats", 	// character stats
														"playercontrols",
                                                        "skeleton"
        };          // player controls 
        

        public static bool ContainsMainTag(string Line)
        {
            Line = Line.ToLower();
            for (int i = 0; i < MyMainTags.Length; i++)
            {
                string CheckTag = "/" + MyMainTags[i];
                if (i < 3)
                    CheckTag += " ";    // those earlier tags need spaces in them
                if (Line.Contains(CheckTag))
                {
                    return true;
                }
            }
            return false;
        }

        // Splits up the sections using the main tags - /id - /quest - /item - /stats
        public static List<string> GetQuestLogSection(List<string> MyFiles)
        {
            List<string> MySections = SplitSections(MyFiles);
            List<string> MyQuestLog = new List<string>();
            for (int i = 0; i < MySections.Count; i++)
            {
                string[] MyLines = MySections[i].Split('\n');
                if (MyLines.Length >= 1 && MyLines[0].Contains("/quest "))
                {
                    MyQuestLog.Add(MySections[i]);
                }
                //MyQuestLog.AddRange(FileUtil.ConvertToList(MySections[i]));
            }
            return MyQuestLog;
        }
        public static List<string> GetDialogueSection(List<string> MyFiles)
        {
            List<string> MySections = SplitSections(MyFiles);
            List<string> MyDialogue = new List<string>();
            for (int i = 0; i < MySections.Count; i++)
            {
                string[] MyLines = MySections[i].Split('\n');
                if (MyLines.Length >= 1 && MyLines[0].Contains("/id "))
                {
                    MyDialogue.Add(MySections[i]);
                }
            }
            return MyDialogue;
        }
        public static List<string> SplitSections(List<string> FileText)
        {
            string MyJoinedText = FileUtil.ConvertToSingle(FileText);
            return SplitSections(MyJoinedText);
        }
        public static List<string> SplitSections(string FileText)
        {
            List<string> MyLines = new List<string>();
            if (FileText == "")
                return MyLines;
            string CurrentSection = "";
            string[] linesInFile = FileText.Split('\n');
            for (int i = 0; i < linesInFile.Length; i++)
            {
                if (ContainsMainTag(linesInFile[i]))
                {
                    if (CurrentSection != "")
                    {
                        MyLines.Add(CurrentSection);
                        CurrentSection = "";
                    }
                }
                //linesInFile[i] = SpeechUtilities.RemoveWhiteSpace(linesInFile[i]);
                if (linesInFile[i] != "\n")
                    CurrentSection += linesInFile[i] + '\n';    // adding the return /n as it is removed with split function
            }
            if (CurrentSection != "")
            {
                MyLines.Add(CurrentSection);
                CurrentSection = "";
            }
            return MyLines;
        }

        public static List<string> SplitCommands(string SavedData)
        {
            string[] MyCommandsArray = SavedData.Split(' ');

            for (int j = 0; j < MyCommandsArray.Length; j++)
            {
                if (MyCommandsArray[j].Contains(","))
                    MyCommandsArray[j] = MyCommandsArray[j].Remove(MyCommandsArray[j].IndexOf(","));
            }

            List<string> MyCommands = new List<string>();
            for (int j = 0; j < MyCommandsArray.Length; j++)
            {
                if (!FileUtil.IsEmptyLine(MyCommandsArray[j]))
                {
                    MyCommands.Add(MyCommandsArray[j]);
                }
            }
            return MyCommands;
        }

        // mostly text reading util down here


        public static string RemoveCommand(string MyString)
        {
            MyString = RemoveWhiteSpace(MyString);
            int NonCommandIndex = 0;
            for (int i = 0; i < MyString.Length; i++)
            {
                if (MyString[i] == ' ')
                {
                    NonCommandIndex = i + 1;
                    i = MyString.Length;
                }
            }
            MyString = MyString.Remove(0, NonCommandIndex);

            // remove white space from end
            MyString = RemoveWhiteSpaceFromEnd(MyString);

            //MyString = SpeechFileReader.CheckStringForLastChar (MyString);
            return MyString;
        }

        // if first non character is a slash, it is a command
        public static string GetCommand(string MyString)
        {
            MyString = RemoveWhiteSpace(MyString);  // removes any enter signs i think
            MyString.ToLower();
            int MyCommandStartIndex = -1;
            for (int i = 0; i < MyString.Length; i++)
            {
                if (MyString[i] == '/')
                {
                    MyCommandStartIndex = i;
                }
                else if (MyString[i] == ' ')
                {
                    if (MyCommandStartIndex != -1)
                    {
                        return MyString.Substring(MyCommandStartIndex, i - MyCommandStartIndex);
                    }
                }
            }
            if (MyCommandStartIndex != -1)
            {
                return MyString.Substring(MyCommandStartIndex, MyString.Length - MyCommandStartIndex);  //-1
            }
            return "No Command";
        }

        public static bool IsAllLetters(string NewInput)
        {
            NewInput = ScriptUtil.RemoveWhiteSpace(NewInput);
            for (int i = 0; i < NewInput.Length; i++)
            {
                if (!System.Char.IsLetter(NewInput[i]))
                    return false;
            }
            return true;
        }

        public static bool IsNumbersInput(string NewInput)
        {
            NewInput = ScriptUtil.RemoveWhiteSpace(NewInput);
            for (int i = 0; i < NewInput.Length; i++)
            {
                if (!System.Char.IsNumber(NewInput[i]))
                    return false;
            }
            return true;
        }

        // if first non character is a slash, it is a command
        public static bool IsCommand(string MyString)
        {
            MyString.ToLower();
            for (int i = 0; i < MyString.Length; i++)
            {
                if (System.Char.IsLetter(MyString[i]))
                {
                    return false;
                }
                else if (MyString[i] == '/')
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsNonWhiteSpace(char CheckChar)
        {
            return (System.Char.IsLetter(CheckChar) || System.Char.IsNumber(CheckChar) || System.Char.IsPunctuation(CheckChar)
                    /*|| CheckChar == '.' || CheckChar == ',' || CheckChar == '/' 
                    || CheckChar == '*' || CheckChar == '!' || CheckChar == '?'*/);
        }

        public static string RemoveWhiteSpaceFromEnd(string MyString)
        {
            for (int i = MyString.Length - 1; i >= 0; i--)
            {
                if (!IsNonWhiteSpace(MyString[i]))
                {
                    MyString = MyString.Remove(i, 1);
                }
                else {
                    i = -1;
                }
            }
            return MyString;
        }
        /// <summary>
        /// Remove the white space from a string
        /// </summary>
        public static string RemoveWhiteSpace(string MyString)
        {
            if (MyString== null)
            {
                return "";
            }
            if (MyString.Length == 0)
            {
                return MyString;
            }
            int NonWhiteSpaceIndex = 0;
            for (int i = 0; i < MyString.Length; i++)
            {
                if (IsNonWhiteSpace(MyString[i]))
                {
                    NonWhiteSpaceIndex = i;
                    i = MyString.Length;
                }
            }
            MyString = MyString.Remove(0, NonWhiteSpaceIndex);
            MyString = RemoveWhiteSpaceFromEnd(MyString);
            return MyString;
        }
        public static string RemoveNonCharacters(string MyString)
        {
            for (int i = MyString.Length - 1; i >= 0; i--)
            {
                if (!System.Char.IsLetter(MyString[i]))
                {
                    MyString.Remove(i, 1);
                }
            }
            //MyString = MyString.Remove(0, NonWhiteSpaceIndex);
            return MyString;
        }

        public static List<int> GetInts(string MyIntsString)
        {
            string[] MyInts = MyIntsString.Split(' ', ',');
            List<int> NewInts = new List<int>();

            if (MyInts != null)
                for (int j = 0; j < MyInts.Length; j++)
                {
                    if (MyInts[j].Length > 0)
                    {
                        try
                        {
                            if (System.Char.IsNumber(MyInts[j][0]))
                            {
                                int IsInt = int.Parse(MyInts[j]);
                                NewInts.Add(IsInt);
                            }
                        }
                        catch (System.FormatException e)
                        {

                        }
                    }
                }
            return NewInts;
        }


        public static string CheckStringForLastChar(string MyString)
        {
            if (string.IsNullOrEmpty(MyString))
                return "";
            if (MyString.Length > 0)
            {
                int LastChar = (int)MyString[MyString.Length - 1];
                if (LastChar == 13 || LastChar == 32)
                {
                    MyString = MyString.Remove(MyString.Length - 1);
                }
            }
            return MyString;
        }
    }

}
