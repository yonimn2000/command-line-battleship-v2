using System;
using System.Collections.Generic;
using System.Drawing;

namespace YonatanMankovich.BattleshipGameEngine
{
    public class ComputerPlayer : Player
    {
        public ComputerPlayer() : base()
        {
            AutomaticallyAddFleetToBoard(Board.Options.GetFleet());
        }

        public void ShootPlayerBoardAutomatically(Board board)
        {
            do ShootPlayerBoard(GetNextShootingPoint(board), board);
            while (Board.Options.AllowExtraMoveAfterHit && HitLastShot && !HasWon);
        }

        private readonly IList<Point> nextPossibleComputerMoves = new List<Point>();
        private Point lastMovePoint;

        public Point GetNextShootingPoint(Board board)
        {
            Random random = new Random();
            Point nextPoint;
            if (Board.IsShipDestroyedAtPoint(lastMovePoint))
                nextPossibleComputerMoves.Clear();
            if (nextPossibleComputerMoves.Count == 0)
            {
                IList<Point> points = board.GetPossibleShootingPoints();
                nextPoint = points[random.Next(points.Count)];
            }
            else
            {
                int indexOfNextMove = random.Next(nextPossibleComputerMoves.Count);
                nextPoint = nextPossibleComputerMoves[indexOfNextMove];
                nextPossibleComputerMoves.RemoveAt(indexOfNextMove);
            }
            // If computer is going to hit a ship:
            if (board.GetPointState(nextPoint) == Board.PointStates.Ship)
                GenerateNextMoves(board, nextPoint);
            lastMovePoint = nextPoint;
            return nextPoint;
        }

        private void GenerateNextMoves(Board board, Point point)
        {
            // HACK: make the moves smarter with bigger ships (of width > 1).
            IList<Point> upDownLeftRight = new List<Point>(4)
            {
                new Point(point.X, point.Y + 1),
                new Point(point.X, point.Y - 1),
                new Point(point.X - 1, point.Y),
                new Point(point.X + 1, point.Y)
            };
            for (int i = 0; i < upDownLeftRight.Count; i++)
                if (board.IsValidPointToShoot(upDownLeftRight[i]) && !nextPossibleComputerMoves.Contains(upDownLeftRight[i]))
                    nextPossibleComputerMoves.Add(upDownLeftRight[i]);
        }
    }
}