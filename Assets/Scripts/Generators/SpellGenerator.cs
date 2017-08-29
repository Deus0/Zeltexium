using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Voxels;
using Zeltex.Util;
using Zeltex.Combat;
using Zeltex.Items;
using Zeltex;

namespace MakerGuiSystem
{
    /// <summary>
    /// Generates some spell trees and spells
    /// </summary>
    public class SpellGenerator : MonoBehaviour
    {
        //public SpellMaker MySpellMaker;
        //public ItemMaker MyItemMaker;

        public IEnumerator GenerateData()
        {
            Spell NewSpell = new Spell();
            NewSpell.Name = Zeltex.NameGenerator.GenerateVoxelName();
            NewSpell.IsProjectile = true;
            NewSpell.StatCost = 0.5f;
            NewSpell.StatUseName = "Mana";
            NewSpell.Size = 1f;
            NewSpell.LifeTime = 1;
            NewSpell.Randomness = 0.1f;
            NewSpell.FireRate = 0.5f;
            NewSpell.AddStatValue = -3f;
            NewSpell.OnModified();
            DataManager.Get().AddElement("Spells", NewSpell);
            yield break;
        }
    }
}
