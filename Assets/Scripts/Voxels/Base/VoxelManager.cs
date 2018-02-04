using UnityEngine;
using System.Collections.Generic;
using Zeltex.Items;
using MakerGuiSystem;
using Zeltex.Util;
using Zeltex.Generators;

namespace Zeltex.Voxels 
{
    
    /// <summary>
    /// this is now - Voxel Meta Generator - will rename later
    /// </summary>
    [ExecuteInEditMode]
    public class VoxelManager : ManagerBase<VoxelManager>
    {
        #region Variables
        [Header("Debug")]
        public bool IsDebugGui;
        public EditorAction ActionGenerateTilemap = new EditorAction();

		[Header("MyMetas")]
        public Dictionary<string, PolyModel> MyModels = new Dictionary<string, PolyModel>();
        public Dictionary<string, VoxelMeta> MyMetas = new Dictionary<string, VoxelMeta>();    // this is much faster for getting MyMetas using a string!
        public Texture2D MyTileMap;
        public Texture2D NormalMap;
        public Texture2D IndexMap;
        public List<Texture2D> DiffuseTextures;
        public List<Texture2D> NormalTextures;
        [Header("Materials")]
        public List<Material> MyMaterials = new List<Material>();
        public string MainTextureName = "_MainTex";
        public string NormalTextureName = "NormalTex";
        private Vector2 TextureSize = new Vector2(16, 16);

        public new static VoxelManager Get()
        {
            if (MyManager == null)
            {
                GameObject LayerManagerObject = GameObject.Find("VoxelManager");
                if (LayerManagerObject)
                {
                    MyManager = LayerManagerObject.GetComponent<VoxelManager>();
                }
                else
                {
                    Debug.LogError("Could not find [VoxelManager].");
                }
            }
            return MyManager;
        }

        public void Update()
        {
            if (ActionGenerateTilemap.IsTriggered())
            {
                GenerateTileMap();
            }
        }
        #endregion

        public Vector2 GetTextureSize()
        {
            return TextureSize;
        }

        public void SetTextureSize(Vector2 NewTextureSize)
        {
            TextureSize = NewTextureSize;
        }

        #region Mono

        /*void Update()
        {
            if (IsCollectDebugMyMetas)
            {
                IsCollectDebugMyMetas = false;
                DebugMyMetas.Clear();
                foreach (string MyMetaName in MyMetas.Keys)
                {
                    DebugMyMetas.Add(MyMetas[MyMetaName]);
                }
				DebugModelMyMetas.Clear();
				foreach (string ModelName in MyModels.Keys)
				{
					DebugModelMyMetas.Add(MyModels[ModelName]);
				}
			}
        }*/

        private void OnGUI()
        {
            if (IsDebugGui)
            {
                GUILayout.Label("Meta " + MyMetas.Count);
                foreach (string MyMetaName in MyMetas.Keys)
                {
                    GUILayout.Label(MyMetaName);
                }
                GUILayout.Label("Models " + MyModels.Count);
                foreach (string ModelName in MyModels.Keys)
                {
                    GUILayout.Label(ModelName + ":" + MyModels[ModelName].Name);
					PolyModel MyModel = MyModels[ModelName];
					for (int i = 0; i < MyModel.TextureMaps.Count; i++)
					{
						GUILayout.Label("	TextureMap [" + i + "] :" + MyModel.TextureMaps[i].Coordinates.Count);
					}
                }
            }
        }
        #endregion

        #region MyMetas

        public void ClearMetas()
        {
            MyMetas.Clear();
        }

        public void ClearModels()
        {
            MyModels.Clear();
        }

        /// <summary>
        /// Clear all the voxel MyMetas
        /// </summary>
       /* public void Clear()
        {
            DataManager.Get().Clear("VoxelMeta");
            DataManager.Get().Clear("VoxelTexturesDiffuse");
            DataManager.Get().Clear("PolyModels");
        }*/

        /// <summary>
        /// Add a Voxel Texture to the game
        /// </summary>
        //public void AddTexture(Texture2D NewTexture)
        //{
            // add to MyMetasbase
            //DiffuseTextures.Add(NewTexture);
            // alsoo add to MyMetasmanager
            //DataManager.Get().AddEmptyString("VoxelTexturesDiffuse", NewTexture.name);
        //}

