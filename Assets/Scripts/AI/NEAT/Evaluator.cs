using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Random = UnityEngine.Random;

namespace AI.NEAT
{
    public abstract class Evaluator
    {
        private Counter nodeInnovation;
        private Counter connectionInnovation;

        private const float C1 = 1.0f;
        private const float C2 = 1.0f;
        private const float C3 = 0.4f;
        private const float DT = 10.0f;
        private const float MutationRate = 0.5f;
        private const float AddConnectionRate = 0.1f;
        private const float AddNodeRate = 0.1f;
        private const int ConnectionMutationMaxAttempts = 10;

        private int populationSize;

        private List<GenomeInfo> genomes = new List<GenomeInfo>();
        private List<Species> species = new List<Species>();

        public void Evaluate()
        {
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
                var adjustedScore = score / g.Species.Members.Count;
                g.Fitness = adjustedScore;
            }

            genomes.Clear();

            foreach (var speciesFitness in species.Select(s => s.CalculateSpeciesFitness()))
                genomes.Add(speciesFitness.BestMember);

            while (genomes.Count < populationSize)
            {
                var s = GetRandomSpecies();

                var p1 = GetRandomGenomeInSpecies(s);
                var p2 = GetRandomGenomeInSpecies(s);

                var mostFitParent = p1.Fitness > p2.Fitness ? p1.Genome : p2.Genome;
                var leastFitParent = p1.Fitness > p2.Fitness ? p2.Genome : p1.Genome;

                var child = Genome.Crossover(mostFitParent, leastFitParent);

                if (RandomnessHandler.RandomZeroToOne() < MutationRate) child.Mutation();
                if (RandomnessHandler.RandomZeroToOne() < AddConnectionRate) child.AddConnectionMutation(connectionInnovation, ConnectionMutationMaxAttempts);
                if (RandomnessHandler.RandomZeroToOne() < AddNodeRate) child.AddNodeMutation(nodeInnovation, connectionInnovation);
                
                genomes.Add(new GenomeInfo(child));
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

        private GenomeInfo GetRandomGenomeInSpecies(Species selectedSpecies)
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

        public abstract float EvaluateGenome(Genome genome);
    }
}