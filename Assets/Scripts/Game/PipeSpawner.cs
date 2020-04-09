using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Game
{
    public class PipeSpawner : MonoBehaviour
    {
        public static PipeSpawner Instance;

        public Transform pipe;
        public Slider pipeHeight;
        public Toggle automaticPipes;

        public List<PipeHandler> pipes = new List<PipeHandler>();

        private float delay;
        private int spawnedPipes;
        private float completeMovement;

        private void Awake() => Instance = this;

        private void Start() => Reset();

        private void Update()
        {
            completeMovement += Time.deltaTime * Settings.Instance.gameSpeed;
            if (automaticPipes.isOn && delay > Settings.Instance.pipeDistance - 0.5f)
            {
                SpawnPipe(spawnedPipes * Settings.Instance.pipeDistance - (int) completeMovement,
                    RandomnessHandler.RandomIntMinMax(1, Settings.Instance.height - 3));
                delay = 0;
            }

            delay += Time.deltaTime * Settings.Instance.gameSpeed;

            pipeHeight.maxValue = Settings.Instance.height - 4;

            if (automaticPipes.isOn) return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) SpawnPipe((int) transform.position.x + 10, 1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SpawnPipe((int) transform.position.x + 10, 2);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SpawnPipe((int) transform.position.x + 10, 3);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SpawnPipe((int) transform.position.x + 10, 4);
            if (Input.GetKeyDown(KeyCode.Alpha5)) SpawnPipe((int) transform.position.x + 10, 5);
            if (Input.GetKeyDown(KeyCode.Alpha6)) SpawnPipe((int) transform.position.x + 10, 6);
        }

        public void SpawnPipeButton()
        {
            if (!automaticPipes.isOn) SpawnPipe((int) transform.position.x + 10, (int) pipeHeight.value);
        }

        public void SpawnRandomPipeButton()
        {
            if (!automaticPipes.isOn)
                SpawnPipe((int) transform.position.x + 10,
                    RandomnessHandler.RandomIntMinMax(1, Settings.Instance.height - 3));
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
            if (!automaticPipes.isOn) return;
            completeMovement = 0;
            for (var i = 0; i < 5; i++)
                SpawnPipe(spawnedPipes * Settings.Instance.pipeDistance - (int) completeMovement,
                    RandomnessHandler.RandomIntMinMax(1, Settings.Instance.height - 3));
        }
    }
}