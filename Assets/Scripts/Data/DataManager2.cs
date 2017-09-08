using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zeltex.Generators;
using Zeltex.Util;
using Zeltex.Combat;
using System.Reflection;

namespace Zeltex
{

    /// <summary>
    /// Generic class to load data from a file
    /// Initializes in editor too
    /// </summary>
    public partial class DataManager : ManagerBase<DataManager>
    {
        #region Debug
        private int MapNameSelected = 0;
        private List<string> MapNames = null;
        private Vector2 scrollPosition;
        private string OpenedFolderName = "";
        private int OpenedFolderIndex = -1;

        private string OpenedFileName = "";
        private int OpenedFileIndex = -1;
        private Element OpenedElement;
        private bool IsDrawAllFields;
        //private bool IsDrawStatistics;

        void OnGUI()
        {
            if (IsDebugGui)
            {
                if (GUILayout.Button("Close"))
                {
                    IsDebugGui = false;
                }
                DrawGui();
            }
        }

        public void DrawGui()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            if (IsLoaded)
            {
                GUILayout.Label("Loaded [" + MapName + "]");
            }
            else
            {
                GUILayout.Label("Selected [" + MapName + "]");
            }
            //IsJSONFormat = GUILayout.Toggle(IsJSONFormat, "JSON");
            if (IsLoaded)
            {
                LoadedDataGui();
            }
            else
            {
                NotLoadedGui();
            }
            GUILayout.EndScrollView();
        }

        public void NotLoadedGui()
        {
            if (GUILayout.Button("Open Folder"))
            {
                FileUtil.OpenPathInWindows(GetResourcesPath());
            }
            GUILayout.Label("MapName:");
            if (MapName == null)
            {
                Debug.LogError("MapName is null.");
                MapName = "";
            }
            if (GUILayout.Button("Load"))
            {
                LoadAll();
            }
            if (MapNames == null || GUILayout.Button("Refresh List"))
            {
                RefreshGuiMapNames();
            }
            GUILayout.Label("PathType: " + MyFilePathType);
            //MyFilePathType = (FilePathType)int.Parse(GUILayout.TextField(((int)MyFilePathType).ToString()));
            GUI.enabled = (MyFilePathType != FilePathType.Normal);
            if (GUILayout.Button("Normal"))
            {
                MyFilePathType = FilePathType.Normal;
                RefreshGuiMapNames();
            }
            GUI.enabled = (MyFilePathType != FilePathType.PersistentPath);
            if (GUILayout.Button("Persistent"))
            {
                MyFilePathType = FilePathType.PersistentPath;
                RefreshGuiMapNames();
            }
            GUI.enabled = (MyFilePathType != FilePathType.StreamingPath);
            if (GUILayout.Button("Streaming"))
            {
                MyFilePathType = FilePathType.StreamingPath;
                RefreshGuiMapNames();
            }
            GUI.enabled = true;

            GUILayout.Space(10);
            int NewMapNameSelected = GUILayout.SelectionGrid(MapNameSelected, MapNames.ToArray(), 1);
            if (MapNameSelected != NewMapNameSelected)
            {
                MapNameSelected = NewMapNameSelected;
                MapName = MapNames[MapNameSelected];
            }
            //MapName = GUILayout.TextField(MapName);
            /*if (GUILayout.Button("Refresh"))
            {
                MyResourceNames = Guis.Maker.ResourcesMaker.GetResourceNames();
            }*/
        }

        private void RefreshGuiMapNames()
        {
            MapNames = Zeltex.Guis.Maker.ResourcesMaker.GetResourceNames();
            if (MapNames.Count == 0)
            {
                MapNameSelected = 0;
                MapName = "";
            }
            else
            {
                MapNameSelected = Mathf.Clamp(MapNameSelected, 0, MapNames.Count - 1);
                MapName = MapNames[MapNameSelected];
            }
            OpenedFileName = "";
            OpenedFileIndex = -1;
            OpenedFolderIndex = -1;
            OpenedFolderName = "";
        }

        private void LoadedDataGui()
        {
            // Info for current map
            if (GUILayout.Button("Unload"))
            {
                // go back!
                UnloadAll();
            }
            if (RenameName == "Null")
            {
                RenameName = MapName;
            }

            if (OpenedFileName != "")
            {
                GUILayout.Label("Loaded [" + RenameName + "]");
                DrawFile();
            }
            else if (OpenedFolderIndex != -1)
            {
                GUILayout.Label("Loaded [" + RenameName + "]");
                DrawSelectedFolder();
            }
            else
            {
                RenameName = GUILayout.TextField(RenameName);
                if (GUILayout.Button("Rename"))
                {
                    if (RenameName != MapName)
                    {
                        RenameResourcesFolder(RenameName);
                    }
                }

                if (GUILayout.Button("Open Folder"))
                {
                    FileUtil.OpenPathInWindows(GetMapPath());
                }
                if (GUILayout.Button("Save"))
                {
                    SaveAll();
                }
                if (GUILayout.Button("Erase"))
                {
                    DeleteAll();
                }
                if (GUILayout.Button("Generate"))
                {
                    GameObject.Find("Generators").GetComponent<MapGenerator>().GenerateMap();
                }
                if (GUILayout.Button("Generate TileMap"))
                {
                    Voxels.VoxelManager.Get().GenerateTileMap();
                }

                DrawFolders();
                /*IsDrawStatistics = GUILayout.Toggle(IsDrawStatistics, "Statistics");
                if (IsDrawStatistics)
                {
                    List<string> MyStatistics = GetStatisticsList();
                    for (int i = 0; i < MyStatistics.Count; i++)
                    {
                        GUILayout.Label(MyStatistics[i]);
                    }
                    if (MyStatistics.Count == 0)
                    {
                        GUILayout.Label("Error, no stats.");
                    }
                }*/
            }
        }

