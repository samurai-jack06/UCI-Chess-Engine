using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChessLogic;
using static Utility;
using static MoveGenerator;
using static MoveTables;
using static Magics;
using static Zobrist;
using System.Numerics;

public class Board
{
    public ulong pawns, knights, bishops, rooks, queens, kings, whitePieces, blackPieces, occupiedSquares, zobristHash;
    public int halfMoveClock, fullMoveCounter;
    public ushort castleRights;
    public byte epSquare;

    public Color sideToMove;

    public BoardState previousState;
    public Stack<BoardState> boardStates = new();

    public Board(string fen)
    {
        Reset();
        ParseFen(fen);
    }

    public void GenMoves(ref Span<Move> moves, bool capturesOnly)
    {
        int moveIndex = 0;
        ulong friendlyPieces = sideToMove == Color.White ? whitePieces : blackPieces;

        while (friendlyPieces != 0)
        {
            byte square = PopLSB(ref friendlyPieces);
            ulong squareBoard = 1UL << square;

            if ((pawns & squareBoard) != 0) GenPawnMoves(this, square, ref moves, ref moveIndex, capturesOnly);
            else if ((knights & squareBoard) != 0) GenKnightMoves(this, square, ref moves, ref moveIndex, capturesOnly);
            else if ((bishops & squareBoard) != 0) GenSlidingMoves(this, square, ref moves, ref moveIndex, isRook: false, capturesOnly);
            else if ((rooks & squareBoard) != 0) GenSlidingMoves(this, square, ref moves, ref moveIndex, isRook: true, capturesOnly);
            else if ((queens & squareBoard) != 0)
            {
                GenSlidingMoves(this, square, ref moves, ref moveIndex, isRook: false, capturesOnly);
                GenSlidingMoves(this, square, ref moves, ref moveIndex, isRook: true, capturesOnly);
            }
            else if ((kings & squareBoard) != 0) GenKingMoves(this, square, ref moves, ref moveIndex, capturesOnly);
        }

        moves = moves.Slice(0, moveIndex);
    }

    public void CapturePiece(PieceType pieceType, ulong square)
    {
        switch (pieceType)
        {
            case PieceType.Pawn: pawns &= ~square; break;
            case PieceType.Knight: knights &= ~square; break;
            case PieceType.Bishop: bishops &= ~square; break;
            case PieceType.Rook: rooks &= ~square; break;
            case PieceType.Queen: queens &= ~square; break;
            case PieceType.King: kings &= ~square; break;
        }
    }

