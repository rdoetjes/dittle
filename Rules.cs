using System;
using System.Collections.Generic;

namespace Dittle
{
    public struct Move
    {
        public int FromX, FromY, ToX, ToY;
        public Die ResultDie;

        public Move(int fx, int fy, int tx, int ty, Die rd)
        {
            FromX = fx; FromY = fy; ToX = tx; ToY = ty;
            ResultDie = rd;
        }
    }

    public static class Rules
    {
        public static List<Move> GetAllLegalMoves(Board board, Player player)
        {
            List<Move> moves = [];
            int horizontalCount = (player == Player.White) ? board.WhiteHorizontalMoves : board.BlackHorizontalMoves;
            bool mustMoveForward = horizontalCount >= Board.MaxHorizontalMoves;

            for (int y = 0; y < Board.Size; y++)
            {
                for (int x = 0; x < Board.Size; x++)
                {
                    Die? die = board.Grid[x, y];
                    if (die.HasValue && die.Value.Owner == player)
                    {
                        AddMovesForDie(board, x, y, die.Value, moves);
                    }
                }
            }

            if (mustMoveForward)
            {
                int forwardYDir = (player == Player.White) ? -1 : 1;
                var forwardMoves = moves.FindAll(m => (m.ToY - m.FromY) * forwardYDir > 0);
                if (forwardMoves.Count > 0)
                {
                    return forwardMoves;
                }
            }

            return moves;
        }

        private static void AddMovesForDie(Board board, int x, int y, Die die, List<Move> moves)
        {
            int forwardY = (die.Owner == Player.White) ? -1 : 1;

            // 1. TILT FORWARD
            TryAddTilt(board, x, y, 0, forwardY, die, moves);

            // 2. TILT SIDEWAYS
            TryAddTilt(board, x, y, 1, 0, die, moves);
            TryAddTilt(board, x, y, -1, 0, die, moves);

            // 3. JUMP VERTICAL / HORIZONTAL (Straight)
            // Rule: "Jump over one or more dice... there must be at least one empty space between each die being jumped."
            AddJumps(board, x, y, x, y, die, moves, false);

            // 4. TILT + JUMP (Straight or Mixed)
            AddTiltJumps(board, x, y, die, moves, forwardY);
        }

        private static void TryAddTilt(Board board, int x, int y, int dx, int dy, Die die, List<Move> moves)
        {
            int nx = x + dx;
            int ny = y + dy;
            if (board.IsInBounds(nx, ny) && board.Grid[nx, ny] == null)
            {
                moves.Add(new Move(x, y, nx, ny, die.Tilted(dx, dy)));
            }
        }

        private static void AddJumps(Board board, int originalX, int originalY, int fromX, int fromY, Die die, List<Move> moves, bool afterTilt)
        {
            int forwardY = (die.Owner == Player.White) ? -1 : 1;

            // Straight Vertical
            AddStraightJump(board, originalX, originalY, fromX, fromY, 0, forwardY, die, moves);
            // Straight Horizontal
            AddStraightJump(board, originalX, originalY, fromX, fromY, 1, 0, die, moves);
            AddStraightJump(board, originalX, originalY, fromX, fromY, -1, 0, die, moves);

            // MIXED JUMP (L-shape)
            // "combine a vertical and horizontal jump — similar to an L-shape — over one or more dice."
            if (afterTilt)
            {
                // From the current position (fromX, fromY), we can do an L-shape jump.
                // An L-shape jump is one vertical segment and one horizontal segment.
                // Both segments must jump over at least one die, and the whole move must end in a gap.
                AddLJump(board, originalX, originalY, fromX, fromY, forwardY, die, moves);
            }
        }

        private static void AddStraightJump(Board board, int originalX, int originalY, int fromX, int fromY, int dx, int dy, Die die, List<Move> moves)
        {
            int tx = fromX + dx;
            int ty = fromY + dy;

            if (!CanStartJump(board, tx, ty, originalX, originalY)) return;

            ProcessJumpSequence(board, originalX, originalY, tx, ty, dx, dy, die, moves);
        }

        private static bool CanStartJump(Board board, int tx, int ty, int originalX, int originalY)
        {
            // Rule: To jump, you must jump over a die that is IMMEDIATELY adjacent.
            return board.IsInBounds(tx, ty) && IsOccupied(board, tx, ty, originalX, originalY);
        }

        private static void ProcessJumpSequence(Board board, int originalX, int originalY, int tx, int ty, int dx, int dy, Die die, List<Move> moves)
        {
            bool onDie = true;
            tx += dx;
            ty += dy;

            while (board.IsInBounds(tx, ty))
            {
                bool occupied = IsOccupied(board, tx, ty, originalX, originalY);
                if (onDie)
                {
                    if (occupied) return; // Illegal: tight cluster
                    onDie = false;
                    moves.Add(new Move(originalX, originalY, tx, ty, die));
                }
                else
                {
                    if (occupied) onDie = true;
                    else return; // Stop at multiple gaps
                }
                tx += dx;
                ty += dy;
            }
        }

