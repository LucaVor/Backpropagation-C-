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

    public struct MoveData
    {
        public int castlingRights;
        public int enPassantSquare;
        public int quietMoves;
    }

    public struct Pin
    {
        public int piecePinning;
        public ulong pinLine;
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

        public static Pin MakePin(int piecePinning, ulong pinLine)
        {
            return new Pin() { piecePinning = piecePinning, pinLine = pinLine };
        }
    }
}