    public bool MakeMove(Move move)
    {
        previousState.Set(pawns, knights, bishops, rooks, queens, kings, whitePieces, blackPieces, occupiedSquares, castleRights, epSquare, sideToMove, zobristHash);
        boardStates.Push(previousState);

        PieceType movingPiece = PieceType.None;
        PieceType promotedPiece = PieceType.None;
        PieceType captured = PieceType.None;

        ulong friendlyPieces = sideToMove == Color.White ? ref whitePieces : ref blackPieces;
        ulong enemyPieces = sideToMove == Color.White ? ref blackPieces : ref whitePieces;

        ulong from = 1UL << move.fromSquare;
        ulong to = 1UL << move.toSquare;

        if ((pawns & from) != 0) movingPiece = PieceType.Pawn;
        else if ((knights & from) != 0) movingPiece = PieceType.Knight;
        else if ((bishops & from) != 0) movingPiece = PieceType.Bishop;
        else if ((rooks & from) != 0) movingPiece = PieceType.Rook;
        else if ((queens & from) != 0) movingPiece = PieceType.Queen;
        else if ((kings & from) != 0) movingPiece = PieceType.King;

        if ((pawns & to) != 0) captured = PieceType.Pawn;
        else if ((knights & to) != 0) captured = PieceType.Knight;
        else if ((bishops & to) != 0) captured = PieceType.Bishop;
        else if ((rooks & to) != 0) captured = PieceType.Rook;
        else if ((queens & to) != 0) captured = PieceType.Queen;
        else if ((kings & to) != 0) captured = PieceType.King;

        if (move.promoType == PromoType.Knight) promotedPiece = PieceType.Knight;
        else if (move.promoType == PromoType.Bishop) promotedPiece = PieceType.Bishop;
        else if (move.promoType == PromoType.Rook) promotedPiece = PieceType.Rook;
        else if (move.promoType == PromoType.Queen) promotedPiece = PieceType.Queen;

        if (sideToMove == Color.White) whitePieces &= ~from;
        if (sideToMove == Color.Black) blackPieces &= ~from;
        occupiedSquares &= ~from;

        if (movingPiece == PieceType.Pawn)
        {
            pawns &= ~from;

            if (move.moveType == MoveType.Normal)
            {
                pawns |= to;
            }
            if (move.moveType == MoveType.Double_push)
            {
                epSquare = (byte)(sideToMove == Color.White ? (move.toSquare - 8) : (move.toSquare + 8));
                pawns |= to;
            }
            if (move.moveType == MoveType.Capture)
            {
                if (sideToMove == Color.White) blackPieces &= ~to;
                if (sideToMove == Color.Black) whitePieces &= ~to;
                occupiedSquares &= ~to;
                CapturePiece(captured, to);
                pawns |= to;
            }
            if (move.moveType == MoveType.En_passant)
            {
                ulong pawnSquare = sideToMove == Color.White ? (1UL << (epSquare - 8)) : (1UL << (epSquare + 8));
                if (sideToMove == Color.White) blackPieces &= ~pawnSquare;
                if (sideToMove == Color.Black) whitePieces &= ~pawnSquare;
                occupiedSquares &= ~pawnSquare;
                pawns &= ~pawnSquare;
                pawns |= to;
            }
            if (move.moveType == MoveType.Promotion)
            {
                if (sideToMove == Color.White) blackPieces &= ~to;
                if (sideToMove == Color.Black) whitePieces &= ~to;
                if (captured != PieceType.None)
                {
                    CapturePiece(captured, to);
                    occupiedSquares &= ~to;
                }

                switch (move.promoType)
                {
                    case PromoType.Knight: knights |= to; break;
                    case PromoType.Bishop: bishops |= to; break;
                    case PromoType.Rook: rooks |= to; break;
                    case PromoType.Queen: queens |= to; break;
                }
            }
        }
        if (movingPiece == PieceType.Knight)
        {
            knights &= ~from;

            if (move.moveType == MoveType.Normal)
            {
                knights |= to;
            }
            if (move.moveType == MoveType.Capture)
            {
                if (sideToMove == Color.White) blackPieces &= ~to;
                if (sideToMove == Color.Black) whitePieces &= ~to;
                occupiedSquares &= ~to;
                CapturePiece(captured, to);
                knights |= to;
            }
        }
        if (movingPiece == PieceType.Bishop)
        {
            bishops &= ~from;

            if (move.moveType == MoveType.Normal)
            {
                bishops |= to;
            }
            if (move.moveType == MoveType.Capture)
            {
                if (sideToMove == Color.White) blackPieces &= ~to;
                if (sideToMove == Color.Black) whitePieces &= ~to;
                occupiedSquares &= ~to;
                CapturePiece(captured, to);
                bishops |= to;
            }
        }
        if (movingPiece == PieceType.Rook)
        {
            rooks &= ~from;

            if (move.moveType == MoveType.Normal)
            {
                rooks |= to;
            }
            if (move.moveType == MoveType.Capture)
            {
                if (sideToMove == Color.White) blackPieces &= ~to;
                if (sideToMove == Color.Black) whitePieces &= ~to;
                occupiedSquares &= ~to;
                CapturePiece(captured, to);
                rooks |= to;
            }

        }
        if (movingPiece == PieceType.Queen)
        {
            queens &= ~from;

            if (move.moveType == MoveType.Normal)
            {
                queens |= to;
            }
            if (move.moveType == MoveType.Capture)
            {
                if (sideToMove == Color.White) blackPieces &= ~to;
                if (sideToMove == Color.Black) whitePieces &= ~to;
                occupiedSquares &= ~to;
                CapturePiece(captured, to);
                queens |= to;
            }
        }
        if (movingPiece == PieceType.King)
        {
            kings &= ~from;

            if (move.moveType == MoveType.Normal)
            {
                kings |= to;
            }
            if (move.moveType == MoveType.Capture)
            {
                if (sideToMove == Color.White) blackPieces &= ~to;
                if (sideToMove == Color.Black) whitePieces &= ~to;
                occupiedSquares &= ~to;
                CapturePiece(captured, to);
                kings |= to;
            }
            if (move.moveType == MoveType.Castle)
            {
                if ((move.toSquare - move.fromSquare) == 2)
                {
                    rooks &= ~(1UL << (move.toSquare + 1));
                    rooks |= (1UL << (move.toSquare - 1));

                    if (sideToMove == Color.White)
                    {
                        whitePieces &= ~(1UL << (move.toSquare + 1));
                        whitePieces |= (1UL << (move.toSquare - 1));
                    }
                    if (sideToMove == Color.Black)
                    {
                        blackPieces &= ~(1UL << (move.toSquare + 1));
                        blackPieces |= (1UL << (move.toSquare - 1));
                    }

                    occupiedSquares &= ~(1UL << (move.toSquare + 1));
                    occupiedSquares |= (1UL << (move.toSquare - 1));
                }
                if ((move.fromSquare - move.toSquare) == 2)
                {
                    rooks &= ~(1UL << (move.toSquare - 2));
                    rooks |= (1UL << (move.toSquare + 1));

                    if (sideToMove == Color.White)
                    {
                        whitePieces &= ~(1UL << (move.toSquare - 2));
                        whitePieces |= (1UL << (move.toSquare + 1));
                    }
                    if (sideToMove == Color.Black)
                    {
                        blackPieces &= ~(1UL << (move.toSquare - 2));
                        blackPieces |= (1UL << (move.toSquare + 1));
                    }

                    occupiedSquares &= ~(1UL << (move.toSquare - 2));
                    occupiedSquares |= (1UL << (move.toSquare + 1));
                }

                kings |= to;
            }
        }

        if (sideToMove == Color.White) whitePieces |= to;
        if (sideToMove == Color.Black) blackPieces |= to;
        occupiedSquares |= to;

        if (!IsKingInCheck())
        {
            if (movingPiece == PieceType.Pawn || move.moveType == MoveType.Capture) halfMoveClock = 0;
            else halfMoveClock++;


            if (move.moveType != MoveType.Double_push) epSquare = (byte)INVALID_SQUARE;

            if (previousState.epSquare != (byte)INVALID_SQUARE) zobristHash ^= EpNumbers[previousState.epSquare];

            if (epSquare != (byte)INVALID_SQUARE) zobristHash ^= EpNumbers[epSquare];

            if (sideToMove == Color.White) zobristHash ^= WhitePieceNumbers[move.fromSquare, (int)movingPiece];
            if (sideToMove == Color.Black) zobristHash ^= BlackPieceNumbers[move.fromSquare, (int)movingPiece];

            if ((move.moveType != MoveType.Promotion) && (move.moveType != MoveType.Capture) && (move.moveType != MoveType.Castle) && (move.moveType != MoveType.En_passant))
            {
                if (sideToMove == Color.White) zobristHash ^= WhitePieceNumbers[move.toSquare, (int)movingPiece];
                if (sideToMove == Color.Black) zobristHash ^= BlackPieceNumbers[move.toSquare, (int)movingPiece];
            }

            if (move.moveType == MoveType.En_passant)
            {
                if (sideToMove == Color.White)
                {
                    zobristHash ^= WhitePieceNumbers[move.toSquare, 0];
                    zobristHash ^= BlackPieceNumbers[move.toSquare - 8, 0];
                }
                if (sideToMove == Color.Black)
                {
                    zobristHash ^= BlackPieceNumbers[move.toSquare, 0];
                    zobristHash ^= WhitePieceNumbers[move.toSquare + 8, 0];
                }
            }

            if (captured == PieceType.Rook)
            {
                if (move.toSquare == 7)
                {
                    if ((castleRights & whiteKingsideMask) != 0) zobristHash ^= CastlingNumbers[0, 0];
                    castleRights &= (ushort)~whiteKingsideMask;
                }
                if (move.toSquare == 0)
                {
                    if ((castleRights & whiteQueensideMask) != 0) zobristHash ^= CastlingNumbers[0, 1];
                    castleRights &= (ushort)~whiteQueensideMask;
                }
                if (move.toSquare == 63)
                {
                    if ((castleRights & blackKingsideMask) != 0) zobristHash ^= CastlingNumbers[1, 0];
                    castleRights &= (ushort)~blackKingsideMask;
                }
                if (move.toSquare == 56)
                {
                    if ((castleRights & blackQueensideMask) != 0) zobristHash ^= CastlingNumbers[1, 1];
                    castleRights &= (ushort)~blackQueensideMask;
                }
            }

            if (movingPiece == PieceType.Rook)
            {
                if (move.fromSquare == 7)
                {
                    if ((castleRights & whiteKingsideMask) != 0) zobristHash ^= CastlingNumbers[0, 0];
                    castleRights &= (ushort)~whiteKingsideMask;
                }
                if (move.fromSquare == 0)
                {
                    if ((castleRights & whiteQueensideMask) != 0) zobristHash ^= CastlingNumbers[0, 1];
                    castleRights &= (ushort)~whiteQueensideMask;
                }
                if (move.fromSquare == 63)
                {
                    if ((castleRights & blackKingsideMask) != 0) zobristHash ^= CastlingNumbers[1, 0];
                    castleRights &= (ushort)~blackKingsideMask;
                }
                if (move.fromSquare == 56)
                {
                    if ((castleRights & blackQueensideMask) != 0) zobristHash ^= CastlingNumbers[1, 1];
                    castleRights &= (ushort)~blackQueensideMask;
                }
            }

            if (movingPiece == PieceType.King)
            {
                if (move.moveType == MoveType.Castle)
                {
                    if (move.toSquare == 6)
                    {
                        zobristHash ^= WhitePieceNumbers[7, (int)PieceType.Rook];
                        zobristHash ^= WhitePieceNumbers[5, (int)PieceType.Rook];
                        zobristHash ^= WhitePieceNumbers[move.toSquare, (int)PieceType.King];
                    }
                    if (move.toSquare == 2)
                    {
                        zobristHash ^= WhitePieceNumbers[0, (int)PieceType.Rook];
                        zobristHash ^= WhitePieceNumbers[3, (int)PieceType.Rook];
                        zobristHash ^= WhitePieceNumbers[move.toSquare, (int)PieceType.King];
                    }
                    if (move.toSquare == 62)
                    {
                        zobristHash ^= BlackPieceNumbers[63, (int)PieceType.Rook];
                        zobristHash ^= BlackPieceNumbers[61, (int)PieceType.Rook];
                        zobristHash ^= BlackPieceNumbers[move.toSquare, (int)PieceType.King];
                    }
                    if (move.toSquare == 58)
                    {
                        zobristHash ^= BlackPieceNumbers[56, (int)PieceType.Rook];
                        zobristHash ^= BlackPieceNumbers[59, (int)PieceType.Rook];
                        zobristHash ^= BlackPieceNumbers[move.toSquare, (int)PieceType.King];
                    }
                }
                if (sideToMove == Color.White)
                {
                    if ((castleRights & whiteKingsideMask) != 0) zobristHash ^= CastlingNumbers[0, 0];
                    if ((castleRights & whiteQueensideMask) != 0) zobristHash ^= CastlingNumbers[0, 1];
                    castleRights &= (ushort)~whiteKingsideMask;
                    castleRights &= (ushort)~whiteQueensideMask;
                }
                if (sideToMove == Color.Black)
                {
                    if ((castleRights & blackKingsideMask) != 0) zobristHash ^= CastlingNumbers[1, 0];
                    if ((castleRights & blackQueensideMask) != 0) zobristHash ^= CastlingNumbers[1, 1];
                    castleRights &= (ushort)~blackKingsideMask;
                    castleRights &= (ushort)~blackQueensideMask;
                }

            }

            if (move.moveType == MoveType.Capture)
            {
                if (sideToMove == Color.White)
                {
                    zobristHash ^= WhitePieceNumbers[move.toSquare, (int)movingPiece];
                    zobristHash ^= BlackPieceNumbers[move.toSquare, (int)captured];
                }
                if (sideToMove == Color.Black)
                {
                    zobristHash ^= BlackPieceNumbers[move.toSquare, (int)movingPiece];
                    zobristHash ^= WhitePieceNumbers[move.toSquare, (int)captured];
                }
            }

            if (move.moveType == MoveType.Promotion)
            {
                if (sideToMove == Color.White)
                {
                    zobristHash ^= WhitePieceNumbers[move.toSquare, (int)promotedPiece];
                    if (captured != PieceType.None) zobristHash ^= BlackPieceNumbers[move.toSquare, (int)captured];
                }
                if (sideToMove == Color.Black)
                {
                    zobristHash ^= BlackPieceNumbers[move.toSquare, (int)promotedPiece];
                    if (captured != PieceType.None) zobristHash ^= WhitePieceNumbers[move.toSquare, (int)captured];
                }
            }

            sideToMove = sideToMove == Color.White ? Color.Black : Color.White;
            if (sideToMove == Color.Black) fullMoveCounter++;

            zobristHash ^= SideToMoveNumber;

            return true;
        }
        else return false;
    }

