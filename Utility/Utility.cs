using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using static ChessLogic;
using static Magics;
using static MoveTables;
using static Zobrist;

public static class Utility
{
    public static void CalculateAll()
    {
        CalculateRays();
        GenRandomNumbers();
        CalculateKingTable();
        CalculateKnightTable();
        CalculatePawnTables();
        PopulateMagicTables();
    }

    public static string MoveToString(Move move)
    {
        string Move = "";

        byte from = move.fromSquare;
        byte to = move.toSquare;

        Move += SquareToCoord(from);
        Move += SquareToCoord(to);

        if (move.moveType == MoveType.Promotion)
        {
            if (move.promoType == PromoType.Knight) Move += "n";
            else Move += move.promoType.ToString().ToLower().First();
        }

        return Move;
    }

    public static string SquareToCoord(byte square)
    {
        string coord = "";

        int file = square % 8;
        int rank = (square / 8) + 1;

        coord += (char)(file + 97);
        coord += rank;

        return coord;
    }

    public static byte CoordToSquare(string coord)
    {
        byte square = 0;

        char file = coord[0];
        char rank = coord[1];

        switch (file)
        {
            case 'a': square += 0; break;
            case 'b': square += 1; break;
            case 'c': square += 2; break;
            case 'd': square += 3; break;
            case 'e': square += 4; break;
            case 'f': square += 5; break;
            case 'g': square += 6; break;
            case 'h': square += 7; break;
        }
        switch (rank)
        {
            case '1': square += 0; break;
            case '2': square += 8; break;
            case '3': square += 16; break;
            case '4': square += 24; break;
            case '5': square += 32; break;
            case '6': square += 40; break;
            case '7': square += 48; break;
            case '8': square += 56; break;
        }

        return square;
    }

    public static Move StringToMove(Board board, string move)
    {
        byte from;
        byte to;

        MoveType moveType = MoveType.Normal;
        PromoType promoType = PromoType.None;

        string fromString = move.Substring(0, 2);
        string toString = move.Substring(2, 2);

        from = CoordToSquare(fromString);
        to = CoordToSquare(toString);

        if (move.Length == 5)
        {
            switch (char.ToLower(move[4]))
            {
                case 'n': promoType = PromoType.Knight; break;
                case 'b': promoType = PromoType.Bishop; break;
                case 'r': promoType = PromoType.Rook; break;
                case 'q': promoType = PromoType.Queen; break;
            }
        }

        if ((board.occupiedSquares & (1UL << to)) != 0) moveType = MoveType.Capture;
        if ((board.pawns & (1UL << from)) != 0)
        {
            if ((board.epSquare == to) && ((to % 8) != (from % 8))) moveType = MoveType.En_passant;
            if ((Math.Abs((to / 8) - (from / 8)) == 2)) moveType = MoveType.Double_push;
        }
        if ((board.kings & (1UL << from)) != 0)
        {
            if (Math.Abs((from % 8) - (to % 8)) == 2) moveType = MoveType.Castle;
        }
        if (move.Length == 5) moveType = MoveType.Promotion;

        return new Move(promoType, moveType, from, to);
    }

    public static byte PopLSB(ref ulong bitboard)
    {
        byte lsb = (byte)BitOperations.TrailingZeroCount(bitboard);
        bitboard &= bitboard - 1;
        return lsb;
    }

    public static bool OnPromoRank(Color color, byte square)
    {
        return color == Color.White ? ((square / 8) == RANK_SEVEN) : ((square / 8) == RANK_TWO);
    }

    public static bool OnStartRank(Color color, byte square)
    {
        return color == Color.White ? ((square / 8) == RANK_TWO) : ((square / 8) == RANK_SEVEN);
    }

    public static void CalculateRays()
    {
        NORTH_RAYS = CalculateRay(new Vector2(0, 1), false);
        EAST_RAYS = CalculateRay(new Vector2(1, 0), false);
        SOUTH_RAYS = CalculateRay(new Vector2(0, -1), false);
        WEST_RAYS = CalculateRay(new Vector2(-1, 0), false);
        NE_RAYS = CalculateRay(new Vector2(1, 1), false);
        NW_RAYS = CalculateRay(new Vector2(-1, 1), false);
        SE_RAYS = CalculateRay(new Vector2(1, -1), false);
        SW_RAYS = CalculateRay(new Vector2(-1, -1), false);

        NORTH_MASK = CalculateRay(new Vector2(0, 1), true);
        EAST_MASK = CalculateRay(new Vector2(1, 0), true);
        SOUTH_MASK = CalculateRay(new Vector2(0, -1), true);
        WEST_MASK = CalculateRay(new Vector2(-1, 0), true);
        NE_MASK = CalculateRay(new Vector2(1, 1), true);
        NW_MASK = CalculateRay(new Vector2(-1, 1), true);
        SE_MASK = CalculateRay(new Vector2(1, -1), true);
        SW_MASK = CalculateRay(new Vector2(-1, -1), true);
    }

    public static ulong[] CalculateRay(Vector2 dir, bool mask)
    {
        ulong[] rays = new ulong[64];

        int max = mask ? 7 : 8;
        int min = mask ? 1 : 0;

        for (int square = 0; square < 64; square++)
        {
            ulong ray = 0UL;

            int fromF = square % 8;
            int fromR = square / 8;

            if (dir == new Vector2(0, 1))
            {
                for (int toR = fromR + 1; toR < max; toR++) ray |= 1UL << (fromF + toR * 8);
            }
            else if (dir == new Vector2(0, -1))
            {
                for (int toR = fromR - 1; toR >= min; toR--) ray |= 1UL << (fromF + toR * 8);
            }
            else if (dir == new Vector2(1, 0))
            {
                for (int toF = fromF + 1; toF < max; toF++) ray |= 1UL << (toF + fromR * 8);
            }
            else if (dir == new Vector2(-1, 0))
            {
                for (int toF = fromF - 1; toF >= min; toF--) ray |= 1UL << (toF + fromR * 8);
            }
            else if (dir == new Vector2(1, 1))
            {
                for (int toF = fromF + 1, toR = fromR + 1; toF < max && toR < max; toF++, toR++) ray |= 1UL << (toF + toR * 8);
            }
            else if (dir == new Vector2(1, -1))
            {
                for (int toF = fromF + 1, toR = fromR - 1; toF < max && toR >= min; toF++, toR--) ray |= 1UL << (toF + toR * 8);
            }
            else if (dir == new Vector2(-1, 1))
            {
                for (int toF = fromF - 1, toR = fromR + 1; toF >= min && toR < max; toF--, toR++) ray |= 1UL << (toF + toR * 8);
            }
            else if (dir == new Vector2(-1, -1))
            {
                for (int toF = fromF - 1, toR = fromR - 1; toF >= min && toR >= min; toF--, toR--) ray |= 1UL << (toF + toR * 8);
            }

            rays[square] = ray;
        }

        return rays;
    }


}