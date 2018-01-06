using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MakerGuiSystem;
using Zeltex.Util;
using Zeltex.Voxels;
using Zeltex.Skeletons;

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// Holds a list of animation files.
    /// </summary>
    [System.Serializable]
    public class SkeletonAnimation
    {
        public List<string> MyNames = new List<string>();    // each one is an animation file
        public List<string> MyData = new List<string>();    // each one is an animation file
    }
    /// <summary>
    /// Skeleton Manager. Handles gui to viewer interactions.
    /// Things to do:
    ///     - Clone Function
    ///     - Make Camera Zoom in on it - auto position function
    ///     - Panning camera x/y position
    /// Skeleton Data relies on:
    ///     Voxel Textures
    ///     Voxel Meta
    ///     Voxel Models
    ///     Possibly, Chunked Models
    ///     Possibly, Items
    /// </summary>
    public class SkeletonMaker : MakerGui
    {
        #region Variables
        [Header("Data")]
        //public List<SkeletonAnimation> MyAnimationData = new List<SkeletonAnimation>(); 
        public SkeletonViewer MyViewer;
        public Material GroundMaterial;
        private GameObject SpawnedGround;
        #endregion

        #region DataManager

        public new Skeleton GetSelected()
        {
            //Debug.LogError("Getting Selected: " + DataManagerFolder);
            return DataManager.Get().GetElement(DataManagerFolder, GetSelectedIndex()) as Skeletons.Skeleton;
        }

        /// <summary>
        /// Set file paths!
        /// </summary>
        protected override void SetFilePaths()
        {
            DataManagerFolder = DataFolderNames.Skeletons;
        }

        /// <summary>
        /// Set the name!
        /// </summary>
        public void SetName(int Index, string NewName)
        {
            DataManager.Get().SetName(DataManagerFolder, Index, NewName);
        }

        /// <summary>
        /// Add a new voxel model
        /// </summary>
        protected override void AddData()
        {
            //string NewSkeletonName = "S" + Random.Range(1, 10000);
            //string GeneratedSkeleton = Generators.SkeletonGenerator.Get().GenerateBasicSkeleton(NewSkeletonName);
            //DataManager.Get().Add(DataManagerFolder, NewSkeletonName, GeneratedSkeleton);
        }
        #endregion

        #region Data
        /// <summary>
        /// Internal Data Adding
        /// </summary>
        public void AddData(string MyName, string NewData)
        {
            /*for (int i = 0; i < MyNames.Count; i++)
            {
                if (MyNames[i] == MyName)
                {
                    return;
                }
            }
            MyData.Add(NewData);
            AddName(MyName);
            MyIndexController.SetMaxSelected(GetSize());*/
        }
        /// <summary>
        /// Create a new bone at skeleton position
        /// </summary>
        public void NewBone()
        {
            MyViewer.MySpawnedSkeleton.GetSkeleton().CreateBone();
            //GameObject NewBone = MyViewer.CreateBone();
        }
        /// <summary>
        /// Used for some methods
        /// </summary>
        //public static SkeletonMaker Get()
        //{
            //return GameObject.Find("GameManager").GetComponent<MapMaker>().MySkeletonManager;
        //}

        /// <summary>
        /// Clear the  all the stored data
        /// </summary>
        //public override void Clear()
        //{
            //MyNames.Clear();
            //MyData.Clear();
         //   MyAnimationData.Clear();
        //}

        /// <summary>
        /// Returns the script data for a certain race that holds a RaceName
        /// </summary>
        public List<string> GetData(string RaceName)
        {
            for (int i = 0; i < GetSize(); i++)
            {
                if (ScriptUtil.RemoveWhiteSpace(GetName(i)) == ScriptUtil.RemoveWhiteSpace(RaceName))   // just incase lel
                {
                    return GetData(i);
                }
            }
            if (GetSize() > 0)
            {
                GetData(0);
            }
            Debug.LogError("Error in skeleton Maker. Could not find " + RaceName);
            return new List<string>();
        }

        /// <summary>
        /// Returns the script data for a certain race that holds a RaceIndex
        /// </summary>
        public List<string> GetData(int RaceIndex)
        {
            if (RaceIndex >= 0 && RaceIndex < GetSize())
            {
                return GetData(GetName(RaceIndex));
            }
            else
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Gets an animation by a Skeleton name
        /// </summary>
        /*public SkeletonAnimation GetAnimation(string MyName)
        {
            for (int i = 0; i < GetSize(); i++)
            {
                if (MyName == GetName(i))
                {
                    return MyAnimationData[i];
                }
            }
            return null;
        }*/
        #endregion

        // index control 
        #region IndexController
        /// <summary>
        /// Load Gui with Input
        /// Only gets called if index is updated
        /// </summary>
        public override void OnUpdatedIndex(int NewIndex)
        {
            /*if (NewIndex != MyIndexController.GetOldIndex() && MyIndexController.GetOldIndex() >= 0 && MyIndexController.GetOldIndex() < GetSize())   // will not work when opening for first time
            {
                Debug.Log("Storing Skeleton as script to: " + MyIndexController.GetOldIndex() + " as moving to " + NewIndex);
                string SkeletonScript = FileUtil.ConvertToSingle(MyViewer.GetSpawn().GetComponent<Skeleton>().GetScriptList());
                DataManager.Get().Set(DataManagerFolder, MyIndexController.GetOldIndex(), SkeletonScript);
            }*/
            base.OnUpdatedIndex(NewIndex);
            //Debug.Log("Updating Index in Skeleton Maker: " + NewIndex);
            Load();
            GetInput("NameInput").text = GetSelectedName();
            CancelInvokes();
        }

		/// <summary>
		/// When an index is removed
		/// </summary>
		protected override void RemovedData(int Index)
		{
			base.RemovedData(Index);
            StartCoroutine(OnRemoveRoutine(Index));
        }

        /// <summary>
        /// Removes on a coroutine
        /// </summary>
        private IEnumerator OnRemoveRoutine(int NewIndex)
        {
			yield return MyViewer.MySpawnedSkeleton.GetSkeleton().ClearRoutine();
        }

        /// <summary>
        /// Called by ZelGui class when turning on gui
        /// </summary>
        public override void OnBegin()
        {
            StartCoroutine(OnBeginCoroutine());
        }

        /// <summary>
        /// needs to be in a coroutine
        /// </summary>
        private IEnumerator OnBeginCoroutine()
        {
            MyViewer.OnBegin();
            base.OnBegin();
            while (MyViewer.MySpawnedSkeleton.GetSkeleton().IsLoadingSkeleton())
            {
                // wait
                yield return null;
            }
        }

        /// <summary>
        /// Sets the selected data as the script
        /// </summary>
        private void UpdateSkeletonScript()
        {
            //if (MyViewer.MySpawnedSkeleton)
            {
                //string SkeletonScript = FileUtil.ConvertToSingle(MyViewer.MySpawnedSkeleton.GetSkeleton().GetScriptList());
                //DataManager.Get().Set(DataManagerFolder, GetSelectedIndex(), SkeletonScript);
            }
            /*else
            {
                Debug.LogError("NO skeleton spawned in skeletonMaker.");
            }*/
        }

        protected void OnDestroy()
        {
            UpdateSkeletonScript();
        }
        /// <summary>
        /// Called by ZelGui class when turning off gui
        /// </summary>
        public override void OnEnd()
        {
            UpdateSkeletonScript();
            base.OnEnd();
            MyViewer.OnEnd();
            GetToggle("GroundToggle").isOn = false;
            SetGround(false);
        }
        #endregion

        #region Files

        /// <summary>
        /// Load just one file! run the loaded script!
        /// </summary>
        public override void Load()
        {
            StopAllCoroutines();
            StartCoroutine(LoadRoutine());
        }

        /// <summary>
        /// Runs the script over time
        /// </summary>
        IEnumerator LoadRoutine()
        {
            if (MyViewer.MySpawnedSkeleton != null)// && GetSelectedIndex() >= 0 && GetSelectedIndex() < MyData.Count)
            {
                if (MyViewer.MySpawnedSkeleton.GetSkeleton() != null)
                {
                    MyViewer.MySpawnedSkeleton.GetComponent<SkeletonAnimator>().Stop();
                    MyViewer.MySpawnedSkeleton.GetSkeleton().ForceStopLoad();
                }
                if (GetSelected() != null)
                {
                    MyViewer.MySpawnedSkeleton.SetSkeletonData(GetSelected());
                    yield return UniversalCoroutine.CoroutineManager.StartCoroutine(GetSelected().ActivateRoutine());
                }

                MyViewer.RefreshCamera();
                //string MyScript = GetSelected();
                //Debug.LogError("Loading Skeleton: " + GetSelectedIndex() + ":\n" + MyScript);
                //yield return MyViewer.MySpawnedSkeleton.GetSkeleton().RunScriptRoutine(FileUtil.ConvertToList(MyScript)); //MyData[GetSelectedIndex()]
                //MyViewer.MySpawnedSkeleton.GetComponent<SkeletonAnimator>().LoadAll();  // takes the animations from the skeleton managers data
            }
            else
            {
                Debug.LogError("Skeleton load failed: " + GetSelectedIndex() + " / " + GetSize() + " - is null:" + (MyViewer.MySpawnedSkeleton == null));
            }
        }
        #endregion

        #region UI
        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(InputField MyInputField)
        {
            base.UseInput(MyInputField);
        }

        /// <summary>
        /// Used for generically updating buttons
        /// </summary>
        public override void UseInput(Button MyButton)
        {
            if (MyButton.name == "SaveButton")
            {
                //DataManager.Get().Set(DataFolderNames.Skeletons, GetSelectedIndex(), 
               //     FileUtil.ConvertToSingle(MyViewer.GetSpawn().GetComponent<Skeleton>().GetScriptList()));
            }
            base.UseInput(MyButton);
            if (MyButton.name == "ExportButton")
            {
                //GetSelected()
                //Debug.Log("Exporting Skeleton: " + GetSelectedName());
                //SetSelected(FileUtil.ConvertToSingle(MyViewer.GetSpawn().GetComponent<Skeleton>().GetScriptList()));    // make sure using latest
                //FileUtil.Export(GetSelectedName(), FileExtension, MyData[GetSelectedIndex()]);
            }
        }

        /// <summary>
        /// set ground in viewer
        /// </summary>
        public override void UseInput(Toggle MyToggle)
        {
            if (MyToggle.name == "GroundToggle")
            {
                SetGround(MyToggle.isOn);
            }
        }
        #endregion

        #region Extra

        public void Mutate()
        {
            for (int i = 0; i < MyViewer.MySpawnedSkeleton.GetBones().Count; i++)
            {
                //MyViewer.RescaleBone(i, Random.Range(-MutateValue, MutateValue));
            }
        }

        public void SetGround(bool NewState)
        {
            if (NewState)
            {
                SpawnedGround = (GameObject)GameObject.CreatePrimitive(PrimitiveType.Plane);
                SpawnedGround.layer = MyViewer.MySpawnedSkeleton.gameObject.layer;
                Bounds MyBounds = MyViewer.MySpawnedSkeleton.GetComponent<Skeleton>().GetBounds();
                SpawnedGround.transform.position = MyViewer.MySpawnedSkeleton.transform.position - new Vector3(0, MyBounds.center.y + MyBounds.extents.y);// -0.4f, 0);
                SpawnedGround.GetComponent<MeshRenderer>().material = GroundMaterial;
            }
            else
            {
                if (SpawnedGround != null)
                {
                    Destroy(SpawnedGround);
                }
            }
        }
        #endregion
    }
}

