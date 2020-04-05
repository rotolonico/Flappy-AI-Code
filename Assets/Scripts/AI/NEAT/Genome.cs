using System;
using System.Collections.Generic;
using System.Linq;
using AI.LiteNN;
using AI.NEAT.Genes;
using Game;
using UnityEngine;
using Utils;

namespace AI.NEAT
{
    public class Genome
    {
        private const float PerturbingProbability = 0.9f;

        public Dictionary<int, NodeGene> Nodes;
        public Dictionary<int, ConnectionGene> Connections;
        public LiteNeuralNetwork Network;
        public float AliveTime;

        public Genome() => InitializeGenome(new Dictionary<int, NodeGene>(), new Dictionary<int, ConnectionGene>());

        public Genome(Dictionary<int, NodeGene> nodes, Dictionary<int, ConnectionGene> connections) =>
            InitializeGenome(nodes, connections);

        public Genome(Genome toBeCopied)
        {
            InitializeGenome(new Dictionary<int, NodeGene>(), new Dictionary<int, ConnectionGene>());

            foreach (var node in toBeCopied.Nodes) Nodes[node.Key] = node.Value;
            foreach (var connection in toBeCopied.Connections) Connections[connection.Key] = connection.Value;
        }

        private void InitializeGenome(Dictionary<int, NodeGene> nodes, Dictionary<int, ConnectionGene> connections)
        {
            Nodes = nodes;
            Connections = connections;
            Network = new LiteNeuralNetwork(Settings.Instance.inputs, this, Settings.Instance.outputs);
            AliveTime = 0;
        }

        public void AddNodeMutation(Counter nodeInnovation, Counter connectionInnovation)
        {
            var oldConnection = Connections[RandomnessHandler.Random.Next(Connections.Count)];
            var inNode = Nodes[oldConnection.InNode];
            var outNode = Nodes[oldConnection.OutNode];

            oldConnection.Disable();

            var newNodeX = (inNode.X + outNode.X) / 2;
            var newNode = new NodeGene(NodeGene.TypeE.Hidden, nodeInnovation.GetInnovation(), newNodeX,
                Nodes.Count(n => n.Value.X == newNodeX));
            var inToNew = new ConnectionGene(inNode.Innovation, newNode.Innovation, 1, true,
                connectionInnovation.GetInnovation());
            var newToOut = new ConnectionGene(newNode.Innovation, outNode.Innovation, oldConnection.Weight, true,
                connectionInnovation.GetInnovation());

            Nodes.Add(newNode.Innovation, newNode);
            Connections.Add(inToNew.Innovation, inToNew);
            Connections.Add(newToOut.Innovation, newToOut);
        }

        public void AddConnectionMutation(Counter innovation, int maxAttempts)
        {
            var currentAttempt = 0;
            var success = false;

            while (currentAttempt < maxAttempts && !success)
            {
                currentAttempt++;

                var node1 = Nodes[RandomnessHandler.Random.Next(Nodes.Count)];
                var node2 = Nodes[RandomnessHandler.Random.Next(Nodes.Count)];
                var weight = RandomnessHandler.RandomMinusOneToOne();

                var reversed = false;
                if (node1.X == node2.X) continue;
                if (node1.X > node2.X) reversed = true;

                var connectionExists = false;
                foreach (var connection in Connections.Values)
                {
                    if (connection.InNode == node1.Innovation && connection.OutNode == node2.Innovation)
                    {
                        connectionExists = true;
                        break;
                    }

                    if (connection.InNode == node2.Innovation && connection.OutNode == node1.Innovation)
                    {
                        connectionExists = true;
                        break;
                    }
                }

                if (connectionExists) continue;

                var newConnection = new ConnectionGene(reversed ? node2.Innovation : node1.Innovation,
                    reversed ? node1.Innovation : node2.Innovation,
                    weight, true, innovation.GetInnovation());
                Connections.Add(newConnection.Innovation, newConnection);

                success = true;
            }
        }

        public void WeightMutation()
        {
            foreach (var connection in Connections.Values)
            {
                if (RandomnessHandler.RandomZeroToOne() < PerturbingProbability)
                    connection.Weight *= connection.Weight * RandomnessHandler.RandomZeroToOne() * 4f - 2;
                else
                    connection.Weight = RandomnessHandler.RandomZeroToOne() * 4f - 2;
            }
        }

        public static float CalculateCompatibilityDistance(Genome genome1, Genome genome2, float c1, float c2, float c3)
        {
            var genomesInfo = CalculateGenomesInfo(genome1, genome2);
            return genomesInfo.ExcessGenes * c1 + genomesInfo.DisjointGenes * c2 +
                   genomesInfo.AverageWeightDifference * c3;
        }

        public static GenomesGenesInfo CalculateGenomesInfo(Genome genome1, Genome genome2)
        {
            var highestInnovation1 = genome1.Nodes.Keys.Max();
            var highestInnovation2 = genome2.Nodes.Keys.Max();
            var indices = Math.Max(highestInnovation1, highestInnovation2);

            var matchingGenes = 0;
            var disjointGenes = 0;
            var excessGenes = 0;

            for (var i = 0; i < indices; i++)
            {
                var node1 = genome1.Nodes[i];
                var node2 = genome1.Nodes[i];

                if (node1 != null && node2 != null) matchingGenes++;
                else if (node1 == null && node2 != null)
                {
                    if (highestInnovation1 > i) disjointGenes++;
                    else excessGenes++;
                }
                else if (node1 != null && node2 == null)
                {
                    if (highestInnovation2 > i) disjointGenes++;
                    else excessGenes++;
                }
            }

            var highestConnectionInnovation1 = genome1.Connections.Keys.Max();
            var highestConnectionInnovation2 = genome2.Connections.Keys.Max();
            var connectionIndices = Math.Max(highestConnectionInnovation1, highestConnectionInnovation2);

            var matchingConnectionGenes = 0;
            var disjointConnectionGenes = 0;
            var excessConnectionGenes = 0;
            var weightDifference = 0f;

            for (var i = 0; i < connectionIndices; i++)
            {
                var connection1 = genome1.Connections[i];
                var connection2 = genome1.Connections[i];

                if (connection1 != null && connection2 != null)
                {
                    matchingConnectionGenes++;
                    weightDifference += Mathf.Abs(connection1.Weight - connection2.Weight);
                }
                else if (connection1 == null && connection2 != null)
                {
                    if (highestConnectionInnovation1 > i) disjointConnectionGenes++;
                    else excessConnectionGenes++;
                }
                else if (connection1 != null && connection2 == null)
                {
                    if (highestConnectionInnovation2 > i) disjointConnectionGenes++;
                    else excessConnectionGenes++;
                }
            }

            return new GenomesGenesInfo(matchingGenes + matchingConnectionGenes,
                disjointGenes + disjointConnectionGenes,
                excessGenes + excessConnectionGenes, weightDifference / matchingConnectionGenes);
        }

        // parent1 should be the most fit parent
        public static Genome Crossover(Genome parent1, Genome parent2)
        {
            var child = new Genome();

            foreach (var node in parent1.Nodes.Values) child.Nodes.Add(node.Innovation, node.Copy());
            foreach (var connection1 in parent1.Connections.Values)
            {
                if (parent2.Connections.TryGetValue(connection1.Innovation, out var connection2))
                {
                    var childConnectionGene = RandomnessHandler.RandomBool() ? connection1 : connection2;
                    child.Connections.Add(childConnectionGene.Innovation, childConnectionGene);
                }
                else
                {
                    child.Connections.Add(connection1.Innovation, connection1);
                }
            }

            return child;
        }
    }
}