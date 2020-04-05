using System.Collections.Generic;
using System.Linq;
using Utils;

namespace AI.NEAT
{
    public class Species
    {
        public GenomeInfo Mascot;
        public List<GenomeInfo> Members;
        public SpeciesFitness LastCalculatedFitness;

        public Species(GenomeInfo mascot)
        {
            Mascot = mascot;
            Members = new List<GenomeInfo> {mascot};
        }

        public SpeciesFitness CalculateSpeciesFitness()
        {
            float fitness = 0;
            var bestMember = Mascot;
            
            foreach (var m in Members)
            {
                fitness += m.Fitness;
                if (m.Fitness > bestMember.Fitness) bestMember = m;
            }

            fitness /= Members.Count;

            LastCalculatedFitness = new SpeciesFitness(fitness, bestMember);
            return LastCalculatedFitness;
        }

        public void Reset()
        {
            Mascot = Members[RandomnessHandler.Random.Next(Members.Count)];
            Members.Clear();
        }
    }
}