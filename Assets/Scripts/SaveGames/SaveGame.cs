using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Zeltex.Characters;

namespace Zeltex
{
    /// <summary>
    /// All the meta data of a save game
    ///     the Variation of the levels is saved in the SaveGame folder
    /// </summary>
    public class SaveGame : Element
    {
        // The level the save game is last on, if blank loads the first level
        [JsonProperty]
        public string LevelName = "";
        // The character the player last used, if blank loads the chracter creator
        [JsonProperty]
        public string CharacterName = "";
        [JsonIgnore]
        public Character MyCharacter;

        public SaveGame()
        {
            // Set level to newest
            List<string> LevelNames = DataManager.Get().GetNames(DataFolderNames.Levels);
            if (LevelNames.Count > 0)
            {
                LevelName = LevelNames[0];
            }
        }

        public Level GetLevel()
        {
            Level MyLevel = DataManager.Get().GetElement(DataFolderNames.Levels, LevelName) as Level;
            return MyLevel;
        }

        public void SetCharacter(Character NewCharacter)
        {
            MyCharacter = NewCharacter;
            if (MyCharacter)
            {
                CharacterName = MyCharacter.name;
            }
            else
            {
                CharacterName = "";
            }
        }
    }

    /*[System.Serializable]
    public class SaveGame
    {
        public string LevelName;        // level that character was last in
        public string CharacterScript;  // A string version of our character
    }*/
}