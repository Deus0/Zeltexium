using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Zeltex.AnimationUtilities
{

    public class SpeechAnimator : MonoBehaviour
    {
        [Header("Events")]
        public UnityEvent OnEnd = new UnityEvent();
        [Header("TextSettings")]
        public bool IsAnimateOnStart = false;
        public float Delay = 0f;
        public float AnimationSpeedMin = 0.02f;
        public float AnimationSpeedMax = 0.06f;
        [Header("SoundSettings")]
        public List<AudioClip> MySpeechSounds = new List<AudioClip>();
        public bool IsSound = true;
        public int SoundInterval = 3;
        public float SoundMin = 0.25f;
        public float SoundMax = 0.8f;
        // privates
        private int CharacterIndex;
        private string MySpeech = "";
        private Text AnimationTextReference;
        private float UpdateCoolDown = 0.1f;
        private float LastUpdatedTime = 0;
        private AudioSource MyAudioSource;

        void Start()
        {
            AnimationTextReference = gameObject.GetComponent<Text>();
            if (MyAudioSource == null)
                MyAudioSource = gameObject.GetComponent<AudioSource>();
            if (MyAudioSource == null)
                MyAudioSource = gameObject.AddComponent<AudioSource>();
        }

        IEnumerator DelayBeginning()
        {
            yield return new WaitForSeconds(Delay);
            ResetAnimation();
        }
	
	    void Update ()
        {
            if (IsAnimateOnStart)
            {
                IsAnimateOnStart = false;
                MySpeech = AnimationTextReference.text;
                AnimationTextReference.text = "";
                StartCoroutine(DelayBeginning());
            }
            UpdateAnimation ();
	    }

	    // animates from AnimationTextReference.text to MySpeech (both strings)
	    private void UpdateAnimation()
        {
		    if (MySpeech != null)
            {
			    if (CharacterIndex < MySpeech.Length && 
				    Time.time - LastUpdatedTime > UpdateCoolDown)
                {
				    // make sound - different sound for each character added - its meant to sound like a type writer
				    if (IsSound)
                    {
					    if (CharacterIndex % SoundInterval == 0 && MySpeechSounds.Count > 0)
                        {
						    MyAudioSource.PlayOneShot (MySpeechSounds [Random.Range (0, MySpeechSounds.Count)]);
						    if (MySpeech [CharacterIndex] == ' ')
							    MyAudioSource.volume = 0.3f;
						    else
							    MyAudioSource.volume = Random.Range (SoundMin, SoundMax);
					    }
				    }
				    // add another character to it
				    AnimationTextReference.text += MySpeech [CharacterIndex];
				    CharacterIndex++;
				    LastUpdatedTime = Time.time;
				    UpdateCoolDown = Random.Range (AnimationSpeedMin, AnimationSpeedMax);

				    if (CharacterIndex == MySpeech.Length)
                    {   // end of animation
                        //Debug.LogError("Ending Animation at " + Time.time);	// debugging
                    
                        OnEnd.Invoke ();
				    }
			    }
		    }
            else
            {
			    Debug.LogError("NoSpeech bubble reference in animator: " + gameObject.name);
		    }
	    }

	    // begins animation a new, normally use new line instead to change what it animates too
	    public void ResetAnimation()
        {
		    if (AnimationTextReference != null)
            {
			    AnimationTextReference.text = "";	// set to nothing
			    LastUpdatedTime = Time.time;
			    CharacterIndex = 0;
			    UpdateCoolDown = Random.Range (AnimationSpeedMin, AnimationSpeedMax);
		    }
            else
            {
			    Debug.LogError("Problem with Speech Animation: " + gameObject.name);
		    }
	    }

	    public void NewLine(string NewLine)
        {
		    MySpeech = NewLine;
            StopCoroutine(DelayBeginning());
            StartCoroutine(DelayBeginning());
        }
        public void UpdateLine(string NewLine, float Delay2)
        {
            MySpeech = NewLine;
            Delay = Delay2;
            StopCoroutine(DelayBeginning());
            StartCoroutine(DelayBeginning());
        }
    }
}