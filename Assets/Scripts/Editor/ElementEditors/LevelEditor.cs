using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Skeletons;
using Zeltex.Characters;
using UnityEditor;

namespace Zeltex
{
    [CustomPropertyDrawer(typeof(Level))]
    public class LevelEditor : ElementEditor<Level>
    {

        protected override void SetPropertyValue(object NewValue)
        {
            Debug.Log("Setting property Level as: " + (NewValue as Level).Name);
            SetTargetObjectOfProperty(MyProperty, NewValue as Level);
        }
    }

}