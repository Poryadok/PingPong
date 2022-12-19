using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PM.PingPong.MainMenu
{
    public class MainMenuLoader : MonoBehaviour
    {
        private void Start()
        {
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }
    }
}