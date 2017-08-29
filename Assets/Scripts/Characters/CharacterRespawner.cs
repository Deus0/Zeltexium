using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Voxels;
using Zeltex;
using Zeltex.Guis;

namespace Zeltex.Characters
{
    /// <summary>
    /// Respawns characters after death.
    /// </summary>
    public class CharacterRespawner : ManagerBase<CharacterRespawner>
    {
        #region Variables
        [Header("References")]
	    [Tooltip("Link to the parent gameobject of the respawn gui")]
	    public ZelGui RespawnGui;
	    [Tooltip("Link to the text in the gui")]
	    public Text MyText;
        public SpawnPositionFinder MyPositionFinder;
        [Header("Audio")]
        public AudioSource MyAudioSource;
        [Tooltip("The maximum sound of respawning")]
	    public float MaxVolume = 5f;
	    [Tooltip("The sound it makes when spawning a character")]
	    public AudioClip OnSpawnSound;	// Rise05
	    [Tooltip("The sound it makes before spawning a character")]
	    public AudioClip OnTickSound;	// clock-1
        [Header("Spawning")]
        [Tooltip("How long it takes the player to respawn.")]
        public float RespawnTime = 10f;
        public List<float> MyRespawnTimes = new List<float>();
        public List<string> MyCharacterRespawners = new List<string>();
        // privates
        bool IsEndRespawning = false;   // ends respawning if game ends
        #endregion

        #region Spawning
        private void PlayerSpawnSound() 
	    {
		    if (MyAudioSource) 
		    {
			    if (OnSpawnSound) 
			    {
                    MyAudioSource.PlayOneShot(OnSpawnSound);
			    }
		    }
        }

        public void AttachCharacter(Character MyCharacter)
        {
            Camera.main.gameObject.GetComponent<Player>().SetCharacter(MyCharacter);
        }


	    public void RevivePlayer(GameObject MyCharacter) 
	    {
            if (MyCharacter.GetComponent<Character>().CanRespawn() == false)
                return;
            IsEndRespawning = false;
		    bool IsPlayer = MyCharacter.GetComponent<Player> ();
		    if (IsPlayer)
            {
                RespawnGui.gameObject.GetComponent<ZelGui>().TurnOn();     // turn gui on when reviving begins
            }
            List<string> MyScript = MyCharacter.GetComponent<Character>().GetScript();
            //MyCharacter.GetComponent<CharacterSaver>().Delete();
            StartCoroutine (RespawnCharacter2(MyCharacter.name,
                                            MyScript,
                                            RespawnTime, 
										    MyCharacter.transform.position, 
										    IsPlayer));
            // MyCharacter.GetComponent<Character>().LastSavedFileName));
        }

        /// <summary>
        /// Repsawns a character
        /// </summary>
        private IEnumerator RespawnCharacter2(string MyCharacterName,
                                            List<string> MyScript,
                                            float RespawnTime,
                                            Vector3 MyPosition,
                                            bool IsPlayer)
        {
            MyCharacterRespawners.Add(MyCharacterName);
            MyRespawnTimes.Add(RespawnTime);
            //UnityEngine.UI.Text MyText = transform.GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Text>();
            if (IsPlayer)
            {
                RespawnGui.TurnOn();
            }
            for (float i = RespawnTime; i >= 0; i--)
            {
                if (IsPlayer)
                {
                    MyAudioSource.PlayOneShot(OnTickSound, MaxVolume - MaxVolume * (i / RespawnTime));
                    MyText.text = "[" + (i) + "]";
                }
                if (IsEndRespawning)
                {
                    yield break;
                }
                yield return new WaitForSeconds(1);
            }
            if (IsEndRespawning)
            {
                yield break;
            }
            if (IsPlayer)
            {
                MyAudioSource.Stop();
            }
            PlayerSpawnSound(); // need to create a sound where the plaeyr spawns

            Character MyCharacter;
            if (IsPlayer)
            {
                MyCharacter = CharacterManager.Get().GetPoolObject();// MyCharacterName, MyPosition);
                AttachCharacter(MyCharacter.GetComponent<Character>());
                //MyCharacter.GetComponent<Character>().AttachCamera(true);
                //Camera.main.gameObject.GetComponent<Player>()
                RespawnGui.TurnOff();
            }
            else
            {
                MyCharacter = CharacterManager.Get().GetPoolObject();// MyCharacterName, MyPosition);
            }
            //MyCharacter.GetComponent<Character>().LastSavedFileName = LastSaveFile;
            yield return MyCharacter.GetComponent<Character>().RunScriptRoutine(MyScript);

            if (MyPositionFinder)
            {
                MyPosition = MyPositionFinder.FindNewPosition();
            }
            MyCharacter.transform.position = MyPosition;
            //MyCharacter.GetComponent<CharacterSaver> ().StartSaving ();
        }

        public void StopRespawning()
	    {
		    IsEndRespawning = true;
        }
        #endregion
    }
}