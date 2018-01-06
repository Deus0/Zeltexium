using UnityEngine;
using UnityEngine.UI;
using Zeltex.Items;
using System.Collections.Generic;
using Zeltex.Util;
using Zeltex.Characters;
using Zeltex.Guis;

namespace Zeltex.Guis.Characters
{
    /// <summary>
    /// Main handler for items.
    /// first click - remove texture from itemgui
    /// change texture of mouse following texture
    /// second click - swap items
    /// To Do:
    ///     - Add in custom item slots
    ///     - Crafting Item Slots, 9 for recipe, upon inserting will alter the 10th slot, which will be the craft item
    ///     - When crafted item picked up, it will decrease quantity of those items
    ///     - When Item Quantity is empty, Move Item into another list 'Seen' items
    /// </summary>
    [ExecuteInEditMode]
    public class InventoryGuiHandler : GuiList 
    {
        [Header("Inventory Actions")]
        public EditorAction ActionRefresh = new EditorAction();

        [Header("Inventory")]
        [SerializeField]
        private Inventory MyInventory;
        private bool IsSpellbook = false;
        //public List<Vector2> MyGuiPositions = new List<Vector2>();

        [HideInInspector]
        public MyEventInt MyOnClickEvent = new MyEventInt();
        [HideInInspector]
        private Character MyCharacter;

        private ItemGui ItemPickupItemGui;
        [Header("Skillbar")]
        public GameObject SkillSelectedIcon;
        public ItemsEvent OnSwapItems = new ItemsEvent();

        public Inventory GetInventory() 
        {
            return MyInventory;
        }

        public void SetCharacter(Character NewCharacter)
        {
            MyCharacter = NewCharacter;
            if (MyCharacter && ItemPickupItemGui == null)
            {
                ZelGui MyItemPickupGui = MyCharacter.GetGuis().GetZelGui("ItemPickup");
                if (MyItemPickupGui == null)
                {
                    MyItemPickupGui = MyCharacter.GetGuis().Spawn("ItemPickup");
                }
                ItemPickupItemGui = MyItemPickupGui.GetComponent<ItemGui>();
            }
        }

        public void SetItemPickup(GameObject ItemPickup)
        {
            if (ItemPickupItemGui == null && ItemPickup != null)
            {
                ItemPickupItemGui = ItemPickup.GetComponent<ItemGui>();
            }
        }

        public void SetInventory(Inventory NewInventory) 
        {
            if (MyInventory != null)
            {
                MyInventory.OnUpdateItem.RemoveEvent(RefreshAt);
                MyInventory.OnAddItem.RemoveEvent(RefreshList);
            }
            MyInventory = NewInventory;
            if (MyInventory != null)
            {
                MyInventory.OnUpdateItem.AddEvent(RefreshAt);
                MyInventory.OnAddItem.AddEvent(RefreshList);
            }
            RefreshList();
        }

        protected override void Update()
        {
            base.Update();
            if (ActionRefresh.IsTriggered())
            {
                RefreshList();
            }
        }

		public void OnChangeSelectedItem(int NewSelectedItem)
		{
            if (SkillSelectedIcon && MyGuis.Count != 0) 
			{
				NewSelectedItem = Mathf.Clamp (NewSelectedItem, 0, MyGuis.Count-1);
                if (MyGuis[NewSelectedItem])
                {
                    SkillSelectedIcon.transform.position = MyGuis[NewSelectedItem].transform.position;
                }
			}
		}

        public void ApplyNoFilter()
        {
            if (IsSpellbook)
            {
                IsSpellbook = false;
                RefreshList();
            }
        }

        public void ApplySpellsFilter()
        {
            if (!IsSpellbook)
            {
                IsSpellbook = true;
                RefreshList();
            }
        }

        private void FilterSpells()
        {
            for (int i = 0; i <= MyInventory.MyItems.Count; i++)
            {
                Item MyItem = MyInventory.GetItem(i);
                if (MyItem != null)
                   // if (MyItem.GetQuantity() > 0)
                        if (MyItem.HasTag("spell") || MyItem.Name == "Empty")
                {
                    ItemToGui(MyItem, i);
                }
            }
        }

        private void ItemToGui(Item MyItem, int ItemIndex)
        {
            Add("", new GuiListElementData(), ItemIndex);
            if  (ItemIndex < MyGuis.Count)
            {
                GameObject CreatedItem = MyGuis[ItemIndex].gameObject;
                ItemGui MyItemGui = CreatedItem.GetComponent<ItemGui>();
                MyItemGui.SetItem(MyItem);
                MyItemGui.OnSwapItems.AddEvent(OnSwapItemsFunction);
            }
            else
            {
                Debug.LogWarning("Inventory is larger then gui");
            }
        }

        private void OnSwapItemsFunction(Item ItemA, Item ItemB) 
        {
            OnSwapItems.Invoke(ItemA, ItemB);
        }

        /// <summary>
        /// Handles updating and only update what has changed!
        /// </summary>
        public override void RefreshList() 
		{
			//Debug.Log ("Refreshing Inventory Gui: " + Time.time);
			Clear();
            if (MyInventory != null)
            {
                if (IsSpellbook)
                {
                    FilterSpells();
                }
                else
                {
                    for (int i = 0; i < MyInventory.MyItems.Count; i++)
                    {
                        Item MyItem = MyInventory.GetItem(i);
                        ItemToGui(MyItem, i);
                    }
                }
            }
        }

        /// <summary>
        /// Refreshes one item gui completely.
        /// </summary>
        public void RefreshAt(int MyIndex)
        {
            Item MyItem = MyInventory.GetItem(MyIndex);
            if (MyGuis[MyIndex])
            {
                MyGuis[MyIndex].gameObject.Die();
            }
            MyGuis.RemoveAt(MyIndex);
            ItemToGui(MyItem, MyIndex);
        }
    }
}