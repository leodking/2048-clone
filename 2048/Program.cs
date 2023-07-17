using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2048
{
    class Program
    {

        static Dictionary<int, ConsoleColor> TileColours = new Dictionary<int, ConsoleColor>
            {
                { 2, ConsoleColor.DarkBlue },
                { 4, ConsoleColor.DarkGreen },
                { 8, ConsoleColor.DarkYellow },
                { 16, ConsoleColor.DarkCyan },
                { 32, ConsoleColor.DarkMagenta },
                { 64, ConsoleColor.DarkRed },
                { 128, ConsoleColor.Blue },
                { 256, ConsoleColor.Gray },
                { 512, ConsoleColor.Magenta },
                { 1024, ConsoleColor.Yellow },
                { 2048, ConsoleColor.Green },
            };

        enum Direction
        {
            Left,
            Up,
            Down,
            Right,
        }

        static int[] startPos = { 2, 1 };
        static int TileWidth = 6;

        static Random rand = new Random();

        static int score;

        public static string PadCentre(int num, char pad, int width)
        {
            string numStr = Convert.ToString(num);
            int spaces = width - numStr.Length;
            int padLeft = spaces / 2 + numStr.Length;
            return numStr.PadLeft(padLeft).PadRight(width);
        }

        public static ConsoleColor GetForeColor(ConsoleColor BackColor)
        {
            if ((int)BackColor < 7)
                return ConsoleColor.White;
            else
                return ConsoleColor.Black;
        }

        public static void DrawBorder(int[,] board)
        {
            Console.SetCursorPosition(startPos[0] - 1, startPos[1] - 1);
            Console.WriteLine("┌" + new string('─', board.GetLength(0) * TileWidth) + "┐");
            for (int i = 0; i < board.GetLength(1) * 3; i++)
            {
                Console.CursorLeft = startPos[0] - 1;
                Console.WriteLine("│" + new string(' ', board.GetLength(0) * TileWidth) + "│");
            }
            Console.CursorLeft = startPos[0] - 1;
            Console.WriteLine("└" + new string('─', board.GetLength(0) * TileWidth) + "┘");
        }

        public static void DrawBoard(int[,] board)
        {
            // Draw main board
            int boardNum;
            string EmptyTileRow = new string(' ', TileWidth);

            Console.SetCursorPosition(startPos[0], startPos[1]);
            for (int row = 0; row < board.GetLength(0); row++)
            {
                for (int col = 0; col < board.GetLength(1); col++)
                {
                    Console.CursorTop = startPos[1] + (row * 3);
                    boardNum = board[row, col];
                    if (boardNum > 0)
                    {
                        Console.CursorLeft = startPos[0] + (col * TileWidth);
                        Console.BackgroundColor = Program.TileColours[boardNum];
                        Console.ForegroundColor = GetForeColor(Console.BackgroundColor);
                        Console.WriteLine(EmptyTileRow);
                        Console.CursorLeft = startPos[0] + (col * TileWidth);
                        Console.WriteLine(PadCentre(boardNum, ' ', TileWidth));
                        Console.CursorLeft = startPos[0] + (col * TileWidth);
                        Console.WriteLine(EmptyTileRow);
                        Console.ResetColor();
                    }
                    else
                    {
                        for (int i = 1; i <= 3; i++)
                        {
                            Console.CursorLeft = startPos[0] + (col * TileWidth);
                            Console.WriteLine(EmptyTileRow);
                        }
                    }
                }
            }
        }

        static bool GameOver(int[,] board)
        {
            // If there are any empty spots, it's not game over.
            for (int row = 0; row < board.GetLength(0); row++)
            {
                for (int col = 0; col < board.GetLength(1); col++)
                {
                    if (board[row, col] == 0)
                        return false;
                }
            }

            // If there are any adjacent numbers on rows, it's not game over.
            for (int row = 0; row < board.GetLength(0); row++)
            {
                for (int col = 0; col < board.GetLength(1) - 1; col++)
                {
                    if (board[row, col] == board[row, col + 1])
                        return false;
                }
            }

            // If there are any adjacent numbers on columns, it's not game over
            for (int col = 0; col < board.GetLength(1); col++)
            {
                for (int row = 0; row < board.GetLength(0) - 1; row++)
                {
                    if (board[row, col] == board[row + 1, col])
                        return false;
                }
            }

            // It's game over when there are no adjacent numbers and no empty spots.
            return true;
        }

        static bool MoveOnce(Direction direction, ref int[,] board, ref bool[,] merged, ref bool moveHappened)
        {
            // Return true if any move took place. If false, do nothing.
            // This sub also checks whether any move can be made in the given direction.
            bool moved = false;
            int boardWidth = board.GetLength(0);
            int boardHeight = board.GetLength(1);
            int currentNum, compareNum;

            switch (direction)
            {
                case Direction.Left:
                    // Start from column 1 (second from left)
                    for (int row = 0; row < boardHeight; row++)
                    {
                        for (int col = 1; col < boardWidth; col++)
                        {
                            currentNum = board[row, col];

                            // Don't try to move empty cells
                            if (currentNum == 0)
                                continue;

                            compareNum = board[row, col - 1];

                            if (compareNum == 0)
                            {
                                // Slot is empty, currentNum can move one place to the left
                                board[row, col - 1] = currentNum;
                                board[row, col] = 0;
                                moved = true;
                            }
                            else if (compareNum == currentNum)
                            {
                                // Numbers are the same and can be merged, unless they've already merged
                                if (!merged[row, col])
                                {
                                    board[row, col - 1] *= 2;
                                    board[row, col] = 0;
                                    merged[row, col - 1] = true;
                                    score += board[row, col - 1];
                                    UpdateScore();
                                    moved = true;
                                }
                            }

                        }
                    }
                    break;

                case Direction.Right:
                    // Start moving tiles from the second-from-the-right column, going to the leftmost column
                    for (int row = 0; row < boardHeight; row++)
                    {
                        for (int col = boardWidth - 2; col >= 0; col--)
                        {
                            currentNum = board[row, col];

                            // Don't try to move empty cells
                            if (currentNum == 0)
                                continue;

                            compareNum = board[row, col + 1];

                            if (compareNum == 0)
                            {
                                // Slot is empty, currentNum can move one place to the left
                                board[row, col + 1] = currentNum;
                                board[row, col] = 0;
                                moved = true;
                            }
                            else if (compareNum == currentNum)
                            {
                                // Numbers are the same and can be merged, unless they've already merged
                                if (!merged[row, col])
                                {
                                    board[row, col + 1] *= 2;
                                    board[row, col] = 0;
                                    merged[row, col + 1] = true;
                                    score += board[row, col + 1];
                                    UpdateScore();
                                    moved = true;
                                }
                            }
                        }
                    }
                    break;

                case Direction.Up:
                    // Move tiles from second row up to the top, going down to the bottom
                    for (int col = 0; col < boardWidth; col++)
                    {
                        for (int row = 1; row < boardHeight; row++)
                        {
                            currentNum = board[row, col];

                            if (currentNum == 0)
                                continue;

                            compareNum = board[row - 1, col];

                            if (compareNum == 0)
                            {
                                board[row - 1, col] = currentNum;
                                board[row, col] = 0;
                                moved = true;
                            }
                            else if (compareNum == currentNum)
                            {
                                if (!merged[row, col])
                                {
                                    board[row - 1, col] *= 2;
                                    board[row, col] = 0;
                                    merged[row - 1, col] = true;
                                    score += board[row - 1, col];
                                    UpdateScore();
                                    moved = true;
                                }
                            }
                        }
                    }
                    break;

                case Direction.Down:
                    for (int col = 0; col < boardWidth; col++)
                    {
                        for (int row = boardHeight - 2; row >= 0; row--)
                        {

                            currentNum = board[row, col];

                            if (currentNum == 0)
                                continue;

                            compareNum = board[row + 1, col];

                            if (compareNum == 0)
                            {
                                board[row + 1, col] = currentNum;
                                board[row, col] = 0;
                                moved = true;
                            }
                            else if (compareNum == currentNum)
                            {
                                if (!merged[row, col])
                                {
                                    board[row + 1, col] *= 2;
                                    board[row, col] = 0;
                                    merged[row + 1, col] = true;
                                    score += board[row + 1, col];
                                    UpdateScore();
                                    moved = true;
                                }
                            }
                        }
                    }
                    break;
            }

            if (moved)
                moveHappened = true;

            return moved;
        }

        static void PlaceNewTile(ref int[,] board)
        {
            // 10% chance of tileNum being 4; 90% chance of it being 2.
            int tileNum = rand.Next(1, 11) == 10 ? 4 : 2;

            List<int[]> EmptyTiles = new List<int[]>();

            for (int row = 0; row < board.GetLength(0); row++)
            {
                for (int col = 0; col < board.GetLength(1); col++)
                {
                    if (board[row, col] == 0)
                    {
                        EmptyTiles.Add(new int[] { row, col });
                    }
                }
            }

            if (EmptyTiles.Count > 0)
            {
                var location = EmptyTiles[rand.Next(0, EmptyTiles.Count)];
                board[location[0], location[1]] = tileNum;
            }

        }

        static void UpdateScore()
        {
            Console.Title = $"2048 Game -- Score: {score}";
        }

        static bool CheckWin(int[,] board)
        {
            for (int row = 0; row < board.GetLength(0); row++)
            {
                for (int col = 0; col < board.GetLength(1); col++)
                {
                    if (board[row, col] == 2048)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static void Main(string[] args)
        {
            // REVISIT - there is currently a bug with the left/right movement algorithm
            // If the board is like below, then if you move left, the first row will cascade all numbers to the left
            // And if you move right, the 2nd row will cascade all numbers to the right
            // As in combine all numbers into one, rather than waiting until the next user turn.
            //int[,] board = {
            //    { 2, 2, 4, 8 },
            //    { 8, 4, 2, 2 },
            //    { 0, 0, 0, 0 },
            //    { 0, 0, 0, 0 },
            //};
            int[,] board = {
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
            };

            score = 0;

            // REVISIT - Can't set the Console window size for some reason
            //int Width = startPos[0] + (board.GetLength(0) * TileWidth) + 3;
            //int Height = startPos[1] + (board.GetLength(1) * 3) + 3;
            //Console.SetWindowSize(Width, Height);

            // Place 2 starting tiles
            PlaceNewTile(ref board);
            PlaceNewTile(ref board);

            DrawBorder(board);
            DrawBoard(board);
            UpdateScore();

            Direction direction = Direction.Left;
            ConsoleKeyInfo cki;

            bool HasWon = false;

            while (!GameOver(board) && !HasWon)
            {
                bool[,] merged = new bool[board.GetLength(0), board.GetLength(1)];
                cki = Console.ReadKey(true);
                switch (cki.Key)
                {
                    case ConsoleKey.LeftArrow:
                        direction = Direction.Left;
                        break;
                    case ConsoleKey.RightArrow:
                        direction = Direction.Right;
                        break;
                    case ConsoleKey.UpArrow:
                        direction = Direction.Up;
                        break;
                    case ConsoleKey.DownArrow:
                        direction = Direction.Down;
                        break;
                }

                bool moveHappened = false;

                // Move all tiles one position at a time to allow animating the movement
                while (MoveOnce(direction, ref board, ref merged, ref moveHappened))
                {
                    DrawBoard(board);
                    Thread.Sleep(30);
                }

                if (moveHappened)
                {
                    PlaceNewTile(ref board);
                    DrawBoard(board);
                }

                HasWon = CheckWin(board);
            }

            if (HasWon)
                MessageBox.Show("Congratulations! You won 2048!");
            else
                MessageBox.Show("Bad luck. You lost!");

            Console.ReadLine();
        }
    }
}
