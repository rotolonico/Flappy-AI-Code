using System;
using AI.LiteNN;
using AI.NEAT;
using Game;
using UnityEngine;

namespace AI
{
    public class NEATInitializer : MonoBehaviour
    {
        public static NEATInitializer Instance;
        
        public GameObject flappyAI;
        public int populationSize;

        public Evaluator evaluator;

        private void Awake() => Instance = this;

        private void Start()
        {
            evaluator = new Evaluator(populationSize, new Counter(), new Counter(), g => g.AliveTime);
            foreach (var genome in evaluator.genomes)
            {
                var newFlappyAI = Instantiate(flappyAI, transform.position, Quaternion.identity);
                newFlappyAI.GetComponent<FlappyController>().genome = genome;
            }
        }
    }
}