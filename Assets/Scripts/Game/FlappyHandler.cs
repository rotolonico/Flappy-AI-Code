using System;
using AI;
using UnityEngine;

namespace Game
{
    public class FlappyHandler : MonoBehaviour
    {
        public int passedPipes;
        private bool isAlive = true;

        public FlappyController flappyController;

        private void Awake() => flappyController = GetComponent<FlappyController>();

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Pipe") || other.CompareTag("StaticPipe"))
            {
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