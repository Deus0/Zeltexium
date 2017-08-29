//-----------------------------------------
//			Universal Coroutine
// Copyright (c) 2016 Jeroen van Pienbroek
//------------------------------------------
using System.Collections;
using UnityEngine;

namespace UniversalCoroutine
{
	/// <summary>Class with extension "wrapper" methods for CoroutineManager methods for MonoBehaviour class</summary>
	public static class MonoBehaviourExtension
	{
		/// <summary>Starts and returns a coroutine with given routine from the Object Pool</summary>
		public static Coroutine UniStartCoroutine(this MonoBehaviour monoBehaviour, IEnumerator routine)
		{
			return CoroutineManager.StartCoroutine(routine);
		}

		/// <summary>Returns a WaitForSeconds instance with given seconds from the Object Pool</summary>
		public static WaitForSeconds UniWaitForSeconds(this MonoBehaviour monoBehaviour, float seconds)
		{
			return CoroutineManager.WaitForSeconds(seconds);
		}

		/// <summary>Returns a WaitForUpdate instance (for FixedUpdate) from the Object Pool</summary>
		public static WaitForUpdate UniWaitForFixedUpdate(this MonoBehaviour monoBehaviour)
		{
			return CoroutineManager.WaitForFixedUpdate();
		}

		/// <summary>Returns a WaitForUpdate instance (for LateUpdate) from the Object Pool</summary>
		public static WaitForUpdate UniWaitForLateUpdate(this MonoBehaviour monoBehaviour)
		{
			return CoroutineManager.WaitForLateUpdate();
		}

		/// <summary>Stops given Coroutine</summary>
		public static void UniStopCoroutine(this MonoBehaviour monoBehaviour, Coroutine coroutine)
		{
			CoroutineManager.StopCoroutine(coroutine);
		}

		/// <summary>Stops all active Coroutines</summary>
		public static void UniStopAllCoroutines(this MonoBehaviour monoBehaviour)
		{
			CoroutineManager.StopAllCoroutines();
		}

		/// <summary>Pauses given Coroutine</summary>
		public static void UniPauseCoroutine(this MonoBehaviour monoBehaviour, Coroutine coroutine)
		{
			CoroutineManager.PauseCoroutine(coroutine);
		}

		/// <summary>Pauses all active Coroutines</summary>
		public static void UniPauseAllCoroutines(this MonoBehaviour monoBehaviour)
		{
			CoroutineManager.PauseAllCoroutines();
		}

		/// <summary>Resumes given Coroutine</summary>
		public static void UniResumeCoroutine(this MonoBehaviour monoBehaviour, Coroutine coroutine)
		{
			CoroutineManager.ResumeCoroutine(coroutine);
		}

		/// <summary>Resumes all paused Coroutines</summary>
		public static void UniResumeAllCoroutines(this MonoBehaviour monoBehaviour)
		{
			CoroutineManager.ResumeAllCoroutines();
		}
	}
}