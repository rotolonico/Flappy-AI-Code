using AI.LiteNN;
using Game;

namespace AI.NEAT
{
    public class GenomeWrapper
    {
        public Genome Genome;
        public float Fitness;
        public Species Species;

        public GenomeWrapper(Genome genome) => Genome = genome;
    }
}