using System;

namespace Dittle
{
    public enum Player { Yellow, Green }

    public struct Die
    {
        public Player Owner;
        public int Top;
        public int Front; // Number facing the player (for orientation)

        public Die(Player owner, int top, int front)
        {
            Owner = owner;
            Top = top;
            Front = front;
        }

        // Returns the number on the right side based on Top and Front
        public int GetRight()
        {
            // Standard right-handed dice layout
            // 1 opposite 6, 2 opposite 5, 3 opposite 4
            // If Top=1, Front=2, then Right=3
            if (Top == 1) return Front == 2 ? 3 : (Front == 3 ? 5 : (Front == 5 ? 4 : 2));
            if (Top == 2) return Front == 1 ? 4 : (Front == 4 ? 6 : (Front == 6 ? 3 : 1));
            if (Top == 3) return Front == 1 ? 2 : (Front == 2 ? 6 : (Front == 6 ? 5 : 1));
            if (Top == 4) return Front == 1 ? 5 : (Front == 5 ? 6 : (Front == 6 ? 2 : 1));
            if (Top == 5) return Front == 1 ? 3 : (Front == 3 ? 6 : (Front == 6 ? 4 : 1));
            if (Top == 6) return Front == 2 ? 4 : (Front == 4 ? 5 : (Front == 5 ? 3 : 2));
            return 0;
        }

        public Die Tilted(int dx, int dy)
        {
            int newTop, newFront;
            int right = GetRight();
            int back = 7 - Front;
            int left = 7 - right;
            int bottom = 7 - Top;

            if (dy == -1) // Forward (Green moves -y, Yellow moves +y usually, but "Forward" is relative)
            {
                newTop = Front;
                newFront = bottom;
            }
            else if (dy == 1) // Backward
            {
                newTop = back;
                newFront = Top;
            }
            else if (dx == 1) // Right
            {
                newTop = left;
                newFront = Front;
            }
            else if (dx == -1) // Left
            {
                newTop = right;
                newFront = Front;
            }
            else
            {
                newTop = Top;
                newFront = Front;
            }

            return new Die(Owner, newTop, newFront);
        }
    }

    public class Board
    {
        public const int Size = 7;
        public Die?[,] Grid = new Die?[Size, Size];

        public Board()
        {
            // Setup dice: 6 on top, 3 facing the player
            // Player Yellow (Bottom, Y=6)
            for (int x = 0; x < Size; x++)
            {
                Grid[x, Size - 1] = new Die(Player.Yellow, 6, 3);
            }
            // Player Green (Top, Y=0)
            for (int x = 0; x < Size; x++)
            {
                Grid[x, 0] = new Die(Player.Green, 6, 3);
            }
        }

        public Board Clone()
        {
            Board b = new Board();
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                    b.Grid[x, y] = this.Grid[x, y];
            return b;
        }

        public bool IsInBounds(int x, int y) => x >= 0 && x < Size && y >= 0 && y < Size;

        public Die?[,] GetBoard() => Grid;
    }
}
