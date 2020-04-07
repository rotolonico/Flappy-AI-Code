using System.Collections.Generic;
using System.Linq;
using NNUtils;
using UnityEngine;
using Utils;

namespace NN
{
    public static class NetworkCalculator
    {
        public static float[] TestNetwork(NeuralNetwork network, float[] inputs)
        {
            if (network.Structure[0] != inputs.Length)
            {
                Debug.Log("Expected " + network.Structure[0] + " inputs, got " + inputs.Length);
                return null;
            }

            for (var i = 0; i < network.Layers[0].Nodes.Length; i++) network.Layers[0].Nodes[i].SetInputValue(inputs[i]);

            var outputs = new float[network.Structure[network.Structure.Length - 1]];

            for (var i = 0; i < network.Layers[network.Layers.Length - 1].Nodes.Length; i++)
                outputs[i] = network.Layers[network.Layers.Length - 1].Nodes[i].CalculateValue();

            foreach (var layer in network.Layers)
            foreach (var node in layer.Nodes)
                node.ValueCalculated = false;

            return outputs;
        }
        
        public static float[] TestNetworkGenome(NeuralNetwork network, float[] inputs)
        {
            var sortedNodes = network.GenomeNodes.Values.ToList().OrderBy(n => n.X).ToList();
            for (var i = 0; i < sortedNodes.Count; i++)
            {
                var node = sortedNodes[i];
                if (node.X == 0) node.SetInputValue(inputs[i]);
                else node.CalculateValue();
            }

            var outputs = new List<float>();
            outputs.AddRange(sortedNodes.Where(n => n.X == 1).Select(n => n.Value));

            foreach (var node in sortedNodes)
                node.ValueCalculated = false;

            return outputs.ToArray();
        }

        public static void TrainNetwork(NeuralNetwork network, List<float[]> inputs, List<float[]> desiredOutputs)
        {
            foreach (var desiredOutput in desiredOutputs.Where(desiredOutput =>
                network.Structure[network.Structure.Length - 1] != desiredOutput.Length))
            {
                Debug.Log("Expected " + network.Structure[network.Structure.Length - 1] + " outputs, got " +
                          desiredOutput.Length);
                return;
            }

            var doneTraining = true;

            for (var i = 0; i < inputs.Count; i++)
            {
                var input = inputs[i];
                TestNetwork(network, input);

                for (var j = 0; j < network.Layers[network.Layers.Length - 1].Nodes.Length; j++)
                    network.Layers[network.Layers.Length - 1].Nodes[j].SetDesiredValue(desiredOutputs[i][j]);

                for (var j = network.Layers.Length - 1; j >= 1; j--)
                {
                    foreach (var node in network.Layers[j].Nodes)
                    {
                        var biasSmudge = BasicFunctions.SigmoidDerivative(node.Value) *
                                         network.ClassificationOverPrecision *
                                         node.CalculateCostDelta(network.ClassificationOverPrecision, network.MaxError);
                        node.TrainingBiasSmudge += biasSmudge;

                        if (!node.isAcceptableValue) doneTraining = false;

                        foreach (var connectedNode in node.GetConnectedNodes())
                        {
                            var weightSmudge = connectedNode.Value * biasSmudge;
                            node.TrainingWeightsSmudge[connectedNode] += weightSmudge;

                            var valueSmudge = node.GetWeight(connectedNode) * biasSmudge;
                            connectedNode.SmudgeDesiredValue(valueSmudge);
                        }
                    }
                }
            }

            if (doneTraining)
            {
                network.Done = true;
                return;
            }

            for (var i = network.Layers.Length - 1; i >= 1; i--)
            {
                foreach (var node in network.Layers[i].Nodes)
                {
                    node.SmudgeBias(node.TrainingBiasSmudge);
                    node.SetBias(node.GetBias() * (1 - network.WeightDecay));
                    node.TrainingBiasSmudge *= network.Momentum;

                    foreach (var connectedNode in node.GetConnectedNodes())
                    {
                        node.SmudgeWeight(connectedNode, node.TrainingWeightsSmudge[connectedNode]);
                        node.TrainingWeightsSmudge[connectedNode] *= network.Momentum;

                        node.SetWeight(connectedNode, node.GetWeight(connectedNode) * (1 - network.WeightDecay));
                    }

                    node.SetDesiredValue(0);
                }
            }
        }

        public static void LerpToCorrectParams(NeuralNetwork network, float speed)
        {
            for (var i = network.Layers.Length - 1; i >= 1; i--)
            {
                foreach (var node in network.Layers[i].Nodes)
                {
                    var nodeBias = node.GetBias();
                    node.SetBias(nodeBias + (node.CorrectBias - nodeBias) * speed);

                    var nodes = node.GetConnectedNodes();
                    foreach (var connectedNode in nodes)
                    {
                        var nodeWeight = node.GetWeight(connectedNode);
                        node.SetWeight(connectedNode,
                            nodeWeight + (node.CorrectWeights[connectedNode] - nodeWeight) * speed);
                    }
                }
            }
        }

        public static float CalculateNetworkAccuracy(NeuralNetwork network, List<float[]> inputs,
            List<float[]> desiredOutputs, bool absolute)
        {
            var accuracies = new List<float>();
            for (var i = 0; i < inputs.Count; i++)
            {
                var input = inputs[i];
                var output = TestNetwork(network, input).ToList();
                var desiredOutput = desiredOutputs[i].ToList();
            
                if (!absolute) accuracies.AddRange(output.Select((t, j) => 1 - Mathf.Abs(desiredOutput[j] - t)));
                else
                    accuracies.Add(output.IndexOf(output.Max()) == desiredOutput.IndexOf(desiredOutput.Max()) ? 1 : 0);
            }

            return accuracies.Average();
        }
    }
}