using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex
{
    [System.Serializable]
    public class AudioFolder : DataFolder<AudioClip>
    {
        public AudioDictionary ElementData = new AudioDictionary();
        public override DataDictionary<AudioClip> Data
        {
            get { return ElementData as DataDictionary<AudioClip>; }
            set { ElementData = value as AudioDictionary; }
        }

        public new static AudioFolder Create(string NewFolderName, string NewFileExtension)
        {
            AudioFolder NewFolder = new AudioFolder();
            NewFolder.Set(NewFolderName, NewFileExtension);
            return NewFolder;
        }

        [System.Serializable()]
        public class AudioDictionary : DataDictionary<AudioClip>
        {

        }
    }

    [System.Serializable]
    public class TextureFolder : DataFolder<Texture2D>
    {
        public TextureDictionary ElementData = new TextureDictionary();
        public override DataDictionary<Texture2D> Data
        {
            get { return ElementData as DataDictionary<Texture2D>; }
            set { ElementData = value as TextureDictionary; }
        }

        public new static TextureFolder Create(string NewFolderName, string NewFileExtension)
        {
            TextureFolder NewFolder = new TextureFolder();
            NewFolder.Set(NewFolderName, NewFileExtension);
            return NewFolder;
        }

        [System.Serializable()]
        public class TextureDictionary : DataDictionary<Texture2D>
        {

        }
    }

    [System.Serializable]
    public class StringFolder : DataFolder<string>
    {
        public StringDictionary ElementData = new StringDictionary();
        public override DataDictionary<string> Data
        {
            get { return ElementData as DataDictionary<string>; }
            set { ElementData = value as StringDictionary; }
        }

        public new static StringFolder Create(string NewFolderName, string NewFileExtension)
        {
            StringFolder NewFolder = new StringFolder();
            NewFolder.Set(NewFolderName, NewFileExtension);
            return NewFolder;
        }

        [System.Serializable()]
        public class StringDictionary : DataDictionary<string>
        {

        }
    }
}