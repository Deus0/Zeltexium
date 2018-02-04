using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Zeltex.Guis.Maker;

namespace Zeltex
{
    /// <summary>
    /// One of these is generated for each TileMap! Each chunk can have its own tile map as well!
    /// </summary>
    [System.Serializable]
    public class TileMap : Element
    {
        [JsonProperty]
        public Vector2 Size = new Vector2(32 * 8f, 32 * 8f);
        [JsonProperty]
        public List<string> TextureNames = new List<string>();
        [JsonProperty]
        public List<Vector2> TexturePositions = new List<Vector2>();

        [JsonIgnore]
        public Texture2D TileMapTexture;
        [JsonIgnore]
        public List<Texture2D> Tiles = new List<Texture2D>();

        // The default variables for our tile map
        public int TilesLengthX = 8;
        public int TilesLengthY = 8;
        public int TileSizeX = 16;
        public int TileSizeY = 16;
        public bool IsMipMaps;
        public bool HasAlpha;

        public TileMap()
        {

        }

        public TileMap(int TileLengthX_, int TileLengthY_, int TileSizeX_, int TileSizeY_)
        {
            Set(TileLengthX_, TileLengthY_, TileSizeX_, TileSizeY_);
        }

        public void Set(int TileLengthX_, int TileLengthY_, int TileSizeX_, int TileSizeY_)
        {
            TilesLengthX = TileLengthX_;
            TilesLengthY = TileLengthY_;
            TileSizeX = TileSizeX_;
            TileSizeY = TileSizeY_;
        }

        public Vector2 GetTilePosition(int TileIndex)
        {
            //float TotalTiles = TilesLengthX * TilesLengthY;
            float PosX = 0;
            float PosY = 0;
            if (TilesLengthX != 1)
            {
                PosX = (TileIndex % (TilesLengthX));  // - 1
            }
            if (TilesLengthY != 1)
            {
                PosY = (TileIndex / (TilesLengthY)); //- 1
            }
            // get float point position from Tile index position
            PosX /= ((float)TilesLengthX);
            PosY /= ((float)TilesLengthY);
            return new Vector2(PosX, PosY);
        }
        // Generate TileMap function here!

        // Add Texture

        // Remove Texture

        #region Generate



        public Texture2D CreateTileMap(List<Texture2D> TiledTextures, int OriginalTileCountX)
        {
            if (TiledTextures.Count == 0)
            {
                Debug.LogError("No Textures for Tilemap.");
                return null;
            }
            Set(OriginalTileCountX, OriginalTileCountX, TiledTextures[0].width, TiledTextures[0].width);
            return CreateTileMap(TiledTextures);
        }

        /// <summary>
        /// Grabs all the textures and chucks them into a tile map
        /// </summary>
        public Texture2D CreateTileMap(List<Texture2D> TiledTextures)
        {
            int BufferLength = Voxels.PolyModel.BufferLength;
            if (TiledTextures.Count == 0
                || TiledTextures[0] == null)
            {
                //Debug.LogError("TiledTextures count is 0. Cannot create a tile map.");
                return null;
            }
            // remove any that are different size
            TiledTextures = CheckForInconsistencies(TiledTextures);
            //int PixelResolution = TiledTextures[0].width;
            int TileCountX = TiledTextures.Count;
            //int MaxX = MyTileMapInfo.TileSizeX;
            //int MaxY = OriginalTileCountX;
            /*if (MyTileMapInfo.TilesLengthX != -1)
            {
            }*/
            TileCountX = TilesLengthX;

            Texture2D NewTileMap = new Texture2D(
                TileSizeX * TilesLengthX,    // 8 x 16 = 128
                TileSizeY * TilesLengthY,
                TextureFormat.ARGB32,
                Zeltex.Voxels.World.IsMipMaps);

            NewTileMap.filterMode = FilterMode.Point;
            NewTileMap.wrapMode = TextureWrapMode.Clamp;

            int MaxTextures = (TilesLengthX - BufferLength) * (TilesLengthY - BufferLength);
            Color32[] TileColors = NewTileMap.GetPixels32(0);
            // Start with blank Colors!
            /*for (int i = 0; i < TileColors.Length; i++)
            {
                TileColors[i] = new Color32(255, 255, 255, 255);
            }*/
            int TileIndex = -1;  // Our real index !
            for (int i = 0; i < TiledTextures.Count; i++)
            {
                TileIndex++;
                if (TiledTextures[i] != null && i < MaxTextures)
                {
                    /*int DatDivisionDoe = ((TileIndex + 1) % ((int)(TileCountX)));
                    if (DatDivisionDoe == 0 && i != 0)
                    {
                        TileIndex += BufferLength;
                    }*/
                    int TilePositionX = (TileIndex / TileCountX);
                    int TilePositionY = (TileIndex % TileCountX);
                    Color32[] BlockColors = TiledTextures[i].GetPixels32(0);
                    TileColors = PlaceTile(
                        TileColors, BlockColors,
                        TileSizeX, BufferLength,
                        TilePositionX, TilePositionY,
                        TileCountX);
                }
            }
            NewTileMap.SetPixels32(TileColors, 0);
            if (IsMipMaps)
            {
                NewTileMap = GenerateMipMaps(NewTileMap, TileCountX);
            }
            if (HasAlpha)
            {
                //NewTileMap.alphaIsTransparency = true;
            }
            NewTileMap.Apply(false);    // don't automate mipmaps
            return NewTileMap;
        }