    public void UnmakeMove()
    {
        BoardState prevState = boardStates.Pop();

        pawns = prevState.pawns;
        knights = prevState.knights;
        bishops = prevState.bishops;
        rooks = prevState.rooks;
        queens = prevState.queens;
        kings = prevState.kings;
        whitePieces = prevState.whitePieces;
        blackPieces = prevState.blackPieces;
        occupiedSquares = prevState.occupiedSquares;
        epSquare = prevState.epSquare;
        castleRights = prevState.castleRights;
        sideToMove = prevState.sideToMove;
        zobristHash = prevState.zobristHash;
    }

    public bool IsLegal(Move move)
    {
        MakeMove(move);
        bool check = IsKingInCheck();
        UnmakeMove();

        return !check;
    }

    public bool IsKingInCheck()
    {
        ulong friendlyPieces = sideToMove == Color.White ? whitePieces : blackPieces;
        ulong enemyPieces = sideToMove == Color.White ? blackPieces : whitePieces;

        ulong friendlyKing = friendlyPieces & kings;

        ulong enemyPawns = enemyPieces & pawns;
        ulong enemyKnights = enemyPieces & knights;
        ulong enemyBishops = enemyPieces & bishops;
        ulong enemyRooks = enemyPieces & rooks;
        ulong enemyQueens = enemyPieces & queens;
        ulong enemyKing = enemyPieces & kings;

        byte fromSquare = (byte)BitOperations.TrailingZeroCount(friendlyKing);

        ulong knightMoves = KnightMovesTable[fromSquare];
        ulong kingMoves = KingMovesTable[fromSquare];
        ulong pawnMoves = sideToMove == Color.White ? WhitePawnMovesTable[fromSquare] : BlackPawnMovesTable[fromSquare];
        ulong bishopMoves = GetMagicBitboard(fromSquare, occupiedSquares, false);
        ulong rookMoves = GetMagicBitboard(fromSquare, occupiedSquares, true);

        if ((enemyPawns & pawnMoves) != 0) return true;
        if ((enemyKnights & knightMoves) != 0) return true;
        if ((enemyBishops & bishopMoves) != 0) return true;
        if ((enemyRooks & rookMoves) != 0) return true;
        if ((enemyQueens & bishopMoves) != 0) return true;
        if ((enemyQueens & rookMoves) != 0) return true;
        if ((enemyKing & kingMoves) != 0) return true;

        return false;
    }

