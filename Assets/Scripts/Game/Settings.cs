using System;
using AI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class Settings : MonoBehaviour
    {
        public static Settings Instance;

        public int gameSpeed;
        public int height;
        public int pipeDistance;
        public int inputs;
        public int outputs;
        public bool pixelsInput;
        public bool randomizePipes;

        private void Awake()
        {
            Instance = this;
            if (pixelsInput) inputs = pipeDistance * (height + 2);
        }
    }
}
