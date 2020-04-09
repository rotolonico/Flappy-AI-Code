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
            evaluator = new Evaluator(populationSize, new Counter(), new Counter(), g => Mathf.Pow(g.AliveTime, 2));
            InitiateFlappys();
        }

        public void InitiateFlappys()
        {
            foreach (var population in alivePopulation) Destroy(population.gameObject);
            alivePopulation.Clear();

            foreach (var genome in evaluator.Genomes)
            {
                var newFlappyAI = Instantiate(flappyAI, transform.position, Quaternion.identity);
                var newFlappyAIController = newFlappyAI.GetComponent<FlappyController>().genome = genome;
                alivePopulation.Add(newFlappyAI.GetComponent<FlappyHandler>());
                
                if (genome.Best)
                {
                    newFlappyAI.GetComponent<SpriteRenderer>().color = Color.red;
                    newFlappyAI.GetComponent<SpriteRenderer>().sortingOrder = 3;
                }

                newFlappyAIController.Best = false;

                if (evaluator.FittestGenome == null) continue;
                NetworkDisplayer.Instance.DisplayNetwork(evaluator.FittestGenome.Genome);
                Debug.Log("Score: " + evaluator.FittestGenome.Fitness);
                evaluator.FittestGenome = null;
            }
        }
    }
}