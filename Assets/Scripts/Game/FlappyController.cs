using System;
using IO;
using UnityEngine;

namespace Game
{
    public class FlappyController : MonoBehaviour
    {
        public Rigidbody2D rb;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space)) rb.AddForce(new Vector2(0, 300));
            var inputs = InputsRetriever.GetInputs();
            Debug.Log($"Flappy Y: {inputs[0]}, Pipe Y: {inputs[1]}, Distance X: {inputs[2]}");
        }
    }
}
