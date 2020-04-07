using AI.LiteNN;
using Game;
using NN;

namespace AI.NEAT
{
    public class GenomeWrapper
    {
        public Genome Genome;
        public float Fitness;
        public Species Species;
        public NeuralNetwork Network;
        public bool Best;

        public GenomeWrapper(Genome genome)
        {
            Genome = genome;
            Network = new NeuralNetwork(Genome);
        }
    }
}