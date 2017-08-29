using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Zeltex.Characters;

namespace Zeltex
{
    /// <summary>
    /// An editor for character data
    /// </summary>
    [CustomPropertyDrawer(typeof(CharacterData))]
    public class CharacterDataEditor : ElementEditor<CharacterData>
    {
        public override void DrawCustomGUI()
        {
            GUILabel("  Skillbar Items: " + Data.Skillbar.GetSize());
            GUILabel("  Stats: " + Data.MyStats.GetSize());
            GUILabel("  Quests: " + Data.MyQuestLog.GetSize());
            GUILabel("  Dialogue: " + Data.MyDialogue.GetSize());
        }

        protected override void SetPropertyValue(object NewValue)
        {
            Character MyCharacter = (MyProperty.serializedObject.targetObject as Character);
            if (MyCharacter)
            {
                MyCharacter.SetData(NewValue as CharacterData);
            }
        }
        /*private CharacterData Data;
        private bool IsPulling;

        public override void OnCustomGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnCustomGUI(position, property, label);
            Data = GetInstance<CharacterData>(property);
            GUILabel("Character Data: " + Data.Name);
            GUILabel("  Skillbar Items: " + Data.Skillbar.GetSize());
            GUILabel("  Stats: " + Data.MyStats.GetSize());
            GUILabel("  Quests: " + Data.MyQuestLog.GetSize());
            GUILabel("  Dialogue: " + Data.MyDialogue.GetSize());
            
            if (GUIButton("PushAsNew"))
            {
                CharacterData NewCharacterData = Data.Clone<CharacterData>();
                NewCharacterData.OnModified();
                DataManager.Get().AddElement(DataFolderNames.Characters, NewCharacterData);
            }
            if (GUIButton("Push Changes"))
            {
                CharacterData NewCharacterData = Data.Clone<CharacterData>();
                NewCharacterData.OnModified();
                DataManager.Get().SetElement(DataFolderNames.Characters, NewCharacterData);
            }
            if (GUIButton("Revert"))
            {
                Debug.Log("Reverting " + property.serializedObject.targetObject.name + "'s Data to " + Data.Name + " in DataManager.");
                PullFromDataManager(property, DataManager.Get().GetFileIndex(DataFolderNames.Characters, Data.Name));
            }
            PullFromDataManager(property);
        }

        private void PullFromDataManager(SerializedProperty property)
        {
            IsPulling = GUIToggle(IsPulling, "Pull Character");
            if (IsPulling)
            {
                if (DataManager.Get())
                {
                    for (int i = 0; i < DataManager.Get().GetSizeElements(DataFolderNames.Characters); i++)
                    {
                        if (GUIButton("Set As [" + DataManager.Get().GetName(DataFolderNames.Characters, i) + "]"))
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
            CharacterData MyElement = DataManager.Get().GetElement(DataFolderNames.Characters, Index) as CharacterData;
            Character MyCharacter = (property.serializedObject.targetObject as Character);
            if (MyCharacter && MyElement != null)
            {
                MyCharacter.SetData(MyElement.Clone() as CharacterData);
            }
        }*/

    }
}
