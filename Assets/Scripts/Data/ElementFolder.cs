using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Voxels;
using Zeltex.Util;
using Newtonsoft.Json;

namespace Zeltex
{

    public partial class ElementFolder : object, ISerializationCallbackReceiver
    {

        /*public new static ElementFolder Create(string NewFolderName, string NewFileExtension)
        {
            ElementFolder NewFolder = new ElementFolder();
            NewFolder.Set(NewFolderName, NewFileExtension);
            return NewFolder;
        }*/

        /// <summary>
        /// Data is set when reloading scripts
        /// </summary>
        public void SetData(List<Element> NewData)
        {
            Data.Clear();
            for (int i = 0; i < NewData.Count; i++)
            {
                Data.Add(NewData[i].Name, NewData[i]);
            }
        }
        /// <summary>
        /// Data is set when reloading scripts
        /// </summary>
        public void SetData<T>(List<T> NewData) where T : Element
        {
            Data.Clear();
            for (int i = 0; i < NewData.Count; i++)
            {
                Data.Add(NewData[i].Name, NewData[i]);
            }
        }

        public int IndexOf(string FileName)
        {
            if (Data.ContainsKey(FileName))
            {
                int FileCount = 0;
                foreach (KeyValuePair<string, Element> MyKeyValue in Data)
                {
                    if (MyKeyValue.Key == FileName)
                    {
                        return FileCount;
                    }
                    FileCount++;
                }
            }
            return -1;
        }

        public void Revert()
        {
            Clear();
            LoadAllElements();
        }

        public void ForceSaveAllElements()
        {
            ElementFolder This = this as ElementFolder;
            foreach (KeyValuePair<string, Element> MyKeyValue in This.Data)
            {
                MyKeyValue.Value.OnModified();
                MyKeyValue.Value.Save();
            }
        }

        public void SaveAllElements()
        {
            ElementFolder This = this as ElementFolder;
            foreach (KeyValuePair<string, Element> MyKeyValue in This.Data)
            {
                MyKeyValue.Value.Save();
            }
        }

        public void AddElement(Element NewElement)
        {
            Add(NewElement.Name, NewElement);
            NewElement.MyFolder = this;
            NewElement.ElementLink = FolderName;
            NewElement.ResetName();
            NewElement.OnModified();
        }

        public bool SetElement(Element NewElement)
        {
            bool DidSet = SetElement(NewElement.Name, NewElement);
            if (DidSet)
            {
                NewElement.MyFolder = this;
                NewElement.ElementLink = FolderName;
                NewElement.ResetName();
                NewElement.OnModified();
            }
            return DidSet;
        }

        public void AddNewElement()
        {
            Type type = DataFolderNames.GetDataType(FolderName);// System.Type.GetType("Foo.MyClass");
            object ElementObject = System.Activator.CreateInstance(type);
            Element NewElement = ElementObject as Element;
            NewElement.SetNameOfClone("Element" + UnityEngine.Random.Range(1, 10000));
            NewElement.MyFolder = this as ElementFolder;
            NewElement.MyFolder.Data.Add(NewElement.Name, NewElement);
            NewElement.MyFolder.CachedData.Add(NewElement);
        }

        public void LoadElement(Element MyElement)
        {
            MyElement = MyElement.Load();
        }
        
        public IEnumerator LoadAllElements()
        {
            Clear();
            List<string> FileNames;
            string ElementFolderPath = GetFolderPath();
            //LogManager.Get().Log(ElementFolderPath + " is beginning to load at [" + Time.realtimeSinceStartup + "]", "DataManager");
            //if (ElementFolderPath.Contains("://") || ElementFolderPath.Contains(":///"))
            //{
            FileNames = new List<string>();
            if (ElementFolderPath != null)
            {
                // string FullFolderPath = DataManager.Get().MapName + "/" + FolderName;
                if (FileManagement.DirectoryExists(ElementFolderPath, true, true))
                {
                    //
                    string[] FoundFiles = FileManagement.ListFiles(ElementFolderPath, new string[] { "." + FileExtension }, DataManager.Get().MyFilePathType == FilePathType.StreamingPath, true);//, true, true); // + "/"
                    if (FoundFiles != null)
                    {
                        //Debug.Log(ElementFolderPath + " has found " + FoundFiles.Length + " [" + FileExtension + "] Files.");
                        for (int i = 0; i < FoundFiles.Length; i++)
                        {
                            if (FoundFiles[i] != null)
                            {
                                LogManager.Get().Log("FoundFiles " + i + " " + FoundFiles[i], "DataManagerFiles");
                                try
                                {
                                    FileManagement.GetFileName(FoundFiles[i]);
                                    string FileExtention = FileManagement.GetFileExtension(FoundFiles[i]);
                                    string CulledFileName = FileManagement.GetFileName(FoundFiles[i]);
                                    if (CulledFileName.Contains(FileExtention))
                                    {
                                        FileNames.Add(ElementFolderPath + CulledFileName);
                                    }
                                    else
                                    {
                                        string FileName = CulledFileName + "." + FileExtension;
                                        FileNames.Add(ElementFolderPath + FileName);
                                    }
                                }
                                catch(FormatException e)
                                {
                                    Debug.LogError("[LoadAllElements] has error: " + e.ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        LogManager.Get().Log(ElementFolderPath + " has no files found.");
                    }
                }
                else
                {
                    Debug.LogError("[LoadAllElements] Directory Path does not exist: " + ElementFolderPath);
                }
            }
            else
            {
                LogManager.Get().Log("Folder is null");
            }
            //Debug.Log(LoadAllElementsDebug);
            LogManager.Get().Log("[LoadAllElements] -=- [" + FolderName + "] Found: " + FileNames.Count + "\nFolderPath: " + ElementFolderPath + " --- " + FileExtension, "DataManager");

            /*try
            {
                FileNames = FileUtil.SortAlphabetically(FileNames);
            }
            catch (System.FormatException e)
            {
                Debug.LogError("Error while loading file : SortAlphabetically: " + e.ToString());
            }*/

            List<string> Scripts = new List<string>();
            string LoadLog = "";
            for (int i = 0; i < FileNames.Count; i++)
            {
                LoadLog = "Loading Element file in : " + FolderName + " of - " + FileNames[i];
                LogManager.Get().Log(LoadLog, "DataManagerFiles");
                //Debug.Log(LoadLog);
                if (FileNames[i] != null && (FileNames[i].Contains("://") || FileNames[i].Contains(":///")))
                {
                    WWW UrlRequest = null;
                    try
                    {
                        UrlRequest = new WWW(FileNames[i]);
                    }
                    catch (FormatException e)
                    {
                        Debug.LogError("Error while loading file : UrlRequest: " + FileNames[i] + ": " + e.ToString());
                    }
                    if (UrlRequest != null)
                    {
                        yield return (UrlRequest);  // UniversalCoroutine.CoroutineManager.StartCoroutine
                        Scripts.Add(UrlRequest.text);
                    }
                }
                else
                {
                    FileReaderRoutiner MyFileReader = new FileReaderRoutiner(FileNames[i]);
                    yield return MyFileReader.Run();
                    Scripts.Add(MyFileReader.Result);
                }
            }
            for (int i = 0; i < FileNames.Count; i++)
            {
                FileNames[i] = Path.GetFileNameWithoutExtension(FileNames[i]);
            }
            yield return DeserializeRoutine(FileNames, Scripts);
        }
    }

}