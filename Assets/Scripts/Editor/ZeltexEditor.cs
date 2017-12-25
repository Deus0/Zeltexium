using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;

namespace Zeltex
{
    public class ZeltexEditor : PropertyDrawer
    {
        private bool IsDefaultGui;
        protected Rect EditorRect = new Rect(0, 0, 0, 0);
        protected float ExtraHeight;
        protected SerializedProperty MyProperty;

        protected virtual bool IsUnityGui()
        {
            return IsDefaultGui;
        }

        protected virtual void SetIsUnityGui(bool NewDefaultGui)
        {
            IsDefaultGui = NewDefaultGui;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            MyProperty = property;
            if (IsUnityGui())
            {
                EditorGUI.PropertyField(position, property, label, true);
                ExtraHeight = EditorGUI.GetPropertyHeight(property);
                EditorRect = new Rect(position.xMin, position.yMax - 20f, position.width, 20f);//  - 20f
                ExtraHeight += EditorRect.height;
                SetIsUnityGui(GUI.Toggle(EditorRect, IsUnityGui(), "Default [true]"));
            }
            else
            {
                EditorRect = new Rect(position.xMin, position.yMin, position.width, 20f);
                ExtraHeight = EditorRect.height;
                string Char = "x";
                if (property.isExpanded)
                {
                    Char = "o";
                }
                property.isExpanded = EditorGUI.Foldout(EditorRect, property.isExpanded, property.name + "[" + Char + "]");
                if (property.isExpanded)
                {
                    OnCustomGUI(position, property, label);
                    SetIsUnityGui(GUIToggle(IsUnityGui(), "Default [false]"));
                }
            }
        }

        public virtual void OnCustomGUI(Rect position, SerializedProperty property, GUIContent label)
        {
        }

        protected bool GUIButton(string Label)
        {
            EditorRect.y += EditorRect.height;
            ExtraHeight += EditorRect.height;
            return GUI.Button(EditorRect, Label);
        }

        protected bool GUIToggle(bool ToggleState, string ToggleLabel)
        {
            EditorRect.y += EditorRect.height;
            ExtraHeight += EditorRect.height;
            return GUI.Toggle(EditorRect, ToggleState, ToggleLabel);
        }

        protected void GUILabel(string ToggleLabel)
        {
            EditorRect.y += EditorRect.height;
            ExtraHeight += EditorRect.height;
            GUI.Label(EditorRect, ToggleLabel);
        }

        private static float TextureMultiple = 4;
        protected void GUILabel(Texture2D ToggleLabel)
        {
            if (ToggleLabel != null)
            {
                float PreviousHeight = EditorRect.height;
                float PreviousWidth = EditorRect.width;
                EditorRect.height = ToggleLabel.height * TextureMultiple;
                EditorRect.width = ToggleLabel.width * TextureMultiple;
                EditorRect.y += 20;
                ExtraHeight += EditorRect.height;
                //GUI.Label(EditorRect, ToggleLabel);
                GUI.DrawTexture(EditorRect, ToggleLabel);
                EditorRect.y += EditorRect.height - 20;
                EditorRect.height = PreviousHeight;
                EditorRect.width = PreviousWidth;
            }
        }

        public string GUITextField(string InputText)
        {
            EditorRect.y += EditorRect.height;
            ExtraHeight += EditorRect.height;
            return GUI.TextField(EditorRect, InputText);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ExtraHeight;
        }

        /// <summary>
        /// Uses reflection to get the property instance
        /// </summary>
        protected T GetInstance<T>(SerializedProperty property) where T : Element
        {
            //Debug.LogError(property.serializedObject.targetObject.GetType());
            Type ComponentType = property.serializedObject.targetObject.GetType();
            //Debug.LogError("Component Type: " + ComponentType.ToString());
            BindingFlags BindFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            string propertyPath = property.propertyPath;
            string[] pathNames = propertyPath.Split('.');
            if (pathNames.Length == 1)
            {
                return fieldInfo.GetValue(property.serializedObject.targetObject) as T;
            }
            else if (pathNames.Length == 2)
            {
                //Debug.LogError(property.serializedObject.targetObject.GetType());
                SerializedProperty parentProperty = property.serializedObject.FindProperty(pathNames[0]);
                FieldInfo ParentPropertyInfo = ComponentType.GetField(pathNames[0], BindFlags);
                if (ParentPropertyInfo != null)
                {
                    object ParentObject = ParentPropertyInfo.GetValue(parentProperty.serializedObject.targetObject);
                    Type ParentType = ParentObject.GetType();
                    //Debug.LogError("ParentType is " + ParentType.ToString() + ":" + pathNames[1]);
                    FieldInfo ChildPropertyInfo = ParentType.GetField(pathNames[1], BindFlags);
                    if (ChildPropertyInfo != null)
                    {
                        T MyObject = ChildPropertyInfo.GetValue(ParentObject) as T;
                        return MyObject;
                    }
                    else
                    {
                        Debug.LogError("ChildPropertyInfo is null");
                    }
                }
                else
                {
                    Debug.LogError("Property not found on type.");
                }
            }
            return null;
        }

        public static object SetTargetObjectOfProperty(SerializedProperty prop, object MyObject)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            // for each children class
            // Start at parent object and movedown to last one
            //foreach (var element in elements)
            for (int i = 0; i < elements.Length; i++)
            {
                // For arrays
                if (elements[i].Contains("["))
                {
                    var elementName = elements[i].Substring(0, elements[i].IndexOf("["));
                    var index = System.Convert.ToInt32(elements[i].Substring(elements[i].IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = SetValue_Imp(obj, elementName, index, MyObject, i == elements.Length - 1);
                }
                else
                {
                    obj = SetValue_Imp(obj, elements[i], MyObject, i == elements.Length - 1);
                }
            }
            return obj;
        }
        private static object SetValue_Imp(object source, string name, object MyObject, bool IsLastIndex)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                {
                    if (IsLastIndex)
                    {
                        f.SetValue(source, MyObject);
                    }
                    return f.GetValue(source);
                }

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                {
                    if (IsLastIndex)
                    {
                        p.SetValue(source, MyObject, null);
                    }
                    return p.GetValue(source, null);
                }

                type = type.BaseType;
            }
            return null;
        }
        private static object SetValue_Imp(object source, string name, int index, object MyObject, bool IsLastIndex)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null)
                return null;
            var enm = enumerable.GetEnumerator();

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext())
                    return null;
            }
            return enm.Current;
        }

        public static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            if (prop == null || prop.propertyPath == null)
            {
                Debug.LogError("Property is null.");
                return null;
            }
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }
        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }
        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }
    }

}