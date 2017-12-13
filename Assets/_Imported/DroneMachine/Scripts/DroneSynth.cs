using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DerelictComputer.DroneMachine
{
    [RequireComponent(typeof(AudioSource))]
    public class DroneSynth : MonoBehaviour
    {
        [Header("Musical Settings")]
        [Range(0.125f, 16f)] public double LfoCycleMultiplier = 1;
        [Range(1, 7)] public int ScaleInterval = 1;
        [Range(0, 8)] public int Octave = 0;

        [Header("Synth Settings")]
        [Range(0f, 1f)] public float MainVolume = 1;
        [Range(0f, 1f)] public float Osc1Volume = 0.5f;
        [Range(-12f, 12f)] public double Osc1Pitch = 0;
        [Range(0f, 1f)] public double Osc1WavetableAmount = 0.5;
        [Range(0f, 1f)] public float Osc2Volume = 0.5f;
        [Range(-12f, 12f)] public double Osc2Pitch = 0;
        [Range(0f, 1f)] public double Osc2WavetableAmount = 0.5;

        [HideInInspector] public string PresetId;

        private IntPtr _droneSynthPtr = IntPtr.Zero;

        private double _mainLfoFrequency;
        private MusicMathUtils.Note _rootNote;
        private MusicMathUtils.ScaleMode _scaleMode;

        private double _lfoCycleMultiplier;
        private int _scaleInterval;
        private int _octave;
        private float _mainVolume;
        private float _osc1Volume;
        private double _osc1Pitch;
        private double _osc1WavetableAmount;
        private float _osc2Volume;
        private double _osc2Pitch;
        private double _osc2WavetableAmount;

        private string _presetId;

        public double LfoPhase
        {
            get
            {
                return _droneSynthPtr == IntPtr.Zero ? 0 : DroneSynth_GetLfoPhase(_droneSynthPtr);
            }
        }

		#if !UNITY_EDITOR && UNITY_IOS
		[DllImport("__Internal")]
		#else
		[DllImport("DroneSynthNative")]
		#endif
        private static extern IntPtr DroneSynth_New(double sampleDuration);

		#if !UNITY_EDITOR && UNITY_IOS
		[DllImport("__Internal")]
		#else
		[DllImport("DroneSynthNative")]
		#endif
        private static extern void DroneSynth_Delete(IntPtr droneSynth);

		#if !UNITY_EDITOR && UNITY_IOS
		[DllImport("__Internal")]
		#else
		[DllImport("DroneSynthNative")]
		#endif
        private static extern void DroneSynth_SetMainVolume(IntPtr droneSynth, float volume);

		#if !UNITY_EDITOR && UNITY_IOS
		[DllImport("__Internal")]
		#else
		[DllImport("DroneSynthNative")]
		#endif
        private static extern void DroneSynth_SetOsc1Volume(IntPtr droneSynth, float volume);

		#if !UNITY_EDITOR && UNITY_IOS
		[DllImport("__Internal")]
		#else
		[DllImport("DroneSynthNative")]
		#endif
        private static extern void DroneSynth_SetOsc1TargetFrequency(IntPtr droneSynth, double frequency, bool immediate);

		#if !UNITY_EDITOR && UNITY_IOS
		[DllImport("__Internal")]
		#else
		[DllImport("DroneSynthNative")]
		#endif
        private static extern void DroneSynth_SetOsc1WavetableAmount(IntPtr droneSynth, double wtAmt);

		#if !UNITY_EDITOR && UNITY_IOS
		[DllImport("__Internal")]
		#else
		[DllImport("DroneSynthNative")]
		#endif
        private static extern void DroneSynth_SetOsc2Volume(IntPtr droneSynth, float volume);

		#if !UNITY_EDITOR && UNITY_IOS
		[DllImport("__Internal")]
		#else
		[DllImport("DroneSynthNative")]
		#endif
        private static extern void DroneSynth_SetOsc2TargetFrequency(IntPtr droneSynth, double frequency, bool immediate);

		#if !UNITY_EDITOR && UNITY_IOS
		[DllImport("__Internal")]
		#else
		[DllImport("DroneSynthNative")]
		#endif
        private static extern void DroneSynth_SetOsc2WavetableAmount(IntPtr droneSynth, double wtAmt);

		#if !UNITY_EDITOR && UNITY_IOS
		[DllImport("__Internal")]
		#else
		[DllImport("DroneSynthNative")]
		#endif
        private static extern void DroneSynth_SetLfoFrequency(IntPtr droneSynth, double frequency);

		#if !UNITY_EDITOR && UNITY_IOS
		[DllImport("__Internal")]
		#else
		[DllImport("DroneSynthNative")]
		#endif
        private static extern void DroneSynth_Process(IntPtr droneSynth, [In, Out] float[] buffer, int numSamples,
            int numChannels);

		#if !UNITY_EDITOR && UNITY_IOS
		[DllImport("__Internal")]
		#else
		[DllImport("DroneSynthNative")]
		#endif
        private static extern double DroneSynth_GetLfoPhase(IntPtr droneSynth);

        public void SetLfoFrequency(double frequency)
        {
            _mainLfoFrequency = frequency;

            if (_droneSynthPtr == IntPtr.Zero)
            {
                return;
            }

            DroneSynth_SetLfoFrequency(_droneSynthPtr, _mainLfoFrequency / _lfoCycleMultiplier);
        }

        public void SetKeyAndScaleMode(MusicMathUtils.Note rootNote, MusicMathUtils.ScaleMode scaleMode)
        {
            _rootNote = rootNote;
            _scaleMode = scaleMode;

            if (_droneSynthPtr == IntPtr.Zero)
            {
                return;
            }

            double baseFrequency = MusicMathUtils.ScaleIntervalToFrequency(ScaleInterval, _rootNote, _scaleMode, Octave);
            DroneSynth_SetOsc1TargetFrequency(_droneSynthPtr, baseFrequency * MusicMathUtils.SemitonesToPitch(Osc1Pitch), false);
            DroneSynth_SetOsc2TargetFrequency(_droneSynthPtr, baseFrequency * MusicMathUtils.SemitonesToPitch(Osc2Pitch), false);
        }

        public void ApplyPreset()
        {
            _presetId = PresetId;

            if (string.IsNullOrEmpty(PresetId) || DroneSynthPresets.Instance.PresetIsDummy(PresetId))
            {
                UpdateSettings(true);
                return;
            }

            var preset = DroneSynthPresets.Instance.GetPresetById(PresetId);
            MainVolume = preset.MainVolume;
            Osc1Volume = preset.Osc1Volume;
            Osc2Volume = preset.Osc2Volume;
            Osc1Pitch = preset.Osc1Pitch;
            Osc2Pitch = preset.Osc2Pitch;
            Osc1WavetableAmount = preset.Osc1Tone;
            Osc2WavetableAmount = preset.Osc2Tone;

            UpdateSettings(true);
        }

        private void OnEnable()
        {
            DroneMachine.Instance.RegisterDroneSynth(this);

            WavetableSet.Load();
            _droneSynthPtr = DroneSynth_New(1.0/AudioSettings.outputSampleRate);

            ApplyPreset();

            // create a dummy clip and start playing it so 3d positioning works
            var dummyClip = AudioClip.Create("dummyclip", 1, 1, AudioSettings.outputSampleRate, false);
            dummyClip.SetData(new float[] { 1 }, 0);
            var audioSource = GetComponent<AudioSource>();
            audioSource.clip = dummyClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        private void OnDisable()
        {
            DroneSynth_Delete(_droneSynthPtr);
            _droneSynthPtr = IntPtr.Zero;
            WavetableSet.Unload();
            DroneMachine.Instance.UnregisterDroneSynth(this);
        }

        private void OnValidate()
        {
            UpdateSettings(false);
        }

        private void UpdateSettings(bool force)
        {
            if (PresetId != _presetId)
            {
                ApplyPreset();
            }

            if (_droneSynthPtr == IntPtr.Zero)
            {
                return;
            }

            bool updateOscFrequency = false;

            if (force || LfoCycleMultiplier != _lfoCycleMultiplier)
            {
                _lfoCycleMultiplier = LfoCycleMultiplier;
                DroneSynth_SetLfoFrequency(_droneSynthPtr, _mainLfoFrequency/_lfoCycleMultiplier);
            }

            if (force || ScaleInterval != _scaleInterval)
            {
                _scaleInterval = ScaleInterval;
                updateOscFrequency = true;
            }

            if (force || Octave != _octave)
            {
                _octave = Octave;
                updateOscFrequency = true;
            }

            if (force || MainVolume != _mainVolume)
            {
                _mainVolume = MainVolume;
                DroneSynth_SetMainVolume(_droneSynthPtr, _mainVolume);
            }

            if (force || Osc1Volume != _osc1Volume)
            {
                _osc1Volume = Osc1Volume;
                DroneSynth_SetOsc1Volume(_droneSynthPtr, _osc1Volume);
            }

            if (force || updateOscFrequency || Osc1Pitch != _osc1Pitch)
            {
                _osc1Pitch = Osc1Pitch;
                double baseFrequency = MusicMathUtils.ScaleIntervalToFrequency(ScaleInterval, _rootNote, _scaleMode, Octave);
                DroneSynth_SetOsc1TargetFrequency(_droneSynthPtr, baseFrequency * MusicMathUtils.SemitonesToPitch(Osc1Pitch), force);
            }

            if (force || Osc1WavetableAmount != _osc1WavetableAmount)
            {
                _osc1WavetableAmount = Osc1WavetableAmount;
                DroneSynth_SetOsc1WavetableAmount(_droneSynthPtr, _osc1WavetableAmount);
            }

            if (force || Osc2Volume != _osc2Volume)
            {
                _osc2Volume = Osc2Volume;
                DroneSynth_SetOsc2Volume(_droneSynthPtr, _osc2Volume);
            }

            if (force || updateOscFrequency || Osc2Pitch != _osc2Pitch)
            {
                _osc2Pitch = Osc2Pitch;
                double baseFrequency = MusicMathUtils.ScaleIntervalToFrequency(ScaleInterval, _rootNote, _scaleMode, Octave);
                DroneSynth_SetOsc2TargetFrequency(_droneSynthPtr, baseFrequency * MusicMathUtils.SemitonesToPitch(Osc2Pitch), force);
            }

            if (force || Osc2WavetableAmount != _osc2WavetableAmount)
            {
                _osc2WavetableAmount = Osc2WavetableAmount;
                DroneSynth_SetOsc2WavetableAmount(_droneSynthPtr, _osc2WavetableAmount);
            }
        }

        private void OnAudioFilterRead(float[] buffer, int numChannels)
        {
            DroneSynth_Process(_droneSynthPtr, buffer, buffer.Length, numChannels);
            Mybuffer = buffer;
        }

        private float[] Mybuffer;
        private float PositionX;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                PositionX = 0;
                for (int i = 0; i < Mybuffer.Length; i += 1)
                {
                    Debug.DrawLine(new Vector3(PositionX, 0, 0), new Vector3(PositionX, Mybuffer[i] * 5f, 0), Color.blue, 5f);
                    PositionX += 0.1f;
                }
            }
        }
    }
}
