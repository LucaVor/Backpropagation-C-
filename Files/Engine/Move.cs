using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessAgain.Engine
{
    public enum MoveFlag
    {
        None,
        Capture,
        KingCastle,
        QueenCastle,
        Promotion,
        PromotionCapture,
        DoublePawnPush,
        EnPassant
    }

    public enum Direction
    {
        None = -1,
        Up,
        Down,
        Right,
        Left,
        UpRight,
        UpLeft,
        DownRight,
        DownLeft
    }

    public class MoveData
    {
        public int castlingRights;
        public int enPassantSquare;
        public int quietMoves;

        public ulong whiteBitboard;
        public ulong blackBitboard;

        public bool IsEqual(MoveData rhs)
        {
            if (this is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return castlingRights == rhs.castlingRights && enPassantSquare == rhs.enPassantSquare && quietMoves == rhs.quietMoves && whiteBitboard == rhs.whiteBitboard && blackBitboard == rhs.blackBitboard;
        }
    }

    public struct Pin
    {
        public int piecePinning;
        public ulong pinLine;
        public bool doublePawnPin;
    }

    public class Move
    {
        public int fromSqr;
        public int toSqr;

        public int promotionPiece;
        public int pieceCaptured;

        public int pieceMoved;

        public MoveFlag moveFlag;

        public static Move CreateMove(int from, int to, MoveFlag flag, int pieceMoved, int _promotionPiece = -1, int _pieceCaptured = -1)
        {
            return new Move() { fromSqr = from, toSqr = to, moveFlag = flag, promotionPiece = _promotionPiece, pieceCaptured = _pieceCaptured, pieceMoved=pieceMoved };
        }

        public static Pin MakePin(int piecePinning, ulong pinLine, bool doublePawnPin = false)
        {
            return new Pin() { piecePinning = piecePinning, pinLine = pinLine, doublePawnPin = doublePawnPin };
        }

        public static Move Copy(Move move)
        {
            return new Move()
            {
                fromSqr = move.fromSqr,
                toSqr = move.toSqr,
                moveFlag = move.moveFlag,
                promotionPiece = move.promotionPiece,
                pieceCaptured = move.pieceCaptured,
                pieceMoved = move.pieceMoved,
            };
        }

        public override string ToString()
        {
            string str = PreProcess.squareNames[fromSqr] + PreProcess.squareNames[toSqr];

            if (moveFlag == MoveFlag.Promotion || moveFlag == MoveFlag.PromotionCapture)
            {
                str += Board.characters[promotionPiece].ToLower();
            }

            return str + " " + moveFlag;
        }
    }
}
