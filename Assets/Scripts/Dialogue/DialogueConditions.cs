using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Zeltex.Dialogue
{
    /// <summary>
    /// The main condition used in dialogue
    /// </summary>
    [System.Serializable]
    public class DialogueCondition
    {
        [SerializeField]
        public string Command;  // /first , /questcheck
        [SerializeField]
        public string NextIndex;   // An index for 'MyNext' List

        public List<string> NextIndexes = new List<string>();

        public DialogueCondition(string NewCommand, string NextPointer)
        {
            Command = NewCommand;
            // string is just input so, make sure to read this properly
            if (Command == "default")
            {
                NextIndex = NextPointer;
            }
            else if (Command == "options")
            {
                string[] MyNextThings = NextPointer.Split(',');
                for (int i = 0; i < MyNextThings.Length; i++)
                {
                    NextIndexes.Add(MyNextThings[i]);
                }
                //NextIndex = NextPointer;
            }
        }

        public void AddNext(List<string> MyNexters)
        {
            NextIndexes = MyNexters;
        }
        public void UpdateIndexes(string OldIndexName, string NewIndexName)
        {
            if (NextIndexes.Count > 0)
            {
                for (int i = 0; i < NextIndexes.Count; i++)
                {
                    if (NextIndexes[i] == OldIndexName)
                    {
                        NextIndexes[i] = NewIndexName;
                    }
                }
            }
            else
            {
                if (NextIndex == OldIndexName)
                {
                    NextIndex = NewIndexName;
                }
            }
        }
        public string GetLabel()
        {
            if (NextIndexes.Count > 0)
            {
                string MyLabel = Command + "[";
                for (int i = 0; i < NextIndexes.Count;i++)
                {
                    MyLabel += NextIndexes[i];
                    if (i != NextIndexes.Count - 1)
                        MyLabel += ",";
                }
                MyLabel += "]";
                return MyLabel;
            }
            return Command + " [" + NextIndex + "]";
        }

        public List<string> GetScriptList()
        {
            List<string> MyScript = new List<string>();
            if (NextIndexes.Count == 0)
            {
                MyScript.Add("/" + Command + " " + NextIndex);
            }
            else
            {
                string MyScriptLine = "/" + Command + " ";
                for (int i = 0; i < NextIndexes.Count; i++)
                {
                    if (i == 0)
                        MyScriptLine += NextIndexes[i];
                    else
                        MyScriptLine += "," + NextIndexes[i];
                }
                MyScript.Add(MyScriptLine);
            }
            return MyScript;
        }
    }

    public static class DialogueConditions 
	{
		public static string[] MyCommands = new string[]
        {
                "default",
                "options",
                "first",
                "noquest",
                "unfinishedquest",
                "hascompletedquest",
                "handedinquest"
        };

        public static bool ContainsCommand(string MyCommand, string MyInput)
        {
            return MyInput.Contains("/" + MyCommand + " ");
        }
        public static bool ContainsCommand(string MyInput)
        {
            for (int i = 0; i < MyCommands.Length; i++)
            {
                if (MyInput.Contains("/" + MyCommands[i] + " "))
                {
                    return true;
                }
            }
            return false;
        }
		public static bool IsCondition(DialogueData MyDialogue, string MyLine)
        {
            //Debug.LogError("Checking for condition: " + MyLine);
            //	===-----=== Conditions ===-----===
            // if the first time talking to the npc, it will give a different route
            if (ContainsCommand(MyLine))
            {
                string MyCommand = GetCommand(MyLine);
                MyLine = RemoveCommand(MyLine);
                //Debug.LogError("Adding: " + MyCommand + ":" + MyLine);
                MyDialogue.AddCondition(MyCommand, MyLine);
                return true;
            }
            return false;
        }
        public static string RemoveCommand(string MyCommand)
        {
            for (int i = 0; i < MyCommand.Length; i++)
            {
                if (MyCommand[i] == '/')
                {
                    for (int j = i + 1; j < MyCommand.Length; j++)
                    {
                        if (MyCommand[j] == ' ')
                        {
                            return MyCommand.Substring(j + 1);
                        }
                    }
                    return "";  // nothing but a command
                }
            }
            return MyCommand;
        }
        public static string GetCommand(string MyCommand)
        {
            for (int i = 0; i < MyCommand.Length; i++)
            {
                if (MyCommand[i] == '/')
                {
                    for (int j = i+1; j < MyCommand.Length; j++)
                    {
                        if (MyCommand[j] == ' ')
                        {
                            return MyCommand.Substring(i + 1, j-i - 1);
                        }
                    }
                    return MyCommand.Substring(i + 1);
                }
            }
            return "";
        }
	}
}


/*if (ContainsCommand(MyCommands[0], MyLine))
{
    MyLine = ScriptUtil.RemoveCommand(MyLine);
    try {
        int NewIndex = int.Parse(MyLine);
        MyDialogue.AddCondition("first", NewIndex-1);	
    }
    catch (System.FormatException e)
    {

    }
    return true;
}
else if (ContainsCommand(MyCommands[1], MyLine))
{
    MyLine = ScriptUtil.RemoveCommand(MyLine);
    try {
        int NewIndex = int.Parse(MyLine);
        MyDialogue.AddCondition("noquest", NewIndex-1);
    } catch(System.FormatException e) {
        Debug.LogError(e.ToString() + " Error on line: " + MyLine);
    }
    return true;
}
else if (ContainsCommand(MyCommands[2], MyLine))
{
    MyLine = ScriptUtil.RemoveCommand(MyLine);
    try {
        int NewIndex = int.Parse(MyLine);
        MyDialogue.AddCondition("unfinishedquest", NewIndex-1);
    } catch(System.FormatException e) {
        Debug.LogError(e.ToString() + " Error on line: " + MyLine);
    }
    return true;
}
else if (ContainsCommand(MyCommands[3], MyLine))
{
    MyLine = ScriptUtil.RemoveCommand(MyLine);
    try {
        int NewIndex = int.Parse(MyLine);
        MyDialogue.AddCondition("hascompletedquest", NewIndex-1);
    } catch(System.FormatException e) {
        Debug.LogError(e.ToString() + " Error on line: " + MyLine);
    }
    return true;
}
else if (ContainsCommand(MyCommands[4], MyLine))
{
    MyLine = ScriptUtil.RemoveCommand(MyLine);
    try {
        int NewIndex = int.Parse(MyLine);
        MyDialogue.AddCondition("handedinquest", NewIndex-1);
    } catch(System.FormatException e) {
        Debug.LogError(e.ToString() + " Error on line: " + MyLine);
    }
    return true;
}
// can specify a different route for the dialogue tree
else if (ContainsCommand(MyCommands[5], MyLine))
{
    string MyCommand = GetCommand(MyLine);
    MyLine = RemoveCommand(MyLine);
    Debug.LogError("Adding Default: " + MyLine);
    MyDialogue.AddCondition(MyCommand, MyLine);
    //Debug.LogError(": " + MyDialogue.MyConditions[MyConditions.Count-1].Command);
}
else if (ContainsCommand(MyCommands[6], MyLine))	// || MyLine.Contains ("/next"))
{
    MyDialogue.ActivateOptionsCondition(MyLine);
    return true;
}*/
