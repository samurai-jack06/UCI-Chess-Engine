using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Zobrist
{
    public static Random random = new(0xCAFE);

    public static ulong[,] WhitePieceNumbers = new ulong[64, 6];
    public static ulong[,] BlackPieceNumbers = new ulong[64, 6];

    public static ulong[] EpNumbers = new ulong[64];
    public static ulong[,] CastlingNumbers = new ulong[2, 2];

    public static ulong SideToMoveNumber;

    public static void GenRandomNumbers()
    {
        for (int square = 0; square < 64; square++)
        {
            EpNumbers[square] = (ulong)(random.NextInt64() & 0xFFFFFFFF | random.NextInt64() << 32);

            for (int piece = 0; piece < 6; piece++)
            {
                WhitePieceNumbers[square, piece] = (ulong)(random.NextInt64() & 0xFFFFFFFF | random.NextInt64() << 32);
                BlackPieceNumbers[square, piece] = (ulong)(random.NextInt64() & 0xFFFFFFFF | random.NextInt64() << 32);
            }
        }

        for (int color = 0; color < 2; color++)
        {
            for (int side = 0; side < 2; side++)
            {
                CastlingNumbers[color, side] = (ulong)(random.NextInt64() & 0xFFFFFFFF | random.NextInt64() << 32);
            }
        }

        SideToMoveNumber = (ulong)(random.NextInt64() & 0xFFFFFFFF | random.NextInt64() << 32);
    }
}