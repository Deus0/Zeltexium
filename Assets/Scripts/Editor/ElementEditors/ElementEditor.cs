using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Zeltex
{
    public class ElementEditor<T> : ZeltexEditor where T : Element
    {
        protected T Data;
        //protected SerializedProperty MyProperty;
        private bool IsPulling;

        protected override bool IsUnityGui()
        {
            return Data.IsDefaultGui;
        }

        protected override void SetIsUnityGui(bool NewDefaultGui)
        {
            Data.IsDefaultGui = NewDefaultGui;
        }

        protected virtual string GetDataFolderName()
        {
            return DataFolderNames.DataTypeToFolderName(typeof(T));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Data = GetTargetObjectOfProperty(property) as T;
            base.OnGUI(position, property, label);
        }

        public override void OnCustomGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnCustomGUI(position, property, label);
            GUILabel(typeof(T).ToString() + ": " + Data.Name);
            Data.Name = GUITextField(Data.Name);

            if (GUIButton("PushAsNew"))
            {
                T NewData = Data.Clone<T>();
                if (NewData != null)
                {
                    DataManager.Get().AddElement(GetDataFolderName(), NewData);
                    Debug.Log("Pushing " + NewData.Name + " To DataManager Folder " + GetDataFolderName());
                    NewData.OnModified();
                }
                else
                {

                }
            }
            if (GUIButton("Save [" + Data.Name + "] To DataManager"))
            {
                T NewData = Data.Clone<T>();
                DataManager.Get().SetElement(GetDataFolderName(), NewData);
                NewData.OnModified();
            }
            if (GUIButton("Revert [" + Data.Name + "]"))
            {
                Debug.Log("Reverting " + property.serializedObject.targetObject.name + "'s Data to " + Data.Name + " in DataManager.");
                PullFromDataManager(property, DataManager.Get().GetFileIndex(GetDataFolderName(), Data.Name));
            }
            PullFromDataManager(property);

            DrawCustomGUI();
            //DataGUI.Get().DrawFieldsForObject(property.objectReferenceValue as object, null, null, true);
        }

        public virtual void DrawCustomGUI()
        {

        }

        private void PullFromDataManager(SerializedProperty property)
        {
            IsPulling = GUIToggle(IsPulling, "Pull " + typeof(T).Name);
            if (IsPulling)
            {
                if (DataManager.Get())
                {
                    for (int i = 0; i < DataManager.Get().GetSizeElements(GetDataFolderName()); i++)
                    {
                        if (GUIButton("Set As [" + DataManager.Get().GetName(GetDataFolderName(), i) + "]"))
                        {
                            PullFromDataManager(property, i);
                            break;
                        }
                    }
                }
            }
        }

        private void PullFromDataManager(SerializedProperty property, int Index)
        {
            MyProperty = property;
            T NewData = DataManager.Get().GetElement(GetDataFolderName(), Index) as T;
            if (NewData != null)
            {
                NewData = NewData.Clone() as T;
                SetPropertyValue(NewData);
            }
        }

        protected virtual void SetPropertyValue(object NewValue)
        {
            SetValue(MyProperty, NewValue);
        }

        public void SetValue(SerializedProperty property, object value)
        {
            System.Type parentType = property.serializedObject.targetObject.GetType();
            System.Reflection.FieldInfo fi = parentType.GetField(property.propertyPath);//this FieldInfo contains the type.
            fi.SetValue(property.serializedObject.targetObject, value);
        }
    }
}
