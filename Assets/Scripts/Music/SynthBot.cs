using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex.Audio
{

    [Serializable]
    public class SynthBot
    {
        public WavetableOscillator OscillatorA = new WavetableOscillator();
        public WavetableOscillator OscillatorB = new WavetableOscillator();
        public float SampleDuration = 0;
        public float MainVolume = 0;
        public float OscillatorAVolume = 0;
        public float OscillatorBVolume = 0;
        public float LfoPhase = 0;
        public float LfoPhaseIncrement = 0;
        //public float OscillatorATargetFrequency = -1;
        //public float OscillatorBTargetFrequency = -1;

        public void Init(float sampleDuration, List<WavetableSet> wavetableSets)
        {
            SampleDuration = sampleDuration;
            OscillatorA.Init(sampleDuration, wavetableSets);
            OscillatorB.Init(sampleDuration, wavetableSets);
        }

        public void ProcessRaw(float[] Data, int DataSize, int NumberOfChannels)
        {
            for (int i = 0; i < DataSize; i += 1)
            {
                Data[i] = OscillatorA.GetSample(i);
            }
            IncreasePhase();
        }

        public void Process(float[] Data, int DataSize, int NumberOfChannels)
        {
            float sample;

            for (int i = 0; i < DataSize; i += NumberOfChannels)
            {
                float LfoVolume = Mathf.Abs(Mathf.Abs(LfoPhase - 0.5f) - 0.5f) * 2;
                sample = (OscillatorA.GetSample() * OscillatorAVolume + OscillatorB.GetSample() * OscillatorBVolume) * MainVolume * LfoVolume * LfoVolume * LfoVolume;

                for (int j = 0; j < NumberOfChannels; ++j)
                {
                    Data[i + j] *= sample;
                }
                IncreasePhase();
            }
        }

        public List<float> ProcessPhase(int NumberOfChannels)
        {
            List<float> Data = new List<float>();
            float sample;
            int MaxPhaseIncreases = Mathf.CeilToInt(1f / LfoPhaseIncrement);
            for (int i = 0; i < MaxPhaseIncreases; i++)
            {
                float LfoVolume = Mathf.Abs(Mathf.Abs(LfoPhase - 0.5f) - 0.5f) * 2;
                sample = (OscillatorA.GetSample() * OscillatorAVolume + OscillatorB.GetSample() * OscillatorBVolume) * MainVolume * LfoVolume * LfoVolume * LfoVolume;

                for (int j = 0; j < NumberOfChannels; ++j)
                {
                    Data.Add(sample);
                }
                IncreasePhase();
            }
            return Data;
        }

        public void SetPhase(float NewPhase)
        {
            LfoPhase = NewPhase;
        }

        private void IncreasePhase()
        {
            LfoPhase += LfoPhaseIncrement;

            if (LfoPhase > 1.0)
            {
                while (LfoPhase > 1.0)
                {
                    LfoPhase -= 1.0f;
                }
            }
        }

        public void SetMainVolume(float AudioValue)
        {
            MainVolume = AudioValue;
        }

        public float GetLfoPhase()
        {
            return LfoPhase;
        }

        public void SetLfoFrequency(float Frequency, float NewSampleDuration)
        {
            SampleDuration = NewSampleDuration;
            LfoPhaseIncrement = Frequency * (SampleDuration / SynthesizerController.AudioQuality);
        }

        #region OsccilatorA

        public void SetOsc1Volume(float AudioValue)
        {
            OscillatorAVolume = AudioValue;
        }

        public void SetOsc1WavetableAmount(float AudioValue)
        {
            OscillatorA.SetWavetableAmount(AudioValue);
        }

        public void SetOsc1TargetFrequency(float Frequency)//, bool IsImmediate)
        {
            //if (IsImmediate)
            {
                OscillatorA.SetFrequency(Frequency);
            }
            //else
            {
            //    OscillatorATargetFrequency = Frequency;
            }
        }
        #endregion

        #region OsccilatorB

        public void SetOsc2Volume(float AudioValue)
        {
            OscillatorBVolume = AudioValue;
        }

        public void SetOsc2WavetableAmount(float AudioValue)
        {
            OscillatorB.SetWavetableAmount(AudioValue);
        }

        public void SetOsc2TargetFrequency(float Frequency)//, bool IsImmediate)
        {
            //if (IsImmediate)
            {
                OscillatorB.SetFrequency(Frequency);
            }
            //else
            {
            //    OscillatorBTargetFrequency = Frequency;
            }
        }
        #endregion

    }
}