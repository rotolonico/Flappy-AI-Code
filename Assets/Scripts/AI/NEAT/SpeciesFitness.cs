using UnityEngine;

namespace AI.NEAT
{
    public class SpeciesFitness
    {
        public float Fitness;
        public GenomeWrapper BestMember;
        
        public SpeciesFitness(float fitness, GenomeWrapper bestMember)
        {
            Fitness = fitness;
            BestMember = bestMember;
        }
    }
}
