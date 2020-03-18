using SnakeBrain.SnakeGame.Interfaces;

namespace SnakeBrain.SnakeGame
{
    public class Point : IMovable
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public Point(int y, int x)
        {
            Y = y;
            X = x;
        }

        public void MoveTo(int y, int x)
        {
            Y = y;
            X = x;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Point))
                return false;

            Point P = obj as Point;
            return P.X == X && P.Y == Y;
        }

        public static Point operator +(Point P1, Point P2) =>
            new Point(P1.Y + P2.Y, P1.X + P2.X);

        public static Point operator -(Point P) =>
            new Point(-P.Y, -P.X);

        public static Point operator -(Point P1, Point P2) =>
            P1 + -P2;

        public override int GetHashCode() => 0;
    }
}
