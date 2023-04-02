using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace YonatanMankovich.BattleshipGameEngine
{
    public class Board
    {
        public Size Size { get; }

        private readonly IList<Ship> ships;
        private readonly PointStates[,] pointStatesMatrix;

        public enum PointStates { Empty, Miss, Ship, HitShipElement, DestroyedShip }

        /// <summary>
        /// Loads game options from file and creates an empty board.
        /// </summary>
        public Board()
        {
            Options.LoadOptions();
            Size = Options.BoardSize;
            ships = new List<Ship>();
            pointStatesMatrix = new PointStates[Size.Height, Size.Width];
            for (int y = 0; y < Size.Height; y++)
                for (int x = 0; x < Size.Width; x++)
                    pointStatesMatrix[y, x] = PointStates.Empty;
        }

        public void AddShipToBoard(Ship ship)
        {
            // Avoid adding too many ships.
            if (ships.Count == Options.GetFleet().Count)
                throw new BoardIsFullException(this, "Cannot add more ships to the board than is specified in the options file.");
            if (!IsShipPositionPossible(ship))
                throw new InvalidShipPositionException(ship);
            if (ship.ShipType.Size.Height > Size.Height && ship.ShipType.Size.Height > Size.Width)
                throw new ShipSizeTooBigForBoardException(ship.ShipType.Size, this);
            ships.Add(ship);
            foreach (Ship.Element element in ship.Elements)
                SetPointState(element.Point, PointStates.Ship);
        }

        /// <summary>
        /// Check if all parts of the ship are on the board, check if there is no overlap of ship elements with other ships on the board,
        /// and if AllowAdjacentShips is set to 'false' in the game options, check if there are no adjacent ships.
        /// </summary>
        public bool IsShipPositionPossible(Ship ship)
        {
            Point initialPoint = ship.GetInitialPoint();
            int allowAdjacentShipsDelta = Options.AllowAdjacentShips ? 0 : 1;
            for (int y = initialPoint.Y - allowAdjacentShipsDelta; y < initialPoint.Y + ship.ShipType.Size.Height + allowAdjacentShipsDelta; y++)
                for (int x = initialPoint.X - allowAdjacentShipsDelta; x < initialPoint.X + ship.ShipType.Size.Width + allowAdjacentShipsDelta; x++)
                {
                    Point possiblePoint = new Point(x, y);
                    if ((IsPointOnBoard(possiblePoint) && IsShipAtPoint(possiblePoint)) || (!IsPointOnBoard(possiblePoint) && ship.IsAtPoint(possiblePoint)))
                        return false;
                }
            return true;
        }

        public IList<Point> GetPossibleShootingPoints()
        {
            IList<Point> points = new List<Point>();
            for (int y = 0; y < Size.Height; y++)
                for (int x = 0; x < Size.Width; x++)
                {
                    Point point = new Point(x, y);
                    if (IsValidPointToShoot(point))
                        points.Add(point);
                }
            return points;
        }

        public bool IsValidPointToShoot(Point point)
        {
            if (!IsPointOnBoard(point))
                return false;
            PointStates pointState = GetPointState(point);
            return pointState == PointStates.Empty || pointState == PointStates.Ship;
        }

        public PointStates ShootPoint(Point point)
        {
            if (!IsPointOnBoard(point))
                throw new PointOutsideOfBoardException(point, this);
            switch (GetPointState(point))
            {
                case PointStates.Empty:
                    SetPointState(point, PointStates.Miss);
                    return PointStates.Empty;
                case PointStates.Ship:
                    Ship hitShip = GetShipAtPoint(point);
                    hitShip.HitElementAtPoint(point);
                    if (hitShip.IsDestroyed())
                    {
                        foreach (Ship.Element element in hitShip.Elements)
                            SetPointState(element.Point, PointStates.DestroyedShip);
                        if (!Options.AllowAdjacentShips)
                        {
                            Point initialShipPoint = hitShip.GetInitialPoint();
                            for (int y = initialShipPoint.Y - 1; y <= initialShipPoint.Y + hitShip.ShipType.Size.Height; y++)
                            {
                                for (int x = initialShipPoint.X - 1; x <= initialShipPoint.X + hitShip.ShipType.Size.Width; x++)
                                {
                                    Point checkPoint = new Point(x, y);
                                    if (IsPointOnBoard(checkPoint))
                                    {
                                        PointStates pointStateAtCheckPoint = GetPointState(checkPoint);
                                        if (pointStateAtCheckPoint == PointStates.Empty)
                                            SetPointState(checkPoint, PointStates.Miss);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        SetPointState(point, PointStates.HitShipElement);
                        return PointStates.Ship;
                    }
                    break;
            }
            return GetPointState(point);
        }

        public bool IsPointOnBoard(Point point)
        {
            return point.X >= 0 && point.Y >= 0 && point.X < this.Size.Width && point.Y < this.Size.Height;
        }

        public bool IsShipDestroyedAtPoint(Point point)
        {
            Ship ship = GetShipAtPoint(point);
            return ship != null && ship.IsDestroyed();
        }

        public bool IsShipHitAtPoint(Point point)
        {
            Ship ship = GetShipAtPoint(point);
            return ship != null && ship.IsHitAtPoint(point);
        }

        public bool IsShipAtPoint(Point point)
        {
            return GetShipAtPoint(point) != null;
        }

        public bool AreAllShipsDestroyed()
        {
            foreach (Ship ship in ships)
                if (!ship.IsDestroyed())
                    return false;
            return true;
        }

        private Ship GetShipAtPoint(Point point)
        {
            if (!IsPointOnBoard(point))
                throw new PointOutsideOfBoardException(point, this);
            foreach (Ship ship in ships)
                if (ship.IsAtPoint(point))
                    return ship;
            return null;
        }

        public string GetShipNameAtPoint(Point point)
        {
            return GetShipAtPoint(point).ShipType.Name;
        }

        public PointStates GetPointState(Point point)
        {
            if (!IsPointOnBoard(point))
                throw new PointOutsideOfBoardException(point, this);
            return pointStatesMatrix[point.Y, point.X];
        }

        public void SetPointState(Point point, PointStates pointState)
        {
            if (!IsPointOnBoard(point))
                throw new PointOutsideOfBoardException(point, this);
            pointStatesMatrix[point.Y, point.X] = pointState;
        }

        public static class Options
        {
            public static Size BoardSize { get; private set; }
            public static bool AllowAdjacentShips { get; private set; }
            public static bool AllowExtraMoveAfterHit { get; private set; }
            public static bool Loaded { get; private set; } = false;
            public const string PATH_TO_OPTIONS = "BoardOptions.xml";

            private static IList<Ship.Type> fleet;

            /// <summary>
            /// Loads the options from the BoardOptions.xml file or loads it from the default file and rebuilds the other file if it is missing.
            /// </summary>
            public static void LoadOptions()
            {
                // Avoid loading settings multiple times.
                if (Loaded) return;
                CreateDefaultBoardOptionsFileIfDoesNotExist();

                XmlDocument boardOptionsXML = new XmlDocument();
                // Read from the embedded XML Schema resource file.
                Stream schemaStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + ".BoardOptionsXMLSchema.xsd");
                boardOptionsXML.Schemas.Add(XmlSchema.Read(schemaStream, null));
                boardOptionsXML.Load("BoardOptions.xml");
                // Validate according to the schema file.
                boardOptionsXML.Validate(null);

                long occupiedPointCount = 0;
                fleet = new List<Ship.Type>();
                foreach (XmlNode xmlNode in boardOptionsXML.DocumentElement)
                {
                    switch (xmlNode.Name)
                    {
                        case "AllowAdjacentShips": AllowAdjacentShips = xmlNode.InnerText.Equals("true"); break;
                        case "AllowExtraMoveAfterHit": AllowExtraMoveAfterHit = xmlNode.InnerText.Equals("true"); break;
                        case "BoardSize": BoardSize = new Size(int.Parse(xmlNode.Attributes["width"].Value), int.Parse(xmlNode.Attributes["height"].Value)); break;
                        case "Ships":
                            {
                                foreach (XmlNode shipNode in xmlNode.ChildNodes)
                                {
                                    uint count = uint.Parse(shipNode.Attributes["count"].Value);
                                    Size size = new Size(int.Parse(shipNode.Attributes["width"].Value), int.Parse(shipNode.Attributes["height"].Value));
                                    occupiedPointCount += (size.Width + (AllowAdjacentShips ? 0 : 2)) * (size.Height + (AllowAdjacentShips ? 0 : 2)) * count;
                                    for (int i = 0; i < count; i++)
                                        fleet.Add(new Ship.Type(size, shipNode.Attributes["name"].Value));
                                }
                                // Order ships from largest to smallest to make it easier putting them on the board later.
                                fleet = fleet.OrderByDescending(shipType => shipType.Size.Height * shipType.Size.Width).ToList();
                            }
                            break;
                    }
                }
                // Calculate ships to board size ratio and throw an exceptions above a certain level.
                double occupiedPointsToBoardSizeRatio = occupiedPointCount / (double)((BoardSize.Width + (AllowAdjacentShips ? 0 : 2)) * (BoardSize.Height + (AllowAdjacentShips ? 0 : 2)));
                if (occupiedPointsToBoardSizeRatio > 1)
                    throw new BoardIsFullException($"The board options file ({PATH_TO_OPTIONS}) has specified adding too many ships and/or too big ships to the board of size {BoardSize}. " +
                        $"Try reducing the number of ships and/or their sizes, or reset the options file ({PATH_TO_OPTIONS}) to default by deleting it.");
                Loaded = true;
            }

            public static Queue<Ship.Type> GetFleet()
            {
                Queue<Ship.Type> newFleet = new Queue<Ship.Type>(fleet.Count);
                foreach (Ship.Type shipType in fleet)
                    newFleet.Enqueue(new Ship.Type(shipType));
                return newFleet;
            }

            public static void CreateDefaultBoardOptionsFileIfDoesNotExist()
            {
                if (!File.Exists(PATH_TO_OPTIONS))
                    RebuildDefaultBoardOptionsFile();
            }

            public static void RebuildDefaultBoardOptionsFile()
            {
                // Read from the embedded resource file.
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + ".DefaultBoardOptions.xml");
                FileStream fileStream = File.Create(PATH_TO_OPTIONS);
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
                fileStream.Close();
            }
        }
    }
}