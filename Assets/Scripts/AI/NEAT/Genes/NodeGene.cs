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
        public int Id { get;}
        
        public NodeGene(TypeE type, int id)
        {
            Type = type;
            Id = id;
        }

        public NodeGene Copy() => new NodeGene(Type, Id);
    }
}
