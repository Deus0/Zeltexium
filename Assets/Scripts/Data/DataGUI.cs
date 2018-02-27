using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using Zeltex.Generators;
using Zeltex.Util;
using Zeltex.Combat;
using Zeltex.Voxels;
using Zeltex.Items;
using Zeltex.Skeletons;

namespace Zeltex
{

    /// <summary>
    /// GUI Class for data manager
    /// </summary>
	public class DataGUI : ManagerBase<DataGUI>
    {
		public bool IsDebugGui;
        private int MapNameSelected = 0;
		private List<string> MapNames = new List<string>();
        private Vector2 scrollPosition;
        private string OpenedFolderName = "";
        private int OpenedFolderIndex = -1;

        private string OpenedFileName = "";
        private int OpenedFileIndex = -1;
        private Element OpenedElement;
		private bool IsDrawAllFields;
		private string RenameName = "Null";
		// Folders
		//private Zexel OpenedTexture;
		private bool IsOpenedElementFolder;
		private ElementFolder OpenedFolder;
		private RenderTexture PolyRenderTexture;
        private Level MyLevel;
        private bool IsExtraOptions;

        #region Main

#if UNITY_EDITOR

        public static void PrepareDataForReload()
        {
            Debug.Log("Custom Serializing DataManager Data as manual refresh of code detected.");
            if (DataManager.Get())
            {
                for (int i = 0; i < DataManager.Get().GetSize(); i++)
                {
                    DataManager.Get().GetFolder(i).Serialize();
                }
            }
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            Debug.Log("Reloaded scripts, so custom deserialized the DataManager Data.");
            // bum!
            if (DataManager.Get() != null && DataManager.Get().GetIsLoaded())
            {
                for (int i = 0; i < DataManager.Get().GetFolders().Count; i++)
                {
                    DataManager.Get().GetFolder(i).Deserialize();
                }
            }   
            DataGUI.Get().Repaint();
        }
#endif
        public new static DataGUI Get()
        {
            if (MyManager == null)
            {
                MyManager = GameObject.Find("DataManager").GetComponent<DataGUI>();
            }
            return MyManager;
        }


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

#if UNITY_EDITOR
        UnityEditor.EditorWindow MyWindow = null;

        public void Repaint()
        {
            if (MyWindow != null)
            {
                MyWindow.Repaint();
            }
        }
#else
        public void Repaint() { }
#endif
        public void DrawGui(object NewWindow = null)
        {
#if UNITY_EDITOR
            MyWindow = NewWindow as UnityEditor.EditorWindow;
#endif
            try
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            }
            catch (System.ArgumentException)
            {
                return;
            }
            MyLevel = OpenedElement as Level;

            if (DataManager.Get().IsLoading())
			{
                GUILabel("==================");
                IncreaseIndent();
                GUILabel("Loading [" + DataManager.Get().MapName + "]");
                DecreaseIndent();
				GUILabel("==================");
                IncreaseIndent();
                if (GUIButton("===== Cancel ( ===== "))
                {
                    DataManager.Get().CancelLoading();
                }
                DecreaseIndent();
                GUILayout.Label("==================");
            }
            else if (MyLevel != null && MyLevel.IsSpawning)
            {
                GUILayout.Label("==================");
                GUILayout.Label("Loading Level [" + MyLevel.Name + "]");
                GUILayout.Label("==================");
            }
			else
			{
				//IsJSONFormat = GUILayout.Toggle(IsJSONFormat, "JSON");
				if (DataManager.Get().GetIsLoaded())
				{
					LoadedDataGui();
				}
				else
				{
					NotLoadedGui();
				}
			}
            GUILayout.EndScrollView();
        }

