using System;
using UnityEngine;

namespace Game
{
    public class Settings : MonoBehaviour
    {
        public static Settings Instance;

        public int gameSpeed;
        public int height;
        public int pipeDistance;
        
        private void Awake() => Instance = this;
    }
}
