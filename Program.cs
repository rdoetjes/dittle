using System;
using System.Collections.Generic;
using Raylib_cs;
using System.Numerics;

namespace Dittle
{
    class Program
    {
        public const int DefaultAiDepth = 3;
        public const int MaxAiDepth = 6;

        public static void Main(string[] args)
        {
            Graphics.InitializeResourcePath();

            int playersCount = 1;
            int aiDepth = DefaultAiDepth;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-players" && i + 1 < args.Length) playersCount = int.Parse(args[++i]);
                if (args[i] == "-depth" && i + 1 < args.Length) aiDepth = int.Parse(args[++i]);
            }

            Raylib.InitWindow(Graphics.BOARD_SIZE_X, Graphics.BOARD_SIZE_Y, "Dittle");
            Raylib.SetTargetFPS(30);

            Graphics.LoadResources();
            Board board = new();
            Player currentPlayer = Player.White;
            int? selectedX = null, selectedY = null;
            List<Move> legalMoves = [];
            Move? lastAiMove = null;
            float aiMoveTimer = 0;

            while (!Raylib.WindowShouldClose())
            {
                float deltaTime = Raylib.GetFrameTime();
                if (aiMoveTimer > 0) aiMoveTimer -= deltaTime;

                bool mouseHandled = HandleUIInput(ref board, ref currentPlayer, ref aiDepth, ref selectedX, ref selectedY, ref legalMoves, ref lastAiMove);

                if (!Rules.IsGameOver(board, out _))
                {
                    if (playersCount == 1 && currentPlayer == Player.Black && aiMoveTimer <= 0)
                    {
                        PerformAiTurn(board, aiDepth, ref lastAiMove, ref aiMoveTimer, ref currentPlayer);
                    }
                    else if (!mouseHandled)
                    {
                        HandleBoardInput(board, ref currentPlayer, ref selectedX, ref selectedY, ref legalMoves);
                    }
                }

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.DarkGray);
                Graphics.DrawBoard(board, selectedX, selectedY, legalMoves);

                Graphics.DrawUI(aiDepth, currentPlayer, board, MaxAiDepth);
                Graphics.DrawAiMoveHighlight(lastAiMove, aiMoveTimer);
                Raylib.EndDrawing();
            }

            Graphics.UnloadResources();
            Raylib.CloseWindow();
        }

        private static void PerformAiTurn(Board board, int depth, ref Move? lastMove, ref float timer, ref Player current)
        {
            Move? best = AI.GetBestMove(board, Player.Black, depth);
            if (best.HasValue)
            {
                lastMove = best.Value;
                AI.ApplyMove(board, best.Value);
                timer = 1.5f;
            }
            current = Player.White;
        }

        private static bool HandleUIInput(ref Board board, ref Player current, ref int depth, ref int? selX, ref int? selY, ref List<Move> moves, ref Move? lastAi)
        {
            if (!Raylib.IsMouseButtonPressed(MouseButton.Left)) return false;
            Vector2 m = Raylib.GetMousePosition();

            // Restart is always allowed
            if (Raylib.CheckCollisionPointRec(m, new Rectangle(Graphics.BOARD_SIZE_X - 120, 10, 100, 30)))
            {
                board = new Board(); current = Player.White;
                selX = null; selY = null; moves.Clear(); lastAi = null;
                return true;
            }

            // Only allow Level adjustments if no moves have been made yet (Board is in initial state)
            if (board.WhiteHorizontalMoves == 0 && board.BlackHorizontalMoves == 0 && IsInitialBoard(board))
            {
                if (Raylib.CheckCollisionPointRec(m, new Rectangle(210, Graphics.BOARD_SIZE_Y - 80, 40, 40)) && depth > 1) { depth--; return true; }
                if (Raylib.CheckCollisionPointRec(m, new Rectangle(310, Graphics.BOARD_SIZE_Y - 80, 40, 40)) && depth < MaxAiDepth) { depth++; return true; }
            }

            return false;
        }

        private static bool IsInitialBoard(Board board)
        {
            for (int y = 0; y < Board.Size; y++)
            {
                for (int x = 0; x < Board.Size; x++)
                {
                    Die? d = board.Grid[x, y];
                    if (y == 0)
                    {
                        if (d == null || d.Value.Owner != Player.Black) return false;
                    }
                    else if (y == 6)
                    {
                        if (d == null || d.Value.Owner != Player.White) return false;
                    }
                    else
                    {
                        if (d != null) return false;
                    }
                }
            }
            return true;
        }

        private static void HandleBoardInput(Board board, ref Player currentPlayer, ref int? selX, ref int? selY, ref List<Move> moves)
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
                        selX = x; selY = y;
                        moves = possibleMovesForDie;
                    }
                    // If no moves for this die, we don't select it,
                    // allowing the user to click another die immediately.
                }
            }
            else
            {
                Move move = moves.Find(m => m.ToX == x && m.ToY == y);
                if (move.FromX == selX && move.FromY == selY)
                {
                    AI.ApplyMove(board, move);
                    currentPlayer = (currentPlayer == Player.White) ? Player.Black : Player.White;
                }
                selX = null; selY = null; moves.Clear();
            }
        }
    }
}
