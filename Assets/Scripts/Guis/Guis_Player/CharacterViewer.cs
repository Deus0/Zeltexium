using UnityEngine;
using UnityEngine.UI;
using ZeltexTools;
using Zeltex.AI;
using Zeltex.Characters;
using Zeltex.Voxels;
using Zeltex.Util;
using Zeltex.Game;
using Zeltex;

namespace Zeltex.Guis.Players
{
    /// <summary>
    /// A gui that creates a character to view on the canvas.
    /// </summary>
    public class CharacterViewer : ObjectViewer 
    {
        #region Variables
        [Header("CharacterViewer References")]
        public SpawnPositionFinder MySpawner;   // Used for finding spawn position for character spawning
        public LayerMask DefaultLayer;          // The default layer, set after the character layer    
        #endregion

        /// <summary>
        /// Loop Logic Events - Begin, End etc
        /// </summary>
        public override void OnBegin()
        {
            //base.OnBegin();
            SpawnCharacter();
            AttachCamera();
        }

        /// <summary>
        /// When canceling the character viewer, remove the character
        /// </summary>
        public void OnCancel()
        {
            // Remove the character
            Reset(false);
        }

        /// <summary>
        /// Spawn the character for the character viewer
        /// </summary>
	    private void SpawnCharacter()
        {
            if (GetSpawn() == null)  // if already spawned
            {
                //SetSpawn(CharacterManager.Get().SpawnCharacter(
                //    NameGenerator.GenerateVoxelName(), 
                //    GetSpawnPosition(), 
                //    Quaternion.identity).gameObject);   // online spawning
                if (GetSpawn() != null)
                {
                    //GetSpawn().GetComponent<Character>().SetMovement(false);   // disable movement
                    //SetLayerRecursive(GetSpawn(), LayerManager.Get().ViewerLayer;);
                }
            }
        }

        /// <summary>
        /// When confirming, convert the bot to a player.
        /// Enable all player variables.
        /// Set the layer to the default one.
        /// </summary>
        public void OnConfirm()
        {
            GameObject MyCharacter = GetSpawn();
            if (MyCharacter != null)
            {
                //Debug.LogError("Setting name to: " + (LayerMask.NameToLayer(LayerMask.LayerToName (CharacterLayer))-2));
                //SetLayerRecursive(MyCharacter, DefaultLayer);
                MyCharacter.transform.position = MySpawner.FindNewPosition();
                GetSpawn().GetComponent<Character>().SetMovement(true);   // disable movement
                // convert to player
                Possess[] MyControllers = Camera.main.gameObject.GetComponents<Possess>();
                for (int i = 0; i < MyControllers.Length; i++)
                {
                    if (MyControllers[i].enabled)
                    {
                        MyControllers[i].SetCharacter(MyCharacter.GetComponent<Character>());
                        break;
                    }
                }
            }
            Reset(true);
        }

        /// <summary>
        /// Destroy the camera and detach skeleton.
        /// </summary>
        /// <param name="IsKeepCharacter"></param>
        public void Reset(bool IsKeepCharacter)
        {
            if (!IsKeepCharacter)
            {
                Clear();
            }
            else
            {
                ClearOthers();
                SetSpawn(null); // detatches spawn
            }
        }
    }
}
