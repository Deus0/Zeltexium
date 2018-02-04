using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Zeltex
{
	namespace Sound
	{
		/// <summary>
		/// Generates sound curves!
		/// </summary>
		public class CurveGenerator : MonoBehaviour
		{
			[Header("Data")]
			public float TimeLengthX = 1f;
			public WrapMode MyWrapMode = WrapMode.Loop;
			public int KeysMax = 3000;
			// Curve Modifiers
			float NoiseValue = 1f;
			public float CurveFrequency = 1f;
			public int Mode = 0;
			//float TimeLength = 1;
			[Header("References")]
			public CurveRenderer MyWave;
			public Text MyTimeLabel;
			public Text FrequencyLabel;
			//[Header("Events")]
			// public UnityEvent OnUpdateCurve;

			public void SetMode(int NewMode)
			{
				Mode = NewMode;
			}
			// GUI
			public void UpdateTime(float NewLength)
			{
				TimeLengthX = Mathf.RoundToInt(NewLength);
				MyTimeLabel.text = "[" + TimeLengthX + "]";
				//gameObject.GetComponent<CurvePlayer>().SoundTime = TimeLengthX;
			}

			/*public void UpdateCurveFrequency(float NewFrequency)
			{
				CurveFrequency = NewFrequency;
			}*/

			public void UseInput(Button MyButton)
			{

			}

			/*public void UseInput(Toggle MyToggle)
			{
				if (MyToggle.name == "FunctionToggle")
				{
					IsFunc = MyToggle.isOn;
				}
				else if (MyToggle.name == "AddToggle")
				{
					IsAdd = MyToggle.isOn;
				}
				else if (MyToggle.name == "LoopToggle")
				{
					IsLoop = MyToggle.isOn;
				}
			}*/
			public void UseInput(Slider MySlider)
			{
				if (MySlider.name == "FrequencySlider")
				{
					CurveFrequency = MySlider.value;
					FrequencyLabel.text = "Frequency [" + CurveFrequency + "]";
				}
			}
			public void UseInput(InputField MyInput)
			{
				if (MyInput.name == "TimeInput")
				{
					//TimeLength = float.Parse(MyInput.text);
				}
			}

			public void UpdateNoiseValue(float NewNoise)
			{
				NewNoise = Mathf.Clamp(NewNoise, 0, 1f);
				NoiseValue = NewNoise;
			}

			/// <summary>
			/// Clear the curves data!
			/// </summary>
			public void Clear()
			{
				MyWave.MyZound.Clear();
				MyWave.UpdateSound();
			}

			#region CurveGeneration


			public void GenerateSinWave()
			{
				GenerateSinWave(MyWave.MyZound, Mode);
				MyWave.UpdateSound();
			}

			#endregion

			#region CurveGenerationStatic

			/// <summary>
			/// Generates a win wave for our curve!
			/// </summary>
			public static void GenerateSinWave(Zound MyCurve, int BlendMode = 0, float CurveFrequency = 1f, float NoiseValue = 1f)	// default is to set
			{
				for (int i = 0; i < MyCurve.GetSize(); i++)
				{
					float TimeValue = i / MyCurve.TimeLength;
					float MyValue = NoiseValue * Mathf.Sin(CurveFrequency * 2f * Mathf.PI * TimeValue); //  / TimeLengthX
																										//float MyRandomValue = NoiseValue * Random.Range(-NoiseValue, NoiseValue);
					if (BlendMode == 0)
					{
						MyCurve.GetSamples()[i] = MyValue;
					}
					else if (BlendMode == 1)
					{
						MyCurve.GetSamples()[i] += MyValue;
						MyCurve.SetSample(i, Mathf.Clamp(MyCurve.GetSample(i), -1, 1));
					}
					else if (BlendMode == 2)
					{
						MyCurve.SetSample(i, Mathf.Sin(MyCurve.GetSample(i)));
					}
				}
			}

			/// <summary>
			/// Generates Random Noise function for our curve!
			/// </summary>
			public void GenerateNoise()
			{
				for (int i = 0; i < MyWave.MyZound.GetSize(); i++)
				{
					float MyRandomValue = NoiseValue * Random.Range(-NoiseValue, NoiseValue);
					if (Mode == 0)
					{
						MyWave.MyZound.SetSample(i, MyRandomValue);
					}
					else if (Mode == 1)
					{
						MyWave.MyZound.AddToSample(i, MyRandomValue);
						MyWave.MyZound.SetSample(i, Mathf.Clamp(MyWave.MyZound.GetSample(i), -1, 1));
					}
					else if (Mode == 2)
					{
						MyWave.MyZound.SetSample(i, Random.Range(
							-MyWave.MyZound.GetSample(i),
							MyWave.MyZound.GetSample(i)));
					}
				}
				MyWave.UpdateSound();
			}

			/// <summary>
			/// Generates perlin noise function for the wave!
			/// </summary>
			public void GeneratePerlinNoise()
            {
                for (int i = 0; i < MyWave.MyZound.GetSize(); i++)
                {
					float MyValue = NoiseValue * (Mathf.PerlinNoise(i * CurveFrequency, (Time.time) * CurveFrequency) * 2f - 1f);  //
					if (Mode == 0)
                    {
                        MyWave.MyZound.SetSample(i, MyValue);
					}
					else if (Mode == 1)
                    {
                        MyWave.MyZound.AddToSample(i, MyValue);
					}
					else if (Mode == 2)
					{
						float MyValue2 = NoiseValue * (Mathf.PerlinNoise(i * CurveFrequency, MyWave.MyZound.GetSample(i) * CurveFrequency) * 2f - 1f);  //
						MyWave.MyZound.SetSample(i, MyValue2);
                    }
                    MyWave.MyZound.ClampSample(i);
                }
				MyWave.UpdateSound();
			}

			/// <summary>
			/// Generates log!
			/// </summary>
			public void GenerateLog()
			{
				/*Keyframe[] MyKeys = MyWave.MyCurve.keys;
				for (float i = 0; i < MyKeys.Length; i++)
				{
					float MySinValue = NoiseValue * Mathf.Log(2f * Mathf.PI * MyKeys[(int)i].time * TimeLengthX);
					if (IsAdd)
					{
						if (i >= MyKeys.Length)
						{
							Debug.LogError("Keys: " + MyKeys.Length + " and KeysMax: " + KeysMax);
						}
						MyKeys[(int)i].value += MySinValue;
						MyKeys[(int)i].value /= 1f + NoiseValue;
					}
					else if (IsFunc)
					{
						MyKeys[(int)i].value = Mathf.Log(MyKeys[(int)i].value);
					}
					else
					{
						MyKeys[(int)i].value = MySinValue;
					}
				}
				MyWave.MyCurve.keys = MyKeys;
				OnUpdateCurve.Invoke();*/
			}
			#endregion
		}
	}
}
/*

    More Curves

                 case waveType.Sine:
                 data[i] = Mathf.Sin(2 * Mathf.PI * frequency * position / sampleRate);
                 break;
             case waveType.Sawtooth:
                 data[i] = (Mathf.PingPong(frequency * position / sampleRate, 0.5f));
                 break;
             case waveType.Square:
                 data[i] = Mathf.Sign(Mathf.Sin(2 * Mathf.PI * frequency * position / sampleRate)) * 0.5f;
                 break;
             case waveType.Noise:
                 data[i] = Mathf.PerlinNoise(frequency * position / sampleRate, 0);
                 break;
*/