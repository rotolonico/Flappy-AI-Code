using System;
using System.Collections.Generic;
using System.Linq;
using AI.NEAT;
using AI.NEAT.Genes;
using UnityEngine;
using Utils;

namespace AI.LiteNN
{
    [Serializable]
    public class LiteNeuralNetwork
    {
        public float[][][] weights;
        public float[][] biases;
        public float[][] values;

        public float[][][] weightsSmudge;
        public float[][] biasesSmudge;
        public float[][] desiredValues;

        private const float WeightDecay = 0.001f;

        public LiteNeuralNetwork(int inputs, IReadOnlyList<int> hiddenLayers, int outputs) =>
            InitializeNetwork(inputs, hiddenLayers, outputs, true);

        public LiteNeuralNetwork(int inputs, Genome genome, int outputs)
        {
            var possibleDistances = new Dictionary<float, int>();
            foreach (var node in genome.Nodes.Values.Where(node => node.Type == NodeGene.TypeE.Hidden))
            {
                if (possibleDistances.ContainsKey(node.X)) possibleDistances[node.X]++;
                else possibleDistances[node.X] = 1;
            }

            var sortedDistances = possibleDistances.Keys.ToList();
            sortedDistances.Sort();
            var hiddenLayers = sortedDistances.Select(t => possibleDistances[t]).ToList();
            sortedDistances.Insert(0, 0);
            sortedDistances.Add(1);

            InitializeNetwork(inputs, hiddenLayers, outputs, false);

            foreach (var connection in genome.Connections.Values.Where(c => c.Expressed))
            {
                weights[sortedDistances.IndexOf(genome.Nodes[connection.InNode].X)][genome.Nodes[connection.OutNode].Y][
                    genome.Nodes[connection.InNode].Y] = connection.Weight;
            }
        }

        private void InitializeNetwork(int inputs, IReadOnlyList<int> hiddenLayers, int outputs, bool initializeParams)
        {
            values = new float[1 + hiddenLayers.Count + 1][];
            desiredValues = new float[1 + hiddenLayers.Count + 1][];
            biases = new float[1 + hiddenLayers.Count + 1][];
            biasesSmudge = new float[1 + hiddenLayers.Count + 1][];
            weights = new float[1 + hiddenLayers.Count][][];
            weightsSmudge = new float[1 + hiddenLayers.Count][][];

            values[0] = new float[inputs];
            desiredValues[0] = new float[inputs];
            biases[0] = new float[inputs];
            biasesSmudge[0] = new float[inputs];

            for (var i = 0; i < hiddenLayers.Count; i++)
            {
                values[i + 1] = new float[hiddenLayers[i]];
                desiredValues[i + 1] = new float[hiddenLayers[i]];
                biases[i + 1] = new float[hiddenLayers[i]];
                biasesSmudge[i + 1] = new float[hiddenLayers[i]];
                weights[i] = new float[values[i + 1].Length][];
                weightsSmudge[i] = new float[values[i + 1].Length][];
                for (var j = 0; j < weights[i].Length; j++)
                {
                    weights[i][j] = new float[values[i].Length];
                    weightsSmudge[i][j] = new float[values[i].Length];
                    if (!initializeParams) continue;
                    for (var k = 0; k < weights[i][j].Length; k++)
                        weights[i][j][k] = RandomnessHandler.RandomZeroToOne();
                }
            }

            values[hiddenLayers.Count + 1] = new float[outputs];
            desiredValues[hiddenLayers.Count + 1] = new float[outputs];
            biases[hiddenLayers.Count + 1] = new float[outputs];
            biasesSmudge[hiddenLayers.Count + 1] = new float[outputs];
            weights[hiddenLayers.Count] = new float[outputs][];
            weightsSmudge[hiddenLayers.Count] = new float[outputs][];
            for (var j = 0; j < weights[hiddenLayers.Count].Length; j++)
            {
                weights[hiddenLayers.Count][j] = new float[values[hiddenLayers.Count].Length];
                weightsSmudge[hiddenLayers.Count][j] = new float[values[hiddenLayers.Count].Length];
                if (!initializeParams) continue;
                for (var k = 0; k < weights[hiddenLayers.Count][j].Length; k++)
                    weights[hiddenLayers.Count][j][k] = RandomnessHandler.RandomZeroToOne();
            }
        }

        public float[] Test(float[] newInput)
        {
            for (var i = 0; i < values[0].Length; i++)
                values[0][i] = newInput[i];

            for (var i = 1; i < values.Length; i++)
            for (var j = 0; j < values[i].Length; j++)
            {
                values[i][j] = BasicFunctions.Sigmoid(Sum(values[i - 1], weights[i - 1][j]) + biases[i][j]);
                desiredValues[i][j] = values[i][j];
            }

            return values[values.Length - 1];
        }

        private static float Sum(IEnumerable<float> values, IReadOnlyList<float> weights) =>
            values.Select((t, i) => t * weights[i]).Sum();

        public void Train(float[][] trainingInputs, float[][] trainingOutputs)
        {
            for (var i = 0; i < trainingInputs.Length; i++)
            {
                Test(trainingInputs[i]);

                for (var j = 0; j < desiredValues[desiredValues.Length - 1].Length; j++)
                    desiredValues[desiredValues.Length - 1][j] = trainingOutputs[i][j];

                for (var j = values.Length - 1; j >= 1; j--)
                {
                    for (var k = 0; k < values[j].Length; k++)
                    {
                        var biasSmudge = BasicFunctions.SigmoidDerivative(values[j][k]) *
                                         (desiredValues[j][k] - values[j][k]);
                        biasesSmudge[j][k] += biasSmudge;

                        for (var l = 0; l < values[j - 1].Length; l++)
                        {
                            var weightSmudge = values[j - 1][l] * biasSmudge;
                            weightsSmudge[j - 1][k][l] += weightSmudge;

                            var valueSmudge = weights[j - 1][k][l] * biasSmudge;
                            desiredValues[j - 1][l] += valueSmudge;
                        }
                    }
                }
            }

            for (var i = values.Length - 1; i >= 1; i--)
            {
                for (var j = 0; j < values[i].Length; j++)
                {
                    biases[i][j] += biasesSmudge[i][j];
                    biases[i][j] *= 1 - WeightDecay;
                    biasesSmudge[i][j] = 0;

                    for (var k = 0; k < values[i - 1].Length; k++)
                    {
                        weights[i - 1][j][k] += weightsSmudge[i - 1][j][k];
                        weights[i - 1][j][k] *= 1 - WeightDecay;
                        weightsSmudge[i - 1][j][k] = 0;
                    }

                    desiredValues[i][j] = 0;
                }
            }
        }

        public float Accuracy(float[][] testingInputs, float[][] testingOutputs)
        {
            var accuracies = new List<float>();
            for (var i = 0; i < testingInputs.Length; i++)
            {
                var output = Test(testingInputs[i]).ToList();
                var desiredOutput = testingOutputs[i].ToList();
                accuracies.Add(output.IndexOf(output.Max()) == desiredOutput.IndexOf(desiredOutput.Max()) ? 1 : 0);
            }

            return accuracies.Average();
        }
    }
}