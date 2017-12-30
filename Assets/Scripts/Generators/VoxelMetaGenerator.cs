using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Util;
using Zeltex.Voxels;
using Zeltex;

namespace Zeltex.Generators
{
    /// <summary>
    /// Generate some voxels for the world
    /// Includes their meta, their texture, and their models
    /// </summary>
    public class VoxelMetaGenerator : MonoBehaviour
    {
        private VoxelManager MyVoxelManager;
		[Header("References")]
		public TextureGenerator MyTexGen;
        [Header("Options")]
        public float TextureNoiseAmplitude = 0.2f;
        public float TextureNoiseFrequency = 0.05f;
        public int TextureResolution = 64;
        [Header("Data")]
        public List<Color> MyPrimaryColours = new List<Color>();
        public List<Color> MySecondaryColours = new List<Color>();
        [Header("Colors")]
        public Color DirtColorMin;
        public Color DirtColorMax;
        public Color BricksColorMin;
        public Color BricksColorMax;
        public Color WoodColorMin;
        public Color WoodColorMax;
        public Color LeavesColorMin;
        public Color LeavesColorMax;
        public Color SandColorMin;
        public Color SandColorMax;

        // Generates Blocks
        public IEnumerator GenerateData(float LoadingDelay)
        {
            if (MyVoxelManager == null)
            {
                MyVoxelManager = VoxelManager.Get();
            }
            //Debug.Log("Generating: " + VoxelsToGenerate + " Voxels.");
            //int OldResolution = Zeltex.Voxels.World.TextureResolution;
            World.TextureResolution = TextureResolution;
            //TextureGenerator MyTexGen = MyTextureManager.MyTextureEditor.GetComponent<TextureGenerator>();
            MyTexGen.NoiseAmplitude = TextureNoiseAmplitude;
            MyTexGen.NoiseFrequency = TextureNoiseFrequency;
            /*if (MyTextureManager == null || MyVoxelManager == null)
            {
                Debug.LogError("Texture Manager is null in Voxel Generator.");
                yield break;
            }*/
            DataManager.Get().Clear("VoxelMeta");
            DataManager.Get().Clear("VoxelTexturesDiffuse");
            DataManager.Get().Clear("PolyModels");
            GeneratePolyModels();
            //MyTexGen.RandomColors();    // all the same colours! or slightly altered
            //int MyColourIndex = Random.Range(0, MyPrimaryColours.Count - 1);
            //MyTexGen.SetColors(MyPrimaryColours[MyColourIndex], MySecondaryColours[MyColourIndex]);
            for (int i = 0; i <= 5; i++)
            {
                VoxelMeta MyMeta = new VoxelMeta();
				// first  create texture for the meta
                Texture2D NewTexture = new Texture2D(TextureResolution, TextureResolution, TextureFormat.ARGB32, false);
                NewTexture.filterMode = FilterMode.Point;
                NewTexture.name = Zeltex.NameGenerator.GenerateVoxelName();
                if (i == 0)
                {
                    NewTexture.name = "Air";
                    MyTexGen.Fill(NewTexture, Color.white);
                }
                if (i == 1)
                {
                    NewTexture.name = "Color";
                    MyTexGen.Fill(NewTexture, Color.white);
                }
                else if (i == 2)
                {
                    MyTexGen.SetColors(DirtColorMin, DirtColorMax);
                    MyTexGen.NoiseMap(NewTexture);
                }
                else if (i == 3)
                {
                    MyTexGen.SetColors(BricksColorMin, BricksColorMax);
                    MyTexGen.Bricks(NewTexture);
                }
                else if (i == 4)
                {
                    MyTexGen.SetColors(WoodColorMin, WoodColorMax);
                    MyTexGen.Voroni(NewTexture, true, 6, 12);
                    //MyTexGen.AddNoise(MyTextureManager.VoxelDiffuseTextures[i]);
                }
                else if (i == 5)
                {
                    MyTexGen.SetColors(LeavesColorMin, LeavesColorMax);
                    MyTexGen.Voroni(NewTexture, false, 6, 5);
                }
                string NewVoxelName = Zeltex.NameGenerator.GenerateVoxelName();
                MyMeta.Name = NewVoxelName;
                MyMeta.ModelID = "Block";
                MyMeta.TextureMapID = i;
                //Debug.LogError("Creating new Voxel: " + NewVoxelName);
                if (i == 0)
                {
                    MyMeta.Name = "Air";
                    MyMeta.ModelID = "";//-1 for air
                }
                else if (i == 1)
                {
                    MyMeta.Name = "Color";
                }
                else if (i == 2)
                {
                    MyMeta.Name += " Dirt";
                }
                else if (i == 3)
                {
                    MyMeta.Name += " Bricks";
                }
                else if (i == 4)
                {
                    MyMeta.Name += " Wood";
                }
                else if (i == 5)
                {
                    MyMeta.Name += " Leaves";
                }
                MyMeta.OnModified();
                //DataManager.Get().AddTexture(DataFolderNames.VoxelDiffuseTextures, NewTexture);
                DataManager.Get().AddElement(DataFolderNames.Voxels, MyMeta);
				yield return new WaitForSeconds(LoadingDelay);
            }
            GeneratePolyModelTextureMap();
            // Generate TileMap
            //MyTextureManager.GenerateTileMap();
            yield return new WaitForSeconds(LoadingDelay);
            Debug.Log("Generated: " + MyVoxelManager.MyMetas.Count + " Voxels.");
        }

