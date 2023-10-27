using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChessLogic;

public class TranspositionTable
{
    public static int defaultTTsize = 2097152;
    public int ttSize;
    public TTEntry[] table;

    public enum ScoreFlag : byte
    {
        Exact,
        Upper_bound,
        Lower_bound
    }

    public struct TTEntry
    {
        public TTEntry(ulong ZobristHash, ScoreFlag ScoreFlag, Move BestMove, short Score, byte Depth)
        {
            zobristHash = ZobristHash;
            scoreFlag = ScoreFlag;
            bestMove = BestMove;
            score = Score;
            depth = Depth;
        }

        public ulong zobristHash;
        public Move bestMove;
        public short score;
        public ScoreFlag scoreFlag;
        public byte depth;
    }

    public TranspositionTable(int size)
    {
        ttSize = size;
        table = new TTEntry[ttSize];
    }

    public void StoreEntry(TTEntry entry)
    {
        table[entry.zobristHash % (ulong)ttSize] = entry;
    }
}