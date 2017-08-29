using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex;

//namespace Zeltex
//{
namespace Zeltex.Items
{
    /// <summary>
    /// Each recipe can be used to create new items
    /// </summary>
    public class Recipe : Element
    {
        //public string Name = "Recipe";
        // first example, 2x2
        public List<Item> IngredientItems;  // the items needed to begin crafting
        public List<Item> CraftedItems;     // the items created by the crafting process


        #region Files
        /// <summary>
        /// Get the data to save a recipe
        /// </summary>
        public override string GetScript()
        {
            List<string> Data = new List<string>();
            Data.Add("/Recipe " + Name);

            Data.Add("/BeginIngredients");
            for (int i = 0; i < IngredientItems.Count; i++)
            {
                Data.Add("/Item " + IngredientItems[i].Name);
                Data.Add("/Quantity " + IngredientItems[i].Name);
            }
            Data.Add("/EndIngredients");

            Data.Add("/BeginCraftedItems");
            for (int i = 0; i < CraftedItems.Count; i++)
            {
                Data.Add("/Item " + CraftedItems[i].Name);
                Data.Add("/Quantity " + CraftedItems[i].Name);
            }
            Data.Add("/EndCraftedItems");
        return Zeltex.Util.FileUtil.ConvertToSingle(Data);
        }

        /// <summary>
        /// Load the recipe!
        /// </summary>
        public override void RunScript(string Script)
        {

        }
        #endregion
    }

}
//}