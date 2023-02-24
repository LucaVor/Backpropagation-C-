﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace ChessAgain.Engine
{
    public class Board
    {
        public Piece[,] board;

        public UInt64[] sideBitboards = new UInt64[]
        {
            0, 0
        };

        #region Static Values
        public static int whiteIndex = 0;
        public static int blackIndex = 1;

        public static int pawnIndex = 0;
        public static int knightIndex = 1;
        public static int bishopIndex = 2;
        public static int rookIndex = 3;
        public static int queenIndex = 4;
        public static int kingIndex = 5;

        public static UInt64 one = 1;

        public static string startingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";

        private List<string> numbers =
        new List<string>() {
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
        };

        private string[] characters = new string[]
        {
            "P", "N", "B", "R", "Q", "K"
        };

        #endregion

        public List<Move> legalMoves = new List<Move>();
        public List<Move> psuedoLegal = new List<Move>();
        public List<Move> psudeo = new List<Move>();

        #region Member Variables
        public bool whiteMove = true;
        public int castlingRights = 0b1111;
        public int quietMoveCount = 0;
        public int enPassantSquare = -1;

        public static int whiteCastle = 0b1100;
        public static int blackCastle = 0b0011;
        public static int kingSide =    0b1010;
        public static int queenSide =   0b0101;

        public static int[] castlingMask = new int[]
        {
            whiteCastle, blackCastle
        };

        public static int[] kingRooks = new int[]
        {
            7, 63
        };

        public static int[] queenRooks = new int[]
        {
            0, 56
        };

        public static int[] castleMasks = new int[]
        {
            whiteCastle, blackCastle
        };

        public static UInt64[,] castleSideMasks = new UInt64[,]
        {
            { (one << 5) | (one << 6), (one << 1) | (one << 2) | (one << 3) },
            { (one << 61) | (one << 62), (one << 57) | (one << 58) | (one << 59) }
        };

        #endregion

        public Board()
        {
            board = new Piece[,]{
                {
                    new Piece(this, 0), new Piece(this, 0), new Piece(this, 0), new Piece(this, 0), new Piece(this, 0), new Piece(this, 0)
                }, 
                {
                    new Piece(this, 1), new Piece(this, 1), new Piece(this, 1), new Piece(this, 1), new Piece(this, 1), new Piece(this, 1)
                }
            };

            LoadForsyth(startingFen);
        }

        public Board(string fen)
        {
            LoadForsyth(fen);
        }

        public MoveData MakeMove(Move move)
        {
            MoveData moveData = new MoveData();

            moveData.castlingRights = castlingRights;
            moveData.enPassantSquare = enPassantSquare;
            moveData.quietMoves = quietMoves;

            int sideToMove = whiteMove ? 0 : 1;
            int otherSide = 1 - sideToMove;

            int down = whiteMove ? -8 : 8;

            quietMoveCount += 1;

            board[sideToMove, move.pieceMoved].RemovePieceAt(move.fromSqr);
            board[sideToMove, move.pieceMoved].AddPieceAt(move.toSqr);

            if (move.moveFlag == MoveFlag.Capture)
            {
                quietMoveCount = 0;

                board[otherSide, move.pieceCaptured].RemovePieceAt(move.toSqr);

                if (move.pieceCaptured == rookIndex)
                {
                    if (move.toSqr == kingRooks[otherSide])
                    {
                        castlingRights -= (castlingRights & kingSide & castleMasks[otherSide]);
                    }
                    if (move.toSqr == queenRooks[otherSide])
                    {
                        castlingRights -= (castlingRights & queenSide & castleMasks[otherSide]);
                    }
                }
            }

            if (move.moveFlag == MoveFlag.Promotion)
            {
                board[sideToMove, pawnIndex].RemovePieceAt(move.toSqr);
                board[sideToMove, move.promotionPiece].AddPieceAt(move.toSqr);
            }

            if (move.moveFlag == MoveFlag.EnPassant)
            {
                board[otherSide, pawnIndex].RemovePieceAt(move.toSqr + down);
            }

            if (move.moveFlag == MoveFlag.KingCastle)
            {
                board[sideToMove, rookIndex].RemovePieceAt(kingRooks[sideToMove]);
                board[sideToMove, rookIndex].RemovePieceAt(move.toSqr - 1);
            }

            if (move.moveFlag == MoveFlag.QueenCastle)
            {
                board[sideToMove, rookIndex].RemovePieceAt(queenRooks[sideToMove]);
                board[sideToMove, rookIndex].RemovePieceAt(move.toSqr + 1);
            }

            enPassantSquare = -1;

            if (move.moveFlag == MoveFlag.DoublePawnPush)
            {
                enPassantSquare = move.toSqr + down;
            }

            if (move.pieceMoved == rookIndex)
            {
                if (move.fromSqr == kingRooks[sideToMove])
                {
                    castlingRights -= (castlingRights & kingSide & castlingMask[sideToMove]);
                }

                if (move.fromSqr == queenRooks[sideToMove])
                {
                    castlingRights -= (castlingRights & queenSide & castlingMask[sideToMove]);
                }
            }

            if (move.pieceMoved == kingIndex)
            {
                castlingRights -= (castlingRights & castlingMask[sideToMove]);
            }

            if (move.pieceMoved == pawnIndex)
            {
                quietMoveCount = 0;
            }

            whiteMove = !whiteMove;

            return moveData;
        }

        public void UnmakeMove(Move move, MoveData moveData)
        {

        }

        public int GetPieceFromSquare(int square)
        {
            for (int col = 0; col < 2; col += 1)
            {
                for (int piece = 0; piece < kingIndex + 1; piece += 1)
                {
                    if (board[col, piece].piecePositions.Contains(square))
                    {
                        return piece;
                    }
                }
            }

            return -1;
        }

        public void LoadForsyth(string fen)
        {
            fen = fen.Replace("/", "").Replace("\\", "");

            int sqrIndexNonRotated = 64;

            foreach (char c in fen)
            {
                string l = c.ToString();
                sqrIndexNonRotated -= 1;

                if (numbers.Contains(l))
                {
                    sqrIndexNonRotated -= (int.Parse(l) - 1);
                    continue;
                }

                int sqrIndex = (int)((Math.Floor(((double)sqrIndexNonRotated) / 8) * 8) + (7 - (sqrIndexNonRotated % 8)));

                switch(l)
                {
                    case "p":
                        board[blackIndex,pawnIndex].AddPieceAt(sqrIndex);
                        sideBitboards[blackIndex] |= (one << sqrIndex);
                        break;
                    case "n":
                        board[blackIndex, knightIndex].AddPieceAt(sqrIndex);
                        sideBitboards[blackIndex] |= (one << sqrIndex);
                        break;
                    case "b":
                        board[blackIndex, bishopIndex].AddPieceAt(sqrIndex);
                        sideBitboards[blackIndex] |= (one << sqrIndex);
                        break;
                    case "r":
                        board[blackIndex, rookIndex].AddPieceAt(sqrIndex);
                        sideBitboards[blackIndex] |= (one << sqrIndex);
                        break;
                    case "q":
                        board[blackIndex, queenIndex].AddPieceAt(sqrIndex);
                        sideBitboards[blackIndex] |= (one << sqrIndex);
                        break;
                    case "k":
                        board[blackIndex, kingIndex].AddPieceAt(sqrIndex);
                        sideBitboards[blackIndex] |= (one << sqrIndex);
                        break;
                    case "P":
                        board[whiteIndex, pawnIndex].AddPieceAt(sqrIndex);
                        sideBitboards[whiteIndex] |= (one << sqrIndex);
                        break;
                    case "N":
                        board[whiteIndex, knightIndex].AddPieceAt(sqrIndex);
                        sideBitboards[whiteIndex] |= (one << sqrIndex);
                        break;
                    case "B":
                        board[whiteIndex, bishopIndex].AddPieceAt(sqrIndex);
                        sideBitboards[whiteIndex] |= (one << sqrIndex);
                        break;
                    case "R":
                        board[whiteIndex, rookIndex].AddPieceAt(sqrIndex);
                        sideBitboards[whiteIndex] |= (one << sqrIndex);
                        break;
                    case "Q":
                        board[whiteIndex, queenIndex].AddPieceAt(sqrIndex);
                        sideBitboards[whiteIndex] |= (one << sqrIndex);
                        break;
                    case "K":
                        board[whiteIndex, kingIndex].AddPieceAt(sqrIndex);
                        sideBitboards[whiteIndex] |= (one << sqrIndex);
                        break;
                }
            }
        }

        public string DisplayBoard()
        {
            string boardResult = "0000000000000000000000000000000000000000000000000000000000000000";

            for (int colour = 0; colour < 2; colour += 1)
            {
                for (int i = 0; i < kingIndex + 1; i += 1)
                {
                    string character = characters[i];

                    if (colour == 1)
                    {
                        character = character.ToLower();
                    }

                    string bitBoard = BitOp.GetBitBoard(board[colour, i].rep);

                    bitBoard = bitBoard.Replace("1", character);
                    int iter = 0;

                    foreach (char c in bitBoard)
                    {
                        if (boardResult[iter] == '0' && c != '0')
                        {
                            char[] boardArray = boardResult.ToCharArray();

                            boardArray[iter] = c;

                            boardResult = new string(boardArray);
                        }

                        iter += 1;
                    }
                }
            }

            string final = "";

            for (int j = 7; j <= 63; j += 8)
            {
                for (int k = j; k >= (j-7); k -= 1)
                {
                    final += boardResult[k].ToString();
                }
            }

            boardResult = final;

            final = "|________________________________\n| ";

            int ep = 0;
            foreach (char c in boardResult) {
                final += c;

                if ((ep + 1) % 8 == 0)
                {
                    final += " | \n|_______________________________|\n| ";
                } else
                {
                    final += " | ";
                }

                ep += 1;
            }

            final = final.Substring(1, final.Length - 3);

            boardResult = final.Replace("0", " ");

            Console.WriteLine(boardResult);

            return boardResult;
        }

        public List<Move> GenerateLegalMoves(int sideToMove)
        {
            bool foundKingMoves = false;

            int checkerCount = 0;

            List<Pin> pins = new List<Pin>();

            ulong pinnedPieces = 0ul;
            ulong checkFilter = 0ul;

            UInt64 attackedSquares = GetAttackedBitboard(1 - sideToMove, board[sideToMove, kingIndex].piecePositions[0], ref checkerCount, ref pins, ref pinnedPieces, ref checkFilter);
            UInt64 kingSquares = board[sideToMove, kingIndex].rep;

            psudeo = GenerateMoves(sideToMove, ref foundKingMoves, checkerCount >= 2, pins, pinnedPieces, checkFilter, attackedSquares);
            legalMoves.Clear();

            bool inCheck = (kingSquares & attackedSquares) > 0;

            foreach (Move move in psudeo)
            {
                if (((one << move.fromSqr) & kingSquares) > 0)
                {
                    if (((one << move.toSqr) & attackedSquares) == 0)
                    {
                        legalMoves.Add(move);
                    }
                } else
                {       
                    legalMoves.Add(move);
                }
            }

            return legalMoves;
        }
    
        public List<Move> GenerateMoves(int sideToMove, ref bool foundKingMoves, bool skipAfterKing, List<Pin> pins, ulong pinnedPieces, ulong checkFilter, ulong attMask)
        {
            psuedoLegal.Clear();

            int otherSide = 1 - sideToMove;

            int upOffset2 = sideToMove == 0 ? 16 : -16;
            UInt64 combined = sideBitboards[0] | sideBitboards[1];

            int sideRights = castlingRights & castleMasks[sideToMove];
            bool canCastle = sideRights != 0;

            foreach (int kingSquare in board[sideToMove, kingIndex].piecePositions)
            {
                List<Move> theoreticalMoves = PreProcess.kings[sideToMove, kingSquare];

                foreach (Move move in theoreticalMoves)
                {
                    if (move.moveFlag == MoveFlag.KingCastle)
                    {
                        if (!canCastle) { continue; }

                        if (move.toSqr > move.fromSqr)
                        {
                            if ((sideRights & kingSide) > 0)
                            {
                                UInt64 blockingMask = castleSideMasks[sideToMove, 0] & combined;

                                if (blockingMask == 0 && ((attMask & castleSideMasks[sideToMove, 0]) == 0))
                                {
                                    psuedoLegal.Add(move);
                                }
                            }
                        }
                    }
                    else if (move.moveFlag == MoveFlag.QueenCastle)
                    {
                        if (!canCastle) { continue; }

                        if ((sideRights & queenSide) > 0)
                        {
                            UInt64 blockingMask = castleSideMasks[sideToMove, 1] & combined;

                            if (blockingMask == 0 && ((attMask & castleSideMasks[sideToMove, 1]) == 0))
                            {
                                psuedoLegal.Add(move);
                            }
                        }
                    }
                    else
                    {
                        bool friendAtTo = (sideBitboards[sideToMove] & (one << move.toSqr)) > 0;

                        if (friendAtTo) { continue; }

                        bool enemyAtTo = (sideBitboards[otherSide] & (one << move.toSqr)) > 0;

                        if (enemyAtTo)
                        {
                            move.moveFlag = MoveFlag.Capture;

                            for (int piece = 0; piece < kingIndex + 1; piece += 1)
                            {
                                if (board[otherSide, piece].PieceAt(move.toSqr))
                                {
                                    move.pieceCaptured = piece;
                                    psuedoLegal.Add(move);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            psuedoLegal.Add(move);
                        }
                    }
                }
            }

            foundKingMoves = psuedoLegal.Count > 0;

            if (skipAfterKing)
                return psuedoLegal;

            foreach (int pawnSquare in board[sideToMove, pawnIndex].piecePositions)
            {
                ulong moveFilter = 0;
                bool useFilter = false;

                if (((1ul << pawnSquare) & pinnedPieces) > 0)
                {
                    foreach (Pin pin in pins)
                    {
                        if (((1ul << pawnSquare) & pin.pinLine) > 0)
                        {
                            moveFilter = pin.pinLine;
                            useFilter = true;
                            break;
                        }
                    }
                }

                if (checkFilter != 0)
                {
                    if (useFilter)
                        moveFilter &= checkFilter;
                    else
                        moveFilter |= checkFilter;

                    useFilter = true;
                }

                List<Move> theoretical = PreProcess.pawnMoves[sideToMove, pawnSquare];

                foreach(Move move in theoretical)
                {
                    bool enemyAtTo = (sideBitboards[otherSide] & (one << move.toSqr)) > 0;
                    bool anyAtTo = (combined & (one << move.toSqr)) > 0;

                    if (useFilter)
                    {
                        if (((1ul << move.toSqr) & moveFilter) == 0)
                        {
                            continue;
                        }
                    }

                    if (move.moveFlag == MoveFlag.Capture && enemyAtTo)
                    {
                        for (int piece = 0; piece < kingIndex + 1; piece += 1)
                        {
                            if (board[otherSide, piece].PieceAt(move.toSqr))
                            {
                                move.pieceCaptured = piece;
                                psuedoLegal.Add(move);
                                break;

                            }
                        }
                    }

                    if ((move.moveFlag == MoveFlag.None || move.moveFlag == MoveFlag.DoublePawnPush) && !anyAtTo)
                    {
                        if (move.toSqr - move.fromSqr == upOffset2)
                        {
                            int middleSqr = (move.toSqr + move.fromSqr) / 2;

                            if ((combined & (one << middleSqr)) == 0)
                            {
                                psuedoLegal.Add(move);
                            }
                        }
                        else
                        {
                            psuedoLegal.Add(move);
                        }
                    }

                    if (move.moveFlag == MoveFlag.EnPassant)
                    {
                        if (move.toSqr == enPassantSquare)
                        {
                            psuedoLegal.Add(move);
                        }
                    }
                }
            }

            foreach(int knightSquare in board[sideToMove, knightIndex].piecePositions)
            {
                ulong moveFilter = 0;
                bool useFilter = false;

                if (((1ul << knightSquare) & pinnedPieces) > 0)
                {
                    foreach (Pin pin in pins)
                    {
                        if (((1ul << knightSquare) & pin.pinLine) > 0)
                        {
                            moveFilter = pin.pinLine;
                            useFilter = true;
                            break;
                        }
                    }
                }

                if (checkFilter != 0)
                {
                    if (useFilter)
                        moveFilter &= checkFilter;
                    else
                        moveFilter |= checkFilter;

                    useFilter = true;
                }

                List<Move> theoreticalMoves = PreProcess.knights[knightSquare];

                foreach (Move move in theoreticalMoves)
                {
                    bool friendAtTo = (sideBitboards[sideToMove] & (one << move.toSqr)) > 0;

                    if (friendAtTo)
                    {
                        continue;
                    }

                    if (useFilter)
                    {
                        if (((1ul << move.toSqr) & moveFilter) == 0)
                        {
                            continue;
                        }
                    }

                    bool enemyAtTo = (sideBitboards[otherSide] & (one << move.toSqr)) > 0;

                    if(enemyAtTo)
                    {
                        move.moveFlag = MoveFlag.Capture;

                        for (int piece = 0; piece < kingIndex + 1; piece += 1)
                        {
                            if (board[otherSide, piece].PieceAt(move.toSqr))
                            {
                                move.pieceCaptured = piece;
                                psuedoLegal.Add(move);
                                break;
                            }
                        }
                    } else
                    {
                        psuedoLegal.Add(move);
                    }                    
                }
            }

            foreach (int bishopSquare in board[sideToMove, bishopIndex].piecePositions)
            {
                ulong moveFilter = 0;
                bool useFilter = false;

                if (((1ul << bishopSquare) & pinnedPieces) > 0)
                {
                    foreach (Pin pin in pins)
                    {
                        if (((1ul << bishopSquare) & pin.pinLine) > 0)
                        {
                            moveFilter = pin.pinLine;
                            useFilter = true;
                            break;
                        }
                    }
                }

                if (useFilter)
                {
                    if (useFilter)
                        moveFilter &= checkFilter;
                    else
                        moveFilter |= checkFilter;

                    useFilter = true;
                }

                for (int i = 0; i < 4; i += 1)
                {
                    List<Move> theoreticalMoves = PreProcess.bishops[bishopSquare,i];

                    foreach (Move move in theoreticalMoves)
                    {
                        bool friendAtTo = (sideBitboards[sideToMove] & (one << move.toSqr)) > 0;

                        if (friendAtTo)
                        {
                            break;
                        }

                        if (moveFilter != 0)
                        {
                            if (((1ul << move.toSqr) & moveFilter) == 0)
                            {
                                continue;
                            }
                        }

                        bool enemyAtTo = (sideBitboards[otherSide] & (one << move.toSqr)) > 0;

                        if (enemyAtTo)
                        {
                            move.moveFlag = MoveFlag.Capture;

                            for (int piece = 0; piece < kingIndex + 1; piece += 1)
                            {
                                if (board[otherSide, piece].PieceAt(move.toSqr))
                                {
                                    move.pieceCaptured = piece;
                                    psuedoLegal.Add(move);
                                    break;
                                }
                            }

                            break;
                        }

                        psuedoLegal.Add(move);
                    }
                }
            }

            foreach (int rookSquare in board[sideToMove, rookIndex].piecePositions)
            {
                ulong moveFilter = 0;
                bool useFilter = false;

                if (((1ul << rookSquare) & pinnedPieces) > 0)
                {
                    foreach (Pin pin in pins)
                    {
                        if (((1ul << rookSquare) & pin.pinLine) > 0)
                        {
                            moveFilter = pin.pinLine;
                            useFilter = true;
                            break;
                        }
                    }
                }

                if (checkFilter != 0)
                {
                    if (useFilter)
                        moveFilter &= checkFilter;
                    else
                        moveFilter |= checkFilter;

                    useFilter = true;
                }

                for (int i = 0; i < 4; i += 1)
                {
                    List<Move> theoreticalMoves = PreProcess.rooks[rookSquare, i];

                    foreach (Move move in theoreticalMoves)
                    {
                        bool friendAtTo = (sideBitboards[sideToMove] & (one << move.toSqr)) > 0;

                        if (friendAtTo)
                        {
                            break;
                        }

                        if (useFilter)
                        {
                            if (((1ul << move.toSqr) & moveFilter) == 0)
                            {
                                continue;
                            }
                        }

                        bool enemyAtTo = (sideBitboards[otherSide] & (one << move.toSqr)) > 0;

                        if (enemyAtTo)
                        {
                            move.moveFlag = MoveFlag.Capture;

                            for (int piece = 0; piece < kingIndex + 1; piece += 1)
                            {
                                if (board[otherSide, piece].PieceAt(move.toSqr))
                                {
                                    move.pieceCaptured = piece;
                                    psuedoLegal.Add(move);
                                    break;
                                }
                            }

                            break;
                        }

                        psuedoLegal.Add(move);
                    }
                }
            }

            foreach (int queenSquare in board[sideToMove, queenIndex].piecePositions)
            {
                ulong moveFilter = 0;
                bool useFilter = false;

                if (((1ul << queenSquare) & pinnedPieces) > 0)
                {
                    foreach (Pin pin in pins)
                    {
                        if (((1ul << queenSquare) & pin.pinLine) > 0)
                        {
                            moveFilter = pin.pinLine;
                            useFilter = true;
                            break;
                        }
                    }
                }

                if (checkFilter != 0)
                {
                    if (useFilter)
                        moveFilter &= checkFilter;
                    else
                        moveFilter |= checkFilter;
                }

                for (int i = 0; i < 8; i += 1)
                {
                    List<Move> theoreticalMoves = PreProcess.queens[queenSquare, i];

                    foreach (Move move in theoreticalMoves)
                    {
                        bool friendAtTo = (sideBitboards[sideToMove] & (one << move.toSqr)) > 0;

                        if (friendAtTo)
                        {
                            break;
                        }

                        if (useFilter)
                        {
                            if (((1ul << move.toSqr) & moveFilter) == 0)
                            {
                                continue;
                            }
                        }

                        bool enemyAtTo = (sideBitboards[otherSide] & (one << move.toSqr)) > 0;

                        if (enemyAtTo)
                        {
                            move.moveFlag = MoveFlag.Capture;

                            for (int piece = 0; piece < kingIndex + 1; piece += 1)
                            {
                                if (board[otherSide, piece].PieceAt(move.toSqr))
                                {
                                    move.pieceCaptured = piece;
                                    psuedoLegal.Add(move);
                                    break;
                                }
                            }

                            break;
                        }

                        psuedoLegal.Add(move);
                    }
                }
            }

            return psuedoLegal;
        }

        public UInt64 GetAttackedBitboard(int sideToMove, int ekingSquare, ref int checkerCount, ref List<Pin> pins, ref ulong pinnedPieces, ref ulong checkFilter)
        {
            UInt64 allMoves = 0;
            UInt64 combined = sideBitboards[0] | sideBitboards[1];

            int otherSide = 1 - sideToMove;

            foreach (int pawnSquare in board[sideToMove, pawnIndex].piecePositions)
            {
                List<Move> theoretical = PreProcess.pawnAttacksExclusive[sideToMove, pawnSquare];

                foreach (Move move in theoretical)
                {
                    if (move.toSqr == ekingSquare) { checkerCount++; }
                    allMoves |= (one << move.toSqr);
                }
            }

            foreach (int knightSquare in board[sideToMove, knightIndex].piecePositions)
            {
                List<Move> theoreticalMoves = PreProcess.knights[knightSquare];

                foreach (Move move in theoreticalMoves)
                {
                    if (move.toSqr == ekingSquare) { checkerCount++; }
                    allMoves |= (one << move.toSqr);
                }
            }


            foreach (int kingSquare in board[sideToMove, kingIndex].piecePositions)
            {
                List<Move> theoreticalMoves = PreProcess.kings[sideToMove, kingSquare];

                foreach (Move move in theoreticalMoves)
                {
                    if (move.toSqr == ekingSquare) { checkerCount++; }
                    allMoves |= (one << move.toSqr);
                }
            }

            foreach (int bishopSquare in board[sideToMove, bishopIndex].piecePositions)
            {
                ulong betweenBits = PreProcess.inBetweenBits[bishopSquare, ekingSquare];

                if (betweenBits != 0)
                {
                    int bitCount = BitOp.MultipleBits(betweenBits & combined);

                    if (bitCount == 1)
                    {
                        checkFilter |= betweenBits;
                    } else if (bitCount == 2)
                    {
                        pinnedPieces |= betweenBits;
                        pins.Add(Move.MakePin(bishopSquare, betweenBits));
                    }
                }

                for (int i = 0; i < 4; i += 1)
                {
                    List<Move> theoreticalMoves = PreProcess.bishops[bishopSquare, i];

                    foreach (Move move in theoreticalMoves)
                    {
                        bool friendAtTo = (sideBitboards[sideToMove] & (one << move.toSqr)) > 0;

                        if (friendAtTo)
                        {
                            break;
                        }

                        if (move.toSqr == ekingSquare) { checkerCount++; }

                        bool enemyAtTo = (sideBitboards[otherSide] & (one << move.toSqr)) > 0;

                        if (enemyAtTo && (move.toSqr != ekingSquare))
                        {
                            allMoves |= (one << move.toSqr);
                            break;
                        }

                        allMoves |= (one << move.toSqr);
                    }
                }
            }

            foreach (int rookSquare in board[sideToMove, rookIndex].piecePositions)
            {
                ulong betweenBits = PreProcess.inBetweenBits[rookSquare, ekingSquare];

                if (betweenBits != 0)
                {
                    int bitCount = BitOp.MultipleBits(betweenBits & combined);

                    if (bitCount == 1)
                    {
                        checkFilter |= betweenBits;
                    }
                    else if (bitCount == 2)
                    {
                        pinnedPieces |= betweenBits;
                        pins.Add(Move.MakePin(rookSquare, betweenBits));
                    }
                }

                for (int i = 0; i < 4; i += 1)
                {
                    List<Move> theoreticalMoves = PreProcess.rooks[rookSquare, i];

                    foreach (Move move in theoreticalMoves)
                    {
                        bool friendAtTo = (sideBitboards[sideToMove] & (one << move.toSqr)) > 0;

                        if (friendAtTo)
                        {
                            break;
                        }

                        if (move.toSqr == ekingSquare) { checkerCount++; }

                        bool enemyAtTo = (sideBitboards[otherSide] & (one << move.toSqr)) > 0;

                        if (enemyAtTo && (move.toSqr != ekingSquare))
                        {
                            allMoves |= (one << move.toSqr);
                            break;
                        }

                        allMoves |= (one << move.toSqr);
                    }
                }
            }

            foreach (int queenSquare in board[sideToMove, queenIndex].piecePositions)
            {
                ulong betweenBits = PreProcess.inBetweenBits[queenSquare, ekingSquare];

                if (betweenBits != 0)
                {
                    int bitCount = BitOp.MultipleBits(betweenBits & combined);

                    if (bitCount == 1)
                    {
                        checkFilter |= betweenBits;
                    }
                    else if (bitCount == 2)
                    {
                        pinnedPieces |= betweenBits;
                        pins.Add(Move.MakePin(queenSquare, betweenBits));
                    }
                }

                for (int i = 0; i < 8; i += 1)
                {
                    List<Move> theoreticalMoves = PreProcess.queens[queenSquare, i];

                    foreach (Move move in theoreticalMoves)
                    {
                        bool friendAtTo = (sideBitboards[sideToMove] & (one << move.toSqr)) > 0;

                        if (friendAtTo)
                        {
                            break;
                        }

                        if (move.toSqr == ekingSquare) { checkerCount++; }

                        bool enemyAtTo = (sideBitboards[otherSide] & (one << move.toSqr)) > 0;

                        if (enemyAtTo && (move.toSqr != ekingSquare))
                        {
                            allMoves |= (one << move.toSqr);
                            break;
                        }

                        allMoves |= (one << move.toSqr);
                    }
                }
            }

            return allMoves;
        }
    }
}