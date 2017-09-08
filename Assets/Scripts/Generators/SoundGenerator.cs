using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Voxels;
using Zeltex.Util;
using Zeltex.Combat;
using Zeltex.Items;
using Zeltex.Sound;

namespace Zeltex.MakerGuiSystem
{
    /// <summary>
    /// Generates sounds for our spells, characters, voxels, items etc
    /// </summary>
    public class SoundGenerator : MonoBehaviour
    {
        public int SampleCount = 512;
        public int SoundsToGenerate = 12;

        public IEnumerator GenerateData()
		{
			for (int i = 0; i < SoundsToGenerate; i++)
			{
				CreateSound(320 + 20 * i);
			}
			yield break;
        }

		private void CreateSound(int Frequency)
		{
            string NewAudioName = Zeltex.NameGenerator.GenerateVoxelName();
            AudioClip NewSound = AudioClip.Create(NewAudioName, SampleCount, 1, Frequency, false);
			Zound MyZound = new Zound();
			MyZound.UseAudioClip(NewSound);
			CurveGenerator.GenerateSinWave(MyZound);
			//MyZound.DebugData();
			NewSound = MyZound.GenerateAudioClip();
            NewSound.name = NewAudioName;
           // DataManager.Get().AddSound("Sounds", NewSound);

		}
	}
}
