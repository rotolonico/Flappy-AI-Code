using AI.LiteNN;
using Game;
using UnityEngine;

namespace AI
{
    public class Population : MonoBehaviour
    {
        public GameObject flappyAI;
        public int populationSize;
        
        public LiteNeuralNetwork[] networks;

        private void Start()
        {
            networks = new LiteNeuralNetwork[populationSize];
            for (var i = 0; i < networks.Length; i++)
            {
                networks[i] = new LiteNeuralNetwork(2, new[] {50}, 2);
                var newFlappyAI = Instantiate(flappyAI, transform.position, Quaternion.identity);
                newFlappyAI.GetComponent<FlappyController>().network = networks[i];
            }
        }
    }
}