using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Game
{
    public class PipeSpawner : MonoBehaviour
    {
        public static PipeSpawner Instance;
        
        public Transform pipe;

        public List<PipeHandler> activePipes = new List<PipeHandler>();
        
        private float delay;
        private int spawnedPipes;

        private void Awake() => Instance = this;

        private void Start()
        {
            for (var i = 0; i < 5; i++) SpawnPipe(spawnedPipes * Settings.Instance.pipeDistance, RandomnessHandler.RandomIntMinMax(1, Settings.Instance.height - 3));
        }

        private void Update()
        {
            if (delay > Settings.Instance.pipeDistance - 0.5f)
            {
                SpawnPipe(spawnedPipes * Settings.Instance.pipeDistance, RandomnessHandler.RandomIntMinMax(1, Settings.Instance.height - 3));
                delay = 0;
            }
            
            delay += Time.deltaTime * Settings.Instance.gameSpeed;
        }

        private void SpawnPipe(int x, int y)
        {
            SpawnComponent(x, y, false);
            SpawnComponent(x, y, true);
            spawnedPipes++;
        }

        private void SpawnComponent(int x, int y, bool complementary)
        {
            var newPipe = Instantiate(pipe, Vector3.zero, Quaternion.identity).GetComponent<PipeHandler>();
            newPipe.x = x;
            newPipe.y = complementary ? Settings.Instance.height - 3 - y : y;
            newPipe.complementary = complementary;
            if (!complementary) activePipes.Add(newPipe);
        }
    }
}
