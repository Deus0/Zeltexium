using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

namespace DerelictComputer.DroneMachine
{
    public class MakeWavetablesMenu
    {
        [MenuItem("Wavetables/Make Wavetables")]
        public static void MakeFromSounds()
        {
            var path = EditorUtility.OpenFolderPanel("Select Sound Folder", Application.dataPath + "/Audio", "");

            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("Cancelled making wavetables");
                return;
            }

            AssetDatabase.Refresh();

            var audioClips = new List<AudioClip>();

            foreach (var filePath in Directory.GetFiles(path))
            {
                var relativePath = filePath.Replace(Application.dataPath, "Assets");
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(relativePath);

                if (clip != null)
                {
                    audioClips.Add(clip);
                }
            }

            var wavetableSets = MakeWavetables.MakeFromAudioClips(audioClips);
            var formatter = new BinaryFormatter();
            var dataPath = Path.Combine(Application.streamingAssetsPath, WavetableSet.WavetableFileName);
            var file = File.Open(dataPath, FileMode.OpenOrCreate);
            formatter.Serialize(file, wavetableSets);
            file.Close();

            AssetDatabase.Refresh();
        }
    }
}
