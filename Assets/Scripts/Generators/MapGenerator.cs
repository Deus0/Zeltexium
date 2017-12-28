using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using MakerGuiSystem;
using Zeltex.Items;
using Zeltex.Util;
using Zeltex.Guis;
using Zeltex.Characters;
using Zeltex.Skeletons;
using Zeltex.Dialogue;
using Zeltex.Voxels;
using Zeltex.MakerGuiSystem;

namespace Zeltex.Generators
{
    /// <summary>
    /// Generate map resources
    /// </summary>
    public class MapGenerator : ManagerBase<MapGenerator>
    {
        #region Variables
        private static MapGenerator Instance;
        [Header("Generators")]
        public List<GeneratorBase> MyGenerators;
        // Meta
        public StatGenerator MyStatGenerator;
        public VoxelMetaGenerator MyVoxelMetaGenerator;
        public ItemGenerator MyItemGenerator;
        public SpellGenerator MySpellGenerator;
        public DialogueGenerator MyDialogueGenerator;
        public ClassGenerator MyClassGenerator;
        // Art
        public PolyModelGenerator MyPolyModelGenerator;
        public SkeletonGenerator MySkeletonGenerator;
        public SoundGenerator MySoundGenerator;
        [Header("UI")]
        public ZelGui MyGui;
        public Text MyStatusText;
        static float LoadingDelay = 0;
        [Header("Events")]
        public UnityEvent OnGenerateMap;    // update the statistics! hopefully - map maker
        #endregion

        public void GenerateMap()
        {
            StartCoroutine(GenerateMapRoutine());
        }

        /// <summary>
        /// Creates the meta data
        /// </summary>
        public IEnumerator GenerateMapRoutine()
        {
            DataManager.Get().ClearAll();
            for (int i = 0; i < MyGenerators.Count; i++)
            {
                yield return MyGenerators[i].Generate();
            }
            yield return MyStatGenerator.GenerateData();
            SetStatusText("Generating Block Meta");
            yield return MyVoxelMetaGenerator.GenerateData(LoadingDelay);        // all the voxel data - including the models
            yield return new WaitForSeconds(LoadingDelay);

            SetStatusText("Generating Dialogue");
            yield return MyDialogueGenerator.GenerateData();
            yield return new WaitForSeconds(LoadingDelay);

            SetStatusText("Generating Spells");
            yield return MySpellGenerator.GenerateData();
            yield return new WaitForSeconds(LoadingDelay);

            // generate the voxel models - after voxel meta and polygonal models are done - base it on them
            SetStatusText("Generating PolyModels");
            yield return MyPolyModelGenerator.GenerateData();
            yield return new WaitForSeconds(LoadingDelay);

            SetStatusText("Generating Voxel Items");
            yield return MyItemGenerator.GenerateData();   // voxels will drop these items when destroyed
            yield return new WaitForSeconds(LoadingDelay);

            SetStatusText("Generating Classes");
            yield return MyClassGenerator.GenerateData();
            yield return new WaitForSeconds(LoadingDelay);

            SetStatusText("Generating Skeletons");
            MySkeletonGenerator.Generate();
            yield return new WaitForSeconds(LoadingDelay);
            yield return MySoundGenerator.GenerateData();

            OnGenerateMap.Invoke(); // Boom!
        }

        public void GenerateMapAndSave()
        {
            StartCoroutine(GenerateMapAndSaveRoutine());
        }

        private IEnumerator GenerateMapAndSaveRoutine()
        {
            yield return GenerateMapRoutine();
            Zeltex.DataManager.Get().SaveAll();
        }

        private void SetStatusText(string NewText)
        {
            if (MyStatusText)
            {
                MyStatusText.text = NewText;
                Debug.Log(MyStatusText.text);
            }
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