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

        public List<PipeHandler> pipes = new List<PipeHandler>();
        
        private float delay;
        private int spawnedPipes;

        private void Awake() => Instance = this;

        private void Start() => Reset();

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
            var complementaryPipe = SpawnComponent(x, y, true);
            SpawnComponent(x, y, false).complementaryPipe = complementaryPipe;
            spawnedPipes++;
        }

        private PipeHandler SpawnComponent(int x, int y, bool complementary)
        {
            var newPipe = Instantiate(pipe, Vector3.zero, Quaternion.identity).GetComponent<PipeHandler>();
            newPipe.x = x;
            newPipe.y = complementary ? Settings.Instance.height - 3 - y : y;
            newPipe.complementary = complementary;
            if (!complementary) pipes.Add(newPipe);
            return newPipe;
        }

        public void Reset()
        {
            spawnedPipes = 0;
            foreach (var pipeToDestroy in GameObject.FindGameObjectsWithTag("Pipe")) Destroy(pipeToDestroy);
            pipes.Clear();
            delay = 0;
            for (var i = 0; i < 5; i++) SpawnPipe(spawnedPipes * Settings.Instance.pipeDistance, RandomnessHandler.RandomIntMinMax(1, Settings.Instance.height - 3));
        }
    }
}
