using System;
using System.Linq;
using AI.LiteNN;
using AI.NEAT;
using IO;
using NN;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class FlappyController : MonoBehaviour
    {
        public Rigidbody2D rb;
        public FlappyHandler handler;
        public GenomeWrapper genome;
        public float aliveTime;

        public bool player;

        private float networkDelay;

        private void FixedUpdate()
        {
            if (player)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow)) Jump();
                else if (Input.GetKeyDown(KeyCode.DownArrow)) Down();
            }
            else
            {
                networkDelay += Time.deltaTime;
                //if (networkDelay < 0.2f) return;
                aliveTime++;
                networkDelay = 0;
                var inputs = InputsRetriever.GetInputs(handler);
                var outputs = NetworkCalculator.TestNetworkGenome(genome.Network, inputs);
                if (Settings.Instance.outputs == 3)
                {
                    var action = Array.IndexOf(outputs, outputs.Max());
                    switch (action)
                    {
                        case 0:
                            Jump();
                            break;
                        case 1:
                            Down();
                            break;
                    }
                }
                else
                {
                    if (outputs[0] > outputs[1]) Jump();
                }
            }
        }

        private void Jump()
        {
            if (Settings.Instance.normalInput)
                rb.AddForce(player ? new Vector2(0, 300) : new Vector2(0, 60));
            else
            {
                var position = transform.position;
                position = new Vector3(position.x, position.y + 1, 0);
                transform.position = position;
            }
        }

        private void Down()
        {
            if (Settings.Instance.normalInput)
                rb.AddForce(player ? new Vector2(0, 300) : new Vector2(0, -60));
            else
            {
                var position = transform.position;
                position = new Vector3(position.x, position.y - 1, 0);
                transform.position = position;
            }
        }
    }
}