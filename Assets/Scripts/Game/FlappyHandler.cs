using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class FlappyHandler : MonoBehaviour
    {
        public int passedPipes;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Pipe")) Destroy(gameObject);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("PipeTrigger")) passedPipes++;
        }
    }
}