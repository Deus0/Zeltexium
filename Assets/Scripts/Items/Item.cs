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
        #region Statics
        [JsonIgnore]
        static private string EndingColor = "</color>";
        //[JsonIgnore]
        //static private string CommandColor = "<color=#989a33>";
        [JsonIgnore]
        static private string TagColor = "<color=#779b33>";
        [JsonIgnore]
        static private string QuantityColor = "<color=#00cca4>";
        [JsonIgnore]
        static private string DescriptionColor = "<color=#474785>";
        #endregion

        [Tooltip("Used in the tooltip to describe the item")]
        [SerializeField, JsonProperty]
		private string Description;
        [Tooltip("Used by the game to describe the item")]
        [SerializeField, JsonProperty]
        private List<string> MetaTags;

        [JsonProperty]
        public Zexel MyZexel;
        [Tooltip("The stats the item contains"), JsonProperty]
		public Stats MyStats;

		[JsonProperty]
        public VoxelModel MyModel = null;
        [JsonProperty]
        public int TextureMapIndex = 0;
        [JsonProperty]
        public PolyModel MyPolyModel = null;

        [JsonProperty]
        public Spell MySpell;

        [Tooltip("How many of that item there is.")]
        [SerializeField, JsonProperty]
        private int Quantity = 1;

        [HideInInspector, JsonIgnore]
        private Inventory ParentInventory = null;
        [HideInInspector, JsonIgnore]
        public UnityEngine.Events.UnityEvent OnUpdate = new UnityEngine.Events.UnityEvent();
        [JsonIgnore]
        public Guis.ItemGui MyGui;
        [JsonIgnore]
        private ItemHandler MyItemHandler;
        [JsonIgnore]
        System.Action MyOnFinishLoading = null;

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
            if (MyZexel != null)
            {
                MyZexel.ParentElement = this;
            }
            if (MySpell != null)
            {
                MySpell.ParentElement = this;
            }
            if (MyStats != null)
            {
                MyStats.ParentElement = this;
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
            MetaTags = new List<string>();
            MyStats = null;
            MyZexel = null;
        }
        #endregion

        #region Getters

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
            for (int i = 0; i < MetaTags.Count; i++)
            {
                DescriptionText += TagColor + "\n[" + MetaTags[i] + "]" + EndingColor;
            }
            if (MetaTags.Count == 0)
                DescriptionText += TagColor + "\n[No Tags]" + EndingColor;
            return DescriptionText;
        }
        public string GetTags()
        {
            string MyTags = "";
            for (int i = 0; i < MetaTags.Count; i++)
                MyTags += MetaTags[i] + "\n";
            return MyTags;
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

        /// <summary>
        /// Has item got a tag?
        /// </summary>
        public bool HasTag(string MyTag)
        {
            for (int i = 0; i < MetaTags.Count; i++)
            {
                if (ScriptUtil.RemoveWhiteSpace(MetaTags[i]) == (ScriptUtil.RemoveWhiteSpace(MyTag)))
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
            if (!AreListsTheSame(MetaTags, NewTags))
            {
                MetaTags.Clear();
                MetaTags.AddRange(NewTags);
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

        public void SetToEmptyItem() 
        {
            SetQuantity(0);
            SetName("Empty");
            EmptyZexel();
            OnUpdate.Invoke();
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
                    if (Quantity == 0)
                    {
                        SetToEmptyItem();
                    }
                    else
                    {
                        OnUpdate.Invoke();
                    }
                    return true;
                }
            }
            return false;
		}
        #endregion

		#region Art

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

		public Texture2D GetTexture()
		{
			return MyZexel.GetTexture();
		}

        #endregion

        public ItemHandler GetSpawn() 
        {
            return MyItemHandler;
        }
        public void SpawnE(System.Action OnFinishLoading = null)
        {
            MyOnFinishLoading = OnFinishLoading;
            Spawn();
        }

        public override void Spawn()
        {
            if (MyItemHandler == null)
            {
                GameObject NewItem = new GameObject();
                NewItem.name = Name + "-Handler";
                MyItemHandler = NewItem.AddComponent<ItemHandler>();
                MyOnFinishLoading += () =>
                {
                    NewItem.SetLayerRecursive(LayerManager.Get().GetItemsLayer());
                };
                MyItemHandler.SetItem(this, MyOnFinishLoading);
                MyOnFinishLoading = null;   // make sure to not use this again
            }
            else
            {
                Debug.LogError("Trying to spawn when handler already exists for: " + Name);
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
        /// ItemB is the mouse pickup item
        /// </summary>
        public void RightClickItem(Item ItemB)
        {
            //Item ReturnItem = null;
            Item ItemA = this;
            if (ItemB == null)
            {
                Debug.LogError("ItemB is null.");
                return;
            }
            if (ItemA.ParentInventory == null && ItemB.ParentInventory == null)
            {
                // nothing i guess
                Debug.LogError("Both Parents are null, Something wrong happened with swapping. again...");
            }
            // If same Element
            //if (ItemA.ParentInventory != null && ItemB.ParentInventory != null)
            Inventory InventoryA = ItemA.ParentInventory;
            //Inventory InventoryB = ItemB.ParentInventory;
            // If same items, that arn't empty, are clicked
            // if placing item in empty place and right click
            if (CanPlaceSingle(ItemB))
            {
                if ((ItemA.Name == "Empty" && ItemB.Name != "Empty"))
                {
                    Item PlacedItem = ItemB.Clone() as Item;
                    PlacedItem.SetParentInventory(InventoryA);
                    int IndexA = InventoryA.MyItems.IndexOf(ItemA);
                    InventoryA.MyItems[IndexA] = PlacedItem;
                    PlacedItem.SetBoneParentsFrom(ItemA);
                    // Cloning ItemB data into ItemA, so have to reset links to A
                    PlacedItem.SetQuantity(1);
                    ItemA.MyGui.SetItem(PlacedItem);    // refreshes ui after
                }
                else
                {
                    ItemA.IncreaseQuantity(1);
                    ItemA.OnUpdate.Invoke();
                    //Debug.LogError("Place only one item! On Same Item~!");
                }
                ItemB.IncreaseQuantity(-1);
                //Debug.LogError("Place only one item! On Empty Item~! ParentA: " + (ItemA.ParentElement != null).ToString()
                //    + " and b: " + (ItemB.ParentElement != null).ToString());
            }
            // Half the item
            else if (CanHalve(ItemB))
            {
                //Debug.LogError("Halving item: " + ItemA.Name);
                Item PickedUpItem = ItemA.Clone() as Item;
                PickedUpItem.SetParentInventory(null);
                int QuantityGrabbing;
                if (ItemA.GetQuantity() == 1)
                {
                    QuantityGrabbing = 1;
                }
                else
                {
                    QuantityGrabbing = ItemA.GetQuantity() / 2;
                }
                // Cloning ItemB data into ItemA, so have to reset links to A
                PickedUpItem.SetQuantity(QuantityGrabbing);
                ItemB.MyGui.SetItem(PickedUpItem);    // refreshes ui after
                ItemA.IncreaseQuantity(-QuantityGrabbing);
            }
            else
            {
                Debug.LogError("Right Clicking Item, under unknown circumstances. " + Name + ", " + ItemB.Name);
            }
            // if either is null
            //Debug.LogError("After switching items, new parents are A: " + (ItemA.ParentElement != null).ToString() + " And B: " + (ItemB.ParentElement != null).ToString());
        }

        public bool CanPlaceSingle(Item ItemB)
        {
            return ((Name == "Empty" && ItemB.Name != "Empty") || (Name != "Empty" && Name == ItemB.Name));
        }
        public bool CanHalve(Item ItemB) 
        {
            return (Name != "Empty" && GetQuantity() >= 1 && ItemB.Name == "Empty");
        }

        public bool CanStack(Item ItemB) 
        {
            return (Name != "Empty" && Name == ItemB.Name);
        }

        public void StackItem(Item ItemB) 
        {
            // increase itemA instead
            //Debug.LogError("Increase Quantity of ItemA: " + ItemA.Name + " " + ItemA.GetQuantity());
            IncreaseQuantity(ItemB.GetQuantity());
            OnUpdate.Invoke();
            ItemB.SetToEmptyItem();
            if (ParentInventory != null)
            {
                int ItemIndex = ParentInventory.MyItems.IndexOf(this);
                ParentInventory.OnUpdateItem.Invoke(ItemIndex);
            }
        }

        public Item SwapItems(Item ItemB) 
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
                Debug.LogError("Both Parents are null, Something wrong happened with swapping. again...");
            }
            // If same Element
            //if (ItemA.ParentInventory != null && ItemB.ParentInventory != null)
            Inventory InventoryA = ItemA.ParentInventory;
            Inventory InventoryB = ItemB.ParentInventory;
            // Set Bones
            ItemB.SetBoneParentsFrom(ItemA);
            ItemA.SetBoneParentsFrom(ItemB);
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
            ItemA.OnUpdate.Invoke();
            ItemB.OnUpdate.Invoke();
            return ItemA;
        }

        public void SetBoneParentsFrom(Item OldItem) 
        {
            Skeletons.Bone ParentBone = OldItem.ParentElement as Skeletons.Bone;
            if (ParentBone != null)
            {
                ParentBone.MyItem = this;
            }

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