using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;

namespace Zeltex
{
    /// <summary>
    /// An editor for character data
    /// </summary>
    [CustomPropertyDrawer(typeof(Int3))]
    public class Int3Editor : ZeltexEditor
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect PartA = new Rect(position.x, position.y, position.width / 3f, position.height);
            Rect PartB = new Rect(position.x + position.width / 3f, position.y, position.width / 3f, position.height);
            Rect PartC = new Rect(position.x + position.width * 2f / 3f, position.y, position.width / 3f, position.height);
            object targetObject = GetTargetObjectOfProperty(property);
            if (targetObject != null)
            {
                Int3 MyInt3 = targetObject as Int3;
                MyInt3.x = int.Parse(GUI.TextField(PartA, MyInt3.x.ToString()));
                MyInt3.y = int.Parse(GUI.TextField(PartB, MyInt3.y.ToString()));
                MyInt3.z = int.Parse(GUI.TextField(PartC, MyInt3.z.ToString()));
            }
            else
            {
                Debug.LogError("Int3 is null");
            }
            // property.
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 20;
        }

        /// <summary>
        /// Uses reflection to get the property instance
        /// </summary>
        /*protected object GetInstance2<T>(SerializedProperty property)
        {
            //Debug.LogError(property.serializedObject.targetObject.GetType());
            Type ComponentType = property.serializedObject.targetObject.GetType();
            //Debug.LogError("Component Type: " + ComponentType.ToString());
            //BindingFlags BindFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            string propertyPath = property.propertyPath;
            Debug.LogError(propertyPath);
            string[] pathNames = propertyPath.Split('.');
            if (pathNames.Length == 1)
            {
                return fieldInfo.GetValue(property.serializedObject.targetObject);
            }
            else if (pathNames.Length  > 0)
            {
                List<string> PathNamesList = new List<string>();
                PathNamesList.AddRange(pathNames);
                return GetInstanceChild(PathNamesList, property.serializedObject.targetObject);
            }
            else
            {
                Debug.LogError("Path Names has " + pathNames.Length);
            }
            return null;
        }

        protected object GetInstanceChild(List<string> PathNames, object ParentObject)
        {
            BindingFlags BindFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            if (PathNames.Count > 0)
            {
                string ProperyName = PathNames[0];
                PathNames.RemoveAt(0);
                //SerializedProperty ChildProperty = ParentProperty.FindProperty(ProperyName);
                System.Type ParentType = ParentObject.GetType();
                FieldInfo ChildFieldInfo = ParentType.GetField(ProperyName, BindFlags);
                Debug.LogError("Size is " + PathNames.Count + " property name is " + ProperyName + " --- ChildFieldInfo null? " + (ChildFieldInfo != null)
                   + " --- ParentType: " + ParentType.ToString());//  + " ---from ChildProperty: " + (ChildProperty != null));
                if (ChildFieldInfo != null)
                {
                    //Debug.LogError("ChildInfo: " + ChildFieldInfo.Name + ": ChildProperty: " + ChildProperty.name);
                    object ChildObject = ChildFieldInfo.GetValue(ParentObject);
                    return GetInstanceChild(PathNames, ChildObject); // ParentProperty, 
                }
                else
                {
                    return null;    // cannot dig deeper
                }
            }
            else
            {
                Debug.LogError("Could not find MyInt3");
                // return
                return ParentObject;
            }
        }*/
    }
}