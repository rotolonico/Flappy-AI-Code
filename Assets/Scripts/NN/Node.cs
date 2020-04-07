using System;
using System.Collections.Generic;
using System.Linq;
using NNUtils;
using UnityEngine;
using Utils;

namespace NN
{
    [Serializable]
    public class Node
    {
        [SerializeField] public Dictionary<Node, float> weights;
        [SerializeField] private float bias;
        public float TrainingBiasSmudge { get; set; }
        public Dictionary<Node, float> TrainingWeightsSmudge { get; set; }

        public Dictionary<Node, float> CorrectWeights { get; set; }
        public float CorrectBias { get; set; }

        [SerializeField] private bool isInput;
        private float inputValue;

        public float Value { get; private set; }
        public bool isAcceptableValue;
        public bool ValueCalculated;
        public float RawValue { get; private set; }
        public float X { get; set; }

        private float desiredValue;

        public Node(bool isInput = false)
        {
            weights = new Dictionary<Node, float>();
            TrainingWeightsSmudge = new Dictionary<Node, float>();
            bias = 0;
            this.isInput = isInput;
        }

        public Node(Node[] nodes, bool isInput = false)
        {
            weights = new Dictionary<Node, float>();
            TrainingWeightsSmudge = new Dictionary<Node, float>();
            foreach (var node in nodes)
            {
                weights.Add(node, RandomnessHandler.RandomZeroToOne() * Mathf.Sqrt(2f / nodes.Length));
                TrainingWeightsSmudge.Add(node, 0);
            }

            bias = 0;
            this.isInput = isInput;
        }

        public Node(Dictionary<Node, float> weights, float bias, bool isInput = false)
        {
            this.weights = weights;
            TrainingWeightsSmudge = weights.Clone();
            foreach (var w in TrainingWeightsSmudge) TrainingWeightsSmudge[w.Key] = 0;
            this.bias = bias;
            this.isInput = isInput;
        }

        public Node(bool isInput, float x)
        {
            weights = new Dictionary<Node, float>();
            TrainingWeightsSmudge = new Dictionary<Node, float>();
            bias = 0;
            this.isInput = isInput;
            this.X = x;
        }

        public float CalculateValue()
        {
            if (ValueCalculated) return Value;

            RawValue = isInput ? inputValue : weights.Sum(weight => weight.Key.CalculateValue() * weight.Value) + bias;
            Value = isInput ? inputValue : BasicFunctions.Sigmoid(RawValue);
            desiredValue = Value;
            ValueCalculated = true;

            return Value;
        }

        public void SetInputValue(float newInputValue) => inputValue = newInputValue;

        public float GetWeight(Node node) => weights[node];

        public void SetWeight(Node node, float weight) => weights[node] = weight;

        public void SmudgeWeight(Node node, float weight) => weights[node] = Mathf.Clamp(-5, weights[node] + weight, 5);

        public float GetBias() => bias;

        public void SetBias(float newBias) => bias = newBias;
        public void SmudgeBias(float newBias) => bias = Mathf.Clamp(-5, bias + newBias, 5);

        public void SetDesiredValue(float newDesiredValue) => desiredValue = newDesiredValue;

        public void SmudgeDesiredValue(float newDesiredValue) => desiredValue += newDesiredValue;

        public float CalculateCostDelta(float classificationOverPrecision, float maxError)
        {
            var cost = desiredValue - Value;
            if (desiredValue < 0.5f && Value < 0.5f || desiredValue > 0.5f && Value > 0.5f)
            {
                isAcceptableValue = Mathf.Abs(cost) <= maxError;
                cost /= classificationOverPrecision;
            }
            else isAcceptableValue = false;

            return cost;
        }

        public Node[] GetConnectedNodes() => weights.Keys.ToArray();

        public void PrepareNodeToSave()
        {
            CorrectBias = bias;
            CorrectWeights = new Dictionary<Node, float>(weights);
            RandomizeWeightsAndBiases();
        }

        public void RandomizeWeightsAndBiases()
        {
            bias = RandomnessHandler.RandomMinMax(-5, 5);
            foreach (var node in CorrectWeights.Keys) weights[node] = RandomnessHandler.RandomMinMax(-5, 5);
        }
    }
}