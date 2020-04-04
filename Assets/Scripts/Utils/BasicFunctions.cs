using System;
using UnityEngine;

namespace Utils
{
    public class BasicFunctions : MonoBehaviour
    {
        public static float Sigmoid(float x) => 1f / (1f + (float) Math.Exp(-x));

        public static float HardSigmoid(float x)
        {
            if (x < -2.5f)
                return 0;
            if (x > 2.5f)
                return 1;
            return 0.2f * x + 0.5f;
        }

        public static float SigmoidDerivative(float x) => x * (1 - x);
    
    
    
    }
}
