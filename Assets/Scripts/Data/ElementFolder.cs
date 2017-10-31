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
            NewElement.Name = "Element" + Random.Range(1, 10000);
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
            //if (ElementFolderPath.Contains("://") || ElementFolderPath.Contains(":///"))
            //{
            FileNames = new List<string>();
            if (ElementFolderPath != null)
            {
                // string FullFolderPath = DataManager.Get().MapName + "/" + FolderName;
                if (FileManagement.DirectoryExists(ElementFolderPath, true, true))
                {
                    string[] FoundFiles = FileManagement.ListFiles(ElementFolderPath, new string[] { "." + FileExtension }, true, true);//, true, true); // + "/"
                    if (FoundFiles != null)
                    {
                        for (int i = 0; i < FoundFiles.Length; i++)
                        {
                            string FileName = System.IO.Path.GetFileNameWithoutExtension(FoundFiles[i]) + "." + FileExtension;
                            FileNames.Add(ElementFolderPath + FileName);
                        }
                    }
                    else
                    {
                        LogManager.Get().Log(ElementFolderPath + " has no files found.");
                    }
                }
                else
                {
                    Debug.LogError("Directory Path does not exist: " + ElementFolderPath);
                }
            }
            else
            {
                LogManager.Get().Log("Folder is null");
            }
            LogManager.Get().Log("[" + FolderName + "] Found: " + FileNames.Count + "\nFolderPath: " + ElementFolderPath + " --- " + FileExtension, "DataManager");
            FileNames = FileUtil.SortAlphabetically(FileNames);
            //List<string> MyNames = new List<string>();
            //for (int i = 0; i < FileNames.Count; i++)
            {
                //MyNames.Add(Path.GetFileNameWithoutExtension(MyFiles[i]));
                //Data.Add(Path.GetFileNameWithoutExtension(FileNames[i]), default(T));
            }
            // RetrieveElements();
            // Load folder files
            //List<string> FileNames = new List<string>();
            List<string> Scripts = new List<string>();
            for (int i = 0; i < FileNames.Count; i++)
            {
                LogManager.Get().Log("Loading Element file in : " + FolderName + " of - " + FileNames[i], "DataManagerFiles");
                if (FileNames[i].Contains("://") || FileNames[i].Contains(":///"))
                {
                    WWW UrlRequest = new WWW(FileNames[i]);
                    yield return (UrlRequest);  // UniversalCoroutine.CoroutineManager.StartCoroutine
                    Scripts.Add(UrlRequest.text);
                }
                else
                {
                    Scripts.Add(File.ReadAllText(FileNames[i]));
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
            //List<string> MyScripts = LoadAllStrings();
            //Debug.LogError("Retrieved " + MyScripts.Count + " element files.");
            List<Element> MyElements = new List<Element>();
            for (int i = 0; i < Scripts.Count; i++)
            {
                Element NewElement = Element.Load(Names[i], this as ElementFolder, Scripts[i]);
                if (FolderName == DataFolderNames.PolygonModels)
                {
                    if (VoxelManager.Get())
                    {
                        VoxelManager.Get().AddModelRaw(NewElement as VoxelModel);
                    }
                }
                else if (FolderName == DataFolderNames.VoxelMeta)
                {
                    if (VoxelManager.Get())
                    {
                        VoxelManager.Get().AddMetaRaw(NewElement as VoxelMeta);
                    }
                }
                //Debug.LogError("Loading: " + NewElement + ":" + MyScripts[i]);
                MyElements.Add(NewElement);
            }
            return MyElements;
        }
    }

}