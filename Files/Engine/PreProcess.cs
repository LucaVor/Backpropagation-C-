﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessAgain.Engine
{
    public static class PreProcess
    {
        public static List<Move>[,] pawnMoves = new List<Move>[2,64];
        public static List<Move>[,] pawnAttacksExclusive = new List<Move>[2,64];
        public static List<Move>[] knights = new List<Move>[64];
        public static List<Move>[,] kings = new List<Move>[2,64];

        public static List<Move>[,] bishops = new List<Move>[64,4];
        public static List<Move>[,] rooks = new List<Move>[64,4];
        public static List<Move>[,] queens = new List<Move>[64,8];
        public static UInt64[,] inBetweenBits = new ulong[64, 64];
        
        public static string[] squareNames = new string[64];
        public static Dictionary<string, int> nameToSquare = new Dictionary<string, int>();
        public static UInt64 MAX_VALUE = 0;

        public static int[,] dirOffsets = new int[,]{
            { 0, 1 },
            { 0,-1 },
            { 1, 0 },
            {-1, 0 },
            { 1, 1 },
            {-1, 1 },
            { 1,-1 },
            {-1,-1 }
        };

        public static int[,] edgeDistances = new int[64,8];

        public static void Init()
        {
            for (int i = 0; i < 64; i += 1)
            {
                MAX_VALUE |= (1UL << i);
            }

            string[] fileNames = new string[]
            {
                "a", "b", "c", "d", "e", "f", "g", "h"
            };

            for (int a = 0; a < 8; a += 1)
            {
                for (int b = 0; b < 8; b += 1)
                {
                    string letter = fileNames[b];
                    string squareName = letter + (a + 1).ToString();

                    squareNames[a * 8 + b] = squareName;

                    nameToSquare.Add(squareName, a * 8 + b);
                }
            }

            for (int sqr = 0; sqr < 64; sqr++)
            {
                bool secondRank = sqr > 7 && sqr < 16;
                bool seventhRank = sqr > 47 && sqr < 56;
                bool fourthRank = sqr > 23 && sqr < 32;
                bool fifthRank = sqr > 31 && sqr < 40;
                bool firstRank = sqr < 8;
                bool eighthRank = sqr > 55;

                bool hFile = (sqr + 1) % 8 == 0;
                bool aFile = sqr % 8 == 0;

                List<Move> whitePawn = new List<Move>();
                List<Move> blackPawn = new List<Move>();
                List<Move> whitePawnAtt = new List<Move>();
                List<Move> blackPawnAtt = new List<Move>();

                if (fourthRank)
                {
                    if (!aFile)
                    {
                        blackPawn.Add(Move.CreateMove(sqr, sqr - 7, MoveFlag.EnPassant, Board.pawnIndex, 0, Board.pawnIndex));
                    } if (!hFile)
                    {
                        blackPawn.Add(Move.CreateMove(sqr, sqr - 9, MoveFlag.EnPassant, Board.pawnIndex, 0, Board.pawnIndex));
                    }
                }

                if (fifthRank)
                {
                    if (!aFile)
                    {
                        whitePawn.Add(Move.CreateMove(sqr, sqr + 9, MoveFlag.EnPassant, Board.pawnIndex, 0, Board.pawnIndex));
                    }
                    if (!hFile)
                    {
                        whitePawn.Add(Move.CreateMove(sqr, sqr + 7, MoveFlag.EnPassant, Board.pawnIndex, 0, Board.pawnIndex));
                    }
                }

                if (!eighthRank)
                {
                    whitePawn.Add(Move.CreateMove(sqr, sqr + 8, MoveFlag.None, Board.pawnIndex));
                }

                if (!firstRank)
                {
                    blackPawn.Add(Move.CreateMove(sqr, sqr - 8, MoveFlag.None, Board.pawnIndex));
                }

                if (secondRank)
                {
                    whitePawn.Add(Move.CreateMove(sqr, sqr + 16, MoveFlag.DoublePawnPush, Board.pawnIndex));

                    for (int piece = 0; piece < Board.kingIndex + 1; piece += 1)
                    {
                        blackPawn.Add(Move.CreateMove(sqr, sqr - 8, MoveFlag.Promotion, Board.pawnIndex, piece));
                    }
                }

                if (seventhRank)
                {
                    blackPawn.Add(Move.CreateMove(sqr, sqr - 16, MoveFlag.DoublePawnPush, Board.pawnIndex));

                    for (int piece = 0; piece < Board.kingIndex + 1; piece += 1)
                    {
                        whitePawn.Add(Move.CreateMove(sqr, sqr + 8, MoveFlag.Promotion, Board.pawnIndex, piece));
                    }
                }

                if (!hFile)
                {
                    whitePawn.Add(Move.CreateMove(sqr, sqr + 7, MoveFlag.Capture, Board.pawnIndex));
                    blackPawn.Add(Move.CreateMove(sqr, sqr - 9, MoveFlag.Capture, Board.pawnIndex));

                    whitePawnAtt.Add(Move.CreateMove(sqr, sqr + 7, MoveFlag.Capture, Board.pawnIndex));
                    blackPawnAtt.Add(Move.CreateMove(sqr, sqr - 9, MoveFlag.Capture, Board.pawnIndex));
                }

                if (!aFile)
                {
                    whitePawn.Add(Move.CreateMove(sqr, sqr + 9, MoveFlag.Capture, Board.pawnIndex));
                    blackPawn.Add(Move.CreateMove(sqr, sqr - 7, MoveFlag.Capture, Board.pawnIndex));

                    whitePawnAtt.Add(Move.CreateMove(sqr, sqr + 9, MoveFlag.Capture, Board.pawnIndex));
                    blackPawnAtt.Add(Move.CreateMove(sqr, sqr - 7, MoveFlag.Capture, Board.pawnIndex));
                }

                pawnMoves[0, sqr] = whitePawn;
                pawnMoves[1, sqr] = blackPawn;

                pawnAttacksExclusive[0, sqr] = whitePawnAtt;
                pawnAttacksExclusive[1, sqr] = blackPawnAtt;

                List<Move> knight = new List<Move>();

                for (int x = -2; x <= 2; x += 1)
                {
                    for (int y = -2; y <= 2; y += 1)
                    {
                        if ((Math.Abs(x) == 1 && Math.Abs(y) == 2) || (Math.Abs(y) == 1 && Math.Abs(x) == 2))
                        {
                            int xOffset = x;
                            int yOffset = y * 8;

                            int targetSqr = sqr + xOffset + yOffset;

                            if (targetSqr > 63 || targetSqr < 0)
                            {
                                continue;
                            }

                            if (x < 0)
                            {
                                if ((targetSqr % 8) > (sqr % 8))
                                {
                                    continue;
                                }
                            }
                            if (x > 0)
                            {
                                if ((targetSqr % 8) < (sqr % 8))
                                {
                                    continue;
                                }
                            }

                            knight.Add(Move.CreateMove(sqr, targetSqr, MoveFlag.None, Board.knightIndex));
                        }
                    }
                }

                knights[sqr] = knight;

                List<Move> whiteKing = new List<Move>();
                List<Move> blackKing = new List<Move>();

                for (int x = -1; x <= 1; x += 1)
                {
                    for (int y = -1; y <= 1; y += 1) { 
                        if (x == 0 && y == 0) { continue; }

                        int yOffset = y * 8;

                        int targetSqr = sqr + x + yOffset;

                        if (targetSqr > 63 || targetSqr < 0)
                        {
                            continue;
                        }

                        if (x < 0)
                        {
                            if ((targetSqr % 8) > (sqr % 8))
                            {
                                continue;
                            }
                        }
                        if (x > 0)
                        {
                            if ((targetSqr % 8) < (sqr % 8))
                            {
                                continue;
                            }
                        }

                        whiteKing.Add(Move.CreateMove(sqr, targetSqr, MoveFlag.None, Board.kingIndex));
                        blackKing.Add(Move.CreateMove(sqr, targetSqr, MoveFlag.None, Board.kingIndex));
                    }
                }

                if (sqr == 4)
                {
                    whiteKing.Add(Move.CreateMove(sqr, 6, MoveFlag.KingCastle, Board.kingIndex));
                    whiteKing.Add(Move.CreateMove(sqr, 2, MoveFlag.QueenCastle, Board.kingIndex));
                }
                if (sqr == 60)
                {
                    blackKing.Add(Move.CreateMove(sqr, 62, MoveFlag.KingCastle, Board.kingIndex));
                    blackKing.Add(Move.CreateMove(sqr, 58, MoveFlag.QueenCastle, Board.kingIndex));
                }

                kings[0, sqr] = whiteKing;
                kings[1, sqr] = blackKing;

                for (int i = 0; i < 8; i += 1)
                {
                    int x = dirOffsets[i, 0];
                    int y = dirOffsets[i, 1];

                    int currSquare = sqr + x + y * 8;

                    List<Move> queen = new List<Move>();
                    List<Move> bishop = new List<Move>();
                    List<Move> rook = new List<Move>();

                    while (true)
                    {
                        if (currSquare < 0 || currSquare > 63)
                        {
                            break;
                        }

                        if (x < 0)
                        {
                            if ((currSquare % 8) > (sqr % 8))
                            {
                                break;
                            }
                        }
                        if (x > 0)
                        {
                            if ((currSquare % 8) < (sqr % 8))
                            {
                                break;
                            }
                        }

                        queen.Add(Move.CreateMove(sqr, currSquare, MoveFlag.None, Board.queenIndex));

                        if ((Math.Abs(x) == 1 && Math.Abs(y) == 0) || (Math.Abs(x) == 0 && Math.Abs(y) == 1))
                        {
                            rook.Add(Move.CreateMove(sqr, currSquare, MoveFlag.None, Board.rookIndex));
                        }

                        if ((Math.Abs(x) == 1 && Math.Abs(y) == 1) || (Math.Abs(x) == 1 && Math.Abs(y) == 1))
                        {
                            bishop.Add(Move.CreateMove(sqr, currSquare, MoveFlag.None, Board.bishopIndex));
                        }

                        currSquare += (x + y * 8);
                    }

                    queens[sqr,i] = queen;

                    if (i < 4)
                    {
                        rooks[sqr, i] = rook;
                    }
                    if (i > 3)
                    {
                        bishops[sqr, i - 4] = bishop;
                    }
                }

                for (int i = 0; i < 8; i += 1)
                {
                    int x = dirOffsets[i, 0];
                    int y = dirOffsets[i, 1];

                    int currSquare = sqr + x + y * 8;
                    int distance = 0;
                    ulong between = (1ul << sqr);

                    while (true)
                    {
                        if (currSquare < 0 || currSquare > 63)
                        {
                            break;
                        }

                        if (x < 0)
                        {
                            if ((currSquare % 8) > (sqr % 8))
                            {
                                break;
                            }
                        }
                        if (x > 0)
                        {
                            if ((currSquare % 8) < (sqr % 8))
                            {
                                break;
                            }
                        }

                        distance += 1;

                        inBetweenBits[sqr, currSquare] = between;

                        between |= (1ul << currSquare);
                        currSquare += (x + y * 8);
                    }

                    edgeDistances[sqr, i] = distance;
                }
            }
        }
    }
}