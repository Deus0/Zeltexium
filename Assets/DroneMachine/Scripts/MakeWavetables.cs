using System;
using System.Collections.Generic;
using UnityEngine;

namespace DerelictComputer.DroneMachine
{
    /// <summary>
    /// Utility for creating wavetables from time- or frequency-domain data
    /// 
    /// Based on http://www.earlevel.com/main/2013/03/03/replicating-wavetables/
    /// </summary>
    public class MakeWavetables
    {
        /// <summary>
        /// The length of one cycle of a time-domain wave used for creating wavetables.
        /// 
        /// 2048 samples per cycle at 48kHz sampling rate is about 23Hz,
        /// which is close enough to the bottom of the audible range.
        /// You could set it to 4096 if you want more harmonics in the sub-audible
        /// range (which is actually audible due to harmonics!), but it uses more memory
        /// 
        /// NOTE: Set this to a power of 2 or conversion will break.
        /// </summary>
        private const int CycleLength = 2048;

        /// <summary>
        /// Make wavetables from a list of AudioClips
        /// </summary>
        /// <param name="audioClips">the list of AudioClips</param>
        /// <returns>the collection of WavetableSets</returns>
        public static WavetableSet[] MakeFromAudioClips(List<AudioClip> audioClips)
        {
            WavetableSet[] wavetableSets = new WavetableSet[audioClips.Count];

            for (int i = 0; i < audioClips.Count; i++)
            {
                wavetableSets[i] = MakeFromAudioClip(audioClips[i]);
            }

            return wavetableSets;
        }

        /// <summary>
        /// Make a wavetable from an AudioClip
        /// </summary>
        /// <param name="clip">the AudioClip</param>
        /// <returns>the WavetableSet</returns>
        public static WavetableSet MakeFromAudioClip(AudioClip clip)
        {
            Debug.Log("Making wavetable from " + clip.name);
            
            double[] waveSamples = new double[CycleLength];
            float[] clipData = new float[clip.samples];

            if (!clip.GetData(clipData, 0))
            {
                Debug.LogError("Something went wrong, or the AudioClip didn't have any samples. Try running it again, because for some reason, this breaks on the first run after compiling scripts.");
                return null;
            }

            for (int i = 0; i < waveSamples.Length; i++)
            {
                // if the wave isn't exactly 2048 samples long, stretch or shrink it
                // and interpolate so we don't get any unintended harmonics
                double fracPart = (double)i*clipData.Length/waveSamples.Length;
                int intPart = (int) fracPart;
                fracPart -= intPart;
                float samp0 = clipData[intPart];
                float samp1;
                if (++intPart >= clipData.Length)
                {
                    samp1 = 0;
                }
                else
                {
                    samp1 = clipData[intPart];
                }
                waveSamples[i] = (samp0 + (samp1 - samp0)*fracPart);
            }

            return MakeFromTimeDomainSamples(waveSamples);
        }

        /// <summary>
        /// Example of building a sawtooth wavetable set from frequency domain information
        /// </summary>
        /// <returns>the WavetableSet</returns>
        public static WavetableSet MakeFromSawtoothSpectrum()
        {
            int tableLen = 2048;    // to give full bandwidth from 20 Hz
            int idx;
            double[] freqWaveRe = new double[tableLen];
            double[] freqWaveIm = new double[tableLen];

            // make a sawtooth
            for (idx = 0; idx < tableLen; idx++)
            {
                freqWaveIm[idx] = 0.0;
            }
            freqWaveRe[0] = freqWaveRe[tableLen >> 1] = 0.0;
            for (idx = 1; idx < (tableLen >> 1); idx++)
            {
                freqWaveRe[idx] = 1.0 / idx;                    // sawtooth spectrum
                freqWaveRe[tableLen - idx] = -freqWaveRe[idx];  // mirror
            }

            return MakeFromFrequencyDomainSamples(freqWaveRe, freqWaveIm);
        }

