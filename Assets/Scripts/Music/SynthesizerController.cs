using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using DerelictComputer;
//using DerelictComputer.DroneMachine;

namespace Zeltex.Audio
{
    /// <summary>
    /// Harmonizes multiple synthesizers
    /// </summary>
    [ExecuteInEditMode]
    public class SynthesizerController : MonoBehaviour
    {
        public List<Synthesizer> Data = new List<Synthesizer>();
        
        [SerializeField]
        private MusicUtils.Note RootNote;
        [SerializeField]
        private MusicUtils.ScaleMode ScaleMode;
        [SerializeField]
        private float FrequencyLowest = 0.05f;
        [SerializeField]
        private float FrequencyHighest = 0.5f;

        private float Frequency = 0.05f;

        private float lastTriggered;
        /*[SerializeField]
        private float CooldownMin = 2f;
        [SerializeField]
        private float CooldownMax = 3f;*/
        private float Cooldown;
        public static int AudioQuality = 1;
        public bool IsBakeAudio;

        public EditorAction ActionLoadWaves = new EditorAction();
        public EditorAction ActionBakeAudio = new EditorAction();
        public DerelictComputer.DroneMachine.WavetableSet MySet = new DerelictComputer.DroneMachine.WavetableSet(null);
        public List<WavetableSet> MyWavetableSets = new List<WavetableSet>();
        private int LastAudioPlayed = -1;
        private List<AudioClip> MyAudioClips = new List<AudioClip>();

        private void Awake()
        {
            if (Application.isPlaying)
            {
                AudioQuality = AudioSettings.outputSampleRate;
                for (int i = Data.Count - 1; i >= 0; i--)
                {
                    if (Data[i].gameObject.activeSelf == false)
                    {
                        Data.RemoveAt(i);
                    }
                }
                Frequency = Random.Range(FrequencyLowest, FrequencyHighest);
                for (int i = 0; i < Data.Count; i++)
                {
                    Data[i].SetKeyAndScaleMode(RootNote, ScaleMode);
                    Data[i].SetFrequency(Frequency);
                }
                lastTriggered = Time.time;
                SetKey(RootNote, ScaleMode);
                SetFrequency(Frequency);
                Cooldown = Mathf.PI * 1.2f;// Random.Range(CooldownMin, CooldownMax);
            }
        }

        private void Start()
        {
            if (Application.isPlaying)
            {
                TriggerSettingSynthesizers();
                if (IsBakeAudio)
                {
                    for (int i = 0; i < (int)(MusicUtils.Note.Gb); i++)
                    {
                        RootNote = (MusicUtils.Note)i;
                        SetKey(RootNote, ScaleMode);
                        SetFrequency(Random.Range(FrequencyLowest, FrequencyHighest));
                        MyAudioClips.Add(Data[0].BakeAudioClip(Frequency));
                    }
                    PlayRandomAudio();
                }
            }
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                UpdateAudio();
            }

            if (ActionLoadWaves.IsTriggered())
            {
                DerelictComputer.DroneMachine.WavetableSet.Load();
                MyWavetableSets.Clear();
                for (int i = 0; i < DerelictComputer.DroneMachine.WavetableSet._allWavetableSets.Length; i++)
                {
                    WavetableSet NewSet = new WavetableSet();
                    for (int j = 0; j < DerelictComputer.DroneMachine.WavetableSet._allWavetableSets[i].Wavetables.Length; j++)
                    {
                        Wavetable NewTable = new Wavetable();
                        DerelictComputer.DroneMachine.WavetableSet.Wavetable OldTable = DerelictComputer.DroneMachine.WavetableSet._allWavetableSets[i].Wavetables[j];
                        NewTable.Samples = OldTable.Table;
                        NewTable.TopFrequency = (float) OldTable.TopFrequency;
                        NewSet.Wavetables.Add(NewTable);
                    }
                    MyWavetableSets.Add(NewSet);
                }
            }

            if (ActionBakeAudio.IsTriggered())
            {
                for (int i = 0; i < Data.Count; i++)
                {
                    Data[i].BakeAudioClip(Frequency);
                }
            }
        }

        private void UpdateAudio()
        {
            if (Cooldown != 0f && Time.time - lastTriggered >= Cooldown)
            {
                lastTriggered = Time.time;
                /*TriggerSettingSynthesizers();
                if (Data.Count >= 4)
                {
                    Data[Data.Count - 3].gameObject.SetActive(!(Random.Range(1, 100) >= 90));
                    Data[Data.Count - 2].gameObject.SetActive(!(Random.Range(1, 100) >= 80));
                    Data[Data.Count - 1].gameObject.SetActive(!(Random.Range(1, 100) >= 70));
                }*/
                if (IsBakeAudio)
                {
                    PlayRandomAudio();
                }
                //Cooldown = Mathf.PI;// Random.Range(CooldownMin, CooldownMax);
            }
        }

        private void PlayRandomAudio()
        {
            if (MyAudioClips.Count > 0)
            {
                for (int i = 0; i < Data.Count; i++)
                {
                    AudioSource MySouce = Data[i].GetComponent<AudioSource>();
                    MySouce.loop = false;
                    int NewAudioToPlay = Random.Range(0, MyAudioClips.Count - 1);
                    while (NewAudioToPlay == LastAudioPlayed)
                    {
                        NewAudioToPlay = Random.Range(0, MyAudioClips.Count - 1);
                    }
                    LastAudioPlayed = NewAudioToPlay;
                    MySouce.clip = MyAudioClips[NewAudioToPlay];
                    MySouce.Play();
                    Cooldown = MySouce.clip.length;
                }
            }
        }

        private void TriggerSettingSynthesizers()
        {
            RootNote = (MusicUtils.Note)Random.Range((int)MusicUtils.Note.A, (int)MusicUtils.Note.Gb);
            SetKey(RootNote, ScaleMode);
            SetFrequency(Random.Range(FrequencyLowest, FrequencyHighest));
        }

        /// <summary>
        /// Set main LFO frequency based on beats per minute
        /// </summary>
        public void SetTempo(float bpm, float duration = -1)
        {
            SetFrequency(bpm / 60.0f, duration);
        }

        /// <summary>
        /// Set the key for any DroneSynths registered to this DroneMachine
        /// </summary>
        public void SetKey(MusicUtils.Note rootNote, MusicUtils.ScaleMode scaleMode)
        {
            for (int i = 0; i < Data.Count; i++)
            {
                Data[i].SetKeyAndScaleMode(rootNote, scaleMode);
            }

            RootNote = rootNote;
            ScaleMode = scaleMode;
        }

        /// <summary>
        /// Set main LFO frequency
        /// </summary>
        public void SetFrequency(float NewFrequency, float duration = -1)
        {
            Frequency = NewFrequency;
            for (int i = 0; i < Data.Count; i++)
            {
                Data[i].SetFrequency(Frequency);
            }
        }

        public float GetFrequency()
        {
            return Frequency;
        }
    }

}