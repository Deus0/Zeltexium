using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Skeletons;
using Zeltex.Characters;
using UnityEditor;

namespace Zeltex
{
    [CustomPropertyDrawer(typeof(Skeleton))]
    public class SkeletonEditor : ElementEditor<Skeleton>
    {

        protected override void SetPropertyValue(object NewValue)
        {
            Character MyCharacter = (MyProperty.serializedObject.targetObject as Character);
            if (MyCharacter)
            {
                MyCharacter.GetData().MySkeleton = NewValue as Skeleton;
            }
            else
            {
                base.SetPropertyValue(NewValue);
            }
        }
    }

}