using System;
using System.Collections.Generic;
using Raylib_cs;
using System.Numerics;

namespace Dittle
{
    class Program
    {
        const int OFFSET = 60;
        const int SIZE = 50;
        const int BOARD_SIZE_X = 500;
        const int BOARD_SIZE_Y = 600;

        public static void Main(string[] args)
        {
            int playersCount = 1;
            int aiDepth = 4;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-players" && i + 1 < args.Length) playersCount = int.Parse(args[++i]);
                if (args[i] == "-depth" && i + 1 < args.Length) aiDepth = int.Parse(args[++i]);
            }

            Raylib.InitWindow(BOARD_SIZE_X, BOARD_SIZE_Y, "Dittle");
            Raylib.SetTargetFPS(60);

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
                DrawBoard(board, selectedX, selectedY, legalMoves);
                DrawUI(aiDepth, currentPlayer, board);
                DrawAiMoveHighlight(lastAiMove, aiMoveTimer);
                Raylib.EndDrawing();
            }
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
            if (Raylib.CheckCollisionPointRec(m, new Rectangle(BOARD_SIZE_X - 120, 10, 100, 30)))
            {
                board = new(); current = Player.White;
                selX = null; selY = null; moves.Clear(); lastAi = null;
                return true;
            }
            if (Raylib.CheckCollisionPointRec(m, new(210, BOARD_SIZE_Y - 80, 40, 40)) && depth > 1) { depth--; return true; }
            if (Raylib.CheckCollisionPointRec(m, new(310, BOARD_SIZE_Y - 80, 40, 40)) && depth < 6) { depth++; return true; }
            return false;
        }

        private static void HandleBoardInput(Board board, ref Player current, ref int? selX, ref int? selY, ref List<Move> moves)
        {
            if (!Raylib.IsMouseButtonPressed(MouseButton.Left)) return;
            Vector2 mouse = Raylib.GetMousePosition();
            int startX = (BOARD_SIZE_X - 7 * OFFSET) / 2;
            int startY = (BOARD_SIZE_Y - 7 * OFFSET) / 2;
            int x = (int)((mouse.X - startX) / OFFSET);
            int y = (int)((mouse.Y - startY) / OFFSET);
            if (!board.IsInBounds(x, y)) return;

            if (selX == null)
            {
                if (board.Grid[x, y]?.Owner == current)
                {
                    selX = x; selY = y;
                    moves = Rules.GetAllLegalMoves(board, current).FindAll(m => m.FromX == x && m.FromY == y);
                }
            }
            else
            {
                Move move = moves.Find(m => m.ToX == x && m.ToY == y);
                if (move.FromX == selX && move.FromY == selY)
                {
                    AI.ApplyMove(board, move);
                    current = (current == Player.White) ? Player.Black : Player.White;
                }
                selX = null; selY = null; moves.Clear();
            }
        }

        private static void DrawBoard(Board board, int? selX, int? selY, List<Move> legalMoves)
        {
            int startX = (BOARD_SIZE_X - 7 * OFFSET) / 2, startY = (BOARD_SIZE_Y - 7 * OFFSET) / 2;
            int padding = (OFFSET - SIZE) / 2;
            Color woodDark = new(101, 67, 33, 255), woodLight = new(193, 154, 107, 255);
            for (int y = 0; y < Board.Size; y++)
                for (int x = 0; x < Board.Size; x++)
                {
                    int px = startX + x * OFFSET, py = startY + y * OFFSET;
                    Raylib.DrawRectangle(px, py, OFFSET, OFFSET, (x + y) % 2 == 0 ? woodLight : woodDark);
                    Raylib.DrawRectangleLines(px, py, OFFSET, OFFSET, Color.Black);
                    if (selX == x && selY == y) Raylib.DrawRectangle(px, py, OFFSET, OFFSET, new(0, 121, 241, 100));
                    foreach (var m in legalMoves) if (m.ToX == x && m.ToY == y)
                    {
                        Raylib.DrawCircle(px + OFFSET / 2, py + OFFSET / 2, OFFSET / 4, new(0, 121, 241, 200));
                        Raylib.DrawText(m.ResultDie.Top.ToString(), px + OFFSET / 2 - 5, py + OFFSET / 2 - 8, 18, Color.White);
                    }
                    Die? d = board.Grid[x, y];
                    if (d is not null && d.HasValue) DrawDie(px + padding, py + padding, SIZE, d.Value);
                }
        }

        private static void DrawUI(int depth, Player current, Board board)
        {
            int uiBottomY = BOARD_SIZE_Y - 80;
            Raylib.DrawRectangleLinesEx(new(BOARD_SIZE_X - 120, 10, 100, 30), 2, Color.White);
            Raylib.DrawText("RESTART", BOARD_SIZE_X - 110, 18, 16, Color.White);

            Raylib.DrawText("LEVEL:", 100, uiBottomY + 12, 18, Color.White);
            Raylib.DrawRectangle(210, uiBottomY, 40, 40, Color.LightGray);
            Raylib.DrawText("-", 225, uiBottomY + 5, 30, Color.Black);

            string dText = depth.ToString();
            int tw = Raylib.MeasureText(dText, 24);
            Raylib.DrawText(dText, 250 + (60 - tw) / 2, uiBottomY + 10, 24, Color.Yellow);

            Raylib.DrawRectangle(310, uiBottomY, 40, 40, Color.LightGray);
            Raylib.DrawText("+", 321, uiBottomY + 5, 30, Color.Black);

            Raylib.DrawText($"Turn: {current}", 10, 10, 20, Color.White);
            if (Rules.IsGameOver(board, out Player? w))
            {
                string winnerText = $"WINNER: {w}";
                int winnerTw = Raylib.MeasureText(winnerText, 30);
                Raylib.DrawText(winnerText, (BOARD_SIZE_X - winnerTw) / 2, BOARD_SIZE_Y / 2, 30, Color.Red);
            }
        }

        private static void DrawAiMoveHighlight(Move? lastMove, float timer)
        {
            if (timer <= 0 || !lastMove.HasValue) return;
            int startX = (BOARD_SIZE_X - 7 * OFFSET) / 2, startY = (BOARD_SIZE_Y - 7 * OFFSET) / 2;
            int fx = startX + lastMove.Value.FromX * OFFSET, fy = startY + lastMove.Value.FromY * OFFSET;
            int tx = startX + lastMove.Value.ToX * OFFSET, ty = startY + lastMove.Value.ToY * OFFSET;
            Raylib.DrawRectangleLinesEx(new(fx, fy, OFFSET, OFFSET), 3, Color.Red);
            Raylib.DrawLineEx(new(fx + OFFSET / 2, fy + OFFSET / 2), new(tx + OFFSET / 2, ty + OFFSET / 2), 3, Color.Red);
            Raylib.DrawRectangleLinesEx(new(tx, ty, OFFSET, OFFSET), 4, Color.Orange);
        }

        static void DrawDie(int x, int y, int size, Die die)
        {
            Color dieColor = die.Owner == Player.White ? Color.White : Color.Black;
            Color pipColor = die.Owner == Player.White ? Color.Black : Color.White;
            Rectangle rec = new(x, y, size, size);
            Raylib.DrawRectangleRounded(rec, 0.2f, 10, dieColor);
            Raylib.DrawRectangleRoundedLines(rec, 0.2f, 10, Color.Gray);
            int r = size / 10, m = size / 2, q1 = size / 4, q3 = 3 * size / 4;
            switch (die.Top)
            {
                case 1: DrawPips1(x, y, m, r, pipColor); break;
                case 2: DrawPips2(x, y, q1, q3, r, pipColor); break;
                case 3: DrawPips3(x, y, m, q1, q3, r, pipColor); break;
                case 4: DrawPips4(x, y, q1, q3, r, pipColor); break;
                case 5: DrawPips5(x, y, m, q1, q3, r, pipColor); break;
                case 6: DrawPips6(x, y, m, q1, q3, r, pipColor); break;
            }
        }

        private static void DrawPips1(int x, int y, int m, int r, Color c) => Raylib.DrawCircle(x + m, y + m, r, c);
        private static void DrawPips2(int x, int y, int q1, int q3, int r, Color c) { Raylib.DrawCircle(x + q1, y + q1, r, c); Raylib.DrawCircle(x + q3, y + q3, r, c); }
        private static void DrawPips3(int x, int y, int m, int q1, int q3, int r, Color c) { Raylib.DrawCircle(x + m, y + m, r, c); DrawPips2(x, y, q1, q3, r, c); }
        private static void DrawPips4(int x, int y, int q1, int q3, int r, Color c) { Raylib.DrawCircle(x + q1, y + q1, r, c); Raylib.DrawCircle(x + q3, y + q1, r, c); Raylib.DrawCircle(x + q1, y + q3, r, c); Raylib.DrawCircle(x + q3, y + q3, r, c); }
        private static void DrawPips5(int x, int y, int m, int q1, int q3, int r, Color c) { Raylib.DrawCircle(x + m, y + m, r, c); DrawPips4(x, y, q1, q3, r, c); }
        private static void DrawPips6(int x, int y, int m, int q1, int q3, int r, Color c) { Raylib.DrawCircle(x + q1, y + q1, r, c); Raylib.DrawCircle(x + q3, y + q1, r, c); Raylib.DrawCircle(x + q1, y + m, r, c); Raylib.DrawCircle(x + q3, y + m, r, c); Raylib.DrawCircle(x + q1, y + q3, r, c); Raylib.DrawCircle(x + q3, y + q3, r, c); }
    }
}
