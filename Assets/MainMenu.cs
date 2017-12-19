using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex.Guis
{
    /// <summary>
    /// The gui for main menu
    /// </summary>
    public class MainMenu : MonoBehaviour
    {

        public void PushAdventureButton()
        {
            GameManager.Get().BeginGame();
        }

        public void PushResourcesButton()
        {
            GameManager.Get().BeginResourcesEditing();
        }

        public void PushSettingsButton()
        {
            GameManager.Get().BeginSettings();
        }
    }

}