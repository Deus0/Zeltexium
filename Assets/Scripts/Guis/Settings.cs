using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Zeltex.Guis
{
    public class Settings : MonoBehaviour
    {
        public AudioMixer MainMixer;
        public AnimationCurve MixerCurve;

        public void SetAudioLevel(float NewAudioLevel)
        {
            MainMixer.SetFloat("Master", -80 + MixerCurve.Evaluate(NewAudioLevel) * 100);
        }
    }
}