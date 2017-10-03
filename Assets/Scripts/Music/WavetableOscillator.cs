using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Zeltex.Audio
{
    [Serializable]
    public class WavetableOscillator
    {
        public List<WavetableSet> WavetableSets = new List<WavetableSet>();
        public float SampleDuration = 0;
        public float Phase = 0;
        public float PhaseIncrement = 0;
        public float WaveTableAmount = 0;
        public float WaveTableInterpolate = 0;
        public int WaveTableSetIndexA = 0;
        public int WaveTableSetIndexB = 0;
        public int WaveTableIndexA = 0;
        public int WaveTableIndexB = 0;

        public void Init(float sampleDuration, List<WavetableSet> NewWavetableSets)
        {
            SampleDuration = sampleDuration;
            WavetableSets = NewWavetableSets;
        }

        public void RefreshWavetables()
        {
            // reset all wavetable indexes
            WaveTableSetIndexA = 0;
            WaveTableSetIndexB = 0;
            WaveTableIndexA = 0;
            WaveTableIndexB = 0;

            // cache the interpolate amount
            WaveTableInterpolate = WaveTableAmount * (WavetableSets.Count - 1);
            int WaveTableSetIndex = (int)WaveTableInterpolate;
            WaveTableInterpolate -= WaveTableSetIndex;

            // if we're out of range, don't try to find wavetables
            if (WaveTableSetIndex >= WavetableSets.Count)
            {
                return;
            }

            // get the first wavetable
            WaveTableSetIndexA = WaveTableSetIndex;
            if (WavetableSets[WaveTableSetIndexA].NumWavetables() > 0)
            {
                WavetableSet MyWavetableSet = WavetableSets[WaveTableSetIndexA];
                WaveTableIndexA = 0;
                if (MyWavetableSet != null)
                {
                    while (WaveTableIndexA < MyWavetableSet.NumWavetables() - 1 && 
                        PhaseIncrement > MyWavetableSet.GetWavetable(WaveTableIndexA).TopFrequency)
                    {
                        ++WaveTableIndexA;
                    }
                }
                else // if (WavetableSets[_wtsIdx1] == null)
                {
                    Debug.LogError("wavetable Set _wtsIdx1 " + WaveTableIndexA + " is null.");
                }
            }

            // if we're not at the end of the wavetables, get the second wavetable
            if (++WaveTableSetIndex < WavetableSets.Count)
            {
                WaveTableSetIndexB = WaveTableSetIndex;
                WavetableSet MyWavetableSet = WavetableSets[WaveTableSetIndexB];

                if (MyWavetableSet.NumWavetables() > 0)
                {
                    WaveTableIndexB = 0;
                    if (MyWavetableSet != null)
                    {
                        while (WaveTableIndexB < MyWavetableSet.NumWavetables() - 1 &&     // if less then max
                            PhaseIncrement > MyWavetableSet.GetWavetable(WaveTableIndexB).TopFrequency)  // find a frequency that is less then the phase increment
                        {
                            ++WaveTableIndexB;
                        }
                    }
                    else
                    {
                        Debug.LogError("wavetable Set _wtsIdx2 " + WaveTableIndexB + " is null.");
                    }
                }
            }
        }

        public void SetFrequency(float Frequency)
        {
            PhaseIncrement = Frequency * (SampleDuration / SynthesizerController.AudioQuality);
            RefreshWavetables();
        }

        public void SetWavetableAmount(float NewWaveTableAmount)
        {
            WaveTableAmount = NewWaveTableAmount;
            RefreshWavetables();
        }

        public float GetSample(int SampleIndex)
        {
            WavetableSet MyWavetableSet = WavetableSets[WaveTableSetIndexA];
            Wavetable MyWavetable = null;
            if (MyWavetableSet != null)
            {
                MyWavetable = MyWavetableSet.GetWavetable(WaveTableIndexA);
            }
            if (MyWavetable != null)
            {
                return MyWavetable.Samples[SampleIndex];
            }
            else
            {
                return 0;
            }
        }

        public float GetSample()
        {
            if (WavetableSets.Count == 0)
            {
                Debug.LogError("Zero Wavetable Sets.");
                return 0;
            }

            float s0, s1, s2;
            float InterpolateValue;
            int SampleIndex;
            // get the sample from the first wavetable set
            WaveTableSetIndexA = Mathf.Clamp(WaveTableSetIndexA, 0, WavetableSets.Count - 1);
            WavetableSet MyWavetableSet = WavetableSets[WaveTableSetIndexA];
            Wavetable MyWavetable = null;
            if (MyWavetableSet != null)
            {
                MyWavetable = MyWavetableSet.GetWavetable(WaveTableIndexA);
            }

            if (MyWavetable != null)
            {
                InterpolateValue = Phase * (MyWavetable.Samples.Length - 1);
                SampleIndex = (int)InterpolateValue;
                InterpolateValue -= SampleIndex;
                SampleIndex = Mathf.Clamp(SampleIndex, 0, MyWavetable.Samples.Length - 1);
                s0 = MyWavetable.Samples[SampleIndex];

                // if we're at the end of the table, loop around
                if (++SampleIndex >= MyWavetable.Samples.Length)
                {
                    SampleIndex = 0;
                }

                s1 = MyWavetable.Samples[SampleIndex];

                // linear interpolate - gets the line from the first wave value to the second
                InterpolateValue = 0.5f;
                s2 = s0 + (s1 - s0) * InterpolateValue;

                // if we don't have a second wavetable set, just return the sample from the first wavetable
                if (WaveTableSetIndexB == -1 || WaveTableIndexB == -1)
                {
                    Phase += PhaseIncrement;

                    while (Phase > 1.0)
                    {
                        Phase -= 1.0f;
                    }

                    return (float)s2;
                }

                // get the sample from the second wavetable set
                {
                    MyWavetable = WavetableSets[WaveTableSetIndexB].GetWavetable(WaveTableIndexB);
                    InterpolateValue = Phase * (MyWavetable.Samples.Length - 1);
                    SampleIndex = (int)InterpolateValue;
                    InterpolateValue -= SampleIndex;
                    SampleIndex = Mathf.Clamp(SampleIndex, 0, MyWavetable.Samples.Length - 1);
                    s0 = MyWavetable.Samples[SampleIndex];

                    // if we're at the end of the table, loop around
                    if (++SampleIndex >= MyWavetable.Samples.Length)
                    {
                        SampleIndex = 0;
                    }

                    s1 = MyWavetable.Samples[SampleIndex];

                    // linear interpolate
                    InterpolateValue = 0.5f;
                    s1 = s0 + (s1 - s0) * InterpolateValue;
                }

                Phase += PhaseIncrement;

                while (Phase > 1.0)
                {
                    Phase -= 1.0f;
                }

                return (float)(s2 + (s1 - s2) * WaveTableInterpolate);
            }
            else
            {
                return 0;
            }
        }
    }
}