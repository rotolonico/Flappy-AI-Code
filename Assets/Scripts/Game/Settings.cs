using System;
using AI;
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
        public int inputs;
        public int outputs;
        
        private void Awake() => Instance = this;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene(0);
            if (Input.GetKeyDown(KeyCode.E)) NEATInitializer.Instance.evaluator.Evaluate();
        }
    }
}
