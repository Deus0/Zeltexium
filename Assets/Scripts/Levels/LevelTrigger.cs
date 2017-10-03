using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex.Levels
{

    /// <summary>
    /// upon entry, the character gets teleported to another level
    /// </summary>
    public class LevelTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "BonePart")
            {
                // BAm!
                Debug.Log("Character Entered trigger");
            }
            if (other.GetComponent<Characters.Character>())
            {
                // BAm!
                Debug.Log("!Character! Entered trigger");
            }
        }
    }

}