using UnityEngine;
using System;  // Needed for Math
using UnityEngine.UI;

namespace ZeltexTools
{
    /// <summary>
    /// Renders the curve for our sound maker!
    /// </summary>
    /*public class CurvePlayer : MonoBehaviour
    {
        public CurveRenderer MyCurveDisplay;
       // public AudioClip MyAudioClip;
        public bool IsLoop;
        public float Amplitude = 1f;
        public float SoundTime = 1f;
        public GameObject Piano;
        public Text MyFrequencyText;
        public int RepeatTimes = 1;

        public void UpdateFrequency(float Frequency_)
        {
            Frequency = Frequency_;
            MyFrequencyText.text = "[" + Frequency + "]";
        }
    }*/
}
//float SinInput2 = 2 * ((float)Mathf.PI) * frequency * ((float)position) / (float)(samplerate);
// data[count] = Mathf.Sin((float)Mathf.Sin(SinInput2));


// un-optimized version
/*public float frequency = 440;
public float gain = 0.05f;

private float increment;
private float phase;
private float sampling_frequency = 48000;

void OnAudioFilterRead1(float[] data, int channels)
{
    // update increment in case frequency has changed
    increment = frequency * 2 * (float)Math.PI / sampling_frequency;
    for (var i = 0; i < data.Length; i = i + channels)
    {
        phase = phase + increment;
        // this is where we copy audio data to make them “available” to Unity
        data[i] = (float)(gain * Math.Sin(phase));
        // if we have stereo, we copy the mono data to each channel
        if (channels == 2) data[i + 1] = data[i];
        if (phase > 2 * Math.PI) phase = 0;
    }
}

private System.Random RandomNumber = new System.Random();
public float offset = 0;

void OnAudioFilterRead2(float[] data, int channels)
{
    for (int i = 0; i < data.Length; i++)
    {
        data[i] = offset + (float)RandomNumber.NextDouble() * 2.0f - 1.0f;
    }
}
*/
