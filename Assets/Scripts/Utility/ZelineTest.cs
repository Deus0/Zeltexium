using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Zeltex;

namespace Zeltex.Tests
{
    /*public class ZelineTest
    {

        [MenuItem("Zeltex/Tests/Test Editor Coroutine")]
        static void TestEditorCoroutine()
        {
            EditorCoroutine.Start(TestRoutine());
        }

        static IEnumerator TestRoutine()
        {
            Debug.Log("hello " + DateTime.Now.Ticks);
            yield return null;
            Debug.Log("done " + DateTime.Now.Ticks);
        }

        [MenuItem("Zeltex/Tests/Test Editor Coroutine With Exception")]
        static void TestEditorCoroutineWithException()
        {
            EditorCoroutine.Start(TestRoutineWithException());
        }

        static IEnumerator TestRoutineWithException()
        {
            Debug.Log("hello " + DateTime.Now.Ticks);
            yield return null;

            for (int i = 0; i < 10; i++)
            {
                TestRandomException();
                yield return null;
            }

            Debug.Log("done " + DateTime.Now.Ticks);
        }

        static void TestRandomException()
        {
            if (Random.value < 0.3f)
            {
                throw new Exception("ahah! " + DateTime.Now.Ticks);
            }
            else
            {
                Debug.Log("ok " + DateTime.Now.Ticks);
            }
        }

        [MenuItem("Zeltex/Tests/TestNestedRoutines")]
        static void TestEditorCoroutine2()
        {
            EditorCoroutine.Start(TestRoutine2());
        }

        static IEnumerator TestRoutine2()
        {
            Debug.Log("hello " + Time.realtimeSinceStartup);
            for (int i = 0; i < 60; i++)
            {
                yield return null;
            }
            
            for (int i = 0; i < 60; i++)
            {
                yield return null;
            }
            Debug.Log("done " + Time.realtimeSinceStartup);
        }
        static IEnumerator TestRoutine3()
        {
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            Debug.Log("hello again " + Time.realtimeSinceStartup);
        }
    }*/
}