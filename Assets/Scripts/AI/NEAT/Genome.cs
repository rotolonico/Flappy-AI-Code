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

        private Counter nodeInnovation;
        private Counter connectionInnovation = new Counter(Settings.Instance.inputs * Settings.Instance.outputs);

        public Genome() => InitializeGenome(new Dictionary<int, NodeGene>(), new Dictionary<int, ConnectionGene>());

        public Genome(Dictionary<int, NodeGene> nodes, Dictionary<int, ConnectionGene> connections) =>
            InitializeGenome(nodes, connections);

        public Genome(Genome toBeCopied)
        {
            InitializeGenome(new Dictionary<int, NodeGene>(), new Dictionary<int, ConnectionGene>());

            foreach (var node in toBeCopied.Nodes) Nodes[node.Key] = node.Value.Copy();
            foreach (var connection in toBeCopied.Connections) Connections[connection.Key] = connection.Value.Copy();
            
            nodeInnovation = toBeCopied.nodeInnovation;
            connectionInnovation = toBeCopied.connectionInnovation;
        }

        private void InitializeGenome(Dictionary<int, NodeGene> nodes, Dictionary<int, ConnectionGene> connections)
        {
            Nodes = nodes;
            Connections = connections;
            nodeInnovation = new Counter(Settings.Instance.inputs + Settings.Instance.outputs);
            connectionInnovation = new Counter(Settings.Instance.inputs * Settings.Instance.outputs);
        }

        public void AddNodeMutation()
        {
            var oldConnection = Connections.Values.ToArray()[RandomnessHandler.Random.Next(Connections.Count)];
            var inNode = Nodes[oldConnection.InNode];
            var outNode = Nodes[oldConnection.OutNode];

            oldConnection.Disable();

            var newNodeInnovation = 0;
            while (Nodes.ContainsKey(newNodeInnovation))
                newNodeInnovation++;
            
            var newConnectionInnovation = 0;
            while (Connections.ContainsKey(newConnectionInnovation))
                newConnectionInnovation++;
            
            var newConnectionInnovation2 = newConnectionInnovation + 1;
            while (Connections.ContainsKey(newConnectionInnovation2))
                newConnectionInnovation2++;

            var newNodeX = (inNode.X + outNode.X) / 2f;
            var newNode = new NodeGene(NodeGene.TypeE.Hidden, newNodeInnovation, newNodeX,
                Nodes.Count(n => Math.Abs(n.Value.X - newNodeX) < 0.001f));
            var inToNew = new ConnectionGene(inNode.Innovation, newNode.Innovation, 1, true,
                newConnectionInnovation);
            var newToOut = new ConnectionGene(newNode.Innovation, outNode.Innovation, oldConnection.Weight, true,
                newConnectionInnovation2);

            Nodes.Add(newNode.Innovation, newNode);
            Connections.Add(inToNew.Innovation, inToNew);
            Connections.Add(newToOut.Innovation, newToOut);
            
            //Debug.Log("Node mutation");
        }

        public void AddConnectionMutation(int maxAttempts)
        {
            var currentAttempt = 0;
            var success = false;

            while (currentAttempt < maxAttempts && !success)
            {
                currentAttempt++;

                var node1 = Nodes.Values.ToArray()[RandomnessHandler.Random.Next(Nodes.Count)];
                var node2 = Nodes.Values.ToArray()[RandomnessHandler.Random.Next(Nodes.Count)];
                var weight = RandomnessHandler.RandomMinusOneToOne();

                var reversed = false;
                if (Math.Abs(node1.X - node2.X) < 0.001f) continue;
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
                
                var newConnectionInnovation = 0;
                while (Connections.ContainsKey(newConnectionInnovation))
                    newConnectionInnovation++;

                var newConnection = new ConnectionGene(reversed ? node2.Innovation : node1.Innovation,
                    reversed ? node1.Innovation : node2.Innovation,
                    weight, true, newConnectionInnovation);
                Connections.Add(newConnection.Innovation, newConnection);

                success = true;
            }
            
            //Debug.Log("Connection mutation: " + success);
        }

        public void WeightMutation()
        {
            foreach (var connection in Connections.Values)
            {
                if (RandomnessHandler.RandomZeroToOne() < PerturbingProbability)
                    connection.Weight *= RandomnessHandler.RandomZeroToOne() * 4f - 2;
                else
                    connection.Weight = RandomnessHandler.RandomZeroToOne() * 4f - 2;
            }
            
            //Debug.Log("Weight mutation");
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
                var isNode1 = genome1.Nodes.ContainsKey(i);
                var isNode2 = genome2.Nodes.ContainsKey(i);

                if (isNode1 && isNode2) matchingGenes++;
                else if (!isNode1 && isNode2)
                {
                    if (highestInnovation1 > i) disjointGenes++;
                    else excessGenes++;
                }
                else if (isNode1)
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
                var isConnection1 = genome1.Connections.TryGetValue(i, out var connection1);
                var isConnection2 = genome2.Connections.TryGetValue(i, out var connection2);

                if (isConnection1 && isConnection2)
                {
                    matchingConnectionGenes++;
                    weightDifference += Mathf.Abs(connection1.Weight - connection2.Weight);
                }
                else if (!isConnection1 && isConnection2)
                {
                    if (highestConnectionInnovation1 > i) disjointConnectionGenes++;
                    else excessConnectionGenes++;
                }
                else if (isConnection1)
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
            var child = new Genome(parent1);
            return child;
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