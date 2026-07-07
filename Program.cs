using System;
using System.Collections.Generic;
using Raylib_cs;
using System.Numerics;

namespace Dittle
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--test")
            {
                RunManualTests();
                return;
            }

            int playersCount = 1;
            int aiDepth = 3;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-players" && i + 1 < args.Length) playersCount = int.Parse(args[++i]);
                if (args[i] == "-depth" && i + 1 < args.Length) aiDepth = int.Parse(args[++i]);
            }

            Raylib.InitWindow(800, 800, "Dittle");
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
                        int x = (int)(mouse.X / 100) - 1;
                        int y = (int)(mouse.Y / 100) - 1;

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
                    Raylib.DrawText($"WINNER: {w}", 300, 400, 40, Color.Red);
                }

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }

        static void DrawBoard(Board board, int? selX, int? selY, List<Move> legalMoves)
        {
            int offset = 100;
            int size = 80;

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
                    if (d.HasValue)
                    {
                        Color c = d.Value.Owner == Player.Yellow ? Color.Yellow : Color.Green;
                        Raylib.DrawRectangle(px + 10, py + 10, size - 20, size - 20, c);
                        Raylib.DrawText(d.Value.Top.ToString(), px + 30, py + 25, 30, Color.Black);
                    }
                }
            }
        }

        private static void RunManualTests()
        {
            Console.WriteLine("Running Manual Rules Validation...");
            Board board = new Board();
            for (int y = 0; y < 7; y++) for (int x = 0; x < 7; x++) board.Grid[x, y] = null;

            // Test 1: Adjacency Requirement
            board.Grid[3, 3] = new Die(Player.Yellow, 6, 4);
            var moves = Rules.GetAllLegalMoves(board, Player.Yellow);
            bool hasLongJump = moves.Exists(m => Math.Abs(m.FromX - m.ToX) > 1 || Math.Abs(m.FromY - m.ToY) > 1);
            Console.WriteLine("Test 1 (No adjacent die -> No jump): " + (!hasLongJump ? "PASSED" : "FAILED"));

            // Test 2: Valid Jump
            board.Grid[3, 2] = new Die(Player.Green, 1, 4);
            moves = Rules.GetAllLegalMoves(board, Player.Yellow);
            bool hasValidJump = moves.Exists(m => m.ToX == 3 && m.ToY == 1);
            Console.WriteLine("Test 2 (Adjacent die -> Valid jump): " + (hasValidJump ? "PASSED" : "FAILED"));

            // Test 3: Tight Cluster
            board.Grid[3, 1] = new Die(Player.Green, 1, 4);
            moves = Rules.GetAllLegalMoves(board, Player.Yellow);
            bool jumpOverCluster = moves.Exists(m => m.ToX == 3 && m.ToY == 0);
            Console.WriteLine("Test 3 (Tight cluster -> No jump): " + (!jumpOverCluster ? "PASSED" : "FAILED"));

            // Test 4: Die Rolling sequence validation
            // Initial: Top 6, Front 4.
            Die d = new Die(Player.Yellow, 6, 4);

            // Roll Forward: 6 top, 4 front. Rolls over the back edge (3).
            // Top becomes 3. Front becomes 6.
            Die d3 = d.Tilted(0, -1);
            bool step1 = d3.Top == 3;

            // Roll Right: 3 top, 6 front.
            // In a standard die: if 3 top, 6 front, then 5 is left and 2 is right.
            // Rolls over right edge (2). Left side (5) becomes the new Top.
            Die d5 = d3.Tilted(1, 0);
            bool step2 = d5.Top == 5;

            Console.WriteLine("Test 4d (6 -> 3 -> 5 sequence): " + (step1 && step2 ? "PASSED" : "FAILED") + " (Values: " + d3.Top + ", " + d5.Top + ")");

            // Test 5: Win Conditions
            RunWinConditionTests();

            Console.WriteLine("Validation Complete.");
        }

        private static void RunWinConditionTests()
        {
            Board board = new Board();
            for (int y = 0; y < 7; y++) for (int x = 0; x < 7; x++) board.Grid[x, y] = null;

            int[] greenVals = { 5, 6, 5, 5, 1, 6, 1 };
            for (int i = 0; i < 7; i++) board.Grid[i, 6] = new Die(Player.Green, greenVals[i], 1);

            int[] yellowVals = { 2, 6, 6, 5, 5, 6 };
            for (int i = 0; i < 6; i++) board.Grid[i, 0] = new Die(Player.Yellow, yellowVals[i], 1);

            bool isOver = Rules.IsGameOver(board, out Player? winner);
            Console.WriteLine("Test 5a (One reached 7, compare scores): " + (isOver && winner == Player.Yellow ? "PASSED" : "FAILED") + " (Winner: " + winner + ")");

            int scoreForYellow = Rules.Evaluate(board, Player.Yellow);
            int scoreForGreen = Rules.Evaluate(board, Player.Green);
            Console.WriteLine("Test 5b (AI Evaluation handles score gap): " + (scoreForYellow > scoreForGreen ? "PASSED" : "FAILED"));
        }
    }
}
