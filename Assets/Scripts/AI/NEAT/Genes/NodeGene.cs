using System;
using UnityEngine;

namespace AI.NEAT.Genes
{
    public class NodeGene
    {
        public enum TypeE
        {
            Input,
            Hidden,
            Output
        }

        public TypeE Type { get;}
        public int Innovation { get;}
        public float X { get; }
        public int Y { get; }
        
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
