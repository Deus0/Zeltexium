using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace DerelictComputer.DroneMachine
{
    public class DroneSynthPresets
    {
        [Serializable]
        public class Preset
        {
            public readonly string Id;
            public string Name;
            public float MainVolume;
            public float Osc1Volume;
            public float Osc2Volume;
            public double Osc1Pitch;
            public double Osc2Pitch;
            public double Osc1Tone;
            public double Osc2Tone;

            public Preset()
            {
                Id = Guid.NewGuid().ToString();
            }
        }

        private const string PresetFileName = "presets.dat";

        private static DroneSynthPresets _instance;

        public static DroneSynthPresets Instance
        {
            get { return _instance ?? (_instance = new DroneSynthPresets()); }
        }

        private List<Preset> _presets;
        private string[] _presetNames;

        private DroneSynthPresets()
        {
            LoadSavedPresets();
        }

        public void LoadSavedPresets()
        {
            _presets = new List<Preset>();

            var formatter = new BinaryFormatter();
            FileStream file;
            var path = Path.Combine(Application.streamingAssetsPath, PresetFileName);
            try
            {
                file = File.OpenRead(path);
            }
            catch (Exception)
            {
                Debug.LogWarning("No presets. Don't worry, we'll save them when you make some");
                // add a special-case dummy preset
                var dummyPreset = new Preset();
                dummyPreset.Name = "<none>";
                _presets.Add(dummyPreset);
                return;
            }

            _presets = (List<Preset>) formatter.Deserialize(file);
            file.Close();
        }

        public void SavePresets()
        {
            var formatter = new BinaryFormatter();
            var dataPath = Path.Combine(Application.streamingAssetsPath, PresetFileName);
            var file = File.Open(dataPath, FileMode.OpenOrCreate);
            formatter.Serialize(file, _presets);
            file.Close();
        }

        public Preset GetPresetById(string id)
        {
            foreach (var preset in _presets)
            {
                if (preset.Id.Equals(id))
                {
                    return preset;
                }
            }
            
            return null;
        }

        public Preset GetPresetByIndex(int index)
        {
            if (index < 0 || index >= _presets.Count)
            {
                return null;
            }

            return _presets[index];
        }

        public bool PresetIsDummy(string id)
        {
            if (_presets.Count <= 1 || _presets[0].Id.Equals(id))
            {
                return true;
            }

            for (int i = 1; i < _presets.Count; i++)
            {
                if (_presets[i].Id.Equals(id))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetPresetIndex(Preset preset)
        {
            for (int i = 0; i < _presets.Count; i++)
            {
                if (_presets[i] == preset)
                {
                    return i;
                }
            }

            return -1;
        }

        public int GetNumPresets()
        {
            return _presets.Count;
        }

        public string[] GetPresetNames()
        {
            _presetNames = new string[_presets.Count];

            for (int i = 0; i < _presets.Count; i++)
            {
                _presetNames[i] = _presets[i].Name;
            }

            return _presetNames;
        }

        public void AddPreset(Preset preset)
        {
            _presets.Add(preset);
        }

        public void DeletePreset(string id)
        {
            foreach (var preset in _presets)
            {
                if (preset.Id.Equals(id))
                {
                    _presets.Remove(preset);
                    return;
                }
            }
        }
    }
}
 