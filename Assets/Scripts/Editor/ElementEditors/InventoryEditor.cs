using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;

namespace Zeltex.Items
{
    /// <summary>
    /// Script for inventory editor
    /// </summary>
    [CustomPropertyDrawer(typeof(Inventory))]
    public class InventoryEditor : ZeltexEditor
    {
        private bool IsShowItems;
        private bool IsAddItems;
        private Inventory MyInventory;

        public override void OnCustomGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnCustomGUI(position, property, label);

            MyInventory = GetInstance<Inventory>(property);

            if (MyInventory != null)
            {
                ShowItemsGUI();
                AddItemsGUI();
            }
        }

        private void ShowItemsGUI()
        {
            IsShowItems = GUIToggle(IsShowItems, "Show Items");
            if (IsShowItems)
            {
                for (int i = 0; i < MyInventory.GetSize(); i++)
                {
                    GUILabel(MyInventory.GetItem(i).Name + ":" + MyInventory.GetItem(i).GetQuantity());
                    Rect ButtonRect = new Rect(EditorRect.x + EditorRect.width / 2f,
                        EditorRect.y,
                        EditorRect.height,
                        EditorRect.height);
                    if (GUI.Button(ButtonRect, "-"))
                    {
                        // remove item
                        MyInventory.Remove(i);
                    }
                }
            }
        }

        private void AddItemsGUI()
        {
            IsAddItems = GUIToggle(IsAddItems, "Add Items");

            if (IsAddItems)
            {
                for (int i = 0; i < DataManager.Get().GetSizeElements(DataFolderNames.Items); i++)
                {
                    if (GUIButton("Add [" + DataManager.Get().GetName(DataFolderNames.Items, i) + "]"))
                    {
                        AddItem(i);
                        break;
                    }
                }
            }
        }

        private void AddItem(int ItemIndex)
        {
            // add a item from datamanager
            if (DataManager.Get())
            {
                Element ItemElement = DataManager.Get().GetElement(DataFolderNames.Items, ItemIndex);
                if (ItemElement != null)
                {
                    Item NewItem = ItemElement.Clone() as Item;
                    MyInventory.Add(NewItem);
                    Debug.LogError("Added item in " + MyInventory.Name);
                }
                else
                {
                    Debug.LogError(ItemIndex + " not contained in datamanager.");
                }
            }
            else
            {
                Debug.LogError("No DataManager.");
            }
        }
    }
}
//object targetObject = property.serializedObject.targetObject;
//var targetObjectClassType = targetObject.GetType();
/*var targetObject = fieldInfo.GetValue(property.serializedObject.targetObject);
Inventory MyInventory = (Inventory)targetObject;
if (MyInventory != null)
{
    ItemIndex = int.Parse(EditorGUILayout.TextField(ItemIndex.ToString()));
    EditorGUILayout.Space();
    if (GUILayout.Button("Add"))
    {
    }
    EditorGUILayout.Space();
}
else
{
    EditorGUILayout.LabelField("NULL");
}*/
//DrawDefaultInspector();
//EditorGUILayout.Space();
//EditorGUILayout.PropertyField(property, label, true);

    

            /* var parentFieldInfo = System.Type.GetType(parentProperty.type); // character data
             FieldInfo parentClassFieldInfo = parentFieldInfo.GetField(pathNames[1]);
             if (parentClassFieldInfo != null)
             {
                 Inventory value = parentClassFieldInfo.GetValue(targetObject) as Inventory;
                 Debug.LogError(value.Name);
             }*/

            //property.FindPropertyRelative("");
            //Debug.LogError(property.propertyPath);

            //FieldInfo FieldInfo2 = System.Type.GetType(property.type).GetField("MyValue");
            //Debug.LogError(property.type);
            //object MyObject = fieldInfo.GetValue(property.serializedObject);
            //object MyObject = fieldInfo.GetValue(property.FindPropertyRelative(property.propertyPath).serializedObject);
            //Debug.LogError(MyObject == null);
            //object MyObject = fieldInfo.GetValue(property.serializedObject);
            //Debug.LogError(MyObject.ToString());
            /*foreach (System.Reflection.PropertyInfo info in property)
            {
                Debug.LogError("Property:" + info.Name);
            }*/
            //return property.serializedObject.FindProperty(property.propertyPath). as Inventory;
            //string[] propertyNames = (property.propertyPath).Split('.');
            //List<System.Type> propertyTypes = new List<System.Type>();
            /*for (int i = 0; i < propertyNames.Length; i++)
            {
                property.serializedObject.get
            }*/
            //object SerializedObject = fieldInfo.GetValue(property.serializedObject);
            /*foreach (System.Reflection.FieldInfo info in fieldInfo)
            {

            }*/
            //return SerializedObject.GetProperty(propName).GetValue(src, null);

            //TypedReference Reference = new TypedReference();
            //return fieldInfo.GetValueDirect(Reference) as Inventory;
            //property.serializedObject.
            //return fieldInfo.GetValue(property.propertyPath) as Inventory;
            /*try
            {
                CharacterData Data = fieldInfo.GetValue(property.serializedObject) as CharacterData;
                if (Data != null)
                {
                    return Data.Skillbar;
                }
            }
            catch (ArgumentException e)
            {

            }
            
            {
                return fieldInfo.GetValue(property.serializedObject) as Inventory;
            }*/

            /* int id = property.serializedObject.targetObject.GetInstanceID();
             UnityEngine.Object targetObject = EditorUtility.InstanceIDToObject(id);
             CharacterData MyData = property.serializedObject.FindProperty("Data") as System.Object as CharacterData;
             return MyData.Skillbar;*/

            //return property.objectReferenceValue as System.Object as Inventory;
            /*object SerializedObject = fieldInfo.GetValue(property.serializedObject.targetObject);
            Inventory MyInventory = null;
            MyInventory = SerializedObject as Inventory; // this is character
            if (MyInventory != null)
            {
                return MyInventory;
            }
            CharacterData MyData = SerializedObject as CharacterData;
            if (MyData != null)
            {
                return MyData.Skillbar;
            }
            return null;*/