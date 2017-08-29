//-----------------------------------------
//			Universal Coroutine
// Copyright (c) 2016 Jeroen van Pienbroek
//------------------------------------------
using UnityEditor;
using UnityEngine;

namespace UniversalCoroutine
{
	/// <summary>Editor script for CoroutineTester class to add buttons to test functionality</summary>
	[CustomEditor(typeof(CoroutineTester))]
	public class CoroutineTesterEditor: Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			CoroutineTester tester = (CoroutineTester)target;
			if(GUILayout.Button("Start coroutine"))
			{
				tester.StartCoroutines();
			}
			else if(GUILayout.Button("Stop coroutines"))
			{
				tester.StopCoroutines();
			}
			else if(GUILayout.Button("Pause coroutines"))
			{
				tester.PauseCoroutines();
			}
			else if(GUILayout.Button("Resume coroutines"))
			{
				tester.ResumeCoroutines();
			}
		}
	}
}