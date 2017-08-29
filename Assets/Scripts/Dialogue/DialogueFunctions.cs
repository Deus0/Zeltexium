using UnityEngine;
using Zeltex.Quests;
using Zeltex.Util;
using Zeltex.Guis;
using Zeltex.Characters;

namespace Zeltex.Dialogue
{

	public static class DialogueFunctions
    {
		public static string[] MyCommands = new string[]
        {
            "givequest",
            "giveitem",
            "completequest",
            "exit",
            "trade",
            "openstats",
            "openquestlog" ,
            "execute"
        };

		public static bool IsFunction(DialogueData MyDialogue, string MyLine, Character MyCharacter)
        {
			string Other = ScriptUtil.RemoveCommand(MyLine);
			if (MyLine.Contains ("/givequest "))	// gives a quest to the player
			{
				//IsQuestGive = true;
				MyDialogue.InputString = Other;
			    #if UNITY_EDITOR
				//if (MyCharacter.GetComponent<SpeechHandler>())
					//UnityEditor.Events.UnityEventTools.AddPersistentListener(MyDialogue.OnNextLine2,
					//                                                         MyCharacter.GetComponent<QuestLog>().GiveCharacterQuest);
				#else
				//if (MyCharacter.GetComponent<GuiSystem.GuiManager>())
				//	MyDialogue.OnNextLine.AddEvent(delegate{ MyCharacter.GetComponent<QuestLog>().SignOffQuest(); });
				#endif
				return false;
			}  
			else if (MyLine.Contains ("/giveitem "))	// gives a quest to the player
			{
				//IsQuestGive = true;
				MyDialogue.InputString = Other;
				#if UNITY_EDITOR
				//if (MyCharacter.GetComponent<SpeechHandler>())
					//UnityEditor.Events.UnityEventTools.AddPersistentListener(MyDialogue.OnNextLine2,
					//                                                         MyCharacter.GetInventory().GiveItem);
				#else
				//if (MyCharacter.GetComponent<GuiSystem.GuiManager>())
				//	MyDialogue.OnNextLine2.AddEvent(delegate{ MyCharacter.GetComponent<Zeltex.Items.Inventory>().GiveItem();});
				#endif
				return false;
			}  
			// if player has finished a quest, rewards them and removes the quest here!
			else if (MyLine.Contains ("/completequest "))
			{
				//AddCondition("completequest");
				MyDialogue.InputString = Other;
                //MyDialogue.OnNextLine2.AddEvent(MyCharacter.GetQuestLog().SignOffQuest);
				return true;
			}
			// ends the dialogue
			else if (MyLine.Contains("/exit"))
            {
                //MyDialogue.OnNextLine.AddEvent(delegate { MyCharacter.GetComponent<SpeechHandler>().ExitChat(); });
            }
			else if (MyLine.Contains("/trade"))
            {
                //MyDialogue.OnNextLine.AddEvent( delegate{MyCharacter.GetComponent<GuiManager>().EnableGui("Inventory"); });
			}
			else if (MyLine.Contains("/openstats"))
            {
                //MyDialogue.OnNextLine.AddEvent(delegate { MyCharacter.GetComponent<GuiManager>().EnableGui("Stats"); });
            }
            else if (MyLine.Contains("/openquestlog"))
            {
                //MyDialogue.OnNextLine.AddEvent(delegate { MyCharacter.GetComponent<GuiManager>().EnableGui("QuestLog"); });
			}
			else if (MyLine.Contains ("/execute "))	// I guess this is the main one!
			{
				MyLine = ScriptUtil.RemoveCommand(MyLine);
				AddExecuteCommand(MyDialogue, MyLine, MyCharacter.gameObject);
				return true;
			}
			return false;
		}

		public static void AddExecuteCommand(DialogueData MyDialogue, string ExecuteCommand, GameObject MyCharacter)
        {
			string[] MyCommands = ExecuteCommand.Split('.');
			if (MyCommands.Length == 3 || MyCommands.Length == 3) 
			{
				GameObject MyTargetObject = GameObject.Find (MyCommands[0]);
				if (MyCommands.Length == 3)
					MyTargetObject = GameObject.Find (MyCommands[0]);
				else 
					MyTargetObject = MyCharacter;
				if (MyTargetObject)
                {
					Component MyComponent;
					if (MyCommands.Length == 3)
                    {
                        MyComponent = MyTargetObject.GetComponent(MyCommands[1]);
                    }
					else
                    {
                        MyComponent = MyTargetObject.GetComponent(MyCommands[0]);
                    }
					if (MyComponent)
                    {
						string MyFunctionName;
						if (MyCommands.Length == 3)
							MyFunctionName = MyCommands[2];
						else
							MyFunctionName = MyCommands[1];
						#if UNITY_EDITOR
						UnityEditor.Events.UnityEventTools.AddStringPersistentListener(MyDialogue.OnNextLine,
						                                                               MyComponent.BroadcastMessage,
						                                                               MyFunctionName);
						#else
						MyDialogue.OnNextLine.AddEvent(delegate{
							//Debug.LogError("Trying to: " + FunctionName + " to " + MyComponent.name);
							if (MyComponent != null)
								MyComponent.BroadcastMessage(MyCommands[2]);
						});
						#endif
					}
				}
			} 
			if (MyCommands.Length == 1)
            {
				#if UNITY_EDITOR
				UnityEditor.Events.UnityEventTools.AddStringPersistentListener(MyDialogue.OnNextLine,
				                                                               MyCharacter.BroadcastMessage,
				                                                               MyCommands[0]);
				#else
				MyDialogue.OnNextLine.AddEvent( delegate{
					//Debug.LogError("Trying to: " + FunctionName + " to " + MyComponent.name);
					MyCharacter.BroadcastMessage(MyCommands[0]);
				});
				#endif
			}
            else {

				Debug.LogError("Dialogue Execute MyCommands.Length: " + MyCommands.Length);
			}
		}

	}
}