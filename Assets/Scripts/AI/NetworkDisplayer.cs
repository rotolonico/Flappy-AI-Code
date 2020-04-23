using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI.NEAT;
using NN;
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
        public Color activeColor;
        public Color inactiveColor;

        private NeuralNetwork currentNetwork;
        private readonly Dictionary<int, Image> currentNodes = new Dictionary<int, Image>();

        private void Awake() => Instance = this;

        private void Update() => UpdateGenomeNodes();

        public void DisplayNetwork(GenomeWrapper genome)
        {
            currentNetwork = genome.Network;
            currentNodes.Clear();
            
            foreach (Transform layer in networkContainer) Destroy(layer.gameObject);
            foreach (Transform layer in networkConnectionsContainer) Destroy(layer.gameObject);

            var nodes = new SortedDictionary<float, List<int>>();
            foreach (var node in genome.Genome.Nodes)
            {
                if (!nodes.ContainsKey(node.Value.X)) nodes[node.Value.X] = new List<int>();
                nodes[node.Value.X].Add(node.Key);
            }
            
            foreach (var nodeLayer in nodes)
            {
                var newLayer = Instantiate(layerPrefab, transform.position, Quaternion.identity).transform;
                newLayer.SetParent(networkContainer, false);
                foreach (var node in nodeLayer.Value)
                {
                    var newNode = Instantiate(nodePrefab, transform.position, Quaternion.identity).transform;
                    newNode.SetParent(newLayer, false);
                    currentNodes.Add(node, newNode.GetComponent<Image>());
                }
            }
            
            StartCoroutine(DisplayNetworkCoroutine(genome));
        }

        private IEnumerator DisplayNetworkCoroutine(GenomeWrapper genome)
        {
            yield return null;
            
            foreach (var connection in genome.Genome.Connections.Where(c => c.Value.Expressed))
            {
                var newConnection = Instantiate(connectionPrefab, transform.position, Quaternion.identity).transform;
                newConnection.SetParent(networkConnectionsContainer, false);
                var newConnectionLine = newConnection.GetComponent<LineRenderer>();
                newConnectionLine.SetPosition(0, currentNodes[connection.Value.InNode].transform.position);
                newConnectionLine.SetPosition(1, currentNodes[connection.Value.OutNode].transform.position);
                var lineColor = connection.Value.Weight < 0 ? negativeColor : positiveColor;
                lineColor.a = (connection.Value.Weight + 2) / 4;
                newConnectionLine.startColor = lineColor;
                newConnectionLine.endColor = lineColor;
            }
        }

        private void UpdateGenomeNodes()
        {
            foreach (var node in currentNodes)
                node.Value.color = Color.Lerp(inactiveColor, activeColor,
                    currentNetwork.GenomeNodes[node.Key].Value);
        }
    }
}