        /// <summary>
        /// Check the list and make sure things are good!
        /// Assuming that texture width and height are the same
        /// </summary>
        public static List<Texture2D> CheckForInconsistencies(List<Texture2D> TiledTextures)
        {
            Vector2 LargestSize = new Vector2(TiledTextures[0].width, TiledTextures[0].height);
            for (int i = TiledTextures.Count - 1; i >= 1; i--)
            {
                if (TiledTextures[i].width > LargestSize.x)
                {
                    LargestSize = new Vector2(TiledTextures[i].width, TiledTextures[i].height);
                }
            }
            for (int i = TiledTextures.Count - 1; i >= 0; i--)
            {
                if (TiledTextures[i] == null || TiledTextures[i].width != TiledTextures[0].width)
                {
                    TiledTextures[i] = TexturePainter.ResizeTexture(LargestSize, TiledTextures[i]);
                }
            }
            return TiledTextures;
        }

        public static Texture2D GenerateMipMaps(Texture2D MyTexture, int TileCountX)    // tile count x = 8
        {
            //Debug.LogError("Generating Mips: " + TileCountX);
            Color32[] TileColors = MyTexture.GetPixels32(0);
            // Generate MipMaps
            for (int MipMapIndex = 1; MipMapIndex < MyTexture.mipmapCount; MipMapIndex++)
            {
                int DivisionCount = ((int)Mathf.Pow(4, MipMapIndex));
                //Debug.LogError(MipMapIndex + " !Generating Mips! " + DivisionCount);
                int DivisionCount2 = ((int)Mathf.Pow(2, MipMapIndex));
                Color32[] MyMipMapColors = new Color32[(TileColors.Length) / DivisionCount];

                //Debug.LogError("Testing DivisionCount div 3: " + GetTileIndex(DivisionCount / 3, MyTexture));
                for (int i = 0; i < MyMipMapColors.Length; i++)
                {
                    //int OriginalTextureIndex = i * DivisionCount;
                    int MipMapWidth = MyTexture.width / DivisionCount2;
                    int MipMapPosX = (i / MipMapWidth);
                    int MipMapPosY = (i % MipMapWidth);
                    int OriginalTextureIndex2 = MipMapPosX * MipMapWidth * DivisionCount2 * DivisionCount2 + MipMapPosY * DivisionCount2;  //
                    Color32 MyCommonColor = TileColors[OriginalTextureIndex2];
                    int OriginalTileIndex = GetTileIndex(OriginalTextureIndex2, MyTexture);
                    float R = 0; float G = 0; float B = 0;
                    int AddedColors = 0;
                    for (int a = 0; a < DivisionCount; a++)
                    {
                        int PixelTileIndex = GetTileIndex(OriginalTextureIndex2 + a, MyTexture);
                        if (PixelTileIndex == OriginalTileIndex)
                        {
                            R += TileColors[OriginalTextureIndex2 + a].r;
                            G += TileColors[OriginalTextureIndex2 + a].g;
                            B += TileColors[OriginalTextureIndex2 + a].b;
                            AddedColors++;
                        }
                    }
                    if (AddedColors != 0)
                    {
                        MyCommonColor.r = (byte)(R / AddedColors);
                        MyCommonColor.g = (byte)(G / AddedColors);
                        MyCommonColor.b = (byte)(B / AddedColors);
                    }
                    MyMipMapColors[i] = MyCommonColor;
                    /*if (MipMapIndex >= 2)
                    {
                        MyMipMapColors[i] = new Color32(
                            (byte)(MyCommonColor.r / (MipMapIndex)),
                            (byte)(MyCommonColor.g / (MipMapIndex)), 
                            (byte)(MyCommonColor.b / (MipMapIndex)), 
                            255);
                    }*/
                }
                MyTexture.SetPixels32(MyMipMapColors, MipMapIndex);
            }
            return MyTexture;
        }

