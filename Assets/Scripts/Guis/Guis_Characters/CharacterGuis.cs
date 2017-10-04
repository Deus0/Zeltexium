using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.UI;
using Zeltex.Dialogue;
using Zeltex.Items;
using Zeltex.Characters;
using Zeltex.Combat;
using Zeltex.Util;
using Zeltex.Quests;
using Newtonsoft.Json;

namespace Zeltex.Guis.Characters
{

    /// <summary>
    /// Managers the guis for characters
    /// To Do:
    ///     Make this static manager, just one
    ///     Characters will call it up to find it and spawn their guis
    ///     Seperate into a custom class, inside character, CharacterGuis, which just holds character guis
	///	lol rethink of the same solution again..
    /// </summary>
	[System.Serializable]
	public class CharacterGuis
	{
        [Header("Actions")]
        [JsonIgnore]
        public EditorAction ActionSpawnGui = new EditorAction();
        [JsonIgnore]
        public string ActionName = "Skillbar";
        [JsonIgnore]
        public EditorAction ActionDespawnGuis = new EditorAction();
        [JsonIgnore]
        public EditorAction ActionSpawnGuis = new EditorAction();
        [JsonIgnore]
        public EditorAction ActionSetGuiStates = new EditorAction();

        [Header("Data")]
        [SerializeField, JsonProperty]
        private List<string> GuisEnabled = new List<string>();
        [SerializeField, JsonProperty]
        private List<bool> GuisEnabledStates = new List<bool>();
        [SerializeField, JsonIgnore]
		private List<ZelGui> MyGuis = new List<ZelGui>();
        [SerializeField, JsonProperty]
        public bool IsHiddenLabel = false;
        [SerializeField, JsonIgnore]
        private Dictionary<string, bool> MyStates = new Dictionary<string, bool>();
        [JsonIgnore]
        private bool IsFrozen = false;
        [JsonIgnore]
        private string LabelName = "";
        [SerializeField, JsonIgnore]//, HideInInspector]
        private Character MyCharacter;

        public void Update()
        {
            if (ActionSpawnGui.IsTriggered())
            {
                Spawn(ActionName);
            }
            if (ActionSpawnGuis.IsTriggered())
            {
                SpawnAllGuis();
            }
            if (ActionDespawnGuis.IsTriggered())
            {
                DespawnAllGuis();
            }
            if (ActionSetGuiStates.IsTriggered())
            {
                SetGuiStates();
            }
        }
        
        private UniversalCoroutine.Coroutine SpawnAllRoutine;
        /// <summary>
        /// Spawns all guis listed in the strings
        /// </summary>
        private void SpawnAllGuis()
        {
            DespawnAllGuis();
            for (int i = 0; i < GuisEnabled.Count; i++)
            {
                ZelGui MySpawn = Spawn(GuisEnabled[i]);
                if (MySpawn && i < GuisEnabledStates.Count)
                {
                    MySpawn.SetState(GuisEnabledStates[i]);
                }
            }
        }

        public void SetGuiStates()
        {
            for (int i = 0; i < GuisEnabled.Count; i++)
            {
                ZelGui MySpawn = GetZelGui(GuisEnabled[i]);
                if (MySpawn && i < GuisEnabledStates.Count)
                {
                    MySpawn.SetState(GuisEnabledStates[i]);
                    AttachGui(MySpawn);
                }
            }
        }

        private void DespawnAllGuis()
        {
            for (int i = 0; i < MyGuis.Count; i++)
            {
                if (MyGuis[i])
                {
                    MyGuis[i].Die();
                }
            }
            MyGuis.Clear();
        }

        public List<string> GetNames()
        {
            List<string> GuiNames = new List<string>();
            for (int i = 0; i < MyGuis.Count; i++)
            {
                if (MyGuis[i])
                {
                    GuiNames.Add(MyGuis[i].name);
                }
            }
            return GuiNames;
        }

        public ZelGui Spawn(string GuiName)
        {
            ZelGui MyGui = GetZelGui(GuiName);
            if (MyGui)
            {
                return MyGui;
            }
            if (MyCharacter && CharacterGuiManager.Get() && GetZelGui(GuiName) == null)
            {
                MyGui = CharacterGuiManager.Get().GetPoolObject(
                    GuiName,
                    MyCharacter.GetComponent<UnityEngine.Networking.NetworkIdentity>());
                Debug.Log("Spawned " + GuiName + ":" + (MyGui != null));
                AttachGui(MyGui);
                return MyGui;
            }
            else
            {
                Debug.LogError("Cannot spawn " + GuiName + " from CharacterGuiManager");
                return MyGui;
            }
        }

