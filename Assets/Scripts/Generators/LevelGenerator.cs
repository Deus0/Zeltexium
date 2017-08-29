using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Util;

namespace Zeltex
{
    /// <summary>
    /// Generates a series of level generation scripts
    /// </summary>
    public class LevelGenerator : GeneratorBase
    {
        /// <summary>
        /// Generate levels - 10 levels - increasing difficulty
        /// </summary>
        public override IEnumerator Generate()
        {
            // bam!
            DataManager.Get().AddElement(DataFolderNames.Levels, GenerateLevel());
            yield break;
        }

        private Level GenerateLevel()
        {
            Level NewLevel = new Level();
            NewLevel.SetName(Zeltex.NameGenerator.GenerateVoxelName());
            NewLevel.SetWorldSize(new Vector3(7,3,7));
            NewLevel.SetWorldAsInfinite(true);
            NewLevel.SetIsGenerateTerrain(true);
            /*List<string> Data = new List<string>();
            Data.Add("7"); Data.Add("3"); Data.Add("7");    // default map size - this should check - with a /mapsize tags
            Data.Add(Zeltex.Voxels.VoxelTerrain.BeginTag);
            Data.Add(Zeltex.Voxels.VoxelTerrain.EndTag);
            return Data;*/
            return NewLevel;
        }
    }

}