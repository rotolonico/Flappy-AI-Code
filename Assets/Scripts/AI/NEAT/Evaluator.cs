using System;
using System.Collections.Generic;
using System.Linq;
using AI.NEAT.Genes;
using Game;
using Utils;
using Random = UnityEngine.Random;

namespace AI.NEAT
{
    public class Evaluator
    {
        private int populationSize;
        private Counter nodeInnovation;
        private Counter connectionInnovation;
        private Func<Genome, float> fitnessFunction;

        private const float C1 = 1.0f;
        private const float C2 = 1.0f;
        private const float C3 = 0.4f;
        private const float DT = 10.0f;
        private const float WeightMutationRate = 0.5f;
        private const float AddConnectionRate = 0.1f;
        private const float AddNodeRate = 0.1f;
        private const int ConnectionMutationMaxAttempts = 10;


        public List<GenomeWrapper> genomes = new List<GenomeWrapper>();
        private List<Species> species = new List<Species>();

        private float highestScore;
        private GenomeWrapper fittestGenome;

        public Evaluator(int populationSize, Counter nodeInnovation,
            Counter connectionInnovation, Func<Genome, float> fitnessFunction)
        {
            this.populationSize = populationSize;
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
            foreach (var inputGene in inputGenes)
            {
                foreach (var outputGene in outputGenes)
                {
                    var newConnectionInnovation = connectionInnovation.GetInnovation();
                    connectionGenes.Add(newConnectionInnovation,
                        new ConnectionGene(inputGenes.FirstOrDefault(x => x.Value == inputGene.Value).Key,
                            outputGenes.FirstOrDefault(x => x.Value == outputGene.Value).Key, 0, true,
                            newConnectionInnovation));
                }
            }

            var nodeGenes = new Dictionary<int, NodeGene>(inputGenes);
            outputGenes.ToList().ForEach(x => nodeGenes.Add(x.Key, x.Value));

            var startingGenome = new Genome(nodeGenes, connectionGenes);
            for (var i = 0; i < populationSize; i++) genomes.Add(new GenomeWrapper(new Genome(startingGenome)));
            this.fitnessFunction = fitnessFunction;
        }

        public void Evaluate()
        {
            foreach (var s in species) s.Reset();
            highestScore = float.MinValue;
            fittestGenome = null;

            foreach (var g in genomes)
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

                if (foundSpecies) continue;
                var newSpecies = new Species(g);
                species.Add(newSpecies);
                g.Species = newSpecies;

                var score = EvaluateGenome(g.Genome);
                g.Fitness = score;

                if (!(highestScore < score)) continue;
                highestScore = score;
                fittestGenome = g;
            }

            species.RemoveAll(s => s.Members.Count == 0);

            genomes.Clear();

            foreach (var speciesFitness in species.Select(s => s.CalculateSpeciesFitness()))
            {
                genomes.Add(speciesFitness.BestMember);
            }

            while (genomes.Count < populationSize)
            {
                var s = GetRandomSpecies();

                var p1 = GetRandomGenomeInSpecies(s);
                var p2 = GetRandomGenomeInSpecies(s);

                var mostFitParent = p1.Fitness > p2.Fitness ? p1.Genome : p2.Genome;
                var leastFitParent = p1.Fitness > p2.Fitness ? p2.Genome : p1.Genome;

                var child = Genome.Crossover(mostFitParent, leastFitParent);

                if (RandomnessHandler.RandomZeroToOne() < WeightMutationRate) child.WeightMutation();
                if (RandomnessHandler.RandomZeroToOne() < AddConnectionRate)
                    child.AddConnectionMutation(connectionInnovation, ConnectionMutationMaxAttempts);
                if (RandomnessHandler.RandomZeroToOne() < AddNodeRate)
                    child.AddNodeMutation(nodeInnovation, connectionInnovation);

                genomes.Add(new GenomeWrapper(child));
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
            var totalFitness = selectedSpecies.Members.Sum(s => s.Fitness);
            var speciesIndex = RandomnessHandler.RandomZeroToOne() * totalFitness;
            var currentFitness = 0f;

            foreach (var s in selectedSpecies.Members)
            {
                currentFitness += s.Fitness;
                if (currentFitness >= speciesIndex)
                    return s;
            }

            return null;
        }

        private float EvaluateGenome(Genome genome) => fitnessFunction.Invoke(genome);
    }
}