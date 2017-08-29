using UnityEngine;
using System.Collections;
using Zeltex.Dialogue;

/*public class DialogueDataGui {
	//
	public GameObject MyLinkedGameObject;
	public int MyIndex = 0;
	//int SelectedFunctionIndex = 0;
	public string Name;
	public Rect MyRect;
	public DialogueData MyDialogueLine;	
	// shrinking
	public bool IsShrunk = false;
	public Vector2 BeforeShrinkSize;
	private static Vector2 ShrinkSize = new Vector2(60,45);
	private static float MarginX = 15f;
	private static float TextHeight = 20f;
	int PositionY = 0;
	int ListBegin = 0;

	public Rect GetColumnRect() {
		return new Rect (0f, ++PositionY * TextHeight, MyRect.width-MarginX, TextHeight);
	}
	public Rect GetListBeginRect(int type) {
		if (type == 0)	// for label of a list
			return new Rect (0f, ListBegin * TextHeight, 80, TextHeight);
		if (type == 1)	// for + button
			return new Rect (80, ListBegin * TextHeight, 20, 16);
		if (type == 2)	// for - button
			return new Rect (80 + 18, ListBegin * TextHeight, 20, 16);
		else
			return GetColumnRect ();
	}
	public void RenderSpeech() 
	{
		GUI.Label (GetColumnRect(), MyLinkedGameObject.name + " Speech");
		//MyDialogueLine.SpeechDialogue = GUI.TextField (new Rect (0f, ++PositionY * TextHeight, MyRect.width - MarginX, TextHeight), MyDialogueLine.SpeechDialogue);
		
		//Reverse list with + and -
		ListBegin = ++PositionY;
		//GUI.Label (GetListBeginRect(0), "Player Speech");
		// Add Function Button
		if (GUI.Button (GetListBeginRect(1), "+")) {
			MyDialogueLine.AddSpeechLine ();
		}
		// Remove Function Button
		if (GUI.Button (GetListBeginRect(2), "-")) {
			MyDialogueLine.SpeechLines.RemoveAt (MyDialogueLine.SpeechLines.Count - 1);
		}
		// and list itself
		for (int i = 0; i < MyDialogueLine.SpeechLines.Count; i++) {
			GUI.TextField (GetColumnRect(), MyDialogueLine.SpeechLines [i].GetLabelText());
			//MyDialogueLine.SpeechLines [i] = GUI.TextField (GetColumnRect(), MyDialogueLine.SpeechLines [i].GetLabelText());
		}
		EditorGUILayout.Space ();
	}
	public void RenderConditions() {
		// Conditions
		ListBegin = ++PositionY;
		GUI.Label (GetListBeginRect(0), "Conditions");
		if (GUI.Button (GetListBeginRect(1), "+")) {
			MyDialogueLine.AddCondition ("", -1);
		}
		// Remove Function Button
		if (GUI.Button (GetListBeginRect(2), "-")) {
			MyDialogueLine.RemoveCondition (MyDialogueLine.MyConditions.Count - 1);
		}
		// and list itself
		for (int i = 0; i < MyDialogueLine.MyConditions.Count; i++) {
			MyDialogueLine.MyConditions [i].Command = (GUI.TextField (GetColumnRect(), MyDialogueLine.MyConditions [i].Command));
		}
		
		// List for Pointer variables
		ListBegin = ++PositionY;
		GUI.Label (new Rect (0f, ListBegin * TextHeight, 80, TextHeight), "Pointers");
		// and list itself
		EditorGUILayout.Space ();
	}
	public void RenderFunctions()
	{
		GUI.Label (GetColumnRect(), "Functions:");
		//MyDialogueLine.IsExitChat = GUI.Toggle (new Rect (0f, ++PositionY * TextHeight, MyRect.width-MarginX, TextHeight), MyDialogueLine.IsExitChat, "Exit Chat");
		//MyDialogueLine.IsCompleteQuest = GUI.Toggle (GetColumnRect(), MyDialogueLine.IsCompleteQuest, "Complete Quest");
		//MyDialogueLine.IsQuestGive = GUI.Toggle (GetColumnRect(), MyDialogueLine.IsQuestGive, "Give Quest");
		//if (MyDialogueLine.IsQuestGive || MyDialogueLine.IsQuestCheck || MyDialogueLine.IsCompleteQuest) 
		MyDialogueLine.InputString = GUI.TextField (GetColumnRect(), MyDialogueLine.InputString);
		
		
		//EditorGUIUtility.LookLikeControls();
		//SerializedObject MyObject = new SerializedObject (MyLinkedGameObject.GetComponent<Zeltex.Dialogue.SpeechHandler> ());
		//if (MyDialogueLine.OnNextLine.GetPersistentEventCount() > 0)
		//	foreach (SerializedProperty tar in MyObject.GetIterator()) 
		//		Debug.LogError (":" + tar.name);
		//SerializedProperty MyProperty = MyObject.FindProperty ("MyDialogues");
		//if (MyProperty.isExpanded)
	}
	
	public void Render() {
		PositionY = 0;
		if (MyLinkedGameObject == null) {
			GUI.Label (GetColumnRect (), "No GameObject Linked!");
			return;
		} else {
			//GUI.Label (GetColumnRect (), "GameObject: " + MyLinkedGameObject.name);
			//return;
		}
		if (!IsShrunk) {
			RenderSpeech ();
			RenderConditions ();
			RenderFunctions ();
		} else {
		}
	}
	float GetThingy() {
		return 0;
	}
	public void Shrink() {
		if (IsShrunk) {
			MyRect.size = BeforeShrinkSize;
		} else {
			BeforeShrinkSize = MyRect.size;
			MyRect.size = ShrinkSize;
			//float CharacterLength = GetThingy();
			//if (CharacterLength != 0)
			//	MyRect.width = CharacterLength*6f+20f;	// assuming font is roughly this size
		}
		IsShrunk = !IsShrunk;
	}
}
#endif
*/


/*public string GetLabelText() 
{
	string NewLabel = "";
	string MySpeech = MyDialogueLine.GetSpeechDialogue (true);
	if (MySpeech != "") 
	{
		NewLabel += "Npc Speech:\n";
		NewLabel += MySpeech + "\n";
	} else {
		NewLabel += "No Npc Speech\n";
	}
	string MyReverseSpeech = "";
	for (int i = 0; i < MyDialogueLine.ReverseDialogueLines.Count; i++) {
		string ReverseSpeech = MyDialogueLine.ReverseDialogueLines[i];
		if (MySpeech != "") 
		{
			MyReverseSpeech += ReverseSpeech + "\n";
		}
	}
	if (MyReverseSpeech == "") {
		NewLabel += "No Reverse Speech\n";
	} else {
		NewLabel += "Reverse Speech:\n";
		NewLabel += MyReverseSpeech;
	}
	
	if (MyDialogueLine.IsCompleteQuest) {
		NewLabel += "Function: IsCompleteQuest\n";
	}
	if (MyDialogueLine.IsQuestGive) {
		NewLabel += "Function: IsQuestGive\n";
	}
	return NewLabel;
}*/
