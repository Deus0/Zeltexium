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
    }

    /// <summary>
    /// Manages routines for Zeltex, interfaces other routine handlers
    /// </summary>
    public class RoutineManager : ManagerBase<RoutineManager>
    {

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
    }
}
