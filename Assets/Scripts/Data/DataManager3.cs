using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zeltex.Generators;
using Zeltex.Util;
using Zeltex.Combat;

namespace Zeltex
{

    /// <summary>
    /// Generic class to load data from a file
    /// Initializes in editor too
    /// </summary>
    public partial class DataManager : ManagerBase<DataManager>
    {

        #region Generic

        /// <summary>
        /// returns the name of an index
        /// </summary>
        public string GetName(string FolderName, int Index)
        {
            DataFolder<string> MyFolder = Get(FolderName);
            if (MyFolder != null)
            {
                //Debug.LogError("Getting name: " + Index + " Inside " + FolderName);
                return MyFolder.GetName(Index);
            }
            /*DataFolder<Texture2D> TextureFolder = GetTextureFolder(FolderName);
            if (TextureFolder != null)
            {
                return TextureFolder.GetName(Index);
            }
            DataFolder<AudioClip> AudioFolder = GetAudioFolder(FolderName);
            if (AudioFolder != null)
            {
                return AudioFolder.GetName(Index);
            }*/
            ElementFolder ElementFolder = GetElementFolder(FolderName);
            if (ElementFolder != null)
            {
                return ElementFolder.GetName(Index);
            }
            return "";
        }

        /// <summary>
        /// returns the size of a folder
        /// </summary>
        public int GetSize(string FolderName)
        {
            DataFolder<string> MyFolder = Get(FolderName);
            if (MyFolder != null)
            {
                return MyFolder.Data.Count;
            }
            ElementFolder ElementFolder = GetElementFolder(FolderName);
            if (ElementFolder != null)
            {
                return ElementFolder.Data.Count;
            }
            /*DataFolder<Texture2D> TextureFolder = GetTextureFolder(FolderName);
            if (TextureFolder != null)
            {
                return TextureFolder.Data.Count;
            }
            DataFolder<AudioClip> SoundFolder = GetAudioFolder(FolderName);
            if (SoundFolder != null)
            {
                return SoundFolder.Data.Count;
            }*/
            return 0;
        }
        #endregion

        #region Elements

        public List<ElementFolder> GetElementFolders()
        {
            return ElementFolders;
        }
        /// <summary>
        /// returns true if the folder has been modified
        /// </summary>
        public bool IsFolderModified(string FolderName)
        {
            ElementFolder ElementFolder = GetElementFolder(FolderName);
            if (ElementFolder != null)
            {
                bool IsModified = false;
                List<Element> MyElements = ElementFolder.GetData();
                for (int i = 0; i < MyElements.Count; i++)
                {
                    if (MyElements[i].CanSave())
                    {
                        IsModified = true;
                        break;
                    }
                }
                return IsModified;
            }

            DataFolder<string> MyFolder = Get(FolderName);
            if (MyFolder != null)
            {
                return false;
            }
            /*
            DataFolder<Texture2D> TextureFolder = GetTextureFolder(FolderName);
            if (TextureFolder != null)
            {
                return false;
            }
             * DataFolder<AudioClip> SoundFolder = GetAudioFolder(FolderName);
            if (SoundFolder != null)
            {
                return false;
            }*/
            return false;
        }

