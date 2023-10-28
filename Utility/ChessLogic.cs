using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class ChessLogic
{
    public enum PieceType
    {
        None = -1,
        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
        King
    }

    public enum Color
    {
        White,
        Black
    }

    public enum PromoType : byte
    {
        None,
        Knight,
        Bishop,
        Rook,
        Queen
    }

    public enum MoveType : byte
    {
        Invalid,
        Normal,
        Double_push,
        Capture,
        Castle,
        En_passant,
        Promotion
    }

    public struct Move
    {
        public Move(PromoType PromoType, MoveType MoveType, byte FromSquare, byte ToSquare)
        {
            promoType = PromoType;
            moveType = MoveType;
            fromSquare = FromSquare;
            toSquare = ToSquare;
        }

        public PromoType promoType;
        public MoveType moveType = MoveType.Invalid;
        public byte fromSquare;
        public byte toSquare;
    }

    public struct BoardState
    {
        public void Set(ulong p, ulong n, ulong b, ulong r, ulong q, ulong k, ulong white, ulong black, ulong occupied, ushort cr, byte ep, Color stm, ulong hash)
        {
            pawns = p;
            knights = n;
            bishops = b;
            rooks = r;
            queens = q;
            kings = k;
            whitePieces = white;
            blackPieces = black;
            occupiedSquares = occupied;
            castleRights = cr;
            epSquare = ep;
            sideToMove = stm;
            zobristHash = hash;
        }
        public ulong pawns, knights, bishops, rooks, queens, kings, whitePieces, blackPieces, occupiedSquares;
        public ulong zobristHash;

        public ushort castleRights;
        public Color sideToMove;
        public byte epSquare;
    }

    public static ushort whiteKingsideMask = 0b1000;
    public static ushort whiteQueensideMask = 0b0100;
    public static ushort blackKingsideMask = 0b0010;
    public static ushort blackQueensideMask = 0b0001;

    public static string startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public static int INFINITY = 32000;
    public static int INVALID_SQUARE = 99;

    public static int FILE_ONE = 0, FILE_TWO = 1, FILE_THREE = 2, FILE_FOUR = 3, FILE_FIVE = 4, FILE_SIX = 5, FILE_SEVEN = 6, FILE_EIGHT = 7;
    public static int RANK_ONE = 0, RANK_TWO = 1, RANK_THREE = 2, RANK_FOUR = 3, RANK_FIVE = 4, RANK_SIX = 5, RANK_SEVEN = 6, RANK_EIGHT = 7;

    public static ulong FILE_A_MASK = 0x0101010101010101UL;
    public static ulong FILE_B_MASK = 0x0202020202020202UL;
    public static ulong FILE_C_MASK = 0x0404040404040404UL;
    public static ulong FILE_D_MASK = 0x0808080808080808UL;
    public static ulong FILE_E_MASK = 0x1010101010101010UL;
    public static ulong FILE_F_MASK = 0x2020202020202020UL;
    public static ulong FILE_G_MASK = 0x4040404040404040UL;
    public static ulong FILE_H_MASK = 0x8080808080808080UL;

    public static ulong RANK_ONE_MASK = 0x00000000000000FFUL;
    public static ulong RANK_TWO_MASK = 0x000000000000FF00UL;
    public static ulong RANK_THREE_MAS = 0x0000000000FF0000UL;
    public static ulong RANK_FOUR_MASK = 0x00000000FF000000UL;
    public static ulong RANK_FIVE_MASK = 0x000000FF00000000UL;
    public static ulong RANK_SIX_MASK = 0x0000FF0000000000UL;
    public static ulong RANK_SEVEN_MASK = 0x00FF000000000000UL;
    public static ulong RANK_EIGHT_MASK = 0xFF00000000000000UL;

    public static ulong[] NORTH_RAYS = new ulong[64], EAST_RAYS = new ulong[64], SOUTH_RAYS = new ulong[64], WEST_RAYS = new ulong[64];
    public static ulong[] NE_RAYS = new ulong[64], NW_RAYS = new ulong[64], SE_RAYS = new ulong[64], SW_RAYS = new ulong[64];

    public static ulong[] NORTH_MASK = new ulong[64], EAST_MASK = new ulong[64], SOUTH_MASK = new ulong[64], WEST_MASK = new ulong[64];
    public static ulong[] NE_MASK = new ulong[64], NW_MASK = new ulong[64], SE_MASK = new ulong[64], SW_MASK = new ulong[64];
}