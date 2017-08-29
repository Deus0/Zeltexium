using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Zeltex
{
	namespace Sound
	{
		/// <summary>
		/// Used to store and produce the audio clip
		/// </summary>
		public class Zound : Element
        {
			//public string Name = "";
            [SerializeField, JsonProperty]
            public float TimeLength = 0;
            [SerializeField, JsonProperty]
            public int Frequency;
            [SerializeField, JsonProperty]
			private float[] MySamples = new float[0];
            [SerializeField, JsonProperty]
            public int Channels = 1;
            [JsonIgnore]
            private AudioClip MyAudioClip;

            public Zound()
			{
				MySamples = new float[256];
				Frequency = 440;
				TimeLength = 1f;
			}

            public float[] GetSamples()
            {
                return MySamples;
            }

            public void SetSamples(float[] NewSamples)
            {
                MySamples = NewSamples;
                OnModified();
            }

            public void SetSample(int SampleIndex, float AddValue)
            {
                MySamples[SampleIndex] = AddValue;
            }

            public void AddToSample(int SampleIndex, float AddValue)
            {
                MySamples[SampleIndex] += AddValue;
            }

            public float GetSample(int SampleIndex)
            {
                return MySamples[SampleIndex];
            }

            public void ClampSample(int SampleIndex)
            {
                MySamples[SampleIndex] = Mathf.Clamp(MySamples[SampleIndex], -1, 1);
            }

			public void DebugData()
			{
				Debug.Log("Zound: " + Name);
				Debug.Log("SampleCount: " + MySamples.Length);
				Debug.Log("Frequency: " + Frequency);
				Debug.Log("TimeLength: " + TimeLength);
				for (int i = 0; i < MySamples.Length; i++)
				{
					Debug.Log(i + " " + MySamples[i]);
				}
			}

			#region AudioClip

			/// <summary>
			/// Use audio clips data
			/// </summary>
			public void UseAudioClip(AudioClip NewAudioClip)
            {
                MyAudioClip = NewAudioClip;
                //Name = MyAudioClip.name;
                Frequency = MyAudioClip.frequency;
				MySamples = new float[MyAudioClip.samples];
				if (MyAudioClip.GetData(MySamples, 0) == false)
                {
                    Debug.LogError("Failure to get data from " + NewAudioClip.name);
                }
				TimeLength = MyAudioClip.length;
				Channels = MyAudioClip.channels;
                GenerateAudioClip();    // for testing
                OnModified();
            }


            public AudioClip GetAudioClip()
            {
                if (MyAudioClip == null)
                {
                    return GenerateAudioClip();
                }
                else
                {
                    return MyAudioClip;
                }
            }
			/// <summary>
			/// Converst a raw curve into an audio clip
			/// </summary>
			public AudioClip GenerateAudioClip()
			{
				AudioClip NewAudioClip = AudioClip.Create(
						Name,
						MySamples.Length, //(int)(samplerate * SoundTime/Frequency)* RepeatTimes, 
						Channels,
						Frequency,//samplerate,
						false);  //false
                if (MyAudioClip != null)
                {
                    Debug.LogError("Before Generated Audio: Samples: " + MyAudioClip.samples + ": frequency: " + MyAudioClip.frequency);
                }
                MyAudioClip = NewAudioClip;
                //MyAudioClip.loadType = AudioClipLoadType.DecompressOnLoad
                MyAudioClip.SetData(MySamples, 0);
                //MyAudioClip.loadType = AudioClipLoadType.DecompressOnLoad;
                //MyAudioClip.LoadAudioData();
                Debug.LogError("After Generated Audio: Samples: " + MyAudioClip.samples + ": frequency: " + MyAudioClip.frequency);
				return MyAudioClip;
			}
			#endregion

			#region Initiation

			/// <summary>
			/// Clear the curve
			/// </summary>
			public void Clear()
			{
				TimeLength = 0;
				//preWrapMode = MyWrapMode;
				//postWrapMode = MyWrapMode;
				for (int i = 0; i < MySamples.Length; i++)
				{
					MySamples[i] = 0;
				}
			}

			/// <summary>
			/// Set the time of the curve
			/// </summary>
			public void SetTime(float NewTime)
			{
				float[] OldData = MySamples;
				int SamplesPerSecond = Mathf.CeilToInt(MySamples.Length / TimeLength);
				float[] NewData = new float[Mathf.CeilToInt(NewTime * SamplesPerSecond)];
				for (int i = 0; i < NewData.Length; i++)
				{
					if (i < OldData.Length)
					{
						NewData[i] = OldData[i];
					}
					else
					{
						NewData[i] = 0;
					}
				}
				//MyZound.SamplesCount = NewData.Length;
				MySamples = NewData;
				TimeLength = NewTime;
			}
			#endregion

			#region Utility

			/// <summary>
			/// Reduces the sound data samples for use in the curve editor
			/// </summary>
			public float[] GetReducedData(int SamplesCount)
			{
				float[] MySamplesReduced = new float[SamplesCount];
				if (MySamples.Length == 0)
				{
					Debug.LogError("Inside Zound, no samples.");
					return MySamplesReduced;
				}
				int MyLength = MySamples.Length;
				float Multiplier = MyLength / (float)SamplesCount;
				for (int i = 0; i < SamplesCount; i++)
				{
					int j = Mathf.CeilToInt(i * Multiplier);
					if (j >= 0 && j < MySamples.Length)
					{
						MySamplesReduced[i] = MySamples[j];
					}
					else
					{
						Debug.LogError(j + " is outside sample bounds (count: " + MySamples.Length + "!");
					}
				}
				return MySamplesReduced;
			}

			/// <summary>
			/// Get the sample size
			/// </summary>
			public int GetSize()
			{
				return MySamples.Length;
			}

            public void ReplaceWithCurve(AnimationCurve MyCurve)
            {
                TimeLength = MyCurve.Evaluate(MyCurve.keys[MyCurve.length - 1].value);
                Debug.Log("Replacing Raw Curve with animation curve. TimeLength: " + TimeLength + " - MySamples.Length: "+ MySamples.Length);
                for (int i = 0; i < MySamples.Length; i++)
                {
                    float MyTime = ((float)(i) / (float) (MySamples.Length)) * TimeLength;
                    //Debug.Log(i + " - MyTime: " + MyTime);
                    MySamples[i] = MyCurve.Evaluate(MyTime);
                }
            }
			#endregion
		}
	}
}