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

        public bool player;

        private void Update()
        {
            if (player && Input.GetKeyDown(KeyCode.UpArrow)) Jump();
        }

        private void FixedUpdate()
        {
            if (player) return;
            genome.Genome.AliveTime++;
            var inputs = InputsRetriever.GetInputs(handler);
            var outputs = NetworkCalculator.TestNetworkGenome(genome.Network, inputs);
            if (outputs[0] > outputs[1]) Jump();
        }

        private void Jump()
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(Vector2.up * (player ? 60 : 300), ForceMode2D.Force);
        }
    }
}