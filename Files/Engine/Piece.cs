using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessAgain.Engine
{
    public class Piece
    {
        public UInt64 rep;
        public List<int> piecePositions = new List<int>();
        public static UInt64 one = 1;

        public Board parent;
        public int mySide;

        public Piece(Board parent, int mySide)
        {
            this.parent = parent;
            this.mySide = mySide;
        }

        public void AddPieceAt(int sqr)
        {
            ulong sqrBitboard = one << sqr;
            rep |= sqrBitboard;

            piecePositions.Add(sqr);

            parent.sideBitboards[mySide] |= sqrBitboard;
        }

        public void RemovePieceAt(int sqr)
        {
            piecePositions.Remove(sqr);

            ulong sqrBitboard = one << sqr;
            parent.sideBitboards[mySide] -= sqrBitboard;

            rep -= sqrBitboard;
        }

        public bool PieceAt(int sqr)
        {
            return (rep & (one << sqr)) > 0;
        }
    }
}
