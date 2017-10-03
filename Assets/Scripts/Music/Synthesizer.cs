using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Zeltex.Audio
{
    /// <summary>
    /// Constantly play sounds
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class Synthesizer : MonoBehaviour
    {
        #region Variables
        [Header("Musical Settings")]
        [Range(0.125f, 16f)]
        public float LfoCycleMultiplier = 1;
        [Range(1, 7)] public int ScaleInterval = 1;
        [Range(0, 8)] public int Octave = 0;

        [Header("Synth Settings")]
        [Range(0f, 1f)]
        public float MainVolume = 1;
        [Range(0f, 1f)] public float Osc1Volume = 0.5f;
        [Range(-12f, 12f)] public float Osc1Pitch = 0;
        [Range(0f, 1f)] public float Osc1WavetableAmount = 0.5f;
        [Range(0f, 1f)] public float Osc2Volume = 0.5f;
        [Range(-12f, 12f)] public float Osc2Pitch = 0;
        [Range(0f, 1f)] public float Osc2WavetableAmount = 0.5f;

        [HideInInspector]
        public string PresetId;

        public SynthesizerController MySynthesizerController;

        [SerializeField]
        private SynthBot MySynth = new SynthBot();


        //private IntPtr _droneSynthPtr = IntPtr.Zero;

        private float Frequency;
        private MusicUtils.Note _rootNote;
        private MusicUtils.ScaleMode _scaleMode;

        private AudioSource MySource;

        [SerializeField]
        private float SampleDuration = 1f;
        [SerializeField]
        private int NumberOfChannels = 1;
        [SerializeField]
        private float Amplification = 1f;

        [Header("Debug")]
        [SerializeField]
        private bool IsRaw;
        [SerializeField]
        private bool IsRecordAudio;
        private List<float> DebugAudioRecording = new List<float>();
        private float TimeStarted = -1;
        [SerializeField]
        private bool DebugIsSave;
        [SerializeField]
        private string AudioFileName = "TestAudioFile";
        [SerializeField]
        private bool IsDebugBuffer;
        private float[] DebugBuffer;   // debugging
        private float PositionX;

        private bool IsProceduralAudio = true;
        #endregion

        #region Functions

        private void Awake()
        {
            MySource = GetComponent<AudioSource>();
            MySource.loop = true;
            MySynth.Init(SampleDuration, MySynthesizerController.MyWavetableSets);  // / AudioSettings.outputSampleRate
        }

        private void Start()
        {
            SetOscillatorDefaults();
            if (!MySynthesizerController.IsBakeAudio)
            {
                DummyAudio();
            }
            BeginRecording();
        }

        private void SetOscillatorDefaults()
        {
            /*MySynth.OscillatorA.WaveTableSetIndexA = 0;
            MySynth.OscillatorA.WaveTableSetIndexB = 1;
            MySynth.OscillatorA.WaveTableIndexA = 1;
            MySynth.OscillatorA.WaveTableIndexB = 1;
            MySynth.OscillatorB.WaveTableSetIndexA = 0;
            MySynth.OscillatorB.WaveTableSetIndexB = 1;
            MySynth.OscillatorB.WaveTableIndexA = 1;
            MySynth.OscillatorB.WaveTableIndexB = 1;*/
            MySynth.OscillatorA.WaveTableSetIndexA = 0;
            MySynth.OscillatorA.WaveTableSetIndexB = 0;
            MySynth.OscillatorA.WaveTableIndexA = 0;
            MySynth.OscillatorA.WaveTableIndexB = 0;
            MySynth.OscillatorB.WaveTableSetIndexA = 0;
            MySynth.OscillatorB.WaveTableSetIndexB = 0;
            MySynth.OscillatorB.WaveTableIndexA = 0;
            MySynth.OscillatorB.WaveTableIndexB = 0;
        }

        private void OnEnable()
        {
            // create a dummy clip and start playing it so 3d positioning works
            if (!MySynthesizerController.IsBakeAudio)
            {
                DummyAudio();
            }
            BeginRecording();
        }

        private void BeginRecording()
        {
            if (IsRecordAudio)
            {
                TimeStarted = Time.time;
                MySource.Play();
            }
        }

        public void SetKeyAndScaleMode(MusicUtils.Note rootNote, MusicUtils.ScaleMode scaleMode)
        {
            _rootNote = rootNote;
            _scaleMode = scaleMode;

            float baseFrequency = (float)MusicUtils.ScaleIntervalToFrequency(ScaleInterval, _rootNote, _scaleMode, Octave);
            MySynth.SetOsc1TargetFrequency(baseFrequency * MusicUtils.SemitonesToPitch(Osc1Pitch));
            MySynth.SetOsc2TargetFrequency(baseFrequency * MusicUtils.SemitonesToPitch(Osc2Pitch));
        }

        public void SetFrequency(float NewFrequency)
        {
            MySynth.SetLfoFrequency(NewFrequency, SampleDuration);
        }

        public AudioClip BakeAudioClip(float NewFrequency)
        {
            if (MySynthesizerController.IsBakeAudio)
            {
                SetOscillatorDefaults();
                MySynth.SetPhase(0);
                SetFrequency(NewFrequency);
                //Debug.LogError("Baking audio for: " + SampleDuration + " seconds with " + AudioSettings.outputSampleRate + " sample quality");
                /*int MaxAudioSamples = Mathf.CeilToInt(SampleDuration * Mathf.PI * 1.1f * AudioSettings.outputSampleRate * NumberOfChannels);
                if (MaxAudioSamples % NumberOfChannels != 0)
                {
                    MaxAudioSamples--;
                }
                float[] Buffer = new float[MaxAudioSamples];  
                for (int i = 0; i < Buffer.Length; i++)
                {
                    Buffer[i] = 1;
                }
                MySynth.Process(Buffer, Buffer.Length, NumberOfChannels);
                */
                float[] Buffer = MySynth.ProcessPhase(NumberOfChannels).ToArray();
                AudioClip MyAudioClip = AudioClip.Create(NameGenerator.GenerateVoxelName(), Buffer.Length / NumberOfChannels, NumberOfChannels, AudioSettings.outputSampleRate, false);    // / NumberOfChannels
                for (int i = 0; i < Buffer.Length; i++)
                {
                    Buffer[i] *= Amplification;
                    Buffer[i] = Mathf.Clamp(Buffer[i], -1, 1);
                }
                MyAudioClip.SetData(Buffer, 0);
                if (IsDebugBuffer)
                {
                    DebugBuffer = Buffer;
                }
                if (DebugIsSave)
                {
                    Util.SavWav.Save(AudioFileName, MyAudioClip);
                }
                return MyAudioClip;
            }
            else
            {
                return null;
            }
        }

        public void DummyAudio()
        {
            AudioClip MyAudioClip = AudioClip.Create("DummyClip", 1, NumberOfChannels, AudioSettings.outputSampleRate, false);
            float[] DummyData = new float[NumberOfChannels];
            for (int i = 0; i < DummyData.Length; i++)
            {
                DummyData[i] = 1;
            }
            MyAudioClip.SetData(DummyData, 0);
            MySource.clip = MyAudioClip;
            MySource.Play();
            IsProceduralAudio = true;
        }

        public double LfoPhase
        {
            get
            {
                return MySynth.GetLfoPhase();
            }
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (!MySynthesizerController.IsBakeAudio && IsProceduralAudio)
            {
                if (IsRaw)
                {
                    MySynth.ProcessRaw(data, data.Length, channels);
                }
                else
                {
                    MySynth.Process(data, data.Length, channels);
                }
                if (IsDebugBuffer)
                {
                    DebugBuffer = data;
                }
                if (IsRecordAudio && TimeStarted != -1)
                {
                    DebugAudioRecording.AddRange(data);
                }
            }
        }
#endif

#if UNITY_EDITOR
        private void Update()
        {
            if (IsDebugBuffer && Input.GetKeyDown(KeyCode.G))
            {
                DebugWave();
            }
            if (IsRecordAudio && !MySynthesizerController.IsBakeAudio && Time.time - TimeStarted >= SampleDuration * 2)
            {
                if (TimeStarted == -1)
                {
                    TimeStarted = Time.time;
                    DebugAudioRecording.Clear();
                }
                else
                {
                    StopRecording();
                }
            }
        }

        private void DebugWave()
        {
            PositionX = 0;
            float ScaleY = 5f;
            Debug.DrawLine(new Vector3(0, -ScaleY, 0), new Vector3(DebugBuffer.Length / 10f, -ScaleY, 0), Color.red, 5f);
            Debug.DrawLine(new Vector3(0, 0, 0), new Vector3(DebugBuffer.Length / 10f, 0, 0), Color.black, 5f);
            Debug.DrawLine(new Vector3(0, ScaleY, 0), new Vector3(DebugBuffer.Length / 10f, ScaleY, 0), Color.red, 5f);
            for (int i = 0; i < DebugBuffer.Length - 1; i++)
            {
                //Debug.DrawLine(new Vector3(PositionX, Mybuffer[i], 0), new Vector3(PositionX, Mybuffer[i + 1], 0), Color.blue, 5f);
                Debug.DrawLine(new Vector3(PositionX, 0, 0), new Vector3(PositionX, DebugBuffer[i] * ScaleY, 0), Color.black, 5f);
                PositionX += 0.1f;
            }
            Debug.DrawLine(new Vector3(PositionX, 0, 0), new Vector3(PositionX, DebugBuffer[DebugBuffer.Length - 1] * ScaleY, 0), Color.black, 5f);
        }

        private void StopRecording()
        {
            IsRecordAudio = false;
            IsProceduralAudio = false;
            //MySynthesizerController.IsBakeAudio = true;
            //Debug.LogError("Baking audio for: " + SampleDuration + " seconds with " + AudioSettings.outputSampleRate + " sample quality");
            /*if (NumberOfChannels == 1)
            {
                for (int i = DebugAudioRecording.Count - 1; i >= 0; i--)
                {
                    if (i % 2 == 0)
                    {
                        DebugAudioRecording.RemoveAt(i);
                    }
                }
            }*/
            AudioClip MyAudioClip = AudioClip.Create(NameGenerator.GenerateVoxelName(), DebugAudioRecording.Count / NumberOfChannels, NumberOfChannels, AudioSettings.outputSampleRate, false);
            DebugBuffer = DebugAudioRecording.ToArray();
            for (int i = 0; i < DebugBuffer.Length; i++)
            {
                DebugBuffer[i] *= Amplification;
                DebugBuffer[i] = Mathf.Clamp(DebugBuffer[i], -1, 1);
            }
            MyAudioClip.SetData(DebugBuffer, 0);
            MySource.clip = MyAudioClip;
            MySource.Play();
            Debug.LogError("Sample Rate: " + AudioSettings.outputSampleRate + ":DataLength:" + DebugAudioRecording.Count + ":Samples:" + MyAudioClip.samples + ":Channels:" + NumberOfChannels);
            TimeStarted = -1;
            Util.SavWav.Save(AudioFileName, MyAudioClip);
        }
#endif

#endregion
    }

}