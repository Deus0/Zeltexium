using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Characters;

/*namespace PlayerGuiSystem
{
    public class PlayerScoreGui : MonoBehaviour
    {
        public GUISkin MySkin;
        private KeyCode MyScoreKey = KeyCode.Tab;

        void OnGUI()
        {
            // show all players and their scores
            if (Input.GetKey(MyScoreKey) &&
                PhotonNetwork.connected && !PhotonNetwork.insideLobby)
            {
                GUI.skin = MySkin;
                List<GameObject> MyCharacters = CharacterManager.Get().GetSpawned();
                for (int i = 0; i < MyCharacters.Count; i++)
                {
                    GUILayout.Label("[" + MyCharacters[i].GetComponent<PhotonView>().owner.name + "]:["
                        + MyCharacters[i].name
                        + "]\t-Score [" + MyCharacters[i].GetComponent<Character>().GetScore() + "]");
                }
            }
        }

        private void DisplayPlayer(PhotonPlayer MyPlayer)
        {
            GUILayout.Label("[" + MyPlayer.ID + "]:["
                + MyPlayer.name
                + "]\tScore [" + MyPlayer.GetScore() + "]");
        }
    }
}
*/