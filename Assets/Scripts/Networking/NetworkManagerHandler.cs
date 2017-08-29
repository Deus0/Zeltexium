using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex.Networking
{
    /// <summary>
    /// A handler for the network manager
    /// </summary>
    public class NetworkManagerHandler : MonoBehaviour
    {
        public void HostGame()
        {
            if (NetworkManager.Get())
            {
                NetworkManager.Get().HostGame();
            }
        }
        public void JoinGame()
        {
            if (NetworkManager.Get())
            {
                NetworkManager.Get().JoinGame();
            }
        }
    }

}