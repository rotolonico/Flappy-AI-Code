using System;
using UnityEngine;

namespace AI.NEAT.Genes
{
    public class ConnectionGene
    {
        public int InNode { get; }
        public int OutNode { get; }
        public float Weight { get; set; }
        public bool Expressed { get; private set; }
        public int Innovation { get; }

        public ConnectionGene(int inNode, int outNode, float weight, bool expressed, int innovation)
        {
            InNode = inNode;
            OutNode = outNode;
            Weight = weight;
            Expressed = expressed;
            Innovation = innovation;
        }

        public void Disable() => Expressed = false;
        
        public ConnectionGene Copy() => new ConnectionGene(InNode, OutNode, Weight, Expressed, Innovation);
    }
}
