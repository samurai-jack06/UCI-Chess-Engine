using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static ChessLogic;
using static Utility;
using static TranspositionTable;
using System.Diagnostics;
using System.Data;


public static class Program
{
    public static void Main(string[] args)
    {
        Board board = new(startFen);
        Search search = new(board, defaultTTsize);

        while (true)
        {
            string line = Console.ReadLine();
            string[] tokens = line.Split();

            if (tokens[0] == "quit") break;

            if (tokens[0] == "ucinewgame")
            {
                search.tt.table = new TTEntry[defaultTTsize];
                CalculateAll();
            }

            if (tokens[0] == "uci") RespondUCI();

            if (tokens[0] == "isready") Console.WriteLine("readyok");

            if (tokens[0] == "position")
            {
                int nextIndex;

                if (tokens[1] == "startpos")
                {
                    board.ParseFen(startFen);
                    nextIndex = 2;
                }
                else if (tokens[1] == "fen")
                {
                    string fen = "";
                    for (int i = 2; i < 8; i++)
                    {
                        fen += tokens[i];
                        fen += " ";
                    }
                    board.ParseFen(fen);
                    nextIndex = 8;
                }
                else continue;

                if (tokens.Length <= nextIndex || tokens[nextIndex] != "moves") continue;

                for (int i = nextIndex + 1; i < tokens.Length; i++)
                {
                    Move move = StringToMove(board, tokens[i]);
                    board.MakeMove(move);
                }
            }
            if (tokens[0] == "go")
            {
                double allocatedTime = 0;
                double wTime = 0;
                double bTime = 0;
                double wInc = 0;
                double bInc = 0;

                for (int i = 1; i < tokens.Length; i += 2)
                {
                    string type = tokens[i];
                    int value = 0;

                    if (tokens.Length > i + 1) value = int.Parse(tokens[i + 1]);

                    if (type == "movetime") allocatedTime = (int)(value * 0.95);

                    if (type == "wtime") wTime = value;

                    if (type == "btime") bTime = value;

                    if (type == "winc") wInc = value;

                    if (type == "binc") bInc = value;
                }

                (double, double) calculated = CalculateTime(board.sideToMove, wTime, bTime, wInc, bInc);
                double time = calculated.Item1;
                double inc = calculated.Item2;

                allocatedTime = time + inc;

                Move bestMove = search.SearchBestMove(128, (int)allocatedTime);
                Console.WriteLine("bestmove " + MoveToString(bestMove));
                Console.WriteLine("info depth " + search.Depth + " nodes " + search.nodes + " score cp " + search.bestMoveRootScore);
            }
        }
    }

    public static (double, double) CalculateTime(Color stm, double wTime, double bTime, double wInc, double bInc)
    {
        double time = 0;
        double inc = 0;

        time = stm == Color.White ? (wTime / 40) : (bTime / 40);
        inc = stm == Color.White ? (wInc * 0.75) : (bInc * 0.75);

        return (time, inc);
    }

    public static void RespondUCI()
    {
        Console.WriteLine("id name v0.2.0-a");
        Console.WriteLine("id author samurai jack");
        Console.WriteLine("uciok");
    }
}