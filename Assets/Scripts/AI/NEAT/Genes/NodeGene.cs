using System;
using UnityEngine;

namespace AI.NEAT.Genes
{
    [Serializable]
    public class NodeGene
    {
        public enum TypeE
        {
            Input,
            Hidden,
            Output
        }

        public TypeE Type;
        public int Innovation;
        public float X;
        public int Y;
        
        public NodeGene(TypeE type, int innovation, float x, int y)
        {
            Type = type;
            Innovation = innovation;
            X = x;
            Y = y;
        }

        public NodeGene Copy() => new NodeGene(Type, Innovation, X, Y);
    }
}
