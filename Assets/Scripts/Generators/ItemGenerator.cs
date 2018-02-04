using UnityEngine;
using System.Collections;
using Zeltex.Voxels;
using Zeltex.Combat;
using Zeltex.Items;
using System.Collections.Generic;

namespace Zeltex.Generators
{
    /// <summary>
    /// Generates items for the game
    /// </summary>
    public class ItemGenerator : MonoBehaviour
    {
        //private VoxelManager MyVoxelManager;
        [Header("References")]
        public TextureGenerator MyTextureGenerator;
        public int TextureResolution = 32;

        public static ItemGenerator Get()
        {
            return GameObject.Find("Generators").GetComponent<ItemGenerator>();
        }

        public IEnumerator GenerateData()
        {
            /*if (MyVoxelManager == null)
            {
                MyVoxelManager = VoxelManager.Get();
            }*/
            yield return GenerateItemsForVoxels();
            yield return GenerateItemSpells();
        }


        public IEnumerator GenerateItemSpells()
        {
            // Generate item textures
            //MyStatusText.text = "Generating ItemTextures";
            int SpellsSize = DataManager.Get().GetSizeElements("Spells");
            for (int i = 0; i < SpellsSize; i++)
            {
				Spell MySpell = DataManager.Get().GetElement("Spells", i) as Spell;
				// Create an item for the spell											 
				Item MyItem = new Item();// GenerateItem(i);
				MyItem.Name = MySpell.Name;
				//MyItem.SetCommands("/Spell " + MySpell.Name);
				// Generate an item texture
				//int TextureIndex = DataManager.Get().GetSize("ItemTextures");//MyTextureManager.ItemTextures.Count;
                Texture2D NewTexture = new Texture2D(TextureResolution, TextureResolution, TextureFormat.ARGB32, false);
                NewTexture.name = MySpell.Name + "_Texture";
                NewTexture.filterMode = FilterMode.Point;
				MyTextureGenerator.RandomColors();
				//MyTexGen.Noise(MyTextureManager.ItemTextures[TextureIndex]);
				MyTextureGenerator.Circle(NewTexture);
                MyItem.SetTexture(NewTexture);
				//DataManager.Get().AddTexture("ItemTextures", NewTexture);
				DataManager.Get().AddElement("ItemMeta", MyItem);
				// Set mesh to spell mesh -> used to 'hold a spell' - like a talisman
				//MyItem.MyMesh = MyVoxelMeta.GetSingleVoxelMesh(i);
				//MyItem.MyMaterial = MyWorld.MyDataBase.MyMaterial;
			}
            yield break;
        }

        public IEnumerator GenerateItemsForVoxels()
        {
            List<Element> MyMetas = DataManager.Get().GetElements(DataFolderNames.Voxels);
            Debug.Log("Generating " + MyMetas.Count + " Items!");
            for (int i = 1; i < MyMetas.Count; i++)    // skip air
            {
                Item MyItem = GenerateItem(i);
                MyItem.Name = DataManager.Get().GetName(DataFolderNames.Voxels, i);
                //MyItem.MyMesh = MyVoxelManager.GetSingleVoxelMesh(i);
                //MyItem.MyMaterial = MyVoxelManager.GetMaterial(0);

                //MyItemMaker.AddData(MyItem.Name, MyItem);
                DataManager.Get().AddElement("ItemMeta", MyItem);
                //yield return new WaitForSeconds(0.01f);
            }
            yield break;
        }

        /// <summary>
        /// Generate the meshes for voxels!
        /// </summary>
        public void GenerateMeshesForBlocks()
        {
            /*MyManager.MyMeshes.Clear();
            for (int i = 0; i < MyVoxelMeta.Data.Count; i++)
            {
                Mesh NewMesh = MyVoxelMeta.GetSingleVoxelMesh(i);
                NewMesh.name = MyVoxelMeta.Data[i].Name;
                MyManager.MyMeshes.Add(NewMesh);
            }*/
        }

        /// <summary>
        /// Generates an item from a voxel meta - should just do this once voxels have been updated - not every time!
        /// </summary>
        public Item GenerateItem(int VoxelIndex)
        {
            Item NewItem = new Item();
            List<Element> MyMetas = DataManager.Get().GetElements(DataFolderNames.Voxels);
            if (MyMetas.Count != 0 && VoxelIndex >= 0 && VoxelIndex < MyMetas.Count)
            {
                VoxelMeta MyMeta = MyMetas[VoxelIndex] as VoxelMeta;//.GetMeta(VoxelIndex);
                NewItem.Name = MyMeta.Name;
                NewItem.SetDescription(MyMeta.GetDescription());
                //NewItem.SetCommands("/Block " + MyMeta.Name);  // make command /Block[MetaIndex] work!
                //NewItem.MeshName = MyMeta.Name;
                //if (MyTextureManager)
                {
                    int VoxelMetaSize = DataManager.Get().GetSize(DataFolderNames.VoxelDiffuseTextures);
                    if (VoxelMetaSize > 0 && MyMeta.TextureMapID >= 0 && MyMeta.TextureMapID < VoxelMetaSize)
                    {
                        Zexel MyZexel = DataManager.Get().GetElement(DataFolderNames.VoxelDiffuseTextures, MyMeta.TextureMapID) as Zexel;
                        NewItem.SetTexture(MyZexel.GetTexture());
                    }
                }

                //NewItem.SetMesh(MyVoxelManager.GetSingleVoxelMesh(VoxelIndex)); 
            }
            return NewItem;
        }
    }
}