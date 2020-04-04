using System;
using Game;
using UnityEngine;

namespace IO
{
    public static class InputsRetriever
    {
        public static float[] GetInputs(FlappyHandler flappy)
        {
            var inputs = new float[2];
            var flappyPosition = flappy.transform.position;
            inputs[0] = Mathf.Clamp((PipeSpawner.Instance.activePipes[flappy.passedPipes].x - flappyPosition.x) / Settings.Instance.height,
                    0, 1);
            inputs[1] = ((flappyPosition.y + Settings.Instance.height / 2f) /
                         Settings.Instance.height - PipeSpawner.Instance.activePipes[flappy.passedPipes].y / Settings.Instance.height) / 2 + 0.5f;

            return inputs;
        }
    }
}