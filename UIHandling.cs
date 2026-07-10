using System.Collections.Generic;
using Raylib_cs;
using System.Numerics;

namespace Dittle
{
    public static class UIHandling
    {
        public static bool HandleUIInput(ref Board board, ref Player current, ref int depth, ref int? selX, ref int? selY, ref List<Move> moves, ref Move? lastAi, int maxAiDepth)
        {
            if (!Raylib.IsMouseButtonPressed(MouseButton.Left)) return false;
            Vector2 m = Raylib.GetMousePosition();

            // Restart is always allowed
            if (Raylib.CheckCollisionPointRec(m, new Rectangle(Graphics.BOARD_SIZE_X - 120, 10, 100, 30)))
            {
                board = new Board();
                current = Player.White;
                selX = null;
                selY = null;
                moves.Clear();
                lastAi = null;
                return true;
            }

            // Only allow Level adjustments if no moves have been made yet (Board is in initial state)
            if (board.WhiteHorizontalMoves == 0 && board.BlackHorizontalMoves == 0 && board.IsInitialBoard())
            {
                int uiControlY = Graphics.BOARD_SIZE_Y - 60;
                if (Raylib.CheckCollisionPointRec(m, new Rectangle(210, uiControlY, 40, 40)) && depth > 1)
                {
                    depth--;
                    return true;
                }
                if (Raylib.CheckCollisionPointRec(m, new Rectangle(310, uiControlY, 40, 40)) && depth < maxAiDepth)
                {
                    depth++;
                    return true;
                }
            }

            return false;
        }

        public static void HandleBoardInput(Board board, ref Player currentPlayer, ref int? selX, ref int? selY, ref List<Move> moves)
        {
            if (!Raylib.IsMouseButtonPressed(MouseButton.Left)) return;
            Vector2 mouse = Raylib.GetMousePosition();
            int startX = (Graphics.BOARD_SIZE_X - 7 * Graphics.OFFSET) / 2;
            int startY = (Graphics.BOARD_SIZE_Y - 7 * Graphics.OFFSET) / 2;
            int x = (int)((mouse.X - startX) / Graphics.OFFSET);
            int y = (int)((mouse.Y - startY) / Graphics.OFFSET);
            if (!board.IsInBounds(x, y)) return;

            if (selX == null)
            {
                if (board.Grid[x, y]?.Owner == currentPlayer)
                {
                    var allLegalMoves = Rules.GetAllLegalMoves(board, currentPlayer);
                    var possibleMovesForDie = allLegalMoves.FindAll(m => m.FromX == x && m.FromY == y);

                    // If valid move select that valid move based on current x and y click value.
                    if (possibleMovesForDie.Count > 0)
                    {
                        selX = x;
                        selY = y;
                        moves = possibleMovesForDie;
                    }
                    // If no moves for this die, we don't select it,
                    // allowing the user to click another die immediately.
                }
            }
            else
            {
                int tx = x, ty = y;
                int moveIdx = moves.FindIndex(m => m.ToX == tx && m.ToY == ty);
                if (moveIdx != -1)
                {
                    AI.ApplyMove(board, moves[moveIdx]);
                }
                selX = null;
                selY = null;
                moves.Clear();
            }
        }
    }
}
