using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Util;
using System.IO;

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// Handles making blocks 
    /// Meta data saved in alphabetical order as blocks use their id to get meta from the VoxelTileGenerator
    /// </summary>
    public class MusicMaker : MakerGui
    {
        #region Variables
        //[Header("SoundMaker")]  // idk why i have 3 references.. lol
        public List<AudioClip> Data;
        //AudioClip
        #endregion

        #region Data
        public static SoundMaker Get()
        {
            return GameObject.Find("GameManager").GetComponent<MapMaker>().MySoundMaker;
        }
        /// <summary>
        /// Clear the  all the stored data
        /// </summary>
        public override void Clear()
        {
            //MyNames.Clear();
            Data.Clear();
        }

        /// <summary>
        /// Returns the selected script
        /// </summary>
        public AudioClip GetSelectedAudio()
        {
            return Data[MyIndexController.SelectedIndex];
        }

        public AudioClip GetAudio(string Name)
        {
            /*for (int i = 0; i < MyNames.Count; i++)
            {
                if (Name == MyNames[i])
                {
                    return Data[i];
                }
            }*/
            return null;
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
            //RefreshViewer();
            //GenerateSolidity();
            //UpdateStatistics();
            //CancelInvokes();
        }

		/// <summary>
		/// Add a new model to our list
		/// </summary>
		protected override void AddData()
		{
            //AudioClip MyClip = new AudioClip();
			//DataManager.Get().AddSound(DataManagerFolder, MyClip);
            //Data.Add(MyClip);
            //MyNames.Add("Sound " + MyNames.Count);
            //base.OnAdd(NewIndex);
        }

		/// <summary>
		/// When an index is removed
		/// </summary>
		/// <param name="NewIndex"></param>
		protected override void RemovedData(int Index)
		{
			//DataManager.Get().RemoveSound(DataManagerFolder, Index);
        }

        #endregion

        #region Files
        /// <summary>
        /// Set file paths!
        /// </summary>
        protected override void SetFilePaths()
        {
            DataManagerFolder = "Music";
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
        }

        /// <summary>
        /// Used for generically updating buttons
        /// </summary>
        public override void UseInput(Button MyButton)
        {
            if (MyButton.name == "ImportButton")
            {
                StartCoroutine(ImportRoutine());
            }
            else if (MyButton.name == "PlayButton")
            {
                GetComponent<AudioSource>().clip = (GetSelectedAudio());
                GetComponent<AudioSource>().Play();
            }
            else if (MyButton.name == "StopButton")
            {
                GetComponent<AudioSource>().Stop();
            }
        }
        IEnumerator ImportRoutine()
        {
            /*Dropdown MyDropdown = GetDropdown("ImportDropdown");
            string FileName = FileUtil.GetFolderPath("MusicImports/") + MyDropdown.options[MyDropdown.value].text + "." + FileExtension;
            WWW MyLoader = new WWW("file://" + FileName);
            yield return MyLoader;
            if (string.IsNullOrEmpty(MyLoader.error) == false)
            {
                Debug.LogError(MyLoader.error);
            }
            else
            {
                Data[GetSelectedIndex()] = MyLoader.audioClip;
            }*/
            yield break;
        }
        /// <summary>
        /// Fill the drop downs with data
        /// </summary>
        public override void FillDropdown(Dropdown MyDropdown)
        {
            /*List<string> MyDropdownNames = new List<string>();
            if (MyDropdown.name == "ImportDropdown")
            {
                List<string> MyFiles = FileUtil.GetFilesOfType(FileUtil.GetFolderPath("MusicImports/"), FileExtension);
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
/// <summary>
/// Load all the polygonal models!
/// </summary>
/* public override void LoadAll()
 {
     base.LoadAll();
     Clear();
     //StartCoroutine(LoadRoutine());
 }
 public IEnumerator LoadRoutine()
 {
     yield break;
     base.LoadAll();
     Clear();
     string MyFolderPath = FileUtil.GetFolderPath(FolderName);
     List<string> MyFiles = FileUtil.GetFilesOfType(MyFolderPath, FileExtension);
     Debug.Log("Loading all Musics: " + MyFolderPath + ":" + MyFiles.Count);
     MyFiles = FileUtil.SortAlphabetically(MyFiles);
     for (int i = 0; i < MyFiles.Count; i++)
     {
         Debug.Log(i + " Loading Music: " + MyFiles[i]);
         MyNames.Add(Path.GetFileNameWithoutExtension(MyFiles[i]));
         WWW MyLoader = new WWW("file://" + MyFiles[i]);
         yield return MyLoader;
         Data.Add(MyLoader.audioClip);
     }
 }
 /// <summary>
 /// Save all the polygonal models!
 /// </summary>
 public override void SaveAll()
 {
     Debug.Log("Saving Sounds to: " + GetFilePath());
     for (int i = 0; i < Data.Count; i++)
     {
         string MyFilePath = GetFilePath() + MyNames[i] + "." + FileExtension;
         Debug.Log("Saving Music " + i + " to: " + MyFilePath);
         SavWav.Save(MyFilePath, Data[i]);
     }
 }*/
