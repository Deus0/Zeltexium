using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;
using Zeltex.Voxels;
using System.Collections.Generic;
using Zeltex.Items;
using Zeltex.Util;
using Zeltex.Characters;
using Zeltex;
#if UNITY_EDITOR || UNITY_STANDALONE
using Ionic.Zip;
#endif

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// Map Maker handles the Map Editing. 
    /// It communicates with all the other gui tools.
    /// Add Create map to the MapMaker Gui ZelGui
    /// </summary>
    public class MapMaker : MonoBehaviour
    {
        #region Variables
        public static string SaveFileName = "Default";
        public static string MapFolderPath = "";
        public static bool IsPersistent = true;
        [Header("Debug")]
        public bool IsDebugGui;
        public bool IsLoadOnAwake = true;
        [Header("References")]
        public World MyWorld;
        // World Meta Data
        public bool IsPersistentPath = true;
        [Header("Makers")]
        public List<MakerGui> MyMakers = new List<MakerGui>();
        public TextureMaker MyTextureManager;
        public ClassMaker MyClassEditor;
        public DialogueMaker MyDialogueMaker;
        public QuestMaker MyQuestMaker;
        public ItemManager MyItemManager;
        public ItemMaker MyItemMaker;
        public BlockMaker MyBlockMaker;
        public StatsMaker MyStatsMaker;
        public SpellMaker MySpellsMaker;
        public RecipeMaker MyRecipeMaker;
        // Complex
        public PolygonMaker MyPolygonMaker;
        public ModelMaker MyModelMaker;
        public SkeletonMaker MySkeletonManager;
        public AnimationTimeline MyAnimationsManager;
        public SoundMaker MySoundMaker;
        [Header("UI")]
        public Text MapFolderText;
        public Text MapMetaInfoText;
        [Header("Events")]
        public UnityEvent OnLoadMap;
        public UnityEvent OnUnloadMap;
        public GameObject SpawnZone;
        public UnityEvent OnMapEmpty;  // Generate map data here
        // privates
        //string LoadStatus = "";
        [Header("Painters")]
        public VoxelPainterGui MyVoxelPainter;
        public SkeletonPainter MySkeletonPainter;
        public TexturePainter MyTexturePainter;
        int TotalFiles;
        #endregion

        #region Mono

        void OnLevelWasLoaded(int level)
        {
            Debug.Log("MapMaker OnLevelWasLoaded " + level);
        }

        void OnGui()
        {
            if (IsDebugGui)
            {
                List<string> DebugList = GetDataInfo();
                for (int i = 0; i < DebugList.Count; i++)
                {
                    GUILayout.Label(DebugList[i]);
                }
            }
        }
        #endregion

        #region GettersAndSetters

        /// <summary>
        /// Gets a list of data info for the map loaded
        /// </summary>
        public List<string> GetDataInfo()
        {
            TotalFiles = 0;
            List<string> MyData = new List<string>();
            MyData.Add("Resources Pack [" + SaveFileName + "]");
            //MyData.Add("[Meta]");
            MyData.Add("Classes [" + MyClassEditor.GetSize() + "]");
            TotalFiles += MyClassEditor.GetSize();
            MyData.Add("Items [" + MyItemMaker.GetSize() + "]");
            TotalFiles += MyItemMaker.GetSize();
            MyData.Add("Voxels [" + MyBlockMaker.GetSize() + "]");
            TotalFiles += MyBlockMaker.GetSize();
            MyData.Add("Dialogues [" + MyDialogueMaker.GetSize() + "]");
            TotalFiles += MyDialogueMaker.GetSize();
            MyData.Add("Quests [" + MyQuestMaker.GetSize() + "]");
            TotalFiles += MyQuestMaker.GetSize();
            MyData.Add("Stats [" + MyStatsMaker.GetSize() + "]");
            TotalFiles += MyStatsMaker.GetSize();
            MyData.Add("Spells [" + MySpellsMaker.GetSize() + "]");
            TotalFiles += MySpellsMaker.GetSize();
            MyData.Add("Recipes [" + MyRecipeMaker.GetSize() + "]");
            TotalFiles += MyRecipeMaker.GetSize();
            MyData.Add("Events [" + 0 + "]");
            //TotalFiles += 0;

            //MyData.Add("[2d art]");
            //MyData.Add("Block Textures [" + MyTextureManager.VoxelDiffuseTextures.Count + "]");
           // TotalFiles += MyTextureManager.VoxelDiffuseTextures.Count;
            //MyData.Add("Item Textures [" + MyItemManager.MyTextures.Count + "]");

            //MyData.Add("[3d art]");
            MyData.Add("Polygon Models [" + MyPolygonMaker.GetSize() + "]");    // VoxelModelMaker
            TotalFiles += MyPolygonMaker.GetSize();
            MyData.Add("Voxel Models [" + MyModelMaker.GetSize() + "]");  // VoxelEditorManager
            TotalFiles += MyModelMaker.GetSize();
            MyData.Add("Skeletons [" + MySkeletonManager.GetSize() + "]");
            TotalFiles += MySkeletonManager.GetSize();

            MyData.Add("[Sound]");
            MyData.Add("Music [" + 0 + "]");
            TotalFiles += 0;
            MyData.Add("Effects [" + MySoundMaker.GetSize() + "]");
            TotalFiles += MySoundMaker.GetSize();
            MyData.Add("Total Files [" + TotalFiles + "]");
            MyData.Add("From Path [" + MapFolderPath + "]");
            return MyData;
        }
        #endregion

        #region Other
        public void RandomizeSpawnPosition()
        {
            if (SpawnZone)
            {
                SpawnZone.GetComponent<SpawnPositionFinder>().IsRandom = true;
            }
        }

        /// <summary>
        /// unloads the map
        /// </summary>
        public void UnLoad()
        {
            //Debug.LogError("Unloading.");
            //SaveFileName = "None";
            // Clear Level
            MyWorld.SetWorldSize(new Vector3(0, 0, 0)); // Clears the map
            CharacterManager.Get().Clear();             // also clear characters
            // Clear world items!
            // Clear zones!
            // Clear the loaded data! unload the resources pack!
            Clear();
            OnUnloadMap.Invoke();
            //SetLights(false);
        }

        public void Delete()
        {
            Debug.Log("[MapMaker: Deleting Map");
            // Meta
            MyBlockMaker.Delete();
            MyItemMaker.Delete();
            MyDialogueMaker.Delete();
            MyQuestMaker.Delete();
            MyStatsMaker.Delete();
            MySpellsMaker.Delete();
            MyRecipeMaker.Delete();
            MyClassEditor.Delete();
            // Art
            MyTextureManager.Delete();
            MyPolygonMaker.Delete();  // the polys?
            MyModelMaker.Delete();    // chunked models?
            MySkeletonManager.Delete();    // loads animations too
            MySoundMaker.Delete();
            OnUpdatedMap();
        }
        /// <summary>
        /// To make sure i have statistics up to date of the map
        /// </summary>
        private void RefreshListeners()
        {
            for (int i = 0; i < MyMakers.Count; i++)
            {
                if (MyMakers[i])
                {
                    MyMakers[i].OnUpdateSize.RemoveAllListeners();
                    MyMakers[i].OnUpdateSize.AddEvent(OnUpdatedMap);
                }
            }
            //Debug.Log("[MapMaker: RefreshListeners");
            /*MyBlockMaker.OnUpdateSize.RemoveAllListeners();
            MyBlockMaker.OnUpdateSize.AddEvent(OnUpdatedMap);
            MyItemMaker.OnUpdateSize.RemoveAllListeners();
            MyItemMaker.OnUpdateSize.AddEvent(OnUpdatedMap);
            MyDialogueMaker.OnUpdateSize.RemoveAllListeners();
            MyDialogueMaker.OnUpdateSize.AddEvent(OnUpdatedMap);
            MyQuestMaker.OnUpdateSize.RemoveAllListeners();
            MyQuestMaker.OnUpdateSize.AddEvent(OnUpdatedMap);
            MyStatsMaker.OnUpdateSize.RemoveAllListeners();
            MyStatsMaker.OnUpdateSize.AddEvent(OnUpdatedMap);
            MySpellsMaker.OnUpdateSize.RemoveAllListeners();
            MySpellsMaker.OnUpdateSize.AddEvent(OnUpdatedMap);
            MyRecipeMaker.OnUpdateSize.RemoveAllListeners();
            MyRecipeMaker.OnUpdateSize.AddEvent(OnUpdatedMap);
            MyClassEditor.OnUpdateSize.RemoveAllListeners();
            MyClassEditor.OnUpdateSize.AddEvent(OnUpdatedMap);
            MyTextureManager.OnUpdateSize.RemoveAllListeners();
            MyTextureManager.OnUpdateSize.AddEvent(OnUpdatedMap);
            MyPolygonMaker.OnUpdateSize.RemoveAllListeners();
            MyPolygonMaker.OnUpdateSize.AddEvent(OnUpdatedMap);
            MyModelMaker.OnUpdateSize.RemoveAllListeners();
            MyModelMaker.OnUpdateSize.AddEvent(OnUpdatedMap);
            MySkeletonManager.OnUpdateSize.RemoveAllListeners();
            MySkeletonManager.OnUpdateSize.AddEvent(OnUpdatedMap);
            MySoundMaker.OnUpdateSize.RemoveAllListeners();
            MySoundMaker.OnUpdateSize.AddEvent(OnUpdatedMap);*/
        }
        /// <summary>
        /// Clears the managers
        /// </summary>
        public void Clear()
        {
            RefreshListeners();
            MyBlockMaker.Clear();
            MyItemMaker.Clear();
            MyDialogueMaker.Clear();
            MyQuestMaker.Clear();
            MyStatsMaker.Clear();
            MySpellsMaker.Clear();
            MyRecipeMaker.Clear();
            MyClassEditor.Clear();
            MyTextureManager.Clear();
            MyPolygonMaker.Clear();  // the polys?
            MyModelMaker.Clear();    // chunked models?
            MySkeletonManager.Clear();    // loads animations too
            MySoundMaker.Clear();
            OnUpdatedMap();
        }
        /// <summary>
        /// Save everything! this is never used..
        /// </summary>
        public void SaveAll()
        {
            // directory
            // string DirectoryPath = FileUtil.GetWorldFolderPath() + WorldName.text;
            // Meta
            MyBlockMaker.SaveAll();
            MyItemMaker.SaveAll();
            MyDialogueMaker.SaveAll();
            MyQuestMaker.SaveAll();
            MyStatsMaker.SaveAll();
            MySpellsMaker.SaveAll();
            MyRecipeMaker.SaveAll();
            MyClassEditor.SaveAll();
            // Art
            MyTextureManager.SaveAll();
            MyPolygonMaker.SaveAll();
            MyModelMaker.SaveAll();
            MySkeletonManager.SaveAll();
            MySoundMaker.SaveAll();
            OnUpdatedMap();
        }

        public void OnCancel()
        {
            UseDefaultPaths();
        }
        void UseDefaultPaths()
        {
            //IsUsingWorldPath = false;
            SaveFileName = "Default";
        }
        void UseWorldPath()
        {
            //IsUsingWorldPath = true;
        }
        #endregion

        #region UI


        private void SetMapFolderText(string NewText)
        {
            if (MapFolderText)
            {
                MapFolderText.text = NewText;
            }
        }
        public void OnUpdatedMap()
        {
            if (MapFolderText)
            {
                MapFolderText.text = "Loaded [" + SaveFileName + "]";
            }
            if (MapMetaInfoText)
            {
                MapMetaInfoText.text = FileUtil.ConvertToSingle(GetDataInfo());
            }
        }
        #endregion

        #region ExportMap
        public void CompressAll()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            OnUpdatedMap();
            string ZipFilePath = DataManager.GetMapPath() + SaveFileName + ".zip";
            Debug.Log("Saving Zip File: " + ZipFilePath);
            using (ZipFile MyZip = new ZipFile())
            {
                /*CompressFolder(MyZip, MyBlockMaker.GetFilePath(), MyBlockMaker.FolderName, MyBlockMaker.FileExtension);
                CompressFolder(MyZip, MyItemMaker.GetFilePath(), MyItemMaker.FolderName, MyItemMaker.FileExtension);
                CompressFolder(MyZip, MyDialogueMaker.GetFilePath(), MyDialogueMaker.FolderName, MyDialogueMaker.FileExtension);
                CompressFolder(MyZip, MyQuestMaker.GetFilePath(), MyQuestMaker.FolderName, MyQuestMaker.FileExtension);
                CompressFolder(MyZip, MyStatsMaker.GetFilePath(), MyStatsMaker.FolderName, MyStatsMaker.FileExtension);
                CompressFolder(MyZip, MySpellsMaker.GetFilePath(), MySpellsMaker.FolderName, MySpellsMaker.FileExtension);
                CompressFolder(MyZip, MyRecipeMaker.GetFilePath(), MyRecipeMaker.FolderName, MyRecipeMaker.FileExtension);
                CompressFolder(MyZip, MyClassEditor.GetFilePath(), MyClassEditor.FolderName, MyClassEditor.FileExtension);

                CompressFolder(MyZip, MyPolygonMaker.GetFilePath(), MyPolygonMaker.FolderName, MyPolygonMaker.FileExtension);
                CompressFolder(MyZip, MyModelMaker.GetFilePath(), MyModelMaker.FolderName, MyModelMaker.FileExtension);
                CompressFolder(MyZip, MySkeletonManager.GetFilePath(), MySkeletonManager.FolderName, MySkeletonManager.FileExtension);
                CompressFolder(MyZip, MySoundMaker.GetFilePath(), MySoundMaker.FolderName, MySoundMaker.FileExtension);
                // Textures
                CompressFolder(MyZip, MyTextureManager.GetTextureFolderPath(0), MyTextureManager.GetTextureFolderName(0), MyTextureManager.FileExtension);
                CompressFolder(MyZip, MyTextureManager.GetTextureFolderPath(1), MyTextureManager.GetTextureFolderName(1), MyTextureManager.FileExtension);
                CompressFolder(MyZip, MyTextureManager.GetTextureFolderPath(2), MyTextureManager.GetTextureFolderName(2), MyTextureManager.FileExtension);
                CompressFolder(MyZip, MyTextureManager.GetTextureFolderPath(3), MyTextureManager.GetTextureFolderName(3), MyTextureManager.FileExtension);*/
                // add the report into a different directory in the archive
                MyZip.Save(ZipFilePath);
            }
            string MyZipScript = System.Convert.ToBase64String(File.ReadAllBytes(ZipFilePath));
            Debug.Log("Exporting Map: " + SaveFileName + " as zip [" + MyZipScript.Length + "]");
            FileUtil.ExportImage(SaveFileName, "zip", MyZipScript);
#endif
        }
