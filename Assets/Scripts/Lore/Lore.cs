using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Zeltex
{
    /// <summary>
    /// Lore will be the 2d collectable item of the game
    /// Similar to metroid scanning things, giving more information about everything
    /// Someones lore level will allow them to view more, the more they collect, the more their lore level can grow
    /// 
    /// When reading them, key words will unlock more information about them
    /// For instance - if it is mentioned a special race of elkens with dakr blue skin created the dungeon cores
    /// It will unlock 'clues' you can use to activate other lore
    /// 
    /// Certain lore require things to unlock
    /// Some require quests to be completed or other events
    /// </summary>
    public class Lore : ElementCore
    {
        // A list of descriptions, the level of description depends on the lore level
        [JsonProperty]
        List<string> Descriptions = new List<string>();
    }
}