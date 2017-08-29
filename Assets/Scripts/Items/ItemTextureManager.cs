using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Zeltex.Util;

/// <summary>
/// All items can exist either in a voxel, in an inventory, in a world item.
/// </summary>
/*namespace MakerGuiSystem
{
    /// <summary>
    /// Manages textures of items.
    /// </summary>
    public class ItemTextureManager : MonoBehaviour
    {
        public static string FolderName = "ItemTextures/";
        public static string FileExtension = "png";
        public List<Texture2D> MyTextures = new List<Texture2D>();

        public void Clear()
        {
            MyTextures.Clear();
        }
        public static ItemTextureManager Get()
        {
            return GameObject.Find("Item Manager").GetComponent<ItemTextureManager>();
        }
        public void Load()
        {
            StopCoroutine(LoadCoroutine());
            StartCoroutine(LoadCoroutine());
        }
        public IEnumerator LoadCoroutine()
        {
            MyTextures.Clear();
            string MyDirectory = FileUtil.GetFolderPath(FolderName);
            List<string> MyFiles = FileUtil.GetFilesOfType(MyDirectory, FileExtension);
            //Debug.LogError("Loading " + MyFiles.Count + " item textures.");
            for (int i = 0; i < MyFiles.Count; i++)
            {
                //Debug.LogError("Loading " + i + ":" + MyFiles[i]);
                if (File.Exists(MyFiles[i]))
                {
                    WWW MyTextureLoader = new WWW("file://" + MyFiles[i]);
                    yield return MyTextureLoader;
                    if (MyTextureLoader.texture)
                    {
                        Texture2D MyTexture = MyTextureLoader.texture as Texture2D;
                        MyTexture.filterMode = FilterMode.Point;
                        MyTexture.name = Path.GetFileName(MyFiles[i]);
                        MyTexture.name = MyTexture.name.Replace(".png", "");
                        //Debug.LogError("Adding Item Texture: " + MyTexture.name);
                        MyTextures.Add(MyTexture);
                    }
                }
            }
            //MyManager.MyTextures = MyTextures;
        }

        public void Save()
        {
            //MyTextures = MyManager.MyTextures;
            for (int i = 0; i < MyTextures.Count; i++)
            {
                string MyDirectory = FileUtil.GetFolderPath(FolderName);
                string TextureSavePath = MyDirectory + MyTextures[i].name;
                Debug.Log("Saving texture to path: " + TextureSavePath);
                TextureMaker.SaveTexture(TextureSavePath, MyTextures[i] as Texture2D);
            }
        }

        public void LoadAll()
        {
            Load();
        }
        public void SaveAll()
        {
            Save();
        }
        public Texture2D GetTexture(string TextureName)
        {
            for (int i = 0; i < MyTextures.Count; i++)
            {
                if (MyTextures[i].name == TextureName)
                {
                    return MyTextures[i];
                }
            }
            return null;
        }
    }
}*/
