using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using ZeltexTools;
using Zeltex.Util;

namespace Zeltex.Generators
{
    /// <summary>
    /// Generates font at runtime
    /// </summary>
    [ExecuteInEditMode]
    public class FontGenerator : MonoBehaviour
    {
        Text MyText;
        public Font MyFont;
        public Texture2D MyTexture;
        public bool IsDoThing;
        public TileMap MyTileMap;
        public List<Texture2D> CapitalLetters;
        //public List<int> AsciiIndexes;
        int CapitalLettersStart = 65;
        int CapitalLettersEnd = 90;
        //public FontSettings;
        public bool IsDoOtherThing;

        private void Start()
        {
            MyText = GetComponent<Text>();
            //Font MyFont = MyText.font;
            MyTexture = MyFont.material.mainTexture as Texture2D;
            //File.WriteAllBytes("Output.png", MyTexture.EncodeToPNG());
        }

        // Use this for initialization
        void Update()
        {

            if (IsDoThing)
            {
                IsDoThing = false;
                GenerateFont();
            }
        }

        private void GenerateFont()
        {
            if (IsDoOtherThing)
            {
                TileMap NewMap = new TileMap();
                MyTexture = NewMap.CreateTileMap(CapitalLetters);
                File.WriteAllBytes(FileUtil.GetResourcesPath() + "Output.png", MyTexture.EncodeToPNG());
                MyFont.material.mainTexture = (MyTexture);
            }
            // Generate rects per texture i add to
            List<CharacterInfo> MyCharacterInfos = new List<CharacterInfo>();
            float CharacterWidth = 1f / ((float) MyTileMap.TilesLengthX);
            float CharacterHeight = 1f / ((float)MyTileMap.TilesLengthY);
            float PositionX = 0;
            float PositionY = 0;
            for (float i = CapitalLettersStart; i <= CapitalLettersEnd; i++)
            {
                CharacterInfo NewCharacter = new CharacterInfo();
                NewCharacter.index = (int)(i);
                NewCharacter.minX = 0;
                NewCharacter.maxX = 16;
                NewCharacter.minY = 0;
                NewCharacter.maxY = 16;
                float UVPositionX = PositionX * CharacterWidth;
                float UVPositionY = PositionY * CharacterHeight;
                NewCharacter.uvBottomLeft =     new Vector2(UVPositionX,                  UVPositionY);
                NewCharacter.uvTopLeft =        new Vector2(UVPositionX,                  UVPositionY + CharacterHeight);
                NewCharacter.uvTopRight =       new Vector2(UVPositionX + CharacterWidth, UVPositionY + CharacterHeight);
                NewCharacter.uvBottomRight =    new Vector2(UVPositionX + CharacterWidth, UVPositionY);
                NewCharacter.advance = 0;
                //NewCharacter.be
                MyCharacterInfos.Add(NewCharacter);
                PositionX++;
                if (PositionX >= MyTileMap.TilesLengthX)
                {
                    PositionX = 0;
                    PositionY++;
                }
            }

            MyFont.characterInfo = MyCharacterInfos.ToArray();
        }
    }
}