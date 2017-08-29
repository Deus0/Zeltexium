using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex.Sounds
{
    /// <summary>
    /// Holds a pool of audio components and plays them
    /// </summary>
    public class SoundManager : PoolBase<AudioSource>
    {
        /// <summary>
        /// Creates a sound in the world position of the created bullet
        /// </summary>
        public static void CreateNewSound(Vector3 Position, AudioClip MySound, float SoundVolume)
        {
            if (MySound != null)
            {
                GameObject BulletSpawnSound = new GameObject();
                BulletSpawnSound.transform.position = Position;
                BulletSpawnSound.name = "SpawnedSound [" + MySound.name + "]";
                AudioSource MySource = BulletSpawnSound.AddComponent<AudioSource>();
                MySource.rolloffMode = AudioRolloffMode.Logarithmic;
                MySource.minDistance = 0.5f;
                MySource.maxDistance = 16;
                MySource.reverbZoneMix = 1;
                //MySource.panStereo = 1;
                //MySource.spread = 180;
                MySource.spatialBlend = 1;
                MySource.PlayOneShot(MySound, SoundVolume);
                BulletSpawnSound.transform.SetParent(SoundManager.Get().transform);
                Destroy(BulletSpawnSound, MySound.length); // * 0.1f
            }
        }
    }

}