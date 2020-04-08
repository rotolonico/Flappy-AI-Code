using UnityEngine;

namespace Game
{
    public class PipeHandler : MonoBehaviour
    {
        public float x;
        public float y;
        public bool complementary;
        public PipeHandler complementaryPipe;

        private void Start()
        {
            transform.position = new Vector3(x, (Settings.Instance.height - y) / 2 * (complementary ? 1 : -1));
            GetComponent<BoxCollider2D>().size = new Vector2(1, y);
            GetComponent<SpriteRenderer>().size = new Vector2(1, y);
            if (complementary) Destroy(transform.GetChild(0).gameObject);
        }
    }
}
