using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Util;
using Zeltex.Items;
using UnityEngine.UI;
using Zeltex.Guis.Characters;

//namespace Zeltex
//{
namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// Handles recipe data
    /// </summary>
    public class RecipeMaker : ElementMakerGui
    {
        //List<Recipe> MyRecipes = new List<Recipe>();
        #region Variables
        [Header("RecipeMaker")]
        public InventoryGuiHandler IngredientsInventoryGui;
        public InventoryGuiHandler CraftedItemsInventoryGui;
        #endregion

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                // Add items onto recipe
                // refresh uis
            }
        }

        #region Files

        /// <summary>
        /// Set file paths!
        /// </summary>
        protected override void SetFilePaths()
        {
            DataManagerFolder = "Recipes";
        }

        public Recipe GetSelectedRecipe()
        {
            return DataManager.Get().GetElement(DataManagerFolder, GetSelectedIndex()) as Recipe;
        }

        #endregion

        #region DataToUI

        /// <summary>
        /// Used for generically updating input buttons
        /// </summary>
        public override void UseInput(InputField MyInputField)
        {
            base.UseInput(MyInputField);
            if (MyInputField.name == "NameInput")
            {
                MyInputField.text = Rename(MyInputField.text);
            }
        }
        #endregion

        #region UIActions

        /// <summary>
        /// Load Gui with Input
        /// Only gets called if index is updated
        /// </summary>
        public override void OnUpdatedIndex(int NewIndex)
        {
            base.OnUpdatedIndex(NewIndex);
            // fill out inventoryGuiHandler here

        }
        #endregion
    }
}
//}