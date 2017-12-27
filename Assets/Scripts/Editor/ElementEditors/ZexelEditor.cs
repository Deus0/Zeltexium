﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Skeletons;
using Zeltex.Characters;
using UnityEditor;

namespace Zeltex
{
    [CustomPropertyDrawer(typeof(Zexel))]
    public class ZexelEditor : ElementEditor<Zexel>
    {

        protected override void SetPropertyValue(object NewValue)
        {
            Debug.LogError("Setting property zexel as: " + (NewValue as Zexel).Name);
            SetTargetObjectOfProperty(MyProperty, NewValue as Zexel);
            /*Zexel MyZexel = GetTargetObjectOfProperty(NewValue) as Zexel;
            /&Character MyCharacter = (MyProperty.serializedObject.targetObject as Character);
            if (MyCharacter)
            {
                MyCharacter.GetData().MySkeleton = NewValue as Skeleton;
            }*/
        }

        public override void DrawCustomGUI()
        {
            if (GUIButton("Generate"))
            {
                Data.GenerateTextureFromBytes();
            }
            //Zexel MyZexel = GetTargetObjectOfProperty(MyProperty) as Zexel;
            GUILabel("Has Pixels? " + Data.HasPixels().ToString());
            Texture2D MyTexture = Data.GetTexture();
            if (MyTexture != null)
            {
                GUILabel("Texture [" + MyTexture.name + "] - [" + MyTexture.width + ":" + MyTexture.height + "].");
                GUILabel(MyTexture);
            }
            else
            {
                GUILabel("No texture data inside [" + Data.Name + "].");
            }
        }
    }

}