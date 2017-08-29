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
        private DataManager MyDataManager;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Zeltex/DataManager")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            DataManagerEditor window = (DataManagerEditor)EditorWindow.GetWindow(typeof(DataManagerEditor), false, "DataManager");
            window.Show();
        }

        void OnGUI()
        {
            MyDataManager = EditorGUILayout.ObjectField(MyDataManager, typeof(DataManager), true) as DataManager;
            if (MyDataManager)
            {
                MyDataManager.DrawGui();
            }
            else
            {
                if (GUILayout.Button("Search"))
                {
                    MyDataManager = GameObject.FindObjectOfType<DataManager>();
                }
            }
        }

    }

}