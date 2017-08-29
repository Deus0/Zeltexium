using UnityEngine;
using System.Collections.Generic;
using Zeltex.Characters;
using Zeltex.Util;


namespace Zeltex.Characters
{

    /// <summary>
    /// Synches characters on the network
    /// To be replaced soon, any functionality to be consumed by character class
    /// </summary>
    public class CharacterSpawnerNetwork : MonoBehaviour
    {
        /*private PhotonPlayer GetPlayerByID(int ID)
        {
            for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
            {
                if (PhotonNetwork.playerList[i].ID == ID)
                    return PhotonNetwork.playerList[i];
            }
            return null;
        }
        private PhotonPlayer GetPlayerByName(string PlayerName)
        {
            Debug.LogError("Searching for photon player: " + PlayerName);
            for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
            {
                if (PhotonNetwork.playerList[i].name == PlayerName)
                    return PhotonNetwork.playerList[i];
            }
            return null;
        }

        private Transform FindCharacterWithID(int ViewID)
        {
            // try this instead
            PhotonView MyView = PhotonView.Find(ViewID);
            if (MyView)
                return MyView.transform;
            else
                return null;
        }*/
        // this is a character recieving all the data for each character loaded in the map
        // off that player
        // say john joins game, and mark is already in the game, this will be updating johns characters on marks machine

        // now tell all the other players to synch their characters
        /*[PunRPC]
        public void SynchCharacterData(int PlayerID)
        {
            int CurrentID = PhotonNetwork.player.ID;

            Debug.LogError("Synching characters from: " + CurrentID);
            // Get all characters in scene
            Zeltex.Characters.Character[] MyCharacters = FindObjectsOfType(typeof(Zeltex.Characters.Character)) as Zeltex.Characters.Character[];
            // if owned by player

            for (int i = 0; i < MyCharacters.Length; i++)
            {
                // if character is active
                if (MyCharacters[i].gameObject.activeSelf)
                {
                    PhotonView MyCharactersView = MyCharacters[i].GetComponent<PhotonView>();
                    if (MyCharactersView)
                        // if character is owned by this player
                        if (MyCharactersView.owner.ID == CurrentID)
                        {
                            //Debug.LogError ("\t- character from: " + CurrentPlayerName + " with character: " + MyCharactersView.name);
                            gameObject.GetComponent<PhotonView>().RPC("UpdateCharacterDataToNewClient",
                                GetPlayerByID(PlayerID),
                                MyCharactersView.gameObject.name,
                                FileUtil.ConvertToSingle(MyCharactersView.gameObject.GetComponent<CharacterSaver>().GetScript()),
                                MyCharactersView.viewID,
                                MyCharacters[i].GetComponent<GuiSystem.GuiManager>().GetZelGuisPacket()
                            );
                        }
                }
            }
        }

        // recieved just by the player with the character
        [PunRPC]
        public void UpdateCharacterDataToNewClient(string NewName, string MyCharacterScript, int ViewID, string ZelGuisPacket)
        {
            Transform MyCharacterTransform = FindCharacterWithID(ViewID);
            //Debug.LogError ("Inside UpdateCharacterDataToNewClient function with " + PhotonNetwork.playerName);
            if (MyCharacterTransform == null)
            {
                Debug.LogError("Could not find character with " + ViewID);
                return;
            }
            //MyPlayerSpawner.ConvertToPlayer(MyCharacterTransform);  // converts it!

            GameObject MyCharacter = MyCharacterTransform.gameObject;
            // fixes up guis too
            UpdateCharacterData(
                MyCharacterTransform, 
                NewName, 
                FileUtil.ConvertToList(MyCharacterScript)
                );
            MyCharacterTransform.GetComponent<GuiSystem.GuiManager>().SetZelGuisPacket(ZelGuisPacket);
        }

        private void UpdateCharacterData(Transform MyCharacter, string NewName, List<string> MyData)
        {
            MyCharacter.GetComponent<Character>().Initialize(true, NewName);
            MyCharacter.GetComponent<CharacterSaver>().RunScript(MyData);
        }*/

    }
}