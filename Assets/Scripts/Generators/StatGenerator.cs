using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Voxels;
using Zeltex.Util;
using Zeltex.Combat;
using Zeltex.Items;
using Zeltex;
using ZeltexTools;

namespace Zeltex.Generators
{
    /// <summary>
    /// Generates some spell trees and spells
    /// </summary>
    public class StatGenerator : MonoBehaviour
    {
		public int TextureResolution = 32;
		public TextureGenerator MyTextureGenerator;

		public IEnumerator GenerateData()
        {
			CreateStat("Level",			"An overall indication of power", StatType.Base);
			CreateStat("Speed",				"Causes you to move faster", StatType.Base);
			CreateStat("Experience",		"It grows larger with every monster you slay", StatType.State);
			CreateStat("Health",			"-------", StatType.State);
			CreateStat("Mana",				"-------", StatType.State);
			CreateStat("Energy",			"-------", StatType.State);
			CreateStat("HealthRegen",		"-------", StatType.Regen);
			CreateStat("ManaRegen",			"-------", StatType.Regen);
			CreateStat("EnergyRegen",		"-------", StatType.Regen);
			CreateStat("Strength",			"-------", StatType.Modifier);
			yield break;
        }

		private void CreateStat(string StatName, string StatDescription, StatType MyStatType)
		{
			Stat NewStat = new Stat(MyStatType);
			NewStat.Name = StatName;
			NewStat.Description = StatDescription;
			if (MyStatType == StatType.State)
            {
                int MaxState = Mathf.RoundToInt(Random.Range(8, 18));
                NewStat.SetMax(MaxState);
                NewStat.SetState(Mathf.RoundToInt(MaxState / 2 + Random.Range(-2,2)));
            }
			if (MyStatType == StatType.Regen)
			{
				NewStat.SetRegenValue(Random.Range(0.5f, 3f));    // value per rate
				NewStat.SetRegenCooldown(1);
				if (StatName.Contains("Regen"))
				{
                    string RegenModifier = StatName.Substring(0, StatName.IndexOf("Regen"));
                    Debug.Log(StatName + " now using modifier [" + RegenModifier + "]");
                    NewStat.SetModifier(RegenModifier);
				}
                Debug.Log("Created new: " + StatName + ": " + NewStat.GetGuiString());
			}
			else if (MyStatType == StatType.Modifier)
			{
				NewStat.SetModifierValue(10);
			}
			else
			{
				NewStat.SetValue(10);
			}
            NewStat.OnModified();
            DataManager.Get().AddElement("Stats", NewStat);
			//  Generate a texture for the stat
			//int TextureIndex = DataManager.Get().GetSize("StatTextures");//MyTextureManager.ItemTextures.Count;
			Texture2D NewTexture = new Texture2D(TextureResolution, TextureResolution, TextureFormat.ARGB32, false);
			NewTexture.name = NewStat.Name + "_Texture";
			NewTexture.filterMode = FilterMode.Point;
			MyTextureGenerator.RandomColors();
			//MyTexGen.Noise(MyTextureManager.ItemTextures[TextureIndex]);
			MyTextureGenerator.Circle(NewTexture);
			NewStat.SetTexture(NewTexture);
			//DataManager.Get().AddTexture("StatTextures", NewTexture);
		}
    }
}

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
