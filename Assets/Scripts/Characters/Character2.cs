using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Zeltex.Quests;
using Zeltex.Util;
using Zeltex.Guis;
using Zeltex.Dialogue;
using Zeltex.Items;
using Zeltex.Combat;
using Zeltex.Skeletons;
using Zeltex.Voxels;
using UniversalCoroutine;

namespace Zeltex.Characters
{
    /// <summary>
    /// The File  part of the characters
    /// </summary>
    public partial class Character : NetworkBehaviour
    {
        #region Variables
        public static string FolderPath = "Characters/";
        private Vector3 LastSavedPosition = new Vector3(0, 0, 0);
        private string LastSavedFileName = "";
        // References used for character
        // These use references until set to false - until they are made unique through ingame changes
        private bool IsStaticRace = true;
        private bool IsStaticClass = true;
        // Need to be implemented - these are for npcs mostly as the players data will change alot
        private bool IsStaticStats = true;
        private bool IsStaticItems = true;
        private bool IsStaticQuests = true;
        private bool IsStaticDialogue = true;
        [Header("LevelData")]
        public Chunk MyChunk;
        public World InWorld;
        #endregion

        #region Utility

        public bool CanRespawn()
        {
            return Data.CanRespawn;
        }

        public CharacterStats GetStats()
        {
            return Data.MyStats;
        }
        /// <summary>
        /// Sets new race name and sets it to nonunique
        /// </summary>
        public void SetRace(string NewRaceName)
        {
            Data.Race = NewRaceName;
            IsStaticRace = !string.IsNullOrEmpty(Data.Race);    // sets true if not null string
        }
        /// <summary>
        /// A race name used for cosmetic purposes
        /// Saves the characters race uniquelly
        /// </summary>
        public void SetRaceUnique(string NewRaceName)
        {
            Data.Race = NewRaceName;
            IsStaticRace = false;
        }

        private void SetClass(string NewClassName)
        {
            Data.Class = NewClassName;
            IsStaticClass = !string.IsNullOrEmpty(Data.Class);
        }

        public World GetWorldInsideOf()
        {
            if (InWorld == null)
            {
                if (WorldManager.Get().MyWorlds.Count > 0)
                {
                    InWorld = WorldManager.Get().MyWorlds[0];
                }
            }
            return InWorld;
        }

        public Int3 GetChunkPosition()
        {
            if (GetWorldInsideOf() != null)
            {
                return InWorld.GetChunkPosition(transform.position.ToInt3());
            }
            else
            {
                return Int3.Zero();
            }
        }

        public Chunk GetChunkInsideOf()
        {
            GetWorldInsideOf();
            if (GetWorldInsideOf() != null)
            {
                Int3 ChunkPosition = InWorld.GetChunkPosition(transform.position.ToInt3());
                MyChunk = InWorld.GetChunk(ChunkPosition);
                return MyChunk;
            }
            else
            {
                Debug.LogError("Could not get a world");
                return null;
            }
        }

