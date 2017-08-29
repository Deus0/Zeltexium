using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Voxels;
using System.IO;
using ZeltexTools;
using Zeltex.Util;
using Zeltex.Generators;

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// Manages the gui interface for the Texture Editor class
    /// Managers files for the texture editor class
    /// </summary>
    public class TextureMaker : ElementMakerGui
    {
        #region Variables
        //public static string Path1 = "VoxelTexturesDiffuse";
        public static string Path2 = "VoxelTexturesNormals";
        public static string Path3 = "ItemTextures";
        public static string Path4 = "StatTextures";
        [Header("Reference")]
        //public VoxelManager MyVoxelManager;
        //public World MyWorld;
        //public PolygonMaker MyPolygonMaker; // refresh when texture name changes
        [SerializeField]
        private RawImage MyTextureEditor;
        public int TileResolution = 8;
        // For copying
        private Color32[] MyCopyPixels = null;
        #endregion

        #region FolderSwitching

        public string GetTextureFolderName(int PathIndex)
        {
            if (PathIndex == 0)
            {
                return DataFolderNames.VoxelDiffuseTextures;
            }
            if (PathIndex == 1)
            {
                return DataFolderNames.VoxelNormalTextures;
            }
            if (PathIndex == 2)
            {
                return DataFolderNames.ItemTextures;
            }
            if (PathIndex == 3)
            {
                return DataFolderNames.StatTextures;
            }
            return "";
        }

        public void SetFolder(string NewFolderName)
        {
            if (DataManagerFolder != NewFolderName)
            {
                DataManagerFolder = NewFolderName;
                if (NewFolderName == DataFolderNames.VoxelDiffuseTextures)
                {
                    GetDropdown("FilePathDropdown").value = 0;
                }
                else if (NewFolderName == DataFolderNames.VoxelNormalTextures)
                {
                    GetDropdown("FilePathDropdown").value = 1;
                }
                else if (NewFolderName == DataFolderNames.ItemTextures)
                {
                    GetDropdown("FilePathDropdown").value = 2;
                }
                else if (NewFolderName == DataFolderNames.StatTextures)
                {
                    GetDropdown("FilePathDropdown").value = 3;
                }
                RefreshFilesList();
                MyIndexController.ForceSelect(0);
            }
        }
        #endregion

        #region DataManager
        /// <summary>
        /// Set file paths!
        /// </summary>
        protected override void SetFilePaths()
        {
            DataManagerFolder = DataFolderNames.VoxelDiffuseTextures;
        }

        public override int GetSize()
		{
			return DataManager.Get().GetSizeElements(DataManagerFolder);
		}


        /// <summary>
        /// Returns the selected script
        /// </summary>
        public Texture2D GetSelectedTexture()
        {
            return (DataManager.Get().GetElement(DataManagerFolder, GetSelectedIndex()) as Zexel).GetTexture();
        }


        /// <summary>
        /// Add a new model to our list
        /// </summary>
        protected override void AddData()
        {
            Vector2 TextureSize = new Vector2(16, 16);
            if (GetSelectedTexture() != null)
            {
                TextureSize = GetSelectedTexture().texelSize;
                Debug.Log("Current texture size: " + TextureSize.ToString());
            }
            VoxelManager.Get().SetTextureSize(TextureSize);

            Texture2D MyTexture = new Texture2D(
                (int)VoxelManager.Get().GetTextureSize().x, 
                (int)VoxelManager.Get().GetTextureSize().y, 
                TextureFormat.RGBA32, 
                false);
            MyTexture.filterMode = FilterMode.Point;
            MyTexture.wrapMode = TextureWrapMode.Clamp;
            MyTexture.name = "T " + Mathf.RoundToInt(Random.Range(1, 10000));
            Debug.Log("Adding new texture: " + MyTexture.name);
            DataManager.Get().AddTexture(DataManagerFolder, MyTexture);
        }

        /// <summary>
        /// Save the folder!
        /// </summary>
        public override void SaveAll()
        {
            DataManager.Get().SaveTextures(DataManagerFolder);
        }

        public override void Delete()
        {
            DataManager.Get().RemoveTexture(DataManagerFolder, GetSelectedIndex());
        }
        #endregion

        #region Data

        /// <summary>
        /// Stores the data in a clipboard
        /// </summary>
        public void Copy()
        {
            MyCopyPixels = MyTextureEditor.GetComponent<TextureEditor>().GetPixelColors();
        }

        /// <summary>
        /// Pastes the data from the clipboard!
        /// </summary>
        public void Paste()
        {
            if (MyCopyPixels != null)
            {
                //MyTextureEditor.GetComponent<TextureEditor>().PaintPixels(MyCopyPixels);
                //SetPixels(MyTexture, PixelColors);
            }
        }
		#endregion

		#region IndexController

		/// <summary>
		/// Called by ZelGui class when turning on gui
		/// </summary>
		public override void OnBegin()
        {
            base.OnBegin();
            RefreshFilesList();
        }

        /// <summary>
        /// Load Gui with Input
        /// Only gets called if index is updated
        /// </summary>
        public override void OnUpdatedIndex(int NewIndex)
        {
            base.OnUpdatedIndex(NewIndex);
            GetInput("NameInput").text = GetSelectedName();
            MyTextureEditor.texture = GetSelectedTexture();
            CancelInvokes();
        }

        /// <summary>
        /// When list is empty, set interactivity of input off
        /// </summary>
        public override void OnListEmpty()
        {
            base.OnListEmpty();
            GetDropdown("FilePathDropdown").interactable = true;
            MyTextureEditor.texture = null;
        }

		/// <summary>
		/// When an index is removed
		/// </summary>
		protected override void RemovedData(int Index)
		{
            //MyViewer.ClearMesh();
            //MyDataBase.MyModels.RemoveAt(NewIndex);
            Debug.Log("Deletng texture: " + GetSelectedName());
			DataManager.Get().RemoveTexture(DataManagerFolder, Index);
        }
        #endregion

        #region Files
        /// <summary>
        /// Refreshes the files list
        /// </summary>
        private void RefreshFilesList()
        {
            GuiList FilesList = GetList("FilesList");
            if (FilesList)
            {
                FilesList.Clear();
                FilesList.AddRange(DataManager.Get().GetNames(DataManagerFolder));
            }
            MyIndexController.SetMaxSelected(GetSize());
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
                string OldName = GetSelectedName();
                MyInputField.text = Rename(MyInputField.text);
                if (OldName != MyInputField.text)
                {
                    // Rename all the texture maps too
                    if (DataManagerFolder == DataFolderNames.VoxelDiffuseTextures)
                    {
                        //Debug.Log("Renaming texture for voxel models: " + GetSelected().name + " to " + MyInputField.text);
                        for (int i = 0; i < VoxelManager.Get().MyModels.Count; i++)
                        {
                            VoxelManager.Get().GetModel(i).RenameTexture(OldName, MyInputField.text);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Used for generically updating buttons
        /// Move these functions to TexturePainter
        /// </summary>
        public override void UseInput(Button MyButton)
        {
            base.UseInput(MyButton);
            if (MyButton.name == "CopyButton")
            {
                Copy();
            }
            else if (MyButton.name == "PasteButton")
            {
                Paste();
            }
            else if (MyButton.name == "ExportButton")
            {
                //byte[] ByteData = MyTextures[i].();
                //FileUtil.ExportImage(GetSelectedName(), FileExtension, System.Convert.ToBase64String(GetSelected().EncodeToPNG()));
            }
            else if (MyButton.name == "ImportButton")
            {
                DataManager.Get().ImportImage(DataManagerFolder, GetSelectedIndex());
                MyTextureEditor.texture = GetSelectedTexture();

            }
        }

        public override void UseInput(Dropdown MyDropdown)
        {
            if (MyDropdown.name == "FilePathDropdown")
            {
                SetFolder(GetTextureFolderName(MyDropdown.value));
            }
        }
        #endregion

        #region Utility

        public void GenerateTileMap()
        {
            TileMap NewMap = new TileMap();
            VoxelManager.Get().UpdateTileMap(NewMap.CreateTileMap(VoxelManager.Get().DiffuseTextures, TileResolution));
        }
        #endregion
    }
}