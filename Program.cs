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

        public static void DrawUI(Board board, Player currentPlayer)
        {
            Raylib.DrawText($"Turn: {currentPlayer}", 10, 10, 20, Color.White);
            if (Rules.IsGameOver(board, out Player? w))
            {
                Raylib.DrawText($"WINNER: {w}", BOARD_SIZE_X / 2 - 200, BOARD_SIZE_Y / 2, 40, Color.Red);
            }
        }

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

            Board board = new();
            Player currentPlayer = Player.Yellow;
            int? selectedX = null, selectedY = null;
            List<Move> legalMoves = [];

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
                        int x = (int)(mouse.X / OFFSET) - 1;
                        int y = (int)(mouse.Y / OFFSET) - 1;

                        if (board.IsInBounds(x, y))
                        {
                            if (selectedX == null)
                            {
                                Die? d = board.Grid[x, y];
                                if (d.HasValue && d.Value.Owner == currentPlayer)
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
                DrawUI(board, currentPlayer);

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }

        static void DrawBoard(Board board, int? selX, int? selY, List<Move> legalMoves)
        {
            int offset = OFFSET;
            int size = SIZE;

            for (int y = 0; y < Board.Size; y++)
            {
                for (int x = 0; x < Board.Size; x++)
                {
                    int px = (x + 1) * offset;
                    int py = (y + 1) * offset;

                    Raylib.DrawRectangle(px, py, size, size, Color.Beige);
                    Raylib.DrawRectangleLines(px, py, size, size, Color.Brown);

                    if (selX == x && selY == y)
                        Raylib.DrawRectangleLinesEx(new Rectangle(px, py, size, size), 3, Color.Blue);

                    foreach (var m in legalMoves)
                    {
                        if (m.ToX == x && m.ToY == y)
                            Raylib.DrawCircle(px + size / 2, py + size / 2, 5, Color.Blue);
                    }

                    Die? d = board.Grid[x, y];
                    if (d is not null && d.HasValue)
                    {
                        Color c = d.Value.Owner == Player.Yellow ? Color.Yellow : Color.Green;
                        Raylib.DrawRectangle(px + 10, py + 10, size - 20, size - 20, c);
                        Raylib.DrawText(d.Value.Top.ToString(), px + 20, py + 18, 20, Color.Black);
                    }
                }
            }
        }
    }
}
