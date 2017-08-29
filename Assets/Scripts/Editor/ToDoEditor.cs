using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Zeltex
{
    public class ToDoEditor : EditorWindow
    {
        private static string Data;
        // Add menu named "My Window" to the Window menu
        [MenuItem("Zeltex/ToDoEditor")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            ToDoEditor window = (ToDoEditor)EditorWindow.GetWindow(typeof(ToDoEditor), false, "ToDO-List");
            window.Show();
            Data = EditorPrefs.GetString("ToDo", "");
        }

        void OnGUI()
        {
            Rect Size = new Rect(0, 0, position.width, position.height);
            string NewData = EditorGUI.TextArea(Size, Data);
            if (NewData != Data)
            {
                Data = NewData;
                EditorPrefs.SetString("ToDo", Data);
            }
        }
    }
}