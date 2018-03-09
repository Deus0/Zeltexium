using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Zeltex.Voxels;

namespace Zeltex
{

    [CustomPropertyDrawer(typeof(PolyModelAction))]
    public class PolyModelActionEditor : ElementActionEditor<PolyModel, PolyModelAction>
    {
        int SelectedTextureMap = 0;
        public string[] TextureMapIndexes = new string[] { };
        /*private bool IsInitial = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            MyAction = GetTargetObjectOfProperty(property) as PolyModelAction;
            if (MyAction != null)
            {
                List<string> MyNames = DataManager.Get().GetNames(DataFolderNames.PolyModels);
                if (MyNames.Count == 0)
                {
                    MyNames.Add("None");
                }
                string[] NamesArray = MyNames.ToArray();
                if (IsInitial)
                {
                    IsInitial = false;
                    RefreshTextureMaps();
                    MyAction.PolyName = MyNames[SelectedVoxelModel];
                }
                int NewSelectedVoxelModel = EditorGUI.Popup(new Rect(position.x, position.y, position.width, 16), "Chose a Poly", SelectedVoxelModel, NamesArray, UnityEditor.EditorStyles.popup);
                if (NewSelectedVoxelModel != SelectedVoxelModel && NewSelectedVoxelModel >= 0 && NewSelectedVoxelModel < MyNames.Count)
                {
                    SelectedVoxelModel = NewSelectedVoxelModel;
                    MyAction.PolyName = MyNames[SelectedVoxelModel];
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                    RefreshTextureMaps();
                }
                int NewTextureMapIndex = EditorGUI.Popup(new Rect(position.x, position.y + 16, position.width, 16), "Chose a PolyTexture", SelectedTextureMap, TextureMapIndexes, UnityEditor.EditorStyles.popup);
                if (NewTextureMapIndex != SelectedTextureMap && NewTextureMapIndex >= 0 && NewTextureMapIndex < TextureMapIndexes.Length)
                {
                    SelectedTextureMap = NewTextureMapIndex;
                    MyAction.TextureMapIndex = SelectedTextureMap;
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
            }
        }*/


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 32;
        }

        protected override void SetName(string NewName)
        {
            MyAction.PolyName = NewName;
            RefreshTextureMaps();
        }

        private void RefreshTextureMaps()
        {
            List<string> TextureMapNames = new List<string>();
            Voxels.PolyModel MyPoly = DataManager.Get().GetElement(DataFolderNames.PolyModels, SelectedIndex) as Voxels.PolyModel;
            if (MyPoly != null)
            {
                for (int i = 0; i < MyPoly.TextureMaps.Count; i++)
                {
                    TextureMapNames.Add(i.ToString());
                }
                TextureMapIndexes = TextureMapNames.ToArray();
            }
            else
            {
                TextureMapIndexes = new string[] { };
            }
        }

        protected override void OnPostGUI(Rect position, SerializedProperty property)
        {
            int NewTextureMapIndex = EditorGUI.Popup(new Rect(position.x, position.y + 16, position.width, 16), "Chose a TextureMap", SelectedTextureMap, TextureMapIndexes, UnityEditor.EditorStyles.popup);
            if (NewTextureMapIndex != SelectedTextureMap && NewTextureMapIndex >= 0 && NewTextureMapIndex < TextureMapIndexes.Length)
            {
                SelectedTextureMap = NewTextureMapIndex;
                MyAction.TextureMapIndex = SelectedTextureMap;
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
        }
    }
}