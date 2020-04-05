namespace AI.NEAT
{
    public class GenomeInfo
    {
        public Genome Genome;
        public float Fitness;
        public Species Species;

        public GenomeInfo(Genome genome)
        {
            Genome = genome;
        }
    }
}