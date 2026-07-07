using System;

namespace Dittle
{
    public enum Player { Yellow, Green }

    public struct Die
    {
        public Player Owner;
        // 0: Top, 1: Bottom, 2: Front (Up screen), 3: Back (Down screen), 4: Left, 5: Right
        public int[] Faces;

        public int Top => Faces != null ? Faces[0] : 0;
        public int Front => Faces != null ? Faces[2] : 0;

        public Die(Player owner, int top, int front)
        {
            Owner = owner;
            Faces = new int[6];
            Faces[0] = top;
            Faces[1] = 7 - top;
            Faces[2] = front;
            Faces[3] = 7 - front;

            int right = 0;
            if (top == 6)
            {
                if (front == 4) right = 2;
                else if (front == 2) right = 3;
                else if (front == 3) right = 5;
                else if (front == 5) right = 4;
            }
            else if (top == 3)
            {
                if (front == 6) right = 2;
                else if (front == 2) right = 1;
                else if (front == 1) right = 5;
                else if (front == 5) right = 6;
            }
            else if (top == 5)
            {
                if (front == 6) right = 3;
                else if (front == 3) right = 1;
                else if (front == 1) right = 4;
                else if (front == 4) right = 6;
            }
            else
            {
                right = CalculateRight(top, front);
            }

            Faces[5] = right;
            Faces[4] = 7 - right;
        }

        private static int CalculateRight(int top, int front)
        {
            if (top == 1) return front == 2 ? 3 : (front == 3 ? 5 : (front == 5 ? 4 : 2));
            if (top == 2) return front == 1 ? 4 : (front == 4 ? 6 : (front == 6 ? 3 : 1));
            if (top == 3) return front == 1 ? 2 : (front == 2 ? 6 : (front == 6 ? 5 : 1));
            if (top == 4) return front == 1 ? 5 : (front == 5 ? 6 : (front == 6 ? 2 : 1));
            if (top == 5) return front == 1 ? 3 : (front == 3 ? 6 : (front == 6 ? 4 : 1));
            if (top == 6) return front == 2 ? 4 : (front == 4 ? 5 : (front == 5 ? 3 : 2));
            return 0;
        }

        public Die Tilted(int dx, int dy)
        {
            if (Faces == null) return this;
            int[] newFaces = new int[6];
            Array.Copy(Faces, newFaces, 6);

            if (dy == -1) // Tilt UP screen
            {
                newFaces[0] = Faces[3]; // Top = old Back
                newFaces[1] = Faces[2]; // Bottom = old Front
                newFaces[2] = Faces[0]; // Front = old Top
                newFaces[3] = Faces[1]; // Back = old Bottom
            }
            else if (dy == 1) // Tilt DOWN screen
            {
                newFaces[0] = Faces[2]; // Top = old Front
                newFaces[1] = Faces[3]; // Bottom = old Back
                newFaces[2] = Faces[1]; // Front = old Bottom
                newFaces[3] = Faces[0]; // Back = old Top
            }
            else if (dx == 1) // Tilt RIGHT
            {
                newFaces[0] = Faces[4]; // Top = old Left
                newFaces[1] = Faces[5]; // Bottom = old Right
                newFaces[4] = Faces[1]; // Left = old Bottom
                newFaces[5] = Faces[0]; // Right = old Top
            }
            else if (dx == -1) // Tilt LEFT
            {
                newFaces[0] = Faces[5]; // Top = old Right
                newFaces[1] = Faces[4]; // Bottom = old Left
                newFaces[4] = Faces[0]; // Left = old Top
                newFaces[5] = Faces[1]; // Right = old Bottom
            }

            Die d = this;
            d.Faces = newFaces;
            return d;
        }
    }

    public class Board
    {
        public const int Size = 7;
        public Die?[,] Grid = new Die?[Size, Size];

        public Board()
        {
            for (int x = 0; x < Size; x++)
            {
                Grid[x, 6] = new Die(Player.Yellow, 6, 4);
            }
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