        #region Folders
        private Zexel OpenedTexture;
        private bool IsOpenedElementFolder;

        private void DrawSelectedFolder()
        {
            GUILayout.Space(30);
            GUILayout.Label("Opened Folder [" + OpenedFolderName + "] " + DataFolderNames.GetDataType(OpenedFolderName).ToString());
            if (GUILayout.Button("Open " + OpenedFolderName))
            {
                string FolderPath = GetMapPath() + OpenedFolderName + "/";
                if (System.IO.Directory.Exists(FolderPath) == false)
                {
                    System.IO.Directory.CreateDirectory(FolderPath);
                }
                FileUtil.OpenPathInWindows(FolderPath);
            }
            // Show folder files
            if (GUILayout.Button("Close"))
            {
                OpenedFolderIndex = -1;
                OpenedFolderName = "";
            }
            else
            {
                if (GUILayout.Button("Revert"))
                {
                    RevertFolder(OpenedFolderName);
                }
                if (GUILayout.Button("Save"))
                {
                    if (IsOpenedElementFolder)
                    {
                        ElementFolder MyFolder = GetElementFolder(OpenedFolderName);
                        if (MyFolder != null)
                        {
                            MyFolder.SaveAllElements();
                        }
                    }
                    else
                    {
                        Debug.LogError("TODO: Save Textures");
                    }
                }
                if (GUILayout.Button("New"))
                {
                    if (IsOpenedElementFolder)
                    {
                        ElementFolder MyFolder = GetElementFolder(OpenedFolderName);
                        if (MyFolder != null)
                        {
                            MyFolder.AddNewElement();
                        }
                    }
                    else
                    {
                        /*DataFolder<Texture2D> MyFolder = GetTextureFolder(OpenedFolderName);
                        if (MyFolder != null)
                        {
                            Texture2D NewTexture = new Texture2D(
                                (int)Voxels.VoxelManager.Get().GetTextureSize().x,
                                (int)Voxels.VoxelManager.Get().GetTextureSize().y,
                                TextureFormat.RGBA32, false);
                            NewTexture.filterMode = FilterMode.Point;
                            NewTexture.wrapMode = TextureWrapMode.Clamp;
                            NewTexture.name = NameGenerator.GenerateVoxelName();
                            MyFolder.Add(NewTexture.name, NewTexture);
                        }*/
                    }
                }
                if (!IsOpenedElementFolder)
                {
                    if (GUILayout.Button("Import"))
                    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                        if (HasOpenedZexels())
                        {
                            ImportZexel();
                        }
#else
                        Debug.LogError("Platform not supported.");
#endif
                        /*#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                                DataFolder<Element> MyFolder = GetElementFolder(OpenedFolderName);
                                if (MyFolder != null)
                                {
                                    System.Windows.Forms.OpenFileDialog MyDialog = new System.Windows.Forms.OpenFileDialog();
                                    System.Windows.Forms.DialogResult MyResult = MyDialog.ShowDialog();
                                    if (MyResult == System.Windows.Forms.DialogResult.OK)
                                    {
                                        byte[] bytes = FileUtil.LoadBytes(MyDialog.FileName);
                                        Texture2D NewTexture = new Texture2D(
                                        (int)Voxels.VoxelManager.Get().GetTextureSize().x,
                                        (int)Voxels.VoxelManager.Get().GetTextureSize().y,
                                        TextureFormat.RGBA32, false);
                                        NewTexture.filterMode = FilterMode.Point;
                                        NewTexture.wrapMode = TextureWrapMode.Clamp;
                                        NewTexture.LoadImage(bytes);// as Texture2D;
                                        NewTexture.name = System.IO.Path.GetFileNameWithoutExtension(MyDialog.FileName);
                                        MyFolder.Add(NewTexture.name, NewTexture);
                                    }
                                    else
                                    {
                                        Debug.LogError("Failure to open file.");
                                    }
                                }
                        #else
                                Debug.LogError("Platform not supported.");
                        #endif*/
                    }
                }
                GUILayout.Space(30);
                DrawFolderFiles();
            }
        }

        private void RevertFolder(string FolderName)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null)
            {
                MyFolder.Revert();
            }
            //DataFolder<Texture2D> MyFolder2 = GetTextureFolder(FolderName);
            //if (MyFolder2 != null)
            {
                //MyFolder2.Revert(IsJSONFormat);
            }
        }

        private void DrawFolderFiles()
        {
            if (IsOpenedElementFolder)
            {
                int ElementCount = 0;
                try
                {
                    foreach (KeyValuePair<string, Element> MyKeyValuePair in ElementFolders[OpenedFolderIndex].Data)
                    {
                        if (GUILayout.Button(MyKeyValuePair.Key))
                        {
                            // open file
                            OpenedElement = MyKeyValuePair.Value;
                            OpenedFileName = MyKeyValuePair.Key;
                            OpenedFileIndex = ElementCount;
                            if (HasOpenedZexels())
                            {
                                OpenedTexture = MyKeyValuePair.Value as Zexel;
                            }
                        }
                        ElementCount++;
                    }
                }
                catch (System.ObjectDisposedException e)
                {

                }
            }
        }

        private void DrawFolders()
        {
            // Show folders
            GUILayout.Space(30);
            GUILayout.Label("Folders: " + ElementFolders.Count);
            for (int i = 0; i < ElementFolders.Count; i++)
            {
                if (GUILayout.Button(ElementFolders[i].FolderName + " [" + GetSize(ElementFolders[i].FolderName) + "]"))
                {
                    OpenedFolderName = ElementFolders[i].FolderName;
                    OpenedFolderIndex = i;
                    IsOpenedElementFolder = true;
                }
            }
            /*for (int i = 0; i < TextureFolders.Count; i++)
            {
                if (GUILayout.Button(TextureFolders[i].FolderName))
                {
                    OpenedFolderName = TextureFolders[i].FolderName;
                    OpenedFolderIndex = i;
                    IsOpenedElementFolder = false;
                }
            }*/
        }

        private List<string> GetStatisticsList()
        {
            List<string> MyStatistics = new List<string>();
            int TotalCount = StringFolders.Count +ElementFolders.Count;   //SpellFolders.Count + ItemFolders.Count + StatFolders.Count +  TextureFolders.Count + AudioFolders.Count + 
            MyStatistics.Add("DataManager -:- Folders: " + TotalCount);
            for (int i = 0; i < StringFolders.Count; i++)
            {
                MyStatistics.Add("String[" + i + "]: " + StringFolders[i].FolderName + ":" + StringFolders[i].Data.Count);
            }
            /*for (int i = 0; i < AudioFolders.Count; i++)
            {
                MyStatistics.Add("Audio[" + i + "]: " + AudioFolders[i].FolderName + ":" + AudioFolders[i].Data.Count);
            }
            
             */
            for (int i = 0; i < ElementFolders.Count; i++)
            {
                MyStatistics.Add("Elements[" + i + "]: " + ElementFolders[i].FolderName + ":" + ElementFolders[i].Data.Count);
            }
            //MyStatistics.Add("-Voxel Manager-");
            //MyStatistics.Add("Voxel Models: " + Voxels.VoxelManager.Get().MyModels.Count);
            //MyStatistics.Add("Voxel Meta: " + Voxels.VoxelManager.Get().MyMetas.Count);
            return MyStatistics;
        }

        private void RenameResourcesFolder(string NewName)
        {
            string OldFolderPath = GetResourcesPath() + MapName + "/";
            string NewFolderPath = GetResourcesPath() + NewName + "/";
            if (System.IO.Directory.Exists(NewFolderPath) == false)
            {
                System.IO.Directory.Move(OldFolderPath, NewFolderPath);
                MapName = NewName;
            }
            else
            {
                Debug.Log("Cannot move to " + NewFolderPath + " as already exists.");
            }
        }
        #endregion

        #region DrawFiles

        private bool HasOpenedZexels()
        {
            return DataFolderNames.GetDataType(OpenedFolderName) == typeof(Zexel);
        }

        private void DrawFile()
        {
            GUILayout.Label("Opened Folder [" + OpenedFolderName + "]");
            GUILayout.Space(30);
            if (IsOpenedElementFolder)
            {
                GUILayout.Label("Opened File [" + OpenedFileName + "] - Type [" + OpenedElement.GetType() + "]");
            }
            else
            {
                GUILayout.Label("Opened File [" + OpenedFileName + "] - Type [" + OpenedTexture.GetType() + "]");
            }
            if (GUILayout.Button("Close"))
            {
                OpenedFileName = "";
            }
            else
            {
                if (IsOpenedElementFolder)
                {
                    GUILayout.Label(OpenedElement.Name + " - [" + OpenedElement.CanSave().ToString() + "]");
                }
                if (GUILayout.Button("ForceSave"))
                {
                    if (IsOpenedElementFolder)
                    {
                        OpenedElement.OnModified();
                        OpenedElement.Save();
                    }
                }
                if (GUILayout.Button("Save"))
                {
                    if (IsOpenedElementFolder)
                    {
                        OpenedElement.Save();
                    }
                }
                if (GUILayout.Button("Revert"))
                {
                    if (IsOpenedElementFolder)
                    {
                        OpenedElement = RevertElement(OpenedElement);
                    }
                }
                if (GUILayout.Button("Delete"))
                {
                    Debug.LogError("TODO:  DElETE");
                    //OpenedElement.Delete();
                }
                if (OpenedElement.GetType() == typeof(Zexel))
                {
                    GUILayout.Space(30);
                    GUILayout.Label("Zexel");
                    // buttons

                    if (GUILayout.Button("Import"))
                    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                        ImportZexel();
#else
                        Debug.LogError("Platform not supported.");
#endif
                    }
                }
                else if (OpenedElement.GetType() == typeof(Voxels.VoxelModel))
                {
                    GUILayout.Space(30);
                    GUILayout.Label("VoxelModel");
                    if (GUILayout.Button("Import"))
                    {
                        ImportPolygon(OpenedFileIndex);
                    }
                }
                else if (OpenedElement.GetType() == typeof(Voxels.WorldModel))
                {
                    GUILayout.Space(30);
                    GUILayout.Label("WorldModel");
                    if (GUILayout.Button("Import"))
                    {
                        UniversalCoroutine.CoroutineManager.StartCoroutine(LoadVoxFile((OpenedElement as Voxels.WorldModel)));
                    }
                    GUILayout.Label(OpenedElement.Name + " - Size: " + (OpenedElement as Voxels.WorldModel).VoxelData.Length);// + ":" + (OpenedElement as Voxels.WorldModel).VoxelData);
                }
                else if(OpenedElement.GetType() == typeof(Sound.Zound))
                {
                    GUILayout.Space(30);
                    GUILayout.Label("Zound: " + (OpenedElement as Sound.Zound).GetSize());
                    // buttons

                    if (GUILayout.Button("Import"))
                    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                        ImportZound(OpenedFolderName, OpenedElement as Sound.Zound);
#else
                        Debug.LogError("Platform not supported.");
#endif
                    }

                    if (GUILayout.Button("Play"))
                    {
                        LatestPlayed = (OpenedElement as Sound.Zound).GetAudioClip();
                        PlayClip(LatestPlayed);
                    }
                }

                GUILayout.Space(30);
                IsDrawAllFields = GUILayout.Toggle(IsDrawAllFields, "IsDrawAllFields");
                DrawFieldsForObject(OpenedElement as object);
                if (HasOpenedZexels() == true && OpenedTexture != null)
                {
                    GUILayout.Label("Size: " + OpenedTexture.GetWidth() + " : " + OpenedTexture.GetHeight());
                    GUILayout.Label("IsNull? [" + (OpenedTexture.GetTexture() != null) + "]");
                    GUILayout.Space(30);
                    Rect OtherRect = GUILayoutUtility.GetRect(new GUIContent("Blargnugg"), GUI.skin.button);
                    Rect MyRect = new Rect(0, 0, OpenedTexture.GetWidth() * 4, OpenedTexture.GetHeight() * 4);
                    MyRect.x = OtherRect.width / 2f - OpenedTexture.GetWidth() * 2f;
                    MyRect.y = OtherRect.y;
                    int BorderSize = 20;
                    Rect BackgroundRect = new Rect(MyRect.x - BorderSize, MyRect.y - BorderSize,
                        MyRect.width + BorderSize * 2f, MyRect.height + BorderSize * 2f);
                    GUI.color = Color.gray;
                    GUI.DrawTexture(BackgroundRect, Texture2D.whiteTexture);
                    GUI.color = Color.white;
                    GUI.DrawTexture(MyRect, OpenedTexture.GetTexture());
                    //GUI.Label(MyRect, OpenedTexture);
                }
            }
        }
        public AudioClip LatestPlayed;

        public void PlayClip(AudioClip clip)
        {
            if (Application.isEditor && Application.isPlaying == false)
            {
                UniversalCoroutine.CoroutineManager.StopAllCoroutines();
                UniversalCoroutine.CoroutineManager.StartCoroutine(PlayClipInEditor(clip));
            }
            else
            {
                gameObject.GetComponent<AudioSource>().PlayOneShot(clip);
            }
        } // PlayClip()

        private System.Collections.IEnumerator PlayClipInEditor(AudioClip clip)
        {
            Debug.LogError("Load audio? " + clip.LoadAudioData().ToString()
               + " : " + clip.loadState.ToString() + " : " + clip.loadType.ToString());
            while (clip.loadState == AudioDataLoadState.Loading)
            {
                yield return null;
            }
#if UNITY_EDITOR
            Assembly unityEditorAssembly = typeof(UnityEditor.AudioImporter).Assembly;
            System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "PlayClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new System.Type[] {
                typeof(AudioClip)
                },
                null
            );
            method.Invoke(
                null,
                new object[] {
                clip
                }
            );
