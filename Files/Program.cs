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
        public static double[] FenToNeural (string fen)
        {
            double[] inputs = new double[768];

            fen = fen.Replace("/", "");
            List<string> numbers = new List<string>();

            for (int i = 0; i < 10; i += 1)
            {
                numbers.Add(i.ToString());
            }

            Dictionary<string,int> pieceOffsets = new Dictionary<string, int>()
            {
                { "P", 0 },
                { "N", 64 },
                { "B", 128 },
                { "R", 192 },
                { "Q", 256 },
                { "K", 320 },
                { "p", 384 },
                { "n", 448 },
                { "b", 512 },
                { "r", 576 },
                { "q", 640 },
                { "k", 704 }
            };

            int sqrIndex = -1;

            foreach (char c in fen)
            {
                string l = c.ToString();
                sqrIndex += 1;

                if (numbers.Contains(l))
                {
                    sqrIndex += (int.Parse(l) - 1);
                    continue;
                }

                inputs[sqrIndex + pieceOffsets[l]] = 1;
            }

            return inputs;
        }
        static void Main(string[] args)
        {
            List<List<DataPoint>> lichessDataBatches = new List<List<DataPoint>>();
            List<DataPoint> currentBatch = new List<DataPoint>();
            List<DataPoint> totalData = new List<DataPoint>();

            const int BATCH_SIZE = 1024;

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

            int inputCount = 768;
            int outputCount = 1;

            //Network network = new Network(inputCount, 768, 128, 64, outputCount);
            string jsonString = System.IO.File.ReadAllText(@"C:\Users\Luca Voros\OneDrive\Documents\LICHESS DATABASE\network.json");
            DeserializedNetwork networkD = JsonConvert.DeserializeObject<DeserializedNetwork>(jsonString);

            Network network = new Network(networkD);

            //for (int i = 0; i < totalData.Count; i += 1)
            //{
            //    double[] output = network.Evaluate(totalData[i].inputs);
            //    Console.WriteLine(output[0] + " observed. Desired: " + totalData[i].desiredOutputs[0]);
            //}

            Console.WriteLine($"Initial cost {network.GetCost(totalData)}");

            double recentCost = 100;
            int epoch = 0;

            while (recentCost > 0.01)
            {
                var c_batch = lichessDataBatches[epoch % lichessDataBatches.Count];
                double dynLearn = 0.001;

                network.TrainDerivative(c_batch, dynLearn);
                double new_cost = network.GetCost(c_batch);

                recentCost = new_cost;

                if (epoch % 1 == 0)
                {
                    Console.WriteLine("Finished Epoch " + epoch + " with cost " + recentCost);
                }

                if (epoch % 30 == 0)
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
