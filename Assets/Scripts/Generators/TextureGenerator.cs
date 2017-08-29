using UnityEngine;
using System.Collections.Generic;
using Zeltex.Util;
using Zeltex.Guis.Maker;

namespace Zeltex.Generators
{
    /// <summary>
    /// Generates textures.
    /// To Do:
    ///      - Rectangular Voroni - keep expanding rectangle sides until no white left
    ///      - Grass - Something more similar
    ///      - Cobble stone - chose colours too - noise onto the voroni
    /// </summary>
    public class TextureGenerator : MonoBehaviour
    {
        #region Variables
        [Header("Reference")]
        public TextureEditor MyTextureEditor;
        [Header("Noise")]
        public float NoiseFrequency = 0.2f;
        public float NoiseAmplitude = 1f;
        public Vector2 NoiseLimits = new Vector2(0.2f, 0.8f);
        public float NoiseMinimum = 0.3f;
        // Biomeish type maps
        //[Header("Biomeish")]
        //public int MaxPoints = 8;
        Color32 VoroniLineColor = Color.black;
        public bool IsReduceBrightness = false;
        public bool IsVoroniLines;
        public float ReductionRate = 0.9f;
        Color32 MyColor;
        Color32 MySecondaryColor;
        public bool IsBrickShading = false;
        private static TextureGenerator Instance;
        public Vector2 NoiseOffset;
        #endregion

        public static TextureGenerator Get()
        {
#if UNITY_EDITOR
            if (Instance == null)
            {
                // for in editor
                GameObject Generators = GameObject.Find("Generators");
                if (Generators)
                {
                    Instance = Generators.GetComponent<TextureGenerator>();
                }
            }
#endif
            return Instance;
        }

        private void Start()
        {
            Instance = this;
        }

        #region TexEditor
        private void NewInstruction(string InstructionText)
        {
            if (MyTextureEditor)
            {
                MyTextureEditor.NewInstruction(InstructionText);
            }
        }
        private Color32 GetMainColor()
        {
            if (MyTextureEditor)
            {
                return MyTextureEditor.GetMainColor();
            }
            else
            {
                return Color.red;
            }
        }
        private Color32 GetSecondaryColor()
        {
            if (MyTextureEditor)
            {
                return MyTextureEditor.GetSecondaryColor();
            }
            else
            {
                return Color.blue;
            }
        }
        void UpdateColors()
        {
            if (MyTextureEditor)
            {
                MyTextureEditor.UpdatePrimaryColor(MyColor);
                MyTextureEditor.UpdateSecondaryColor(MySecondaryColor);
            }
        }
        #endregion

        #region TextureEditor
        public void Bricks()
        {
            NewInstruction("Bricks");
            GetColours();
            Bricks(MyTextureEditor.GetTexture());
        }

        public void Noise()
        {
            NewInstruction("Noise");
            GetColours();
            Noise(MyTextureEditor.GetTexture());
        }
        public void Voroni()
        {
            NewInstruction("Voroni");
            Voroni(MyTextureEditor.GetTexture());
        }
        #endregion

        #region Colors
        public void RandomColors()
        {
            MyColor = new Color32(
                (byte)Random.Range(88, 255),
                (byte)Random.Range(88, 255),
                (byte)Random.Range(88, 255),
                255);
            MySecondaryColor = new Color32(
                (byte)Random.Range(0, 180),
                (byte)Random.Range(0, 180),
                (byte)Random.Range(0, 180),
                255);
            /*MySecondaryColor = new Color32(
                (byte)(MyColor.r + Random.Range(-55, -145)),
                (byte)(MyColor.g + Random.Range(-55, -145)),
                (byte)(MyColor.b + Random.Range(-555, -145)),
                255);*/
            UpdateColors();
        }

        public void SetPrimaryColor(Color32 NewPrimary)
        {
            MyColor = NewPrimary;
            UpdateColors();
        }
        public void SetSecondaryColor(Color32 NewPrimary)
        {
            MySecondaryColor = NewPrimary;
            UpdateColors();
        }

        public void SetColors(Color32 NewPrimaryColor, Color32 NewSecondaryColor)
        {
            MyColor = NewPrimaryColor;
            MySecondaryColor = NewSecondaryColor;
            UpdateColors();
        }

        void GetColours()
        {
            MyColor = GetMainColor();
            MySecondaryColor = GetSecondaryColor();
        }
        void ClearColors(Color32 ColorToClearWith)
        {
            ClearColors(MyTextureEditor.GetTexture(), ColorToClearWith);
        }

        void ClearColors(Texture2D MyTexture, Color32 ColorToClearWith)
        {
            GetColours();
            Color32[] PixelColors = MyTexture.GetPixels32(0);
            //Debug.LogError("Cllearing Colors with: " + ColorToClearWith.ToString() + " - With length of " + PixelColors.Length);
            for (int i = 0; i < PixelColors.Length; i++)
            {
                PixelColors[i] = ColorToClearWith;
            }
            SetPixels(MyTexture, PixelColors);
        }
        private void SetPixels(Texture2D MyTexture, Color32[] PixelColors)
        {
            MyTexture.SetPixels32(PixelColors, 0);
            MyTexture.Apply(true);
        }
        #endregion

        public void Fill(Texture2D MyTexture)
        {
            Fill(MyTexture, MyColor);
        }

