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
        public int myIndex;

        public Piece(Board parent, int mySide, int myIndex)
        {
            this.parent = parent;
            this.mySide = mySide;
            this.myIndex = myIndex; 
        }

        public void AddPieceAt(int sqr)
        {
            ulong sqrBitboard = one << sqr;
            rep |= sqrBitboard;

            if (!piecePositions.Contains(sqr))
            {
                piecePositions.Add(sqr);
            }

            parent.sideBitboards[mySide] |= sqrBitboard;
        }

        public void RemovePieceAt(int sqr)
        {
            if (!piecePositions.Contains(sqr))
            {
                throw new Exception($"Square not found in position {PreProcess.squareNames[sqr]}. Piece {myIndex}, Side {mySide}");
            }

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
