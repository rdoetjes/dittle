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
        const int BOARD_SIZE_Y = 500;

        public static void Main(string[] args)
        {
            int playersCount = 1;
            int aiDepth = 3;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-players" && i + 1 < args.Length) playersCount = int.Parse(args[++i]);
                if (args[i] == "-depth" && i + 1 < args.Length) aiDepth = int.Parse(args[++i]);
            }

            Raylib.InitWindow(BOARD_SIZE_X, BOARD_SIZE_Y, "Dittle");
            Raylib.SetTargetFPS(60);

            Board board = new Board();
            Player currentPlayer = Player.Yellow;
            int? selectedX = null, selectedY = null;
            List<Move> legalMoves = new List<Move>();

            // AI Animation State
            Move? lastAiMove = null;
            float aiMoveTimer = 0;
            const float AI_MOVE_DISPLAY_TIME = 1.5f;

            while (!Raylib.WindowShouldClose())
            {
                float deltaTime = Raylib.GetFrameTime();
                if (aiMoveTimer > 0) aiMoveTimer -= deltaTime;

                if (Rules.IsGameOver(board, out Player? winner))
                {
                }
                else if (playersCount == 1 && currentPlayer == Player.Green && aiMoveTimer <= 0)
                {
                    Move? best = AI.GetBestMove(board, Player.Green, aiDepth);
                    if (best.HasValue)
                    {
                        lastAiMove = best.Value;
                        AI.ApplyMove(board, best.Value);
                        aiMoveTimer = AI_MOVE_DISPLAY_TIME;
                    }
                    currentPlayer = Player.Yellow;
                }
                else
                {
                    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
                        Vector2 mouse = Raylib.GetMousePosition();
                        int x = (int)((mouse.X - (BOARD_SIZE_X - 7 * OFFSET) / 2) / OFFSET);
                        int y = (int)((mouse.Y - (BOARD_SIZE_Y - 7 * OFFSET) / 2) / OFFSET);

                        if (board.IsInBounds(x, y))
                        {
                            Die? d = board.Grid[x, y];
                            if (selectedX == null)
                            {
                                if (d is not null && d.HasValue && d.Value.Owner == currentPlayer)
                                {
                                    selectedX = x;
                                    selectedY = y;
                                    legalMoves = Rules.GetAllLegalMoves(board, currentPlayer);
                                    legalMoves = legalMoves.FindAll(m => m.FromX == x && m.FromY == y);
                                }
                            }
                            else
                            {
                                Move move = legalMoves.Find(m => m.ToX == x && m.ToY == y);
                                if (move.FromX == selectedX && move.FromY == selectedY)
                                {
                                    AI.ApplyMove(board, move);
                                    currentPlayer = (currentPlayer == Player.Yellow) ? Player.Green : Player.Yellow;
                                    selectedX = null;
                                    selectedY = null;
                                    legalMoves.Clear();
                                }
                                else
                                {
                                    selectedX = null;
                                    selectedY = null;
                                    legalMoves.Clear();
                                }
                            }
                        }
                    }
                }

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.DarkGray);

                DrawBoard(board, selectedX, selectedY, legalMoves);

                // Highlight AI's last move
                if (aiMoveTimer > 0 && lastAiMove.HasValue)
                {
                    int startX = (BOARD_SIZE_X - 7 * OFFSET) / 2;
                    int startY = (BOARD_SIZE_Y - 7 * OFFSET) / 2;

                    int fx = startX + lastAiMove.Value.FromX * OFFSET;
                    int fy = startY + lastAiMove.Value.FromY * OFFSET;
                    int tx = startX + lastAiMove.Value.ToX * OFFSET;
                    int ty = startY + lastAiMove.Value.ToY * OFFSET;

                    // Draw a ghost of where it was
                    Raylib.DrawRectangleLinesEx(new Rectangle(fx, fy, OFFSET, OFFSET), 3, Color.Red);
                    // Draw an arrow or line to where it went
                    Raylib.DrawLineEx(new Vector2(fx + OFFSET / 2, fy + OFFSET / 2),
                                     new Vector2(tx + OFFSET / 2, ty + OFFSET / 2), 3, Color.Red);
                    Raylib.DrawRectangleLinesEx(new Rectangle(tx, ty, OFFSET, OFFSET), 4, Color.Orange);
                }

                Raylib.DrawText($"Turn: {currentPlayer}", 10, 10, 20, Color.White);
                if (Rules.IsGameOver(board, out Player? w))
                {
                    Raylib.DrawText($"WINNER: {w}", BOARD_SIZE_X / 2 - 100, BOARD_SIZE_Y / 2, 30, Color.Red);
                }

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }

        static void DrawBoard(Board board, int? selX, int? selY, List<Move> legalMoves)
        {
            int startX = (BOARD_SIZE_X - 7 * OFFSET) / 2;
            int startY = (BOARD_SIZE_Y - 7 * OFFSET) / 2;
            int padding = (OFFSET - SIZE) / 2;

            Color woodDark = new(101, 67, 33, 255);
            Color woodLight = new(193, 154, 107, 255);
            Color highlightColor = new(0, 121, 241, 100);

            for (int y = 0; y < Board.Size; y++)
            {
                for (int x = 0; x < Board.Size; x++)
                {
                    int px = startX + x * OFFSET;
                    int py = startY + y * OFFSET;

                    Color tileColor = (x + y) % 2 == 0 ? woodLight : woodDark;
                    Raylib.DrawRectangle(px, py, OFFSET, OFFSET, tileColor);
                    Raylib.DrawRectangleLines(px, py, OFFSET, OFFSET, Color.Black);

                    if (selX == x && selY == y)
                    {
                        Raylib.DrawRectangle(px, py, OFFSET, OFFSET, highlightColor);
                    }

                    foreach (var m in legalMoves)
                    {
                        if (m.ToX == x && m.ToY == y)
                        {
                            Raylib.DrawCircle(px + OFFSET / 2, py + OFFSET / 2, OFFSET / 4, new Color(0, 121, 241, 200));
                            Raylib.DrawText(m.ResultDie.Top.ToString(), px + OFFSET / 2 - 5, py + OFFSET / 2 - 8, 18, Color.White);
                        }
                    }

                    Die? d = board.Grid[x, y];
                    if (d.HasValue)
                    {
                        DrawDie(px + padding, py + padding, SIZE, d.Value);
                    }
                }
            }
        }

        static void DrawDie(int x, int y, int size, Die die)
        {
            Color dieColor = die.Owner == Player.Yellow ? Color.Yellow : Color.Green;
            Rectangle rec = new(x, y, size, size);
            Raylib.DrawRectangleRounded(rec, 0.2f, 10, dieColor);
            Raylib.DrawRectangleRoundedLines(rec, 0.2f, 10, Color.Black);

            int r = size / 10;
            int m = size / 2;
            int q1 = size / 4;
            int q3 = 3 * size / 4;

            switch (die.Top)
            {
                case 1: DrawPips1(x, y, m, r); break;
                case 2: DrawPips2(x, y, q1, q3, r); break;
                case 3: DrawPips3(x, y, m, q1, q3, r); break;
                case 4: DrawPips4(x, y, q1, q3, r); break;
                case 5: DrawPips5(x, y, m, q1, q3, r); break;
                case 6: DrawPips6(x, y, m, q1, q3, r); break;
            }
        }

        private static void DrawPips1(int x, int y, int m, int r) =>
            Raylib.DrawCircle(x + m, y + m, r, Color.Black);

        private static void DrawPips2(int x, int y, int q1, int q3, int r)
        {
            Raylib.DrawCircle(x + q1, y + q1, r, Color.Black);
            Raylib.DrawCircle(x + q3, y + q3, r, Color.Black);
        }

        private static void DrawPips3(int x, int y, int m, int q1, int q3, int r)
        {
            Raylib.DrawCircle(x + m, y + m, r, Color.Black);
            DrawPips2(x, y, q1, q3, r);
        }

        private static void DrawPips4(int x, int y, int q1, int q3, int r)
        {
            Raylib.DrawCircle(x + q1, y + q1, r, Color.Black);
            Raylib.DrawCircle(x + q3, y + q1, r, Color.Black);
            Raylib.DrawCircle(x + q1, y + q3, r, Color.Black);
            Raylib.DrawCircle(x + q3, y + q3, r, Color.Black);
        }

        private static void DrawPips5(int x, int y, int m, int q1, int q3, int r)
        {
            Raylib.DrawCircle(x + m, y + m, r, Color.Black);
            DrawPips4(x, y, q1, q3, r);
        }

        private static void DrawPips6(int x, int y, int m, int q1, int q3, int r)
        {
            Raylib.DrawCircle(x + q1, y + q1, r, Color.Black);
            Raylib.DrawCircle(x + q3, y + q1, r, Color.Black);
            Raylib.DrawCircle(x + q1, y + m, r, Color.Black);
            Raylib.DrawCircle(x + q3, y + m, r, Color.Black);
            Raylib.DrawCircle(x + q1, y + q3, r, Color.Black);
            Raylib.DrawCircle(x + q3, y + q3, r, Color.Black);
        }
    }
}
