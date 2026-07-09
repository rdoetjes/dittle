using System;
using System.Collections.Generic;
using Raylib_cs;

namespace Dittle
{
    class Program
    {
        public const int DefaultAiDepth = 3;
        public const int MaxAiDepth = 6;

        // Game State
        private static Board board = new();
        private static int playersCount = 1;
        private static int aiDepth = DefaultAiDepth;
        private static int? selectedX = null, selectedY = null;
        private static List<Move> legalMoves = [];
        private static Move? lastAiMove = null;
        private static float aiMoveTimer = 0;
        private static bool isAiThinking = false;

        // Timers
        private static float matchTime = 0;
        private static float whiteThinkTime = 0;
        private static float blackThinkTime = 0;

        public static void Main(string[] args)
        {
            Initialize(args);

            while (!Raylib.WindowShouldClose())
            {
                Update();
                Draw();
            }

            Cleanup();
        }

        private static void Initialize(string[] args)
        {
            Graphics.InitializeResourcePath();
            ParseArguments(args);

            Raylib.InitWindow(Graphics.BOARD_SIZE_X, Graphics.BOARD_SIZE_Y, "Dittle");
            Raylib.SetTargetFPS(30);
            Graphics.LoadResources();
        }

        private static void ParseArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-players" && i + 1 < args.Length) playersCount = int.Parse(args[++i]);
                if (args[i] == "-depth" && i + 1 < args.Length) aiDepth = int.Parse(args[++i]);
            }
        }

        private static void Update()
        {
            float deltaTime = Raylib.GetFrameTime();
            if (aiMoveTimer > 0) aiMoveTimer -= deltaTime;

            UpdateTimers(deltaTime);
            HandleInput();

            if (!Rules.IsGameOver(board, out _))
            {
                HandleTurnLogic();
            }
        }

        private static void UpdateTimers(float deltaTime)
        {
            if (!Rules.IsGameOver(board, out _))
            {
                matchTime += deltaTime;
                if (board.CurrentTurn == Player.White) whiteThinkTime += deltaTime;
                else blackThinkTime += deltaTime;
            }
        }

        private static void HandleInput()
        {
            bool mouseHandled = UIHandling.HandleUIInput(ref board, ref board.CurrentTurn, ref aiDepth, ref selectedX, ref selectedY, ref legalMoves, ref lastAiMove, MaxAiDepth);

            if (mouseHandled && board.IsInitialBoard() && board.WhiteHorizontalMoves == 0 && board.BlackHorizontalMoves == 0)
            {
                ResetGame();
            }
        }

        private static void ResetGame()
        {
            AI.CancelAi();
            matchTime = 0;
            whiteThinkTime = 0;
            blackThinkTime = 0;
            isAiThinking = false;
            aiMoveTimer = 0;
        }

        private static void HandleTurnLogic()
        {
            bool isAiTurn = (playersCount == 0) || (playersCount == 1 && board.CurrentTurn == Player.Black);

            if (isAiTurn && aiMoveTimer <= 0 && !isAiThinking)
            {
                TriggerAiMove();
            }
            else if (playersCount > 0 && !isAiThinking)
            {
                UIHandling.HandleBoardInput(board, ref board.CurrentTurn, ref selectedX, ref selectedY, ref legalMoves);
            }
        }

        private static void TriggerAiMove()
        {
            isAiThinking = true;
            var currentBoard = board.Clone();
            var currentDepth = aiDepth;
            var isFastMode = (playersCount == 0);

            _ = AI.PerformAiTurnAsync(currentBoard, currentDepth, (bestMove) => {
                AI.ApplyMove(board, bestMove);
                lastAiMove = bestMove;
                aiMoveTimer = isFastMode ? 0.1f : 1.5f;
                isAiThinking = false;
            });
        }

        private static void Draw()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.DarkGray);

            Graphics.DrawBoard(board, selectedX, selectedY, legalMoves);

            var uiState = new UiState
            {
                Depth = aiDepth,
                CurrentTurn = board.CurrentTurn,
                Board = board,
                MaxAiDepth = MaxAiDepth,
                MatchTime = matchTime,
                WhiteThinkTime = whiteThinkTime,
                BlackThinkTime = blackThinkTime,
                IsAiThinking = isAiThinking
            };

            Graphics.DrawUI(uiState);

            Graphics.DrawAiMoveHighlight(lastAiMove, aiMoveTimer);

            Raylib.EndDrawing();
        }

        private static void Cleanup()
        {
            Graphics.UnloadResources();
            Raylib.CloseWindow();
        }
    }
}
