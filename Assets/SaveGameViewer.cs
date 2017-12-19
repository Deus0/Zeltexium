using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Zeltex.Saves
{
    public class SaveGameViewer : MonoBehaviour
    {
        private SaveGame MySaveGame;
        public Text NameText;
        public Text LevelText;
        public Text CharacterText;

        public void RefreshUI(SaveGame NewSaveGame = null, bool IsForce = false)
        {
            if (NewSaveGame != null || IsForce)
            {
                MySaveGame = NewSaveGame;
            }
            if (MySaveGame != null)
            {
                NameText.text = MySaveGame.Name;
                CharacterText.text = MySaveGame.CharacterName;
                LevelText.text = MySaveGame.LevelName;
            }
            else
            {
                NameText.text = "";//.Name;
                CharacterText.text = "";//MySaveGame.CharacterName;
                LevelText.text = "";//MySaveGame.LevelName;
            }
        }
    }

}