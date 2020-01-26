using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using YonatanMankovich.BattleshipGameEngine;
using YonatanMankovich.SimpleConsoleMenus;

namespace YonatanMankovich.CommandLineBattleship
{
    class GameController
    {
        private readonly Player player;
        private readonly ComputerPlayer computer;

        public GameController()
        {
            Console.WriteLine("Please wait. The computer board is being created...");
            try
            {
                computer = new ComputerPlayer();
                player = new Player();
            }
            catch (Exception e) when (e is System.Xml.XmlException || e is System.Xml.Schema.XmlSchemaException || e is BoardIsFullException)
            {
                if (e is System.Xml.XmlException)
                    ConsoleGrapher.WriteInColor("Error reading BoardOptions.xml file: " + e.Message, ConsoleColor.Red, ConsoleColor.White);
                else
                    ConsoleGrapher.WriteInColor(e.Message, ConsoleColor.Red, ConsoleColor.White);
                Console.WriteLine("\n\nWhat would you like to do next?");
                SimpleActionConsoleMenu badOptionsMenu = new SimpleActionConsoleMenu();
                badOptionsMenu.AddOption("Rebuild the default game options file", Board.Options.RebuildDefaultBoardOptionsFile);
                badOptionsMenu.AddOption("Change game options", delegate
                {
                    Board.Options.CreateDefaultBoardOptionsFileIfDoesNotExist();
                    Process.Start(Board.Options.PATH_TO_OPTIONS);
                });
                badOptionsMenu.AddOption("Exit", delegate { Environment.Exit(0); });
                badOptionsMenu.ShowAndDoAction();
                Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
                Environment.Exit(1);
            }
        }

        private void AddShipsToPlayerBoard()
        {
            ConsoleGrapher.WriteTextOnLine("Use the arrow keys to position the ship and hit ENTER.", 0);
            ConsoleGrapher.WriteTextOnLine("Hit 'R' to rotate the ship. Hit 'P' to place the ship automatically. Hit 'F' to finish automatically.", 1);
            Queue<Ship.Type> fleet = Board.Options.GetFleet();
            bool finishAutomatically = false;
            Point initialPoint = default;
            while (fleet.Count != 0 && !finishAutomatically)
            {
                Ship.Type shipType = fleet.Peek();
                bool userDone = false;
                Ship ship;
                do
                {
                    ConsoleGrapher.DrawBoardWithSelectedArea(2, player.Board, true, initialPoint, shipType.Size);
                    switch (Console.ReadKey().Key)
                    {
                        case ConsoleKey.Enter: userDone = true; break;
                        case ConsoleKey.LeftArrow: initialPoint.X--; break;
                        case ConsoleKey.UpArrow: initialPoint.Y--; break;
                        case ConsoleKey.RightArrow: initialPoint.X++; break;
                        case ConsoleKey.DownArrow: initialPoint.Y++; break;
                        case ConsoleKey.F: finishAutomatically = userDone = true; break;
                        case ConsoleKey.P: initialPoint = player.GetRandomPossibleInitialPoint(shipType); break;
                        case ConsoleKey.R: shipType.FlipSize(); break;
                    }
                    ship = new Ship(shipType, initialPoint);
                    initialPoint = MoveShipBackToBoardIfOutside(ship);
                } while (!userDone);
                if (!finishAutomatically && player.Board.IsShipPositionPossible(ship))
                {
                    player.Board.AddShipToBoard(ship);
                    fleet.Dequeue();
                }
                else
                    WriteMessage(new InvalidShipPositionException(new Ship(shipType, initialPoint)).Message);
            }
            player.AutomaticallyAddFleetToBoard(fleet);
        }

        private Point MoveShipBackToBoardIfOutside(Ship ship)
        {
            Point lastShipPoint = ship.Elements[ship.Elements.Length - 1].Point;
            Point initialShipPoint = ship.GetInitialPoint();
            if (lastShipPoint.X >= player.Board.Size.Width)
                initialShipPoint = new Point(initialShipPoint.X - (lastShipPoint.X + 1 - player.Board.Size.Width), initialShipPoint.Y);
            if (lastShipPoint.Y >= player.Board.Size.Height)
                initialShipPoint = new Point(initialShipPoint.X, initialShipPoint.Y - (lastShipPoint.Y + 1 - player.Board.Size.Height));
            if (initialShipPoint.X < 0)
                initialShipPoint = new Point(0, initialShipPoint.Y);
            if (initialShipPoint.Y < 0)
                initialShipPoint = new Point(initialShipPoint.X, 0);
            return initialShipPoint;
        }

