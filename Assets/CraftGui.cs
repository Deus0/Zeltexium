using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Guis.Characters;
using Zeltex.Items;

namespace Zeltex.Guis 
{
    /// <summary>
    /// Craft GUI for crafting
    ///     Link up item gui for crafted item
    /// </summary>
    public class CraftGui : MonoBehaviour
    {
        public ItemGui CraftedItemGui;
        public InventoryGuiHandler MyIngredients;
        public Inventory CraftingBench;
        public Recipe CraftingWithRecipe;

        public void Awake() 
        {
            CraftingBench = new Inventory();
            CraftingBench.SetNameOfClone("Crafting");
            for (int i = 0; i < 9; i++)
            {
                CraftingBench.AddRaw(new Item());
            }
            CraftingBench.OnLoad(); // set parents etc
            MyIngredients.SetInventory(CraftingBench);
            SetCraftedItemToNull();
            // Events
            MyIngredients.OnSwapItems.AddEvent(OnSwapItems);
            CraftedItemGui.OnSwapItems.AddEvent(OnPickupCraftingItem);
        }

        public void OnSwapItems(Item ItemA, Item ItemB)
        {
            Inventory IngredientsInventory = MyIngredients.GetInventory();
            if (IngredientsInventory.MyItems.Contains(ItemA) == false)
            {
                //Debug.LogError("Dont check recipes.");
                return;
            }
            int RecipesCount = DataManager.Get().GetSize(DataFolderNames.Recipes);
           // Debug.LogError("Checking recipes for Inventory: " + RecipesCount
            //    + "\n of items: " + ItemA.Name + " And " + ItemB.Name);
            for (int i = 0; i < RecipesCount; i++)
            {
                Recipe MyRecipe = DataManager.Get().GetElement(DataFolderNames.Recipes, i) as Recipe;
                Item CraftedItem = MyRecipe.GetCraftedItem(IngredientsInventory);
                if (CraftedItem != null && CraftedItem.Name != "Empty")
                {
                    CraftedItem.SetParentInventory(CraftingBench);
                    CraftedItemGui.SetItem(CraftedItem);
                    Debug.Log("Crafted new item!");
                    CraftingWithRecipe = MyRecipe;
                    return;
                }
            }
            if (CraftingWithRecipe != null)
            {
                SetCraftedItemToNull();
            }
        }

        private void SetCraftedItemToNull()
        {
            CraftingWithRecipe = null;
            Item CraftedItem = new Item();
            CraftedItem.SetParentInventory(CraftingBench);
            CraftedItemGui.SetItem(CraftedItem);
        }

        public void OnPickupCraftingItem(Item ItemA, Item ItemB)
        {
            if (CraftingWithRecipe != null)
            {
                Debug.Log("Taking awake items from crafting inventory");
                CraftingWithRecipe.RemoveItems(CraftingBench);
            }
            else
            {
                Debug.LogError("No Recipe for Taking awake items from crafting inventory");
            }
        }

        /// <summary>
        /// If closing gui, drop all the items on ground
        /// </summary>
        public void DropAllItems() 
        {

        }
    }
}
