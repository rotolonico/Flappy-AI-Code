using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AI.LiteNN;
using AI.NEAT.Genes;
using Game;
using NNUtils;
using UnityEngine;
using Utils;

namespace AI.NEAT
{
    [Serializable]
    public class Genome
    {
        private const float PerturbingProbability = 0.9f;
        private const float PerturbingStrength = 0.01f;
        private const float WeightStrength = 1f;

        public Dictionary<int, NodeGene> Nodes;
        public Dictionary<int, ConnectionGene> Connections;

        public Counter NodeCounter;
        public Counter ConnectionsCounter;
        
        public float Score;

        public Genome(Counter nodeCounter, Counter connectionsCounter) => InitializeGenome(new Dictionary<int, NodeGene>(), new Dictionary<int, ConnectionGene>(), nodeCounter, connectionsCounter);

        public Genome(Dictionary<int, NodeGene> nodes, Dictionary<int, ConnectionGene> connections, Counter nodeCounter, Counter connectionsCounter) =>
            InitializeGenome(nodes, connections, nodeCounter, connectionsCounter);

        public Genome(Genome toBeCopied)
        {
            InitializeGenome(new Dictionary<int, NodeGene>(), new Dictionary<int, ConnectionGene>(), NodeCounter, ConnectionsCounter);

            foreach (var node in toBeCopied.Nodes) Nodes[node.Key] = node.Value.Copy();
            foreach (var connection in toBeCopied.Connections) Connections[connection.Key] = connection.Value.Copy();

            NodeCounter = toBeCopied.NodeCounter;
            ConnectionsCounter = toBeCopied.ConnectionsCounter;
        }

        public int AliveTime { get; set; }

        private void InitializeGenome(Dictionary<int, NodeGene> nodes, Dictionary<int, ConnectionGene> connections, Counter nodeCounter, Counter connectionsCounter)
        {
            Nodes = nodes;
            Connections = connections;
            NodeCounter = nodeCounter;
            ConnectionsCounter = connectionsCounter;
        }

        public void AddNodeMutation(Counter nodeInnovation, Counter connectionInnovation)
        {
            if (Connections.Values.Count == 0) return;
            
            var oldConnection = Connections.Values.ToArray()[RandomnessHandler.Random.Next(Connections.Count)];
            var inNode = Nodes[oldConnection.InNode];
            var outNode = Nodes[oldConnection.OutNode];

            oldConnection.Disable();

            var newNodeX = (inNode.X + outNode.X) / 2f;
            var newNode = new NodeGene(NodeGene.TypeE.Hidden, nodeInnovation.GetInnovation(), newNodeX,
                Nodes.Count(n => Math.Abs(n.Value.X - newNodeX) < 0.001f));
            var inToNew = new ConnectionGene(inNode.Innovation, newNode.Innovation, 1, true,
                connectionInnovation.GetInnovation());
            var newToOut = new ConnectionGene(newNode.Innovation, outNode.Innovation, oldConnection.Weight, true,
                connectionInnovation.GetInnovation());

            Nodes.Add(newNode.Innovation, newNode);
            Connections.Add(inToNew.Innovation, inToNew);
            Connections.Add(newToOut.Innovation, newToOut);
        }

        public void AddConnectionMutation(Counter connectionInnovation, int maxAttempts)
        {
            var currentAttempt = 0;
            var success = false;

            while (currentAttempt < maxAttempts)
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

                var newConnection = new ConnectionGene(reversed ? node2.Innovation : node1.Innovation,
                    reversed ? node1.Innovation : node2.Innovation,
                    weight, true, connectionInnovation.GetInnovation());
                Connections.Add(newConnection.Innovation, newConnection);

                success = true;
            }
        }

        public void WeightMutation()
        {
            foreach (var connection in Connections.Values)
            {
                if (RandomnessHandler.RandomZeroToOne() < PerturbingProbability)
                    connection.Weight *= RandomnessHandler.RandomZeroToOne() * 2f - 1 * PerturbingStrength;
                else
                    connection.Weight = RandomnessHandler.RandomZeroToOne() * 2f - 1 * WeightStrength;
            }
        }

        public void ToggleConnectionMutation()
        {
            if (Connections.Values.Count == 0) return;
            
            var connection = Connections.Values.ToArray()[RandomnessHandler.Random.Next(Connections.Values.Count)];
            connection.Expressed = !connection.Expressed;
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

            if (genome1.Connections.Count == 0 || genome2.Connections.Count == 0) return new GenomesGenesInfo(0, 0, 0, 0);
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
                excessGenes + excessConnectionGenes, matchingConnectionGenes == 0 ? 0 : weightDifference / matchingConnectionGenes);
        }

        // parent1 should be the most fit parent
        public static Genome Crossover(Genome parent1, Genome parent2)
        {
            var child = new Genome(parent1.NodeCounter, parent1.ConnectionsCounter);
            
            foreach (var node in parent1.Nodes.Values) child.Nodes.Add(node.Innovation, node.Copy());
            foreach (var connection1 in parent1.Connections.Values)
            {
                if (parent2.Connections.TryGetValue(connection1.Innovation, out var connection2) && connection2.Expressed)
                {
                    var childConnectionGene = RandomnessHandler.RandomBool() ? connection1 : connection2;
                    child.Connections.Add(childConnectionGene.Innovation, childConnectionGene.Copy());
                }
                else
                {
                    child.Connections.Add(connection1.Innovation, connection1.Copy());
                }
            }

            return child;
        }

        public void SaveGenome(string name)
        {
            var json = StringSerializationAPI.Serialize(typeof(Genome), this);

            var file = new FileInfo($"{Application.persistentDataPath}/Genomes/{name}");
            file.Directory?.Create();
            File.WriteAllText(file.FullName, json);
        }

        public static Genome LoadGenome(string name)
        {
            var path = $"{Application.persistentDataPath}/Genomes/{name}";
            
            var json = !File.Exists(path)
                ? ""
                : File.ReadAllText(path);

            return json == "" ? null : StringSerializationAPI.Deserialize(typeof(Genome), json) as Genome;
        }
    }
}