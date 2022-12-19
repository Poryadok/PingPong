using System;
using System.Collections;
using System.Collections.Generic;
using PM.PingPong.Gameplay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PM.PingPong.MainMenu
{
    public class MainMenuNetworkController : MonoBehaviour
    {
        [SerializeField]
        private ModeSettings settings;
        
        private  NetworkManager NetworkManager;
        private  UnityTransport Transport;
        
        private readonly string gameplaySceneName = "Gameplay";

        public event System.Action OnHostingSuccess;

        private void Start()
        {
            NetworkManager = NetworkManager.Singleton;
            Transport = NetworkManager.GetComponent<UnityTransport>();
        }

        public void Host()
        {
            NetworkManager.OnServerStarted += SendHostingSuccess;
            NetworkManager.StartHost();
            NetworkManager.OnClientConnectedCallback += Play;
        }

        public void Client(string ip, ushort port)
        {
            Transport.SetConnectionData(ip, port);
            NetworkManager.StartClient();
        }
    
        public void Play(ulong clientId)
        {
            NetworkManager.OnClientConnectedCallback -= Play;
            NetworkManager.OnServerStarted -= SendHostingSuccess;
            NetworkManager.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
        }

        public void CancelHost()
        {
            NetworkManager.OnClientConnectedCallback -= Play;
            NetworkManager.OnServerStarted -= SendHostingSuccess;
            NetworkManager.Shutdown();
        }

        private void SendHostingSuccess()
        {
            OnHostingSuccess?.Invoke();
        }
    }
}