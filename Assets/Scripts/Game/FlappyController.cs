using System;
using AI.LiteNN;
using AI.NEAT;
using IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class FlappyController : MonoBehaviour
    {
        public Rigidbody2D rb;
        public FlappyHandler handler;
        public GenomeWrapper genome;
        
        public bool player;

        private float networkDelay;
        
        private void Update()
        {
            if (player)
            {
                if (Input.GetKeyDown(KeyCode.Space)) Jump();
            } else
            {
                genome.Genome.AliveTime += Time.deltaTime;
                networkDelay += Time.deltaTime;
                if (networkDelay < 0.1f) return;
                networkDelay = 0;
                var inputs = InputsRetriever.GetInputs(handler);
                var outputs = genome.Genome.Network.Test(inputs);
                if (outputs[0] > outputs[1]) Jump();
            }
            
        }

        private void Jump() => rb.AddForce(new Vector2(0, 300));
    }
}
