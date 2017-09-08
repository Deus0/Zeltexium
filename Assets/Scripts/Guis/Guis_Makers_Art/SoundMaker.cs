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
    public class SoundMaker : MakerGui
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
            return DataManager.Get().GetSizeElements(DataManagerFolder);
        }
        #endregion

        #region Data
        /*public static SoundMaker Get()
        {
            return GameObject.Find("GameManager").GetComponent<MapMaker>().MySoundMaker;
        }*/
        /// <summary>
        /// Clear the  all the stored data
        /// </summary>
        public override void Clear()
        {
           // MyNames.Clear();
            //Data.Clear();
        }

        /// <summary>
        /// Returns the selected script
        /// </summary>
        public AudioClip GetSelectedAudio()
        {
            return null;
            //return DataManager.Get().GetSound(DataManagerFolder, GetSelectedIndex());
        }

        public AudioClip GetAudio(string Name)
        {
            return null;
            //return DataManager.Get().GetSound(DataManagerFolder, Name);
        }


        public void SetSelected(AudioClip MyAudioClip)
        {
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
            if (GetSelectedAudio() != null)
            {
                OnUpdatedAudioClip();
			}
			// quick hack fixes dropdown menus!
			ScrollRect MyScrollRect = GetDropdown("FunctionTypeDropdown").transform.Find("Template").GetComponent<ScrollRect>();	//
			MyScrollRect.content = MyScrollRect.transform.Find("Viewport").Find("Content").GetComponent<RectTransform>();
		}

		private void OnUpdatedAudioClip()
        {
            GetInput("TimeInput").text = "" + GetSelectedAudio().length;
            MyCurveRenderer.UpdateCurve(GetSelectedAudio(), true);
            GetLabel("SamplesLabel").text = "Samples\n[" + GetSelectedAudio().samples + "]";
            gameObject.GetComponent<AudioSource>().clip = GetSelectedAudio();
        }

        /// <summary>
        /// Add a new model to our list
        /// </summary>
        protected override void AddData()
        {
			AudioClip MyClip = AudioClip.Create(Zeltex.NameGenerator.GenerateVoxelName(), 256, 1, 440, false);
			//DataManager.Get().AddElement(DataManagerFolder, MyClip);
		}

		/// <summary>
		/// When an index is removed
		/// </summary>
		protected override void RemovedData(int Index)
		{
            //DataManager.Get().RemoveSound(DataManagerFolder, Index);
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
            if (MyButton.name == "ImportButton")
            {
                StartCoroutine(Import());
            }
            else if (MyButton.name == "PlayButton")
            {
                //Debug.LogError("Playing sound: " + GetSelectedAudio().name);
                GetComponent<AudioSource>().PlayOneShot(GetSelectedAudio());
            }
            else if (MyButton.name == "RecordButton")
            {

#if UNITY_WEBGL
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
#if UNITY_WEBGL
// Find a webgl solution for recording audio
#else
            Microphone.End("Built-in Microphone");
            SetSelected(GetSelectedAudio()); // update the things
            MyCurveRenderer.UpdateCurve(GetSelectedAudio(), true);
#endif
        }

        IEnumerator Import()
        {
            yield break;
            /*Dropdown MyDropdown = GetDropdown("ImportDropdown");
            string FileName = FileUtil.GetFolderPath("SoundImports/") + MyDropdown.options[MyDropdown.value].text + "." + FileExtension;
            WWW MyWavLoader = new WWW("file://" + FileName);
            yield return MyWavLoader;
            // Create an audioclip from the path
            SetSelected(MyWavLoader.audioClip);
            MyCurveRenderer.UpdateCurve(MyWavLoader.audioClip, true);*/
        }
        /// <summary>
        /// Fill the drop downs with data
        /// </summary>
        public override void FillDropdown(Dropdown MyDropdown)
        {
            /*List<string> MyDropdownNames = new List<string>();
            if (MyDropdown.name == "ImportDropdown")
            {
                List<string> MyFiles = FileUtil.GetFilesOfType(FileUtil.GetFolderPath("SoundImports/"), FileExtension);
                for (int i = 0; i < MyFiles.Count; i++)
                {
                    MyDropdownNames.Add(Path.GetFileNameWithoutExtension(MyFiles[i]));
                }
                FillDropDownWithList(MyDropdown, MyDropdownNames);
            }*/
        }
#endregion
    }
}
