using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using ZeltexTools;
using System.IO;
using Zeltex.Voxels;
using Zeltex.Util;

namespace Zeltex.Guis.Maker
{
    // TextureMap Maker ToDo:
    // Store UVs in a texturemap class
    // Generate normal UVs from texture map
    // Generate Tilemap just with that tile list
    // Load TextureMap using Viewer

    // Use auto generated ones as per normal
    // Save/Load them
    // Load them as gui overlays, on top of the texture
    // Link them to multiple tiles
    // Controller code for looking at different tiles
    // The Selected Vertex to highlight the selected UV

    //  POLYGONAL MODELS PER VOXEL
    /// <summary>
    /// Edits polygonal models for the voxels
    /// </summary>
    public class PolygonMaker : ElementMakerGui
    {
        #region Variables
        [Header("PolygonMaker")]
        public VoxelPolygonViewer MyViewer;
        #endregion

        #region DataManager

        /// <summary>
        /// Use the folder name!
        /// </summary>
        protected override void SetFilePaths()
        {
            DataManagerFolder = DataFolderNames.PolyModels;
        }

        /// <summary>
        /// Returns the selected PolyModel
        /// </summary>
        public PolyModel GetSelectedModel()
        {
            //Debug.LogError("Getting model name: " + GetSelectedName() + " at " + GetSelectedIndex() + " out of " + GetSize());
            return DataManager.Get().GetElement(DataManagerFolder, GetSelectedIndex()) as PolyModel;
            //return VoxelManager.Get().GetModel(GetSelectedName());
        }

        /// <summary>
        /// returns the model by a name
        /// </summary>
        public PolyModel GetModel(string MyName)
        {
            return DataManager.Get().GetElement(DataManagerFolder, GetSelected()) as PolyModel;
            //return VoxelManager.Get().GetModel(GetSelectedName());
        }

        /// <summary>
        /// Add a new voxel model
        /// </summary>
        protected override void AddData()
        {
            PolyModel NewModel = new PolyModel();
            NewModel.Name = "M" + Random.Range(1, 10000);
            NewModel.GenerateCubeMesh();
            NewModel.GenerateSolidity();
            DataManager.Get().AddElement(DataManagerFolder, NewModel);
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
            // Default texture map is 0
            PolyModel MyModel = GetSelectedModel();
            if (MyModel != null)
            {
                MyViewer.GetSpawn().GetComponent<PolyModelHandle>().LoadVoxelMesh(MyModel);
                //MyViewer.LoadVoxelMesh(MyModel, 0);
                UpdateStatistics();
            }
            else
            {
                Debug.LogError("Model is null at: " + NewIndex);
            }
            CancelInvokes();
        }

		/// <summary>
		/// When an index is removed
		/// </summary> 
		protected override void RemovedData(int Index)
		{
            MyViewer.ClearMesh();
            VoxelManager.Get().RemoveModel(Index);
			base.RemovedData(Index);	// alsso remove palceholder string
        }
        /// <summary>
        /// Called by ZelGui class when turning on gui
        /// </summary>
        public override void OnBegin()
        {
            MyViewer.OnBegin();
            base.OnBegin();
            if (VoxelManager.Get().DiffuseTextures.Count > 0)
            {
                if (GetDropdown("TexturesDropdown"))
                {
                    GetDropdown("TexturesDropdown").value = 0;
                }
                if (GetImage("TexturesImage") && VoxelManager.Get().DiffuseTextures.Count > 0)
                {
                    GetImage("TexturesImage").texture = VoxelManager.Get().DiffuseTextures[0];
                }
            }
            Debug.LogError("PolygonMaker has " + GetSize() + " elements.");
		}
        /// <summary>
        /// Called by ZelGui class when turning off gui
        /// </summary>
        public override void OnEnd()
        {
            base.OnEnd();
            MyViewer.OnEnd();
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
            base.UseInput(MyButton);
            if (MyButton.name == "ImportButton")
            {
                //MyVoxelGui.VoxelType = MyDropdown.value;

                //DataManager.Get().ImportImage(DataManagerFolder, GetSelectedIndex());
                //MyTextureEditor.texture = GetSelectedTexture();
            }
            else if (MyButton.name == "ExportButton")
            {
                //MyViewer.GetSpawn().GetComponent<PolyModelHandle>().GetMesh()
                //DataManager.Get().ExportPolygon(MyViewer.GetSpawn().GetComponent<MeshFilter>());
                //FileUtil.Export(GetSelectedName(), FileExtension, FileUtil.ConvertToSingle(GetSelected().GetScript()));
            }
        }