        /// <summary>
        ///  uses buffering and finds what the tile index is
        /// </summary>
        public static int GetTileIndex(int PixelIndex, Texture2D MyTexture)
        {
            int BufferLength = Zeltex.Voxels.PolyModel.BufferLength;
            //int BufferLength = 1;
            //int OriginalTileCountX = 8;
            //int MaxTextures = (OriginalTileCountX - BufferLength) * (OriginalTileCountX - BufferLength);
            int TileWidth = 16 + 2 * BufferLength;  // 18
            int TileCountX = MyTexture.width / TileWidth;
            // position in texture
            int PosX = (PixelIndex / TileWidth);
            int PosY = (PixelIndex % TileWidth);
            return (PosX * TileCountX + PosY);
        }

        /// <summary>
        /// Places a tile on our tile map
        /// </summary>
        private static Color32[] PlaceTile(
            Color32[] TileColors,
            Color32[] BlockColors,
            int PixelResolution,
            int BufferLength,
            int TilePositionX,
            int TilePositionY,
            int TileCountX)
        {
            for (int i = 0; i < PixelResolution; i++)
            {
                for (int j = 0; j < PixelResolution; j++)
                {
                    int TileIndex = Mathf.FloorToInt(i * PixelResolution + j);
                    // Get x and y index of our entire pixel position
                    int i2 = i + BufferLength + TilePositionX * (BufferLength * 2 + PixelResolution);
                    int j2 = j + BufferLength + TilePositionY * (BufferLength * 2 + PixelResolution);
                    int TileMapIndex = Mathf.FloorToInt(i2 * TileCountX * PixelResolution + j2);
                    if (TileIndex < BlockColors.Length && TileMapIndex < TileColors.Length)
                    {
                        TileColors[TileMapIndex] = BlockColors[TileIndex];
                    }
                }
            }
            return TileColors;
        }
        /// <summary>
        /// Updates the surrounding pixels of a tile with a buffer layer
        /// </summary>
        private static Color32[] UpdateTileEdge(
            Color32[] TileColors,
            Color32[] BlockColors,
            int PixelResolution,
            int BufferLength,
            int TilePositionX,
            int TilePositionY,
            int TileCountX)
        {

            // buffers texture edges 
            // also update the edge
            for (int i = -1; i <= PixelResolution; i++)
                for (int j = -1; j <= PixelResolution; j++)
                {
                    if (i == -1 || j == -1 || i == PixelResolution || j == PixelResolution)
                    {
                        int i2 = i + BufferLength + TilePositionX * (BufferLength * 2 + Mathf.RoundToInt(PixelResolution));   // the size fot the tilemap
                        int j2 = j + BufferLength + TilePositionY * (BufferLength * 2 + Mathf.RoundToInt(PixelResolution));   // the size fot the tilemap

                        //int PixelIndex1 = Mathf.RoundToInt(i * PixelResolution + j);
                        int PixelIndex2 = Mathf.RoundToInt(i2 * TileCountX * PixelResolution + j2);

                        if (PixelIndex2 >= 0 && PixelIndex2 < TileColors.Length)
                        {
                            if (i == -1 && (j > -1 && j < PixelResolution)) // bottom line
                            {
                                int PixelIndex1 = Mathf.RoundToInt(0 * PixelResolution + j);
                                TileColors[PixelIndex2] = BlockColors[PixelIndex1];
                            }
                            // top line
                            else if (i == PixelResolution && (j > -1 && j < PixelResolution))
                            {
                                int PixelIndex1 = Mathf.RoundToInt((PixelResolution - 1) * PixelResolution + j);
                                TileColors[PixelIndex2] = BlockColors[PixelIndex1];
                            }
                            else if (j == PixelResolution && (i > -1 && i < PixelResolution))
                            {
                                int PixelIndex1 = Mathf.RoundToInt(i * PixelResolution + (PixelResolution - 1));
                                TileColors[PixelIndex2] = BlockColors[PixelIndex1];
                            }
                            else if (j == -1 && (i > -1 && i < PixelResolution))
                            {
                                int PixelIndex1 = Mathf.RoundToInt(i * PixelResolution + (0));
                                TileColors[PixelIndex2] = BlockColors[PixelIndex1];
                            }
                            // corners
                            else if (i == -1 && j == -1)
                            {
                                int PixelIndex1 = Mathf.RoundToInt(0 * PixelResolution + 0);
                                if (PixelIndex1 >= 0 && PixelIndex1 < BlockColors.Length)
                                {
                                    TileColors[PixelIndex2] = BlockColors[PixelIndex1];
                                }
                            }
                            else if (i == PixelResolution && j == -1)
                            {
                                int PixelIndex1 = Mathf.RoundToInt((PixelResolution - 1) * PixelResolution + 0);
                                TileColors[PixelIndex2] = BlockColors[PixelIndex1];
                            }
                            else if (i == -1 && j == PixelResolution)
                            {
                                int PixelIndex1 = Mathf.RoundToInt((0) * PixelResolution + (PixelResolution - 1));
                                TileColors[PixelIndex2] = BlockColors[PixelIndex1];
                            }
                            else if (i == PixelResolution && j == PixelResolution)
                            {
                                int PixelIndex1 = Mathf.RoundToInt((PixelResolution - 1) * PixelResolution + (PixelResolution - 1));
                                TileColors[PixelIndex2] = BlockColors[PixelIndex1];
                            }
                            else
                            {
                                TileColors[PixelIndex2] = new Color32(0, 0, 0, 255);
                            }
                        }
                    }
                }
            return TileColors;
        }
        #endregion

