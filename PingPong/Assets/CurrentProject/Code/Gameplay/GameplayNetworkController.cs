using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;

namespace PM.PingPong.Gameplay
{
    public class GameplayNetworkController : NetworkBehaviour
    {
        private Dictionary<ulong, bool> clientReadyState = new Dictionary<ulong, bool>();

        [SerializeField]
        private PingPongController PingPongController;


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsHost)
            {
                foreach (var client in NetworkManager.Singleton.ConnectedClients)
                {
                    clientReadyState[client.Key] = false;
                }
            }

            PingPongController.OnQuitRequest += QuitGame;
            
            SetClientReadyServerRpc(NetworkManager.Singleton.LocalClientId);
            
            NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnectedHandler;
        }

        private void OnDisconnectedHandler(ulong obj)
        {
            if (!PingPongController.IsGameOver)
                QuitGame();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            PingPongController.OnQuitRequest -= QuitGame;
            if (NetworkManager.Singleton)
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnectedHandler;
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetClientReadyServerRpc(ulong clientId)
        {
            Debug.LogError($"ready {clientId}");
            clientReadyState[clientId] = true;

            TryStartGame();
        }

        private void TryStartGame()
        {
            foreach (var pair in clientReadyState)
            {
                if (!pair.Value)
                    return;
            }

            PingPongController.StartGame();
        }

        private void QuitGame()
        {
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }
    }
}