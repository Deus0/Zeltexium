using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex
{
    namespace AI
    {

        /// <summary>
        /// The object that commands a group of characters
        /// Managers:
        ///     - Formations
        ///     - Statistical Information about entire Group
        ///     - Group Shared Goals (more gold, etc)
        /// </summary>j
        public class Party : MonoBehaviour
        {
            public bool IsDebug;

	        // Use this for initialization
	        void Start ()
            {
		
	        }
	
	        // Update is called once per frame
	        void Update ()
            {
		        if (IsDebug && Input.GetKeyDown(KeyCode.B))
                {

                }
	        }

            /// <summary>
            /// Invites the surrounding Characters into the party
            /// </summary>
            private void InviteSurroundingCharacters()
            {

            }
        }
    }
}