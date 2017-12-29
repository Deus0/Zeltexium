using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using Zeltex.Generators;
using Zeltex.Util;
using Zeltex.Combat;

namespace Zeltex
{

    /// <summary>
    /// GUI Class for data manager
    /// </summary>
	public class DataGUI : ManagerBase<DataGUI>
    {
		public bool IsDebugGui;
        private int MapNameSelected = 0;
        private List<string> MapNames = null;
        private Vector2 scrollPosition;
        private string OpenedFolderName = "";
        private int OpenedFolderIndex = -1;

        private string OpenedFileName = "";
        private int OpenedFileIndex = -1;
        private Element OpenedElement;
		private bool IsDrawAllFields;
		private string RenameName = "Null";
		// Folders
		private Zexel OpenedTexture;
		private bool IsOpenedElementFolder;
		private ElementFolder OpenedFolder;
		private RenderTexture PolyRenderTexture;

        #region Main
        
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
			if (DataManager.Get().GetIsLoaded())
            {
				GUILayout.Label("Loaded [" + DataManager.Get().MapName + "]");
            }
            else
            {
				GUILayout.Label("Selected [" + DataManager.Get().MapName + "]");
            }
            //IsJSONFormat = GUILayout.Toggle(IsJSONFormat, "JSON");
			if (DataManager.Get().GetIsLoaded())
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
				FileUtil.OpenPathInWindows(DataManager.Get().GetResourcesPath());
            }
            GUILayout.Label("MapName:");
            if (GUILayout.Button("Load"))
			{
				DataManager.Get().InitializeFolders();
				DataManager.Get().LoadAll();
            }
            if (MapNames == null || GUILayout.Button("Refresh List"))
            {
                RefreshGuiMapNames();
            }
			GUILayout.Label("PathType: " + DataManager.Get().MyFilePathType);
            //MyFilePathType = (FilePathType)int.Parse(GUILayout.TextField(((int)MyFilePathType).ToString()));
			GUI.enabled = (DataManager.Get().MyFilePathType != FilePathType.Normal);
            if (GUILayout.Button("Normal"))
            {
				DataManager.Get().MyFilePathType = FilePathType.Normal;
                RefreshGuiMapNames();
            }
			GUI.enabled = (DataManager.Get().MyFilePathType != FilePathType.PersistentPath);
            if (GUILayout.Button("Persistent"))
            {
				DataManager.Get().MyFilePathType = FilePathType.PersistentPath;
                RefreshGuiMapNames();
            }
			GUI.enabled = (DataManager.Get().MyFilePathType != FilePathType.StreamingPath);
            if (GUILayout.Button("Streaming"))
            {
				DataManager.Get().MyFilePathType = FilePathType.StreamingPath;
                RefreshGuiMapNames();
            }
            GUI.enabled = true;

