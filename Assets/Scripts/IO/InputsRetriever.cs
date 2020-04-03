using System;
using Game;
using UnityEngine;

namespace IO
{
    public static class InputsRetriever
    {
        public static float[] GetInputs()
        {
            var inputs = new float[3];
            var flappyPosition = FlappyHandler.Instance.transform.position;
            inputs[0] = (flappyPosition.y + Settings.Instance.height / 2f) /
                        Settings.Instance.height;
            inputs[1] = PipeSpawner.Instance.activePipes.Count == 0
                ? 0
                : PipeSpawner.Instance.activePipes[0].y / Settings.Instance.height;
            inputs[2] = PipeSpawner.Instance.activePipes.Count == 0
                ? 1
                : Mathf.Clamp((PipeSpawner.Instance.activePipes[0].x - flappyPosition.x) / Settings.Instance.height,
                    0, 1);

            return inputs;
        }
    }
}