        public void NotLoadedGui()
		{
			//GUILayout.Label("Selected [" + DataManager.Get().MapName + "]");
			//GUILabel("MapName:");
			string MapName = "None";
			if (MapNames.Count > 0)
			{
				MapName = MapNames[MapNameSelected];
			}
			if (GUIButton("Load [" + MapName + "]"))
			{
				DataManager.Get().InitializeFolders();
				DataManager.Get().LoadAll(
                    () =>
                    {
                        Repaint();
                    });
                DataManager.Get().WasPlayingOnLoad = false;
            }

			// List of map names!
            //GUILayout.Space(30);
			GUILabel("Select from " + MapNames.Count + " different Resources.");
			IncreaseIndent();
            //int NewMapNameSelected = GUILayout.SelectionGrid(MapNameSelected, MapNames.ToArray(), 1);
			int NewMapNameSelected = MapNameSelected;
			for (int i = 0; i < MapNames.Count; i++)
			{
				if (MapNames[i] == MapName)
				{
					GUI.enabled = false;
				}
				if (GUIButton(MapNames[i]))
				{
					NewMapNameSelected = i;
				}
				GUI.enabled = true;
			}
            if (MapNameSelected != NewMapNameSelected)
            {
                MapNameSelected = NewMapNameSelected;
				DataManager.Get().MapName = MapNames[MapNameSelected];
			}
			DecreaseIndent();

			// Extra Options
			//GUILabel("Extra Options");
			GUILayout.Space(30);
			IsExtraOptions = GUIToggle("Extra Options", IsExtraOptions);
			if (IsExtraOptions)
            {
                GUILabel("DataPath [" + DataManager.Get().DataPath + "]");
                GUILabel("PersistentDataPath [" + DataManager.Get().PersistentDataPath + "]");
                if (GUIButton("Refresh Path"))
                {
                    DataManager.Get().DataPath = Application.dataPath;
                    DataManager.Get().PersistentDataPath = Application.persistentDataPath;
                    RefreshGuiMapNames();
                }
				if (GUIButton("Open Resources Folder"))
				{
					FileUtil.OpenPathInWindows(DataManager.Get().GetResourcesPath());
				}

				if (MapNames == null || GUILayout.Button("Refresh List"))
				{
					RefreshGuiMapNames();
				}
				GUILabel("PathType: " + DataManager.Get().MyFilePathType);
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
			}
        }

        private void RefreshGuiMapNames()
        {
            MapNames = Guis.Maker.ResourcesMaker.GetResourceNames();
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
			GUILayout.Label("Loaded [" + RenameName + "]");
            if (OpenedFileName != "")
            {
                DrawFile();
            }
            else if (OpenedFolderIndex != -1)
            {
                DrawSelectedFolder();
            }
            else
			{
				DrawResourcesMainGui();
            }
        }

