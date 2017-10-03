using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Dialogue;
using Zeltex.Characters;
using UnityEditor;

namespace Zeltex
{
    [CustomPropertyDrawer(typeof(DialogueTree))]
    public class DialogueTreeEditor : ElementEditor<DialogueTree>
    {
        public override void DrawCustomGUI()
        {
            base.DrawCustomGUI();
            if (GUIButton("Generate"))
            {
                DialogueGenerator.Get().SetTargetData(Data);
                UniversalCoroutine.CoroutineManager.StartCoroutine(DialogueGenerator.Get().Generate());
            }
        }

        protected override void SetPropertyValue(object NewValue)
        {
            Character MyCharacter = (MyProperty.serializedObject.targetObject as Character);
            if (MyCharacter)
            {
                MyCharacter.GetData().MyDialogue = NewValue as DialogueTree;
            }
        }
    }

}