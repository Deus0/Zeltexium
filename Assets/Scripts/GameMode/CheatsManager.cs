using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex
{
    /// <summary>
    /// Manages cheating in Zeltex
    /// </summary>
    public class CheatsManager : ManagerBase<CheatsManager>
    {
        public bool IsCheatsEnabled = true;

        [Header("Keys")]
        public KeyCode DamageCheatKey = KeyCode.F3;
        public KeyCode AddDirtKey = KeyCode.F2;

        [Header("References")]
        public Player MyPlayer;

        public void Update()
        {
            if (Input.GetKeyDown(AddDirtKey))
            {
                Characters.Character MyCharacter = MyPlayer.GetCharacter();
                if (MyCharacter)
                { 
                    Items.Item DirtItem = (DataManager.Get().GetElement(DataFolderNames.Items, "Dirt") as Items.Item).Clone() as Items.Item;
                    MyCharacter.GetBackpackItems().Add(DirtItem);
                }
            }
        }

    }

}