        /// <summary>
        /// Save all the elements in a folder, the ui implementation
        /// </summary>
        public void SaveElements(string FolderName)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null)
            {
                List<Element> MyData = MyFolder.GetData();
                //Debug.LogError(FolderName + " is saving: " + MyData.Count);
                for (int i = 0; i < MyData.Count; i++)
                {
                    if (MyData[i].CanSave())
                    {
                        string Script = "";
                        if (IsJSONFormat)
                        {
                            Script = Newtonsoft.Json.JsonConvert.SerializeObject(MyData[i]);//MyData[i].GetScript();
                        }
                        else
                        {
                            Script = MyData[i].GetScript();
                        }
                        MyFolder.SaveFile(i, Script);
                        MyData[i].OnSaved();
                    }
                }
            }
            else
            {
                Debug.LogError(FolderName + " was not found.");
            }
        }

        /// <summary>
        /// Get the quest folder
        /// </summary>
        private ElementFolder GetElementFolder(string FolderName)
        {
            for (int i = 0; i < ElementFolders.Count; i++)
            {
                if (ElementFolders[i].FolderName == FolderName)
                {
                    return ElementFolders[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Loads all the data!
        /// </summary>
        private System.Collections.IEnumerator LoadAllElements()
        {
            Debug.Log("Loading all elements for [" + MapName + "]");
            for (int i = 0; i < ElementFolders.Count; i++)
            {
                yield return UniversalCoroutine.CoroutineManager.StartCoroutine(ElementFolders[i].LoadAllElements());
            }
            OnUpdatedResources.Invoke();
        }

        public Element RevertElement(Element MyElement)
        {
            ElementFolder MyFolder = GetElementFolder(MyElement.GetFolder());
            if (MyFolder != null)
            {
                MyElement = MyElement.Revert();
                MyFolder.Set(MyElement.Name, MyElement);
            }
            return MyElement;
        }

        public void LoadElement(Element MyElement)
        {
            ElementFolder MyFolder = GetElementFolder(MyElement.GetFolder());
            if (MyFolder != null)
            {
                MyFolder.LoadElement(MyElement);
            }
        }

        /// <summary>
        /// Save all the elements!
        /// </summary>
        private void SaveAllElements()
        {
            for (int i = 0; i < ElementFolders.Count; i++)
            {
                ElementFolders[i].SaveAllElements();
            }
        }

        public void SaveElement(string ElementFolderName, int ElementIndex)
        {
            ElementFolder MyFolder = GetElementFolder(ElementFolderName);
            if (MyFolder != null)
            {
                Element MyElement = MyFolder.Get(ElementIndex);
                SaveElement(MyElement);
            }
        }
        public void SaveElement(string ElementFolderName, string ElementName)
        {
            ElementFolder MyFolder = GetElementFolder(ElementFolderName);
            if (MyFolder != null)
            {
                Element MyElement = MyFolder.Get(ElementName);
                SaveElement(MyElement);
            }
        }

        public void SaveElement(Element MyElement)
        {
            ElementFolder MyFolder = GetElementFolder(MyElement.GetFolder());
            if (MyFolder != null
                && MyElement.CanSave())
            {
                MyFolder.SaveFile(MyElement.Name, MyElement.GetSerial());
                MyElement.OnSaved();
            }
        }

        /// <summary>
        /// Get an item  from a folder, using an index
        /// </summary>
        public Element GetElement(string FolderName, int Index)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null)
            {
                return MyFolder.Get(Index);
            }
            return null;
        }

        /// <summary>
        /// Get an item  from a folder, using an index
        /// </summary>
        public Element GetElement(string FolderName, string FileName)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null && MyFolder.Data.ContainsKey(FileName))
            {
                return MyFolder.Get(FileName);
            }
            else
            {
                Debug.LogError("Could not find folder with name: " + FolderName);
            }
            return null;
        }

        /// <summary>
        /// returns the element if its in the folder
        /// </summary>
        public int GetElementIndex(string FolderName, Element MyElement)
        {
            int MyIndex = -1;
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null)
            {
                List<Element> MyData = MyFolder.GetData();
                for (int i = 0; i < MyData.Count; i++)
                {
                    if (MyData[i] == MyElement)
                    {
                        MyIndex = i;
                        break;
                    }
                }
            }
            return MyIndex;
        }

        public void SetElement(string FolderName, Element NewElement)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null && NewElement != null)
            {
                if (MyFolder.SetElement(NewElement.Name, NewElement))
                {
                    NewElement.MyFolder = MyFolder;
                    OnUpdatedResources.Invoke();
                }
            }
        }

        /// <summary>
        /// Add a texture
        /// </summary>
        public void AddElement(string FolderName, Element NewElement)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null && NewElement != null)
            {
                if (MyFolder.Add(NewElement.Name, NewElement))
                {
                    NewElement.MyFolder = MyFolder;
                    OnUpdatedResources.Invoke();
                }
            }
        }

        /// <summary>
        /// Removes a particular data
        /// </summary>
        public void RemoveElement(string FolderName, int FileIndex)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null)
            {
                MyFolder.Remove(FileIndex);
                if (FolderName == DataFolderNames.VoxelMeta)
                {
                    Voxels.VoxelManager.Get().RemoveMeta(FileIndex);
                }
                if (FolderName == DataFolderNames.PolygonModels)
                {
                    Voxels.VoxelManager.Get().RemoveModel(FileIndex);
                }
                OnUpdatedResources.Invoke();
            }
        }

        /// <summary>
        /// Removes a particular data
        /// </summary>
        public void RemoveElement(string FolderName, string FileName)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null)
            {
                int IndexOf = MyFolder.IndexOf(FileName);
                if (IndexOf != -1)
                {
                    MyFolder.Remove(IndexOf);
                    if (FolderName == DataFolderNames.VoxelMeta)
                    {
                        Voxels.VoxelManager.Get().RemoveMeta(IndexOf);
                    }
                    if (FolderName == DataFolderNames.PolygonModels)
                    {
                        Voxels.VoxelManager.Get().RemoveModel(IndexOf);
                    }
                    OnUpdatedResources.Invoke();
                }
            }
        }

        /// <summary>
        /// returns the size of a folder
        /// </summary>
        public int GetSizeElements(string FolderName)
        {
            ElementFolder MyFolder = GetElementFolder(FolderName);
            if (MyFolder != null)
            {
                return MyFolder.Data.Count;
            }
            else
            {
                return 0;
            }
        }
        #endregion

        #region Strings

        /// <summary>
        /// Save all the elements in a folder, the ui implementation
        /// </summary>
        public void Save(string FolderName)
        {
            DataFolder<string> MyFolder = Get(FolderName);
            if (MyFolder != null)
            {
                List<string> MyData = MyFolder.GetData();
                for (int i = 0; i < MyData.Count; i++)
                {
                    MyFolder.SaveFile(i, MyData[i]);
                }
            }
        }

        private void LoadAllStrings()
        {
            for (int i = 0; i < StringFolders.Count; i++)
            {
                List<string> MyData = StringFolders[i].LoadAllStrings();
                for (int j = 0; j < MyData.Count; j++)
                {
                    StringFolders[i].Set(j, MyData[j]);
                }
            }
            OnUpdatedResources.Invoke();
        }

        private void SaveAllStrings()
        {
            for (int i = 0; i < StringFolders.Count; i++)
            {
                SaveStrings(StringFolders[i]);
            }
        }

        private void SaveStrings(DataFolder<string> MyFolder)
        {
            if (MyFolder != null)
            {
                List<string> MyData = new List<string>();
                MyData = MyFolder.GetData();

                for (int j = 0; j < MyData.Count; j++)
                {
                    MyFolder.SaveFile(j, MyData[j]);
                }
            }
        }

        /// <summary>
        /// Sets a file with a name
        /// </summary>
        public void Set(string FolderName, string FileName, string Data)
        {
            DataFolder<string> MyFolder = Get(FolderName);
            if (MyFolder != null)
            {
                MyFolder.Set(FileName, Data);
            }
        }

        /// <summary>
        /// Sets a file with an index
        /// </summary>
        public void Set(string FolderName, int FileIndex, string Data)
        {
            DataFolder<string> MyFolder = Get(FolderName);
            if (MyFolder != null)
            {
                MyFolder.Set(FileIndex, Data);
            }
        }

        /// <summary>
        /// Adds the data
        /// </summary>
        public void Add(string FolderName, string NewName, string NewData)
        {
            for (int i = 0; i < StringFolders.Count; i++)
            {
                if (StringFolders[i].FolderName == FolderName)
                {
                    StringFolders[i].Add(NewName, NewData);
                    OnUpdatedResources.Invoke();
                }
            }
        }
        private DataFolder<string> Get(string FolderName)
        {
            for (int i = 0; i < StringFolders.Count; i++)
            {
                if (StringFolders[i].FolderName == FolderName)
                {
                    return StringFolders[i];
                }
            }
            //Debug.LogError("Could not find: " + FolderName);
            return null;
        }

        /// <summary>
        /// returns the data
        /// </summary>
        public string Get(string FolderName, int FileIndex)
        {
            DataFolder<string> MyFolder = Get(FolderName);
            if (MyFolder != null)
            {
                return MyFolder.Get(FileIndex);
            }
            return "";
        }

        /// <summary>
        /// returns the data
        /// </summary>
        public string Get(string FolderName, string FileName)
        {
            DataFolder<string> MyFolder = Get(FolderName);
            if (MyFolder != null)
            {
                return MyFolder.Get(FileName);
            }
            return "";
        }

        /// <summary>
        /// Removes a particular data
        /// </summary>
        public void Remove(string FolderName, int FileIndex)
        {
            DataFolder<string> MyFolder = Get(FolderName);
            if (MyFolder != null)
            {
                MyFolder.Remove(FileIndex);
                OnUpdatedResources.Invoke();
            }
        }
        #endregion

        #region Textures
        /*
        /// <summary>
        /// Save all the elements in a folder, the ui implementation
        /// </summary>
        public void SaveTextures(string FolderName)
        {
            DataFolder<Texture2D> MyFolder = GetTextureFolder(FolderName);
            if (MyFolder != null)
            {
                List<Texture2D> MyData = GetTextures(MyFolder);
                for (int i = 0; i < MyData.Count; i++)
                {
                    MyFolder.SaveFileTexture(i, MyData[i]);
                }
            }
        }

        private List<Texture2D> GetTextures(DataFolder<Texture2D> MyFolder)
        {
            List<Texture2D> MyData = new List<Texture2D>();
            if (MyFolder != null)
            {
                if (MyFolder.FolderName == "VoxelTexturesDiffuse")
                {
                    Zeltex.Voxels.VoxelManager MyManager = Zeltex.Voxels.VoxelManager.Get();
                    MyData.AddRange(MyManager.DiffuseTextures);
                }
                else if (MyFolder.FolderName == "VoxelTexturesNormals")
                {
                    Zeltex.Voxels.VoxelManager MyManager = Zeltex.Voxels.VoxelManager.Get();
                    MyData.AddRange(MyManager.NormalTextures);
                }
                else
                {
                    MyData = MyFolder.GetData();
                }
            }
            return MyData;
        }
        private void SaveAllTextures()
        {
            for (int i = 0; i < TextureFolders.Count; i++)
            {
                List<Texture2D> MyData = GetTextures(TextureFolders[i]);
                for (int j = 0; j < MyData.Count; j++)
                {
                    TextureFolders[i].SaveFileTexture(j, MyData[j]);
                }
            }
        }

        private void LoadAllTextures()
        {

            for (int i = 0; i < TextureFolders.Count; i++)
            {
                List<Texture2D> MyData = TextureFolders[i].LoadAllTextures();
                for (int j = 0; j < MyData.Count; j++)
                {
                    TextureFolders[i].Set(j, MyData[j]);
                }
            }
            OnUpdatedResources.Invoke();
        }
        private DataFolder<Texture2D> GetTextureFolder(string FolderName)
        {
            for (int i = 0; i < TextureFolders.Count; i++)
            {
                if (TextureFolders[i].FolderName == FolderName)
                {
                    return TextureFolders[i];
                }
            }
            return null;
        }

        public Texture2D GetTexture(string FolderName, string FileName)
        {
            DataFolder<Texture2D> MyFolder = GetTextureFolder(FolderName);
            if (MyFolder != null)
            {
                if (MyFolder.FolderName == "VoxelTexturesDiffuse")
                {
                    Zeltex.Voxels.VoxelManager.Get().GetTextureDiffuse(FileName);
                }
                return MyFolder.Get(FileName);
            }
            return null;
        }

        public Texture2D GetTexture(string FolderName, int FileIndex)
        {
            DataFolder<Texture2D> MyFolder = GetTextureFolder(FolderName);
            if (MyFolder != null)
            {
                if (MyFolder.FolderName == "VoxelTexturesDiffuse")
                {
                    Zeltex.Voxels.VoxelManager.Get().GetTextureDiffuse(FileIndex);
                }
                return MyFolder.Get(FileIndex);
            }
            return null;
        }

        public void AddTexture(string FolderName, Texture2D NewTexture)
        {
            DataFolder<Texture2D> MyFolder = GetTextureFolder(FolderName);
            if (MyFolder != null && NewTexture != null)
            {
                MyFolder.Add(NewTexture.name, NewTexture);
                // also add textures to other places for this case
                if (MyFolder.FolderName == DataFolderNames.VoxelDiffuseTextures)
                {
                    Voxels.VoxelManager.Get().AddTexture(NewTexture);
                }
                OnUpdatedResources.Invoke();
            }
        }

        /// <summary>
        /// Removes a particular data
        /// </summary>
        public void RemoveTexture(string FolderName, int FileIndex)
        {
            DataFolder<Texture2D> MyFolder = GetTextureFolder(FolderName);
            if (MyFolder != null)
            {
                MyFolder.Remove(FileIndex);
                OnUpdatedResources.Invoke();
            }
        }

        /// <summary>
        /// returns the size of a folder
        /// </summary>
        public int GetSizeTextures(string FolderName)
        {
            DataFolder<Texture2D> MyFolder = GetTextureFolder(FolderName);
            if (MyFolder != null)
            {
                return MyFolder.Data.Count;
            }
            else
            {
                Debug.LogError("Could not fold texture folder: " + FolderName);
                return 0;
            }
        }
        #endregion

        #region Sounds

        /// <summary>
        /// Save all the elements in a folder, the ui implementation
        /// </summary>
        public void SaveSounds(string FolderName)
        {
            DataFolder<AudioClip> MyFolder = GetAudioFolder(FolderName);
            if (MyFolder != null)
            {
                List<AudioClip> MyData = MyFolder.GetData();
                for (int i = 0; i < MyData.Count; i++)
                {
                    MyFolder.SaveFileSound(i, MyData[i]);
                }
            }
        }

        private void SaveAllAudio()
        {
            for (int i = 0; i < AudioFolders.Count; i++)
            {
                List<AudioClip> MyData = AudioFolders[i].GetData();

                for (int j = 0; j < MyData.Count; j++)
                {
                    AudioFolders[i].SaveFileSound(j, MyData[j]);
                }
            }
        }

        private void LoadAllAudio()
        {
            for (int i = 0; i < AudioFolders.Count; i++)
            {
                List<AudioClip> MyData = AudioFolders[i].LoadAllAudio();
                for (int j = 0; j < MyData.Count; j++)
                {
                    AudioFolders[i].Set(j, MyData[j]);
                }
            }
            OnUpdatedResources.Invoke();
        }

        private DataFolder<AudioClip> GetAudioFolder(string FolderName)
        {
            for (int i = 0; i < AudioFolders.Count; i++)
            {
                if (AudioFolders[i].FolderName == FolderName)
                {
                    return AudioFolders[i];
                }
            }
            return null;
        }

        public void AddSound(string FolderName, AudioClip NewSound)
        {
            DataFolder<AudioClip> MyFolder = GetAudioFolder(FolderName);
            if (MyFolder != null && NewSound != null)
            {
                MyFolder.Add(NewSound.name, NewSound);
                OnUpdatedResources.Invoke();
            }
        }

        public AudioClip GetSound(string FolderName, int FileIndex)
        {
            DataFolder<AudioClip> MyFolder = GetAudioFolder(FolderName);
            if (MyFolder != null)
            {
                return MyFolder.Get(FileIndex);
            }
            return null;
        }
        public AudioClip GetSound(string FolderName, string FileName)
        {
            DataFolder<AudioClip> MyFolder = GetAudioFolder(FolderName);
            if (MyFolder != null)
            {
                return MyFolder.Get(FileName);
            }
            return null;
        }

        public void SetSound(string FolderName, int FileIndex, AudioClip NewSound)
        {
            DataFolder<AudioClip> MyFolder = GetAudioFolder(FolderName);
            if (MyFolder != null)
            {
                MyFolder.Set(FileIndex, NewSound);
            }
        }

        /// <summary>
        /// Removes a particular data
        /// </summary>
        public void RemoveSound(string FolderName, int FileIndex)
        {
            DataFolder<AudioClip> MyFolder = GetAudioFolder(FolderName);
            if (MyFolder != null)
            {
                MyFolder.Remove(FileIndex);
            }
        }

        /// <summary>
        /// returns the size of a folder
        /// </summary>
        public int GetSizeAudio(string FolderName)
        {
            DataFolder<AudioClip> MyFolder = GetAudioFolder(FolderName);
            if (MyFolder != null)
            {
                return MyFolder.Data.Count;
            }
            else
            {
                return 0;
            }
        }*/
        #endregion
    }
}