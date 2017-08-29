using UnityEngine;
using System.Collections;
using Zeltex.Guis;

namespace Zeltex.Guis.Players
{
	public class MusicPlayer : MonoBehaviour    // music looper should be
	{
		AudioSource MySource;
		public AudioClip MyMusic;
		public MusicList MyMusicList;
		bool IsLooping = true;

		// Use this for initialization
		void Start () 
		{
			MySource = gameObject.GetComponent<AudioSource> ();
			MyMusic = MyMusicList.Music [0];
		}
		
		// Update is called once per frame
		void Update () 
		{
			if (IsLooping && !MySource.isPlaying) 
			{
				MySource.PlayOneShot (MyMusic);
			}
		}
	}
}
