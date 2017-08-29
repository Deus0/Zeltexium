//-----------------------------------------
//			Universal Coroutine
// Copyright (c) 2016 Jeroen van Pienbroek
//------------------------------------------
#if UNITY_EDITOR
using System.Collections;
using UnityEditor;

namespace UniversalCoroutine
{
	/// <summary>Class with extension "wrapper" methods for CoroutineManager methods for EditorWindow class</summary>
	public static class EditorWindowExtension
	{
		/// <summary>Starts and returns a coroutine with given routine from the Object Pool</summary>
		public static Coroutine UniStartCoroutine(this EditorWindow editorWindow, IEnumerator routine)
		{
			return CoroutineManager.StartCoroutine(routine);
		}

		/// <summary>Returns a WaitForSeconds instance with given seconds from the Object Pool</summary>
		public static WaitForSeconds UniWaitForSeconds(this EditorWindow editorWindow, float seconds)
		{
			return CoroutineManager.WaitForSeconds(seconds);
		}

		/// <summary>Returns a WaitForUpdate instance (for FixedUpdate) from the Object Pool</summary>
		public static WaitForUpdate UniWaitForFixedUpdate(this EditorWindow editorWindow)
		{
			return CoroutineManager.WaitForFixedUpdate();
		}

		/// <summary>Returns a WaitForUpdate instance (for LateUpdate) from the Object Pool</summary>
		public static WaitForUpdate UniWaitForLateUpdate(this EditorWindow editorWindow)
		{
			return CoroutineManager.WaitForLateUpdate();
		}

		/// <summary>Stops given Coroutine</summary>
		public static void UniStopCoroutine(this EditorWindow editorWindow, Coroutine coroutine)
		{
			CoroutineManager.StopCoroutine(coroutine);
		}

		/// <summary>Stops all active Coroutines</summary>
		public static void UniStopAllCoroutines(this EditorWindow editorWindow)
		{
			CoroutineManager.StopAllCoroutines();
		}

		/// <summary>Pauses given Coroutine</summary>
		public static void UniPauseCoroutine(this EditorWindow editorWindow, Coroutine coroutine)
		{
			CoroutineManager.PauseCoroutine(coroutine);
		}

		/// <summary>Pauses all active Coroutines</summary>
		public static void UniPauseAllCoroutines(this EditorWindow editorWindow)
		{
			CoroutineManager.PauseAllCoroutines();
		}

		/// <summary>Resumes given Coroutine</summary>
		public static void UniResumeCoroutine(this EditorWindow editorWindow, Coroutine coroutine)
		{
			CoroutineManager.ResumeCoroutine(coroutine);
		}

		/// <summary>Resumes all paused Coroutines</summary>
		public static void UniResumeAllCoroutines(this EditorWindow editorWindow)
		{
			CoroutineManager.ResumeAllCoroutines();
		}
	}
}
#endif