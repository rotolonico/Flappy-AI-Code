using System;
using System.Linq;
using AI;
using UnityEngine;

namespace Game
{
    public class GameHandler : MonoBehaviour
    {
        public static GameHandler Instance;
        
        public GameObject flappy;
        public bool playerAlive;

        private Camera mainCamera;
        
        private void Awake() => Instance = this;

        private void Start() => mainCamera = Camera.main;

        private void Update()
        {
            if (!playerAlive && NEATHandler.Instance.alivePopulation.Count == 0)
            {
                NEATHandler.Instance.evaluator.Evaluate();
                ResetGame();
            }
        }

        public void ResetGameAndNetwork()
        {
            PipeSpawner.Instance.Reset();
            mainCamera.transform.position = new Vector3(7, 0, -10);
            NEATHandler.Instance.InitializeNetwork();
            Instantiate(flappy, transform.position, Quaternion.identity);
            playerAlive = true;
        }

        public void ResetGame()
        {
            if (Settings.Instance.randomizePipes) PipeSpawner.Instance.Reset();
            mainCamera.transform.position = new Vector3(7, 0, -10);
            NEATHandler.Instance.InitiateGeneration();
            Instantiate(flappy, transform.position, Quaternion.identity);
            playerAlive = true;
        }
    }
}