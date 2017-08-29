//-----------------------------------------
//			Universal Coroutine
// Copyright (c) 2016 Jeroen van Pienbroek
//------------------------------------------
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace UniversalCoroutine
{
	/// <summary>Class that manages all coroutines</summary>
	public class CoroutineManager: MonoBehaviour
	{
		/// <summary>The initial size of object pools</summary>
		private const int INITIAL_OBJECT_POOL_SIZE = 100;

		/// <summary>Singleton instance of CoroutineManager</summary>
		private static CoroutineManager instance;

		/// <summary>Singleton instance of CoroutineManager</summary>
		public static CoroutineManager Instance
		{
			get
			{
				if(instance == null)
				{
					CoroutineManager manager = GameObject.FindObjectOfType<CoroutineManager>();
					if(manager != null)
					{
						instance = manager;
					}
					else
					{
						GameObject gameObject = new GameObject("CoroutineManager");
						instance = gameObject.AddComponent<CoroutineManager>();
					}
					instance.Init();
				}
				return instance;
			}
		}

		/// <summary>The object pool for Coroutines</summary>
		private List<Coroutine> coroutinePool;
		/// <summary>The object pool for WaitForSeconds</summary>
		private List<WaitForSeconds> waitForSecondsPool;
		/// <summary>The object pool for WaitForFixedUpdate</summary>
		private List<WaitForUpdate> waitForFixedUpdatePool;
		/// <summary>The object pool for WaitForLateUpdate</summary>
		private List<WaitForUpdate> waitForLateUpdatePool;

		/// <summary>Initializes the CoroutineManager instance</summary>
		private void Init()
		{
			coroutinePool = new List<Coroutine>(INITIAL_OBJECT_POOL_SIZE);
			waitForSecondsPool = new List<WaitForSeconds>(INITIAL_OBJECT_POOL_SIZE);
			waitForFixedUpdatePool = new List<WaitForUpdate>(); //No initial capacity, because these will probably be used less
			waitForLateUpdatePool = new List<WaitForUpdate>(); //No initial capacity, because these will probably be used less
#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				EditorApplication.update += Update;
			}
#endif
		}

		#region UNITY
		private void Update()
		{
            if (coroutinePool == null)
            {
                Init();
            }
			int length = coroutinePool.Count;
			for(int i = 0; i < length; i++)
			{
				Coroutine coroutine = coroutinePool[i];
				if(coroutine.IsActive && !coroutine.IsSubroutine)
				{
					coroutine.MoveNext();
				}
			}
		}

		private void FixedUpdate()
		{
			UpdateWaitForUpdate(ref waitForFixedUpdatePool);
		}

		private void LateUpdate()
		{
			UpdateWaitForUpdate(ref waitForLateUpdatePool);
		}
		#endregion

		/// <summary>Updates the WaitForUpdate instances in given list</summary>
		private void UpdateWaitForUpdate(ref List<WaitForUpdate> waitForUpdatePool)
		{
			if(waitForUpdatePool != null && waitForUpdatePool.Count > 0)
			{
				int length = waitForUpdatePool.Count;
				bool updateReached = false;
				for(int i = 0; i < length; i++)
				{
					WaitForUpdate waitForUpdate = waitForUpdatePool[i];
					if(waitForUpdate.IsActive)
					{
						waitForUpdate.Stop();
						updateReached = true;
					}
				}
				if(updateReached)
				{
					Update(); //Execute normal update so the coroutine(s) will continue now
				}
			}
		}

		#region START_ROUTINE
		/// <summary>Starts and returns a coroutine with given routine from the Object Pool</summary>
		public static new Coroutine StartCoroutine(IEnumerator routine)
		{
			Coroutine coroutine = Instance.coroutinePool.Find((x) => x.IsIdle);
			if(coroutine == null)
			{
				coroutine = new Coroutine();
				Instance.coroutinePool.Add(coroutine); //Expand list when needed
			}

			coroutine.Reset(routine);
			return coroutine;
		}

		/// <summary>Returns a WaitForSeconds instance with given seconds from the Object Pool</summary>
		public static WaitForSeconds WaitForSeconds(float seconds)
		{
			WaitForSeconds waitForSeconds = Instance.waitForSecondsPool.Find((x) => x.IsIdle);
			if(waitForSeconds == null)
			{
				waitForSeconds = new WaitForSeconds();
				Instance.waitForSecondsPool.Add(waitForSeconds); //Expand list when needed
			}

			waitForSeconds.Reset(seconds);
			return waitForSeconds;
		}

		/// <summary>Returns a WaitForUpdate instance (for FixedUpdate) from the Object Pool</summary>
		public static WaitForUpdate WaitForFixedUpdate()
		{
			if(Application.isPlaying)
			{
				return WaitForUpdate(ref Instance.waitForFixedUpdatePool, UpdateType.FIXED_UPDATE);
			}
			else
			{
				Debug.LogWarning("WaitForFixedUpdate() not supported in Edit mode, will block until next Update() call");
				return null;
			}
		}

		/// <summary>Returns a WaitForUpdate instance (for LateUpdate) from the Object Pool</summary>
		public static WaitForUpdate WaitForLateUpdate()
		{
			if(Application.isPlaying)
			{
				return WaitForUpdate(ref Instance.waitForLateUpdatePool, UpdateType.LATE_UPDATE);
			}
			else
			{
				Debug.LogWarning("WaitForLateUpdate() not supported in Edit mode, will block until next Update() call");
				return null;
			}
		}

		/// <summary>Returns a WaitForUpdate instance for given UpdateType from given list</summary>
		private static WaitForUpdate WaitForUpdate(ref List<WaitForUpdate> waitForUpdatePool, UpdateType updateType)
		{
			WaitForUpdate waitForUpdate = waitForUpdatePool.Find((x) => x.IsIdle);
			if(waitForUpdate == null)
			{
				waitForUpdate = new WaitForUpdate(updateType);
				waitForUpdatePool.Add(waitForUpdate); //Expand list when needed
			}

			waitForUpdate.Reset();
			return waitForUpdate;
		}
		#endregion

		#region STOP_ROUTINE
		/// <summary>Stops given Coroutine</summary>
		public static void StopCoroutine(Coroutine coroutine)
		{
			coroutine.Stop();
		}

		/// <summary>Stops all active Coroutines</summary>
		public static new void StopAllCoroutines()
		{
			StopManagedIEnumerators(ref Instance.coroutinePool);
			StopManagedIEnumerators(ref Instance.waitForSecondsPool);
			StopManagedIEnumerators(ref Instance.waitForFixedUpdatePool);
			StopManagedIEnumerators(ref Instance.waitForLateUpdatePool);
		}

		/// <summary>Stops all active Coroutines in given list</summary>
		private static void StopManagedIEnumerators<T>(ref List<T> iEnumerators) where T : ManagedIEnumerator
		{
			int length = iEnumerators.Count;
			for(int i = 0; i < length; i++)
			{
				iEnumerators[i].Stop();
			}
		}
		#endregion

		#region PAUSE_ROUTINE
		/// <summary>Pauses given Coroutine</summary>
		public static void PauseCoroutine(Coroutine coroutine)
		{
			coroutine.Pause();
		}

		/// <summary>Pauses all active Coroutines</summary>
		public static void PauseAllCoroutines()
		{
			PauseManagedIEnumerators(ref Instance.coroutinePool);
			PauseManagedIEnumerators(ref Instance.waitForSecondsPool);
			PauseManagedIEnumerators(ref Instance.waitForFixedUpdatePool);
			PauseManagedIEnumerators(ref Instance.waitForLateUpdatePool);
		}

		/// <summary>Pause all active Coroutines in given list</summary>
		private static void PauseManagedIEnumerators<T>(ref List<T> iEnumerators) where T : ManagedIEnumerator
		{
			int length = iEnumerators.Count;
			for(int i = 0; i < length; i++)
			{
				iEnumerators[i].Pause();
			}
		}
		#endregion

		#region RESUME_ROUTINE
		/// <summary>Resumes given Coroutine</summary>
		public static void ResumeCoroutine(Coroutine coroutine)
		{
			coroutine.Resume();
		}

		/// <summary>Resumes all paused Coroutines</summary>
		public static void ResumeAllCoroutines()
		{
			ResumeManagedIEnumerators(ref Instance.coroutinePool);
			ResumeManagedIEnumerators(ref Instance.waitForSecondsPool);
			ResumeManagedIEnumerators(ref Instance.waitForFixedUpdatePool);
			ResumeManagedIEnumerators(ref Instance.waitForLateUpdatePool);
		}

		/// <summary>Resumes all paused Coroutine in given list</summary>
		private static void ResumeManagedIEnumerators<T>(ref List<T> iEnumerators) where T : ManagedIEnumerator
		{
			int length = iEnumerators.Count;
			for(int i = 0; i < length; i++)
			{
				iEnumerators[i].Resume();
			}
		}
		#endregion
	}
}