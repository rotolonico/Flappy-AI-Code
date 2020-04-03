
namespace Utils
{
    public static class RandomnessHandler
    {
        private static readonly System.Random Random = new System.Random();
    
        public static float RandomMinusOneToOne() => RandomMinMax(-1, 1);

        public static float RandomZeroToOne() => (float) Random.NextDouble();

        public static float RandomMinMax(float min, float max) => (float) Random.NextDouble() * (max - min) + min;
        
        public static int RandomIntMinMax(int min, int max) => Random.Next(max - min) + min;
    }
}
