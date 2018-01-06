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
    /// Item is the main quantified data stored in the game.
    /// Can be stored in inventories, item objects, Voxel-chests.
    /// </summary>
    [System.Serializable]
    public class Item : Element
    {
        #region Variables
        [Tooltip("Used in the tooltip to describe the item")]
        [SerializeField, JsonProperty]
		private string Description;
		[Tooltip("The stats the item contains"), JsonProperty]
		public Stats MyStats;
		[JsonProperty]
		public Zexel MyZexel;
		[JsonProperty]
        public VoxelModel MyModel = null;
        [JsonProperty]
        public PolyModel MyPolyModel = null;
        [JsonProperty]
        public int TextureMapIndex = 0;

        [Tooltip("Used in activation of the item")]
        [SerializeField, JsonProperty]
        private List<string> Commands;
        [SerializeField, JsonProperty]
        private List<string> Tags;
        [Tooltip("How many of that item there is.")]
        [SerializeField, JsonProperty]
        private int Quantity = 1;

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
        [JsonIgnore]
        public Mesh MyMesh;
        [JsonIgnore]
        public Material MyMaterial;
        [HideInInspector, JsonIgnore]
        private Inventory ParentInventory = null;
        [HideInInspector, JsonIgnore]
        public UnityEngine.Events.UnityEvent OnUpdate = new UnityEngine.Events.UnityEvent();

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
        #endregion

        #region Initiation

        public void EmptyZexel()
        {
            if (MyZexel != null)
            {
                MyZexel.SetTexture(null);
            }
        }

        public override void OnLoad()
        {
            base.OnLoad();
            if (MyStats != null)
            {
                MyStats.ParentElement = this;
            }
            if (MyZexel != null)
            {
                MyZexel.ParentElement = this;
            }
            if (MyModel != null)
            {
                MyModel.ParentElement = this;
            }
            if (MyPolyModel != null)
            {
                MyPolyModel.ParentElement = this;
            }
        }


        public void SetParentInventory(Inventory NewParent)
        {
            if (ParentElement == null || ParentElement.GetType() == typeof(Inventory))
            {
                ParentElement = NewParent;
            }
            ParentInventory = NewParent;
        }

        public Item()
        {
            Name = "Empty";
            Description = "Null";
            MyStats = new Stats();
            MyStats.ParentElement = this;
            MyZexel = new Zexel();
            MyZexel.ParentElement = this;
            Commands = new List<string>();
            Tags = new List<string>();
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
			return 0;
        }

        public float GetSellValue()
        {
           // return SellValue;
           return 0;
        }

        public float GetBuyValue()
        {
			// return BuyValue;
			return 0;
        }
        public bool IsBuyable()
		{
			return false;
        }

        public bool IsSellable()
        {
			return false;
        }

        public bool IsSelling()
		{
			return false;
        }

        public bool IsBuying()
		{
			return false;
        }
        #endregion

        #region Has

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

		#region Art


		/// <summary>
		/// Set a polygonal mesh for the item
		/// </summary>
		public void SetMesh(Mesh MyMesh_)
		{
			if (MyMesh != MyMesh_)
			{
				//M//eshType = ItemMeshType.Polygonal;
				MyMesh = MyMesh_;
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

		#endregion

		private ItemHandler MyItemHandler;

		public override void Spawn() 
		{
			if (MyItemHandler == null)
			{
				GameObject NewItem = new GameObject();
				NewItem.name = Name + "-Handler";
				MyItemHandler = NewItem.AddComponent<ItemHandler>();
				MyItemHandler.SetItem(this);
			}
		}

        public override void DeSpawn() 
		{
			if (MyItemHandler)
			{
				MyItemHandler.gameObject.Die();
			}
		}

        public override bool HasSpawned() 
		{
			return (MyItemHandler != null);
		}

        public Inventory GetParentInventory()
        {
            return ParentInventory;
        }
        /// <summary>
        /// Returns the dragged Item
        /// </summary>
        public Item SwitchItems(Item ItemB)
        {
            Item ReturnItem = null;
            Item ItemA = this;
            if (ItemB == null)
            {
                Debug.LogError("ItemB is null.");
                return ReturnItem;
            }

            if (ItemA.ParentInventory == null && ItemB.ParentInventory == null)
            {
                // nothing i guess
                Debug.LogError("Both Parents are null.");
            }
            // If same Element
            //if (ItemA.ParentInventory != null && ItemB.ParentInventory != null)
            {
                Inventory InventoryA = ItemA.ParentInventory;
                Inventory InventoryB = ItemB.ParentInventory;
                if (ItemA.Name != "Empty" && ItemA.Name == ItemB.Name)
                {
                    // increase itemA instead
                    //Debug.LogError("Increase Quantity of ItemA: " + ItemA.Name + " " + ItemA.GetQuantity());
                    ItemA.IncreaseQuantity(ItemB.GetQuantity());
                    ItemB.SetQuantity(0);
                    ItemB.SetName("Empty");
                    ItemB.EmptyZexel();
                    ItemA.OnUpdate.Invoke();
                    ItemB.OnUpdate.Invoke();
                    if (InventoryA != null)
                    {
                        InventoryA.OnUpdateItem.Invoke(InventoryA.MyItems.IndexOf(ItemA));
                    }
                    return ItemB;
                }
                else
                {
                    // Set Bones
                    Skeletons.Bone BoneA = ItemA.ParentElement as Skeletons.Bone;
                    Skeletons.Bone BoneB = ItemB.ParentElement as Skeletons.Bone;
                    if (BoneA != null)
                    {
                        BoneA.MyItem = ItemB;
                    }
                    if (BoneB != null)
                    {
                        BoneB.MyItem = ItemA;
                    }
                    // Switch Parent Elements
                    Element TemporaryElement = ItemA.ParentElement;
                    ItemA.ParentElement = ItemB.ParentElement;
                    ItemB.ParentElement = TemporaryElement;

                    // Swap items
                    if (InventoryA != null && InventoryA.MyItems.Contains(ItemA))
                    {
                        int IndexA = InventoryA.MyItems.IndexOf(ItemA);
                        InventoryA.MyItems[IndexA] = ItemB;
                        ItemB.SetParentInventory(InventoryA);
                        InventoryA.OnUpdateItem.Invoke(IndexA);
                    }
                    else
                    {
                        ItemB.SetParentInventory(null);
                    }
                    if (InventoryB != null && InventoryB.MyItems.Contains(ItemB))
                    {
                        int IndexB = InventoryB.MyItems.IndexOf(ItemB);
                        InventoryB.MyItems[IndexB] = ItemA;
                        ItemA.SetParentInventory(InventoryB);
                        InventoryB.OnUpdateItem.Invoke(IndexB);
                    }
                    else
                    {
                        ItemA.SetParentInventory(null);
                    }
                }
            }
            // if either is null
            ItemA.OnUpdate.Invoke();
            ItemB.OnUpdate.Invoke();
            return ItemA;
        }
    }

}
// maybe make item action as well, ie (open a door)

// give worldItem, a function, so i can have other scripts activate when they are selected - ie flip a car, open a door


/*int IndexA = InventoryA.MyItems.IndexOf(ItemA);
                    int IndexB = InventoryA.MyItems.IndexOf(ItemB);
                    InventoryA.MyItems[IndexA] = ItemB;
                    InventoryA.OnUpdateItem.Invoke(IndexA);
                    InventoryA.MyItems[IndexB] = ItemA;
                    InventoryA.OnUpdateItem.Invoke(IndexB);*/
//Debug.Log(IndexA + " Has switched with " + IndexB + " of Inventory: " + InventoryA.Name);
//Debug.Log("ItemA: " + IndexA + " Has switched with ItemB: " + IndexB + " of InventoryA: " + InventoryA.Name + " and InventoryB: " +  InventoryB.Name);


/*else
            {
                else if (ItemA.ParentInventory == null && ItemB.ParentInventory != null)
                {
                    Inventory InventoryB = ItemB.ParentInventory;
                    if (!InventoryB.MyItems.Contains(ItemB)) 
                    {
                        Debug.LogError("InventoryB[" + InventoryB.Name + "] does not contain ItemB: " + ItemB.Name + " out of total items: " + InventoryB.GetSize());
                        return null;
                    }
                    int IndexB = InventoryB.MyItems.IndexOf(ItemB);
                    InventoryB.MyItems[IndexB] = ItemA;
                    InventoryB.OnUpdateItem.Invoke(IndexB);
                    ItemA.SetParentInventory(InventoryB);
                    ItemB.SetParentInventory(null);
                    //Debug.Log(IndexB + " Has switched with an individual item of Inventory: " + InventoryB.Name);
                    ReturnItem = ItemB;
                }
                else if (ItemB.ParentInventory == null && ItemA.ParentInventory != null)
                {
                    Inventory InventoryA = ItemA.ParentInventory;
                    if (!InventoryA.MyItems.Contains(ItemA)) 
                    {
                        Debug.LogError("InventoryA[" + InventoryA.Name + "] does not contain ItemA: " + ItemA.Name + " out of total items: " + InventoryA.GetSize());
                        return null;
                    }
                    int IndexA = InventoryA.MyItems.IndexOf(ItemA);
                    InventoryA.MyItems[IndexA] = ItemB;
                    InventoryA.OnUpdateItem.Invoke(IndexA);
                    ItemB.SetParentInventory(InventoryA);
                    ItemA.SetParentInventory(null);
                    //Debug.Log(IndexA + " Has switched with an individual item of Inventory: " + InventoryA.Name);
                    ReturnItem = ItemA;
                }
            }*/