        public override void UseInput(Toggle MyToggle)
        {
            if (MyToggle.name == "DebugUVs")
            {
                UpdateStatistics();
            }
            else if (MyToggle.name == "DebugVerticies")
            {
                UpdateStatistics();
            }
        }

        /// <summary>
        /// Called when voxeldiffuse textures names change
        /// </summary>
        public void RefreshTexturesDropdown()
        {
            FillDropdown(GetDropdown("TexturesDropdown"));
        }
        #endregion

        #region ImportExport

        /// <summary>
        /// called on mouse down
        /// </summary>
        public void Import()
        {
            //FileUtil.Import(name, "UploadPolygon", FileExtension);
        }
        /// <summary>
        /// Called from javascript, uploading a model data
        /// </summary>
        public void UploadPolygon(string MyData)
        {
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
                Debug.Log("Uploading new polygon:" + UploadFileName + ":" + MyData.Length);
                //AddData2(UploadFileName, MyData);
            }
        }
        /// <summary>
        /// Add a new data!
        /// </summary>
        /*public void AddData2(string MyName, string MyScript)
        {
            for (int i = 0; i < GetSize(); i++)
            {
                if (GetName(i) == MyName)
                {
                    return;
                }
            }
            AddName(MyName);
            PolyModel NewModel = new PolyModel();
            NewModel.RunScript(FileUtil.ConvertToList(MyScript));
            VoxelManager.Get().AddModel(NewModel);
            MyIndexController.SetMaxSelected(GetSize());
        }*/
        
        #endregion

        #region UVs
        public void AddTextureCoordinates()
        {
            int MetaToAdd = 0;
            for (int i = 0; i < 6; i++)
            {
				GetSelectedModel().MyModels[i].TextureCoordinates.Clear();
				GetSelectedModel().MyModels[i].AddQuadUVs(
                    MetaToAdd,
                    8,
                    World.TextureResolution,
                    i);
            }
        }
        public void ClearUVs()
        {
            for (int i = 0; i < 6; i++)
            {
				GetSelectedModel().MyModels[i].TextureCoordinates.Clear();
            }
        }
        // Mesh Starter Functions
        #endregion

        #region PolyModelHelpers
        #endregion

