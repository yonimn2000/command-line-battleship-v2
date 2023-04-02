using System;
using System.Drawing;

namespace YonatanMankovich.BattleshipGameEngine
{
    public class Ship
    {
        private readonly Type shipType;
        private readonly Element[] elements;

        public Type ShipType { get => new Type(shipType); }
        public Element[] Elements
        {
            get
            {
                Element[] elementsCopy = new Element[elements.Length];
                Array.Copy(elements, elementsCopy, elements.Length);
                return elementsCopy;
            }
        }

        public Ship(Type shipType, Point initialPoint)
        {
            if (shipType.Size.Height < 1 && shipType.Size.Height < 1)
                throw new ArgumentException($"Ship dimensions cannot be smaller than 1. Given: {ShipType.Size}.");
            this.shipType = new Type(shipType);
            Element[] shipElements = new Element[shipType.Size.Height * shipType.Size.Width];
            for (int x = 0; x < shipType.Size.Width; x++)
                for (int y = 0; y < shipType.Size.Height; y++)
                    shipElements[x * shipType.Size.Height + y] = new Element(new Point(x + initialPoint.X, y + initialPoint.Y));
            elements = shipElements;
        }

        public Point GetInitialPoint()
        {
            return Elements[0].Point;
        }

        public bool IsAtPoint(Point pointSearch)
        {
            foreach (Element shipElement in Elements)
                if (shipElement.Point.Equals(pointSearch))
                    return true;
            return false;
        }

        public bool IsHitAtPoint(Point pointSearch)
        {
            foreach (Element shipElement in Elements)
                if (shipElement.Point.Equals(pointSearch))
                    return shipElement.IsHit;
            return false;
        }

        public bool IsDestroyed()
        {
            foreach (Element shipElement in Elements)
                if (!shipElement.IsHit)
                    return false;
            return true;
        }

        public void HitElementAtPoint(Point point)
        {
            foreach (Element element in Elements)
            {
                if (element.Point == point)
                {
                    element.IsHit = true;
                    return;
                }
            }
            throw new PointNotOnShipException(point, this);
        }

        public override string ToString()
        {
            return ShipType + " at " + GetInitialPoint();
        }

        public class Element
        {
            public Point Point { get; }
            public bool IsHit { get; set; }

            public Element(Point pointOfElement)
            {
                Point = pointOfElement;
                IsHit = false;
            }

            public override string ToString()
            {
                return (IsHit ? "Hit" : "Not hit") + " at " + Point;
            }
        }

        public class Type
        {
            public Size Size { get; private set; }
            public string Name { get; }

            public Type(Size size, string name)
            {
                Size = size;
                Name = name;
            }

            public Type(Type type)
            {
                Size = type.Size;
                Name = type.Name;
            }

            public void FlipSize()
            {
                Size = new Size(Size.Height, Size.Width);
            }

            public override string ToString()
            {
                return Name + " " + Size;
            }
        }
    }
}