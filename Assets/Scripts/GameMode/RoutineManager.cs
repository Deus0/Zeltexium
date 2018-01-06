using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex
{
    /// <summary>
    /// A handle to a routine of any type
    /// </summary>
    [System.Serializable]
    public class Zeltine
    {
        public UnityEngine.Coroutine UnityRoutine;
        public UniversalCoroutine.Coroutine UniversalRoutine;

        public bool IsUpdating()
        {
            return (UnityRoutine != null || UniversalRoutine != null);
        }

		/*public IEnumerator WaitFor() 
		{
			if (UnityRoutine)
			{
				yield return RoutineManager.Get().StartCoroutine(UnityRoutine);
			}
			else if (UniversalRoutine)
			{
				yield return UniversalCoroutine.CoroutineManager.StartCoroutine(UniversalRoutine);
			}
		}*/
    }
    /// <summary>
    /// Manages routines for Zeltex, interfaces other routine handlers
    /// </summary>
    public class RoutineManager : ManagerBase<RoutineManager>
    {

        public static new RoutineManager Get()
        {
            if (MyManager == null)
            {
                MyManager = GameObject.Find("GameManager").GetComponent<RoutineManager>();
            }
            return MyManager;
        }

        public void StopCoroutine(Zeltine MyHandle)
        {
            if (MyHandle.UnityRoutine != null)
            {
                base.StopCoroutine(MyHandle.UnityRoutine);
            }
            else if (MyHandle.UniversalRoutine != null)
            {
                UniversalCoroutine.CoroutineManager.StopCoroutine(MyHandle.UniversalRoutine);
            }
        }

        /// <summary>
        /// If previous handle is not null it will get stopped first
        /// </summary>
        public Zeltine StartCoroutine(Zeltine MyHandle, IEnumerator MyAction)
        {
            if (MyHandle != null)
            {
                StopCoroutine(MyHandle);
            }
            return StartCoroutine(MyAction);
        }

        /// <summary>
        /// Start a new coroutine!
        /// </summary>
        public new Zeltine StartCoroutine(IEnumerator NewAction)
        {
            Zeltine NewRoutine = new Zeltine();
            if (Application.isPlaying)
            {
                NewRoutine.UnityRoutine = base.StartCoroutine(NewAction);
            }
            else
            {
                NewRoutine.UniversalRoutine = UniversalCoroutine.CoroutineManager.StartCoroutine(NewAction);
            }
            return NewRoutine;
        }

		/*public new Coroutine StartRoutine(IEnumerator NewAction) 
		{
			Coroutine NewRoutine = new Coroutine();
			if (Application.isPlaying)
			{
				NewRoutine = base.StartCoroutine(NewAction);
			}
			else
			{
				NewRoutine = UniversalCoroutine.CoroutineManager.StartCoroutine(NewAction);
			}
			return NewRoutine;
		}*/
    }
}
