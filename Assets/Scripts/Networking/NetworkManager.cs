using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Zeltex.Characters;
using Zeltex;
using System.Reflection;
using UnityEngine.Networking;

namespace Zeltex.Networking
{
    /// <summary>
    /// Things to do:
    /// List characters in gui
    /// List Players in gui
    /// </summary>
    public class NetworkManager : UnityEngine.Networking.NetworkManager
    {
        private bool IsHost;
        private static NetworkManager Instance;
        [SerializeField]
        private bool IsDebugGui = false;
        [SerializeField]
        private bool IsPossessOnBegin = true;

        public static NetworkManager Get()
        {
            return Instance;
        }

        private void Awake()
        {
            Instance = this;
        }

        #region GUI
        private void OnGUI()
        {
            if (IsDebugGui)
            {
                GUILayout.Space(100);
                if (!NetworkClient.active && !NetworkServer.active && matchMaker == null)
                //if (client != null && client.isConnected)
                {
                    if (GUILayout.Button("Host"))
                    {
                        HostGame();
                    }
                    if (GUILayout.Button("Join"))
                    {
                        JoinGame();
                    }
                }
               /* else
                {
                    if (NetworkServer.active)
                    {
                        GUILayout.Label("Server: port=" + networkPort);
                    }
                    if (NetworkClient.active)
                    {
                        GUILayout.Label("Client: address=" + networkAddress + " port=" + networkPort);
                    }
                }*/

                /*if (NetworkClient.active && !ClientScene.ready)
                {
                    if (GUILayout.Button("Client Ready"))
                    {
                        ClientScene.Ready(client.connection);
                        if (ClientScene.localPlayers.Count == 0)
                        {
                            ClientScene.AddPlayer(0);
                        }
                    }
                }*/

                if (NetworkServer.active || NetworkClient.active)
                {
                    if (IsHost)
                    {
                       /* if (GUILayout.Button("Spawn Objects"))
                        {
                            NetworkServer.SpawnObjects();
                        }
                        if (GUILayout.Button("Clear Objects"))
                        {
                            NetworkServer.ClearLocalObjects();
                        }
                        */
                        if (GUILayout.Button("Stop Host"))
                        {
                            StopHosting();
                        }
                    }
                    else
                    {
                       /* if (GUILayout.Button("Exit Game"))
                        {
                            Camera.main.GetComponent<Possess>().RemoveCharacter(true);
                            Game.GameMode.IsPlaying = false;
                            StopClient();
                        }*/
                    }
                }
                /*GUILayout.Label("Me: " + Network.natFacilitatorIP + " -:- " + Network.natFacilitatorPort);
                for (int i = 0; i < Network.connections.Length; i++)
                {
                    GUILayout.Label(i + ": " + Network.connections[i].ipAddress + " -:- " + Network.connections[i].port);
                }*/
            }
        }
        
        public void HostGame()
        {
//#if UNITY_WEBGL
           // useWebSockets = true;
//#endif
            //Debug.LogError("Hosting from " + name);
            IsHost = true;
            StartHost();
           // GameManager.Get().BeginGame();
        }

        public void StopHosting()
        {
            IsHost = false;
            /*if (CameraManager.Get().GetMainCamera() && CameraManager.Get().GetMainCamera().GetComponent<Possess>())
            {
                CameraManager.Get().GetMainCamera().GetComponent<Possess>().RemoveCharacter(true);
            }*/
            //GameManager.Get().EndGame();
            StopHost();
        }

        public void JoinGame()
        {
/*#if UNITY_WEBGL
            useWebSockets = true;
#endif*/
            StartClient();
        }

        private void CleanupPlayer()
        {
            Camera.main.GetComponent<Possess>().RemoveCharacter(true);
            if (CameraManager.Get().GetGuiCamera())
            {
                CameraManager.Get().GetGuiCamera().gameObject.SetActive(true);
            }
        }

        private void DrawConenctedGui()
        {
            /*if (Network.isServer)
            {
                if (GUILayout.Button("Stop Hosting"))
                {
                    CleanupPlayer();
                    for (int i = 0; i < Network.connections.Length; i++)
                    {
                        Network.CloseConnection(Network.connections[i], true);
                    }
                    IsHost = false;
                    //MyClient.Disconnect();
                    NetworkServer.DisconnectAll();
                    Network.Disconnect();
                }
            }
            else
            {
                if (GUILayout.Button("Exit Game"))
                {
                    CleanupPlayer();
                    for (int i = 0; i < Network.connections.Length; i++)
                    {
                        Network.CloseConnection(Network.connections[i], true);
                    }
                    //StopClient();
                    //MyClient.Disconnect();
                    Network.Disconnect();
                }
            }*/

            /*if (GUILayout.Button("Spawn"))
            {
                if (Network.isServer)
                {
                    OnServerAddPlayer(client.connection, 1);
                }
                else
                {
                    ClientScene.AddPlayer(client.connection, 1);
                }
            }*/
        }
        #endregion

        public override void OnServerConnect(NetworkConnection conn)
        {
            LogManager.Get().Log("New client connected, ID: " + conn.connectionId.ToString(), "NetworkManager");
        }
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            LogManager.Get().Log("Client disconnected, ID: " + conn.connectionId.ToString(), "NetworkManager");
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            LogManager.Get().Log("Connected to Server connectionId: " + conn.connectionId, "NetworkManager");
            base.OnClientConnect(conn);
            StartCoroutine(OnClientConnectRoutine());
        }

        private IEnumerator OnClientConnectRoutine()
        {
            yield return new WaitForSeconds(0.2f);
            PoolsManager.Get().SynchPools.Invoke();
            if (IsPossessOnBegin)
            {
                yield return new WaitForSeconds(1f);
                Character MyCharacter = CharacterManager.Get().GetPoolObject();
                yield return new WaitForSeconds(1f);
                if (MyCharacter)
                {
                    Camera.main.GetComponent<Possess>().SetCharacter(MyCharacter);
                }
                else
                {
                    Debug.LogError("Could not get character from pool");
                }
            }
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            LogManager.Get().Log("Disconnected from Server.", "NetworkManager");
            if (Camera.main == null)
            {
                CameraManager.Get().SpawnCamera();
            }
        }
    }
}