






using System;

namespace Dittle
{
    public enum Player { White, Black }

    public struct Die
    {
        public Player Owner;
        // 0: Top, 1: Bottom, 2: Front (Up screen), 3: Back (Down screen), 4: Left, 5: Right
        public int[] Faces;

        public int Top => (Faces != null && Faces.Length > 0) ? Faces[0] : 0;
        public int Front => (Faces != null && Faces.Length > 2) ? Faces[2] : 0;

        public Die(Player owner, int top, int front)
        {
            Owner = owner;

            Faces =
            [
                top,
                7 - top,
                front,
                7 - front,
                0, // filled below
                0  // filled below
            ];

            int right = CalculateRightFace(top, front);

            Faces[5] = right;
            Faces[4] = 7 - right;
        }

        public static int CalculateRightFace(int top, int front)
        {
            return (top, front) switch
            {
                (6, 4) => 2,
                (6, 2) => 3,
                (6, 3) => 5,
                (6, 5) => 4,

                (3, 6) => 2,
                (3, 2) => 1,
                (3, 1) => 5,
                (3, 5) => 6,

                (5, 6) => 3,
                (5, 3) => 1,
                (5, 1) => 4,
                (5, 4) => 6,

                _ => CalculateRight(top, front)
            };
        }

        public static int CalculateRight(int top, int front)
        {
            if (top == 1) return front == 2 ? 3 : (front == 3 ? 5 : (front == 5 ? 4 : 2));
            if (top == 2) return front == 1 ? 4 : (front == 4 ? 6 : (front == 6 ? 3 : 1));
            if (top == 3) return front == 1 ? 2 : (front == 2 ? 6 : (front == 6 ? 5 : 1));
            if (top == 4) return front == 1 ? 5 : (front == 5 ? 6 : (front == 6 ? 2 : 1));
            if (top == 5) return front == 1 ? 3 : (front == 3 ? 6 : (front == 6 ? 4 : 1));
            if (top == 6) return front == 2 ? 4 : (front == 4 ? 5 : (front == 5 ? 3 : 2));
            return 0;
        }

        public int GetForwardValue()
        {
            if (Faces == null) return 0;
            // For White, forward is Up (-y). Tilt UP screen makes old Back become Top.
            // For Black, forward is Down (+y). Tilt DOWN screen makes old Front become Top.
            return Owner == Player.White ? Faces[3] : Faces[2];
        }

        public int GetRightValue()
        {
            if (Faces == null) return 0;
            // Tilt RIGHT: Left face (idx 4) becomes Top.
            return Faces[4];
        }

        public int GetLeftValue()
        {
            if (Faces == null) return 0;
            // Tilt LEFT: Right face (idx 5) becomes Top.
            return Faces[5];
        }

        public readonly Die Tilted(int dx, int dy)
        {
            int[] newFaces = (int[])Faces.Clone();

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
            // White (Bottom): 4 is facing Center (UP screen / Front)
            for (int x = 0; x < Size; x++)
            {
                Grid[x, 6] = new Die(Player.White, 6, 4);
            }

            // Black (Top): 4 is facing Center (DOWN screen / Back)
            // If 4 is Back (idx 3), then Front (idx 2) is 3.
            for (int x = 0; x < Size; x++)
            {
                Grid[x, 0] = new Die(Player.Black, 6, 3);
            }
        }

        public Board Clone()
        {
            Board b = new();
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                    b.Grid[x, y] = this.Grid[x, y];
            return b;
        }

        public bool IsInBounds(int x, int y) => x >= 0 && x < Size && y >= 0 && y < Size;

        public Die?[,] GetBoard() => Grid;
    }
}
