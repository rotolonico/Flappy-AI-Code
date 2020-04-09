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

        private void Awake() => Instance = this;

        private void Update()
        {
            Debug.Log("Alive population:" + NEATHandler.Instance.alivePopulation.Count);
            if (!playerAlive && NEATHandler.Instance.alivePopulation.Count == 0)
            {
                NEATHandler.Instance.evaluator.Evaluate();
                ResetGame();
            }
        }

        private void ResetGame()
        {
            if (Settings.Instance.randomizePipes) PipeSpawner.Instance.Reset();
            Camera.main.transform.position = new Vector3(7, 0, -10);
            Instantiate(flappy, transform.position, Quaternion.identity);
            playerAlive = true;
            NEATHandler.Instance.InitiateFlappys();
            Debug.Log("Score: " + NEATHandler.Instance.evaluator.FittestGenome.Fitness);
        }
    }
}