        public void Fill(Texture2D MyTexture, Color32 FillColor)
        {
            NewInstruction("Fill");
            GetColours();
            Color32[] PixelColors = MyTexture.GetPixels32(0);
            for (int i = 0; i < MyTexture.width; i++)
            {
                for (int j = 0; j < MyTexture.height; j++)
                {
                    PixelColors[TextureEditor.GetPixelIndex(i, j, MyTexture.width)] = FillColor;
                }
            }
            SetPixels(MyTexture, PixelColors);
        }
        public void Bricks(Texture2D MyTexture)
        {
            Vector2 NoiseOffset = new Vector2(Random.Range(0, 10000), Random.Range(0, 10000));
            Color32[] PixelColors = MyTexture.GetPixels32(0);
            // Divide with boxes
            int DivisionsX = 3;  // alternative offset
            int DivisionsY = 3;
            int SizeX = (MyTexture.width) / DivisionsX;
            int SizeY = (MyTexture.height) / DivisionsY;
            for (int i = 0; i < MyTexture.width; i++)
            {
                for (int j = 0; j < MyTexture.height; j++)
                {
                    PixelColors[TextureEditor.GetPixelIndex(i, j, MyTexture.width)] = MySecondaryColor;
                }
            }
            bool IsAlternative = false;
            for (int j = 0; j < MyTexture.height; j++)
            {
                for (int i = 0; i < MyTexture.width; i++)
                {
                    if (!IsAlternative)
                    {
                        if (i % SizeX == 0 || j % SizeY == 0
                             || i == MyTexture.width - 1 || j == MyTexture.height - 1)
                        {
                            PixelColors[TextureEditor.GetPixelIndex(i, j, MyTexture.width)] = MyColor;
                        }
                    }
                    else
                    {
                        if ((i+ SizeX/2) % SizeX == 0 || j % SizeY == 0
                            || j == MyTexture.height - 1)
                        {
                            PixelColors[TextureEditor.GetPixelIndex(i, j, MyTexture.width)] = MyColor;// new Color32(255, 0, 0, 255);
                        }
                    }
                    if (j != 0 && j % SizeY == 0 && i == MyTexture.width - 1)
                    {
                        IsAlternative = !IsAlternative;
                        //PixelColors[MyTextureEditor.GetPixelIndex(i, j, MyTexture.width)] = MySecondaryColor;// new Color32(0, 255, 0, 255);
                    }
                }
            }
            if (IsBrickShading)
            {
                // Shading
                int EdgeTop = 3;
                int EdgeBottom = 7;
                int EdgeLeft = 3;
                int EdgeRight = 3;
                float PerlinFrequency = 0.4f;
                float PerlinAmplitude = 40;
                Color32 MyShadeColor = MySecondaryColor;
                for (int j = 0; j < MyTexture.height; j++)
                {
                    for (int i = 0; i < MyTexture.width; i++)
                    {
                        if (j != MyTexture.height - 1) // EdgeTop
                        {
                            for (int k = 1; k <= EdgeBottom; k++)
                            {
                                int DistanceFromEdge = EdgeBottom - k + 1;
                                float Modifier = ((float)DistanceFromEdge) / EdgeBottom;
                                int LinePosition = Mathf.Clamp(j + k, 0, MyTexture.height - 1);
                                Color32 ThisColor = PixelColors[TextureEditor.GetPixelIndex(i, j, MyTexture.width)];
                                Color32 ThisColor1 = PixelColors[TextureEditor.GetPixelIndex(i, LinePosition, MyTexture.width)];
                                Color32 ThisColor2 = PixelColors[TextureEditor.GetPixelIndex(Mathf.Clamp(i + 1, 0, MyTexture.width - 1), LinePosition, MyTexture.width)];
                                Color32 ThisColor3 = PixelColors[TextureEditor.GetPixelIndex(Mathf.Clamp(i - 1, 0, MyTexture.width - 1), LinePosition, MyTexture.width)];
                                if (ThisColor.r == MyColor.r && ThisColor.g == MyColor.g && ThisColor.b == MyColor.b
                                    && ThisColor1.r == MySecondaryColor.r && ThisColor1.g == MySecondaryColor.g && ThisColor1.b == MySecondaryColor.b
                                    && ThisColor2.r == MySecondaryColor.r && ThisColor2.g == MySecondaryColor.g && ThisColor2.b == MySecondaryColor.b
                                    && ThisColor3.r == MySecondaryColor.r && ThisColor3.g == MySecondaryColor.g && ThisColor3.b == MySecondaryColor.b
                                    )
                                {
                                    PixelColors[TextureEditor.GetPixelIndex(i, j, MyTexture.width)] = new Color32(
                                                (byte)(MyShadeColor.r - Modifier * PerlinAmplitude * Mathf.PerlinNoise(i * PerlinFrequency, j * PerlinFrequency)),    //MyColor.r * 
                                                (byte)(MyShadeColor.g - Modifier * PerlinAmplitude * Mathf.PerlinNoise(i * PerlinFrequency, j * PerlinFrequency)),
                                                (byte)(MyShadeColor.b - Modifier * PerlinAmplitude * Mathf.PerlinNoise(i * PerlinFrequency, j * PerlinFrequency)),
                                                  255);
                                }
                            }
                            for (int k = 1; k <= EdgeTop; k++)
                            {
                                int DistanceFromEdge = EdgeTop - k + 1;
                                float Modifier = ((float)DistanceFromEdge) / EdgeBottom;
                                int LinePosition = Mathf.Clamp(j - k, 0, MyTexture.height - 1);
                                Color32 ThisColor = PixelColors[TextureEditor.GetPixelIndex(i, j, MyTexture.width)];
                                Color32 ThisColor1 = PixelColors[TextureEditor.GetPixelIndex(i, LinePosition, MyTexture.width)];
                                Color32 ThisColor2 = PixelColors[TextureEditor.GetPixelIndex(Mathf.Clamp(i + 1, 0, MyTexture.width - 1), LinePosition, MyTexture.width)];
                                Color32 ThisColor3 = PixelColors[TextureEditor.GetPixelIndex(Mathf.Clamp(i - 1, 0, MyTexture.width - 1), LinePosition, MyTexture.width)];
                                if (ThisColor.r == MyColor.r && ThisColor.g == MyColor.g && ThisColor.b == MyColor.b
                                    && ThisColor1.r == MySecondaryColor.r && ThisColor1.g == MySecondaryColor.g && ThisColor1.b == MySecondaryColor.b
                                    && ThisColor2.r == MySecondaryColor.r && ThisColor2.g == MySecondaryColor.g && ThisColor2.b == MySecondaryColor.b
                                    && ThisColor3.r == MySecondaryColor.r && ThisColor3.g == MySecondaryColor.g && ThisColor3.b == MySecondaryColor.b
                                    )
                                {
                                    PixelColors[TextureEditor.GetPixelIndex(i, j, MyTexture.width)] = new Color32(
                                                (byte)(MyShadeColor.r - Modifier * PerlinAmplitude * Mathf.PerlinNoise(i * PerlinFrequency, j * PerlinFrequency)),    //MyColor.r * 
                                                (byte)(MyShadeColor.g - Modifier * PerlinAmplitude * Mathf.PerlinNoise(i * PerlinFrequency, j * PerlinFrequency)),
                                                (byte)(MyShadeColor.b - Modifier * PerlinAmplitude * Mathf.PerlinNoise(i * PerlinFrequency, j * PerlinFrequency)),
                                                  255);
                                }
                            }
                            for (int k = 1; k <= EdgeLeft; k++)
                            {
                                int DistanceFromEdge = EdgeLeft - k + 1;
                                float Modifier = ((float)DistanceFromEdge) / EdgeBottom;
                                Color32 ThisColor = PixelColors[TextureEditor.GetPixelIndex(i, j, MyTexture.width)];
                                int LinePosition = Mathf.Clamp(i - k, 0, MyTexture.width - 1);
                                Color32 ThisColor1 = PixelColors[TextureEditor.GetPixelIndex(LinePosition, j, MyTexture.width)];
                                Color32 ThisColor2 = PixelColors[TextureEditor.GetPixelIndex(LinePosition, Mathf.Clamp(j + 1, 0, MyTexture.width - 1), MyTexture.width)];
                                Color32 ThisColor3 = PixelColors[TextureEditor.GetPixelIndex(LinePosition, Mathf.Clamp(j - 1, 0, MyTexture.width - 1), MyTexture.width)];
                                if (ThisColor.r == MyColor.r && ThisColor.g == MyColor.g && ThisColor.b == MyColor.b
                                    && ThisColor1.r == MySecondaryColor.r && ThisColor1.g == MySecondaryColor.g && ThisColor1.b == MySecondaryColor.b
                                    && ThisColor2.r == MySecondaryColor.r && ThisColor2.g == MySecondaryColor.g && ThisColor2.b == MySecondaryColor.b
                                    && ThisColor3.r == MySecondaryColor.r && ThisColor3.g == MySecondaryColor.g && ThisColor3.b == MySecondaryColor.b
                                    )
                                {
                                    PixelColors[TextureEditor.GetPixelIndex(i, j, MyTexture.width)] = new Color32(
                                                (byte)(MyShadeColor.r - Modifier * PerlinAmplitude * Mathf.PerlinNoise(i * PerlinFrequency, j * PerlinFrequency)),    //
                                                (byte)(MyShadeColor.g - Modifier * PerlinAmplitude * Mathf.PerlinNoise(i * PerlinFrequency, j * PerlinFrequency)),
                                                (byte)(MyShadeColor.b - Modifier * PerlinAmplitude * Mathf.PerlinNoise(i * PerlinFrequency, j * PerlinFrequency)),
                                                  255);
                                }
                            }
                            for (int k = 1; k <= EdgeRight; k++)
                            {
                                int DistanceFromEdge = EdgeRight - k + 1;
                                float Modifier = ((float)DistanceFromEdge) / EdgeBottom;
                                Color32 ThisColor = PixelColors[TextureEditor.GetPixelIndex(i, j, MyTexture.width)];
                                int LinePosition = Mathf.Clamp(i + k, 0, MyTexture.width - 1);
                                Color32 ThisColor1 = PixelColors[TextureEditor.GetPixelIndex(LinePosition, j, MyTexture.width)];
                                Color32 ThisColor2 = PixelColors[TextureEditor.GetPixelIndex(LinePosition, Mathf.Clamp(j + 1, 0, MyTexture.width - 1), MyTexture.width)];
                                Color32 ThisColor3 = PixelColors[TextureEditor.GetPixelIndex(LinePosition, Mathf.Clamp(j - 1, 0, MyTexture.width - 1), MyTexture.width)];
                                if (ThisColor.r == MyColor.r && ThisColor.g == MyColor.g && ThisColor.b == MyColor.b
                                    && ThisColor1.r == MySecondaryColor.r && ThisColor1.g == MySecondaryColor.g && ThisColor1.b == MySecondaryColor.b
                                    && ThisColor2.r == MySecondaryColor.r && ThisColor2.g == MySecondaryColor.g && ThisColor2.b == MySecondaryColor.b
                                    && ThisColor3.r == MySecondaryColor.r && ThisColor3.g == MySecondaryColor.g && ThisColor3.b == MySecondaryColor.b
                                    )
                                {
                                    PixelColors[TextureEditor.GetPixelIndex(i, j, MyTexture.width)] = new Color32(
                                                (byte)(MyShadeColor.r - Modifier * PerlinAmplitude * Mathf.PerlinNoise(i * PerlinFrequency, j * PerlinFrequency)),    //
                                                (byte)(MyShadeColor.g - Modifier * PerlinAmplitude * Mathf.PerlinNoise(i * PerlinFrequency, j * PerlinFrequency)),
                                                (byte)(MyShadeColor.b - Modifier * PerlinAmplitude * Mathf.PerlinNoise(i * PerlinFrequency, j * PerlinFrequency)),
                                                  255);
                                }
                            }
                        }
                    }
                }
            }
            SetPixels(MyTexture, PixelColors);
        }