/// <summary>
/// Load all the polygonal models!
/// </summary>
/*public override void LoadAll()
    {
    //StopCoroutine(LoadAllRoutine()); // use this for now, that animations are seperate from skeletons
    //StartCoroutine(LoadAllRoutine()); // use this for now, that animations are seperate from skeletons
}

/// <summary>
/// Loads the skeletons in time
/// </summary>
public override IEnumerator LoadAllRoutine()
{
    yield break;
    if (DataManager.Get() == null)
    {
        base.LoadAll();
        Clear();
        string MyFolderPath = FileUtil.GetFolderPath(FolderName);
        List<string> MyFiles = FileUtil.GetFilesOfType(MyFolderPath, FileExtension);
        //Debug.Log("Loading all Polygonal Models: " + MyFolderPath + ":" + MyFiles.Count);
        MyFiles = FileUtil.SortAlphabetically(MyFiles);
        for (int i = 0; i < MyFiles.Count; i++)
        {
            //Debug.Log("Loading Skeleton with path: " + MyFiles[i]);
            AddName(Path.GetFileNameWithoutExtension(MyFiles[i]));
            MyData.Add(FileUtil.Load(MyFiles[i]));
            yield return new WaitForSeconds(0.05f); // pause after every loaded animation!
        }
    }

    if (gameObject.activeInHierarchy)
    {
        MyIndexController.RemovedOldIndex();
        OnBegin();  // refresh if already opened
    }
}

/// <summary>
/// Save all the polygonal models!
/// </summary>
public override void SaveAll()
{
    // now save our list to a save file

    Debug.Log("Saving Skeletons to: " + GetFilePath());
    for (int i = 0; i < GetSize(); i++)
    {
        string MyFilePath = GetFilePath() + GetName(i) + "." + FileExtension;
        //Debug.Log("Saving Poly " + i + " to: " + MyFilePath);
        FileUtil.Save(MyFilePath, FileUtil.ConvertToSingle(GetData(i)));
    }
}*/

