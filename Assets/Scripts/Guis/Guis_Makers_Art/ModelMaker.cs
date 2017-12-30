using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using Zeltex.Voxels;
using Zeltex.Util;
using Zeltex.Generators;

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// An interface between the viewer and the voxel model.
    /// Primarily edits a Voxel Model.
    /// </summary>
    public class ModelMaker : MakerGui
    {
        #region Variables
        [Header("References")]
		[SerializeField] private VoxelViewer MyVoxelGui;
		//[SerializeField] private VoxelManager MyVoxelManager;
        //[Header("UI")]
       // public VoxelPolygonViewer MyPolyModelViewer;
        //private bool IsGridShown;
        //private float MyPaintSize = 0;
        //public VoxelMetaGenerator MyMetaGenerator;
        #endregion

        #region DataManager

        public new VoxelModel GetSelected()
        {
            return DataManager.Get().GetElement(DataFolderNames.VoxelModels, GetSelectedIndex()) as VoxelModel;
        }

        private void OnDestroy()
        {
            OnEnd();
        }

        /*public override void SaveAll()
        {
            // turn world into script
            //string Script = FileUtil.ConvertToSingle(MyVoxelGui.GetSpawn().GetComponent<World>().GetScript());
            //Debug.LogError(GetSelectedIndex() + ":Saving:\n" + Script);
            DataManager.Get().Set(
                DataFolderNames.PolyModels, 
                GetSelectedIndex(), 
                Script
               );
            base.SaveAll();
        }*/

        /// <summary>
        /// Use the folder name!
        /// </summary>
        protected override void SetFilePaths()
        {
            DataManagerFolder = DataFolderNames.PolyModels;// "PolyModels";
        }

        /// <summary>
        /// Add a new data!
        /// </summary>
        /*public void AddData(string MyName, string MyScript)
        {
            if (DataManager.Get())
            {
                DataManager.Get().Add(DataManagerFolder, MyName, MyScript);// DataFolders[0].MyData.Count;
            }
            MyIndexController.SetMaxSelected(GetSize());
        }*/

        #endregion

        #region Data

        /// <summary>
        /// Used for some methods
        /// </summary>
        /// <returns></returns>
        /*public static ModelMaker Get()
        {
            return GameObject.Find("GameManager").GetComponent<MapMaker>().MyModelMaker;
        }*/
        /// <summary>
        /// Returns a script using the name
        /// </summary>
        //public string Get(string MyName)
        //{
            /*for (int i = 0; i < GetSize(); i++)
            {
                if (ScriptUtil.RemoveWhiteSpace(MyNames[i]) == ScriptUtil.RemoveWhiteSpace(MyName))
                {
                    return GetData(i);
                }
            }*/
           // return "";
        //}
        /// <summary>
        /// Get the gui's world
        /// </summary>
        World GetWorld()
        {
            if (MyVoxelGui.GetSpawn())
            {
                return MyVoxelGui.GetSpawn().GetComponent<World>();
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Clears all the Voxels - sets them all to air
        /// </summary>
        public void ClearVoxels()
        {
            if (GetWorld() != null)
            {
                GetWorld().UpdateAll("Air");
                //GetWorld().UpdateBlockType(new Vector3(8, 0, 8), 1, 0);
            }
        }
        #endregion
        
        #region IndexController

        /// <summary>
        /// Load Gui with Input
        /// Only gets called if index is updated
        /// </summary>
        public override void OnUpdatedIndex(int NewIndex)
        {
            //Debug.Log(" Model Maker: Updating Index to [" + NewIndex + "]");
            base.OnUpdatedIndex(NewIndex);
            CancelInvokes();
            if (GetSelected() != null)
            {
                GetInput("NameInput").text = GetSelectedName();
                string MyScript = GetSelected().VoxelData;
                //Debug.LogError("Loading Model " + GetSelectedIndex() + ":" + MyScript);
                RoutineManager.Get().StartCoroutine(MyVoxelGui.RunScript(FileUtil.ConvertToList(MyScript)));
                //MyVoxelGui.GetSpawn().GetComponent<
            }
            else
            {
                GetInput("NameInput").text = "Null Voxel Model [" + GetSelectedIndex() + "]";
                RoutineManager.Get().StartCoroutine(MyVoxelGui.RunScript(new List<string>()));
            }
        }

        /// <summary>
        /// Used by VoxelViewer
        /// </summary>
        /*public void SetVoxelType(int VoxelType_)
        {
            GetDropdown("VoxelDropdown").value = VoxelType_;
            //MyPolyModelViewer.LoadVoxelMesh(MyVoxelManager.GetMeta(VoxelType_));
            VoxelPrimitives.Get().SetVoxelType(VoxelType_);
        }*/

		/// <summary>
		/// Add a new model to our list
		/// </summary>
		//protected override void AddData()
		//{
			//StartCoroutine(CreateNew(NewIndex, ""));
			//ataManager.Get().Add(DataManagerFolder, "Data " + GetSize(), PolyModelGenerator.Get().GetComponent<PolyModelGenerator>().GetSphere());   //GameObject.Find("Generators")
       // }

        private IEnumerator CreateNew(int NewIndex, string NewData)
        {
            // if meta.count == 0, create a meta for air!
            /*if (MyVoxelManager.MyMetas.Count == 0 && MyVoxelManager.MyModels.Count == 0)
            {
                Debug.Log("Database Empty. Generating Meta Data for Voxel Models.");
                // Add 2 blocks
                //yield return MyMetaGenerator.GenerateData(0.025f);
                //FillAllContainers();
            }*/
            if (GetSize() == 0)
            {
                Color32 MyColor = MyVoxelGui.GetComponent<RawImage>().color;
                MyVoxelGui.GetComponent<RawImage>().color =
                    new Color32(
                (byte)(MyColor.r),
                (byte)(MyColor.g),
                (byte)(MyColor.b),
                (byte)(255));
            }
            string NewName = "Model_" + UnityEngine.Random.Range(1, 10000);
            base.OnAdd(NewIndex);
            // Modify world here
            if (NewData != "")
            {
                GetWorld().RunScript(FileUtil.ConvertToList(NewData));
                //AddData(NewName, NewData);
            }
            else
            {
                // For blank data create a sphere!
                yield return GetWorld().SetWorldSizeRoutine(new Int3(1, 1, 1));
                //GetWorld().Clear();
                VoxelPrimitives.Get().MyWorld = GetWorld();
                VoxelPrimitives.Get().CreateSphere();
                NewData = (FileUtil.ConvertToSingle(GetWorld().GetScript()));
                //AddData(NewName, NewData);
            }
            yield break;
        }
		/// <summary>
		/// When an index is removed
		/// </summary>
		/// <param name="NewIndex"></param>
		protected override void RemovedData(int Index)
		{
            ClearVoxels();
			base.RemovedData(Index);
        }
        /// <summary>
        /// Called with ZelGuis OnBegin function and in LoadAll
        /// </summary>
        public override void OnBegin()
        {
            /*if (MyVoxelManager == null)
            {
                MyVoxelManager = VoxelManager.Get();
            }*/
            //MyPolyModelViewer.OnBegin();
            MyVoxelGui.OnBegin();
            base.OnBegin();
        }
        /// <summary>
        /// Called on ZelGuis OnEnd function
        /// </summary>
        public override void OnEnd()
        {
            if (GetSelectedIndex() >= 0 && GetSelectedIndex() < GetSize())
            {
                //Set(FileUtil.ConvertToSingle(GetWorld().GetScript()), GetSelectedIndex());
                /*DataManager.Get().Set(
                    DataFolderNames.PolyModels,
                    GetSelectedIndex(),
                    FileUtil.ConvertToSingle(MyVoxelGui.GetSpawn().GetComponent<World>().GetScript()));*/
            }
            base.OnEnd();
            MyVoxelGui.OnEnd();
            //MyPolyModelViewer.OnEnd();
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
        /// 
        /// </summary>
        public override void UseInput(Button MyButton)
        {
            base.UseInput(MyButton);
            if (MyButton.name == "ImportVoxButton")
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                //System.Diagnostics.Process.Start("explorer.exe", "/select," + open.FileName);
                World SpawnedWorld = MyVoxelGui.GetSpawn().GetComponent<World>();
                StartCoroutine(DataManager.Get().LoadVoxFile(SpawnedWorld));
#else
                Debug.LogError("Platform not supported.");
#endif
            }
            else if (MyButton.name == "ExportButton")
            {
                //FileUtil.Export(GetSelectedName(), FileExtension, GetSelected());
            }
        }

        /// <summary>
        /// called on mouse down
        /// </summary>
        public void Import()
        {
            //FileUtil.Import(name, "UploadModel", FileExtension);
        }
        private string UploadFileName = "";
        public void UploadModelFileName(string FileName)
        {
            UploadFileName = Path.GetFileNameWithoutExtension(FileName);
        }

        /// <summary>
        /// Called from javascript, uploading a model data
        /// </summary>
        public void UploadModel(string MyData)
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
                Debug.Log("Uploading new model:" + UploadFileName + "\n" + MyData.Length);
                //AddData(UploadFileName, MyData);
                UploadFileName = "";
            }
        }
        #endregion

        #region Generation

        /*public void Fill()
        {
            VoxelPrimitives.Get().Fill();
        }

        public void Sphere()
        {
            VoxelPrimitives.Get().CreateNoiseSphere();
        }
        public void Tree()
        {
            VoxelPrimitives.Get().CreateTree();
        }

        public void Cube()
        {
            //MyVoxelGui.GetSpawn().GetComponent<VoxelPrimitives>().CreateCube();
        }*/
        #endregion
        
        #region CopyPaste
        private List<string> MyCopyData = new List<string>();

        public void Copy()
        {
            MyCopyData = GetWorld().GetScript();
        }

        public void Paste()
        {
            Debug.Log("Pasting copied Voxels: " + MyCopyData.Count);
            if (MyCopyData.Count != 0)
            {
                GetWorld().RunScript(MyCopyData);
            }
        }
        #endregion

        #region MeshHandling

        public Mesh GetMesh(int SelectedIndex)
        {
            return GetMesh(SelectedIndex, new Vector3(1, 1, 1));
        }

       /* public string GetMeshData(int SelectedIndex)
        {
            if (SelectedIndex >= 0 && SelectedIndex < GetSize())
            {
                return GetData(SelectedIndex);
            }
            else
            {
                return "";
            }
        }*/

        /// <summary>
        /// Returns the mesh for a selected model
        /// </summary>
        public Mesh GetMesh(int SelectedIndex, Vector3 MyVoxelScale)
        {
            //GameObject MyWorld = GameObject.Find("World");
            /*string FilePath = GetFilePath()
                + "PolyModel_" + SelectedIndex + "." + FileExtension;
            Debug.Log("Loading " + FilePath);
            GameObject MyLoader = new GameObject();
            MyLoader.name = "MeshLoader [" + Time.time + "]";
            World MyLoaderWorld = MyLoader.AddComponent<World>();
            MyLoaderWorld.VoxelScale = MyVoxelScale;

            WorldUpdater MyUpdater = MyLoader.AddComponent<WorldUpdater>();

            MyLoaderWorld.IsCentreWorld = true;
            //WorldUpdater MyUpdater = MyWorld.GetComponent<WorldUpdater>();
            MyLoaderWorld.MyUpdater = MyUpdater;
            MyLoaderWorld.MyDataBase = MyVoxelManager;
            //MyLoaderWorld.MySaver = MyWorld.GetComponent<World>().MySaver;    // no saving
            MyLoaderWorld.MyMaterials = MyVoxelManager.MyMaterials;

            MeshFilter MyMeshFilter = MyLoader.AddComponent<MeshFilter>();
            MyLoader.AddComponent<MeshRenderer>().enabled = false;  // don't display
            MyMeshFilter.sharedMesh = new Mesh();

            if (File.Exists(FilePath))
            {
                Debug.Log("Loading From Script " + FilePath);
                string MyData = File.ReadAllText(FilePath);
                Debug.Log("Data - " + MyData);
                //MyVoxelGui.GetSpawnedObject().GetComponent<VoxelLoader>().Refresh();
                MyLoaderWorld.RunScript(FileUtil.ConvertToList(MyData));
            }
            Destroy(MyLoader, 1f);
            return MyMeshFilter.sharedMesh;*/
            return null;
        }
        /// <summary>
        /// Quickly used this to add polys to item meshes
        /// </summary>
        public void AddToMeshes()
        {
            //MyItemManager.MyMeshes.Add(MyVoxelGui.GetSpawn().GetComponent<MeshFilter>().mesh);
        }
        #endregion
    }
}





