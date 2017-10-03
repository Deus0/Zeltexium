using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Zeltex.Audio
{
    [Serializable]
    public class WavetableSet
    {
        public int WAVETABLESET_MAX_WAVETABLES = 32;
        public List<Wavetable> Wavetables = new List<Wavetable>();

        /*public void Initialize()
        {
            for (int i = 0; i < WAVETABLESET_MAX_WAVETABLES; i++)
            {
                Wavetables.Add(new Wavetable());
                Wavetables[i].TopFrequency = 0;
                Wavetables[i].samples = new float[0];
            }
        }*/

        public int AddWavetable(float topFreq, float[] samples)
        {
            /*if (Wavetables.Count >= WAVETABLESET_MAX_WAVETABLES)
            {
                return _numWavetables;
            }*/
            Wavetable NewTable = new Wavetable();
            NewTable.TopFrequency = topFreq;
            NewTable.Samples = new float[samples.Length];

            for (int i = 0; i < samples.Length; i++)
            {
                NewTable.Samples[i] = samples[i];
            }

            Wavetables.Add(NewTable);
            return 0;
        }

        public Wavetable GetWavetable(int idx)
        {
            if (idx < 0 || idx >= Wavetables.Count)
            {
                return null;
            }
            return Wavetables[idx];
        }

        public int NumWavetables()
        {
            return Wavetables.Count;
        }
    }
}