/// <summary>
/// called on mouse down
/// </summary>
/* public void Import()
 {
     //FileUtil.Import(name, "UploadSkeleton", FileExtension);
 }

 public void UploadSkeletonFileName(string FileName)
 {
     UploadFileName = Path.GetFileNameWithoutExtension(FileName);
 }
 /// <summary>
 /// Called from javascript, uploading a model data
 /// </summary>
 public void UploadSkeleton(string MyData)
 {
     for (int i = 0; i < MyData.Length; i++)
     {
         if (MyData[i] == '\n')
         {
             UploadFileName = MyData.Substring(0, i);
             UploadFileName = Path.GetFileNameWithoutExtension(UploadFileName);
             MyData = MyData.Substring(i + 1);
             break;
         }
     }
     if (UploadFileName != "")
     {
         Debug.Log("Uploading new Skeleton: " + UploadFileName + "\n" + MyData.Length);
         AddData(UploadFileName, MyData);
         UploadFileName = "";
     }
 }*/
/// <summary>
/// Updates the name of a skeleton
/// </summary>
/*public void UpdateName(string NewName)
{
    if (GetNamesSize() > 0)  // if data file exists
    {
        if (NewName == "")
        {
            NewName = "Race"; // need to add (i) iterations to the names
        }
        if (GetName(GetSelectedIndex()) != NewName)
        {
            // Update Files, if file is open
            string SelectedName = GetName(GetSelectedIndex());
            string OldPath = GetFilePath(SelectedName);
            string NewPath = GetFilePath(NewName);
            if (File.Exists(OldPath))
            {
                File.Move(OldPath, NewPath);
                if (File.Exists(OldPath + ".meta"))
                {
                    File.Move(OldPath + ".meta", NewPath + ".meta");
                }
                // Find Animations for the skeleton
                string MyFolderPath = FileUtil.GetFolderPath(FolderName);
                var MyInfo = new DirectoryInfo(MyFolderPath);
                var MyFiles = MyInfo.GetFiles();
                for (int i = 0; i < MyFiles.Length; i++)
                {
                    string FileName = MyFiles[i].Name;
                    if (FileName.Length >= SelectedName.Length)
                    {
                        string StartString = FileName.Substring(0, SelectedName.Length);
                        string EndString = FileName.Substring(SelectedName.Length, FileName.Length - SelectedName.Length);
                        string NewAnimationPath = MyFolderPath + NewName + EndString;
                        //Debug.LogError("Checking " + FileName + " - to - " + NewAnimationPath);
                        if (FileName.Contains(StartString + "_Animation"))
                        {
                            File.Move(MyFiles[i].FullName, NewAnimationPath);
                        }
                    }
                }
            }
            // change name
            SetName(GetSelectedIndex(), NewName);
            MyViewer.GetSpawn().GetComponent<Skeleton>().SkeletonName = NewName;
        }
    }
}*/