/*private void AddData()
{
    if (DataManager.Get())
    {
        DataManager.Get().Add(DataManagerFolder);// DataFolders[0].MyData.Count;
    }
}

private int GetNamesSize()
{
    return DataManager.Get().GetSize(DataManagerFolder);// DataFolders[0].MyData.Count;
}


/// <summary>
/// Returns the selected name
/// </summary>
public string GetName(int Index)
{
    return DataManager.Get().GetName(DataManagerFolder, Index);// DataFolders[0].MyData[MyIndexController.SelectedIndex];
}

/// <summary>
/// Set the name!
/// </summary>
public void SetName(int Index, string NewName)
{
    if (DataManager.Get() != null)
    {
        DataManager.Get().SetName(DataManagerFolder, Index, NewName);// DataFolders[0].MyData[MyIndexController.SelectedIndex];
    }
    else
    {
        if (Index >= 0 && Index < GetNamesSize())
        {
            MyNames[Index] = NewName;
        }
    }
}

/// <summary>
/// Gets data
/// Is used by skeleton generator too
/// </summary>
public string GetData(int Index)
{
    if (DataManager.Get() != null)
    {
        return DataManager.Get().GetData(DataManagerFolder, MyIndexController.SelectedIndex);
    }
    else
    {
        return "";
    }
}*/

