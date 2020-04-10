using System;
using AI;
using Game;
using UnityEngine;

namespace IO
{
    public static class InputsRetriever
    {
        public static float[] GetInputs(FlappyHandler flappy)
        {
            var inputs = new float[NEATHandler.Instance.sevenInputsMode.isOn ? 7 : 3];
            var flappyPosition = flappy.transform.position;
            
            inputs[0] = flappy.passedPipes < PipeSpawner.Instance.pipes.Count
                ? (PipeSpawner.Instance.pipes[flappy.passedPipes].x - flappyPosition.x) / 5
                : 1;
            inputs[1] = (flappyPosition.y - (flappy.passedPipes < PipeSpawner.Instance.pipes.Count
                             ? PipeSpawner.Instance.pipes[flappy.passedPipes].transform.position.y +
                               PipeSpawner.Instance.pipes[flappy.passedPipes].y / 2
                             : 0)) / 5;
            inputs[2] = flappy.flappyController.rb.velocity.y / 10;
            
            if (!NEATHandler.Instance.sevenInputsMode.isOn) return inputs;
            
            inputs[3] = (flappyPosition.y - (flappy.passedPipes < PipeSpawner.Instance.pipes.Count
                             ? PipeSpawner.Instance.pipes[flappy.passedPipes].transform.position.y +
                               PipeSpawner.Instance.pipes[flappy.passedPipes].complementaryPipe.y / 2
                             : 0)) / 5;
            inputs[4] = flappy.passedPipes + 1 < PipeSpawner.Instance.pipes.Count
                ? (PipeSpawner.Instance.pipes[flappy.passedPipes + 1].x - flappyPosition.x) / 10
                : 1;
            inputs[5] = (flappyPosition.y - (flappy.passedPipes + 1 < PipeSpawner.Instance.pipes.Count
                             ? PipeSpawner.Instance.pipes[flappy.passedPipes + 1].transform.position.y +
                               PipeSpawner.Instance.pipes[flappy.passedPipes].y / 2
                             : 0)) / 10;
            inputs[6] = (flappyPosition.y - (flappy.passedPipes + 1 < PipeSpawner.Instance.pipes.Count
                             ? PipeSpawner.Instance.pipes[flappy.passedPipes + 1].transform.position.y +
                               PipeSpawner.Instance.pipes[flappy.passedPipes + 1].complementaryPipe.y / 2
                             : 0)) / 10;

            return inputs;
        }

        public static float[] GetPixelsInputs(FlappyHandler flappy) =>
            PixelsRetriever.Instance.RetrievePixels(flappy.gameObject);
    }
}