#endif
        }
        private void ImportZexel()
        {
            ImportImage(OpenedFolderName, OpenedTexture);
        }

        public void ImportPolygon(int FileIndex)
        {
            //ElementFolder MyFolder = GetElementFolder(FolderName);
            //if (MyFolder != null)
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            {
                System.Windows.Forms.OpenFileDialog MyDialog = new System.Windows.Forms.OpenFileDialog();
                System.Windows.Forms.DialogResult MyResult = MyDialog.ShowDialog();
                if (MyResult == System.Windows.Forms.DialogResult.OK)
                {
                    Mesh MyMesh = ObjImport.ImportFile(MyDialog.FileName);
                    //byte[] bytes = FileUtil.LoadBytes(MyDialog.FileName);
                    //MyZexel.LoadImage(bytes);
                }
                else
                {
                    Debug.LogError("Failure to open file.");
                }
            }
#endif
        }

#region ImportVox
        private Int3 VoxelIndex = Int3.Zero();
        private int VoxelIndex2 = 0;
        private Int3 VoxelIndex3 = Int3.Zero();
        private Color VoxelColor;
        private string ImportVoxelData = "";
        
        public System.Collections.IEnumerator LoadVoxFile(Voxels.WorldModel MyModel)
        {
            yield return UniversalCoroutine.CoroutineManager.StartCoroutine(LoadVoxFile());
            MyModel.VoxelData = ImportVoxelData;
            OpenedElement.OnModified();
        }

        public System.Collections.IEnumerator LoadVoxFile(Voxels.World SpawnedWorld = null)
        {
            yield return null;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            System.Windows.Forms.OpenFileDialog MyDialog = new System.Windows.Forms.OpenFileDialog();
            System.Windows.Forms.DialogResult MyResult = MyDialog.ShowDialog();
            string FilePath = MyDialog.FileName;
            if (MyResult == System.Windows.Forms.DialogResult.OK
                && FilePath != null && FilePath.Length > 0)
            {
                MVMainChunk MyVoxelMainChunk = MVImporter.LoadVOX(FilePath, null);
                if (MyVoxelMainChunk != null)
                {
                    //if (SpawnedWorld != null)
                    {
                        Debug.Log("Loading world from .Vox: " + MyVoxelMainChunk.voxelChunk.sizeX + ", " + MyVoxelMainChunk.voxelChunk.sizeY + ", " + MyVoxelMainChunk.voxelChunk.sizeZ);
                        Int3 NewWorldSize = new Int3(
                            Mathf.CeilToInt(MyVoxelMainChunk.voxelChunk.sizeX / Voxels.Chunk.ChunkSize) + 1,
                            Mathf.CeilToInt(MyVoxelMainChunk.voxelChunk.sizeY / Voxels.Chunk.ChunkSize) + 1,
                            Mathf.CeilToInt(MyVoxelMainChunk.voxelChunk.sizeZ / Voxels.Chunk.ChunkSize) + 1);
                        Voxels.VoxelData Data = null;
                        if (SpawnedWorld)
                        {
                            yield return SpawnedWorld.SetWorldSizeRoutine(NewWorldSize);
                            while (SpawnedWorld.IsWorldLoading())
                            {
                                yield return null;
                            }
                        }
                        else
                        {
                            // Create new voxel Data here
                            Data = new Voxels.VoxelData(
                                Voxels.Chunk.ChunkSize * Mathf.CeilToInt((float) MyVoxelMainChunk.voxelChunk.sizeX / Voxels.Chunk.ChunkSize),
                                Voxels.Chunk.ChunkSize * Mathf.CeilToInt((float)MyVoxelMainChunk.voxelChunk.sizeY / Voxels.Chunk.ChunkSize),
                                Voxels.Chunk.ChunkSize * Mathf.CeilToInt((float)MyVoxelMainChunk.voxelChunk.sizeZ / Voxels.Chunk.ChunkSize));
                        }
                        for (VoxelIndex.x = 0; VoxelIndex.x < MyVoxelMainChunk.voxelChunk.sizeX; ++VoxelIndex.x)
                        {
                            for (VoxelIndex.y = 0; VoxelIndex.y < MyVoxelMainChunk.voxelChunk.sizeY; ++VoxelIndex.y)
                            {
                                for (VoxelIndex.z = 0; VoxelIndex.z < MyVoxelMainChunk.voxelChunk.sizeZ; ++VoxelIndex.z)
                                {
                                    VoxelIndex2 = (int)MyVoxelMainChunk.voxelChunk.voxels[VoxelIndex.x, VoxelIndex.y, VoxelIndex.z];
                                    //VoxelIndex3.Set(VoxelIndex.x - MyVoxelMainChunk.voxelChunk.sizeX / 2,
                                    //    VoxelIndex.y, VoxelIndex.z - MyVoxelMainChunk.voxelChunk.sizeZ / 2);
                                    if (VoxelIndex2 > 0)
                                    {
                                        //Debug.Log(MyVoxelMainChunk.voxelChunk.voxels[x, y, z].ToString());
                                        // minus 1 off the pallete to get the real index
                                        VoxelColor = MyVoxelMainChunk.palatte[VoxelIndex2 - 1];
                                        if (SpawnedWorld)
                                        {
                                            SpawnedWorld.UpdateBlockTypeMass(
                                                "Color",
                                                VoxelIndex,
                                                VoxelColor);
                                        }
                                        else
                                        {
                                            Data.SetVoxelTypeColorRaw(VoxelIndex, 1, VoxelColor);
                                            Debug.LogError(VoxelColor.ToString());
                                        }
                                        /*MassUpdateVoxelIndex = MyBlockType;
                                        MassUpdateVoxelName = MyWorld.MyLookupTable.GetName(MyBlockType);
                                        MassUpdateColor = Color.white;
                                        MassUpdatePosition.Set(LoadingVoxelIndex);
                                        UpdateBlockTypeLoading();*/
                                    }
                                    else
                                    {
                                        if (SpawnedWorld)
                                        {
                                            SpawnedWorld.UpdateBlockTypeMass("Air", VoxelIndex);
                                        }
                                        /*else
                                        {
                                            Data.SetVoxelTypeRaw(VoxelIndex, 0);
                                        }*/
                                    }
                                }
                            }
                        }
                        if (SpawnedWorld)
                        {
                            SpawnedWorld.OnMassUpdate();
                        }
                        else
                        {
                            int VoxelType = 0;
                            System.Text.StringBuilder ImportVoxelDataBuilder = new System.Text.StringBuilder();
                            for (VoxelIndex.x = 0; VoxelIndex.x < Data.GetSize().x; ++VoxelIndex.x)
                            {
                                for (VoxelIndex.y = 0; VoxelIndex.y < Data.GetSize().y; ++VoxelIndex.y)
                                {
                                    for (VoxelIndex.z = 0; VoxelIndex.z < Data.GetSize().z; ++VoxelIndex.z)
                                    {
                                        VoxelType = Data.GetVoxelType(VoxelIndex);
                                        if (VoxelType > 0)
                                        {
                                            VoxelColor = Data.GetVoxelColorColor(VoxelIndex);
                                            //ImportVoxelDataBuilder.AppendLine(1 + " " + VoxelColor.r + " " + VoxelColor.g + " " + VoxelColor.b);
                                            int Red = (int)(255f * VoxelColor.r);
                                            int Green = (int)(255f * VoxelColor.g);
                                            int Blue = (int)(255f * VoxelColor.b);
                                            ImportVoxelDataBuilder.AppendLine("" + 1 + " " + Red + " " + Green + " " + Blue);
                                        }
                                        else
                                        {
                                            ImportVoxelDataBuilder.AppendLine(0.ToString());
                                        }
                                    }
                                }
                            }
                            ImportVoxelData = ImportVoxelDataBuilder.ToString();
                            Debug.LogError(Data.GetSize().GetVector().ToString() + " - Imported voxel data:\n" + ImportVoxelData);
                        }

                        /*if (MyVoxelMainChunk.alphaMaskChunk != null)
                        {
                            Debug.Log("Checking Alpha from .Vox: " + MyVoxelMainChunk.alphaMaskChunk.sizeX + ", " + MyVoxelMainChunk.alphaMaskChunk.sizeY + ", " + MyVoxelMainChunk.alphaMaskChunk.sizeZ);
                            for (int x = 0; x < MyVoxelMainChunk.alphaMaskChunk.sizeX; ++x)
                            {
                                for (int y = 0; y < MyVoxelMainChunk.alphaMaskChunk.sizeY; ++y)
                                {
                                    for (int z = 0; z < MyVoxelMainChunk.alphaMaskChunk.sizeZ; ++z)
                                    {
                                        Debug.Log(MyVoxelMainChunk.alphaMaskChunk.voxels[x, y, z].ToString());
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("Alpha Mask is null");
                        }*/
                    }
                    /*else
                    {
                        Debug.LogError("[World in Viewer] is null.");
                    }**/
                    // for our voxel data, set it to 
                }
                else
                {
                    Debug.LogError("[MyVoxelMainChunk] is null.");
                }
            }
            else
            {
                Debug.LogError("[MVVoxModel] Invalid file path");
            }
            //yield break;
#endif
        }
