using System.Collections.Generic;
using Raylib_cs;

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

                bool mouseHandled = UIHandling.HandleUIInput(ref board, ref currentPlayer, ref aiDepth, ref selectedX, ref selectedY, ref legalMoves, ref lastAiMove, MaxAiDepth);

                if (!Rules.IsGameOver(board, out _))
                {
                    bool isAiTurn = (playersCount == 0) || (playersCount == 1 && currentPlayer == Player.Black);

                    if (isAiTurn && aiMoveTimer <= 0)
                    {
                        AI.PerformAiTurn(board, aiDepth, ref lastAiMove, ref aiMoveTimer, ref currentPlayer);
                        if (playersCount == 0) aiMoveTimer = 0.1f;
                    }
                    else if (!mouseHandled && playersCount > 0)
                    {
                        UIHandling.HandleBoardInput(board, ref currentPlayer, ref selectedX, ref selectedY, ref legalMoves);
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
    }
}
