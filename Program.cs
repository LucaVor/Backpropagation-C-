using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using ChessAgain.NeuralNetwork;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

namespace ChessAgain
{
    public class Program
    {
        static void Main(string[] args)
        {
            List<List<DataPoint>> lichessDataBatches = new List<List<DataPoint>>();
            List<DataPoint> currentBatch = new List<DataPoint>();
            List<DataPoint> totalData = new List<DataPoint>();

            const int BATCH_SIZE = 2048;

            for (int i = 0; i < 1; i += 1)
            {
                string json = System.IO.File.ReadAllText(@"C:\Users\Luca Voros\OneDrive\Documents\LICHESS DATABASE\Outputs\output0.json");
                Console.WriteLine("Has JSON string.");

                dynamic lichessTraining = JsonConvert.DeserializeObject<dynamic>(json);
                Shuffle(lichessTraining, new Random());

                foreach (var item in lichessTraining)
                {
                    DataPoint dataPoint = new DataPoint();
                    dataPoint.inputs = Static.ConvertToDoubles(item[1]);
                    dataPoint.desiredOutputs = new double[] { double.Parse(item[2].ToString()) };
                    currentBatch.Add(dataPoint);
                    totalData.Add(dataPoint);

                    if (currentBatch.Count == BATCH_SIZE)
                    {
                        lichessDataBatches.Add(currentBatch);
                        currentBatch = new List<DataPoint>();
                    }
                }
            }

            int inputCount = lichessDataBatches[0][0].inputs.Length;
            int outputCount = lichessDataBatches[0][0].desiredOutputs.Length;

            Network network = new Network(inputCount, 256, 128, 32, outputCount);

            double recentCost = 100;
            int epoch = 0;

            while (recentCost > 0.01)
            {
                var c_batch = lichessDataBatches[epoch % lichessDataBatches.Count];
                double dynLearn = 1E-07;

                network.TrainDerivative(c_batch, dynLearn);
                double new_cost = network.GetCost(c_batch);

                recentCost = new_cost;

                if (epoch % 10 == 0)
                {
                    Console.WriteLine("Total Cost: " + network.GetCost(totalData));
                    network.Serialize();

                    Shuffle(totalData, new Random());

                    for (int i = 0; i < 10; i += 1)
                    {
                        double[] output = network.Evaluate(totalData[i].inputs);
                        Console.WriteLine(output[0] + " observed. Desired: " + totalData[i].desiredOutputs[0]);
                    }
                }

                epoch += 1;
            }
        }

        public static void Shuffle(List<DataPoint> list, Random rnd)
        {
            for (var i = list.Count; i > 0; i--)
                Swap(list, 0, rnd.Next(0, i));
        }
        public static void Shuffle(JArray list, Random rnd)
        {
            for (var i = list.Count; i > 0; i--)
                Swap(list, 0, rnd.Next(0, i));
        }

        public static void Swap(JArray list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        public static void Swap(List<DataPoint> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