#endregion

        public void ExportPolygon(MeshFilter MyMeshFilter)//int FileIndex)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            System.Windows.Forms.SaveFileDialog MyDialog = new System.Windows.Forms.SaveFileDialog();
            System.Windows.Forms.DialogResult MyResult = MyDialog.ShowDialog();
            if (MyResult == System.Windows.Forms.DialogResult.OK)
            {
                string MyMeshString = MeshToString(MyMeshFilter);
                FileUtil.Save(MyDialog.FileName, MyMeshString);
            }
#endif
        }

        public static string MeshToString(MeshFilter MyMeshFilter)
        {
            Mesh mesh = MyMeshFilter.mesh;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append("g ").Append(mesh.name).Append("\n");
            foreach (Vector3 v in mesh.vertices)
            {
                sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
            }
            sb.Append("\n");
            foreach (Vector3 v in mesh.normals)
            {
                sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
            }
            sb.Append("\n");
            foreach (Vector3 v in mesh.uv)
            {
                sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
            }
            Material[] mats = MyMeshFilter.GetComponent<MeshRenderer>().sharedMaterials;
            for (int material = 0; material < mesh.subMeshCount; material++)
            {
                sb.Append("\n");
                sb.Append("usemtl ").Append(mats[material].name).Append("\n");
                sb.Append("usemap ").Append(mats[material].name).Append("\n");

                int[] triangles = mesh.GetTriangles(material);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                        triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
                }
            }
            return sb.ToString();
        }

        public void ImportImage(string FolderName, int FileIndex)
        {
            Zexel MyZexel = GetElement(FolderName, FileIndex) as Zexel;
            if (MyZexel != null)
            {
                ImportImage(FolderName, MyZexel);
            }
        }

        public void ImportImage(string FolderName, Zexel MyZexel)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null)
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                System.Windows.Forms.OpenFileDialog MyDialog = new System.Windows.Forms.OpenFileDialog();
                System.Windows.Forms.DialogResult MyResult = MyDialog.ShowDialog();
                if (MyResult == System.Windows.Forms.DialogResult.OK)
                {
                    byte[] bytes = FileUtil.LoadBytes(MyDialog.FileName);
                    MyZexel.LoadImage(bytes);
                }
                else
                {
                    Debug.LogError("Failure to open file.");
                }
