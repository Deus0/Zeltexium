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
            Element NewElement = new Element();
            NewElement.MyFolder = this as ElementFolder;
            Debug.LogError("Adding new type: " + NewElement.GetDataType().ToString());
#if NET_4_6
            System.Reflection.ConstructorInfo MyConstructor = NewElement.GetDataType().GetConstructor(System.Type.EmptyTypes);
            dynamic NewElement2 = MyConstructor.Invoke(null);
            NewElement = NewElement2 as Element;
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

        public void LoadAllElements()
        {
            List<Element> MyData = RetrieveElements();
            for (int j = 0; j < MyData.Count; j++)
            {
                (this as ElementFolder).Set(j, MyData[j]);
                MyData[j].MyFolder = (this as ElementFolder);
            }
        }

        public List<Element> RetrieveElements()
        {
            List<string> MyScripts = LoadAllStrings();
            //Debug.LogError("Retrieved " + MyScripts.Count + " element files.");
            List<Element> MyElements = new List<Element>();
            for (int i = 0; i < MyScripts.Count; i++)
            {
                Element NewElement = Element.Load(GetName(i), this as ElementFolder, MyScripts[i]);
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