/// <summary>
/// Returns the selected script
/// </summary>
/* public string GetSelected()
 {
     if (DataManager.Get() != null)
     {
         return DataManager.Get().GetData(DataManagerFolder, MyIndexController.SelectedIndex);// DataFolders[0].MyData[MyIndexController.SelectedIndex];
     }
     else
     {
         return "";
     }
 }*/
/*
/// <summary>
/// Loads all the data
/// </summary>
public override void LoadAll()
{

#if UNITY_WEBGL
      Application.ExternalCall("SyncFiles");
#endif
  base.LoadAll();
  Clear();
  List<string> MyFiles = FileUtil.GetFilesOfType(GetFilePath(), FileExtension);
  MyFiles = FileUtil.SortAlphabetically(MyFiles);
  //Debug.LogError("Loading " + MyFiles.Count + " Blocks from: " + GetFilePath());
  for (int i = 0; i < MyFiles.Count; i++)
  {
      // Add Name
      string MyName = Path.GetFileName(MyFiles[i]);
      if (MyName.Contains("." + FileExtension))
      {
          MyName = MyName.Substring(0, MyName.IndexOf(FileExtension) - 1);
      }
      AddName(MyName);    // name of the model
      // Add Script
      string MyScript = FileUtil.Load(MyFiles[i]);
      AddData(MyScript);
  }
  MyIndexController.SetMaxSelected(GetSize());
  if (GetSize() > 0)
  {
      Color32 MyColor = MyVoxelGui.GetComponent<RawImage>().color;
      //Debug.Log("Red: " + MyColor.r);
      MyVoxelGui.GetComponent<RawImage>().color = 
          new Color32(
      (byte) (MyColor.r),
      (byte) (MyColor.g),
      (byte) (MyColor.b),
      (byte) (255));
  }
  if (gameObject.activeInHierarchy)
  {
      MyIndexController.RemovedOldIndex();
      OnBegin();  // refresh if already opened
  }
  // Load All
}
/// <summary>
/// Saves all the quests
/// </summary>
public override void SaveAll()
{
  if (GetWorld())
  {
      SetSelected(FileUtil.ConvertToSingle(GetWorld().GetScript()));
  }
  string FolderPath = GetFilePath();
  Debug.Log("Inside ModelMaker: Saving [" + GetSize() + "] Models in Path [" + FolderPath + "]");
  for (int i = 0; i < GetSize(); i++)
  {
      string FileName = FolderPath + MyNames[i] + "." + FileExtension;
      FileUtil.Save(FileName, GetData(i));
  }
  //StartCoroutine(GetWorld().SaveRoutine(FilePath, this));
}*/
