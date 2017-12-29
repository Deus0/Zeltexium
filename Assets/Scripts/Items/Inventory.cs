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
        [JsonProperty]
        public List<Item> MyItems = new List<Item>();
        [Header("Options")]
        [SerializeField]
        private int MaxItems = 0;
        [SerializeField]
        private float Value;	// money player has
		[SerializeField]
        private bool CanBuyAllItems;	// for player
		[SerializeField]
        private bool CanSellAllItems;  // for merchants
        private string EmptyItemName = "Empty";
        private bool IsFillWithEmptySlots = true;
        //[Header("Events")]
        [HideInInspector]
        public UnityEvent OnAddItem = new UnityEvent();
        [HideInInspector]
        public MyEventInt OnUpdateItem = new MyEventInt();
        [HideInInspector]
        public EventObjectString OnPickupItem = new EventObjectString();
        [HideInInspector]
        public EventObjectString OnExchangeItem = new EventObjectString();
        [HideInInspector]
        public UnityEvent OnExchangeCurrency;
        [HideInInspector]
        public MyEventInt OnItemUpdate;
        [HideInInspector]
        public UnityEvent OnLoadEvent;
        #endregion

        #region Initiation

        public override void OnLoad()
        {
            base.OnLoad();
            for (int i = 0; i < MyItems.Count; i++)
            {
                MyItems[i].ParentElement = this;
                MyItems[i].OnLoad();    // any sub stats will be set as well
            }
        }
        /// <summary>
        /// Initiator part!
        /// </summary>
        public Inventory()
        {
            //Debug.Log("StartUp [" + Time.realtimeSinceStartup + "] Setting Inventory Items in " + Name);
            if (MyItems.Count == 0)
            {
                Clear();    // add empty items
            }
        }
        #endregion

        #region ItemObject

        /// <summary>
        /// 
        /// </summary>
        public void PickupItem(GameObject MyObject)
        {
            ItemObject MyItemObject = MyObject.GetComponent<ItemObject>();
            if (MyItemObject)
            {
                //Debug.LogError("Adding " + MyItemObject.name + " to inventory");
                Add(MyItemObject.GetItem());
                OnPickupItem.Invoke(MyObject, "Picked Up");
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

        /// <summary>
        /// Returns true if removes item
        /// </summary>
        public Item SwitchItems(int ItemIndex, Item ItemB)
        {
            Item MyItem1 = MyItems[ItemIndex].Clone<Item>();
            Item MyItem2 = ItemB.Clone<Item>();
            MyItems[ItemIndex] = MyItem2;
            ItemB = MyItem1;
            if (OnAddItem != null)
            {
                OnAddItem.Invoke();    // refresh gui
            }
            if (OnUpdateItem != null)
            {
                OnUpdateItem.Invoke(ItemIndex);
            }
            if (OnItemUpdate != null)
            {
                OnItemUpdate.Invoke(ItemIndex);
            }
            return ItemB;
        }

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
            if (CanBuyAllItems)
                return true;

            Item MyItemFound = GetItem(MyItem.Name);
            if (MyItemFound == null)
            {
                return false;
            }
            return MyItemFound.IsBuyable();
        }

        public bool CanSell(Item MyItem)
        {
            if (CanSellAllItems)
                return true;
            Item MyItemFound = GetItem(MyItem.Name);
            if (MyItemFound == null)
                return false;
            return MyItemFound.IsSellable();
        }

        public float GetAverageValue(Item Item1, Item Item2)
        {
            float AverageValue = 0f;
            if (Item2 != null && Item1 != null)
                AverageValue = (Item1.GetMidValue() + Item2.GetMidValue()) / 2f;
            else if (Item1 != null && Item2 == null)
                AverageValue = Item1.GetMidValue();
            else if (Item1 == null && Item2 != null)
                AverageValue = Item2.GetMidValue();
            return AverageValue;
        }

        public void IncreaseValue(float AdditionValue)
        {
            Value += AdditionValue;
            if (OnExchangeCurrency != null)
                OnExchangeCurrency.Invoke();
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
            //Debug.LogError(InventoryA.Name + " is giving value " + ExchangeValue + " to " + MyCharacterTaker.name);
            if (InventoryTake != null && InventoryGive != null)
            {
                if (InventoryGive.Value >= ExchangeValue)
                {
                    InventoryGive.IncreaseValue(-ExchangeValue);
                    InventoryTake.IncreaseValue(ExchangeValue);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// If inventories are buying/selling
        ///     Assume A(this) is buying off B(OtherInventory)
        /// </summary>
        public bool ExchangeItems(Inventory InventoryGive, Inventory InventoryTake, string ItemName, int ItemQuantity, bool IsValueExchanged)
        {
            Debug.LogError(InventoryGive.Name + " is giving value " + ItemName + " to " + InventoryTake.Name);
            // get item by name from each inventory :3
            Item MyItem = InventoryGive.GetItem(ItemName);
            Item MyItem2 = InventoryTake.GetItem(ItemName);
            if (MyItem == null)
            {
                Debug.LogError(InventoryGive.Name + " Does not have: " + ItemName + " to give to " + InventoryTake.Name);
                return false;
            }
            else if (MyItem.GetQuantity() < ItemQuantity)
            {
                Debug.Log(InventoryGive.Name + "has " + ItemName + " to give to " + InventoryTake.Name + " but not enough quantity.");
                return false;
            }
            // haggling step here!
            float BuyValue = InventoryGive.GetAverageValue(MyItem, MyItem2) * ItemQuantity;
            if (InventoryTake.Value < BuyValue)
                return false;
            // exchange currency
            if (IsValueExchanged)
            {
                bool IsExchangeValue = GiveValue(InventoryGive, InventoryTake, BuyValue);
                if (!IsExchangeValue)
                    return false;
            }
            // the item switching here!
            Debug.LogError(InventoryGive.Name + " giving " + ItemName + " to " + InventoryTake.Name);
            bool IsRemoveItem = InventoryGive.Remove(MyItem, ItemQuantity);
            if (IsRemoveItem)
            {
                InventoryTake.Add(MyItem, ItemQuantity);
            }
            return IsRemoveItem;
        }
        #endregion

        #region Adding

        /// <summary>
        /// Is there an empty slot free?
        /// </summary>
        public bool CanAddItem()
        {
            if (MyItems.Count < MaxItems)
            {
                return true;
            }
            else
            {
                return false;
            }
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
		public void Add(Item NewItem) 
		{
			Add(NewItem, -1);
		}

        /// <summary>
        /// The main function to add an item
        /// </summary>
        public void Add(Item NewItem, int Quantity)
		{
            if (NewItem == null)
            {
                Debug.LogError("Trying to add null item.");
                return;
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
                        return;
                    }
                }
                /*if (MySkillbar != null)
                {
                    for (int i = 0; i < MySkillbar.MyItems.Count; i++)
                    {
                        if (MySkillbar.MyItems[i].Name == NewItem.Name) // Empty Does not stack
                        {
                            //Debug.LogError ("Stacking Item " + NewItem.Name);
                            MySkillbar.MyItems[i].IncreaseQuantity(Quantity);
                            MySkillbar.OnAddItem.Invoke();
                            return;
                        }
                    }
                }*/
            }
            bool DoesHaveEmpty = HasEmptyItem();
            // if no item of type, add to list
            if (!CanAddItem() && !DoesHaveEmpty)
                return; // cannot add 
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
			Item NewItem2 = NewItem.Clone<Item>();
			if (Quantity != -1)
            {
                NewItem2.SetQuantity(Quantity);
            }

			MyItems.Insert (InsertIndex, NewItem2);
            OnAddItem.Invoke();
            OnUpdateItem.Invoke(InsertIndex);
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
            /*for (int i = 0; i < MyItems.Count; i++)
            {
                if (MyItems[i].Name == EmptyItemName)
                {
                    return true;
                }
            }
            return false;*/
            return true;
        }
        #endregion

        #region File

        /// <summary>
        /// Adds an item to inventory
        /// </summary>
        /*public void AddItemScript(List<string> MyData)
        {
            if (MyData.Count > 2 && MyData[0].Contains("/item") && MyData[MyData.Count - 1] == "/EndItem")
            {
                Add(new Item(MyData));
            }
        }
        /// <summary>
        /// Runs a list of scripts and creates items for them
        /// </summary>
        public void RunScript(List<string> MyData)
        {
            Clear();    // add empty items
                        //Debug.LogError ("Running Inventory Script");
                        //Clear ();
                        // now go through data and set items
            for (int i = 0; i < MyData.Count; i++)
            {
                if (MyData[i].Contains("/item"))
                {
                    for (int j = i + 1; j < MyData.Count; j++)
                    {
                        if (MyData[j] == "/EndItem")
                        {
                            List<string> MyItemData = MyData.GetRange(i, j - i + 1);
                            //Debug.LogError(name + " - New Item at " + i + "\n" + FileUtil.ConvertToSingle(MyItemData));
                            Add(new Item(MyItemData));
                            i = j;
                            break;
                        }
                    }
                }
            }
            Debug.Log("Loaded Inventory");
            OnLoadEvent.Invoke();
        }

        /// <summary>
        /// Called when another script loads inventory
        /// </summary>
        public void OnRunScript()
        {
            OnLoadEvent.Invoke();
        }

        /// <summary>
        /// Gets a list of the items scripts
        /// </summary>
        /// <returns></returns>
		public List<string> GetScriptList()
        {
            List<string> MyScript = new List<string>();
            for (int i = 0; i < MyItems.Count; i++)
            {
                MyScript.AddRange(MyItems[i].GetScript2());
            }
            return MyScript;
        }*/
        #endregion
    }
}

/*public float GetValue()
{
    return Value;
}
public string GetValueText()
{
    return "#" + Value;
}*/

/*public void RunScript(string[] MyData) 
{
    List<string> MyDataList = new List<string> ();
    for (int i = 0; i < MyData.Length; i++)
        MyDataList.Add (MyData [i]);
    RunScript (MyDataList);
}*/

/*void OnGUI()
{
    if (DebugMode || DebugCommands || DebugTextures)
    {
        GUILayout.Label("Inventory of [" + gameObject.name + "]");
        GUILayout.Label("Cash [" + Value + "]");
        GUILayout.Label("Number of Items [" + MyItems.Count + "]");
        for (int i = 0; i < MyItems.Count; i++)
        {
            GUILayout.Label("\t" + i + " - Item[" + MyItems[i].Name + "]");
            if (DebugCommands)
            {
                GUILayout.Label("\t" + " - Commands [" + MyItems[i].GetCommands() + "]");
            }
            if (DebugTextures)
            {
                if (MyItems[i].GetTexture() != null)
                    GUILayout.Label("\t" + " - Texture [" + MyItems[i].GetTexture().name + "]");
                else
                    GUILayout.Label("\t" + " - TextureName [" + MyItems[i].TextureName + "]");
            }
        }
    }
}*/
