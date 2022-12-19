using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PM.PingPong.Gameplay
{
    public class PingPongUIController : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text scoreText;
        [SerializeField]
        private TMP_Text gameEndText;

        [SerializeField]
        private PingPongController pingPongController;
        [SerializeField]
        private GameObject backButton;

        private void Start()
        {
            gameEndText.gameObject.SetActive(false);
            backButton.SetActive(false);
            
            pingPongController.OnGoal += OnGoalHandler;
            pingPongController.OnWin += OnWinHandler;
            pingPongController.OnCountdown += OnCountdownHandler;
        }

        private void OnDestroy()
        {
            pingPongController.OnGoal -= OnGoalHandler;
            pingPongController.OnWin -= OnWinHandler;
            pingPongController.OnCountdown -= OnCountdownHandler;
        }

        private void OnGoalHandler()
        {
            scoreText.text = $"Score {pingPongController.Score[0]}:{pingPongController.Score[1]}";
        }

        private void OnWinHandler(ulong winnerId)
        {
            if (NetworkManager.Singleton.LocalClientId == winnerId)
            {
                gameEndText.text = "You Win!";
            }
            else
            {
                gameEndText.text = "You Lose!";
            }
            
            gameEndText.gameObject.SetActive(true);
            backButton.SetActive(true);
        }

        private void OnCountdownHandler(int value)
        {
            switch (value)
            {
                case -1:
                    gameEndText.gameObject.SetActive(false);
                    break;
                case 0:
                    gameEndText.text = "Start!";
                    gameEndText.gameObject.SetActive(true);
                    break;
                default:
                    gameEndText.text = value.ToString();
                    gameEndText.gameObject.SetActive(true);
                    break;
            }
        }

        public void BackToMainMenu()
        {
            pingPongController.LeaveGame();
        }
    }
}