        public void Play()
        {
            // TODO: add Console.Beeps
            AddShipsToPlayerBoard();
            Console.Clear();
            while (!player.HasWon && !computer.HasWon)
            {
                HandlePlayerMoves();
                if(!player.HasWon)
                computer.ShootPlayerBoardAutomatically(player.Board);
            }
            VisualizeBoards();
            if (player.HasWon)
                WriteMessage("YOU WON! :)");
            else
                WriteMessage("COMPUTER WON! :(");
            Console.WriteLine("Press ENTER to exit to main menu:");
            Console.ReadLine();
        }

        //Optional to see the computer moves one by one.
        /*private void HandleComputerMoves()
        {
            do
            {
                VisualizeBoards();
                if (computer.HitLastShot)
                    System.Threading.Thread.Sleep(500);
                Point shootingPoint = computer.GetNextShootingPoint(player.Board);
                switch (computer.ShootPlayerBoard(shootingPoint, player.Board))
                {
                    case Board.PointStates.Empty: WriteMessage("Computer missed."); break;
                    case Board.PointStates.Ship: WriteMessage($"Computer hit a {player.Board.GetShipAtPoint(shootingPoint).ShipType.Name}."); break;
                    case Board.PointStates.DestroyedShip: WriteMessage($"Computer destroyed a {player.Board.GetShipAtPoint(shootingPoint).ShipType.Name}."); break;
                }
            }
            while (computer.HitLastShot && !computer.HasWon);
        }*/

        private void HandlePlayerMoves()
        {
            do
            {
                VisualizeBoards();
                Point shootingPoint = GetPlayerShootingPoint();
                switch (player.ShootPlayerBoard(shootingPoint, computer.Board))
                {
                    case Board.PointStates.Empty: WriteMessage("Missed."); break;
                    case Board.PointStates.Ship: WriteMessage("Hit a ship."); break;
                    case Board.PointStates.DestroyedShip: WriteMessage($"Destroyed a {computer.Board.GetShipNameAtPoint(shootingPoint)}."); break;
                }
            }
            while (Board.Options.AllowExtraMoveAfterHit && player.HitLastShot && !player.HasWon);
        }

        private void VisualizeBoards()
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("Your board");
            ConsoleGrapher.DrawBoard(1, player.Board, true);
            Console.WriteLine();
            Console.WriteLine("Opponent's board");
            ConsoleGrapher.DrawBoard(Console.CursorTop, computer.Board, false);
        }

        private void WriteMessage(string msg)
        {
            int line = player.Board.Size.Height * 2 + 4;
            ConsoleGrapher.ClearLine(line);
            Console.SetCursorPosition(0, line);
            ConsoleGrapher.WriteInColor(msg, ConsoleColor.Yellow, ConsoleColor.Black);
            Console.WriteLine();
        }

        Point playerLastShotPoint;
        private Point GetPlayerShootingPoint()
        {
            bool wasEnterPressed = false;
            do
            {
                Point newPoint = playerLastShotPoint;
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.Enter:
                        {
                            if (computer.Board.IsValidPointToShoot(newPoint))
                                wasEnterPressed = true;
                            else
                                WriteMessage("You have already hit this point...");
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        if (newPoint.X - 1 >= 0)
                            newPoint.X--;
                        break;
                    case ConsoleKey.UpArrow:
                        if (newPoint.Y - 1 >= 0)
                            newPoint.Y--;
                        break;
                    case ConsoleKey.RightArrow:
                        if (newPoint.X + 1 < computer.Board.Size.Width)
                            newPoint.X++;
                        break;
                    case ConsoleKey.DownArrow:
                        if (newPoint.Y + 1 < computer.Board.Size.Height)
                            newPoint.Y++;
                        break;
                }
                playerLastShotPoint = newPoint;
                if (computer.Board.IsPointOnBoard(newPoint))
                    ConsoleGrapher.DrawBoardWithSelectedPoint(player.Board.Size.Height + 3, computer.Board, false, playerLastShotPoint);
            } while (!wasEnterPressed);
            return playerLastShotPoint;
        }
    }
}