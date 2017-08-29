using UnityEngine;
//using System.Collections;
using System.Collections.Generic;
//using UnityEngine.UI;
//using UnityEngine.Events;
//using Zeltex.Dialogue;
//using GUI3D;
//using Zeltex.Guis;
using MyCustomDrawers;
//using Zeltex.Items;
using Zeltex.Util;
//using F
//using UnityEditor;
/*	Used for easier control and stream-lining of characters
 * 		-Mostly interfaces between Character spawn and Guis
 * 
 * */


/*namespace Zeltex.Characters
{
	[SelectionBase]
	[ExecuteInEditMode]
	public class CharacterSpawner : MonoBehaviour
    {
		
		[Header("Reference")]
		public GameObject MyCharacterSpawn;
		[Tooltip("Reloads everything. Starting with the the NPC spawn. Then the scripts.")]
		private bool IsReloadAll = false;
		
		[Header("Text Data")]
		[Tooltip("DataFile InSections")]
		[Multiline(6)]
		public List<string> MyScriptSections;
		//
		#if UNITY_EDITOR
		[ReadOnly] [SerializeField] private int DialogueParts = 0;
		[ReadOnly] [SerializeField] private int QuestParts = 0;
		[ReadOnly] [SerializeField] private int ItemParts = 0;
		#else
		private int DialogueParts = 0;
		private int QuestParts = 0;
		private int ItemParts = 0;
		#endif
		[Header("Scripting")]
		[Tooltip("Location of the file. Uses spawner name if empty.")]
		[HideInInspector] public string SaveFileName = "";
		//[Tooltip("Converts the script text into character data.")]
		private bool IsActivateScript;
		//[Tooltip("Loads the script from the file. With the same name as the gameobject.")]
		private bool IsLoadScript;
		//[Tooltip("Saves the script back into the file.")]
		private bool IsSaveScript;


		[Header("Text Functions")]
		[Tooltip("Removes any empty lines in the script")]
		[SerializeField] private bool IsRemoveEmptyLines;
		[Tooltip("Removes white space")]
		[SerializeField] private bool IsRemoveWhiteSpace;
		[Tooltip("Indents any subheader lines")]
		[SerializeField] private bool IsIndentLines;

		// move these to the scripting soon

		public void DoneEditing()
        {
			SaveScript ();
			//ActivateScript ();
		}
		public void Load()
        {
			LoadScript ();
			//ActivateScript ();
		}
		// Update is called once per frame
		void Update () {
			if (IsReloadAll) 
			{
				IsReloadAll = false;
				//IsReloadCharacter = true;
				IsLoadScript = true;
				IsActivateScript = true;
			}
			UpdateTextActions ();
			UpdateScriptActions ();
		}

		public string GetDefaultLoadFileName() 
		{
			string LoadString = SaveFileName;
			if (LoadString == "")
				LoadString = gameObject.name + ".txt";
			return LoadString;
		}
		public void LoadScript() 
		{
			LoadScript (SaveFileName);
		}
		public void LoadScript(string LoadString)
		{
			MyScriptSections.Clear();
			string MyScript = FileUtil.ReadTextFile(LoadString);
			MyScriptSections = ScriptUtil.SplitSections(MyScript);
			OnUpdateScript();
		}

		private void SaveScript() 
		{
			string LoadString = SaveFileName;
			if (LoadString == "")
				LoadString = gameObject.name + ".txt";
            File.Save
			//FileUtil.SaveFile(MyScriptSections, LoadString);
		}

		private void UpdateScriptActions()
        {
			if (IsLoadScript || (IsActivateScript && MyScriptSections.Count == 0)) 
			{
				IsLoadScript = false;
				LoadScript();
			}
			if (IsSaveScript) {
				IsSaveScript = false;
				SaveScript();
			}
			if (IsActivateScript) 
			{
				IsActivateScript = false;
				//ActivateScript();
			}
		}
		private void UpdateTextActions() {
			if (IsRemoveWhiteSpace) {
				IsRemoveWhiteSpace = false;
				RemoveWhiteSpace();
			}
			if (IsRemoveEmptyLines) {
				IsRemoveEmptyLines = false;
				RemoveEmptyLines();
			}
			if (IsIndentLines) {
				IsIndentLines = false;
				IndentLines();
			}
		}
		private void OnUpdateScript() {
			DialogueParts = 0;
			ItemParts = 0;
			QuestParts = 0;
			for (int i = 0; i < MyScriptSections.Count; i++) {
				List<string> MyLines = FileUtil.ConvertToList (MyScriptSections [i]);
				if (MyLines.Count > 0) {
					if (MyLines[0].Contains("/id "))
					    DialogueParts++;
					else if (MyLines[0].Contains("/quest "))
					    QuestParts++;
					else if (MyLines[0].Contains("/item "))
					    ItemParts++;
				}
			}
		}
		private void IndentLines() 
		{
			for (int i = 0; i < MyScriptSections.Count; i++) {
				List<string> MyLines = FileUtil.ConvertToList(MyScriptSections[i]);
				for (int j = 1; j < MyLines.Count; j++) {
					if (MyLines[j].Length > 0) 
						if (MyLines[j][0] != '\t')
							MyLines[j] = '\t' + MyLines[j];
				}
				string MyLinesSingle = FileUtil.ConvertToSingle(MyLines);
				MyScriptSections[i] = MyLinesSingle;
			}
		}
		private void RemoveEmptyLines() {
			for (int i = 0; i < MyScriptSections.Count; i++) {
				List<string> MyLines = FileUtil.ConvertToList(MyScriptSections[i]);
				for (int j = MyLines.Count-1; j >= 0; j--) {
					MyLines[j] = ScriptUtil.RemoveWhiteSpace(MyLines[j]);
					if (MyLines[j] == "")
						MyLines.RemoveAt(j);
				}
				string MyLinesSingle = FileUtil.ConvertToSingle(MyLines);
				MyScriptSections[i] = MyLinesSingle;
			}
		}
		private void RemoveWhiteSpace() 
		{
			for (int i = 0; i < MyScriptSections.Count; i++) {
				List<string> MyLines = FileUtil.ConvertToList(MyScriptSections[i]);
				for (int j = 0; j < MyLines.Count; j++) {
					MyLines[j] = ScriptUtil.RemoveWhiteSpace(MyLines[j]);
				}
				string MyLinesSingle = FileUtil.ConvertToSingle(MyLines);
				MyScriptSections[i] = MyLinesSingle;
			}
		}
		private GameObject GetBodySpawn() 
		{
			if (MyCharacterSpawn == null) {
				for (int i = 0; i < transform.childCount; i++)
				{
					if (transform.GetChild(i).GetComponent<Character>()) {
						MyCharacterSpawn = transform.GetChild(i).gameObject;
						return MyCharacterSpawn;
					}
				}
			}
			return MyCharacterSpawn;
		}
	}
}*/