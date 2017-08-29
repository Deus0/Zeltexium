using UnityEngine;
using System.Collections;

namespace Zeltex.AI
{

    public class GravityZone : ArtificialGravity
    {
        void Update()
        {

        }

        /// <summary>
        /// All objects that enter the zone will have special gravity applied
        /// </summary>
        void ApplyGravity()
        {

        }

	    public void OnCharacterEnter(GameObject MyCharacter)
        {
		    if (MyCharacter.GetComponent<Zeltex.Characters.Character> ())
            {
			    enabled = true;
		    }
	    }
    }
}
