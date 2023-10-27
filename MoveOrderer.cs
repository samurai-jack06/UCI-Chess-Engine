using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChessLogic;

public class MoveOrderer
{
    struct MoveScore : IComparable<MoveScore>
    {
        public MoveScore(Move Move, int Score)
        {
            move = Move;
            score = Score;
        }
        public Move move;
        public int score;

        public int CompareTo(MoveScore other)
        {
            return other.score.CompareTo(score);
        }
    }

    public void OrderMoves(Board board, ref Span<Move> moves)
    {
        Span<MoveScore> ordered = stackalloc MoveScore[moves.Length];

        for (int i = 0; i < moves.Length; i++)
        {
            Move move = moves[i];
            MoveScore moveScore = new(move, 0);
            PieceType attacker = PieceType.None;
            PieceType victim = PieceType.None;

            if ((board.pawns & (1UL << move.fromSquare)) != 0) attacker = PieceType.Pawn;
            if ((board.knights & (1UL << move.fromSquare)) != 0) attacker = PieceType.Knight;
            if ((board.bishops & (1UL << move.fromSquare)) != 0) attacker = PieceType.Bishop;
            if ((board.rooks & (1UL << move.fromSquare)) != 0) attacker = PieceType.Rook;
            if ((board.queens & (1UL << move.fromSquare)) != 0) attacker = PieceType.Queen;
            if ((board.kings & (1UL << move.fromSquare)) != 0) attacker = PieceType.King;

            if ((board.occupiedSquares & (1UL << move.toSquare)) != 0)
            {
                moveScore.score += 25;

                if ((board.pawns & (1UL << move.toSquare)) != 0) victim = PieceType.Pawn;
                if ((board.knights & (1UL << move.toSquare)) != 0) victim = PieceType.Knight;
                if ((board.bishops & (1UL << move.toSquare)) != 0) victim = PieceType.Bishop;
                if ((board.rooks & (1UL << move.toSquare)) != 0) victim = PieceType.Rook;
                if ((board.queens & (1UL << move.toSquare)) != 0) victim = PieceType.Queen;
                if ((board.kings & (1UL << move.toSquare)) != 0) victim = PieceType.King;

                if (attacker == PieceType.Pawn && victim != PieceType.Pawn) moveScore.score += 50;
                else moveScore.score += (((int)victim - (int)attacker) * 20);
            }

            if (move.moveType == MoveType.Promotion) moveScore.score += 50;
            if (move.moveType == MoveType.Castle) moveScore.score += 25;
            ordered[i] = moveScore;
        }

        ordered.Sort();
        for (int i = 0; i < moves.Length; i++) moves[i] = ordered[i].move;
    }
}
