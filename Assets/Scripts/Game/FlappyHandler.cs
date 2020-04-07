using System;
using AI;
using UnityEngine;

namespace Game
{
    public class FlappyHandler : MonoBehaviour
    {
        public int passedPipes;
        private bool isAlive = true;

        private FlappyController flappyController;

        private void Start() => flappyController = GetComponent<FlappyController>();

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Pipe") || other.CompareTag("StaticPipe"))
            {
                if (!flappyController.player)
                {
                    flappyController.genome.Fitness = Mathf.Pow(flappyController.aliveTime * 1000, 4);
                }
                
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (isAlive) NEATHandler.Instance.alivePopulation.Remove(this);
            isAlive = false;
            if (flappyController.player) GameHandler.Instance.playerAlive = false;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("PipeTrigger")) passedPipes++;
        }
    }
}