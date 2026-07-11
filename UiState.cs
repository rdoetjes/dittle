using System.Collections.Generic;

namespace Dittle
{
    public struct UiState
    {
        public int Depth;
        public Player CurrentTurn;
        public Board Board;
        public int MaxAiDepth;
        public float MatchTime;
        public float WhiteThinkTime;
        public float BlackThinkTime;
        public int ScoreWhite;
        public int ScoreBlack;
        public bool IsAiThinking;
        public int? SelectedX;
        public int? SelectedY;
        public List<Move> LegalMoves;
        public Move? LastAiMove;
    }
}