		private void DrawResourcesMainGui() 
		{
			if (GUILayout.Button("UnLoad"))
			{
				// go back!
				DataManager.Get().UnloadAll();
				CloseAll();
			}

			DrawFolders();

			GUILayout.Space(30);
			IsExtraOptions = GUIToggle("Extra Options", IsExtraOptions);
			if (IsExtraOptions)
			{
				IncreaseIndent();
				if (GUIButton("Open Folder"))
				{
					FileUtil.OpenPathInWindows(DataManager.Get().GetMapPathNS());
				}
				if (GUIButton("Save All"))
				{
					DataManager.Get().SaveAll();
				}
				if (GUIButton("Erase"))
				{
					DataManager.Get().DeleteAll();
				}
				if (GUIButton("Generate"))
				{
					GameObject.Find("Generators").GetComponent<MapGenerator>().GenerateMap();
				}
				if (GUIButton("Generate TileMap"))
				{
					VoxelManager.Get().GenerateTileMap();
				}

				if (RenameName == "Null")
				{
					RenameName = DataManager.Get().MapName;
				}
				RenameName = GUIText(RenameName);
				if (GUIButton("Rename"))
				{
					if (RenameName != DataManager.Get().MapName)
					{
						RenameResourcesFolder(RenameName);
					}
				}
				DecreaseIndent();
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
			if (GUIButton("Back"))
			{
				OpenedFolderIndex = -1;
				OpenedFolderName = "";
			}
			GUILayout.Label("Opened Folder [" + OpenedFolderName + "] of Type: " + DataFolderNames.GetDataType(OpenedFolderName).ToString());

			// Show folder files
			if (OpenedFolder.CanSave() == false) 
			{
				GUI.enabled = false;
			}
			if (GUIButton("Save"))
			{
				OpenedFolder.SaveAllElements();
			}
			GUI.enabled = true;
			GUILayout.Space(30);

			DrawFolderFiles();

			GUILayout.Space(30);
			IsExtraOptions = GUIToggle("Extra Options", IsExtraOptions);
			if (IsExtraOptions)
			{
				if (GUIButton("New File"))
				{
					DataManager.Get().AddNew(OpenedFolderName);
				}

				if (GUIButton("Revert Folder To Last Saved"))
				{
					RevertFolder(OpenedFolderName);
				}

				if (GUIButton("Open Folder [" + OpenedFolderName + "] In Windows"))
				{
					string FolderPath = DataManager.Get().GetMapPathNS() + OpenedFolderName + "/";
					if (FileManagement.DirectoryExists(FolderPath, true, true) == false)
					{
						FileManagement.CreateDirectory(FolderPath, true);
					}
					FileUtil.OpenPathInWindows(FolderPath);
				}
                if (GUIButton("Force Save All Files"))
                {
                    OpenedFolder.ForceSaveAllElements();
                }
			}
			/*if (GUILayout.Button("New " + DataFolderNames.GetDataType(OpenedFolderName).ToString()))
			{
				ElementFolder MyFolder = DataManager.Get().GetElementFolder(OpenedFolderName);
				if (MyFolder != null)
				{
					MyFolder.AddNewElement();
				}
            }*/
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
			IncreaseIndent();
			if (IsOpenedElementFolder && OpenedFolder != null)
            {
                int ElementCount = 0;
                try
                {
					foreach (KeyValuePair<string, Element> MyKeyValuePair in OpenedFolder.Data)
                    {
						if (GUIButton(MyKeyValuePair.Key))
                        {
                            // open file
                            OpenedElement = MyKeyValuePair.Value;
                            OpenedFileName = MyKeyValuePair.Key;
                            OpenedFileIndex = ElementCount;
                            /*if (HasOpenedZexels())
                            {
                                OpenedTexture = MyKeyValuePair.Value as Zexel;
                            }*/
							OpenedElement.IsDrawGui = true;
                        }
                        ElementCount++;
                    }
                }
                catch (System.ObjectDisposedException)
                {

                }
			}
			DecreaseIndent();
        }

		/// <summary>
		/// Draws the folders.
		/// </summary>
        private void DrawFolders()
        {
			// Show folders
			List<ElementFolder> ElementFolders = DataManager.Get().GetFolders();
			GUILayout.Space(20);
			GUILabel("Resource Folders: " + ElementFolders.Count);
			IncreaseIndent();
            for (int i = 0; i < ElementFolders.Count; i++)
            {
				if (GUIButton(ElementFolders[i].FolderName + " [" + DataManager.Get().GetSize(ElementFolders[i].FolderName) + "]"))
                {
                    OpenedFolderName = ElementFolders[i].FolderName;
                    OpenedFolderIndex = i;
					OpenedFolder = ElementFolders[i];
                    IsOpenedElementFolder = true;
                }
            }
			DecreaseIndent();
        }
        #endregion

        #region DrawFiles

        private bool HasOpenedZexels()
        {
            return DataFolderNames.GetDataType(OpenedFolderName) == typeof(Zexel);
        }

        private void CloseFile()
        {
            OpenedFileName = "";
            OpenedElement.IsDrawGui = true;
            OpenedElement = null;
        }
        private void DrawFile()
		{
			if (GUIButton("Back"))
			{
                CloseFile();
			}
			if (OpenedElement == null)
			{
				GUILabel("Null Element");
				return;
			}
			GUILabel("File [" + OpenedFileName + "]"
				+ " Of Type [" + OpenedElement.GetType() + "] "
				+ " In Folder [" + OpenedFolderName + "]");
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
			GUILayout.Space(10);

			IsDrawAllFields = GUILayout.Toggle(IsDrawAllFields, "IsDrawAllFields");
			try
			{
				DrawFieldsForObject(OpenedElement as object, null, null, true);
			} 
			catch (System.StackOverflowException e) 
			{
				Debug.LogError("Overflow inside element: " + OpenedElement.Name + " - " + e.ToString());
			}
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
			IsExtraOptions = GUIToggle("Extra Options", IsExtraOptions);
			if (IsExtraOptions)
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
				if (GUIButton("Reload From File"))
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

				if (GUIButton("Check Size"))
				{
					long StopBytes = 0;
					long StartBytes = System.GC.GetTotalMemory(true);
					Element ClonedElement = OpenedElement.Clone();
					StopBytes = System.GC.GetTotalMemory(true);
					//System.GC.KeepAlive (ClonedElement); // This ensure a reference to object keeps object in memory
					#if UNITY_EDITOR
					int TotalBytes = (int)(StopBytes - StartBytes);
					int KiloBytes = TotalBytes / 1024;
					TotalBytes = TotalBytes % 1024;
					int MegaBytes = KiloBytes / 1024;
					KiloBytes = KiloBytes % 1024;

					UnityEditor.EditorUtility.DisplayDialog(OpenedElement.GetType().ToString() + " Size Check", 
						"Your element [" + ClonedElement.Name + "] is "
						+ MegaBytes.ToString() + " Megabytes, "
						+ KiloBytes.ToString() + " Kilobytes, "
						+ TotalBytes.ToString() + " Bytes in Size.",
						"Thanks");
					#endif
				}

                if (GUIButton("Clone"))
                {
                    string NewName = CloneName;
                    if (NewName == "")
                    {
                        NewName = NameGenerator.GenerateVoxelName();
                    }
                    Element Clone = OpenedElement.Clone();
                    Clone.SetNameOfClone(NewName);
                    OpenedFolder.AddElement(Clone);
                    CloseFile();
                }
                GUILabel("Clone As:");
                CloneName = GUIText(CloneName);
			}
        }

