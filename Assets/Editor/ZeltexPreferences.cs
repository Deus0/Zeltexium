using UnityEngine;
using UnityEditor;

namespace Zeltex.Editor
{
    public class ZeltexPreferences : MonoBehaviour
    {
        // Have we loaded the prefs yet
        private static bool hasLoaded = false;

        // The Preferences
        private static bool IsPlayGame = false;

        [PreferenceItem("Zeltex")]
        private static void CustomPreferencesGUI()
        {
            if (!hasLoaded)
            {
                IsPlayGame = EditorPrefs.GetBool("PlayGame", false);
                if (IsPlayGame)
                {

                }
                else
                {

                }
                hasLoaded = true;
            }

            EditorGUILayout.LabelField("Version: X.x");
            IsPlayGame = EditorGUILayout.Toggle("Play Game ", IsPlayGame);

            if (GUI.changed)
            {
                EditorPrefs.SetBool("PlayGame", IsPlayGame);
            }
        }
    }
}