        /// <summary>
        /// Generate various models for the voxels
        /// </summary>
        public void GeneratePolyModels()
        {
            Debug.Log("Generating Default Voxel Models at [" + Time.realtimeSinceStartup + "]");
            //Data.Clear ();
            //MyPolygonMaker.Clear();
            PolyModel NewModel = new PolyModel();
            NewModel.Name = "Block";
            NewModel.GenerateCubeMesh();
            DataManager.Get().AddElement(DataFolderNames.PolyModels, NewModel);

			// Second is a squashed model
			/*PolyModel NewModel2 = new PolyModel();
            NewModel2.Clear();
            NewModel2.GenerateSquashedCubeMesh(new Vector3(0.5f, 1f, 0.5f));
            MyModels.Add(NewModel2);

            // for door
            PolyModel NewModel3 = new PolyModel();
            NewModel3.Clear();
            NewModel3.GenerateSquashedCubeMesh(new Vector3(1f, 1f, 0.1f));
            NewModel3.MovePosition(new Vector3(0, 0, 0.2f));   // starts at z = 0.5f, need it to go to 0.9f, so +.4f
            MyModels.Add(NewModel3);

            // platform thingy
            PolyModel NewModel4 = new PolyModel();
            NewModel4.Clear();
            NewModel4.GenerateSquashedCubeMesh(new Vector3(1f, 0.25f, 1f));
            NewModel3.MovePosition(new Vector3(0, 0, 0.25f));   // starts at z = 0.5f, need it to go to 0.9f, so +.4f
            MyModels.Add(NewModel4);*/
		}

        /// <summary>
        /// generate texture maps
        /// For each texture, map them to the models - automatically
        /// This will be one texture to one model, or a blend between the two
        /// </summary>
        private void GeneratePolyModelTextureMap()
        {
			// Create a texture map for each texture
			int DiffuseTexturesCount = MyVoxelManager.DiffuseTextures.Count;
			Debug.LogError("Generating texture map for " + DiffuseTexturesCount + " Textures.");
			for (int i = 0; i < MyVoxelManager.MyModels.Count; i++)
            {
				PolyModel MyModel = MyVoxelManager.GetModel(i);
				for (int j = 0; j < DiffuseTexturesCount; j++)
                {
					string TextureName = MyVoxelManager.DiffuseTextures[j].name;
					MyModel.NewTextureMap();
					MyModel.GenerateTextureMap(TextureName, j);
                }
            }
            MyVoxelManager.GenerateTileMap();
        }

        //  uses textures to create blocks - isnt used anymore
        /*public void GenerateVoxelMetaBasic()
        {
            // Create block meta data
            MyWorld.Data.Clear();
            for (int i = 0; i <= MyTextureManager.VoxelDiffuseTextures.Count; i++)
            {
                VoxelMeta MyMeta = new VoxelMeta();
                if (i == 0)
                {
                    MyMeta.ModelID = "";
                    MyMeta.TextureMapID = 0;
                    MyMeta.Name = "Air";
                }
                else
                {
                    MyMeta.Name = MyTextureManager.VoxelDiffuseTextures[i - 1].name;
                    MyMeta.ModelID = "Block";
                    MyMeta.TextureMapID = i - 1;
                }
                MyWorld.Data.Add(MyMeta);
            }
        }*/
    }
}