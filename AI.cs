using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dittle
{
    public static class AI
    {
        public static Move? GetBestMove(Board board, Player player, int depth)
        {
            List<Move> moves = Rules.GetAllLegalMoves(board, player);
            if (moves.Count == 0) return null;

            // To introduce variety, we store evaluations of all moves
            var moveEvaluations = new List<(Move move, int score)>();
            object lockObj = new();

            Parallel.ForEach(moves, move =>
            {
                Board nextBoard = board.Clone();
                ApplyMove(nextBoard, move);

                int val = Minimax(nextBoard, depth - 1, int.MinValue, int.MaxValue, false, player);

                lock (lockObj)
                {
                    moveEvaluations.Add((move, val));
                }
            });

            if (moveEvaluations.Count == 0) return null;

            // Find the best score
            int bestVal = moveEvaluations.Max(item => item.score);

            // Collect all moves that share the best score
            var bestMoves = moveEvaluations.FindAll(m => m.score == bestVal);

            // Randomly select one of the best moves so the AI isn't deterministic
            Random rng = new();

            return bestMoves[rng.Next(bestMoves.Count)].move;
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

            // Update horizontal move tracking
            bool isHorizontal = move.FromY == move.ToY;
            if (move.ResultDie.Owner == Player.White)
            {
                if (isHorizontal) board.WhiteHorizontalMoves++;
                else board.WhiteHorizontalMoves = 0;
            }
            else
            {
                if (isHorizontal) board.BlackHorizontalMoves++;
                else board.BlackHorizontalMoves = 0;
            }
        }
    }
}
