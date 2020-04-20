using System;

namespace AI.NEAT
{
    [Serializable]
    public class Counter
    {
        public int currentInnovation;

        public int GetInnovation()
        {
            currentInnovation++;
            return currentInnovation;
        }

        public Counter(int currentInnovation = 0) => this.currentInnovation = currentInnovation;
    }
}