        /// <summary>
        /// Randomizes the noise offset between -1000000 and 1000000
        /// </summary>
        public void RandomizeNoiseOffset()
        {
            float NoiseDifference = 1000000;
            NoiseOffset = new Vector2(Random.Range(-NoiseDifference, NoiseDifference),
                Random.Range(-NoiseDifference, NoiseDifference));
        }

        public bool IsNoiseCutoff;
        public static Color32 DarkenColor(Color32 OriginalColor, float DarkenAmount = 0.7f)
        {
            return new Color32( (byte)(OriginalColor.r * DarkenAmount),
                                (byte)(OriginalColor.g * DarkenAmount),
                                (byte)(OriginalColor.b * DarkenAmount),
                                OriginalColor.a);
        }
        /// <summary>
        /// Generates basic noise as values between primary and secondary colours
        /// </summary>
        public void Noise(Texture2D MyTexture, float Percentage = 1f)
        {
            float MinusPercentage = 1 - Percentage;
            Color32[] PixelColors = MyTexture.GetPixels32(0);

            for (int i = 0; i < MyTexture.width; i++)
            {
                for (int j = 0; j < MyTexture.height; j++)
                {
                    int PixelIndex = TextureEditor.GetPixelIndex(i, j, MyTexture.width);
                    Color32 PixelColor = PixelColors[PixelIndex];
                    //float PositionX = Mathf.Abs(i - Random.Range(MyTexture.width / 2f - 4, MyTexture.width / 2f + 4));
                    //float PositionY = Mathf.Abs(j - Random.Range(MyTexture.width / 2f - 4, MyTexture.width / 2f + 4));

                    //float NoiseValue = NoiseAmplitude * Mathf.PerlinNoise((NoiseOffset.x + i) * NoiseFrequency, (NoiseOffset.y + j) * NoiseFrequency);

                    float NoiseValue = NoiseAmplitude * SimplexNoise.Noise((NoiseOffset.x + i) * NoiseFrequency, (NoiseOffset.y + j) * NoiseFrequency);
                    NoiseValue = (NoiseValue + 1.0f) * 0.5f;

                    //ZeltexTools.SimplexNoise.SeamlessNoise(i* NoiseFrequency, j * NoiseFrequency, NoiseAmplitude, NoiseAmplitude, NoiseOffset.x);
                    NoiseValue += NoiseMinimum;
                    NoiseValue = Mathf.Clamp(NoiseValue, NoiseLimits.x, NoiseLimits.y);

                    int RedValue = Mathf.RoundToInt(Mathf.Lerp(MySecondaryColor.r, MyColor.r, NoiseValue));
                    int GreenValue = Mathf.RoundToInt(Mathf.Lerp(MySecondaryColor.g, MyColor.g, NoiseValue));
                    int BlueValue = Mathf.RoundToInt(Mathf.Lerp(MySecondaryColor.b, MyColor.b, NoiseValue));
                    RedValue = Mathf.Clamp(RedValue, 0, 255);
                    GreenValue = Mathf.Clamp(GreenValue, 0, 255);
                    BlueValue = Mathf.Clamp(BlueValue, 0, 255);
                    if (Percentage != 1)
                    {
                        RedValue = Mathf.RoundToInt(RedValue * Percentage + MinusPercentage * PixelColor.r);
                        GreenValue = Mathf.RoundToInt(GreenValue * Percentage + MinusPercentage * PixelColor.g);
                        BlueValue = Mathf.RoundToInt(BlueValue * Percentage + MinusPercentage * PixelColor.b);
                    }
                    if (IsNoiseCutoff)
                    {
                        if (NoiseValue >= 0.73f)
                        {
                            PixelColors[PixelIndex] = DarkenColor(MyColor, 0.6f);
                        }
                        if (NoiseValue >= 0.72f)
                        {
                            PixelColors[PixelIndex] = MyColor;
                        }
                        else
                        {
                            PixelColors[PixelIndex] = MySecondaryColor;
                        }
                    }
                    else
                    {
                        PixelColors[PixelIndex] = new Color32(
                            (byte)RedValue,
                            (byte)GreenValue,
                            (byte)BlueValue,
                            255);
                    }
                }
            }
            SetPixels(MyTexture, PixelColors);
        }

