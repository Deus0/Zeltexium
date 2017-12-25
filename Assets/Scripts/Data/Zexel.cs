using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Zeltex
{
    /// <summary>
    /// Textures implementation for zeltex
    /// </summary>
    [System.Serializable]
    public class Zexel : Element
    {
        [SerializeField, JsonProperty]
        private byte[] Pixels = null;
        [SerializeField, JsonProperty]
        private Vector2 Size = new Vector2(32, 32);

        [SerializeField, JsonIgnore]
        private Texture2D MyTexture;

        public Texture2D GetTexture()
        {
            if (MyTexture == null)
            {
                GenerateTextureFromBytes();
            }
            return MyTexture;
        }

        public int GetWidth()
        {
            return (int) Size.x;
        }
        public int GetHeight()
        {
            return (int) Size.y;
        }

        public void SetTexture(Texture2D NewTexture)
        {
            if (NewTexture != null)
            {
                Size.Set(NewTexture.width, NewTexture.height);
                Pixels = NewTexture.GetRawTextureData();
                GenerateTextureFromBytes();
            }
        }

        public bool HasPixels()
        {
            return Pixels != null;
        }

        /// <summary>
        /// This should only be used for debug
        /// </summary>
        public void GenerateTextureFromBytes()
        {
            if (HasPixels())
            {
                MyTexture = new Texture2D(
                (int)Size.x,
                (int)Size.y,
                TextureFormat.ARGB32,
                false);
                MyTexture.filterMode = FilterMode.Point;
                MyTexture.wrapMode = TextureWrapMode.Clamp;
                MyTexture.name = Name;//"T " + Mathf.RoundToInt(Random.Range(1, 10000));
                //Debug.LogError("Raw Bytes: " + Pixels.Length + " with size " + Size.ToString());
                MyTexture.LoadRawTextureData(Pixels);
                MyTexture.Apply();
                //DataManager.Get().TestTexture2 = MyTexture;
            }
        }

        public void LoadImage(byte[] Bytes)
        {
            if (Bytes != null)
            {
                Debug.Log("Loading image as Zexel: " + Bytes.Length);
                if (MyTexture == null)
                {
                    MyTexture = new Texture2D(
                        (int)Size.x,
                        (int)Size.y,
                        TextureFormat.ARGB32,
                        false);
                }
                MyTexture.filterMode = FilterMode.Point;
                MyTexture.wrapMode = TextureWrapMode.Clamp;
                //MyTexture.name = "T " + Mathf.RoundToInt(Random.Range(1, 10000));
               // Debug.LogError("Image Bytes: " + Bytes.Length + " with size " + Size.ToString());
                MyTexture.LoadImage(Bytes, false);
                MyTexture.Apply();
                //DataManager.Get().TestTexture = MyTexture;

                Size.x = MyTexture.width;
                Size.y = MyTexture.height;
                Pixels = MyTexture.GetRawTextureData();
                GenerateTextureFromBytes();
                OnModified();
            }
        }
    }

}