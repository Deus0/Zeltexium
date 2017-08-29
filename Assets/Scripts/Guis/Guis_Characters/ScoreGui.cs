using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Characters;
using Zeltex.Util;

namespace Zeltex.Guis.Characters
{
    /// <summary>
    /// Gui for score board for Characters/Players
    /// </summary>
    public class ScoreGui : MonoBehaviour
    {
        [Header("References")]
        public Text NamesList;
        public Text KillsList;
        public Text DeathsList;
        public Text AssistsList;
        // when there is a team, put it in the names list, then make everything else space

        /// <summary>
        /// Using ZelGuis OnBegin
        /// </summary>
        public void OnBegin()
        {
            List<Character> MyCharacters = CharacterManager.Get().GetSpawned();
            List<string> MyNames = new List<string>();
            List<string> MyKills = new List<string>();
            List<string> MyDeaths = new List<string>();
            List<string> MyAssists = new List<string>();
            for (int i = 0; i < MyCharacters.Count; i++)
            {
                MyNames.Add(MyCharacters[i].name);
                MyKills.Add("" + MyCharacters[i].GetComponent<Character>().GetScore());
                MyDeaths.Add("" + 0);// MyCharacters[i].GetComponent<Character>().GetDeaths());
                MyAssists.Add("" + 0);// MyCharacters[i].GetComponent<Character>().GetAssists());
                //"[" + MyCharacters[i].GetComponent<PhotonView>().owner.name + "]:["
                //   + MyCharacters[i].name
                //    + "]\t-Score [" + MyCharacters[i].GetComponent<Character>().GetScore() + "]");
            }
            NamesList.text = FileUtil.ConvertToSingle(MyNames);
            KillsList.text = FileUtil.ConvertToSingle(MyKills);
            DeathsList.text = FileUtil.ConvertToSingle(MyDeaths);
            AssistsList.text = FileUtil.ConvertToSingle(MyAssists);
        }
    }
}