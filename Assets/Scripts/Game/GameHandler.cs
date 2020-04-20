using System;
using System.Linq;
using AI;
using AI.NEAT;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class GameHandler : MonoBehaviour
    {
        public static GameHandler Instance;

        public GameObject flappy;
        public bool playerAlive;

        private Camera mainCamera;

        public Toggle raceNetwork;
        public TextMeshProUGUI raceNetworkText;

        private bool inRaceMode;

        private void Awake() => Instance = this;

        private void Start() => mainCamera = Camera.main;

        private void Update()
        {
            if (!raceNetwork.isOn
                ? NEATHandler.Instance.alivePopulation.Count != 0
                : playerAlive) return;
            if (raceNetwork.isOn)
                ResetRaceGame();
            else
            {
                NEATHandler.Instance.evaluator.Evaluate();
                ResetGame();
            }
        }

        public void ResetRaceGame()
        {
            if (!inRaceMode)
            {
                NEATHandler.Instance.evaluator.Evaluate();
                NEATHandler.Instance.sevenInputsMode.interactable = false;
                PipeSpawner.Instance.automaticPipes.SetIsOnWithoutNotify(true);
                PipeSpawner.Instance.automaticPipes.interactable = false;
            }
            inRaceMode = true;
            ResetGameAndNetwork(raceNetwork.isOn ? NEATHandler.Instance.evaluator.FittestGenome : null);
            raceNetworkText.text = raceNetwork.isOn ? "Train new network" : "Race this network";
        }

        public void ResetGameAndNetworkUI()
        {
            Settings.Instance.inputs = NEATHandler.Instance.sevenInputsMode.isOn ? 7 : 3;
            ResetGameAndNetwork();
        }

        public void ResetGameAndNetwork(GenomeWrapper startingGenome = null)
        {
            foreach (var leftOvers in GameObject.FindGameObjectsWithTag("Flappy")) Destroy(leftOvers);
            PipeSpawner.Instance.Reset();
            mainCamera.transform.position = new Vector3(7, 0, -10);
            NEATHandler.Instance.InitializeNetwork(startingGenome);
            if (raceNetwork.isOn) Instantiate(flappy, transform.position, Quaternion.identity);
            else
            {
                inRaceMode = false;
                NEATHandler.Instance.sevenInputsMode.interactable = true;
                PipeSpawner.Instance.automaticPipes.interactable = true;
            }
            playerAlive = true;
        }

        public void ResetGame()
        {
            foreach (var leftOvers in GameObject.FindGameObjectsWithTag("Flappy")) Destroy(leftOvers);
            if (Settings.Instance.randomizePipes) PipeSpawner.Instance.Reset();
            mainCamera.transform.position = new Vector3(7, 0, -10);
            NEATHandler.Instance.InitiateGeneration();
            if (raceNetwork.isOn) Instantiate(flappy, transform.position, Quaternion.identity);
            playerAlive = true;
        }
    }
}