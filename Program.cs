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
        private static UiState state = new()
        {
            Board = NewBoard(),
            Depth = DefaultAiDepth,
            MaxAiDepth = MaxAiDepth,
            LegalMoves = []
        };
        private static Board NewBoard() => new();

        private static int playersCount = 1;
        private static float aiMoveTimer = 0;

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
                if (args[i] == "-depth" && i + 1 < args.Length) state.Depth = int.Parse(args[++i]);
            }
        }

        private static void UpdateUiState()
        {
            Rules.CalculateScores(state.Board, out int scoreWhite, out int scoreBlack);
            state.ScoreWhite = scoreWhite;
            state.ScoreBlack = scoreBlack;
            state.CurrentTurn = state.Board.CurrentTurn;
        }

        private static void Update()
        {
            float deltaTime = Raylib.GetFrameTime();
            if (aiMoveTimer > 0) aiMoveTimer -= deltaTime;

            UpdateTimers(deltaTime);
            HandleInput();
            UpdateUiState();

            if (!Rules.IsGameOver(state.Board, out _))
            {
                HandleTurnLogic();
            }
        }

        private static void UpdateTimers(float deltaTime)
        {
            if (!Rules.IsGameOver(state.Board, out _))
            {
                state.MatchTime += deltaTime;
                if (state.Board.CurrentTurn == Player.White) state.WhiteThinkTime += deltaTime;
                else state.BlackThinkTime += deltaTime;
            }
        }

        private static void HandleInput()
        {
            bool mouseHandled = UIHandling.HandleUIInput(ref state);

            if (mouseHandled && state.Board.IsInitialBoard() && state.Board.WhiteHorizontalMoves == 0 && state.Board.BlackHorizontalMoves == 0)
            {
                ResetGame();
            }
        }

        private static void ResetGame()
        {
            AI.CancelAi();
            AI.Reseed();
            state.MatchTime = 0;
            state.WhiteThinkTime = 0;
            state.BlackThinkTime = 0;
            state.IsAiThinking = false;
            aiMoveTimer = 0;
        }

        private static void HandleTurnLogic()
        {
            bool isAiTurn = (playersCount == 0) || (playersCount == 1 && state.Board.CurrentTurn == Player.Black);

            if (isAiTurn && aiMoveTimer <= 0 && !state.IsAiThinking)
            {
                TriggerAiMove();
            }
            else if (playersCount > 0 && !state.IsAiThinking)
            {
                UIHandling.HandleBoardInput(ref state);
            }
        }

        private static void TriggerAiMove()
        {
            state.IsAiThinking = true;
            var currentBoard = state.Board.Clone();
            var currentDepth = state.Depth;
            var isFastMode = (playersCount == 0);

            _ = AI.PerformAiTurnAsync(currentBoard, currentDepth, (bestMove) => {
                AI.ApplyMove(state.Board, bestMove);
                state.LastAiMove = bestMove;
                aiMoveTimer = isFastMode ? 0.1f : 1.5f;
                state.IsAiThinking = false;
            });
        }

        private static void Draw()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.DarkGray);

            Graphics.DrawBoard(state.Board, state.SelectedX, state.SelectedY, state.LegalMoves);

            Graphics.DrawUI(state);

            Graphics.DrawAiMoveHighlight(state.LastAiMove, aiMoveTimer);

            Raylib.EndDrawing();
        }

        private static void Cleanup()
        {
            Graphics.UnloadResources();
            Raylib.CloseWindow();
        }
    }
}
