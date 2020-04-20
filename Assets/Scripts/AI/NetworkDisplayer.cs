using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI.NEAT;
using UnityEngine;
using UnityEngine.UI;

namespace AI
{
    public class NetworkDisplayer : MonoBehaviour
    {
        public static NetworkDisplayer Instance;

        public Transform networkContainer;
        public Transform networkConnectionsContainer;

        public GameObject layerPrefab;
        public GameObject nodePrefab;
        public GameObject connectionPrefab;

        public Color negativeColor;
        public Color positiveColor;

        private void Awake() => Instance = this;

        public void DisplayNetwork(Genome genome)
        {
            foreach (Transform layer in networkContainer) Destroy(layer.gameObject);
            foreach (Transform layer in networkConnectionsContainer) Destroy(layer.gameObject);

            var nodes = new SortedDictionary<float, List<int>>();
            foreach (var node in genome.Nodes)
            {
                if (!nodes.ContainsKey(node.Value.X)) nodes[node.Value.X] = new List<int>();
                nodes[node.Value.X].Add(node.Key);
            }

            var nodesGameObjects = new Dictionary<int, GameObject>();
            foreach (var nodeLayer in nodes)
            {
                var newLayer = Instantiate(layerPrefab, transform.position, Quaternion.identity).transform;
                newLayer.SetParent(networkContainer, false);
                foreach (var node in nodeLayer.Value)
                {
                    var newNode = Instantiate(nodePrefab, transform.position, Quaternion.identity).transform;
                    newNode.SetParent(newLayer, false);
                    nodesGameObjects.Add(node, newNode.gameObject);
                }
            }
            
            StartCoroutine(DisplayNetworkCoroutine(genome, nodesGameObjects));
        }

        private IEnumerator DisplayNetworkCoroutine(Genome genome, Dictionary<int, GameObject> nodesGameObjects)
        {
            yield return null;
            
            foreach (var connection in genome.Connections.Where(c => c.Value.Expressed))
            {
                var newConnection = Instantiate(connectionPrefab, transform.position, Quaternion.identity).transform;
                newConnection.SetParent(networkConnectionsContainer, false);
                var newConnectionLine = newConnection.GetComponent<LineRenderer>();
                newConnectionLine.SetPosition(0, nodesGameObjects[connection.Value.InNode].transform.position);
                newConnectionLine.SetPosition(1, nodesGameObjects[connection.Value.OutNode].transform.position);
                var lineColor = Color.Lerp(negativeColor, positiveColor, (connection.Value.Weight + 2) / 4);
                newConnectionLine.startColor = lineColor;
                newConnectionLine.endColor = lineColor;
            }
        }
    }
}