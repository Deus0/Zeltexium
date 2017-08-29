using UnityEngine;
using UnityEditor;

public class CountObjects : EditorWindow
{
    //string myString = "Hello World";
    //bool groupEnabled;
    //bool myBool = true;
    //float myFloat = 1.23f;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Zeltex/Utility/CountObjects")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        CountObjects window = (CountObjects)EditorWindow.GetWindow(typeof(CountObjects), false, "CountObjects");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Objects Selected", EditorStyles.boldLabel);
        //myString = EditorGUILayout.TextField("Text Field", myString);
        GUILayout.Label("" + Selection.objects.Length);

        //groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        //myBool = EditorGUILayout.Toggle("Toggle", myBool);
        //myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
        //EditorGUILayout.EndToggleGroup();
    }

    private static void OnSelectionChanged()
    {
        EditorWindow.GetWindow <CountObjects>(false, "CountObjects", false).Repaint();
    }

    private void OnFocus()
    {
        Selection.selectionChanged -= OnSelectionChanged;
        Selection.selectionChanged += OnSelectionChanged;
    }

    private void OnDestroy()
    {
        Selection.selectionChanged -= OnSelectionChanged;
    }
}