namespace NN
{
    public class Layer
    {
        public Node[] Nodes;
        private bool isInput;

        public Layer(int nodesNumber, Layer prevLayer = null)
        {
            Nodes = new Node[nodesNumber];
        
            if (prevLayer == null) isInput = true;
        
            for (var i = 0; i < Nodes.Length; i++)
            {
                if (isInput) Nodes[i] = new Node(isInput);
                else Nodes[i] = new Node(prevLayer.Nodes);
            }
        }
    }
}
