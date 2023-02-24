using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessAgain.Engine
{
    public static class BitOp
    {
        public static string GetBitBoard(UInt64 bit)
        {
            string binary = "";

            while (bit > 0)
            {
                UInt64 rem = bit % 2;
                bit /= 2;

                binary += rem;
            }

            while (binary.Length < 64)
            {
                binary += "0";
            }

            char[] binaryArray = binary.ToCharArray();
            Array.Reverse(binaryArray);
            binary = new string(binaryArray);

            return binary;
        }

        public static string GetBitBoardFormat(UInt64 bit)
        {
            string binary = "";
            int i = 0;

            while (bit > 0)
            {
                UInt64 rem = bit % 2;
                bit /= 2;

                binary += rem;

                if ((i + 1) % 8 == 0)
                {
                    binary += "\n";
                } else
                {
                    binary += " ";
                }

                i += 1;
            }

            while (binary.Length < 64+64)
            {
                binary += "0";

                if ((i + 1) % 8 == 0)
                {
                    binary += "\n";
                }
                else
                {
                    binary += " ";
                }

                i += 1;
            }

            char[] binaryArray = binary.ToCharArray();
            Array.Reverse(binaryArray);
            binary = new string(binaryArray);

            return binary;
        }

        public static int MultipleBits(ulong bits)
        {
            int c = 0;

            while (bits != 0)
            {
                c += 1;

                bits &= bits - 1;
            }

            return c;
        }
    }
}