        public Chunk GetChunkInsideOfRaw()
        {
            if (GetWorldInsideOf() != null)
            {
                if (MyChunk == null)
                {
                    Int3 ChunkPosition = InWorld.GetChunkPosition(transform.position.ToInt3());
                    MyChunk = InWorld.GetChunk(ChunkPosition);
                }
                return MyChunk;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sets a new world for a character
        /// </summary>
        public void SetWorld(World NewWorld)
        {
            if (InWorld != NewWorld)
            {
                InWorld = NewWorld;
            }
        }

		/// <summary>
		/// Sets the guis target
		/// </summary>
		public void SetGuisTarget(Transform NewTarget)
		{
			for (int i = 0; i < MyGuis.GetSize(); i++)
			{
				Transform MyGui = MyGuis.GetZelGui(i).transform;
				if (MyGui.GetComponent<Orbitor>())
				{
					MyGui.GetComponent<Orbitor>().SetTarget(NewTarget);
				}
				if (MyGui.GetComponent<Billboard>())
				{
					MyGui.GetComponent<Billboard>().SetTarget(NewTarget);
				}
			}
		}

        /// <summary>
        /// Returns players inventory
        /// </summary>
        public Inventory GetInventory()
        {
            return Data.Skillbar;
        }

        public Inventory GetEquipment()
        {
            return Data.Equipment;
        }

        public QuestLog GetQuestLog()
        {
            return Data.MyQuestLog;
        }

        /// <summary>
        /// Gets a characters skeleton
        /// </summary>
        public Skeleton GetSkeleton()
        {
            if (MySkeleton == null)
            {
                Transform Body = transform.Find("Body");
                if (Body)
                {
                    MySkeleton = Body.GetComponent<Skeleton>();
                }
                else
                {
                    return null;
                }
            }
            return MySkeleton;
        }

        public Transform GetCameraBone()
        {
            if (MySkeleton)
            {
                return MySkeleton.GetCameraBone();
            }
            else
            {
                return transform;
            }
        }

        public List<string> GetStatistics()
        {
            List<string> MyData = new List<string>();
            MyData.Add("Character [" + name + "]\n" +
                "   Stats: " + Data.MyStats.GetSize() + "\n" +
                "   Inventory Items: " + GetInventory().GetSize() + "\n" +
                "   Quests: " + Data.MyQuestLog.GetSize() + "\n" +
                "   Dialogue: " + MyDialogueHandler.MyTree.GetSize() + "\n" +
                "   Skeleton: " + MySkeleton.MyBones.Count + "\n");

			MyData.Add("   Equipment: " + Data.Equipment.GetSize() + "\n");
            return MyData;
        }

        /// <summary>
        ///  Refresh all the references to components and datamanager
        /// </summary>
        private void RefreshComponents()
        {
            if (Data.MyQuestLog == null)
            {
                Data.MyQuestLog = gameObject.GetComponent<QuestLog>();
            }
            if (MyDialogueHandler == null)
            {
                MyDialogueHandler = gameObject.GetComponent<DialogueHandler>();
            }
            if (MySkeleton == null)
            {
                Transform BodyTransform = transform.Find("Body");
                if (BodyTransform)
                {
                    MySkeleton = transform.Find("Body").GetComponent<Skeleton>();
                }
            }
            if (MyCollider == null)
            {
                MyCollider = GetComponent<CapsuleCollider>();
            }
            GetSkillbar();
        }

        public Skillbar GetSkillbar()
        {
            if (MySkillbar == null)
            {
                MySkillbar = GetComponent<Skillbar>();
            }
            return MySkillbar;
        }
        #endregion

        #region Naming

        public void UpdateName(string NewName)
        {
            transform.name = NewName;
        }
        #endregion

        #region SavingLoading

        /// <summary>
        /// Clears character data
        /// </summary>
        private void Clear()
        {
            Data.MyStats.Clear();
            Data.MyQuestLog.Clear();
            MyDialogueHandler.MyTree.Clear();
            if (GetInventory() != null)
            {
                GetInventory().Clear();
                GetEquipment().Clear();
            }
            //IsStaticRace = false;
            //IsStaticClass = false;
        }

        private UniversalCoroutine.Coroutine RunScriptCoroutine = null;
        // Turning script back into the data
        // called internally and by CharacterMaker class!
        public void RunScript(List<string> MySaveFile)
        {
            if (RunScriptCoroutine != null)
            {
                UniversalCoroutine.CoroutineManager.StopCoroutine(RunScriptCoroutine);
            }
            RunScriptCoroutine = CoroutineManager.StartCoroutine(RunScriptRoutine(MySaveFile));
        }

        public IEnumerator RunScriptRoutine(string NewClassName, List<string> MySaveFile)
        {
            SetClass(NewClassName);
            yield return CoroutineManager.StartCoroutine(RunScriptRoutine(MySaveFile));
        }

        public void SetClassName(string NewClass)
        {
            Data.Class = NewClass;
        }

        public IEnumerator RunScriptRoutineWithSkeleton(string ClassScript, string SkeletonScript)
        {
            ZelGui MyStatsBar = MyGuis.GetZelGui("StatsBar");
            if (MyStatsBar)
            {
                MyStatsBar.SetState(false);
            }
            SetMovement(false);
            float TimeStartedLoading = Time.realtimeSinceStartup;
            List<string> ClassScriptList = FileUtil.ConvertToList(ClassScript);//ClassMaker.Get().GetData(ClassName);
            yield return CoroutineManager.StartCoroutine(RunScriptRoutine(ClassScriptList));
            if (MyStatsBar)
            {
                MyStatsBar.SetState(false);
            }
            if (MySkeleton)
            {
                List<string> SkeletonScriptList = FileUtil.ConvertToList(SkeletonScript);
                yield return CoroutineManager.StartCoroutine(MySkeleton.RunScriptRoutine(SkeletonScriptList));
            }
            //Debug.Break();
            yield return null;
            //Debug.LogError("Loading character taken: " + (Time.realtimeSinceStartup - TimeStartedLoading));
            SetMovement(true);
            if (MyStatsBar)
            {
                MyStatsBar.SetState(true);
            }
        }

        /// <summary>
        /// Loads the character from a script
        /// Make sure it only goes through the loop once! And everything else is loaded from this one loop!
        /// </summary>
        public IEnumerator RunScriptRoutine(List<string> MySaveFile)
        {
            ZelGui MyStatsBar = MyGuis.GetZelGui("StatsBar");
            if (MyStatsBar)
            {
                MyStatsBar.SetState(false);
            }
            enabled = true;
            //Debug.LogError("Loading character " + name + " with " + MySaveFile.Count + " lines of data.");
            // Clear Things
            RefreshComponents();
            yield return null;
        }

        /// <summary>
        /// Gets teh character data
        /// - To Do: Use Save Flags in them, and set to false if the data changes!
        /// </summary>
        public List<string> GetScript()
        {
            RefreshComponents();
            List<string> MyCharacterData = new List<string>();
            /*MyCharacterData.Add("/Name " + name);
            // Transform
            MyCharacterData.AddRange(GetTransformText());
            // First load race
            if (IsStaticRace)
            {
                MyCharacterData.Add("/LoadRace " + Data.Race);
            }
            else
            {
                if (Data.Race != "")
                {
                    MyCharacterData.Add("/Race " + Data.Race);
                }
                // Skeleton
                if (MySkeleton)
                {
                    MyCharacterData.AddRange(MySkeleton.GetScriptList());
                }
            }
            if (IsStaticClass)
            {
                MyCharacterData.Add("/LoadClass " + Data.Class);
            }
            else
            {
                // Meta data for character
                if (Data.Class != "")
                {
                    MyCharacterData.Add("/Class " + Data.Class);
                }
                // Stats
                if (Data.MyStats != null)
                {
                    MyCharacterData.AddRange(Data.MyStats.GetScriptList());
                }
                // SkillBar
                if (GetInventory() != null)
                {
                    List<string> SkillbarInventoryData = GetInventory().GetScriptList();
                    SkillbarInventoryData.Insert(0, "/SkillBar");
                    SkillbarInventoryData.Add("/EndSkillBar");
                    MyCharacterData.AddRange(SkillbarInventoryData);
                }
                // Dialogue
                if (MyDialogueHandler)
                {
                    MyCharacterData.AddRange(MyDialogueHandler.MyTree.GetScriptList());
                }
                // Equipment
                if (Data.Equipment != null)
                {
                    List<string> MyEquipmentData = Data.Equipment.GetScriptList();
                    MyEquipmentData.Insert(0, "/Equipment");
                    MyEquipmentData.Add("/EndEquipment");
                    MyCharacterData.AddRange(MyEquipmentData);
                }
            }*/
            return MyCharacterData;
        }
        #endregion

        #region Transform
        private void CheckForInWorld()
        {
            if (InWorld == null)
            {
                //Debug.LogWarning("While loading " + name + " had to get world.");
                InWorld = WorldManager.Get().MyWorlds[0];
            }
        }

        /// <summary>
        /// Gets transform script
        /// </summary>
        private List<string> GetTransformText()
        {
            List<string> MyData = new List<string>();
            CheckForInWorld();
            // save position
            MyData.Add("/transform");
            if (InWorld)
            {
                Vector3 LocalPosition = InWorld.transform.InverseTransformPoint(transform.position);
                MyData.Add("" + LocalPosition.x);
                MyData.Add("" + LocalPosition.y);
                MyData.Add("" + LocalPosition.z);
            }
            else
            {
                Vector3 LocalPosition = transform.position;
                MyData.Add("" + LocalPosition.x);
                MyData.Add("" + LocalPosition.y);
                MyData.Add("" + LocalPosition.z);
            }
            // save rotation of body
            MyData.Add("" + transform.rotation.x);
            MyData.Add("" + transform.rotation.y);
            MyData.Add("" + transform.rotation.z);
            MyData.Add("" + transform.rotation.w);
            // Save Camera Bone Position
            return MyData;
        }

        /// <summary>
        /// Loads transform
        /// </summary>
        public void SetTransformText(List<string> MyData)
        {
            CheckForInWorld();
            //Debug.LogError("Loading Transform: " + FileUtil.ConvertToSingle(MyData));
            int TransformStartIndex = -1;
            for (int i = 0; i < MyData.Count; i++)
            {
                if (MyData[i] == "/transform")
                {
                    TransformStartIndex = i;
                    i = MyData.Count;
                }
            }
            if (TransformStartIndex == -1)
            {
                //Debug.LogError (MyTransform.name + " Did not have a /transform command");
                return; // did not find transform data
            }
            if (MyData.Count > TransformStartIndex + 7)
            {
                try
                {
                    //Debug.LogError(MyTransform.name + " Has a old position of " + MyTransform.position.ToString());
                    Vector3 NewPosition = new Vector3(
                        float.Parse(MyData[TransformStartIndex + 1]),
                        float.Parse(MyData[TransformStartIndex + 2]),
                        float.Parse(MyData[TransformStartIndex + 3])
                    );
                    if (InWorld)
                    {
                        transform.position = InWorld.transform.TransformPoint(NewPosition);
                    }
                    else
                    {
                        transform.position = NewPosition;
                    }
                    transform.rotation = new Quaternion(
                        float.Parse(MyData[TransformStartIndex + 4]),
                        float.Parse(MyData[TransformStartIndex + 5]),
                        float.Parse(MyData[TransformStartIndex + 6]),
                        float.Parse(MyData[TransformStartIndex + 7])
                    );
                    //Debug.LogError(MyTransform.name + " Has a new position of " + MyTransform.position.ToString());
                }
                catch (System.FormatException e) { }
            }
        }
        #endregion
    }
}
/*Clear();
// Special Commands
int BeginIndex = -1;
string LoadType = "";
string SubLoadType = "";

#region CharacterData
for (int i = 0; i < MySaveFile.Count; i++)
{
    if (BeginIndex == -1)
    {
        if (MySaveFile[i].Contains("/Name "))
        {
            UpdateName(ScriptUtil.RemoveCommand(MySaveFile[i]));
        }
        else if (MySaveFile[i].Contains("/LoadRace "))
        {
            Data.Race = ScriptUtil.RemoveCommand(MySaveFile[i]);
            string SkeletonScript = DataManager.Get().Get("Skeletons", Data.Race);
            yield return UniversalCoroutine.CoroutineManager.StartCoroutine(MySkeleton.RunScriptRoutine(FileUtil.ConvertToList(SkeletonScript)));
            IsStaticRace = true;
        }
        else if (MySaveFile[i].Contains("/LoadClass "))
        {
            Data.Class = ScriptUtil.RemoveCommand(MySaveFile[i]);
            string ClassScript = DataManager.Get().Get("Classes", Data.Class);
            MySaveFile.InsertRange(i + 1, FileUtil.ConvertToList(ClassScript));
            IsStaticClass = true;
        }
        else if (MySaveFile[i].Contains("/Class "))
        {
            Data.Class = ScriptUtil.RemoveCommand(MySaveFile[i]);
        }
        else if (MySaveFile[i].Contains("/Race "))
        {
            GetComponent<Character>().Data.Race = ScriptUtil.RemoveCommand(MySaveFile[i]);
        }
        else if (MySaveFile[i].Contains("/GiveQuest "))
        {
            string QuestName = ScriptUtil.RemoveCommand(MySaveFile[i]);
            //MySaveFile.RemoveAt(i);
            Debug.Log("Giving Quest [" + QuestName + "] to " + name);
            Quest MyQuest = Zeltex.DataManager.Get().GetElement("Quests", QuestName) as Quest;
            if (MyQuest != null)
            {
                Data.MyQuestLog.Add(MyQuest);
                //MySaveFile.InsertRange(i, MyQuest.GetScriptList());
            }
            //yield return new WaitForSeconds(0.01f);
        }
        else if (MySaveFile[i].Contains("/GiveDialogue "))
        {
            string Dialogue = ScriptUtil.RemoveCommand(MySaveFile[i]);
            //MySaveFile.RemoveAt(i);
            Debug.Log("Giving Dialogue [" + Dialogue + "] to " + name);
            string MyScript = Zeltex.DataManager.Get().Get("Dialogues", Dialogue);
            if (MyScript != "")
            {
                MyDialogueHandler.MyTree.RunScript(FileUtil.ConvertToList(MyScript));
                //MySaveFile.InsertRange(i, FileUtil.ConvertToList(MyScript));
            }
        }
        else if (MySaveFile[i].Contains("/GiveItem "))
        {
            string ItemName = ScriptUtil.RemoveCommand(MySaveFile[i]);
            //MySaveFile.RemoveAt(i);
            Debug.Log("Giving Item [" + ItemName + "] to " + name);
            Item MyItem = DataManager.Get().GetElement("ItemMeta", ItemName) as Item; // TODO: Fix 
            //MyMaker.MyItemMaker.MyInventory.GetItem(ItemName);
            if (MyItem != null)
            {
                //List<string> MyItemScript = MyItem.GetScript2();
                //MySaveFile.InsertRange(i, MyItemScript);
                //MyInventory.Add(MyItem);
                if (SubLoadType == "")
                {
                    GetInventory().Add(MyItem);
                }
                else if (SubLoadType == "SkillBar")
                {
                    if (Data.Skillbar != null)
                    {
                        Data.Skillbar.Add(MyItem);
                    }
                }
            }
            //yield return new WaitForSeconds(0.01f);
        }
        else if (MySaveFile[i].Contains("/GiveStat "))
        {
            string StatName = ScriptUtil.RemoveCommand(MySaveFile[i]);
            //MySaveFile.RemoveAt(i);
            Debug.Log("Giving Stat [" + StatName + "] to " + name);
            Stat MyStat = DataManager.Get().GetElement("Stats", StatName) as Stat;//MyMaker.MyStatsMaker.Get(StatName);
            if (MyStat != null)
            {
                //MySaveFile.InsertRange(i, MyStat.GetScriptList());
                Data.MyStats.Add(MyStat);
            }
        }
        else if (MySaveFile[i].Contains("/Permission "))
        {
            //MySaveFile.RemoveAt(i);
            string MyInput = ScriptUtil.RemoveCommand(MySaveFile[i]);
            //GetComponent<Character>().PlayerPermissions.Clear();
            //GetComponent<Character>().PlayerPermissions.Add(MyInput);
        }
        else if (MySaveFile[i].Contains("/transform"))
        {
            SetTransformText(MySaveFile.GetRange(i, 8));
            i += 7;
        }
        else if (MySaveFile[i].Contains("/BeginSkeleton"))
        {
            BeginIndex = i;
            LoadType = "Skeleton";
        }
        else if (ScriptUtil.GetCommand(MySaveFile[i]) == "/SkillBar")
        {
            SubLoadType = "SkillBar";
        }
        else if (ScriptUtil.GetCommand(MySaveFile[i]) == "/EndSkillBar")
        {
            SubLoadType = "";
        }
        else if (MySaveFile[i].Contains("/item"))
        {
            BeginIndex = i;
            LoadType = "Item";
        }
        else if (Stats.IsBeginTag(MySaveFile[i]))
        {
            BeginIndex = i;
            LoadType = "Stats";
        }
    }
    else
    {
        if (LoadType == "Skeleton" && ScriptUtil.RemoveWhiteSpace(MySaveFile[i]) == "/EndSkeleton")
        {
            List<string> MySkeletonScript = MySaveFile.GetRange(BeginIndex, i - BeginIndex + 1);
            //Debug.LogError("SkeletonScript: " + MySkeletonScript.Count + ":" + MySkeletonScript[MySkeletonScript.Count-1]);
            //Debug.LogError("Skeleton Section: \n" + FileUtil.ConvertToSingle(MySkeletonScript));
            yield return UniversalCoroutine.CoroutineManager.StartCoroutine(MySkeleton.RunScriptRoutine(MySkeletonScript));
            LoadType = "";
            BeginIndex = -1;
        }
        else if (LoadType == "Item" && ScriptUtil.RemoveWhiteSpace(MySaveFile[i]) == "/EndItem")
        {
            List<string> MyScript = MySaveFile.GetRange(BeginIndex, i - BeginIndex + 1);
            if (SubLoadType == "")
            {
                GetInventory().AddItemScript(MyScript);
            }
            else if (SubLoadType == "SkillBar")
            {
                Data.Skillbar.AddItemScript(MyScript);
            }
            LoadType = "";
            BeginIndex = -1;
        }
        else if (LoadType == "Stats" && Stats.IsEndTag(MySaveFile[i]))
        {
            List<string> MyScript = MySaveFile.GetRange(BeginIndex, i - BeginIndex + 1);
            Data.MyStats.RunScript(MyScript);
            LoadType = "";
            BeginIndex = -1;
        }
    }
    //yield return null;
}

MyGuis.SetNameLabel(name);
GetInventory().OnRunScript();
//yield return new WaitForSeconds(0.01f);

Debug.Log("Character loaded with: \n" +
    "Stats: " + Data.MyStats.GetSize() + "\n" +
    "Inventory Items: " + GetInventory().GetSize() + "\n" +
    "Quests: " + Data.MyQuestLog.GetSize() + "\n" +
    "Dialogue: " + MyDialogueHandler.MyTree.GetSize() + "\n");

ZelGui SkillbarZelGui = MyGuis.GetZelGui("SkillBar");
if (SkillbarZelGui)
{
    Skillbar SkillbarInventory = gameObject.GetComponent<Skillbar>();
    if (SkillbarInventory)
    {
        SkillbarInventory.RefreshSkillbar();
    }
    else
    {
        Debug.LogError(name + " has no skillbar..");
    }
}
#endregion

if (MyStatsBar)
{
    MyStatsBar.SetState(true);
}*/
