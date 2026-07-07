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

            while (!Raylib.WindowShouldClose())
            {
                if (Rules.IsGameOver(board, out Player? winner))
                {
                }
                else if (playersCount == 1 && currentPlayer == Player.Green)
                {
                    Move? best = AI.GetBestMove(board, Player.Green, aiDepth);
                    if (best.HasValue)
                    {
                        AI.ApplyMove(board, best.Value);
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
                            if (selectedX == null)
                            {
                                if (board.Grid[x, y].HasValue && board.Grid[x, y].Value.Owner == currentPlayer)
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

            Color woodDark = new Color(101, 67, 33, 255);
            Color woodLight = new Color(193, 154, 107, 255);
            Color highlightColor = new Color(0, 121, 241, 100);

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
            Color dotColor = Color.Black;

            Rectangle rec = new Rectangle(x, y, size, size);
            Raylib.DrawRectangleRounded(rec, 0.2f, 10, dieColor);
            Raylib.DrawRectangleRoundedLines(rec, 0.2f, 10, Color.Black);

            int val = die.Top;
            int r = size / 10;
            int m = size / 2;
            int q1 = size / 4;
            int q3 = 3 * size / 4;

            if (val == 1)
            {
                Raylib.DrawCircle(x + m, y + m, r, dotColor);
            }
            else if (val == 2)
            {
                Raylib.DrawCircle(x + q1, y + q1, r, dotColor);
                Raylib.DrawCircle(x + q3, y + q3, r, dotColor);
            }
            else if (val == 3)
            {
                Raylib.DrawCircle(x + m, y + m, r, dotColor);
                Raylib.DrawCircle(x + q1, y + q1, r, dotColor);
                Raylib.DrawCircle(x + q3, y + q3, r, dotColor);
            }
            else if (val == 4)
            {
                Raylib.DrawCircle(x + q1, y + q1, r, dotColor);
                Raylib.DrawCircle(x + q3, y + q1, r, dotColor);
                Raylib.DrawCircle(x + q1, y + q3, r, dotColor);
                Raylib.DrawCircle(x + q3, y + q3, r, dotColor);
            }
            else if (val == 5)
            {
                Raylib.DrawCircle(x + m, y + m, r, dotColor);
                Raylib.DrawCircle(x + q1, y + q1, r, dotColor);
                Raylib.DrawCircle(x + q3, y + q1, r, dotColor);
                Raylib.DrawCircle(x + q1, y + q3, r, dotColor);
                Raylib.DrawCircle(x + q3, y + q3, r, dotColor);
            }
            else if (val == 6)
            {
                Raylib.DrawCircle(x + q1, y + q1, r, dotColor);
                Raylib.DrawCircle(x + q3, y + q1, r, dotColor);
                Raylib.DrawCircle(x + q1, y + m, r, dotColor);
                Raylib.DrawCircle(x + q3, y + m, r, dotColor);
                Raylib.DrawCircle(x + q1, y + q3, r, dotColor);
                Raylib.DrawCircle(x + q3, y + q3, r, dotColor);
            }
        }
    }
}
