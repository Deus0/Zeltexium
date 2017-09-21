using System.Collections.Generic;
using UnityEngine;
using Zeltex.Util;
using Zeltex.Combat;
using MakerGuiSystem;
using Zeltex.Voxels;
using Zeltex;
using Newtonsoft.Json;

namespace Zeltex.Items 
{
    /// <summary>
    /// The meshes used for items
    /// </summary>
    [System.Serializable]
    public enum ItemMeshType
    {
        None,               // default cube will be used
        Polygonal,          // polygonal model stored inside the item
        PolygonalReference, // polygonal reference, using a Mesh
        Voxel,              // Voxel model - Stored inside the item
        VoxelReference      // Voxel model reference - using ModelMaker data
    }
    /// <summary>
    /// The meshes used for items
    /// </summary>
    [System.Serializable]
    public enum ItemTextureType
    {
        None,                   // default texture will be used
        Pixels,                 // Individual pixels will be stored
        PixelsReference,        // Reference to pixels will be stored
        Instructions,           // TextureInstructions will be stored inside the item
        InstructionsReference   // Reference to a TextureInstructions file will be stored
    }
    /// <summary>
    /// Item is the main quantified data stored in the game.
    /// Can be stored in inventories, item objects, Voxel-chests.
    /// </summary>
    [System.Serializable]
    public class Item : Element
    {
        #region Variables
        //public string TextureName = "";
        //public string MeshName = "";
        [Tooltip("Used in the tooltip to describe the item")]
        [SerializeField, JsonProperty]
        private string Description;
        [Tooltip("Used in activation of the item")]
        [SerializeField, JsonProperty]
        private List<string> Commands = new List<string>();
        [SerializeField, JsonProperty]
        private List<string> Tags = new List<string>();
        [Tooltip("How many of that item there is.")]
        [SerializeField, JsonProperty]
        private int Quantity = 1;
        [Tooltip("How much the item is worth, Selling at the highest")]
        [SerializeField]
        private float SellValue = -1;
        [Tooltip("How much the item is worth, Buying at the lowest")]
        [SerializeField]
        private float BuyValue = -1;
        [Tooltip("The stats the item contains"), JsonProperty]
        public Stats MyStats;
        [JsonProperty]
        public Zexel MyZexel = new Zexel();

        // Mesh too!
        public ItemMeshType MeshType;
        [JsonIgnore]
        [Tooltip("The mesh of the item. Displayed in the world.")]
        public Mesh MyMesh;
        [JsonIgnore]
        public Material MyMaterial;
        public string ModelName = "";
        //[JsonProperty, SerializeField]
        //private string TextureName = "";

        [JsonIgnore]
        static private string EndingColor = "</color>";
        [JsonIgnore]
        static private string CommandColor = "<color=#989a33>";
        [JsonIgnore]
        static private string TagColor = "<color=#779b33>";
        [JsonIgnore]
        static private string QuantityColor = "<color=#00cca4>";
        [JsonIgnore]
        static private string DescriptionColor = "<color=#474785>";
        #endregion

        #region Initiation
        public Item()
        {
            Name = "Empty";
            Description = "Null";
            MyStats = new Stats();
        }
        #endregion

