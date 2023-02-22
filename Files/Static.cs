using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessAgain.NeuralNetwork
{
    public static class Static
    {
        public static double e = (double)Math.E;

        public static double Activate(double x)
        {
            return Linear(x);
        }

        public static double Derivative(double x)
        {
            return LinearDeriv(x);
        }

        public static double Custom(double x)
        {
            return (120 / (1 + Math.Pow(e, -0.2 * x))) - 60;
        }

        public static double CustomDeriv(double x)
        {
            return ((24*Math.Pow(e, -0.2 * x))/Math.Pow((1 + Math.Pow(e, -0.2 * x)), 2));
        }

        public static double TanH(double x)
        {
            return Math.Tanh(x);
        }

        public static double TanHDeriv(double x)
        {
            return 1 - Math.Pow(Math.Tanh(x), 2);
        }
        
        public static double Sigmoid(double x)
        {
            return 1 / (1 + pow(e, -x));
        }

        public static double SigmoidDeriv(double x)
        {
            return Sigmoid(x) * (1 - Sigmoid(x));
        }

        public static double Linear(double x)
        {
            return x;
        }

        public static double LinearDeriv(double x)
        {
            return 1;
        }

        public static double Step(double x)
        {
            return x > 0 ? 1 : 0;
        }
        
        public static double pow(double b, double e)
        {
            return (double)Math.Pow(b, e);
        }
        public static string DisplayArray(double[] arr)
        {
            return string.Join(" ", arr);
        }

        public static string DisplayArray(double[,] arr)
        {
            string result = "";
            
            for (int x = 0; x < arr.GetLength(0); x += 1)
            {
                result += "[";

                for (int y = 0; y < arr.GetLength(1); y += 1)
                {
                    result += arr[x, y] + (y == (arr.GetLength(1)-1) ? "] " : " ");
                }
            }

            return result;
        }

        public static void CopyArray(ref List<double> output, double[] arr)
        {
            output.Clear();

            for (int x = 0; x < arr.Length; x += 1)
            {
                output.Add(arr[x]);
            }
        }

        public static double DesiredObservedDerivative(double desired, double observed)
        {
            return 2 * (observed - desired);
        }

        public static double[] ConvertToDoubles(JArray elements)
        {
            double[] output = new double[elements.Count];

            int i = 0;
            foreach (JValue subelement in elements)
            {
                output[i] = double.Parse(subelement.ToString());
                i += 1;
            }

            return output;
        }

        public static double RoundNearestPow2(int x)
        {
            return 1 << ((int)(Math.Log(x) / Math.Log(2)));
        }
    }
}