#if UNITY_EDITOR || UNITY_STANDALONE
        private void CompressFolder(ZipFile MyZip, string FilePath, string FolderName, string FileExtension)
        {
            List<string> FilePaths = FileUtil.GetFilesOfType(FilePath, FileExtension);
            Debug.Log("Compressing folder: " + FolderName + " with " + FilePaths.Count + " Files of type " + FileExtension);
            for (int i = 0; i < FilePaths.Count; i++)
            {
                MyZip.AddFile(FilePaths[i], FolderName);
            }
        }
#endif

        public void Import()
        {
            FileUtil.Import(name, "Upload", "zip");
        }

        public void Upload(string MyData)
        {
            string ZipFilePath = DataManager.GetMapPath() + SaveFileName + ".zip";
            string UploadFileName = "";
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
                Debug.Log("Unity Uploading Map: " + UploadFileName + " - [" + MyData.Length + "]");
                byte[] MyBytes = System.Convert.FromBase64String(MyData);
                File.WriteAllBytes(ZipFilePath, MyBytes);
#if UNITY_WEBGL
            Application.ExternalCall("SyncFiles");
#endif
                //LoadThing();
                /*using (ZipInputStream stream = new ZipInputStream(ZipFilePath))
                {
                    ZipEntry e;
                    while ((e = stream.GetNextEntry()) != null)
                    {
                        //if (e.FileName.ToLower().EndsWith(".cs") ||
                        //    e.FileName.ToLower().EndsWith(".xaml"))
                        {
                            //var ms = new MemoryStream();
                            //e.Extract(ms);
                            var sr = new StreamReader(stream);
                            {
                                //ms.Position = 0;
                                CodeFiles.Add(new CodeFile() { Content = sr.ReadToEnd(), FileName = e.FileName });
                            }
                        }
                    }
                }*/
            }
        }
        /* public void LoadThing()
         {
             float BeginTime = Time.realtimeSinceStartup;
             string ZipFilePath = FileUtil.GetMapPath() + SaveFileName + ".zip";
             string OutputPath = FileUtil.GetMapPath();
             Debug.Log("Extracting from ZipFilePath: " + ZipFilePath + " to " + OutputPath);// + " - DoesFileExist? " + File.Exists(ZipFilePath));
 #if UNITY_WEBGL && UNITY_EDITOR == false
             UnzipUsingSharpZip(ZipFilePath);
             Application.ExternalCall("SyncFiles");
 #else
             Debug.Log("Unzipping using dotnetzip");
             using (ZipFile MyZip = ZipFile.Read(ZipFilePath))
             {
                 //MyZip.ExtractAll(OutputPath, ExtractExistingFileAction.OverwriteSilently);//);//, ExtractExistingFileAction.OverwriteSilently);
                 foreach (ZipEntry MyZipEntry in MyZip)
                 {
                     if (Time.realtimeSinceStartup - BeginTime >= 5)
                     {
                         Debug.Log("Over Time 5 seconds.. ");
                         break;
                     }
                     Debug.Log("Reading ZIp Entry: " + MyZipEntry.ToString());
                     MyZipEntry.Extract(OutputPath, ExtractExistingFileAction.OverwriteSilently);
                 }
             }
 #endif
         }
         /// <summary>
         /// Sharpzip works in browser!
         /// </summary>
         /// <param name="ZipFilePath"></param>
         private void UnzipUsingSharpZip(string ZipFilePath)
         {
             using (ICSharpCode.SharpZipLib.Zip.ZipInputStream s = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(File.OpenRead(ZipFilePath)))
             {
                 ICSharpCode.SharpZipLib.Zip.ZipEntry theEntry;
                 while ((theEntry = s.GetNextEntry()) != null)
                 {
                     //Debug.Log(theEntry.Name);

                     string directoryName = Path.GetDirectoryName(theEntry.Name);
                     string fileName = Path.GetFileName(theEntry.Name);

                     // create directory
                     if (directoryName.Length > 0)
                     {
                         Directory.CreateDirectory(directoryName);
                     }

                     if (fileName != "")
                     {
                         string filename = Path.GetDirectoryName(ZipFilePath) + "/";    // DefaultVoxelMeta/Air.vmt
                         filename += theEntry.Name;
                         Debug.Log("Unzipping: " + filename + "-" + Path.GetDirectoryName(ZipFilePath) + ":" + theEntry.Name);
                         using (FileStream streamWriter = File.Create(filename))
                         {
                             int size = 2048;
                             byte[] fdata = new byte[2048];
                             while (true)
                             {
                                 size = s.Read(fdata, 0, fdata.Length);
                                 if (size > 0)
                                 {
                                     streamWriter.Write(fdata, 0, size);
                                 }
                                 else
                                 {
                                     break;
                                 }
                             }
                         }
                     }
                 }
         }*/
    }
    #endregion
}