        #region Getters
        public string GetInput(string Command)
        {
            for (int i = 0; i < Commands.Count; i++)
            {
                string ThisCommand = ScriptUtil.GetCommand(Commands[i]);
                if (Command == ThisCommand)
                {
                    return ScriptUtil.RemoveCommand(Commands[i]);
                }
            }
            return "";
        }
        public int GetInputInt(string Command)
        {
            for (int i = 0; i < Commands.Count; i++)
            {
                if (Commands[i].Contains(Command))
                {
                    string Input = ScriptUtil.RemoveCommand(Commands[i]);
                    if (Input == "")
                        return 1;
                    try
                    {
                        int IntInput = int.Parse(Input);
                        return IntInput;
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            return 1;
        }
        public string GetDescription()
        {
            return Description;
        }

        public Mesh GetMesh()
        {
            return MyMesh;
        }

        public Material GetMaterial()
        {
            return MyMaterial;
        }

        public Texture2D GetTexture()
        {
            return MyZexel.GetTexture();
        }

        public int GetQuantity()
        {
            return Quantity;
        }

        public string GetDescriptionLabel()
        {
            string DescriptionText = "";
            DescriptionText += QuantityColor + "Quantity x" + Quantity + "\n" + EndingColor;

            DescriptionText += DescriptionColor + Description + EndingColor;
            for (int j = 0; j < MyStats.GetSize(); j++)
            {
                Stat MyStat = MyStats.GetStat(j);
                DescriptionText += "\n   " + MyStat.Name;
                if (MyStat.GetValue() > 0)
                {
                    DescriptionText += ": +" + MyStat.GetValue();
                }
                else if (MyStat.GetValue() < 0)
                {
                    DescriptionText += ": -" + Mathf.Abs(MyStat.GetValue()).ToString();
                }
            }
            for (int i = 0; i < Tags.Count; i++)
            {
                DescriptionText += TagColor + "\n[" + Tags[i] + "]" + EndingColor;
            }
            if (Tags.Count == 0)
                DescriptionText += TagColor + "\n[No Tags]" + EndingColor;
            for (int i = 0; i < Commands.Count; i++)
            {
                DescriptionText += CommandColor + "\n[" + Commands[i] + "]" + EndingColor;
            }
            if (Commands.Count == 0)
                DescriptionText += CommandColor + "\n[No Commands]" + EndingColor;
            return DescriptionText;
        }
        public string GetTags()
        {
            string MyTags = "";
            for (int i = 0; i < Tags.Count; i++)
                MyTags += Tags[i] + "\n";
            return MyTags;
        }

        public string GetCommands()
        {
            string MyCommands = "";
            for (int i = 0; i < Commands.Count; i++)
                MyCommands += Commands[i] + "\n";
            return MyCommands;
        }
        public string GetCommand(string Data)
        {
            if (Data.Length == 0)
            {
                return "";
            }
            for (int i = 0; i < Data.Length; i++)
            {
                if (Data[i] == '/')
                {
                    Data = Data.Substring(i);
                    i = Data.Length;
                }
            }
            if (Data[0] == '/')
            {
                string[] New = Data.Split(' ');
                return New[0];
            }
            else
            {
                return "";
            }
        }

        // buy-sell stuff
        // assumong buy value is minimum, sell value is max
        public float GetMidValue()
        {
            if (SellValue == -1 && BuyValue != -1)
                return BuyValue;
            else if (SellValue != -1 && BuyValue == -1)
                return SellValue;
            else if (SellValue == -1 && BuyValue == -1)
                return -1;
            else
                return BuyValue + (SellValue - BuyValue) / 2f;
        }

        public float GetSellValue()
        {
            return SellValue;
        }

        public float GetBuyValue()
        {
            return BuyValue;
        }
        public bool IsBuyable()
        {
            if (BuyValue == -1)
                return false;
            else {
                return true;
            }
        }

        public bool IsSellable()
        {
            if (SellValue == -1)
                return false;
            //if (MyValue >= SellValue)
            if (Quantity == 0)
                return false;
            else
                return true;
        }

        public bool IsSelling()
        {
            return (SellValue != -1);
        }

        public bool IsBuying()
        {
            return (BuyValue != -1);
        }
        #endregion

        #region Has
        /*public bool HasUpdated()
        {
            if (NeedsUpdating)
            {
                NeedsUpdating = false;
                return true;
            }
            return false;
        }*/
        public bool HasCommand()
        {
            return (Commands.Count > 0);
        }

        public bool HasCommand(string Command)
        {
            for (int i = 0; i < Commands.Count; i++)
            {
                if (Commands[i].Contains(Command))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Has item got a tag?
        /// </summary>
        public bool HasTag(string MyTag)
        {
            for (int i = 0; i < Tags.Count; i++)
            {
                if (ScriptUtil.RemoveWhiteSpace(Tags[i]) == (ScriptUtil.RemoveWhiteSpace(MyTag)))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region ModifyData

        /// <summary>
        /// Sets the item tags
        /// </summary>
        public void SetTags(string TagsCombined)
        {
            List<string> NewTags = new List<string>();
            string[] SeperatedTags = TagsCombined.Split('\n');
            for (int i = 0; i < SeperatedTags.Length; i++)
            {
                NewTags.Add(SeperatedTags[i]);
            }
            if (!AreListsTheSame(Tags, NewTags))
            {
                Tags.Clear();
                Tags.AddRange(NewTags);
                OnModified();
            }
        }

        /// <summary>
        /// Sets the commands of the item
        /// </summary>
        public void SetCommands(string CommandsCombined)
        {
            List<string> NewCommands = new List<string>();
            string[] SeperatedCommands = CommandsCombined.Split('\n');
            for (int i = 0; i < SeperatedCommands.Length; i++)
            {
                NewCommands.Add(SeperatedCommands[i]);
            }
            if (!AreListsTheSame(Commands, NewCommands))
            {
                Commands.Clear();
                Commands.AddRange(NewCommands);
                OnModified();
            }
        }

        private bool AreListsTheSame(List<string> ListA, List<string> ListB)
        {
            if (ListA.Count != ListB.Count)
            {
                return false;
            }
            for (int i = 0; i < ListA.Count; i++)
            {
                if (ListA[i] != ListB[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Sets the item commands
        /// </summary>
        /* public void SetCommands(string Input)
         {
             int LastIndex = 0;
             Commands = new List<string>();
             for (int i = 0; i < Input.Length; i++)
             {
                 if (Input[i] == '\n' || i == Input.Length - 1)
                 {
                     int EndIndex = i;
                     if (i == Input.Length - 1)
                         EndIndex = Input.Length;
                     string Command = Input.Substring(LastIndex, EndIndex);
                     Commands.Add(Command);
                     LastIndex = i + 1;
                 }
             }
             OnModified();
         }*/

        /// <summary>
        /// Sets the item description
        /// </summary>
        public void SetDescription(string NewDescription)
        {
            if (Description != NewDescription)
            {
                Description = NewDescription;
                OnModified();
            }
        }

        /// <summary>
        /// Sets the item texture
        /// </summary>
        public void SetTexture(Texture2D NewTexture)
        {
            if (NewTexture != null)
            {
                MyZexel.SetTexture(NewTexture);
                //MyTexture = NewTexture;
                //TextureName = MyTexture.name;
                OnModified();
            }
        }

        /// <summary>
        /// Set a polygonal mesh for the item
        /// </summary>
        public void SetMesh(Mesh MyMesh_)
        {
            if (MyMesh != MyMesh_)
            {
                MeshType = ItemMeshType.Polygonal;
                MyMesh = MyMesh_;
                OnModified();
            }
        }

        /// <summary>
        /// Sets the model of the item
        /// </summary>
        public void SetModel(string ModelName_, string MyModel_)
        {
            /*if (MyModel != MyModel_ || ModelName != ModelName_)
            {
                MeshType = ItemMeshType.VoxelReference;
                MyModel = MyModel_;
                ModelName = ModelName_;
                OnModified();
            }*/
        }

        /// <summary>
        /// Sets the item quantity
        /// </summary>
        public void SetQuantity(int NewQuantity)
        {
            if (NewQuantity != Quantity)
            {
                Quantity = NewQuantity;
                OnModified();
            }
        }
        #endregion

        #region LoadingFromDatabase
        /// <summary>
        /// Load the texture from manager!
        /// </summary>
        /*public void LoadTextureFromManager(string NewTextureName)
        {
            //Debug.Log("Loading Texture " + TextureName + " for item " + Name);
            TextureName = NewTextureName;
            GetTexture();
            //IsUniqueTexture = false;
        }*/
        /// <summary>
        /// Used only for editor items. Loads Model from ModelMaker
        /// </summary>
        private void LoadModelFromManager(string ModelName_)
        {
            //Debug.LogError("Converting " + Name + " To voxel Reference of " + ModelName_);
            MeshType = ItemMeshType.VoxelReference;
            ModelName = ModelName_;
            //MyModel = DataManager.Get().Get("Models", ModelName);
            //Debug.LogError("New MyModel: " + MyModel);
        }
        #endregion

        #region Utility
        /// <summary>
        /// Returns true if changed
        /// </summary>
        public bool IncreaseQuantity(int Addition)
        {
            if (Addition != 0)
            {
                int OldQuantity = Quantity;
                Quantity += Addition;
                if (Quantity < 0)
                {
                    Quantity = 0;
                }
                if (OldQuantity != Quantity)
                {
                    OnModified();
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region File

        /*public override void RunScript(string Script)
        {
            RunScript(FileUtil.ConvertToList(Script));
        }
        public override string GetScript()
        {
            return FileUtil.ConvertToSingle(GetScript2());
        }

        public List<string> GetScript2()
        {
            List<string> MyScript = new List<string>();
            MyScript.Add("/item " + Name);
            if (Description != "")
            {
                MyScript.Add("/description " + Description);
            }
            if (Quantity != 1)
            {
                MyScript.Add("/quantity " + Quantity);
            }
            if (SellValue != -1)
            {
                MyScript.Add("/sellvalue " + SellValue);
            }
            if (BuyValue != -1)
            {
                MyScript.Add("/buybalue " + BuyValue);
            }
            if (Commands.Count != 0)
            {
                MyScript.Add("/commands");
                for (int i = 0; i < Commands.Count; i++)
                {
                    MyScript.Add(Commands[i]);
                }
                MyScript.Add("/endcommands");
            }
            if (Tags.Count != 0)
            {
                MyScript.Add("/tags");
                for (int i = 0; i < Tags.Count; i++)
                {
                    MyScript.Add(Tags[i]);
                }
                MyScript.Add("/endtags");
            }
            //NewSaveData.Add ("/texture " + MyTexture.name);
            if (MyStats.GetSize() != 0)
            {
                //MyScript.Add ("/stats");
                MyScript.AddRange(MyStats.GetScriptList(false));
            }

            if (MyTexture != null)
            {
                if (IsUniqueTexture)
                {
                    MyScript.AddRange(GetTextureScript());
                }
                else
                {
                    MyScript.Add("/LoadTexture " + MyTexture.name);
                }
            }
            if (MeshType == ItemMeshType.VoxelReference)
            {
                MyScript.Add("/VoxelReference " + ModelName);
            }
            else if (MeshType == ItemMeshType.Polygonal)
            {
                if (MyMesh != null)
                {
                    MyScript.AddRange(GetMeshScript());
                }
            }
            MyScript.Add("/EndItem");
            return MyScript;
        }*/
        /*public Item(List<string> Data)
        {
            RunScript(Data);
        }

        /// <summary>
        /// Load the item
        /// </summary>
		public void RunScript(List<string> Data)
        {
            //Debug.LogError("Loading Item: " + Name + " with " + Data.Count + " lines");
            NeedsUpdating = true;
            Quantity = 1;               // default
            MyStats = new Stats();       // default
            for (int i = 0; i < Data.Count; i++)
            {
                if (ScriptUtil.IsCommand(Data[i]))
                {
                    string Command = ScriptUtil.GetCommand(Data[i]);
                    string Other = ScriptUtil.RemoveCommand(Data[i]);
                    //Debug.LogError ("CommandName [" + Command + "] - Other [" + Other + "]");
                    //	Debug.LogError ("Command Length: " + Command.Length);	// extra debug
                    //Debug.LogError ("Input [" + Other + "]");

                    switch (Command)
                    {
                        case ("/item"):
                            Name = Other;
                            break;
                        case ("/commands"):
                            {
                                //Debug.LogError ("Reading Commands.");
                                int j = i + 1;
                                while (ScriptUtil.RemoveWhiteSpace(Data[j]) != "/endcommands")
                                {
                                    string NewCommand = ScriptUtil.RemoveWhiteSpace(Data[j]);
                                    Commands.Add(NewCommand);
                                    //Debug.LogError ("Adding New Command [" + NewCommand + "] of length [" + NewCommand.Length + "]");
                                    //Debug.Break ();
                                    j++;
                                }
                                i = j;
                            }
                            break;
                        case ("/tags"):
                            {
                                int j = i + 1;
                                while (ScriptUtil.RemoveWhiteSpace(Data[j]) != "/endtags")
                                {
                                    string NewTag = ScriptUtil.RemoveWhiteSpace(Data[j]);
                                    Tags.Add(NewTag);
                                    j++;
                                }
                                i = j;
                            }
                            break;
                        case ("/description"):
                            Description = Other;
                            break;
                        case ("/TextureBegin"):
                            //Debug.LogError("looking for  /TextureEnd");
                            for (int j = i + 1; j < Data.Count; j++)
                            {
                                if (ScriptUtil.RemoveWhiteSpace(Data[j]) == "/TextureEnd")
                                {
                                    if (j - i - 1 < Data.Count)
                                    {
                                        List<string> MyData = Data.GetRange(i + 1, j - i - 1);   // + 1
                                        LoadTexture(MyData);
                                        i = j;
                                    }
                                    break;
                                }
                            }
                            break;
                        case ("/MeshBegin"):
                            //Debug.LogError(Name + " - Item Mesh Begin on: " + i);
                            for (int j = i + 1; j < Data.Count; j++)
                            {
                                if (ScriptUtil.RemoveWhiteSpace(Data[j]) == "/MeshEnd")
                                {
                                    if (j - i - 1 < Data.Count)
                                    {
                                        List<string> MyData = Data.GetRange(i + 1, j - i - 1);   // + 1
                                        LoadMesh(MyData);
                                        i = j;
                                    }
                                    break;
                                }
                            }
                            break;
                        case ("/quantity"):
                            try
                            {
                                Quantity = int.Parse(Other);
                            }
                            catch (System.FormatException e)
                            {

                            }
                            break;
                        case ("/sellvalue"):
                            try {
                                SellValue = int.Parse(Other);
                            }
                            catch (System.FormatException e)
                            {
                            }
                            break;
                        case ("/buyvalue"):
                            try {
                                BuyValue = int.Parse(Other);
                            }
                            catch (System.FormatException e)
                            {
                            }
                            break;
                        case ("/stats"):
                            Debug.Log("Now loading stats!");
                            break;
                        case ("/LoadTexture"):
                            LoadTextureFromManager(Other);
                            break;
                        case ("/VoxelReference"):
                            LoadModelFromManager(Other);
                            break;
                    }
                    if (Command.Contains("/stats"))
                    {
                        //Debug.Log ("Now loading stats!");
                        MyStats.RunScript(Data);
                    }
                }
            }
            //Debug.LogError ("Loaded Item [" + Name + "] with [" + Commands.Count + "] Commands.");
        }*/

        /// <summary>
        /// Returns a script of the texture pixels and size.
        /// </summary>
        /*public List<string> GetTextureScript()
        {
            List<string> MyScript = new List<string>();
            MyScript.Add("/TextureBegin");
            MyScript.Add(MyTexture.width + " " + MyTexture.height);
            Color32[] MyPixels = (MyTexture as Texture2D).GetPixels32(0);
            for (int i = 0; i < MyPixels.Length; i++)
            {
                MyScript.Add(MyPixels[i].r + " " + MyPixels[i].g + " " + MyPixels[i].b + " " + MyPixels[i].a);
            }
            MyScript.Add("/TextureEnd");
            return MyScript;
        }*/

        /// <summary>
        /// Loads a texture from a list of strings
        /// </summary>
        /*public void LoadTexture(List<string> MyData)
        {
            Debug.Log("Loading Unique Texture for item " + Name);
            IsUniqueTexture = true;
            string[] MyDimensions = MyData[0].Split(' ');
            MyTexture = new Texture2D(int.Parse(MyDimensions[0]), int.Parse(MyDimensions[1]));
            MyTexture.filterMode = FilterMode.Point;
            Color32[] MyPixels = MyTexture.GetPixels32(0);
            //Debug.LogError(Name + " - Loading Texture: " + (MyData.Count - 1) + "\n"
            //    + "Of Size: " + MyTexture.width + ":" + MyTexture.height + "\n"
            //    + FileUtil.ConvertToSingle(MyData));
            for (int i = 1; i < MyData.Count; i++)
            {
                string[] MyColours = MyData[i].Split(' ');
                Color32 MyColor = Color.red;
                try
                {
                    MyColor = new Color32(byte.Parse(MyColours[0]), byte.Parse(MyColours[1]), byte.Parse(MyColours[2]), byte.Parse(MyColours[3]));
                }
                catch (System.FormatException e) { }
                if (i - 1 >= 0 && i - 1 < MyPixels.Length)
                {
                    MyPixels[i - 1] = (MyColor);
                }
            }
            MyTexture.SetPixels32(MyPixels);
            MyTexture.Apply();
        }*/

        public List<string> GetMeshScript()
        {
            List<string> MyScript = new List<string>();
            MyScript.Add("/MeshBegin");
            Zeltex.Voxels.MeshData MyMeshData = new Zeltex.Voxels.MeshData(MyMesh);
            MyScript.AddRange(MyMeshData.GetScript());
            //Debug.LogError("Saving Mesh: " + (MyScript.Count-1));
            MyScript.Add("/MeshEnd");
            return MyScript;
        }
        public void LoadMesh(List<string> MyData)
        {
            MeshData MyMeshData = new MeshData();
            MyMeshData.RunScript(MyData);
            MyMesh = MyMeshData.GetMesh();
            //Debug.LogError(Name + " - Loaded Mesh: " + MyData.Count + ": vertices: " + MyMesh.vertices.Length + ": triangles: " + MyMesh.triangles.Length
            //    + ": uvs: " + MyMesh.uv.Length + ": colors32: " + MyMesh.colors32.Length);
        }
        #endregion


        #region ElementOverrides

        /*public override string GetFolder()
        {
            return DataFolderNames.Items;
        }*/

        #endregion
    }

}
// maybe make item action as well, ie (open a door)

// give worldItem, a function, so i can have other scripts activate when they are selected - ie flip a car, open a door