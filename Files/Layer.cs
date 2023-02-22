using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace ChessAgain.NeuralNetwork
{
    public class Layer
    {
        public double[] nodes;
        public double[] unactivatedNodes;

        public double[] biases;
        public double[,] outgoingWeights;

        public double[] gradient_B;
        public double[,] gradient_W;

        public int layerLen;
        public int nLayerLen;

        private System.Random rand;

        public Layer(int size, int nextSize)
        {
            nodes = new double[size];
            unactivatedNodes = new double[size];
            biases = new double[size];

            outgoingWeights = new double[size, nextSize];

            gradient_B = new double[size];
            gradient_W = new double[size, nextSize];

            rand = new System.Random();

            for (int x = 0; x < size; x += 1)
            {
                nodes[x] = 0;
                unactivatedNodes[x] = 0;

                biases[x] = 0;
                
                for (int y = 0; y < nextSize; y += 1)
                {
                    outgoingWeights[x, y] = GetRandomWeight();
                }
            }

            layerLen = nodes.Length;
            nLayerLen = nextSize;
        }
        public double GetRandomWeight()
        {
            return Math.Pow((((rand.NextDouble())) * 2 - 1), 14) * 1;
        }

        public void AdjustWeight(int x, int y, double value)
        {
            outgoingWeights[x, y] = outgoingWeights[x, y] + value;
        }

        public void AdjustBias(int x, double value)
        {
            biases[x] = biases[x] + value;
        }

        public void SetWeightGradient(int x, int y, double value)
        {
            gradient_W[x, y] = value;
        }

        public void SetBiasGradient(int x, double value)
        {
            gradient_B[x] = value;
        }

        public void AddWeightGradient(int x, int y, double value)
        {
            gradient_W[x, y] += value;
        }

        public void AddBiasGradient(int x, double value)
        {
            gradient_B[x] += value;
        }

        public void ResetGradients()
        {
            for (int x = 0; x < gradient_W.GetLength(0); x += 1)
            {
                for (int y = 0; y < gradient_W.GetLength(1); y += 1)
                {
                    gradient_W[x, y] = 0;
                }
            }
        }

        public void ApplyLearning(double learnRate, double optionalCoefficient = 1)
        {
            for (int i = 0; i < layerLen; i += 1)
            {
                for (int j = 0; j < nLayerLen; j += 1)
                {
                    if (gradient_W[i, j] != 0)
                    {
                        //Console.WriteLine("NON ZERO DERIV " + gradient_W[i, j].ToString());
                    }
                    outgoingWeights[i, j] -= gradient_W[i, j] * learnRate;
                }

                biases[i] -= gradient_B[i] * learnRate * optionalCoefficient;
            }    
        }

        public void Evaluate(Layer prevLayer)
        {
            for (int nodeID = 0; nodeID < layerLen; nodeID += 1)
            {
                double nodeWeight = 0;

                for (int prevID = 0; prevID < prevLayer.layerLen; prevID += 1)
                {
                    nodeWeight += prevLayer.nodes[prevID] * prevLayer.outgoingWeights[prevID, nodeID];
                }

                double bias = biases[nodeID];

                unactivatedNodes[nodeID] = nodeWeight;

                nodeWeight = Static.Activate(nodeWeight + bias);

                nodes[nodeID] = nodeWeight;
            }
        }

        public Layer Clone()
        {
            Layer output = new Layer(nodes.Length, nLayerLen);

            for (int x = 0; x < outgoingWeights.GetLength(0); x += 1)
            {
                for (int y = 0; y < outgoingWeights.GetLength(1); y += 1)
                {
                    output.outgoingWeights[x, y] = outgoingWeights[x, y];
                    output.gradient_W[x, y] = gradient_W[x, y];
                }
            }

            for (int x = 0; x < nodes.Length; x += 1)
            {
                output.biases[x] = biases[x];
                output.gradient_B[x] = gradient_B[x];
            }

            output.layerLen = layerLen;
            output.nLayerLen = nLayerLen;

            return output;
        }
    }
}