        public void Remove(ZelGui MyZelGui)
        {
            if (MyZelGui)
            {
                if (MyGuis.Contains(MyZelGui))
                {
                    MyGuis.Remove(MyZelGui);
                }
                CharacterGuiManager.Get().ReturnObject(MyZelGui, MyZelGui.name);
                //GameObject.Destroy(MyZelGui.gameObject);
            }
        }
        /// <summary>
        /// Get the character that the guis are attached to
        /// </summary>
        public Character GetCharacter()
        {
            return MyCharacter;
        }

        #region ZelGuis

        /// <summary>
        /// Clears all the guis from the character
        /// </summary>
        public void Clear()
        {
            for (int i = MyGuis.Count-1; i >= 0; i--)
            {
                CharacterGuiManager.Get().ReturnObject(MyGuis[i]);
            }
            MyGuis.Clear();
        }

        /// <summary>
        /// Returns true if the character has a gui with the name
        /// </summary>
        public bool HasGui(string GuiName)
        {
            for (int i = MyGuis.Count - 1; i >= 0; i--)
            {
                if (MyGuis[i].name == GuiName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Disables all the guis
        /// </summary>
		public void HideAll() 
		{
			for (int i = 0; i < MyGuis.Count; i++) 
			{
				if (MyGuis[i])
                {
                    MyGuis[i].TurnOff();
				}
			}
		}

        public void ShowAll()
        {
            for (int i = 0; i < MyGuis.Count; i++)
            {
                if (MyGuis[i])
                {
                    MyGuis[i].TurnOn();
                }
            }
        }

        public int GetSize()
        {
            return MyGuis.Count;
        }

        public ZelGui GetZelGui(int i)
        {
            return MyGuis[i];
        }

        /// <summary>
        /// Set the Active of all the guis
        /// </summary>
        public void SetGuisActive(bool NewActiveState)
        {
            for (int i = 0; i < MyGuis.Count; i++)
            {
                if (MyGuis[i] != null)
                {
                    MyGuis[i].SetState(NewActiveState);
                }
            }
        }

        /// <summary>
        /// Toggles all the guis
        /// </summary>
        public void ToggleGuis()
        {
            for (int i = 0; i < MyGuis.Count; i++)
            {
                if (MyGuis[i] != null)
                {
                    MyGuis[i].Toggle();
                }
            }
        }

        /// <summary>
        /// Toggles a gui's on state
        /// </summary>
        public void ToggleGui(string GuiName)
        {
            if (IsFrozen == false)
            {
                ZelGui MyZelGui = GetZelGui(GuiName);
                if (MyZelGui != null)
                {
                    MyZelGui.Toggle();
                }
            }
        }

        /// <summary>
        /// turns on a specific gui
        /// </summary>
        public void EnableGui(string GuiName) 
		{
            if (IsFrozen == false)
            {
                ZelGui MyZelGui = GetZelGui(GuiName);
                if (MyZelGui != null)
                {
                    MyZelGui.TurnOn();
                }
            }
        }

        /// <summary>
        /// Disable a gui by name
        /// </summary>
        public void DisableGui(string GuiName)
        {
            if (IsFrozen == false)
            {
                ZelGui MyZelGui = GetZelGui(GuiName);
                if (MyZelGui != null)
                {
                    MyZelGui.TurnOff();
                }
            }
        }

        /// <summary>
        /// Add a ZelGui to the list
        /// </summary>
        public void AddZelGui(ZelGui NewZelGui)
        {
            // if already exists in list, dont add
            if (NewZelGui == null || MyGuis.Contains(NewZelGui))
            {
                return; // Already exists
            }
            // add it onto the menu gui as a toggle gui
            ZelGui MyMenu = GetZelGui("Menu");
            if (MyMenu != null)
            {
                MyMenu.GetComponent<MenuGui>().AddElement(NewZelGui.name);
            }
            // Finally add it to the list
            MyGuis.Add(NewZelGui);
        }

        /// <summary>
        /// Get a ZelGui by name
        /// </summary>
		public ZelGui GetZelGui(string GuiName)
        {
            //Debug.LogError ("Enabling: " + GuiName);
            for (int i = 0; i < MyGuis.Count; i++)
            {
                if (MyGuis[i])
                {
                    if (MyGuis[i].name == GuiName)
                    {
                        //Debug.LogError ("Found: " + GuiName);
                        return MyGuis[i];
                    }
                }
            }
            return null;
        }
        #endregion

        #region ToggleStates

        /// <summary>
        /// Saves all the toggle states
        /// </summary>
        public void SaveStates()
        {
            MyStates.Clear();
            for (int i = 0; i < MyGuis.Count; i++)
            {
                if (MyGuis[i])
                {
                    if (MyStates.ContainsKey(MyGuis[i].name) == false)
                    {
                        MyStates.Add(MyGuis[i].name, MyGuis[i].GetState());
                    }
                    else
                    {
                        Debug.LogError("Repeated gui with name: " + MyGuis[i].name);
                    }
                }
            }
            IsFrozen = true;
        }

        /// <summary>
        /// Restores all the toggle states from the saved ones
        /// </summary>
        public void RestoreStates()
        {
            IsFrozen = false;
            foreach(KeyValuePair<string, bool> MyKeyValuePair in MyStates)
            {
                ZelGui MyGui = GetZelGui(MyKeyValuePair.Key);
                if (MyGui)
                {
                    MyGui.SetState(MyKeyValuePair.Value);
                }
            }
           /* for (int i = 0; i < MyGuis.Count; i++)
            {
                if (MyGuis[i])
                {
                    MyGuis[i].SetState(MyStates[i]);
                }
            }*/
            MyStates.Clear();
        }
		#endregion

		#region Label

        /// <summary>
        /// Updates the label! Move this onto the stat bar manager
        /// </summary>
        public void SetNameLabel(string MyName)
        {
            ZelGui MyStatsBarZelGui = GetZelGui("StatsBar");
            if (MyStatsBarZelGui)
            {
                StatBarManager MyStatsBarManager = MyStatsBarZelGui.GetComponent<StatBarManager>();
                if (MyStatsBarManager)
                {
                    MyStatsBarManager.RefreshStats();
                }
            }

            ZelGui MyLabelZelGui = GetZelGui("Label");
			if (MyLabelZelGui == null)
            {
                //Debug.LogError(name + " does not have a Label Gui.");
                return;
            }
            Transform LabelHeader = MyLabelZelGui.transform.Find("Header");
            if (LabelHeader && LabelHeader.childCount >= 1)
            {
                if (!IsHiddenLabel)
                {
					LabelName = MyName;

                }
                else
                {
					LabelName = GenerateHiddenName(MyName);
				}
				LabelHeader.Find("LabelText").gameObject.GetComponent<Text>().text = LabelName;
			}
            else
            {
                Debug.LogError("LabelBackground not found in " + LabelHeader.name + ". It is needed to change the text of the characters label.");
            }
            MyLabelZelGui.GetComponent<StatBarManager>().RefreshStats();
        }

		public string GenerateHiddenName(string MyName)
        {
			string NewName = "";
			float RandomRange = MyName.Length+Random.Range(-2,2);
			if (RandomRange < 3) // min ?'s is 3
				RandomRange = 3;	
			for (int i = 0; i < RandomRange; i++)
            {
                NewName += "?";
            }
			return NewName;
        }

		#endregion

		#region Spawning

		/// <summary>
		/// Spawns all the guis that are prefabbed.
		///     - To Do: make this part of the save file for enabling/disabling
		/// </summary>
		public void SetCharacter(Character MyCharacter2)
        {
            if (MyCharacter != MyCharacter2)
            {
                MyCharacter = MyCharacter2;
                SpawnAllGuis();
            }
        }

        /// <summary>
        ///  Attaches a GUI onto a character
        /// </summary>
        public void AttachGui(ZelGui GuiObject, string Name = "")
        {
			if (GuiObject == null)
			{
                Debug.LogError(Name + " is null");
				return;
            }
            if (MyCharacter == null)
            {
                Debug.LogError("Character is null while attaching gui: " + GuiObject.name);
                return;
            }

            if (LayerManager.Get())
            {
                LayerManager.Get().SetLayerGui(GuiObject.gameObject);
            }
            CharacterGuiHandle MyHandle = GuiObject.GetComponent<CharacterGuiHandle>();
            if (MyHandle == null)
            {
                MyHandle = GuiObject.gameObject.AddComponent<CharacterGuiHandle>();
            }
            MyHandle.SetCharacter(MyCharacter);
            // put on same layer as character
            //ObjectViewer.SetLayerRecursive(GuiObject, MyCharacter.gameObject.layer);//1 << 

            //Debug.Log(name + " is attaching gui: " + Name);
            //GuiObject.gameObject.SetActive(true);
            if (CharacterGuiManager.Get())
            {
                GuiObject.transform.SetParent(CharacterGuiManager.Get().transform);
            }
            GuiObject.transform.localPosition = Vector3.zero;
            if (Name != "")
            {
                GuiObject.name = Name;
            }
            ZelGui MyZelGui = GuiObject.gameObject.GetComponent<ZelGui>();
            if (MyZelGui)
            {
                AddZelGui(MyZelGui);
            }
            else
            {
                //Debug.LogError(Name + " Does not have a ZelGui inside " + name + ".");
            }
            // link these to the main body

            Orbitor MyOrbitor = GuiObject.gameObject.GetComponent<Orbitor>();
            if (MyOrbitor)
            {
                if (MyCharacter)
                {
                    MyOrbitor.SetTarget(MyCharacter.GetCameraBone());
                    MyOrbitor.TargetSkeleton = MyCharacter.GetSkeleton();
                }
                else
                {
                    Debug.LogError("Guis has no character");
                }
                //Zeltex.Skeletons.Skeleton MySkeleton = MyCharacter.transform.Find("Body").GetComponent<Zeltex.Skeletons.Skeleton>();
                MyOrbitor.IsFollowUserAngleAddition = true;
            }

            Billboard MyBillboard = GuiObject.gameObject.GetComponent<Billboard>();
            if (MyBillboard  && MyCharacter)
            {
                MyBillboard.SetTarget(MyCharacter.GetCameraBone());
            }

            Follower MyFollower = GuiObject.gameObject.GetComponent<Follower>();
            if (MyFollower && MyCharacter)
            {
                MyFollower.SetTarget(MyCharacter.transform);
            }

            InventoryGuiHandler MyInventoryHandler = GuiObject.GetComponent<InventoryGuiHandler>();
            if (MyInventoryHandler)
            {
                MyInventoryHandler.RefreshList();
            }
            // Special Updates
            if (GuiObject.name.Contains("Label"))
            {
                UpdateLabel(GuiObject.gameObject);
            }
            else if(GuiObject.name.Contains("StatsBar"))
            {
                UpdateLabel(GuiObject.gameObject);
            }
            else if (GuiObject.name.Contains("Inventory"))
            {
                UpdateInventory(GuiObject.gameObject);
            }
            else if (GuiObject.name.Contains("SkillBar"))
            {
                UpdateSkillbar(GuiObject.gameObject);
            }
            else if (GuiObject.name.Contains("Dialogue"))    // link up texts to the dialogue handler
            {
                UpdateDialogue(GuiObject.gameObject);
            }
            else if (GuiObject.name.Contains("Stats"))
            {
                UpdateStats(GuiObject.gameObject);
            }
            else if (GuiObject.name.Contains("QuestLog"))
            {
                UpdateQuestLog(GuiObject.gameObject);
            }
            else if (GuiObject.name.Contains("Log"))
            {
                UpdateLog(GuiObject.gameObject);
            }
            else if (GuiObject.name.Contains("Equipment"))
            {
                UpdateEquipment(GuiObject.gameObject);
            }
            else if (GuiObject.name.Contains("ItemPickup"))
            {
                UpdateItemPickup(GuiObject.gameObject);
            }
            else if (GuiObject.name.Contains("Tooltip"))
            {
                UpdateTooltip(GuiObject.gameObject);
            }
            else if (GuiObject.name.Contains("Menu"))
            {
                GuiObject.GetComponent<MenuGui>().MyGuiManager = this;
                GuiObject.GetComponent<MenuGui>().RefreshElements();
            }
        }

        private void UpdateDialogue(GameObject GuiObject)
        {
            //DialogueHandler CharacterDialogueHandler = MyCharacter.GetComponent<DialogueHandler>();
            DialogueHandler GuiDialogueHandler = GuiObject.GetComponent<DialogueHandler>();
            //GuiDialogueHandler.MyTree = CharacterDialogueHandler.MyTree;    // set tree up
            //DialogueHandler MySpeech = MyCharacter.GetComponent<DialogueHandler>();
            if (GuiDialogueHandler && GuiObject.transform.Find("SpeechTextBackground"))
            {
                GuiDialogueHandler.UpdateDialogueText(GuiObject.transform.Find("SpeechTextBackground").GetChild(0).GetComponent<Text>());
            }
        }

        public void UpdateLabel(GameObject GuiObject)
        {
            StatBarManager MyStatBarManager = GuiObject.GetComponent<StatBarManager>();
            if (MyStatBarManager && MyCharacter)
            {
                MyStatBarManager.SetTarget(MyCharacter.gameObject);
                if (MyCharacter.GetStats() != null)
                {
                    // delegates
                    MyCharacter.GetStats().OnUpdateStats.AddEvent(MyStatBarManager.OnUpdateStats);
                    MyCharacter.GetStats().OnNewStats.AddEvent(MyStatBarManager.OnNewStats);

                    MyStatBarManager.RefreshStats();
                }
            }
            if (GuiObject.name == "Label")
            {
                SetNameLabel(MyCharacter.name);
            }
        }

        private void UpdateStats(GameObject GuiObject)
        {
            StatGuiHandler MyStatsGuiHandler = GuiObject.GetComponent<StatGuiHandler>();
            if (MyCharacter && MyCharacter.GetStats() != null)
            {
                MyStatsGuiHandler.MyCharacter = MyCharacter;
                MyStatsGuiHandler.UpdateGuiStats(); // update it now that it exists
                MyCharacter.GetStats().OnUpdateStats.AddEvent(MyStatsGuiHandler.UpdateGuiStats);
                MyCharacter.GetStats().OnNewStats.AddEvent(MyStatsGuiHandler.OnNewGuiStats);
            }
        }

        private void UpdateInventory(GameObject GuiObject)
        {
            if (MyCharacter)
            {
                InventoryGuiHandler MyInventoryGuiHandler = GuiObject.GetComponent<InventoryGuiHandler>();
                Inventory MyInventory = MyCharacter.GetBackpackItems();
                MyInventoryGuiHandler.MyCharacter = MyCharacter;
                MyInventoryGuiHandler.MyInventory = MyInventory;
                QuestLog MyQuestLog = MyCharacter.GetData().MyQuestLog;
                if (MyInventory != null)
                {
                    MyInventory.OnExchangeCurrency = new UnityEvent();
                    MyInventory.OnAddItem.AddEvent(MyInventoryGuiHandler.RefreshList);
                    MyInventory.OnAddItem.AddEvent(MyQuestLog.OnAddItem);
                    MyInventory.OnUpdateItem.AddEvent(MyInventoryGuiHandler.RefreshAt);
                }
                else
                {
                    Debug.LogError(LabelName + " has no inventory");
                }
            }
        }

        private void UpdateSkillbar(GameObject GuiObject)
        {
            if (MyCharacter)
            {
                // Get References
                InventoryGuiHandler MyInventoryGuiHandler = GuiObject.GetComponent<InventoryGuiHandler>();
                Inventory MyInventory = MyCharacter.GetSkillbarItems();
                Skillbar MySkillbar = MyCharacter.GetComponent<Skillbar>();
                MySkillbar.OnChangeItem.AddEvent(MyInventoryGuiHandler.OnChangeSelectedItem);
                if (MyInventory != null)
                {
                    MyInventory.OnUpdateItem.AddEvent(MySkillbar.SetItem);
                    MyInventoryGuiHandler.MyCharacter = MyCharacter;
                    MyInventoryGuiHandler.MyInventory = MyInventory;
                    MyInventory.OnAddItem.AddEvent(MyInventoryGuiHandler.RefreshList);
                    MyInventory.OnUpdateItem.AddEvent(MyInventoryGuiHandler.RefreshAt);
                }
                else
                {
                    Debug.LogError("Inventory is null: " + MyCharacter.name);
                }
            }
        }

        private void UpdateQuestLog(GameObject GuiObject)
        {
            QuestLogGuiHandler MyQuestLogGuiHandler = GuiObject.GetComponent<QuestLogGuiHandler>();
            if (MyCharacter && MyQuestLogGuiHandler)
            {
                MyQuestLogGuiHandler.MyCharacter = MyCharacter;
                QuestLog MyQuestLog = MyCharacter.GetData().MyQuestLog;
                MyQuestLog.OnAddQuest.AddEvent(MyQuestLogGuiHandler.UpdateQuestGuis);
                MyQuestLogGuiHandler.UpdateQuestGuis();// refresh quests now
            }
        }

        private void UpdateEquipment(GameObject GuiObject)
        {
            if (MyCharacter)
            {
                CharacterStats MyStats = MyCharacter.GetData().MyStats;
                Inventory MyEquipment = MyCharacter.GetEquipment();
                // link characterstats to equipment inventory
                MyStats.SetEquipment(MyEquipment);
                // link characterStats to StatsGui
                GuiObject.GetComponent<StatGuiHandler>().MyCharacter = MyCharacter;
                MyEquipment.OnAddItem.AddEvent(GuiObject.GetComponent<StatGuiHandler>().OnNewGuiStats);
            }
        }

        private void UpdateItemPickup(GameObject GuiObject)
        {
            GameObject MyDraggedItem = GuiObject.transform.GetChild(0).gameObject;
            if (GetZelGui("Inventory"))
            {
                ZelGui InventoryZelGui = GetZelGui("Inventory");
                if (InventoryZelGui)
                {
                    GameObject InventoryGui = InventoryZelGui.gameObject;
                    InventoryGui.GetComponent<InventoryGuiHandler>().MyDraggedItem = MyDraggedItem;
                }

                ZelGui SkillbarZelGui = GetZelGui("SkillBar");
                if (SkillbarZelGui)
                {
                    GameObject SkillbarGui = SkillbarZelGui.gameObject;
                    SkillbarGui.GetComponent<InventoryGuiHandler>().MyDraggedItem = MyDraggedItem;
                }

                ZelGui MyEquipmentZelGui = GetZelGui("Equipment");
                if (MyEquipmentZelGui)
                {
                    GameObject EquipmentGui = MyEquipmentZelGui.gameObject;
                    EquipmentGui.GetComponent<InventoryGuiHandler>().MyDraggedItem = MyDraggedItem;
                }

                ZelGui CraftZelGui = GetZelGui("Craft");
                if (CraftZelGui)
                {
                    GameObject CraftGui = CraftZelGui.gameObject;
                    CraftGui.GetComponent<InventoryGuiHandler>().MyDraggedItem = MyDraggedItem;
                }
            }
            else if (MyCharacter)
            {
                Debug.LogError(MyCharacter.name + " does not have right guis.");
            }
        }

		/// <summary>
		/// Updates the tool tip with all the ZelGuis
		/// </summary>
        private void UpdateTooltip(GameObject GuiObject)
        {
            if (GetZelGui("Menu"))
            {
				ZelGui MenuToggle = GetZelGui("Menu");
				if (MenuToggle)
				{
					MenuToggle.GetComponent<MenuGui>().SetTooltip(GuiObject);
				}

				ZelGui LabelToggle = GetZelGui("Label");
				if (LabelToggle)
				{
					LabelToggle.GetComponent<StatBarManager>().SetTooltip(GuiObject);
				}

				ZelGui InventoryToggle = GetZelGui("Inventory");
				if (InventoryToggle)
				{
					InventoryToggle.GetComponent<InventoryGuiHandler>().SetTooltip(GuiObject);
				}

				ZelGui EquipmentToggle = GetZelGui("Equipment");
				if (EquipmentToggle)
				{
					EquipmentToggle.GetComponent<InventoryGuiHandler>().SetTooltip(GuiObject);
				}

				ZelGui SkillBarToggle = GetZelGui("SkillBar");
				if (SkillBarToggle)
				{
					SkillBarToggle.GetComponent<InventoryGuiHandler>().SetTooltip(GuiObject);
				}

				ZelGui QuestLogToggle = GetZelGui("QuestLog");
				if (QuestLogToggle)
				{
					QuestLogToggle.GetComponent<QuestLogGuiHandler>().SetTooltip(GuiObject);
				}
				ZelGui StatsToggle = GetZelGui("Stats");
				if (StatsToggle)
				{
					StatsToggle.GetComponent<StatGuiHandler>().SetTooltip(GuiObject);
				}

				ZelGui LogToggle = GetZelGui("Log");
				if (LogToggle)
				{
					LogToggle.GetComponent<GuiList>().SetTooltip(GuiObject);
				}

            }
            else
            {
                Debug.LogError(MyCharacter.name + " does not have right guis.");
            }
            //GetZelGui("Crafting").GetComponent<InventoryGuiHandler>().TooltipGui = NewGui;
            //GetZelGui("Shop").GetComponent<InventoryGuiHandler>().TooltipGui = NewGui;
        }

        /// <summary>
        /// Update the Log Handles
        /// </summary>
        private void UpdateLog(GameObject GuiObject)
        {
            if (MyCharacter)
            {
                Log MyLog = MyCharacter.GetComponent<Log>();
                if (MyLog)
                {
                    /*GuiList MyLogGuiList = GuiObject.GetComponent<GuiList>();
                    Inventory MyInventory = MyCharacter.GetInventory();
                    if (MyLogGuiList)
                    {
                        //MyLog.OnAddLogString = new EventString();
                        //UnityEditor.Events.UnityEventTools.AddPersistentListener(
                        //    MyLog.OnAddLogString,
                        // delegate { MyLogGuiList.AddGui(); });
                    }
                    if (MyInventory != null)
                    {
                        MyInventory.OnPickupItem = new EventObjectString();
                        MyInventory.OnPickupItem.AddEvent<GameObject, string>(MyLog.AddLogEvent);
                        MyInventory.OnExchangeItem = new EventObjectString();
                        MyInventory.OnExchangeItem.AddEvent<GameObject, string>(MyLog.AddLogEvent);
                    }*/
                }
            }
        }
        #endregion
    }
}


/// <summary>
/// Gets all the IDs
/// </summary>
/*public string GetZelGuisPacket()
{
    string MyZelGuisPacket = "";
    for (int i = 0; i < MyGuis.Count; i++)
    {
        MyZelGuisPacket += MyGuis[i].name + ',' + MyGuis[i].GetComponent<PhotonView>().viewID + ' ';
    }
    return MyZelGuisPacket;
}*/

/// <summary>
/// From the packet, it contains all the IDs. It usees this to reattach guis.
/// </summary>
/*public void SetZelGuisPacket(string MyPacket)
{
    string[] MyData = MyPacket.Split(' ');
    for (int i = 0; i < MyData.Length; i++)
    {
        string[] MyData2 = MyData[i].Split(',');
        int MyPhotonID = int.Parse(MyData2[1]);
        //PhotonView MyView = PhotonView.Find();
        //ZelGui MyZelGui = MyView.GetComponent<ZelGui>();
        AttachGui(MyPhotonID, MyData2[0]);
    }
}*/

/*private void RefreshGuiList()
{
    MyGuiSpawns.Clear();
    MyGuiSpawns.Add("Crosshair");
    MyGuiSpawns.Add("StatsBar");
    MyGuiSpawns.Add("SkillBar");
    MyGuiSpawns.Add("Stats");
    MyGuiSpawns.Add("Clock");
    MyGuiSpawns.Add("Map");
    MyGuiSpawns.Add("ItemPickup");
    MyGuiSpawns.Add("Chest");
    MyGuiSpawns.Add("Clan");
    MyGuiSpawns.Add("Craft");
    MyGuiSpawns.Add("Dialogue");
    MyGuiSpawns.Add("Equipment");
    MyGuiSpawns.Add("Inventory");
    MyGuiSpawns.Add("Label");
    MyGuiSpawns.Add("Log");
    MyGuiSpawns.Add("Menu");
    MyGuiSpawns.Add("Party");
    MyGuiSpawns.Add("QuestBegin");
    MyGuiSpawns.Add("QuestLog");
    MyGuiSpawns.Add("ScoreBoard");
    MyGuiSpawns.Add("Shop");
    MyGuiSpawns.Add("Spellbook");
    MyGuiSpawns.Add("Tooltip");
}*/
/*SpeechHandler MySpeech = MyCharacter.GetComponent<SpeechHandler>();
if (MySpeech)
{
    MySpeech.OnBeginTalkTo = new CustomEvents.MyEvent3();
    UnityEditor.Events.UnityEventTools.AddPersistentListener(MySpeech.OnBeginTalkTo,
    MyLog.AddLogEvent);
}*/
/*Zeltex.Quests.QuestHelper MyQuestHelper = MyCharacter.GetComponent<Zeltex.Quests.QuestHelper>();
if (MyQuestHelper) {
UnityEditor.Events.UnityEventTools.AddPersistentListener(MyQuestLog.OnAddQuest,
MyQuestHelper.FindCurrentQuestTarget
);
UnityEditor.Events.UnityEventTools.AddPersistentListener(MyQuestLog.OnCompletedQuest,
MyQuestHelper.FindCurrentQuestTarget
);
}*/
