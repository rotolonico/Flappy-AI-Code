namespace AI.NEAT
{
    public class Counter
    {
        private int currentInnovation;

        public int GetInnovation()
        {
            currentInnovation++;
            return currentInnovation;
        }

        public Counter(int currentInnovation = 0) => this.currentInnovation = currentInnovation;
    }
}