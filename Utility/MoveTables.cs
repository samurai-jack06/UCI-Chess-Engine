using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChessLogic;

public static class MoveTables
{
    public static ulong[] KnightMovesTable = new ulong[64];
    public static ulong[] KingMovesTable = new ulong[64];

    public static ulong[] WhitePawnMovesTable = new ulong[64];
    public static ulong[] BlackPawnMovesTable = new ulong[64];

    public static void CalculateKnightTable()
    {
        for (int square = 0; square < 64; square++)
        {
            ulong moves = 0UL;
            ulong knight = 1UL << square;

            moves |= (knight << 6) & ~FILE_G_MASK & ~FILE_H_MASK;
            moves |= (knight << 10) & ~FILE_A_MASK & ~FILE_B_MASK;
            moves |= (knight << 15) & ~FILE_H_MASK;
            moves |= (knight << 17) & ~FILE_A_MASK;

            moves |= (knight >> 6) & ~FILE_A_MASK & ~FILE_B_MASK;
            moves |= (knight >> 10) & ~FILE_G_MASK & ~FILE_H_MASK;
            moves |= (knight >> 15) & ~FILE_A_MASK;
            moves |= (knight >> 17) & ~FILE_H_MASK;

            KnightMovesTable[square] = moves;
        }
    }

    public static void CalculateKingTable()
    {
        for (int square = 0; square < 64; square++)
        {
            ulong moves = 0UL;
            ulong king = 1UL << square;

            moves |= (king << 1) & ~FILE_A_MASK;
            moves |= (king >> 1) & ~FILE_H_MASK;
            moves |= (king << 8);
            moves |= (king >> 8);
            moves |= (king << 9) & ~FILE_A_MASK;
            moves |= (king >> 9) & ~FILE_H_MASK;
            moves |= (king << 7) & ~FILE_H_MASK;
            moves |= (king >> 7) & ~FILE_A_MASK;

            KingMovesTable[square] = moves;
        }
    }

    public static void CalculatePawnTables()
    {
        for (int square = 0; square < 64; square++)
        {
            ulong whiteAttacks = 0UL;
            ulong blackAttacks = 0UL;
            ulong pawn = 1UL << square;

            whiteAttacks |= (pawn << 7) & ~FILE_H_MASK;
            whiteAttacks |= (pawn << 9) & ~FILE_A_MASK;

            blackAttacks |= (pawn >> 7) & ~FILE_A_MASK;
            blackAttacks |= (pawn >> 9) & ~FILE_H_MASK;

            WhitePawnMovesTable[square] = whiteAttacks;
            BlackPawnMovesTable[square] = blackAttacks;
        }
    }
}