        private static void AddLJump(Board board, int originalX, int originalY, int fromX, int fromY, int forwardY, Die die, List<Move> moves)
        {
            // Vertical then Horizontal
            foreach (int dx in new[] { 1, -1 })
                AddLJumpSegments(board, originalX, originalY, fromX, fromY, 0, forwardY, dx, 0, die, moves);

            // Horizontal then Vertical
            foreach (int dx in new[] { 1, -1 })
                AddLJumpSegments(board, originalX, originalY, fromX, fromY, dx, 0, 0, forwardY, die, moves);
        }

        private static void AddLJumpSegments(Board board, int originalX, int originalY, int fromX, int fromY, int dx1, int dy1, int dx2, int dy2, Die die, List<Move> moves)
        {
            // First leg: Must jump over an IMMEDIATELY adjacent die
            int tx = fromX + dx1;
            int ty = fromY + dy1;
            if (!board.IsInBounds(tx, ty) || !IsOccupied(board, tx, ty, originalX, originalY)) return;

            // Move to the gap after that die
            tx += dx1;
            ty += dy1;
            if (!board.IsInBounds(tx, ty) || IsOccupied(board, tx, ty, originalX, originalY)) return;

            // We are in a gap after the first jump leg.
            // Now start second leg from here (tx, ty)
            AddSecondLeg(board, originalX, originalY, tx, ty, dx2, dy2, die, moves);
        }

        private static void AddSecondLeg(Board board, int originalX, int originalY, int fromX, int fromY, int dx, int dy, Die die, List<Move> moves)
        {
            // Second leg: Must jump over an IMMEDIATELY adjacent die from current gap
            int tx = fromX + dx;
            int ty = fromY + dy;
            if (!board.IsInBounds(tx, ty) || !IsOccupied(board, tx, ty, originalX, originalY)) return;

            // Move to the gap after that die
            tx += dx;
            ty += dy;
            if (!board.IsInBounds(tx, ty) || IsOccupied(board, tx, ty, originalX, originalY)) return;

            // Legal landing
            moves.Add(new Move(originalX, originalY, tx, ty, die));
        }

        private static bool IsOccupied(Board board, int x, int y, int originalX, int originalY)
        {
            if (x == originalX && y == originalY) return false;
            return board.Grid[x, y] != null;
        }

        private static void AddTiltJumps(Board board, int x, int y, Die die, List<Move> moves, int forwardY)
        {
            int[] dxs = { 0, 1, -1 };
            int[] dys = { forwardY, 0, 0 };

            for (int i = 0; i < 3; i++)
            {
                int dx = dxs[i];
                int dy = dys[i];
                int tx = x + dx;
                int ty = y + dy;

                if (board.IsInBounds(tx, ty) && board.Grid[tx, ty] == null)
                {
                    Die tilted = die.Tilted(dx, dy);
                    AddJumps(board, x, y, tx, ty, tilted, moves, true);
                }
            }
        }

        public static bool IsGameOver(Board board, out Player? winner)
        {
            winner = null;
            int whiteInBase = 0;
            int blackInBase = 0;
            int whiteScore = 0;
            int blackScore = 0;

            for (int y = 0; y < Board.Size; y++)
            {
                for (int x = 0; x < Board.Size; x++)
                {
                    Die? d = board.Grid[x, y];
                    if (d.HasValue)
                    {
                        if (d.Value.Owner == Player.White && y == 0)
                        {
                            whiteInBase++;
                            whiteScore += d.Value.Top;
                        }
                        else if (d.Value.Owner == Player.Black && y == Board.Size - 1)
                        {
                            blackInBase++;
                            blackScore += d.Value.Top;
                        }
                    }
                }
            }

            // Game ends when ONE player gets ALL 7 dice in the opponent's base row
            if (whiteInBase == 7 || blackInBase == 7)
            {
                // Winner is the one with the higher score in their respective goal row
                if (whiteScore > blackScore) winner = Player.White;
                else if (blackScore > whiteScore) winner = Player.Black;
                else
                {
                    // Draw-breaker: the player who reached the goal row with all 7 dice wins
                    winner = (whiteInBase == 7) ? Player.White : Player.Black;
                }
                return true;
            }
            return false;
        }

        public static int Evaluate(Board board, Player player)
        {
            int whiteTotal = 0;
            int blackTotal = 0;

            for (int y = 0; y < Board.Size; y++)
            {
                for (int x = 0; x < Board.Size; x++)
                {
                    Die? d = board.Grid[x, y];
                    if (d.HasValue)
                    {
                        if (d.Value.Owner == Player.White)
                        {
                            // Priority 1: High value in base row
                            if (y == 0) whiteTotal += (d.Value.Top * 100);
                            // Priority 2: Getting into base row
                            else whiteTotal += (6 - y) * 10 + d.Value.Top;
                        }
                        else
                        {
                            if (y == 6) blackTotal += (d.Value.Top * 100);
                            else blackTotal += y * 10 + d.Value.Top;
                        }
                    }
                }
            }

            // Check if game is over to award massive win bonus
            if (IsGameOver(board, out Player? winner))
            {
                if (winner == player) return 1000000;
                if (winner != null) return -1000000;
            }

            return (player == Player.White) ? (whiteTotal - blackTotal) : (blackTotal - whiteTotal);
        }
    }
}
