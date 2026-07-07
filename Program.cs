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

            Board board = new Board();
            Player currentPlayer = Player.White;
            int? selectedX = null, selectedY = null;
            List<Move> legalMoves = new List<Move>();

            Move? lastAiMove = null;
            float aiMoveTimer = 0;
            const float AI_MOVE_DISPLAY_TIME = 1.5f;

            while (!Raylib.WindowShouldClose())
            {
                float deltaTime = Raylib.GetFrameTime();
                if (aiMoveTimer > 0) aiMoveTimer -= deltaTime;

                // UI Rects
                int uiBottomY = BOARD_SIZE_Y - 80;
                Rectangle restartRect = new Rectangle(BOARD_SIZE_X - 120, 10, 100, 30); // Top Right
                Rectangle downRect = new Rectangle(250, uiBottomY, 30, 40);
                Rectangle upRect = new Rectangle(330, uiBottomY, 30, 40);

                bool mouseHandledByUI = false;
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    Vector2 mPos = Raylib.GetMousePosition();
                    if (Raylib.CheckCollisionPointRec(mPos, restartRect))
                    {
                        board = new Board();
                        currentPlayer = Player.White;
                        selectedX = null; selectedY = null;
                        legalMoves.Clear();
                        lastAiMove = null;
                        mouseHandledByUI = true;
                    }
                    else if (Raylib.CheckCollisionPointRec(mPos, downRect))
                    {
                        if (aiDepth > 1) aiDepth--;
                        mouseHandledByUI = true;
                    }
                    else if (Raylib.CheckCollisionPointRec(mPos, upRect))
                    {
                        if (aiDepth < 6) aiDepth++;
                        mouseHandledByUI = true;
                    }
                }

                if (!Rules.IsGameOver(board, out _))
                {
                    if (playersCount == 1 && currentPlayer == Player.Black && aiMoveTimer <= 0)
                    {
                        Move? best = AI.GetBestMove(board, Player.Black, aiDepth);
                        if (best.HasValue)
                        {
                            lastAiMove = best.Value;
                            AI.ApplyMove(board, best.Value);
                            aiMoveTimer = AI_MOVE_DISPLAY_TIME;
                        }
                        currentPlayer = Player.White;
                    }
                    else if (!mouseHandledByUI && Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
                        Vector2 mouse = Raylib.GetMousePosition();
                        int startX = (BOARD_SIZE_X - 7 * OFFSET) / 2;
                        int startY = (BOARD_SIZE_Y - 7 * OFFSET) / 2;
                        int x = (int)((mouse.X - startX) / OFFSET);
                        int y = (int)((mouse.Y - startY) / OFFSET);

                        if (board.IsInBounds(x, y))
                        {
                            if (selectedX == null)
                            {
                                Die? d = board.Grid[x, y];
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
                                    currentPlayer = (currentPlayer == Player.White) ? Player.Black : Player.White;
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

                // Draw UI
                Raylib.DrawRectangleRec(restartRect, Color.LightGray);
                Raylib.DrawRectangleLinesEx(restartRect, 2, Color.Black);
                Raylib.DrawText("RESTART", (int)restartRect.X + 10, (int)restartRect.Y + 8, 16, Color.Black);

                Raylib.DrawText("AI DEPTH:", 150, uiBottomY + 12, 18, Color.White);
                Raylib.DrawRectangleRec(downRect, Color.LightGray);
                Raylib.DrawText("-", (int)downRect.X + 10, (int)downRect.Y + 5, 30, Color.Black);
                Raylib.DrawText(aiDepth.ToString(), 295, uiBottomY + 10, 24, Color.Yellow);
                Raylib.DrawRectangleRec(upRect, Color.LightGray);
                Raylib.DrawText("+", (int)upRect.X + 7, (int)upRect.Y + 5, 30, Color.Black);

                if (aiMoveTimer > 0 && lastAiMove.HasValue)
                {
                    int startX = (BOARD_SIZE_X - 7 * OFFSET) / 2;
                    int startY = (BOARD_SIZE_Y - 7 * OFFSET) / 2;
                    int fx = startX + lastAiMove.Value.FromX * OFFSET;
                    int fy = startY + lastAiMove.Value.FromY * OFFSET;
                    int tx = startX + lastAiMove.Value.ToX * OFFSET;
                    int ty = startY + lastAiMove.Value.ToY * OFFSET;
                    Raylib.DrawRectangleLinesEx(new(fx, fy, OFFSET, OFFSET), 3, Color.Red);
                    Raylib.DrawLineEx(new(fx + OFFSET / 2, fy + OFFSET / 2), new(tx + OFFSET / 2, ty + OFFSET / 2), 3, Color.Red);
                    Raylib.DrawRectangleLinesEx(new(tx, ty, OFFSET, OFFSET), 4, Color.Orange);
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

                    if (selX == x && selY == y) Raylib.DrawRectangle(px, py, OFFSET, OFFSET, highlightColor);

                    foreach (var m in legalMoves)
                    {
                        if (m.ToX == x && m.ToY == y)
                        {
                            Raylib.DrawCircle(px + OFFSET / 2, py + OFFSET / 2, OFFSET / 4, new Color(0, 121, 241, 200));
                            Raylib.DrawText(m.ResultDie.Top.ToString(), px + OFFSET / 2 - 5, py + OFFSET / 2 - 8, 18, Color.White);
                        }
                    }

                    Die? d = board.Grid[x, y];
                    if (d.HasValue) DrawDie(px + padding, py + padding, SIZE, d.Value);
                }
            }
        }

        static void DrawDie(int x, int y, int size, Die die)
        {
            Color dieColor = die.Owner == Player.White ? Color.White : Color.Black;
            Color pipColor = die.Owner == Player.White ? Color.Black : Color.White;
            Rectangle rec = new(x, y, size, size);
            Raylib.DrawRectangleRounded(rec, 0.2f, 10, dieColor);
            Raylib.DrawRectangleRoundedLines(rec, 0.2f, 10, Color.Gray);
            int r = size / 10;
            int m = size / 2;
            int q1 = size / 4;
            int q3 = 3 * size / 4;
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