        #region Statistics
        private int GetSelectedTextureMap()
        {
            return 0;
        }
        /// <summary>
        /// Updates the debug info
        /// </summary>
        private void UpdateStatistics()
        {
            bool IsDebugVerticies = GetToggle("DebugVerticies").isOn;
            bool IsDebugUVs = GetToggle("DebugUVs").isOn;
            Text MyStatistics = GetLabel("StatisticsGui");
            MyStatistics.text = "";
            MyStatistics.text += "Model: " + GetSelectedName() + "\n";
            MeshData CombinedMesh = GetSelectedModel().GetCombinedMesh(GetSelectedTextureMap());
            MyStatistics.text += "Triangle Index Count: " + CombinedMesh.Triangles.Count + "\n";
            MyStatistics.text += "Vertex Count [" + CombinedMesh.Verticies.Count + "]\n";
            //MyStatistics.text += "Normals: AutoGenerated\n";
            //MyStatistics.text += "Colors: " + CombinedMesh.Colors.Count + "\n";
            MyStatistics.text += "UV Count [" + CombinedMesh.TextureCoordinates.Count + "]\n";
           // MyStatistics.text += "TextureMaps: " + GetSelected().TextureMaps.Count + "\n";
            MyStatistics.text += "  Selected Texture Map [" + GetSelectedTextureMap() + "] out of [" + GetSelectedModel().TextureMaps.Count + "]\n";
            // Show texture names used
            if (GetSelectedModel().TextureMaps.Count > 0)
            {
                PolyTextureMap MyTextureMap = GetSelectedModel().TextureMaps[GetSelectedTextureMap()];
                List<string> TileMapNames;
                List<int> TileMapCounts;
                MyTextureMap.GetTileMapInfo(out TileMapNames, out TileMapCounts);
                MyStatistics.text += "Selected TextureMap has [" + TileMapNames.Count + "] Textures used\n";
                for (int i = 0; i < TileMapNames.Count; i++)
                {
                    MyStatistics.text += "Texture Name is [" + TileMapNames[i] + "] Count of [" + TileMapCounts[i] + "]\n";
                }
                /*if (MyTextureMap != null && MyTextureMap.Coordinates.Count > 0)
                {
                    string MyTextureName = MyTextureMap.Coordinates[0].TileName;
                    MyStatistics.text += 0 + " - Tile Name [" + MyTextureName + "]" + "\n";
                    for (int i = 1; i < MyTextureMap.Coordinates.Count; i++)
                    {
                        if (MyTextureMap.Coordinates[i].TileName != MyTextureName)
                        {
                            MyTextureName = MyTextureMap.Coordinates[i].TileName;
                            MyStatistics.text += i + " - Tile Name [" + MyTextureName + "]" + "\n";
                        }
                    }
                }*/
            }
            if (IsDebugVerticies)
            {
                for (int i = 0; i < 6; i++)
                {
                    MyStatistics.text += "Side [" + i + "] Verticies " + GetSelectedModel().MyModels[i].Verticies.Count + "\n";
                    for (int j = 0; j < GetSelectedModel().MyModels[i].Verticies.Count; j++)
                    {
                        MyStatistics.text += "  " + GetSelectedModel().MyModels[i].Verticies[j].ToString() + " ";
                        if (j != GetSelectedModel().MyModels[i].Verticies.Count - 1)
                        {
                            MyStatistics.text += ", ";
                        }
                        else
                        {
                            MyStatistics.text += ".\n";
                        }
                    }
                }
            }
            if (IsDebugUVs && GetSelectedModel().TextureMaps.Count > 0)
            {
                PolyTextureMap MyTextureMap = GetSelectedModel().TextureMaps[GetSelectedTextureMap()];
                if (MyTextureMap != null)
                {
                    for (int i = 0; i < MyTextureMap.Coordinates.Count; i++)
                    {
                        MyStatistics.text += i + " - Data [" + MyTextureMap.Coordinates[i].TileName + ", "
                            + MyTextureMap.Coordinates[i].MyCoordinate.ToString() + "]" + "\n";
                    }
                }
            }
        }
        #endregion
    }
}
/// <summary>
/// Load all the polygonal models!
/// </summary>
/* public override void LoadAll()
 {

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
             string MyName = Path.GetFileNameWithoutExtension(MyFiles[i]);
             AddName(MyName);
             PolyModel NewModel = new PolyModel();
             NewModel.Clear();
             List<string> MyLines = FileUtil.ConvertToList(FileUtil.Load(MyFiles[i]));
             NewModel.RunScript(MyLines);
             NewModel.GenerateSolidity();
             NewModel.Name = MyName;
             MyVoxelManager.AddModel(NewModel);
         }
     }
     else
     {
         // Wait this needs to be done in data manager!
        for (int i = 0; i < GetSize(); i++)
         {
             PolyModel NewModel = new PolyModel();
             NewModel.RunScript(FileUtil.ConvertToList(GetData(i)));
             NewModel.GenerateSolidity();
             NewModel.Name = GetName(i);
             MyVoxelManager.AddModel(NewModel);
         }
     }
 }
 /// <summary>
 /// Save all the polygonal models!
 /// </summary>
 public override void SaveAll()
 {
     Debug.Log("Saving polygonal models to: " + GetFilePath());
     for (int i = 0; i < GetSize(); i++)
     {
         PolyModel MyModel = MyVoxelManager.GetModel(i);
         string MyFilePath = GetFilePath() + GetName(i) + "." + FileExtension;
         //Debug.Log("Saving Poly " + i + " to: " + MyFilePath);
         List<string> MyScript = MyModel.GetScript();
         string MySingle = FileUtil.ConvertToSingle(MyScript);
         FileUtil.Save(MyFilePath, MySingle);
     }
 }*/
