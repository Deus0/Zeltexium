using UnityEngine;
using Zeltex.Characters;

/*namespace MyCustomDrawers 
{
	#if UNITY_EDITOR
	using UnityEditor;
	[CustomEditor(typeof(CharacterSpawner))]
	public class ObjectBuilderEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			var style = new GUIStyle(GUI.skin.button);
			style.normal.textColor = Color.white;
			style.hover.textColor = Color.cyan;
			style.onHover.textColor = Color.cyan;
			GUI.backgroundColor = Color.grey;
			GUILayout.Label ("Character Spawner", style);
			CharacterSpawner MyCharacterSpawner = (CharacterSpawner)target;
			GUI.backgroundColor = Color.black;
			string MyLabelThing = MyCharacterSpawner.SaveFileName;
			if (MyLabelThing == "")
				MyLabelThing = "(Click Me)";
			if (GUILayout.Button (MyLabelThing, style)) {
				MyCharacterSpawner.SaveFileName = EditorUtility.OpenFilePanel (
					"Select a script",
					"",
					"txt");
			}
			
			if(GUILayout.Button("Load", style))
			{
				MyCharacterSpawner.Load();
			}
			if(GUILayout.Button("Update", style))
			{
				MyCharacterSpawner.DoneEditing();
			}
			GUI.backgroundColor = Color.white;

			DrawDefaultInspector();
		}
	}
#endif
}*/