        /// <summary>
        /// Grabs all the textures and chucks them into a tile map
        /// </summary>
        public static Texture2D CreateTileMapMeta(List<Texture2D> TiledTextures, int OriginalTileCountX) // Texture2D NewTileMap, 
        {
            int BufferLength = Zeltex.Voxels.PolyModel.BufferLength;

            int PixelResolution = TiledTextures[0].width;
            int TileCountX = TiledTextures.Count;
            int MaxX = OriginalTileCountX;
            int MaxY = OriginalTileCountX;
            if (MaxX != -1)
            {
                TileCountX = MaxX;
            }
            int TileLengthY = MaxY;

            Texture2D NewTileMap = new Texture2D(
                PixelResolution * TileCountX,    // 8 x 16 = 128
                PixelResolution * TileLengthY,
                TextureFormat.ARGB32,
                Zeltex.Voxels.World.IsMipMaps);
            NewTileMap.filterMode = FilterMode.Point;
            NewTileMap.wrapMode = TextureWrapMode.Clamp;

            int MaxTextures = (OriginalTileCountX - BufferLength) * (OriginalTileCountX - BufferLength);
            Color32[] TileColors = NewTileMap.GetPixels32(0);

            int TileIndex = -1;  // Our real index !
            for (int i = 0; i < TiledTextures.Count; i++)
            {
                TileIndex++;
                if (TiledTextures[i] && i < MaxTextures)
                {
                    int DatDivisionDoe = ((TileIndex + 1) % ((int)(TileCountX)));
                    if (DatDivisionDoe == 0 && i != 0)
                    {
                        TileIndex += BufferLength;
                    }
                    int TilePositionX = (TileIndex / TileCountX);
                    int TilePositionY = (TileIndex % TileCountX);
                    Texture2D BlockTexture = TiledTextures[i];
                    Color32[] BlockColors = BlockTexture.GetPixels32(0);
                    TileColors = PlaceTileMeta(
                        TileColors, BlockColors,
                        PixelResolution, BufferLength,
                        TilePositionX, TilePositionY,
                        TileCountX,
                        TileIndex);
                }
            }
            NewTileMap.SetPixels32(TileColors, 0);
            if (Zeltex.Voxels.World.IsMipMaps)
            {
                NewTileMap = GenerateMipMaps(NewTileMap, TileCountX);
            }
            NewTileMap.Apply(false);    // don't automate mipmaps
            return NewTileMap;
        }

        private static Color32[] PlaceTileMeta(
           Color32[] TileColors,
           Color32[] BlockColors,
           int PixelResolution,
           int BufferLength,
           int TilePositionX,
           int TilePositionY,
           int TileCountX,
           int TileIndex)
        {
            for (int i = 0; i < PixelResolution; i++)
            {
                for (int j = 0; j < PixelResolution; j++)
                {
                    // Get x and y index of our entire pixel position
                    int i2 = i + BufferLength + TilePositionX * (BufferLength * 2 + Mathf.RoundToInt(PixelResolution));
                    int j2 = j + BufferLength + TilePositionY * (BufferLength * 2 + Mathf.RoundToInt(PixelResolution));

                    int PixelIndex1 = Mathf.RoundToInt(i * PixelResolution + j);
                    int PixelIndex2 = Mathf.RoundToInt(i2 * TileCountX * PixelResolution + j2);
                    if (PixelIndex1 < BlockColors.Length && PixelIndex2 < TileColors.Length)
                    {
                        TileColors[PixelIndex2] = new Color32((byte)TileIndex, (byte)TileIndex, (byte)TileIndex, (byte)TileIndex);
                    }
                }
            }
            return TileColors;
        }
    }
}
