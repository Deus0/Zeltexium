using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex;
using Newtonsoft.Json;

namespace Zeltex.Items
{
    /// <summary>
    /// Used for Recipes
    /// </summary>
    [System.Serializable()]
    public class IntDictionary : SerializableDictionaryBase<Int3, string> 
    {
        [JsonIgnore]
        public bool IsEditing;
    }

    [System.Serializable()]
    public class DoubleIntDictionary : SerializableDictionaryBase<Int3, int> 
    {
        [JsonIgnore]
        public bool IsEditing;
    }

    /// <summary>
    /// Each recipe can be used to create new items
    /// </summary>
    [System.Serializable]
    public class Recipe : Element
    {
        //public string Name = "Recipe";
        // first example, 2x2

        [JsonProperty]
        public IntDictionary Ingredients = new IntDictionary();  // the items needed to begin crafting
        [JsonProperty]
        public DoubleIntDictionary IngredientsQuantity = new DoubleIntDictionary();  // the items needed to begin crafting
        [JsonProperty]
        public string Crafted = "";     // the items created by the crafting process
        // Temporary used in crafting data
        [JsonIgnore]
        private List<int> ItemSlots = new List<int>();
        [JsonIgnore]
        private List<int> ItemQuantities = new List<int>();
        [JsonIgnore]
        private List<Int3> ItemPositions = new List<Int3>();
        [JsonIgnore]
        private int QuantityCanMake = 0;

        /// <summary>
        /// Gets the max size of a recipe
        /// Normally 2x2 or 3x3
        /// </summary>
        public Int3 GetSize() 
        {
            Int3 MySize = Int3.Zero();
            foreach (KeyValuePair<Int3, string> MyValuePair in Ingredients)
            {
                if (MyValuePair.Key.x > MySize.x)
                {
                    MySize.x = MyValuePair.Key.x;
                }
                if (MyValuePair.Key.y > MySize.y)
                {
                    MySize.y = MyValuePair.Key.y;
                }
                if (MyValuePair.Key.z > MySize.z)
                {
                    MySize.z = MyValuePair.Key.z;
                }
            }
            MySize.x++;
            MySize.y++;
            return MySize;
        }

        /// <summary>
        /// Return the string of a crafted item
        /// </summary>
        public string GetCraftedItemName(Inventory MyInventory)
        {
            Int3 MySize = GetSize();
            Int3 InventorySize = new Int3();
            if (MyInventory.GetSize() == 9)
            {
                InventorySize.Set(3, 3, 0);
            }
            else if (MyInventory.GetSize() == 4)
            {
                InventorySize.Set(2, 2, 0);
            }
            if (MyInventory.GetSize() == 9 && (MySize == (new Int3(3, 3, 0)) || MySize == (new Int3(2, 2, 0))))
            {
                // for all possible combinations of the recipe fitting inside the crafting bench
                // for example 2x2 will fit in 3x3 -> 4 times
                for (int i = 0; i < InventorySize.x - MySize.x + 1; i++)
                {
                    for (int j = 0; j < InventorySize.y - MySize.y + 1; j++)
                    {
                        bool IsCrafted = true;
                        ItemSlots.Clear();
                        ItemQuantities.Clear();
                        ItemPositions.Clear();
                        //Debug.LogError("Clearing ItemSlots1.");
                        Item MyItem;
                        int InventoryIndex;
                        int QuantityRequired;
                        foreach (KeyValuePair<Int3, string> MyValuePair in Ingredients)
                        {
                            InventoryIndex = (i + MyValuePair.Key.x) + (MyValuePair.Key.y + j) * (InventorySize.x);  // gets the index in the inventory
                            MyItem = MyInventory.GetItem(InventoryIndex);
                            if (IngredientsQuantity.ContainsKey(MyValuePair.Key))
                            {
                                QuantityRequired = IngredientsQuantity[MyValuePair.Key];
                            }
                            else
                            {
                                QuantityRequired = 1;
                            }
                            if (MyItem.Name != MyValuePair.Value || MyItem.GetQuantity() < QuantityRequired)
                            {
                                //Debug.LogError("Inventory doesn't match recipe: " + MyInventory.Name + " at " + MyValuePair.Value + " : " 
                                //    + MyInventory.GetItem(InventoryIndex).Name + "\n" + InventoryIndex + " - at position: " + MyValuePair.Key.ToString());
                                IsCrafted = false;
                                break;
                            }
                            else
                            {
                                ItemSlots.Add(InventoryIndex);
                                ItemQuantities.Add(Mathf.FloorToInt(MyItem.GetQuantity() / QuantityRequired));
                                ItemPositions.Add(MyValuePair.Key);
                            }
                        }
                        if (IsCrafted)
                        {
                            QuantityCanMake = 1000;
                            for (int z = 0; z < ItemQuantities.Count; z++) 
                            {
                                if (ItemQuantities[z] < QuantityCanMake) 
                                {
                                    QuantityCanMake = ItemQuantities[z];
                                }
                            }
                            if (QuantityCanMake == 1000)
                            {
                                QuantityCanMake = 1;
                                Debug.LogError("Error getting item quantities");
                            }
                            //Debug.LogError("Created " + ItemSlots.Count + " Item Slots.");
                            return Crafted; // returns
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Not checking! Size Invalid for recipe " + Name + " at " + MySize.ToString());
            }
            ItemSlots.Clear();
            //Debug.LogError("Clearing ItemSlots2.");
            return "";
        }

        public Item GetCraftedItem(Inventory MyInventory)
        {
            string MyCrafted = GetCraftedItemName(MyInventory);
            if (MyCrafted != "")
            {
                Item CraftedItem = DataManager.Get().GetElement(DataFolderNames.Items, MyCrafted).Clone() as Item;
                CraftedItem.SetQuantity(QuantityCanMake);
                return CraftedItem;
            }
            return new Item();
        }

        /// <summary>
        /// Take away recipe items from crafting inventory!
        /// </summary>
        public void RemoveItems(Inventory MyCraftingBench, int QuantityPickup) 
        {
            //Debug.Log(Name + " is Taking away: " + ItemSlots.Count + " Items.");
            for (int i = 0; i < ItemSlots.Count; i++)
            {
                //Debug.LogError("Taking away slot: " + ItemSlots[i]);
                int QuantityToDecrease;
                if (IngredientsQuantity.ContainsKey(ItemPositions[i]))
                {
                    QuantityToDecrease = IngredientsQuantity[ItemPositions[i]];
                }
                else
                {
                    QuantityToDecrease = 1;
                }
                if (QuantityPickup == -1)
                {
                    QuantityPickup = QuantityCanMake;
                }
                MyCraftingBench.Decrease(ItemSlots[i], QuantityToDecrease * QuantityPickup);
            }
        }
    }

}