using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Zeltex
{

    public class ElementActionEditor<T0, T1> : ZeltexEditor where T0 : Element where T1 : class
    {
        private bool IsInitial = true;
        protected T1 MyAction;
        protected int SelectedIndex = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            MyAction = GetTargetObjectOfProperty(property) as T1;
            if (MyAction != null)
            {
                List<string> MyNames = DataManager.Get().GetNames(DataFolderNames.DataTypeToFolderName(typeof(T0)));
                if (MyNames.Count == 0)
                {
                    MyNames.Add("None");
                }
                string[] NamesArray = MyNames.ToArray();
                if (IsInitial)
                {
                    IsInitial = false;
                    SetName(MyNames[SelectedIndex]);
                }
                int NewSelected = EditorGUI.Popup(
                    new Rect(position.x, position.y, position.width, 16), 
                    "Chose a " + typeof(T0).ToString(), 
                    SelectedIndex, NamesArray, UnityEditor.EditorStyles.popup);
                if (NewSelected != SelectedIndex && NewSelected >= 0 && NewSelected < MyNames.Count)
                {
                    SelectedIndex = NewSelected;
                    SetName(MyNames[SelectedIndex]);
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 16;
        }

        protected virtual void SetName(string NewName)
        {

        }

        protected virtual void OnPostGUI(Rect position, SerializedProperty property)
        {

        }
    }
}