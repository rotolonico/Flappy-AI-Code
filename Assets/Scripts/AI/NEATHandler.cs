using System;
using System.Collections.Generic;
using System.Linq;
using AI.LiteNN;
using AI.NEAT;
using Game;
using UnityEngine;

namespace AI
{
    public class NEATHandler : MonoBehaviour
    {
        public static NEATHandler Instance;
        
        public GameObject flappyAI;
        public int populationSize;
        public List<FlappyHandler> alivePopulation = new List<FlappyHandler>();

        public Evaluator evaluator;

        private void Awake() => Instance = this;

        private void Start()
        {
            evaluator = new Evaluator(populationSize, new Counter(), new Counter(), g => 0);
            InitiateFlappys();
        }

        public void InitiateFlappys()
        {
            foreach (var genome in evaluator.Genomes)
            {
                var newFlappyAI = Instantiate(flappyAI, transform.position, Quaternion.identity);
                newFlappyAI.GetComponent<FlappyController>().genome = genome;
                alivePopulation.Add(newFlappyAI.GetComponent<FlappyHandler>());
                if (genome.Best)
                {
                    newFlappyAI.GetComponent<SpriteRenderer>().color = Color.red;
                    newFlappyAI.GetComponent<SpriteRenderer>().sortingOrder = 3;
                }
            }
        }
    }
}