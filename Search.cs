using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ChessLogic;
using static TranspositionTable;
using static Utility;

public class Search
{
    Board board;
    Evaluation evaluation;
    Stopwatch stopwatch;
    MoveOrderer moveOrderer;
    public TranspositionTable tt;
    Move bestMoveRoot;

    double maxTime;
    int nodes;

    bool searchCancelled;

    public Search(Board Board, int ttSize)
    {
        board = Board;
        evaluation = new();
        stopwatch = new();
        bestMoveRoot = new();
        moveOrderer = new();
        tt = new(ttSize);
        nodes = 0;
        maxTime = 0;
    }

    public Move SearchBestMove(int maxDepth, double time)
    {
        nodes = 0;
        maxTime = time;
        searchCancelled = false;
        Move move = new();
        int depth = 1;

        stopwatch.Restart();

        while (stopwatch.ElapsedMilliseconds < maxTime && (depth < maxDepth))
        {
            int eval = Negamax(board, depth, 0, -INFINITY, INFINITY);
            if (!searchCancelled)
            {
                move = bestMoveRoot;
                Console.WriteLine("info depth " + depth + " nodes " + nodes + " score cp " + eval);
            }
            depth++;
        }

        stopwatch.Stop();
        return move;
    }

    public int Qsearch(Board board, int alpha, int beta)
    {
        int bestScore = -INFINITY;

        int score = evaluation.Evaluate(board);

        if (score > bestScore) bestScore = score;
        if (score >= beta) return score;
        if (score > alpha) alpha = score;

        Span<Move> captures = stackalloc Move[256];
        board.GenMoves(ref captures, capturesOnly: true);

        moveOrderer.OrderMoves(board, ref captures);

        for (int i = 0; i < captures.Length; i++)
        {
            Move move = captures[i];

            if (!board.MakeMove(move))
            {
                board.UnmakeMove();
                continue;
            }

            nodes++;

            score = -Qsearch(board, -beta, -alpha);
            board.UnmakeMove();

            if (score > bestScore) bestScore = score;
            if (score >= beta) return score;
            if (score > alpha) alpha = score;
        }

        return bestScore;
    }

    public int Negamax(Board board, int depth, int ply, int alpha, int beta)
    {
        ulong zobristHash = board.zobristHash;
        int index = (int)(zobristHash % (ulong)tt.ttSize);

        ScoreFlag scoreFlag = ScoreFlag.Upper_bound;

        TTEntry entry = tt.table[index];

        if (ply != 0 && entry.zobristHash == zobristHash && entry.depth >= depth)
        {
            if (entry.scoreFlag == ScoreFlag.Exact) return entry.score;
            else if ((entry.scoreFlag == ScoreFlag.Upper_bound) && (entry.score <= alpha)) return entry.score;
            else if ((entry.scoreFlag == ScoreFlag.Lower_bound) && (entry.score >= beta)) return entry.score;
        }


        if (depth == 0) return Qsearch(board, alpha, beta);

        Span<Move> moves = stackalloc Move[256];
        board.GenMoves(ref moves, capturesOnly: false);

        moveOrderer.OrderMoves(board, ref moves);

        Move bestMove = new();
        int bestScore = -INFINITY;
        int legalMovesPlayed = 0;

        for (int i = 0; i < moves.Length; i++)
        {
            Move move = moves[i];

            if (!board.MakeMove(move))
            {
                board.UnmakeMove();
                continue;
            }

            nodes++;
            legalMovesPlayed++;

            int score = -Negamax(board, depth - 1, ply + 1, -beta, -alpha);
            board.UnmakeMove();

            if (((nodes % 8192) == 0) && (stopwatch.ElapsedMilliseconds > maxTime))
            {
                CancelSearch();
                return INFINITY;
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
                bestMoveRoot = (ply == 0) ? bestMove : bestMoveRoot;
            }
            if (score >= beta)
            {
                tt.StoreEntry(new TTEntry(zobristHash, ScoreFlag.Lower_bound, move, (short)score, (byte)depth));
                return score;
            }
            if (score > alpha)
            {
                alpha = score;
                scoreFlag = ScoreFlag.Exact;
            }
        }

        if (legalMovesPlayed == 0) return board.IsKingInCheck() ? (-INFINITY + ply) : 0;

        tt.StoreEntry(new TTEntry(zobristHash, scoreFlag, bestMove, (short)bestScore, (byte)depth));

        return bestScore;
    }

    public void CancelSearch()
    {
        searchCancelled = true;
    }
}