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
            RemoveSkills(); // make sure they all gone
            SetItem(0);
        }

        /// <summary>
        /// Removes all skills
        /// Input is the new item
        /// </summary>
        public void RemoveSkills(Item MyItem = null)
        {
            // Remove all skills
            if (MyItem == null || (!MyItem.HasCommand("/Block") && !MyItem.HasCommand("/Pickaxe")))
            {
                if (GetComponent<VoxelBrush>())
                {
                    //Debug.LogError("Destroying brush, as " + MyItem.Name + " has no brush command");
                    DestroyImmediate(GetComponent<VoxelBrush>());
                }
            }
            if (MyItem == null || !MyItem.HasCommand("/Command"))
            {
                if (GetComponent<BotCommander>())
                {
                    DestroyImmediate(GetComponent<BotCommander>());
                }
            }
            Skill[] MySkills = gameObject.GetComponents<Skill>();
            for (int i = MySkills.Length - 1; i >= 0; i--)
            {
                if ((MySkills[i] is Sheild) == false)//if not a sheild 
                {
                    //Debug.LogError(i + "Destroying: " + MySkills[i].ToString());
                    DestroyImmediate(MySkills[i]);
                }
                else
                {
                    if (!MySkills[i].IsActivate())
                    {
                        DestroyImmediate(MySkills[i]);
                    }
                }
            }
            //Debug.LogError("yoyoyoyo: " + MySkills.Length);
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
            int NewItemIndex = IncreaseSelectedIcon(1);
            //OnChangeSelectedItem();
        }
        
        private void DecreaseSelectedIcon()
        {
            int NewItemIndex = IncreaseSelectedIcon(-1);
            //OnChangeSelectedItem(MyCharacter.IncreaseSelectedIcon(-1));
        }

        /// <summary>
        /// Checks to see if item has any of the commands
        /// </summary>
        bool HasAnyCommand(Item MyItem)
        {
            return (SelectedIcon.HasCommand("/Block")
                || SelectedIcon.HasCommand("/Spell")
                || SelectedIcon.HasCommand("/Summoner")
                || SelectedIcon.HasCommand("/Sheild")
                || SelectedIcon.HasCommand("/Commander")
                || SelectedIcon.HasCommand("/Pickaxe"));
        }

        public Spell GetSelectedSpell()
        {
            if (SelectedIcon.HasCommand("/Spell"))
            {
                Inventory MyInventory = GetInventory();
                SelectedIcon = MyInventory.GetItem(SelectedIndex);
                string MyInput = SelectedIcon.GetInput("/Spell");
                Spell SelectedSpell = Zeltex.DataManager.Get().GetElement("Spells", MyInput) as Spell;
                return SelectedSpell;
            }
            else
            {
                return null;
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
            NewIndex = Mathf.Clamp(NewIndex, 0, MyInventory.GetSize() - 1);
            if (MyInventory.GetSize() == 0)
            {
                NewIndex = -1;
            }
            else if (SelectedIndex != NewIndex)
            {
                SelectedIndex = NewIndex;
                SelectedIcon = MyInventory.GetItem(SelectedIndex);
                if (SelectedIcon != null)
                {
                    bool HasCommand = HasAnyCommand(SelectedIcon);
                    // remove any previous command
                    RemoveSkills(SelectedIcon);
                    // Active skills
                    if (SelectedIcon.HasCommand("/Block"))
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
                        VoxelBrush MyBrush = gameObject.GetComponent<VoxelBrush>();
                        if (MyBrush == null)
                        {
                            MyBrush = gameObject.AddComponent<VoxelBrush>();
                        }
                        MyBrush.SetAsPickaxe();
                    }

                    if (SelectedIcon.HasCommand("/Spell"))     // /Spell [SpellName]
                    {
                        //string MyInput = ScriptUtil.RemoveCommand();
                        Shooter MyShooter = gameObject.GetComponent<Shooter>();
                        if (MyShooter == null)
                        {
                            MyShooter = gameObject.AddComponent<Shooter>();
                        }
                        MyShooter.SetSpell(GetSelectedSpell());// SpellMaker.Get().GetSpell(MyInput) as Spell);    // spell name ie Fireball
                    }

                    if (SelectedIcon.HasCommand("/Summoner"))
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
                    }
                    OnChangeItem.Invoke(SelectedIndex);
                }
            }
        }


    }
}