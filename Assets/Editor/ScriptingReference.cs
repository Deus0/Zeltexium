using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Zeltex.Util;
using Zeltex.Dialogue;

/*
To do:
make a text file with the commands
the commands like functions will translate into /execute codes
 * */
	
public class ScriptingReference : EditorWindow 
{
	public static List<bool> IsOn = new List<bool>();

	[MenuItem ("Marz/Scripting Reference")]
	public static void  ShowWindow () {
		IsOn.Clear ();
		for (int i = 0; i < ScriptUtil.MyMainTags.Length; i++)
        {
			IsOn.Add(false);
		}
		ScriptingReference window  = (ScriptingReference)EditorWindow.GetWindow(typeof(ScriptingReference), false, "Scripting Reference");
	}
	public int selGridInt;
	public void OnGUI () 
	{
		if (IsOn.Count == 0) {
			for (int i = 0; i < ScriptUtil.MyMainTags.Length; i++)
            {
				IsOn.Add (false);
			}
		}
		GUIStyle MyStyle1 = new GUIStyle(GUI.skin.button);
		MyStyle1.normal.textColor = new Color(0.4f, 0.9f, 0.9f);	
		MyStyle1.alignment = TextAnchor.MiddleCenter;
		MyStyle1.fontStyle = FontStyle.Bold;

		GUIStyle MyStyle = new GUIStyle(GUI.skin.button);
		MyStyle.normal.textColor = new Color(0.4f, 0.9f, 0.9f);	
		MyStyle.alignment = TextAnchor.MiddleLeft;
		GUIStyle MyStyle2 = new GUIStyle (EditorStyles.foldout);
		MyStyle.normal.textColor = new Color(0.8f, 0.9f, 0.7f);	
		MyStyle.onFocused.textColor = Color.cyan; 
		Color MyStyleColour = Color.cyan;
		MyStyle2.fontStyle = FontStyle.Bold;
		MyStyle2.normal.textColor = MyStyleColour;
		MyStyle2.onNormal.textColor = MyStyleColour;
		MyStyle2.hover.textColor = Color.white;
		MyStyle2.onHover.textColor = Color.white;
		MyStyle2.focused.textColor = Color.green;
		MyStyle2.onFocused.textColor = MyStyleColour;
		MyStyle2.active.textColor = MyStyleColour;
		MyStyle2.onActive.textColor = MyStyleColour;

		EditorGUIUtility.LookLikeInspector ();
		GUI.backgroundColor = new Color(0.08f, 0.08f, 0.08f);

		GUI.Label(new Rect(0,0,position.width, position.height),"", MyStyle);
		GUILayout.Label("Scripting Reference", MyStyle1);
		GUILayout.Label("A quick list of all the script commands",MyStyle);

		GUI.BeginGroup(new Rect(0,0,position.width, position.height));
		GUILayout.Label("Main Commands",MyStyle);
		for (int i = 0; i < ScriptUtil.MyMainTags.Length; i++) {
			
			GUI.backgroundColor = new Color(1f, 1f, 1f);
			GUI.contentColor = new Color(1f, 1f, 1f);
			IsOn[i] =  EditorGUILayout.Foldout(IsOn[i], "[" + (i+1) + "] : /" + ScriptUtil.MyMainTags[i], MyStyle2);
			GUI.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
			if (IsOn[i]) {
				if (i == 0 ) 
				{
					GUILayout.Label("\tSpeech",MyStyle);
					GUILayout.Label("\t/[Character_Name] [SpeechText]",MyStyle);
					GUILayout.Label("\t[SpeechText] (Ommited command for player speech)",MyStyle);
					GUILayout.Label("\t/questname [Quest_Name]",MyStyle);
					
					GUI.backgroundColor = new Color(0.15f, 0.12f, 0.12f);
					GUILayout.Label("\n\tConditions",MyStyle);
					GUI.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
					for (int j = 0; j < DialogueConditions.MyCommands.Length; j++) {
						GUILayout.Label("\t\t[" + (j+1) + "] : /" + DialogueConditions.MyCommands[j],MyStyle);
					}
					GUI.backgroundColor = new Color(0.15f, 0.12f, 0.12f);
					GUILayout.Label("\n\tFunctions",MyStyle);
					GUI.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
					for (int j = 0; j < DialogueFunctions.MyCommands.Length; j++) {
						GUILayout.Label("\t\t[" + (j+1) + "] : /" + DialogueFunctions.MyCommands[j],MyStyle);
					}
				}
				else if (i == 1 && IsOn[i]) 
				{
					GUI.backgroundColor = new Color(0.15f, 0.12f, 0.12f);
					GUILayout.Label("\tQuest Commands",MyStyle);
					GUI.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
					/*for (int j = 0; j < QuestSystem.Quest.MyCommands.Length; j++) {
						GUILayout.Label("\t\t[" + (j+1) + "] : /" + QuestSystem.Quest.MyCommands[j],MyStyle);
					}*/
				}
			}
		}
		GUI.EndGroup();
	}

}