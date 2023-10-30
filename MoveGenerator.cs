using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static ChessLogic;
using static Magics;
using static MoveTables;
using static Utility;

public static class MoveGenerator
{
    public static void GenPawnMoves(Board board, byte fromSquare, ref Span<Move> moves, ref int moveIndex, bool capturesOnly)
    {
        Color color = (((1UL << fromSquare) & board.whitePieces) != 0) ? Color.White : Color.Black;

        ulong enemyPieces = color == Color.White ? board.blackPieces : board.whitePieces;

        byte toSquare = (byte)(color == Color.White ? (fromSquare + 8) : (fromSquare - 8));
        byte doublePushSquare = (byte)(color == Color.White ? (toSquare + 8) : (toSquare - 8));

        if (!capturesOnly)
        {
            if (OnPromoRank(color, fromSquare))
            {
                if ((board.occupiedSquares & (1UL << toSquare)) == 0)
                {
                    moves[moveIndex++] = new Move(PromoType.Knight, MoveType.Promotion, fromSquare, toSquare);
                    moves[moveIndex++] = new Move(PromoType.Bishop, MoveType.Promotion, fromSquare, toSquare);
                    moves[moveIndex++] = new Move(PromoType.Rook, MoveType.Promotion, fromSquare, toSquare);
                    moves[moveIndex++] = new Move(PromoType.Queen, MoveType.Promotion, fromSquare, toSquare);
                }
            }
            else
            {
                if (OnStartRank(color, fromSquare))
                {
                    if (((board.occupiedSquares & (1UL << toSquare)) == 0) && ((board.occupiedSquares & (1UL << doublePushSquare)) == 0))
                    {
                        moves[moveIndex++] = new Move(PromoType.None, MoveType.Double_push, fromSquare, doublePushSquare);
                    }
                }

                if ((board.occupiedSquares & (1UL << toSquare)) == 0) moves[moveIndex++] = new Move(PromoType.None, MoveType.Normal, fromSquare, toSquare);
            }
        }


        ulong attackBoard = color == Color.White ? WhitePawnMovesTable[fromSquare] : BlackPawnMovesTable[fromSquare];
        attackBoard &= enemyPieces;

        while (attackBoard != 0)
        {
            toSquare = PopLSB(ref attackBoard);

            if (OnPromoRank(color, fromSquare))
            {
                moves[moveIndex++] = new Move(PromoType.Knight, MoveType.Promotion, fromSquare, toSquare);
                moves[moveIndex++] = new Move(PromoType.Bishop, MoveType.Promotion, fromSquare, toSquare);
                moves[moveIndex++] = new Move(PromoType.Rook, MoveType.Promotion, fromSquare, toSquare);
                moves[moveIndex++] = new Move(PromoType.Queen, MoveType.Promotion, fromSquare, toSquare);
            }
            else moves[moveIndex++] = new Move(PromoType.None, MoveType.Capture, fromSquare, toSquare);
        }

        if (board.epSquare != INVALID_SQUARE)
        {
            ulong epAttackBoard = color == Color.White ? WhitePawnMovesTable[fromSquare] : BlackPawnMovesTable[fromSquare];

            if ((epAttackBoard & (1UL << board.epSquare)) != 0) moves[moveIndex++] = new Move(PromoType.None, MoveType.En_passant, fromSquare, board.epSquare);
        }
    }

    public static void GenKnightMoves(Board board, byte fromSquare, ref Span<Move> moves, ref int moveIndex, bool capturesOnly)
    {
        Color color = (((1UL << fromSquare) & board.whitePieces) != 0) ? Color.White : Color.Black;

        ulong friendlyPieces = color == Color.White ? board.whitePieces : board.blackPieces;
        ulong movesBoard = KnightMovesTable[fromSquare] & ~friendlyPieces;

        while (movesBoard != 0)
        {
            byte toSquare = PopLSB(ref movesBoard);

            if ((board.occupiedSquares & (1UL << toSquare)) != 0) moves[moveIndex++] = new Move(PromoType.None, MoveType.Capture, fromSquare, toSquare);
            else
            {
                if (!capturesOnly) moves[moveIndex++] = new Move(PromoType.None, MoveType.Normal, fromSquare, toSquare);
            }
        }
    }

