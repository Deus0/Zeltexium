using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using Zeltex.Guis;
using Zeltex.Util;
using Zeltex.AnimationUtilities;
using MakerGuiSystem;
using Zeltex.Voxels;

namespace Zeltex.Guis.Player
{
    /// <summary>
    /// The gui handler used in map selection.
    /// Remember to add ForceRefresh, Stop Refresh functions onto the ZelGuis   
    /// </summary>
   public class MapSelectGui : GuiList
    {
        /* #region Variables
        static string DebugMapListFolder;
        bool IsRefreshingList = false;
        List<string> MyMaps = new List<string>();
        List<string> LastGatheredMaps = new List<string>();
        public Text TooltipLabel;
        [Header("References")]
        public World MyWorld;
        public MapMaker MyMapMaker;

        public GameObject ConfirmButton;
        public GameObject NewWorldButton;
        public GameObject RemoveWorldButton;
        public GameObject ConnectLabel;
        #endregion

        #region UI
        /// <summary>
        /// Called by ZelGuis on begin
        /// </summary>
        public void OnBegin()
        {
            Clear();
            LastGatheredMaps.Clear();
            ConnectLabel.SetActive(true);
            NewWorldButton.SetActive(false);
            SetButtons(false);
            StopCoroutine(BeginOnTimer());
            StartCoroutine(BeginOnTimer());
        }
        /// <summary>
        /// Begins the gui in the timer
        /// </summary>
        /// <returns></returns>
        IEnumerator BeginOnTimer()
        {
            while (PhotonNetwork.masterClient == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            ConnectLabel.SetActive(false);
            NewWorldButton.SetActive(true);
            ForceRefresh();
        }
        /// <summary>
        /// Sets the buttons for the gui
        /// </summary>
        /// <param name="IsSelect"></param>
        public void SetButtons(bool IsSelect)
        {
            if (IsSelect)
            {
                if (MyMaps.Count == 0)
                    DeSelect();
                if (!IsGuiSelected()) // if turning them on make sure its selected
                    return;
            }
            ConfirmButton.SetActive(IsSelect);
            RemoveWorldButton.SetActive(IsSelect);
            if (IsSelect)
            {
                Debug.Log("OnSelected");
                RefreshTooltip();
            }
        }

        /// <summary>
        /// Refreshes the map tooltip
        /// </summary>
        void RefreshTooltip()
        {
            SpeechAnimator MyTextAnimator = TooltipLabel.gameObject.GetComponent<SpeechAnimator>();
            if (MyTextAnimator == null)
            {
                MyTextAnimator = TooltipLabel.gameObject.AddComponent<SpeechAnimator>();
            }
            MyTextAnimator.NewLine(GetToolTip());
        }

        /// <summary>
        /// A tooltip of the map, describing all the data counts
        /// </summary>
        public string GetToolTip()
        {
            string MyTooltipText = "";
            MyTooltipText = "[" + SelectedName + "]";
            //Vector3 MyWorldSize = MakerGuiSystem.MapMaker.GetMapSize(SelectedName);
            //MyTooltipText += "\n" + "WorldSize \n\t" + MyWorldSize.ToString();
            int CharacterCount = GetFolderCount(SelectedName, "Characters/", "chr");// GetCharactersCount(SelectedName);
            MyTooltipText += "\n" + "Characters " + CharacterCount;
            int ChunksCount = GetChunksCount(SelectedName);
            MyTooltipText += "\n" + "Chunks " + ChunksCount;

            int ClassesCount = GetFolderCount(SelectedName, "Classes/", "txt");
            int ItemMetaCount = GetFolderCount(SelectedName, "ItemMeta/", "itm");
            int VoxelMetaCount = GetFolderCount(SelectedName, "VoxelMeta/", "vmt");
            MyTooltipText += "\n" + "[MetaData]";
            MyTooltipText += "\n" + "Classes " + ClassesCount;
            MyTooltipText += "\n" + "Items " + ItemMetaCount;
            MyTooltipText += "\n" + "Blocks " + VoxelMetaCount;

            int ItemTexturesCount = GetFolderCount(SelectedName, "ItemTextures/", "png");
            int BlockTexturesCount = GetFolderCount(SelectedName, "BlockTextures/", "png");
            MyTooltipText += "\n" + "[ArtData]";
            MyTooltipText += "\n" + "Item Textures " + ItemTexturesCount;
            MyTooltipText += "\n" + "Block Textures " + BlockTexturesCount;

            int SkeletonsCount = GetFolderCount(SelectedName, "Skeletons/", "skl");
            int PolyModels = GetFolderCount(SelectedName, "PolyModel/", "vmd");
            int VoxelModels = GetFolderCount(SelectedName, "PolyModels/", "vxm");
            MyTooltipText += "\n" + "Block Models " + PolyModels;
            MyTooltipText += "\n" + "Skeletons " + SkeletonsCount;
            MyTooltipText += "\n" + "Models " + VoxelModels;
            return MyTooltipText;
        }

        #endregion

        #region GuiList

        // Handles updating and only update what has changed!
        void CheckEvents()
        {
            OnActivateEvent.RemoveAllListeners();
			//OnActivateEvent.AddEvent(delegate { LoadWorld(); });
		}

		public override void Select(int NewSelectedIndex)
		{
            base.Select(NewSelectedIndex);
            SetButtons(true);
        }

        override public void RefreshList()
        {
            StartRefresh();
        }

        override public void StartRefresh()
        {
            if (!IsRefreshingList)
            {
                IsRefreshingList = true;
                CheckEvents();
                StopCoroutine(OnGoingRefresh());
                StartCoroutine(OnGoingRefresh());
            }
        }

        override public void StopRefresh()
        {
            StopCoroutine(OnGoingRefresh());
            IsRefreshingList = false;
        }

        /// <summary>
        /// on going rrefresh of characters in map while this list is open
        /// </summary>
        IEnumerator OnGoingRefresh()
        {
            SetButtons(false);    // turn of select buttons
            //yield return new WaitForSeconds(0.5f);
            //Zeltex.Voxels.World MyWorld = GameObject.FindObjectOfType<Zeltex.Voxels.World>();
            while (transform.parent.gameObject.activeSelf)
            {
                //MyCharacters = GetCharactersListWorld(MyWorld);
                MyMaps = GetMapsListSorted();
                if (!FileUtil.AreListsTheSame(MyMaps, LastGatheredMaps))
                {
                    //Debug.LogError (Time.time + " Refreshing Character List.");
                    SetButtons(false);
                    DeSelect();
                    Clear();
                    for (int i = 0; i < MyMaps.Count; i++)
                    {
                        yield return new WaitForSeconds(0.025f);
                        Add(MyMaps[i]);
                    }
                    SetButtons(true);
                }
                LastGatheredMaps = MyMaps;
                yield return new WaitForSeconds(15f);
            }
            IsRefreshingList = false;
        }
        #endregion

        #region Files
        /// <summary>
        /// Removes the map from the list and deletes the files
        /// </summary>
        public void RemoveMap()
        {
            if (SelectedIndex != -1)
            {
                string MyWorldName = GetSelectedName();
                DeSelect();
                // delete from file
                DeleteMapFiles(MyWorldName);
                // refresh list
                ForceRefresh();
            }
        }

        /// <summary>
        /// Deletes the map files
        /// Put this in MapMakerGui
        /// </summary>
        /// <param name="WorldName"></param>
        public static void DeleteMapFiles(string WorldName)
        {  // make sure gui list updates
            string DirectoryPath = FileUtil.GetResourcesPath() + WorldName + "/";
            Debug.Log("Deleting Map at path: " + DirectoryPath);
            if (Directory.Exists(DirectoryPath))
            {
                DirectoryInfo MyFileDirectory = new DirectoryInfo(DirectoryPath);
                var MyFiles = MyFileDirectory.GetFiles();
                for (int i = 0; i < MyFiles.Length; i++)
                {
                    //Debug.LogError("Deleting file: " + MyFiles [i].FullName);
                    File.Delete(MyFiles[i].FullName);
                }
                var MySubDirectories = MyFileDirectory.GetDirectories();
                for (int i = 0; i < MySubDirectories.Length; i++)
                {

                    var MySubDirectoriesFiles = MySubDirectories[i].GetFiles();
                    for (int j = 0; j < MySubDirectoriesFiles.Length; j++)
                    {
                        //Debug.LogError("Deleting file: " + MyFiles [i].FullName);
                        File.Delete(MySubDirectoriesFiles[j].FullName);
                    }
                    Directory.Delete(MySubDirectories[i].FullName);
                }
                Directory.Delete(DirectoryPath);
                string MyMetaPath = DirectoryPath.Remove(DirectoryPath.Length - 1) + ".meta";
                //Debug.LogError ("Checking for [" + MyMetaPath + "]");
                if (File.Exists(MyMetaPath))
                {
                    File.Delete(MyMetaPath);
                }
            }
        }
        /// <summary>
        /// returns the type of file in a folder
        /// </summary>
        int GetFolderCount(string MyWorldName, string FolderName, string Extension)
        {
            string FolderPath = FileUtil.GetWorldFolderPath() + MyWorldName + "/";
            FolderPath += FolderName;
            if (Directory.Exists(FolderPath))
            {
                return FileUtil.GetFilesOfType(FolderPath, Extension).Count;
            }
            else
            {
                return 0;
            }
        }

        int GetCharactersCount(string MyWorldName)
        {
            string FileName = FileUtil.GetWorldFolderPath() + MyWorldName + "/";
            return FileUtil.GetFilesOfType(FileName, "chr").Count;
        }

        int GetChunksCount(string MyWorldName)
        {
            string FileName = FileUtil.GetWorldFolderPath() + MyWorldName + "/";
            return FileUtil.GetFilesOfType(FileName, "dat").Count;
        }

        /// <summary>
        ///  sorts by last written
        /// </summary>
        /// <returns></returns>
        public static List<string> GetMapsListSorted()
        {
            List<string> MyMaps = new List<string>();
            string MyMapthsFilePath = FileUtil.GetWorldFolderPath();
            DebugMapListFolder = MyMapthsFilePath;
            //Debug.LogError("MyMapthsFilePath: " + MyMapthsFilePath);
            var info = new DirectoryInfo(MyMapthsFilePath);
            var MyDirectories = info.GetDirectories();
            List<DirectoryInfo> MyMaps2 = new List<DirectoryInfo>();
            for (int i = 0; i < MyDirectories.Length; i++)
            {
                if (MyDirectories[i].Name != "Default")
                {
                    MyMaps2.Add(MyDirectories[i]);
                }
            }
            while (MyMaps2.Count != 0)
            {
                // find earliest time
                System.DateTime LatestUpdateTime = MyMaps2[MyMaps2.Count - 1].LastWriteTime;
                int LatestTimeIndex = MyMaps2.Count - 1;
                for (int i = MyMaps2.Count - 2; i >= 0; i--)
                {
                    System.DateTime ThisTime = MyMaps2[i].LastWriteTime;
                    if (LatestUpdateTime < ThisTime)
                    {
                        LatestUpdateTime = ThisTime;
                        LatestTimeIndex = i;
                    }
                }
                // add it to list, remove directory
                MyMaps.Add(MyMaps2[LatestTimeIndex].Name);
                MyMaps2.RemoveAt(LatestTimeIndex);
            }
            return MyMaps;
        }

        #endregion*/
    }
}