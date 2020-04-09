using System;
using Game;
using UnityEngine;

namespace IO
{
    public static class InputsRetriever
    {
        public static float[] GetInputs(FlappyHandler flappy)
        {
            var inputs = new float[3];
            var flappyPosition = flappy.transform.position;
            inputs[0] = (PipeSpawner.Instance.pipes[flappy.passedPipes].x - flappyPosition.x) / 5;
            inputs[1] = (flappyPosition.y - (PipeSpawner.Instance.pipes[flappy.passedPipes].transform.position.y + 1 + PipeSpawner.Instance.pipes[flappy.passedPipes].y / 2)) / 5;
            //inputs[2] = (flappyPosition.y - (PipeSpawner.Instance.pipes[flappy.passedPipes].complementaryPipe.transform.position.y - 1 + PipeSpawner.Instance.pipes[flappy.passedPipes].complementaryPipe.y / 2)) / 5;
            inputs[2] = flappy.flappyController.rb.velocity.y / 10;

            return inputs;
        }

        public static float[] GetPixelsInputs(FlappyHandler flappy) => PixelsRetriever.Instance.RetrievePixels(flappy.gameObject);
    }
}