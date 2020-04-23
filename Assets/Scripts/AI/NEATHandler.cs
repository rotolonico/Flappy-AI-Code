using System;
using System.Collections.Generic;
using System.Linq;
using AI.LiteNN;
using AI.NEAT;
using Game;
using UnityEngine;
using UnityEngine.UI;

namespace AI
{
    public class NEATHandler : MonoBehaviour
    {
        public static NEATHandler Instance;

        public GameObject flappyAI;
        public int populationSize;
        public List<FlappyHandler> alivePopulation = new List<FlappyHandler>();
        public Toggle sevenInputsMode;

        public Evaluator evaluator;

        private void Awake() => Instance = this;

        private void Start() => InitializeNetwork();

        public void InitializeNetwork(GenomeWrapper startingGenome = null)
        {
            if (!GameHandler.Instance.raceNetwork.isOn)
            {
                evaluator = new Evaluator(populationSize, new Counter(), new Counter(), g => Mathf.Pow(g.AliveTime, 3),
                    startingGenome?.Genome);
                InitiateGeneration();
            }
            else InitializeGenome(startingGenome);
        }

        public void InitiateGeneration()
        {
            alivePopulation.Clear();

            for (var i = 0; i < evaluator.Genomes.Count; i++)
            {
                var genome = evaluator.Genomes[i];
                var newPlayerAI = Instantiate(flappyAI, transform.position, Quaternion.identity);
                newPlayerAI.name = i.ToString();
                var newPlayerController = newPlayerAI.GetComponent<FlappyController>();
                newPlayerController.genome = genome;
                alivePopulation.Add(newPlayerAI.GetComponent<FlappyHandler>());

                if (!genome.Best) continue;
                NetworkDisplayer.Instance.DisplayNetwork(genome);
                var newPlayerAISpriteRenderer = newPlayerAI.GetComponent<SpriteRenderer>();
                newPlayerAISpriteRenderer.color = Color.red;
                newPlayerAISpriteRenderer.sortingOrder = 1;
            }
        }

        public void InitializeGenome(GenomeWrapper genome)
        {
            var newPlayerAI = Instantiate(flappyAI, transform.position, Quaternion.identity);
            newPlayerAI.name = "0";
            var newPlayerController = newPlayerAI.GetComponent<FlappyController>();
            newPlayerController.genome = genome;
            newPlayerController.genome.Best = true;
            alivePopulation.Add(newPlayerAI.GetComponent<FlappyHandler>());

            NetworkDisplayer.Instance.DisplayNetwork(genome);
            newPlayerAI.GetComponent<SpriteRenderer>().color = Color.red;
        }
    }
}