        private string CloneName = "";

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
            try
            {
#if UNITY_EDITOR
            return UnityEditor.EditorGUILayout.TextField(OldValue);
#else
            return GUILayout.TextField(OldValue);
#endif
            }
            catch (System.ArgumentException e)
            {
                Debug.LogError("DataGui GUIText: " + e.ToString());
                return "";
            }
        }

        public bool GUIToggle(string Label, bool OldValue)
        {
#if UNITY_EDITOR
            return UnityEditor.EditorGUILayout.Toggle(Label, OldValue);
#else
            return GUILayout.Toggle(OldValue, Label);
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

		public void IncreaseIndent() 
		{
			#if UNITY_EDITOR
				UnityEditor.EditorGUI.indentLevel++;
			#endif
		}

		public void DecreaseIndent()
		{
			#if UNITY_EDITOR
			UnityEditor.EditorGUI.indentLevel--;
			#endif
		}

		/// <summary>
		/// Dynamically creates a gui for an object
		/// </summary>
		public object DrawFieldsForObject(object MyObject, object ParentObject, FieldInfo MyField, bool IsFirstField = false)
        {
            if (MyObject == null)
            {
                return null;
            }
            var Fields = (MyObject.GetType()).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            object ReturnObject = null;
            Element MyElement = MyObject as Element;
            if (MyElement != null)
            {
                MyElement.IsDrawGui = GUIFoldout(MyElement.IsDrawGui, "[" + MyElement.Name + "] " + MyObject.GetType().ToString() + " -Parent? " + (MyElement.ParentElement != null).ToString());
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
			if (UnityEditor.EditorGUI.indentLevel > 5)
			{
				return ReturnObject;
			}
			#endif
			IncreaseIndent();

            // If has header
            if (MyField != null)
            {
                object[] MyAttributes = MyField.GetCustomAttributes(true);    // false
                for (int j = 0; j < MyAttributes.Length; j++)
                {
                    if (MyAttributes[j].GetType() == typeof(HeaderAttribute))
                    {
                        GUILabel(MyAttributes[j].ToString());
                        break;
                    }
                }
            }
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
                //for (int i = Fields.Length - 1; i >= 0; i--)
                for (int i = 0; i < Fields.Length; i++)
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
					bool IsIgnoreJson = HasJsonIgnore(Fields[i]);
					if (IsIgnoreJson)
                    {
                        // nothing
                    }
                    else if (value == null)
                    {
                        if (GUIButton("Spawn [" + Fields[i].Name + "]?"))
                        {
                            object NewValue = System.Activator.CreateInstance(Fields[i].FieldType);
                            Fields[i].SetValue(MyObject, NewValue);
                            MyElement = NewValue as Element;
                            if (MyElement != null)
                            {
                                MyElement.OnModified();
                                MyElement.ParentElement = ParentObject as Element;
                            }
                        }
                    }
                    // For normal fields!
                    else if (Fields[i] != null && Fields[i].IsStatic == false)
                    {
                        if (Fields[i].FieldType == typeof(string)
                            || Fields[i].FieldType == typeof(float)
                            || Fields[i].FieldType == typeof(int))
                        {
                            string OldValue = value.ToString();
                            GUILabel("[" + Fields[i].Name + "]: " + OldValue);
                            string NewValue = GUIText(OldValue);
                            if (OldValue != NewValue)
                            {
                                if (Fields[i].FieldType == typeof(float))
                                {
                                    Fields[i].SetValue(MyObject, float.Parse(NewValue));
                                }
                                else if (Fields[i].FieldType == typeof(int))
                                {
                                    Fields[i].SetValue(MyObject, int.Parse(NewValue));
                                }
                                else
                                {
                                    Fields[i].SetValue(MyObject, NewValue);
                                }
                                if (MyElement != null)
                                {
                                    MyElement.OnModified();
                                }
                            }
                        }
                        else if (Fields[i].FieldType == typeof(bool))
                        {
                            bool OldValue = (bool)value;
                            bool NewValue = GUIToggle(Fields[i].Name, OldValue);
                            if (NewValue != OldValue)
                            {
                                Fields[i].SetValue(MyObject, NewValue);
                                if (MyElement != null)
                                {
                                    MyElement.OnModified();
                                }
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
                                if (MyElement != null)
                                {
                                    MyElement.OnModified();
                                }
                            }
#endif
                        }
                        else if (Fields[i].FieldType == typeof(Vector3))
                        {
                            Vector3 OldValue = (Vector3)Fields[i].GetValue(MyObject);
#if UNITY_EDITOR
                                Vector3 NewValue = UnityEditor.EditorGUILayout.Vector3Field(Fields[i].Name + ": ", OldValue);
                                if (OldValue != NewValue)
                                {
                                    Fields[i].SetValue(MyObject, NewValue);
                                    if (MyElement != null)
                                    {
                                        MyElement.OnModified();
                                    }
                                }
#endif
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
                                if (MyElement != null)
                                {
                                    MyElement.OnModified();
                                }
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
                                if (MyElement != null)
                                {
                                    MyElement.OnModified();
                                }
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
                                if (MyElement != null)
                                {
                                    MyElement.OnModified();
                                }
                            }
                        }
                        else if (Fields[i].FieldType == typeof(List<bool>))
                        {
                            bool WasModified;
                            List<bool> OldValue = Fields[i].GetValue(MyObject) as List<bool>;
                            GUILabel(" List<bool> [" + Fields[i].Name + "]: " + OldValue.Count);
                            List<bool> NewValue = DrawListGui(OldValue, MyObject, Fields[i], out WasModified);
                            if (WasModified)
                            {
                                Fields[i].SetValue(MyObject, NewValue);
                                if (MyElement != null)
                                {
                                    MyElement.OnModified();
                                }
                            }
                        }
                        else if (Fields[i].FieldType == typeof(IntDictionary))
                        {
                            bool WasModified;
                            IntDictionary OldValue = Fields[i].GetValue(MyObject) as IntDictionary;
                            if (OldValue != null)
                            {
                                GUILabel(" IntDictionary [" + Fields[i].Name + "]: " + OldValue.Count);
                                IntDictionary NewValue = DrawIntDictionary(OldValue, MyObject, Fields[i], out WasModified);
                                if (WasModified)
                                {
                                    Fields[i].SetValue(MyObject, NewValue);
                                    if (MyElement != null)
                                    {
                                        MyElement.OnModified();
                                    }
                                }
                            }
                            else
                            {
                                GUILabel(" IntDictionary [" + Fields[i].Name + "] null!");
                            }
                        }
                        else if (Fields[i].FieldType == typeof(FloatDictionary))
                        {
                            bool WasModified;
                            FloatDictionary OldValue = Fields[i].GetValue(MyObject) as FloatDictionary;
                            if (OldValue != null)
                            {
                                GUILabel(" FloatDictionary [" + Fields[i].Name + "]: " + OldValue.Count);
                                FloatDictionary NewValue = DrawFloatDictionary(OldValue, MyObject, Fields[i], out WasModified);
                                if (WasModified)
                                {
                                    Fields[i].SetValue(MyObject, NewValue);
                                    if (MyElement != null)
                                    {
                                        MyElement.OnModified();
                                    }
                                }
                            }
                            else
                            {
                                GUILabel(" FloatDictionary [" + Fields[i].Name + "] null!");
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
                        else if (DrawListGui<Item>(Fields[i], MyObject))
                        {

                        }
                        else if (DrawListGui<Stat>(Fields[i], MyObject))
                        {

                        }
                        else if (DrawListGui<Bone>(Fields[i], MyObject))
                        {

                        }
                        else if (DrawListGui<ZeltexTransformCurve>(Fields[i], MyObject))
                        {

                        }
                        // Draw for elements!
                        if (!IsIgnoreJson && 
                            (Fields[i].FieldType.BaseType == typeof(Element) || Fields[i].FieldType.BaseType == typeof(ElementCore)))
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
                    ReturnObject = DrawElementGui(MyElement, IsFirstField, ParentObject, MyField, ReturnObject);

                    DrawSpawnGUI<Level>(MyElement);
                    DrawSpawnGUI<CharacterData>(MyElement);
                    DrawSpawnGUI<SaveGame>(MyElement);

                    DrawZexelGui(MyElement);
                    DrawSpawnGUI<VoxelMeta>(MyElement);
                    DrawSpawnGUI<PolyModel>(MyElement);
                    DrawVoxelModelGUI(MyElement);
                    DrawSpawnGUI<Skeleton>(MyElement);
                    DrawSpawnGUI<Zanimation>(MyElement);

                    DrawSpawnGUI<Item>(MyElement);
                    DrawSpawnGUI<Spell>(MyElement);
                    DrawSpawnGUI<ZoneData>(MyElement);
                    DrawSpawnGUI<Lore>(MyElement);
                    DrawSpawnGUI<Quests.Quest>(MyElement);
                    DrawSpawnGUI<Dialogue.DialogueTree>(MyElement);

                    if (MyElement.GetType() == typeof(Stat))
                    {
                        Stat MyStat = MyElement as Stat;
                        GUILabel("-----");
                        GUILabel(MyStat.GetGuiString());
                        GUILabel("-----");
                        if (MyStat.GetStatType() != StatType.Base
                            && GUIButton("Change To Type [" + StatType.Base + "]"))
                        {
                            MyStat.SetValuesAsType(StatType.Base);
                        }
                        if (MyStat.GetStatType() != StatType.Modifier
                            && GUIButton("Change To Type [" + StatType.Modifier + "]"))
                        {
                            MyStat.SetValuesAsType(StatType.Modifier);
                        }
                        if (MyStat.GetStatType() != StatType.Regen
                            && GUIButton("Change To Type [" + StatType.Regen + "]"))
                        {
                            MyStat.SetValuesAsType(StatType.Regen);
                        }
                        if (MyStat.GetStatType() != StatType.State
                            && GUIButton("Change To Type [" + StatType.State + "]"))
                        {
                            MyStat.SetValuesAsType(StatType.State);
                        }
                        if (MyStat.GetStatType() != StatType.TemporaryModifier
                            && GUIButton("Change To Type [" + StatType.TemporaryModifier + "]"))
                        {
                            MyStat.SetValuesAsType(StatType.TemporaryModifier);
                        }
                        if (MyStat.GetStatType() != StatType.TemporaryRegen
                            && GUIButton("Change To Type [" + StatType.TemporaryRegen + "]"))
                        {
                            MyStat.SetValuesAsType(StatType.TemporaryRegen);
                        }
                    }
                }
                if (MyElement != null && MyElement.ParentElement != null)
                {
                    GUILayout.Space(6);
                    if (GUIButton("Erase [" + MyElement.Name + "] to become Null"))
                    {
                        MyField.SetValue(ParentObject, null);
                    }
                    GUILayout.Space(6);
                }
            }
			DecreaseIndent();
            return ReturnObject;
        }
        
        private object DrawElementGui(Element MyElement, bool IsFirstField, object ParentObject, FieldInfo MyField, object ReturnObject)
        {
            if (MyElement.ParentElement != null)
            {
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
                        MyElement.ElementLink = DataFolderNames.DataTypeToFolderName(MyElement.GetType());
                    }
                }
                GUILabel("Pull Data From DataManager");
                if (GUIButton("From [" + MyElement.ElementLink + "]"))
                {
                    ReturnObject = PullElement(MyElement, MyField, ParentObject, ReturnObject);
                }
                GUILabel("Push Data To the DataManager");
                if (GUIButton("Push To [" + MyElement.ElementLink + "]"))
                {
                    //Element MyValue = Fields[i].GetValue(MyObject) as Element;
                    DataManager.Get().PushElement(MyElement.ElementLink, MyElement);
                }
            }
            return ReturnObject;
        }

        /// <summary>
        /// Pulls one element over another, needs fieldinfo and objects in order to set the value
        /// </summary>
        private object PullElement(Element MyElement, FieldInfo MyField, object ParentObject, object ReturnObject)
        {
            //Element OldValue = Fields[i].GetValue(MyObject) as Element;
            //Debug.Log(MyElement.Name + " is being overwritten from an element in database folder: " + MyElement.ElementLink);
            Element DataClone = DataManager.Get().GetElement(MyElement.ElementLink, MyElement.Name);
            if (DataClone != null)
            {
                DataClone = DataClone.Clone();
                ElementFolder MyFolder = MyElement.MyFolder;
                DataClone.IsDrawGui = MyElement.IsDrawGui;
                DataClone.MyFolder = MyElement.MyFolder;
                DataClone.ElementLink = MyElement.ElementLink;
                DataClone.ParentElement = MyElement.ParentElement;
                DataClone.ResetName();
                if (MyFolder != null)
                {
                    MyFolder.SetElement(DataClone);
                }
                if (MyField != null)
                {
                    try
                    {
                        MyField.SetValue(ParentObject, DataClone);
                        Debug.Log(MyElement.Name + " is using MyField.SetValue");
                    }
                    catch (System.ArgumentException e)
                    {
                        // From a list of elements
                        Debug.LogError(e.ToString());
                        Debug.LogError(DataClone.GetType() + " compared to: " + MyElement.GetType() + " Field: " + MyField.Name + " of object " + MyElement.ToString());
                        ReturnObject = DataClone as object;
                        Debug.Log(MyElement.Name + " is using ReturnObject to a list");
                    }
                    DataClone.OnModified();
                }
                else
                {
                    Debug.LogError("Could not pull element as FieldInfo is null.");
                }
                /*else if (IsFirstField)
                {
                    OpenedFolder.SetElement(NewElement);
                }*/
            }
            else
            {
                Debug.LogError("Could not find file " + MyElement.Name + " in folder " + MyElement.ElementLink);
            }
            return ReturnObject;
        }

        private void DrawVoxelModelGUI(Element MyElement)
        {
            if (MyElement.GetType() == typeof(VoxelModel))
            {
                VoxelModel MyVoxelModel = MyElement as VoxelModel;
                if (MyVoxelModel != null)
                {
                    if (GUIButton("Import"))
                    {
                        DataManager.Get().ImportVox(MyVoxelModel);
                    }
                }
                if (MyElement.ParentElement == null) 
                {
                    DrawSpawnGUI<VoxelModel>(MyElement);
                }
            }
        }

        private void DrawZexelGui(Element MyElement)
        {
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

        private void DrawLevelGui(Element MyElement)
        {
            if (MyElement.GetType() == typeof(Level))
            {
                Level MyLevel = MyElement as Level;
				if (!MyLevel.HasSpawned())
                {
                    if (GUIButton("Spawn"))
                    {
                        MyLevel.Spawn();
                    }
                }
                else
                {
                    if (GUIButton("DeSpawn"))
                    {
                        MyLevel.DeSpawn();
                    }
                }
            }
        }

		private void DrawSkeletonGui(Element MyElement)
		{
			if (MyElement.GetType() == typeof(Skeletons.Skeleton))
			{
				Skeletons.Skeleton MySkeleton = MyElement as Skeletons.Skeleton;
				if (!MySkeleton.HasSpawned())
				{
					if (GUIButton("Spawn"))
					{
						MySkeleton.Spawn();
					}
				}
				else
				{
					if (GUIButton("DeSpawn"))
					{
						MySkeleton.DeSpawn();
					}
				}
			}
		}

        private string SpawnZanimationWithSkeletonName = "";
        private string SpawnInLevelName = "";
        private void DrawSpawnGUI<T>(Element MyElement) where T : Element
		{
			if (MyElement.GetType() == typeof(T))
			{
				T MyElementConverted = MyElement as T;
				if (!MyElementConverted.HasSpawned())
				{
					if (GUIButton("Spawn"))
					{
						MyElementConverted.Spawn();
					}
                    if (typeof(T) == typeof(Zanimation))
                    {
                        GUILabel("Spawn with Skeleton:");
                        SpawnZanimationWithSkeletonName = GUIText(SpawnZanimationWithSkeletonName);
                        if (GUIButton("Spawn With Skeleton"))
                        {
                            Zanimation MyZanimation = MyElementConverted as Zanimation;
                            MyZanimation.SpawnWithSkeleton(SpawnZanimationWithSkeletonName);
                        }
                    }
                    else if (typeof(T) == typeof(ZoneData))
                    {
                        GUILabel("Spawn in Level:");
                        SpawnInLevelName = GUIText(SpawnInLevelName);
                        if (GUIButton("Spawn in " + SpawnInLevelName))
                        {
                            ZoneData Data = MyElementConverted as ZoneData;
                            Data.SpawnInLevel(SpawnInLevelName);
                        }
                    }
                    else if (typeof(T) == typeof(Level))
                    {
                        GUILabel("Characters Spawn with Level:");
                        Level MyLevel = MyElementConverted as Level;
                        for (int i = 0; i < MyLevel.CharacterNames.Count; i++)
                        {
                            if (MyLevel.CanSpawnCharacterInEditor(i))
                            {
                                if (GUIButton("Spawn Character [" + MyLevel.CharacterNames[i] + "]"))
                                {
                                    MyLevel.SpawnCharacterInEditor(i);
                                }
                            }
                            else
                            {
                                if (GUIButton("Despawn Character [" + MyLevel.CharacterNames[i] + "]"))
                                {
                                    MyLevel.DespawnCharacterInEditor(i);
                                    
                                }
                            }
                        }
                    }
                }
				else
				{
					if (GUIButton("DeSpawn"))
					{
						MyElementConverted.DeSpawn();
					}
				}
			}
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
            return HasAttribute(MyField, typeof(Newtonsoft.Json.JsonIgnoreAttribute));
        }

        public bool HasAttribute(FieldInfo MyField, System.Type AttributeType)
        {
            object[] attributes = MyField.GetCustomAttributes(true);	// false

            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i].GetType() == AttributeType)
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
            List<bool> ListBool = MyList as List<bool>;
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
                else if (typeof(T) == typeof(bool))
                {
                    ListBool.Add(false);
                }
                else if (typeof(T).BaseType == typeof(Element) || typeof(T).BaseType == typeof(ElementCore))
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
                else if (typeof(T) == typeof(bool))
                {
                    bool NewValue = GUIToggle(MyField.Name + "[" + j + "]", (bool)ListBool[j]);
                    if (ListBool[j] != NewValue)
                    {
                        ListBool[j] = NewValue;
                        WasModified = true;
                    }
                }
                else if (typeof(T).BaseType == typeof(Element) || typeof(T).BaseType == typeof(ElementCore))
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
            else if (typeof(T) == typeof(bool))
            {
                MyList = ListBool as List<T>;
            }
            else if (typeof(T).BaseType == typeof(Element) || typeof(T).BaseType == typeof(ElementCore))
            {
                MyList = ListElements as List<T>;
            }
            return MyList;
        }

        Int3 GUIInt3(Int3 OldInt3)
        {
            Rect OldRect = GUILayoutUtility.GetRect(new GUIContent("Blargnugg"), GUI.skin.button);
            OldRect.position = new Vector2(OldRect.position.x + GUIIndentPositionX(), OldRect.position.y);
            OldRect.width -= GUIIndentPositionX();
            Rect PartA = new Rect(OldRect.x + OldRect.width * 1f / 4f, OldRect.y, OldRect.width / 4f, OldRect.height);
            Rect PartB = new Rect(OldRect.x + OldRect.width * 2f / 4f, OldRect.y, OldRect.width / 4f, OldRect.height);
            Rect PartC = new Rect(OldRect.x + OldRect.width * 3f / 4f, OldRect.y, OldRect.width / 4f, OldRect.height);
            Int3 NewInt3 = new Int3();
            NewInt3.x = int.Parse(GUI.TextField(PartA, OldInt3.x.ToString()));
            NewInt3.y = int.Parse(GUI.TextField(PartB, OldInt3.y.ToString()));
            NewInt3.z = int.Parse(GUI.TextField(PartC, OldInt3.z.ToString()));
            return NewInt3;
        }

        string GUIInt3Text(string OldText) 
        {
            Rect OldRect = GUILayoutUtility.GetLastRect();//(new GUIContent("Blargnugg"), GUI.skin.button);
            OldRect.position = new Vector2(OldRect.position.x + GUIIndentPositionX(), OldRect.position.y);
            OldRect.width -= GUIIndentPositionX();
            //Rect PartA = new Rect(OldRect.x, OldRect.y, OldRect.width / 4f, OldRect.height);
            return GUI.TextField(OldRect, OldText);
        }

        public FloatDictionary DrawFloatDictionary(FloatDictionary MyDictionary, object MyObject, FieldInfo MyField, out bool WasModified)
        {
            WasModified = false;
            MyDictionary.IsEditing = GUIToggle("Is Editing?", MyDictionary.IsEditing);
            if (GUIButton("Add"))
            {
                MyDictionary.Add(Random.Range(0f, 1f), 0);
                WasModified = true;
            }

            if (MyDictionary != null)
            {
                try
                {
                    foreach (KeyValuePair<float, float> MyPair in MyDictionary)
                    {
                        if (MyDictionary.IsEditing == false)
                        {
                            GUILabel(MyPair.Key + ":" + MyPair.Value);
                        }
                        else
                        {
                            float NewKey = float.Parse( GUIText(MyPair.Key.ToString()));
                            if (MyPair.Key != NewKey)
                            {
                                MyDictionary.Remove(MyPair.Key);
                                MyDictionary.Add(NewKey, MyPair.Value);
                                WasModified = true;
                            }
                            else
                            {
                                float NewValue = float.Parse(GUIText(MyPair.Value.ToString()));
                                if (MyPair.Value != NewValue)
                                {
                                    MyDictionary[MyPair.Key] = NewValue;
                                    WasModified = true;
                                }
                            }
                        }
                    }
                }
                catch (System.ObjectDisposedException)
                {
                    //Debug.LogError("DrawIntDictionary: " + e.ToString());
                }
            }
            return MyDictionary;
        }

        public IntDictionary DrawIntDictionary(IntDictionary MyDictionary, object MyObject, FieldInfo MyField, out bool WasModified) 
        {
            WasModified = false;
            MyDictionary.IsEditing = GUIToggle("Is Editing?", MyDictionary.IsEditing);
            if (GUIButton("Add"))
            {
                MyDictionary.Add(new Int3(0, 0, Random.Range(-100, -1)), "Empty");
                WasModified = true;
            }

            if (MyDictionary != null)
            {
                try
                {
                    foreach (KeyValuePair<Int3, string> MyPair in MyDictionary)
                    {
                        if (MyDictionary.IsEditing == false)
                        {
                            GUILabel(MyPair.Key + ":" + MyPair.Value);
                        }
                        else
                        {
                            Int3 NewKey = GUIInt3(MyPair.Key);
                            if (MyPair.Key != NewKey)
                            {
                                MyDictionary.Remove(MyPair.Key);
                                MyDictionary.Add(NewKey, MyPair.Value);
                                WasModified = true;
                            }
                            else
                            {
                                string NewValue = GUIInt3Text(MyPair.Value);
                                if (MyPair.Value != NewValue)
                                {
                                    MyDictionary[MyPair.Key] = NewValue;
                                    WasModified = true;
                                }
                            }
                        }
                    }
                } 
                catch (System.ObjectDisposedException)
                {
                    //Debug.LogError("DrawIntDictionary: " + e.ToString());
                }
            }
            return MyDictionary;
        }
#endregion
    }
}