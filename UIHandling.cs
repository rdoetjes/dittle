using System.Collections.Generic;
using Raylib_cs;
using System.Numerics;

namespace Dittle
{
    public static class UIHandling
    {
        public static bool HandleUIInput(ref UiState state)
        {
            if (!Raylib.IsMouseButtonPressed(MouseButton.Left)) return false;
            Vector2 m = Raylib.GetMousePosition();

            // Restart is always allowed
            if (Raylib.CheckCollisionPointRec(m, new Rectangle(Graphics.BOARD_SIZE_X - 120, 10, 100, 30)))
            {
                state.Board = new Board();
                state.CurrentTurn = Player.White;
                state.SelectedX = null;
                state.SelectedY = null;
                state.LegalMoves.Clear();
                state.LastAiMove = null;
                return true;
            }

            // Only allow Level adjustments if no moves have been made yet (Board is in initial state)
            if (state.Board.WhiteHorizontalMoves == 0 && state.Board.BlackHorizontalMoves == 0 && state.Board.IsInitialBoard())
            {
                int uiControlY = Graphics.BOARD_SIZE_Y - 60;
                if (Raylib.CheckCollisionPointRec(m, new Rectangle(210, uiControlY, 40, 40)) && state.Depth > 1)
                {
                    state.Depth--;
                    return true;
                }
                if (Raylib.CheckCollisionPointRec(m, new Rectangle(310, uiControlY, 40, 40)) && state.Depth < state.MaxAiDepth)
                {
                    state.Depth++;
                    return true;
                }
            }

            return false;
        }

        public static void HandleBoardInput(ref UiState state)
        {
            if (!Raylib.IsMouseButtonPressed(MouseButton.Left)) return;
            Vector2 mouse = Raylib.GetMousePosition();
            int startX = (Graphics.BOARD_SIZE_X - 7 * Graphics.OFFSET) / 2;
            int startY = (Graphics.BOARD_SIZE_Y - 7 * Graphics.OFFSET) / 2;
            int x = (int)((mouse.X - startX) / Graphics.OFFSET);
            int y = (int)((mouse.Y - startY) / Graphics.OFFSET);
            if (!state.Board.IsInBounds(x, y)) return;

            if (state.SelectedX == null)
            {
                if (state.Board.Grid[x, y]?.Owner == state.CurrentTurn)
                {
                    var allLegalMoves = Rules.GetAllLegalMoves(state.Board, state.CurrentTurn);
                    var possibleMovesForDie = allLegalMoves.FindAll(m => m.FromX == x && m.FromY == y);

                    // If valid move select that valid move based on current x and y click value.
                    if (possibleMovesForDie.Count > 0)
                    {
                        state.SelectedX = x;
                        state.SelectedY = y;
                        state.LegalMoves = possibleMovesForDie;
                    }
                    // If no moves for this die, we don't select it,
                    // allowing the user to click another die immediately.
                }
            }
            else
            {
                int tx = x, ty = y;
                int moveIdx = state.LegalMoves.FindIndex(m => m.ToX == tx && m.ToY == ty);
                if (moveIdx != -1)
                {
                    AI.ApplyMove(state.Board, state.LegalMoves[moveIdx]);
                }
                state.SelectedX = null;
                state.SelectedY = null;
                state.LegalMoves.Clear();
            }
        }
    }
}
