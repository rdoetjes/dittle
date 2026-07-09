using System;
using System.Collections.Generic;
using Raylib_cs;
using System.Numerics;

namespace Dittle
{
    public static class Graphics
    {
        public const int OFFSET = 60;
        public const int SIZE = 50;
        public const int BOARD_SIZE_X = 500;
        public const int BOARD_SIZE_Y = 600;

        static Font customFont;
        static Texture2D bgImg;
        static Texture2D boardImg;
        static Texture2D[] whiteDice = new Texture2D[6];
        static Texture2D[] blackDice = new Texture2D[6];

        public static void InitializeResourcePath()
        {
            // MacOS App Bundle resource path handling
            if (OperatingSystem.IsMacOS() && (AppContext.BaseDirectory.Contains(".app/Contents/MacOS") || AppContext.BaseDirectory.Contains(".app/Contents/Resources")))
            {
                // Try to find Resources folder relative to the binary
                string baseDir = AppContext.BaseDirectory;
                string? resourcePath = null;

                if (baseDir.Contains(".app/Contents/MacOS"))
                {
                    resourcePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDir, "..", "Resources"));
                }
                else if (baseDir.Contains(".app/Contents/Resources"))
                {
                    resourcePath = baseDir;
                }

                if (resourcePath != null && System.IO.Directory.Exists(resourcePath))
                {
                    System.IO.Directory.SetCurrentDirectory(resourcePath);
                }
            }
        }

        public static void LoadResources()
        {
            // Using assets found in resources
            string fontPath = "resources/fonts/revvy.ttf";
            // Fallback for MacOS App Bundle where working directory is Resources/
            if (!System.IO.File.Exists(fontPath) && System.IO.File.Exists("fonts/revvy.ttf")) fontPath = "fonts/revvy.ttf";

            if (System.IO.File.Exists(fontPath))
            {
                customFont = Raylib.LoadFontEx(fontPath, 64, null, 0);
                Raylib.SetTextureFilter(customFont.Texture, TextureFilter.Bilinear);
            }

            string bgPath = "resources/img/bg.png";
            if (!System.IO.File.Exists(bgPath) && System.IO.File.Exists("img/bg.png")) bgPath = "img/bg.png";
            if (System.IO.File.Exists(bgPath)) bgImg = Raylib.LoadTexture(bgPath);

            string boardPath = "resources/img/board.png";
            if (!System.IO.File.Exists(boardPath) && System.IO.File.Exists("img/board.png")) boardPath = "img/board.png";
            if (System.IO.File.Exists(boardPath)) boardImg = Raylib.LoadTexture(boardPath);

            LoadDiceTextures();
        }

        private static void LoadDiceTextures()
        {
            for (int i = 1; i <= 6; i++)
            {
                whiteDice[i - 1] = LoadDiceTexture(i, "white");
                blackDice[i - 1] = LoadDiceTexture(i, "black");
            }
        }

        private static Texture2D LoadDiceTexture(int value, string color)
        {
            string[] paths = {
                $"resources/img/{value}_{color}.png",
                $"resources/img/{value}_{color}.jpg",
                $"img/{value}_{color}.png",
                $"img/{value}_{color}.jpg"
            };

            foreach (var path in paths)
            {
                if (System.IO.File.Exists(path)) return Raylib.LoadTexture(path);
            }
            return new Texture2D();
        }

        public static void UnloadResources()
        {
            if (customFont.Texture.Id > 0) Raylib.UnloadFont(customFont);
            if (bgImg.Id > 0) Raylib.UnloadTexture(bgImg);
            if (boardImg.Id > 0) Raylib.UnloadTexture(boardImg);
            for (int i = 0; i < 6; i++)
            {
                if (whiteDice[i].Id > 0) Raylib.UnloadTexture(whiteDice[i]);
                if (blackDice[i].Id > 0) Raylib.UnloadTexture(blackDice[i]);
            }
        }

        public static void DrawTextCustom(string text, int x, int y, int fontSize, Color color)
        {
            if (customFont.Texture.Id > 0)
                Raylib.DrawTextEx(customFont, text, new Vector2(x, y), (float)fontSize, 1.5f, color);
            else
                Raylib.DrawText(text, x, y, fontSize, color);
        }

        public static void DrawBoard(Board board, int? selX, int? selY, List<Move> legalMoves)
        {
            int startX = (BOARD_SIZE_X - 7 * OFFSET) / 2, startY = (BOARD_SIZE_Y - 7 * OFFSET) / 2;
            int padding = ((OFFSET - SIZE) / 2)-5;

            //Sand bankground
            Raylib.DrawTexturePro(bgImg,
                    new(0, 0, bgImg.Width, bgImg.Height),
                    new(0, 0, BOARD_SIZE_X, BOARD_SIZE_Y),
                    new(0, 0), 0, Color.White);

            //Board background
            Raylib.DrawTextureEx(boardImg, new(startX, startY), 0.0f, 0.5f, Color.Beige);
            for (int y = 0; y < Board.Size; y++)
                for (int x = 0; x < Board.Size; x++)
                {
                    int px = startX + x * OFFSET, py = startY + y * OFFSET;
                    foreach (var m in legalMoves)
                    {
                        if (m.ToX == x && m.ToY == y)
                        {
                            Raylib.DrawCircle(px + OFFSET / 2, py + OFFSET / 2, OFFSET / 4, new(0, 121, 241, 200));
                            DrawTextCustom(m.ResultDie.Top.ToString(), px + OFFSET / 2 - 5, py + OFFSET / 2 - 8, 18, Color.White);
                        }
                    }

                    Die? d = board.Grid[x, y];
                    if (d is not null && d.HasValue) DrawDie(px + padding, py + padding, SIZE+10, d.Value);
                }
        }

        public static void DrawHorizontalMoves(Player player, uint nrHorizontalMoves)
        {
            int startX = (BOARD_SIZE_X - 7 * OFFSET) / 2;
            int boardHeight = 7 * OFFSET;
            int startY = (BOARD_SIZE_Y - boardHeight) / 2;

            // Position indicators
            int xPos = startX;
            int yPos = (player == Player.White) ? startY + boardHeight + 10 : startY - 25;

            int radius = 8;
            int spacing = 22;

            for (int i = 0; i < 4; i++)
            {
                int cx = xPos + i * spacing + radius;
                int cy = yPos + radius;

                if (i < nrHorizontalMoves)
                {
                    // LED ON - Green "on" state
                    Color glowColor = new(0, 228, 48, 255); // Raylib Lime/Green

                    // Draw outer glow/body
                    Raylib.DrawCircle(cx, cy, (float)radius, glowColor);
                    // Draw white hot spot
                    Raylib.DrawCircle(cx - 2, cy - 2, (float)radius / 2, new(255, 255, 255, 200));
                    // Outline
                    Raylib.DrawCircleLines(cx, cy, (float)radius, Color.DarkGreen);
                }
                else
                {
                    // LED OFF - Just outline
                    Raylib.DrawCircle(cx, cy, (float)radius, Color.DarkGreen);
                    Raylib.DrawCircle(cx - 2, cy - 2, (float)radius / 2, new(128, 128, 128, 200));
                }
            }
        }

        public static void DrawUI(int depth, Player current, Board board, int maxAiDepth, float matchTime, float whiteThinkTime, float blackThinkTime, bool isAiThinking)
        {
            // Match Time at top
            string matchTimeStr = $"TIME: {TimeSpan.FromSeconds(matchTime):mm\\:ss}";
            DrawTextCustom(matchTimeStr, 10, 35, 18, Color.DarkBrown);

            int uiControlY = BOARD_SIZE_Y - 80;
            Raylib.DrawRectangleLinesEx(new Rectangle(BOARD_SIZE_X - 120, 10, 100, 30), 2, Color.DarkBrown);
            DrawTextCustom(" RESTART", BOARD_SIZE_X - 110, 18, 16, Color.DarkBrown);

            DrawTextCustom("LEVEL:", 100, uiControlY + 14, 20, Color.DarkBrown);
            Raylib.DrawRectangle(210, uiControlY, 40, 40, Color.Beige);
            DrawTextCustom("-", 225, uiControlY + 5, 30, Color.Black);

            Raylib.DrawRectangle(310, uiControlY, 40, 40, Color.Beige);
            DrawTextCustom("+", 321, uiControlY + 5, 30, Color.Black);

            string dText = depth.ToString();
            int textW = 10;
            if (customFont.Texture.Id > 0) textW = (int)Raylib.MeasureTextEx(customFont, dText, 24, 2).X;
            else textW = Raylib.MeasureText(dText, 24);

            DrawTextCustom(dText, 250 + (60 - textW) / 2, uiControlY + 10, 25, Color.DarkBrown);

            string turnText = isAiThinking ? $"THINKING: {current.ToString().ToUpper()}" : $"TURN: {current.ToString().ToUpper()}";
            DrawTextCustom(turnText, 10, 10, 20, Color.DarkBrown);

            if (Rules.IsGameOver(board, out Player? w))
            {
                string winnerText = $"WINNER: {w}";
                int winW = 100;
                if (customFont.Texture.Id > 0) winW = (int)Raylib.MeasureTextEx(customFont, winnerText, 40, 2).X;
                else winW = Raylib.MeasureText(winnerText, 40);

                Raylib.DrawRectangle(0, BOARD_SIZE_Y / 2 - 20, BOARD_SIZE_X, 80, new(0, 0, 0, 200));
                DrawTextCustom(winnerText, (BOARD_SIZE_X - winW) / 2, BOARD_SIZE_Y / 2, 40, Color.Gold);
            }

            // The number of horizontal move indicators
            DrawHorizontalMoves(Player.White, (uint)board.WhiteHorizontalMoves);
            DrawHorizontalMoves(Player.Black, (uint)board.BlackHorizontalMoves);

            // Think times next to LEDs
            int startX = (BOARD_SIZE_X - 7 * OFFSET) / 2;
            int boardHeight = 7 * OFFSET;
            int startY = (BOARD_SIZE_Y - boardHeight) / 2;
            
            string wThink = $"{whiteThinkTime:F1}s";
            string bThink = $"{blackThinkTime:F1}s";
            DrawTextCustom(wThink, startX + 4 * 22 + 20, startY + boardHeight + 10, 16, Color.DarkBrown);
            DrawTextCustom(bThink, startX + 4 * 22 + 20, startY - 25, 16, Color.DarkBrown);
        }

        public static void DrawAiMoveHighlight(Move? lastMove, float timer)
        {
            if (timer <= 0 || !lastMove.HasValue) return;
            int startX = (BOARD_SIZE_X - 7 * OFFSET) / 2, startY = (BOARD_SIZE_Y - 7 * OFFSET) / 2;
            int fx = startX + lastMove.Value.FromX * OFFSET, fy = startY + lastMove.Value.FromY * OFFSET;
            int tx = startX + lastMove.Value.ToX * OFFSET, ty = startY + lastMove.Value.ToY * OFFSET;
            Raylib.DrawRectangleLinesEx(new Rectangle(fx, fy, OFFSET, OFFSET), 3, Color.Red);
            Raylib.DrawLineEx(new(fx + OFFSET / 2, fy + OFFSET / 2), new(tx + OFFSET / 2, ty + OFFSET / 2), 3, Color.Red);
            Raylib.DrawRectangleLinesEx(new Rectangle(tx, ty, OFFSET, OFFSET), 4, Color.Orange);
        }

        public static void DrawDie(int x, int y, int size, Die die)
        {
            if (die.Top < 1 || die.Top > 6) return;
            Texture2D tex = (die.Owner == Player.White) ? whiteDice[die.Top - 1] : blackDice[die.Top - 1];
            if (tex.Id > 0)
            {
                float scale = (float)size / tex.Width;
                Raylib.DrawTextureEx(tex, new Vector2(x, y), 0f, scale, Color.White);
            }
        }
    }
}
