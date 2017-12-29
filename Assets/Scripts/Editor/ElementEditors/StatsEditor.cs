using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Zeltex.Combat
{
    /// <summary>
    /// Script for Stats editor
    /// </summary>
    [CustomPropertyDrawer(typeof(Stats))]
    public class StatsEditor : ZeltexEditor
    {
        private bool IsAddItems;
        private bool IsShowItems;
        private Stats MyStats;

        public override void OnCustomGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnCustomGUI(position, property, label);
            MyStats = GetInstance<Stats>(property);
            ShowStats(property);
            AddStats(property);
        }

        private void ShowStats(SerializedProperty property)
        {
            IsShowItems = GUIToggle(IsShowItems, "Show Stats");
            if (IsShowItems)
            {
                if (MyStats != null)
                {
                    for (int i = 0; i < MyStats.GetSize(); i++)
                    {
                        Stat MyStat = MyStats.GetStat(i);
                        if (MyStat != null)
                        {
                            GUILabel(MyStat.Name + ":" + MyStat.GuiString());
                        }
                        else
                        {
                            GUILabel(i + " is null");
                        }
                    }
                }
            }
        }

        private void AddStats(SerializedProperty property)
        {
            IsAddItems = GUIToggle(IsAddItems, "Add Stats");
            if (IsAddItems)
            {
                for (int i = 0; i < DataManager.Get().GetSizeElements(DataFolderNames.Stats); i++)
                {
                    if (GUIButton("Add [" + DataManager.Get().GetName(DataFolderNames.Stats, i) + "]"))
                    {
                        AddStat(property, i);
                    }
                }
            }
        }

        private void AddStat(SerializedProperty property, int ItemIndex)
        {
            // add a item from datamanager
            if (DataManager.Get())
            {
                //var targetObject = fieldInfo.GetValue(property.serializedObject.targetObject);
                //CharacterStats MyStats = GetInstance<CharacterStats>(property);
                if (MyStats != null)
                {
                    Element MyElement = DataManager.Get().GetElement(DataFolderNames.Stats, ItemIndex);
                    if (MyElement != null)
                    {
                        Stat NewItem = MyElement.Clone() as Stat;
                        MyStats.Add(NewItem);
                        property.serializedObject.ApplyModifiedProperties();
                        Debug.Log("Added stat in " + MyStats.Name);
                    }
                    else
                    {
                        Debug.LogError(ItemIndex + " not contained in datamanager.");
                    }
                }
                else
                {
                    Debug.LogError("MyStats is null in StatsEditor");
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