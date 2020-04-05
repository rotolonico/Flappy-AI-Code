using UnityEngine;

namespace AI.NEAT
{
    public class SpeciesFitness
    {
        public float Fitness;
        public GenomeInfo BestMember;
        
        public SpeciesFitness(float fitness, GenomeInfo bestMember)
        {
            Fitness = fitness;
            BestMember = bestMember;
        }
    }
}