#endif
            }
            else
            {
                Debug.LogError("Failed to find folder: " + OpenedFolderName);
            }
        }

        public void ImportZound(string FolderName, Sound.Zound MyZound)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null)
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                System.Windows.Forms.OpenFileDialog MyDialog = new System.Windows.Forms.OpenFileDialog();
                System.Windows.Forms.DialogResult MyResult = MyDialog.ShowDialog();
                if (MyResult == System.Windows.Forms.DialogResult.OK)
                {
                    UniversalCoroutine.CoroutineManager.StartCoroutine(ImportSoundRoutine(MyDialog.FileName, MyZound));
                }
                else
                {
                    Debug.LogError("Failure to open file.");
                }
#endif
            }
            else
            {
                Debug.LogError("Failed to find folder: " + OpenedFolderName);
            }
        }

        private System.Collections.IEnumerator ImportSoundRoutine(string FileName, Sound.Zound MyZound)
        {
            WWW MyWavLoader = new WWW("file://" + FileName);
            yield return MyWavLoader;
            LatestPlayed = WWWAudioExtensions.GetAudioClip(MyWavLoader);
            MyZound.UseAudioClip(LatestPlayed);
        }

        public void DrawFieldsForObject(object MyObject)
        {
            var Fields = (MyObject.GetType()).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            GUILayout.Space(10);
            Element MyElement = MyObject as Element;
            if (MyElement != null)
            {
                MyElement.IsDrawGui = GUILayout.Toggle(MyElement.IsDrawGui, MyElement.Name);
                if (MyElement.IsDrawGui == false)
                {
                    return;
                }
            }
            GUILayout.Space(15);
            GUILayout.Label("-----=====-----");
            if (IsDrawAllFields)
            {
                GUILayout.Label("Fields: [" + Fields.Length + "]");
                for (int i = 0; i < Fields.Length; i++)
                {
                    GUILayout.Label((i + 1) + " [" + Fields[i].Name + "]");
                }
                var Members = (MyObject.GetType()).GetMembers();
                GUILayout.Space(30);
                GUILayout.Label("Members: [" + Members.Length + "]");
                for (int i = 0; i < Members.Length; i++)
                {
                    GUILayout.Label((i + 1) + " [" + Members[i].Name + "]");
                }
                var Properties = (MyObject.GetType()).GetProperties();
                GUILayout.Label("Properties: [" + Properties.Length + "]");
                for (int i = 0; i < Properties.Length; i++)
                {
                    GUILayout.Label((i + 1) + " [" + Properties[i].Name + "]");
                }
            }
            else
            {
                for (int i = Fields.Length - 1; i >= 0; i--)
                {
                    object value = Fields[i].GetValue(MyObject);
                    if (Fields[i].FieldType == typeof(Texture))
                    {
                        Texture OldValue = Fields[i].GetValue(MyObject) as Texture;
                        GUILayout.Label(OldValue);
                    }
                    else if (Fields[i].FieldType == typeof(Texture2D))
                    {
                        Texture2D OldValue = Fields[i].GetValue(MyObject) as Texture2D;
                        GUILayout.Label(OldValue);
                    }
                    else if(HasJsonIgnore(Fields[i]))
                    {
                        // nothing
                    }
                    else if (value == null)
                    {
                        if (GUILayout.Button("[" + Fields[i].Name + "]: Null"))
                        {
#if NET_4_6
                            ConstructorInfo MyConstructor = Fields[i].FieldType.GetConstructor(System.Type.EmptyTypes);
                            dynamic NewValue = MyConstructor.Invoke(null);
                            //if (NewValue != null)
                            {
                                Fields[i].SetValue(MyObject, NewValue);
                                (MyObject as Element).OnModified();
                            }
#else
                            Debug.LogError("Function not defined in this type.");
#endif
                        }

                    }
                    else if (Fields[i] != null && Fields[i].IsStatic == false)
                    {
                        if (Fields[i].FieldType == typeof(string)
                         || Fields[i].FieldType == typeof(float)
                         || Fields[i].FieldType == typeof(int)
                         || Fields[i].FieldType == typeof(bool))
                        {
                            string OldValue = value.ToString();
                            GUILayout.Label("[" + Fields[i].Name + "]: " + OldValue);
                            string NewValue = GUILayout.TextField(OldValue);
                            if (OldValue != NewValue)
                            {
                                Fields[i].SetValue(MyObject, NewValue);
                                (MyObject as Element).OnModified();
                            }
                        }
                        else if (Fields[i].FieldType == typeof(Int3))
                        {
                            Int3 OldValue = Fields[i].GetValue(MyObject) as Int3;
                            //GUILayout.Label(" List<float> [" + Fields[i].Name + "]");
#if UNITY_EDITOR
                            Int3 NewValue = UnityEditor.EditorGUILayout.Vector3Field(Fields[i].Name + ": ", OldValue.GetVector()).ToInt3();
                            if (OldValue != NewValue)
                            {
                                Fields[i].SetValue(MyObject, NewValue);
                            }
#endif
                        }
                        else if (Fields[i].FieldType == typeof(List<float>))
                        {
                            bool WasModified;
                            List<float> OldValue = Fields[i].GetValue(MyObject) as List<float>;
                            GUILayout.Label(" List<float> [" + Fields[i].Name + "]: " + OldValue.Count);
                            List<float> NewValue = DrawListGui(OldValue, out WasModified);
                            if (WasModified)
                            {
                                Fields[i].SetValue(MyObject, NewValue);
                            }
                        }
                        else if (Fields[i].FieldType == typeof(List<string>))
                        {
                            bool WasModified;
                            List<string> OldValue = Fields[i].GetValue(MyObject) as List<string>;
                            GUILayout.Label(" List<string> [" + Fields[i].Name + "]: " + OldValue.Count);
                            List<string> NewValue = DrawListGui(OldValue, out WasModified);
                            if (WasModified)
                            {
                                Fields[i].SetValue(MyObject, NewValue);
                            }
                        }
                        else if (Fields[i].FieldType == typeof(List<int>))
                        {
                            bool WasModified;
                            List<int> OldValue = Fields[i].GetValue(MyObject) as List<int>;
                            GUILayout.Label(" List<int> [" + Fields[i].Name + "]: " + OldValue.Count);
                            List<int> NewValue = DrawListGui(OldValue, out WasModified);
                            if (WasModified)
                            {
                                Fields[i].SetValue(MyObject, NewValue);
                            }
                        }
                        else if (Fields[i].FieldType == typeof(StatType))
                        {
                            int OldValue = (int)value;
                            GUILayout.Label("[" + Fields[i].Name + "]: " + OldValue);
                            int NewValue = int.Parse(GUILayout.TextField(OldValue.ToString()));
                            if (OldValue != NewValue)
                            {
                                Fields[i].SetValue(MyObject, NewValue);
                                (MyObject as Element).OnModified();
                            }
                        }
                        else if (Fields[i].FieldType.BaseType == typeof(Element))
                        {
                            GUILayout.Label("Element: " + i + ": " + Fields[i].Name);
                            DrawFieldsForObject(value);
                        }
                        else if (DrawListGui<Items.Item>(Fields[i], MyObject))
                        {

                        }
                        else if (DrawListGui<Combat.Stat>(Fields[i], MyObject))
                        {

                        }
                    }
                    //Fields[i].SetValue(GUILayout.TextField(Fields[i].GetValue()));
                }
            }
            GUILayout.Label("-----=====-----");
            GUILayout.Space(15);
        }

        /// <summary>
        /// Draws for a list of type T
        /// </summary>
        private bool DrawListGui<T>(FieldInfo MyField, object MyObject)
        {
            if (MyField.FieldType == typeof(List<T>))
            {
                bool WasModified;
                List<T> OldValue = MyField.GetValue(MyObject) as List<T>;
                GUILayout.Label(" List<" + typeof(T).ToString() + "> [" + MyField.Name + "]: " + OldValue.Count);
                List<T> NewValue = DrawListGui(OldValue, out WasModified);
                if (WasModified)
                {
                    MyField.SetValue(MyObject, NewValue);
                }
                return true;
            }
            return false;
        }

        public bool HasJsonIgnore(FieldInfo MyField)
        {
            object[] attributes = MyField.GetCustomAttributes(false);

            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i].GetType() == typeof(Newtonsoft.Json.JsonIgnoreAttribute))
                {
                    return true;
                }
            }
            return false;
        }
        
        List<T> DrawListGui<T>(List<T> MyList, out bool WasModified)
        {
            WasModified = false;
            //GUILayout.Label("[" + MyList.Count + "]");
            List<string> ListString = MyList as List<string>;
            List<float> ListFloat = MyList as List<float>;
            List<T> ListElements = MyList as List<T>;
            if (GUILayout.Button("Add"))
            {
                if (typeof(T) == typeof(string))
                {
                    ListString.Add("");
                }
                else if (typeof(T) == typeof(float))
                {
                    ListFloat.Add(0);
                }
                else if (typeof(T).BaseType == typeof(Element))
                {
#if NET_4_6
                    ConstructorInfo MyConstructor = typeof(T).GetConstructor(System.Type.EmptyTypes);
                    dynamic NewValue = MyConstructor.Invoke(null);
                    //if (NewValue != null)
                    {
                        ListElements.Add(NewValue);
                    }
#else
                    Debug.LogError("Not implemented for net 3.5");
#endif
                }
                WasModified = true;
            }
            for (int j = 0; j < MyList.Count; j++)
            {
                //GUILayout.Label("\t" + (j + 1) + ": [" + MyList[j] + "]");
                if (typeof(T) == typeof(string))
                {
                    string NewValue = GUILayout.TextArea(ListString[j]);
                    if (ListString[j] != NewValue)
                    {
                        ListString[j] = NewValue;
                        WasModified = true;
                    }
                }
                else if (typeof(T) == typeof(float))
                {
                    float NewValue = float.Parse(GUILayout.TextArea(ListFloat[j].ToString()));
                    if (ListFloat[j] != NewValue)
                    {
                        ListFloat[j] = NewValue;
                        WasModified = true;
                    }
                }
                else if (typeof(T).BaseType == typeof(Element))
                {
                    DrawFieldsForObject(ListElements[j]);
                }
            }
            if (typeof(T) == typeof(string))
            {
                MyList = ListString as List<T>;
            }
            else if (typeof(T) == typeof(float))
            {
                MyList = ListFloat as List<T>;
            }
            else if (typeof(T).BaseType == typeof(Element))
            {
                MyList = ListElements as List<T>;
            }
            return MyList;
        }
#endregion
#endregion
    }
}