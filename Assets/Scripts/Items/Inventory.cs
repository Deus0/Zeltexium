using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Zeltex.Util;
using Zeltex.Skeletons;   // for drop transform
using Newtonsoft.Json;

namespace Zeltex.Items 
{
    /// <summary>
    /// Used to hold items.
    /// Attach to same object as the character or treasure chest.
    /// To Do:
    ///     - Reimplement trading utility
    ///     - Trading Stats - increase level when trading
    ///     - maybe put this in a normal class, and have components use it?
    /// </summary>
    [System.Serializable]
    public class Inventory : Element 
	{
        #region Variables
        [ JsonIgnore]
        private static string EmptyItemName = "Empty";
        [JsonProperty]
        public List<Item> MyItems = new List<Item>();
        [HideInInspector, JsonIgnore]
        private bool IsFillWithEmptySlots = false;
        //[Header("Events")]
        [HideInInspector, JsonIgnore]
        public UnityEvent OnAddItem = new UnityEvent();
        [HideInInspector, JsonIgnore]
        public MyEventInt OnUpdateItem = new MyEventInt();
        [HideInInspector, JsonIgnore]
        public EventObjectString OnPickupItem = new EventObjectString();
        [HideInInspector, JsonIgnore]
        public EventObjectString OnExchangeItem = new EventObjectString();
        [HideInInspector, JsonIgnore]
        public UnityEvent OnExchangeCurrency = new UnityEvent();
        //[HideInInspector, JsonIgnore]
        //public MyEventInt OnItemUpdate = new MyEventInt();
        [HideInInspector, JsonIgnore]
        public UnityEvent OnLoadEvent = new UnityEvent();
        #endregion

        #region Initiation

        public override void OnLoad()
        {
            base.OnLoad();
            for (int i = 0; i < MyItems.Count; i++)
            {
                MyItems[i].SetParentInventory(this);
                MyItems[i].OnLoad();    // any sub stats will be set as well
            }
        }

        /// <summary>
        /// Initiator part!
        /// </summary>
        public Inventory()
        {
        }
        #endregion

        #region ItemHandler

        /// <summary>
        /// 
        /// </summary>
        public void PickupItem(ItemHandler MyItemHandler)
        {
            if (MyItemHandler)
            {
                //Debug.LogError("Adding " + MyItemHandler.name + " to inventory");
                Add(MyItemHandler.GetItem());
                OnPickupItem.Invoke(MyItemHandler.gameObject, "Picked Up");
            }
        }

        /// <summary>
        /// Drops items from inventory
        /// </summary>
        public void DropAllItems() 
		{
			//DropAllItems(transform);
		}

		public void DropAllItems(Transform DropTransform) 
		{
			//Debug.LogError ("Inside 'DropAllItems' with [" + DropTransform.name + "]");
			//StartCoroutine (DropItemsTimed(DropTransform));
		}

		/*IEnumerator DropItemsTimed(Transform DropTransform)
		{

			for (int i = 0; i < MyItems.Count; i++) 
			{
				yield return new WaitForSeconds (1f);
				DropItem (i, DropTransform);
			}
		}*/

        public void DropItem(int ItemIndex, Transform DropTransform)
        {
            DropItem(GetItem(ItemIndex), DropTransform);
        }

        public void DropItem(Item MyItem, Transform DropTransform) 
		{
            /*if (DropTransform == null && transform.FindChild("Body") != null)
            {
                DropTransform = MyCharacter.FindChild("Body").GetComponent<Skeleton>().MyCameraBone;
            }*/
            //Debug.LogError ("Droping item from [" + DropTransform.name + "]");
            if (MyItem.Name == EmptyItemName)
            {
                Debug.LogError("Cannot drop Empty Item");
                return; // don't drop empty items
            }
			if (MyItem.GetQuantity() > 0) 
			{
                ItemManager.Get().SpawnItem(DropTransform, MyItem);
                Remove(MyItem);
                OnAddItem.Invoke();
            }
        }
        #endregion

        #region Data
        /// <summary>
        ///  refreshes all items
        /// </summary>
        public void Refresh()
        {
            OnAddItem.Invoke();
        }
        /// <summary>
        /// Get size of items
        /// </summary>
        public int GetSize()
        {
            return MyItems.Count;
        }
        /// <summary>
        /// Clear all the items
        /// </summary>
        public void Clear()
        {
            MyItems.Clear();
            /*if (IsFillWithEmptySlots)
            {
                for (int i = 0; i < MaxItems; i++)
                {
                    MyItems.Add(new Item());  // add an empty item
                }
            }*/
        }

