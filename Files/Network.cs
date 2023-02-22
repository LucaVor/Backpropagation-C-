using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ChessAgain.NeuralNetwork
{
    public class Network
    {
        public List<Layer> layers;
        int numLayers;
        int[] networkSize;

        public Network(params int[] networkSize)
        {
            layers = new List<Layer>();

            for (int i = 0; i < networkSize.Length - 1; i++)
            {
                Layer layer = new Layer(networkSize[i], networkSize[i + 1]);
                layers.Add(layer);
            }

            Layer outputLayer = new Layer(networkSize[networkSize.Length - 1], 0);
            layers.Add(outputLayer);

            numLayers = layers.Count;
            this.networkSize = networkSize;
        }

        public double[] Evaluate(double[] inputs)
        {
            layers[0].nodes = inputs;

            for (int i = 1; i < numLayers; i += 1)
            {
                layers[i].Evaluate(layers[i - 1]);
            }

            return layers[numLayers - 1].nodes;
        }

        public void TrainPrimitive(List<DataPoint> dataPoints, double learnRate, bool applyGradient = true)
        {
            const double h = 0.0001;

            double originalCost = GetCost(dataPoints);

            for (int layerID = 0; layerID < numLayers; layerID += 1)
            {
                layers[layerID].ResetGradients();
            }

            for (int layerID = 0; layerID < numLayers; layerID += 1)
            { 
                for (int nodeID = 0; nodeID < layers[layerID].layerLen; nodeID += 1)
                {
                    for (int prevID = 0; prevID < layers[layerID].nLayerLen; prevID += 1)
                    {
                        layers[layerID].outgoingWeights[nodeID, prevID] += h;
                        double weight_delta = (GetCost(dataPoints) - originalCost) / h;

                        layers[layerID].outgoingWeights[nodeID, prevID] -= h;

                        layers[layerID].SetWeightGradient(nodeID, prevID, weight_delta);
                    }

                    layers[layerID].biases[nodeID] += h;

                    double delta = (GetCost(dataPoints) - originalCost) / h;

                    layers[layerID].biases[nodeID] -= h;

                    layers[layerID].SetBiasGradient(nodeID, delta);
                }
            }

            for (int layerID = 0; layerID < numLayers; layerID += 1)
            {
                if (!applyGradient) {
                    continue;
                }

                layers[layerID].ApplyLearning(learnRate);
            }
        }

        public void TrainDerivative(List<DataPoint> dataPoints, double learnRate, bool applyGradient = true)
        {
            for (int layerID = 0; layerID < numLayers; layerID += 1)
            {
                layers[layerID].ResetGradients();
            }
            
            foreach(DataPoint dataPoint in dataPoints)
            {
                List<double> desiredOutputs = new List<double>();
                Static.CopyArray(ref desiredOutputs, dataPoint.desiredOutputs);

                Evaluate(dataPoint.inputs);

                for (int layerID = numLayers-2; layerID >= 0; layerID -= 1)
                {
                    double[] desiredActivationGrad = new double[layers[layerID].layerLen];

                    for (int nodeID = 0; nodeID < layers[layerID].layerLen; nodeID += 1)
                    {
                        for (int prevID = 0; prevID < layers[layerID].nLayerLen; prevID += 1)
                        {
                            double observedDesiredDeriv = Static.DesiredObservedDerivative(
                                desiredOutputs[prevID], layers[layerID + 1].nodes[prevID]
                            );

                            if (layerID != numLayers-2)
                            {
                                observedDesiredDeriv = desiredOutputs[prevID];
                            }

                            double zDerivative = Static.Derivative(layers[layerID + 1].unactivatedNodes[prevID]);

                            double thisActivation = layers[layerID].nodes[nodeID];

                            double weight_delta = thisActivation * zDerivative * observedDesiredDeriv;

                            //if (observedDesiredDeriv != 0 && zDerivative != 0 && thisActivation != 0)
                            //    Console.WriteLine(observedDesiredDeriv + " : " + zDerivative + " : " + thisActivation);

                            layers[layerID].AddWeightGradient(nodeID, prevID, weight_delta / dataPoints.Count);

                            double thisWeight = layers[layerID].outgoingWeights[nodeID, prevID];

                            desiredActivationGrad[nodeID] += (thisWeight * zDerivative * observedDesiredDeriv);

                            layers[layerID + 1].AddBiasGradient(prevID, (zDerivative * observedDesiredDeriv) / dataPoints.Count);
                        }
                    }

                    Static.CopyArray(ref desiredOutputs, desiredActivationGrad);
                }
            }

            for (int layerID = 0; layerID < numLayers; layerID += 1)
            {
                if (!applyGradient)
                {
                    continue;
                }

                layers[layerID].ApplyLearning(learnRate, (1/dataPoints.Count));
            }
        }

        public double GetCost(List<DataPoint> dataPoints)
        {
            double cost = 0;

            foreach (DataPoint dataPoint in dataPoints)
            {
                cost += GetCost(dataPoint.inputs, dataPoint.desiredOutputs);
            }

            return cost / dataPoints.Count;
        }

        public double GetCost(double[] inputs, double[] desiredOutputs)
        {
            double[] outputs = Evaluate(inputs);
            double cost = 0;

            for (int i = 0; i < outputs.Length; i += 1)
            {
                cost += GetCost(outputs[i], desiredOutputs[i]);
            }

            return cost;
        }

        public double GetCost(double observed, double desired)
        {
            return (desired - observed) * (desired - observed);
        }

        public Network Clone()
        {
            Network output = new Network(this.networkSize);

            for (int layerID = 0; layerID < numLayers; layerID += 1)
            {
                output.layers[layerID] = layers[layerID].Clone();
            }

            return output;
        }

        public void Serialize()
        {
            string jsonString = JsonConvert.SerializeObject(this);
            string path = @"C:\Users\Luca Voros\OneDrive\Documents\LICHESS DATABASE\network.json";

            using (StreamWriter sw = File.CreateText(path))
            {
                sw.Write(jsonString);
            }
        }

        public HyperParameters AutoCalculateHyperParameters(List<DataPoint> totalTrainingData)
        {
            double mean = 0.0;
            double total = 0.0;

            List<double> outputValues = new List<double>();

            for (int i = 0; i < totalTrainingData.Count; i += 1)
            {
                foreach (int output in totalTrainingData[i].desiredOutputs)
                {
                    mean += output;
                    total += 1;

                    outputValues.Add(output);
                }
            }

            mean /= total;

            double standardDev = 0.0;

            for (int i = 0; i < outputValues.Count; i += 1)
            {
                standardDev += (outputValues[i] - mean) * (outputValues[i] - mean);
            }

            standardDev = Math.Sqrt(standardDev / outputValues.Count);

            double networkMagnitude = 0.0;

            for (int i = layers.Count - 2; i >= 0; i--)
            {
                networkMagnitude += layers[i].outgoingWeights.GetLength(0) * layers[i].outgoingWeights.GetLength(1);
            }

            HyperParameters hyperParameters = new HyperParameters();

            hyperParameters.batchSize = Static.RoundNearestPow2((int)(totalTrainingData.Count * 0.1));

            hyperParameters.sigmoidRange = standardDev * 5;
            hyperParameters.learningRate = (mean / networkMagnitude) * Math.Sqrt(hyperParameters.batchSize);
            hyperParameters.weightPower = standardDev;

            return hyperParameters;
        }
    }
}
