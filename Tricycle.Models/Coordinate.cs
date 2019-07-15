namespace Tricycle.Models
{
    public struct Coordinate<T> where T : struct
    {
        public T X { get; }
        public T Y { get; }

        public Coordinate(T x, T y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            if (obj is Coordinate<T> coordinate)
            {
                return object.Equals(coordinate.X, X) && object.Equals(coordinate.Y, Y);
            }

            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 23 + X.GetHashCode();
            hash = hash * 23 + Y.GetHashCode();

            return hash;
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        }
    }
}
