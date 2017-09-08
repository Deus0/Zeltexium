using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Skeletons;
using Zeltex.Characters;
using UnityEditor;

namespace Zeltex
{
    [CustomPropertyDrawer(typeof(Zanimation))]
    public class ZanimationEditor : ElementEditor<Zanimation>
    {

        protected override void SetPropertyValue(object NewValue)
        {
            /*Character MyCharacter = (MyProperty.serializedObject.targetObject as Character);
            if (MyCharacter)
            {
                MyCharacter.GetData().MyAnimations = NewValue as List<Zanimation>;
            }*/
        }
    }

}