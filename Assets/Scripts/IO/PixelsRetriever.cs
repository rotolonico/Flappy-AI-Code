using System;
using Game;
using UnityEngine;

namespace IO
{
    public class PixelsRetriever : MonoBehaviour
    {
        public static PixelsRetriever Instance;

        public Transform pixelsContainer;

        private void Awake()
        {
            Instance = this;
            pixelsContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(Settings.Instance.pipeDistance, Settings.Instance.height + 2);
        }

        public float[] RetrievePixels(GameObject flappy)
        {
            var pixels = new float[(Settings.Instance.pipeDistance + 2) * (Settings.Instance.height + 2)];
            for (var x = 0; x < Settings.Instance.pipeDistance + 2; x++)
            {
                for (var y = 0; y < Settings.Instance.height + 2; y++)
                {
                    var position = pixelsContainer.position + new Vector3(x - Settings.Instance.pipeDistance / 2 - 1, y - Settings.Instance.height / 2 - 1, 0);
                    var colliders = new Collider2D[100];
                    var collidersNumber = Physics2D.OverlapPointNonAlloc(position, colliders);
                    for (var j = 0; j < colliders.Length; j++)
                    {
                        var col = colliders[j];
                        if (j >= collidersNumber)
                        {
                            pixels[y * (Settings.Instance.pipeDistance + 2) + x] = 0;
                            break;
                        }

                        if (col.CompareTag("Pipe") || col.CompareTag("StaticPipe"))
                        {
                            pixels[y * (Settings.Instance.pipeDistance + 2) + x] = 0.5f;
                            break;
                        }

                        if (col.CompareTag("Flappy") && col.gameObject == flappy)
                        {
                            pixels[y * (Settings.Instance.pipeDistance + 2) + x] = 1;
                            break;
                        }

                        pixels[y * (Settings.Instance.pipeDistance + 2) + x] = 0;
                    }
                }
            }

            return pixels;
        }
    }
}
