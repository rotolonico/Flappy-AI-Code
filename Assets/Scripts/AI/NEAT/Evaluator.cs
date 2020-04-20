using System;
using System.Collections.Generic;
using System.Linq;
using AI.NEAT.Genes;
using Game;
using Utils;

namespace AI.NEAT
{
    public class Evaluator
    {
        private readonly int populationSize;
        private readonly Func<Genome, float> fitnessFunction;

        private readonly Counter nodeInnovation;
        private readonly Counter connectionInnovation;

        private const float C1 = 1.0f;
        private const float C2 = 1.0f;
        private const float C3 = 0.4f;
        private const float DT = 5.0f;
        private const float WeightMutationRate = 0.5f;
        private const float AddConnectionRate = 0.5f;
        private const float ToggleConnectionRate = 0f;
        private const float AddNodeRate = 0.3f;
        private const int ConnectionMutationMaxAttempts = 10;


        public List<GenomeWrapper> Genomes = new List<GenomeWrapper>();
        public readonly List<Species> species = new List<Species>();

        public float HighestScore;
        public GenomeWrapper FittestGenome;

        public Evaluator(int populationSize, Counter nodeInnovation,
            Counter connectionInnovation, Func<Genome, float> fitnessFunction, Genome startingGenome = null)
        {
            this.populationSize = populationSize;

            if (startingGenome == null)
            {
                this.nodeInnovation = nodeInnovation;
                this.connectionInnovation = connectionInnovation;

                var inputGenes = new Dictionary<int, NodeGene>();
                var outputGenes = new Dictionary<int, NodeGene>();
                for (var i = 0; i < Settings.Instance.inputs; i++)
                {
                    var newNodeInnovation = nodeInnovation.GetInnovation();
                    inputGenes.Add(newNodeInnovation, new NodeGene(NodeGene.TypeE.Input, newNodeInnovation, 0, i));
                }

                for (var i = 0; i < Settings.Instance.outputs; i++)
                {
                    var newNodeInnovation = nodeInnovation.GetInnovation();
                    outputGenes.Add(newNodeInnovation, new NodeGene(NodeGene.TypeE.Output, newNodeInnovation, 1, i));
                }

                var connectionGenes = new Dictionary<int, ConnectionGene>();
                if (Settings.Instance.autoGenerateConnections)
                {
                    foreach (var inputGene in inputGenes)
                    {
                        foreach (var outputGene in outputGenes)
                        {
                            var newConnectionInnovation = connectionInnovation.GetInnovation();
                            connectionGenes.Add(newConnectionInnovation,
                                new ConnectionGene(inputGenes.FirstOrDefault(x => x.Value == inputGene.Value).Key,
                                    outputGenes.FirstOrDefault(x => x.Value == outputGene.Value).Key,
                                    RandomnessHandler.RandomZeroToOne() * 4 - 2, true,
                                    newConnectionInnovation));
                        }
                    }
                }

                var nodeGenes = new Dictionary<int, NodeGene>(inputGenes);
                outputGenes.ToList().ForEach(x => nodeGenes.Add(x.Key, x.Value));

                startingGenome = new Genome(nodeGenes, connectionGenes, nodeInnovation, connectionInnovation);
            }
            else
            {
                this.nodeInnovation = startingGenome.NodeCounter;
                this.connectionInnovation = startingGenome.ConnectionsCounter;
            }

            for (var i = 0; i < populationSize; i++) Genomes.Add(new GenomeWrapper(new Genome(startingGenome)));
            this.fitnessFunction = fitnessFunction;
        }

        public void Evaluate()
        {
            species.Clear();
            HighestScore = float.MinValue;
            FittestGenome = null;

            foreach (var g in Genomes)
            {
                var foundSpecies = false;
                foreach (var s in species.Where(
                    s => Genome.CalculateCompatibilityDistance(g.Genome, s.Mascot.Genome, C1, C2, C3) < DT))
                {
                    s.Members.Add(g);
                    g.Species = s;
                    foundSpecies = true;
                    break;
                }

                if (!foundSpecies)
                {
                    var newSpecies = new Species(g);
                    species.Add(newSpecies);
                    g.Species = newSpecies;
                }

                var score = EvaluateGenome(g.Genome);
                g.Fitness = g.Fitness * 0.5f + score;

                if (!(HighestScore <= score)) continue;
                HighestScore = score;
                if (FittestGenome != null) FittestGenome.Best = false;
                FittestGenome = g;
                g.Best = true;
            }

            species.RemoveAll(s => s.Members.Count == 0);

            if (species.Count > populationSize / 5)
            {
                var orderedSpecies = species.OrderBy(s => s.CalculateSpeciesFitness().Fitness).ToArray();
                var speciesIndex = 0;
                while (species.Count > populationSize / 5)
                {
                    species.Remove(orderedSpecies[speciesIndex]);
                    speciesIndex++;
                }
            }

            Genomes.Clear();

            foreach (var speciesFitness in species.Select(s => s.CalculateSpeciesFitness())
                .Where(f => !f.BestMember.Best))
                Genomes.Add(new GenomeWrapper(new Genome(speciesFitness.BestMember.Genome)));
            Genomes.Add(new GenomeWrapper(new Genome(FittestGenome.Genome)) {Best = true});

            while (Genomes.Count < populationSize)
            {
                var s = GetRandomSpecies();

                var p1 = GetRandomGenomeInSpecies(s);
                var p2 = GetRandomGenomeInSpecies(s);

                var mostFitParent = p1.Fitness > p2.Fitness ? p1 : p2;
                var leastFitParent = p1.Fitness > p2.Fitness ? p2 : p1;

                var child = Genome.Crossover(mostFitParent.Genome, leastFitParent.Genome);

                if (RandomnessHandler.RandomZeroToOne() < WeightMutationRate) child.WeightMutation();
                if (RandomnessHandler.RandomZeroToOne() < AddConnectionRate)
                    child.AddConnectionMutation(connectionInnovation, ConnectionMutationMaxAttempts);
                if (RandomnessHandler.RandomZeroToOne() < ToggleConnectionRate)
                    child.ToggleConnectionMutation();
                if (RandomnessHandler.RandomZeroToOne() < AddNodeRate)
                    child.AddNodeMutation(nodeInnovation, connectionInnovation);

                Genomes.Add(new GenomeWrapper(child) {Fitness = leastFitParent.Fitness});
            }
        }

        private Species GetRandomSpecies()
        {
            var totalFitness = species.Sum(s => s.LastCalculatedFitness.Fitness);
            var speciesIndex = RandomnessHandler.RandomZeroToOne() * totalFitness;
            var currentFitness = 0f;

            foreach (var s in species)
            {
                currentFitness += s.LastCalculatedFitness.Fitness;
                if (currentFitness >= speciesIndex)
                    return s;
            }

            return null;
        }

        private GenomeWrapper GetRandomGenomeInSpecies(Species selectedSpecies)
        {
            var totalFitness = selectedSpecies.Members.Sum(g => g.Fitness);
            var speciesIndex = RandomnessHandler.RandomZeroToOne() * totalFitness;
            var currentFitness = 0f;

            foreach (var g in selectedSpecies.Members)
            {
                currentFitness += g.Fitness;
                if (currentFitness >= speciesIndex)
                    return g;
            }

            return null;
        }

        private float EvaluateGenome(Genome genome) => fitnessFunction.Invoke(genome);
    }
}