            GUILayout.Space(10);
            int NewMapNameSelected = GUILayout.SelectionGrid(MapNameSelected, MapNames.ToArray(), 1);
            if (MapNameSelected != NewMapNameSelected)
            {
                MapNameSelected = NewMapNameSelected;
				DataManager.Get().MapName = MapNames[MapNameSelected];
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
				DataManager.Get().MapName = "";
            }
            else
            {
                MapNameSelected = Mathf.Clamp(MapNameSelected, 0, MapNames.Count - 1);
				DataManager.Get().MapName = MapNames[MapNameSelected];
            }
			CloseAll ();
        }

		private void CloseAll()
		{
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
				DataManager.Get().UnloadAll();
				CloseAll();
            }
            if (RenameName == "Null")
            {
				RenameName = DataManager.Get().MapName;
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
					if (RenameName != DataManager.Get().MapName)
                    {
                        RenameResourcesFolder(RenameName);
                    }
                }

                if (GUILayout.Button("Open Folder"))
                {
					FileUtil.OpenPathInWindows(DataManager.Get().GetMapPathNS());
                }
                if (GUILayout.Button("Save"))
                {
					DataManager.Get().SaveAll();
                }
                if (GUILayout.Button("Erase"))
                {
					DataManager.Get().DeleteAll();
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

        /// <summary>
        /// Rename the entire resources directory
        /// </summary>
        private void RenameResourcesFolder(string NewName)
        {
            string OldFolderPath = DataManager.Get().GetResourcesPath() + DataManager.Get().MapName + "/";
            string NewFolderPath = DataManager.Get().GetResourcesPath() + NewName + "/";
            if (FileManagement.DirectoryExists(NewFolderPath, true, true) == false)
            {
                System.IO.Directory.Move(OldFolderPath, NewFolderPath);
                DataManager.Get().MapName = NewName;
            }
            else
            {
                Debug.Log("Cannot move to " + NewFolderPath + " as already exists.");
            }
        }

        #endregion

        #region Folders

        private void DrawSelectedFolder()
        {
            GUILayout.Space(30);
            GUILayout.Label("Opened Folder [" + OpenedFolderName + "] " + DataFolderNames.GetDataType(OpenedFolderName).ToString());
			ElementFolder TheFolder = DataManager.Get().GetElementFolder(OpenedFolderName);
            if (GUILayout.Button("Open " + OpenedFolderName))
            {
				string FolderPath = DataManager.Get().GetMapPathNS() + OpenedFolderName + "/";
                if (FileManagement.DirectoryExists(FolderPath, true, true) == false)
                {
                    FileManagement.CreateDirectory(FolderPath, true);
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
						ElementFolder MyFolder = DataManager.Get().GetElementFolder(OpenedFolderName);
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
						ElementFolder MyFolder = DataManager.Get().GetElementFolder(OpenedFolderName);
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
                    //if (GUILayout.Button("Import"))
                    {
#if UNITY_EDITOR// || UNITY_STANDALONE_WIN
                       // if (HasOpenedZexels())
                        {
                            //ImportZexel();
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
			ElementFolder MyFolder = DataManager.Get().GetElementFolder(FolderName);
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
			if (IsOpenedElementFolder && OpenedFolder != null)
            {
                int ElementCount = 0;
                try
                {
					foreach (KeyValuePair<string, Element> MyKeyValuePair in OpenedFolder.Data)
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
			List<ElementFolder> ElementFolders = DataManager.Get().GetFolders();
            GUILayout.Space(30);
            GUILayout.Label("Folders: " + ElementFolders.Count);
            for (int i = 0; i < ElementFolders.Count; i++)
            {
				if (GUILayout.Button(ElementFolders[i].FolderName + " [" + DataManager.Get().GetSize(ElementFolders[i].FolderName) + "]"))
                {
                    OpenedFolderName = ElementFolders[i].FolderName;
                    OpenedFolderIndex = i;
					OpenedFolder = ElementFolders[i];
                    IsOpenedElementFolder = true;
                }
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
            GUILabel("Opened Folder [" + OpenedFolderName + "]");
            GUILayout.Space(30);
            if (IsOpenedElementFolder)
            {
                GUILabel("Opened File [" + OpenedFileName + "] - Type [" + OpenedElement.GetType() + "]");
            }
            else
            {
                GUILabel("Opened File [" + OpenedFileName + "] - Type [" + OpenedTexture.GetType() + "]");
            }
            if (GUIButton("Close"))
            {
                OpenedFileName = "";
            }
            else
            {
                if (IsOpenedElementFolder)
                {
                    GUILabel(OpenedElement.Name + " - [" + OpenedElement.CanSave().ToString() + "]");
                }
                /*if (GUIButton("ForceSave"))
                {
                    if (IsOpenedElementFolder)
                    {
                        OpenedElement.OnModified();
                        OpenedElement.Save();
                    }
                }*/
				if (OpenedElement.CanSave() == false) 
				{
					GUI.enabled = false;
				}
                if (GUIButton("Save"))
                {
                    if (IsOpenedElementFolder)
                    {
                        OpenedElement.Save();
                    }
				}
				GUI.enabled = true;
                if (GUIButton("Revert"))
                {
                    if (IsOpenedElementFolder)
                    {
						OpenedElement = DataManager.Get().RevertElement(OpenedElement);
                    }
                }
                if (GUIButton("Delete"))
                {
                    OpenedElement.Delete();
                }
                /*if (OpenedElement.GetType() == typeof(Zexel))
                {
                    DrawZexelGui(OpenedTexture);
                }
                else*/
                if (OpenedElement.GetType() == typeof(Voxels.PolyModel))
                {
					DrawPolyModel();
                }
                else if (OpenedElement.GetType() == typeof(Voxels.VoxelModel))
                {
					DrawVoxelModel();
                }
                else if(OpenedElement.GetType() == typeof(Sound.Zound))
                {
					DrawZound();
                }

                GUILayout.Space(30);
                IsDrawAllFields = GUILayout.Toggle(IsDrawAllFields, "IsDrawAllFields");
                DrawFieldsForObject(OpenedElement as object, null, null, true);
            }
        }

		private void DrawPolyModel() 
		{
			GUILayout.Label("PolyModel");
			if (PolyRenderTexture == null)
			{
				PolyRenderTexture = Resources.Load("TestRenderTexture") as RenderTexture;
			}
			GUITexture(PolyRenderTexture);
			if (GUIButton("Import"))
			{
				DataManager.Get().ImportPolygon(OpenedFileIndex);
			}
			if (GUIButton("Export"))
			{
				DataManager.Get().ExportPolygon(OpenedElement as Voxels.PolyModel);
			}
		}

		private void DrawZound() 
		{
			GUILayout.Space(30);
			GUILabel("Zound: " + (OpenedElement as Sound.Zound).GetSize());
			// buttons

			if (GUIButton("Import"))
			{
				#if UNITY_EDITOR// || UNITY_STANDALONE_WIN
				DataManager.Get().ImportZound(OpenedFolderName, OpenedElement as Sound.Zound);
				#else
				Debug.LogError("Platform not supported.");
				#endif
			}

			if (GUIButton("Play"))
			{
				DataManager.Get().PlayClip((OpenedElement as Sound.Zound).GetAudioClip());
			}
		}
		private void DrawVoxelModel() 
		{
			GUILayout.Space(30);
			GUILabel("VoxelModel");
			if (GUIButton("Import"))
			{
				UniversalCoroutine.CoroutineManager.StartCoroutine(DataManager.Get().LoadVoxFile((OpenedElement as Voxels.VoxelModel)));
			}
			GUILabel(OpenedElement.Name + " - Size: " + (OpenedElement as Voxels.VoxelModel).VoxelData.Length);// + ":" + (OpenedElement as Voxels.VoxelModel).VoxelData);
		}

        /*public void DrawZexelGui(Zexel MyZexel)
        {
            //GUILayout.Space(30);
            //GUILabel("Zexel");
            //if (HasOpenedZexels() == true && OpenedTexture != null)
            MyZexel.IsDrawGui = GUIFoldout(MyZexel.IsDrawGui, "[" + MyZexel.Name + "] " + MyZexel.GetType().ToString());
            {
                GUILabel("Size: " + MyZexel.GetWidth() + " : " + MyZexel.GetHeight());
                //GUILabel("IsNull? [" + (MyZexel.GetTexture() != null) + "]");
                GUILayout.Space(30);
				GUITexture(MyZexel.GetTexture(), 4);
            }
            // buttons

            if (GUIButton("Import"))
            {
#if UNITY_EDITOR// || UNITY_STANDALONE_WIN
				DataManager.Get().ImportImage(MyZexel);
#else
				Debug.LogError("Platform not supported.");
#endif
            }
            if (GUIButton("Export"))
            {
#if UNITY_EDITOR// || UNITY_STANDALONE_WIN
				DataManager.Get().ExportZexel(MyZexel);
#else
                        Debug.LogError("Platform not supported.");
#endif
            }
        }*/

		private void GUITexture(Texture MyTexture, float Multiple = 1f) 
		{
			if (GUI.skin == null)
			{
				Debug.LogError("Skin is null in GUITexture DataGUI");
				return;
			}
            GUILayout.Space(30);
			if (MyTexture == null) 
			{
				MyTexture = Texture2D.blackTexture as Texture;
			}
			Rect OtherRect = GUILayoutUtility.GetRect(new GUIContent("Blargnugg"), GUI.skin.button);
			Rect MyRect = new Rect(0, 0, MyTexture.width * Multiple, MyTexture.height * Multiple);
			MyRect.x = GUIIndentPositionX() + MyTexture.width * (Multiple / 2f);// OtherRect.width / 2f - MyZexel.GetWidth() * 2f;
			MyRect.y = OtherRect.y;
			int BorderSize = 20;
			Rect BackgroundRect = new Rect(MyRect.x - BorderSize, MyRect.y - BorderSize,
				MyRect.width + BorderSize * 2f, MyRect.height + BorderSize * 2f);
			GUI.color = Color.gray;
			GUI.DrawTexture(BackgroundRect, Texture2D.whiteTexture);
			GUI.color = Color.white;
			GUI.DrawTexture(MyRect, MyTexture);
			GUILayout.Space(MyRect.height);
		}

        public void GUILabel(string MyLabelText)
        {
#if UNITY_EDITOR
            UnityEditor.EditorGUILayout.LabelField(MyLabelText);
#else
            GUILayout.Label(MyLabelText);
#endif
        }

        public string GUIText(string OldValue)
        {
#if UNITY_EDITOR
            return UnityEditor.EditorGUILayout.TextField(OldValue);
#else
            return GUILayout.TextField(OldValue);
#endif
        }

        public int GUIIndentPositionX()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorGUI.indentLevel * 16;
#else
            return 0;
#endif
        }
        public bool GUIButton(string ButtonLabel)
        {
#if UNITY_EDITOR
            Rect MyRect = GUILayoutUtility.GetRect(new GUIContent(ButtonLabel), GUI.skin.GetStyle("button"));
            //Rect MyRect = UnityEditor.EditorGUILayout.
            //return UnityEditor.EditorGUILayout.By(OldValue);
            MyRect.position = new Vector2(GUIIndentPositionX() + MyRect.position.x, MyRect.position.y);
            return GUI.Button(MyRect, ButtonLabel);
#else
            return GUILayout.Button(ButtonLabel);
#endif
        }

        public bool GUIFoldout(bool OldValue, string MyLabelText)
        {
#if UNITY_EDITOR
                return UnityEditor.EditorGUILayout.Foldout(OldValue, MyLabelText);
#else
                return GUILayout.Toggle(OldValue, MyLabelText);
#endif
        }

		/// <summary>
		/// Dynamically creates a gui for an object
		/// </summary>
		public object DrawFieldsForObject(object MyObject, object ParentObject, FieldInfo MyField, bool IsFirstField = false)
        {
            var Fields = (MyObject.GetType()).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            object ReturnObject = null;
            Element MyElement = MyObject as Element;
            if (MyElement != null)
            {
                MyElement.IsDrawGui = GUIFoldout(MyElement.IsDrawGui, "[" + MyElement.Name + "] " + MyObject.GetType().ToString());
                if (MyElement.IsDrawGui == false)
                {
                    return null;
                }
            }
            else
            {
                GUILabel("Object: " + MyObject.ToString());
            }
#if UNITY_EDITOR
            UnityEditor.EditorGUI.indentLevel++;
#endif
            //GUILayout.Label("-----=====-----");
            if (IsDrawAllFields)
            {
                GUILabel("Fields: [" + Fields.Length + "]");
                for (int i = 0; i < Fields.Length; i++)
                {
                    GUILabel((i + 1) + " [" + Fields[i].Name + "]");
                }
                var Members = (MyObject.GetType()).GetMembers();
                GUILayout.Space(30);
                GUILabel("Members: [" + Members.Length + "]");
                for (int i = 0; i < Members.Length; i++)
                {
                    GUILabel((i + 1) + " [" + Members[i].Name + "]");
                }
                var Properties = (MyObject.GetType()).GetProperties();
                GUILabel("Properties: [" + Properties.Length + "]");
                for (int i = 0; i < Properties.Length; i++)
                {
                    GUILabel((i + 1) + " [" + Properties[i].Name + "]");
                }
            }
            else
            {
                for (int i = Fields.Length - 1; i >= 0; i--)
                {
                    object value = Fields[i].GetValue(MyObject);
                    /*if (Fields[i].FieldType == typeof(Texture))
                    {
                        Texture OldValue = Fields[i].GetValue(MyObject) as Texture;
                        GUILayout.Label(OldValue);
                    }
                    else if (Fields[i].FieldType == typeof(Texture2D))
                    {
                        Texture2D OldValue = Fields[i].GetValue(MyObject) as Texture2D;
                        GUILayout.Label(OldValue);
                    }
                    else (*/
                    /*if (Fields[i].FieldType == typeof(CharacterStats))
                    {
                        GUILabel(Fields[i].Name + " is null? " + (value == null).ToString() + " has json ignore? " + (HasJsonIgnore(Fields[i])).ToString()
                             + " is value null? " + ((value == null)).ToString() + " is static? " + Fields[i].IsStatic.ToString());
                    }*/
                    if (HasJsonIgnore(Fields[i]))
                    {
                        // nothing
                    }
                    else if (value == null)
                    {
                        if (GUIButton("[" + Fields[i].Name + "]: Null"))
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
                    // For normal fields!
                    else if (Fields[i] != null && Fields[i].IsStatic == false)
                    {
                        if (Fields[i].FieldType == typeof(string)
                         || Fields[i].FieldType == typeof(float)
                         || Fields[i].FieldType == typeof(int)
                         || Fields[i].FieldType == typeof(bool))
                        {
                            string OldValue = value.ToString();
                            GUILabel("[" + Fields[i].Name + "]: " + OldValue);
                            string NewValue = GUIText(OldValue);
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
                        else if (Fields[i].FieldType == typeof(Vector3))
                        {
                            Vector3 OldValue = (Vector3)Fields[i].GetValue(MyObject);
                            //GUILayout.Label(" List<float> [" + Fields[i].Name + "]");
                            if (OldValue != null)
                            {
#if UNITY_EDITOR
                                Vector3 NewValue = UnityEditor.EditorGUILayout.Vector3Field(Fields[i].Name + ": ", OldValue);
                                if (OldValue != NewValue)
                                {
                                    Fields[i].SetValue(MyObject, NewValue);
                                }
#endif
                            }
                            else
                            {
                                GUILabel("Broken Vector3 Field: " + Fields[i].Name);
                            }
                        }
                        else if (Fields[i].FieldType == typeof(List<float>))
                        {
                            bool WasModified;
                            List<float> OldValue = Fields[i].GetValue(MyObject) as List<float>;
                            GUILabel(" List<float> [" + Fields[i].Name + "]: " + OldValue.Count);
                            List<float> NewValue = DrawListGui(OldValue, MyObject, Fields[i], out WasModified);
                            if (WasModified)
                            {
                                Fields[i].SetValue(MyObject, NewValue);
                            }
                        }
                        else if (Fields[i].FieldType == typeof(List<string>))
                        {
                            bool WasModified;
                            List<string> OldValue = Fields[i].GetValue(MyObject) as List<string>;
                            GUILabel(" List<string> [" + Fields[i].Name + "]: " + OldValue.Count);
                            List<string> NewValue = DrawListGui(OldValue, MyObject, Fields[i], out WasModified);
                            if (WasModified)
                            {
                                Fields[i].SetValue(MyObject, NewValue);
                            }
                        }
                        else if (Fields[i].FieldType == typeof(List<int>))
                        {
                            bool WasModified;
                            List<int> OldValue = Fields[i].GetValue(MyObject) as List<int>;
                            GUILabel(" List<int> [" + Fields[i].Name + "]: " + OldValue.Count);
                            List<int> NewValue = DrawListGui(OldValue, MyObject, Fields[i], out WasModified);
                            if (WasModified)
                            {
                                Fields[i].SetValue(MyObject, NewValue);
                            }
                        }
                        else if (Fields[i].FieldType == typeof(StatType))
                        {
                            int OldValue = (int)value;
                            GUILabel("[" + Fields[i].Name + "]: " + OldValue);
                            int NewValue = int.Parse(GUILayout.TextField(OldValue.ToString()));
                            if (OldValue != NewValue)
                            {
                                Fields[i].SetValue(MyObject, NewValue);
                                (MyObject as Element).OnModified();
                            }
                        }
                        /*else if (Fields[i].FieldType == typeof(Skeletons.Skeleton))
                        {
                            GUILabel("Element: " + i + ": " + Fields[i].Name);
                            DrawFieldsForObject(value);
                            if (GUIButton("Pull"))
                            {
                                Skeletons.Skeleton OldValue = (Skeletons.Skeleton)Fields[i].GetValue(MyObject);
								Fields[i].SetValue(MyObject, DataManager.Get().GetElement(DataFolderNames.Skeletons, OldValue.Name).Clone<Skeletons.Skeleton>());
                            }
                        }*/
                        /*else if (Fields[i].FieldType.BaseType == typeof(Element))
                        {
                            GUILabel("Element: " + i + ": " + Fields[i].Name);
                            DrawFieldsForObject(value);
                        }*/
                        else if (DrawListGui<Items.Item>(Fields[i], MyObject))
                        {

                        }
                        else if (DrawListGui<Combat.Stat>(Fields[i], MyObject))
                        {

                        }
                        // Draw for elements!
                        if (Fields[i].FieldType.BaseType == typeof(Element))
                        {
                            //GUILabel(Fields[i].FieldType.ToString() + ": " + i + ": " + Fields[i].Name);
                            DrawFieldsForObject(value, MyObject, Fields[i]);
                        }
                        /*else if (Fields[i].FieldType.BaseType == typeof(Stats))
                        {
                            DrawFieldsForObject(value, Fields[i]);
                        }*/
                    }
                    //Fields[i].SetValue(GUILayout.TextField(Fields[i].GetValue()));
                }

                //Element FieldElement = Fields[i].GetValue(MyObject) as Element;
                if (MyElement != null)
                {
                    GUILabel("Data Moving");
                    // At the moment this only supports single positioning, but later on it will support more
                    MyElement.ElementLink = GUIText(MyElement.ElementLink);
                    if (MyElement.ElementLink == "")
                    {
                        if (IsFirstField)
                        {
                            MyElement.ElementLink = MyElement.GetFolder();
                        }
                        else
                        {
                            MyElement.ElementLink = DataFolderNames.DataTypeToFolderName(MyObject.GetType());
                        }
                    }
                    if (GUIButton("Pull From [" + MyElement.ElementLink + "]"))
                    {
                        //Element OldValue = Fields[i].GetValue(MyObject) as Element;
                        Debug.Log(MyElement.Name + " is being overwritten from an element in database folder: " + MyElement.ElementLink);
                        Element NewElement = DataManager.Get().GetElement(MyElement.ElementLink, MyElement.Name).Clone();
                        NewElement.IsDrawGui = MyElement.IsDrawGui;
                        NewElement.MyFolder = MyElement.MyFolder;
                        NewElement.ElementLink = MyElement.ElementLink;
                        NewElement.ParentElement = MyElement.ParentElement;
                        NewElement.ResetName();
                        if (MyField != null)
                        {
                            try
                            {
                                MyField.SetValue(ParentObject, NewElement);
                                Debug.Log(MyElement.Name + " is using MyField.SetValue");
                            }
                            catch (System.ArgumentException e)
                            {
                                // From a list of elements
                                Debug.LogError(e.ToString());
                                Debug.LogError(NewElement.GetType() + " compared to: " + MyObject.GetType() + " Field: " + MyField.Name + " of object " + MyObject.ToString());
                                ReturnObject = NewElement as object;
                                Debug.Log(MyElement.Name + " is using ReturnObject to a list");
                            }
                            NewElement.OnModified();
                        }
                        else if (IsFirstField)
                        {
                            OpenedFolder.SetElement(NewElement);
                        }
                        else
                        {
                            Debug.LogError("Could not pull element.");
                        }
                    }
                    if (GUIButton("Push To [" + MyElement.ElementLink + "]"))
                    {
                        //Element MyValue = Fields[i].GetValue(MyObject) as Element;
                        DataManager.Get().PushElement(MyElement.ElementLink, MyElement);
                    }
                    if (MyElement.GetType() == typeof(Zexel))
                    {
                        Zexel MyZexel = MyElement as Zexel;
                        if (MyZexel != null)
                        {
                            GUITexture(MyZexel.GetTexture(), 4);
                            if (GUIButton("Import"))
                            {
                                DataManager.Get().ImportImage(MyZexel);
                            }
                            if (GUIButton("Export"))
                            {
                                DataManager.Get().ExportZexel(MyZexel);
                            }
                        }
                    }
                }
            }
#if UNITY_EDITOR
            UnityEditor.EditorGUI.indentLevel--;
#endif
            //GUILabel("-----=====-----");
            return ReturnObject;
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
                GUILabel(" List<" + typeof(T).ToString() + "> [" + MyField.Name + "]: " + OldValue.Count);
                List<T> NewValue = DrawListGui(OldValue, MyObject, MyField, out WasModified);
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
        
        List<T> DrawListGui<T>(List<T> MyList, object MyObject, FieldInfo MyField, out bool WasModified)
        {
            WasModified = false;
            //GUILayout.Label("[" + MyList.Count + "]");
            List<string> ListString = MyList as List<string>;
            List<float> ListFloat = MyList as List<float>;
            List<T> ListElements = MyList as List<T>;
            if (GUIButton("Add"))
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
                    object NewValue = System.Activator.CreateInstance(typeof(T));
                    ListElements.Add((T)NewValue);
#endif
                    Element ParentOfList = MyObject as Element;
                    (ListElements[ListElements.Count - 1] as Element).ParentElement = ParentOfList;
                    ParentOfList.OnModified();
                }
                WasModified = true;
            }
            for (int j = 0; j < MyList.Count; j++)
            {
                //GUILayout.Label("\t" + (j + 1) + ": [" + MyList[j] + "]");
                if (typeof(T) == typeof(string))
                {
                    string NewValue = GUIText(ListString[j]);
                    if (ListString[j] != NewValue)
                    {
                        ListString[j] = NewValue;
                        WasModified = true;
                    }
                }
                else if (typeof(T) == typeof(float))
                {
                    float NewValue = float.Parse(GUIText(ListFloat[j].ToString()));
                    if (ListFloat[j] != NewValue)
                    {
                        ListFloat[j] = NewValue;
                        WasModified = true;
                    }
                }
                else if (typeof(T).BaseType == typeof(Element))
                {
                    T NewElement = (T) DrawFieldsForObject(ListElements[j], MyObject, MyField);
                    if (NewElement != null)
                    {
                        ListElements[j] = NewElement;
                        WasModified = true;
                    }
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
    }
}