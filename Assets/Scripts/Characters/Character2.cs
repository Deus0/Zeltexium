
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
