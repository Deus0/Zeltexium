using UnityEngine;
using UnityEngine.UI;
using Zeltex.Items;
using System.Collections.Generic;
using Zeltex.Util;
using Zeltex.Characters;

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
        [Header("Actions")]
        public EditorAction ActionRefresh = new EditorAction();
        [Header("Options")]
        public bool IsSpellbook;    // used to filter spells
        public List<Vector2> MyGuiPositions = new List<Vector2>();
        [Header("Events")]
		public MyEventInt MyOnClickEvent = new MyEventInt();
		[Header("References")]
		public Character MyCharacter;
		public Inventory MyInventory;
        public AudioSource MyAudioSource;
        public GameObject MyDraggedItem;    // item to be dragged
        //private Text MyValueText;

        // updates selected item icon
        public GameObject HighlightedIcon;
        [Header("Audio")]
		public AudioClip OnBuyItemSound;
		public AudioClip OnNonBuyItemSound;
		public AudioClip OnNotSellingSound;
		public AudioClip OnSellItemSound;

        [Header("Actions")]
        public EditorAction RefreshInventory;

        protected override void Update()
        {
            base.Update();
            if (RefreshInventory.IsTriggered())
            {
                MyInventory = MyCharacter.GetData().Skillbar;
            }
            if (ActionRefresh.IsTriggered())
            {
                RefreshList();
            }
        }

        /// <summary>
        /// Returns a position of a cell depending on an index
        /// </summary>
        public override Vector3 GetCellPosition(int Index)
        {
            if (Index >= 0 && Index < MyGuiPositions.Count)
            {
                return MyGuiPositions[Index] + 
                    new Vector2(gameObject.GetComponent<RectTransform>().GetWidth(), -gameObject.GetComponent<RectTransform>().GetHeight())/2f
                    + new Vector2(-30, -60+80 + 30);
            }
            else
            {
                return base.GetCellPosition(Index);
            }
        }

		public override void Select(int NewSelectedIndex)
		{
            if (NewSelectedIndex >= 0 && NewSelectedIndex < MyGuis.Count && MyDraggedItem != null && MyGuis[NewSelectedIndex])
            {
                // Get the 2 item objects
                GameObject SelectedItem = MyGuis[NewSelectedIndex].gameObject;
                // Get the images
                RawImage ClickedItemImage = MyGuis[NewSelectedIndex].transform.GetChild(0).GetComponent<RawImage>();
                RawImage MouseItemImage = MyDraggedItem.GetComponent<RawImage>();
                // Get the item dragged tool tip
                //Debug.Log("Clicked item - with dragitem: " + MyDraggedItem.name);
                GuiListElementData DraggedItemTooltip = MyDraggedItem.GetComponent<GuiListElement>().MyGuiListElementData;    // dragging items data
                // Get the empty states
                if (MyInventory.MyItems.Count > 0)
                {
                    //MyData.Index = Mathf.Clamp(MyData.Index, 0, MyInventory.MyItems.Count - 1);
                    DraggedItemTooltip.MyItem = MyInventory.SwitchItems(
								NewSelectedIndex, 
                                DraggedItemTooltip.MyItem);
                    MouseItemImage.texture = DraggedItemTooltip.MyItem.GetTexture();   // Set dragged item Texture  // switch textures
                    SetItemAlpha(
                        (MyInventory.MyItems[NewSelectedIndex].Name == "Empty"), 
                        ClickedItemImage,
                        MyInventory.MyItems[NewSelectedIndex]);
                    // If Item drag is empty, turn off, otherwise turn on
                    if (DraggedItemTooltip.MyItem.Name == "Empty")  // if item drag is empty
                    {
                        MyDraggedItem.transform.parent.gameObject.GetComponent<ZelGui>().TurnOff();
                    }
                    else if (MyDraggedItem.transform.parent.gameObject.activeSelf == false)
                    {
                        MyDraggedItem.transform.parent.gameObject.GetComponent<ZelGui>().TurnOn();
                        MyDraggedItem.transform.parent.position = SelectedItem.transform.position;
                        MyDraggedItem.transform.parent.rotation = SelectedItem.transform.rotation;
                    }
                }
            }
            else
            {
                //Debug.LogError("Inventory Selected Item: " + NewSelectedIndex + " out of count: " + MyGuis.Count);
            }
        }

		public void OnChangeSelectedItem(int NewSelectedItem)
		{
			if (HighlightedIcon) 
			{
				if (MyGuis.Count == 0)
					return;
				NewSelectedItem = Mathf.Clamp (NewSelectedItem, 0, MyGuis.Count-1);
                if (MyGuis[NewSelectedItem])
                {
                    HighlightedIcon.transform.position = MyGuis[NewSelectedItem].transform.position;
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

        void FilterSpells()
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

        void ItemToGui(Item MyItem, int ItemIndex)
        {
            GuiListElementData MyData = new GuiListElementData();
            MyData.LabelText = MyItem.Name;
            MyData.DescriptionText = MyItem.GetDescriptionLabel();
            if (MyItem.Name == "Empty")
            {
                Debug.LogError("Item is empty");
                MyData.LabelText = "";
                MyData.DescriptionText = "";
                MyData.IsToolTip = false;
                Add("", MyData, ItemIndex); // no tool tip for emptyness!
            }
            else
            {
                if (MyItem.GetQuantity() == 1)
                {
                    Add(MyItem.Name, MyData, ItemIndex);
                }
                else
                {
                    Add(MyItem.Name + "x" + MyItem.GetQuantity(), MyData, ItemIndex);
                }
            }
            if  (ItemIndex < MyGuis.Count)
            {
                GameObject CreatedItem = MyGuis[ItemIndex].gameObject;
                // set the texture
                RawImage MyItemImage = CreatedItem.transform.Find("ItemTexture").GetComponent<RawImage>();
                if (MyItemImage)
                {
                    bool IsEmpty = (MyInventory.MyItems[ItemIndex].Name == "Empty");
                    MyItemImage.texture = MyInventory.MyItems[ItemIndex].GetTexture();
                    SetItemAlpha(IsEmpty, MyItemImage, MyInventory.MyItems[ItemIndex]);
                }
                else
                {
                    Debug.LogError("Cannot find item image");
                }
            }
            else
            {
                Debug.LogWarning("Inventory is larger then gui");
            }
            /*if (CreatedItem)
            {
                ItemHandler MyItemHandler = CreatedItem.AddComponent<ItemHandler>();
                AddDefaultsToItemHandler(MyItemHandler, ItemIndex);
            }*/
        }

        void SetItemAlpha(bool IsEmpty, RawImage MyItemImage, Item MyItem)
        {
            if (IsEmpty)//ItemName == "Empty")
            {
                MyItemImage.color = new Color32(
                    (byte)(MyItemImage.color.r * 255),
                    (byte)(MyItemImage.color.g * 255),
                    (byte)(MyItemImage.color.b * 255),
                    0);
            }
            else
            {
                MyItemImage.color = new Color32(
                    (byte)(MyItemImage.color.r * 255),
                    (byte)(MyItemImage.color.g * 255),
                    (byte)(MyItemImage.color.b * 255),
                    255);
            }
            Text MyItemText = MyItemImage.transform.parent.Find("ItemText").GetComponent<Text>();
            int MyItemQuantity = MyItem.GetQuantity();
            MyItemText.enabled = false;
            if (!IsEmpty && MyItemQuantity > 1)
            {
                MyItemText.enabled = true;
                MyItemText.text = "x" + MyItemQuantity;
            }
        }

        /// <summary>
        /// Handles updating and only update what has changed!
        /// </summary>
        public override void RefreshList() 
		{
			//Debug.Log ("Refreshing Inventory Gui: " + Time.time);
			Clear ();
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
            DestroyImmediate(MyGuis[MyIndex]);
            MyGuis.RemoveAt(MyIndex);
            ItemToGui(MyItem, MyIndex);
        }
    }
}
/*public void AddDefaultsToItemHandler(ItemHandler MyItemHandler, int ItemIndex)
{
    MyItemHandler.InventoryIndex = ItemIndex;
    MyItemHandler.MyCharacter = MyCharacter;
    MyItemHandler.MyInventory = MyInventory;
    MyItemHandler.MyItem = MyInventory.MyItems[ItemIndex];
    MyItemHandler.OnBuyItemSound = OnBuyItemSound;
    MyItemHandler.OnSellItemSound = OnSellItemSound;
    MyItemHandler.OnNonBuyItemSound = OnNonBuyItemSound;
    MyItemHandler.OnNotSellingSound = OnNotSellingSound;
    MyItemHandler.MyAudioSource = MyAudioSource;
    //MyItemHandler.MyItemHold = MyItemHold;
    MyItemHandler.MyOnClickEvent = MyOnClickEvent;
}*/

/*for (int i = MyGuis.Count-1; i >= 0; i--)
{
    bool DoesContain = MyInventory.ContainsItem(MyGuis[i].name);
    if (!DoesContain || (DoesContain && MyInventory.GetItem(MyGuis[i].name).HasUpdated())) 
    {
        Destroy (MyGuis[i]);
        MyGuis.RemoveAt (i);
    }
}*/

//int InventoryIndex = .InventoryIndex;
//ItemDragIndex = SelectedIndex;
/*ItemHandler MyItemHandler = MyDraggedItem.GetComponent<ItemHandler>();
if (MyItemHandler == null)
{
    MyItemHandler = MyDraggedItem.AddComponent<ItemHandler>();
    AddDefaultsToItemHandler(MyItemHandler, InventoryIndex);
    MyItemHandler.MyItem = new Item();
}*/