        public Texture2D GetTextureDiffuse(int FileIndex)
        {
            if (FileIndex >= 0 && FileIndex < DiffuseTextures.Count)
            {
                return DiffuseTextures[FileIndex];
            }
            else
            {
                return null;
            }
        }

        public Texture2D GetTextureDiffuse(string TextureName)
        {
            for (int i = 0; i < DiffuseTextures.Count; i++)
            {
                if (DiffuseTextures[i].name == TextureName)
                {
                    return DiffuseTextures[i];
                }
            }
            return null;
        }
        
        public Texture2D GetTextureNormal(string TextureName)
        {
            for (int i = 0; i < NormalTextures.Count; i++)
            {
                if (NormalTextures[i].name == TextureName)
                {
                    return NormalTextures[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Add a Voxel Model to the game
        /// </summary>
        public void AddMetaRaw(VoxelMeta NewMeta)
        {
            if (NewMeta != null && MyMetas.ContainsKey(NewMeta.Name) == false)
            {
                MyMetas.Add(NewMeta.Name, NewMeta);
            }
        }

        /// <summary>
        /// Add a Voxel Model to the game
        /// </summary>
        public void AddModelRaw(PolyModel NewModel)
        {
            if (NewModel != null && MyModels.ContainsKey(NewModel.Name) == false)
            {
                MyModels.Add(NewModel.Name, NewModel);
            }
        }

        /// <summary>
        /// Inefficient, outdated. Try not to use this.
        /// </summary>
        public PolyModel GetModel(int Index)
        {
            if (Index >= 0 && Index < MyModels.Values.Count)
            {
                int i = 0;
                foreach (string MyModelName in MyModels.Keys)
                {
                    if (i == Index)
                    {
                        return MyModels[MyModelName];
                    }
                    i++;
                }
            }
            return null;
        }

        public void RemoveModel(int Index)
        {
            int i = 0;
            foreach (string MyModelName in MyModels.Keys)
            {
                if (i == Index)
                {
                    MyModels.Remove(MyModelName);
                    break;
                }
                i++;
            }
        }
        #endregion

        #region MetaMyMetas

		/// <summary>
		/// This gets called by Chunk On Meshing - thousands of times an update
		/// </summary>
        public VoxelMeta GetMeta(string MyName)
        {
            if (MyMetas.ContainsKey(MyName))
            {
                return MyMetas[MyName];
            }
            else
            {
				//Debug.LogError("Voxel Manager does not contain " + MyName);
                return null;
            }
        }
        /// <summary>
        /// Remove a meta by index - Depreciated!
        /// </summary>
        public void RemoveMeta(int Index)
        {
            int i = 0;
            foreach (var MyMeta in MyMetas.Values)
            {
                if (i == Index)
                {
                    MyMetas.Remove(MyMeta.Name);
                    break;
                }
                i++;
            }
        }

        /// <summary>
        /// Get a meta by index - Depreciated!
        /// TODO: This!
        /// </summary>
        public VoxelMeta GetMeta(int Index)
        {
            int i = 0;
            foreach (var MyMeta in MyMetas.Values)
            {
                if (i == Index)
                {
                    return MyMeta;
                }
                i++;
            }
            if (MyMetas.ContainsKey("Air"))
            {
                return MyMetas["Air"];
            }
            else
            {
                return new VoxelMeta();
            }
        }

        /// <summary>
        /// Get a meta.name by index - Depreciated!
        /// </summary>
        public string GetMetaName(int Index)
        {
            int i = 0;
            foreach (var MyMeta in MyMetas.Values)
            {
                if (i == Index)
                {
                    return MyMeta.Name;
                }
                i++;
            }
            return "Air";
        }
        /*for ()
        if (Index >= 0 && Index < MyMetas.Count)
        {
            return MyMetas[Index].Name;
        }
        else
        {
            return "Air";
        }*/
        #endregion

        #region SingleVoxelMesh

        /// <summary>
        /// Returns the mesh using a model, or returns null if no model
        /// </summary>
        public PolyModel GetModel(string ModelName)
        {
            if (ModelName != null && MyModels.Count > 0 && MyModels.ContainsKey(ModelName))
            {
                return MyModels[ModelName];
            }
            else
            {
                //Debug.LogError("Failure to retrieve model: " + MyModels.Count + ":" + MyModels[0].Name);
				return null;
            }
        }

        /// <summary>
        /// returns the model index!
        /// </summary>s
        public int GetModelIndex(string ModelName)
        {
            //Debug.Log("Searching for " + ModelName);
            ModelName = ScriptUtil.RemoveWhiteSpace(ModelName);
            if (MyModels.Count > 0)
            {
                int i = 0;
                foreach (string MyModelName in MyModels.Keys)
                {
                    if (ModelName == MyModelName)
                    {
                        return i;
                    }
                    i++;
                }
                //Debug.Log("Could not find " + ModelName + ":" + ModelName.Length);
                return 0;
            }
            else
            {
                return -1;
            }
        }


        #endregion

        #region Mesh

        /// <summary>
        /// Gets a PolyModel with a name
        /// </summary>
        public PolyModel GetPolyModel(string ModelName)
        {
            if (MyModels.ContainsKey(ModelName))
            {
                return MyModels[ModelName];
            }
            return null;
        }

        PolyModel MyModel;
        /// <summary>
        /// used for chunk models
        /// </summary>
        public MeshData GetMeshData(string ModelIndex, int SideIndex)
        {
            MyModel = GetPolyModel(ModelIndex);
            if (MyModel != null)
            {
                return MyModel.GetModel(SideIndex);
            }
            else
            {
                //Debug.LogError(ModelIndex + " is null.");
                return new MeshData();
            }
        }
        // maybe take in light sources too
        // lights will be 3x3 in size
        #endregion

        #region Material

        /// <summary>
        /// Generate a tile map for our diffuse textures
        /// </summary>
        public void GenerateTileMap()
        {
            TileMap NewMap = new TileMap();
            DiffuseTextures.Clear();
            for (int i = 0; i < DataManager.Get().GetSize(DataFolderNames.VoxelDiffuseTextures); i++)
            {
                DiffuseTextures.Add((DataManager.Get().GetElement(DataFolderNames.VoxelDiffuseTextures, i) as Zexel).GetTexture());
            }
            Debug.Log("Generating tilemap using " + DiffuseTextures.Count + " textures.");
            Texture2D TileMap = NewMap.CreateTileMap(DiffuseTextures, 8);
            TileMap.name = DataManager.Get().MapName + "_TileMap";
#if UNITY_EDITOR
            if (Application.isEditor && Application.isPlaying == false)
            {
                string FilePath = Application.dataPath + "/" + TileMap.name + ".png";
                FileUtil.SaveBytes(FilePath, TileMap.EncodeToPNG());
                //TileMap.LoadImage(FileUtil.LoadBytes(FilePath), false);
                //UnityEditor.AssetImporter MyImporter = UnityEditor.TextureImporter.GetAtPath(FilePath);
                TileMap = (Texture2D)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/" + TileMap.name + ".png", typeof(Texture2D));
            }
#endif
            UpdateTileMap(TileMap);
        }

        public Material GetMaterial(int Index)
        {
            if (Index >= 0 && Index < MyMaterials.Count)
            {
                return MyMaterials[Index];
            }
            else
            {
                return null;
            }
        }

        public void UpdateTileMap(Texture2D MyTileMap_)
        {
            if (MyTileMap_ != null && MyMaterials.Count > 0)
            {
                MyTileMap = MyTileMap_;
                //MyMaterial.mainTexture = MyTileMap;
                if (MyMaterials[0] != null)
                {
                    MyMaterials[0].SetTexture(MainTextureName, MyTileMap);
                }
                else
                {
                    Debug.LogError("Material null in voxel manager.");
                }
            }
            else
            {
                Debug.LogError("No materials in voxel manager [" + name + "]");
            }
        }
        public void UpdateTileMapMeta(Texture2D MyTileMap_)
        {
            if (MyTileMap_ != null)
            {
                IndexMap = MyTileMap_;
                //MyMaterial.SetTexture("_TileMap", MyTileMap_);
            }
        }
        public void UpdateNormalMap(Texture2D NormalMap_)
        {
            if (NormalMap_ != null && MyMaterials.Count > 0)
            {
                //Debug.LogError("Setting Normal Map!");
                NormalMap = NormalMap_;
                MyMaterials[0].SetTexture(NormalTextureName, NormalMap);
                //MyMaterial.SetTexture("_SpecGlossMap", NormalMap);   //_BumpMap    Shader.PropertyToID("Specular")
            }
        }
        #endregion
    }
}