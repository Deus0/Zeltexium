using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine.Events;
using Zeltex.Util;
using Zeltex.Guis;

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// Checks the game if default map exists.
    /// If it doesn't it will download it.
    /// Contains functions to download a map from a server.
    /// </summary>
    public class DefaultMapChecker : MonoBehaviour
    {
        #region Variables
        string MainURL = "http://zeltex.000webhostapp.com/Zeltex/"; //"ftp://zeltex@files.000webhost.com/";//   //"files.000webhost.com";    //"http://localhost/Public/";
        public bool DebugGui;
        //public GameObject MyWorld;
        //public string DefaultMapWebsite;
        public MapMaker MyMapMaker;
        bool IsLoading;
        string FolderState = "";
        string FileState = "";
        string PathDescriptor = "";
        // UI
        //public ZelGui MainMenuGui;
        public ZelGui PreviousGui;
        public ZelGui LoadingGui;
        public Text GuiHeaderLabel;
        public Text HeaderLabel;
        public Text StateLabel;
        public Text SectionLabel;
        string ImageUploaderScript = "ImageUploader.php";
        string FileUploaderScript = "FileUploader.php"; // "MyUploader.php"
        List<string> MyFolderPaths = new List<string>();
        List<string> MyFileExtensions = new List<string>();
        #endregion

        // Use this for initialization
        /*void OnGUI()
        {
            if (DebugGui && IsLoading)
            {
                GUILayout.Label("Loading Default.." + MyLoadingState);
            }
            if (UploadSection != "")
                GUILayout.Label("[" + UploadSection + "]");
            if (UploadState != "" && UploadState != "Finished Updating")
                GUILayout.Label(UploadState);
        }*/

        void Update()
        {
            if (StateLabel && SectionLabel)
            {
                HeaderLabel.text = FolderState;
                StateLabel.text = FileState;
                SectionLabel.text = PathDescriptor;
            }
        }

        #region Paths
        void CreateDefaultFolderPaths()
        {
            MyFolderPaths = new List<string>();
            MyFileExtensions = new List<string>();
            for (int i = 0; i < MyMapMaker.MyMakers.Count; i++)
            {
                /*if (MyMapMaker.MyMakers[i].FolderName != "Error/")
                {
                    MyFolderPaths.Add(MyMapMaker.MyMakers[i].FolderName);
                    MyFileExtensions.Add(MyMapMaker.MyMakers[i].FileExtension);
                }
                else
                {
                    Debug.LogError("MakerGui Has an Error Folder: " + MyMapMaker.MyMakers[i].name);
                }*/
            }
        }
        #endregion
        //string MainURL = "http://zeltex.x10.bz/";
        //string UploadSection = "";
        //string UploadState = "";

        #region Gui
        private void OnBegin()
        {
            CreateDefaultFolderPaths();
            //LoadingHeaderLabel.text = "Uploading";
            PreviousGui.TurnOff();
            LoadingGui.TurnOn();
        }
        private void OnEnd()
        {
            LoadingGui.TurnOff();
            PreviousGui.TurnOn();
        }
        #endregion

        #region Uploading
        /// <summary>
        /// Upload the map!
        /// </summary>
        public void Upload()
        {
            GuiHeaderLabel.text = "Uploading";
            StartCoroutine(UploadDefaultMap());
        }

        private IEnumerator UploadDefaultMap()
        {
            OnBegin();
            yield return null;
            CreateDefaultFolderPaths();
            for (int i = 0; i < MyFolderPaths.Count; i++)
            {
                FolderState =   "[" + (i + 1) + " / " + MyFolderPaths.Count + "] - " +
                                "[" + MyFolderPaths[i] + ":" + MyFileExtensions[i] + "]";
                //MySection = (i + 1) + ": " + MyFolderPaths.Count + " [" + MyFolderPaths[i] + "]";//.Substring(0, MyFolderPaths[i].Length-1);
                if (MyFileExtensions[i] != "png")
                {
                    yield return UploadFolder(i);
                }
                else
                {
                    yield return UploadFolderImages(i);
                }
                //yield return new WaitForSeconds(0.5f);  // for our gui to show better
            }
            //MySection = "Zeltex";
            //FolderState = "Finished Updating";
            //yield return new WaitForSeconds(0.5f);
            OnEnd();
            yield break;
        }

        //public Texture2D MyTexture;
        private IEnumerator UploadFolderImages(int i )
        {
            //UploadState = "Beginning Upload";
            //yield return new WaitForSeconds(1f);
            string MyFolderPath = DataManager.GetFolderPath(MyFolderPaths[i]);
            List<string> MyFiles = FileUtil.GetFilesOfType(MyFolderPath, MyFileExtensions[i]);  //.Substring(1)
            Debug.LogError("UPloading from " + MyFolderPath + ":" + MyFiles.Count + ":" + MyFileExtensions[i]);
            for (int j = 0; j < MyFiles.Count; j++) //
            {
                FileState = "[" + (j+1) + "/" + MyFiles.Count + "] - [" + Path.GetFileName(MyFiles[j]) + "]";
                //Debug.LogError("Loading from: " + MyFiles[j]);
                //var MyTextureRequest = new WWW("file://" + MyFiles[j]);
                // wait until the download is done
                //yield return MyTextureRequest;
                byte[] TextureBytes = FileUtil.LoadBytes(MyFiles[j]);

                    // assign the downloaded image to the main texture of the object
                    var MyForm = new WWWForm();
                    // Upload Name
                    string DirectoryName = MapMaker.SaveFileName + "/" + MyFolderPaths[i];
                    string UploadURLName = DirectoryName + Path.GetFileName(MyFiles[j]);
                    Debug.LogError(j + " : " + MyFiles[j] + " : to : " + MainURL + UploadURLName);
                    MyForm.AddField("FileName", UploadURLName);  //
                    MyForm.AddField("DirectoryName", DirectoryName);
                    //MyTexture = MyTextureRequest.texture;
                    //byte[] MyImageBytes = MyTextureRequest.texture.EncodeToPNG();
                    MyForm.AddBinaryData(
                        "Data",
                        TextureBytes,
                        "image/png");
                    // Upload to a cgi script
                    WWW MyUploadRequest = new WWW(
                        MainURL + ImageUploaderScript,
                        MyForm);

                   // WWW MyRequest = new WWW(MainURL + "ImageUploader.php", MyForm);
                    //UploadState = "Uploading [" + Path.GetFileName(MyFiles[j]) + "]";
                    yield return MyUploadRequest;
                    if (!string.IsNullOrEmpty(MyUploadRequest.error))
                    {
                        Debug.LogError(MyUploadRequest.error);
                    }

            }
        }
        // https://zeltex.000webhostapp.com/Zeltex/FileUploader.php
        // https://zeltex.000webhostapp.com/?dir=Zeltex/FileUploader.php

        private IEnumerator UploadFolder(int i)
        {
           // UploadState = "Beginning Upload";
            //yield return new WaitForSeconds(1f);
            string MyFolderPath = DataManager.GetFolderPath(MyFolderPaths[i]);
            List<string> MyFiles = FileUtil.GetFilesOfType(MyFolderPath, MyFileExtensions[i]);//.Substring(1));
            //Debug.LogError("Uploading from " + MyFolderPath + ":" + MyFiles.Count + ":" + MyFileExtensions[i]);
            for (int j = 0; j < MyFiles.Count; j++) //
            {
                FileState = "[" + (j + 1) + "/" + MyFiles.Count + "] - [" + Path.GetFileName(MyFiles[j]) + "]";
                //MyState = "" + Path.GetFileName(MyFiles[j]) + "";
                var MyText = File.ReadAllText(MyFiles[j]);
                var MyForm = new WWWForm();
                // Upload Name
                string DirectoryName = MapMaker.SaveFileName + "/" + MyFolderPaths[i];
                string UploadURLName = DirectoryName + Path.GetFileName(MyFiles[j]);
                //Debug.LogError(j + " : " + MyFiles[j] + " : to : " + UploadURLName);
                MyForm.AddField("FileName", UploadURLName);
                MyForm.AddField("Data", MyText);
                MyForm.AddField("DirectoryName", DirectoryName);

                //Debug.LogError(j + " Creating Request: " + MainURL + FileUploaderScript);
                WWW MyRequest = new WWW(MainURL + FileUploaderScript, MyForm);
                //UploadState = "Uploading [" + Path.GetFileName(MyFiles[j]) + "]";
                yield return MyRequest;
                if (!string.IsNullOrEmpty(MyRequest.error))
                {
                    Debug.LogError(MyRequest.error);
                }
                else
                {
                   // Debug.LogError(MyRequest.text);
                }
            }
        }
        #endregion

        #region Downloading
        public void DownloadMap()
        {
            GuiHeaderLabel.text = "Downloading";
            StartCoroutine(LoadMap());
        }
        /// <summary>
        /// Loads the map from the URL
        /// </summary>
        private IEnumerator LoadMap()
        {
            yield return null;
            string MapPath = DataManager.GetFolderPath("");
            Debug.Log("Checking Directory " + MapPath);
            OnBegin();
            if (Directory.Exists(MapPath) == false)
            {
                Directory.CreateDirectory(MapPath);
            }
            for (int FolderIndex = 0; FolderIndex < MyFolderPaths.Count; FolderIndex++)
            {
               // string MyFolderName = MyFolderPaths[FolderIndex];// "Classes/";
                string FileDirectory = MapPath + MyFolderPaths[FolderIndex];
                string URLDirectory =  MapMaker.SaveFileName + "/" + MyFolderPaths[FolderIndex];    //MainURL + 
                Debug.Log("Downloading From: " + URLDirectory);
                //FolderState = FileDirectory + "=-=" + URLDirectory;
                FolderState ="[" + (FolderIndex + 1) + "/" + MyFolderPaths.Count + "] - " +
                    "[" + MyFolderPaths[FolderIndex] + ":" + MyFileExtensions[FolderIndex] + "] ";
                PathDescriptor = "[" + URLDirectory + "]" + "\n" + "[" + FileDirectory + "]";
                FileState = "Scanning Paths";
                yield return DownloadDirectory(FileDirectory, MyFileExtensions[FolderIndex], URLDirectory);
            }
            Debug.Log("Finished!");
            OnEnd();
        }
        /// <summary>
        /// File Directory - To save the files to the disk
        /// </summary>
        private IEnumerator DownloadDirectory(string FileDirectory, string FileExtension, string URLDirectory)
        {
            List<string> MyURLs = new List<string>();// GetFilesList(URLDirectory);
            // request files at directory
            var FileCheckerForm = new WWWForm();
            // Upload Name
            FileCheckerForm.AddField("DirectoryName", URLDirectory);
            // Upload to a cgi script
            WWW MyUploadRequest = new WWW(
                MainURL + "FileChecker.php",
                FileCheckerForm);
            yield return MyUploadRequest;
            if (!string.IsNullOrEmpty(MyUploadRequest.error))
            {
                Debug.LogError(MyUploadRequest.error);
            }
            else
            {
                //Debug.LogError("Success:" + MyUploadRequest.text);
                string[] MyLines = MyUploadRequest.text.Split('\n');
                MyURLs.AddRange(MyLines);
            }
            URLDirectory = MainURL + URLDirectory;

            //FileState = "Inside " + URLDirectory + ", Found " + MyURLs.Count + " Paths";
           // yield return new WaitForSeconds(1.5f);
            for (int i = MyURLs.Count - 1; i >= 0; i--)
            {
                if (MyURLs[i].Contains(FileExtension))//".txt"))
                {
                    //Debug.Log(i + " File found on site " + MyURLs[i]);
                    if (MyURLs[i][0] == ' ')
                    {
                        MyURLs[i] = MyURLs[i].Substring(1);
                    }
                   // DefaultMapWebsite = URLDirectory + MyURLs[i];
                }
                else
                {
                    MyURLs.RemoveAt(i);
                }
            }
            //MyLoadingState = ThisDirectory;
            Debug.Log("Downloading new Files at: " + URLDirectory + " And moving to " + FileDirectory);
            if (Directory.Exists(FileDirectory) == false)
            {
                Directory.CreateDirectory(FileDirectory);
            }
            for (int i = 0; i < MyURLs.Count; i++)
            {
                FileState = "[" + (i + 1) + "/" + MyURLs.Count + "] - [" + Path.GetFileName(MyURLs[i]) + "]";
                //FileState = MyURLs[i];    //ThisDirectory
                string URLPath = URLDirectory + MyURLs[i];
                string FilePath = FileDirectory + Path.GetFileName(MyURLs[i]);
                Debug.Log("Download file " + i + " from [" + URLDirectory + "]");
                PathDescriptor = "[" + URLPath + "]" + "\n=====\n" + "[" + FileDirectory + "]";
                WWW MyFile = new WWW(URLPath);
                yield return MyFile;
                if (MyFile.error != null)
                {
                    Debug.LogError("Error .. " + MyFile.error + " at " + MyURLs[i]);
                    // for example, often 'Error .. 404 Not Found'
                }
                else
                {
                    if (FileExtension != "png")
                    {
                        //Debug.Log("Saving file " + i + " to [" + FilePath + "]");
                        File.WriteAllText(FilePath, MyFile.text);
                    }
                    else
                    {
                        //Debug.Log("Saving Texture " + i + " to [" + FilePath + "]");
                        File.WriteAllBytes(FilePath, MyFile.texture.EncodeToPNG());
                    }
                }
            }
        }
        
        #endregion

        #region Utility
        /// <summary>
        /// Gets a list of files inside a URL
        /// </summary>
        List<string> GetFilesList(string MyURL)
        {
            List<string> MyFiles = new List<string>();
            WebRequest request = WebRequest.Create(MyURL);

            try
            {
                WebResponse response = request.GetResponse();
                Regex regex = new Regex("<a href=\".*\">(?<name>.*)</a>");

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string result = reader.ReadToEnd();

                    MatchCollection matches = regex.Matches(result);
                    if (matches.Count == 0)
                    {
                        //Debug.Log("parse failed.");
                        return MyFiles;
                    }

                    foreach (Match match in matches)
                    {
                        if (!match.Success) { continue; }
                        //Console.WriteLine(match.Groups["name"]);
                        MyFiles.Add(match.Groups["name"].Value);
                    }
                    return MyFiles;
                }
            }
            catch
            {
                Debug.LogError("Error with web url");
                return MyFiles;
            }
        }

        IEnumerator UploadScreenshot()
        {
            //Debug.LogError("Uploading Screenshot");
            // We should only read the screen after all rendering is complete
            yield return null;

            // Create a texture the size of the screen, RGB24 format
            int width = Screen.width;
            int height = Screen.height;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            // Read screen contents into the texture
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            // Encode texture into PNG
            byte[] MyImageBytes = tex.EncodeToPNG();
            tex.Die();

            // Create a Web Form
            WWWForm MyForm = new WWWForm();
            //MyForm.AddField("action", "Upload Image");
            //MyForm.AddField("MyFileName", "Screenshot.png");
            MyForm.AddBinaryData("fileUpload", 
                MyImageBytes,
                "image.png", 
                "image/png");

            // Upload to a cgi script
            WWW MyUploadRequest = new WWW(
                MainURL + "ImageUploader.php",
                MyForm);
            yield return MyUploadRequest;
            if (!string.IsNullOrEmpty(MyUploadRequest.error))
            {
                Debug.LogError(MyUploadRequest.error);
            }
            else {
                //Debug.LogError("Finished Uploading Screenshot");
            }
           // Debug.LogError("MyUploadRequest: " + MyUploadRequest.text);
        }
        #endregion
    }
}
/*Debug.LogError("Uploading File.");
string MyURLUpload = MainURL + "fileupload.php?txt=YourStringToBeAdded";
string mycontent = "test";
WWW www = new WWW(MyURLUpload); // + "?txt=mycontent"
yield return www;
Debug.LogError(www.uploadProgress);*/

/*MyFolderPaths.Add("BlockTextures/");
MyFileExtensions.Add(".png");
MyFolderPaths.Add("Classes/");
MyFileExtensions.Add(".txt");
MyFolderPaths.Add("Dialogues/");
MyFileExtensions.Add(".dlg");
MyFolderPaths.Add("Quests/");
MyFileExtensions.Add(".qst");
MyFolderPaths.Add("ItemMeta/");
MyFileExtensions.Add(".itm");
MyFolderPaths.Add("ItemTextures/");
MyFileExtensions.Add(".png");
MyFolderPaths.Add("Skeletons/");
MyFileExtensions.Add(".skl");
MyFolderPaths.Add("Skeletons/");
MyFileExtensions.Add(".anm");
MyFolderPaths.Add("VoxelMeta/");
MyFileExtensions.Add(".vmt");
MyFolderPaths.Add("PolyModel/");
MyFileExtensions.Add(".vmd");
MyFolderPaths.Add("PolyModels/");
MyFileExtensions.Add(".vxm");*/
