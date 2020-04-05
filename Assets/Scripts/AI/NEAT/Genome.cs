using System;
using System.Collections.Generic;
using System.Linq;
using AI.NEAT.Genes;
using UnityEngine;
using Utils;

namespace AI.NEAT
{
    public class Genome
    {
        private const float PerturbingProbability = 0.9f;

        private readonly Dictionary<int, NodeGene> nodes;
        private readonly Dictionary<int, ConnectionGene> connections;

        public Genome()
        {
            nodes = new Dictionary<int, NodeGene>();
            connections = new Dictionary<int, ConnectionGene>();
        }

        public void Mutation()
        {
            foreach (var connection in connections.Values)
            {
                if (RandomnessHandler.RandomZeroToOne() < PerturbingProbability)
                    connection.Weight *= connection.Weight * RandomnessHandler.RandomZeroToOne() * 4f - 2;
                else
                    connection.Weight = RandomnessHandler.RandomZeroToOne() * 4f - 2;
            }
        }

        public void AddNodeMutation(Counter nodeInnovation, Counter connectionInnovation)
        {
            var oldConnection = connections[RandomnessHandler.Random.Next(connections.Count)];
            var inNode = nodes[oldConnection.InNode];
            var outNode = nodes[oldConnection.OutNode];

            oldConnection.Disable();

            var newNode = new NodeGene(NodeGene.TypeE.Hidden, nodeInnovation.GetInnovation());
            var inToNew = new ConnectionGene(inNode.Id, newNode.Id, 1, true, connectionInnovation.GetInnovation());
            var newToOut = new ConnectionGene(newNode.Id, outNode.Id, oldConnection.Weight, true,
                connectionInnovation.GetInnovation());

            nodes.Add(newNode.Id, newNode);
            connections.Add(inToNew.Innovation, inToNew);
            connections.Add(newToOut.Innovation, newToOut);
        }

        public void AddConnectionMutation(Counter innovation, int maxAttempts)
        {
            var currentAttempt = 0;
            var success = false;

            while (currentAttempt < maxAttempts && !success)
            {
                currentAttempt++;
                
                var node1 = nodes[RandomnessHandler.Random.Next(nodes.Count)];
                var node2 = nodes[RandomnessHandler.Random.Next(nodes.Count)];
                var weight = RandomnessHandler.RandomMinusOneToOne();

                var reversed = false;
                if (node1.Type == NodeGene.TypeE.Hidden && node2.Type == NodeGene.TypeE.Input) reversed = true;
                else if (node1.Type == NodeGene.TypeE.Output && node2.Type == NodeGene.TypeE.Hidden) reversed = true;
                else if (node1.Type == NodeGene.TypeE.Output && node2.Type == NodeGene.TypeE.Input) reversed = true;

                var connectionExists = false;
                foreach (var connection in connections.Values)
                {
                    if (connection.InNode == node1.Id && connection.OutNode == node2.Id)
                    {
                        connectionExists = true;
                        break;
                    }

                    if (connection.InNode == node2.Id && connection.OutNode == node1.Id)
                    {
                        connectionExists = true;
                        break;
                    }
                }

                if (connectionExists || node1 == node2) return;

                var newConnection = new ConnectionGene(reversed ? node2.Id : node1.Id, reversed ? node1.Id : node2.Id,
                    weight, true, innovation.GetInnovation());
                connections.Add(newConnection.Innovation, newConnection);

                success = true;
            }
        }
        
        public static float CalculateCompatibilityDistance(Genome genome1, Genome genome2, float c1, float c2, float c3)
        {
            var genomesInfo = CalculateGenomesInfo(genome1, genome2);
            return genomesInfo.ExcessGenes * c1 + genomesInfo.DisjointGenes * c2 + genomesInfo.AverageWeightDifference * c3;
        }

        public static GenomesGenesInfo CalculateGenomesInfo(Genome genome1, Genome genome2)
        {
            var highestInnovation1 = genome1.nodes.Keys.Max();
            var highestInnovation2 = genome2.nodes.Keys.Max();
            var indices = Math.Max(highestInnovation1, highestInnovation2);

            var matchingGenes = 0;
            var disjointGenes = 0;
            var excessGenes = 0;

            for (var i = 0; i < indices; i++)
            {
                var node1 = genome1.nodes[i];
                var node2 = genome1.nodes[i];

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

            var highestConnectionInnovation1 = genome1.connections.Keys.Max();
            var highestConnectionInnovation2 = genome2.connections.Keys.Max();
            var connectionIndices = Math.Max(highestConnectionInnovation1, highestConnectionInnovation2);

            var matchingConnectionGenes = 0;
            var disjointConnectionGenes = 0;
            var excessConnectionGenes = 0;
            var weightDifference = 0f;

            for (var i = 0; i < connectionIndices; i++)
            {
                var connection1 = genome1.connections[i];
                var connection2 = genome1.connections[i];

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

            return new GenomesGenesInfo(matchingGenes + matchingConnectionGenes, disjointGenes + disjointConnectionGenes,
                excessGenes + excessConnectionGenes, weightDifference / matchingConnectionGenes);
        }

        // parent1 should be the most fit parent
        public static Genome Crossover(Genome parent1, Genome parent2)
        {
            var child = new Genome();

            foreach (var node in parent1.nodes.Values) child.nodes.Add(node.Id, node.Copy());
            foreach (var connection1 in parent1.connections.Values)
            {
                if (parent2.connections.TryGetValue(connection1.Innovation, out var connection2))
                {
                    var childConnectionGene = RandomnessHandler.RandomBool() ? connection1 : connection2;
                    child.connections.Add(childConnectionGene.Innovation, childConnectionGene);
                }
                else
                {
                    child.connections.Add(connection1.Innovation, connection1);
                }
            }

            return child;
        }
    }
}