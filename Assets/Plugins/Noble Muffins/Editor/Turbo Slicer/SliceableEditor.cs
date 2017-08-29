using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using NobleMuffins.TurboSlicer;
using NobleMuffins.TurboSlicer.Guts;

[CustomEditor(typeof(Sliceable))]
[CanEditMultipleObjects]
public class SliceableEditor : Editor
{
	SerializedProperty refreshCollidersProperty;
	SerializedProperty alternatePrefabProperty;
	SerializedProperty alwaysCloneFromAlternateProperty;
	SerializedProperty channelNormalsProperty;
	SerializedProperty channelTangentsProperty;
	SerializedProperty channelUV2Property;
	SerializedProperty shreddableProperty;
	
	public void OnEnable()
	{
		refreshCollidersProperty = serializedObject.FindProperty("refreshColliders");
		alternatePrefabProperty = serializedObject.FindProperty("alternatePrefab");
		alwaysCloneFromAlternateProperty = serializedObject.FindProperty("alwaysCloneFromAlternate");
		channelNormalsProperty = serializedObject.FindProperty("channelNormals");
		channelTangentsProperty = serializedObject.FindProperty("channelTangents");
		channelUV2Property = serializedObject.FindProperty("channelUV2");
		shreddableProperty = serializedObject.FindProperty("shreddable");
	}
	
	public override void OnInspectorGUI ()
	{
		bool someTargetsHaveMultipleRenderers = false;

		var allRenderers = new List<Renderer>();
		
		foreach(Object o in targets)
		{
			Sliceable s = (Sliceable) o;

			Component[] _allRenderersOnThisTarget = s.GetComponentsInChildren(typeof(Renderer), true);

			Renderer[] allRenderersOnThisTarget = new Renderer[_allRenderersOnThisTarget.Length];

			for(int i = 0; i < _allRenderersOnThisTarget.Length; i++)
				allRenderersOnThisTarget[i] = _allRenderersOnThisTarget[i] as Renderer;
			
			allRenderers.AddRange( allRenderersOnThisTarget );

			someTargetsHaveMultipleRenderers |= allRenderersOnThisTarget.Length > 1;
		}
		
		EditorGUILayout.PropertyField(refreshCollidersProperty, new GUIContent("Refresh colliders"));
		EditorGUILayout.PropertyField(alternatePrefabProperty, new GUIContent("Alternate prefab"));
		EditorGUILayout.PropertyField(shreddableProperty, new GUIContent("Shreddable"));

		bool atLeastSomeHaveAlternatePrefab = alternatePrefabProperty.hasMultipleDifferentValues || alternatePrefabProperty.objectReferenceValue != null;

		if(atLeastSomeHaveAlternatePrefab)
			EditorGUILayout.PropertyField(alwaysCloneFromAlternateProperty, new GUIContent("Always clone from alternate"));

		EditorGUILayout.PropertyField(channelNormalsProperty, new GUIContent("Process Normals"));
		EditorGUILayout.PropertyField(channelTangentsProperty, new GUIContent("Process Tangents"));
		EditorGUILayout.PropertyField(channelUV2Property, new GUIContent("Process UV2"));
		
		EditorGUILayout.Separator();
		
		//Ensure that all the targets are vetted and if they're not, we can only vet them one at a time
		//through the unity inspector.
		
		if(allRenderers.Count == 0)
		{
			EditorGUILayout.LabelField("No mesh renderers found!");
		}
		
		serializedObject.ApplyModifiedProperties();
		
		//Assuming we're all legit, let's multi-edit the infillers.

		var mats = new HashSet<Material>();
		
		foreach(var r in allRenderers)
		{
			Material[] _mats = r.sharedMaterials;
			foreach(Material mat in _mats)
			{
				mats.Add(mat);
			}
		}
		
		if(mats.Count > 0)
		{
			EditorGUILayout.LabelField("For each material, define what region is used for infill.");
		}

		var preexistingInfillers = new List<InfillConfiguration>();
		
		foreach(Object o in targets)
		{
			Sliceable s = o as Sliceable;

			Renderer renderer;

			renderer = s.gameObject.GetComponent<Renderer>();

			if(renderer != null) {
				Material[] _mats = renderer.sharedMaterials;
				
				foreach(Material mat in _mats)
				{
					if(mats.Contains(mat) == false)
					{
						mats.Add(mat);
					}
				}
			}

			preexistingInfillers.AddRange(s.infillers);
		}

		var infillersBuilder = new List<InfillConfiguration>();

		var forceDirty = false;

		foreach(var mat in mats)
		{
			InfillConfiguration infiller = null;
			
			foreach(var _infiller in preexistingInfillers)
			{
				if(_infiller.material == mat)
				{
					infiller = _infiller;
					break;
				}
			}

			//If there is no infiller, than the UI will create one. However, the GUI will not be seen as changed, and
			//therefore if we do not set some flag, than the code lower down will not recognize that it ought to
			//set the item as 'dirty'.

			if(infiller == null)
			{
				infiller = new InfillConfiguration();
				infiller.material = mat;

				infiller.regionForInfill = new Rect(0f, 0f, 1f, 1f);
				forceDirty = true;
			}

			infillersBuilder.Add(infiller);
		}
		
		foreach(var infiller in infillersBuilder)
		{
			EditorGUILayout.Separator();

			var material = infiller.material;
			var materialName = material == null ? "Null" : material.name;

			EditorGUILayout.LabelField("Material: " + materialName);

			infiller.regionForInfill = EditorGUILayout.RectField("Region for infill", infiller.regionForInfill);
		}
		
		if(GUI.changed || forceDirty)
		{
			var infillersArray = infillersBuilder.ToArray();
			
			foreach(Object o in targets)
			{
				Sliceable s = o as Sliceable;

				s.infillers = infillersArray;
				
				EditorUtility.SetDirty(o);
			}
		}
    }

}
