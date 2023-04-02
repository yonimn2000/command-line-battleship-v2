using System;
using System.Drawing;

namespace YonatanMankovich.BattleshipGameEngine
{
    public class BoardIsFullException : Exception
    {
        public BoardIsFullException(string msg = "") : base(msg) { }

        public BoardIsFullException(Board board, string msg = "") : this($"The board {board.Size} is full. " + msg) { }

        public BoardIsFullException(Size shipSize, Board board) : this(board, $"Room for the ship of size {shipSize} cannot be found.") { }
    }

    public class ShipSizeTooBigForBoardException : Exception
    {
        public ShipSizeTooBigForBoardException(string msg = "") : base(msg) { }

        public ShipSizeTooBigForBoardException(Size shipSize, Board board) : this($"Ship size {shipSize} is too big for board {board.Size}.") { }
    }

    public class InvalidShipPositionException : Exception
    {
        public InvalidShipPositionException(string msg = "") : base(msg) { }

        public InvalidShipPositionException(Ship ship) : this($"Invalid ship position. The ship {ship} was added to an invalid position. "
                   + $"The whole ship has to be on the board, should not overlap with other ships, "
                   + $"and if AllowAdjacentShips is set to false, ships cannot be adjacent.")
        { }
    }

    public class PointOutsideOfBoardException : Exception
    {
        public PointOutsideOfBoardException(string msg = "") : base(msg) { }

        public PointOutsideOfBoardException(Point point, Board board) : this($"The point {point} is outside the board {board.Size}.") { }
    }

    public class PointNotOnShipException : Exception
    {
        public PointNotOnShipException(string msg = "") : base(msg) { }

        public PointNotOnShipException(Point point, Ship ship) : this($"The point {point} is not part of the ship {ship} elements.") { }
    }
}