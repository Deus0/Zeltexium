using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Zeltex
{
    public static class UnityEventExtentions
    {
        public static void SetEvent(this UnityEvent MyEvent, UnityAction MyAction)
        {
            MyEvent.RemoveAllEvents();
            MyEvent.AddEvent(MyAction);
        }

        public static void RemoveAllEvents(this UnityEvent MyEvent)
        {
#if UNITY_EDITOR
            try
            {
                for (int i = 0; i < 30; i++)
                {
                    UnityEditor.Events.UnityEventTools.RemovePersistentListener(MyEvent, i);
                }
            }
            catch (System.Exception e)
            {
                //Debug.LogWarning(e.ToString());
            }
#else
            MyEvent.RemoveAllListeners();
#endif
        }
        /// <summary>
        /// Add an action to an event, also shows in editor
        /// </summary>
        public static void AddEvent(this UnityEvent MyEvent, UnityAction MyAction)
        {
            RemoveEvent(MyEvent, MyAction);
#if UNITY_EDITOR
            try
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(MyEvent, MyAction);
            }
            catch (System.ArgumentException e)
            {
                MyEvent.AddListener(MyAction);
            }
            catch (System.NullReferenceException e)
            {

            }
#else
            MyEvent.AddListener(MyAction);
#endif
        }

        /// <summary>
        /// Add an action to an event, also shows in editor
        /// </summary>
        public static void RemoveEvent(this UnityEvent MyEvent, UnityAction MyAction)
        {
            if (MyEvent != null && MyAction != null)
            {
#if UNITY_EDITOR
                try
                {
                    UnityEditor.Events.UnityEventTools.RemovePersistentListener(MyEvent, MyAction);
                }
                catch (System.ArgumentException e)
                {
                    MyEvent.RemoveListener(MyAction);
                }
#else
                MyEvent.RemoveListener(MyAction);
#endif
            }
        }

        /// <summary>
        /// Add an action to an event, also shows in editor
        /// </summary>
        public static void AddEvent<T>(this UnityEvent<T> MyEvent, UnityAction<T> MyAction)
        {
#if UNITY_EDITOR
            try
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener<T>(MyEvent, MyAction);
            }
            catch (System.ArgumentException e)
            {
                MyEvent.AddListener(MyAction);
            }
#else
            MyEvent.AddListener(MyAction);
#endif
        }

        /// <summary>
        /// Add an action to an event, also shows in editor
        /// </summary>
        public static void RemoveEvent<T>(this UnityEvent<T> MyEvent, UnityAction<T> MyAction)
        {
            if (MyEvent != null && MyAction != null)
            {
#if UNITY_EDITOR
                UnityEditor.Events.UnityEventTools.RemovePersistentListener<T>(MyEvent, MyAction);
#else
                MyEvent.RemoveListener(MyAction);
#endif
            }
        }

        /// <summary>
        /// Add an action to an event, also shows in editor
        /// </summary>
        public static void AddEvent<T0, T1>(this UnityEvent<T0, T1> MyEvent, UnityAction<T0, T1> MyAction)
        {
#if UNITY_EDITOR
            UnityEditor.Events.UnityEventTools.AddPersistentListener<T0, T1>(MyEvent, MyAction);
#else
            MyEvent.AddListener(MyAction);
#endif

        }
    }

}