using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex
{
    /// <summary>
    /// The names of the data folders
    /// </summary>
	public static class DataFolderNames
    {
        //public static string VoxelMeta = "VoxelMeta";
		public static string PolyModels = "PolyModels";
        public static string VoxelModels = "VoxelModels";
        public static string Skeletons = "Skeletons";
        public static string Zanimations = "Zanimations";

        public static string Voxels = "VoxelMeta";
        public static string Items = "ItemMeta";
        public static string Recipes = "Recipes";
        public static string Inventorys = "Inventorys";

        public static string Stats = "Stats";
        public static string StatGroups = "StatGroups";
        public static string Spells = "Spells";

        public static string Quests = "Quests";
        public static string QuestLogs = "QuestLogs";
        public static string Dialogues = "Dialogues";
        public static string DialogueTrees = "DialogueTrees";

        public static string Saves = "Saves";
        public static string Levels = "Levels";
        public static string Characters = "Characters";

        public static string Sounds = "Sounds";
        public static string Musics = "Musics";

        public static string VoxelDiffuseTextures = "VoxelDiffuseTextures";
        public static string VoxelNormalTextures = "VoxelNormalTextures";
        public static string StatTextures = "StatTextures";
        public static string ItemTextures = "ItemTextures";

        public static string FolderToGuiName(string FolderName)
        {
            if (FolderName == Spells)
            {
                return "SpellMaker";
            }
            else if (FolderName == Stats)
            {
                return "StatsMaker";
            }
            else if (FolderName == Quests)
            {
                return "QuestMaker";
            }
            else if (FolderName == Voxels)
            {
                return "VoxelMaker";
            }
            else if (FolderName == Items)
            {
                return "ItemMaker";
            }
            else if (FolderName == Recipes)
            {
                return "RecipeMaker";
            }
            else if (FolderName == Dialogues)
            {
                return "DialogueMaker";
            }
            /*else if (FolderName == Classes)
            {
                return "ClassMaker";
            }*/

            else if (FolderName == VoxelDiffuseTextures)
            {
                return "TextureMaker";
            }
            else if (FolderName == VoxelNormalTextures)
            {
                return "TextureMaker";
            }
            else if (FolderName == ItemTextures)
            {
                return "TextureMaker";
            }
            else if (FolderName == StatTextures)
            {
                return "TextureMaker";
            }

            else if (FolderName == Skeletons)
            {
                return "SkeletonMaker";
            }
            else if (FolderName == Zanimations)
            {
                return "";
            }
			else if (FolderName == PolyModels)
            {
                return "PolygonMaker";
            }
            else if (FolderName == VoxelModels)
            {
                return "ModelMaker";
            }
            else if (FolderName == Sounds)
            {
                return "SoundMaker";
            }
            return "";
        }
        
        public static System.Type GetDataType(string FolderName)
        {
            System.Type DataType;
            if (FolderName == DataFolderNames.Recipes)
            {
                DataType = typeof(Items.Recipe);
            }
            else if (FolderName == DataFolderNames.Items)
            {
                DataType = typeof(Items.Item);
            }

            else if (FolderName == DataFolderNames.Stats)
            {
                DataType = typeof(Combat.Stat);
            }
            else if (FolderName == DataFolderNames.Spells)
            {
                DataType = typeof(Combat.Spell);
            }

            else if (FolderName == DataFolderNames.PolyModels)
            {
                DataType = typeof(Voxels.PolyModel);
            }
            else if (FolderName == DataFolderNames.Voxels)
            {
                DataType = typeof(Voxels.VoxelMeta);
            }
            else if (FolderName == DataFolderNames.VoxelModels)
            {
                DataType = typeof(Voxels.VoxelModel);
            }
            else if (FolderName == DataFolderNames.Skeletons)
            {
                DataType = typeof(Skeletons.Skeleton);
            }
            else if (FolderName == DataFolderNames.Zanimations)
            {
                DataType = typeof(Skeletons.Zanimation);
            }

            else if (FolderName == DataFolderNames.Characters)
            {
                DataType = typeof(CharacterData);
            }
            else if (FolderName == DataFolderNames.Levels)
            {
                DataType = typeof(Level);
            }
            else if (FolderName == DataFolderNames.Saves)
            {
                DataType = typeof(SaveGame);
            }

            else if (FolderName == DataFolderNames.StatGroups)
            {
                DataType = typeof(Combat.Stats);
            }
            else if (FolderName == DataFolderNames.Inventorys)
            {
                DataType = typeof(Items.Inventory);
            }
            else if(FolderName == DataFolderNames.Quests)
            {
                DataType = typeof(Quests.Quest);
            }
            else if (FolderName == DataFolderNames.QuestLogs)
            {
                DataType = typeof(Quests.QuestLog);
            }

            else if (FolderName == DataFolderNames.DialogueTrees)
            {
                DataType = typeof(Dialogue.DialogueTree);
            }

            else if (FolderName == DataFolderNames.VoxelDiffuseTextures)
            {
                DataType = typeof(Zexel);
            }
            else if (FolderName == DataFolderNames.VoxelNormalTextures)
            {
                DataType = typeof(Zexel);
            }
            else if (FolderName == DataFolderNames.ItemTextures)
            {
                DataType = typeof(Zexel);
            }
            else if (FolderName == DataFolderNames.StatTextures)
            {
                DataType = typeof(Zexel);
            }
            else if (FolderName == DataFolderNames.Sounds)
            {
                DataType = typeof(Sound.Zound);
            }
            else if (FolderName == DataFolderNames.Musics)
            {
                DataType = typeof(Sound.Zound);
            }

            else if (FolderName == DataFolderNames.Dialogues)
            {
                DataType = typeof(Dialogue.DialogueData);
            }
            else if (FolderName == DataFolderNames.DialogueTrees)
            {
                DataType = typeof(Dialogue.DialogueTree);
            }
            
            else
            {
                DataType = typeof(Element);
            }
            return DataType;
        }

        public static string DataTypeToFolderName(System.Type DataType)
        {
            if (DataType == typeof(Items.Recipe))
            {
                return DataFolderNames.Recipes;
            }
            else if (DataType == typeof(Items.Item))
            {
                return DataFolderNames.Items;
            }

            else if (DataType == typeof(Combat.Stat))
            {
                return DataFolderNames.Stats;
            }
            else if (DataType == typeof(Combat.Spell))
            {
                return DataFolderNames.Spells;
            }

            else if (DataType == typeof(Voxels.PolyModel))
            {
                return DataFolderNames.PolyModels;
            }
            else if (DataType == typeof(Voxels.VoxelMeta))
            {
                return DataFolderNames.Voxels;
            }
            else if (DataType == typeof(Voxels.VoxelModel))
            {
                return DataFolderNames.VoxelModels;
            }
            else if (DataType == typeof(Skeletons.Skeleton))
            {
                return DataFolderNames.Skeletons;
            }
            else if (DataType == typeof(Skeletons.Zanimation))
            {
                return DataFolderNames.Zanimations;
            }

            else if (DataType == typeof(CharacterData))
            {
                return DataFolderNames.Characters;
            }
            else if (DataType == typeof(Level))
            {
                return DataFolderNames.Levels;
            }

            else if (DataType == typeof(Combat.Stats))
            {
                return DataFolderNames.StatGroups;
            }
            else if (DataType == typeof(Items.Inventory))
            {
                return DataFolderNames.Inventorys;
            }
            else if (DataType == typeof(Quests.Quest))
            {
                return DataFolderNames.Quests;
            }
            else if (DataType == typeof(Quests.QuestLog))
            {
                DataType = typeof(Quests.QuestLog);
                return DataFolderNames.QuestLogs;
            }

            else if (DataType == typeof(Dialogue.DialogueData))
            {
                return DataFolderNames.Dialogues;
            }
            else if (DataType == typeof(Dialogue.DialogueTree))
            {
                return DataFolderNames.DialogueTrees;
            }

            else if (DataType == typeof(Zexel))
            {
                return DataFolderNames.VoxelDiffuseTextures;
            }
            else if (DataType == typeof(Zexel))
            {
                return DataFolderNames.VoxelNormalTextures;
            }
            else if (DataType == typeof(Zexel))
            {
                return DataFolderNames.ItemTextures;
            }
            else if (DataType == typeof(Zexel))
            {
                return DataFolderNames.StatTextures;
            }
            return "";
        }
    }
}
