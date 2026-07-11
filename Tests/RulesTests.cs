using Xunit;
using Dittle;
using System.Collections.Generic;

namespace Dittle.Tests
{
    public class RulesTests
    {
        [Fact]
        public void InitialBoard_HasCorrectLegalMovesCount()
        {
            var board = new Board();
            var whiteMoves = Rules.GetAllLegalMoves(board, Player.White);
            
            // Each of the 7 white dice can tilt forward (1 move each)
            Assert.Equal(7, whiteMoves.Count);
        }

        [Fact]
        public void TiltMove_UpdatesDieFacesCorrectly()
        {
            var board = new Board();
            var die = board.Grid[0, 6].Value;
            
            // Tilt forward (Up for White, dy=-1)
            var tilted = die.Tilted(0, -1);
            
            // For White initial die (6, 4):
            // Top: 6, Bottom: 1, Front: 4, Back: 3, Left: 5, Right: 2
            // Tilted Up (dy=-1): New Top = Old Back (3)
            Assert.Equal(3, tilted.Top);
        }

        [Fact]
        public void SimpleJump_IsDetected()
        {
            var board = new Board();
            ClearBoard(board);

            // D X .
            board.Grid[0, 2] = new Die(Player.White, 6, 4);
            board.Grid[1, 2] = new Die(Player.Black, 6, 3);

            var moves = Rules.GetAllLegalMoves(board, Player.White);
            
            // Should find a jump landing at (2,2)
            Assert.Contains(moves, m => m.ToX == 2 && m.ToY == 2);
        }

        [Fact]
        public void IllegalTightClusterJump_IsBlocked()
        {
            var board = new Board();
            ClearBoard(board);

            // D X X .
            board.Grid[0, 2] = new Die(Player.White, 6, 4);
            board.Grid[1, 2] = new Die(Player.Black, 6, 3);
            board.Grid[2, 2] = new Die(Player.Black, 6, 3);

            var moves = Rules.GetAllLegalMoves(board, Player.White);
            
            // Should NOT be able to jump to (3,2) because X X is a tight cluster
            Assert.DoesNotContain(moves, m => m.ToX == 3 && m.ToY == 2);
        }

        [Fact]
        public void LJump_IsDetected()
        {
            var board = new Board();
            ClearBoard(board);
            
            // Path: 
            // 1. Tilt from (0,4) to (0,3) [gap]
            // 2. Jump Up over (0,2) to (0,1) [gap]
            // 3. Jump Right over (1,1) to (2,1) [landing]
            
            board.Grid[0, 4] = new Die(Player.White, 6, 4);
            board.Grid[0, 2] = new Die(Player.Black, 6, 3);
            board.Grid[1, 1] = new Die(Player.Black, 6, 3);
            
            var moves = Rules.GetAllLegalMoves(board, Player.White);
            Assert.Contains(moves, m => m.ToX == 2 && m.ToY == 1);
        }

        [Fact]
        public void ForcedForward_Rule_Works()
        {
            var board = new Board();
            ClearBoard(board);
            board.WhiteHorizontalMoves = 4;
            
            board.Grid[3, 3] = new Die(Player.White, 6, 4);
            
            var moves = Rules.GetAllLegalMoves(board, Player.White);
            
            // Should only contain forward moves (dy < 0 for White)
            Assert.NotEmpty(moves);
            Assert.All(moves, m => Assert.True(m.ToY < m.FromY));
        }

        [Fact]
        public void GameOver_And_Scoring()
        {
            var board = new Board();
            ClearBoard(board);

            // Fill White's goal row (y=0) with 7 white dice
            int expectedScore = 0;
            for(int x=0; x<Board.Size; x++)
            {
                int topVal = (x % 6) + 1;
                board.Grid[x, 0] = new Die(Player.White, topVal, 4);
                expectedScore += topVal;
            }

            bool isOver = Rules.IsGameOver(board, out Player? winner);
            Rules.CalculateScores(board, out int scoreWhite, out int _);

            Assert.True(isOver);
            Assert.Equal(Player.White, winner);
            Assert.Equal(expectedScore, scoreWhite);
        }

        [Fact]
        public void Evaluate_PrefersWinningPosition()
        {
            var board = new Board();
            ClearBoard(board);

            // Position A: White is one move away from winning
            board.Grid[0, 1] = new Die(Player.White, 6, 4);
            int scoreA = Rules.Evaluate(board, Player.White);

            // Position B: White is far away
            ClearBoard(board);
            board.Grid[0, 5] = new Die(Player.White, 6, 4);
            int scoreB = Rules.Evaluate(board, Player.White);

            Assert.True(scoreA > scoreB, "AI should value being closer to the goal row higher.");
        }

        [Fact]
        public void LongJump_WithGaps_IsLegal()
        {
            var board = new Board();
            ClearBoard(board);

            // Layout: D X . X .
            // Should be able to jump to the second gap
            board.Grid[0, 4] = new Die(Player.White, 6, 4);
            board.Grid[0, 3] = new Die(Player.Black, 6, 3); // First die
            board.Grid[0, 1] = new Die(Player.Black, 6, 3); // Second die (after a gap at 0,2)

            var moves = Rules.GetAllLegalMoves(board, Player.White);
            
            // Landing at the very end (0,0)
            Assert.Contains(moves, m => m.ToX == 0 && m.ToY == 0);
        }

        private void ClearBoard(Board board)
        {
            for(int y=0; y<Board.Size; y++)
                for(int x=0; x<Board.Size; x++)
                    board.Grid[x, y] = null;
        }
    }
}
