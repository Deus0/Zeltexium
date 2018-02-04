using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Zeltex.Items;
using Zeltex.Voxels;
using Zeltex.AI;
using Zeltex.Util;
using MakerGuiSystem;
using Zeltex.Characters;

namespace Zeltex.Combat
{
    /// <summary>
    /// Handles item switching and skills
    /// </summary>
    public class Skillbar : MonoBehaviour
    {
        public bool IsDebug;
        // item selection
        private Item SelectedIcon;
        private int MaxItems = 5;
        private int SelectedIndex = -1;
        [Header("Events")]
        public MyEventInt OnChangeItem;
        private Character MyCharacter;
        private UnityAction OnLoadInventoryAction;
        private Spell SelectedSpell;
        private Shooter MyShooter;

        void Start()
        {
            MyCharacter = GetComponent<Character>();
            SetItem(0);
            OnLoadInventoryAction = RefreshSkillbar;
            GetInventory().OnLoadEvent.AddEvent(OnLoadInventoryAction);
        }

        private Inventory GetInventory()
        {
            return MyCharacter.GetSkillbarItems();
        }


        private void OnGUI()
		{
			if (IsDebug)
            {
                GUILayout.Label("[" + name + "]'s Skillbar Count: " + GetInventory().GetSize());
                GUILayout.Label("    Selected Index [" + SelectedIndex + "]");
                for (int i = 0; i < GetInventory().GetSize(); i++)
                {
                    Item MyItem = GetInventory().GetItem(i);
                    GUILayout.Label("Item [" + i + "]: " + MyItem.Name);
                    GUILayout.Label("       : " + MyItem.GetDescription());
                }
			}
		}

        /// <summary>
        /// Makes sure to update the index!
        /// </summary>
        public void RefreshSkillbar()
        {
            SelectedIndex = -1;// for refreshing at beginning
            SetItem(0);
        }
        
        /// <summary>
        /// returns the selected item
        /// </summary>
        public Item GetSelectedItem()
        {
            return GetInventory().GetItem(GetSelectedIndex());
        }

        /// <summary>
        /// returns the selected item index
        /// </summary>
        public int GetSelectedIndex()
        {
            return SelectedIndex;
        }

        public void SwitchItemTo(int ItemIndex)
        {
            if (ItemIndex >= 0 && ItemIndex < 5)
            {
                SetItem(ItemIndex);
            }
        }

        /// <summary>
        /// Switches selected item to the first spell found
        /// </summary>
        public void SwitchToAttackSpell()
        {
            Debug.Log(name + " is now attacking!");
            if (GetInventory() != null)
            {
                for (int i = 0; i < GetInventory().GetSize(); i++)
                {
                    if (GetInventory().GetItem(i).HasTag("Weapon") || GetInventory().GetItem(i).HasTag("Spell"))
                    {
                        Debug.Log(name + " is now using " + GetInventory().GetItem(i).Name + " to attack!");
                        SetItem(i);
                        break;
                    }
                }
            }
            else
            {
                Debug.LogError(name + " has no skillbar. Cannot use items.");
            }
            //Debug.Log(name + " is has no item to attack with " + MySkillBar.MyItems.Count);
        }

        public void SwitchItemTo(string ItemName)
        {
            int ItemIndex = GetInventory().GetItemIndex(ItemName);
            if (SelectedIndex != ItemIndex
                && ItemIndex != -1)
            {
                SetItem(ItemIndex);
            }
        }

        public void SwitchItemDown()
        {
            IncreaseSelectedIcon();
        }

        public void SwitchItemUp()
        {
            DecreaseSelectedIcon();
        }

        // for icon selection
        private int IncreaseSelectedIcon(int IncreaseAmount)
        {
            // this should be different for each skillbar!
            return IncreaseSelectedIcon(IncreaseAmount, MaxItems); // gameObject.GetComponent<Inventory> ().GetSize ());
        }
        /// <summary>
        /// The main function to switch items
        /// </summary>
        private int IncreaseSelectedIcon(int IncreaseAmount, int MaxIcons)
        {
            //Debug.Log(name + " is switching items to: " + SelectedIconIndex + " from " + PreviousIndex);
            SetItem(SelectedIndex + IncreaseAmount);
            return SelectedIndex;
        }

