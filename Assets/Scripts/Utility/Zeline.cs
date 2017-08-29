using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Zeltex
{
    /*public class EditorCoroutine
    {
        public IEnumerator MyRoutine;

        EditorCoroutine(IEnumerator _routine)
        {
            MyRoutine = _routine;
        }

        void Start()
        {
            //Debug.Log("start");
            EditorApplication.update += Update;
        }
        public void Stop()
        {
            //Debug.Log("stop");
            EditorApplication.update -= Update;
        }

        void  Update()
        {

            //Debug.Log("update");
            if (!MyRoutine.MoveNext())
            {
                Stop();
            }
        }

        public static EditorCoroutine Start(IEnumerator _routine)
        {
            EditorCoroutine coroutine = new EditorCoroutine(_routine);
            coroutine.Start();
            return coroutine;
        }
    }*/
}