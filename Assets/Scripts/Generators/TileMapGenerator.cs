using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using Zeltex.Guis.Maker;

namespace Zeltex.Generators
{
    /// <summary>
    /// Generates a tilemap
    /// </summary>
    public class TileMapGenerator : MonoBehaviour
    {
        // this has to effect UVs as well
        public List<Texture2D> MyTextures = new List<Texture2D>();
        public Texture2D OuputTexture;
        public FilterMode MyFilterMode;
        public int MaxX = -1;
        public int MaxY = -1;

        /*TileColors = UpdateTileEdge( 
            TileColors,BlockColors,
            PixelResolution, BufferLength,
            TilePositionX, TilePositionY, 
            TileCountX);*/


        public static Texture2D LoadTextureFromFile(string FileName)
        {
            Texture2D MyTexture = Resources.Load(FileName, typeof(Texture2D)) as Texture2D;	// resources.load uses local file path from resources and not normal file path
            if (MyTexture)
            {
                MyTexture.filterMode = FilterMode.Point;

            }
            return MyTexture;
        }


    }
}