using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Zeltex
{

    [CustomPropertyDrawer(typeof(EditorAction))]
    public class EditorActionEditor : ZeltexEditor
    {
        EditorAction MyAction;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            MyAction = GetTargetObjectOfProperty(property) as EditorAction;
            Color BeforeColor = GUI.color;
            GUI.color = new Color(0, 255, 255);
            if (GUI.Button(position, property.name))
            {
                MyAction.Trigger();
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
            GUI.color = BeforeColor;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 30;
        }
    }
}