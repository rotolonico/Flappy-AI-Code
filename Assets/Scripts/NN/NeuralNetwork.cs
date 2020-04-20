using System;
using System.Collections.Generic;
using System.Linq;
using AI.NEAT;
using AI.NEAT.Genes;
using UnityEngine;
using Utils;

namespace NN
{
    public class NeuralNetwork
    {
        public readonly int[] Structure;
        public readonly Layer[] Layers;
        public readonly Dictionary<int, Node> GenomeNodes;

        public float WeightDecay = 0;
        public float ClassificationOverPrecision = 1;
        public float Momentum = 0;
        public float MaxError = 0;

        public bool Done;

        public NeuralNetwork(int[] structure)
        {
            Structure = structure;

            Layers = new Layer[Structure.Length];
            for (var i = 0; i < Layers.Length; i++)
            {
                if (i == 0) Layers[i] = new Layer(Structure[i]);
                else Layers[i] = new Layer(Structure[i], Layers[i - 1]);
            }
        }

        public NeuralNetwork(Genome genome)
        {
            GenomeNodes = new Dictionary<int, Node>();
            foreach (var node in genome.Nodes)
                GenomeNodes.Add(node.Key, new Node(node.Value.Type == NodeGene.TypeE.Input, node.Value.X));

            foreach (var node in GenomeNodes)
            {
                var inNodes = genome.Connections.Where(c => c.Value.OutNode == node.Key && c.Value.Expressed)
                    .Select(c => new KeyValuePair<int, int>(c.Key, c.Value.InNode)).ToArray();
                foreach (var inNode in inNodes)
                    node.Value.weights.Add(GenomeNodes[inNode.Value], genome.Connections[inNode.Key].Weight);
            }
        }
    }
}