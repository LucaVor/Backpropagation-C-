using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ChessAgain.NeuralNetwork;
using ChessAgain.Engine;
using Newtonsoft.Json.Linq;
using System.Linq;

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

        public static string Join<T>(List<T> values)
        {
            string result = "[";

            for (int i = 0; i < values.Count; i++)
            {
                result += values[i].ToString();

                if (i == values.Count - 1)
                {
                    result += "]";
                } else
                {
                    result += ", ";
                }
            }

            return result;
        }

        public static Board board;

        static int Perft(int depth, int distanceFromPly = 0)
        {
            List<Move> moves = board.GenerateLegalMoves(board.whiteMove ? 0 : 1);

            if (depth == 1)
            {
                //Console.WriteLine(Join<string>(parentMoves));
                return moves.Count;
            }

            int total = 0;

            foreach (Move move in moves)
            {
                MoveData moveData = board.MakeMove(move);
                int subCount = Perft(depth - 1, distanceFromPly + 1);
                total += subCount;

                if (distanceFromPly == 0)
                {
                    Console.WriteLine(move.ToString().Split(' ')[0] + " : " + subCount);
                }

                board.UnmakeMove(move, moveData);
            }

            return total;
        }

        static int Search(int depth)
        {
            if (depth == 0)
            {
                return 1;
            }

            List<Move> allLegalMoves = board.GenerateLegalMoves(board.whiteMove ? 0 : 1).ToList();
            int resulting = 0;

            foreach(Move startingMove in allLegalMoves)
            {
                MoveData mData = board.MakeMove(startingMove);
                MoveData copy = JsonConvert.DeserializeObject<MoveData>(JsonConvert.SerializeObject(mData));

                ulong wb = board.sideBitboards[0];
                ulong bb = board.sideBitboards[1];

                //board.DisplayBoard();
                int subCount = Search(depth - 1);
                resulting += subCount;

                if (depth == 3)
                {
                    Console.WriteLine("Ayo?");
                    Console.WriteLine(startingMove.ToString() + " : " + subCount);
                }

                board.sideBitboards = new ulong[] { wb, bb };

                board.UnmakeMove(startingMove, mData);

                //board.DisplayBoard();
            }

            return resulting;
        }

        static void Main(string[] args)
        {
            PreProcess.Init();

            board = new Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            List<Move> allMoves = board.GenerateLegalMoves(board.whiteMove ? 0 : 1);

            //board.whiteMove = true;
            //board.MakeMove(Move.CreateMove(PreProcess.nameToSquare["e1"], PreProcess.nameToSquare["d1"], MoveFlag.None, Board.kingIndex));
            //board.MakeMove(Move.CreateMove(PreProcess.nameToSquare["h3"], PreProcess.nameToSquare["g2"], MoveFlag.Capture, Board.pawnIndex, -1, 0));
            //board.MakeMove(Move.CreateMove(PreProcess.nameToSquare["c3"], PreProcess.nameToSquare["a4"], MoveFlag.None, Board.knightIndex));
            //board.MakeMove(Move.CreateMove(PreProcess.nameToSquare["e8"], PreProcess.nameToSquare["d8"], MoveFlag.None, Board.kingIndex));
            //board.MakeMove(Move.CreateMove(PreProcess.nameToSquare["d5"], PreProcess.nameToSquare["d6"], MoveFlag.None, Board.pawnIndex));

            board.DisplayBoard();

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            int result = Perft(6);

            watch.Stop();

            Console.WriteLine(result + $". Took {watch.ElapsedMilliseconds}ms.");
            //List<Move> sd = board.GenerateLegalMoves(board.whiteMove ? 0 : 1);
            ////List<Move> sd = PreProcess.pawnMoves[1, PreProcess.nameToSquare["g2"]];
            //foreach (Move fg in sd)
            //{
            //    Console.WriteLine(fg.ToString().Split(' ')[0] + " : " + 1);
            //}
            //Console.WriteLine(sd.Count);

            Console.ReadLine();
        }

        static void MainTest(string[] args)
        {
            PreProcess.Init();

            Board board = new Board();

            var watch = new System.Diagnostics.Stopwatch();
            List<Move> moves = board.GenerateLegalMoves(0);
            int x = 0;

            string[] pieceNames = new string[]{
                "p", "k", "b", "r", "q", "k"
            };

            //watch.Start();
            //for (int i = 0; i < 1; i += 1)
            //{
            //    moves = board.GenerateLegalMoves(0);
            //    x += moves.Count;
            //}
            //watch.Stop();

            //Console.WriteLine($"Took {watch.ElapsedMilliseconds}ms. {x}");

            while (true)
            {
                board.DisplayBoard();

                List<Move> legalMoves = board.GenerateLegalMoves(board.whiteMove ? 0 : 1);

                string joinedString = "";

                foreach (Move lMove in legalMoves)
                {
                    joinedString += PreProcess.squareNames[lMove.fromSqr] + PreProcess.squareNames[lMove.toSqr] + ", ";
                }

                Console.WriteLine(joinedString);

                string moveStr = Console.ReadLine();

                string from = (moveStr[0].ToString() + moveStr[1].ToString()).ToString();
                string to = (moveStr[2].ToString() + moveStr[3].ToString()).ToString();

                bool wasValid = (from num in legalMoves select (PreProcess.squareNames[num.fromSqr] + PreProcess.squareNames[num.toSqr])).Contains(moveStr);

                while (!wasValid)
                {
                    Console.WriteLine("Move was invalid.");

                    moveStr = Console.ReadLine();
                    from = (moveStr[0].ToString() + moveStr[1].ToString()).ToString();
                    to = (moveStr[2].ToString() + moveStr[3].ToString()).ToString();

                    wasValid = (from num in legalMoves select (PreProcess.squareNames[num.fromSqr] + PreProcess.squareNames[num.toSqr])).Contains(moveStr);
                }

                int ifrom = PreProcess.nameToSquare[from];
                int ito = PreProcess.nameToSquare[to];

                Move move = Move.CreateMove(ifrom, ito, MoveFlag.None, board.GetPieceFromSquare(ifrom));

                if (board.GetPieceFromSquare(ito) != -1)
                {
                    move.moveFlag = MoveFlag.Capture;
                    move.pieceCaptured = board.GetPieceFromSquare(ito);
                }

                if (move.pieceMoved == Board.pawnIndex && ito == board.enPassantSquare)
                {
                    move.moveFlag = MoveFlag.EnPassant;
                }

                if (move.pieceMoved == Board.kingIndex && (move.fromSqr - move.toSqr) == -2)
                {
                    move.moveFlag = MoveFlag.KingCastle;
                }

                if (move.pieceMoved == Board.kingIndex && (move.fromSqr - move.toSqr) == 2)
                {
                    move.moveFlag = MoveFlag.QueenCastle;
                }

                if (move.pieceMoved == Board.pawnIndex && board.whiteMove && move.toSqr >= 56)
                {
                    move.moveFlag = MoveFlag.Promotion;
                    move.promotionPiece = Board.queenIndex;
                }

                if (move.pieceMoved == Board.pawnIndex && !board.whiteMove && move.toSqr <= 7)
                {
                    move.moveFlag = MoveFlag.Promotion;
                    move.promotionPiece = Board.queenIndex;
                }

                board.MakeMove(move);
            }

            Console.WriteLine(moves.Count);

            board.DisplayBoard();

            Console.ReadKey();
        }

        static void EvaluateMain(string[] args)
        {
            string jsonString = System.IO.File.ReadAllText(@"C:\Users\Luca Voros\OneDrive\Documents\LICHESS DATABASE\network.json");
            DeserializedNetwork networkD = JsonConvert.DeserializeObject<DeserializedNetwork>(jsonString);

            Network network = new Network(networkD);

            double[] inputs = FenToNeural("5k2/3R4/2K1p1p1/4P1P1/5P2/8/3r4/8");

            var watch = new System.Diagnostics.Stopwatch();

            watch.Start();
            double[] output = network.Evaluate(inputs);
            watch.Stop();

            Console.WriteLine(output[0]);
            Console.WriteLine($"Took {watch.ElapsedMilliseconds}ms.");
        }

        static void TrainingMain(string[] args)
        {
            List<List<DataPoint>> lichessDataBatches = new List<List<DataPoint>>();
            List<DataPoint> currentBatch = new List<DataPoint>();
            List<DataPoint> totalData = new List<DataPoint>();

            const int BATCH_SIZE = 100;

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

            for (int i = 0; i < totalData.Count; i += 1)
            {
                double[] output = network.Evaluate(totalData[i].inputs);
                Console.WriteLine(output[0] + " observed. Desired: " + totalData[i].desiredOutputs[0]);
            }

            return;

            //Console.WriteLine($"Initial cost {network.GetCost(totalData)}");

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

                if (epoch % 130 == 0)
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