    public bool KingsideClear(Color color)
    {
        ulong rank = color == Color.White ? RANK_ONE_MASK : RANK_EIGHT_MASK;
        return ((rank & FILE_F_MASK & occupiedSquares) == 0) && ((rank & FILE_G_MASK & occupiedSquares) == 0);
    }
    public bool QueensideClear(Color color)
    {
        ulong rank = color == Color.White ? RANK_ONE_MASK : RANK_EIGHT_MASK;
        return ((rank & FILE_B_MASK & occupiedSquares) == 0) && ((rank & FILE_C_MASK & occupiedSquares) == 0) && ((rank & FILE_D_MASK & occupiedSquares) == 0);
    }

    public void GenerateZobristHash()
    {
        ulong hash = 0;

        ulong[] white = new ulong[6]
        {
            whitePieces & pawns, whitePieces & knights, whitePieces & bishops, whitePieces & rooks, whitePieces & queens, whitePieces & kings
        };
        ulong[] black = new ulong[6]
        {
            blackPieces & pawns, blackPieces & knights, blackPieces & bishops, blackPieces & rooks, blackPieces & queens, blackPieces & kings
        };

        for (int i = 0; i < 6; i++)
        {
            ulong whitePiece = white[i];
            ulong blackPiece = black[i];

            while (whitePiece != 0)
            {
                byte square = PopLSB(ref whitePiece);
                hash ^= WhitePieceNumbers[square, i];
            }
            while (blackPiece != 0)
            {
                byte square = PopLSB(ref blackPiece);
                hash ^= BlackPieceNumbers[square, i];
            }
        }

        if (epSquare != INVALID_SQUARE) hash ^= EpNumbers[epSquare];
        if (sideToMove == Color.White) hash ^= SideToMoveNumber;

        if ((castleRights & whiteKingsideMask) != 0) hash ^= CastlingNumbers[0, 0];
        if ((castleRights & whiteQueensideMask) != 0) hash ^= CastlingNumbers[0, 1];
        if ((castleRights & blackKingsideMask) != 0) hash ^= CastlingNumbers[1, 0];
        if ((castleRights & blackQueensideMask) != 0) hash ^= CastlingNumbers[1, 1];

        zobristHash = hash;
    }