        public int GetItemIndex(string ItemName)
        {
            for (int i = 0; i < MyItems.Count; i++)
            {
                if (ItemName == MyItems[i].Name)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets an item of a certain index
        /// </summary>
        public Item GetItem(int Index)
        {
            if (Index >= 0 && Index < MyItems.Count)
            {
                return MyItems[Index];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets an item in the list by a name
        /// </summary>
		public Item GetItem(string name)
        {
            // Debug.Log("Searching for item: " + name + " -length: " + name.Length);
            for (int i = 0; i < MyItems.Count; i++)
            {
                if (MyItems[i].Name != EmptyItemName)
                {
                    //Debug.Log(i + " Checking Item: " + MyItems[i].Name + " -length: " + MyItems[i].Name.Length);
                    if (name == ScriptUtil.RemoveWhiteSpace(MyItems[i].Name))
                    {
                        return MyItems[i];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Does inventory contain a certain item
        /// </summary>
        public bool ContainsItem(string name)
        {
            for (int i = 0; i < MyItems.Count; i++)
            {
                if (name == MyItems[i].Name)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get item quantity, 0 if doesn't exist
        /// </summary>
        public int GetItemQuantity(string ItemName)
        {
            for (int i = 0; i < MyItems.Count; i++)
            {
                if (MyItems[i].Name == ItemName)
                {
                    return MyItems[i].GetQuantity();
                }
                else {
                    //Debug.LogError(ItemName + " != " + MyItems[i].Name);
                    //Debug.LogError(ItemName.Length + " != " + MyItems[i].Name.Length);
                }
            }

            return 0;
        }

        public bool Remove(int ItemIndex)
        {
            if (ItemIndex >= 0 && ItemIndex < MyItems.Count)
            {
                MyItems.RemoveAt(ItemIndex);
                OnModified();
                Refresh();
                return true;
            }
            return false;
        }

        public void Remove(string ItemName)
        {
            for (int i = 0; i < MyItems.Count; i++)
            {
                if (MyItems[i].Name == ItemName)
                {
                    MyItems[i] = new Item();
                    MyItems[i].SetParentInventory(this);
                    OnUpdateItem.Invoke(i);
                    return;
                }
            }
        }

        public bool Remove(Item MyItem)
        {
            return Remove(MyItem, MyItem.GetQuantity());
        }

        /// <summary>
        /// Remove an item from inventory
        /// </summary>
        public bool Remove(Item MyItem, int Quantity)
        {
            for (int i = 0; i < MyItems.Count; i++)
            {
                if (MyItem.Name == MyItems[i].Name)
                {
                    if (MyItems[i].GetQuantity() >= Quantity)
                    {
                        MyItems[i].IncreaseQuantity(-Quantity);
                        Refresh();
                        return true;
                    }
                    else
                    {
                        Debug.LogError("Item has insufficient quantity");
                        return false;
                    }
                }
            }
            return false;
        }
        
        /// <summary>
        /// Removes an item from inventory
        /// </summary>
        public void DeleteItem(Item MyItem)
        {
            int ItemIndex = -1;
            for (int i = 0; i < MyItems.Count; i++)
            {
                if (MyItem == MyItems[i])
                {
                    ItemIndex = i;
                    i = MyItems.Count;
                }
            }
            if (ItemIndex != -1)
            {
                MyItems.RemoveAt(ItemIndex);
            }
        }
        #endregion

        #region Trading


        public void SwitchItems(int ItemIndex, int ItemIndex2)
        {
            Item MyItem = MyItems[ItemIndex].Clone<Item>();
            Item MyItem2 = MyItems[ItemIndex2].Clone<Item>();
            MyItems[ItemIndex] = MyItem2;
            MyItems[ItemIndex2] = MyItem;
            if (OnAddItem != null)
            {
                OnAddItem.Invoke();    // refresh gui
            }
        }

        /// <summary>
        /// Trade Mechanics - maybe seperate from inventory
        /// </summary>
        public bool CanBuy(Item MyItem)
        {
            return false;
        }

        public bool CanSell(Item MyItem)
        {
            return false;
        }

        public float GetAverageValue(Item Item1, Item Item2)
        {
            return 0;
        }

        public void IncreaseValue(float AdditionValue)
        {
        }

        public void GiveItem(Inventory OtherInventory2, string ItemName)
        {
            GiveItem(OtherInventory2, ItemName, 1);
        }

        public bool GiveItem(Inventory OtherInventory, string ItemName, int Quantity)
        {
            if (OtherInventory != null)
            {
                if (ItemName == "Money")
                {
                    GiveValue(this, OtherInventory, Quantity);
                    return true;
                }
                //OnExchangeItem.Invoke(OtherInventory2, "Gave " + ItemName);
                return ExchangeItems(this, OtherInventory, ItemName, Quantity, false);
            }
            else
            {
                Debug.LogError(Name + "trying to give item to a null pointer");
                return false;
            }
        }

        public bool BuyItem(Inventory OtherInventory, string ItemName, int BuyQuantity)
        {
            return ExchangeItems(OtherInventory, this, ItemName, BuyQuantity, true);
        }

        public bool SellItem(Inventory OtherInventory, string ItemName, int BuyQuantity)
        {
            return ExchangeItems(this, OtherInventory, ItemName, BuyQuantity, true);
        }

        public static bool GiveValue(Inventory InventoryGive, Inventory InventoryTake, float ExchangeValue)
        {
            return false;
        }

        /// <summary>
        /// If inventories are buying/selling
        ///     Assume A(this) is buying off B(OtherInventory)
        /// </summary>
        public bool ExchangeItems(Inventory InventoryGive, Inventory InventoryTake, string ItemName, int ItemQuantity, bool IsValueExchanged)
        {
            return false;
        }
        #endregion

        #region Adding

        public void AddRaw(Item NewItem)
        {
            MyItems.Add(NewItem);
        }

        /// <summary>
        /// Is there an empty slot free?
        /// </summary>
        public bool CanAddItem()
        {
            return false;
        }

        public void IncreaseQuantity(int ItemIndex, int Addition)
        {
            Item MyItem = GetItem(ItemIndex);
            if (MyItem != null)
            {
                if (MyItem.IncreaseQuantity(Addition))  // if does increase
                {
                    if (MyItem.GetQuantity() == 0)
                    {
                        MyItems[ItemIndex] = new Item();    // empty
                        MyItems[ItemIndex].ParentElement = this;
                        OnAddItem.Invoke();
                    }
                    OnUpdateItem.Invoke(ItemIndex);
                }
            }
        }

        /// <summary>
        /// Creates a new item in inventory, only if there is an empty slot
        /// </summary>
		public void Add()
		{
            if (!CanAddItem())
            {
                return;
            }
			Item NewItem = new Item();
			NewItem.Name += " " + MyItems.Count;
			MyItems.Add(NewItem);
            NewItem.ParentElement = this;
        }

		// Normal list handling
        public bool Add(Item NewItem) 
		{
			return Add(NewItem, -1);
		}

        /// <summary>
        /// The main function to add an item
        /// When decreasing quantity of item, clone it
        /// </summary>
        public bool Add(Item NewItem, int Quantity)
		{
            if (NewItem == null)
            {
                Debug.LogError("Trying to add null item.");
                return false;
            }
			if (Quantity == -1)
            {
                Quantity = NewItem.GetQuantity();
            }
			// first check to stack item
			if (NewItem != null && NewItem.Name != EmptyItemName)
            {
                for (int i = 0; i < MyItems.Count; i++)
                {
                    if (MyItems[i].Name == NewItem.Name) // Empty Does not stack
                    {
                        //Debug.LogError ("Stacking Item " + NewItem.Name);
                        MyItems[i].IncreaseQuantity(Quantity);
                        OnAddItem.Invoke();
                        return true;
                    }
                }
            }
            bool DoesHaveEmpty = HasEmptyItem();
            // if no item of type, add to list
            if (!CanAddItem() && !DoesHaveEmpty)
            {
                return false;   // cannot add as no slots
            }
            // remove an empty to add the item
            int InsertIndex = MyItems.Count;
            if (DoesHaveEmpty)
            {
                for (int i = 0; i < MyItems.Count; i++)
                {
                    if (MyItems[i].Name == EmptyItemName)
                    {
                        MyItems.RemoveAt(i);
                        InsertIndex = i;
                        break;
                    }
                }
            }
			//Item NewItem2 = NewItem.Clone<Item>();
			if (Quantity != -1)
            {
                NewItem.SetQuantity(Quantity);
            }

            MyItems.Insert(InsertIndex, NewItem);
            NewItem.SetParentInventory(this);
            NewItem.OnUpdate.Invoke();
            OnAddItem.Invoke();
            OnUpdateItem.Invoke(InsertIndex);
            return true;
        }

        bool HasItem(string ItemName)
        {
            Item MyItem = GetItem(ItemName);
            if (MyItem != null)
                return true;
            return false;
        }

        /// <summary>
        /// Checks to see if there are any empty spots left
        /// </summary>
        private bool HasEmptyItem()
        {
            return true;
        }
        #endregion
    }
}