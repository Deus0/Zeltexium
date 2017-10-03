using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Characters;

namespace Zeltex.Guis.Characters
{
    public class CharacterGuiHandle : MonoBehaviour
    {
        [SerializeField]
        private Character MyCharacter;

        public void SetCharacter(Character NewCharacter)
        {
            if (MyCharacter != NewCharacter)
            {
                MyCharacter = NewCharacter;
            }
        }

        public Character GetCharacter()
        {
            return MyCharacter;
        }
        
        public void Close()
        {
            MyCharacter.GetGuis().Remove(GetComponent<ZelGui>());
        }
    }
}
