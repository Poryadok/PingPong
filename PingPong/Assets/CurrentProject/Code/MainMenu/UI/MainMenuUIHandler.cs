using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PM.PingPong.MainMenu
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private MainMenuNetworkController MainMenuNetworkController;
        [SerializeField] private TMP_InputField ipInputField;
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject waitingPanel;
        [SerializeField] private GameObject hostPanel;
        [SerializeField] private TMP_Dropdown difficultyDropdown;

        private readonly string defaultIP = "127.0.0.1";
        private readonly ushort defaultPort = 7777;

        private void Start()
        {
            ShowPanel(mainPanel);
        }

        private void ShowPanel(GameObject panel)
        {
            mainPanel.SetActive(mainPanel == panel);
            waitingPanel.SetActive(waitingPanel == panel);
            hostPanel.SetActive(hostPanel == panel);
        }

        public void Cancel()
        {
            MainMenuNetworkController.OnHostingSuccess -= ShowHostPanel;
            MainMenuNetworkController.CancelHost();
            difficultyDropdown.onValueChanged.RemoveListener(OnDifficultyChanged);
            ShowPanel(mainPanel);
        }

        public void Host()
        {
            ShowPanel(waitingPanel);
            MainMenuNetworkController.OnHostingSuccess += ShowHostPanel;
            MainMenuNetworkController.Host();
            
            difficultyDropdown.SetValueWithoutNotify(PlayerPrefs.GetInt("Difficulty"));
            difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
        }

        private void ShowHostPanel()
        {
            ShowPanel(hostPanel);
        }

        public void Client()
        {
            if (string.IsNullOrWhiteSpace(ipInputField.text))
            {
                MainMenuNetworkController.Client(defaultIP, defaultPort);
                ShowPanel(waitingPanel);
                return;
            }
            
            var data = ipInputField.text.Split(':');
            if (data.Length != 2)
            {
                Debug.LogError("Wrong ip format");
                return;
            }
                
            string ip = data[0];
            ushort port;
                
            if (!ushort.TryParse(data[1], out port))
            {
                Debug.LogError("Wrong ip format");
                return;
            }
                
            var ipData = ip.Split('.');
            if (ipData.Length == 4)
            {
                Debug.LogError("Wrong ip format");
                return;
            }

            foreach (var ipNum in ipData)
            {
                if (!ushort.TryParse(ipNum, out ushort result))
                {
                    Debug.LogError("Wrong ip format");
                    return;
                }
            }
                
            MainMenuNetworkController.Client(ip, port);
            ShowPanel(waitingPanel);
        }

        private void OnDifficultyChanged(int newValue)
        {
            PlayerPrefs.SetInt("Difficulty", newValue);
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}