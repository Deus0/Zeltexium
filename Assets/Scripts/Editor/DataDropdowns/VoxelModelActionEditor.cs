using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Zeltex
{

    [CustomPropertyDrawer(typeof(VoxelModelAction))]
    public class VoxelModelActionEditor : ZeltexEditor
    {
        VoxelModelAction MyAction;
        int SelectedVoxelModel = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            MyAction = GetTargetObjectOfProperty(property) as VoxelModelAction;
            if (MyAction != null)
            {
                List<string> MyNames = DataManager.Get().GetNames(DataFolderNames.VoxelModels);
                if (MyNames.Count == 0)
                {
                    MyNames.Add("None");
                }
                string[] NamesArray = MyNames.ToArray();
                int NewSelectedVoxelModel = EditorGUI.Popup(position, "Chose a Model", SelectedVoxelModel, NamesArray, UnityEditor.EditorStyles.popup);
                if (NewSelectedVoxelModel != SelectedVoxelModel && NewSelectedVoxelModel >= 0 && NewSelectedVoxelModel < MyNames.Count)
                {
                    SelectedVoxelModel = NewSelectedVoxelModel;
                    MyAction.VoxelModelName = MyNames[SelectedVoxelModel];
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 30;
        }
    }
}