        /// <summary>
        /// Black at height = 0, White at height = 16
        /// </summary>
        public void ChunkHeight(Texture2D MyTexture, Zeltex.Voxels.Chunk MyChunk)
        {
            Color32[] PixelColors = MyTexture.GetPixels32(0);

            if (MyChunk)
            {
                for (int i = 0; i < MyTexture.width; i++)
                {
                    for (int j = 0; j < MyTexture.height; j++)
                    {
                        int PixelIndex = TextureEditor.GetPixelIndex(i, j, MyTexture.width);
                        int HeightValue = MyChunk.GetHighestPoint(i, j);
                        if (HeightValue != -1)
                        {
                            HeightValue = (int)Mathf.Lerp(0, 255, (15f - (float)HeightValue) / 15f);
                            PixelColors[PixelIndex] = new Color32(
                                (byte)HeightValue,
                                (byte)HeightValue,
                                (byte)HeightValue,
                                255);
                        }
                        else
                        {
                            PixelColors[PixelIndex] = new Color32(38, 2, 5, 255);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < MyTexture.width; i++)
                {
                    for (int j = 0; j < MyTexture.height; j++)
                    {
                        int PixelIndex = TextureEditor.GetPixelIndex(i, j, MyTexture.width);
                        PixelColors[PixelIndex] = new Color32(48, 2, 5, 255);
                    }
                }
            }
            SetPixels(MyTexture, PixelColors);
        }

        public void NoiseMap(Texture2D MyTexture)
        {
            Vector2 NoiseOffset = new Vector2(Random.Range(0, 10000), Random.Range(0, 10000));
            Color32[] PixelColors = MyTexture.GetPixels32(0);

            for (int i = 0; i < MyTexture.width; i++)
            {
                for (int j = 0; j < MyTexture.height; j++)
                {
                    int DistanceToMid = Mathf.RoundToInt(Vector2.Distance(new Vector2(i, j), new Vector2(MyTexture.width, MyTexture.height) / 2));
                    float SkewedDistance = (float) MyTexture.width -  ((float)DistanceToMid / (float)MyTexture.width);
                    //float PositionX = Mathf.Abs(i - Random.Range(MyTexture.width / 2f - 4, MyTexture.width / 2f + 4));
                    //float PositionY = Mathf.Abs(j - Random.Range(MyTexture.width / 2f - 4, MyTexture.width / 2f + 4));
                    float NoiseValue = NoiseAmplitude * SimplexNoise.Noise(i * NoiseFrequency * SkewedDistance, j * NoiseFrequency * SkewedDistance);
                    //ZeltexTools.SimplexNoise.SeamlessNoise(i* NoiseFrequency, j * NoiseFrequency, NoiseAmplitude, NoiseAmplitude, NoiseOffset.x);
                    NoiseValue = (NoiseValue + 1.0f) * .5f;
                    NoiseValue += NoiseMinimum;
                    NoiseValue = Mathf.Clamp(NoiseValue, NoiseLimits.x, NoiseLimits.y);
                    byte RedValue = (byte)(Mathf.RoundToInt(Mathf.Lerp(MySecondaryColor.r, MyColor.r, NoiseValue)));
                    byte GreenValue = (byte)(Mathf.RoundToInt(Mathf.Lerp(MySecondaryColor.g, MyColor.g, NoiseValue)));
                    byte BlueValue = (byte)(Mathf.RoundToInt(Mathf.Lerp(MySecondaryColor.b, MyColor.b, NoiseValue)));
                    RedValue = (byte)Mathf.RoundToInt(Mathf.Clamp(RedValue, 0, 255));
                    GreenValue = (byte)Mathf.RoundToInt(Mathf.Clamp(GreenValue, 0, 255));
                    BlueValue = (byte)Mathf.RoundToInt(Mathf.Clamp(BlueValue, 0, 255));
                    PixelColors[TextureEditor.GetPixelIndex(i, j, MyTexture.width)] = new Color32(RedValue, GreenValue, BlueValue, 255);
                }
            }
            SetPixels(MyTexture, PixelColors);
        }

        public void Circle(Texture2D MyTexture)
        {
            NewInstruction("Circle");
            Vector2 NoiseOffset = new Vector2(Random.Range(0, 10000), Random.Range(0, 10000));
            GetColours();
            Color32[] PixelColors = MyTexture.GetPixels32(0);
            for (int i = 0; i < MyTexture.width; i++)
            {
                for (int j = 0; j < MyTexture.height; j++)
                {
                    float NoiseValue = 1.5f * SimplexNoise.Noise( i * NoiseFrequency, j * NoiseFrequency);
                    Color32 MyColor = PixelColors[TextureEditor.GetPixelIndex(i, j, MyTexture.width)];
                    float Distance = Vector2.Distance(new Vector2(i, j), new Vector2(MyTexture.width / 2, MyTexture.height / 2));   // distance to mid
                    if (Distance > MyTexture.width * 0.5f) // oriinal shape
                    {
                        MyColor.a = 0;
                    }
                    else
                    {   // derformation after
                        //if (Distance > MyTexture.width * NoiseValue)    // second cut off value
                        //    MyColor.a = 0;
                        //else 
                        if (Distance > MyTexture.width * 0.4f)
                            MyColor.a = (byte)(255 * ( (Distance / (MyTexture.width / 2)) + NoiseValue));
                    }
                    PixelColors[TextureEditor.GetPixelIndex(i, j, MyTexture.width)] = MyColor;
                }
            }
            SetPixels(MyTexture, PixelColors);
        }

        bool AreColousEqual(Color32 Color1, Color32 Color2)
        {
            return (Color1.r == Color2.r && Color1.g == Color2.g &&
                            Color1.b == Color2.b && Color1.a == Color2.a);
        }
        Color32 MyBackgroundColor = new Color32(0, 0, 0, 0);
        public void Voroni(Texture2D MyTexture)
        {
            Voroni(MyTexture, true, 6, 10);
        }
        public void Voroni(Texture2D MyTexture, bool IsLines, int ColorVariation, int ColorCount)
        {
            // now start with all colours being empty
            ClearColors(MyTexture, MyBackgroundColor);// );
            // Get Colors
            Color32[] PixelColors = MyTexture.GetPixels32(0);
            PixelColors = CreateVoroniColors(MyTexture, PixelColors, ColorVariation, ColorCount);
            if (IsLines)
            {
                PixelColors = CreateBorderLines(MyTexture, PixelColors, MySecondaryColor);
            }
            // paint all our colours
            SetPixels(MyTexture, PixelColors);
        }

        public Color32[] CreateBorderLines(Texture2D MyTexture, Color32[] PixelColors, Color32 VoroniLineColor)
        {
            for (int i = 0; i < MyTexture.width; i++)
            {
                for (int j = 0; j < MyTexture.height; j++)
                {
                    Vector2 ThisPosition = new Vector2(i, j);
                    int ThisIndex = TextureEditor.GetIndex(MyTexture, ThisPosition);
                    Color32 ThisColor = PixelColors[ThisIndex];
                    if (!AreColousEqual(ThisColor, VoroniLineColor))
                    {
                        Vector2 AbovePosition = ThisPosition + new Vector2(0, 1);
                        Vector2 BelowPosition = ThisPosition + new Vector2(0, -1);
                        Vector2 RightPosition = ThisPosition + new Vector2(1, 0);
                        Vector2 LeftPosition = ThisPosition + new Vector2(-1, 0);
                        if (!TextureEditor.IsPointWithinBounds(MyTexture, AbovePosition))
                            AbovePosition.y = 0;
                        if (!TextureEditor.IsPointWithinBounds(MyTexture, BelowPosition))
                            BelowPosition.y = MyTexture.height - 1;
                        if (!TextureEditor.IsPointWithinBounds(MyTexture, RightPosition))
                            RightPosition.x = 0;
                        if (!TextureEditor.IsPointWithinBounds(MyTexture, LeftPosition))
                            LeftPosition.x = MyTexture.width - 1;

                        int AboveIndex = TextureEditor.GetIndex(MyTexture, AbovePosition);
                        int BelowIndex = TextureEditor.GetIndex(MyTexture, BelowPosition);
                        int RightIndex = TextureEditor.GetIndex(MyTexture, RightPosition);
                        int LeftIndex = TextureEditor.GetIndex(MyTexture, LeftPosition);
                        Color32 AboveColor = PixelColors[AboveIndex];
                        Color32 BelowColor = PixelColors[BelowIndex];
                        Color32 RightColor = PixelColors[RightIndex];
                        Color32 LeftColor = PixelColors[LeftIndex];
                        if (!AreColousEqual(ThisColor, AboveColor))
                        {
                            PixelColors[AboveIndex] = VoroniLineColor;
                        }
                        if (!AreColousEqual(ThisColor, BelowColor))
                        {
                            PixelColors[BelowIndex] = VoroniLineColor;
                        }
                        if (!AreColousEqual(ThisColor, RightColor))
                        {
                            PixelColors[RightIndex] = VoroniLineColor;
                        }
                        if (!AreColousEqual(ThisColor, LeftColor))
                        {
                            PixelColors[LeftIndex] = VoroniLineColor;
                        }
                    }
                }
            }
            return PixelColors;
        }
        /// <summary>
        /// Create voroni colours - using flood fill type algorithm, expands colours
        /// </summary>
        public Color32[] CreateVoroniColors(Texture2D MyTexture, Color32[] PixelColors, int ColorVariation, int ColorCount)
        {
            // First grab 10 random colors
            List<Color32> MyColors = GetRandomColors(ColorCount, ColorVariation);
            // now get random points
            List<Vector2> MyPoints = GetRandomPoints(MyTexture, ColorCount);
            // draw colours in each point
            for (int i = 0; i < MyPoints.Count; i++)
            {
                int PixelIndex = TextureEditor.GetIndex(MyTexture, MyPoints[i]);
                PixelColors[PixelIndex] = MyColors[i];
            }
            // now for each colour, expand until it can't expand anymore
            // bool CanKeepExpanding = false; // is true when all the has finished expanding has been finished

            // For each colour, store the open nodes, the closed ones will be surrounded on all 4 sides by a non background color
            List<Vector2> OpenNodePoints = new List<Vector2>();
            // The starting open nodes are our points
            for (int i = 0; i < MyPoints.Count; i++)
            {
                if (OpenNodePoints.Count == 0)
                {
                    OpenNodePoints.Add(MyPoints[i]);
                }
                else
                {
                    OpenNodePoints.Insert(Random.Range(0, OpenNodePoints.Count - 1), MyPoints[i]);    // insert at random position
                }
            }
            while (OpenNodePoints.Count > 0)
            {
                // for all our open nodes, expand their color
                for (int i = OpenNodePoints.Count - 1; i >= 0; i--)
                {
                    int MyNodeIndex = TextureEditor.GetIndex(MyTexture, OpenNodePoints[i]);
                    //Debug.LogError("Expanding at node: " + OpenNodePoints[i].ToString() + " With Index " + MyNodeIndex + " out of " + PixelColors.Length);
                    Color32 MyNodesColor = PixelColors[MyNodeIndex];
                    Vector2 AbovePosition = OpenNodePoints[i] + new Vector2(0, 1);
                    Vector2 BelowPosition = OpenNodePoints[i] + new Vector2(0, -1);
                    Vector2 RightPosition = OpenNodePoints[i] + new Vector2(1, 0);
                    Vector2 LeftPosition = OpenNodePoints[i] + new Vector2(-1, 0);

                    if (!TextureEditor.IsPointWithinBounds(MyTexture, AbovePosition))
                    {
                        AbovePosition.y = 0;
                    }
                    if (!TextureEditor.IsPointWithinBounds(MyTexture, BelowPosition))
                    {
                        BelowPosition.y = MyTexture.height - 1;
                    }
                    if (!TextureEditor.IsPointWithinBounds(MyTexture, RightPosition))
                    {
                        RightPosition.x = 0;
                    }
                    if (!TextureEditor.IsPointWithinBounds(MyTexture, LeftPosition))
                    {
                        LeftPosition.x = MyTexture.width - 1;
                    }
                    if (TextureEditor.IsPointWithinBounds(MyTexture, AbovePosition))
                    {
                        int AdjacentIndex = TextureEditor.GetIndex(MyTexture, AbovePosition);
                        Color32 AdjacentColor = PixelColors[AdjacentIndex];
                        //Debug.LogError("Painting at new position: " + AbovePosition.ToString() + " and replacing color " + ColorAbove.ToString());
                        if (AreColousEqual(AdjacentColor, MyBackgroundColor))
                        {
                            if (IsReduceBrightness)
                            {
                                PixelColors[AdjacentIndex] = new Color32(((byte)Mathf.RoundToInt(MyNodesColor.r * 0.95f)),
                                                                        ((byte)Mathf.RoundToInt(MyNodesColor.g * 0.95f)),
                                                                        ((byte)Mathf.RoundToInt(MyNodesColor.b * 0.95f)),
                                                                        (byte)MyNodesColor.a);
                            }
                            else
                            {
                                PixelColors[AdjacentIndex] = MyNodesColor;
                            }
                            if (OpenNodePoints.Count == 0)
                            {
                                OpenNodePoints.Add(AbovePosition);
                            }
                            else
                            {
                                OpenNodePoints.Insert(Random.Range(0, OpenNodePoints.Count - 1), AbovePosition);
                            }
                            i++;    // since list has grown
                        }
                    }
                    if (TextureEditor.IsPointWithinBounds(MyTexture, BelowPosition))
                    {
                        int AdjacentIndex = TextureEditor.GetIndex(MyTexture, BelowPosition);
                        Color32 AdjacentColor = PixelColors[AdjacentIndex];
                        if (AreColousEqual(AdjacentColor, MyBackgroundColor))
                        {
                            if (IsReduceBrightness)
                            {
                                PixelColors[AdjacentIndex] = new Color32(((byte)Mathf.RoundToInt(MyNodesColor.r * ReductionRate)),
                                                                        ((byte)Mathf.RoundToInt(MyNodesColor.g * ReductionRate)),
                                                                        ((byte)Mathf.RoundToInt(MyNodesColor.b * ReductionRate)),
                                                                        (byte)MyNodesColor.a);
                            }
                            else
                            {
                                PixelColors[AdjacentIndex] = MyNodesColor;
                            }
                            if (OpenNodePoints.Count == 0)
                            {
                                OpenNodePoints.Add(BelowPosition);
                            }
                            else
                            {
                                OpenNodePoints.Insert(Random.Range(0, OpenNodePoints.Count - 1), BelowPosition);
                            }
                            i++;    // since list has grown
                        }
                    }
                    if (TextureEditor.IsPointWithinBounds(MyTexture, RightPosition))
                    {
                        int AdjacentIndex = TextureEditor.GetIndex(MyTexture, RightPosition);
                        Color32 AdjacentColor = PixelColors[AdjacentIndex];
                        if (AreColousEqual(AdjacentColor, MyBackgroundColor))
                        {
                            if (IsReduceBrightness)
                            {
                                PixelColors[AdjacentIndex] = new Color32(((byte)Mathf.RoundToInt(MyNodesColor.r * ReductionRate)),
                                                                        ((byte)Mathf.RoundToInt(MyNodesColor.g * ReductionRate)),
                                                                        ((byte)Mathf.RoundToInt(MyNodesColor.b * ReductionRate)),
                                                                        (byte)MyNodesColor.a);
                            }
                            else
                            {
                                PixelColors[AdjacentIndex] = MyNodesColor;
                            }
                            if (OpenNodePoints.Count == 0)
                            {
                                OpenNodePoints.Add(RightPosition);
                            }
                            else
                            {
                                OpenNodePoints.Insert(Random.Range(0, OpenNodePoints.Count - 1), RightPosition);
                            }
                            i++;    // since list has grown
                        }
                    }
                    if (TextureEditor.IsPointWithinBounds(MyTexture, LeftPosition))
                    {
                        int AdjacentIndex = TextureEditor.GetIndex(MyTexture, LeftPosition);
                        Color32 AdjacentColor = PixelColors[AdjacentIndex];
                        if (AreColousEqual(AdjacentColor, MyBackgroundColor))
                        {
                            if (IsReduceBrightness)
                            {
                                PixelColors[AdjacentIndex] = new Color32(((byte)Mathf.RoundToInt(MyNodesColor.r * ReductionRate)),
                                                                        ((byte)Mathf.RoundToInt(MyNodesColor.g * ReductionRate)),
                                                                        ((byte)Mathf.RoundToInt(MyNodesColor.b * ReductionRate)),
                                                                        (byte)MyNodesColor.a);
                            }
                            else
                            {
                                PixelColors[AdjacentIndex] = MyNodesColor;
                            }
                            if (OpenNodePoints.Count == 0)
                            {
                                OpenNodePoints.Add(LeftPosition);
                            }
                            else
                            {
                                OpenNodePoints.Insert(Random.Range(0, OpenNodePoints.Count - 1), LeftPosition);
                            }
                            i++;    // since list has grown
                        }
                    }
                    OpenNodePoints.RemoveAt(i);
                }
            }
            return PixelColors;
        }

        public List<Color32> GetRandomColors(int Count, int Variation)
        {
            Color32 MyBeginColor = MyColor;
            Color32 MyEndColor = MySecondaryColor;
            List<Color32> MyColors = new List<Color32>();
            int VariationR = Variation;
            //int VariationG = Variation;
            //int VariationB = Variation;
            Color32 NewColor = new Color32(
                    (byte)Random.Range(MyBeginColor.r, MyEndColor.r),
                    (byte)Random.Range(MyBeginColor.g, MyEndColor.g),
                    (byte)Random.Range(MyBeginColor.b, MyEndColor.b),
                    255);
            MyColors.Add(NewColor);
            for (int i = 1; i < Count; i++)
            {
                bool HasFoundNewColor = false;
                while (!HasFoundNewColor)
                {
                    int ThisVariationR = Random.Range(-VariationR, VariationR);
                    //int ThisVariationR = Random.Range(-VariationB, VariationR);
                    //int ThisVariationR = Random.Range(-VariationB, VariationR);
                    Color32 LastColor = new Color32(
                            (byte)Random.Range(MyBeginColor.r, MyEndColor.r),
                            (byte)Random.Range(MyBeginColor.g, MyEndColor.g),
                            (byte)Random.Range(MyBeginColor.b, MyEndColor.b),
                            255);// MyColors[MyColors.Count - 1];
                    Color32 PossibleColor = new Color32(
                        (byte)(LastColor.r + ThisVariationR),
                        (byte)(LastColor.g + ThisVariationR),
                        (byte)(LastColor.b + ThisVariationR),
                        255);
                    /*Color32 PossibleColor = new Color32(
                        (byte)Random.Range(MyBeginColor.r, MyEndColor.r),
                        (byte)Random.Range(MyBeginColor.g, MyEndColor.g),
                        (byte)Random.Range(MyBeginColor.b, MyEndColor.b),
                        255);*/
                    bool IsColorAdded = false;
                    for (int j = 0; j < MyColors.Count; j++)
                    {
                        // If Color is already added keep searching for a new one to add
                        if (PossibleColor.r == MyColors[j].r &&
                            PossibleColor.g == MyColors[j].g &&
                            PossibleColor.b == MyColors[j].b)
                        {
                            IsColorAdded = true;
                            break;
                        }
                    }
                    if (!IsColorAdded)
                    {
                        NewColor = PossibleColor;
                        HasFoundNewColor = true;
                    }   // else keep searching
                }
                MyColors.Add(NewColor);
            }
            return MyColors;
        }
        public List<Vector2> GetRandomPoints(Texture2D MyTexture, int Count)
        {
            List<Vector2> MyPositions = new List<Vector2>();
            for (int i = 0; i < Count; i++)
            {
                Vector2 NewPosition = new Vector2(Mathf.RoundToInt(Random.Range(0, MyTexture.width - 1)),
                                                Mathf.RoundToInt(Random.Range(0, MyTexture.height - 1)));
                bool HasFoundNewPosition = false;
                while (!HasFoundNewPosition)
                {
                    Vector2 PossiblePosition = new Vector2(Mathf.RoundToInt(Random.Range(0, MyTexture.width - 1)),
                                                  Mathf.RoundToInt(Random.Range(0, MyTexture.height - 1)));
                    bool HasFoundNewPosition2 = false;
                    for (int j = 0; j < MyPositions.Count; j++)
                    {
                        // If Color is already added keep searching for a new one to add
                        if (MyPositions[j] == PossiblePosition)
                        {
                            HasFoundNewPosition2 = true;
                            break;
                        }
                    }
                    if (!HasFoundNewPosition2)
                    {
                        NewPosition = PossiblePosition;
                        HasFoundNewPosition = true;
                    }   // else keep searching
                }
                MyPositions.Add(NewPosition);
            }
            return MyPositions;
        }
    }
}