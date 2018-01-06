using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zeltex.Items;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Zeltex.Guis
{
    [System.Serializable]
    public class ItemsEvent : UnityEvent<Item, Item> { }

    /// <summary>
    /// Item GUI. Everything for interacting with items
    /// </summary>
    public class ItemGui : MonoBehaviour, IPointerClickHandler,
                        IPointerEnterHandler, IPointerExitHandler
    {
        public static ItemGui ItemPickupGui;
        public RawImage IconTexture;
        public Text QuantityText;
        [Tooltip("Reference to item the gui is using")]
        [SerializeField]
        private Item MyItem;
        private bool IsItemPickup;
        private RawImage IconBackground;
        private Outline MyOutline;

        [Tooltip("Used for crafted item.")]
        public bool IsGrabItemOnly;
        public ItemsEvent OnSwapItems = new ItemsEvent();

        private void Awake() 
        {
            IsItemPickup = (GetComponent<MouseFollower>() != null);
            IconBackground = GetComponent<RawImage>();
            if (IsItemPickup && IconBackground)
            {
                IconBackground.enabled = false;
            }
            if (IsItemPickup)
            {
                MyItem = new Item();
                MyItem.SetParentInventory(null);
                ItemPickupGui = this;
                gameObject.SetActive(false);
            }
            MyOutline = GetComponent<Outline>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            MyOutline.enabled = true;
            if (MyItem.Name != "Empty")
            {
                TooltipGui.Get().gameObject.SetActive(true);
                if (MyItem.GetQuantity() == 1)
                {
                    TooltipGui.Get().SetTexts(MyItem.Name, MyItem.GetDescription());
                }
                else
                {
                    TooltipGui.Get().SetTexts(MyItem.Name + " [x" + MyItem.GetQuantity() + "]", MyItem.GetDescription());
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            MyOutline.enabled = false;
            TooltipGui.Get().gameObject.SetActive(false);
        }

        void OnDisable() 
        {
            if (MyOutline)
            {
                MyOutline.enabled = false;
            }
        }

        public Item GetItem() 
        {
            return MyItem;
        }

        public void SetItem(Item NewItem) 
        {
            if (MyItem != null)
            {
                MyItem.OnUpdate.RemoveEvent(RefreshGui);
            }
            MyItem = NewItem;
            if (MyItem != null)
            {
                MyItem.OnUpdate.AddEvent(RefreshGui);
            }
            RefreshGui();
        }

        /// <summary>
        /// Refresh when an item is swapped, or when an item quantity updates
        /// </summary>
        public void RefreshGui() 
        {
            if (!(MyItem == null || MyItem.Name == "Empty" || MyItem.Name == ""))
            {
                if (IsItemPickup)
                {
                    gameObject.SetActive(true);
                }
                IconTexture.gameObject.SetActive(true);
                IconTexture.texture = MyItem.GetTexture();
                if (MyItem.GetQuantity() > 1)
                {
                    QuantityText.text = "x" + MyItem.GetQuantity();
                    QuantityText.gameObject.SetActive(true);
                }
                else
                {
                    QuantityText.gameObject.SetActive(false);
                }
            }
            else
            {
                if (IsItemPickup)
                {
                    gameObject.SetActive(false);
                }
                IconTexture.gameObject.SetActive(false);
                QuantityText.gameObject.SetActive(false);
            }
        }

        public void OnPointerClick(PointerEventData eventData) 
        {
            //Item MyItem = MyInventory.GetItem(NewSelectedIndex);
            if (MyItem != null)
            {
                if (IsGrabItemOnly)
                {
                    if (ItemPickupGui.GetItem().Name != "Empty")
                    {
                        Debug.Log("Can only grab from UI: " + name);
                        return;
                    }
                    else
                    {
                        Debug.LogError("Picking up crafted item!");
                        Item TemporaryItemA = ItemPickupGui.MyItem;
                        ItemPickupGui.SetItem(MyItem);
                        MyItem = TemporaryItemA;
                        MyItem.SetParentInventory(MyItem.ParentElement as Inventory);
                        ItemPickupGui.MyItem.SetParentInventory(null);
                        MyItem.OnUpdate.Invoke();
                        ItemPickupGui.MyItem.OnUpdate.Invoke();
                        OnSwapItems.Invoke(MyItem, ItemPickupGui.MyItem);
                        return;
                    }
                }
                Item TemporaryItem = ItemPickupGui.GetItem();
                Item ItemToSet = MyItem.SwitchItems(ItemPickupGui.GetItem());
                if (ItemToSet != TemporaryItem)
                {
                    ItemPickupGui.SetItem(ItemToSet);
                    MyItem = TemporaryItem;
                }
                if (MyItem != null) 
                {
                    Skeletons.Bone MyBone = MyItem.ParentElement as Skeletons.Bone;
                    if (!IsItemPickup && transform.parent.parent.name.Contains("Equipment"))
                    {
                        // now get Skeleton
                        if (MyBone != null)
                        {
                            Debug.Log("Reactivating bone: " + MyBone.Name + " with item " + ItemPickupGui.GetItem().Name);
                            MyBone.Reactivate();
                        }
                        else
                        {
                            Debug.LogError("Bone is null for " + MyItem.Name);
                        }
                    }
                }
                OnSwapItems.Invoke(MyItem, ItemPickupGui.MyItem);
            }
        }
    }

}