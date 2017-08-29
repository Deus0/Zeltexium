using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Quests;
using Zeltex.Characters;
using UnityEditor;

namespace Zeltex
{
    [CustomPropertyDrawer(typeof(QuestLog))]
    public class QuestLogEditor : ElementEditor<QuestLog>
    {

        protected override void SetPropertyValue(object NewValue)
        {
            Character MyCharacter = (MyProperty.serializedObject.targetObject as Character);
            if (MyCharacter)
            {
                MyCharacter.GetData().MyQuestLog = NewValue as QuestLog;
            }
        }
    }

}