using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Zeltex.Audio
{
    public static class MusicUtils
    {
        public enum Note { A, Bb, B, C, Db, D, Eb, E, F, Gb, G, Ab }

        public enum ScaleMode { Ionian = 0, Dorian, Phrygian, Lydian, Mixolydian, Aeolian, Locrian }

        public const float A0 = 27.5f;

        /// <summary>
        /// Converts a semitone offset to a percentage pitch
        /// </summary>
        /// <param name="semitones">number of semitones from center</param>
        /// <returns>percentage-based pitch</returns>
        /*public static float SemitonesToPitch(float semitones)
        {
            return Mathf.Pow(2f, semitones / 12f);
        }*/

        /// <summary>
        /// Converts a semitone offset to a percentage pitch
        /// </summary>
        /// <param name="semitones">number of semitones from center</param>
        /// <returns>percentage-based pitch</returns>
        public static float SemitonesToPitch(float semitones)
        {
            return Mathf.Pow(2.0f, semitones / 12.0f);
        }

        public static float PitchToSemitones(float pitch)
        {
            // pitch = 2^(semitones/12)
            // log2(pitch) = semitones/12
            // semitones = log2(pitch)*12
            return Mathf.Log(pitch, 2f) * 12;
        }

        public static float ScaleIntervalToFrequency(int interval, Note rootNote, ScaleMode mode, int octave)
        {
            if (interval < 1 || interval > 7)
            {
                Debug.LogError("Scale interval out of range: " + interval);
                return 0;
            }

            if (octave < 0)
            {
                Debug.LogError("Octave less than zero");
                return 0;
            }

            int ionianRoot = (int)rootNote;

            switch (mode)
            {
                case ScaleMode.Ionian:
                    break;
                case ScaleMode.Dorian:
                    ionianRoot -= 2;
                    break;
                case ScaleMode.Phrygian:
                    ionianRoot -= 4;
                    break;
                case ScaleMode.Lydian:
                    ionianRoot -= 5;
                    break;
                case ScaleMode.Mixolydian:
                    ionianRoot -= 7;
                    break;
                case ScaleMode.Aeolian:
                    ionianRoot -= 9;
                    break;
                case ScaleMode.Locrian:
                    ionianRoot -= 11;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("mode", mode, null);
            }

            interval -= 1; // zero-index
            int ionianInterval = (interval + (int)mode) % 7;
            int extraOctaves = (interval + (int)mode) / 7;
            int semitones;

            if (ionianInterval == 0)
            {
                semitones = 0;
            }
            else if (ionianInterval == 1)
            {
                semitones = 2;
            }
            else if (ionianInterval == 2)
            {
                semitones = 4;
            }
            else if (ionianInterval == 3)
            {
                semitones = 5;
            }
            else if (ionianInterval == 4)
            {
                semitones = 7;
            }
            else if (ionianInterval == 5)
            {
                semitones = 9;
            }
            else// if (ionianInterval == 6)
            {
                semitones = 11;
            }

            int notesFromA0 = (octave + extraOctaves) * 12 + semitones + ionianRoot;

            return A0 * Mathf.Pow(2.0f, notesFromA0 / 12.0f);
        }
    }

}