        private void IncreaseSelectedIcon()
        {
            IncreaseSelectedIcon(1);
            //OnChangeSelectedItem();
        }
        
        private void DecreaseSelectedIcon()
        {
            IncreaseSelectedIcon(-1);
            //OnChangeSelectedItem(MyCharacter.IncreaseSelectedIcon(-1));
        }

        private void RefreshSelectedSpell()
        {
            Inventory MyInventory = GetInventory();
            SelectedIcon = MyInventory.GetItem(SelectedIndex);
            if (SelectedIcon != null)
            {
                SelectedSpell = SelectedIcon.MySpell;
            }
            else
            {
                SelectedSpell = null;
            }
        }

        public Spell GetSelectedSpell()
        {
            return SelectedSpell;
        }

        public void OnDeath()
        {
            if (MyShooter)
            {
                MyShooter.Die();
            }
        }

        /// <summary>
        /// Called whenever SkillBar(Inventory) item switched
        /// Input is the item that was updated
        /// </summary>
        public void SetItem(int NewIndex)
        {
            Inventory MyInventory = GetInventory();
            Debug.Log(name + "'s SetItem in Skillbar: " + NewIndex + " - previous: " + SelectedIndex + " with inventory: " + MyInventory.GetSize());
            NewIndex = Mathf.Clamp(NewIndex, -1, MyInventory.GetSize() - 1);
            if (MyInventory.GetSize() == 0)
            {
                NewIndex = -1;
            }

            if (SelectedIndex != NewIndex)
            {
                SelectedIndex = NewIndex;
                SelectedIcon = MyInventory.GetItem(SelectedIndex);
                RefreshSelectedSpell();
                if (SelectedIcon != null)
                {
                    // Active skills
                    /*if (SelectedIcon.HasCommand("/Block"))
                    {
                        VoxelBrush MyBrush = gameObject.GetComponent<VoxelBrush>();
                        if (MyBrush == null)
                        {
                            MyBrush = gameObject.AddComponent<VoxelBrush>();
                        }
                        MyBrush.UpdateItem(MyInventory, SelectedIcon, SelectedIndex);
                        MyBrush.UpdateBrushType(SelectedIcon.GetInput("/Block"));
                    }
                    else if (SelectedIcon.HasCommand("/Pickaxe"))
                    {
                        Pickaxe MyBrush = gameObject.GetComponent<Pickaxe>();
                        if (MyBrush == null)
                        {
                            MyBrush = gameObject.AddComponent<Pickaxe>();
                        }
                    }*/

                    if (SelectedSpell != null)     // /Spell [SpellName]
                    {
                        if (MyShooter == null)
                        {
                            MyShooter = gameObject.AddComponent<Shooter>();
                        }

                        Spell EquipSpell = GetSelectedSpell();
                        EquipSpell.MyCharacter = MyCharacter;
                        MyShooter.SetSpell(EquipSpell);// SpellMaker.Get().GetSpell(MyInput) as Spell);    // spell name ie Fireball
                    }
                    else if (MyShooter)
                    {
                        MyShooter.Die();
                    }

                    /*if (SelectedIcon.HasCommand("/Summoner"))
                    {
                        Summoner MySummoner = gameObject.GetComponent<Summoner>();
                        if (MySummoner == null)
                            gameObject.AddComponent<Summoner>();
                    }

                    if (SelectedIcon.HasCommand("/Commander"))
                    {
                        BotCommander MyCommander = gameObject.GetComponent<BotCommander>();
                        if (MyCommander == null)
                            gameObject.AddComponent<BotCommander>();
                    }

                    // For ZelGuis
                    Sheild MySheild = gameObject.GetComponent<Sheild>();
                    if (SelectedIcon.HasCommand("/Sheild"))
                    {
                        if (MySheild == null)
                        {
                            gameObject.AddComponent<Sheild>();
                        }
                        else
                        {
                            MySheild.ActivateInput();
                        }
                    }
                    else // keeps sheild on
                    {
                        if (MySheild != null)
                            MySheild.DisableInput();
                    }*/
                    OnChangeItem.Invoke(SelectedIndex);
                }
            }
        }


    }
}