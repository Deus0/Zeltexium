using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Util;
using System.IO;
using Zeltex.Sound;

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// Handles making blocks 
    /// Meta data saved in alphabetical order as blocks use their id to get meta from the VoxelTileGenerator
    /// </summary>
    public class SoundMaker : ElementMakerGui
    {
        #region Variables
        [Header("SoundMaker")]  // idk why i have 3 references.. lol
        //public List<AudioClip> Data;
        public CurveGenerator MyCurveGenerator;
        public CurveRenderer MyCurveRenderer;
        #endregion

        #region DataManager
        /// <summary>
        /// Set file paths!
        /// </summary>
        protected override void SetFilePaths()
        {
            DataManagerFolder = "Sounds";
        }

        public override int GetSize()
        {
            return DataManager.Get().GetSizeElements(DataFolderNames.Sounds);
        }
        #endregion

        #region Data

        /// <summary>
        /// Returns the selected script
        /// </summary>
        public Zound GetSelectedAudio()
        {
            return DataManager.Get().GetElement(DataFolderNames.Sounds, GetSelectedIndex()) as Zound;
        }

        public AudioClip GetSelectedAudioClip()
        {
            if (GetSelectedAudio() != null)
            {
                return GetSelectedAudio().GetAudioClip();
            }
            else
            {
                return null;
            }
        }

        public AudioClip GetAudio(string Name)
        {
            return (DataManager.Get().GetElement(DataFolderNames.Sounds, Name) as Zound).GetAudioClip();
        }


        public void SetSelected(AudioClip MyAudioClip)
        {
            (DataManager.Get().GetElement(DataFolderNames.Sounds, GetSelectedIndex()) as Zound).UseAudioClip(MyAudioClip);
            //DataManager.Get().SetSound(DataManagerFolder, GetSelectedIndex(), MyAudioClip);
            //OnUpdatedAudioClip();
        }

        #endregion

        #region IndexController

        /// <summary>
        /// Load Gui with Input
        /// Only gets called if index is updated
        /// </summary>
        public override void OnUpdatedIndex(int NewIndex)
        {
            base.OnUpdatedIndex(NewIndex);
            Debug.Log("Updating Index in Polygon Maker: " + NewIndex);
            GetInput("NameInput").text = GetSelectedName();
            OnUpdatedAudioClip();
            // quick hack fixes dropdown menus!
            ScrollRect MyScrollRect = GetDropdown("FunctionTypeDropdown").transform.Find("Template").GetComponent<ScrollRect>();	//
			MyScrollRect.content = MyScrollRect.transform.Find("Viewport").Find("Content").GetComponent<RectTransform>();
		}

		private void OnUpdatedAudioClip()
        {
            if (GetSelectedAudio() != null)
            {
                GetButton("PlayButton").interactable = true;
                GetInput("TimeInput").text = "" + GetSelectedAudio().TimeLength;
                MyCurveRenderer.UpdateCurve(GetSelectedAudioClip(), true);
                GetLabel("SamplesLabel").text = "Samples\n[" + GetSelectedAudio().GetSamples() + "]";
                gameObject.GetComponent<AudioSource>().clip = GetSelectedAudioClip();
            }
            else
            {
                GetInput("TimeInput").text = "";
                MyCurveRenderer.UpdateCurve(null, true);
                GetLabel("SamplesLabel").text = "";
                GetButton("PlayButton").interactable = false;
            }
        }

        /// <summary>
        /// Add a new model to our list
        /// </summary>
        protected override void AddData()
        {
			AudioClip MyClip = AudioClip.Create(Zeltex.NameGenerator.GenerateVoxelName(), 256, 1, 440, false);
            Zound NewZound = new Zound();
            NewZound.UseAudioClip(MyClip);
	        DataManager.Get().AddElement(DataManagerFolder, NewZound);
		}

        #endregion

        #region UI
        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(InputField MyInputField)
        {
            if (MyInputField.name == "NameInput")
            {
                MyInputField.text = Rename(MyInputField.text);
            }
            if (MyInputField.name == "TimeInput")
            {
                float NewTimeLength = float.Parse(MyInputField.text);
                Debug.Log("Updating time to: " + NewTimeLength);
                MyCurveRenderer.SetTime(NewTimeLength);
            }
        }

        public override void UseInput(Dropdown MyDropdown)
        {
            if (MyDropdown.name == "FunctionTypeDropdown")
            {
                GetComponent<CurveGenerator>().SetMode(MyDropdown.value);
            }
        }

        /// <summary>
        /// Used for generically updating buttons
        /// </summary>
        public override void UseInput(Button MyButton)
        {
            if (MyButton.name == "GenerateAudioClip")
            {
                GetSelectedAudio().GenerateAudioClip();
            }
            else if (MyButton.name == "PlayButton")
            {
                //Debug.LogError("Playing sound: " + GetSelectedAudio().name);
                GetComponent<AudioSource>().PlayOneShot(GetSelectedAudioClip());
            }
            else if (MyButton.name == "RecordButton")
            {

#if UNITY_WEBGL || UNITY_ANDROID
                // Find a webgl solution for recording audio
#else
                SetSelected(Microphone.Start("Built-in Microphone", true, 3, 44100));
                StartCoroutine(StopRecording());
#endif
            }
        }
        public IEnumerator StopRecording()
        {
            yield return new WaitForSeconds(3f);
#if UNITY_WEBGL || UNITY_ANDROID
            // Find a webgl solution for recording audio
#else
            Microphone.End("Built-in Microphone");
            SetSelected(GetSelectedAudio()); // update the things
            MyCurveRenderer.UpdateCurve(GetSelectedAudio(), true);
#endif
        }
#endregion
    }
}
