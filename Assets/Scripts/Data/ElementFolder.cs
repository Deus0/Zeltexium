using System.Collections.Generic;
using UnityEngine;
using Zeltex.Items;
using Zeltex.Combat;
using Zeltex.Util;
using System.IO;
using Zeltex.Voxels;
using System.Linq;
using Zeltex.Quests;

namespace Zeltex
{

    [System.Serializable()]
    public class ElementDictionary : DataDictionary<Element>
    {

    }

    [System.Serializable]
    public class ElementFolder : DataFolder<Element>
    {
		public ElementDictionary ElementData = new ElementDictionary();

        public override DataDictionary<Element> Data
        {
            get { return ElementData as DataDictionary<Element>; }
            set { ElementData = value as ElementDictionary; }
        }

        public new static ElementFolder Create(string NewFolderName, string NewFileExtension)
        {
            ElementFolder NewFolder = new ElementFolder();
            NewFolder.Set(NewFolderName, NewFileExtension);
            return NewFolder;
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
            //Element NewElement = new Element();
            //NewElement.MyFolder = this as ElementFolder;
            //Debug.LogError("Adding new type: " + NewElement.GetDataType().ToString());
#if NET_4_6
            System.Reflection.ConstructorInfo MyConstructor = NewElement.GetDataType().GetConstructor(System.Type.EmptyTypes);
            dynamic NewElement2 = MyConstructor.Invoke(null);
            NewElement = NewElement2 as Element;
            NewElement.Name = "Element" + Random.Range(1, 10000);
            NewElement.MyFolder = this as ElementFolder;
            NewElement.MyFolder.Data.Add(NewElement.Name, NewElement);
#else
            System.Type type = DataFolderNames.GetDataType(FolderName);// System.Type.GetType("Foo.MyClass");
            object ElementObject = System.Activator.CreateInstance(type);
            Element NewElement = ElementObject as Element;
            NewElement.SetNameOfClone("Element" + Random.Range(1, 10000));
            NewElement.MyFolder = this as ElementFolder;
            NewElement.MyFolder.Data.Add(NewElement.Name, NewElement);
#endif
        }

        public void LoadElement(Element MyElement, bool IsJSONFormat = false)
        {
            MyElement = MyElement.Load(IsJSONFormat);
            /*SetDataType();
            string LoadPath = GetFolderPath() + MyElement.Name + "." + FileExtension;
            string Script = FileUtil.Load(LoadPath);
            //MyElement.RunScript(Script);
            if (IsJSONFormat)
            {
                MyElement = Newtonsoft.Json.JsonConvert.DeserializeObject(Script) as Element;
            }
            else
            {
                System.Reflection.ConstructorInfo MyConstructor = DataType.GetConstructor(System.Type.EmptyTypes);
                dynamic NewElement2 = MyConstructor.Invoke(null);
                MyElement = NewElement2 as Element;
                MyElement.RunScript(Script);
            }*/
        }
        
        public System.Collections.IEnumerator LoadAllElements()
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
                                catch(System.FormatException e)
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
                    catch (System.FormatException e)
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
                    try
                    {
                        Scripts.Add(File.ReadAllText(FileNames[i]));
                    }
                    catch (System.FormatException e)
                    {
                        Debug.LogError("Error while loading file : ReadAllText: " + FileNames[i] + ": " + e.ToString());
                    }
                }
            }
            for (int i = 0; i < FileNames.Count; i++)
            {
                FileNames[i] = Path.GetFileNameWithoutExtension(FileNames[i]);
            }
            List<Element> ElementData = RetrieveElements(FileNames, Scripts);
            for (int i = 0; i < ElementData.Count; i++)
            {
                Data.Add(ElementData[i].Name, ElementData[i]);
                ElementData[i].MyFolder = (this as ElementFolder);
            }
        }

        public List<Element> RetrieveElements(List<string> Names, List<string> Scripts)
        {
            List<Element> MyElements = new List<Element>();
            for (int i = 0; i < Scripts.Count; i++)
            {
                try
                {
                    Element NewElement = Element.Load(Names[i], this as ElementFolder, Scripts[i]);
                    if (DataFolderNames.GetDataType(FolderName) == typeof(Level))
                    {
                        // Do the thing!
                        Level MyLevel = NewElement as Level;
                        MyLevel.SetFilePathType(DataManager.Get().MyFilePathType);
                    }
                    if (FolderName == DataFolderNames.PolyModels)
                    {
                        if (VoxelManager.Get())
                        {
                            VoxelManager.Get().AddModelRaw(NewElement as PolyModel);
                        }
                    }
                    else if (FolderName == DataFolderNames.Voxels)
                    {
                        if (VoxelManager.Get())
                        {
                            VoxelManager.Get().AddMetaRaw(NewElement as VoxelMeta);
                        }
                    }
                    //Debug.LogError("Loading: " + NewElement + ":" + MyScripts[i]);
                    MyElements.Add(NewElement);
                }
                catch (System.FormatException e)
                {
                    Debug.LogError("Error while loading file: " + Names[i] + ": " + e.ToString());
                }
            }
            return MyElements;
        }
    }

}