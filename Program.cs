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
            int? selectedX = null, selectedY = null;
            List<Move> legalMoves = [];
            Move? lastAiMove = null;
            float aiMoveTimer = 0;
            bool isAiThinking = false;
            
            float matchTime = 0;
            float whiteThinkTime = 0;
            float blackThinkTime = 0;

            while (!Raylib.WindowShouldClose())
            {
                float deltaTime = Raylib.GetFrameTime();
                if (aiMoveTimer > 0) aiMoveTimer -= deltaTime;
                
                if (!Rules.IsGameOver(board, out _))
                {
                    matchTime += deltaTime;
                    if (board.CurrentTurn == Player.White) whiteThinkTime += deltaTime;
                    else blackThinkTime += deltaTime;
                }

                bool mouseHandled = UIHandling.HandleUIInput(ref board, ref board.CurrentTurn, ref aiDepth, ref selectedX, ref selectedY, ref legalMoves, ref lastAiMove, MaxAiDepth);
                if (mouseHandled) 
                {
                    // Reset times on restart
                    if (board.IsInitialBoard() && board.WhiteHorizontalMoves == 0 && board.BlackHorizontalMoves == 0)
                    {
                        matchTime = 0;
                        whiteThinkTime = 0;
                        blackThinkTime = 0;
                        isAiThinking = false;
                    }
                }

                if (!Rules.IsGameOver(board, out _))
                {
                    bool isAiTurn = (playersCount == 0) || (playersCount == 1 && board.CurrentTurn == Player.Black);

                    if (isAiTurn && aiMoveTimer <= 0 && !isAiThinking)
                    {
                        isAiThinking = true;
                        var currentBoard = board.Clone();
                        var currentDepth = aiDepth;
                        var isFastMode = (playersCount == 0);
                        
                        _ = AI.PerformAiTurnAsync(currentBoard, currentDepth, (bestMove) => {
                            AI.ApplyMove(board, bestMove);
                            lastAiMove = bestMove;
                            board.CurrentTurn = (board.CurrentTurn == Player.White) ? Player.Black : Player.White;
                            aiMoveTimer = isFastMode ? 0.1f : 1.5f;
                            isAiThinking = false;
                        });
                    }
                    else if (!mouseHandled && playersCount > 0 && !isAiThinking)
                    {
                        Player prevTurn = board.CurrentTurn;
                        UIHandling.HandleBoardInput(board, ref board.CurrentTurn, ref selectedX, ref selectedY, ref legalMoves);
                        if (prevTurn != board.CurrentTurn)
                        {
                             // Player just moved
                        }
                    }
                }

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.DarkGray);
                Graphics.DrawBoard(board, selectedX, selectedY, legalMoves);
                Graphics.DrawUI(aiDepth, board.CurrentTurn, board, MaxAiDepth, matchTime, whiteThinkTime, blackThinkTime, isAiThinking);
                Graphics.DrawAiMoveHighlight(lastAiMove, aiMoveTimer);
                Raylib.EndDrawing();
            }

            Graphics.UnloadResources();
            Raylib.CloseWindow();
        }
    }
}
