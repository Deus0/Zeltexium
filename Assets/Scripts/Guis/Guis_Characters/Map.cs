using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zeltex.Characters;
using Zeltex.Voxels;

namespace Zeltex.Guis.Characters
{
    /// <summary>
    /// A map for the character
    /// </summary>
    [ExecuteInEditMode]
    public class Map : MonoBehaviour
    {
        private static Vector2 MapPieceSize = new Vector2(16 * 8, 16 * 8);
        [Header("Debug")]
        public bool IsCreateMap;
        public bool IsDestroyMap;
        private List<RawImage> MapPieces = new List<RawImage>();    // all the pieces of the maps
        //private Character MyCharacter;
        private World MyWorld;
        public RectTransform MapParent;
        public Material MapPieceMaterial;
        public Vector2 MapSize = new Vector2(4, 4);
        [Header("Test Generation")]
        public Color PrimaryColor = Color.black;
        public Color SecondaryColor = new Color(100, 15, 5);
        public float NoiseAmplitude = 1f;
        public float NoiseFrequency = 0.1f;
        public float NoiseIterations = 0;
        public float NoiseIterationDivision = 1.4f;
        public RectTransform CharacterMapIcon;
        private Vector3 BlockPosition = Vector3.zero;

        private void Start()
        {
            if (MapParent == null)
            {
                MapParent = GetComponent<RectTransform>();
            }
            DestroyMap();
            CreateMap();
        }

        public void DestroyMap()
        {
            for (int i = 0; i < MapPieces.Count; i++)
            {
                if (MapPieces[i])
                {
                    DestroyImmediate(MapPieces[i].gameObject);
                }
            }
            MapPieces.Clear();
        }

        private void Update()
        {
            if (IsCreateMap)
            {
                IsCreateMap = false;
                DestroyMap();
                CreateMap();
            }
            if (IsDestroyMap)
            {
                IsDestroyMap = false;
                DestroyMap();
            }
            // move position of transform
            if (CharacterMapIcon && MyWorld)
            {
                Vector3 NewBlockPosition = MyWorld.RealToBlockPosition(transform.position).GetVector();
                if (NewBlockPosition != BlockPosition)
                {
                    BlockPosition = NewBlockPosition;
                    CharacterMapIcon.anchoredPosition3D = new Vector3(8 * BlockPosition.x, 8 * BlockPosition.z, 0) + MapParentOffset;
                }
            }
        }

        public Texture GetMapTexture(int PositionX, int PositionZ)
        {
            Texture2D NewMapTexture = new Texture2D(Chunk.ChunkSize, Chunk.ChunkSize);
            NewMapTexture.filterMode = FilterMode.Point;
            if (MyWorld)
            {
                Chunk MyChunk = MyWorld.GetChunk(new Int3(PositionX, 0, PositionZ));
                Generators.TextureGenerator.Get().ChunkHeight(NewMapTexture, MyChunk);
            }
            else
            {
                Generators.TextureGenerator.Get().NoiseOffset = new Vector2(PositionX * Chunk.ChunkSize, PositionZ * Chunk.ChunkSize);
                if (PositionX == 0)
                {
                    Debug.Log("Generating texture for chunk: " + PositionZ + ":" + Generators.TextureGenerator.Get().NoiseOffset.ToString());
                }
                Generators.TextureGenerator.Get().Noise(NewMapTexture);
                if (NoiseIterations != 0)
                {
                    float StartPercent = 1f;
                    for (int i = 0; i < NoiseIterations; i++)
                    {
                        Generators.TextureGenerator.Get().SetColors(Generators.TextureGenerator.DarkenColor(PrimaryColor, StartPercent), SecondaryColor);
                        Generators.TextureGenerator.Get().Noise(NewMapTexture, StartPercent);
                        StartPercent /= NoiseIterationDivision;
                    }
                    Generators.TextureGenerator.Get().SetColors(PrimaryColor, SecondaryColor);
                }
            }
            return NewMapTexture as Texture;
        }

        private Vector3 MapParentOffset = Vector3.zero;
        /// <summary>
        /// Spawns all the images for the map
        /// </summary>
        public void CreateMap()
        {
            Generators.TextureGenerator.Get().NoiseLimits = new Vector2(0, 1);
            Generators.TextureGenerator.Get().NoiseAmplitude = NoiseAmplitude;
            Generators.TextureGenerator.Get().NoiseFrequency = NoiseFrequency;
            Generators.TextureGenerator.Get().NoiseMinimum = 0;
            Generators.TextureGenerator.Get().SetColors(PrimaryColor, SecondaryColor);
            //Generators.TextureGenerator.Get().IsNoiseCutoff = true;
            MapParentOffset = -new Vector3(MapParent.GetWidth() / 2f, MapParent.GetHeight() / 2f, 0);
            Vector3 MapPieceSizeOffset = new Vector3(MapPieceSize.x / 2f, MapPieceSize.y / 2f, 0);
            for (int j = 0; j < MapSize.y; j++)
            {
                for (int i = 0; i < MapSize.x; i++)
                {
                    GameObject MapPiece = new GameObject();
                    MapPiece.name = "MapPiece_" + i + "_" + j;
                    RawImage MapPieceImage = MapPiece.AddComponent<RawImage>();
                    MapPiece.transform.SetParent(MapParent);
                    MapPieceImage.material = MapPieceMaterial;
                    RectTransform MapPieceTransform = MapPiece.GetComponent<RectTransform>();
                    MapPieceTransform.SetSize(MapPieceSize);
                    MapPieceTransform.anchoredPosition3D = new Vector3(MapPieceSize.x * i, MapPieceSize.y * j, 0) + MapParentOffset + MapPieceSizeOffset;
                    MapPieceTransform.localScale = new Vector3(1, 1, 1);
                    MapPieceTransform.localEulerAngles = Vector3.zero;
                    MapPieces.Add(MapPieceImage);
                }
            }
            if (CharacterMapIcon)
            {
                CharacterMapIcon.transform.SetAsLastSibling();  // display after the map pieces
            }
            RefreshMap();
            //Generators.TextureGenerator.Get().IsNoiseCutoff = false;
        }

        //public void SetCharacter(Character NewCharacter)
        //{
            //MyCharacter = NewCharacter;
        //}

        /// <summary>
        /// Scans the characters current surroundings
        /// </summary>
        public void RefreshMap()
        {
            // get world
            if (WorldManager.Get())
            {
                MyWorld = WorldManager.Get().MyWorlds[0];
            }
            Int3 MyPosition = new Int3();
            if (MyWorld)
            {
                VoxelFreeRoam MyRoam = Camera.main.gameObject.GetComponent<VoxelFreeRoam>();
                if (MyRoam && MyRoam.enabled == true)
                {
                    MyPosition = MyWorld.GetChunkPosition(transform.position);
                    //Debug.Log("MyPosition: " + MyPosition.GetVector().ToString());
                }
                // else stay 0, 0
            }
            for (int j = 0; j < MapSize.y; j++)
            {
                for (int i = 0; i < MapSize.x; i++)
                {
                    int MapPiecesIndex = i + j * (int)MapSize.x;
                    MapPieces[MapPiecesIndex].texture = GetMapTexture(MyPosition.x + i, MyPosition.z + j);
                }
            }
        }
    }
}