    public static void GenSlidingMoves(Board board, byte fromSquare, ref Span<Move> moves, ref int moveIndex, bool isRook, bool capturesOnly)
    {
        Color color = (((1UL << fromSquare) & board.whitePieces) != 0) ? Color.White : Color.Black;

        ulong friendlyPieces = color == Color.White ? board.whitePieces : board.blackPieces;
        ulong movesBoard = GetMagicBitboard(fromSquare, board.occupiedSquares, isRook) & ~friendlyPieces;

        while (movesBoard != 0)
        {
            byte toSquare = PopLSB(ref movesBoard);

            if ((board.occupiedSquares & (1UL << toSquare)) != 0) moves[moveIndex++] = new Move(PromoType.None, MoveType.Capture, fromSquare, toSquare);
            else
            {
                if (!capturesOnly) moves[moveIndex++] = new Move(PromoType.None, MoveType.Normal, fromSquare, toSquare);
            }
        }
    }

    public static void GenKingMoves(Board board, byte fromSquare, ref Span<Move> moves, ref int moveIndex, bool capturesOnly)
    {
        Color color = (((1UL << fromSquare) & board.whitePieces) != 0) ? Color.White : Color.Black;

        ulong friendlyPieces = color == Color.White ? board.whitePieces : board.blackPieces;
        ulong enemyKing = color == Color.White ? (board.blackPieces & board.kings) : (board.whitePieces & board.kings);
        ulong movesBoard = KingMovesTable[fromSquare] & ~friendlyPieces & ~enemyKing;

        while (movesBoard != 0)
        {
            byte toSquare = PopLSB(ref movesBoard);

            if ((board.occupiedSquares & (1UL << toSquare)) != 0) moves[moveIndex++] = new Move(PromoType.None, MoveType.Capture, fromSquare, toSquare);
            else
            {
                if (!capturesOnly) moves[moveIndex++] = new Move(PromoType.None, MoveType.Normal, fromSquare, toSquare);
            }
        }

        if (!capturesOnly)
        {
            GenKingsideCastle(board, color, fromSquare, ref moves, ref moveIndex);
            GenQueensideCastle(board, color, fromSquare, ref moves, ref moveIndex);
        }
    }

    public static void GenKingsideCastle(Board board, Color color, byte fromSquare, ref Span<Move> moves, ref int moveIndex)
    {
        ulong friendlyPieces = color == Color.White ? board.whitePieces : board.blackPieces;
        ushort castleRights = board.castleRights;
        byte rookSquare = (byte)(color == Color.White ? 7 : 63);
        ushort mask = color == Color.White ? whiteKingsideMask : blackKingsideMask;

        if ((mask & castleRights) == 0) return;
        if ((friendlyPieces & board.rooks & (1UL << rookSquare)) == 0) return;
        if (board.IsKingInCheck() || !board.KingsideClear(color)) return;
        if (!board.IsLegal(new Move(PromoType.None, MoveType.Normal, fromSquare, (byte)(fromSquare + 1)))) return;

        moves[moveIndex++] = new Move(PromoType.None, MoveType.Castle, fromSquare, (byte)(fromSquare + 2));
    }

    public static void GenQueensideCastle(Board board, Color color, byte fromSquare, ref Span<Move> moves, ref int moveIndex)
    {
        ulong friendlyPieces = color == Color.White ? board.whitePieces : board.blackPieces;
        ushort castleRights = board.castleRights;
        byte rookSquare = (byte)(color == Color.White ? 0 : 56);
        ushort mask = color == Color.White ? whiteQueensideMask : blackQueensideMask;

        if ((mask & castleRights) == 0) return;
        if ((friendlyPieces & board.rooks & (1UL << rookSquare)) == 0) return;
        if (board.IsKingInCheck() || !board.QueensideClear(color)) return;
        if (!board.IsLegal(new Move(PromoType.None, MoveType.Normal, fromSquare, (byte)(fromSquare - 1)))) return;

        moves[moveIndex++] = new Move(PromoType.None, MoveType.Castle, fromSquare, (byte)(fromSquare - 2));
    }
}