        /// <summary>
        /// Make a wavetable from an array of time-domain samples
        /// NOTE: the array must have a length that is a power of 2
        /// </summary>
        /// <param name="waveSamples">the samples</param>
        /// <returns>the WavetableSet</returns>
        private static WavetableSet MakeFromTimeDomainSamples(double[] waveSamples)
        {
            int idx;
            double[] freqWaveRe = new double[waveSamples.Length];
            double[] freqWaveIm = new double[waveSamples.Length];

            // take FFT
            for (idx = 0; idx < waveSamples.Length; idx++)
            {
                freqWaveIm[idx] = waveSamples[idx];
                freqWaveRe[idx] = 0.0;
            }
            FFT.Forward(freqWaveRe, freqWaveIm);

            // build a wavetable oscillator
            return MakeFromFrequencyDomainSamples(freqWaveRe, freqWaveIm);
        }

        /// <summary>
        /// Make a wavetable from frequency-domain samples
        /// </summary>
        /// <param name="freqWaveRe"></param>
        /// <param name="freqWaveIm"></param>
        /// <returns></returns>
        private static WavetableSet MakeFromFrequencyDomainSamples(double[] freqWaveRe, double[] freqWaveIm)
        {
            int numSamples = freqWaveRe.Length;

            // zero DC offset and Nyquist
            freqWaveRe[0] = freqWaveIm[0] = 0.0;
            freqWaveRe[numSamples >> 1] = freqWaveIm[numSamples >> 1] = 0.0;

            // determine maxHarmonic, the highest non-zero harmonic in the wave
            int maxHarmonic = numSamples >> 1;
            const double minVal = 0.000001; // -120 dB
            while ((Math.Abs(freqWaveRe[maxHarmonic]) + Math.Abs(freqWaveIm[maxHarmonic]) < minVal) && (maxHarmonic != 0)) --maxHarmonic;
            Debug.Log("Max harmonic: " + maxHarmonic);

            // calculate topFreq for the initial wavetable
            // maximum non-aliasing playback rate is 1 / (2 * maxHarmonic), but we allow aliasing up to the
            // point where the aliased harmonic would meet the next octave table, which is an additional 1/3
            double topFreq = 2.0 / 3.0 / maxHarmonic;
            Debug.Log("Top freq: " + topFreq);

            // for subsquent tables, double topFreq and remove upper half of harmonics
            List<WavetableSet.Wavetable> wavetables = new List<WavetableSet.Wavetable>();
            double[] ar = new double[numSamples];
            double[] ai = new double[numSamples];
            double scale = -1;
            while (maxHarmonic != 0)
            {
                // fill the table in with the needed harmonics
                int idx;
                for (idx = 0; idx < numSamples; idx++)
                    ar[idx] = ai[idx] = 0.0;
                for (idx = 1; idx <= maxHarmonic; idx++)
                {
                    ar[idx] = freqWaveRe[idx];
                    ai[idx] = freqWaveIm[idx];
                    ar[numSamples - idx] = freqWaveRe[numSamples - idx];
                    ai[numSamples - idx] = freqWaveIm[numSamples - idx];
                }

                // get the time domain samples
                FFT.Inverse(ar, ai);

                // get the scale the first time around
                if (scale < 0)
                {
                    double max = 0;
                    for (idx = 0; idx < numSamples; idx++)
                    {
                        double tmp = Math.Abs(ai[idx]);
                        if (tmp > max)
                            max = tmp;
                    }
                    scale = 1/max*0.999;
                }

                float[] wave = new float[numSamples];

                for (idx = 0; idx < numSamples; idx++)
                {
                    wave[idx] = (float) (ai[idx]*scale);
                }

                // make the wavetable
                wavetables.Add(new WavetableSet.Wavetable(topFreq, wave));

                // prepare for next table
                topFreq *= 2;
                maxHarmonic >>= 1;
            }

            return new WavetableSet(wavetables.ToArray());
        }
    }
}
