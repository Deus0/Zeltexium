using UnityEngine;
using System.Collections;

namespace ZeltexTools
{
    /// <summary>
    /// Attached to keys to play sound
    /// </summary>
    public class AlterAudio : MonoBehaviour
    {
        public float SemitoneOffset;
        public KeyCode MyKeyCode;

        void Start()
        {
            AudioSource MySource = GetComponent<AudioSource>();
            MySource.pitch = Mathf.Pow(2f, SemitoneOffset / 12f);
        }

        void Update()
        {
            AudioSource MySource = GetComponent<AudioSource>();
            if (Input.GetKey(MyKeyCode))    //Down
            {
                if (!MySource.isPlaying)
                {
                    MySource.loop = true;
                    MySource.Play();
                }
            }
            if (Input.GetKeyUp(MyKeyCode))    //Down
            {
                MySource.loop = false;
            }
        }
    }
}