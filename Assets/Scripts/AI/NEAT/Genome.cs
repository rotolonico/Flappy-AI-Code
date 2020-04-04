using System.Collections.Generic;
using AI.NEAT.Genes;
using UnityEngine;
using Utils;

namespace AI.NEAT
{
    public class Genome
    {
        private readonly Dictionary<int, NodeGene> nodes;
        private readonly Dictionary<int, ConnectionGene> connections;

        public Genome()
        {
            nodes = new Dictionary<int, NodeGene>();
            connections = new Dictionary<int, ConnectionGene>();
        }

        public void AddNodeMutation()
        {
            var oldConnection = connections[RandomnessHandler.Random.Next(connections.Count)];
            var inNode = nodes[oldConnection.InNode];
            var outNode = nodes[oldConnection.OutNode];
            
            oldConnection.Disable();

            var newNode = new NodeGene(NodeGene.TypeE.Hidden, nodes.Count);
            var inToNew = new ConnectionGene(inNode.Id, newNode.Id, 1, true, 0);
            var newToOut = new ConnectionGene(newNode.Id, outNode.Id, oldConnection.Weight, true, 0);
            
            nodes.Add(newNode.Id, newNode);
            connections.Add(inToNew.Innovation, inToNew);
            connections.Add(newToOut.Innovation, newToOut);
        }
        
        public void AddConnectionMutation()
        {
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

            var newConnection = new ConnectionGene(reversed ? node2.Id : node1.Id, reversed ? node1.Id : node2.Id, weight, true, 0);
            connections.Add(newConnection.Innovation, newConnection);
        }

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