    public void ParseFen(string fen)
    {
        Reset();

        string[] fenParts = fen.Split();
        int file = 0;
        int rank = 7;

        foreach (char c in fenParts[0])
        {
            if (c == '/')
            {
                file = 0;
                rank--;
            }
            else if (char.IsDigit(c))
            {
                file += (int)char.GetNumericValue(c);
            }
            else
            {
                ulong bitboard = 1UL << (file + rank * 8);
                char piece = char.ToLower(c);

                switch (piece)
                {
                    case 'p': pawns |= bitboard; break;
                    case 'n': knights |= bitboard; break;
                    case 'b': bishops |= bitboard; break;
                    case 'r': rooks |= bitboard; break;
                    case 'q': queens |= bitboard; break;
                    case 'k': kings |= bitboard; break;
                }

                if (char.IsLower(c)) blackPieces |= bitboard;
                if (char.IsUpper(c)) whitePieces |= bitboard;
                occupiedSquares |= bitboard;

                file++;
            }
        }

        sideToMove = fenParts[1] == "w" ? Color.White : Color.Black;

        if (fenParts[2].ToString().Contains('K')) castleRights |= whiteKingsideMask;
        if (fenParts[2].ToString().Contains('Q')) castleRights |= whiteQueensideMask;
        if (fenParts[2].ToString().Contains('k')) castleRights |= blackKingsideMask;
        if (fenParts[2].ToString().Contains('q')) castleRights |= blackQueensideMask;

        epSquare = fenParts[3] == "-" ? (byte)INVALID_SQUARE : CoordToSquare(fenParts[3]);

        halfMoveClock = int.Parse(fenParts[4]);
        fullMoveCounter = int.Parse(fenParts[5]);

        GenerateZobristHash();
    }

