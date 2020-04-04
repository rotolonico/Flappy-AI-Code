using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class Settings : MonoBehaviour
    {
        public static Settings Instance;

        public int gameSpeed;
        public int height;
        public int pipeDistance;
        
        private void Awake() => Instance = this;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene(0);
        }
    }
}
