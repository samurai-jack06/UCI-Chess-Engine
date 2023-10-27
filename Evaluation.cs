using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static ChessLogic;
using static PieceSquareTables;
using static Utility;

public class Evaluation
{
    static int PAWN = 100;
    static int KNIGHT = 350;
    static int BISHOP = 375;
    static int ROOK = 500;
    static int QUEEN = 1000;

    static int[] PieceValues = new int[5]
    {
        PAWN,
        KNIGHT,
        BISHOP,
        ROOK,
        QUEEN,
    };

    public int Evaluate(Board board)
    {
        int whiteEval = 0;
        int blackEval = 0;
        int perspective = board.sideToMove == Color.White ? 1 : -1;

        double transition = BitOperations.PopCount(board.occupiedSquares) / 36;

        ulong[] pieces = new ulong[6] { board.pawns, board.knights, board.bishops, board.rooks, board.queens, board.kings };

        for (int i = 0; i < 5; i++)
        {
            whiteEval += BitOperations.PopCount(board.whitePieces & pieces[i]) * PieceValues[i];
            blackEval += BitOperations.PopCount(board.blackPieces & pieces[i]) * PieceValues[i];
        }

        whiteEval += PSTScore(pieces, board.whitePieces, transition, true);
        blackEval += PSTScore(pieces, board.blackPieces, transition, false);

        return (whiteEval - blackEval) * perspective;
    }

    int PSTScore(ulong[] pieces, ulong friendlyPieces, double transition, bool white)
    {
        int score = 0;

        for (int i = 0; i < 6; i++)
        {
            ulong piece = friendlyPieces & pieces[i];

            while (piece != 0)
            {
                int square = PopLSB(ref piece);

                if (white) square ^= 56;

                switch (i)
                {
                    case 0:
                        score += (int)((transition * MG_PAWN_TABLE[square]) + ((1 - transition) * EG_PAWN_TABLE[square]));
                        break;
                    case 1:
                        score += (int)((transition * MG_KNIGHT_TABLE[square]) + ((1 - transition) * EG_KNIGHT_TABLE[square]));
                        break;
                    case 2:
                        score += (int)((transition * MG_BISHOP_TABLE[square]) + ((1 - transition) * EG_BISHOP_TABLE[square]));
                        break;
                    case 3:
                        score += (int)((transition * MG_ROOK_TABLE[square]) + ((1 - transition) * EG_ROOK_TABLE[square]));
                        break;
                    case 4:
                        score += (int)((transition * MG_QUEEN_TABLE[square]) + ((1 - transition) * EG_QUEEN_TABLE[square]));
                        break;
                    case 5:
                        score += (int)((transition * MG_KING_TABLE[square]) + ((1 - transition) * EG_KING_TABLE[square]));
                        break;
                }
            }
        }

        return score;
    }
}