using UnityEngine;
using System.Collections;

namespace GuiSystem
{
    /// <summary>
    /// Manages gui button sounds
    /// </summary>
    public class GuiSoundManager : MonoBehaviour
    {
        public AudioSource MySource;
        public AudioClip MySound;

        /// <summary>
        /// play the default sound
        /// </summary>
        public void PlaySound()
        {
            MySource.PlayOneShot(MySound);
        }
    }
}
