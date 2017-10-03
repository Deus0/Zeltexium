using System.Collections.Generic;
using UnityEngine;

namespace DerelictComputer.DroneMachine
{
    /// <summary>
    /// The dispatcher for key and tempo changes in the music
    /// </summary>
    public class DroneMachine : MonoBehaviour
    {
        private static DroneMachine _instance;

        public static DroneMachine Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("DroneMachine", typeof(DroneMachine));
                    _instance = go.GetComponent<DroneMachine>();
                }

                return _instance;
            }
        }

        private readonly List<DroneSynth> _synths = new List<DroneSynth>();

        private double _frequencyChangeStartFrequency;
        private double _frequencyChangeEndFrequency;
        private double _frequencyChangeTimeElapsed;
        private double _frequencyChangeTimeTotal;
        private double _currentFrequency;
        private MusicMathUtils.Note _currentRootNote;
        private MusicMathUtils.ScaleMode _currentScaleMode;

        /// <summary>
        /// Register a DroneSynth to get updates from this DroneMachine
        /// </summary>
        /// <param name="synth">the synth</param>
        public void RegisterDroneSynth(DroneSynth synth)
        {
            if (_synths.Contains(synth))
            {
                Debug.LogWarning("Synth already registered: " + synth.name);
                return;
            }

            _synths.Add(synth);

            synth.SetKeyAndScaleMode(_currentRootNote, _currentScaleMode);
            synth.SetLfoFrequency(_currentFrequency);
        }

        public void UnregisterDroneSynth(DroneSynth synth)
        {
            if (_synths.Contains(synth))
            {
                _synths.Remove(synth);
            }
        }

        /// <summary>
        /// Set the key for any DroneSynths registered to this DroneMachine
        /// </summary>
        /// <param name="rootNote">root note of the scale</param>
        /// <param name="scaleMode">scale mode (Dorian, Aeolian, etc)</param>
        public void SetKey(MusicMathUtils.Note rootNote, MusicMathUtils.ScaleMode scaleMode)
        {
            for (int i = 0; i < _synths.Count; i++)
            {
                _synths[i].SetKeyAndScaleMode(rootNote, scaleMode);
            }

            _currentRootNote = rootNote;
            _currentScaleMode = scaleMode;
        }

        /// <summary>
        /// Set main LFO frequency
        /// </summary>
        /// <param name="frequency">frequency (cycles per second)</param>
        /// <param name="duration">(optional) time to reach new frequency</param>
        public void SetFrequency(double frequency, double duration = -1)
        {
            if (duration > 0)
            {
                _frequencyChangeTimeTotal = duration;
                _frequencyChangeTimeElapsed = 0;
                _frequencyChangeStartFrequency = _currentFrequency;
                _frequencyChangeEndFrequency = frequency;
            }
            else
            {
                for (int i = 0; i < _synths.Count; i++)
                {
                    _synths[i].SetLfoFrequency(frequency);
                }

                _currentFrequency = frequency;
            }
        }

        /// <summary>
        /// Set main LFO frequency based on beats per minute
        /// </summary>
        /// <param name="bpm">tempo (beats per minute)</param>
        /// <param name="duration">(optional) time to reach new BPM</param>
        public void SetTempo(double bpm, double duration = -1)
        {
            SetFrequency(bpm/60.0, duration);
        }

        private void ResetFrequencyChange()
        {
            _frequencyChangeTimeTotal = -1;
        }

        private void Update()
        {
            if (_frequencyChangeTimeTotal > 0)
            {
                _frequencyChangeTimeElapsed += Time.deltaTime;

                double frequency;

                if (_frequencyChangeTimeElapsed > _frequencyChangeTimeTotal)
                {
                    ResetFrequencyChange();
                    frequency = _frequencyChangeEndFrequency;
                }
                else
                {
                    frequency = (_frequencyChangeTimeElapsed/_frequencyChangeTimeTotal)*
                                (_frequencyChangeEndFrequency - _frequencyChangeStartFrequency) +
                                _frequencyChangeStartFrequency;
                }

                SetFrequency(frequency);
            }
        }
    }
}
