using UnityEngine;

namespace AI.NEAT
{
    public class GenomesGenesInfo
    {
        public int MatchingGenes;
        public int DisjointGenes;
        public int ExcessGenes;
        public float AverageWeightDifference;
        
        public GenomesGenesInfo(int matchingGenes, int disjointGenes, int excessGenes, float averageWeightDifference)
        {
            MatchingGenes = matchingGenes;
            DisjointGenes = disjointGenes;
            ExcessGenes = excessGenes;
            AverageWeightDifference = averageWeightDifference;
        }
    }
}
