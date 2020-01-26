using System;
using System.Drawing;
using YonatanMankovich.BattleshipGameEngine;

namespace YonatanMankovich.CommandLineBattleship
{
    public static class ConsoleGrapher
    {
        public static void DrawBoard(int line, Board board, bool showShips)
        {
            DrawBoardWithSelectedArea(line, board, showShips, new Point(), new Size());
        }

        public static void DrawBoardWithSelectedPoint(int line, Board board, bool showShips, Point selectedAreaStartingPoint)
        {
            DrawBoardWithSelectedArea(line, board, showShips, selectedAreaStartingPoint, new Size(1, 1));
        }

        public static void DrawBoardWithSelectedArea(int line, Board board, bool showShips, Point selectedAreaStartingPoint, Size selectedAreaSize)
        {
            Console.SetCursorPosition(0, line);
            for (int y = 0; y < board.Size.Height; y++)
            {
                for (int x = 0; x < board.Size.Width; x++)
                {
                    Point currentPoint = new Point(x, y);
                    bool isSelected = IsPointInArea(currentPoint, selectedAreaStartingPoint, selectedAreaSize);
                    switch (board.GetPointState(currentPoint))
                    {
                        case Board.PointStates.Empty:
                            WriteInColor("~ ", ConsoleColor.Blue, ConsoleColor.White, isSelected);
                            break;
                        case Board.PointStates.Miss:
                            WriteInColor("x ", ConsoleColor.DarkGreen, ConsoleColor.White, isSelected);
                            break;
                        case Board.PointStates.Ship:
                            if (showShips)
                                WriteInColor("* ", ConsoleColor.Cyan, ConsoleColor.Black, isSelected);
                            else
                                WriteInColor("~ ", ConsoleColor.Blue, ConsoleColor.White, isSelected);
                            break;
                        case Board.PointStates.HitShipElement:
                            WriteInColor("* ", ConsoleColor.Magenta, ConsoleColor.White, isSelected);
                            break;
                        case Board.PointStates.DestroyedShip:
                            WriteInColor("XX", ConsoleColor.DarkMagenta, ConsoleColor.White, isSelected);
                            break;
                    }
                }
                Console.WriteLine();
            }
        }

        public static void WriteInColor(string text, ConsoleColor bgColor, ConsoleColor fgColor, bool isSelected = false)
        {
            ConsoleColor prevBgColor = Console.BackgroundColor;
            ConsoleColor prevFgColor = Console.ForegroundColor;
            if (!isSelected)
            {
                Console.BackgroundColor = bgColor;
                Console.ForegroundColor = fgColor;
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            Console.Write(text);
            Console.BackgroundColor = prevBgColor;
            Console.ForegroundColor = prevFgColor;
        }

        private static bool IsPointInArea(Point point, Point startingPoint, Size areaSize)
        {
            return startingPoint.X <= point.X && startingPoint.Y <= point.Y && point.X < startingPoint.X + areaSize.Width && point.Y < startingPoint.Y + areaSize.Height;
        }

        public static void WriteTextOnLine(string text, int line)
        {
            Console.SetCursorPosition(0, line);
            Console.Write(text);
        }

        public static void ClearLine(int line)
        {
            ClearArea(new Point(0, line), new Point(Console.BufferWidth - 1, line));
        }

        public static void ClearArea(Point start, Point end)
        {
            for (int y = start.Y; y <= end.Y; y++)
            {
                for (int x = start.X; x <= end.X; x++)
                {
                    Console.CursorLeft = x;
                    Console.CursorTop = y;
                    Console.Write(' ');
                }
                Console.WriteLine();
            }
        }
    }
}