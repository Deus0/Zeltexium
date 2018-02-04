using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Characters;
using Zeltex.AI;

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
        /// </summary>
        public class Team : MonoBehaviour
        {
            public KeyCode DebugKey = KeyCode.B;
            public string Name;
            public List<Character> PartyMembers = new List<Character>();
            private Character MyCharacter;

            private void Start()
            {
                MyCharacter = GetComponent<Character>();
            }

            // Update is called once per frame
            void Update ()
            {
		        if (DebugKey != KeyCode.None && Input.GetKeyDown(DebugKey))
                {
                    // spawn team around me
                    //CharacterManager.Get().SpawnCharactersOnCharacter(transform, 5);  // spawns a random unit aroun this one
                    InviteSurroundingCharacters();
                    CommandTeamToFollow();
                }
	        }

            /// <summary>
            /// Commands entire team to follow me
            /// </summary>
            private void CommandTeamToFollow()
            {
                for (int i = 0; i < PartyMembers.Count; i++)
                {
                    Bot MyBot = PartyMembers[i].GetComponent<Bot>();
                    MyBot.FollowTarget(gameObject);
                }
            }

            /// <summary>
            /// Invites the surrounding Characters into the party
            /// </summary>
            private void InviteSurroundingCharacters()
            {
                PartyMembers.Clear();
                float InviteRadius = 5;
                for (int i = 0; i < CharacterManager.Get().GetSize(); i++)
                {
                    Character OtherCharacter = CharacterManager.Get().GetSpawn(i);
                    if (OtherCharacter)
                    {
                        float DistanceToCharacter = Vector3.Distance(OtherCharacter.transform.position, transform.position);
                        if (DistanceToCharacter <= InviteRadius)
                        {
                            PartyMembers.Add(OtherCharacter);
                            OtherCharacter.GetComponent<Team>().PartyMembers.Add(MyCharacter);
                        }
                    }
                }
            }
        }
    }
}