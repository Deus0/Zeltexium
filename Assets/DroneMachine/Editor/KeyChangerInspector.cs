using UnityEditor;
using UnityEngine;

namespace DerelictComputer.DroneMachine
{
    [CustomEditor(typeof(KeyChanger)), CanEditMultipleObjects]
    public class KeyChangerInspector : Editor
    {
        private SerializedProperty _triggerType;
        private SerializedProperty _rootNote;
        private SerializedProperty _scaleMode;
        private SerializedProperty _frequency;
        private SerializedProperty _frequencyChangeTime;
        private SerializedProperty cooldown;

        private void OnEnable()
        {
            _triggerType = serializedObject.FindProperty("_triggerType");
            _rootNote = serializedObject.FindProperty("_rootNote");
            _scaleMode = serializedObject.FindProperty("_scaleMode");
            _frequency = serializedObject.FindProperty("_frequency");
            cooldown = serializedObject.FindProperty("Cooldown");
            _frequencyChangeTime = serializedObject.FindProperty("_frequencyChangeTime");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_triggerType);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("New Key");
            EditorGUILayout.PropertyField(_rootNote);
            EditorGUILayout.PropertyField(_scaleMode);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("New Frequency/Tempo");
            float freq = (float)_frequency.doubleValue;
            freq = EditorGUILayout.Slider("Frequency (Hz)", freq, 0.125f, 8f);
            freq = EditorGUILayout.Slider("Tempo (BPM)", freq*60f, 7.5f, 480f)/60f;
            if (GUI.changed)
            {
                _frequency.doubleValue = freq;
            }
            EditorGUILayout.Slider(_frequencyChangeTime, 0f, 10f);

            EditorGUILayout.PropertyField(cooldown);

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
