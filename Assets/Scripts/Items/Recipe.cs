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
        public string Crafted = "";     // the items created by the crafting process

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
                foreach (KeyValuePair<Int3, string> MyValuePair in Ingredients)
                {
                    int InventoryIndex = MyValuePair.Key.x + MyValuePair.Key.y * (InventorySize.x);
                    if (MyInventory.GetItem(InventoryIndex).Name != MyValuePair.Value)
                    {
                        //Debug.LogError("Inventory doesn't match recipe: " + MyInventory.Name + " at " + MyValuePair.Value + " : " 
                        //    + MyInventory.GetItem(InventoryIndex).Name + "\n" + InventoryIndex + " - at position: " + MyValuePair.Key.ToString());
                        return "";
                    }
                }
                return Crafted; // returns
            }
            else
            {
                Debug.LogError("Not checking! Size Invalid for recipe " + Name + " at " + MySize.ToString());
            }
            return "";
        }

        public Item GetCraftedItem(Inventory MyInventory)
        {
            string MyCrafted = GetCraftedItemName(MyInventory);
            if (MyCrafted != "")
            {
                return DataManager.Get().GetElement(DataFolderNames.Items, MyCrafted).Clone() as Item;
            }
            return new Item();
        }
    }

}