    public string GetFen()
    {
        string fen = "";
        int emptySquares;

        for (int rank = 7; rank >= 0; rank--)
        {
            emptySquares = 0;

            for (int file = 0; file < 8; file++)
            {
                int square = (file + rank * 8);
                ulong squareMask = 1UL << square;
                char letter = ' ';

                if ((occupiedSquares & squareMask) == 0) emptySquares++;
                else
                {
                    if (emptySquares > 0) fen += emptySquares.ToString();
                    emptySquares = 0;

                    if ((pawns & squareMask) != 0) letter = 'P';
                    if ((knights & squareMask) != 0) letter = 'N';
                    if ((bishops & squareMask) != 0) letter = 'B';
                    if ((rooks & squareMask) != 0) letter = 'R';
                    if ((queens & squareMask) != 0) letter = 'Q';
                    if ((kings & squareMask) != 0) letter = 'K';

                    if ((blackPieces & squareMask) != 0) letter = char.ToLower(letter);
                    fen += letter;
                }
            }
            if (emptySquares > 0) fen += emptySquares.ToString();

            if (rank != 0) fen += '/';
        }
        fen += ' ';

        fen += sideToMove.ToString().ToLower().First();
        fen += ' ';

        string castling = "";
        if ((castleRights & whiteKingsideMask) != 0) castling += 'K';
        if ((castleRights & whiteQueensideMask) != 0) castling += 'Q';
        if ((castleRights & blackKingsideMask) != 0) castling += 'k';
        if ((castleRights & blackQueensideMask) != 0) castling += 'q';

        if (castling == "") fen += '-';
        else fen += castling;

        fen += ' ';

        fen += (epSquare != INVALID_SQUARE) ? SquareToCoord(epSquare) : '-';
        fen += ' ';

        fen += halfMoveClock.ToString();
        fen += ' ';

        fen += fullMoveCounter.ToString();

        return fen;
    }

    public void Reset()
    {
        pawns = 0;
        knights = 0;
        bishops = 0;
        rooks = 0;
        queens = 0;
        kings = 0;
        whitePieces = 0;
        blackPieces = 0;
        occupiedSquares = 0;
        zobristHash = 0;
        castleRights = 0;
        epSquare = 0;
        halfMoveClock = 0;
        fullMoveCounter = 0;
        sideToMove = 0;
        previousState = new();
        boardStates.Clear();
    }
}