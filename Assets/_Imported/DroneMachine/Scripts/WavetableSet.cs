using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace DerelictComputer.DroneMachine
{
    /// <summary>
    /// Container for wavetables used in WavetableOscillator
    /// </summary>
    [Serializable]
    public class WavetableSet
    {
        [Serializable]
        public class Wavetable
        {
            public readonly double TopFrequency;
            public readonly float[] Table;

            public Wavetable(double topFrequency, float[] table)
            {
                TopFrequency = topFrequency;
                Table = table;
            }
        }

        public const string WavetableFileName = "wavetables.dat";

        public static WavetableSet[] _allWavetableSets;

        private static int _loadCount = 0;

        public readonly Wavetable[] Wavetables;

        public WavetableSet(Wavetable[] wavetables)
        {
            Wavetables = wavetables;
        }

		#if !UNITY_EDITOR && UNITY_IOS
		[DllImport("__Internal")]
		#else
        [DllImport("DroneSynthNative")]
		#endif
        private static extern void WavetableSet_CreateArray(int numWavetableSets);

		#if !UNITY_EDITOR && UNITY_IOS
		[DllImport("__Internal")]
		#else
		[DllImport("DroneSynthNative")]
		#endif
        private static extern void WavetableSet_FreeArray();

		#if !UNITY_EDITOR && UNITY_IOS
		[DllImport("__Internal")]
		#else
		[DllImport("DroneSynthNative")]
		#endif
        private static extern void WavetableSet_AddWavetable(int wavetableSetIdx, double topFreq, [In] float[] samples,
            int numSamples);

        public static void Load()
        {
            if (_allWavetableSets == null)
            {
                var formatter = new BinaryFormatter();
                FileStream file;
                var path = Path.Combine(Application.streamingAssetsPath, WavetableFileName);
                try
                {
					file = File.OpenRead(path);
                }
                catch (Exception)
                {
                    Debug.LogWarning("No wavetable data yet. Make it via the editor menu!");
                    return;
                }

                _allWavetableSets = (WavetableSet[]) formatter.Deserialize(file);
                file.Close();

                WavetableSet_CreateArray(_allWavetableSets.Length);

                for (int i = 0; i < _allWavetableSets.Length; i++)
                {
                    for (int j = 0; j < _allWavetableSets[i].Wavetables.Length; j++)
                    {
                        Wavetable wt = _allWavetableSets[i].Wavetables[j];
                        WavetableSet_AddWavetable(i, wt.TopFrequency, wt.Table, wt.Table.Length);
                    }
                }
            }

            _loadCount++;
        }

        public static void Unload()
        {
            if (--_loadCount > 0)
            {
                return;
            }

            WavetableSet_FreeArray();
            _allWavetableSets = null;
        }
    }
}
