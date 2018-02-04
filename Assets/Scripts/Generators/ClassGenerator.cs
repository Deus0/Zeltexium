using UnityEngine;
using System.Collections;
using MakerGuiSystem;
using Zeltex.Items;
using Zeltex.Util;
using Zeltex;
using Zeltex.Combat;

namespace Zeltex.Characters
{
    /// <summary>
    /// Generates Character Classes.
    /// To Do:
    ///     - heirarchy of classes and subclasses -ie melee -> warrior,rogue,pallidan mage->archmage,warlock,priest
    /// </summary>
    public class ClassGenerator : MonoBehaviour
    {
        [Header("References")]
        float PauseTime = 0;

        /// <summary>
        /// Generates Classes
        /// </summary>
        public IEnumerator GenerateData()
        {
			string NewClassName = Zeltex.NameGenerator.GenerateVoxelName();
			string MyScript = "";
			MyScript += "/Class " + NewClassName + "\n";
			MyScript += GetDefaultStats() + "\n";
			MyScript += "/SkillBar\n";
			MyScript += AddRandomInventory();
			MyScript += "/EndSkillBar\n";
			// MyScript1 += "/GiveDialogue " + MyDialogueMaker.MyNames[0];
			//MyClassMaker.AddData(Zeltex.NameGenerator.GenerateVoxelName(), MyScript1);
			//Debug.LogError("Adding new class:\n" + MyScript);
            //DataManager.Get().Add("Classes", NewClassName, MyScript);
            yield return new WaitForSeconds(PauseTime);

            /*string MyScript2 = GetDefaultStats();
            MyScript2 += "/SkillBar\n";
            MyScript2 += AddRandomInventory();
            MyScript2 += "/EndSkillBar\n";
            MyScript2 += MyDialogueMaker.MyData[0];
            MyClassMaker.AddData(Zeltex.NameGenerator.GenerateVoxelName(), MyScript2);
            yield return new WaitForSeconds(PauseTime);*/

           /* MyClassMaker.MyData.Add(GetDefaultStats());
            MyClassMaker.MyNames.Add(Zeltex.NameGenerator.GenerateVoxelName());
            yield return new WaitForSeconds(PauseTime);*/
        }

        /// <summary>
        /// Adds an inventory to the character class
        /// </summary>
        public string AddRandomInventory()
        {
            string MyScript = "";
            int ItemsSize = DataManager.Get().GetSizeElements("ItemMeta");
            // first add a spell
            for (int i = 0; i < ItemsSize; i++)
            {
                //Item MyItem = DataManager.Get().GetElement("ItemMeta", i) as Item;
                /**f (MyItem.HasCommand("/Spell"))
                {
                    MyScript += "/GiveItem " + MyItem.Name + "\n";
                    break;
                }*/
            }
            // Give 3 blocks
            if (ItemsSize > 0)
            {
                //int ResouceCount = 0;
                //for (int i = 0; i < ItemsSize; i++)
                {
                    //Item MyItem = DataManager.Get().GetElement("ItemMeta", i) as Item;
                    /*if (MyItem.HasCommand("/Block"))
                    {
                        MyScript += "/GiveItem " + MyItem.Name + "\n";
                        ResouceCount++;
                        if (ResouceCount >= 3)
                        {
                            break;
                        }
                    }*/
                }
            }
            return MyScript;
        }

        /// <summary>
        /// Gets the default game stats, given to each character
        /// </summary>
        /// <returns></returns>
        public string GetDefaultStats()
        {
			string MyScript = "";
			int StatsSize = DataManager.Get().GetSizeElements("Stats");
			for (int i = 0; i < StatsSize; i++)
			{
				Stat MyStat = (DataManager.Get().GetElement("Stats", i) as Stat);
				MyScript += "/GiveStat " + MyStat.Name + "\n";
			}
			return MyScript;
			/*string MyScript =
@"
/characterstats
/Base Level,1
/Description An overall indication of power.
/Base Speed,1
/Description Causes you to move faster.
/State Experience,0,10
/Description Experience is giving for completing quests or slaying monsters.
/State Health,15,15
/Description Lets you not die for longer
/LoadTexture Health
/State Mana,10,10
/Description Allows you to manapulate magic
/LoadTexture Mana
/State Energy,5,10
/Description Lets you run through mountains
/LoadTexture Energy
/Regen HealthRegen,Health,1,1
/Description Regrows you into a stronger man
/LoadTexture Health
/Regen ManaRegen,Mana,1,1
/Description Regrows you into a stronger man
/LoadTexture Mana
/Regen EnergyRegen,Energy,1,1
/Description Regrows you into a stronger man
/LoadTexture Energy
/Modifier Strength,3,Health,5
/Description Lets you move mountains.
/Modifier Vitality,1,HealthRegen,1
/Description Let's you overcome unfavourable odds.
/Modifier Intelligence,2,Mana,5
/Description Strengthens your brainmuscle.
/Modifier Wisdom,1,ManaRegen,1
/Description Opens new paths for your future.
/Modifier Agility,2,Energy,5
/Description Makes your body move more efficiently.
/Modifier Dexterity,1,EnergyRegen,1
/Description Allows you to run for longer.
/Dot Renew, 1, Health, 3, 15
/Description It hurts. But it's for the best.
/Buff Blessing,1,Strength,30
/Description Rawwwwwrr.
/endstats

";*/
			//MyScriptText.text = MyScript;
		}
    }
}

/*
// Buff variables
/Buff Rage,10,Strength,10
/Description Rawwwwwrr.
/Buff Brains,10,Intelligence,10
/Description Brainnnss....
/Buff Quickness,10,Agility,10
/Description Schhwoooo
/Buff Stun,-20,Speed,15
/Description Rawr
/Buff Slowness,-6,Speed,35
/Description Grrrr
/Dot Burn, -4, Health, 1, 30
/Description It hurts.
*/

/*


/SkillBar
/item Command
/description Let's you remember who you are. One who commands others.
/commands
/Commander
/endcommands
/tags
Voice
/endtags
/LoadTexture Texture_0

/item Fireball
/description Magic flows through your veins, controlling it, transforming it into fire, you lunge it at your enemies.
/commands
/Spell Fireball
/endcommands
/tags
Spell
/endtags
/LoadTexture Texture_1

/item Frostbolt
/description Slow down everything so it meets your own pace.
/commands
/Spell Frostbolt
/endcommands
/tags
Spell
/endtags
/LoadTexture Texture_6

/item Summoner
/description After unlocking the secrets of magic, you have gained the ability to use spatial magic. You use it to call forth the power of a level 1 demonic goblin.
/commands
/Summoner
/endcommands
/tags
Spell
/endtags
/LoadTexture Texture_4

/EndSkillBar

/item Sword
/description After unlocking the secrets of magic, you have gained the ability to use spatial magic. You use it to call forth the power of a level 1 demonic goblin.
/commands
/Sword
/endcommands
/tags
Weapon
/endtags
/LoadTexture Texture_4

/item Pickaxe
/description Smelted out or copper and spare rocks. A very crude looking mining pick.
/commands
/Pickaxe
/endcommands
/tags
Tool
/endtags
/LoadTexture Texture_2

/item Sheild
/description Using mana to sheild your body
/commands
/Sheild
/endcommands
/tags
Spell
/endtags
/LoadTexture Texture_3

    */