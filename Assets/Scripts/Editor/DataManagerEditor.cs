using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Zeltex
{
    /// <summary>
    /// A quick editor for the data
    /// </summary>
    public class DataManagerEditor : EditorWindow
    {
        public static DataManagerEditor Instance;
        private static DataGUI MyGui;
        private bool HasSearched = false;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Zeltex/Zatazel")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            Instance = (DataManagerEditor)EditorWindow.GetWindow(typeof(DataManagerEditor), false, "Zeltex");
            Instance.Show();
            MyGui = DataGUI.Get();
        }

        void OnGUI()
        {
            Instance = this;
            //MyDataManager = EditorGUILayout.ObjectField(MyDataManager, typeof(DataManager), true) as DataManager;
            if (MyGui)
            {
                MyGui.DrawGui(this);
                HasSearched = false;
            }
            else
            {
                if (!HasSearched)
                {
                    HasSearched = true;
                    MyGui = GameObject.FindObjectOfType<DataGUI>();
                }
                if (GUILayout.Button("Search"))
                {
                    MyGui = GameObject.FindObjectOfType<DataGUI>();
                }
            }
        }

        void CheckInput()
        {
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.isKey && Event.current.keyCode == KeyCode.Alpha2 )
                {
                    Debug.Log("Number 2 was pressed, default unity hotkey is overridden.");
                    Event.current.Use();    // if you don't use the event, the default action will still take place.

                }
            }
        }

        public void Awake()
        {
            Debug.Log("Awaken Zatazel");
            Instance = this;
        }

        /*public void OnKeyPress(KeyCode MyKeys)
        {
            if (MyKeys != KeyCode.None)
            {
                if (KeyCode.R == MyKeys)
                {
                    RefreshCode();
                    Event.current.Use();
                }
            }
        }

        public void RefreshCode()
        {
           // DataGUI.PrepareDataForReload();
            //AssetDatabase.Refresh();
        }

        [InitializeOnLoadMethod]
        static void EditorInit()
        {
            System.Reflection.FieldInfo info = typeof(EditorApplication).GetField(
                "globalEventHandler", 
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            EditorApplication.CallbackFunction value = (EditorApplication.CallbackFunction)info.GetValue(null);
            value += EditorGlobalKeyPress;
            info.SetValue(null, value);
        }
        static KeyCode CurrentPressed = KeyCode.None;
        static void EditorGlobalKeyPress()
        {
            if ((Event.current.keyCode != KeyCode.LeftControl) && Event.current.keyCode != KeyCode.None && CurrentPressed != Event.current.keyCode)
            {
                CurrentPressed = Event.current.keyCode;
                //Debug.Log("KEY CHANGE " + Event.current.keyCode);
                if (Event.current != null)
                {
                    Instance.OnKeyPress(Event.current.keyCode);
                }
            }
            if (Event.current.keyCode == KeyCode.R || (Event.current.keyCode == KeyCode.LeftControl && CurrentPressed == KeyCode.R))
            {
                Event.current.Use();    // cancel editor one
            }   
        }*/
    }

}