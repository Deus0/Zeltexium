using UnityEditor;
using UnityEngine;

namespace DerelictComputer.DroneMachine
{
    [CustomEditor(typeof(DroneSynth)), CanEditMultipleObjects]
    public class DroneSynthInspector : Editor
    {
        private string _newPresetName = "New Preset";
        private SerializedProperty _presetId;

        private void OnEnable()
        {
            _presetId = serializedObject.FindProperty("PresetId");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Synth Presets");

            EditorGUILayout.BeginHorizontal();
            _newPresetName = EditorGUILayout.TextField("Name", _newPresetName);
            if (GUILayout.Button("Add Preset"))
            {
                var preset = new DroneSynthPresets.Preset();
                var ds = (DroneSynth) target;
                preset.Name = _newPresetName;
                preset.MainVolume = ds.MainVolume;
                preset.Osc1Volume = ds.Osc1Volume;
                preset.Osc2Volume = ds.Osc2Volume;
                preset.Osc1Pitch = ds.Osc1Pitch;
                preset.Osc2Pitch = ds.Osc2Pitch;
                preset.Osc1Tone = ds.Osc1WavetableAmount;
                preset.Osc2Tone = ds.Osc2WavetableAmount;
                DroneSynthPresets.Instance.AddPreset(preset);

                _presetId.stringValue = preset.Id;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            var originalPresetIdx =
                DroneSynthPresets.Instance.GetPresetIndex(DroneSynthPresets.Instance.GetPresetById(_presetId.stringValue));

            if (originalPresetIdx < 0)
            {
                originalPresetIdx = 0;
            }

            var newPresetIdx = EditorGUILayout.Popup("Preset", originalPresetIdx,
                DroneSynthPresets.Instance.GetPresetNames());

            if (newPresetIdx != originalPresetIdx)
            {
                _presetId.stringValue = DroneSynthPresets.Instance.GetPresetByIndex(newPresetIdx).Id;
            }

            if (GUILayout.Button("Apply"))
            {
                var preset = DroneSynthPresets.Instance.GetPresetByIndex(newPresetIdx);
                var ds = (DroneSynth)target;
                preset.MainVolume = ds.MainVolume;
                preset.Osc1Volume = ds.Osc1Volume;
                preset.Osc2Volume = ds.Osc2Volume;
                preset.Osc1Pitch = ds.Osc1Pitch;
                preset.Osc2Pitch = ds.Osc2Pitch;
                preset.Osc1Tone = ds.Osc1WavetableAmount;
                preset.Osc2Tone = ds.Osc2WavetableAmount;
            }

            if (GUILayout.Button("Delete"))
            {
                DroneSynthPresets.Instance.DeletePreset(DroneSynthPresets.Instance.GetPresetByIndex(newPresetIdx).Id);
                _presetId.stringValue = "";
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reload Presets"))
            {
                DroneSynthPresets.Instance.LoadSavedPresets();
            }
            if (GUILayout.Button("Save Presets"))
            {
                DroneSynthPresets.Instance.SavePresets();
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
