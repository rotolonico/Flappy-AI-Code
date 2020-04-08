using System;
using System.Linq;
using AI.NEAT;
using IO;
using NN;
using UnityEngine;

namespace Game
{
    public class FlappyController : MonoBehaviour
    {
        public Rigidbody2D rb;
        public FlappyHandler handler;
        public GenomeWrapper genome;
        public float aliveTime;

        public bool player;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) Jump();
            var inputs = InputsRetriever.GetInputs(handler);
            //Debug.Log(inputs[0] + " " + inputs[1] + " " + inputs[2] + " ");
        }

        private void FixedUpdate()
        {
            if (player) return;
            aliveTime++;
            var inputs = InputsRetriever.GetInputs(handler);
            var outputs = NetworkCalculator.TestNetworkGenome(genome.Network, inputs);
            if (outputs[0] > outputs[1]) Jump();
        }

        private void Jump()
        {
            rb.AddForce(player ? new Vector2(0, 300) : new Vector2(0, 60));
        }
    }
}