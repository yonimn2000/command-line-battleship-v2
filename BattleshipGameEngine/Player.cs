using System;
using System.Collections.Generic;
using System.Drawing;

namespace YonatanMankovich.BattleshipGameEngine
{
    public class Player
    {
        public Board Board { get; }
        public bool HitLastShot { get; private set; } = false;
        public bool HasWon { get; private set; } = false;

        public Player()
        {
            Board = new Board();
        }

        public void AutomaticallyAddFleetToBoard(Queue<Ship.Type> fleetQueue)
        {
            while (fleetQueue.Count != 0)
                AutomaticallyAddShipToBoard(fleetQueue.Dequeue());
        }

        public void AutomaticallyAddShipToBoard(Ship.Type shipType)
        {
            Point point = GetRandomPossibleInitialPoint(shipType);
            Board.AddShipToBoard(new Ship(shipType, point));
        }

        private readonly Random random = new Random();
        /// <summary>
        /// Finds a space for a specific ship size randomly on the board by picking 
        /// random initial points until the whole ship fits, or throws an exception.
        /// </summary>
        public Point GetRandomPossibleInitialPoint(Ship.Type shipType)
        {
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            // Avoid looking for initial point for too long. Not the best solution...
            while (watch.ElapsedMilliseconds < Board.Size.Height * Board.Size.Width * 100)
            {
                // Flip the ship to make sure the ship fits on the board OR pick a random ship orientation.
                if (shipType.Size.Height > Board.Size.Height || shipType.Size.Height > Board.Size.Width || random.Next() % 2 == 0)
                    shipType.FlipSize();
                Point initialPoint;
                initialPoint.X = random.Next(Board.Size.Width - shipType.Size.Width + 1);
                initialPoint.Y = random.Next(Board.Size.Height - shipType.Size.Height + 1);
                if (Board.IsShipPositionPossible(new Ship(shipType, initialPoint)))
                    return initialPoint;
            }
            throw new BoardIsFullException(shipType.Size, Board);
        }

        public Board.PointStates ShootPlayerBoard(Point point, Board board)
        {
            Board.PointStates shootingResult = board.ShootPoint(point);
            switch (shootingResult)
            {
                case Board.PointStates.Empty: HitLastShot = false; break;
                case Board.PointStates.Ship: HitLastShot = true; break;
                case Board.PointStates.DestroyedShip:
                    {
                        HitLastShot = true;
                        if (board.AreAllShipsDestroyed())
                            HasWon = true;
                    }
                    break;
            }
            return shootingResult;
        }
    }
}