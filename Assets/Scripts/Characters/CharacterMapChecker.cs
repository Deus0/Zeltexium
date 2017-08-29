using System.Collections;
using UnityEngine;
using Zeltex.AI;
using Zeltex.Voxels;

namespace Zeltex.Characters
{
    /// <summary>
    /// Keeps the character inside the map upon loading.
    /// </summary>
    public class CharacterMapChecker : MonoBehaviour
    {
        private Vector3 BeginPosition;
        //private World MyWorld;
        private Chunk MyChunk;
        private Character MyCharacter;
        private bool isCheckingChunk = true;

        // Use this for initialization
        void Start()
        {
            MyCharacter = GetComponent<Character>();
            UpdateReferences();
        }

        void Update()
        {
            if (isCheckingChunk)
            {
                UpdateReferences();
                FreezePosition();
            }
        }

        /// <summary>
        /// Checks world references
        /// </summary>
        private void UpdateReferences()
        {
            /*if (MyWorld == null)
            {
                MyWorld = MyCharacter.GetWorldInsideOf();
            }
            if (MyWorld != null)
            {
                if (MyChunk == null)
                {
                    BeginPosition = transform.position;
                    Int3 ChunkPosition = MyWorld.GetChunkPosition(BeginPosition.ToInt3());
                    MyChunk = MyWorld.GetChunk(ChunkPosition);
                }
            }*/
            if (MyChunk == null)
            {
                MyChunk = MyCharacter.GetChunkInsideOfRaw();
                if (MyCharacter)
                {
                    BeginPosition = transform.position;
                }
            }
        }
        /// <summary>
        /// Freezes the position of the character inside the map
        /// </summary>
        private void FreezePosition()
        {
            if (MyChunk != null)
            {
                transform.position = BeginPosition;
                if (MyCharacter)
                {
                    MyCharacter.SetMovement(false);
                }
                if (MyChunk.HasLoaded())
                {
                    isCheckingChunk = false;
                    StartCoroutine(EnableCharacterMovement());
                }
            }
            else
            {
                if (MyCharacter)
                {
                    MyCharacter.SetMovement(false);
                }
            }
        }

        private IEnumerator EnableCharacterMovement()
        {
            yield return new WaitForSeconds(0.5f);
            if (MyCharacter)
            {
                MyCharacter.SetMovement(true);
            }
            enabled = false;
        }
    }
}