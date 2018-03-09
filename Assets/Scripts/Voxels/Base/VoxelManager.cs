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

		[Header("MyMetas")]
        //public Dictionary<string, PolyModel> MyModels = new Dictionary<string, PolyModel>();
        //public Dictionary<string, VoxelMeta> MyMetas = new Dictionary<string, VoxelMeta>();    // this is much faster for getting MyMetas using a string!
        public Texture2D MyTileMap;
        public Texture2D NormalMap;
        //public Texture2D IndexMap;
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
        #endregion

        public Vector2 GetTextureSize()
        {
            return TextureSize;
        }

        public void SetTextureSize(Vector2 NewTextureSize)
        {
            TextureSize = NewTextureSize;
        }

        #region Material

        /// <summary>
        /// Generate a tile map for our diffuse textures
        /// </summary>
        public void GenerateTileMap()
        {
            TileMap NewMap = new TileMap();
            List<Texture2D> DiffuseTextures = new List<Texture2D>();
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
        //public void UpdateTileMapMeta(Texture2D MyTileMap_)
        //{
        //   if (MyTileMap_ != null)
        //    {
        //IndexMap = MyTileMap_;
        //MyMaterial.SetTexture("_TileMap", MyTileMap_);
        //    }
        //}
        #endregion
    }
}