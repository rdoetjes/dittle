using System;
using System.Collections.Generic;

namespace Dittle
{
    public static class AI
    {
        public static Move? GetBestMove(Board board, Player player, int depth)
        {
            int bestVal = int.MinValue;
            Move? bestMove = null;
            List<Move> moves = Rules.GetAllLegalMoves(board, player);

            foreach (var move in moves)
            {
                Board nextBoard = board.Clone();
                ApplyMove(nextBoard, move);
                int val = Minimax(nextBoard, depth - 1, int.MinValue, int.MaxValue, false, player);
                if (val > bestVal)
                {
                    bestVal = val;
                    bestMove = move;
                }
            }
            return bestMove;
        }

        private static int Minimax(Board board, int depth, int alpha, int beta, bool maximizing, Player player)
        {
            if (depth == 0 || Rules.IsGameOver(board, out _))
            {
                return Rules.Evaluate(board, player);
            }

            Player current = maximizing ? player : (player == Player.White ? Player.Black : Player.White);
            List<Move> moves = Rules.GetAllLegalMoves(board, current);

            if (maximizing)
            {
                int maxEval = int.MinValue;
                foreach (var move in moves)
                {
                    Board next = board.Clone();
                    ApplyMove(next, move);
                    int eval = Minimax(next, depth - 1, alpha, beta, false, player);
                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha) break;
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (var move in moves)
                {
                    Board next = board.Clone();
                    ApplyMove(next, move);
                    int eval = Minimax(next, depth - 1, alpha, beta, true, player);
                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha) break;
                }
                return minEval;
            }
        }

        public static void ApplyMove(Board board, Move move)
        {
            board.Grid[move.FromX, move.FromY] = null;
            board.Grid[move.ToX, move.ToY] = move.ResultDie;
        }
    }
}
