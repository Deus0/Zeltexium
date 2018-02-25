using UnityEngine;
using System.Collections;

namespace Zeltex.Physics
{
    /// <summary>
    /// Imposes Gravity Rules on all those in a zone, with the gravity script attached
    /// </summary>
    public class GravityZone : Gravity
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
