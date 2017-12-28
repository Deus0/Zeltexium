using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MakerGuiSystem;
using Zeltex;

namespace Zeltex.Voxels
{

    // Each texture Map has multiple coordinates per side
    // Each coordinates groups the coordinates further, depending on TileNames
    // When getting the UVs, it must have the TileMapInfo stored
    // Each TileMapInfo has TileNames, linked to TilePositions

    /// <summary>
    /// Used for storing uvs
    /// The uvs are grouped by tile and side index
    /// </summary>
    [System.Serializable]
    public class VoxelCoordinate
    {
        //public List<RawVoxelCoordinate> MyTextureCoordinates = new List<RawVoxelCoordinate>();
        //public int VertexIndex;   // this is just done by index of the list
        public Vector2 MyCoordinate = new Vector2();
        public string TileName = "";

        public VoxelCoordinate()
        {

        }
        public VoxelCoordinate(Vector2 MyCoordinate_, string TileName_)
        {
            MyCoordinate = MyCoordinate_;
            TileName = TileName_;
        }
    }
    /// <summary>
    /// Each texture map links up to textures and uvs
    /// </summary>
    [System.Serializable]
    public class PolyTextureMap
    {
        public List<VoxelCoordinate> Coordinates = new List<VoxelCoordinate>();
        // functions
        private List<string> TilemapNames = new List<string>();
        private List<Vector2> MyCoordinates = new List<Vector2>();
        private List<Vector2> MyTextureCoords = new List<Vector2>();
        private string TileMapName = "";
        private int TileIndex = 0;
        private Vector2 MyUV;
        private Vector2 TilePosition;

        /// <summary>
        /// Initiate the voxel texturemap, giving it some coordinates
        /// </summary>
        public PolyTextureMap()
        {
        }

        public List<string> GetTilemapNames(List<int> MyIndexes)
        {
            TilemapNames.Clear();
            for (int i = 0; i < MyIndexes.Count; i++)
            {
                TilemapNames.Add(Coordinates[MyIndexes[i]].TileName);
            }
            return TilemapNames;
        }
        public void GetTileMapInfo(out List<string> TilemapNames, out List<int> TileMapCounts)
        {
            TilemapNames = new List<string>();
            TileMapCounts = new List<int>();
            for (int i = 0; i < Coordinates.Count; i++)
            {
                if (TilemapNames.Contains(Coordinates[i].TileName) == false)
                {
                    TilemapNames.Add(Coordinates[i].TileName);
                    TileMapCounts.Add(1);
                }
                else
                {
                    TileMapCounts[TilemapNames.IndexOf(Coordinates[i].TileName)]++;
                }
            }
        }
        public void Add(Vector2 NewCoordinate, string TileName)
        {
            Coordinates.Add(new VoxelCoordinate(NewCoordinate, TileName));
        }
        public void Set(int Index, Vector2 NewCoordinate)
        {
            if (Index >= 0 && Index < Coordinates.Count)
            {
                Coordinates[Index].MyCoordinate = NewCoordinate;
            }
        }

        public void SetName(string NewName)
        {
            for (int i = 0; i < Coordinates.Count; i++)
            {
                Coordinates[i].TileName = NewName;
            }
        }
        public void SetName(string NewName, int Index)
        {
            if (Index >= 0 && Index <  Coordinates.Count)
            {
                Coordinates[Index].TileName = NewName;
            }
        }
        public List<Vector2> GetCoordinates(string TileName)
        {
            MyCoordinates.Clear();
            for (int i = 0; i < Coordinates.Count; i++)
            {
                if (Coordinates[i].TileName == TileName)
                {
                    MyCoordinates.Add(Coordinates[i].MyCoordinate);
                }
            }
            return MyCoordinates;
        }

        public List<Vector2> GetCoordinates()
        {
            return GetCoordinates(new TileMap());
        }

        private int TextureCoordinateIndex = 0;
        private int TextureNameIndex = 0;
        /// <summary>
        /// Gets coordinates per side index to add onto the mesh
        /// Need to know the textures position in the TileMap and Add it to the UVs
        /// </summary>
        public List<Vector2> GetCoordinates(TileMap MyInfo)
        {
            MyTextureCoords.Clear();
            //Debug.Log("Getting texture map coordinates with : " + MyInfo.TilesLengthX + ":" + MyInfo.TilesLengthY);
            //TextureMaker MyTextureMaker = MyData.MyTextureMaker;// TextureMaker.Get();
            TileMapName = "";
            TileIndex = 0;
            for (TextureCoordinateIndex = 0; TextureCoordinateIndex < Coordinates.Count; TextureCoordinateIndex++)
            {
                // For each coordinate, check if texture name changes
                // if it changed, then get the index for it
                if (Coordinates[TextureCoordinateIndex].TileName != TileMapName)
                {
                    TileMapName = Coordinates[TextureCoordinateIndex].TileName;
                    TileIndex = -1;
                    for (TextureNameIndex = 0; TextureNameIndex < DataManager.Get().GetSizeElements(DataFolderNames.VoxelDiffuseTextures); TextureNameIndex++)
                    {
                        if (DataManager.Get().GetName(DataFolderNames.VoxelDiffuseTextures, TextureNameIndex) == Coordinates[TextureCoordinateIndex].TileName)//.DiffuseTextures[j].name == Coordinates[i].TileName)
                        {
                            TileIndex = TextureNameIndex;
                            break;
                        }
                    }
                    if (TileIndex == -1)
                    {
                        //Debug.LogError(Coordinates[i].TileName + " not found in voxel manager!");
                        TileIndex = 0;
                    }
                }
                MyUV = Coordinates[TextureCoordinateIndex].MyCoordinate;
                // now alter uvs depending on position in map
                TilePosition = MyInfo.GetTilePosition(TileIndex);
                //Debug.Log("Correcting uv  TilePosition:" + TilePosition.ToString() + ":MyUV:" + MyUV.ToString());
                MyUV = TilePosition + (MyUV / (float)MyInfo.TilesLengthX);  // also divide it by the resolution of the TileMap  0.99f* 
                MyTextureCoords.Add(MyUV);
            }
            return MyTextureCoords;
        }

        #region Files
        /// <summary>
        /// Gets the script for a texture map
        /// </summary>
        /// <returns></returns>
        public List<string> GetScript()
        {
            List<string> Data = new List<string>();
            string TileName = "";
            for (int i = 0; i < Coordinates.Count; i++)
            {
                if (Coordinates[i].TileName != TileName)
                {
                    TileName = Coordinates[i].TileName;
                    Data.Add("tile_" + Coordinates[i].TileName);
                }
                Data.Add("uv_" + Coordinates[i].MyCoordinate.x + "_" + Coordinates[i].MyCoordinate.y);
            }
            return Data;
        }
        /// <summary>
        /// Loads a texture map
        /// </summary>
        /// <param name="Data"></param>
        public void RunScript(List<string> Data)
        {
            string TileName = "";
            for (int i = 0; i < Data.Count; i++)
            {
                string[] SplitData = Data[i].Split('_');
                if (SplitData.Length > 0)
                {
                    if (SplitData[0] == "tile")
                    {
                        TileName = Data[i].Substring(5, Data[i].Length - 5);
                    }
                    else
                    {
                        VoxelCoordinate NewCoordinate = new VoxelCoordinate();
                        NewCoordinate.MyCoordinate = 
                            new Vector2(float.Parse(SplitData[1]),
                            float.Parse(SplitData[2]));
                        NewCoordinate.TileName = TileName;
                        Coordinates.Add(NewCoordinate);
                    }
                }
            }
        }
        #endregion
    }
}