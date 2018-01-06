using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Zeltex.Items;
using System.IO;
using Zeltex.Util;
using ZeltexTools;

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// Handles the gui to make items
    /// To Do:
    ///     - Make drop down set value  
    /// </summary>
    public class ItemMaker : ElementMakerGui
    {
        #region Variables
        [Header("ItemMaker")]
        //public Inventory MyInventory;
        public RawImage TextureImage;
       // public TextureMaker MyTextureMaker;
        //public ItemManager MyItemManager;
        public MeshViewer MyMeshViewer;
        public ModelMaker MyModelMaker;
        public VoxelViewer MyVoxelViewer;
        private static ItemMaker MyItemMaker;
        #endregion

        #region DataManager

        /// <summary>
        /// Sets the file path for the files
        /// </summary>
        protected override void SetFilePaths()
        {
            DataManagerFolder = DataFolderNames.Items;
        }

        /// <summary>
        /// return selected data
        /// </summary>
        public Item GetSelectedItem()
        {
            return DataManager.Get().GetElement(DataManagerFolder, GetSelectedIndex()) as Item;
        }

        /// <summary>
        /// Index of where it is added
        /// </summary>
        protected override void AddData()
        {
            Item NewItem = new Item();
            NewItem.Name = "I" + Random.Range(1, 10000);
            DataManager.Get().AddElement(DataManagerFolder, NewItem);
        }
        /// <summary>
        /// Add a new voxel to the game!
        /// </summary>
        public void AddData(Item NewItem)
        {
            DataManager.Get().AddElement(DataManagerFolder, NewItem);
        }
        #endregion

        #region UI

        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(InputField MyInputField)
        {
            base.UseInput(MyInputField);
            if (GetSelectedItem() != null)
            {
                if (MyInputField.name == "DescriptionInput")
                {
                    GetSelectedItem().SetDescription(MyInputField.text);
                }
                else if (MyInputField.name == "TagsInput")
                {
                    GetSelectedItem().SetTags(MyInputField.text);
                }
                else if (MyInputField.name == "CommandsInput")
                {
                    GetSelectedItem().SetCommands(MyInputField.text);
                }
            }
        }

        public override void UseInput(Toggle MyToggle)
        {
            if (GetSelectedItem() != null)
            {
                if (MyToggle.name == "UniqueTextureToggle")
                {
                    //GetSelectedItem().IsUniqueTexture = MyToggle.isOn;
                }
            }
        }

        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(Dropdown MyDropdown)
        {
            if (MyDropdown.name == "TextureDropdown")
            {
                string TextureName = MyDropdown.options[MyDropdown.value].text;
                if (MyDropdown.value != 0)
                {
                    /*Texture MyTexture = DataManager.Get().GetTexture(DataFolderNames.ItemTextures, MyDropdown.value - 1);//MyTextureMaker.GetItemTexture(TextureName);
                    if (MyTexture != null)
                    {
                        MyTexture.name = DataManager.Get().GetName(DataFolderNames.ItemTextures, MyDropdown.value - 1);
                        GetSelectedItem().SetTexture(MyTexture as Texture2D);
                        TextureImage.texture = GetSelectedItem().GetTexture();
                    }*/
                }
            }
            else if (MyDropdown.name == "MeshDropdown")
            {
                if (MyDropdown.value > 1)
                {
                    // Set new mesh
                    //int ModelIndex = MyDropdown.value - 2;
                    //GetSelected().SetModel(MyModelMaker.MyNames[ModelIndex], MyModelMaker.GetData(ModelIndex));
                    //StartCoroutine(MyVoxelViewer.RunScript(FileUtil.ConvertToList(GetSelectedItem().MyModel)));


                    //Debug.Log("Loading new model: " + MyModelMaker.MyNames[ModelIndex] +":" + GetSelected().MyModel);
                    /*Mesh MyMesh = MyItemManager.MyMeshes[MyDropdown.value - 1];
                    GetSelected().MyMesh = MyMesh;
                    MyMeshViewer.SetMesh(GetSelected().MyMesh);*/
                }
                else
                {
                    if (MyDropdown.value == 0)
                    {
                        // None!
                        MyVoxelViewer.GetSpawn().GetComponent<Zeltex.Voxels.World>().Clear();
                        //GetSelected().MyMesh = new Mesh();
                        //MyMeshViewer.SetMesh(GetSelected().MyMesh);
                    }
                    else
                    {
                        // keep as Unique Mesh
                    }
                }

            }
        }

        public override void FillDropdown(Dropdown MyDropdown)
        {
            if (MyDropdown)
            {
                if (MyDropdown.name == "TextureDropdown")
                {
                    List<string> MyNames = new List<string>();
                    //Debug.Log(MyItemTextureManager.name + " - Filling " + MyDropdown.name + " with " + MyItemTextureManager.MyTextures.Count + " Textures.");
                    MyNames.Add("Unique");
                   /* int ItemTexturesCount = DataManager.Get().GetSizeTextures("ItemTextures");
                    for (int i = 0; i < ItemTexturesCount; i++)
                    {
                        MyNames.Add(DataManager.Get().GetTexture("ItemTextures", i).name);
                    }*/
                    FillDropDownWithList(MyDropdown, MyNames);
                }
                else if (MyDropdown.name == "MeshDropdown")
                {
                    List<string> MyNames = new List<string>();
                    MyNames.Add("None");
                    MyNames.Add("Unique");
                    ItemManager MyItemManager = ItemManager.Get();
                    for (int i = 0; i < MyItemManager.MyMeshes.Count; i++)
                    {
                        MyNames.Add(MyItemManager.MyMeshes[i].name);
                    }
                    /*for (int i = 0; i < MyModelMaker.MyNames.Count; i++)
                    {
                        MyNames.Add(MyModelMaker.MyNames[i]);
                    }*/
                    FillDropDownWithList(MyDropdown, MyNames);
                }

            }
        }
        #endregion

        #region IndexController

        /// <summary>
        /// Load Gui with all the items data
        /// </summary>
        public override void OnUpdatedIndex(int NewIndex)
        {
            base.OnUpdatedIndex(NewIndex);
            Item SelectedItem = GetSelectedItem();
            if (SelectedItem != null)
            {
                //Debug.Log("Updating Index in ItemMaker.");
                GetInput("NameInput").text = SelectedItem.Name;
                GetInput("DescriptionInput").text = SelectedItem.GetDescription();
                GetInput("TagsInput").text = "" + SelectedItem.GetTags();
                GetInput("CommandsInput").text = "" + SelectedItem.GetCommands();
                GetImage("TextureImage").texture = SelectedItem.GetTexture();
                //GetToggle("UniqueTextureToggle").isOn = SelectedItem.IsUniqueTexture;
                UpdateMeshDropdown();
                StartCoroutine(UpdateTextureDropdown());
                CancelInvokes();
            }
            else
            {
                Debug.LogError("[ItemMaker] Null item at index: " + GetSelectedIndex() + " when loading item");
            }
        }

        /// <summary>
        /// Set the dropdown to the correct value
        /// </summary>
        private IEnumerator UpdateTextureDropdown()
        {
            yield return new WaitForSeconds(0.01f);
            //Item SelectedItem = GetSelectedItem();
            //Dropdown TextureDropdown = GetDropdown("TextureDropdown");
            //Debug.LogError("TextureDropdown has " + TextureDropdown.options.Count + " options.");
            //if (SelectedItem.IsUniqueTexture == false)
            /*{
                TextureDropdown.onValueChanged = new Dropdown.DropdownEvent();
                if (SelectedItem.GetTexture() != null)
                {
                    bool WasFound = false;
                    int ItemTexturesCount = DataManager.Get().GetSizeTextures(DataFolderNames.ItemTextures);
                    for (int i = 0; i < ItemTexturesCount; i++)
                    {
                        // if using same name as filename
                        string ItemTextureFileName = DataManager.Get().GetName(DataFolderNames.ItemTextures, i);
                        if (ItemTextureFileName == SelectedItem.MyTexture.name)
                        {
                            int NewValue = i + 1;
                            //Debug.LogError(SelectedItem.MyTexture.name + " - Texture Found: " + i + " - " + ItemTextureFileName
                            //     + " - Setting " + TextureDropdown.name + " to " + NewValue);
                            TextureDropdown.value = NewValue;   // because 0 is Unique value
                            WasFound = true;
                            i = ItemTexturesCount;
                            break;
                        }
                    }
                    if (WasFound == false)
                    {
                        TextureDropdown.value = 0;  // default is 0 ie unique - if not found
                    }
                }
                else
                {
                    //Debug.LogError("Item [" + SelectedItem.Name + "] Texture is null");
                    TextureDropdown.value = 0;
                }
                // readd the listener
                TextureDropdown.onValueChanged.AddEvent(
                    delegate 
                    {
                        UseInput(TextureDropdown);
                    });
            }
            else
            {
                TextureDropdown.value = 0;   // as it doesn't keep texture linked, for possible edit reasons
            }*/
        }

        private void UpdateMeshDropdown()
        {
            //MyMeshViewer.SetMesh(GetSelected().MyMesh);
            // set drop down too
            /*if (GetSelected().MyMesh && GetSelected().MyMesh.vertexCount > 0)
            {
                GetDropdown("MeshDropdown").value = 1;   // as it doesn't keep texture linked, for possible edit reasons
            }
            else
            {
                GetDropdown("MeshDropdown").value = 0;
            }*/
            GetDropdown("MeshDropdown").onValueChanged = new Dropdown.DropdownEvent();
            //if (GetSelectedItem().MeshType == ItemMeshType.VoxelReference)
            {
                /*StartCoroutine(MyVoxelViewer.RunScript(FileUtil.ConvertToList(GetSelected().MyModel)));
                for (int i = 0; i < MyModelMaker.MyNames.Count; i++)
                {
                    if (MyModelMaker.MyNames[i] == GetSelected().ModelName)
                    {
                        //Debug.Log(GetSelected().MyTexture.name + " - Texture Found: " + i + " - " + MyTextureMaker.ItemTextures[i].name);
                        GetDropdown("MeshDropdown").value = i + 2;   // because 0 is Unique value
                    }
                }*/
            }
            //else if (GetSelectedItem().MeshType == ItemMeshType.Voxel)
            {
                //StartCoroutine(MyVoxelViewer.RunScript(FileUtil.ConvertToList(GetSelectedItem().MyModel)));
                //GetDropdown("MeshDropdown").value = 1;
            }
            //else if (GetSelectedItem().MeshType == ItemMeshType.None)
            {
                /*GetDropdown("MeshDropdown").value = 0;
                if (MyVoxelViewer.GetSpawn())
                {
                    MyVoxelViewer.GetSpawn().GetComponent<Zeltex.Voxels.World>().Clear();
                }*/
            }
            GetDropdown("MeshDropdown").onValueChanged.AddEvent(delegate { UseInput(GetDropdown("MeshDropdown")); });
        }
        #endregion

    }
}
/// <summary>
/// Loads all the data
/// </summary>
/*public override void LoadAll()
{
    base.LoadAll();
    Clear();
    List<string> MyFiles = FileUtil.GetFilesOfType(GetFilePath(), FileExtension);
    //Debug.Log("Loading " + MyFiles.Count + " Items.");
    for (int i = 0; i < MyFiles.Count; i++)
    {
        string MyScript = FileUtil.Load(MyFiles[i]);
        Item MyData = new Item();
        MyData.RunScript(FileUtil.ConvertToList(MyScript));
        MyData.Name = Path.GetFileNameWithoutExtension(MyFiles[i]);
        AddName(MyData.Name);
        MyInventory.Add(MyData);
    }
    MyIndexController.SetMaxSelected(GetSize());
    if (gameObject.activeInHierarchy)
    {
        OnBegin();  // refresh if already opened
    }
}

/// <summary>
/// Saves all the quests
/// </summary>
public override void SaveAll()
{
    string FolderPath = GetFilePath();
    //Debug.Log("Saving All in ItemMaker [" + FolderPath + "] Saving all of " + MyInventory.GetSize() + " Items.");
    for (int i = 0; i < MyInventory.GetSize(); i++)
    {
        Item MyData = MyInventory.GetItem(i);
        List<string> MyScript = FileUtil.ConvertToList(MyData.GetScript());
        FileUtil.Save(FolderPath + MyData.Name + "." + FileExtension, FileUtil.ConvertToSingle(MyScript));
    }
}*/

/*#region ImportExport

/// <summary>
/// Export the file using webgl
/// </summary>
public void Export()
{
    //FileUtil.Export(GetSelectedName(), FileExtension, GetSelected().GetScript());
}
/// <summary>
/// Import Data using Webgl
/// Called on mouse down - instead of mouse up like normal buttons
/// </summary>
public void Import()
{
    //FileUtil.Import(name, "Upload", FileExtension);
}
/// <summary>
/// Called from javascript, uploading a model data
/// </summary>
public void Upload(string MyScript)
{
    string UploadFileName = "";
    for (int i = 0; i < MyScript.Length; i++)
    {
        if (MyScript[i] == '\n')
        {
            UploadFileName = MyScript.Substring(0, i);
            UploadFileName = Path.GetFileNameWithoutExtension(UploadFileName);
            MyScript = MyScript.Substring(i + 1);
            break;
        }
    }
    if (UploadFileName != "")
    {
        Debug.Log("Uploading new voxel:" + UploadFileName + ":" + MyScript.Length);
        Item NewData = new Item();
        NewData.RunScript(FileUtil.ConvertToList(MyScript));
        NewData.Name = UploadFileName;
        AddData(NewData);
    }
}
#endregion*/
