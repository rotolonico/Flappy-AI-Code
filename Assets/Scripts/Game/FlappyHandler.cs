using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class FlappyHandler : MonoBehaviour
    {
        public static FlappyHandler Instance;

        private void Awake() => Instance = this;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Pipe")) SceneManager.LoadScene(0);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("PipeTrigger")) PipeSpawner.Instance.activePipes.RemoveAt(0);
        }
    }
}